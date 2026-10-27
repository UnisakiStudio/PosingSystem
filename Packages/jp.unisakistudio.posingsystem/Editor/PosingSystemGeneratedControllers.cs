#if NDMF && MODULAR_AVATAR
using System;
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    internal static class PosingSystemGeneratedControllers
    {
        internal static string SessionId
        {
            get
            {
                const string key = "USSPS.GeneratedControllers.Session";
                var id = SessionState.GetString(key, "");
                if (id.Length == 0) { id = Guid.NewGuid().ToString("N"); SessionState.SetString(key, id); }
                return id;
            }
        }
        internal static Session Current;
        internal static string OutputRootOverride;
        internal static bool IsGenerating => Current != null;
        internal static bool BelongsTo(Component component, GameObject avatar) =>
            component != null && component.GetComponentInParent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>(true)?.gameObject == avatar;

        internal readonly struct Owner
        {
            internal readonly string Kind, Guid, GlobalId, Path;
            internal readonly GameObject Avatar;
            internal Owner(GameObject avatar)
            {
                Avatar = avatar;
                var stage = PrefabStageUtility.GetPrefabStage(avatar);
                Path = stage != null ? stage.assetPath : avatar.scene.path;
                Kind = stage != null ? "Prefabs" : string.IsNullOrEmpty(Path) ? "Unsaved" : "Scenes";
                Guid = Kind == "Unsaved" ? SessionId + "-" + avatar.scene.handle : AssetDatabase.AssetPathToGUID(Path);
                if (string.IsNullOrEmpty(Guid)) throw new InvalidOperationException("生成先のシーンまたはPrefabを識別できません。");
                GlobalId = GlobalObjectId.GetGlobalObjectIdSlow(avatar).ToString();
            }
            internal bool Matches(PosingSystemGeneratedAsset data)
            {
                if (data == null || data.formatVersion != 1 || data.ownerKind != Kind || data.ownerGuid != Guid) return false;
                if (data.editorSession == SessionId && data.ownerInstanceId == Avatar.GetInstanceID()) return true;
                return Kind != "Unsaved" && data.ownerGlobalId == GlobalId;
            }
            internal void Write(PosingSystemGeneratedAsset data)
            {
                data.ownerKind = Kind; data.ownerGuid = Guid; data.ownerGlobalId = GlobalId;
                data.editorSession = SessionId; data.ownerInstanceId = Avatar.GetInstanceID();
                data.ownerName = Avatar.name; data.ownerPath = Path;
                EditorUtility.SetDirty(data);
            }
        }

        internal static PosingSystemGeneratedAsset Metadata(Object asset)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            return string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAllAssetsAtPath(path)
                .OfType<PosingSystemGeneratedAsset>().SingleOrDefault();
        }

        internal static void Convert(PosingSystem ps, IAssetSaver destination, Action<IAssetSaver> convert)
        {
            if (Current != null) throw new InvalidOperationException("ポーズ生成の再入を検出しました。");
            using var session = new Session(ps, destination);
            Current = session;
            try { session.Prepare(); convert(session); session.Commit(); }
            finally { Current = null; }
        }

        internal static void EnsureBase(PosingSystem ps)
        {
            if (Current == null) { Convert(ps, null, _ => EnsureBase(ps)); return; }
            var merge = PosingSystemConverter.GetCommonMergeAnimator(ps, true);
            if (merge == null || !(merge.animator is AnimatorController controller))
                throw new InvalidOperationException("共通ポージングAnimatorが見つかりません。");
            // Prepare has already cloned the complete mutable graph and shared it within this avatar.
            controller.layers = controller.layers.Where(l => l.name != "USSPS_IsDefault").ToArray();
        }

        internal static void EnsureBreathing(PosingSystem ps)
        {
            if (!PosingSystemFeatureUtility.UsesBreathing(ps)) return;
            if (Current == null) { Convert(ps, null, _ => EnsureBreathing(ps)); return; }
            var merge = PosingSystemConverter.GetAdditiveBreathingMergeAnimator(ps, true);
            if (merge == null || !(merge.animator is AnimatorController controller))
                throw new InvalidOperationException("呼吸用ポージングAnimatorが見つかりません。");
            if (!Current.IsTemporaryAsset(controller))
                throw new InvalidOperationException("呼吸用Controllerを生成用コピーへ分離できませんでした。");
        }

        // The converter receives an in-memory saver even for prebuilds. Only after successful
        // conversion are reachable objects persisted. No file is allocated during an NDMF build.
        internal sealed class Session : IAssetSaver
        {
            readonly PosingSystem ps;
            readonly GameObject avatar;
            readonly IAssetSaver destination;
            readonly Dictionary<ModularAvatarMergeAnimator, RuntimeAnimatorController> original = new();
            readonly Dictionary<Object, Object> copies = new();
            readonly HashSet<Object> temporary = new();
            readonly HashSet<Object> retained = new();
            readonly HashSet<AnimatorController> roots = new();
            AnimatorController mainSource, main;
            PosingSystemGeneratedAsset existing;
            bool committed;
            internal Session(PosingSystem system, IAssetSaver saver)
            {
                ps = system; destination = saver;
                avatar = ps.GetAvatar() != null ? ps.GetAvatar().gameObject : null;
                if (avatar == null) throw new InvalidOperationException("導入先アバターが見つかりません。");
                if (EditorUtility.IsPersistent(avatar))
                    throw new InvalidOperationException("Project内のPrefabを直接変更できません。シーン内のアバター、またはPrefab編集モードでプレビルドしてください。");
            }

            internal void Prepare()
            {
                foreach (var merge in avatar.GetComponentsInChildren<ModularAvatarMergeAnimator>(true).Where(m => BelongsTo(m, avatar)))
                    original[merge] = merge.animator;
                var systems = avatar.GetComponentsInChildren<PosingSystem>(true)
                    .Where(p => BelongsTo(p, avatar) && !p.GetComponentsInParent<Transform>(true).Any(t => t.CompareTag("EditorOnly"))).ToArray();
                var mainMerges = systems.Select(p => PosingSystemConverter.GetCommonMergeAnimator(p, true))
                    .Where(m => BelongsTo(m, avatar)).Distinct().ToArray();
                mainSource = mainMerges.Select(m => m.animator as AnimatorController)
                    .FirstOrDefault(c => c != null && !c.layers.Any(l => l.name == "USSPS_IsDefault")) ??
                    mainMerges.Select(m => m.animator as AnimatorController).FirstOrDefault(c => c != null);
                if (mainSource == null) throw new InvalidOperationException("共通ポージングControllerが見つかりません。");
                existing = Metadata(mainSource);

                var breathing = systems.Where(PosingSystemFeatureUtility.UsesBreathing)
                    .Select(p => PosingSystemConverter.GetAdditiveBreathingMergeAnimator(p, true))
                    .Where(m => BelongsTo(m, avatar)).Distinct().ToArray();
                foreach (var merge in mainMerges.Concat(breathing))
                    if (!original.ContainsKey(merge)) original[merge] = merge.animator;
                var breathSource = breathing.Select(m => m.animator as AnimatorController)
                    .FirstOrDefault(c => c != null && AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(c)) !=
                        PosingSystemConverter.DefaultAdditiveBreathingControllerGuid) ??
                    breathing.Select(m => m.animator as AnimatorController).FirstOrDefault(c => c != null);
                var sources = new List<AnimatorController> { mainSource };
                if (breathSource != null) sources.Add(breathSource);
                // Include previously generated feature controllers, but never another tool's controllers.
                if (existing != null && existing.controllers != null) sources.AddRange(existing.controllers.Where(c => c != null));
                using var compiled = PosingSystemControllerGeneration.TryCreate(avatar, mainSource);
                var cloneSources = compiled == null ? sources : sources.Where(c => c != mainSource).ToList();
                foreach (var pair in PosingSystemControllerGraph.Clone(cloneSources))
                { copies.Add(pair.Key, pair.Value); temporary.Add(pair.Value); }
                if (compiled != null)
                {
                    if (copies.ContainsKey(mainSource))
                        throw new InvalidOperationException("Another controller unexpectedly references the reference template graph.");
                    copies.Add(mainSource, compiled.Controller);
                    temporary.UnionWith(compiled.Objects.Values);
                    // Session takes ownership, including unreachable objects that must be cleaned up.
                    compiled.Objects.Clear();
                }
                foreach (var source in sources) roots.Add((AnimatorController)copies[source]);
                main = (AnimatorController)copies[mainSource];
                foreach (var merge in mainMerges) merge.animator = main;
                if (breathSource != null)
                    foreach (var merge in breathing) merge.animator = (AnimatorController)copies[breathSource];
                foreach (var merge in original.Keys)
                    if (merge != null && original[merge] != null && copies.TryGetValue(original[merge], out var clone))
                        merge.animator = (RuntimeAnimatorController)clone;
                // Main/breathing roles must be shared even if legacy inputs were inconsistent.
                foreach (var merge in mainMerges) merge.animator = main;
                if (breathSource != null)
                    foreach (var merge in breathing) merge.animator = (AnimatorController)copies[breathSource];
            }

            public void SaveAsset(Object obj) { if (obj != null && !EditorUtility.IsPersistent(obj)) temporary.Add(obj); }
            public bool IsTemporaryAsset(Object obj) => obj == null || !EditorUtility.IsPersistent(obj);
            public Object CurrentContainer => null;
            public IEnumerable<Object> GetPersistedAssets() => Array.Empty<Object>();

            internal void Commit()
            {
                var merges = avatar.GetComponentsInChildren<ModularAvatarMergeAnimator>(true)
                    .Where(m => BelongsTo(m, avatar) && m.animator is AnimatorController c && (temporary.Contains(c) || roots.Contains(c))).ToArray();
                var controllers = merges.Select(m => (AnimatorController)m.animator).Distinct().ToArray();
                var graph = PosingSystemControllerGraph.Collect(controllers,
                    o => PosingSystemControllerGraph.IsGraphObject(o) && !EditorUtility.IsPersistent(o));
                temporary.UnionWith(graph);
                if (destination != null)
                {
                    foreach (var obj in graph) destination.SaveAsset(obj);
                    // NullAssetSaver leaves valid build objects in memory. They belong to the caller.
                    temporary.ExceptWith(graph);
                    retained.UnionWith(graph);
                    committed = true;
                    return;
                }

                var owner = new Owner(avatar);
                var reusable = owner.Matches(existing) && !HasOtherUsers(existing, avatar);
                var path = reusable ? AssetDatabase.GetAssetPath(existing) : NewPath(owner, avatar.name);
                var previousControllers = reusable ? existing.controllers.Where(c => c != null).ToArray() : Array.Empty<AnimatorController>();
                var remap = new Dictionary<Object, Object>();
                var used = new HashSet<AnimatorController>();
                foreach (var controller in controllers)
                {
                    var old = controller == main ? (reusable ? mainSource : null) :
                        previousControllers.FirstOrDefault(c => c != mainSource && !used.Contains(c) && c.name == controller.name);
                    if (old == null) continue;
                    used.Add(old); remap[controller] = old;
                }
                var backups = remap.Values.ToDictionary(o => o, o => EditorJsonUtility.ToJson(o));
                var metadataBackup = reusable ? EditorJsonUtility.ToJson(existing) : null;
                var added = new List<Object>();
                var isNew = !reusable;
                PosingSystemGeneratedAsset metadata = existing;
                try
                {
                    if (isNew)
                    {
                        main.name = PosingSystemEditor.SanitizeAssetFileName(avatar.name, "Avatar");
                        AssetDatabase.CreateAsset(main, path);
                        added.Add(main);
                        metadata = ScriptableObject.CreateInstance<PosingSystemGeneratedAsset>();
                        metadata.name = "Posing System ownership";
                        metadata.assetId = Guid.NewGuid().ToString("N");
                        AssetDatabase.AddObjectToAsset(metadata, main); added.Add(metadata);
                    }
                    foreach (var obj in graph) PosingSystemControllerGraph.Remap(obj, remap);
                    foreach (var obj in graph)
                        if (!remap.ContainsKey(obj) && !EditorUtility.IsPersistent(obj))
                        { AssetDatabase.AddObjectToAsset(obj, path); added.Add(obj); }
                    // Keep old graph objects for Undo. Cleanup removes unreachable objects only when
                    // both Undo and Redo are empty and external references have been checked.
                    if (remap.Count > 0) Undo.RegisterCompleteObjectUndo(remap.Values.ToArray(), "ポーズのプレビルド");
                    foreach (var pair in remap) { EditorUtility.CopySerialized(pair.Key, pair.Value); EditorUtility.SetDirty(pair.Value); }
                    owner.Write(metadata);
                    metadata.controllers = controllers.Select(c => remap.TryGetValue(c, out var value) ? (AnimatorController)value : c).ToArray();
                    metadata.committedUtc = DateTime.UtcNow.ToString("O");
                    EditorUtility.SetDirty(metadata);
                    AssetDatabase.SaveAssetIfDirty(metadata);
                    var ids = new HashSet<long>(metadata.generatedLocalIds ?? Array.Empty<long>());
                    foreach (var obj in graph.Select(o => remap.TryGetValue(o, out var mapped) ? mapped : o))
                        if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string _, out long localId) && localId != 0)
                            ids.Add(localId);
                    metadata.generatedLocalIds = ids.OrderBy(id => id).ToArray();
                    EditorUtility.SetDirty(metadata);
                    AssetDatabase.SaveAssetIfDirty(metadata);
                    foreach (var merge in merges)
                    {
                        var working = merge.animator;
                        var target = remap.TryGetValue(working, out var value) ? (RuntimeAnimatorController)value : working;
                        if (original.TryGetValue(merge, out var old)) merge.animator = old;
                        Undo.RecordObject(merge, "ポーズの生成物を割り当て");
                        merge.animator = target;
                        EditorUtility.SetDirty(merge);
                        if (PrefabUtility.IsPartOfPrefabInstance(merge)) PrefabUtility.RecordPrefabInstancePropertyModifications(merge);
                    }
                    committed = true;
                }
                catch
                {
                    foreach (var backup in backups) { EditorJsonUtility.FromJsonOverwrite(backup.Value, backup.Key); EditorUtility.SetDirty(backup.Key); }
                    if (metadataBackup != null) EditorJsonUtility.FromJsonOverwrite(metadataBackup, existing);
                    if (isNew && AssetDatabase.LoadMainAssetAtPath(path) != null) AssetDatabase.DeleteAsset(path);
                    else foreach (var obj in added.AsEnumerable().Reverse())
                        if (obj != null) { AssetDatabase.RemoveObjectFromAsset(obj); Object.DestroyImmediate(obj, true); }
                    if (!isNew)
                    {
                        try { EditorUtility.SetDirty(existing); AssetDatabase.SaveAssetIfDirty(existing); }
                        catch (Exception rollbackError)
                        { Debug.LogError("[PosingSystem] 生成失敗前のデータをメモリ上で復元しましたが、保存できません: " + rollbackError.Message); }
                    }
                    throw;
                }
            }

            public void Dispose()
            {
                if (!committed)
                    foreach (var pair in original) if (pair.Key != null) pair.Key.animator = pair.Value;
                // Include objects created by Animator APIs which do not explicitly call SaveAsset.
                var graph = PosingSystemControllerGraph.Collect(temporary.Where(o => o != null).ToArray(),
                    o => PosingSystemControllerGraph.IsGraphObject(o) && !EditorUtility.IsPersistent(o) && !retained.Contains(o));
                foreach (var obj in graph.Reverse()) if (obj != null) Object.DestroyImmediate(obj);
            }
        }

        internal static string NewPath(Owner owner, string name)
        {
            var folder = (OutputRootOverride ?? PosingSystemEditor.GetGeneratedFolderPath()) + "/" + owner.Kind + "/" + owner.Guid;
            PosingSystemEditor.EnsureAssetFolderExists(folder);
            return AssetDatabase.GenerateUniqueAssetPath(folder + "/" +
                PosingSystemEditor.SanitizeAssetFileName(name, "Avatar") + "_" + Guid.NewGuid().ToString("N") + ".controller");
        }

        internal static bool HasOtherUsers(PosingSystemGeneratedAsset metadata, GameObject avatar)
        {
            var path = AssetDatabase.GetAssetPath(metadata);
            // Loaded copies (including unsaved scenes and inactive objects) are authoritative.
            foreach (var merge in Resources.FindObjectsOfTypeAll<ModularAvatarMergeAnimator>())
                if (!EditorUtility.IsPersistent(merge) && merge.gameObject.scene.IsValid() &&
                    !BelongsTo(merge, avatar) && AssetDatabase.GetAssetPath(merge.animator) == path)
                    return true;
            foreach (var component in Resources.FindObjectsOfTypeAll<Component>())
            {
                if (component == null || component is Transform || EditorUtility.IsPersistent(component) ||
                    BelongsTo(component, avatar)) continue;
                if (PosingSystemControllerGraph.References(component).Any(o => AssetDatabase.GetAssetPath(o) == path))
                    return true;
            }
            foreach (var obj in Resources.FindObjectsOfTypeAll<ScriptableObject>())
            {
                if (obj == null || obj is PosingSystemGeneratedAsset || obj is Editor || obj is EditorWindow ||
                    (EditorUtility.IsPersistent(obj) && !EditorUtility.IsDirty(obj))) continue;
                if (EditorUtility.CollectDependencies(new Object[] { obj }).Any(o => AssetDatabase.GetAssetPath(o) == path))
                    return true;
            }
            var stage = PrefabStageUtility.GetPrefabStage(avatar);
            var ownerPath = stage != null ? stage.assetPath : avatar.scene.path;
            var otherAssets = AssetDatabase.GetAllAssetPaths().Where(p => p != path && p != ownerPath &&
                (p.StartsWith("Assets/") || p.StartsWith("Packages/")) && !AssetDatabase.IsValidFolder(p)).ToArray();
            return AssetDatabase.GetDependencies(otherAssets, false).Contains(path);
        }
    }
}
#endif
