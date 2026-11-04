using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemPoseListTests
    {
        private string folder;
        private GameObject root;
        private PosingSystem ps;
        private AnimationClip first, second;

        [SetUp]
        public void SetUp()
        {
            folder = "Assets/USSPS_PoseListTests_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folder.Substring(7));
            first = new AnimationClip { name = "Same name" }; second = new AnimationClip { name = "Same name" };
            AssetDatabase.CreateFolder(folder, "A"); AssetDatabase.CreateFolder(folder, "B");
            AssetDatabase.CreateAsset(first, folder + "/A/Same name.anim"); AssetDatabase.CreateAsset(second, folder + "/B/Same name.anim");
            root = new GameObject("Pose list tests"); root.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Poses"); child.transform.SetParent(root.transform);
            ps = child.AddComponent<PosingSystem>();
            ps.defines = new List<PosingSystem.LayerDefine> { Layer("pose"), Layer("other") };
        }
        private static PosingSystem.LayerDefine Layer(string parameter) => new PosingSystem.LayerDefine("", "", parameter, 0, parameter, new List<PosingSystem.AnimationDefine>());

        [TearDown]
        public void TearDown()
        {
            Undo.ClearUndo(ps);
            UnityEngine.Object.DestroyImmediate(root);
            AssetDatabase.DeleteAsset(folder);
        }

        [Test]
        public void BulkAdd_DeduplicatesByReference_PreservesNamesAndInitialPose_AndUndoesTogether()
        {
            ps.defines[0].animations.Add(new PosingSystem.AnimationDefine(first, "Existing") { initial = true, initialSet = true });
            Assert.That(PosingSystemPoseListUtility.AddClips(ps, 0, new[] { first, second, second }), Is.EqualTo(1));
            Assert.That(ps.defines[0].animations.Count, Is.EqualTo(2));
            Assert.That(ps.defines[0].animations[0].initial, Is.True);
            Assert.That(ps.defines[0].animations[1].displayName, Is.EqualTo("Same name"));
            Assert.That(ps.defines[0].animations[1].initial, Is.False);
            Undo.FlushUndoRecordObjects(); Undo.PerformUndo();
            Assert.That(ps.defines[0].animations.Count, Is.EqualTo(1));
            Assert.That(ps.defines[0].animations[0].displayName, Is.EqualTo("Existing"));
            Undo.PerformRedo();
            Assert.That(ps.defines[0].animations.Count, Is.EqualTo(2));
        }

        [Test]
        public void BulkAdd_FiltersTransientAndPreviewClips_AndTargetsOnlySelectedCategory()
        {
            var transient = new AnimationClip();
            var preview = new AnimationClip { name = "__preview__Take" };
            AssetDatabase.CreateAsset(preview, folder + "/__preview__Take.anim");
            try
            {
                Assert.That(PosingSystemPoseListUtility.AddClips(ps, 1, new[] { transient, preview, first, second }), Is.EqualTo(2));
                Assert.That(ps.defines[0].animations, Is.Empty);
                Assert.That(ps.defines[1].animations.Count, Is.EqualTo(2));
                Assert.That(PosingSystemPoseListUtility.AddClips(ps, -1, new[] { first }), Is.Zero);
            }
            finally { UnityEngine.Object.DestroyImmediate(transient); }
        }

        [Test]
        public void InitialPose_IsExclusiveAcrossToolsWithSameParameter_AndUndoRestoresBoth()
        {
            var otherObject = new GameObject("Second tool"); otherObject.transform.SetParent(root.transform);
            var other = otherObject.AddComponent<PosingSystem>();
            other.defines = new List<PosingSystem.LayerDefine> { Layer("pose"), Layer("other") };
            ps.defines[0].animations.Add(new PosingSystem.AnimationDefine(first, "A"));
            other.defines[0].animations.Add(new PosingSystem.AnimationDefine(second, "B") { initial = true, initialSet = true });
            other.defines[1].animations.Add(new PosingSystem.AnimationDefine(first, "C") { initial = true, initialSet = true });
            PosingSystemPoseListUtility.SetInitial(ps, 0, 0, true);
            Assert.That(ps.defines[0].animations[0].initial, Is.True);
            Assert.That(other.defines[0].animations[0].initial, Is.False);
            Assert.That(other.defines[1].animations[0].initial, Is.True);
            Undo.FlushUndoRecordObjects(); Undo.PerformUndo();
            Assert.That(ps.defines[0].animations[0].initial, Is.False);
            Assert.That(other.defines[0].animations[0].initial, Is.True);
            Undo.ClearUndo(other);
        }

        [Test]
        public void Search_MatchesDisplayNameOrAssetName_WithoutCaseOrOuterWhitespace()
        {
            Assert.That(PosingSystemPoseListUtility.Matches(" SAME ", "別名", first), Is.True);
            Assert.That(PosingSystemPoseListUtility.Matches("寝", "寝姿勢", second), Is.True);
            Assert.That(PosingSystemPoseListUtility.Matches("unknown", "寝姿勢", null), Is.False);
        }

        [Test]
        public void CollectClips_ExpandsContainerSubassets_AndIgnoresNonClipsAndPreview()
        {
            var container = UnityEditor.Animations.AnimatorController.CreateAnimatorControllerAtPath(folder + "/Container.controller");
            var take = new AnimationClip { name = "Take" };
            var preview = new AnimationClip { name = "__preview__Take" };
            AssetDatabase.AddObjectToAsset(take, container); AssetDatabase.AddObjectToAsset(preview, container);
            AssetDatabase.SaveAssets();
            var clips = PosingSystemPoseListUtility.CollectClips(new UnityEngine.Object[] { root, container, take });
            Assert.That(clips, Is.EqualTo(new[] { take }));
        }

        [Test]
        public void BulkAdd_RecordsPrefabOverrides_AndSerializedReloadPreservesEntries()
        {
            var prefab = PrefabUtility.SaveAsPrefabAsset(ps.gameObject, folder + "/Poses.prefab");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                var component = instance.GetComponent<PosingSystem>();
                Assert.That(PosingSystemPoseListUtility.AddClips(component, 0, new[] { first, second }), Is.EqualTo(2));
                Assert.That(PrefabUtility.HasPrefabInstanceAnyOverrides(instance, false), Is.True);
                var json = EditorJsonUtility.ToJson(component);
                component.defines[0].animations.Clear();
                EditorJsonUtility.FromJsonOverwrite(json, component);
                Assert.That(component.defines[0].animations.Count, Is.EqualTo(2));
                Assert.That(component.defines[0].animations[1].animationClip, Is.EqualTo(second));
                Undo.ClearUndo(component);
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }

        private static readonly Type BrowserType = typeof(PosingSystemPoseListUtility).Assembly.GetType(
            "jp.unisakistudio.posingsystemeditor.PosingSystemPoseBrowser", true);
        private static void SetBrowser(object browser, string field, object value) => BrowserType.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(browser, value);
        private static T ReadBrowser<T>(object browser, string field) => (T)BrowserType.GetField(field, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(browser);
        private bool Move(object browser, int from, int to, int layer = 0) => (bool)BrowserType.GetMethod("MoveRow", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(browser, new object[] { new SerializedObject(ps), ps, layer, from, to, false });

        [Test]
        public void Reorder_ConsecutiveMovesKeepDetailsAndViewportAnchor_WithAllPoseSettings()
        {
            var moved = new PosingSystem.AnimationDefine(second, "Moved") {
                initial = true, isRollover = true, isMotionTime = true, motionTimeParamName = "PreviousTime",
                isRotate = true, rotate = 45, adjustmentClip = first, isCustomIcon = true, icon = Texture2D.whiteTexture };
            ps.defines[0].animations.AddRange(new[] { new PosingSystem.AnimationDefine(first, "A"), new PosingSystem.AnimationDefine(first, "B"), moved });
            ps.defines[1].animations.Add(new PosingSystem.AnimationDefine(second, "Other category"));
            var browser = Activator.CreateInstance(BrowserType, true);
            SetBrowser(browser, "expanded", "0:2");
            SetBrowser(browser, "expandedRowY", 250f);
            SetBrowser(browser, "listScroll", new Vector2(0, 230));
            Assert.That(Move(browser, 2, 1), Is.True);
            Assert.That(ReadBrowser<string>(browser, "expanded"), Is.EqualTo("0:1"));
            BrowserType.GetMethod("AnchorExpandedRow", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(browser, new object[] { 190f });
            Assert.That(ReadBrowser<Vector2>(browser, "listScroll").y, Is.EqualTo(170));
            Assert.That(Move(browser, 1, 0), Is.True);
            Assert.That(ReadBrowser<string>(browser, "expanded"), Is.EqualTo("0:0"));
            BrowserType.GetMethod("AnchorExpandedRow", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(browser, new object[] { 130f });
            Assert.That(ReadBrowser<Vector2>(browser, "listScroll").y, Is.EqualTo(110));
            var result = ps.defines[0].animations[0];
            Assert.That(result.animationClip, Is.SameAs(second));
            Assert.That(result.adjustmentClip, Is.SameAs(first));
            Assert.That(result.initial && result.isRollover && result.isMotionTime && result.isCustomIcon, Is.True);
            Assert.That(result.motionTimeParamName, Is.EqualTo("PreviousTime"));
            Assert.That(result.rotate, Is.EqualTo(45));
            Assert.That(result.icon, Is.SameAs(Texture2D.whiteTexture));
            Assert.That(ps.defines[1].animations.Single().displayName, Is.EqualTo("Other category"));
        }

        [TestCase(0, 3)]
        [TestCase(3, 0)]
        public void Reorder_DestinationJumpInSearch_ChangesOnlyOwningCategory_AndSupportsUndo(int from, int to)
        {
            for (var i = 0; i < 4; i++) ps.defines[1].animations.Add(new PosingSystem.AnimationDefine(first, "Pose " + i));
            var browser = Activator.CreateInstance(BrowserType, true);
            SetBrowser(browser, "category", -1);
            SetBrowser(browser, "search", "Pose");
            Undo.IncrementCurrentGroup();
            Assert.That(Move(browser, from, to, 1), Is.True);
            Assert.That(ps.defines[1].animations[to].displayName, Is.EqualTo("Pose " + from));
            Assert.That(ReadBrowser<string>(browser, "expanded"), Is.EqualTo("1:" + to));
            Assert.That(ReadBrowser<int>(browser, "category"), Is.EqualTo(-1));
            Assert.That(ReadBrowser<string>(browser, "search"), Is.EqualTo("Pose"));
            Assert.That(ps.defines[0].animations, Is.Empty);
            Undo.FlushUndoRecordObjects(); Undo.PerformUndo();
            Assert.That(ps.defines[1].animations.Select(a => a.displayName), Is.EqualTo(Enumerable.Range(0, 4).Select(i => "Pose " + i)));
            Undo.PerformRedo();
            Assert.That(ps.defines[1].animations[to].displayName, Is.EqualTo("Pose " + from));
        }

        [TestCase(-1, 0)]
        [TestCase(0, 2)]
        [TestCase(1, 1)]
        public void Reorder_InvalidOrUnchangedDestinationDoesNotCollapseDetails(int from, int to)
        {
            ps.defines[0].animations.AddRange(new[] { new PosingSystem.AnimationDefine(first, "A"), new PosingSystem.AnimationDefine(second, "B") });
            var browser = Activator.CreateInstance(BrowserType, true);
            SetBrowser(browser, "expanded", "0:1");
            Assert.That(Move(browser, from, to), Is.False);
            Assert.That(ReadBrowser<string>(browser, "expanded"), Is.EqualTo("0:1"));
            Assert.That(ps.defines[0].animations.Select(a => a.displayName), Is.EqualTo(new[] { "A", "B" }));
        }

        [Test]
        public void RolloverFlag_SavesAsPrefabOverride_ReloadsAndInvalidatesPrebuild_WithoutChangingBits()
        {
            ps.defines[0].animations.Add(new PosingSystem.AnimationDefine(first, "A") { isMotionTime = true, motionTimeParamName = "OriginalTime" });
            var prefab = PrefabUtility.SaveAsPrefabAsset(ps.gameObject, folder + "/Rollover.prefab");
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            try
            {
                var component = instance.GetComponent<PosingSystem>();
                var before = PosingSystemConverter.GetDefineSerializeJson(component, true);
                var bits = PosingSystemFeatureUtility.OptionalBits(component);
                using (var serialized = new SerializedObject(component))
                {
                    serialized.FindProperty("defines").GetArrayElementAtIndex(0).FindPropertyRelative("animations")
                        .GetArrayElementAtIndex(0).FindPropertyRelative("isRollover").boolValue = true;
                    serialized.ApplyModifiedProperties();
                    PosingSystemPoseListUtility.MarkChanged(component);
                }
                Assert.That(PrefabUtility.HasPrefabInstanceAnyOverrides(instance, false), Is.True);
                Assert.That(PosingSystemConverter.GetDefineSerializeJson(component, true), Is.Not.EqualTo(before));
                Assert.That(PosingSystemFeatureUtility.OptionalBits(component), Is.EqualTo(bits));
                PrefabUtility.SaveAsPrefabAsset(instance, folder + "/Configured.prefab");
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(folder + "/Configured.prefab", ImportAssetOptions.ForceUpdate);
                var reloaded = AssetDatabase.LoadAssetAtPath<GameObject>(folder + "/Configured.prefab").GetComponent<PosingSystem>().defines[0].animations[0];
                Assert.That(reloaded.isRollover, Is.True);
                Assert.That(reloaded.motionTimeParamName, Is.EqualTo("OriginalTime"));
                Assert.That(reloaded.isMotionTime, Is.True);
                Undo.ClearUndo(component);
            }
            finally { UnityEngine.Object.DestroyImmediate(instance); }
        }
    }
}
