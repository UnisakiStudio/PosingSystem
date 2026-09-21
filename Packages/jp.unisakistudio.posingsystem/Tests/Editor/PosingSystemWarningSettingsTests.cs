using System;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemWarningSettingsTests
    {
        private GameObject root;
        private VRCAvatarDescriptor avatar;
        private PosingSystem posing;
        private string prefabPath;
        private GameObject prefabInstance;

        [SetUp]
        public void SetUp()
        {
            root = new GameObject("WarningSettingsTests");
            root.AddComponent<Animator>();
            avatar = root.AddComponent<VRCAvatarDescriptor>();
            avatar.autoFootsteps = false;
            avatar.baseAnimationLayers = new[]
            {
                new VRCAvatarDescriptor.CustomAnimLayer
                {
                    type = VRCAvatarDescriptor.AnimLayerType.Base,
                    isDefault = true,
                }
            };
            posing = root.AddComponent<PosingSystem>();
            posing.savedInstanceId = GlobalObjectId.GetGlobalObjectIdSlow(posing).ToString();
        }

        [TearDown]
        public void TearDown()
        {
            if (prefabInstance != null) Object.DestroyImmediate(prefabInstance);
            if (root != null) Object.DestroyImmediate(root);
            if (prefabPath != null) AssetDatabase.DeleteAsset(prefabPath);
        }

        [Test]
        public void DefaultSettings_ShowWarningsWithoutAddingUnbuiltHierarchyBadge()
        {
            Assert.That(posing.ignoredWarnings, Is.EqualTo(PosingSystem.WarningType.None));
            Assert.That(PosingSystemConverter.GetVisibleWarnings(posing), Is.EqualTo(PosingSystem.WarningType.PrebuildNotRun));
            Assert.That(PosingSystemConverter.HasWarning(posing), Is.False);
            avatar.autoFootsteps = true;
            Assert.That(PosingSystemConverter.HasWarning(posing), Is.True);
        }

        [TestCase(PosingSystem.WarningType.AutoFootsteps)]
        [TestCase(PosingSystem.WarningType.PrebuildNotRun)]
        [TestCase(PosingSystem.WarningType.PrebuildOutOfDate)]
        public void IgnoreAndRestore_AffectsOnlySelectedWarning(PosingSystem.WarningType warning)
        {
            avatar.autoFootsteps = true;
            posing.data = warning == PosingSystem.WarningType.PrebuildOutOfDate ? "legacy-data" : null;
            var before = PosingSystemConverter.GetVisibleWarnings(posing);
            Assert.That(before & warning, Is.EqualTo(warning));

            SetIgnored(posing, warning, true);

            Assert.That(PosingSystemConverter.GetVisibleWarnings(posing), Is.EqualTo(before & ~warning));
            Assert.That(avatar.autoFootsteps, Is.True, "Ignoring the warning must not change avatar settings.");
            SetIgnored(posing, warning, false);
            Assert.That(PosingSystemConverter.GetVisibleWarnings(posing), Is.EqualTo(before));
        }

        [Test]
        public void IgnoreAll_CanRestoreOneWarningIndependently()
        {
            avatar.autoFootsteps = true;
            SetIgnored(posing, PosingSystem.WarningType.All, true);
            Assert.That(PosingSystemConverter.GetVisibleWarnings(posing), Is.EqualTo(PosingSystem.WarningType.None));
            posing.data = "legacy-data";
            Assert.That(PosingSystemConverter.GetVisibleWarnings(posing), Is.EqualTo(PosingSystem.WarningType.None));
            Assert.That(PosingSystemConverter.HasWarning(posing), Is.False);

            SetIgnored(posing, PosingSystem.WarningType.AutoFootsteps, false);

            Assert.That(posing.IsWarningIgnored(PosingSystem.WarningType.All), Is.False);
            Assert.That(PosingSystemConverter.GetVisibleWarnings(posing), Is.EqualTo(PosingSystem.WarningType.AutoFootsteps));
            Assert.That(posing.IsWarningIgnored(PosingSystem.WarningType.PrebuildOutOfDate), Is.True);
        }

        [Test]
        public void IgnorePrebuildUpdate_SuppressesBuildNotificationButStillRequiresRegeneration()
        {
            posing.data = "legacy-data";
            Assert.That(PosingSystemConverter.ShouldReportPrebuildUpdateWarning(posing), Is.True);

            SetIgnored(posing, PosingSystem.WarningType.PrebuildOutOfDate, true);

            Assert.That(PosingSystemConverter.ShouldReportPrebuildUpdateWarning(posing), Is.False);
            Assert.That(PosingSystemConverter.IsPosingSystemDataUpdated(posing), Is.True);
            Assert.That(posing.data, Is.EqualTo("legacy-data"));
            SetIgnored(posing, PosingSystem.WarningType.PrebuildOutOfDate, false);
            Assert.That(PosingSystemConverter.ShouldReportPrebuildUpdateWarning(posing), Is.True);
        }

        [Test]
        public void DisplayPreferences_DoNotInvalidateCurrentPrebuild()
        {
            posing.data = PosingSystemConverter.GetDefineSerializeJson(posing);
            var before = posing.data;
            SetIgnored(posing, PosingSystem.WarningType.All, true);
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(posing), Is.EqualTo(before));
            Assert.That(PosingSystemConverter.IsPosingSystemDataUpdated(posing), Is.False);
        }

        [Test]
        public void HierarchyBadge_IgnoringWarningsKeepsErrorsVisible()
        {
            // Deliberately missing a humanoid Avatar: still an error.
            avatar.autoFootsteps = true;
            var method = typeof(PosingSystemHierarchyBadge).GetMethod("GetStatus", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            var before = ((bool, bool))method.Invoke(null, new object[] { posing });
            Assert.That(before.Item1, Is.True);
            Assert.That(before.Item2, Is.True);

            SetIgnored(posing, PosingSystem.WarningType.All, true);

            var after = ((bool, bool))method.Invoke(null, new object[] { posing });
            Assert.That(after.Item1, Is.False);
            Assert.That(after.Item2, Is.True);
        }

        [Test]
        public void IgnoreButtonChange_SupportsUndoRedoAndInvalidatesBadge()
        {
            Undo.IncrementCurrentGroup();
            posing.previousErrorCheckTime = DateTime.Now;
            SetIgnored(posing, PosingSystem.WarningType.AutoFootsteps, true);
            Undo.FlushUndoRecordObjects();
            Assert.That(posing.previousErrorCheckTime, Is.EqualTo(DateTime.MinValue));
            Undo.IncrementCurrentGroup();
            Undo.PerformUndo();
            Assert.That(posing.IsWarningIgnored(PosingSystem.WarningType.AutoFootsteps), Is.False);
            Undo.PerformRedo();
            Assert.That(posing.IsWarningIgnored(PosingSystem.WarningType.AutoFootsteps), Is.True);
        }

        [Test]
        public void Preferences_AreSerializedAndRemainLocalToEachComponent()
        {
            SetIgnored(posing, PosingSystem.WarningType.PrebuildNotRun, true);
            var copy = new GameObject("WarningPreferenceCopy");
            try
            {
                var target = copy.AddComponent<PosingSystem>();
                Assert.That(target.ignoredWarnings, Is.EqualTo(PosingSystem.WarningType.None));
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(posing), target);
                Assert.That(target.ignoredWarnings, Is.EqualTo(PosingSystem.WarningType.PrebuildNotRun));
                SetIgnored(target, PosingSystem.WarningType.All, false);
                Assert.That(posing.IsWarningIgnored(PosingSystem.WarningType.PrebuildNotRun), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(copy);
            }
        }

        [Test]
        public void IgnoreButtonChange_RecordsPrefabInstanceOverride()
        {
            prefabPath = "Assets/__WarningSettingsTest_" + Guid.NewGuid().ToString("N") + ".prefab";
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            prefabInstance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            var instancePosing = prefabInstance.GetComponent<PosingSystem>();

            SetIgnored(instancePosing, PosingSystem.WarningType.PrebuildOutOfDate, true);

            var modifications = PrefabUtility.GetPropertyModifications(prefabInstance);
            Assert.That(Array.Exists(modifications, m => m.propertyPath == "ignoredWarnings" && m.value == "4"), Is.True);
            Assert.That(prefab.GetComponent<PosingSystem>().ignoredWarnings, Is.EqualTo(PosingSystem.WarningType.None));
        }

        private static void SetIgnored(PosingSystem target, PosingSystem.WarningType warning, bool ignored)
        {
            var method = typeof(PosingSystemEditor).GetMethod("SetWarningIgnored", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(null, new object[] { target, warning, ignored });
        }
    }
}
