using System;
using System.Linq;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(false, false, 1f)]
        [TestCase(false, true, 1f)]
        [TestCase(false, true, .6f)]
        [TestCase(false, true, 1.5f)]
        [TestCase(true, false, 1f)]
        public void UserControls_PreviewPlacement_IsAppliedExactlyOnce(bool ik, bool legacy, float scale)
        {
            var ps = UserSettings(out var entry);
            ps.enablePoseIk = ik;
            ps.enableAvatarSpecificAdjustment = legacy;
            if (legacy)
            {
                var adjustment = new AnimationClip(); assets.Add(adjustment);
                AnimationUtility.SetEditorCurve(adjustment,
                    EditorCurveBinding.FloatCurve("", typeof(Animator), "Spine Front-Back"),
                    AnimationCurve.Constant(0, 1, .12f));
                ps.avatarSpecificAdjustmentClip = adjustment;
            }
            var bones = new[] { HumanBodyBones.Hips, HumanBodyBones.Head, HumanBodyBones.LeftHand,
                HumanBodyBones.RightHand, HumanBodyBones.LeftFoot, HumanBodyBones.RightFoot };
            root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            root.transform.localScale = Vector3.one * scale;
            Vector3[] localPositions;
            Quaternion[] localRotations;
            try
            {
                Assert.That(PosingSystemQuickPreview.Start(ps, entry, "Test_Pose", out var message, false), Is.True, message);
                var preview = UserPreviewAnimator;
                localPositions = bones.Select(b => preview.transform.InverseTransformPoint(preview.GetBoneTransform(b).position)).ToArray();
                localRotations = bones.Select(b => Quaternion.Inverse(preview.transform.rotation) * preview.GetBoneTransform(b).rotation).ToArray();
            }
            finally { PosingSystemQuickPreview.Stop(); }
            root.transform.SetPositionAndRotation(new Vector3(3, 1, -2), Quaternion.Euler(0, 67, 0));
            var sourceBones = root.GetComponentsInChildren<Transform>(true);
            var sourceBefore = sourceBones.Select(t => (t.localPosition, t.localRotation, t.localScale)).ToArray();
            try
            {
                Assert.That(PosingSystemQuickPreview.Start(ps, entry, "Test_Pose", out var message, false), Is.True, message);
                var preview = UserPreviewAnimator;
                for (var i = 0; i < bones.Length; i++)
                {
                    Assert.That(Vector3.Distance(preview.GetBoneTransform(bones[i]).position,
                        root.transform.TransformPoint(localPositions[i])), Is.LessThan(.001f), bones[i] + " placement");
                    Assert.That(Quaternion.Angle(preview.GetBoneTransform(bones[i]).rotation,
                        root.transform.rotation * localRotations[i]), Is.LessThan(.1f), bones[i] + " rotation");
                }
            }
            finally { PosingSystemQuickPreview.Stop(); }
            for (var i = 0; i < sourceBones.Length; i++)
                Assert.That((sourceBones[i].localPosition, sourceBones[i].localRotation, sourceBones[i].localScale), Is.EqualTo(sourceBefore[i]));
        }

        [Test]
        public void UserControls_NoMuscleCorrection_DoesNotRetargetBones()
        {
            var ps = UserSettings(out _);
            ps.enablePoseIk = false; ps.enableAvatarSpecificAdjustment = false;
            ps.enableReverseKneeSafetyClamp = false;
            root.transform.SetPositionAndRotation(new Vector3(4, 1, -3), Quaternion.Euler(0, 60, 0));
            var transforms = root.GetComponentsInChildren<Transform>(true);
            var before = transforms.Select(t => (t.localPosition, t.localRotation, t.localScale)).ToArray();
            PosingSystemAvatarAdjustmentUtility.ApplyToAnimator(ps, animator);
            for (var i = 0; i < transforms.Length; i++)
                Assert.That((transforms[i].localPosition, transforms[i].localRotation, transforms[i].localScale), Is.EqualTo(before[i]));
        }

        [TestCase("Breathing", "breathing")]
        [TestCase("Rollover", "rollover")]
        [TestCase("FloorHeight", "floorHeight")]
        [TestCase("PhotoAssistance", "photoMode")]
        [TestCase("PhotoDistance", "photoDistance")]
        [TestCase("PhotoDirection", "photoDirection")]
        [TestCase("PhotoExit", "photoExit")]
        public void UserControls_ProductIconDefaults_UseStoredReferenceAndPreserveCustom(string key, string field)
        {
            var ps = UserSettings(out _);
            var icon = new Texture2D(2, 2); assets.Add(icon);
            typeof(PosingSystem.FeatureMenuIcons).GetField(field).SetValue(ps.featureMenuIcons, icon);
            var menuObject = new GameObject("Generated menu"); menuObject.transform.SetParent(ps.transform, false);
            menuObject.SetActive(false);
            var menu = menuObject.AddComponent<ModularAvatarMenuItem>();
            var method = typeof(PosingSystemFeatureUtility).GetMethod("SetGeneratedMenuIcon", BindingFlags.NonPublic | BindingFlags.Static);
            method.Invoke(null, new object[] {menu, key, false});
            Assert.That(menu.Control.icon, Is.SameAs(icon));
            var custom = new Texture2D(2, 2); assets.Add(custom); menu.Control.icon = custom;
            method.Invoke(null, new object[] {menu, key, false});
            Assert.That(menu.Control.icon, Is.SameAs(custom));
        }

        [TestCase("Breathing", "breathing")]
        [TestCase("Rollover", "rollover")]
        [TestCase("FloorHeight", "floorHeight")]
        [TestCase("PhotoAssistance", "photoMode")]
        [TestCase("PhotoDistance", "photoDistance")]
        [TestCase("PhotoDirection", "photoDirection")]
        [TestCase("PhotoExit", "photoExit")]
        public void UserControls_ProductIconDefaults_UnityNullUsesSharedIcon(string key, string field)
        {
            var ps = UserSettings(out _);
            // Reproduce the Unity-null wrapper returned by a serialized empty reference.
            var missing = new Texture2D(2, 2);
            typeof(PosingSystem.FeatureMenuIcons).GetField(field).SetValue(ps.featureMenuIcons, missing);
            Object.DestroyImmediate(missing);
            Assert.That(missing == null, Is.True);
            Assert.That(ReferenceEquals(missing, null), Is.False);
            var expected = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Packages/jp.unisakistudio.posingsystem/ExpressionsMenuIcons/PosingSystem_" + key + ".png");
            Assert.That(expected != null, Is.True, "Shared artwork must be imported");
            var obj = new GameObject("Generated menu"); obj.transform.SetParent(ps.transform, false);
            var menu = obj.AddComponent<ModularAvatarMenuItem>();
            var method = typeof(PosingSystemFeatureUtility).GetMethod("SetGeneratedMenuIcon", BindingFlags.NonPublic | BindingFlags.Static);
            method.Invoke(null, new object[] { menu, key, false });
            Assert.That(menu.Control.icon, Is.SameAs(expected));
            method.Invoke(null, new object[] { menu, key, false });
            Assert.That(menu.Control.icon, Is.SameAs(expected), "Repeated synchronization retains the icon");
        }

        [TestCase("features.breathingControl", "Breathing", "breathing", "USSPS_Breathing", false)]
        [TestCase("features.rollover", "Rollover", "rollover", "USSPS_Rollover", false)]
        [TestCase("features.floating", "FloorHeight", "floorHeight", "USSPS_FloatHeight", true)]
        [TestCase("features.photography", "PhotoAssistance", "photoMode", "", false)]
        public void UserControls_MenuIcon_EmptyExistingMenuUsesSharedOrProductIcon(
            string setting, string key, string field, string parameter, bool radial)
        {
            var ps = UserSettings(out _);
            var obj = new GameObject(setting == "features.photography" ? "撮影モード" : "Empty menu");
            obj.transform.SetParent(ps.transform, false); obj.SetActive(false); obj.tag = "EditorOnly";
            var menu = obj.AddComponent<ModularAvatarMenuItem>();
            if (radial) menu.Control.subParameters = new[] { new VRCExpressionsMenu.Control.Parameter { name = parameter } };
            else menu.Control.parameter = new VRCExpressionsMenu.Control.Parameter { name = parameter };
            var expected = AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Packages/jp.unisakistudio.posingsystem/ExpressionsMenuIcons/PosingSystem_" + key + ".png");
            Assert.That(expected != null, Is.True);
            Assert.That(PosingSystemFeatureUtility.GetFeatureMenuIcon(ps, setting), Is.SameAs(expected));
            var productIcon = new Texture2D(2, 2); assets.Add(productIcon);
            typeof(PosingSystem.FeatureMenuIcons).GetField(field).SetValue(ps.featureMenuIcons, productIcon);
            Assert.That(PosingSystemFeatureUtility.GetFeatureMenuIcon(ps, setting), Is.SameAs(productIcon));
            Assert.That(menu.Control.icon == null, Is.True, "Inspector lookup must not modify the menu");
        }

        private void AddMissingBehaviourFixture()
        {
            EnsureDefaultIdleTestFolder();
            var path = testFolder + "/MissingScript.prefab";
            // Import a genuine missing MonoBehaviour: a null entry in the native component list.
            var yaml = @"%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!1 &100
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  serializedVersion: 6
  m_Component:
  - component: {fileID: 400}
  - component: {fileID: 11400}
  m_Layer: 0
  m_Name: MissingScriptFixture
  m_TagString: Untagged
  m_IsActive: 1
--- !u!4 &400
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100}
  serializedVersion: 2
  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}
  m_LocalPosition: {x: 0, y: 0, z: 0}
  m_LocalScale: {x: 1, y: 1, z: 1}
  m_Children: []
  m_Father: {fileID: 0}
