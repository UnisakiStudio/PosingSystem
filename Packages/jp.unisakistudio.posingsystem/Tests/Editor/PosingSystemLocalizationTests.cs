using System;
using NUnit.Framework;
using UnityEditor;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemLocalizationTests
    {
        [Test]
        public void LanguageCatalogHasUniqueCodesAndMatchingNames()
        {
            Assert.That(PosingSystemLocalization.Codes.Length, Is.EqualTo(30));
            Assert.That(PosingSystemLocalization.Names.Length, Is.EqualTo(PosingSystemLocalization.Codes.Length));
            Assert.That(new System.Collections.Generic.HashSet<string>(PosingSystemLocalization.Codes).Count,
                Is.EqualTo(PosingSystemLocalization.Codes.Length));
        }
        [TestCase(null)]
        [TestCase("")]
        [TestCase("invalid")]
        public void InvalidLanguageFallsBackToJapanese(string code)
        {
            Assert.That(PosingSystemLocalization.Normalize(code), Is.EqualTo("ja"));
        }
        [Test]
        public void EverySelectableLanguageHasNavigationData()
        {
            foreach (var code in PosingSystemLocalization.Codes)
            {
                if (code == "ja") continue;
                Assert.That(PosingSystemLocalization.Translate(code, "manual", "missing"), Is.Not.EqualTo("missing"), code);
            }
        }
        [Test]
        public void SelectionAndFallbackDoNotDependOnUnityLanguage()
        {
            const string key = "jp.unisakistudio.posingsystem.language";
            var existed = EditorPrefs.HasKey(key);
            var previous = EditorPrefs.GetString(key);
            try
            {
                PosingSystemLocalization.Select("en");
                Assert.That(PosingSystemLocalization.Text("manual", "使い方"), Is.EqualTo("Manual"));
                Assert.That(PosingSystemLocalization.Text("missing", "元の表示"), Is.EqualTo("元の表示"));
                PosingSystemLocalization.Select("fr");
                Assert.That(PosingSystemLocalization.Language, Is.EqualTo("fr"));
                Assert.That(PosingSystemLocalization.Text("manual", "使い方"), Is.EqualTo("Guide"));
            }
            finally
            {
                if (existed) EditorPrefs.SetString(key, previous);
                else EditorPrefs.DeleteKey(key);
            }
        }
    }
}
