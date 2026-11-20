#if NDMF && MODULAR_AVATAR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;

namespace jp.unisakistudio.posingsystemeditor
{
    /// <summary>
    /// While lying, a jump turns the avatar over (face up / face down). The layer that decides this waits in four
    /// states for the landing. Those states had no way out when the user left the lying poses before landing: on
    /// landing the layer wrote the lying pose type although the user was standing, and nothing wrote the standing
    /// type again, so the avatar stayed lying until the user lay down once more. Each of the four states now leaves
    /// as soon as the pose group is no longer the lying one.
    /// </summary>
    internal static class PosingSystemSleepSwitchExit
    {
        internal const string Layer = "USSPS_SitSleepSwitch";
        internal const string Group = "LocomotionTypeGroup";
        internal const int Lying = 3;
        internal const string Idle = "Disable";
        internal static readonly string[] Waiting = { "SitSleepDown_Judge", "SitSleepUp_Judge", "SitSleepDown_WaitGrounded", "SitSleepUp_WaitGrounded" };

        /// <returns>How many states got the exit (0 when the controller has it already or has no such layer).</returns>
        internal static int Apply(AnimatorController controller)
        {
            if (controller == null) return 0;
            var machine = controller.layers.FirstOrDefault(l => l.name == Layer)?.stateMachine;
            if (machine == null) return 0;
            var states = machine.states.Select(s => s.state).Where(s => s != null).ToArray();
            var idle = states.FirstOrDefault(s => s.name == Idle);
            if (idle == null) return 0;
            var changed = 0;
            foreach (var state in states.Where(s => Waiting.Contains(s.name)))
            {
                if (state.transitions.Any(t => t.destinationState == idle)) continue;
                var exit = state.AddTransition(idle);
                exit.hasExitTime = false; exit.exitTime = 0; exit.hasFixedDuration = true; exit.duration = 0; exit.offset = 0;
                exit.canTransitionToSelf = false;
                exit.AddCondition(AnimatorConditionMode.NotEqual, Lying, Group);
                // First: leaving the lying poses wins over the landing in the same evaluation.
                state.transitions = new[] { exit }.Concat(state.transitions.Where(t => t != exit)).ToArray();
                EditorUtility.SetDirty(state);
                changed++;
            }
            return changed;
        }
    }
}
#endif
