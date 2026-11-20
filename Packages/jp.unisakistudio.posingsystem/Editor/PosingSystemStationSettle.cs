#if NDMF && MODULAR_AVATAR
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace jp.unisakistudio.posingsystemeditor
{
    // The merged tracking layers write a part's tracking only when the merged value changes. VRChat's Sitting
    // controller is not part of the merge: it writes tracking and pose space itself, and it finishes late. Sitting
    // down starts a fixed sequence (every part to Animation, pose space, upper body to Tracking: 2.2 s in VRChat's
    // controller), and only after it does leaving the station set every part back to Tracking and leave pose space.
    // After a sit shorter than that, this write came after everything the avatar's own layers had written, and
    // nothing wrote again: the feet of a pose that animates them stayed on Tracking until the pose changed. On
    // desktop the pose space of the pose was gone after any sit.
    //
    // This pass adds a layer that knows when the Sitting controller has settled. Then every merged layer writes
    // the value it stands for once more, and on desktop the pose space of the pose is entered again. It does not
    // matter what else holds a part at that moment (an emote, AFK): the merged value is what is written.
    internal static class PosingSystemStationSettle
    {
        internal const string Layer = "Merge_StationSettle";
        internal const string Refresh = "USSPS_StationRefresh";
        internal const string Again = " (again)";
        internal const string RefreshState = "Refresh";
        internal const string RefreshPoseSpaceState = "RefreshPoseSpace";
        // After sitting down the Sitting controller needs 2.2 s, and up to 1.2 s more when it was still finishing
        // an earlier sit; the rest is margin for slow frames. A user who leaves earlier than this gets the values
        // written this long after leaving, which is at least this long after sitting down.
        internal const float EarlySeconds = 4f;
        // Later the controller answers in its next evaluation. A few frames of margin.
        internal const float LateSeconds = .15f;
        // Long enough for every layer to see the request at any frame rate.
        internal const float RefreshSeconds = .25f;

        // The states of the controller are given a one-second clip afterwards (RegisterGeneratedController), so an
        // exit time is a time in seconds.
        internal static int Apply(AnimatorController controller)
        {
            if (controller == null) return 0;
            var layers = controller.layers;
            if (layers.Any(l => l.name == Layer)) return 0;
            var merged = layers.Where(l => l.stateMachine != null &&
                l.stateMachine.states.Any(s => s.state != null && s.state.behaviours.OfType<VRCAnimatorTrackingControl>().Any())).ToArray();
            if (merged.Length == 0) return 0;
            bool Parameter(string name, AnimatorControllerParameterType type)
            {
                var existing = controller.parameters.FirstOrDefault(p => p.name == name);
                if (existing != null) return existing.type == type;
                controller.AddParameter(new AnimatorControllerParameter { name = name, type = type });
                return true;
            }
            if (!Parameter(Refresh, AnimatorControllerParameterType.Bool) || !Parameter("InStation", AnimatorControllerParameterType.Bool)) return 0;

            foreach (var layer in merged)
            {
                var machine = layer.stateMachine;
                foreach (var state in machine.states.Select(s => s.state).ToArray())
                {
                    var control = state.behaviours.OfType<VRCAnimatorTrackingControl>().FirstOrDefault();
                    if (control == null) continue;
                    // The same write again: a state is entered, so its behaviour runs.
                    var twin = machine.AddState(state.name + Again);
                    twin.writeDefaultValues = state.writeDefaultValues;
                    twin.motion = state.motion;
                    var copy = twin.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
                    if (copy == null) throw new System.InvalidOperationException("VRCAnimatorTrackingControl could not be added to " + layer.name);
                    EditorUtility.CopySerialized(control, copy);
                    Go(state, twin, null, C(Refresh, AnimatorConditionMode.If));
                    Go(twin, state, null, C(Refresh, AnimatorConditionMode.IfNot));
                }
            }

            var settle = new AnimatorControllerLayer { name = Layer, stateMachine = new AnimatorStateMachine() };
            var states = settle.stateMachine;
            var sit = C("InStation", AnimatorConditionMode.If);
            var stand = C("InStation", AnimatorConditionMode.IfNot);
            var idle = Driving(states.AddState("Idle"), false);
            var seatedEarly = Driving(states.AddState("SeatedEarly"), false);
            var leftEarly = states.AddState("LeftEarly");
            var seatedLate = states.AddState("SeatedLate");
            var leftLate = states.AddState("LeftLate");
            var refresh = Driving(states.AddState(RefreshState), true);
            // On desktop every pose except plain standing enters pose space when its state is entered, and the
            // Sitting controller has just left it.
            AnimatorState refreshPoseSpace = null;
            if (Parameter("VRMode", AnimatorControllerParameterType.Int) && Parameter("IsLocal", AnimatorControllerParameterType.Bool) &&
                Parameter("USSPS_Pose", AnimatorControllerParameterType.Int))
            {
                refreshPoseSpace = Driving(states.AddState(RefreshPoseSpaceState), true);
                var space = refreshPoseSpace.AddStateMachineBehaviour<VRCAnimatorTemporaryPoseSpace>();
                space.enterPoseSpace = true; space.fixedDelay = false; space.delayTime = 0;
            }
            states.defaultState = idle;
            void Settled(AnimatorState from, float seconds)
            {
                Go(from, seatedEarly, null, sit);
                if (refreshPoseSpace != null)
                    Go(from, refreshPoseSpace, seconds, C("VRMode", AnimatorConditionMode.Equals), C("IsLocal", AnimatorConditionMode.If), C("USSPS_Pose", AnimatorConditionMode.NotEqual));
                Go(from, refresh, seconds);
            }
            Go(idle, seatedEarly, null, sit);
            Go(seatedEarly, leftEarly, null, stand);
            Go(seatedEarly, seatedLate, EarlySeconds);
            Settled(leftEarly, EarlySeconds);
            Go(seatedLate, leftLate, null, stand);
            Settled(leftLate, LateSeconds);
            foreach (var done in new[] { refresh, refreshPoseSpace }.Where(s => s != null))
            {
                Go(done, seatedEarly, null, sit);
                Go(done, idle, RefreshSeconds);
            }
            controller.AddLayer(settle);
            return merged.Length;

            AnimatorCondition C(string parameter, AnimatorConditionMode mode, float threshold = 0) =>
                new AnimatorCondition { parameter = parameter, mode = mode, threshold = threshold };
            void Go(AnimatorState from, AnimatorState to, float? seconds, params AnimatorCondition[] conditions)
            {
                var transition = from.AddTransition(to, false);
                transition.hasExitTime = seconds.HasValue; transition.exitTime = seconds ?? 0;
                transition.hasFixedDuration = true; transition.duration = 0; transition.offset = 0;
                transition.canTransitionToSelf = false; transition.conditions = conditions;
            }
            AnimatorState Driving(AnimatorState state, bool request)
            {
                var driver = state.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                // Every client works this out for itself from the station state it sees.
                driver.localOnly = false;
                driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter { name = Refresh, type = VRC_AvatarParameterDriver.ChangeType.Set, value = request ? 1 : 0 });
                return state;
            }
        }
    }
}
#endif
