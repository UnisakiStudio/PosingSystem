#if NDMF && MODULAR_AVATAR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    [InitializeOnLoad]
    internal static class PosingSystemGeneratedCleanup
    {
        internal sealed class Decision
        {
            internal string Path, Reason;
            internal bool DeleteFile;
            internal Object[] Unused = Array.Empty<Object>();
        }
        // Tests disable event-driven cleanup; they never sweep the user's open scene.
        internal static bool Suspend;
        static readonly HashSet<string> Pending = new();
        static PosingSystemGeneratedCleanup()
        {
            EditorSceneManager.sceneOpened += (scene, _) => Schedule(scene);
            EditorSceneManager.sceneSaved += OnSaved;
            EditorApplication.delayCall += () => {
                for (var i = 0; i < SceneManager.sceneCount; i++) Schedule(SceneManager.GetSceneAt(i));
            };
        }

        static void Schedule(Scene scene)
        {
            if (Suspend || !scene.IsValid() || !scene.isLoaded || string.IsNullOrEmpty(scene.path) ||
                EditorSceneManager.IsPreviewScene(scene)) return;
            var guid = AssetDatabase.AssetPathToGUID(scene.path);
            if (!Pending.Add(guid)) return;
            EditorApplication.delayCall += () => {
                Pending.Remove(guid);
                if (Suspend || !scene.IsValid() || !scene.isLoaded ||
                    AssetDatabase.AssetPathToGUID(scene.path) != guid) return;
                try
                {
                    var paths = Candidates(guid);
                    if (paths.Length == 0 || GuardReason() != null) return;
                    foreach (var decision in Analyze(scene.path, paths))
                    {
                        if (!decision.DeleteFile && decision.Unused.Length == 0) continue;
                        // A failed component import can temporarily hide serialized references from Unity.
                        if (scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                            .Any(t => GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) != 0)) return;
                        // No delayed deletion based on an old snapshot. Recheck all evidence now.
                        if (GuardReason() != null) return;
                        var fresh = Analyze(scene.path, Candidates(guid)).FirstOrDefault(d => d.Path == decision.Path);
                        if (fresh != null) Apply(fresh);
                    }
                }
                catch (Exception ex) { Debug.LogWarning("[PosingSystem] " + PosingSystemLocalization.Format("生成物の整理を保留しました: {0}", PosingSystemLocalization.TextMessage(ex.Message))); }
            };
        }

        static void OnSaved(Scene scene)
        {
            try { UpdateSavedOwner(scene); }
            catch (Exception ex) { Debug.LogWarning("[PosingSystem] " + PosingSystemLocalization.Format("生成物の所有情報の更新を保留しました: {0}", PosingSystemLocalization.TextMessage(ex.Message))); }
        }

        static void UpdateSavedOwner(Scene scene)
        {
            if (Suspend || PosingSystemGeneratedControllers.IsGenerating || EditorSceneManager.IsPreviewScene(scene)) return;
            // First save gives an unsaved scene a GUID. Moving its file preserves all subasset IDs.
            foreach (var root in scene.GetRootGameObjects())
            foreach (var avatar in root.GetComponentsInChildren<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>(true))
            {
                var owner = new PosingSystemGeneratedControllers.Owner(avatar.gameObject);
                var metadata = avatar.GetComponentsInChildren<nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator>(true)
                    .Select(m => PosingSystemGeneratedControllers.Metadata(m.animator)).Where(m => m != null).Distinct().ToArray();
                foreach (var item in metadata)
                {
                    if (item.editorSession != PosingSystemGeneratedControllers.SessionId ||
                        item.ownerInstanceId != avatar.gameObject.GetInstanceID()) continue;
                    if (item.ownerKind == "Unsaved")
                    {
                        if (PosingSystemGeneratedControllers.HasOtherUsers(item, avatar.gameObject)) continue;
                        var oldPath = AssetDatabase.GetAssetPath(item);
                        var newPath = PosingSystemGeneratedControllers.NewPath(owner, avatar.name);
                        var error = AssetDatabase.MoveAsset(oldPath, newPath);
                        if (!string.IsNullOrEmpty(error)) { Debug.LogWarning("[PosingSystem] " + PosingSystemLocalization.Format("生成物の移動を保留しました: {0}", PosingSystemLocalization.TextMessage(error))); continue; }
                    }
                    else if (item.ownerKind != owner.Kind || item.ownerGuid != owner.Guid) continue; // Save As: never steal the old scene's file.
                    owner.Write(item);
                    AssetDatabase.SaveAssetIfDirty(item);
                }
            }
            Schedule(scene);
        }

        internal static string[] Candidates(string sceneGuid)
        {
            // Filter indexed paths before loading subassets. The generated root may have moved.
            var sceneDirectory = "/Scenes/" + sceneGuid + "/";
            return AssetDatabase.FindAssets("t:PosingSystemGeneratedAsset").Select(AssetDatabase.GUIDToAssetPath)
                .Distinct().Where(path => path.StartsWith("Assets/", StringComparison.Ordinal) &&
                    path.Contains(sceneDirectory) && path.EndsWith(".controller", StringComparison.Ordinal))
                .Where(path => {
                    var data = AssetDatabase.LoadAllAssetsAtPath(path).OfType<PosingSystemGeneratedAsset>().SingleOrDefault();
                    return data != null && data.formatVersion == 1 && data.ownerKind == "Scenes" &&
                        data.ownerGuid == sceneGuid && !string.IsNullOrEmpty(data.assetId) &&
                        data.controllers != null && data.controllers.Length > 0 &&
                        data.controllers.All(c => c != null && AssetDatabase.GetAssetPath(c) == path) &&
                        data.generatedLocalIds != null && data.generatedLocalIds.Length > 0 &&
                        GlobalObjectId.TryParse(data.ownerGlobalId, out _) &&
                        AssetDatabase.LoadMainAssetAtPath(path) is UnityEditor.Animations.AnimatorController;
                }).ToArray();
        }

        internal static string GuardReason()
        {
            if (EditorUtility.scriptCompilationFailed) return "コンパイルエラーのため参照を確認できない";
            if (PosingSystemGeneratedControllers.IsGenerating || EditorApplication.isCompiling || EditorApplication.isUpdating ||
                EditorApplication.isPlayingOrWillChangePlaymode || BuildPipeline.isBuildingPlayer) return "生成・ビルド・再生・インポート中";
            if (PrefabStageUtility.GetCurrentPrefabStage() != null) return "Prefab編集中";
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && (scene.isDirty || string.IsNullOrEmpty(scene.path))) return "未保存のシーン編集";
            }
            // Unity has no supported API for enumerating references held by Undo. Inspect the two
            // stacks conservatively; if this internal read-only API changes, automatic deletion stops.
            try
            {
                var method = typeof(Undo).GetMethod("GetRecords", BindingFlags.Static | BindingFlags.NonPublic,
                    null, new[] { typeof(List<string>), typeof(List<string>) }, null);
                if (method == null) return "Undoの保護状態を確認できない";
                var undo = new List<string>(); var redo = new List<string>();
                method.Invoke(null, new object[] { undo, redo });
                if (undo.Count != 0 || redo.Count != 0) return "UndoまたはRedoの履歴あり";
            }
            catch { return "Undoの保護状態を確認できない"; }
            if (Resources.FindObjectsOfTypeAll<Object>().Any(o => EditorUtility.IsPersistent(o) && EditorUtility.IsDirty(o)))
                return "未保存のアセット編集";
            return null;
        }

        internal static List<Decision> Analyze(string scenePath, string[] candidates)
        {
            var sceneGuid = AssetDatabase.AssetPathToGUID(scenePath);
            if (string.IsNullOrEmpty(sceneGuid)) throw new InvalidOperationException(PosingSystemLocalization.TextMessage("所有シーンを確認できません。"));
            var allowed = Candidates(sceneGuid).ToHashSet();
            if (candidates.Any(p => !allowed.Contains(p))) throw new InvalidOperationException(PosingSystemLocalization.TextMessage("管理対象外の生成物です。"));
            var candidateSet = candidates.ToHashSet();
            var assets = AssetDatabase.GetAllAssetPaths().Where(p => !candidateSet.Contains(p) &&
                (p.StartsWith("Assets/") || p.StartsWith("Packages/")) && !AssetDatabase.IsValidFolder(p)).ToArray();
            // Every file is a root in this check. Direct edges suffice (including edges between
            // generated files below) and avoid repeatedly walking the entire dependency graph.
            var usedFiles = AssetDatabase.GetDependencies(assets, false).ToHashSet();
            var externallyUsedFiles = AssetDatabase.GetDependencies(assets.Where(p => p != scenePath).ToArray(), false).ToHashSet();
            foreach (var path in candidates)
                foreach (var dependency in AssetDatabase.GetDependencies(path, false).Where(p => p != path))
                { usedFiles.Add(dependency); externallyUsedFiles.Add(dependency); }

            var liveRoots = Resources.FindObjectsOfTypeAll<Object>().Where(o => !EditorUtility.IsPersistent(o) &&
                (o is GameObject || o is Component || o is ScriptableObject ||
                 o is RuntimeAnimatorController || o is Motion || o is AvatarMask)).ToArray();
            var live = EditorUtility.CollectDependencies(liveRoots).ToHashSet();
            foreach (var obj in live) { var path = AssetDatabase.GetAssetPath(obj); if (path.Length > 0) usedFiles.Add(path); }
            var result = new List<Decision>();
            foreach (var path in candidates)
            {
                var decision = new Decision { Path = path };
                result.Add(decision);
                var contents = AssetDatabase.LoadAllAssetsAtPath(path);
                var data = contents.OfType<PosingSystemGeneratedAsset>().Single();
                var generated = data.generatedLocalIds.ToHashSet();
                bool WasGenerated(Object obj) => obj != null &&
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string _, out long id) && generated.Contains(id);
                if (contents.Any(o => o != null && o != data && !WasGenerated(o)))
                { decision.Reason = "生成元を確認できない追加データあり"; continue; }
                if (!usedFiles.Contains(path)) { decision.DeleteFile = true; decision.Reason = "保存済み・編集中の参照なし"; continue; }
                if (externallyUsedFiles.Contains(path)) { decision.Reason = "別アセットから参照あり"; continue; }
                if (data.controllers == null || data.controllers.Any(c => c == null)) { decision.Reason = "管理情報が不完全"; continue; }
                var roots = data.controllers.Cast<Object>().Concat(live.Where(o => AssetDatabase.GetAssetPath(o) == path))
                    .Append(data).Append(AssetDatabase.LoadMainAssetAtPath(path));
                var reachable = PosingSystemControllerGraph.Collect(roots, o => AssetDatabase.GetAssetPath(o) == path);
                decision.Unused = contents.Where(o => o != null && WasGenerated(o) && !reachable.Contains(o) && PosingSystemControllerGraph.IsGraphObject(o)).ToArray();
                decision.Reason = decision.Unused.Length > 0 ? "ファイル内の未参照生成物" : "使用中";
            }
            return result;
        }

        // Called only with freshly checked evidence. Tests exercise this on their own asset folder;
        // production reaches it exclusively through Schedule, after GuardReason and Analyze.
        internal static void Apply(Decision decision)
        {
            if (decision.DeleteFile)
            {
                if (!AssetDatabase.MoveAssetToTrash(decision.Path))
                    throw new InvalidOperationException(PosingSystemLocalization.TextMessage(string.Format("生成ファイルをごみ箱へ移動できません: {0}", decision.Path)));
                Debug.Log("[PosingSystem] " + PosingSystemLocalization.Format("未使用の生成ファイルをごみ箱へ移動しました: {0}", decision.Path));
                return;
            }
            var removedIds = new HashSet<long>();
            foreach (var obj in decision.Unused)
                if (obj != null && AssetDatabase.GetAssetPath(obj) == decision.Path)
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string _, out long id)) removedIds.Add(id);
                    AssetDatabase.RemoveObjectFromAsset(obj); Object.DestroyImmediate(obj, true);
                }
            if (decision.Unused.Length > 0)
            {
                var data = AssetDatabase.LoadAllAssetsAtPath(decision.Path).OfType<PosingSystemGeneratedAsset>().Single();
                data.generatedLocalIds = data.generatedLocalIds.Except(removedIds).ToArray();
                EditorUtility.SetDirty(data); AssetDatabase.SaveAssetIfDirty(data);
            }
        }
    }
}
#endif
