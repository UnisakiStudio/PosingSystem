using System;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemMultilingualCompletenessTests : global::jp.unisakistudio.posingsystemeditor.tests.SharedLanguagePreferenceTestBase
    {
        private const string Root = "Packages/jp.unisakistudio.posingsystem/Editor/Localization/";

        [Test]
        public void EveryAdvertisedLanguageContainsTheFullDisplayInventory()
        {
            var english = PosingSystemLocalization.ParseCatalog(AssetDatabase.LoadAssetAtPath<TextAsset>(Root + "en.json").text);
            // Shared inventory only: each product keeps its own names in its own package.
            Assert.That(english.Count, Is.GreaterThanOrEqualTo(840));
            foreach (var language in PosingSystemLocalization.Codes.Where(c => c != "ja"))
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(Root + language + ".json");
                Assert.That(asset, Is.Not.Null, language);
                var entries = PosingSystemLocalization.ParseCatalog(asset.text);
                Assert.That(english.Keys.Except(entries.Keys), Is.Empty, language + " contains fallback-only text");
                foreach (var key in english.Keys)
                    Assert.That(PosingSystemLocalization.Translate(language, key, "MISSING"), Is.EqualTo(entries[key]), language + "/" + key);
            }
        }

        [Test]
        public void InstalledProductCatalogsAreRegisteredAndCoverEveryAdvertisedLanguage()
        {
            foreach (var path in new[]
            {
                "Packages/jp.unisakistudio.kawaiiposing/Editor/Localization/translations.json",
                "Packages/jp.unisakistudio.coolposing/Editor/Localization/translations.json",
            })
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                if (asset == null) continue; // That product is not installed in this project.
                var catalog = JsonUtility.FromJson<PosingSystemLocalization.ProductCatalog>(asset.text);
                var english = catalog.languages.Single(language => language.code == "en").entries.Select(entry => entry.key).ToArray();
                Assert.That(english, Is.Not.Empty, path);
                Assert.That(catalog.displayNames.Except(english), Is.Empty, path + " has display names without translations");
                Assert.That(PosingSystemLocalization.IsProductCatalog(path), Is.True, path + " is not registered by its product");
                foreach (var language in PosingSystemLocalization.Codes.Where(c => c != "ja"))
                    foreach (var key in english)
                        Assert.That(PosingSystemLocalization.Translate(language, key, "MISSING"), Is.Not.EqualTo("MISSING"), language + "/" + key);
                foreach (var name in catalog.displayNames)
                {
                    // Product names are display identities: translated by the allowlist path, literal in Japanese.
                    Assert.That(PosingSystemLocalization.TranslateDisplayName("ja", name), Is.EqualTo(name));
                    Assert.That(PosingSystemLocalization.TranslateDisplayName("en", name), Is.EqualTo(PosingSystemLocalization.Translate("en", name, "MISSING")), name);
                }
            }
        }

        [Test]
        public void MuscleAndBoneNamesUseTheSelectedLanguageWithoutChangingIndices()
        {
            foreach (var language in PosingSystemLocalization.Codes.Where(c => c != "ja"))
            {
                var entries = PosingSystemLocalization.ParseCatalog(AssetDatabase.LoadAssetAtPath<TextAsset>(Root + language + ".json").text);
                for (var i = 0; i < HumanTrait.MuscleCount; i++)
                    Assert.That(entries.ContainsKey("muscle." + i), Is.True, language + "/muscle." + i);
                Assert.That(entries.ContainsKey("bone.LeftUpperLeg"), Is.True, language);
                Assert.That(entries.ContainsKey("bone.RightUpperLeg"), Is.True, language);
            }
        }

        [Test]
        public void CachedBlockedObjectMessageChangesLanguageWithoutTranslatingItsPath()
        {
            const string template = "共通ポージングAnimatorを使用できません。「{0}」{1}。";
            const string path = "Custom / 対象";
            var root = new GameObject("Cached diagnostic language test");
            try
            {
                var system = root.AddComponent<PosingSystem>();
                PosingSystemLocalization.Select("en");
                string cached;
                using (PosingSystemLocalization.ForInspector(system))
                    cached = PosingSystemLocalization.Format(template, path, PosingSystemLocalization.Text("が無効になっています"));
                foreach (var language in new[] { "fr", "ja", "en" })
                {
                    PosingSystemLocalization.Select(language);
                    using (PosingSystemLocalization.ForInspector(system))
                    {
                        var expected = PosingSystemLocalization.Format(template, path, PosingSystemLocalization.Text("が無効になっています"));
                        cached = PosingSystemLocalization.TextMessage(cached);
                        Assert.That(cached, Is.EqualTo(expected), language);
                        Assert.That(cached, Does.Contain(path), language);
                    }
                }
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void MaintenanceDialogsAndDraftCanReturnToJapanese()
        {
            const string preference = "jp.unisakistudio.posingsystem.language";
            var root = new GameObject("Language round trip test");
            try
            {
                var system = root.AddComponent<PosingSystem>();
                PosingSystemLocalization.Select("en");
                using (PosingSystemLocalization.ForInspector(system))
                {
                    Assert.That(PosingSystemLocalization.Text("保存時から変わった項目を確認"), Is.EqualTo("Show settings changed since prebuild"));
                    Assert.That(PosingSystemSupportReport.Draft("[PS050]").StartsWith("The pose tool reported"), Is.True);
                    Assert.That(PosingSystemLocalization.Format("共通ポージングAnimatorを使用できません。「{0}」{1}。", "Custom / 対象", PosingSystemLocalization.Text("が無効になっています")),
                        Is.EqualTo("The common posing Animator is unavailable. \"Custom / 対象\" is disabled."));
                }
                PosingSystemLocalization.Select("ja");
                using (PosingSystemLocalization.ForInspector(system))
                {
                    Assert.That(PosingSystemLocalization.Text("保存時から変わった項目を確認"), Is.EqualTo("保存時から変わった項目を確認"));
                    Assert.That(PosingSystemSupportReport.Draft("[PS050]").StartsWith("ポーズツールで次のエラー"), Is.True);
                    Assert.That(PosingAnimationAdjustmentMuscle.GetMuscleNameJapanese(0), Is.EqualTo("腰 前後"));
                }
                Assert.That(EditorPrefs.GetString(preference, ""), Is.EqualTo("ja"));
            }
            finally { Object.DestroyImmediate(root); }
        }
    }
}
