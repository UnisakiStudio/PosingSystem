#if NDMF && MODULAR_AVATAR
using System.Collections.Generic;
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
        public const string FaceForwardLayer = "USSPS_FaceForward";
        public const string PhotoMoveLayer = "USSPS_PhotoMove";
        public const string PhotoRotateLayer = "USSPS_PhotoRotate";

        private static string FeatureSettingsSignature(PosingSystem ps)
        {
            var settings = JsonUtility.FromJson<PosingSystemFeatureSettings>(JsonUtility.ToJson(ps.features ?? new PosingSystemFeatureSettings()));
            settings.ApplyFinalizedMovementSettings();
            settings.rolloverAnimation = null; // Use the asset GUID and content hash, never a transient instance ID.
            // Regenerate comparison-era prebuilds, including ones which already selected E.
            return "movement-final-v6:" + JsonUtility.ToJson(settings);
        }

        public static void ConvertOptionalFeatures(PosingSystem ps, IAssetSaver saver = null)
        {
            var merge = GetPrebuiltMergeAnimator(ps) ?? GetCommonMergeAnimator(ps);
            if (merge?.animator is not AnimatorController controller) return;
            // NDMF also persists its build-owned controller copies under Packages/.
            // Use the current build's ownership information before rejecting package assets.
            if (AssetDatabase.GetAssetPath(controller).StartsWith("Packages/") &&
                !(saver?.IsTemporaryAsset(controller) ?? false))
                throw new System.InvalidOperationException("先にアバター用Controllerを複製してください。配布元のControllerには追加機能を書き込みません。");
            var systems = ps.GetAvatar().GetActiveAvatarChildren<PosingSystem>()
                .Where(p => p.tag != "EditorOnly").ToArray();
            foreach (var system in systems) PosingSystemFeatureUtility.Migrate(system);
            var main = controller.layers.FirstOrDefault(l => l.name == "USSPS_Locomotion");
            if (main == null) return;
            ConfigureAvatarSelectionEntry(main.stateMachine);
            ConfigureSleepJumpSwitch(controller, systems.Any(p => p.disableSleepJumpSwitch));
            foreach (var state in AllFeatureStates(main.stateMachine))
            {
                if (state.mirrorParameter == "USSPS_Mirror") state.mirrorParameterActive = systems.Any(p => p.features.mirror);
                if (!systems.Any(p => p.features.mirror))
                {
                    state.mirrorParameterActive = false;
                    state.mirror = false;
                }
                if (!systems.Any(p => p.features.footHeight)) state.motion = WithoutFootHeight(state.motion);
            }
            // Rebuild only our generated layers. Never modify a distributed controller asset.
            controller.layers = controller.layers.Where(l => l.name != FaceForwardLayer && l.name != "USSPS_FaceForwardSwitch" &&
                 !l.name.StartsWith(PhotoMoveLayer) && l.name != PhotoRotateLayer && l.name != "USSPS_PhotoReset" && l.name != "USSPS_Floating" && l.name != FootHeightAdditiveLayer && l.name != RolloverTimeLayer && !l.name.StartsWith(PoseSpaceMovementPrefix) && !l.name.StartsWith(FloatingPoseSpacePrefix)).ToArray();
            foreach (var item in ps.GetAvatar().GetComponentsInChildren<Transform>(true).Where(t =>
                t.name == PosingSystemHeadSensorUtility.SensorRoot || t.name == "USSPS_HeadDirectionProbe" ||
                t.name == "USSPS_PhotoTracking" || t.name == "USSPS_FloatActivation" || t.name == "USSPS_RolloverController" ||
                t.name == "USSPS_PhotoMotion" || t.name == "USSPS_FloatMotion").ToArray())
                if (item != null) Object.DestroyImmediate(item.gameObject);
            var rolloverSystem = systems.FirstOrDefault(p => p.features.rollover);
            if (rolloverSystem != null)
                PosingSystemHeadSensorUtility.Create(ps.GetAvatar().GetComponent<Animator>(), rolloverSystem.transform);
            var floatingSystem = systems.FirstOrDefault(p => p.features.floating);
            if (floatingSystem != null) BuildFloating(floatingSystem,
                MovementController(floatingSystem, controller, "USSPS_FloatMotion", floatingSystem.features.floatingBackend, saver), saver);
            if (systems.Any(p => p.features.rollover))
            {
                BuildRollover(ps, systems, saver);
                if (systems.SelectMany(p => p.defines).SelectMany(d => d.animations).Any(a => a.enabled && a.isRollover))
                    BuildRolloverTime(controller, systems.First(p => p.features.rollover).features, saver);
            }
            if (systems.Any(p => p.features.faceForward)) BuildFaceForward(ps, controller, main, systems, saver);
            if (UsesAdditiveFootHeight(systems)) BuildAdditiveFootHeight(controller, main, saver);
            var photoSystem = systems.FirstOrDefault(p => p.features.photography);
            if (photoSystem != null) BuildPhotography(photoSystem,
                MovementController(photoSystem, controller, "USSPS_PhotoMotion", photoSystem.features.photographyBackend, saver), saver);
            BuildPoseSpaceMovement(ps, controller, main, photoSystem, floatingSystem, saver);
            if (floatingSystem != null) BuildFloatingPoseSpace(floatingSystem, controller, main, saver);
            EditorUtility.SetDirty(controller);
            if (EditorUtility.IsPersistent(controller)) RemoveOldSubAssets(controller);
        }

        internal const string AvatarSelectionFallback = "USSPS_AfterAvatarSelection";

        private static void ConfigureAvatarSelectionEntry(AnimatorStateMachine machine)
        {
            var select = machine.states.FirstOrDefault(child => child.state.name == "AvatarSelecting").state;
            if (select == null) return;
            var entry = machine.entryTransitions.FirstOrDefault(t => t.destinationState == select &&
                t.conditions.Length == 1 && t.conditions[0].parameter == "TrackingType" &&
                t.conditions[0].mode == AnimatorConditionMode.Equals && t.conditions[0].threshold == 0);
            if (entry == null)
            {
                entry = machine.AddEntryTransition(select);
                entry.AddCondition(AnimatorConditionMode.Equals, 0, "TrackingType");
            }
            var fallback = machine.entryTransitions.FirstOrDefault(t => t.name == AvatarSelectionFallback);
            var previousDefault = machine.defaultState;
            if (fallback == null && previousDefault != null && previousDefault != select)
            {
                // Keep the previous fallback when tracking has initialized and no pose matches.
                fallback = machine.AddEntryTransition(previousDefault);
                fallback.name = AvatarSelectionFallback;
                fallback.AddCondition(AnimatorConditionMode.NotEqual, 0, "TrackingType");
            }
            var entries = machine.entryTransitions.Where(t => t != entry && t != fallback).Prepend(entry);
            machine.entryTransitions = (fallback != null ? entries.Append(fallback) : entries).ToArray();
            machine.defaultState = select;
        }

        // All pose tools on the avatar share one sleep selector.
        private static void ConfigureSleepJumpSwitch(AnimatorController controller, bool disabled)
        {
            var layer = controller.layers.FirstOrDefault(l => l.name == "USSPS_SitSleepSwitch");
            if (layer == null) return;
            foreach (var child in layer.stateMachine.states)
            {
                var state = child.state;
                if (state.name != "SitSleepUp" && state.name != "SitSleepDown") continue;
                var target = state.name == "SitSleepUp" ? "SitSleepDown" : "SitSleepUp";
                foreach (var jump in state.transitions.Where(t => t.destinationState?.name == target + "_Judge"))
                {
                    // An existing prebuild can still contain the 3.1.0 guard on the source pose.
                    var conditions = jump.conditions;
                    for (var i = 0; i < conditions.Length; i++)
                        if (conditions[i].parameter == state.name && conditions[i].mode == AnimatorConditionMode.NotEqual &&
                            conditions[i].threshold == 0)
                            conditions[i].parameter = target;
                    jump.conditions = conditions;
                    jump.mute = disabled;
                    EditorUtility.SetDirty(jump);
                }
            }
        }

        private static IEnumerable<AnimatorState> AllFeatureStates(AnimatorStateMachine machine) =>
            machine.states.Select(s => s.state).Concat(machine.stateMachines.SelectMany(m => AllFeatureStates(m.stateMachine)));

        private static Motion WithoutFootHeight(Motion motion)
        {
            if (motion is BlendTree tree && tree.blendParameter == "FootHeight")
                return tree.children.OrderBy(c => Mathf.Abs(c.threshold - .5f)).FirstOrDefault().motion;
            return motion;
        }

        private static AvatarMask FeatureMask(string name, AvatarMaskBodyPart part)
        {
            var mask = new AvatarMask { name = name };
            for (var i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
                mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, part == AvatarMaskBodyPart.Root || (AvatarMaskBodyPart)i == part);
            // Root translation must include all humanoid parts; excluded limbs retain their previous world targets.
            // These additive clips have zero muscle deltas, so the original pose is preserved.
            // An empty transform mask permits every non-Humanoid transform. In a synced
            // additive layer, an unanimated hair/accessory rotation can then be accumulated
            // every frame (especially when another state in Base binds that transform).
            // These features only animate Humanoid properties and Animator parameters.
            mask.transformCount = 1;
            mask.SetTransformPath(0, "");
            mask.SetTransformActive(0, false);
            return mask;
        }

        private static void FeatureParameter(AnimatorController controller, string name,
            AnimatorControllerParameterType type, float value = 0)
        {
            if (controller.parameters.Any(p => p.name == name)) return;
            controller.AddParameter(new AnimatorControllerParameter { name = name, type = type,
                defaultFloat = value, defaultBool = value != 0, defaultInt = (int)value });
        }

        private static AnimatorControllerLayer FeatureLayer(AnimatorController controller, string name,
            AvatarMaskBodyPart part, bool additive, IAssetSaver saver)
        {
            var layer = new AnimatorControllerLayer { name = name, defaultWeight = 1,
                blendingMode = additive ? AnimatorLayerBlendingMode.Additive : AnimatorLayerBlendingMode.Override,
                avatarMask = FeatureMask(name + "Mask", part), stateMachine = new AnimatorStateMachine { name = name } };
            SaveGeneratedAsset(layer.avatarMask, controller, saver);
            SaveGeneratedAsset(layer.stateMachine, controller, saver);
            controller.AddLayer(layer);
            return layer;
        }

        private static void BuildFaceForward(PosingSystem ps, AnimatorController controller,
            AnimatorControllerLayer main, PosingSystem[] systems, IAssetSaver saver)
        {
            const string blendParameter = "USSPS_FaceForwardBlend";
            FeatureParameter(controller, "HeadTrackingLock", AnimatorControllerParameterType.Bool);
            FeatureParameter(controller, "LocomotionType", AnimatorControllerParameterType.Int);
            FeatureParameter(controller, blendParameter, AnimatorControllerParameterType.Float, 1);
            var switchLayer = FeatureLayer(controller, "USSPS_FaceForwardSwitch", AvatarMaskBodyPart.Head, false, saver);
            var off = switchLayer.stateMachine.AddState("元の顔の向き");
            var on = switchLayer.stateMachine.AddState("正面に補正");
            switchLayer.stateMachine.defaultState = on;
            foreach (var state in new[] { off, on })
            {
                state.writeDefaultValues = false;
                var control = new AnimationClip { name = state.name };
                AnimationUtility.SetEditorCurve(control, EditorCurveBinding.FloatCurve("", typeof(Animator), blendParameter),
                    AnimationCurve.Constant(0, 1, state == on ? 1 : 0));
                SaveGeneratedAsset(control, controller, saver);
                state.motion = control;
            }
            FeatureTransition(on.AddTransition(off)).AddCondition(AnimatorConditionMode.If, 0, "HeadTrackingLock");
            var unlocked = FeatureTransition(off.AddTransition(on));
            unlocked.AddCondition(AnimatorConditionMode.IfNot, 0, "HeadTrackingLock");
            // The existing AutoLockHead controller identifies supine poses as LocomotionType 32.
            // Their original upward-facing head must survive even when manual HeadTrackingLock is false.
            FeatureTransition(on.AddTransition(off)).AddCondition(AnimatorConditionMode.Equals, 32, "LocomotionType");
            unlocked.AddCondition(AnimatorConditionMode.NotEqual, 32, "LocomotionType");
            if (systems.Any(p => p.features.photography))
            {
                FeatureParameter(controller, PosingSystemFeatureUtility.PhotoActive, AnimatorControllerParameterType.Bool);
                FeatureTransition(on.AddTransition(off)).AddCondition(AnimatorConditionMode.If, 0, PosingSystemFeatureUtility.PhotoActive);
                unlocked.AddCondition(AnimatorConditionMode.IfNot, 0, PosingSystemFeatureUtility.PhotoActive);
            }
            PersistFeatureStates(switchLayer, controller, saver);
            var layer = FeatureLayer(controller, FaceForwardLayer, AvatarMaskBodyPart.Head, false, saver);
            layer.syncedLayerIndex = System.Array.FindIndex(controller.layers, l => l.name == main.name);
            layer.syncedLayerAffectsTiming = false;
            var clone = Object.Instantiate(ps.GetAvatar().gameObject, PosingSystemEditor.GetPreviewAvatarRoot());
            clone.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                var animator = clone.GetComponent<Animator>();
                var cache = new Dictionary<Motion, Motion>();
                foreach (var system in systems.Where(p => p.features.faceForward))
                foreach (var definition in system.defines.Where(d => d.locomotionTypeValue != 32).SelectMany(d => d.animations).Where(a => a.enabled))
                {
                    var values = SplitSyncParameters(definition.syncdParameterValue, GetMaxSyncedParameterValue(ps.GetAvatar()));
                    foreach (var vr in new[] { true, false })
                    {
                        var original = main.stateMachine.entryTransitions.FirstOrDefault(t =>
                            CompareTransitionConditions(t.conditions, GetEntryTransitionConditions(values, vr)))?.destinationState;
                        if (original?.motion == null) continue;
                        var source = WithoutFootHeight(original.motion);
                        var blend = new BlendTree { name = "Face_" + definition.syncdParameterValue,
                            blendParameter = blendParameter, blendType = BlendTreeType.Simple1D, useAutomaticThresholds = false };
                        blend.AddChild(source, 0);
                        blend.AddChild(CorrectMotion(source), 1);
                        SaveGeneratedAsset(blend, controller, saver);
                        layer.SetOverrideMotion(original, blend);
                    }
                }
                Motion CorrectMotion(Motion source)
                {
                    if (source == null) return null;
                    if (cache.TryGetValue(source, out var existing)) return existing;
                    Motion result;
                    if (source is AnimationClip clip) result = PosingSystemFaceForwardUtility.Create(animator, clip);
                    else if (source is BlendTree tree)
                    {
                        var copy = Object.Instantiate(tree);
                        cache[source] = copy;
                        copy.children = tree.children.Select(child => { child.motion = CorrectMotion(child.motion); return child; }).ToArray();
                        result = copy;
                    }
                    else return source;
                    cache[source] = result;
                    SaveGeneratedAsset(result, controller, saver);
                    return result;
                }
            }
            finally { Object.DestroyImmediate(clone); }
            var layers = controller.layers;
            layers[layers.Length - 1] = layer;
            controller.layers = layers;
        }

        private static AnimatorStateTransition FeatureTransition(AnimatorStateTransition transition)
        {
            transition.hasExitTime = false;
            transition.duration = 0;
            transition.canTransitionToSelf = false;
            return transition;
        }

        public static AnimationClip RootOffsetClip(Vector3 translation, Quaternion rotation, string name)
        {
            var clip = new AnimationClip { name = name };
            // RootT/RootQ alone import as generic curves. A neutral jaw channel marks the clip humanoid;
            // the same zero marker in the reference pose gives an additive delta of zero.
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), "Jaw Left-Right"),
                AnimationCurve.Constant(0, 1, 0));
            for (var axis = 0; axis < 3; axis++)
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT." + "xyz"[axis]),
                    AnimationCurve.Constant(0, 1, translation[axis]));
            for (var axis = 0; axis < 4; axis++)
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), "RootQ." + "xyzw"[axis]),
                    AnimationCurve.Constant(0, 1, rotation[axis]));
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.keepOriginalOrientation = true;
            settings.keepOriginalPositionXZ = true;
            settings.keepOriginalPositionY = true;
            settings.loopBlendOrientation = true;
            settings.loopBlendPositionY = true;
            settings.loopBlendPositionXZ = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;
        }

        private static void BuildPhotography(PosingSystem ps, AnimatorController controller, IAssetSaver saver)
        {
            FeatureParameter(controller, PosingSystemFeatureUtility.PhotoActive, AnimatorControllerParameterType.Bool);
            FeatureParameter(controller, PosingSystemFeatureUtility.PhotoDistance, AnimatorControllerParameterType.Float);
            FeatureParameter(controller, PosingSystemFeatureUtility.PhotoYaw, AnimatorControllerParameterType.Float, .5f);
            FeatureParameter(controller, "USSPS_PhotoReset", AnimatorControllerParameterType.Bool);
            FeatureParameter(controller, "LocomotionLock", AnimatorControllerParameterType.Bool);
            if (ps.features.photographyBackend != PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive)
            {
                BuildPhotographyInputs(controller, saver, false);
                MeasureRootTranslationUnits(ps.GetAvatar().gameObject, out var units);
                var neutral = RootOffsetClip(Vector3.zero, Quaternion.identity, "Photo_Zero");
                SaveGeneratedAsset(neutral, controller, saver);
                // Combine position and yaw: separate Override layers would erase each other in backend B.
                var distance = new BlendTree { name = "PhotoDistanceAndYaw", blendParameter = PosingSystemFeatureUtility.PhotoDistance,
                    blendType = BlendTreeType.Simple1D, useAutomaticThresholds = false };
                foreach (var offset in new[] { 0f, 1f })
                {
                    var yaw = new BlendTree { name = "PhotoYawAtDistance_" + offset, blendParameter = MovementYaw,
                        blendType = BlendTreeType.Simple1D, useAutomaticThresholds = false };
                    foreach (var value in MovementYawSamples)
                    {
                        var clip = RootOffsetClip(Vector3.forward * offset * 3 / Mathf.Max(.001f, Mathf.Abs(units.z)),
                            Quaternion.Euler(0, (value - .5f) * 360, 0), "Photo_" + offset + "_" + value);
                        AnimationUtility.SetAdditiveReferencePose(clip, neutral, 0);
                        SaveGeneratedAsset(clip, controller, saver);
                        yaw.AddChild(clip, value);
                    }
                    SaveGeneratedAsset(yaw, controller, saver);
                    distance.AddChild(yaw, offset);
                }
                SaveGeneratedAsset(distance, controller, saver);
                AddFeatureMotion(controller, PhotoMoveLayer, distance, neutral, PosingSystemFeatureUtility.PhotoActive, saver,
                    ps.features.photographyBackend != PosingSystemFeatureSettings.MovementBackend.AdditiveOverride, ps.features.movementWriteDefaults);
            }

            var resetLayer = FeatureLayer(controller, "USSPS_PhotoReset", AvatarMaskBodyPart.Root, false, saver);
            var idle = resetLayer.stateMachine.AddState("待機");
            var reset = resetLayer.stateMachine.AddState("リセット");
            idle.writeDefaultValues = reset.writeDefaultValues = false;
            var driver = reset.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            driver.localOnly = true;
            driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter { name = PosingSystemFeatureUtility.PhotoDistance, value = 0 });
            driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter { name = PosingSystemFeatureUtility.PhotoYaw, value = .5f });
            driver.parameters.Add(new VRC_AvatarParameterDriver.Parameter { name = "LocomotionLock", value = 0 });
            FeatureTransition(idle.AddTransition(reset)).AddCondition(AnimatorConditionMode.If, 0, "USSPS_PhotoReset");
            FeatureTransition(reset.AddTransition(idle)).AddCondition(AnimatorConditionMode.IfNot, 0, "USSPS_PhotoReset");
            PersistFeatureStates(resetLayer, controller, saver);

            // A separate tracking request participates in the existing request merger. It never overwrites saved locks.
            var fx = CreateFeatureController(ps, "USSPS_PhotoTracking", VRCAvatarDescriptor.AnimLayerType.FX, saver);
            FeatureParameter(fx, PosingSystemFeatureUtility.PhotoActive, AnimatorControllerParameterType.Bool);
            var layer = FeatureLayer(fx, "Photography tracking", AvatarMaskBodyPart.Root, false, saver);
            var off = layer.stateMachine.AddState("通常");
            var on = layer.stateMachine.AddState("撮影中");
            off.writeDefaultValues = on.writeDefaultValues = false;
            foreach (var state in new[] { off, on })
            {
                var tracking = state.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
                var mode = state == on ? VRC_AnimatorTrackingControl.TrackingType.Animation : VRC_AnimatorTrackingControl.TrackingType.Tracking;
                tracking.trackingHead = tracking.trackingLeftHand = tracking.trackingRightHand = mode;
                // Keep manual foot/hip locks and locomotion under the user's control while placing the avatar.
                tracking.trackingHip = tracking.trackingLeftFoot = tracking.trackingRightFoot = VRC_AnimatorTrackingControl.TrackingType.NoChange;
            }
            FeatureParameter(fx, PosingSystemFeatureUtility.PhotoDistance, AnimatorControllerParameterType.Float);
            FeatureParameter(fx, PosingSystemFeatureUtility.PhotoYaw, AnimatorControllerParameterType.Float, .5f);
            // Both clients derive the mode from the two synced values; mode itself consumes no sync bit.
            // The deadband includes the nearest quantized representations of the middle of the radial.
            foreach (var state in new[] { off, on })
            {
                var modeDriver = state.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
                modeDriver.localOnly = false;
                modeDriver.parameters.Add(new VRC_AvatarParameterDriver.Parameter { name = PosingSystemFeatureUtility.PhotoActive, value = state == on ? 1 : 0 });
            }
            FeatureTransition(off.AddTransition(on)).AddCondition(AnimatorConditionMode.Greater, PosingSystemFeatureUtility.PhotoDistanceThreshold, PosingSystemFeatureUtility.PhotoDistance);
            FeatureTransition(off.AddTransition(on)).AddCondition(AnimatorConditionMode.Less, .495f, PosingSystemFeatureUtility.PhotoYaw);
            FeatureTransition(off.AddTransition(on)).AddCondition(AnimatorConditionMode.Greater, .505f, PosingSystemFeatureUtility.PhotoYaw);
            var leave = FeatureTransition(on.AddTransition(off));
            leave.AddCondition(AnimatorConditionMode.Less, PosingSystemFeatureUtility.PhotoDistanceThreshold, PosingSystemFeatureUtility.PhotoDistance);
            leave.AddCondition(AnimatorConditionMode.Greater, .495f, PosingSystemFeatureUtility.PhotoYaw);
            leave.AddCondition(AnimatorConditionMode.Less, .505f, PosingSystemFeatureUtility.PhotoYaw);
            PersistFeatureStates(layer, fx, saver);

        }

        private static AnimatorControllerLayer AddFeatureMotion(AnimatorController controller, string name, Motion motion,
            AnimationClip neutral, string enabled, IAssetSaver saver, bool additive = true, bool writeDefaults = true)
        {
            var layer = FeatureLayer(controller, name, AvatarMaskBodyPart.Root, additive, saver);
            var off = layer.stateMachine.AddState("無効");
            var on = layer.stateMachine.AddState("有効");
            off.motion = neutral;
            on.motion = motion;
            off.writeDefaultValues = on.writeDefaultValues = writeDefaults;
            off.iKOnFeet = on.iKOnFeet = false;
            FeatureTransition(off.AddTransition(on)).AddCondition(AnimatorConditionMode.If, 0, enabled);
            FeatureTransition(on.AddTransition(off)).AddCondition(AnimatorConditionMode.IfNot, 0, enabled);
            PersistFeatureStates(layer, controller, saver);
            return layer;
        }

        private static AnimatorController MovementController(PosingSystem ps, AnimatorController baseController, string name,
            PosingSystemFeatureSettings.MovementBackend backend, IAssetSaver saver)
        {
            if (backend == PosingSystemFeatureSettings.MovementBackend.BaseAdditive ||
                backend == PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive) return baseController;
            var type = backend == PosingSystemFeatureSettings.MovementBackend.GestureAdditive
                ? VRCAvatarDescriptor.AnimLayerType.Gesture : VRCAvatarDescriptor.AnimLayerType.Additive;
            var controller = CreateFeatureController(ps, name, type, saver);
            // Keep motion layers above layer 0, whose weight and mask have special VRChat semantics.
            var seed = FeatureLayer(controller, name + "_Reference", AvatarMaskBodyPart.Root, false, saver);
            var state = seed.stateMachine.AddState("基準");
            state.writeDefaultValues = false;
            var clip = new AnimationClip { name = name + "_NoOp" };
            FeatureParameter(controller, "USSPS_InternalNoOp", AnimatorControllerParameterType.Float);
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), "USSPS_InternalNoOp"), AnimationCurve.Constant(0, 1, 0));
            state.motion = clip;
            SaveGeneratedAsset(clip, controller, saver);
            PersistFeatureStates(seed, controller, saver);
            return controller;
        }

        private static AnimatorController CreateFeatureController(PosingSystem ps, string name,
            VRCAvatarDescriptor.AnimLayerType type, IAssetSaver saver)
        {
            var obj = new GameObject(name);
            obj.transform.SetParent(ps.transform, false);
            var merge = obj.AddComponent<nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator>();
            merge.layerType = type;
            if (type == VRCAvatarDescriptor.AnimLayerType.FX) merge.matchAvatarWriteDefaults = true;
            var controller = new AnimatorController { name = name };
            merge.animator = controller;
            if (saver != null) saver.SaveAsset(controller);
            else
            {
                var directory = PosingSystemEditor.GetGeneratedFolderPath();
                var path = AssetDatabase.GenerateUniqueAssetPath(directory + "/" + name + ".controller");
                AssetDatabase.CreateAsset(controller, path);
            }
            return controller;
        }

        private static void PersistFeatureStates(AnimatorControllerLayer layer, AnimatorController controller, IAssetSaver saver)
        {
            AnimationClip noOp = null;
            foreach (var state in AllFeatureStates(layer.stateMachine))
            {
                // VRChat treats None/empty motions as Write Defaults On, even when the checkbox is off.
                if (state.motion == null)
                {
                    if (noOp == null)
                    {
                        FeatureParameter(controller, "USSPS_InternalNoOp", AnimatorControllerParameterType.Float);
                        noOp = new AnimationClip { name = layer.name + "_NoOp" };
                        AnimationUtility.SetEditorCurve(noOp, EditorCurveBinding.FloatCurve("", typeof(Animator), "USSPS_InternalNoOp"), AnimationCurve.Constant(0, 1, 0));
                        SaveGeneratedAsset(noOp, controller, saver);
                    }
                    state.motion = noOp;
                }
                SaveGeneratedAsset(state, controller, saver);
                foreach (var behaviour in state.behaviours) SaveGeneratedAsset(behaviour, controller, saver);
                foreach (var transition in state.transitions) SaveGeneratedAsset(transition, controller, saver);
            }
            foreach (var transition in layer.stateMachine.anyStateTransitions) SaveGeneratedAsset(transition, controller, saver);
        }
    }
}
#endif
