using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void PerPoseRollover_ConversionAndOptionalFeatures_KeepSeparateClipsAndSupportRebuild(bool prebuilt)
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
                new(source, "A") { isRollover = true, syncdParameterValue = 4, typeParameterValue = 1 },
                new(second, "B") { isRollover = true, syncdParameterValue = 5, typeParameterValue = 2 },
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

        private void AssertConvertedHeadCentering(Motion motion, bool rollover)
        {
            while (motion is BlendTree tree) motion = tree.children.OrderBy(c => Mathf.Abs(c.threshold - .5f)).First().motion;
            Assert.That(motion.name.Contains("_USSPS_RolloverCentered"), Is.EqualTo(rollover));
            var clip = (AnimationClip)motion;
            var working = Object.Instantiate(root);
            try
            {
                var head = working.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
                var maxOffset = 0f;
                for (var i = 0; i <= 30; i++)
                {
                    clip.SampleAnimation(working, clip.length * i / 30);
                    working.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    maxOffset = Mathf.Max(maxOffset, new Vector2(head.position.x, head.position.z).magnitude);
                }
                if (rollover) Assert.That(maxOffset, Is.LessThan(.0015f), "Converted / persisted rollover");
                else Assert.That(maxOffset, Is.GreaterThan(.002f), "Ordinary poses retain their authored head movement");
            }
            finally { Object.DestroyImmediate(working); }
        }
    }

    public class PosingSystemRolloverPoseTests
    {
        private string folder;
        private AnimatorController controller;
        private GameObject root;
        private PlayableGraph graph;
        private static object Invoke(string name, params object[] args) => typeof(PosingSystemConverter)
            .GetMethod(name, BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);

        [SetUp]
        public void SetUp()
        {
            folder = "Assets/USSPS_RolloverPoseTests_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folder.Substring(7));
            controller = AnimatorController.CreateAnimatorControllerAtPath(folder + "/Base.controller");
        }

        [TearDown]
        public void TearDown()
        {
            if (graph.IsValid()) graph.Destroy();
            if (root != null) Object.DestroyImmediate(root);
            AssetDatabase.DeleteAsset(folder);
        }

        [Test]
        public void Rollover_OverridesCustomTimeAndRestoresItWhenDisabled_WithoutEmptyParameters()
        {
            var state = controller.layers[0].stateMachine.AddState("Pose");
            var pose = new PosingSystem.AnimationDefine { isRollover = true, isMotionTime = true, motionTimeParamName = "ManualTime" };
            Invoke("ConfigurePoseMotionTime", controller, state, pose);
            Assert.That(state.timeParameterActive, Is.True);
            Assert.That(state.timeParameter, Is.EqualTo(PosingSystemFeatureUtility.RollTime));
            Assert.That(controller.parameters.Single().defaultFloat, Is.EqualTo(.5f));
            Assert.That(pose.motionTimeParamName, Is.EqualTo("ManualTime"));
            pose.isRollover = false;
            Invoke("ConfigurePoseMotionTime", controller, state, pose);
            Assert.That(state.timeParameter, Is.EqualTo("ManualTime"));
            pose.isMotionTime = false;
            Invoke("ConfigurePoseMotionTime", controller, state, pose);
            Assert.That(state.timeParameterActive, Is.False);
            Assert.That(state.timeParameter, Is.Empty);
            Assert.That(controller.parameters.Any(p => string.IsNullOrWhiteSpace(p.name)), Is.False);
        }

        [Test]
        public void Rollover_EmptyManualParameterIsAllowedAndAutomaticallyAssigned()
        {
            var state = controller.layers[0].stateMachine.AddState("Pose");
            Invoke("ConfigurePoseMotionTime", controller, state, new PosingSystem.AnimationDefine { isRollover = true, isMotionTime = true });
            Assert.That(state.timeParameter, Is.EqualTo(PosingSystemFeatureUtility.RollTime));
            Assert.That(controller.parameters.Length, Is.EqualTo(1));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Rollover_PlaybackRespondsToHeadTilt_AndCentersWhenOffLockedOrPhotographing(bool reload)
        {
            root = new GameObject("Rollover playback test");
            var animator = root.AddComponent<Animator>();
            var probe = new GameObject("Probe"); probe.transform.SetParent(root.transform, false);
            var clip = new AnimationClip { name = "LeftCenterRight" };
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("Probe", typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Linear(0, 0, 1, 1));
            AssetDatabase.AddObjectToAsset(clip, controller);
            var state = controller.layers[0].stateMachine.AddState("Rollover pose");
            state.motion = clip; state.writeDefaultValues = false;
            Invoke("ConfigurePoseMotionTime", controller, state, new PosingSystem.AnimationDefine { isRollover = true });
            Invoke("BuildRolloverTime", controller, new PosingSystemFeatureSettings(), null);
            if (reload)
            {
                EditorUtility.SetDirty(controller); AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(folder + "/Base.controller", ImportAssetOptions.ForceUpdate);
                controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(folder + "/Base.controller");
            }
            graph = PlayableGraph.Create("Rollover time test");
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            var playable = AnimatorControllerPlayable.Create(graph, controller);
            AnimationPlayableOutput.Create(graph, "Pose", animator).SetSourcePlayable(playable);
            graph.Play();
            Check(.5f);
            playable.SetBool(PosingSystemFeatureUtility.RollActive, true);
            playable.SetFloat(PosingSystemFeatureUtility.RollSensor, .95f); Check(1);
            playable.SetFloat(PosingSystemFeatureUtility.RollSensor, .05f); Check(0);
            playable.SetFloat(PosingSystemFeatureUtility.RollSensor, .5f); Check(.5f);
            playable.SetFloat(PosingSystemFeatureUtility.RollSensor, .95f); Check(1);
            playable.SetBool("HeadTrackingLock", true); Check(.5f);
            playable.SetBool("HeadTrackingLock", false); Check(1);
            playable.SetBool(PosingSystemFeatureUtility.PhotoActive, true); Check(.5f);
            playable.SetBool(PosingSystemFeatureUtility.PhotoActive, false); Check(1);
            playable.SetBool(PosingSystemFeatureUtility.RollActive, false); Check(.5f);
            playable.SetBool(PosingSystemFeatureUtility.RollActive, true);
            playable.SetFloat(PosingSystemFeatureUtility.RollSensor, 0); Check(.5f);

            void Check(float expected)
            {
                for (var i = 0; i < 10; i++) graph.Evaluate(1f / 60);
                Assert.That(playable.GetFloat(PosingSystemFeatureUtility.RollTime), Is.EqualTo(expected).Within(.025f), "Calculated time");
                Assert.That(probe.transform.localPosition.x, Is.EqualTo(expected).Within(.025f), "Actual sampled pose");
            }
        }
    }
}
