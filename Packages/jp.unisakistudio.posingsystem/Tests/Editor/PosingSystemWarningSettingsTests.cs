using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
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
            if (posing != null) Undo.ClearUndo(posing);
            if (prefabInstance != null) Object.DestroyImmediate(prefabInstance);
            if (root != null) Object.DestroyImmediate(root);
            if (prefabPath != null) AssetDatabase.DeleteAsset(prefabPath);
        }

        [Test]
        public void UnbuiltHierarchyBadge_IsVisibleByDefaultAndRespectsWarningPreferences()
        {
            Assert.That(posing.ignoredWarnings, Is.EqualTo(PosingSystem.WarningType.None));
            Assert.That(PosingSystemConverter.GetVisibleWarnings(posing), Is.EqualTo(PosingSystem.WarningType.PrebuildNotRun));
            Assert.That(PosingSystemConverter.HasWarning(posing), Is.True);
            Assert.That(PosingSystemHierarchyBadge.GetDisplayStatus(posing).IsWarning, Is.True);
            SetIgnored(posing, PosingSystem.WarningType.PrebuildNotRun, true);
            Assert.That(PosingSystemHierarchyBadge.GetDisplayStatus(posing).IsWarning, Is.False);
            SetIgnored(posing, PosingSystem.WarningType.PrebuildNotRun, false);
            Assert.That(PosingSystemHierarchyBadge.GetDisplayStatus(posing).IsWarning, Is.True);
            posing.hideHierarchyWarnings = true;
            Assert.That(PosingSystemHierarchyBadge.GetDisplayStatus(posing).IsWarning, Is.False);
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
        public void DisplayedOperationWarning_FollowsItsInspectorAndTarget()
        {
            SetIgnored(posing, PosingSystem.WarningType.PrebuildNotRun, true);
            avatar.autoFootsteps = true;
            Assert.That(PosingSystemConverter.GetVisibleWarningsForInspector(posing), Is.EqualTo(PosingSystem.WarningType.None));
            Assert.That(PosingSystemHierarchyBadge.GetDisplayStatus(posing).IsWarning, Is.False);
            var other = new GameObject("Other inspector target");
            var otherPose = other.AddComponent<PosingSystem>();
            const int first = 1001, second = 1002;
            try
            {
                PosingSystemHierarchyBadge.SetInspectorWarning(first, posing);
                PosingSystemHierarchyBadge.SetInspectorWarning(second, posing);
                Assert.That(PosingSystemHierarchyBadge.GetDisplayStatus(posing).IsWarning, Is.True);
                Assert.That(PosingSystemHierarchyBadge.HasInspectorWarning(otherPose), Is.False);
                PosingSystemHierarchyBadge.SetInspectorWarning(first, otherPose);
                Assert.That(PosingSystemHierarchyBadge.GetDisplayStatus(posing).IsWarning, Is.True);
                posing.hideHierarchyWarnings = true;
                Assert.That(PosingSystemHierarchyBadge.GetDisplayStatus(posing).IsWarning, Is.False);
                posing.hideHierarchyWarnings = false;
                PosingSystemHierarchyBadge.SetInspectorWarning(second, null);
                Assert.That(PosingSystemHierarchyBadge.GetDisplayStatus(posing).IsWarning, Is.False);
            }
            finally
            {
                PosingSystemHierarchyBadge.SetInspectorWarning(first, null);
                PosingSystemHierarchyBadge.SetInspectorWarning(second, null);
                Object.DestroyImmediate(other);
            }
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
        public void PhotographyFloorFix_InvalidatesPreviousPrebuildButKeepsCurrentCache()
        {
            var current = PosingSystemConverter.GetDefineSerializeJson(posing, true);
            posing.data = current;
            Assert.That(PosingSystemConverter.IsPosingSystemDataUpdated(posing), Is.False);
            // Alpha.2 has no photography/floor coordination revision in its saved cache.
            posing.data = System.Text.RegularExpressions.Regex.Replace(current,
                "\\\"floatingPhotographyLogicVersion\\\":\\d+,?", "");
            Assert.That(PosingSystemConverter.IsPosingSystemDataUpdated(posing), Is.True,
                "A prebuild saved before the photography/floor fix must not silently keep the old graph");
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
            posing.warningDisplayPosition = PosingSystem.WarningDisplayPosition.InspectorBottom;
            posing.hideHierarchyWarnings = true;
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(posing), Is.EqualTo(before));
            Assert.That(PosingSystemConverter.IsPosingSystemDataUpdated(posing), Is.False);
        }

        [Test]
        public void HierarchyBadge_IgnoringWarningsKeepsErrorsVisible()
        {
            // Deliberately missing a humanoid Avatar: still an error.
            avatar.autoFootsteps = true;
            posing.data = "legacy-data";
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

        [Test]
        public void DisplayPreferences_SurvivePrefabSaveAndReload()
        {
            posing.warningDisplayPosition = PosingSystem.WarningDisplayPosition.InspectorBottom;
            posing.hideHierarchyWarnings = true;
            SetIgnored(posing, PosingSystem.WarningType.PrebuildOutOfDate, true);
            prefabPath = "Assets/__WarningSettingsTest_" + Guid.NewGuid().ToString("N") + ".prefab";
            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            var loaded = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                var restored = loaded.GetComponent<PosingSystem>();
                Assert.That(restored.warningDisplayPosition, Is.EqualTo(PosingSystem.WarningDisplayPosition.InspectorBottom));
                Assert.That(restored.hideHierarchyWarnings, Is.True);
                Assert.That(restored.ignoredWarnings, Is.EqualTo(PosingSystem.WarningType.PrebuildOutOfDate));
            }
            finally { PrefabUtility.UnloadPrefabContents(loaded); }
        }

        private static object Badge(string method, params object[] args) => typeof(PosingSystemHierarchyBadge)
            .GetMethod(method, BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, args);

        private static int Revision => (int)typeof(PosingSystemHierarchyBadge)
            .GetProperty("Revision", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

        private (bool Warning, bool Error) Status() => ((bool, bool))Badge("GetStatus", posing);

        private static readonly Type InspectionCache = typeof(PosingSystemEditor).Assembly
            .GetType("jp.unisakistudio.posingsystemeditor.PosingSystemInspectionCache");
        private bool InspectionUpdated() => (bool)InspectionCache.GetMethod("IsUpdated", BindingFlags.NonPublic | BindingFlags.Static)
            .Invoke(null, new object[] { posing });
        private static int InspectionEvaluations => (int)InspectionCache.GetProperty("EvaluationCount", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

        [Test]
        public void Inspection_UnchangedDrawsReuseComparison_AndDirtySettingsInvalidateImmediately()
        {
            posing.data = PosingSystemConverter.GetDefineSerializeJson(posing);
            Assert.That(InspectionUpdated(), Is.False);
            var count = InspectionEvaluations;
            for (var i = 0; i < 100; i++) Assert.That(InspectionUpdated(), Is.False);
            Assert.That(InspectionEvaluations, Is.EqualTo(count));
            posing.isIconSmall = !posing.isIconSmall;
            EditorUtility.SetDirty(posing);
            Assert.That(InspectionUpdated(), Is.True);
            Assert.That(InspectionEvaluations, Is.EqualTo(count + 1));
            posing.data = PosingSystemConverter.GetDefineSerializeJson(posing);
            Assert.That(InspectionUpdated(), Is.False, "Successful rebuild updates the display immediately.");
        }

        [Test]
        public void Inspection_UnsavedImportedClipEdit_InvalidatesCacheAndHierarchy()
        {
            var clip = new AnimationClip();
            try
            {
                posing.overrideDefines = new List<PosingSystem.OverrideAnimationDefine> {
                    new PosingSystem.OverrideAnimationDefine { animationClip = clip } };
                posing.data = PosingSystemConverter.GetDefineSerializeJson(posing);
                Assert.That(Status().Warning, Is.False);
                var count = InspectionEvaluations;
                clip.SetCurve("", typeof(Transform), "m_LocalPosition.x", AnimationCurve.Linear(0, 0, 1, 1));
                EditorUtility.SetDirty(clip);
                Badge("CheckTrackedChanges");
                Assert.That(InspectionUpdated(), Is.True);
                Assert.That(InspectionEvaluations, Is.GreaterThan(count));
                Assert.That(Status().Warning, Is.True);
            }
            finally { Object.DestroyImmediate(clip); }
        }

        [Test]
        public void Inspection_CacheDoesNotAffectStrictBuildComparison()
        {
            posing.data = PosingSystemConverter.GetDefineSerializeJson(posing);
            Assert.That(InspectionUpdated(), Is.False);
            // Conversion may edit inputs directly before SetDirty or editor notifications.
            posing.isIconSmall = !posing.isIconSmall;
            Assert.That(PosingSystemConverter.IsPosingSystemDataUpdated(posing), Is.True);
        }

        [Test]
        public void Inspection_OtherProductOnSameAvatar_InvalidatesSharedImportInputs()
        {
            var child = new GameObject("Other product");
            child.transform.SetParent(root.transform);
            var other = child.AddComponent<PosingSystem>();
            other.autoImportAvatarAnimations = false;
            posing.data = PosingSystemConverter.GetDefineSerializeJson(posing);
            Assert.That(InspectionUpdated(), Is.False);
            other.autoImportAvatarAnimations = true;
            EditorUtility.SetDirty(other);
            Assert.That(InspectionUpdated(), Is.True);
        }

        [Test]
        public void Inspection_UnsavedBaseControllerStateEdit_InvalidatesComparison()
        {
            var controller = new UnityEditor.Animations.AnimatorController();
            var machine = new UnityEditor.Animations.AnimatorStateMachine();
            var state = machine.AddState("Original");
            try
            {
                controller.AddLayer(new UnityEditor.Animations.AnimatorControllerLayer { name = "Base", stateMachine = machine });
                avatar.baseAnimationLayers = new[] { new VRCAvatarDescriptor.CustomAnimLayer {
                    type = VRCAvatarDescriptor.AnimLayerType.Base, isDefault = false, animatorController = controller } };
                posing.autoImportAvatarAnimations = true;
                posing.data = PosingSystemConverter.GetDefineSerializeJson(posing);
                Assert.That(InspectionUpdated(), Is.False);
                state.speed = 2;
                EditorUtility.SetDirty(state);
                Assert.That(InspectionUpdated(), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(state); Object.DestroyImmediate(machine); Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void SettingsChangeAndUndoRedo_UpdateWarningWithoutReopeningScene()
        {
            posing.data = PosingSystemConverter.GetDefineSerializeJson(posing);
            Assert.That(Status().Warning, Is.False);
            var before = Revision;
            Undo.IncrementCurrentGroup();
            using (var serialized = new SerializedObject(posing))
            {
                serialized.FindProperty("isIconSmall").boolValue = !posing.isIconSmall;
                PosingSystemEditorChangeUtility.Commit(serialized);
            }
            Assert.That(Revision, Is.GreaterThan(before), "The change must request hierarchy repaint, not just set a timestamp.");
            Assert.That(Status().Warning, Is.True);
            Undo.PerformUndo();
            Assert.That(Status().Warning, Is.False);
            Undo.PerformRedo();
            Assert.That(Status().Warning, Is.True);
            // Same snapshot update as the converter at the end of a successful prebuild.
            posing.data = PosingSystemConverter.GetDefineSerializeJson(posing);
            PosingSystemEditorChangeUtility.MarkChanged(posing);
            Assert.That(Status().Warning, Is.False);
        }

        [Test]
        public void DirectDirtyEdit_RequestsRefreshWithoutHierarchyChange()
        {
            posing.data = PosingSystemConverter.GetDefineSerializeJson(posing);
            Assert.That(Status().Warning, Is.False);
            var before = Revision;
            posing.isIconSmall = !posing.isIconSmall;
            EditorUtility.SetDirty(posing);
            Badge("CheckTrackedChanges");
            Assert.That(Revision, Is.GreaterThan(before));
            Assert.That(Status().Warning, Is.True);
            var unchanged = Revision;
            Badge("CheckTrackedChanges");
            Assert.That(Revision, Is.EqualTo(unchanged), "Idle ticks must not continually rebuild warning state.");
        }

        [Test]
        public void UnsavedIkAssetChange_RefreshesWarningForAllUsersOfTheAsset()
        {
            var catalog = ScriptableObject.CreateInstance<PosingSystemAutoPoseCatalog>();
            try
            {
                posing.autoPoseCatalog = catalog;
                posing.data = PosingSystemConverter.GetDefineSerializeJson(posing);
                Assert.That(Status().Warning, Is.False);
                var before = Revision;
                catalog.productionReady = !catalog.productionReady;
                EditorUtility.SetDirty(catalog);
                Badge("CheckTrackedChanges");
                Assert.That(Revision, Is.GreaterThan(before));
                Assert.That(Status().Warning, Is.True);
            }
            finally { Object.DestroyImmediate(catalog); }
        }

        [Test]
        public void ConfigurationWarning_IsConsistentBetweenInspectorAndHierarchy()
        {
            posing.defines.Add(new PosingSystem.LayerDefine("", "", "", 0, "Test", new List<PosingSystem.AnimationDefine>
                { new PosingSystem.AnimationDefine(null, "Missing clip") }));
            Assert.That(PosingSystemDiagnostics.Collect(posing).Any(i => i.Code == "PS025"), Is.True);
            Assert.That(Status().Warning, Is.True);
            SetIgnored(posing, PosingSystem.WarningType.Configuration, true);
            Assert.That(Status().Warning, Is.True, "The independent unbuilt warning still applies.");
            SetIgnored(posing, PosingSystem.WarningType.PrebuildNotRun, true);
            Assert.That(Status().Warning, Is.False);
            Assert.That(Status().Error, Is.True, "Other invalid settings must still report errors.");
        }

        [Test]
        public void HierarchyDisplayPreference_DoesNotSuppressInspectorOrBuildWarningsOrErrors()
        {
            posing.data = "outdated";
            posing.hideHierarchyWarnings = true;
            var displayed = ((bool, bool))Badge("GetDisplayStatus", posing);
            Assert.That(displayed.Item1, Is.False);
            Assert.That(displayed.Item2, Is.True);
            Assert.That(PosingSystemConverter.GetVisibleWarnings(posing) & PosingSystem.WarningType.PrebuildOutOfDate, Is.Not.Zero);
            Assert.That(PosingSystemConverter.ShouldReportPrebuildUpdateWarning(posing), Is.True);
        }

        [Test]
        public void EmptyDescriptorLayers_DoNotBreakHierarchyWarnings()
        {
            avatar.baseAnimationLayers = Array.Empty<VRCAvatarDescriptor.CustomAnimLayer>();
            Assert.DoesNotThrow(() => Status());
            Assert.DoesNotThrow(() => PosingSystemEditor.CheckExistProduct(avatar));
            avatar.baseAnimationLayers = null;
            Assert.DoesNotThrow(() => Status());
            Assert.DoesNotThrow(() => PosingSystemEditor.CheckExistProduct(avatar));
            Assert.DoesNotThrow(() => PosingSystemEditor.CheckExistProduct(null));
        }

        [Test]
        public void IncompleteExpressionAssets_DoNotBreakOldProductDiagnostics()
        {
            var menu = ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
            var parameters = ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>();
            try
            {
                avatar.expressionsMenu = menu;
                avatar.expressionParameters = parameters;
                menu.controls = null; parameters.parameters = null;
                Assert.DoesNotThrow(() => PosingSystemEditor.CheckExistProduct(avatar));
                menu.controls = new List<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control> { null };
                parameters.parameters = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter[] { null };
                Assert.DoesNotThrow(() => PosingSystemEditor.CheckExistProduct(avatar));
            }
            finally { Object.DestroyImmediate(menu); Object.DestroyImmediate(parameters); }
        }

        [Test]
        public void DiagnosticsCache_RefreshesImmediatelyAfterEditAndWhenTargetChanges()
        {
            var type = typeof(PosingSystemEditor).Assembly.GetType("jp.unisakistudio.posingsystemeditor.PosingSystemDiagnosticsInspector");
            var inspector = Activator.CreateInstance(type);
            var method = type.GetMethod("GetIssues", BindingFlags.Instance | BindingFlags.NonPublic);
            List<PosingSystemDiagnostics.Issue> Get() => (List<PosingSystemDiagnostics.Issue>)method.Invoke(inspector, new object[] { posing });
            Assert.That(Get().Any(i => i.Code == "PS025"), Is.False);
            posing.defines.Add(new PosingSystem.LayerDefine("", "", "", 0, "Test", new List<PosingSystem.AnimationDefine>
                { new PosingSystem.AnimationDefine(null, "Missing clip") }));
            PosingSystemEditorChangeUtility.MarkChanged(posing);
            Assert.That(Get().Any(i => i.Code == "PS025"), Is.True);
            var other = new GameObject("Other");
            try
            {
                var otherPosing = other.AddComponent<PosingSystem>();
                var issues = (List<PosingSystemDiagnostics.Issue>)method.Invoke(inspector, new object[] { otherPosing });
                Assert.That(issues.Any(i => i.Code == "PS002"), Is.True);
                Assert.That(issues.Any(i => i.Code == "PS025"), Is.False);
            }
            finally { Object.DestroyImmediate(other); }
        }

        [Test]
        public void DisplayPlacement_SupportsUndoRedoAndSerialization()
        {
            Undo.IncrementCurrentGroup();
            using (var serialized = new SerializedObject(posing))
            {
                serialized.FindProperty("warningDisplayPosition").intValue = (int)PosingSystem.WarningDisplayPosition.InspectorBottom;
                serialized.FindProperty("hideHierarchyWarnings").boolValue = true;
                PosingSystemEditorChangeUtility.Commit(serialized);
            }
            Undo.PerformUndo();
            Assert.That(posing.warningDisplayPosition, Is.EqualTo(PosingSystem.WarningDisplayPosition.InspectorTop));
            Assert.That(posing.hideHierarchyWarnings, Is.False);
            Undo.PerformRedo();
            var copy = new GameObject("Copy");
            try
            {
                var restored = copy.AddComponent<PosingSystem>();
                JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(posing), restored);
                Assert.That(restored.warningDisplayPosition, Is.EqualTo(PosingSystem.WarningDisplayPosition.InspectorBottom));
                Assert.That(restored.hideHierarchyWarnings, Is.True);
            }
            finally { Object.DestroyImmediate(copy); }
        }

        private static void SetIgnored(PosingSystem target, PosingSystem.WarningType warning, bool ignored)
        {
            var method = typeof(PosingSystemEditor).GetMethod("SetWarningIgnored", BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);
            method.Invoke(null, new object[] { target, warning, ignored });
        }
    }
}