--- !u!114 &11400
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {fileID: 0}
  m_PrefabInstance: {fileID: 0}
  m_PrefabAsset: {fileID: 0}
  m_GameObject: {fileID: 100}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {fileID: 11500000, guid: MISSING_GUID, type: 3}
  m_Name:
  m_EditorClassIdentifier:
";
            System.IO.File.WriteAllText(path, yaml.Replace("MISSING_GUID", Guid.NewGuid().ToString("N")));
            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.That(prefab, Is.Not.Null);
            Object.Instantiate(prefab, root.transform);
            Assert.That(root.GetComponentsInChildren<Behaviour>(true).Count(b => b == null), Is.EqualTo(1));
        }

        [TestCase("Preview")]
        [TestCase("BakeIk")]
        [TestCase("KneeBaseline")]
        [TestCase("Rollover")]
        public void UserControls_MissingScripts_DoNotBlockPoseOperationsOrModifySource(string operation)
        {
            var ps = UserSettings(out var entry);
            AddMissingBehaviourFixture();
            var transforms = root.GetComponentsInChildren<Transform>(true);
            var before = transforms.Select(t => (t.localPosition, t.localRotation, t.localScale)).ToArray();
            var sourceClip = (AnimationClip)entry.animationClip;
            var clipBefore = EditorJsonUtility.ToJson(sourceClip);
            try
            {
                if (operation == "Preview")
                    Assert.That(PosingSystemQuickPreview.Start(ps, entry, "Test_Pose", out var message, false), Is.True, message);
                else if (operation == "BakeIk")
                    Assert.That(PosingSystemPoseIkUserUtility.TryBake(ps, entry, "Test_Pose", testFolder + "/IK.anim", out var message), Is.True, message);
                else if (operation == "KneeBaseline")
                    Assert.That(PosingSystemAvatarAdjustmentUtility.TryReadKneeBaseline(animator, out _, out var message), Is.True, message);
                else
                {
                    var clip = PosingSystemRolloverGenerator.Create(root, sourceClip, sourceClip);
                    assets.Add(clip);
                    Assert.That(AnimationUtility.GetCurveBindings(clip), Is.Not.Empty);
                }
            }
            finally { PosingSystemQuickPreview.Stop(); }
            Assert.That(root.GetComponentsInChildren<Behaviour>(true).Count(b => b == null), Is.EqualTo(1), "Do not remove the user's missing components");
            for (var i=0; i<transforms.Length; i++)
                Assert.That((transforms[i].localPosition, transforms[i].localRotation, transforms[i].localScale), Is.EqualTo(before[i]));
            Assert.That(EditorJsonUtility.ToJson(sourceClip), Is.EqualTo(clipBefore));
        }

        private PosingSystem UserSettings(out PosingSystem.AnimationDefine entry)
        {
            root.AddComponent<VRCAvatarDescriptor>();
            var obj = new GameObject("Pose settings"); obj.transform.SetParent(root.transform, false);
            var ps = obj.AddComponent<PosingSystem>();
            ps.featureSettingsMigrated = true;
            ps.enableAvatarSpecificAdjustment = false;
            ps.overrideDefines = new();
            var source = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.identity, "UserPose");
            assets.Add(source);
            AnimationUtility.SetEditorCurve(source, EditorCurveBinding.FloatCurve("", typeof(Animator), "Left Forearm Stretch"), AnimationCurve.Constant(0, 1, -.35f));
            entry = new PosingSystem.AnimationDefine(source, "Pose");
            ps.defines.Add(new PosingSystem.LayerDefine("", "Sitting", "Pose", 1, "Test", new() { entry }));
            source.SampleAnimation(root, 0);
            var rule = new PosingSystemAutoPoseLimbRule { limb = PosingSystemAutoPoseLimb.LeftArm,
                targetSpace = PosingSystemAutoPoseTargetSpace.AvatarRoot, lengthReference = PosingSystemAutoPoseLengthReference.UpperArm };
            Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryCaptureRuleFromCurrentPose(animator, rule, out var message), Is.True, message);
            rule.normalizedTargetOffset.y += .2f;
            ps.autoPoseCatalog = ScriptableObject.CreateInstance<PosingSystemAutoPoseCatalog>();
            assets.Add(ps.autoPoseCatalog);
            ps.autoPoseCatalog.productionReady = true;
            ps.autoPoseCatalog.poses.Add(new PosingSystemAutoPoseDefinition { poseKey="Test_Pose", sourceAnimation=source, limbRules=new() {rule} });
            return ps;
        }

        private static Animator UserPreviewAnimator => (Animator)typeof(PosingSystemQuickPreview)
            .GetProperty("PreviewAnimator", BindingFlags.NonPublic | BindingFlags.Static).GetValue(null);

        [TestCase("features.breathingControl", "呼吸の動きをつける")]
        [TestCase("features.photographySynced", "撮影モード")]
        [TestCase("features.photographyKeepHeadVisible", "撮影モード")]
        public void UserControls_DependencyNotice_OnlyWhenUnavailable(string property, string required)
        {
            var ps = UserSettings(out _);
            var type = typeof(PosingSystemFeatureUtility).Assembly.GetType("jp.unisakistudio.posingsystemeditor.PosingSystemSettingsInspector");
            var setting = System.Activator.CreateInstance(type.GetNestedType("Setting", BindingFlags.NonPublic), BindingFlags.Instance | BindingFlags.NonPublic,
                null, new object[] { property, "Title", "Description", 0, false, false }, null);
            var method = type.GetMethod("DescriptionFor", BindingFlags.NonPublic | BindingFlags.Static);
            ps.features.breathing = ps.features.photography = false;
            Assert.That((string)method.Invoke(null, new[] {ps, setting}), Does.Contain(required));
            ps.features.breathing = ps.features.photography = true; ps.additiveBreathing.enabled = true;
            Assert.That((string)method.Invoke(null, new[] {ps, setting}), Is.EqualTo("Description"));
        }

        [TestCase("features.mirror", "USSPS_Mirror", false)]
        [TestCase("features.fingerGestureControl", "USSPS_FingerGestureDisable_Menu", false)]
        [TestCase("features.breathingControl", "USSPS_Breathing", false)]
        [TestCase("features.rollover", PosingSystemFeatureUtility.RollActive, false)]
        [TestCase("features.footHeight", "FootHeight", true)]
        [TestCase("features.floating", "USSPS_FloatHeight", true)]
        [TestCase("features.photography", "", false)]
        public void UserControls_MenuIcon_UsesObjectReferenceEvenWhenDisabled(string setting, string parameter, bool radial)
        {
            var ps = UserSettings(out _);
            var icon = new Texture2D(2, 2); assets.Add(icon);
            var obj = new GameObject(setting == "features.photography" ? "撮影モード" : "Custom menu");
            obj.transform.SetParent(ps.transform, false); obj.SetActive(false); obj.tag = "EditorOnly";
            var menu = obj.AddComponent<ModularAvatarMenuItem>();
            menu.Control.icon = icon;
            if (radial) menu.Control.subParameters = new[] { new VRCExpressionsMenu.Control.Parameter {name=parameter} };
            else menu.Control.parameter = new VRCExpressionsMenu.Control.Parameter {name=parameter};
            Assert.That(PosingSystemFeatureUtility.GetFeatureMenuIcon(ps, setting), Is.SameAs(icon));
            var secondIcon = new Texture2D(2, 2); assets.Add(secondIcon); menu.Control.icon = secondIcon;
            Assert.That(PosingSystemFeatureUtility.GetFeatureMenuIcon(ps, setting), Is.SameAs(secondIcon), "Do not cache another product's art");
        }

        [Test]
        public void UserControls_PerPoseSwitch_SerializesAndInvalidatesPrebuildWithoutChangingCatalog()
        {
            var ps = UserSettings(out var entry);
            var other = new PosingSystem.AnimationDefine((AnimationClip)entry.animationClip, "Other"); ps.defines[0].animations.Add(other);
            var catalog = EditorJsonUtility.ToJson(ps.autoPoseCatalog);
            var before = PosingSystemConverter.GetDefineSerializeJson(ps);
            Assert.That(ps.UsesAutoPoseFor(entry), Is.True);
            PosingSystemPoseIkUserUtility.SetEnabled(ps, entry, false);
            Assert.That(ps.UsesAutoPoseFor(entry), Is.False);
            Assert.That(ps.UsesAutoPoseFor(other), Is.True);
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps), Is.Not.EqualTo(before));
            Assert.That(EditorJsonUtility.ToJson(ps.autoPoseCatalog), Is.EqualTo(catalog));
            var serialized = EditorJsonUtility.ToJson(ps);
            entry.disablePoseIk = false;
            EditorJsonUtility.FromJsonOverwrite(serialized, ps);
            Assert.That(ps.defines[0].animations[0].disablePoseIk, Is.True);
            Assert.That(ps.defines[0].animations[1].disablePoseIk, Is.False);
        }

        [Test]
        public void UserControls_PerPoseSwitch_UndoRedoRestoresValueAndDirtyState()
        {
            var ps = UserSettings(out var entry);
            Undo.IncrementCurrentGroup();
            PosingSystemPoseIkUserUtility.SetEnabled(ps, entry, false);
            Undo.FlushUndoRecordObjects();
            Assert.That(EditorUtility.IsDirty(ps), Is.True);
            Undo.PerformUndo();
            Assert.That(ps.defines[0].animations[0].disablePoseIk, Is.False);
            Undo.PerformRedo();
            Assert.That(ps.defines[0].animations[0].disablePoseIk, Is.True);
            Undo.ClearUndo(ps);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void UserControls_QuickPreview_LeavesSourceUntouchedAndRestoresVisibility(bool initiallyHidden)
        {
            var ps = UserSettings(out var entry);
            var body = new GameObject("Body mesh", typeof(SkinnedMeshRenderer)); body.transform.SetParent(root.transform, false);
            var clothing = new GameObject("Already hidden clothing", typeof(SkinnedMeshRenderer)); clothing.transform.SetParent(root.transform, false);
            SceneVisibilityManager.instance.Hide(clothing, false);
            root.transform.SetPositionAndRotation(new Vector3(3, 2, -4), Quaternion.Euler(0, 27, 0));
            var transforms = root.GetComponentsInChildren<Transform>(true);
            var before = transforms.Select(t => (t.localPosition, t.localRotation, t.localScale)).ToArray();
            var json = EditorJsonUtility.ToJson(ps);
            if (initiallyHidden) SceneVisibilityManager.instance.Hide(root, false);
            try
            {
                Assert.That(PosingSystemQuickPreview.Start(ps, entry, "Test_Pose", out var message, false), Is.True, message);
                var clone = UserPreviewAnimator;
                Assert.That(clone, Is.Not.SameAs(animator));
                Assert.That(SceneVisibilityManager.instance.IsHidden(body), Is.True, "The source's child meshes must also be hidden");
                Assert.That(SceneVisibilityManager.instance.IsHidden(clothing), Is.True);
                Assert.That(clone.transform.position, Is.EqualTo(root.transform.position));
                Assert.That((clone.gameObject.hideFlags & HideFlags.DontSaveInBuild) != 0, Is.True);
                for (var i=0; i<transforms.Length; i++)
                    Assert.That((transforms[i].localPosition, transforms[i].localRotation, transforms[i].localScale), Is.EqualTo(before[i]));
                Assert.That(EditorJsonUtility.ToJson(ps), Is.EqualTo(json));
                Assert.That(PosingSystemQuickPreview.Start(ps, entry, "Test_Pose", out message, false), Is.True, message);
                Assert.That(clone == null, Is.True, "Switching previews must destroy the previous clone");
            }
            finally { PosingSystemQuickPreview.Stop(); }
            Assert.That(SceneVisibilityManager.instance.IsHidden(root), Is.EqualTo(initiallyHidden));
            Assert.That(SceneVisibilityManager.instance.IsHidden(body), Is.False);
            Assert.That(SceneVisibilityManager.instance.IsHidden(clothing), Is.True, "Preserve the user's per-mesh visibility");
            Assert.That(PosingSystemQuickPreview.IsActive, Is.False);
            SceneVisibilityManager.instance.Show(root, false);
        }

        [Test]
        public void UserControls_QuickPreview_DoesNotInterruptAnotherAnimationPreview()
        {
            var ps = UserSettings(out var entry);
            AnimationMode.StartAnimationMode();
            try
            {
                Assert.That(PosingSystemQuickPreview.Start(ps, entry, "Test_Pose", out _, false), Is.False);
                PosingSystemQuickPreview.Stop();
                Assert.That(AnimationMode.InAnimationMode(), Is.True);
            }
            finally { AnimationMode.StopAnimationMode(); }
        }

        [Test]
        public void UserControls_Bake_PreservesOldAssetAndCatalog_ManualPreviewMatchesIk()
        {
            var ps = UserSettings(out var entry);
            EnsureDefaultIdleTestFolder();
            var old = new AnimationClip(); AssetDatabase.CreateAsset(old, testFolder + "/Existing.anim");
            entry.adjustmentClip = old;
            var catalog = EditorJsonUtility.ToJson(ps.autoPoseCatalog);
            float[] expected;
            try
            {
                Assert.That(PosingSystemQuickPreview.Start(ps, entry, "Test_Pose", out var message, false), Is.True, message);
                using var handler = new HumanPoseHandler(UserPreviewAnimator.avatar, UserPreviewAnimator.transform);
                var pose = new HumanPose(); handler.GetHumanPose(ref pose); expected = (float[])pose.muscles.Clone();
            }
            finally { PosingSystemQuickPreview.Stop(); }
            Assert.That(PosingSystemPoseIkUserUtility.TryBake(ps, entry, "Test_Pose", testFolder + "/Existing.anim", out var path), Is.True, path);
            Assert.That(entry.disablePoseIk, Is.True);
            Assert.That(path, Is.Not.EqualTo(testFolder + "/Existing.anim"));
            Assert.That(AssetDatabase.LoadAssetAtPath<AnimationClip>(testFolder + "/Existing.anim"), Is.SameAs(old));
            Assert.That(AnimationUtility.GetCurveBindings(old), Is.Empty);
            Assert.That(AnimationUtility.GetCurveBindings(entry.adjustmentClip), Is.Not.Empty);
            Assert.That(EditorJsonUtility.ToJson(ps.autoPoseCatalog), Is.EqualTo(catalog));
            try
            {
                Assert.That(PosingSystemQuickPreview.Start(ps, entry, "Test_Pose", out var message, false), Is.True, message);
                using var handler = new HumanPoseHandler(UserPreviewAnimator.avatar, UserPreviewAnimator.transform);
                var pose = new HumanPose(); handler.GetHumanPose(ref pose);
                for (var i=0; i<expected.Length; i++) Assert.That(pose.muscles[i], Is.EqualTo(expected[i]).Within(.002f), HumanTrait.MuscleName[i]);
            }
            finally { PosingSystemQuickPreview.Stop(); }
        }

        [Test]
        public void UserControls_Bake_UndoRedoRestoresClipAndSwitchTogether()
        {
            var ps = UserSettings(out var entry);
            EnsureDefaultIdleTestFolder();
            Undo.IncrementCurrentGroup();
            Assert.That(PosingSystemPoseIkUserUtility.TryBake(ps, entry, "Test_Pose", testFolder + "/Baked.anim", out var path), Is.True, path);
            var baked = entry.adjustmentClip;
            Undo.FlushUndoRecordObjects();
            Undo.PerformUndo();
            Assert.That(ps.defines[0].animations[0].disablePoseIk, Is.False);
            Assert.That(ps.defines[0].animations[0].adjustmentClip, Is.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<AnimationClip>(path), Is.SameAs(baked));
            Undo.PerformRedo();
            Assert.That(ps.defines[0].animations[0].disablePoseIk, Is.True);
            Assert.That(ps.defines[0].animations[0].adjustmentClip, Is.SameAs(baked));
            Undo.ClearUndo(ps);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void UserControls_NdmfBuild_PerPoseOffUsesManualAdjustment(bool prebuild)
        {
            var authored = UserSettings(out var entry);
            var catalog = authored.autoPoseCatalog;
            Object.DestroyImmediate(authored.gameObject);
            var ps = DefaultIdleSettings(false, false);
            ps.autoPoseCatalog = catalog;
            ps.enableAvatarSpecificAdjustment = false;
            ps.defines.Add(new PosingSystem.LayerDefine("", "Sitting", "Pose", 1, "Test", new() {entry}));
            entry.adjustmentClip = Clip();
            var binding = EditorCurveBinding.FloatCurve("", typeof(Animator), "Left Forearm Stretch");
            AnimationUtility.SetEditorCurve(entry.adjustmentClip, binding, AnimationCurve.Constant(0, 1, .3f));
            PosingSystemPoseIkUserUtility.SetEnabled(ps, entry, false);
            if (prebuild) PosingSystemEditor.Prebuild(ps);
            BuildDefaultIdleAvatar((descriptor, additive) => {
                var controller = (UnityEditor.Animations.AnimatorController)descriptor.baseAnimationLayers
                    .Single(l => l.type == VRCAvatarDescriptor.AnimLayerType.Base).animatorController;
                var clips = controller.layers.SelectMany(l => DefaultIdleTestStates(l.stateMachine))
                    .Where(s => s.name.StartsWith("UserPose")).Select(s => s.motion).OfType<AnimationClip>().ToArray();
                // Other layers also contain head/finger-only clips for this pose.
                var armCurves = clips.Select(c => AnimationUtility.GetEditorCurve(c, binding)).Where(c => c != null).ToArray();
                Assert.That(armCurves.Length, Is.GreaterThanOrEqualTo(2), string.Join("\n", clips.Select(c => c.name + ": " + string.Join(",", AnimationUtility.GetCurveBindings(c).Select(b => b.propertyName)))));
                foreach (var curve in armCurves)
                    Assert.That(curve.Evaluate(0), Is.EqualTo(-.05f).Within(.0002f));
            });
        }
    }
}
