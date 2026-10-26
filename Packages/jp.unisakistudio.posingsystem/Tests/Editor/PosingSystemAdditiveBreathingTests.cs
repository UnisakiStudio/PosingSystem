using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemAdditiveBreathingTests
    {
        private const string LocomotionControllerGuid = "44c97b69ea5e25340aabee19781f30ad";
        private const string SettingsPrefabGuid = "ecde0b1d67f9e854992715d64f778d8c";
        private const string KawaiiEightBitPrefabGuid = "27ac1b7ee5664254c83f7f21d620a049";
        private const string CoolEightBitPrefabGuid = "e21117159ebf40209c13ef86596f2a15";

        [Test]
        public void Converter_UsesSynchronizedPoseAndGameToggleInDedicatedAdditiveController()
        {
            var temporaryControllerPath = AssetDatabase.GenerateUniqueAssetPath(
                "Assets/USSPS_AdditiveBreathingTest.controller");
            var controllerPath = AssetDatabase.GUIDToAssetPath(LocomotionControllerGuid);
            Assert.That(AssetDatabase.CopyAsset(controllerPath, temporaryControllerPath), Is.True);
            var temporaryAdditiveControllerPath = AssetDatabase.GenerateUniqueAssetPath(
                "Assets/USSPS_DedicatedAdditiveBreathingTest.controller");
            var defaultAdditiveControllerPath = AssetDatabase.GUIDToAssetPath(
                PosingSystemConverter.DefaultAdditiveBreathingControllerGuid);
            Assert.That(
                AssetDatabase.CopyAsset(defaultAdditiveControllerPath, temporaryAdditiveControllerPath),
                Is.True);

            var avatarObject = new GameObject("AdditiveBreathingTestAvatar");
            try
            {
                avatarObject.AddComponent<VRCAvatarDescriptor>();
                var prefab = LoadAssetByGuid<GameObject>(SettingsPrefabGuid);
                var settingsObject = Object.Instantiate(prefab, avatarObject.transform);
                var posingSystem = settingsObject.GetComponentInChildren<PosingSystem>(true);
                // This test specifically exercises the optional in-game breathing toggle.
                posingSystem.features ??= new PosingSystemFeatureSettings();
                posingSystem.features.breathing = true;
                posingSystem.features.breathingControl = true;
                posingSystem.additiveBreathing.enabled = true;
                posingSystem.additiveBreathing.standingLevel = PosingSystem.BreathingLevel.Rapid;
                posingSystem.defines = new List<PosingSystem.LayerDefine>
                {
                    CreateLayerDefineWithSyncedPose("立ち姿勢", 1, 4),
                    CreateLayerDefineWithSyncedPose("中腰姿勢", 12, 260),
                    CreateLayerDefineWithSyncedPose("座り姿勢", 22, 513),
                };
                var locomotionDuplicateEraser = settingsObject.GetComponentsInChildren<DuplicateEraser>(true)
                    .Single(component => component.ID == "jp.unisakistudio.posingsystem_locomotion");
                var locomotionMergeAnimator =
                    locomotionDuplicateEraser.GetComponent<ModularAvatarMergeAnimator>();
                var locomotionController =
                    AssetDatabase.LoadAssetAtPath<AnimatorController>(temporaryControllerPath);
                locomotionMergeAnimator.animator = locomotionController;

                var additiveDuplicateEraser = settingsObject.GetComponentsInChildren<DuplicateEraser>(true)
                    .Single(component =>
                        component.ID == PosingSystemConverter.AdditiveBreathingDuplicateEraserId);
                var additiveMergeAnimator = additiveDuplicateEraser.GetComponent<ModularAvatarMergeAnimator>();
                // The settings prefab disables this optional child with EditorOnly. Activate the
                // copied child before conversion so recovery cannot select the package template.
                additiveDuplicateEraser.gameObject.tag = "Untagged";
                additiveDuplicateEraser.gameObject.SetActive(true);
                var additiveController =
                    AssetDatabase.LoadAssetAtPath<AnimatorController>(temporaryAdditiveControllerPath);
                additiveMergeAnimator.animator = additiveController;

                Assert.That(PosingSystemConverter.GetAdditiveBreathingMergeAnimator(posingSystem, true),
                    Is.SameAs(additiveMergeAnimator));
                Assert.That(PosingSystemFeatureUtility.AvatarUsesBreathingControl(posingSystem), Is.True);

                PosingSystemConverter.ConvertAdditiveBreathingAnimations(posingSystem);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(
                    temporaryAdditiveControllerPath,
                    ImportAssetOptions.ForceUpdate | ImportAssetOptions.ForceSynchronousImport);
                additiveController =
                    AssetDatabase.LoadAssetAtPath<AnimatorController>(temporaryAdditiveControllerPath);

                Assert.That(
                    locomotionController.layers.Any(candidate =>
                        candidate.name == PosingSystemConverter.AdditiveBreathingLayerName),
                    Is.False);
                Assert.That(additiveMergeAnimator.layerType, Is.EqualTo(VRCAvatarDescriptor.AnimLayerType.Additive));
                Assert.That(additiveMergeAnimator.mergeAnimatorMode, Is.EqualTo(MergeAnimatorMode.Append));
                Assert.That(additiveMergeAnimator.pathMode, Is.EqualTo(MergeAnimatorPathMode.Absolute));

                var layer = additiveController.layers.Single();
                Assert.That(layer.name, Is.EqualTo(PosingSystemConverter.AdditiveBreathingLayerName));
                Assert.That(layer.blendingMode, Is.EqualTo(AnimatorLayerBlendingMode.Additive));
                Assert.That(layer.defaultWeight, Is.EqualTo(1f));
                Assert.That(layer.stateMachine.states.Length, Is.EqualTo(7));
                Assert.That(layer.stateMachine.anyStateTransitions.Length, Is.EqualTo(8));

                var breathingParameter = additiveController.parameters.Single(parameter =>
                    parameter.name == PosingSystemConverter.AdditiveBreathingEnabledParameterName);
                Assert.That(breathingParameter.type, Is.EqualTo(AnimatorControllerParameterType.Bool));
                Assert.That(breathingParameter.defaultBool, Is.True);
                Assert.That(additiveController.parameters.Any(parameter =>
                    parameter.name == PosingSystemConverter.AdditiveBreathingPoseParameterName &&
                    parameter.type == AnimatorControllerParameterType.Int), Is.True);
                Assert.That(additiveController.parameters.Any(parameter =>
                    parameter.name == "USSPS_Pose0" &&
                    parameter.type == AnimatorControllerParameterType.Bool), Is.True);
                Assert.That(additiveController.parameters.Any(parameter =>
                    parameter.name == "LocomotionTypeGroup" ||
                    parameter.name == "LocomotionType" ||
                    parameter.name == "USSPS_TypeFix"), Is.False);

                var disabledState = layer.stateMachine.states.Single(child =>
                    child.state.name == PosingSystemConverter.AdditiveBreathingDisabledStateName).state;
                var disabledTransition = layer.stateMachine.anyStateTransitions.Single(candidate =>
                    candidate.destinationState == disabledState);
                Assert.That(disabledTransition.conditions.Single().parameter,
                    Is.EqualTo(PosingSystemConverter.AdditiveBreathingEnabledParameterName));
                Assert.That(disabledTransition.conditions.Single().mode,
                    Is.EqualTo(AnimatorConditionMode.IfNot));

                var resetClip = disabledState.motion as AnimationClip;
                Assert.That(resetClip, Is.Not.Null);
                Assert.That(resetClip.name,
                    Is.EqualTo(PosingSystemConverter.AdditiveBreathingResetClipName));
                var breathingClips = System.Enum
                    .GetValues(typeof(PosingSystem.LocomotionTypeGroup))
                    .Cast<PosingSystem.LocomotionTypeGroup>()
                    .Select(group => PosingSystemConverter.ResolveAdditiveBreathingClip(
                        posingSystem.additiveBreathing, group))
                    .Where(clip => clip != null);
                var expectedResetBindings = breathingClips
                    .SelectMany(AnimationUtility.GetCurveBindings)
                    .Distinct()
                    .ToArray();
                var actualResetBindings = AnimationUtility.GetCurveBindings(resetClip);
                foreach (var expectedBinding in expectedResetBindings)
                {
                    Assert.That(actualResetBindings.Contains(expectedBinding), Is.True,
                        expectedBinding.propertyName);
                    var resetCurve = AnimationUtility.GetEditorCurve(resetClip, expectedBinding);
                    Assert.That(resetCurve.keys.All(key => Mathf.Approximately(key.value, 0f)), Is.True,
                        expectedBinding.propertyName);
                }

                var poseTransitions = layer.stateMachine.anyStateTransitions.Where(candidate =>
                    candidate.destinationState != disabledState).ToArray();
                Assert.That(poseTransitions.All(transition => transition.conditions.Any(condition =>
                    condition.parameter == PosingSystemConverter.AdditiveBreathingEnabledParameterName &&
                    condition.mode == AnimatorConditionMode.If)), Is.True);
                Assert.That(poseTransitions.SelectMany(transition => transition.conditions).Any(condition =>
                    condition.parameter == "LocomotionTypeGroup" ||
                    condition.parameter == "LocomotionType" ||
                    condition.parameter == "USSPS_TypeFix"), Is.False);

                foreach (var expected in new[]
                         {
                             (PoseValue: 0, Group: PosingSystem.LocomotionTypeGroup.Standing),
                             (PoseValue: 1, Group: PosingSystem.LocomotionTypeGroup.SitShallow),
                             (PoseValue: 2, Group: PosingSystem.LocomotionTypeGroup.SitDeep),
                             (PoseValue: 3, Group: PosingSystem.LocomotionTypeGroup.SitSleep),
                             (PoseValue: 4, Group: PosingSystem.LocomotionTypeGroup.Standing),
                             (PoseValue: 260, Group: PosingSystem.LocomotionTypeGroup.SitShallow),
                             (PoseValue: 513, Group: PosingSystem.LocomotionTypeGroup.SitDeep),
                         })
                {
                    var state = layer.stateMachine.states.Single(child =>
                        child.state.name == expected.Group.ToString()).state;
                    Assert.That(state.motion, Is.SameAs(PosingSystemConverter.ResolveAdditiveBreathingClip(
                        posingSystem.additiveBreathing, expected.Group)));
                    var transition = poseTransitions.Single(candidate =>
                        candidate.destinationState == state &&
                        HasSynchronizedPoseConditions(candidate, expected.PoseValue, 513));
                    Assert.That(transition.canTransitionToSelf, Is.False);
                }
            }
            finally
            {
                Object.DestroyImmediate(avatarObject);
                AssetDatabase.DeleteAsset(temporaryControllerPath);
                AssetDatabase.DeleteAsset(temporaryAdditiveControllerPath);
            }
        }

        [Test]
        public void SettingsPrefab_HasSavedSyncedBreathingToggleEnabledByDefault()
        {
            var prefab = LoadAssetByGuid<GameObject>(SettingsPrefabGuid);
            Assert.That(prefab, Is.Not.Null);

            var parameter = prefab.GetComponent<ModularAvatarParameters>().parameters.Single(candidate =>
                candidate.nameOrPrefix == PosingSystemConverter.AdditiveBreathingEnabledParameterName);
            Assert.That(parameter.syncType, Is.EqualTo(ParameterSyncType.Bool));
            Assert.That(parameter.localOnly, Is.False);
            Assert.That(parameter.saved, Is.True);
            Assert.That(parameter.defaultValue, Is.EqualTo(1f));

            var menuItem = prefab.GetComponentsInChildren<ModularAvatarMenuItem>(true).Single(candidate =>
                candidate.Control != null &&
                candidate.Control.type == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.Toggle &&
                candidate.Control.parameter != null &&
                candidate.Control.parameter.name == PosingSystemConverter.AdditiveBreathingEnabledParameterName);
            Assert.That(menuItem.transform.parent.name, Is.EqualTo("設定"));
            Assert.That(menuItem.gameObject.name, Is.EqualTo("呼吸"));
            Assert.That(menuItem.Control.name, Is.EqualTo("呼吸"));
            Assert.That(menuItem.Control.value, Is.EqualTo(1f));
        }

        [TestCase(KawaiiEightBitPrefabGuid)]
        [TestCase(CoolEightBitPrefabGuid)]
        public void EightBitPrefab_CompletelyDisablesBreathingWithoutUsingAnotherSyncedBit(
            string prefabGuid)
        {
            var prefab = LoadAssetByGuid<GameObject>(prefabGuid);
            var posingSystem = prefab.GetComponentInChildren<PosingSystem>(true);
            Assert.That(posingSystem, Is.Not.Null);
            Assert.That(posingSystem.isEightBitVersion, Is.True);
            Assert.That(posingSystem.additiveBreathing.enabled, Is.False);
            var allParameters = prefab.GetComponentsInChildren<ModularAvatarParameters>(true)
                .SelectMany(component => component.parameters)
                .ToArray();
            Assert.That(allParameters.Any(parameter =>
                    parameter.nameOrPrefix == PosingSystemConverter.AdditiveBreathingEnabledParameterName),
                Is.False);
            var synchronizedParameters = allParameters
                .Where(parameter =>
                    !parameter.localOnly && parameter.syncType != ParameterSyncType.NotSynced)
                .ToArray();
            Assert.That(synchronizedParameters.Length, Is.EqualTo(1));
            Assert.That(synchronizedParameters[0].nameOrPrefix,
                Is.EqualTo(PosingSystemConverter.AdditiveBreathingPoseParameterName));
            Assert.That(synchronizedParameters[0].syncType, Is.EqualTo(ParameterSyncType.Int));
            Assert.That(prefab.GetComponentsInChildren<ModularAvatarMenuItem>(true)
                .Any(item => item.Control != null && item.Control.parameter != null &&
                             item.Control.parameter.name ==
                             PosingSystemConverter.AdditiveBreathingEnabledParameterName),
                Is.False);
            Assert.That(prefab.GetComponentsInChildren<DuplicateEraser>(true)
                .Any(component =>
                    component.ID == PosingSystemConverter.AdditiveBreathingDuplicateEraserId),
                Is.False);
        }

        [Test]
        public void EightBitBuildCleanup_RemovesInheritedBreathingComponents()
        {
            var avatarObject = new GameObject("EightBitBreathingCleanupTestAvatar");
            try
            {
                avatarObject.AddComponent<VRCAvatarDescriptor>();
                var settingsObject = Object.Instantiate(
                    LoadAssetByGuid<GameObject>(SettingsPrefabGuid),
                    avatarObject.transform);
                var posingSystem = settingsObject.GetComponentInChildren<PosingSystem>(true);
                posingSystem.isEightBitVersion = true;

                PosingSystemConverter.DisableAdditiveBreathingForEightBit(posingSystem);

                Assert.That(posingSystem.additiveBreathing.enabled, Is.False);
                Assert.That(settingsObject.GetComponentsInChildren<ModularAvatarParameters>(true)
                    .SelectMany(component => component.parameters)
                    .Any(parameter =>
                        parameter.nameOrPrefix ==
                        PosingSystemConverter.AdditiveBreathingEnabledParameterName),
                    Is.False);
                Assert.That(settingsObject.GetComponentsInChildren<ModularAvatarMenuItem>(true)
                    .Any(item => item.Control != null && item.Control.parameter != null &&
                                 item.Control.parameter.name ==
                                 PosingSystemConverter.AdditiveBreathingEnabledParameterName),
                    Is.False);
                Assert.That(PosingSystemConverter.GetAdditiveBreathingMergeAnimator(
                    posingSystem,
                    true), Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(avatarObject);
            }
        }

        [Test]
        public void BuiltInClips_HaveThreeStrengths_AndFasterRapidPreset()
        {
            var strong = LoadClipByGuid(
                PosingSystemConverter.BreathingStrongClipGuid,
                "USSPS_Breathing_Strong");
            var medium = LoadClipByGuid(
                PosingSystemConverter.BreathingMediumClipGuid,
                "USSPS_Breathing_Medium");
            var weak = LoadClipByGuid(
                PosingSystemConverter.BreathingWeakClipGuid,
                "USSPS_Breathing_Weak");

            Assert.That(strong.name, Is.EqualTo("USSPS_Breathing_Strong"));
            Assert.That(medium.name, Is.EqualTo("USSPS_Breathing_Medium"));
            Assert.That(weak.name, Is.EqualTo("USSPS_Breathing_Weak"));
            Assert.That(strong.length, Is.EqualTo(5f).Within(0.01f));
            Assert.That(medium.length, Is.EqualTo(5f).Within(0.01f));
            Assert.That(weak.length, Is.EqualTo(5f).Within(0.01f));
            var rapid = LoadClipByGuid(PosingSystemConverter.BreathingRapidClipGuid, "USSPS_Breathing_Rapid");
            Assert.That(rapid.length, Is.EqualTo(1.8f).Within(0.01f));
            Assert.That(GetMaximumAbsoluteValue(rapid, "UpperChest Front-Back"),
                Is.EqualTo(.1134f).Within(.0001f));

            Assert.That(GetMaximumAbsoluteValue(strong, "UpperChest Front-Back"),
                Is.EqualTo(0.1344f).Within(0.0001f));
            Assert.That(GetMaximumAbsoluteValue(medium, "UpperChest Front-Back"),
                Is.EqualTo(0.0903f).Within(0.0001f));
            Assert.That(GetMaximumAbsoluteValue(weak, "UpperChest Front-Back"),
                Is.EqualTo(0.0462f).Within(0.0001f));

            foreach (var binding in AnimationUtility.GetCurveBindings(medium))
            {
                if (binding.propertyName.Contains("Leg") || binding.propertyName.Contains("Foot"))
                {
                    Assert.That(GetMaximumAbsoluteValue(strong, binding.propertyName),
                        Is.EqualTo(GetMaximumAbsoluteValue(medium, binding.propertyName)).Within(.00001f),
                        "Large breaths keep leg motion subtle: " + binding.propertyName);
                    continue;
                }
                var weakCurve = AnimationUtility.GetEditorCurve(weak, binding);
                var strongCurve = AnimationUtility.GetEditorCurve(strong, binding);
                var mediumCurve = AnimationUtility.GetEditorCurve(medium, binding);
                for (var sample = 0; sample <= 20; sample++)
                {
                    var time = medium.length * sample / 20f;
                    Assert.That(mediumCurve.Evaluate(time),
                        Is.EqualTo((weakCurve.Evaluate(time) + strongCurve.Evaluate(time)) * .5f).Within(.00001f),
                        "Medium is halfway between weak and strong, including compensation: " + binding.propertyName);
                }
            }
        }

        [Test]
        public void NewSettings_DefaultEveryPoseToMedium_AndResolveAllSixChoices()
        {
            var settings = new PosingSystem.AdditiveBreathingSettings();
            foreach (var group in System.Enum
                         .GetValues(typeof(PosingSystem.LocomotionTypeGroup))
                         .Cast<PosingSystem.LocomotionTypeGroup>())
            {
                Assert.That(settings.GetLevel(group), Is.EqualTo(PosingSystem.BreathingLevel.Medium));
                AssertClipGuid(
                    PosingSystemConverter.ResolveAdditiveBreathingClip(settings, group),
                    PosingSystemConverter.BreathingMediumClipGuid);
                Assert.That(settings.GetCustomClip(group), Is.Null);
            }

            settings.standingLevel = PosingSystem.BreathingLevel.None;
            Assert.That(PosingSystemConverter.ResolveAdditiveBreathingClip(
                settings, PosingSystem.LocomotionTypeGroup.Standing), Is.Null);

            settings.standingLevel = PosingSystem.BreathingLevel.Weak;
            AssertClipGuid(
                PosingSystemConverter.ResolveAdditiveBreathingClip(
                    settings, PosingSystem.LocomotionTypeGroup.Standing),
                PosingSystemConverter.BreathingWeakClipGuid);

            settings.standingLevel = PosingSystem.BreathingLevel.Strong;
            AssertClipGuid(
                PosingSystemConverter.ResolveAdditiveBreathingClip(
                    settings, PosingSystem.LocomotionTypeGroup.Standing),
                PosingSystemConverter.BreathingStrongClipGuid);

            settings.standingLevel = PosingSystem.BreathingLevel.Rapid;
            AssertClipGuid(
                PosingSystemConverter.ResolveAdditiveBreathingClip(
                    settings, PosingSystem.LocomotionTypeGroup.Standing),
                PosingSystemConverter.BreathingRapidClipGuid);

            var custom = new AnimationClip();
            try
            {
                settings.standing = custom;
                settings.standingLevel = PosingSystem.BreathingLevel.Custom;
                Assert.That(PosingSystemConverter.ResolveAdditiveBreathingClip(
                    settings, PosingSystem.LocomotionTypeGroup.Standing), Is.SameAs(custom));
            }
            finally
            {
                Object.DestroyImmediate(custom);
            }
        }

        [Test]
        public void Presets_LoopSmoothly_AndOnlyAnimateCoordinatedBodyMuscles()
        {
            foreach (var level in new[] { PosingSystem.BreathingLevel.Weak, PosingSystem.BreathingLevel.Medium,
                         PosingSystem.BreathingLevel.Strong, PosingSystem.BreathingLevel.Rapid })
            {
                var settings = new PosingSystem.AdditiveBreathingSettings { standingLevel = level };
                var clip = PosingSystemConverter.ResolveAdditiveBreathingClip(settings, PosingSystem.LocomotionTypeGroup.Standing);
                Assert.That(AnimationUtility.GetAnimationClipSettings(clip).loopTime, Is.True);
                Assert.That(HasMuscleCurve(clip, "Neck Tilt Left-Right"), Is.False);
                Assert.That(HasMuscleCurve(clip, "Neck Turn Left-Right"), Is.False);
                foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                {
                    Assert.That(binding.type, Is.EqualTo(typeof(Animator)));
                    Assert.That(binding.path, Is.Empty);
                    Assert.That(HumanTrait.MuscleName, Does.Contain(binding.propertyName), "No root/transform curves");
                    Assert.That(binding.propertyName, Does.Not.Contain("Finger"));
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    Assert.That(curve.Evaluate(0), Is.Zero.Within(.00001f));
                    Assert.That(curve.Evaluate(clip.length), Is.Zero.Within(.00001f));
                    Assert.That(curve.keys.First().outTangent, Is.Zero);
                    Assert.That(curve.keys.Last().inTangent, Is.Zero);
                    for (var sample = 0; sample <= 40; sample++)
                        Assert.That(Mathf.Abs(curve.Evaluate(clip.length * sample / 40f)), Is.LessThanOrEqualTo(.135f));
                }
                foreach (var side in new[] { "Left ", "Right " })
                {
                    Assert.That(GetMaximumAbsoluteValue(clip, side + "Upper Leg Front-Back"), Is.InRange(.0001f, .003f));
                    Assert.That(GetMaximumAbsoluteValue(clip, side + "Lower Leg Stretch"), Is.InRange(.0001f, .003f));
                    Assert.That(HasMuscleCurve(clip, side + "Arm Front-Back"), Is.True);
                }
                var chest = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), "Chest Front-Back"));
                var neck = AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), "Neck Nod Down-Up"));
                for (var sample = 1; sample < 40; sample++)
                    Assert.That(chest.Evaluate(clip.length * sample / 40f) * neck.Evaluate(clip.length * sample / 40f), Is.LessThan(0));
            }
        }

        [Test]
        public void RapidSelection_RoundTrips_WithoutReassigningExistingSerializedLevels()
        {
            Assert.That((int)PosingSystem.BreathingLevel.Medium, Is.EqualTo(0));
            Assert.That((int)PosingSystem.BreathingLevel.None, Is.EqualTo(1));
            Assert.That((int)PosingSystem.BreathingLevel.Weak, Is.EqualTo(2));
            Assert.That((int)PosingSystem.BreathingLevel.Strong, Is.EqualTo(3));
            Assert.That((int)PosingSystem.BreathingLevel.Custom, Is.EqualTo(4));
            var settings = new PosingSystem.AdditiveBreathingSettings { standingLevel = PosingSystem.BreathingLevel.Rapid };
            var restored = JsonUtility.FromJson<PosingSystem.AdditiveBreathingSettings>(JsonUtility.ToJson(settings));
            Assert.That(restored.standingLevel, Is.EqualTo(PosingSystem.BreathingLevel.Rapid));
        }

        [Test]
        public void RapidSelection_InvalidatesPrebuildWithoutIncreasingParameterCost()
        {
            var go = Object.Instantiate(LoadAssetByGuid<GameObject>(SettingsPrefabGuid));
            try
            {
                var ps = go.GetComponentInChildren<PosingSystem>(true);
                var original = PosingSystemConverter.GetDefineSerializeJson(ps);
                var parameters = EditorJsonUtility.ToJson(go.GetComponent<ModularAvatarParameters>());
                ps.additiveBreathing.standingLevel = PosingSystem.BreathingLevel.Rapid;
                Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps), Is.Not.EqualTo(original));
                Assert.That(EditorJsonUtility.ToJson(go.GetComponent<ModularAvatarParameters>()), Is.EqualTo(parameters));
                ps.additiveBreathing.standingLevel = PosingSystem.BreathingLevel.Medium;
                Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps), Is.EqualTo(original));
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void DisplayNames_AreDerivedFromDefines_AndExcludeMissingGroups()
        {
            var gameObject = new GameObject("BreathingDisplayNamesTest");
            try
            {
                var posingSystem = gameObject.AddComponent<PosingSystem>();
                posingSystem.defines = new List<PosingSystem.LayerDefine>
                {
                    CreateLayerDefine("立ち姿勢", 1),
                    CreateLayerDefine("中腰姿勢", 12),
                    CreateLayerDefine("座り姿勢", 22),
                    CreateLayerDefine("仰向け寝", 32),
                    CreateLayerDefine("うつ伏せ寝", 33),
                };

                var displayNames = PosingSystemEditor.GetAdditiveBreathingDisplayNames(posingSystem);

                CollectionAssert.AreEqual(
                    new[]
                    {
                        PosingSystem.LocomotionTypeGroup.Standing,
                        PosingSystem.LocomotionTypeGroup.SitShallow,
                        PosingSystem.LocomotionTypeGroup.SitDeep,
                        PosingSystem.LocomotionTypeGroup.SitSleep,
                    },
                    displayNames.Select(entry => entry.Key));
                CollectionAssert.AreEqual(
                    new[] { "立ち姿勢", "中腰姿勢", "座り姿勢", "仰向け寝／うつ伏せ寝" },
                    displayNames.Select(entry => entry.Value));
                Assert.That(
                    displayNames.Any(entry => entry.Key == PosingSystem.LocomotionTypeGroup.VirtualLove),
                    Is.False);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        private static PosingSystem.LayerDefine CreateLayerDefine(string menuName, int locomotionTypeValue)
        {
            return new PosingSystem.LayerDefine(
                string.Empty,
                string.Empty,
                string.Empty,
                locomotionTypeValue,
                menuName,
                new List<PosingSystem.AnimationDefine>());
        }

        private static PosingSystem.LayerDefine CreateLayerDefineWithSyncedPose(
            string menuName,
            int locomotionTypeValue,
            int syncedPoseValue)
        {
            var animation = new PosingSystem.AnimationDefine
            {
                enabled = true,
                syncdParameterValue = syncedPoseValue,
            };
            return new PosingSystem.LayerDefine(
                string.Empty,
                string.Empty,
                string.Empty,
                locomotionTypeValue,
                menuName,
                new List<PosingSystem.AnimationDefine> { animation });
        }

        private static bool HasSynchronizedPoseConditions(
            AnimatorStateTransition transition,
            int syncedPoseValue,
            int maxSyncedPoseValue)
        {
            return PosingSystemConverter.SplitSyncParameters(syncedPoseValue, maxSyncedPoseValue)
                .All(parameter => transition.conditions.Any(condition =>
                    condition.parameter == parameter.name &&
                    condition.mode == (parameter.type == ParameterSyncType.Bool
                        ? (parameter.value != 0 ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot)
                        : AnimatorConditionMode.Equals) &&
                    condition.threshold == parameter.value));
        }

        private static AnimationClip LoadClipByGuid(string guid, string name)
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            Assert.That(clip, Is.Not.Null, name);
            return clip;
        }

        private static T LoadAssetByGuid<T>(string guid) where T : Object
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            Assert.That(asset, Is.Not.Null, guid);
            return asset;
        }

        private static bool HasMuscleCurve(AnimationClip clip, string propertyName)
        {
            return AnimationUtility.GetCurveBindings(clip)
                .Any(binding => binding.type == typeof(Animator) && binding.propertyName == propertyName);
        }

        private static float GetMaximumAbsoluteValue(AnimationClip clip, string propertyName)
        {
            var binding = AnimationUtility.GetCurveBindings(clip)
                .Single(candidate =>
                    candidate.type == typeof(Animator) && candidate.propertyName == propertyName);
            return AnimationUtility.GetEditorCurve(clip, binding).keys
                .Max(keyframe => Mathf.Abs(keyframe.value));
        }

        private static void AssertClipGuid(AnimationClip clip, string expectedGuid)
        {
            Assert.That(clip, Is.Not.Null);
            var actualGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(clip));
            Assert.That(actualGuid, Is.EqualTo(expectedGuid));
        }
    }
}
