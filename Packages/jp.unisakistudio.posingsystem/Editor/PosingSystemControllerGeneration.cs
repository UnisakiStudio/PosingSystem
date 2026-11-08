#if NDMF && MODULAR_AVATAR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    // Normal builds use desktop one-evaluation playback. Explicit comparison scopes
    // still select reference/three-evaluation output without a project-wide preference.
    internal static class PosingSystemControllerGeneration
    {
        internal const string TemplateGuid = "44c97b69ea5e25340aabee19781f30ad";
        private static readonly Dictionary<GameObject, Scope> Scopes = new();

        // NDMF owns this state for one build; no static build/merge references survive it.
        internal sealed class BuildState
        {
            public BuildState() { }
            internal readonly HashSet<ModularAvatarMergeAnimator> CompiledMerges = new();
            internal readonly HashSet<PosingSystem> MissingPrebuilds = new();
        }

        internal static Scope BeginDefaultScope(GameObject avatar, BuildState state = null)
        {
            if (avatar == null || Scopes.ContainsKey(avatar)) return null;
            var scope = new Scope(avatar, true, true, true);
            if (state != null) scope.CompiledMerges.UnionWith(state.CompiledMerges);
            return scope;
        }

        internal static void MarkOwnedPrebuild(GameObject avatar, PosingSystemGeneratedAsset metadata,
            IEnumerable<ModularAvatarMergeAnimator> merges)
        {
            if (metadata != null && !metadata.preserveCustomController &&
                Scopes.TryGetValue(avatar, out var scope) && scope.IsDefault)
                scope.CompiledMerges.UnionWith(merges);
        }

        internal sealed class Scope : IDisposable
        {
            private readonly GameObject avatar;
            internal readonly bool Optimize;
            internal readonly bool OneFrame;
            internal readonly bool IsDefault;
            internal readonly List<string> Decisions = new();
            internal int Created;
            internal readonly HashSet<ModularAvatarMergeAnimator> CompiledMerges = new();
            internal Scope(GameObject root, bool optimize, bool oneFrame = false, bool isDefault = false)
            {
                if (root == null || Scopes.ContainsKey(root)) throw new InvalidOperationException("Invalid or overlapping controller generation scope.");
                if(oneFrame && !optimize)throw new ArgumentException("One-frame selection requires the optimized compiler scope");
                avatar = root; Optimize = optimize; OneFrame = oneFrame; IsDefault = isDefault; Scopes.Add(root, this);
            }
            public void Dispose() => Scopes.Remove(avatar);
        }

        internal static void MarkCompiled(GameObject avatar, IEnumerable<ModularAvatarMergeAnimator> merges)
        {
            if (Scopes.TryGetValue(avatar, out var scope) && scope.Optimize) scope.CompiledMerges.UnionWith(merges);
        }
        internal static bool ShouldOptimize(GameObject avatar, IEnumerable<ModularAvatarMergeAnimator> merges) =>
            Scopes.TryGetValue(avatar, out var scope) && scope.Optimize && merges.All(scope.CompiledMerges.Contains);
        internal static bool ShouldUseOneFrame(GameObject avatar, IEnumerable<ModularAvatarMergeAnimator> merges) =>
            Scopes.TryGetValue(avatar, out var scope) && scope.OneFrame && merges.All(scope.CompiledMerges.Contains);

        internal static PosingSystemControllerCompiler.Result TryCreate(GameObject avatar, AnimatorController source)
        {
            if (!Scopes.TryGetValue(avatar, out var scope)) return null;
            var path = AssetDatabase.GetAssetPath(source);
            scope.Decisions.Add(source.name + ": " + path + ", default=" + source.layers.Any(l => l.name == "USSPS_IsDefault"));
            // Preserve already generated poses on repeated conversion and arbitrary custom controllers.
            var referencePath = AssetDatabase.GUIDToAssetPath(TemplateGuid);
            var reference = AssetDatabase.LoadAssetAtPath<AnimatorController>(referencePath);
            if (AssetDatabase.AssetPathToGUID(path) != TemplateGuid)
            {
                if (!string.IsNullOrEmpty(path) || !source.layers.Any(l => l.name == "USSPS_IsDefault")) return null;
                if (!PosingSystemControllerEquivalence.Matches(reference, source, out var reason))
                { scope.Decisions.Add("Preserved differing in-memory controller: " + reason); return null; }
            }
            if (EditorUtility.IsDirty(reference)) throw new InvalidOperationException(PosingSystemLocalization.Text("共通Controllerに未保存の変更があります。変更を保存してからやり直してください。") + "\n" + referencePath);
            using var sha = SHA256.Create();
            var hash = BitConverter.ToString(sha.ComputeHash(File.ReadAllBytes(referencePath))).Replace("-", "").ToLowerInvariant();
            if (hash != PosingSystemControllerCompiler.ReferenceSha256)
            {
                // Unity can import the same graph from differently serialized YAML (e.g. CRLF).
                using var definition = PosingSystemControllerCompiler.Create(false);
                if (!PosingSystemControllerEquivalence.Matches(reference, definition.Controller, out var difference))
                    throw new InvalidOperationException(PosingSystemLocalization.Text("共通Controllerの内容が、このバージョンの生成定義と一致しません。"
                        + "独自の変更は別のControllerに保存し、ALCOM / VCCで同じバージョンの「ゆにさきポーズシステム」を入れ直してください。") + "\n"
                        + referencePath + "\n" + PosingSystemLocalization.Text("差異:") + " " + difference);
            }
            var result = PosingSystemControllerCompiler.Create(scope.Optimize);
            scope.Created++;
            return result;
        }

        internal static void PrepareBeforeModularAvatar(GameObject avatar, nadena.dev.ndmf.IAssetSaver saver, BuildState state = null)
        {
            state?.CompiledMerges.Clear();
            state?.MissingPrebuilds.Clear();
            using var defaults = state == null ? null : BeginDefaultScope(avatar);
            if (avatar == null || !Scopes.ContainsKey(avatar)) return;
            var merges = new HashSet<ModularAvatarMergeAnimator>();
            foreach (var system in avatar.GetComponentsInChildren<PosingSystem>(true)
                         .Where(p => p.GetAvatar()?.gameObject == avatar && !p.GetComponentsInParent<Transform>(true).Any(t => t.CompareTag("EditorOnly"))))
            {
                var marker = PosingSystemDiagnostics.CommonAnimatorMarkers(system).FirstOrDefault(PosingSystemDiagnostics.ActiveCommonMarker);
                var previousSource = marker != null && marker.TryGetComponent<ModularAvatarMergeAnimator>(out var sourceMerge)
                    ? sourceMerge.animator : null;
                var merge = PosingSystemConverter.GetCommonMergeAnimator(system, true);
                if (merge == null) continue;
                // A recovered template has no generated poses, even when the saved settings signature matches.
                if (state != null && !string.IsNullOrEmpty(system.data) && merge.animator != null &&
                    (!previousSource || AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(merge.animator)) == TemplateGuid))
                    state.MissingPrebuilds.Add(system);
                merges.Add(merge);
            }
            foreach (var group in merges.GroupBy(m => m.animator))
            {
                if (!(group.Key is AnimatorController source)) continue;
                MarkOwnedPrebuild(avatar, PosingSystemGeneratedControllers.Metadata(source), group);
                using var generated = TryCreate(avatar, source);
                if (generated == null) continue;
                var controller = generated.Controller;
                foreach (var obj in generated.ReachableObjects()) saver.SaveAsset(obj);
                // The build owns the referenced graph; discard only definition objects pruned by optimization.
                var reachable = generated.ReachableObjects().ToHashSet();
                foreach (var id in generated.Objects.Where(p => reachable.Contains(p.Value)).Select(p => p.Key).ToArray())
                    generated.Objects.Remove(id);
                foreach (var merge in group) merge.animator = controller;
                MarkCompiled(avatar, group);
            }
            if (defaults != null) state.CompiledMerges.UnionWith(defaults.CompiledMerges);
        }

