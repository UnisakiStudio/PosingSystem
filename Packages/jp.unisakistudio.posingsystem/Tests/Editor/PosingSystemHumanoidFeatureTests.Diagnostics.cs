using System;
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [Test]
        public void Diagnostics_RecognizesPlayerLayoutErrorWithoutSharingLocalDetails()
        {
            const string raw = "Type '[jp.unisakistudio.posingsystem]jp.unisakistudio.posingsystem.PosingSystem' has an extra field 'legacyAutoHeadMenuChecked' of type 'System.Boolean' in the player and thus can't be serialized (expected 'privateField' in private/avatar).";
            var signature = PosingSystemSupportReport.SafeError(raw);
            Assert.That(signature, Is.EqualTo("jp.unisakistudio.posingsystem.PosingSystem / legacyAutoHeadMenuChecked / can't be serialized"));
            Assert.That(PosingSystemSupportReport.SafeError(raw.Replace("legacyAutoHeadMenuChecked", "privateField")), Is.Empty);
            var draft = PosingSystemSupportReport.Draft(raw);
            Assert.That(draft, Does.Contain(signature).And.Not.Contain("private"));
            Assert.That(PosingSystemSupportReport.SafeError("[PosingSystem] PS070 private pose"), Is.EqualTo("PS070"));
        }

        [Test]
        public void Diagnostics_RolloverInquiryCarriesGuidanceWithoutCustomPoseNames()
        {
            var empty = new AnimationClip();
            assets.Add(empty);
            var pose = new PosingSystem.AnimationDefine(DefaultIdleProxy(), "private-pose-name")
                { isRollover = true, rolloverSideAnimation = empty };
            var ps = DiagnosticPoseSettings(pose);
            var failure = Assert.Throws<PosingSystemDiagnostics.DiagnosticException>(() => PosingSystemDiagnostics.ThrowIfInvalid(ps));
            Assert.That(failure.Message, Does.Contain("PS070").And.Contain("private-pose-name"));
            var draft = PosingSystemSupportReport.Draft(failure.Message);
            Assert.That(draft, Does.Contain(PosingSystemDiagnostics.RolloverClipAction));
            Assert.That(draft, Does.Not.Contain("private-pose-name"));
            Assert.That(PosingSystemSupportReport.Draft("PS050"), Does.Not.Contain(PosingSystemDiagnostics.RolloverClipAction));
            pose.rolloverSideAnimation = DefaultIdleProxy();
            Assert.DoesNotThrow(() => PosingSystemDiagnostics.ThrowIfInvalid(ps));
        }

        [Test]
        public void Diagnostics_VrcFuryDraftIncludesOnlyKnownObsoleteApiNames()
        {
            const string path = "C:/private-user/project/Packages/com.vrcfury.vrcfury/Editor/VRCExpressionParameterExtensions.cs";
            const string error = "(43,20): error CS0618: 'VRCExpressionParameters.TypeCost(VRCExpressionParameters.ValueType)' is obsolete: 'Use Parameter.GetBitWidthByType() instead.'";
            var draft = PosingSystemSupportReport.SafeError(path + error + " private-details");
            Assert.That(draft, Is.EqualTo("com.vrcfury.vrcfury/VRCExpressionParameterExtensions.cs:43 CS0618（VRCExpressionParameters.TypeCost → Parameter.GetBitWidthByType）"));
            Assert.That(PosingSystemSupportReport.SafeError(path + error.Replace("TypeCost", "PersonalMethod")), Does.Not.Contain("TypeCost"));
            Assert.That(PosingSystemSupportReport.SafeError(path + error.Replace("error CS0618", "warning CS0618")), Is.Empty);
        }

        [Test]
        public void Diagnostics_ValidHumanoidWithoutUpperChest_HasNoErrorsOrChestWarning_AndIsReadOnly()
        {
            var ps = DefaultIdleSettings(false, false);
            var before = EditorJsonUtility.ToJson(ps);
            var count = root.GetComponentsInChildren<Transform>(true).Length;
            var issues = PosingSystemDiagnostics.Collect(ps);
            Assert.That(issues.Where(i => i.Severity == MessageType.Error), Is.Empty);
            Assert.That(issues.Any(i => i.Code == "PS007"), Is.False, "UpperChest is optional, and must not be mistaken for Chest");
            Assert.That(EditorJsonUtility.ToJson(ps), Is.EqualTo(before));
            Assert.That(root.GetComponentsInChildren<Transform>(true).Length, Is.EqualTo(count));
        }

        [Test]
        public void Diagnostics_MissingAnimator_ReportsRepairWithoutThrowingInHierarchy()
        {
            var ps = DefaultIdleSettings(false, false);
            Object.DestroyImmediate(animator);
            Assert.That(PosingSystemDiagnostics.Collect(ps).Any(i => i.Code == "PS003"), Is.True);
            Assert.That(PosingSystemConverter.HasError(ps), Is.True);
        }

        [Test]
        public void Diagnostics_MissingAvatarReference_IsDistinctFromNonHumanoid()
        {
            var ps = DefaultIdleSettings(false, false);
            animator.avatar = null;
            var issues = PosingSystemDiagnostics.Collect(ps);
            Assert.That(issues.Any(i => i.Code == "PS004"), Is.True);
            Assert.That(issues.Any(i => i.Code == "PS005"), Is.False);
        }

        [TestCase(false, "PS011")]
        [TestCase(true, "PS010")]
        public void Diagnostics_MissingCommonAnimator_WarnsAndRebuildsOnce(bool removeChild, string code)
        {
            var ps = DefaultIdleSettings(false, false);
            var merge = PosingSystemConverter.GetCommonMergeAnimator(ps);
            Object.DestroyImmediate(removeChild ? (Object)merge.gameObject : merge);
            var before = ps.GetComponentsInChildren<Transform>(true).Length;
            var issues = PosingSystemDiagnostics.Collect(ps);
            Assert.That(issues.Single(i => i.Code == code).Severity, Is.EqualTo(MessageType.Warning));
            Assert.That(ps.GetComponentsInChildren<Transform>(true).Length, Is.EqualTo(before));
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(code));
            var repaired = PosingSystemConverter.GetCommonMergeAnimator(ps, true);
            Assert.That(repaired.animator, Is.InstanceOf<UnityEditor.Animations.AnimatorController>());
            Assert.That(PosingSystemConverter.GetCommonMergeAnimator(ps, true), Is.SameAs(repaired));
            Assert.That(PosingSystemDiagnostics.Collect(ps).Any(i => i.Code == code), Is.False);
        }

        [Test]
        public void Diagnostics_MissingRoleMarker_RecoversExistingMergeWithoutDuplicatingIt()
        {
            var ps = DefaultIdleSettings(false, false);
            var existing = PosingSystemConverter.GetCommonMergeAnimator(ps);
            var source = existing.animator;
            Object.DestroyImmediate(existing.GetComponent<DuplicateEraser>());
            var before = ps.GetComponentsInChildren<ModularAvatarMergeAnimator>(true).Length;
            Assert.That(PosingSystemDiagnostics.Collect(ps).Single(i => i.Code == "PS010").Severity, Is.EqualTo(MessageType.Warning));
            Assert.That(PosingSystemConverter.GetCommonMergeAnimator(ps, true), Is.SameAs(existing));
            Assert.That(existing.animator, Is.SameAs(source));
            Assert.That(ps.GetComponentsInChildren<ModularAvatarMergeAnimator>(true).Length, Is.EqualTo(before));
        }

        [Test]
        public void Diagnostics_MissingBreathingMerge_RestoresAdditiveController()
        {
            var ps = DefaultIdleSettings(false, true);
            var original = PosingSystemConverter.GetAdditiveBreathingMergeAnimator(ps, true);
            var child = original.gameObject;
            Object.DestroyImmediate(original);
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("PS052"));
            UnityEngine.TestTools.LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex("PS053"));
            var repaired = PosingSystemConverter.GetAdditiveBreathingMergeAnimator(ps, true);
            Assert.That(repaired.gameObject, Is.SameAs(child));
            Assert.That(repaired.layerType, Is.EqualTo(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Additive));
            Assert.That(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(repaired.animator)),
                Is.EqualTo(PosingSystemConverter.DefaultAdditiveBreathingControllerGuid));
            Assert.That(child.GetComponents<ModularAvatarMergeAnimator>().Length, Is.EqualTo(1));
        }

        [Test]
        public void Diagnostics_MissingController_AllowsExistingRecovery_WithoutMutatingOnInspection()
        {
            var ps = DefaultIdleSettings(false, false);
            var merge = PosingSystemConverter.GetCommonMergeAnimator(ps);
            merge.animator = null;
            var issues = PosingSystemDiagnostics.Collect(ps);
            Assert.That(issues.Single(i => i.Code == "PS012").Severity, Is.EqualTo(MessageType.Warning));
            Assert.That(merge.animator, Is.Null);
            Assert.DoesNotThrow(() => PosingSystemDiagnostics.ThrowIfInvalid(ps));
            Assert.That(PosingSystemConverter.GetCommonMergeAnimator(ps, true).animator, Is.Not.Null);
        }

        [Test]
        public void Diagnostics_EmptyCategory_IsInformational_AndDisabledEmptySlotIsIgnored()
        {
            var ps = DiagnosticPoseSettings(new PosingSystem.AnimationDefine { enabled = false });
            var issues = PosingSystemDiagnostics.Collect(ps);
            Assert.That(issues.Single(i => i.Code == "PS022").Severity, Is.EqualTo(MessageType.Info));
            Assert.That(issues.Any(i => i.Code == "PS025"), Is.False);
            Assert.That(issues.Where(i => i.Severity == MessageType.Error), Is.Empty);
        }

        [Test]
        public void Diagnostics_EnabledEmptyClipAndIcon_IdentifyCategoryAndSlot()
        {
            var ps = DiagnosticPoseSettings(new PosingSystem.AnimationDefine { displayName = "My pose", isCustomIcon = true });
            var issues = PosingSystemDiagnostics.Collect(ps);
            Assert.That(issues.Single(i => i.Code == "PS025").Message, Does.Contain("Test category").And.Contain("1番目").And.Contain("My pose"));
            Assert.That(issues.Any(i => i.Code == "PS028"), Is.True);
        }

        [Test]
        public void Diagnostics_LegacyClipAndEmptyMotionTime_BlockBuild()
        {
            var clip = new AnimationClip { legacy = true }; assets.Add(clip);
            var ps = DiagnosticPoseSettings(new PosingSystem.AnimationDefine(clip, "Legacy") { isMotionTime = true });
            var issues = PosingSystemDiagnostics.Collect(ps);
            Assert.That(issues.Any(i => i.Code == "PS026" && i.Severity == MessageType.Error), Is.True);
            Assert.That(issues.Any(i => i.Code == "PS027"), Is.True);
        }

        [Test]
        public void Diagnostics_NullCategoryAndPose_ProduceActionableErrors()
        {
            var ps = DiagnosticPoseSettings(null);
            ps.defines.Add(null);
            var issues = PosingSystemDiagnostics.Collect(ps);
            Assert.That(issues.Any(i => i.Code == "PS021"), Is.True);
            Assert.That(issues.Any(i => i.Code == "PS024"), Is.True);
        }

        [Test]
        public void Diagnostics_AutoFootstepsAlone_DoesNotRaiseHierarchyWarning()
        {
            var ps = DefaultIdleSettings(false, false);
            ps.GetAvatar().autoFootsteps = true;
            ps.data = PosingSystemConverter.GetDefineSerializeJson(ps, true);
            Assert.That(PosingSystemConverter.HasWarning(ps), Is.False);
        }

        [Test]
        public void Diagnostics_OutsideAvatar_FailsBeforeSavedInstanceOrDataChange()
        {
            var ps = DefaultIdleSettings(false, false);
            ps.transform.SetParent(null);
            try
            {
                var before = EditorJsonUtility.ToJson(ps);
                var error = Assert.Throws<PosingSystemDiagnostics.DiagnosticException>(() => PosingSystemEditor.Prebuild(ps));
                Assert.That(error.Message, Does.Contain("PS002"));
                Assert.That(EditorJsonUtility.ToJson(ps), Is.EqualTo(before));
            }
            finally { ps.transform.SetParent(root.transform); }
        }

        private PosingSystem DiagnosticPoseSettings(PosingSystem.AnimationDefine pose)
        {
            var ps = DefaultIdleSettings(false, false);
            ps.defines.Add(new PosingSystem.LayerDefine("", "Test", "Test", 0, "Test category", new List<PosingSystem.AnimationDefine> { pose }));
            return ps;
        }
    }
}
