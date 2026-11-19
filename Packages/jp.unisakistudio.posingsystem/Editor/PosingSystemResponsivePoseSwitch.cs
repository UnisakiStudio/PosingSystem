#if NDMF && MODULAR_AVATAR
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;

namespace jp.unisakistudio.posingsystemeditor
{
    // In VR a pose change is a blend of half a second. Since 3.0.13 a blend that had started always ran to
    // its end, and the layer that chooses the pose number waited as long: lowering the head below a height
    // for a moment (a nod, a bow, one bend of the knees) played the whole way down and the whole way back,
    // more than a second, while the user was standing again. Repeated squats ran out of step with the user.
    //
    // 3.0.13 had switched the interruption off for a reason: until 3.0.12 an interrupted blend could leave
    // the avatar crouching for good, because the frozen pose of the interrupted blend also held the
    // height-comparison values and nothing wrote them again. The classifier layers at the end of the
    // controller (USSPS_Final*Threshold) now write those values after the pose layer in every evaluation, so
    // the blend can be interrupted again: the body turns back from where it is.
    internal static class PosingSystemResponsivePoseSwitch
    {
        internal const string PoseLayer = "USSPS_Locomotion";
        internal const string TypeLayer = "USSPS_LocomotionType";
        private static readonly string[] Classifiers = { "USSPS_FinalShallowThreshold", "USSPS_FinalDeepThreshold", "USSPS_FinalSleepThreshold" };

        /// <returns>The number of transitions that were changed.</returns>
        internal static int Apply(AnimatorController controller)
        {
            if (controller == null) return 0;
            var layers = controller.layers;
            // Without the final classifiers an interrupted blend can freeze the height comparison (the 3.0.12 defect).
            var pose = Array.FindIndex(layers, l => l.name == PoseLayer);
            if (pose < 0 || Classifiers.Any(name => Array.FindIndex(layers, l => l.name == name) < pose)) return 0;
            var changed = 0;
            foreach (var layer in layers)
            {
                if (layer.stateMachine == null) continue;
                if (layer.name == PoseLayer)
                    Visit(layer.stateMachine, transition =>
                    {
                        // The blended (VR) pose changes only: desktop changes have no duration.
                        if (transition.duration <= 0 || !transition.conditions.Any(c => c.parameter.StartsWith("USSPS_Pose", StringComparison.Ordinal))) return false;
                        if (transition.interruptionSource == TransitionInterruptionSource.Destination) return false;
                        transition.interruptionSource = TransitionInterruptionSource.Destination;
                        return true;
                    }, ref changed);
                else if (layer.name == TypeLayer)
                    Visit(layer.stateMachine, transition =>
                    {
                        // This layer plays nothing; its duration only held the pose number back.
                        if (transition.duration <= 0) return false;
                        transition.duration = 0;
                        return true;
                    }, ref changed);
            }
            return changed;
        }

        private static void Visit(AnimatorStateMachine machine, Func<AnimatorStateTransition, bool> change, ref int changed)
        {
            foreach (var child in machine.states)
                foreach (var transition in child.state.transitions)
                    if (change(transition)) { EditorUtility.SetDirty(transition); changed++; }
            foreach (var transition in machine.anyStateTransitions)
                if (change(transition)) { EditorUtility.SetDirty(transition); changed++; }
            foreach (var child in machine.stateMachines) Visit(child.stateMachine, change, ref changed);
        }
    }
}
#endif
