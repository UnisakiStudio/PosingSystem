#if NDMF && MODULAR_AVATAR
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;

namespace jp.unisakistudio.posingsystemeditor
{
    /// <summary>
    /// Write Defaults of the layers the tool adds to the avatar's FX controller.
    ///
    /// While a layer whose weight is above zero is in a state that has Write Defaults off and a motion, Unity writes
    /// no default value in that controller (measured 2026-11-23: any clip counts, even one without curves; where the
    /// layer sits does not matter; a state without motion, or with Write Defaults on, does not do it). One such layer
    /// of ours therefore breaks animations of the avatar that the tool never touches: a toggle that relies on Write
    /// Defaults stays on, a gesture expression does not go back, an Additive layer after it adds up without end.
    ///
    /// So every FX state of the tool is created with Write Defaults on. Modular Avatar ("match avatar write
    /// defaults") turns a merged controller off only for an avatar whose states are all off, where nothing relies on
    /// default values. For an avatar it judges mixed it changes nothing, and the layers stay on.
    /// </summary>
    internal static class PosingSystemFxWriteDefaults
    {
        /// <summary>What an FX state of the tool is created with.</summary>
        internal const bool Authored = true;

        /// <summary>The objects the tool generates for its FX controllers start with this.</summary>
        private const string GeneratedPrefix = "USSPS_";

        internal static void Author(AnimatorController controller)
        {
            foreach (var layer in controller.layers)
                foreach (var state in States(layer.stateMachine))
                    state.writeDefaultValues = Authored;
        }

        /// <summary>
        /// FX controllers generated before this rule (a prebuild made with 3.2.0 or older) still have Write Defaults
        /// off. Correct them in every build, before Modular Avatar merges them: nobody has to update a prebuild.
        /// </summary>
        internal static void AuthorGeneratedMerges(BuildContext ctx)
        {
            if (ctx == null || ctx.AvatarRootObject == null) return;
            var controllers = ctx.Extension<AnimatorServicesContext>().ControllerContext.Controllers;
            foreach (var merge in ctx.AvatarRootObject.GetComponentsInChildren<ModularAvatarMergeAnimator>(true))
            {
                if (!IsGeneratedFxMerge(merge)) continue;
                merge.matchAvatarWriteDefaults = true;
                if (!controllers.TryGetValue(merge, out var controller) || controller == null) continue;
                foreach (var layer in controller.Layers)
                    if (layer.StateMachine != null)
                        foreach (var state in layer.StateMachine.AllStates())
                            state.WriteDefaultValues = Authored;
            }
        }

        // Only what the tool generates itself. A controller a prefab or a user brings keeps what its author chose.
        private static bool IsGeneratedFxMerge(ModularAvatarMergeAnimator merge) =>
            merge.layerType == VRCAvatarDescriptor.AnimLayerType.FX && merge.name.StartsWith(GeneratedPrefix) &&
            merge.GetComponentInParent<PosingSystem>(true) != null;

        /// <summary>
        /// A layer added to the built FX controller after Modular Avatar has finished gets no matching. It takes
        /// what Modular Avatar decided for a layer of ours that it merged; without such a layer, on.
        /// </summary>
        internal static bool AfterModularAvatar(AnimatorController fx, string mergedLayer)
        {
            var merged = fx.layers.Where(l => l.name == mergedLayer).SelectMany(l => States(l.stateMachine)).FirstOrDefault();
            return merged == null ? Authored : merged.writeDefaultValues;
        }

        private static IEnumerable<AnimatorState> States(AnimatorStateMachine machine)
        {
            if (machine == null) yield break;
            foreach (var child in machine.states) yield return child.state;
            foreach (var child in machine.stateMachines)
                foreach (var state in States(child.stateMachine)) yield return state;
        }
    }
}
#endif
