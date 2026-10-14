#if NDMF && MODULAR_AVATAR
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
        private const string RolloverTimeLayer = "USSPS_RolloverTime";

        private static void ConfigurePoseMotionTime(AnimatorController controller, AnimatorState state,
            PosingSystem.AnimationDefine pose)
        {
            var parameter = pose.isRollover ? PosingSystemFeatureUtility.RollTime :
                pose.isMotionTime ? pose.motionTimeParamName : "";
            state.timeParameterActive = !string.IsNullOrWhiteSpace(parameter);
            state.timeParameter = state.timeParameterActive ? parameter : "";
            if (state.timeParameterActive)
                FeatureParameter(controller, parameter, AnimatorControllerParameterType.Float, pose.isRollover ? .5f : 0);
        }

        private static void BuildRolloverTime(AnimatorController controller, PosingSystemFeatureSettings settings, IAssetSaver saver)
        {
            // Animated local parameters belong to their controller. Calculate inside Base too,
            // rather than relying on the legacy Gesture controller's animated RollTime value.
            FeatureParameter(controller, PosingSystemFeatureUtility.RollActive, AnimatorControllerParameterType.Bool);
            FeatureParameter(controller, PosingSystemFeatureUtility.RollSensor, AnimatorControllerParameterType.Float, .5f);
            FeatureParameter(controller, PosingSystemFeatureUtility.RollTime, AnimatorControllerParameterType.Float, .5f);
            FeatureParameter(controller, "HeadTrackingLock", AnimatorControllerParameterType.Bool);
            FeatureParameter(controller, PosingSystemFeatureUtility.PhotoActive, AnimatorControllerParameterType.Bool);
            var layer = FeatureLayer(controller, RolloverTimeLayer, AvatarMaskBodyPart.Root, false, saver);
            MaskRolloverCalculation(layer);
            var neutral = layer.stateMachine.AddState("寝返りオフ・中央の姿勢");
            var calculate = layer.stateMachine.AddState("頭の傾きで寝返り");
            layer.stateMachine.defaultState = neutral;
            neutral.writeDefaultValues = calculate.writeDefaultValues = false;
            var clip = new AnimationClip { name = "RolloverNeutral" };
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), PosingSystemFeatureUtility.RollTime),
                AnimationCurve.Constant(0, 1, .5f));
            SaveGeneratedAsset(clip, controller, saver);
            neutral.motion = clip;
            calculate.motion = CreateRolloverMapping(controller, settings, saver);
            var enter = FeatureTransition(neutral.AddTransition(calculate));
            enter.AddCondition(AnimatorConditionMode.If, 0, PosingSystemFeatureUtility.RollActive);
            enter.AddCondition(AnimatorConditionMode.IfNot, 0, "HeadTrackingLock");
            enter.AddCondition(AnimatorConditionMode.IfNot, 0, PosingSystemFeatureUtility.PhotoActive);
            FeatureTransition(calculate.AddTransition(neutral)).AddCondition(AnimatorConditionMode.IfNot, 0, PosingSystemFeatureUtility.RollActive);
            FeatureTransition(calculate.AddTransition(neutral)).AddCondition(AnimatorConditionMode.If, 0, "HeadTrackingLock");
            FeatureTransition(calculate.AddTransition(neutral)).AddCondition(AnimatorConditionMode.If, 0, PosingSystemFeatureUtility.PhotoActive);
            PersistFeatureStates(layer, controller, saver);
        }

        private static BlendTree CreateRolloverMapping(AnimatorController controller, PosingSystemFeatureSettings settings, IAssetSaver saver)
        {
            var mapping = new BlendTree { name = "HeadTiltToMotionTime", blendParameter = PosingSystemFeatureUtility.RollSensor,
                blendType = BlendTreeType.Simple1D, useAutomaticThresholds = false };
            // A missing contact produces zero. Keep it neutral, distinct from the valid leftmost contact.
            var samples = new[] { 0f, .01f, .5f - 1 / 2.2f }.Concat(Enumerable.Range(0, 65).Select(i =>
                .5f + Mathf.Sin(Mathf.Lerp(-90, 90, i / 64f) * Mathf.Deg2Rad) / 2.2f)).Distinct().OrderBy(x => x);
            foreach (var sample in samples)
            {
                var clip = new AnimationClip { name = "Tilt_" + sample };
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), PosingSystemFeatureUtility.RollTime),
                    AnimationCurve.Constant(0, 1, PosingSystemHeadSensorUtility.RolloverTime(sample, settings.rolloverAngle, settings.rolloverDeadZone)));
                SaveGeneratedAsset(clip, controller, saver);
                mapping.AddChild(clip, sample);
            }
            SaveGeneratedAsset(mapping, controller, saver);
            return mapping;
        }

        private static void BuildFloating(PosingSystem ps, AnimatorController controller, IAssetSaver saver)
        {
            FeatureParameter(controller, PosingSystemFeatureUtility.FloatActive, AnimatorControllerParameterType.Bool);
            FeatureParameter(controller, PosingSystemFeatureUtility.FloatHeight, AnimatorControllerParameterType.Float);
            BuildFloatingActivation(ps, saver);
            if (ps.features.floatingBackend == PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive) return;
            MeasureRootTranslationUnits(ps.GetAvatar().gameObject, out var units);
            var zero = RootOffsetClip(Vector3.zero, Quaternion.identity, "FloatZero");
            var up = RootOffsetClip(Vector3.up * 3 / Mathf.Max(.001f, Mathf.Abs(units.y)), Quaternion.identity, "Float3m");
            AnimationUtility.SetAdditiveReferencePose(up, zero, 0);
            SaveGeneratedAsset(zero, controller, saver);
            SaveGeneratedAsset(up, controller, saver);
            var blend = new BlendTree { name = "FloatingHeight", blendParameter = PosingSystemFeatureUtility.FloatHeight,
                blendType = BlendTreeType.Simple1D, useAutomaticThresholds = false };
            blend.AddChild(zero, 0);
            blend.AddChild(up, 1);
            SaveGeneratedAsset(blend, controller, saver);
            AddFeatureMotion(controller, "USSPS_Floating", blend, zero, PosingSystemFeatureUtility.FloatActive, saver,
                ps.features.floatingBackend != PosingSystemFeatureSettings.MovementBackend.AdditiveOverride, ps.features.movementWriteDefaults);
            // No backend emits Tracking Control here. Interaction with VRChat IK is verified in-game.
        }

        private static void BuildFloatingActivation(PosingSystem ps, IAssetSaver saver)
        {
            // Derive this unsynced flag on every client from the single synced height value.
            var controller = CreateFeatureController(ps, "USSPS_FloatActivation", VRCAvatarDescriptor.AnimLayerType.FX, saver);
            FeatureParameter(controller, PosingSystemFeatureUtility.FloatActive, AnimatorControllerParameterType.Bool);
            FeatureParameter(controller, PosingSystemFeatureUtility.FloatHeight, AnimatorControllerParameterType.Float);
            var layer = FeatureLayer(controller, "床の高さの有効化", AvatarMaskBodyPart.Root, false, saver);
            var off = layer.stateMachine.AddState("床の高さ0");
            var on = layer.stateMachine.AddState("床を上げる");
            foreach (var state in new[] { off, on })
            {
                state.writeDefaultValues = false;
                var driver = state.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                driver.localOnly = false;
                driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
                {
                    name = PosingSystemFeatureUtility.FloatActive,
                    value = state == on ? 1 : 0
                });
            }
            FeatureTransition(off.AddTransition(on)).AddCondition(AnimatorConditionMode.Greater,
                PosingSystemFeatureUtility.FloatHeightThreshold, PosingSystemFeatureUtility.FloatHeight);
            FeatureTransition(on.AddTransition(off)).AddCondition(AnimatorConditionMode.Less,
                PosingSystemFeatureUtility.FloatHeightThreshold, PosingSystemFeatureUtility.FloatHeight);
            PersistFeatureStates(layer, controller, saver);
        }

        private static void BuildRollover(PosingSystem ps, PosingSystem[] systems, IAssetSaver saver)
        {
            // Per-pose rollover plays in Base. Do not append an unused, WD-Off Gesture controller
            // to the avatar merely to calculate a value which Base already calculates itself.
            var legacyPoses = systems.Where(p => p.features.rollover && p.features.rolloverAnimation != null)
                .SelectMany(system => system.defines.Where(d => d.locomotionTypeValue / 10 == (int)PosingSystem.LocomotionTypeGroup.SitSleep)
                    .SelectMany(d => d.animations).Where(a => a.enabled && !a.isRollover)
                    .Select(pose => (system, pose))).ToArray();
            if (legacyPoses.Length == 0) return;
            var controller = CreateFeatureController(ps, "USSPS_RolloverController", VRCAvatarDescriptor.AnimLayerType.Gesture, saver);
            ps.GetComponentsInChildren<nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator>()
                .Single(m => m.animator == controller).matchAvatarWriteDefaults = true;
            FeatureParameter(controller, PosingSystemFeatureUtility.RollActive, AnimatorControllerParameterType.Bool);
            FeatureParameter(controller, PosingSystemFeatureUtility.RollSensor, AnimatorControllerParameterType.Float, .5f);
            FeatureParameter(controller, PosingSystemFeatureUtility.RollTime, AnimatorControllerParameterType.Float, .5f);
            FeatureParameter(controller, "HeadTrackingLock", AnimatorControllerParameterType.Bool);
            FeatureParameter(controller, PosingSystemFeatureUtility.PhotoActive, AnimatorControllerParameterType.Bool);
            var mappingLayer = FeatureLayer(controller, "寝返りの傾き計算", AvatarMaskBodyPart.Root, false, saver);
            MaskRolloverCalculation(mappingLayer);
            var settings = systems.First(p => p.features.rollover).features;
            var mapping = CreateRolloverMapping(controller, settings, saver);
            var calculate = mappingLayer.stateMachine.AddState("頭の傾きを計算");
            calculate.motion = mapping;
            calculate.writeDefaultValues = false;
            PersistFeatureStates(mappingLayer, controller, saver);

            var poseLayer = FeatureLayer(controller, "寝返りの姿勢", AvatarMaskBodyPart.Root, false, saver);
            for (var part = 0; part < (int)AvatarMaskBodyPart.LastBodyPart; part++)
                poseLayer.avatarMask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)part, part != (int)AvatarMaskBodyPart.Head);
            poseLayer.defaultWeight = 0;
            var idle = poseLayer.stateMachine.AddState("無効");
            idle.writeDefaultValues = false;
            SetWeight(idle, 0);
            var maxPose = GetMaxSyncedParameterValue(ps.GetAvatar());
            foreach (var (system, pose) in legacyPoses)
            {
                var state = poseLayer.stateMachine.AddState("寝返り_" + pose.syncdParameterValue);
                state.motion = system.features.rolloverAnimation;
                state.timeParameterActive = true;
                state.timeParameter = PosingSystemFeatureUtility.RollTime;
                state.writeDefaultValues = false;
                SetWeight(state, 1);
                var enter = FeatureTransition(idle.AddTransition(state));
                enter.AddCondition(AnimatorConditionMode.If, 0, PosingSystemFeatureUtility.RollActive);
                enter.AddCondition(AnimatorConditionMode.IfNot, 0, "HeadTrackingLock");
                enter.AddCondition(AnimatorConditionMode.IfNot, 0, PosingSystemFeatureUtility.PhotoActive);
                FeatureTransition(state.AddTransition(idle)).AddCondition(AnimatorConditionMode.IfNot, 0, PosingSystemFeatureUtility.RollActive);
                FeatureTransition(state.AddTransition(idle)).AddCondition(AnimatorConditionMode.If, 0, "HeadTrackingLock");
                FeatureTransition(state.AddTransition(idle)).AddCondition(AnimatorConditionMode.If, 0, PosingSystemFeatureUtility.PhotoActive);
                foreach (var parameter in SplitSyncParameters(pose.syncdParameterValue, maxPose).Where(p => p.sync))
                {
                    FeatureParameter(controller, parameter.name, parameter.aType);
                    var condition = parameter.aType == AnimatorControllerParameterType.Bool
                        ? (parameter.value != 0 ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot) : AnimatorConditionMode.Equals;
                    enter.AddCondition(condition, parameter.value, parameter.name);
                    var opposite = condition == AnimatorConditionMode.If ? AnimatorConditionMode.IfNot :
                        condition == AnimatorConditionMode.IfNot ? AnimatorConditionMode.If : AnimatorConditionMode.NotEqual;
                    FeatureTransition(state.AddTransition(idle)).AddCondition(opposite, parameter.value, parameter.name);
                }
            }
            PersistFeatureStates(poseLayer, controller, saver);
            var layers = controller.layers;
            layers[1] = poseLayer;
            controller.layers = layers;

            void SetWeight(AnimatorState state, float weight)
            {
                var control = state.AddStateMachineBehaviour<VRCAnimatorLayerControl>();
                control.playable = VRC_AnimatorLayerControl.BlendableLayer.Gesture;
                control.layer = 1;
                control.goalWeight = weight;
                control.blendDuration = .12f;
            }
        }

        private static void MaskRolloverCalculation(AnimatorControllerLayer layer)
        {
            // This layer animates only a float parameter, never the body or fingers.
            for (var part = 0; part < (int)AvatarMaskBodyPart.LastBodyPart; part++)
                layer.avatarMask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)part, false);
        }
    }
}
#endif
