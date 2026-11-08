using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using jp.unisakistudio.posingsystem;
using nadena.dev.ndmf;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(0, 0)]
        [TestCase(90, 0)]
        [TestCase(-90, 0)]
        [TestCase(45, 0)]
        [TestCase(90, 90)]
        [TestCase(45, -65)]
        [TestCase(15, 60, true)]
        [TestCase(90, 0, false, true)]
        public void PoseSpaceMovement_KeepsTranslationVerticalAndForward_AndYawUpright(float tilt, float yaw, bool animated = false, bool clipMirror = false)
        {
            var source = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.Euler(tilt, 25, 30), "TiltedPose");
            assets.Add(source);
            if (animated)
            {
                var from = Quaternion.Euler(tilt, 25, 30); var to = Quaternion.Euler(tilt + 100, 40, 55);
                for (var axis = 0; axis < 4; axis++) AnimationUtility.SetEditorCurve(source,
                    EditorCurveBinding.FloatCurve("", typeof(Animator), "RootQ." + "xyzw"[axis]), AnimationCurve.Linear(0, from[axis], 1, to[axis]));
            }
            var info = AnimationUtility.GetAnimationClipSettings(source); info.mirror = clipMirror;
            AnimationUtility.SetAnimationClipSettings(source, info);
            AnimationUtility.SetEditorCurve(source, EditorCurveBinding.FloatCurve("", typeof(Animator), "Left Upper Leg Front-Back"), AnimationCurve.Constant(0, 1, .3f));
            var zero = PosingSystemConverter.RootOffsetClip(Vector3.zero, Quaternion.identity, "Reference"); assets.Add(zero);
            var delta = new Vector3(0, .7f, 1.3f);
            var rotation = Quaternion.Euler(0, yaw, 0);
            var correction = PosingSystemConverter.CreatePoseSpaceOffsetClip(source, delta, rotation, zero); assets.Add(correction);
            var reference = AnimationUtility.GetAnimationClipSettings(correction);
            Assert.That(reference.hasAdditiveReferencePose, Is.True);
            Assert.That(reference.additiveReferencePoseClip, Is.SameAs(zero));
            var units = Vector3.one * animator.humanScale;
            var bones = new[] { HumanBodyBones.Hips, HumanBodyBones.Head, HumanBodyBones.LeftHand, HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot };
            foreach (var time in animated ? new[] { .15, .75 } : new[] { 0.0 })
                EvaluateLayers(source, correction, true, AvatarMaskBodyPart.Root, (mixer, graph) =>
                {
                    mixer.SetInputWeight(1, 0); graph.Evaluate(0);
                    var before = bones.Select(b => animator.GetBoneTransform(b).position).ToArray();
                    mixer.SetInputWeight(1, 1); graph.Evaluate(0);
                    var after = bones.Select(b => animator.GetBoneTransform(b).position).ToArray();
                    for (var i = 0; i < bones.Length; i++)
                    {
                        Assert.That(Vector3.Distance(after[i] - after[0], rotation * (before[i] - before[0])), Is.LessThan(.003f), "Preserve pose: " + bones[i]);
                        Assert.That(after[i].y - before[i].y, Is.EqualTo(delta.y * units.y).Within(.003f), "Yaw must not tilt: " + bones[i]);
                        if (yaw == 0) Assert.That(Vector3.Distance(after[i] - before[i], Vector3.Scale(delta, units)), Is.LessThan(.003f), "Horizontal/vertical translation: " + bones[i]);
                    }
                    mixer.SetInputWeight(1, 0); graph.Evaluate(0);
                    for (var i = 0; i < bones.Length; i++) Assert.That(Vector3.Distance(animator.GetBoneTransform(bones[i]).position, before[i]), Is.LessThan(.002f));
                }, time);
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(true, true)]
        public void PoseSpaceMovement_GeneratedController_CombinesModesMirrorsAndRestores(bool mirror, bool additiveHeight)
        {
            root.AddComponent<VRCAvatarDescriptor>();
            var settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("ecde0b1d67f9e854992715d64f778d8c")), root.transform);
            var ps = settings.GetComponent<PosingSystem>();
            ps.features = new PosingSystemFeatureSettings { photography = true, floating = true, footHeight = true,
                footHeightBackend = additiveHeight ? PosingSystemFeatureSettings.FootHeightBackend.PoseSpaceAdditive : PosingSystemFeatureSettings.FootHeightBackend.ClipLevel,
                photographyBackend = PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive,
                floatingBackend = PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive };
            testFolder = "Assets/USSPS_FeatureTests_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", testFolder.Substring(7));
            var controller = AnimatorController.CreateAnimatorControllerAtPath(testFolder + "/Base.controller");
            var layers = controller.layers; layers[0].name = "USSPS_Locomotion"; controller.layers = layers;
            controller.AddParameter("USSPS_Mirror", AnimatorControllerParameterType.Bool);
            controller.AddParameter("UseProxy", AnimatorControllerParameterType.Bool);
            controller.AddParameter("FootHeight", AnimatorControllerParameterType.Float);
            var state = controller.layers[0].stateMachine.AddState("Pose");
            state.mirrorParameter = "USSPS_Mirror"; state.mirrorParameterActive = true; state.iKOnFeet = false;
            var source = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.Euler(65, 20, 35), "ControllerPose"); assets.Add(source);
            AnimationUtility.SetEditorCurve(source, EditorCurveBinding.FloatCurve("", typeof(Animator), "Left Upper Leg Front-Back"), AnimationCurve.Constant(0, 1, .3f));
            var height = new BlendTree { name = "_USSPS_Controller_footheight", blendParameter = "FootHeight", useAutomaticThresholds = false }; assets.Add(height);
            foreach (var value in new[] { 0f, .5f, 1f })
            {
                var clip = Object.Instantiate(source); assets.Add(clip);
                var clipSettings = AnimationUtility.GetAnimationClipSettings(clip); clipSettings.level = 2 - 4 * value;
                AnimationUtility.SetAnimationClipSettings(clip, clipSettings); height.AddChild(clip, value);
            }
            state.motion = height;
            var proxy = controller.layers[0].stateMachine.AddState("Walking");
            proxy.motion = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.identity, "proxy_Test"); assets.Add(proxy.motion);
            var transition = state.AddTransition(proxy); transition.hasExitTime = false; transition.duration = 0; transition.AddCondition(AnimatorConditionMode.If, 0, "UseProxy");
            transition = proxy.AddTransition(state); transition.hasExitTime = false; transition.duration = 0; transition.AddCondition(AnimatorConditionMode.IfNot, 0, "UseProxy");
            PosingSystemConverter.GetCommonMergeAnimator(ps).animator = controller;
            var context = new BuildContext(root, null, false);
            var bones = new[] { HumanBodyBones.Hips, HumanBodyBones.Head, HumanBodyBones.LeftHand, HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot };
            try
            {
                var expected = Sample(false, false, .5f, false);
                var walk = Sample(false, false, .5f, true);
                PosingSystemConverter.ConvertOptionalFeatures(ps, context.AssetSaver);
                PosingSystemConverter.ConvertOptionalFeatures(ps, context.AssetSaver);
                Assert.That(controller.layers[0].blendingMode, Is.EqualTo(AnimatorLayerBlendingMode.Override));
                var movement = controller.layers.Where(l => l.name.StartsWith(PosingSystemConverter.PoseSpaceMovementPrefix) && l.syncedLayerIndex >= 0).ToArray();
                Assert.That(movement.Length, Is.EqualTo(5));
                Assert.That(movement.All(l => l.syncedLayerIndex == 0 && l.GetOverrideBehaviours(state).Length == 0), Is.True);
                Assert.That(movement.Where(l => l.name.EndsWith("_Forward") || l.name.EndsWith("_Yaw") || l.name.EndsWith("_Vertical")).All(l => l.GetOverrideMotion(proxy) is BlendTree), Is.True, "Default locomotion participates in movement");
                var facing = Sample(true, false, 1, false, distance: 0);
                foreach (var photo in new[] { false, true }) foreach (var floating in new[] { false, true })
                foreach (var floorHeight in floating ? new[] { 0f, .25f, .5f, .7f, 1f } : new[] { .5f })
                {
                    var actual = Sample(photo, floating, .5f, false, floorHeight: floorHeight);
                    var delta = new Vector3(0, floating ? (floorHeight - .5f) * 6 : 0, photo ? 1.5f : 0);
                    var baseline = photo ? facing : expected;
                    for (var i = 0; i < bones.Length; i++) Assert.That(Vector3.Distance(actual[i], baseline[i] + delta), Is.LessThan(.004f), $"photo={photo}, floating={floating}, {bones[i]}");
                }
                foreach (var distance in new[] { 0f, 1f / 255, .5f, 1f })
                foreach (var yaw in new[] { 0f, .125f, .25f, .5f, .75f, .875f, 1f })
                {
                    var actual = Sample(true, true, yaw, false, distance: distance);
                    var angle = (yaw - .5f) * 360 + (distance > 0 ? 180 : 0);
                    var rotation = Quaternion.Euler(0, angle, 0);
                    var baselineYaw = distance > 0 ? (yaw <= .5f ? yaw + .5f : yaw - .5f) : yaw;
                    var baseline = Sample(true, true, baselineYaw, false, distance: 0);
                    for (var i = 0; i < bones.Length; i++)
                    {
                        Assert.That(Vector3.Distance(actual[i] - actual[0], rotation * (expected[i] - expected[0])), Is.LessThan(.004f), $"Yaw/mirror at distance={distance}, yaw={yaw}");
                        Assert.That(Vector3.Distance(actual[i] - baseline[i], Vector3.forward * distance * 3), Is.LessThan(.004f), "Auto-facing must not reverse or tilt forward translation");
                    }
                }
                var returnedToZero = Sample(true, false, .75f, false, returnDistanceToZero: true);
                var yawAtOrigin = Sample(true, false, .75f, false, distance: 0);
                for (var i = 0; i < bones.Length; i++) Assert.That(Vector3.Distance(returnedToZero[i], yawAtOrigin[i]), Is.LessThan(.004f), "Returning distance to zero must remove only the automatic half-turn");
                var walkingWithFeature = Sample(true, true, .75f, true);
                var walkingFacing = Sample(true, true, .25f, true, distance: 0);
                for (var i = 0; i < bones.Length; i++) Assert.That(Vector3.Distance(walkingWithFeature[i] - walkingFacing[i], Vector3.forward * 1.5f), Is.LessThan(.004f), "Move default locomotion");
                var walkingRestored = Sample(true, true, .75f, true, true);
                for (var i = 0; i < bones.Length; i++) Assert.That(Vector3.Distance(walkingRestored[i], walk[i]), Is.LessThan(.004f), "Restore default locomotion");
                var restored = Sample(true, true, .75f, false, true);
                for (var i = 0; i < bones.Length; i++) Assert.That(Vector3.Distance(restored[i], expected[i]), Is.LessThan(.004f), "Return to original pose after switching off");
                ps.features.photography = ps.features.floating = false;
                PosingSystemConverter.ConvertOptionalFeatures(ps, context.AssetSaver);
                Assert.That(controller.layers.Any(l => l.name.StartsWith(PosingSystemConverter.PoseSpaceMovementPrefix)), Is.False);
            }
            finally { context.DeactivateAllExtensionContexts(); }

            Vector3[] Sample(bool photo, bool floating, float yaw, bool walking, bool switchOff = false, float distance = .5f, bool returnDistanceToZero = false, float floorHeight = .7f)
            {
                var graph = PlayableGraph.Create("PoseSpaceControllerComparison");
                try
                {
                    graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                    var playable = AnimatorControllerPlayable.Create(graph, controller);
                    playable.SetBool("USSPS_Mirror", mirror); playable.SetBool("UseProxy", walking);
                    playable.SetFloat("FootHeight", .8f);
                    if (controller.parameters.Any(p => p.name == PosingSystemFeatureUtility.PhotoActive))
                    {
                        playable.SetBool(PosingSystemFeatureUtility.PhotoActive, photo); playable.SetBool(PosingSystemFeatureUtility.FloatActive, floating);
                        playable.SetFloat(PosingSystemFeatureUtility.PhotoDistance, distance); playable.SetFloat(PosingSystemFeatureUtility.PhotoYaw, yaw);
                        playable.SetFloat(PosingSystemFeatureUtility.FloatHeight, floorHeight);
                    }
                    AnimationPlayableOutput.Create(graph, "Avatar", animator).SetSourcePlayable(playable);
                    graph.Play(); for (var i = 0; i < 12; i++) graph.Evaluate(.02f);
                    if (returnDistanceToZero)
                    {
                        playable.SetFloat(PosingSystemFeatureUtility.PhotoDistance, 0);
                        for (var i = 0; i < 12; i++) graph.Evaluate(.02f);
                    }
                    if (switchOff)
                    {
                        playable.SetBool(PosingSystemFeatureUtility.PhotoActive, false); playable.SetBool(PosingSystemFeatureUtility.FloatActive, false);
                        for (var i = 0; i < 6; i++) graph.Evaluate(.02f);
                    }
                    return bones.Select(b => animator.GetBoneTransform(b).position).ToArray();
                }
                finally { graph.Destroy(); }
            }
        }

        [Test]
        public void FaceForward_SupinePreservesOriginalHead_AndGatesOnAutomaticLockPoseType()
        {
            root.AddComponent<VRCAvatarDescriptor>();
            var settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("ecde0b1d67f9e854992715d64f778d8c")), root.transform);
            var ps = settings.GetComponent<PosingSystem>();
            ps.features = new PosingSystemFeatureSettings { faceForward = true, footHeight = false };
            var source = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.Euler(90, 0, 0), "Supine"); assets.Add(source);
            ps.defines = new List<PosingSystem.LayerDefine> {
                new("Other", "Other", "SitSleepDown", 31, "Other", new List<PosingSystem.AnimationDefine> { new() { enabled=true, animationClip=source, syncdParameterValue=4 } }),
                new("Supine", "Supine", "SitSleepUp", 32, "Supine", new List<PosingSystem.AnimationDefine> { new() { enabled=true, animationClip=source, syncdParameterValue=5 } }) };
            testFolder = "Assets/USSPS_FeatureTests_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", testFolder.Substring(7));
            var controller = AnimatorController.CreateAnimatorControllerAtPath(testFolder + "/Base.controller");
            var layers = controller.layers; layers[0].name = "USSPS_Locomotion"; controller.layers = layers;
            var main = controller.layers[0];
            var states = new List<AnimatorState>();
            foreach (var value in new[] { 4, 5 }) foreach (var vr in new[] { false, true })
            {
                var state = main.stateMachine.AddState(value + "_" + vr); state.motion = source; states.Add(state);
                main.stateMachine.AddEntryTransition(state).conditions = PosingSystemConverter.GetEntryTransitionConditions(PosingSystemConverter.SplitSyncParameters(value, 5), vr);
            }
            PosingSystemConverter.GetCommonMergeAnimator(ps).animator = controller;
            var context = new BuildContext(root, null, false);
            try
            {
                PosingSystemConverter.ConvertOptionalFeatures(ps, context.AssetSaver);
                var face = controller.layers.Single(l => l.name == PosingSystemConverter.FaceForwardLayer);
                foreach (var state in states.Take(2)) Assert.That(face.GetOverrideMotion(state), Is.TypeOf<BlendTree>());
                foreach (var state in states.Skip(2)) Assert.That(face.GetOverrideMotion(state), Is.SameAs(source), "Supine must retain original upward head even without manual lock");
                var gate = controller.layers.Single(l => l.name == "USSPS_FaceForwardSwitch");
                var on = gate.stateMachine.states.Single(s => s.state.name == "正面に補正").state;
                Assert.That(on.transitions.Any(t => t.conditions.Any(c => c.parameter == "LocomotionType" && c.mode == AnimatorConditionMode.Equals && c.threshold == 32)), Is.True);
            }
            finally { context.DeactivateAllExtensionContexts(); }
        }
    }
}
