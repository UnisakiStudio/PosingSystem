using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Presets;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemPresetDefinesCacheTests
    {
        private Type cacheType;
        private string folder;
        private string catalogPath;
        private string preferenceKey;
        private bool hadPreference, oldPreference;
        private Scene scene;
        private PosingSystem target;
        private GameObject prefab;
        private Preset preset;
        private PosingSystemPresetDefines definitions;

        [SetUp]
        public void SetUp()
        {
            cacheType = typeof(PosingSystemEditor).Assembly.GetType(
                "jp.unisakistudio.posingsystemeditor.PosingSystemPresetDefinesCache");
            Invoke("ResetForTests");
            preferenceKey = (string)cacheType.GetProperty("VisibilityPreferenceKey", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
            hadPreference = EditorPrefs.HasKey(preferenceKey);
            oldPreference = EditorPrefs.GetBool(preferenceKey);
            EditorPrefs.SetBool(preferenceKey, false);
            folder = "Assets/__PosingPresetCacheTest_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folder.Substring("Assets/".Length));
            catalogPath = folder + "/Catalog.asset";
            scene = EditorSceneManager.NewPreviewScene();
            var template = NewTool("PresetTestTool");
            template.settingName = "Preset setting";
            template.isIconSmall = false;
            template.defines.Add(new PosingSystem.LayerDefine("", "", "Pose", 0, "Saved poses",
                new List<PosingSystem.AnimationDefine> { new PosingSystem.AnimationDefine { displayName = "Saved pose" } }));
            template.overrideDefines = new List<PosingSystem.OverrideAnimationDefine>
            {
                new PosingSystem.OverrideAnimationDefine { enabled = false }
            };
            prefab = PrefabUtility.SaveAsPrefabAsset(template.gameObject, folder + "/Tool.prefab");
            preset = new Preset(template);
            AssetDatabase.CreateAsset(preset, folder + "/Pose.preset");
            definitions = ScriptableObject.CreateInstance<PosingSystemPresetDefines>();
            definitions.presetDefines.Add(new PosingSystemPresetDefines.PresetDefine
            {
                avatarName = "Original avatar", preset = preset,
                prefabs = new List<LazyLoadReference<GameObject>> { prefab },
                prefabsHashes = new List<string> { "avatar-hash" },
                prefabsNames = new List<string> { "Avatar" }
            });
            AssetDatabase.CreateAsset(definitions, folder + "/Definitions.asset");
            AssetDatabase.SaveAssetIfDirty(definitions);
            Object.DestroyImmediate(template.gameObject);
            target = NewTool("PresetTestTool");
        }

        [TearDown]
        public void TearDown()
        {
            Invoke("ResetForTests");
            if (scene.IsValid()) EditorSceneManager.ClosePreviewScene(scene);
            if (!string.IsNullOrEmpty(folder)) AssetDatabase.DeleteAsset(folder);
            if (hadPreference) EditorPrefs.SetBool(preferenceKey, oldPreference);
            else EditorPrefs.DeleteKey(preferenceKey);
        }

        [Test]
        public void Catalog_IsCompleteImmediatelyAndKeptOnlyInMemory()
        {
            var catalog = Load();
            Assert.That(catalog.entries, Has.Count.EqualTo(1));
            Assert.That(catalog.entries[0].avatarName, Is.EqualTo("Original avatar"));
            Assert.That(catalog.entries[0].prefabsHashes, Is.EquivalentTo(new[] { "avatar-hash" }));
            Assert.That(EditorUtility.IsPersistent(catalog), Is.False);
            Assert.That(catalog.hideFlags, Is.EqualTo(HideFlags.HideAndDontSave));
            Assert.That(File.Exists(catalogPath), Is.False);
            for (var i = 0; i < 100; i++) Assert.That(Load(), Is.SameAs(catalog));
            Assert.That(Count("AssetSearchRequestCount"), Is.EqualTo(1));
            Assert.That(Count("AssetLoadRequestCount"), Is.EqualTo(1));
            Assert.That(Count("PresetLoadRequestCount"), Is.Zero);
            Invoke("ResetForTests");
            Assert.That(Load().entries[0].avatarName, Is.EqualTo("Original avatar"));
            Assert.That(Count("AssetSearchRequestCount"), Is.EqualTo(1));
            Assert.That(File.Exists(catalogPath), Is.False);
        }

        [Test]
        public void DefinitionInspectorEdits_AutomaticallyRefreshMetadataAndUndo()
        {
            var catalog = Load();
            var so = new SerializedObject(definitions);
            so.FindProperty("presetDefines").GetArrayElementAtIndex(0).FindPropertyRelative("avatarName").stringValue = "Renamed avatar";
            so.ApplyModifiedProperties();
            Assert.That(Load().entries[0].avatarName, Is.EqualTo("Renamed avatar"));
            Undo.FlushUndoRecordObjects();
            Undo.PerformUndo();
            Assert.That(Load().entries[0].avatarName, Is.EqualTo("Original avatar"));
            Undo.PerformRedo();
            Assert.That(Load().entries[0].avatarName, Is.EqualTo("Renamed avatar"));
            Assert.That(catalog.revision, Is.EqualTo(4));
        }

        [Test]
        public void AddedMovedDeletedDefinitions_RefreshWithoutManualReload()
        {
            Load();
            var extra = ScriptableObject.CreateInstance<PosingSystemPresetDefines>();
            extra.presetDefines.Add(new PosingSystemPresetDefines.PresetDefine { avatarName = "Extra" });
            AssetDatabase.CreateAsset(extra, folder + "/Extra.asset");
            Assert.That(Load().entries, Has.Count.EqualTo(2));
            Assert.That(AssetDatabase.MoveAsset(folder + "/Extra.asset", folder + "/Moved.asset"), Is.Empty);
            Assert.That(Load().entries.Any(e => e.sourcePath == folder + "/Moved.asset"), Is.True);
            AssetDatabase.DeleteAsset(folder + "/Moved.asset");
            Assert.That(Load().entries, Has.Count.EqualTo(1));
        }

        [Test]
        public void UnrelatedAnimationImport_DoesNotRescanPresets()
        {
            Load();
            AssetDatabase.CreateAsset(new AnimationClip(), folder + "/Unrelated.anim");
            AssetDatabase.ImportAsset(folder + "/Unrelated.anim", ImportAssetOptions.ForceUpdate);
            Load();
            Assert.That(Count("AssetSearchRequestCount"), Is.EqualTo(1));
        }

        [Test]
        public void LegacyCatalog_MigratesVisibilityWithoutChangingAvatarOrSavedFile()
        {
            var legacy = ScriptableObject.CreateInstance<PosingSystemPresetCatalog>();
            legacy.hideBundledPresets = true;
            legacy.entries.Add(new PosingSystemPresetCatalog.Entry { avatarName = "Outdated" });
            AssetDatabase.CreateAsset(legacy, catalogPath);
            AssetDatabase.SaveAssetIfDirty(legacy);
            var bytes = File.ReadAllBytes(catalogPath);
            target.presetCatalogObject = legacy;
            var targetJson = EditorJsonUtility.ToJson(target);
            EditorPrefs.DeleteKey(preferenceKey);
            var current = (PosingSystemPresetCatalog)Invoke("GetCatalog", target, false);
            Assert.That(current.hideBundledPresets, Is.True);
            Assert.That(current, Is.Not.SameAs(legacy));
            Assert.That(current.entries.Any(e => e.avatarName == "Outdated"), Is.False);
            CollectionAssert.AreEqual(bytes, File.ReadAllBytes(catalogPath));
            Assert.That(EditorJsonUtility.ToJson(target), Is.EqualTo(targetJson));
            var other = NewTool("Other tool");
            var dirty = EditorUtility.IsDirty(other);
            Assert.That(Invoke("GetCatalog", other, false), Is.SameAs(current));
            Assert.That(other.presetCatalogObject, Is.Null);
            Assert.That(EditorUtility.IsDirty(other), Is.EqualTo(dirty));
        }

        [Test]
        public void Visibility_SupportsUndoRedoAndSurvivesMemoryReset()
        {
            Load();
            Invoke("SetHideBundledPresets", true);
            Undo.FlushUndoRecordObjects();
            Assert.That(EditorPrefs.GetBool(preferenceKey), Is.True);
            Undo.PerformUndo();
            Assert.That(Load().hideBundledPresets, Is.False);
            Assert.That(EditorPrefs.GetBool(preferenceKey), Is.False);
            Undo.PerformRedo();
            Assert.That(Load().hideBundledPresets, Is.True);
            Invoke("ResetForTests");
            Assert.That(Load().hideBundledPresets, Is.True);
        }

        [Test]
        public void LazyReferences_SaveReloadInOriginalObjectReferenceFormat()
        {
            var yaml = File.ReadAllText(folder + "/Definitions.asset");
            Assert.That(yaml, Does.Contain("preset: {fileID:"));
            Assert.That(yaml, Does.Contain(AssetDatabase.AssetPathToGUID(folder + "/Pose.preset")));
            Resources.UnloadAsset(definitions);
            definitions = AssetDatabase.LoadAssetAtPath<PosingSystemPresetDefines>(folder + "/Definitions.asset");
            var so = new SerializedObject(definitions);
            var reference = so.FindProperty("presetDefines").GetArrayElementAtIndex(0).FindPropertyRelative("preset");
            Assert.That(reference.propertyType, Is.EqualTo(SerializedPropertyType.ObjectReference));
            Assert.That(reference.objectReferenceValue, Is.SameAs(preset));
            Assert.That(definitions.presetDefines[0].prefabs[0].asset, Is.SameAs(prefab));
            Assert.That(Invoke("ApplyPreset", target, Load().entries[0]), Is.True);
        }

        [Test]
        public void MetadataRead_DoesNotLoadReferencedPresetOrAnimation()
        {
            var clip = new AnimationClip { name = "LazyPresetClip_" + Guid.NewGuid().ToString("N") };
            var clipName = clip.name;
            AssetDatabase.CreateAsset(clip, folder + "/" + clipName + ".anim");
            var template = NewTool("Clip template");
            template.defines.Add(new PosingSystem.LayerDefine("", "", "Pose", 0, "Clips",
                new List<PosingSystem.AnimationDefine> { new PosingSystem.AnimationDefine { animationClip = clip } }));
            var lazyPreset = new Preset(template);
            AssetDatabase.CreateAsset(lazyPreset, folder + "/WithClip.preset");
            definitions.presetDefines[0].preset = lazyPreset;
            EditorUtility.SetDirty(definitions);
            AssetDatabase.SaveAssetIfDirty(definitions);
            Object.DestroyImmediate(template.gameObject);
            Resources.UnloadAsset(definitions);
            Resources.UnloadAsset(lazyPreset);
            Resources.UnloadAsset(clip);
            Assert.That(Resources.FindObjectsOfTypeAll<AnimationClip>().Any(c => c.name == clipName), Is.False);
            var entry = Load().entries[0];
            Assert.That(Resources.FindObjectsOfTypeAll<AnimationClip>().Any(c => c.name == clipName), Is.False);
            Assert.That(Resources.FindObjectsOfTypeAll<Preset>().Any(p => AssetDatabase.GetAssetPath(p) == folder + "/WithClip.preset"), Is.False);
            Assert.That(Invoke("ApplyPreset", target, entry), Is.True);
            Assert.That(target.defines[0].animations[0].animationClip.name, Is.EqualTo(clipName));
        }

        [Test]
        public void MovedPresetAndTool_AreResolvedByGuid()
        {
            var entry = Load().entries[0];
            Assert.That(AssetDatabase.MoveAsset(folder + "/Pose.preset", folder + "/Moved.preset"), Is.Empty);
            Assert.That(AssetDatabase.MoveAsset(folder + "/Tool.prefab", folder + "/RenamedTool.prefab"), Is.Empty);
            Assert.That(Invoke("ApplyPreset", target, entry), Is.True);
            var catalog = Load();
            var matches = (List<PosingSystemPresetCatalog.Entry>)Invoke("GetMatchingEntries", catalog, prefab, "");
            Assert.That(matches, Has.Count.EqualTo(1));
            Assert.That(catalog.entries[0].toolNames, Does.Contain("RenamedTool"));
        }

        [Test]
        public void MissingReferences_DoNotBreakMetadataList()
        {
            AssetDatabase.DeleteAsset(folder + "/Pose.preset");
            AssetDatabase.DeleteAsset(folder + "/Tool.prefab");
            var entries = Load().entries;
            Assert.That(entries, Has.Count.EqualTo(1));
            Assert.That(Invoke("ApplyPreset", target, entries[0]), Is.False);
        }

        [Test]
        public void CreatingInspector_DoesNotScanOrLoadPresets()
        {
            var editor = Editor.CreateEditor(target);
            try
            {
                Assert.That(Count("AssetSearchRequestCount"), Is.Zero);
                Assert.That(Count("AssetLoadRequestCount"), Is.Zero);
                Assert.That(target.presetCatalogObject, Is.Null);
            }
            finally { Object.DestroyImmediate(editor); }
        }

        [TestCase("Packages/jp.unisakistudio.kawaiiposing/Resources/Presets/Avatar.asset", true)]
        [TestCase("Packages/jp.unisakistudio.coolposing/Resources/Presets/Subfolder/Avatar.asset", true)]
        [TestCase("Assets/Resources/Presets/Avatar.asset", false)]
        [TestCase("Packages/com.example/Resources/Presets/Avatar.asset", false)]
        [TestCase("Packages/jp.unisakistudio.coolposing/Resources/PresetsOther/Avatar.asset", false)]
        [TestCase("Packages/jp.unisakistudio.coolposing/Other/Resources/Presets/Avatar.asset", false)]
        public void OfficialGroup_UsesStudioPackagePresetFolder(string path, bool expected)
        {
            Assert.That(Invoke("IsOfficialPath", path), Is.EqualTo(expected));
        }

        [Test]
        public void MatchingList_FiltersToolsAndSortsEachGroupWithUnnamedLast()
        {
            var catalog = Load();
            var original = catalog.entries[0];
            var alpha = Entry("alpha", false, prefab.name);
            var unnamed = Entry(null, false, prefab.name);
            var official = Entry("AAA official", true, prefab.name);
            var other = Entry("Other tool", false, "Different tool");
            catalog.entries.AddRange(new[] { official, unnamed, other, alpha });
            var matches = (List<PosingSystemPresetCatalog.Entry>)Invoke("GetMatchingEntries", catalog, prefab, "");
            CollectionAssert.AreEqual(new[] { alpha, original, unnamed, official }, matches);
            var byName = (List<PosingSystemPresetCatalog.Entry>)Invoke("GetMatchingEntries", catalog, null, prefab.name);
            CollectionAssert.AreEqual(matches, byName);
            Assert.That(Count("PresetLoadRequestCount"), Is.Zero);
        }

        [Test]
        public void ApplyingSelectedPreset_LoadsItOnDemandAndPreservesSettingsAndReference()
        {
            var catalog = Load();
            target.presetCatalogObject = catalog;
            target.settingName = "Keep this setting";
            target.isIconSmall = true;
            target.enableAvatarSpecificAdjustment = true;
            target.avatarSpecificAdjustmentUsageInitialized = false;
            Assert.That(Count("PresetLoadRequestCount"), Is.Zero);

            Assert.That(Invoke("ApplyPreset", target, catalog.entries[0]), Is.True);
            Assert.That(Count("PresetLoadRequestCount"), Is.EqualTo(1));
            Assert.That(target.defines[0].animations[0].displayName, Is.EqualTo("Saved pose"));
            Assert.That(target.overrideDefines, Has.Count.EqualTo(1));
            Assert.That(target.overrideDefines[0].enabled, Is.False);
            Assert.That(target.settingName, Is.EqualTo("Keep this setting"));
            Assert.That(target.isIconSmall, Is.True);
            Assert.That(target.enableAvatarSpecificAdjustment, Is.False);
            Assert.That(target.avatarSpecificAdjustmentUsageInitialized, Is.True);
            Assert.That(target.presetCatalogObject, Is.SameAs(catalog));
            Undo.FlushUndoRecordObjects();
            Undo.PerformUndo();
            Assert.That(target.enableAvatarSpecificAdjustment, Is.True);
            Assert.That(target.avatarSpecificAdjustmentUsageInitialized, Is.False);
            Assert.That(target.defines, Is.Empty);
            Assert.That(target.presetCatalogObject, Is.SameAs(catalog));
        }

        [Test]
        public void DeletedPreset_CannotBeAppliedAndLeavesTargetUnchanged()
        {
            var entry = Load().entries[0];
            AssetDatabase.DeleteAsset(folder + "/Pose.preset");
            Assert.That(Invoke("ApplyPreset", target, entry), Is.False);
            Assert.That(target.defines, Is.Empty);
        }

        [Test]
        public void AvatarMatches_RenamedPrefab_PrefersAllGuidMatchesOverNames()
        {
            var catalog = Load();
            var avatar = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            avatar.name = "Renamed avatar";
            var hash = PosingSystemMenuItems.GetPosingSystemGUIDHash(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab)));
            var byName = new PosingSystemPresetCatalog.Entry { prefabsNames = new List<string> { avatar.name } };
            var first = new PosingSystemPresetCatalog.Entry { prefabsHashes = new List<string> { hash } };
            var second = new PosingSystemPresetCatalog.Entry { prefabsHashes = new List<string> { hash } };
            var loads = Count("AssetLoadRequestCount");
            var matches = Invoke("GetAvatarMatchingEntries", new[] { byName, first, second }, avatar);
            CollectionAssert.AreEqual(new[] { first, second }, (System.Collections.IEnumerable)matches);
            Assert.That(Count("AssetLoadRequestCount"), Is.EqualTo(loads));
            Assert.That(Count("PresetLoadRequestCount"), Is.Zero);
        }

        [Test]
        public void AvatarMatches_Variant_MatchesItsOwnAndBasePrefabGuids()
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var variant = PrefabUtility.SaveAsPrefabAsset(instance, folder + "/Variant.prefab");
            Object.DestroyImmediate(instance);
            var avatar = (GameObject)PrefabUtility.InstantiatePrefab(variant, scene);
            avatar.name = "Customized avatar";
            var baseMatch = new PosingSystemPresetCatalog.Entry
            {
                prefabsHashes = new List<string> { PosingSystemMenuItems.GetPosingSystemGUIDHash(
                    AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab))) }
            };
            var variantMatch = new PosingSystemPresetCatalog.Entry
            {
                prefabsHashes = new List<string> { PosingSystemMenuItems.GetPosingSystemGUIDHash(
                    AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(variant))) }
            };
            var matches = Invoke("GetAvatarMatchingEntries", new[] { baseMatch, variantMatch }, avatar);
            CollectionAssert.AreEqual(new[] { baseMatch, variantMatch }, (System.Collections.IEnumerable)matches);
        }

        [Test]
        public void AvatarMatches_NameFallback_IgnoresEmptyGuidAndUnrelatedDisplayNames()
        {
            target.name = "Avatar";
            var unrelated = new PosingSystemPresetCatalog.Entry
            {
                avatarName = "Avatar",
                prefabsHashes = new List<string> { PosingSystemMenuItems.GetPosingSystemGUIDHash("") }
            };
            var byName = new PosingSystemPresetCatalog.Entry { prefabsNames = new List<string> { "Avatar" } };
            var entries = new[] { unrelated, byName };
            CollectionAssert.AreEqual(new[] { byName },
                (System.Collections.IEnumerable)Invoke("GetAvatarMatchingEntries", entries, target.gameObject));
            target.name = "Other avatar";
            Assert.That((System.Collections.IEnumerable)Invoke("GetAvatarMatchingEntries", entries, target.gameObject), Is.Empty);
            Assert.That((System.Collections.IEnumerable)Invoke("GetAvatarMatchingEntries", entries, null), Is.Empty);
        }

        [Test]
        public void AvatarMatchKey_ChangesWhenAvatarIsRenamedSwitchedOrUnpacked()
        {
            var avatar = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
            var original = Invoke("GetAvatarMatchKey", avatar);
            avatar.name = "Changed name";
            var renamed = Invoke("GetAvatarMatchKey", avatar);
            Assert.That(renamed, Is.Not.EqualTo(original));
            Assert.That(Invoke("GetAvatarMatchKey", target.gameObject), Is.Not.EqualTo(renamed));
            PrefabUtility.UnpackPrefabInstance(avatar, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            Assert.That(Invoke("GetAvatarMatchKey", avatar), Is.Not.EqualTo(renamed));
            Assert.That(Count("AssetSearchRequestCount"), Is.Zero);
        }

        [Test]
        public void AvatarGroup_AppearsFirstAndRespectsSearchAndBundledVisibility()
        {
            var keys = new[] { "PosingSystem.Presets.Avatar", "PosingSystem.Presets.User", "PosingSystem.Presets.Official" };
            var expansion = keys.Select(key => SessionState.GetBool(key, key != keys[2])).ToArray();
            try
            {
                var browserType = typeof(PosingSystemEditor).Assembly.GetType(
                    "jp.unisakistudio.posingsystemeditor.PosingSystemPresetBrowser");
                var type = browserType.GetNestedType("PresetTreeView", BindingFlags.NonPublic);
                PosingSystemPresetCatalog.Entry selected = null;
                Action<PosingSystemPresetCatalog.Entry> changed = entry => selected = entry;
                var tree = (UnityEditor.IMGUI.Controls.TreeView)Activator.CreateInstance(type,
                    BindingFlags.Instance | BindingFlags.NonPublic, null, new object[] { changed }, null);
                var user = Entry("User preset", false, prefab.name);
                var official = Entry("Official preset", true, prefab.name);
                type.GetMethod("SetEntries", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(tree, new object[] { new List<PosingSystemPresetCatalog.Entry> { user, official }, new[] { user, official } });
                tree.SetExpanded(new[] { 3, 1, 2 });
                var rows = tree.GetRows();
                Assert.That(rows[0].displayName, Is.EqualTo("アバターに適合 (2)"));
                CollectionAssert.AreEqual(new[] { "アバターに適合 (2)", "ユーザー作成 (1)", "ゆにさきスタジオ公式 (1)" },
                    rows.Where(row => row.depth == 0).Select(row => row.displayName));
                Assert.That(rows.Select(row => row.id).Distinct().Count(), Is.EqualTo(rows.Count));
                var recommended = rows.First(row => row.displayName == "Official preset（公式）");
                tree.SetSelection(new[] { recommended.id }, UnityEditor.IMGUI.Controls.TreeViewSelectionOptions.FireSelectionChanged);
                Assert.That(selected, Is.SameAs(official));

                var filter = type.GetMethod("SetFilter", BindingFlags.Instance | BindingFlags.NonPublic);
                filter.Invoke(tree, new object[] { "", true });
                Assert.That(selected, Is.Null);
                Assert.That(tree.GetRows()[0].displayName, Is.EqualTo("アバターに適合 (1)"));
                Assert.That(tree.GetRows().Any(row => row.displayName.Contains("Official") || row.id == 2), Is.False);
                Assert.That(type.GetProperty("MatchCount", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(tree), Is.EqualTo(1));

                filter.Invoke(tree, new object[] { "Official", true });
                Assert.That(tree.GetRows().Any(row => row.depth == 1), Is.False);
                filter.Invoke(tree, new object[] { "Official", false });
                Assert.That(tree.GetRows()[0].displayName, Is.EqualTo("アバターに適合 (1)"));
                Assert.That(tree.GetRows().Count(row => row.depth == 1), Is.EqualTo(2));
                Assert.That(Count("AssetSearchRequestCount"), Is.Zero);
            }
            finally
            {
                for (var i = 0; i < keys.Length; i++) SessionState.SetBool(keys[i], expansion[i]);
            }
        }

        private PosingSystem NewTool(string name)
        {
            var go = new GameObject(name);
            SceneManager.MoveGameObjectToScene(go, scene);
            return go.AddComponent<PosingSystem>();
        }

        private static PosingSystemPresetCatalog.Entry Entry(string name, bool official, string tool)
        {
            return new PosingSystemPresetCatalog.Entry
            {
                avatarName = name, official = official, toolNames = new List<string> { tool }
            };
        }

        private PosingSystemPresetCatalog Load(bool force = false)
        {
            var result = (PosingSystemPresetCatalog)Invoke("RequestCatalog", new[] { folder }, force);
            Assert.That(Error(), Is.Null.Or.Empty);
            Assert.That(result, Is.Not.Null);
            return result;
        }

        private string Error() => (string)cacheType.GetProperty("LastError", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);

        private object Invoke(string methodName, params object[] arguments)
        {
            var method = cacheType.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(method, methodName);
            return method.Invoke(null, arguments);
        }

        private int Count(string propertyName)
        {
            return (int)cacheType.GetProperty(propertyName, BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        }
    }
}
