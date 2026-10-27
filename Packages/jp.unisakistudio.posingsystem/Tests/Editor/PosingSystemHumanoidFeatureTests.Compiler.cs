using System;
using System.Linq;
using System.Collections.Generic;
using nadena.dev.ndmf;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    [UnityEngine.TestTools.PrebuildSetup(typeof(ControllerCompilerBatchSetup))]
    [UnityEngine.TestTools.PostBuildCleanup(typeof(ControllerCompilerBatchSetup))]
    public partial class PosingSystemHumanoidFeatureTests
    {
        static IEnumerable<AnimatorState> CompilerStates(AnimatorStateMachine machine)
        {
            foreach (var child in machine.states) yield return child.state;
            foreach (var child in machine.stateMachines)
                foreach (var state in CompilerStates(child.stateMachine)) yield return state;
        }
        static (int select, int sync) CompilerBinding(AnimatorController controller, string parameter)
        {
            var entry = controller.layers.Single(l => l.name == "USSPS_LocomotionType").stateMachine.entryTransitions
                .Single(t => t.conditions.Any(c => c.parameter == parameter && c.mode == AnimatorConditionMode.Equals));
            var selection = (int)entry.conditions.Single(c => c.parameter == parameter).threshold;
            var sync = (int)entry.destinationState.behaviours.OfType<VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver>()
                .SelectMany(d => d.parameters).Single(p => p.name == "USSPS_Pose").value;
            Assert.That(selection, Is.GreaterThan(0));
            Assert.That(sync, Is.GreaterThan(2));
            return (selection, sync);
        }
        PosingSystem CompilerSettings(int poseCount = 5)
        {
            root.AddComponent<VRCAvatarDescriptor>();
            var settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("ecde0b1d67f9e854992715d64f778d8c")), root.transform);
            var ps = settings.GetComponent<PosingSystem>();
            ps.autoImportAvatarAnimations = false; ps.isIconDisabled = true; ps.enablePoseIk = false;
            ps.features = new PosingSystemFeatureSettings { mirror=false, footHeight=false, breathing=false, fingerGestureControl=false };
            ps.defines.Clear(); ps.overrideDefines.Clear();
            var categories = new[] {("SitStand", 1), ("SitShallow", 12), ("SitDeep", 22), ("SitSleepUp", 32), ("SitSleepDown", 33)};
            var clip = Clip(); clip.name = "CompilerPose";
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), "Spine Front-Back"), AnimationCurve.Constant(0, 1, .2f));
            for (var i = 0; i < poseCount; i++)
            {
                var category = categories[i % categories.Length];
                var layer = ps.defines.FirstOrDefault(d => d.paramName == category.Item1);
                if (layer == null) { layer = new PosingSystem.LayerDefine("", category.Item1, category.Item1, category.Item2, category.Item1, new()); ps.defines.Add(layer); }
                layer.animations.Add(new PosingSystem.AnimationDefine { enabled=true, displayName="CompilerPose" + i, animationClip=clip });
            }
            return ps;
        }

        AnimatorController CompileSettings(PosingSystem ps, bool optimize)
        {
            PosingSystemConverter.GetCommonMergeAnimator(ps).animator = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(PosingSystemControllerGeneration.TemplateGuid));
            ps.data = "";
            var context = new BuildContext(root, null, false);
            try
            {
                using (new PosingSystemControllerGeneration.Scope(root, optimize))
                    PosingSystemConverter.ConvertToModularAvatarComponents(ps, context.AssetSaver);
                var controller = (AnimatorController)PosingSystemConverter.GetCommonMergeAnimator(ps).animator;
                foreach (var obj in PosingSystemControllerGraph.Collect(new[] { controller }, o => PosingSystemControllerGraph.IsGraphObject(o) && !EditorUtility.IsPersistent(o)))
                    if (!assets.Contains(obj)) assets.Add(obj);
                return controller;
            }
            finally { context.DeactivateAllExtensionContexts(); }
        }

        [TestCase(1)] [TestCase(16)] [TestCase(64)] [TestCase(128)]
        public void Compiler_PERF01_ControllerGrowthIsLinearInPoseCount(int count)
        {
            var ps = CompilerSettings(count);
            var original = CompileSettings(ps, false);
            var compiled = CompileSettings(ps, true);
            var a = PosingSystemControllerGraph.Collect(new[] {original}, o => !EditorUtility.IsPersistent(o));
            var b = PosingSystemControllerGraph.Collect(new[] {compiled}, o => !EditorUtility.IsPersistent(o));
            var before = a.OfType<AnimatorTransitionBase>().Count(); var after = b.OfType<AnimatorTransitionBase>().Count();
            var conditions = b.OfType<AnimatorTransitionBase>().Sum(t => t.conditions.Length);
            Assert.That(b.OfType<AnimatorState>().Count(), Is.EqualTo(a.OfType<AnimatorState>().Count()));
            Assert.That(after - before, Is.InRange(0, 140), "Extra branches must stay bounded independently of pose count");
            Assert.That(after, Is.LessThan(400 + count * 30));
            Assert.That(conditions, Is.LessThan(1500 + count * 60));
            Assert.That(compiled.parameters.Select(p => p.name), Is.EqualTo(original.parameters.Select(p => p.name)));
            TestContext.WriteLine($"poses={count}: states={b.OfType<AnimatorState>().Count()}, transitions={before}->{after}, conditions={conditions}");
        }

        [TestCase(false, 30)] [TestCase(false, 60)] [TestCase(false, 90)] [TestCase(false, 144)]
        [TestCase(true, 30)] [TestCase(true, 90)] [TestCase(true, 90, 6)]
        public void Compiler_MEM_TRK_RealGraphMatchesAfterLocksSleepAndRapidInputs(bool vr, int fps, int trackingType = 3)
        {
            var ps = CompilerSettings();
            var original = CompileSettings(ps, false); var compiled = CompileSettings(ps, true);
            var baseline = Run(original); var actual = Run(compiled);
            CollectionAssert.AreEqual(baseline, actual);

            List<string> Run(AnimatorController controller)
            {
                using var rig = new ControllerCompilerRuntimeProbe(animator, controller, true, vr);
                rig.Set("TrackingType", trackingType);
                foreach (var layer in ps.defines) rig.Set(layer.paramName, CompilerBinding(controller, layer.paramName).select);
                var observations = new List<string>();
                Hold("standing", 1);
                Hold("crouch", .6f);
                Assert.That(rig.Playable.GetInteger("LocomotionTypeGroup"), Is.EqualTo(1));
                Assert.That(rig.Playable.GetInteger("LocomotionType"), Is.EqualTo(12));
                Assert.That(rig.Playable.GetInteger("USSPS_Pose"), Is.EqualTo(CompilerBinding(controller, "SitShallow").sync));
                rig.Set("LocomotionLock", 1); Hold("locked-standing-input", 1);
                rig.Set("LocomotionLock", 0); Hold("unlock", 1);
                rig.Set("USSPS_TypeFix", 1); rig.Set("USSPS_LastMenu", 12); Hold("fixed-shallow", 1);
                rig.Set("USSPS_LastMenu", 22); Hold("fixed-deep", 1);
                rig.Set("USSPS_LastMenu", 33); Hold("fixed-prone", 1);
                rig.Set("USSPS_TypeFix", 0); Hold("sleep-up", .1f);
                rig.Set("KawaiiPosing_IsDown", 1); Hold("sleep-down", .1f);
                rig.Set("Grounded", 0); rig.Set("VelocityY", 3); Hold("airborne", .1f);
                rig.Set("Grounded", 1); rig.Set("VelocityY", 0); Hold("landed", .1f);
                foreach (var interval in new[] {1, 2, 3})
                {
                    for (var cycle = 0; cycle < 20; cycle++) { rig.Set("Upright", .6f); rig.Tick(interval, fps); rig.Set("Upright", 1); rig.Tick(interval, fps); }
                    Hold("recover-c-" + interval, .6f); Hold("recover-z-" + interval, .35f); Hold("recover-stand-" + interval, 1);
                }
                Assert.That(rig.AppliedDrivers, Is.GreaterThan(20), "Actual SDK callbacks must execute in this probe");
                return observations;
                void Hold(string label, float upright) { rig.Set("Upright", upright); rig.Tick(fps * 2, fps); observations.Add(label + ":" + rig.Snapshot()); }
            }
        }

        [TestCase(false)] [TestCase(true)]
        public void Compiler_NET01_RemotePlaybackUsesReceivedPoseWhileLocalInputsChange(bool vr)
        {
            var ps = CompilerSettings();
            var controller = CompileSettings(ps, true);
            var selected = CompilerBinding(controller, "SitShallow");
            var playback = controller.layers.Single(l => l.name == "USSPS_Locomotion").stateMachine;
            var expected = playback.entryTransitions.Single(t => t.conditions.Any(c => c.parameter == "USSPS_Pose" &&
                c.threshold == selected.sync && c.mode == AnimatorConditionMode.Equals) && t.conditions.Any(c =>
                c.parameter == "VRMode" && c.mode == (vr ? AnimatorConditionMode.NotEqual : AnimatorConditionMode.Equals))).destinationState;
            using var rig = new ControllerCompilerRuntimeProbe(animator, controller, false, vr);
            rig.Set("USSPS_Pose", selected.sync);
            foreach (var upright in new[] {1f, .6f, .35f, .1f, 1f})
            {
                rig.Set("Upright", upright); rig.Set("SitShallow", 0); rig.Set("USSPS_TypeFix", 1); rig.Set("LocomotionLock", 1);
                rig.Tick(180, 90);
                Assert.That(rig.Playable.GetInteger("USSPS_Pose"), Is.EqualTo(selected.sync));
                Assert.That(rig.Playable.GetCurrentAnimatorStateInfo(rig.Layer("USSPS_Locomotion")).shortNameHash, Is.EqualTo(Animator.StringToHash(expected.name)));
            }
        }

        [TestCase(30)] [TestCase(60)] [TestCase(90)] [TestCase(144)]
        public void Compiler_SEL02_SelectedPosePlaybackStartsEarlier(int fps)
        {
            var ps = CompilerSettings();
            var original = CompileSettings(ps, false); var compiled = CompileSettings(ps, true);
            var baseline = Measure(original); var optimized = Measure(compiled);
            TestContext.WriteLine($"Full graph selected crouch at {fps}fps: reference={baseline}, compiled={optimized} frames");
            Assert.That(optimized, Is.LessThan(baseline));

            int Measure(AnimatorController controller)
            {
                var binding = CompilerBinding(controller, "SitShallow");
                var playback = controller.layers.Single(l => l.name == "USSPS_Locomotion").stateMachine;
                var target = playback.entryTransitions.Single(t => t.conditions.Any(c => c.parameter == "USSPS_Pose" && c.threshold == binding.sync) &&
                    t.conditions.Any(c => c.parameter == "VRMode" && c.mode == AnimatorConditionMode.Equals)).destinationState;
                using var rig = new ControllerCompilerRuntimeProbe(animator, controller, true, false);
                rig.Set("SitShallow", binding.select); rig.Tick(fps * 2, fps);
                rig.Set("Upright", .6f);
                for (var i = 1; i <= fps; i++)
                {
                    rig.Tick(1, fps);
                    if (rig.Playable.GetCurrentAnimatorStateInfo(rig.Layer("USSPS_Locomotion")).shortNameHash == Animator.StringToHash(target.name)) return i;
                }
                Assert.Fail("Selected crouch never became the current playback state");
                return -1;
            }
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void Compiler_OPT01_NET02_NdmfBuildPreservesFeaturesMenusAndSyncBits(bool features, bool additiveFootHeight)
        {
            using var scope = new PosingSystemControllerGeneration.Scope(root, true);
            NdmfBuild_ExpressionParametersAndMenusMatchSelectedFeatures(features, features ? 53 : 8,
                PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive, additiveFootHeight);
            Assert.That(scope.Created, Is.EqualTo(1), "Source generation decisions: " + string.Join(";", scope.Decisions));
            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            var controller = (AnimatorController)descriptor.baseAnimationLayers.Single(l => l.type == VRCAvatarDescriptor.AnimLayerType.Base).animatorController;
            var group = controller.layers.Single(l => l.name.EndsWith("USSPS_GroupSwitch")).stateMachine;
            Assert.That(group.states.Single(s => s.state.name == "Standing").state.transitions
                .Any(t => t.destinationState != null && t.destinationState.name == "SitShallow"), Is.True,
                "The NDMF build must actually contain the generated/optimized dispatcher");
        }

        [Test]
        public void Compiler_BUILD01_ComparisonCopyPreservesOriginalAndRepeatedPrebuild()
        {
            root.AddComponent<VRCAvatarDescriptor>();
            var pipeline = root.AddComponent<VRC.Core.PipelineManager>();
            pipeline.blueprintId = "avtr_test_original_unchanged";
            var settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("ecde0b1d67f9e854992715d64f778d8c")), root.transform);
            var ps = settings.GetComponent<PosingSystem>();
            ps.autoImportAvatarAnimations = false; ps.isIconDisabled = true;
            ps.features = new PosingSystemFeatureSettings { breathing = false, fingerGestureControl = false, footHeight = false };
            ps.defines.Clear(); ps.overrideDefines.Clear();
            var sourceMerge = PosingSystemConverter.GetCommonMergeAnimator(ps);
            var sourceController = sourceMerge.animator;
            var sourceData = EditorJsonUtility.ToJson(ps);
            testFolder = "Assets/USSPS_CompilerIntegration_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", testFolder.Substring(7));
            PosingSystemGeneratedControllers.OutputRootOverride = testFolder + "/Generated";
            GameObject copy = null;
            try
            {
                copy = PosingSystemControllerGeneration.CreateComparisonCopy(root);
                Assert.That(sourceMerge.animator, Is.SameAs(sourceController));
                Assert.That(EditorJsonUtility.ToJson(ps), Is.EqualTo(sourceData));
                Assert.That(pipeline.blueprintId, Is.EqualTo("avtr_test_original_unchanged"));
                Assert.That(copy.GetComponent<VRC.Core.PipelineManager>().blueprintId, Is.Empty);
                var copiedSystem = copy.GetComponentInChildren<PosingSystem>();
                var merge = PosingSystemConverter.GetCommonMergeAnimator(copiedSystem);
                var controller = (AnimatorController)merge.animator;
                var path = AssetDatabase.GetAssetPath(controller);
                Assert.That(path, Does.StartWith(testFolder));
                Assert.That(PosingSystemControllerSelectionTests.Group(controller).states.Single(s => s.state.name == "Standing")
                    .state.transitions.Any(t => t.destinationState.name == "SitShallow"), Is.True);
                controller.AddParameter("PreserveExistingPoseMarker", AnimatorControllerParameterType.Bool);
                using (new PosingSystemControllerGeneration.Scope(copy, true))
                    PosingSystemConverter.ConvertToModularAvatarComponents(copiedSystem);
                Assert.That(merge.animator, Is.SameAs(controller));
                Assert.That(controller.parameters.Any(p => p.name == "PreserveExistingPoseMarker"), Is.True);
                Assert.That(AssetDatabase.GetAssetPath(controller), Is.EqualTo(path));
            }
            finally
            {
                PosingSystemGeneratedControllers.OutputRootOverride = null;
                if (copy != null) Object.DestroyImmediate(copy);
            }
        }
    }
}