#if USSPS_EXPERIMENTAL_CONTROLLER
        [MenuItem("Tools/ゆにさきスタジオ/実験/選択アバターの最適化比較コピーを作成")]
#endif
        private static void CreateSelectedComparison()
            => CreateSelectedComparison(false);

#if USSPS_EXPERIMENTAL_CONTROLLER
        [MenuItem("Tools/ゆにさきスタジオ/実験/選択アバターの1評価版比較コピーを作成")]
#endif
        private static void CreateSelectedOneFrameComparison()
            => CreateSelectedComparison(true);

        private static void CreateSelectedComparison(bool oneFrame)
        {
            var source = Selection.activeGameObject?.GetComponentInParent<VRCAvatarDescriptor>(true);
            if (source == null) throw new InvalidOperationException("Hierarchyで比較するアバター、またはその子オブジェクトを選択してください。");
            var copy = CreateComparisonCopy(source.gameObject, oneFrame);
            Undo.RegisterCreatedObjectUndo(copy, "最適化Controllerの比較コピーを作成");
            Selection.activeGameObject = copy;
            Debug.Log("最適化版の比較コピーを作成しました。元アバターは変更していません。VRChatへのアップロードは行っていません。", copy);
        }

        internal static GameObject CreateComparisonCopy(GameObject source, bool oneFrame = false)
        {
            if (source == null || source.GetComponent<VRCAvatarDescriptor>() == null || EditorUtility.IsPersistent(source))
                throw new ArgumentException("A scene avatar root is required.");
            var copy = Object.Instantiate(source, source.transform.parent);
            if (copy.scene != source.scene && copy.transform.parent == null)
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(copy, source.scene);
            copy.name = source.name + (oneFrame ? "_OneFrame_Comparison" : "_ControllerCompiler_Comparison");
            try
            {
                var pipeline = copy.GetComponent<VRC.Core.PipelineManager>();
                if (pipeline != null) pipeline.blueprintId = "";
                var systems = copy.GetComponentsInChildren<PosingSystem>(true)
                    .Where(p => p.GetAvatar()?.gameObject == copy &&
                        !p.GetComponentsInParent<Transform>(true).Any(t => t.CompareTag("EditorOnly"))).ToArray();
                if (systems.Length == 0) throw new InvalidOperationException("対象アバターにポーズシステムがありません。");
                var template = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(TemplateGuid));
                var merges = systems.Select(p => PosingSystemConverter.GetCommonMergeAnimator(p, true)).Distinct().ToArray();
                foreach (var merge in merges)
                {
                    var controller = merge.animator as AnimatorController;
                    if (controller != template && PosingSystemGeneratedControllers.Metadata(controller) == null)
                        throw new InvalidOperationException("独自の共通Controllerが設定されています。自動置換せず比較コピーの作成を中止しました。");
                }
                // Reset only the comparison copy's tool-owned common controller. Definition data,
                // avatar Base motions, imported motion adjustments and optional features remain inputs.
                foreach (var merge in merges) merge.animator = template;
                foreach (var ps in systems) ps.data = "";
                using (new Scope(copy, true, oneFrame))
                    foreach (var ps in systems) PosingSystemConverter.ConvertToModularAvatarComponents(ps);
                return copy;
            }
            catch { Object.DestroyImmediate(copy); throw; }
        }
    }
}
#endif
