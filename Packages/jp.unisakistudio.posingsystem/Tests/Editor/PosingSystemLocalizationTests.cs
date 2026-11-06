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
        [TestCase(0, 0, "No detectable configuration errors")]
        [TestCase(0, 1, "1 warning")]
        [TestCase(0, 3, "3 warnings")]
        [TestCase(1, 3, "1 error")]
        [TestCase(2, 3, "2 errors")]
        public void DiagnosticSummaryUsesEnglishCounts(int errors, int warnings, string expected)
        {
            const string key = "jp.unisakistudio.posingsystem.language";
            var existed = EditorPrefs.HasKey(key); var previous = EditorPrefs.GetString(key);
            try
            {
                PosingSystemLocalization.Select("en");
                Assert.That(PosingSystemDiagnosticsInspector.Summary(errors, warnings), Is.EqualTo(expected));
            }
            finally { if (existed) EditorPrefs.SetString(key, previous); else EditorPrefs.DeleteKey(key); }
        }

        [TestCase("可愛いポーズツール設定")]
        [TestCase("アバター固有補正を使う")]
        [TestCase("膝が逆向きに曲がるのを防ぐ")]
        [TestCase("アバターの元の立ち・しゃがみ・伏せ姿勢を使う")]
        [TestCase("各固定機能の他ギミックとの統合")]
        [TestCase("▶ 設定を開く")]
        [TestCase("▶ 取り込み設定を開く")]
        [TestCase("警告の表示設定")]
        [TestCase("設定の更新あり")]
        [TestCase("ほかのプリセットを探す")]
        [TestCase("しなの（着せ替え）")]
        [TestCase("このプリセットに変更する")]
        [TestCase("プリセットの保存・管理")]
        public void ReportedInspectorLabelsHaveEnglishTranslations(string source)
        {
            Assert.That(PosingSystemLocalization.Translate("en", source, source), Is.Not.EqualTo(source));
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
