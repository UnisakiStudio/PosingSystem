using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using UnityEngine;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void RolloverGeneration_ConversionAndPersistedBuild(bool prebuilt)
        {
            root.AddComponent<VRCAvatarDescriptor>();
            var obj = new GameObject("Poses"); obj.transform.SetParent(root.transform, false);
            var ps = obj.AddComponent<PosingSystem>();
            ps.features = new PosingSystemFeatureSettings { rollover = true, footHeight = false, mirror = false };
            var source = Clip(); source.name = "RolloverA";
            for (var axis = 0; axis < 3; axis++) AnimationUtility.SetEditorCurve(source,
                EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT." + "xyz"[axis]), AnimationCurve.Constant(0, 1, axis == 1 ? 1 : 0));
            for (var axis = 0; axis < 4; axis++) AnimationUtility.SetEditorCurve(source,
                EditorCurveBinding.FloatCurve("", typeof(Animator), "RootQ." + "xyzw"[axis]), AnimationCurve.Constant(0, 1, axis == 3 ? 1 : 0));
            AnimationUtility.SetEditorCurve(source, EditorCurveBinding.FloatCurve("", typeof(Animator), "Spine Front-Back"), AnimationCurve.Linear(0, -.1f, 1, .1f));
            var second = Object.Instantiate(source); second.name = "RolloverB"; assets.Add(second);
            ps.features.rolloverAnimation = source; // Legacy fallback must not replace the per-pose animations.
            ps.defines = new List<PosingSystem.LayerDefine> { new("Sleep", "Sleep", "Sleep", 30, "Sleep", new List<PosingSystem.AnimationDefine> {
                new(source, "A") { isRollover = true, rolloverSideAnimation = second, syncdParameterValue = 4, typeParameterValue = 1 },
                new(second, "B") { isRollover = true, rolloverSideAnimation = source, syncdParameterValue = 5, typeParameterValue = 2 },
                new(source, "Legacy") { syncdParameterValue = 6, typeParameterValue = 3 } }) };
            testFolder = "Assets/USSPS_RolloverBuild_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", testFolder.Substring(7));
            var controller = AnimatorController.CreateAnimatorControllerAtPath(testFolder + "/Base.controller");
            var layers = controller.layers; layers[0].name = "USSPS_Locomotion"; controller.layers = layers;
            controller.AddLayer("USSPS_LocomotionType");
            controller.AddParameter("VRMode", AnimatorControllerParameterType.Int);
            controller.AddParameter("TrackingType", AnimatorControllerParameterType.Int);
            controller.AddParameter("LocomotionType", AnimatorControllerParameterType.Int);
            controller.AddParameter("Sleep", AnimatorControllerParameterType.Int);
            controller.AddParameter("USSPS_Mirror", AnimatorControllerParameterType.Bool);
            var common = new GameObject("Common"); common.transform.SetParent(obj.transform, false);
            common.AddComponent<DuplicateEraser>().ID = "jp.unisakistudio.posingsystem_locomotion";
            common.AddComponent<ModularAvatarMergeAnimator>().animator = controller;
            var generated = new HashSet<string>();
            var sourceJson = EditorJsonUtility.ToJson(source);
            try
            {
                PosingSystemConverter.ResetAnimatorControllerParameters(ps);
                PosingSystemConverter.ConvertAnimatorController(ps);
                PosingSystemConverter.ConvertOptionalFeatures(ps);
                TrackGenerated();
                var states = controller.layers[0].stateMachine.states.Select(s => s.state).ToArray();
                Assert.That(EditorJsonUtility.ToJson(source), Is.EqualTo(sourceJson));
                foreach (var state in states) AssertConvertedHeadCentering(state.motion, state.timeParameterActive);
                StringAssert.Contains("\"rolloverCenteringLogicVersion\":1", PosingSystemConverter.GetDefineSerializeJson(ps));
                Assert.That(states.Count(s => s.timeParameterActive && s.timeParameter == PosingSystemFeatureUtility.RollTime), Is.EqualTo(4), "Both Desktop and VR states for each marked pose");
                Assert.That(states.Where(s => s.timeParameterActive).Select(s => s.motion.name).Distinct().Count(), Is.EqualTo(2));
                var legacy = (AnimatorController)obj.GetComponentsInChildren<ModularAvatarMergeAnimator>().Single(m => m.name == "USSPS_RolloverController").animator;
                Assert.That(legacy.layers[1].stateMachine.states.Count(s => s.state.timeParameterActive), Is.EqualTo(1));
                Assert.That(legacy.layers[1].stateMachine.states.Single(s => s.state.timeParameterActive).state.name, Is.EqualTo("寝返り_6"));
                if (prebuilt)
                {
                    // Save/reload the controller like the persisted prebuild path, then rebuild it.
                    EditorUtility.SetDirty(controller); AssetDatabase.SaveAssets();
                    AssetDatabase.ImportAsset(testFolder + "/Base.controller", ImportAssetOptions.ForceUpdate);
                    controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(testFolder + "/Base.controller");
                    common.GetComponent<ModularAvatarMergeAnimator>().animator = controller;
                    foreach (var child in controller.layers[0].stateMachine.states)
                        AssertConvertedHeadCentering(child.state.motion, child.state.timeParameterActive);
                }
                PosingSystemConverter.ConvertOptionalFeatures(ps); TrackGenerated();
                Assert.That(controller.layers.Count(l => l.name == "USSPS_RolloverTime"), Is.EqualTo(1));
                ps.features.rollover = false;
                PosingSystemConverter.ConvertOptionalFeatures(ps);
                Assert.That(controller.layers.Any(l => l.name == "USSPS_RolloverTime"), Is.False);
                Assert.That(controller.parameters.Single(p => p.name == PosingSystemFeatureUtility.RollTime).defaultFloat, Is.EqualTo(.5f));
                ps.defines[0].animations[0].isRollover = false;
                PosingSystemConverter.ConvertAnimatorController(ps);
                Assert.That(controller.layers[0].stateMachine.states.Count(s => s.state.timeParameterActive), Is.EqualTo(2), "Only the second pose remains time driven");
                foreach (var child in controller.layers[0].stateMachine.states)
                    AssertConvertedHeadCentering(child.state.motion, child.state.timeParameterActive);
            }
            finally
            {
                TrackGenerated();
                foreach (var path in generated) AssetDatabase.DeleteAsset(path);
            }
            void TrackGenerated()
            {
                foreach (var merge in obj.GetComponentsInChildren<ModularAvatarMergeAnimator>())
                    if (merge.name == "USSPS_RolloverController") generated.Add(AssetDatabase.GetAssetPath(merge.animator));
            }
        }

        private AnimationClip GenerationPose(bool side, bool mirror = false)
        {
            var clip = PosingSystemConverter.RootOffsetClip(new Vector3(0, .4f, 0),
                (side ? Quaternion.AngleAxis(85, Vector3.forward) : Quaternion.identity) * Quaternion.Euler(-90, 0, 0), "Source");
            assets.Add(clip);
            foreach (var name in new[] { "Left Arm Front-Back", "Right Arm Front-Back", "Spine Twist Left-Right" })
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), name), AnimationCurve.Constant(0, 1, side ? .3f : -.1f));
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.mirror = mirror; settings.keepOriginalPositionXZ = settings.keepOriginalPositionY = settings.keepOriginalOrientation = true;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;
        }

        [TestCase(0f)] [TestCase(.25f)] [TestCase(1f)]
        public void RolloverGeneration_CenterAndSourcesPreserved(float strength)
        {
            var center = GenerationPose(false); var side = GenerationPose(true);
            var beforeCenter = EditorJsonUtility.ToJson(center); var beforeSide = EditorJsonUtility.ToJson(side);
            var output = PosingSystemRolloverGenerator.Create(root, center, side, strength); assets.Add(output);
            Assert.That(output.length, Is.EqualTo(1).Within(.00001f));
            var settings = AnimationUtility.GetAnimationClipSettings(output);
            Assert.That(settings.loopBlendOrientation && settings.loopBlendPositionY && settings.loopBlendPositionXZ, Is.True, "Bake root motion into pose for MotionTime");
            var bones = new[] { HumanBodyBones.Head, HumanBodyBones.Hips, HumanBodyBones.LeftHand, HumanBodyBones.RightFoot };
            center.SampleAnimation(root, 0); root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            var positions = bones.Select(b => animator.GetBoneTransform(b).position).ToArray();
            var rotations = bones.Select(b => animator.GetBoneTransform(b).rotation).ToArray();
            output.SampleAnimation(root, .5f); root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            for (var i = 0; i < bones.Length; i++)
            {
                Assert.That(Vector3.Distance(positions[i], animator.GetBoneTransform(bones[i]).position), Is.LessThan(.001f), bones[i].ToString());
                Assert.That(Quaternion.Angle(rotations[i], animator.GetBoneTransform(bones[i]).rotation), Is.LessThan(.1f));
            }
            Assert.That(EditorJsonUtility.ToJson(center), Is.EqualTo(beforeCenter));
            Assert.That(EditorJsonUtility.ToJson(side), Is.EqualTo(beforeSide));
            var muscle = AnimationUtility.GetEditorCurve(output, EditorCurveBinding.FloatCurve("", typeof(Animator), "Left Arm Front-Back"));
            Assert.That(muscle.Evaluate(.25f), Is.EqualTo((muscle.Evaluate(0) + muscle.Evaluate(.5f)) / 2).Within(.00001f));
            Assert.That(muscle.Evaluate(.75f), Is.EqualTo((muscle.Evaluate(1) + muscle.Evaluate(.5f)) / 2).Within(.00001f));
        }

        [TestCase(0f)] [TestCase(.25f)]
        public void RolloverGeneration_AutoDirectionWorksForMirroredSource(float strength)
        {
            var center = GenerationPose(false); var left = GenerationPose(true); var right = GenerationPose(true, true);
            var a = PosingSystemRolloverGenerator.Create(root, center, left, strength); assets.Add(a);
            var b = PosingSystemRolloverGenerator.Create(root, center, right, strength); assets.Add(b);
            for (var frame = 0; frame <= 32; frame++)
            {
                var t = frame / 32f;
                a.SampleAnimation(root, t); root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                var head = animator.GetBoneTransform(HumanBodyBones.Head);
                var position = head.position; var rotation = head.rotation;
                b.SampleAnimation(root, t); root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                Assert.That(Vector3.Distance(position, head.position), Is.LessThan(.002f), "direction at " + t);
                Assert.That(Quaternion.Angle(rotation, head.rotation), Is.LessThan(.15f));
            }
        }

        [Test] public void RolloverGeneration_RejectsUnsupportedInputsAndPreservesAvatar()
        {
            var center = GenerationPose(false); var side = GenerationPose(true);
            root.transform.SetPositionAndRotation(new Vector3(4, 2, -3), Quaternion.Euler(0, 70, 0));
            root.transform.localScale = Vector3.one * .7f;
            var before = root.GetComponentsInChildren<Transform>().Select(t => (t.localPosition, t.localRotation, t.localScale)).ToArray();
            var clip = PosingSystemRolloverGenerator.Create(root, center, side, 0); assets.Add(clip);
            Assert.That(root.GetComponentsInChildren<Transform>().Select(t => (t.localPosition, t.localRotation, t.localScale)).ToArray(), Is.EqualTo(before));
            Assert.Throws<ArgumentOutOfRangeException>(() => PosingSystemRolloverGenerator.Create(root, center, side, float.NaN));
            var empty = new AnimationClip(); assets.Add(empty);
            Assert.Throws<ArgumentException>(() => PosingSystemRolloverGenerator.Create(root, center, empty));
        }

        [Test] public void RolloverGeneration_SettingsSavedAndInvalidatePrebuild()
        {
            var ps = root.AddComponent<PosingSystem>();
            root.AddComponent<ModularAvatarParameters>();
            ps.features.rollover = true;
            var pose = new PosingSystem.AnimationDefine(GenerationPose(false), "Center") { isRollover = true, rolloverSideAnimation = GenerationPose(true) };
            ps.defines = new List<PosingSystem.LayerDefine> { new("Sleep", "Sleep", "Sleep", 30, "Sleep", new List<PosingSystem.AnimationDefine>{pose}) };
            testFolder = "Assets/RolloverSettings_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", testFolder.Substring(7));
            AssetDatabase.CreateAsset(pose.rolloverSideAnimation, testFolder + "/Side.anim");
            var before = PosingSystemConverter.GetDefineSerializeJson(ps);
            pose.rolloverContactStrength = 0;
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps), Is.Not.EqualTo(before));
            before = PosingSystemConverter.GetDefineSerializeJson(ps);
            pose.rolloverSideAnimation = GenerationPose(true, true);
            AssetDatabase.CreateAsset(pose.rolloverSideAnimation, testFolder + "/OtherSide.anim");
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps), Is.Not.EqualTo(before));
            PosingSystemFeatureUtility.Synchronize(ps);
            var parameters = ps.GetComponentsInChildren<ModularAvatarParameters>(true).SelectMany(p => p.parameters).ToArray();
            Assert.That(parameters.Single(p => p.nameOrPrefix == PosingSystemFeatureUtility.RollActive).saved, Is.True);
            Assert.That(parameters.Single(p => p.nameOrPrefix == PosingSystemFeatureUtility.RollSensor).saved, Is.False);
            var json = EditorJsonUtility.ToJson(ps);
            var other = new GameObject("copy"); assets.Add(other);
            var copy = other.AddComponent<PosingSystem>(); EditorJsonUtility.FromJsonOverwrite(json, copy);
            Assert.That(copy.defines[0].animations[0].rolloverSideAnimation, Is.EqualTo(pose.rolloverSideAnimation));
            Assert.That(copy.defines[0].animations[0].rolloverContactStrength, Is.Zero);
        }

        [Test] public void RolloverGeneration_IconBadgeOnlyChangesBottomLeft()
        {
            var icon = new Texture2D(64, 64); var badge = new Texture2D(4, 4); assets.Add(icon); assets.Add(badge);
            icon.SetPixels(Enumerable.Repeat(Color.red, 4096).ToArray()); icon.Apply();
            badge.SetPixels(Enumerable.Repeat(Color.blue, 16).ToArray()); badge.Apply();
            PosingSystemRolloverIcon.Stamp(icon, badge);
            Assert.That(icon.GetPixel(0, 0), Is.EqualTo(Color.blue));
            Assert.That(icon.GetPixel(0, 63), Is.EqualTo(Color.red));
            Assert.That(icon.GetPixel(63, 0), Is.EqualTo(Color.red));
            Assert.That(icon.GetPixel(32, 32), Is.EqualTo(Color.red));
        }
    }
}
