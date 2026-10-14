using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
#if NDMF && MODULAR_AVATAR
using nadena.dev.ndmf;

namespace jp.unisakistudio.posingsystemeditor
{
    public partial class PosingSystemConverter
    {
        // SDK asset identity, not a state/controller/clip name heuristic.
        public const string StandardIdleClipGuid = "b0f4aa27579b9c442a87f46f90d20192";
        public const string SuppressedIdleClipName = "USSPS_DefaultIdleDisabled";

        private class DefaultIdleSuppressionState
        {
            public bool enabled;
        }

        private static void SuppressDefaultIdle(BuildContext ctx)
        {
            if (!ctx.GetState<DefaultIdleSuppressionState>().enabled) return;
            var descriptor = ctx.AvatarDescriptor;
            if (descriptor == null) return;
            var standardIdle = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                AssetDatabase.GUIDToAssetPath(StandardIdleClipGuid));
            if (standardIdle == null)
                throw new InvalidOperationException("VRChat標準の待機アニメーションを特定できません。VRChat SDKを確認してください。");

            var slots = descriptor.baseAnimationLayers;
            for (var i = 0; i < slots.Length; i++)
            {
                if (slots[i].type != VRCAvatarDescriptor.AnimLayerType.Additive) continue;
                // MA/NDMF materializes Default/None as an explicit, deep-cloned controller.
                // Leave the layers and behaviours in place; only replace the standard proxy.
                if (slots[i].animatorController is not AnimatorController controller)
                    throw new InvalidOperationException("標準の揺れを無効化するAdditive Controllerが生成されていません。");
                RequireBuildAsset(controller);
                var replaced = new Dictionary<Motion, Motion>();
                AnimationClip empty = null;
                var layers = controller.layers;
                foreach (var layer in layers)
                {
                    foreach (var state in IdleStates(layer.stateMachine))
                    {
                        var replacement = Replace(state.motion);
                        if (replacement == state.motion) continue;
                        RequireBuildAsset(state);
                        state.motion = replacement;
                    }
                }
                foreach (var layer in layers)
                {
                    if (layer.syncedLayerIndex < 0 || layer.syncedLayerIndex >= layers.Length) continue;
                    foreach (var state in IdleStates(layers[layer.syncedLayerIndex].stateMachine))
                    {
                        var motion = layer.GetOverrideMotion(state);
                        var replacement = Replace(motion);
                        if (replacement != motion) layer.SetOverrideMotion(state, replacement);
                    }
                }
                // Assigning layers also invalidates NDMF's LastCommit cache, so a later
                // AnimatorServicesContext activation cannot restore the old proxy reference.
                controller.layers = layers;
                slots[i].isDefault = false;
                descriptor.customizeAnimationLayers = true;

                Motion Replace(Motion motion)
                {
                    if (motion == null) return null;
                    if (replaced.TryGetValue(motion, out var existing)) return existing;
                    if (motion == standardIdle || ObjectRegistry.GetReference(motion).Object == standardIdle)
                    {
                        if (empty == null)
                        {
                            empty = new AnimationClip { name = SuppressedIdleClipName };
                            ctx.AssetSaver.SaveAsset(empty);
                        }
                        return replaced[motion] = empty;
                    }
                    replaced[motion] = motion;
                    if (motion is BlendTree tree)
                    {
                        var children = tree.children;
                        var changed = false;
                        for (var child = 0; child < children.Length; child++)
                        {
                            var replacement = Replace(children[child].motion);
                            changed |= replacement != children[child].motion;
                            children[child].motion = replacement;
                        }
                        if (changed)
                        {
                            RequireBuildAsset(tree);
                            tree.children = children;
                        }
                    }
                    return motion;
                }
            }
            descriptor.baseAnimationLayers = slots;

            void RequireBuildAsset(UnityEngine.Object asset)
            {
                // Fail safely if processing order changes: never edit a shared source asset.
                if (!ctx.IsTemporaryAsset(asset))
                    throw new InvalidOperationException("標準の揺れの無効化にはビルド用Controllerの複製が必要です: " + asset.name);
            }
        }

        private static IEnumerable<AnimatorState> IdleStates(AnimatorStateMachine machine)
        {
            if (machine == null) yield break;
            foreach (var child in machine.states) yield return child.state;
            foreach (var child in machine.stateMachines)
                foreach (var state in IdleStates(child.stateMachine)) yield return state;
        }
    }
}
#endif
