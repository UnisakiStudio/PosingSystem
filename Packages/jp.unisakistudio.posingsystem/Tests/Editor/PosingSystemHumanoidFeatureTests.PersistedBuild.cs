using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        private readonly HashSet<string> persistedBuildSourcePaths = new();
        private bool? persistedCompilerOptimization;
        private bool persistedCompilerOneFrame;
        private int persistedCompilerCreated;

        [TearDown]
        public void CleanupPersistedBuildSources()
        {
            foreach (var path in persistedBuildSourcePaths) AssetDatabase.DeleteAsset(path);
            persistedBuildSourcePaths.Clear();
        }

        private void PrebuildPersistedSettings(PosingSystem ps)
        {
            using var oneFrame=persistedCompilerOneFrame?new PosingSystemControllerGeneration.Scope(root,true,true):null;
            PosingSystemEditor.Prebuild(ps);
            foreach (var merge in root.GetComponentsInChildren<nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator>(true))
            {
                var path = AssetDatabase.GetAssetPath(merge.animator);
                if (path.StartsWith("Assets/") && !existingAssets.Contains(path)) persistedBuildSourcePaths.Add(path);
            }
        }

        [Test]
        public void OptionalFeatureBuild_PackageSourceRemainsProtectedWithAssetSaver()
        {
            var ps = DefaultIdleSettings(false, false);
            var source = PosingSystemConverter.GetCommonMergeAnimator(ps).animator;
            var before = EditorJsonUtility.ToJson(source);
            WithPersistedBuild(context =>
            {
                var buildSettings = context.AvatarRootObject.GetComponentInChildren<PosingSystem>();
                Assert.That(context.IsTemporaryAsset(source), Is.False);
                Assert.Throws<InvalidOperationException>(() => PosingSystemConverter.ConvertOptionalFeatures(buildSettings));
                Assert.Throws<InvalidOperationException>(() => PosingSystemConverter.ConvertOptionalFeatures(buildSettings, context.AssetSaver));
                Assert.That(EditorJsonUtility.ToJson(source), Is.EqualTo(before));
            });
        }

        [Test]
        public void OptionalFeatureBuild_PersistedCopyPreservesSharedContainerMotions()
        {
            var ps = PersistedBuildSettings();
            PrebuildPersistedSettings(ps);
            var source = PosingSystemConverter.GetCommonMergeAnimator(ps).animator;
            var before = EditorJsonUtility.ToJson(source);
            WithPersistedBuild(context =>
            {
                context.ActivateExtensionContextRecursive<AnimatorServicesContext>();
                context.DeactivateAllExtensionContexts();
                var buildSettings = context.AvatarRootObject.GetComponentInChildren<PosingSystem>();
                var controller = (AnimatorController)PosingSystemConverter.GetPrebuiltMergeAnimator(buildSettings).animator;
                Assert.That(controller, Is.Not.SameAs(source));
                Assert.That(context.IsTemporaryAsset(controller), Is.True);
                Assert.That(AssetDatabase.GetAssetPath(controller), Does.StartWith("Packages/"));
                Assert.That(AssetDatabase.IsMainAsset(controller), Is.False, "NDMF stores controllers in a shared asset container");
                // NDMF can place another controller's generated motions in the same file.
                var otherMotion = new AnimationClip { name = "_USSPS_other_controller_pose" };
                AnimationUtility.SetEditorCurve(otherMotion, EditorCurveBinding.FloatCurve("", typeof(Animator), "Spine Front-Back"),
                    AnimationCurve.Constant(0, 1, .25f));
                AssetDatabase.AddObjectToAsset(otherMotion, controller);
                PosingSystemConverter.ConvertOptionalFeatures(buildSettings, context.AssetSaver);
                Assert.That(otherMotion != null, Is.True, "Controller cleanup must not prune a shared NDMF container");
                Assert.That(AnimationUtility.GetCurveBindings(otherMotion), Has.Length.EqualTo(1));
                Assert.That(controller.layers.Any(l => l.name.StartsWith("USSPS_FloatPoseSpace")), Is.True);
                Assert.That(EditorJsonUtility.ToJson(source), Is.EqualTo(before));
            });
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void NdmfBuild_PersistedPackageAssets_SupportFreshCachedAndRegeneratedControllers(bool prebuild, bool regenerate)
        {
            var ps = PersistedBuildSettings();
            if (prebuild) PrebuildPersistedSettings(ps);
            var source = PosingSystemConverter.GetCommonMergeAnimator(ps).animator;
            var before = EditorJsonUtility.ToJson(source);
            WithPersistedBuild(context =>
            {
                var buildSettings = context.AvatarRootObject.GetComponentInChildren<PosingSystem>();
                if (regenerate) buildSettings.data = null;
                else if (prebuild)
                    Assert.That(buildSettings.data, Is.EqualTo(PosingSystemConverter.GetDefineSerializeJson(buildSettings, true)));
                var method = typeof(AvatarProcessor).GetMethod("ProcessAvatar", BindingFlags.NonPublic | BindingFlags.Static,
                    null, new[] { typeof(BuildContext), typeof(BuildPhase), typeof(BuildPhase) }, null);
                try { method.Invoke(null, new object[] { context, BuildPhase.Resolving, BuildPhase.Optimizing }); }
                catch (TargetInvocationException exception) { throw exception.InnerException ?? exception; }
                context.DeactivateAllExtensionContexts();
                Assert.That(context.Successful, Is.True, "The persisted build must complete without NDMF errors");
                var descriptor = context.AvatarRootObject.GetComponent<VRCAvatarDescriptor>();
                var locomotion = (AnimatorController)descriptor.baseAnimationLayers.Single(l => l.type == VRCAvatarDescriptor.AnimLayerType.Base).animatorController;
                if(persistedCompilerOneFrame)
                {
                    Assert.That(locomotion.parameters.Any(p=>p.name.EndsWith(PosingSystemOneFrameSelection.Guard)),Is.True);
                    Assert.That(descriptor.expressionParameters.parameters.Any(p=>p.name.EndsWith(PosingSystemOneFrameSelection.Guard)),Is.False,"Private guard must not consume synchronization bits");
                    var layer=locomotion.layers.Single(l=>l.name.EndsWith("USSPS_Locomotion"));
                    var shallow=layer.stateMachine.stateMachines.Single(m=>m.stateMachine.name.EndsWith("DirectSitShallow")).stateMachine;
                    var entry=shallow.entryTransitions.Single(t=>t.conditions.Single().threshold>0);var input=entry.conditions.Single();
                    var buildAnimator=context.AvatarRootObject.GetComponent<Animator>();
                    using var handler=new HumanPoseHandler(buildAnimator.avatar,buildAnimator.transform);
                    var pose=new HumanPose();var spine=Array.IndexOf(HumanTrait.MuscleName,"Spine Front-Back");
                    using var rig=new ControllerCompilerRuntimeProbe(buildAnimator,locomotion,true,false);
                    rig.Set(input.parameter,input.threshold);rig.Tick(180,90);
                    handler.GetHumanPose(ref pose);
                    Assert.That(Mathf.Abs(pose.muscles[spine]-.4f),Is.GreaterThan(.02f),"The selected pose must differ from the standing output before the input change");
                    rig.Set("Upright",.6f);rig.Tick(1,90);
                    Assert.That(rig.Playable.GetCurrentAnimatorStateInfo(rig.Layer(layer.name)).shortNameHash,Is.EqualTo(Animator.StringToHash(entry.destinationState.name)),"Persisted NDMF output must retain one-evaluation selection");
                    handler.GetHumanPose(ref pose);
                    TestContext.WriteLine($"NDMF oneFrame: prebuild={prebuild}, regenerate={regenerate}, spine={pose.muscles[spine]:R}");
                    Assert.That(pose.muscles[spine],Is.EqualTo(.4f).Within(.002f),"The built avatar must output the selected Humanoid motion in that same evaluation");
                }
                else if(persistedCompilerOptimization==true) Assert.That(locomotion.layers.Single(l=>l.name.EndsWith("USSPS_Locomotion"))
                    .stateMachine.entryTransitions.Any(t=>t.conditions.Any(c=>c.parameter.EndsWith("LocomotionTypeGroup"))),Is.True);
                Assert.That(locomotion.layers.Any(l => l.name.StartsWith("USSPS_FloatPoseSpace")), Is.True);
                Assert.That(locomotion.layers.Any(l => l.name == PosingSystemConverter.FootHeightAdditiveLayer), Is.True);
                Assert.That(locomotion.layers.Any(l => l.name.EndsWith("KeyDelta")), Is.False);
                Assert.That(locomotion.animationClips.Any(c => c.name.StartsWith("_USSPS_movement_offset_")), Is.True);
                var additive = (AnimatorController)descriptor.baseAnimationLayers.Single(l => l.type == VRCAvatarDescriptor.AnimLayerType.Additive).animatorController;
                Assert.That(additive.layers.Any(l => l.name == PosingSystemConverter.AdditiveBreathingLayerName), Is.True);
                Assert.That(additive.animationClips.Any(c => c.name == PosingSystemConverter.SuppressedIdleClipName), Is.True);
                Assert.That(EditorJsonUtility.ToJson(source), Is.EqualTo(before), "Do not edit the scene's prebuilt or package controller");
            });
        }

        private PosingSystem PersistedBuildSettings()
        {
            var ps = DefaultIdleSettings(true, true);
            var clip = Clip();
            clip.name = "PersistedBuildPose";
            ps.defines.Add(new PosingSystem.LayerDefine("Sleep", "Sleep", "SitSleepUp", 30, "Sleep",
                new List<PosingSystem.AnimationDefine> { new() { enabled = true, displayName = "Sleep", animationClip = clip } }));
            if(persistedCompilerOneFrame)
            {
                var shallow=Clip();shallow.name="OneFrameNdmfShallow";
                AnimationUtility.SetEditorCurve(shallow,EditorCurveBinding.FloatCurve("",typeof(Animator),"Spine Front-Back"),AnimationCurve.Constant(0,1,.4f));
                ps.defines.Add(new PosingSystem.LayerDefine("Shallow","Shallow","SitShallow",12,"Shallow",
                    new List<PosingSystem.AnimationDefine>{new(){enabled=true,displayName="Shallow",animationClip=shallow}}));
            }
            ps.features.photography = ps.features.floating = ps.features.footHeight = true;
            // Persisted scenes from the comparison period must also use the final implementation.
            ps.featureSettingsMigrated = true;
            ps.features.footHeightBackend = PosingSystemFeatureSettings.FootHeightBackend.ClipLevel;
            ps.features.photographyBackend = ps.features.floatingBackend = PosingSystemFeatureSettings.MovementBackend.AdditiveOverride;
            ps.features.floatingPoseSpace = PosingSystemFeatureSettings.FloatingPoseSpaceMode.EnterEveryFrame;
            return ps;
        }

        private void WithPersistedBuild(Action<BuildContext> verify)
        {
            // Production SDK builds persist NDMF copies below Packages; NullAssetSaver cannot reproduce this.
            var folder = "Packages/jp.unisakistudio.posingsystem/Tests/__PersistedBuild_" + Guid.NewGuid().ToString("N");
            var build = Object.Instantiate(root);
            build.name = root.name;
            BuildContext context = null;
            try
            {
                context = new BuildContext(build, folder, false);
                using var compiler = persistedCompilerOptimization.HasValue ?
                    new PosingSystemControllerGeneration.Scope(build, persistedCompilerOptimization.Value,persistedCompilerOneFrame) : null;
                verify(context);
                persistedCompilerCreated = compiler?.Created ?? 0;
            }
            finally
            {
                if (context != null)
                {
                    context.DeactivateAllExtensionContexts();
                    context.AssetSaver.Dispose();
                }
                Object.DestroyImmediate(build);
                AssetDatabase.DeleteAsset(folder);
            }
        }
    }
}
