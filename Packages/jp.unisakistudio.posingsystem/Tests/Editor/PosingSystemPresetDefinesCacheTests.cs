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
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemPresetDefinesCacheTests
    {
        private Type cacheType;
        private string folder;
        private string catalogPath;
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
                prefabs = new List<GameObject> { prefab },
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
        }

        [Test]
        public void ReopeningCatalog_UsesSavedMetadataWithoutSearchingOrLoadingDefinitions()
        {
            var first = Load();
            Assert.That(first.entries, Has.Count.EqualTo(1));
            Assert.That(first.entries[0].avatarName, Is.EqualTo("Original avatar"));
            Assert.That(first.entries[0].prefabsHashes, Is.EquivalentTo(new[] { "avatar-hash" }));
            Assert.That(Count("AssetLoadRequestCount"), Is.EqualTo(1));
            var saved = File.ReadAllBytes(catalogPath);
            Assert.That(Load(), Is.SameAs(first));
            Assert.That(Count("AssetSearchRequestCount"), Is.EqualTo(1));

            Invoke("ResetForTests");
            Resources.UnloadAsset(first);
            var reopened = Load();
            Assert.That(reopened.entries[0].avatarName, Is.EqualTo("Original avatar"));
            Assert.That(Count("AssetSearchRequestCount"), Is.Zero);
            Assert.That(Count("AssetLoadRequestCount"), Is.Zero);
            Assert.That(Count("PresetLoadRequestCount"), Is.Zero);
            CollectionAssert.AreEqual(saved, File.ReadAllBytes(catalogPath));
            Assert.That(AssetDatabase.GetDependencies(catalogPath), Does.Not.Contain(folder + "/Pose.preset"));
            Assert.That(AssetDatabase.GetDependencies(catalogPath), Does.Not.Contain(folder + "/Tool.prefab"));
        }

        [Test]
        public void EditingAndAddingDefinitions_OnlyChangesListAfterExplicitReload()
        {
            var catalog = Load();
            definitions.presetDefines[0].avatarName = "Renamed avatar";
            EditorUtility.SetDirty(definitions);
            AssetDatabase.SaveAssetIfDirty(definitions);
            var extra = ScriptableObject.CreateInstance<PosingSystemPresetDefines>();
            extra.presetDefines.Add(new PosingSystemPresetDefines.PresetDefine { avatarName = "Added avatar" });
            AssetDatabase.CreateAsset(extra, folder + "/Extra.asset");
            Invoke("ResetForTests");

            Assert.That(Load().entries.Select(e => e.avatarName), Is.EquivalentTo(new[] { "Original avatar" }));
            Assert.That(Count("AssetLoadRequestCount"), Is.Zero);
            Assert.That(Load(force: true).entries.Select(e => e.avatarName),
                Is.EquivalentTo(new[] { "Renamed avatar", "Added avatar" }));
            Assert.That(catalog.revision, Is.EqualTo(2));
            Assert.That(Count("AssetSearchRequestCount"), Is.EqualTo(1));
        }

        [Test]
        public void DeletedDefinition_RemainsCachedUntilExplicitReload()
        {
            Load();
            AssetDatabase.DeleteAsset(folder + "/Definitions.asset");
            Assert.That(Load().entries, Has.Count.EqualTo(1));
            Assert.That(Load(force: true).entries, Is.Empty);
        }

        [Test]
        public void ChangedToolVersion_RebuildsOnceAndPersistsNewVersion()
        {
            Load("1.0");
            definitions.presetDefines[0].avatarName = "Updated avatar";
            var updated = Load("2.0");
            Assert.That(updated.entries[0].avatarName, Is.EqualTo("Updated avatar"));
            Assert.That(updated.revision, Is.EqualTo(2));
            Assert.That(updated.packageVersions, Is.EqualTo("2.0"));
            Assert.That(Load("2.0"), Is.SameAs(updated));
            Assert.That(Count("AssetSearchRequestCount"), Is.EqualTo(2));
            Invoke("ResetForTests");
            Assert.That(Load("2.0").revision, Is.EqualTo(2));
            Assert.That(Count("AssetLoadRequestCount"), Is.Zero);
        }

        [Test]
        public void DifferentAvatars_ShareCatalogEvenAfterMovingFileAndResettingMemory()
        {
            var version = (string)Invoke("GetVersionSignature");
            var catalog = Load(version);
            target.presetCatalogObject = catalog;
            var moved = folder + "/MovedCatalog.asset";
            Assert.That(AssetDatabase.MoveAsset(catalogPath, moved), Is.Empty);
            Invoke("ResetForTests");
            var first = Invoke("GetCatalog", target, false);
            var other = NewTool("Another avatar");
            var second = Invoke("GetCatalog", other, false);
            Assert.That(first, Is.SameAs(catalog));
            Assert.That(second, Is.SameAs(catalog));
            Assert.That(other.presetCatalogObject, Is.SameAs(target.presetCatalogObject));
            Assert.That(Count("AssetSearchRequestCount"), Is.Zero);
            Assert.That(Count("AssetLoadRequestCount"), Is.Zero);
            Assert.That(AssetDatabase.GetAssetPath(other.presetCatalogObject), Is.EqualTo(moved));
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

        private PosingSystemPresetCatalog Load(string version = "1.0", bool force = false)
        {
            return (PosingSystemPresetCatalog)Invoke("LoadCatalog", catalogPath, version, force, new[] { folder }, false);
        }

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