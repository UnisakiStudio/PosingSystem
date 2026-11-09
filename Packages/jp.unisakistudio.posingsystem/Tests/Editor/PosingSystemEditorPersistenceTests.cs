using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemEditorPersistenceTests
    {
        private GameObject root;
        private PosingSystem ps;
        private string folder;
        private Scene scene;
        private Scene previousScene;
        private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;
        private Scene batchStartingScene;
        private string batchStartingPath;

        [OneTimeSetUp]
        public void CreateOwnedSavedStartingSceneForBatch()
        {
            if (!Application.isBatchMode || !string.IsNullOrEmpty(SceneManager.GetActiveScene().path)) return;
            batchStartingScene = SceneManager.GetActiveScene();
            // Unity refuses another additive scene while the runner scene is untitled.
            // In batch only, prove it contains no user objects before giving it a test path.
            Assert.That(batchStartingScene.GetRootGameObjects().All(r =>
                r.name == "nadena.dev.ndmf__Activator" && r.transform.childCount == 0 &&
                r.GetComponents<Component>().All(c => c is Transform ||
                    (c != null && c.GetType().FullName == "nadena.dev.ndmf.runtime.ApplyOnPlayGlobalActivator"))),
                Is.True, "Refusing to save an untitled scene containing user objects");
            batchStartingPath = "Assets/USSPS_PersistenceBatch_" + Guid.NewGuid().ToString("N") + ".unity";
            Assert.That(EditorSceneManager.SaveScene(batchStartingScene, batchStartingPath), Is.True);
            SceneManager.SetActiveScene(batchStartingScene);
        }

        [OneTimeTearDown]
        public void RemoveOwnedBatchStartingScene()
        {
            if (string.IsNullOrEmpty(batchStartingPath)) return;
            if (batchStartingScene.IsValid() && batchStartingScene.isLoaded)
            {
                var replacement = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                EditorSceneManager.CloseScene(batchStartingScene, true);
                SceneManager.SetActiveScene(replacement);
            }
            AssetDatabase.DeleteAsset(batchStartingPath);
        }

        [SetUp] public void SetUp()
        {
            previousScene = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(previousScene.path))
                Assert.Ignore("Scene persistence tests require a saved starting scene; do not save a user's untitled scene automatically.");
            scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            folder = "Assets/USSPS_Persistence_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folder.Substring(7));
            root = new GameObject("Persistence test");
            root.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("Poses"); child.transform.SetParent(root.transform);
            ps = child.AddComponent<PosingSystem>();
            ps.gameObject.AddComponent<ModularAvatarParameters>();
            ps.features = new PosingSystemFeatureSettings { mirror = false, footHeight = false,
                breathing = false, fingerGestureControl = false };
            ps.featureSettingsMigrated = true;
            ps.defines = new List<PosingSystem.LayerDefine> {
                new PosingSystem.LayerDefine("", "", "pose", 0, "poses", new List<PosingSystem.AnimationDefine> {
                    new PosingSystem.AnimationDefine(null, "A"), new PosingSystem.AnimationDefine(null, "B") }) };
            Undo.IncrementCurrentGroup();
        }

        [TearDown] public void TearDown()
        {
            if (root != null)
            {
                foreach (var component in root.GetComponentsInChildren<Component>(true))
                    if (component != null) Undo.ClearUndo(component);
                foreach (var transform in root.GetComponentsInChildren<Transform>(true)) Undo.ClearUndo(transform.gameObject);
                Object.DestroyImmediate(root);
            }
            SceneManager.SetActiveScene(previousScene);
            if (scene.IsValid()) EditorSceneManager.CloseScene(scene, true);
            AssetDatabase.DeleteAsset(folder);
            EditorGUIUtility.editingTextField = false;
        }

        private static object Call(object target, string method, params object[] args) =>
            target.GetType().GetMethod(method, Private).Invoke(target, args);
        private static T Field<T>(object target, string name) => (T)target.GetType().GetField(name, Private).GetValue(target);
        private static void Set(object target, string name, object value) => target.GetType().GetField(name, Private).SetValue(target, value);
        private static SerializedProperty Name(SerializedObject serialized, int index) => serialized.FindProperty("defines")
            .GetArrayElementAtIndex(0).FindPropertyRelative("animations").GetArrayElementAtIndex(index).FindPropertyRelative("displayName");

        [Test] public void CopiedAvatarRefreshingImages_PreservesOriginalThumbnailAndCustomIcon()
        {
            var pack = ScriptableObject.CreateInstance<PosingSystemThumbnailPack>();
            var path = folder + "/Original.asset";
            AssetDatabase.CreateAsset(pack, path);
            var thumbnail = new Texture2D(2, 2) { name = "Original thumbnail" };
            AssetDatabase.AddObjectToAsset(thumbnail, pack);
            AssetDatabase.SaveAssets();
            ps.thumbnailPackObject = pack;
            ps.defines[0].animations[0].previewImage = thumbnail;
            ps.defines[0].animations[0].icon = Texture2D.whiteTexture;
            Assert.That(EditorSceneManager.SaveScene(scene, folder + "/Test.unity"), Is.True);
            ps.savedInstanceId = GlobalObjectId.GetGlobalObjectIdSlow(ps).ToString();
            ps.data = PosingSystemConverter.GetDefineSerializeJson(ps);
            var copy = Object.Instantiate(root);
            try
            {
                EditorSceneManager.SaveScene(scene);
                var other = copy.GetComponentInChildren<PosingSystem>();
                Assert.That(GlobalObjectId.GetGlobalObjectIdSlow(other).ToString(), Is.Not.EqualTo(ps.savedInstanceId));
                PosingSystemEditor.ClearThumbnailPack(other);
                Assert.That(PosingSystemConverter.IsPosingSystemDataUpdated(other), Is.False);
                Assert.That(AssetDatabase.LoadAllAssetRepresentationsAtPath(path), Does.Contain(thumbnail));
                Assert.That(other.thumbnailPackObject, Is.Null);
                Assert.That(other.defines[0].animations[0].previewImage, Is.Null);
                Assert.That(other.defines[0].animations[0].icon, Is.EqualTo(Texture2D.whiteTexture));
                Assert.That(ps.defines[0].animations[0].previewImage, Is.EqualTo(thumbnail));
                PosingSystemEditor.ClearThumbnailPack(ps);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
                Assert.That(AssetDatabase.LoadAllAssetRepresentationsAtPath(path).OfType<Texture2D>(), Is.Empty);
            }
            finally { Object.DestroyImmediate(copy); }
        }

        [Test] public void DetailSwitch_CommitsPendingName_EndsTextEditing_AndUndoRedoKeepsNamesSeparate()
        {
            using var serialized = new SerializedObject(ps);
            var browser = System.Activator.CreateInstance(typeof(PosingSystemEditor).Assembly.GetType("jp.unisakistudio.posingsystemeditor.PosingSystemPoseBrowser"));
            Name(serialized, 0).stringValue = "入力中の名前";
            EditorGUIUtility.editingTextField = true;
            Call(browser, "SwitchDetail", serialized, "0:1");
            Assert.That(ps.defines[0].animations.Select(a => a.displayName), Is.EqualTo(new[] { "入力中の名前", "B" }));
            Assert.That(EditorGUIUtility.editingTextField, Is.False);
            Assert.That(GUIUtility.keyboardControl, Is.Zero);
            Undo.IncrementCurrentGroup();
            Name(serialized, 1).stringValue = "別の名前";
            Call(browser, "SwitchDetail", serialized, "0:0");
            Undo.PerformUndo();
            Assert.That(ps.defines[0].animations.Select(a => a.displayName), Is.EqualTo(new[] { "入力中の名前", "B" }));
            Undo.PerformUndo();
            Assert.That(ps.defines[0].animations.Select(a => a.displayName), Is.EqualTo(new[] { "A", "B" }));
            Undo.PerformRedo(); Undo.PerformRedo();
            Assert.That(ps.defines[0].animations.Select(a => a.displayName), Is.EqualTo(new[] { "入力中の名前", "別の名前" }));
        }

        [Test] public void DetailSwitch_WithoutEdits_DoesNotDirtySavedScene()
        {
            Assert.That(EditorSceneManager.SaveScene(scene, folder + "/Test.unity"), Is.True);
            using var serialized = new SerializedObject(ps);
            Call(System.Activator.CreateInstance(typeof(PosingSystemEditor).Assembly.GetType("jp.unisakistudio.posingsystemeditor.PosingSystemPoseBrowser")), "SwitchDetail", serialized, "0:1");
            Assert.That(scene.isDirty, Is.False);
        }

        [Test] public void RenameThenMove_HasIndependentUndoSteps()
        {
            using var serialized = new SerializedObject(ps);
            Name(serialized, 0).stringValue = "Renamed";
            Call(System.Activator.CreateInstance(typeof(PosingSystemEditor).Assembly.GetType("jp.unisakistudio.posingsystemeditor.PosingSystemPoseBrowser")), "MoveRow", serialized, ps, 0, 0, 1, false);
            Undo.FlushUndoRecordObjects(); Undo.PerformUndo();
            Assert.That(ps.defines[0].animations.Select(a => a.displayName), Is.EqualTo(new[] { "Renamed", "B" }));
            Undo.PerformUndo();
            Assert.That(ps.defines[0].animations.Select(a => a.displayName), Is.EqualTo(new[] { "A", "B" }));
        }

        [Test] public void Commit_RecordsPrefabOverride_AndSurvivesSceneReload()
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, folder + "/Avatar.prefab", InteractionMode.AutomatedAction);
            EditorSceneManager.SaveScene(scene, folder + "/Test.unity");
            using (var serialized = new SerializedObject(ps))
            {
                Name(serialized, 0).stringValue = "Saved name";
                PosingSystemEditorChangeUtility.Commit(serialized);
            }
            Assert.That(scene.isDirty, Is.True);
            Assert.That(PrefabUtility.GetPropertyModifications(root).Any(p => p.propertyPath.EndsWith("displayName") && p.value == "Saved name"), Is.True);
            EditorSceneManager.SaveScene(scene);
            Undo.ClearUndo(ps);
            EditorSceneManager.CloseScene(scene, true);
            scene = EditorSceneManager.OpenScene(folder + "/Test.unity", OpenSceneMode.Additive);
            root = scene.GetRootGameObjects().Single(); ps = root.GetComponentInChildren<PosingSystem>();
            Assert.That(ps.defines[0].animations[0].displayName, Is.EqualTo("Saved name"));
        }

        [Test] public void DirectChange_RecordsPrefabOverride_AndUndoRedo()
        {
            PrefabUtility.SaveAsPrefabAssetAndConnect(root, folder + "/Avatar.prefab", InteractionMode.AutomatedAction);
            EditorSceneManager.SaveScene(scene, folder + "/Test.unity");
            Undo.RecordObject(ps, "Direct field edit");
            ps.defines[0].animations[0].displayName = "Direct";
            PosingSystemEditorChangeUtility.MarkChanged(ps);
            Undo.FlushUndoRecordObjects();
            Assert.That(scene.isDirty, Is.True);
            Assert.That(PrefabUtility.GetPropertyModifications(root).Any(p => p.value == "Direct"), Is.True);
            Undo.PerformUndo(); Assert.That(ps.defines[0].animations[0].displayName, Is.EqualTo("A"));
            Undo.PerformRedo(); Assert.That(ps.defines[0].animations[0].displayName, Is.EqualTo("Direct"));
        }

        [TestCase(false)] [TestCase(true)]
        public void CatalogDelete_AppliesBeforeExit_AndSupportsUndoRedoAndDiskSave(bool rule)
        {
            var catalog = ScriptableObject.CreateInstance<PosingSystemAutoPoseCatalog>();
            catalog.poses.Add(new PosingSystemAutoPoseDefinition { poseKey = "A", limbRules = new List<PosingSystemAutoPoseLimbRule> {
                new PosingSystemAutoPoseLimbRule { limb = PosingSystemAutoPoseLimb.LeftLeg },
                new PosingSystemAutoPoseLimbRule { limb = PosingSystemAutoPoseLimb.RightLeg } } });
            catalog.poses.Add(new PosingSystemAutoPoseDefinition { poseKey = "B" });
            var path = folder + "/IK.asset";
            AssetDatabase.CreateAsset(catalog, path); AssetDatabase.SaveAssets();
            using var serialized = new SerializedObject(catalog);
            var array = serialized.FindProperty("poses");
            if (rule) array = array.GetArrayElementAtIndex(0).FindPropertyRelative("limbRules");
            PosingSystemEditorChangeUtility.DeleteArrayElement(array, 0);
            Func<int> count = () => rule ? catalog.poses[0].limbRules.Count : catalog.poses.Count;
            Assert.That(count(), Is.EqualTo(1)); Assert.That(EditorUtility.IsDirty(catalog), Is.True);
            Undo.FlushUndoRecordObjects(); Undo.PerformUndo(); Assert.That(count(), Is.EqualTo(2));
            Undo.PerformRedo(); Assert.That(count(), Is.EqualTo(1));
            AssetDatabase.SaveAssetIfDirty(catalog);
            var saved = System.IO.File.ReadAllText(path);
            Assert.That(saved, Does.Contain(rule ? "limb: 3" : "poseKey: B"));
            Undo.ClearUndo(catalog);
        }

        [TestCase("photography")] [TestCase("floating")] [TestCase("rollover")]
        public void FeatureUndoRedo_RestoresParametersAndCompleteMenuHierarchy(string feature)
        {
            PosingSystemFeatureUtility.Synchronize(ps);
            var parameters = ps.GetComponent<ModularAvatarParameters>();
            var before = EditorJsonUtility.ToJson(parameters);
            var editor = UnityEditor.Editor.CreateEditor(ps);
            try
            {
                using var serialized = new SerializedObject(ps);
                serialized.FindProperty("features").FindPropertyRelative(feature).boolValue = true;
                PosingSystemEditorChangeUtility.Apply(serialized);
                PosingSystemFeatureUtility.Synchronize(ps, true);
                Undo.FlushUndoRecordObjects();
                var after = EditorJsonUtility.ToJson(parameters);
                Func<string[]> menu = () => ps.GetComponentsInChildren<ModularAvatarMenuItem>(true)
                    .Select(m => m.transform.parent.name + "/" + m.name + ":" + EditorJsonUtility.ToJson(m.Control)).OrderBy(s => s).ToArray();
                var expectedMenu = menu();
                Assert.That(expectedMenu, Is.Not.Empty);
                Undo.PerformUndo();
                Assert.That(EditorJsonUtility.ToJson(parameters), Is.EqualTo(before));
                Assert.That(ps.GetComponentsInChildren<ModularAvatarMenuItem>(true), Is.Empty);
                Assert.That((bool)typeof(PosingSystemFeatureSettings).GetField(feature).GetValue(ps.features), Is.False);
                Undo.PerformRedo();
                Assert.That(EditorJsonUtility.ToJson(parameters), Is.EqualTo(after));
                Assert.That(menu(), Is.EqualTo(expectedMenu));
                Assert.That((bool)typeof(PosingSystemFeatureSettings).GetField(feature).GetValue(ps.features), Is.True);
            }
            finally { Object.DestroyImmediate(editor); }
        }

        [Test] public void UnrelatedUndo_DoesNotRegenerateMenusOrDirtyPosingSettings()
        {
            var editor = UnityEditor.Editor.CreateEditor(ps);
            var other = ScriptableObject.CreateInstance<PosingSystemAutoPoseCatalog>();
            try
            {
                EditorUtility.ClearDirty(ps);
                Undo.RecordObject(other, "Other asset"); other.productName = "changed";
                Undo.FlushUndoRecordObjects(); Undo.PerformUndo();
                Assert.That(EditorUtility.IsDirty(ps), Is.False);
                Assert.That(ps.GetComponentsInChildren<ModularAvatarMenuItem>(true), Is.Empty);
            }
            finally { Undo.ClearUndo(other); Object.DestroyImmediate(other); Object.DestroyImmediate(editor); }
        }

        [Test] public void AdjustmentDrafts_UndoAfterChangingPose_RestoresAllPoseCaches()
        {
            var window = ScriptableObject.CreateInstance<PosingAnimationAdjustmentWindow>();
            try
            {
                var data = Field<Dictionary<string, float[]>>(window, "_allAdjustmentData");
                data["A"] = new float[HumanTrait.MuscleCount]; data["B"] = new float[HumanTrait.MuscleCount];
                var a = (PosingAnimationAdjustmentWindow.Adjustments)Call(window, "GetAdjustmentState", "A");
                var b = (PosingAnimationAdjustmentWindow.Adjustments)Call(window, "GetAdjustmentState", "B");
                Undo.RecordObject(a, "Pose A"); a.adjustments[0] = 15; a.rootTYAdjustment = .3f;
                Undo.FlushUndoRecordObjects(); Undo.IncrementCurrentGroup();
                Undo.RecordObject(b, "Pose B"); b.adjustments[0] = 30; b.rootQAdjustments = Vector3.one * 20;
                Undo.FlushUndoRecordObjects();
                Call(window, "SynchronizeAdjustmentStates");
                Undo.PerformUndo(); Call(window, "SynchronizeAdjustmentStates");
                Assert.That(data["A"][0], Is.EqualTo(15)); Assert.That(data["B"][0], Is.Zero);
                Undo.PerformUndo(); Call(window, "SynchronizeAdjustmentStates");
                Assert.That(data["A"][0], Is.Zero);
                Undo.PerformRedo(); Undo.PerformRedo(); Call(window, "SynchronizeAdjustmentStates");
                Assert.That(data["A"][0], Is.EqualTo(15)); Assert.That(data["B"][0], Is.EqualTo(30));
                Assert.That(Field<Dictionary<string, float>>(window, "_allRootTYAdjustmentData")["A"], Is.EqualTo(.3f));
                Undo.IncrementCurrentGroup(); Call(window, "ResetAllAdjustmentValues");
                Undo.FlushUndoRecordObjects(); Assert.That(data["A"][0], Is.Zero); Assert.That(data["B"][0], Is.Zero);
                Undo.PerformUndo(); Call(window, "SynchronizeAdjustmentStates");
                Assert.That(data["A"][0], Is.EqualTo(15)); Assert.That(data["B"][0], Is.EqualTo(30));
            }
            finally { Object.DestroyImmediate(window); }
        }

        [TestCase("ReloadRootTYParameter")] [TestCase("ReloadRootQParameter")] [TestCase("ReloadMuscleParameter")]
        public void ReloadAdjustmentParameter_CanUndoToUnsavedDraft(string method)
        {
            var window = ScriptableObject.CreateInstance<PosingAnimationAdjustmentWindow>();
            try
            {
                Set(window, "_posingSystem", ps);
                Call(window, "RebuildAnimationEntries");
                Set(window, "_selectedAnimationIndex", 0);
                Call(window, "PreloadAllAdjustmentData", false);
                var key = Field<Dictionary<string, float[]>>(window, "_allAdjustmentData").Keys.First();
                var state = (PosingAnimationAdjustmentWindow.Adjustments)Call(window, "GetAdjustmentState", key);
                Set(window, "_adjustments", state);
                state.rootTYAdjustment = .4f; state.rootQAdjustments = new Vector3(20, 30, 40); state.adjustments[0] = 15;
                if (method == "ReloadRootTYParameter") Call(window, method); else Call(window, method, 0);
                Undo.FlushUndoRecordObjects();
                Assert.That(method == "ReloadRootTYParameter" ? state.rootTYAdjustment :
                    method == "ReloadRootQParameter" ? state.rootQAdjustments.x : state.adjustments[0], Is.Zero);
                Undo.PerformUndo();
                Assert.That(state.rootTYAdjustment, Is.EqualTo(.4f));
                Assert.That(state.rootQAdjustments, Is.EqualTo(new Vector3(20, 30, 40)));
                Assert.That(state.adjustments[0], Is.EqualTo(15));
            }
            finally { Object.DestroyImmediate(window); }
        }

        [Test] public void UndoClipSave_RestoresCurves_AndRefreshesSavedBaselineWithoutLosingDrafts()
        {
            var clip = new AnimationClip();
            var binding = EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.y");
            AnimationUtility.SetEditorCurve(clip, binding, AnimationCurve.Constant(0, 1, .1f));
            AssetDatabase.CreateAsset(clip, folder + "/Adjustment.anim");
            ps.defines[0].animations[0].adjustmentClip = clip;
            var window = ScriptableObject.CreateInstance<PosingAnimationAdjustmentWindow>();
            try
            {
                Set(window, "_posingSystem", ps); Call(window, "RebuildAnimationEntries");
                Set(window, "_selectedAnimationIndex", 0); Call(window, "PreloadAllAdjustmentData", false);
                var key = Field<Dictionary<string, float[]>>(window, "_allAdjustmentData").Keys.First();
                var state = (PosingAnimationAdjustmentWindow.Adjustments)Call(window, "GetAdjustmentState", key);
                state.rootTYAdjustment = .6f; Set(window, "_adjustments", state);
                Call(window, "SaveCurrentAdjustmentData");
                var entry = Call(window, "GetSelectedEntry");
                Call(window, "SaveAdjustmentClipForEntry", entry, state.adjustments, Vector3.zero, .6f);
                Undo.FlushUndoRecordObjects();
                Assert.That(AnimationUtility.GetEditorCurve(clip, binding).keys[0].value, Is.EqualTo(.6f));
                Set(window, "_isOpen", true);
                Undo.PerformUndo();
                Assert.That(AnimationUtility.GetEditorCurve(clip, binding).keys[0].value, Is.EqualTo(.1f));
                Assert.That(Field<Dictionary<string, float>>(window, "_originalRootTYAdjustmentData")[key], Is.EqualTo(.1f));
                Assert.That(Field<Dictionary<string, float>>(window, "_allRootTYAdjustmentData")[key], Is.EqualTo(.6f));
                Undo.PerformRedo();
                Assert.That(AnimationUtility.GetEditorCurve(clip, binding).keys[0].value, Is.EqualTo(.6f));
                Assert.That(Field<Dictionary<string, float>>(window, "_originalRootTYAdjustmentData")[key], Is.EqualTo(.6f));
            }
            finally { typeof(EditorWindow).GetProperty("hasUnsavedChanges").SetValue(window, false); Object.DestroyImmediate(window); Undo.ClearUndo(clip); }
        }
    }
}
