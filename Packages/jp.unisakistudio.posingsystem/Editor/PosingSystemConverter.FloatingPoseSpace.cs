#if NDMF && MODULAR_AVATAR
using System;
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace jp.unisakistudio.posingsystemeditor
{
    public partial class PosingSystemConverter
    {
        public const string FloatingPoseSpacePrefix = "USSPS_FloatPoseSpace";
        private const string FloatPoseSupported = FloatingPoseSpacePrefix + "Supported";
        private const string FloatPoseRestore = FloatingPoseSpacePrefix + "Restore";
        private const string FloatPoseKey = FloatingPoseSpacePrefix + "Key";
        private const string FloatPoseSavedKey = FloatingPoseSpacePrefix + "SavedKey";
        private const string FloatPoseSavedHeight = FloatingPoseSpacePrefix + "SavedHeight";
        private const string FloatPoseHeightDelta = FloatingPoseSpacePrefix + "HeightDelta";
        private const string FloatPoseKeyDelta = FloatingPoseSpacePrefix + "KeyDelta";
        public const float FloatPoseChangeThreshold = 1f / 255;

        private static void BuildFloatingPoseSpace(PosingSystem ps, AnimatorController controller,
            AnimatorControllerLayer main, IAssetSaver saver)
        {
            var mode = ps.features.floatingPoseSpace;
            if (!ps.features.floating || mode == PosingSystemFeatureSettings.FloatingPoseSpaceMode.Disabled ||
                ps.features.floatingBackend != PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive) return;
            var everyFrame = mode == PosingSystemFeatureSettings.FloatingPoseSpaceMode.EnterEveryFrame;
            var immediateChanges = mode == PosingSystemFeatureSettings.FloatingPoseSpaceMode.ReenterOnChangeImmediately;
            var immediateHeight = mode == PosingSystemFeatureSettings.FloatingPoseSpaceMode.ReenterHeightImmediately;
            var heightMenuOnly = mode == PosingSystemFeatureSettings.FloatingPoseSpaceMode.HeightMenuOnly;
            var fastChanges = immediateChanges || immediateHeight || heightMenuOnly;
            var watchChanges = mode == PosingSystemFeatureSettings.FloatingPoseSpaceMode.ReenterOnChange ||
                               mode == PosingSystemFeatureSettings.FloatingPoseSpaceMode.ExitThenReenter || fastChanges;
            FeatureParameter(controller, "IsLocal", AnimatorControllerParameterType.Bool);
            FeatureParameter(controller, "VRMode", AnimatorControllerParameterType.Int);
            if (heightMenuOnly) FeatureParameter(controller, PosingSystemFeatureUtility.FloatHeightAdjusting, AnimatorControllerParameterType.Bool);
            foreach (var name in new[] { FloatPoseSupported, FloatPoseRestore, FloatPoseKey, FloatPoseSavedKey,
                         FloatPoseSavedHeight, FloatPoseHeightDelta, FloatPoseKeyDelta, "USSPS_InternalNoOp" })
                FeatureParameter(controller, name, AnimatorControllerParameterType.Float);

            // Observe the same pose state, without executing its tracking controls or pose-space behaviours twice.
            var states = AllFeatureStates(main.stateMachine).ToArray();
            var info = FeatureLayer(controller, FloatingPoseSpacePrefix + "Info", AvatarMaskBodyPart.Root, false, saver);
            info.syncedLayerIndex = Array.FindIndex(controller.layers, l => l.name == main.name);
            info.syncedLayerAffectsTiming = false;
            for (var i = 0; i < states.Length; i++)
            {
                var state = states[i];
                var clip = new AnimationClip { name = "_" + FloatingPoseSpacePrefix + "Info_" + i };
                Set(clip, FloatPoseKey, i + 1);
                Set(clip, FloatPoseSupported, SupportsPoseSpaceMovement(WithoutFootHeight(state.motion)) ? 1 : 0);
                Set(clip, FloatPoseRestore, state.behaviours.OfType<VRC_AnimatorTemporaryPoseSpace>().LastOrDefault()?.enterPoseSpace == true ? 1 : 0);
                SaveGeneratedAsset(clip, controller, saver);
                info.SetOverrideMotion(state, clip);
                info.SetOverrideBehaviours(state, Array.Empty<StateMachineBehaviour>());
            }
            var layers = controller.layers; layers[layers.Length - 1] = info; controller.layers = layers;
            if (watchChanges)
            {
                Difference(PosingSystemFeatureUtility.FloatHeight, FloatPoseSavedHeight, FloatPoseHeightDelta, 1);
                if (!heightMenuOnly) Difference(FloatPoseKey, FloatPoseSavedKey, FloatPoseKeyDelta, Mathf.Max(1, states.Length));
            }

            var timer = new AnimationClip { name = "_" + FloatingPoseSpacePrefix + "Timer" };
            Set(timer, "USSPS_InternalNoOp", 0);
            SaveGeneratedAsset(timer, controller, saver);
            var layer = FeatureLayer(controller, FloatingPoseSpacePrefix + "Control", AvatarMaskBodyPart.Root, false, saver);
            var idle = State("Idle");
            var settle = everyFrame || immediateChanges ? null : State("Settle");
            var entered = State("Entered");
            var watching = State(everyFrame ? "EnteredAgain" : "Watching");
            var release = State("Release");
            var restoreWait = State("RestoreWait");
            var restoreReady = State("RestoreReady");
            var restore = State("RestoreDesktop");
            layer.stateMachine.defaultState = idle;
            PoseSpace(entered, true);
            PoseSpace(release, false);
            PoseSpace(restore, true);
            if (watchChanges)
            {
                var snapshot = entered.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                snapshot.localOnly = true;
                snapshot.debugString = FloatingPoseSpacePrefix + " Snapshot";
                snapshot.parameters.Add(new VRC_AvatarParameterDriver.Parameter { type = VRC_AvatarParameterDriver.ChangeType.Copy,
                    name = FloatPoseSavedHeight, source = PosingSystemFeatureUtility.FloatHeight });
                if (!heightMenuOnly)
                    snapshot.parameters.Add(new VRC_AvatarParameterDriver.Parameter { type = VRC_AvatarParameterDriver.ChangeType.Copy,
                        name = FloatPoseSavedKey, source = FloatPoseKey });
            }

            AnimatorState refresh = settle;
            if (mode == PosingSystemFeatureSettings.FloatingPoseSpaceMode.ExitThenReenter)
            {
                refresh = State("ExitBeforeRefresh");
                PoseSpace(refresh, false);
                Cancel(refresh);
                After(refresh, settle, .04f);
            }
            Cancel(entered);
            Cancel(watching);
            if (everyFrame)
            {
                // Two distinct states re-enter once per Animator evaluation without an Exit request.
                // Cancellation transitions stay first so disabling cannot be starved by the loop.
                PoseSpace(watching, true);
                Active(FeatureTransition(idle.AddTransition(entered)));
                Active(FeatureTransition(entered.AddTransition(watching)));
                Active(FeatureTransition(watching.AddTransition(entered)));
            }
            else
            {
                Active(FeatureTransition(idle.AddTransition(immediateChanges ? entered : refresh)));
                if (settle != null)
                {
                    Cancel(settle);
                    Active(After(settle, entered, .08f)); // Let the pose layers settle before sampling the head.
                }
                if (fastChanges) Active(FeatureTransition(entered.AddTransition(watching)));
                else After(entered, watching, .04f); // Legacy comparison: let driver copies and difference curves settle.
            }
            if (watchChanges)
            {
                // Pose changes take precedence when height and pose change together.
                if (!heightMenuOnly) Changed(watching, immediateChanges ? entered : refresh, FloatPoseKeyDelta);
                var heightTarget = fastChanges ? entered : refresh;
                if (immediateHeight)
                {
                    // The synced pose key reaches the difference tree one evaluation after height.
                    // Check it once more before sampling, without a fixed-time wait for height alone.
                    heightTarget = State("HeightReady");
                    Cancel(heightTarget);
                    Changed(heightTarget, settle, FloatPoseKeyDelta);
                    Active(FeatureTransition(heightTarget.AddTransition(entered)));
                }
                Changed(watching, heightTarget, FloatPoseHeightDelta);
            }
            // Idle never clears a pose space it did not own. Only leaving our active states reaches Release.
            After(release, restoreWait, .04f);
            After(restoreWait, restoreReady, .08f);
            var restoreDesktop = FeatureTransition(restoreReady.AddTransition(restore));
            restoreDesktop.AddCondition(AnimatorConditionMode.Equals, 0, "VRMode");
            restoreDesktop.AddCondition(AnimatorConditionMode.Greater, .5f, FloatPoseRestore);
            restoreDesktop.AddCondition(AnimatorConditionMode.If, 0, "IsLocal");
            FeatureTransition(restoreReady.AddTransition(idle)).AddCondition(AnimatorConditionMode.NotEqual, 0, "VRMode");
            FeatureTransition(restoreReady.AddTransition(idle)).AddCondition(AnimatorConditionMode.Less, .5f, FloatPoseRestore);
            FeatureTransition(restoreReady.AddTransition(idle)).AddCondition(AnimatorConditionMode.IfNot, 0, "IsLocal");
            After(restore, idle, .04f);
            PersistFeatureStates(layer, controller, saver);

            AnimatorState State(string name)
            {
                var state = layer.stateMachine.AddState(name);
                state.motion = timer; state.writeDefaultValues = false;
                return state;
            }
            void PoseSpace(AnimatorState state, bool enter)
            {
                var behaviour = state.AddStateMachineBehaviour<VRCAnimatorTemporaryPoseSpace>();
                behaviour.enterPoseSpace = enter; behaviour.fixedDelay = true; behaviour.delayTime = 0;
                behaviour.name = FloatingPoseSpacePrefix + " " + state.name;
                // Avoid writing a VRChat debug log on every frame in the continuous comparison.
                behaviour.debugString = everyFrame && enter ? string.Empty : behaviour.name;
            }
            void Cancel(AnimatorState state)
            {
                FeatureTransition(state.AddTransition(release)).AddCondition(AnimatorConditionMode.IfNot, 0, PosingSystemFeatureUtility.FloatActive);
                // The menu-only mode keeps the existing view through pose changes, including proxy locomotion.
                // It releases only when floating is explicitly switched off.
                if (!heightMenuOnly) FeatureTransition(state.AddTransition(release)).AddCondition(AnimatorConditionMode.Less, .5f, FloatPoseSupported);
            }
            void Active(AnimatorStateTransition transition)
            {
                transition.AddCondition(AnimatorConditionMode.If, 0, PosingSystemFeatureUtility.FloatActive);
                transition.AddCondition(AnimatorConditionMode.If, 0, "IsLocal");
                transition.AddCondition(AnimatorConditionMode.Greater, .5f, FloatPoseSupported);
            }
            AnimatorStateTransition After(AnimatorState from, AnimatorState to, float seconds)
            {
                var transition = FeatureTransition(from.AddTransition(to));
                transition.hasExitTime = true;
                // All these states use the same one-second, non-looping timer clip, at speed 1.
                transition.exitTime = seconds;
                return transition;
            }
            void Changed(AnimatorState from, AnimatorState to, string parameter)
            {
                foreach (var greater in new[] { false, true })
                {
                    // No Exit Time: keep detecting changes even after a long stable pose.
                    var changed = FeatureTransition(from.AddTransition(to));
                    Active(changed);
                    if (heightMenuOnly) changed.AddCondition(AnimatorConditionMode.If, 0, PosingSystemFeatureUtility.FloatHeightAdjusting);
                    var threshold = parameter == FloatPoseHeightDelta ? FloatPoseChangeThreshold : .5f;
                    changed.AddCondition(greater ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less,
                        greater ? threshold : -threshold, parameter);
                }
            }
            void Set(AnimationClip clip, string parameter, float value) => AnimationUtility.SetEditorCurve(clip,
                EditorCurveBinding.FloatCurve("", typeof(Animator), parameter), AnimationCurve.Constant(0, 1, value));
            void Difference(string current, string saved, string output, float maximum)
            {
                var math = FeatureLayer(controller, output, AvatarMaskBodyPart.Root, false, saver);
                var currentTree = new BlendTree { name = "_" + output, blendParameter = current, useAutomaticThresholds = false };
                foreach (var a in new[] { 0f, maximum })
                {
                    var savedTree = new BlendTree { name = "_" + output + "_" + a, blendParameter = saved, useAutomaticThresholds = false };
                    foreach (var b in new[] { 0f, maximum })
                    {
                        var clip = new AnimationClip { name = "_" + output + "_" + a + "_" + b };
                        Set(clip, output, a - b); SaveGeneratedAsset(clip, controller, saver); savedTree.AddChild(clip, b);
                    }
                    SaveGeneratedAsset(savedTree, controller, saver); currentTree.AddChild(savedTree, a);
                }
                SaveGeneratedAsset(currentTree, controller, saver);
                var state = math.stateMachine.AddState("Difference"); state.motion = currentTree; state.writeDefaultValues = false;
                PersistFeatureStates(math, controller, saver);
            }
        }
    }
}
#endif
