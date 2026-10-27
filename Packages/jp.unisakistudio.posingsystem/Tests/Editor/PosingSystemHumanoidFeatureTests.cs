using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using UnityEditor.Animations;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        private GameObject root;
        private Animator animator;
        private readonly List<Object> assets = new();
        private string testFolder;
        private HashSet<string> existingAssets;

        [SetUp]
        public void SetUp()
        {
            existingAssets = AssetDatabase.FindAssets("t:AnimatorController", new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath).ToHashSet();
            root = new GameObject("FeatureTestHumanoid");
            var bones = new Dictionary<HumanBodyBones, Transform>();
            Bone(HumanBodyBones.Hips, null, new Vector3(0, 1, 0));
            Bone(HumanBodyBones.Spine, HumanBodyBones.Hips, new Vector3(0, .15f, 0));
            Bone(HumanBodyBones.Chest, HumanBodyBones.Spine, new Vector3(0, .15f, 0));
            Bone(HumanBodyBones.Neck, HumanBodyBones.Chest, new Vector3(0, .2f, 0));
            Bone(HumanBodyBones.Head, HumanBodyBones.Neck, new Vector3(0, .12f, 0));
            foreach (var left in new[] { true, false })
            {
                var side = left ? "Left" : "Right";
                var sign = left ? -1 : 1;
                var upperLeg = (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), side + "UpperLeg");
                var lowerLeg = (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), side + "LowerLeg");
                var foot = (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), side + "Foot");
                var shoulder = (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), side + "Shoulder");
                var upperArm = (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), side + "UpperArm");
                var lowerArm = (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), side + "LowerArm");
                var hand = (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), side + "Hand");
                Bone(upperLeg, HumanBodyBones.Hips, new Vector3(.1f * sign, -.05f, 0));
                Bone(lowerLeg, upperLeg, new Vector3(0, -.43f, .01f));
                Bone(foot, lowerLeg, new Vector3(0, -.43f, -.01f));
                Bone(shoulder, HumanBodyBones.Chest, new Vector3(.1f * sign, .1f, 0));
                Bone(upperArm, shoulder, new Vector3(.1f * sign, 0, 0));
                Bone(lowerArm, upperArm, new Vector3(.25f * sign, 0, -.01f));
                Bone(hand, lowerArm, new Vector3(.25f * sign, 0, .01f));
            }
            var avatar = AvatarBuilder.BuildHumanAvatar(root, new HumanDescription {
                human = bones.Select(b => new HumanBone { boneName = b.Value.name, humanName = HumanTrait.BoneName[(int)b.Key],
                    limit = new HumanLimit { useDefaultValues = true } }).ToArray(),
                skeleton = root.GetComponentsInChildren<Transform>().Select(t => new SkeletonBone { name=t.name,
                    position=t.localPosition, rotation=t.localRotation, scale=t.localScale }).ToArray(),
                upperArmTwist=.5f, lowerArmTwist=.5f, upperLegTwist=.5f, lowerLegTwist=.5f, armStretch=.05f, legStretch=.05f });
            assets.Add(avatar);
            Assert.That(avatar.isValid && avatar.isHuman, Is.True);
            animator = root.AddComponent<Animator>();
            animator.avatar = avatar;
            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

            void Bone(HumanBodyBones bone, HumanBodyBones? parent, Vector3 position)
            {
                var obj = new GameObject(bone.ToString());
                obj.transform.SetParent(parent.HasValue ? bones[parent.Value] : root.transform, false);
                obj.transform.localPosition = position;
                bones.Add(bone, obj.transform);
            }
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(root);
            foreach (var asset in assets) Object.DestroyImmediate(asset);
            assets.Clear();
            if (!string.IsNullOrEmpty(testFolder)) AssetDatabase.DeleteAsset(testFolder);
            foreach (var path in AssetDatabase.FindAssets("FeatureTestHumanoid t:AnimatorController", new[] { "Assets" }).Select(AssetDatabase.GUIDToAssetPath))
                if (!existingAssets.Contains(path)) AssetDatabase.DeleteAsset(path);
        }

        [Test]
        public void FaceSolver_PreservesBodyAndCorrectsTiltedHead()
        {
            using var handler = new HumanPoseHandler(animator.avatar, root.transform);
            var pose = new HumanPose { muscles = new float[HumanTrait.MuscleCount], bodyRotation = Quaternion.identity };
            handler.SetHumanPose(ref pose);
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            var target = head.rotation;
            foreach (var name in new[] { "Spine Front-Back", "Chest Twist Left-Right", "Neck Nod Down-Up", "Head Turn Left-Right" })
                pose.muscles[Array.IndexOf(HumanTrait.MuscleName, name)] = .25f;
            handler.SetHumanPose(ref pose);
            var before = pose.muscles.ToArray();
            var originalError = Quaternion.Angle(target, head.rotation);
            PosingSystemFaceForwardUtility.Solve(handler, head, target, ref pose);
            Assert.That(originalError, Is.GreaterThan(10));
            Assert.That(Quaternion.Angle(target, head.rotation), Is.LessThan(.5f));
            for (var i = 0; i < before.Length; i++)
                if (!PosingSystemFaceForwardUtility.Muscles.Contains(i)) Assert.That(pose.muscles[i], Is.EqualTo(before[i]));
        }

        [Test]
        public void FaceClip_OnMaskedLayer_CorrectsHeadWithoutChangingLimbs()
        {
            var source = Clip();
            foreach (var name in new[] { "Spine Front-Back", "Chest Twist Left-Right", "Neck Nod Down-Up", "Head Turn Left-Right" })
                AnimationUtility.SetEditorCurve(source, EditorCurveBinding.FloatCurve("", typeof(Animator), name), AnimationCurve.Constant(0, 1, .25f));
            using var handler = new HumanPoseHandler(animator.avatar, root.transform);
            var pose = new HumanPose { muscles = new float[HumanTrait.MuscleCount], bodyRotation = Quaternion.identity };
            handler.SetHumanPose(ref pose);
            var target = animator.GetBoneTransform(HumanBodyBones.Head).rotation;
            var correction = PosingSystemFaceForwardUtility.Create(animator, source);
            assets.Add(correction);
            Assert.That(AnimationUtility.GetCurveBindings(correction).Length, Is.EqualTo(6));
            EvaluateLayers(source, correction, false, AvatarMaskBodyPart.Head, (mixer, graph) =>
            {
                mixer.SetInputWeight(1, 0); graph.Evaluate(.02f);
                var before = animator.GetBoneTransform(HumanBodyBones.LeftHand).position;
                mixer.SetInputWeight(1, 1); graph.Evaluate(.02f);
                Assert.That(Vector3.Distance(before, animator.GetBoneTransform(HumanBodyBones.LeftHand).position), Is.LessThan(.001f));
                Assert.That(Quaternion.Angle(target, animator.GetBoneTransform(HumanBodyBones.Head).rotation), Is.LessThan(1f));
            });
        }

        [Test]
        public void FloatingRootLayer_MovesEveryBoneEqually_AndRestoresOnDisable()
        {
            var source = Clip();
            AnimationUtility.SetEditorCurve(source, EditorCurveBinding.FloatCurve("", typeof(Animator), "Left Upper Leg Front-Back"), AnimationCurve.Constant(0, 1, .3f));
            var zero = PosingSystemConverter.RootOffsetClip(Vector3.zero, Quaternion.identity, "Zero");
            var up = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.identity, "Up");
            assets.Add(zero); assets.Add(up);
            AnimationUtility.SetAdditiveReferencePose(up, zero, 0);
            EvaluateLayers(source, up, true, AvatarMaskBodyPart.Root, (mixer, graph) =>
            {
                var bones = new[] { HumanBodyBones.Hips, HumanBodyBones.Head, HumanBodyBones.LeftHand, HumanBodyBones.LeftFoot };
                mixer.SetInputWeight(1, 0); graph.Evaluate(.02f);
                var positions = bones.Select(b => animator.GetBoneTransform(b).position).ToArray();
                mixer.SetInputWeight(1, 1); graph.Evaluate(.02f);
                var offsets = bones.Select((b, i) => animator.GetBoneTransform(b).position - positions[i]).ToArray();
                Assert.That(offsets[0].y, Is.GreaterThan(.1f));
                for (var i = 0; i < offsets.Length; i++) Assert.That(Vector3.Distance(offsets[i], offsets[0]), Is.LessThan(.002f), "Translation of " + bones[i] + ": " + offsets[i] + " vs " + offsets[0]);
                mixer.SetInputWeight(1, 0); graph.Evaluate(.02f);
                foreach (var pair in bones.Select((b, i) => (b, i)))
                    Assert.That(Vector3.Distance(animator.GetBoneTransform(pair.b).position, positions[pair.i]), Is.LessThan(.002f), "Restore " + pair.b);
            });
        }

        [TestCase(0f, -2f)]
        [TestCase(0f, 2f)]
        [TestCase(90f, -2f)]
        [TestCase(90f, 2f)]
        [TestCase(-90f, 2f)]
        [TestCase(45f, 1f)]
        [TestCase(0f, 2f, true, .15f)]
        [TestCase(0f, -2f, true, .75f)]
        public void FootHeight_AdditiveTranslationMatchesLegacyLevel(float tilt, float height, bool animated = false, float sampleTime = 0)
        {
            var source = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.Euler(tilt, 0, 0), "Pose");
            assets.Add(source);
            if (animated)
            {
                var from = Quaternion.Euler(tilt, 0, 0);
                var to = Quaternion.Euler(tilt + 110, 0, 0);
                for (var axis = 0; axis < 4; axis++)
                    AnimationUtility.SetEditorCurve(source, EditorCurveBinding.FloatCurve("", typeof(Animator), "RootQ." + "xyzw"[axis]), AnimationCurve.Linear(0, from[axis], 1, to[axis]));
            }
            AnimationUtility.SetEditorCurve(source, EditorCurveBinding.FloatCurve("", typeof(Animator), "Left Upper Leg Front-Back"), AnimationCurve.Constant(0, 1, .3f));
            var legacy = Object.Instantiate(source); assets.Add(legacy);
            var settings = AnimationUtility.GetAnimationClipSettings(legacy);
            settings.level -= height;
            AnimationUtility.SetAnimationClipSettings(legacy, settings);
            var zero = PosingSystemConverter.RootOffsetClip(Vector3.zero, Quaternion.identity, "Zero");
            var offset = PosingSystemConverter.CreateFootHeightOffsetClip(source, height, zero);
            assets.Add(zero); assets.Add(offset);
            var bones = new[] { HumanBodyBones.Hips, HumanBodyBones.Head, HumanBodyBones.LeftHand, HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot };
            Vector3[] expected = null;
            EvaluateLayers(legacy, zero, true, AvatarMaskBodyPart.Root, (mixer, graph) =>
            {
                mixer.SetInputWeight(1, 0); graph.Evaluate(.02f);
                expected = bones.Select(b => animator.GetBoneTransform(b).position).ToArray();
            }, sampleTime);
            EvaluateLayers(source, offset, true, AvatarMaskBodyPart.Root, (mixer, graph) =>
            {
                mixer.SetInputWeight(1, 1); graph.Evaluate(.02f);
                foreach (var pair in bones.Select((b, i) => (b, i)))
                    Assert.That(Vector3.Distance(animator.GetBoneTransform(pair.b).position, expected[pair.i]), Is.LessThan(.002f), "Legacy / additive " + pair.b);
            }, sampleTime);
        }

        private AnimationClip Clip() { var clip = new AnimationClip(); assets.Add(clip); return clip; }

        [Test]
        public void FootHeight_PrebuildMigratesLegacySettings_KeepsOneFullPosePerState_AndEightBits()
        {
            root.AddComponent<VRCAvatarDescriptor>();
            var settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("ecde0b1d67f9e854992715d64f778d8c")), root.transform);
            var ps = settings.GetComponent<PosingSystem>();
            ps.features = new PosingSystemFeatureSettings { footHeight = true, mirror = false, breathing = false, fingerGestureControl = false,
                footHeightBackend = PosingSystemFeatureSettings.FootHeightBackend.ClipLevel };
            ps.autoImportAvatarAnimations = false; ps.isIconDisabled = true;
            ps.overrideDefines = new List<PosingSystem.OverrideAnimationDefine>();
            var source = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.Euler(90, 0, 0), "FootHeightTestPose");
            assets.Add(source);
            foreach (var name in HumanTrait.MuscleName)
                AnimationUtility.SetEditorCurve(source, EditorCurveBinding.FloatCurve("", typeof(Animator), name), AnimationCurve.Constant(0, 1, .1f));
            ps.defines = new List<PosingSystem.LayerDefine> { new("Sleep", "Sleep", "SitSleepUp", 30, "Sleep", new List<PosingSystem.AnimationDefine> {
                new() { enabled = true, displayName = "FootHeightTestPose", animationClip = source } }) };

            PosingSystemConverter.ConvertToModularAvatarComponents(ps);
            var controller = (AnimatorController)PosingSystemConverter.GetPrebuiltMergeAnimator(ps).animator;
            Assert.That(ps.features.footHeightBackend, Is.EqualTo(PosingSystemFeatureSettings.FootHeightBackend.PoseSpaceAdditive));
            Assert.That(PosingSystemFeatureUtility.OptionalBits(ps), Is.EqualTo(8));
            var fullPoses = controller.animationClips.Distinct().Where(c => c.name.Contains("FootHeightTestPose") &&
                AnimationUtility.GetCurveBindings(c).Length >= HumanTrait.MuscleCount).ToArray();
            var heightTrees = States(controller.layers.Single(l => l.name == "USSPS_Locomotion").stateMachine)
                .Select(state => state.motion).OfType<BlendTree>().Where(tree => tree.blendParameter == "FootHeight" &&
                    tree.children.Any(child => child.motion.name.Contains("FootHeightTestPose"))).ToArray();
            Assert.That(heightTrees, Is.Not.Empty);
            Assert.That(heightTrees.All(tree => tree.children.Length == 1), Is.True);
            // Three-point and full-body-tracking states each retain their neutral pose, not three height copies.
            Assert.That(fullPoses, Is.EquivalentTo(heightTrees.Select(tree => tree.children[0].motion).Distinct()));
            var layer = controller.layers.Single(l => l.name == PosingSystemConverter.FootHeightAdditiveLayer);
            Assert.That(layer.blendingMode, Is.EqualTo(AnimatorLayerBlendingMode.Additive));
            Assert.That(layer.syncedLayerIndex, Is.GreaterThanOrEqualTo(0));
            var states = States(controller.layers[layer.syncedLayerIndex].stateMachine).ToArray();
            Assert.That(states.All(s => layer.GetOverrideMotion(s) != null), Is.True);
            Assert.That(states.All(s => layer.GetOverrideBehaviours(s)?.Length == 0), Is.True);
            int compactCurves = PoseCurves();
            PosingSystemConverter.ConvertToModularAvatarComponents(ps);
            Assert.That(PoseCurves(), Is.EqualTo(compactCurves), "Repeated prebuild must not retain unused height variants");
            Assert.That(controller.layers.Count(l => l.name == PosingSystemConverter.FootHeightAdditiveLayer), Is.EqualTo(1));
            var currentSignature = PosingSystemConverter.GetDefineSerializeJson(ps);
            ps.data = System.Text.RegularExpressions.Regex.Replace(currentSignature, @"movement-final-v\d+:", "menu-controls-v4:");
            ps.features.footHeightBackend = PosingSystemFeatureSettings.FootHeightBackend.ClipLevel;
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps), Is.Not.EqualTo(ps.data),
                "Comparison-era prebuilds must be invalidated");
            PosingSystemConverter.ConvertToModularAvatarComponents(ps);
            Assert.That(controller.layers.Count(l => l.name == PosingSystemConverter.FootHeightAdditiveLayer), Is.EqualTo(1));
            Assert.That(PoseCurves(), Is.EqualTo(compactCurves));
            Assert.That(ps.data, Is.EqualTo(PosingSystemConverter.GetDefineSerializeJson(ps)));

            int PoseCurves() => controller.animationClips.Distinct().Where(c => c.name.Contains("FootHeightTestPose"))
                .Sum(c => AnimationUtility.GetCurveBindings(c).Length);
            IEnumerable<AnimatorState> States(AnimatorStateMachine machine) => machine.states.Select(s => s.state)
                .Concat(machine.stateMachines.SelectMany(m => States(m.stateMachine)));
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        public void FootHeight_GeneratedControllerMatchesLegacy_IncludingMirrorAndBlendTreeFallback(bool mirror, bool nested)
        {
            root.AddComponent<VRCAvatarDescriptor>();
            var settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("ecde0b1d67f9e854992715d64f778d8c")), root.transform);
            var ps = settings.GetComponent<PosingSystem>();
            ps.features = new PosingSystemFeatureSettings { mirror = true, breathing = false, fingerGestureControl = false,
                footHeightBackend = PosingSystemFeatureSettings.FootHeightBackend.PoseSpaceAdditive };
            testFolder = "Assets/USSPS_FeatureTests_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", testFolder.Substring(7));
            var controller = AnimatorController.CreateAnimatorControllerAtPath(testFolder + "/Base.controller");
            var layers = controller.layers; layers[0].name = "USSPS_Locomotion"; controller.layers = layers;
            controller.AddParameter("FootHeight", AnimatorControllerParameterType.Float);
            controller.AddParameter("VelocityX", AnimatorControllerParameterType.Float);
            var state = controller.layers[0].stateMachine.AddState("Pose");
            state.mirror = mirror; state.iKOnFeet = false; state.writeDefaultValues = true;
            var source = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.Euler(65, 20, 35), "ControllerPose");
            assets.Add(source);
            foreach (var muscle in HumanTrait.MuscleName)
                AnimationUtility.SetEditorCurve(source, EditorCurveBinding.FloatCurve("", typeof(Animator), muscle), AnimationCurve.Constant(0, 1, .1f));
            AnimationUtility.SetEditorCurve(source, EditorCurveBinding.FloatCurve("", typeof(Animator), "Left Upper Leg Front-Back"), AnimationCurve.Constant(0, 1, .3f));
            var proxy = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.identity, "proxy_Test"); assets.Add(proxy);
            var wrapper = new BlendTree { name = "_USSPS_Test_footheight", blendParameter = "FootHeight", useAutomaticThresholds = false };
            assets.Add(wrapper);
            wrapper.AddChild(HeightMotion(2), 0);
            wrapper.AddChild(HeightMotion(0), .5f);
            wrapper.AddChild(HeightMotion(-2), 1);
            state.motion = wrapper;
            PosingSystemConverter.GetCommonMergeAnimator(ps).animator = controller;
            var bones = new[] { HumanBodyBones.Hips, HumanBodyBones.Head, HumanBodyBones.LeftHand, HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot };
            var heights = new[] { 0f, .25f, .5f, .75f, 1f };
            var expected = heights.Select(Sample).ToArray();
            var legacyCurves = controller.animationClips.Distinct().Sum(c => AnimationUtility.GetCurveBindings(c).Length);
            var context = new BuildContext(root, null, false);
            try
            {
                PosingSystemConverter.ConvertOptionalFeatures(ps, context.AssetSaver);
                Assert.That(wrapper.children.Length, Is.EqualTo(nested ? 3 : 1));
                if (!nested)
                {
                    var compactCurves = controller.animationClips.Distinct().Sum(c => AnimationUtility.GetCurveBindings(c).Length);
                    Assert.That(compactCurves, Is.LessThan(legacyCurves * .6f), "Full pose curves must not be triplicated");
                    Debug.Log($"FootHeight curve comparison: legacy={legacyCurves}, additive={compactCurves}");
                }
                for (var h = 0; h < heights.Length; h++)
                {
                    var actual = Sample(heights[h]);
                    for (var i = 0; i < bones.Length; i++)
                        Assert.That(Vector3.Distance(actual[i], expected[h][i]), Is.LessThan(.002f), $"Height {heights[h]} / {bones[i]}");
                }
            }
            finally { context.DeactivateAllExtensionContexts(); }

            Motion HeightMotion(float level)
            {
                var clip = Object.Instantiate(source); assets.Add(clip);
                var info = AnimationUtility.GetAnimationClipSettings(clip); info.level = level;
                AnimationUtility.SetAnimationClipSettings(clip, info);
                if (!nested) return clip;
                var tree = new BlendTree { blendParameter = "VelocityX", useAutomaticThresholds = false };
                assets.Add(tree); tree.AddChild(clip, 0); tree.AddChild(proxy, 1);
                return tree;
            }
            Vector3[] Sample(float height)
            {
                var graph = PlayableGraph.Create("FootHeightControllerComparison");
                try
                {
                    graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                    var playable = AnimatorControllerPlayable.Create(graph, controller);
                    playable.SetFloat("FootHeight", height); playable.SetFloat("VelocityX", .35f);
                    AnimationPlayableOutput.Create(graph, "Avatar", animator).SetSourcePlayable(playable);
                    graph.Play(); graph.Evaluate(.02f);
                    return bones.Select(b => animator.GetBoneTransform(b).position).ToArray();
                }
                finally { graph.Destroy(); }
            }
        }

        [TestCase(false, 8, PosingSystemFeatureSettings.MovementBackend.BaseAdditive)]
        [TestCase(true, 53, PosingSystemFeatureSettings.MovementBackend.BaseAdditive)]
        [TestCase(true, 53, PosingSystemFeatureSettings.MovementBackend.AdditiveOverride)]
        [TestCase(true, 53, PosingSystemFeatureSettings.MovementBackend.AdditiveAdditive)]
        [TestCase(true, 53, PosingSystemFeatureSettings.MovementBackend.GestureAdditive)]
        [TestCase(true, 53, PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive)]
        [TestCase(true, 53, PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive, true)]
        [TestCase(true, 53, PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive, true, PosingSystemFeatureSettings.FloatingPoseSpaceMode.ReenterOnChange)]
        [TestCase(true, 53, PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive, true, PosingSystemFeatureSettings.FloatingPoseSpaceMode.ExitThenReenter)]
        [TestCase(true, 53, PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive, true, PosingSystemFeatureSettings.FloatingPoseSpaceMode.EnterEveryFrame)]
        [TestCase(true, 53, PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive, true, PosingSystemFeatureSettings.FloatingPoseSpaceMode.ReenterOnChangeImmediately)]
        [TestCase(true, 53, PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive, true, PosingSystemFeatureSettings.FloatingPoseSpaceMode.ReenterHeightImmediately)]
        [TestCase(true, 53, PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive, true, PosingSystemFeatureSettings.FloatingPoseSpaceMode.HeightMenuOnly)]
        [TestCase(true, 53, PosingSystemFeatureSettings.MovementBackend.BaseAdditive, true)]
        public void NdmfBuild_ExpressionParametersAndMenusMatchSelectedFeatures(bool enabled, int expectedBits, PosingSystemFeatureSettings.MovementBackend backend, bool additiveFootHeight = false,
            PosingSystemFeatureSettings.FloatingPoseSpaceMode floatingPoseSpace = PosingSystemFeatureSettings.FloatingPoseSpaceMode.Disabled)
        {
            var descriptor = root.AddComponent<VRCAvatarDescriptor>();
            descriptor.customExpressions = true;
            descriptor.expressionParameters = ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>();
            descriptor.expressionParameters.parameters = Array.Empty<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter>();
            assets.Add(descriptor.expressionParameters);
            var settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("ecde0b1d67f9e854992715d64f778d8c")), root.transform);
            var ps = settings.GetComponent<PosingSystem>();
            ps.features = new PosingSystemFeatureSettings { mirror=enabled, footHeight=enabled, breathing=enabled, fingerGestureControl=enabled,
                faceForward=enabled, photography=enabled, floating=enabled, rollover=enabled };
            // The settings prefab defaults breathing to OFF; this test explicitly exercises every feature.
            ps.additiveBreathing.enabled = enabled;
            ps.defines.Clear(); ps.overrideDefines = new List<PosingSystem.OverrideAnimationDefine>();
            ps.features.photographyBackend = ps.features.floatingBackend = backend;
            ps.features.floatingPoseSpace = floatingPoseSpace;
            ps.features.footHeightBackend = additiveFootHeight ? PosingSystemFeatureSettings.FootHeightBackend.PoseSpaceAdditive : PosingSystemFeatureSettings.FootHeightBackend.ClipLevel;
            if (enabled)
            {
                var pose = Clip(); pose.name = "TestSleep";
                AnimationUtility.SetEditorCurve(pose, EditorCurveBinding.FloatCurve("", typeof(Animator), "Head Turn Left-Right"), AnimationCurve.Constant(0, 1, .25f));
                ps.defines.Add(new PosingSystem.LayerDefine("Sleep", "Sleep", "SitSleepUp", 30, "Sleep", new List<PosingSystem.AnimationDefine> {
                    new() { enabled=true, displayName="Sleep", animationClip=pose } }));
                ps.features.rolloverAnimation = pose;
            }
            ps.autoImportAvatarAnimations = false; ps.isIconDisabled = true;
            var context = new BuildContext(root, null, false);
            try
            {
                var method = typeof(AvatarProcessor).GetMethod("ProcessAvatar", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
                    null, new[] { typeof(BuildContext), typeof(BuildPhase), typeof(BuildPhase) }, null);
                try { method.Invoke(null, new object[] { context, BuildPhase.Resolving, BuildPhase.Transforming }); }
                catch (System.Reflection.TargetInvocationException exception) { throw exception.InnerException ?? exception; }
                var parameters = descriptor.expressionParameters.parameters;
                var bits = parameters.Where(p => p.networkSynced).Sum(p =>
                    p.valueType == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.ValueType.Bool ? 1 : 8);
                Assert.That(bits, Is.EqualTo(expectedBits));
                var menus = new HashSet<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
                Collect(descriptor.expressionsMenu);
                if (enabled)
                {
                    var heightMenu = menus.SelectMany(m => m.controls).Single(c => c.type == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.RadialPuppet &&
                        c.subParameters.Any(p => p.name == PosingSystemFeatureUtility.FloatHeight));
                    Assert.That(heightMenu.parameter.name, Is.EqualTo(PosingSystemFeatureUtility.FloatHeightAdjusting));
                    Assert.That(heightMenu.value, Is.EqualTo(1));
                    var floorHeight = parameters.Single(p => p.name == PosingSystemFeatureUtility.FloatHeight);
                    Assert.That(floorHeight.defaultValue, Is.EqualTo(.5f));
                    Assert.That(floorHeight.saved, Is.False);
                    var adjusting = parameters.Single(p => p.name == PosingSystemFeatureUtility.FloatHeightAdjusting);
                    Assert.That(adjusting.networkSynced || adjusting.saved, Is.False);
                    Assert.That(adjusting.defaultValue, Is.Zero);
                    Assert.That(heightMenu.name, Is.EqualTo("床の高さ"));
                    Assert.That(parameters.Single(p => p.name == PosingSystemFeatureUtility.FloatActive).networkSynced, Is.False);
                    Assert.That(menus.SelectMany(m => m.controls).Any(c => c.parameter?.name == PosingSystemFeatureUtility.FloatActive), Is.False);
                    var photo = menus.SelectMany(m => m.controls).Single(c => c.name == "撮影モード").subMenu;
                    Assert.That(photo.controls.Select(c => c.name), Is.EqualTo(new[] { "姿勢選択", "距離", "向き", "足固定", "ジェスチャー無効", "撮影モードを終了" }));
                    var poseLinks = menus.SelectMany(m => m.controls).Where(c => c.name == "姿勢選択").ToArray();
                    Assert.That(poseLinks.Length, Is.EqualTo(2));
                    Assert.That(poseLinks[0].subMenu.controls.Select(c => c.name), Is.EquivalentTo(poseLinks[1].subMenu.controls.Select(c => c.name)));
                    Assert.That(menus.Any(m => m.controls.Contains(heightMenu) && m.controls.Any(c => c.name == "撮影モード")), Is.True);
                }
                var used = menus.SelectMany(m => m.controls).SelectMany(c => new[] { c.parameter?.name }
                    .Concat(c.subParameters?.Select(p => p.name) ?? Enumerable.Empty<string>())).Where(n => !string.IsNullOrEmpty(n)).ToArray();
                foreach (var parameter in new[] { "USSPS_Mirror", "FootHeight", "USSPS_Breathing", "USSPS_FingerGestureDisable_Menu",
                             PosingSystemFeatureUtility.PhotoDistance, PosingSystemFeatureUtility.FloatHeight, PosingSystemFeatureUtility.RollActive })
                    Assert.That(used.Contains(parameter), Is.EqualTo(enabled), parameter);
                Assert.That(parameters.SingleOrDefault(p => p.name == PosingSystemFeatureUtility.PhotoActive)?.networkSynced ?? false, Is.False);
                if (enabled)
                {
                    var finalBase = (AnimatorController)descriptor.baseAnimationLayers.Single(l => l.type == VRCAvatarDescriptor.AnimLayerType.Base).animatorController;
                    Assert.That(finalBase.layers.Any(l => l.name.Contains(PosingSystemConverter.FootHeightAdditiveLayer)), Is.True);
                    Assert.That(finalBase.layers.Any(l => l.name.Contains(PosingSystemConverter.FloatingPoseSpacePrefix + "Control")), Is.True);
                    Assert.That(finalBase.layers.Any(l => l.name.EndsWith("KeyDelta")), Is.False, "Legacy view modes must migrate too");
                    var offsets = descriptor.baseAnimationLayers.Where(l => l.animatorController != null)
                        .SelectMany(l => l.animatorController.animationClips).Distinct()
                        .Where(c => c.name.StartsWith("_USSPS_movement_offset_") && !c.name.EndsWith("Zero")).ToArray();
                    Assert.That(offsets, Is.Not.Empty, "NDMF must retain the generated pose offsets");
                    foreach (var offset in offsets)
                    {
                        var reference = AnimationUtility.GetAnimationClipSettings(offset);
                        Assert.That(reference.hasAdditiveReferencePose, Is.True, offset.name);
                        Assert.That(reference.additiveReferencePoseClip, Is.Not.Null, "Additive reference must survive NDMF: " + offset.name);
                    }
                }

                void Collect(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu menu)
                {
                    if (menu == null || !menus.Add(menu)) return;
                    foreach (var control in menu.controls) Collect(control.subMenu);
                }
            }
            finally { context.DeactivateAllExtensionContexts(); }
        }

        [TestCase(PosingSystemFeatureSettings.MovementBackend.BaseAdditive)]
        [TestCase(PosingSystemFeatureSettings.MovementBackend.AdditiveOverride)]
        [TestCase(PosingSystemFeatureSettings.MovementBackend.AdditiveAdditive)]
        [TestCase(PosingSystemFeatureSettings.MovementBackend.GestureAdditive)]
        public void OptionalFeatureBuild_MigratesBackends_GeneratesSensorsMasksAndTimePlayback_AndCanBeDisabled(PosingSystemFeatureSettings.MovementBackend backend)
        {
            root.AddComponent<VRCAvatarDescriptor>();
            var settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("ecde0b1d67f9e854992715d64f778d8c")), root.transform);
            var ps = settings.GetComponent<PosingSystem>();
            var source = Clip();
            AnimationUtility.SetEditorCurve(source, EditorCurveBinding.FloatCurve("", typeof(Animator), "Head Turn Left-Right"), AnimationCurve.Constant(0, 1, .2f));
            ps.defines = new List<PosingSystem.LayerDefine> { new("Sleep", "Sleep", "Sleep", 30, "Sleep", new List<PosingSystem.AnimationDefine> {
                new() { enabled=true, animationClip=source, syncdParameterValue=4 } }) };
            ps.features.faceForward = ps.features.photography = ps.features.floating = ps.features.rollover = true;
            ps.features.rolloverAnimation = source;
            ps.features.photographyBackend = ps.features.floatingBackend = backend;
            PosingSystemFeatureUtility.Synchronize(ps);
            testFolder = "Assets/USSPS_FeatureTests_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", testFolder.Substring(7));
            var controller = AnimatorController.CreateAnimatorControllerAtPath(testFolder + "/Base.controller");
            var layers = controller.layers; layers[0].name = "USSPS_Locomotion"; controller.layers = layers;
            var main = controller.layers[0];
            var poseState = main.stateMachine.AddState("Pose"); poseState.motion = source;
            poseState.mirrorParameter = "USSPS_Mirror"; poseState.mirrorParameterActive = true;
            var transition = main.stateMachine.AddEntryTransition(poseState);
            transition.conditions = PosingSystemConverter.GetEntryTransitionConditions(PosingSystemConverter.SplitSyncParameters(4, 4), true);
            PosingSystemConverter.GetCommonMergeAnimator(ps).animator = controller;
            var context = new BuildContext(root, null, false);
            PosingSystemConverter.ConvertOptionalFeatures(ps, context.AssetSaver);
            Assert.That(ps.features.photographyBackend, Is.EqualTo(PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive));
            Assert.That(ps.features.floatingBackend, Is.EqualTo(PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive));
            Assert.That(ps.GetComponentsInChildren<ModularAvatarMergeAnimator>().Any(m =>
                m.name == "USSPS_PhotoMotion" || m.name == "USSPS_FloatMotion"), Is.False);
            var movementLayers = controller.layers.Where(l => l.name.StartsWith(PosingSystemConverter.PoseSpaceMovementPrefix) && l.syncedLayerIndex >= 0).ToArray();
            Assert.That(movementLayers, Is.Not.Empty);
            Assert.That(movementLayers.All(l => l.blendingMode == AnimatorLayerBlendingMode.Additive && l.syncedLayerIndex == 0), Is.True);
            Assert.That(controller.layers.Count(l => l.name == PosingSystemConverter.FaceForwardLayer), Is.EqualTo(1));
            var face = controller.layers.Single(l => l.name == PosingSystemConverter.FaceForwardLayer);
            Assert.That(face.syncedLayerIndex, Is.Zero);
            Assert.That(face.avatarMask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.Head), Is.True);
            Assert.That(face.avatarMask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.Body), Is.False);
            Assert.That(face.GetOverrideMotion(poseState), Is.TypeOf<BlendTree>());
            var tracking = ps.GetComponentsInChildren<ModularAvatarMergeAnimator>().Single(m => m.name == "USSPS_PhotoTracking");
            var tc = (AnimatorController)tracking.animator;
            var trackingStates = tc.layers.SelectMany(l => l.stateMachine.states).Select(s => s.state).ToArray();
            Assert.That(trackingStates.SelectMany(s => s.behaviours).OfType<VRCAvatarParameterDriver>()
                .SelectMany(d => d.parameters).Any(p => p.name.EndsWith("TrackingLock")), Is.False, "Photography must preserve saved manual locks");
            Assert.That(trackingStates.Single(s => s.name == "撮影中").behaviours.OfType<VRCAnimatorTrackingControl>().Single().trackingHead,
                Is.EqualTo(VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType.Animation));
            Assert.That(trackingStates.SelectMany(s => s.behaviours).OfType<VRCAnimatorLocomotionControl>(), Is.Empty,
                "Photography must allow the user to move before manually locking their feet");
            foreach (var request in trackingStates.SelectMany(s => s.behaviours).OfType<VRCAnimatorTrackingControl>())
            {
                Assert.That(request.trackingHip, Is.EqualTo(VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType.NoChange));
                Assert.That(request.trackingLeftFoot, Is.EqualTo(VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType.NoChange));
                Assert.That(request.trackingRightFoot, Is.EqualTo(VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType.NoChange));
            }
            var roll = (AnimatorController)ps.GetComponentsInChildren<ModularAvatarMergeAnimator>().Single(m => m.name == "USSPS_RolloverController").animator;
            var rollState = roll.layers[1].stateMachine.states.Single(s => s.state.name.StartsWith("寝返り_")).state;
            Assert.That(rollState.timeParameterActive, Is.True);
            Assert.That(rollState.timeParameter, Is.EqualTo(PosingSystemFeatureUtility.RollTime));
            Assert.That(roll.layers[1].avatarMask.GetHumanoidBodyPartActive(AvatarMaskBodyPart.Head), Is.False);
            Assert.That(root.GetComponentsInChildren<Transform>().Count(t => t.name == PosingSystemHeadSensorUtility.SensorRoot), Is.EqualTo(1));
            PosingSystemConverter.ConvertOptionalFeatures(ps, context.AssetSaver);
            Assert.That(root.GetComponentsInChildren<Transform>().Count(t => t.name == "USSPS_HeadDirectionProbe"), Is.EqualTo(1), "Repeated builds must not duplicate contacts");
            ps.features.faceForward = ps.features.photography = ps.features.floating = ps.features.rollover = false;
            ps.features.mirror = ps.features.footHeight = false;
            PosingSystemFeatureUtility.Synchronize(ps);
            PosingSystemConverter.ConvertOptionalFeatures(ps, context.AssetSaver);
            Assert.That(controller.layers.Length, Is.EqualTo(1));
            Assert.That(poseState.mirrorParameterActive, Is.False);
            ps.features.mirror = true;
            PosingSystemConverter.ConvertOptionalFeatures(ps, context.AssetSaver);
            Assert.That(poseState.mirrorParameterActive, Is.True);
            Assert.That(root.GetComponentsInChildren<Transform>().Any(t => t.name == PosingSystemHeadSensorUtility.SensorRoot || t.name == "USSPS_HeadDirectionProbe"), Is.False);
        }

        private void EvaluateLayers(AnimationClip first, AnimationClip second, bool additive, AvatarMaskBodyPart part,
            Action<AnimationLayerMixerPlayable, PlayableGraph> check, double sampleTime = 0)
        {
            var graph = PlayableGraph.Create("FeatureRegression");
            try
            {
                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                var mixer = AnimationLayerMixerPlayable.Create(graph, 2);
                var firstPlayable = AnimationClipPlayable.Create(graph, first);
                var secondPlayable = AnimationClipPlayable.Create(graph, second);
                firstPlayable.SetApplyFootIK(false);
                secondPlayable.SetApplyFootIK(false);
                firstPlayable.SetTime(sampleTime);
                secondPlayable.SetTime(sampleTime);
                graph.Connect(firstPlayable, 0, mixer, 0);
                graph.Connect(secondPlayable, 0, mixer, 1);
                mixer.SetInputWeight(0, 1);
                mixer.SetLayerAdditive(1, additive);
                var mask = new AvatarMask(); assets.Add(mask);
                for (var i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++) mask.SetHumanoidBodyPartActive((AvatarMaskBodyPart)i, part == AvatarMaskBodyPart.Root || i == (int)part);
                mixer.SetLayerMaskFromAvatarMask(1, mask);
                AnimationPlayableOutput.Create(graph, "Avatar", animator).SetSourcePlayable(mixer);
                graph.Play();
                check(mixer, graph);
            }
            finally { graph.Destroy(); }
        }
    }
}
