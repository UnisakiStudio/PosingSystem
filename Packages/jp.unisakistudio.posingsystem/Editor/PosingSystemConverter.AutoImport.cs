#if NDMF && MODULAR_AVATAR
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using jp.unisakistudio.posingsystem;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;
using StateType = jp.unisakistudio.posingsystem.PosingSystem.OverrideAnimationDefine.AnimationStateType;

namespace jp.unisakistudio.posingsystemeditor
{
    public partial class PosingSystemConverter
    {
        private static PosingSystem[] ImportSystems(PosingSystem ps)
        {
            var avatar = ps != null ? ps.GetAvatar() : null;
            if (avatar == null) return ps == null ? Array.Empty<PosingSystem>() : new[] { ps };
            return avatar.GetComponentsInChildren<PosingSystem>(true)
                .Where(p => p.GetAvatar() == avatar && (p == ps || p.gameObject.activeInHierarchy) &&
                    !p.GetComponentsInParent<Transform>(true).Any(t => t.CompareTag("EditorOnly"))).ToArray();
        }

        internal static RuntimeAnimatorController GetAutoImportSource(PosingSystem ps)
        {
            var avatar = ps != null ? ps.GetAvatar() : null;
            if (avatar?.baseAnimationLayers == null) return null;
            // Layer arrays need not use the enum's numerical order.
            foreach (var layer in avatar.baseAnimationLayers)
                if (layer.type == VRCAvatarDescriptor.AnimLayerType.Base)
                    return layer.isDefault ? null : layer.animatorController;
            return null;
        }

        internal static List<PosingSystem.OverrideAnimationDefine> ResolveImportedMotionDefinitions(PosingSystem ps)
        {
            var systems = ImportSystems(ps);
            // Shared locomotion belongs to the avatar. Rebuilding either product must preserve
            // the other product's explicit choices. Match the Inspector's last valid enabled
            // choice, keeping disabled legacy alternatives from hiding that selection.
            var manual = systems.SelectMany(p => p.overrideDefines ?? new List<PosingSystem.OverrideAnimationDefine>())
                .Where(d => d != null).GroupBy(d => d.stateType)
                .Select(g => g.LastOrDefault(d => d.enabled && IsValidOverrideMotion(d.stateType, d.animationClip)) ?? g.Last()).ToArray();
            var manualTypes = manual.Select(d => d.stateType).ToHashSet();
            var automatic = systems.FirstOrDefault(p => p.autoImportAvatarAnimations);
            var detected = automatic == null ? new List<PosingSystem.OverrideAnimationDefine>() : AutoImportAvatarAnimations(automatic);
            // Disabled or empty manual rows intentionally block automatic import of that type.
            // Keep the original row objects and adjustments untouched.
            return detected.Where(d => !manualTypes.Contains(d.stateType)).Concat(manual)
                .OrderBy(d => d.stateType == StateType.Stand ? 1 : 0).ToList();
        }

        private static string ImportedMotionInputSignature(PosingSystem ps)
        {
            var systems = ImportSystems(ps);
            var text = new StringBuilder("auto-import-v1;");
            var fingerprints = new Dictionary<Object, string>();
            string Fingerprint(Object root)
            {
                if (root == null) return "null";
                if (fingerprints.TryGetValue(root, out var cached)) return cached;
                var graph = PosingSystemControllerGraph.Collect(new[] { root }, PosingSystemControllerGraph.IsGraphObject);
                // Include the wrapper identity for unsupported AnimatorOverrideController too.
                graph.Add(root);
                var source = new StringBuilder();
                foreach (var obj in graph.OrderBy(GetStableObjectIdentifier, StringComparer.Ordinal))
                    source.Append(GetStableObjectIdentifier(obj)).Append(':')
                        .Append(EditorJsonUtility.ToJson(obj)).Append(';');
                return fingerprints[root] = Hash128.Compute(source.ToString()).ToString();
            }
            foreach (var system in systems)
            {
                text.Append(system.autoImportAvatarAnimations ? "auto;" : "manual;");
                if (system.autoImportAvatarAnimations) text.Append(Fingerprint(GetAutoImportSource(system)));
                foreach (var row in system.overrideDefines ?? new List<PosingSystem.OverrideAnimationDefine>())
                {
                    if (row == null) { text.Append("null-row;"); continue; }
                    // No previews or transient output-controller references in the signature.
                    text.Append(JsonUtility.ToJson(new OverrideAnimationDefineDTO {
                        stateType = row.stateType, enabled = row.enabled, disablePoseIk = row.disablePoseIk,
                        isRotate = row.isRotate, rotate = row.rotate, isMotionTime = row.isMotionTime,
                        motionTimeParamName = row.motionTimeParamName,
                        animationClipGuid = GetStableObjectIdentifier(row.animationClip),
                        adjustmentClipGuid = GetStableObjectIdentifier(row.adjustmentClip),
                    }));
                    text.Append(Fingerprint(row.animationClip)).Append(Fingerprint(row.adjustmentClip));
                }
            }
            return Hash128.Compute(text.ToString()).ToString();
        }

        // Reset just the standard locomotion slots. Product pose states and other products'
        // layers remain intact, while OFF/deleted/invalid imports cannot leave stale motions.
        internal static void ResetImportedMotionStates(PosingSystem ps, IAssetSaver saver)
        {
            var merge = GetPrebuiltMergeAnimator(ps) ?? GetCommonMergeAnimator(ps);
            if (merge?.animator is not AnimatorController target) return;
            var template = FindDefaultLocomotionController();
            if (template == null) throw new InvalidOperationException("元姿勢を復元する標準Controllerが見つかりません。");
            if (target == template) throw new InvalidOperationException("配布元Controllerへ取り込み結果を書き込めません。");
            var copies = new Dictionary<BlendTree, BlendTree>();
            Motion CopyMotion(Motion motion)
            {
                if (motion is not BlendTree tree) return motion;
                if (copies.TryGetValue(tree, out var found)) return found;
                var copy = new BlendTree();
                EditorUtility.CopySerialized(tree, copy);
                copies.Add(tree, copy);
                var children = copy.children;
                for (var i = 0; i < children.Length; i++) children[i].motion = CopyMotion(children[i].motion);
                copy.children = children;
                SaveGeneratedAsset(copy, target, saver);
                return copy;
            }
            AnimatorState Find(AnimatorController controller, string layerName, string machineName, string stateName)
            {
                var machine = controller.layers.FirstOrDefault(l => l.name == layerName)?.stateMachine;
                if (machineName.Length > 0) machine = machine?.stateMachines.FirstOrDefault(m => m.stateMachine.name == machineName).stateMachine;
                return machine?.states.FirstOrDefault(s => s.state.name == stateName).state;
            }
            var visited = new HashSet<AnimatorState>();
            foreach (var setting in OverrideSettings)
            {
                var state = Find(target, setting.layerName, setting.stateMachineName, setting.stateName);
                if (state == null || !visited.Add(state)) continue;
                var original = Find(template, setting.layerName, setting.stateMachineName, setting.stateName);
                if (original == null) throw new InvalidOperationException("標準Controllerの元姿勢が見つかりません: " + setting.stateName);
                state.motion = CopyMotion(original.motion);
                state.timeParameter = original.timeParameter;
                state.timeParameterActive = original.timeParameterActive;
                EditorUtility.SetDirty(state);
            }
        }
    }
}
#endif
