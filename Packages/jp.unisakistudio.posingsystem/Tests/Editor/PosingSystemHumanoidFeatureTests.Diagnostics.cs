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

        [Test]
        public void Diagnostics_MissingMergeComponent_StopsBeforeMenuOrDataMutation()
        {
            var ps = DefaultIdleSettings(false, false);
            var merge = PosingSystemConverter.GetCommonMergeAnimator(ps);
            Object.DestroyImmediate(merge);
            ps.data = "previous successful build";
            var before = EditorJsonUtility.ToJson(ps);
            var count = ps.GetComponentsInChildren<Transform>(true).Length;
            var error = Assert.Throws<PosingSystemDiagnostics.DiagnosticException>(() => PosingSystemConverter.ConvertToModularAvatarComponents(ps));
            Assert.That(error.Message, Does.Contain("PS011"));
            Assert.That(error.Message, Does.Contain("対処:"));
            Assert.That(EditorJsonUtility.ToJson(ps), Is.EqualTo(before));
            Assert.That(ps.GetComponentsInChildren<Transform>(true).Length, Is.EqualTo(count));
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
