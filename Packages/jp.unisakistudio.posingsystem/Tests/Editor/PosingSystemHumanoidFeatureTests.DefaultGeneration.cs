using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        private PosingSystem DefaultGenerationSettings()
        {
            var ps = PersistedBuildSettings();
            ps.enablePoseIk = false;
            var shallow = Clip(); shallow.name = "DefaultDesktopSelectedPose";
            AnimationUtility.SetEditorCurve(shallow, EditorCurveBinding.FloatCurve("", typeof(Animator), "Spine Front-Back"),
                AnimationCurve.Constant(0, 1, .4f));
            ps.defines.Add(new PosingSystem.LayerDefine("Shallow", "Shallow", "SitShallow", 12, "Shallow",
                new List<PosingSystem.AnimationDefine> { new() { enabled = true, displayName = "Shallow", animationClip = shallow } }));
            return ps;
        }

        [TestCase(false)] [TestCase(true)]
        public void DefaultGeneration_PrebuildIsOneEvaluationAndKeepsOneAssetAcrossUpdates(bool legacy)
        {
            var ps = DefaultGenerationSettings();
            if (legacy)
            {
                using (new PosingSystemControllerGeneration.Scope(root, false)) PrebuildPersistedSettings(ps);
                ps.data = ps.data.Replace("\"controllerLogicVersion\":2", "\"controllerLogicVersion\":1");
                Assert.That(PosingSystemConverter.IsPosingSystemDataUpdated(ps), Is.True, "alpha.3 cache must be invalidated");
            }
            string path = null;
            for (var i = 0; i < 3; i++)
            {
                PrebuildPersistedSettings(ps); // No experimental scope or comparison copy.
                var controller = (AnimatorController)PosingSystemConverter.GetCommonMergeAnimator(ps).animator;
                AssertDefaultGenerationOutput(animator, controller);
                if (path == null) path = AssetDatabase.GetAssetPath(controller);
                else Assert.That(AssetDatabase.GetAssetPath(controller), Is.EqualTo(path));
                Assert.That(PosingSystemConverter.IsPosingSystemDataUpdated(ps), Is.False);
                ps.features.mirror = !ps.features.mirror;
            }
        }

        // 0=fresh, 1=current prebuild, 2=alpha.3 cache, 3=changed prebuild.
        [TestCase(0, false)] [TestCase(1, false)] [TestCase(2, false)] [TestCase(3, false)]
        [TestCase(0, true)] [TestCase(1, true)] [TestCase(2, true)] [TestCase(3, true)]
        [TestCase(4, true)]
        public void DefaultGeneration_NdmfBuildUsesOneEvaluationWithoutOptIn(int cache, bool optimizing)
        {
            var ps = DefaultGenerationSettings();
            if (cache == 2)
            {
                using (new PosingSystemControllerGeneration.Scope(root, false)) PrebuildPersistedSettings(ps);
                ps.data = ps.data.Replace("\"controllerLogicVersion\":2", "\"controllerLogicVersion\":1");
            }
            else if (cache != 0) PrebuildPersistedSettings(ps);
            if (cache == 3) ps.features.mirror = !ps.features.mirror;
            var sceneMerge = PosingSystemConverter.GetCommonMergeAnimator(ps);
            var source = sceneMerge.animator;
            if (cache == 4) sceneMerge.animator = null;
            var before = EditorJsonUtility.ToJson(source);
            WithPersistedBuild(context =>
            {
                var method = typeof(AvatarProcessor).GetMethod("ProcessAvatar", BindingFlags.NonPublic | BindingFlags.Static,
                    null, new[] { typeof(BuildContext), typeof(BuildPhase), typeof(BuildPhase) }, null);
                try { method.Invoke(null, new object[] { context, BuildPhase.Resolving,
                    optimizing ? BuildPhase.Optimizing : BuildPhase.Transforming }); }
                catch (TargetInvocationException ex) { throw ex.InnerException ?? ex; }
                context.DeactivateAllExtensionContexts();
                Assert.That(context.Successful, Is.True);
                var descriptor = context.AvatarRootObject.GetComponent<VRCAvatarDescriptor>();
                var controller = (AnimatorController)descriptor.baseAnimationLayers.Single(
                    l => l.type == VRCAvatarDescriptor.AnimLayerType.Base).animatorController;
                Assert.That(descriptor.expressionParameters.parameters.Any(
                    p => p.name.EndsWith(PosingSystemOneFrameSelection.Guard)), Is.False);
                AssertDefaultGenerationOutput(context.AvatarRootObject.GetComponent<Animator>(), controller);
            });
            Assert.That(EditorJsonUtility.ToJson(source), Is.EqualTo(before), "Build must leave the scene/source controller unchanged");
            if (cache == 4) Assert.That(sceneMerge.animator, Is.Null, "Recover only the build copy");
        }

        [Test]
        public void DefaultGeneration_CustomCommonControllerIsNotAutomaticallyAccelerated()
        {
            var ps = DefaultGenerationSettings();
            EnsureDefaultIdleTestFolder();
            var template = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(PosingSystemControllerGeneration.TemplateGuid));
            var custom = UnityEngine.Object.Instantiate(template);
            custom.name = "UserCustomCommon";
            AssetDatabase.CreateAsset(custom, testFolder + "/UserCustom.controller");
            PosingSystemConverter.GetCommonMergeAnimator(ps).animator = custom;
            var source = EditorJsonUtility.ToJson(custom);
            for (var i = 0; i < 3; i++)
            {
                PrebuildPersistedSettings(ps);
                var generated = (AnimatorController)PosingSystemConverter.GetCommonMergeAnimator(ps).animator;
                Assert.That(generated.parameters.Any(p => p.name == PosingSystemOneFrameSelection.Guard), Is.False);
                ps.features.mirror = !ps.features.mirror;
            }
            Assert.That(EditorJsonUtility.ToJson(custom), Is.EqualTo(source));
        }

        [TestCase(false, false)] [TestCase(true, false)]
        [TestCase(false, true)] [TestCase(true, true)]
        public void DefaultGeneration_NdmfPreservesCustomCommonController(bool prebuild, bool persisted)
        {
            var ps = DefaultGenerationSettings();
            EnsureDefaultIdleTestFolder();
            var template = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(PosingSystemControllerGeneration.TemplateGuid));
            var custom = UnityEngine.Object.Instantiate(template);
            AssetDatabase.CreateAsset(custom, testFolder + "/CustomCommon.controller");
            PosingSystemConverter.GetCommonMergeAnimator(ps).animator = custom;
            var before = EditorJsonUtility.ToJson(custom);
            if (prebuild) PrebuildPersistedSettings(ps);
            if (persisted)
                WithPersistedBuild(context =>
                {
                    var method = typeof(AvatarProcessor).GetMethod("ProcessAvatar", BindingFlags.NonPublic | BindingFlags.Static,
                        null, new[] { typeof(BuildContext), typeof(BuildPhase), typeof(BuildPhase) }, null);
                    try { method.Invoke(null, new object[] { context, BuildPhase.Resolving, BuildPhase.Optimizing }); }
                    catch (TargetInvocationException ex) { throw ex.InnerException ?? ex; }
                    context.DeactivateAllExtensionContexts();
                    Assert.That(context.Successful, Is.True);
                    Verify(context.AvatarRootObject.GetComponent<VRCAvatarDescriptor>());
                });
            else BuildDefaultIdleAvatar((descriptor, _) => Verify(descriptor), true);
            Assert.That(EditorJsonUtility.ToJson(custom), Is.EqualTo(before));

            void Verify(VRCAvatarDescriptor descriptor)
            {
                var controller = (AnimatorController)descriptor.baseAnimationLayers.Single(
                    l => l.type == VRCAvatarDescriptor.AnimLayerType.Base).animatorController;
                Assert.That(controller.parameters.Any(p => p.name.EndsWith(PosingSystemOneFrameSelection.Guard)), Is.False,
                    "A user-supplied common controller must retain its path through MA cloning and NDMF");
            }
        }

        private static void AssertDefaultGenerationOutput(Animator target, AnimatorController controller)
        {
            var layer = controller.layers.Single(l => l.name.EndsWith("USSPS_Locomotion"));
            Assert.That(layer.stateMachine.defaultState.name, Is.EqualTo("AvatarSelecting"));
            Assert.That(controller.parameters.Count(p => p.name.EndsWith(PosingSystemOneFrameSelection.Guard)), Is.EqualTo(1));
            foreach (var category in new[] { "SitStand", "SitShallow", "SitDeep" })
                Assert.That(layer.stateMachine.stateMachines.Count(s => s.stateMachine.name.EndsWith("Direct" + category)), Is.EqualTo(1));
            foreach (var threshold in new[] { "Shallow", "Deep", "Sleep" })
                Assert.That(controller.layers.Count(l => l.name.EndsWith("USSPS_Final" + threshold + "Threshold")), Is.EqualTo(1));
            var shallow = layer.stateMachine.stateMachines.Single(m => m.stateMachine.name.EndsWith("DirectSitShallow")).stateMachine;
            var input = shallow.entryTransitions.Single(t => t.conditions.Single().threshold > 0).conditions.Single();
            using var handler = new HumanPoseHandler(target.avatar, target.transform);
            var pose = new HumanPose(); var spine = Array.IndexOf(HumanTrait.MuscleName, "Spine Front-Back");
            using var rig = new ControllerCompilerRuntimeProbe(target, controller, true, false);
            rig.Set(input.parameter, input.threshold); rig.Tick(180, 90);
            handler.GetHumanPose(ref pose);
            Assert.That(Mathf.Abs(pose.muscles[spine] - .4f), Is.GreaterThan(.02f));
            rig.Set("Upright", .6f); rig.Tick(1, 90); handler.GetHumanPose(ref pose);
            Assert.That(pose.muscles[spine], Is.EqualTo(.4f).Within(.002f), "Normal build must output the selected Humanoid pose in one evaluation");
            rig.Set("VRMode", 1); rig.Tick(120, 90);
            Assert.That(rig.Playable.GetInteger(PosingSystemOneFrameSelection.Guard), Is.LessThanOrEqualTo(0), "VR must use the retained fallback");
        }
    }
}
