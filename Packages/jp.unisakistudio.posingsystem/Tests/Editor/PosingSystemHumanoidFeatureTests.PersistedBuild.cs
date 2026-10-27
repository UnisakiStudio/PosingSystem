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
        private int persistedCompilerCreated;

        [TearDown]
        public void CleanupPersistedBuildSources()
        {
            foreach (var path in persistedBuildSourcePaths) AssetDatabase.DeleteAsset(path);
            persistedBuildSourcePaths.Clear();
        }

        private void PrebuildPersistedSettings(PosingSystem ps)
        {
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
                if(persistedCompilerOptimization==true) Assert.That(locomotion.layers.Single(l=>l.name.EndsWith("USSPS_Locomotion"))
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
                    new PosingSystemControllerGeneration.Scope(build, persistedCompilerOptimization.Value) : null;
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
