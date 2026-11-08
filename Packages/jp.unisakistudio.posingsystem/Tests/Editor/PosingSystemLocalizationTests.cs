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
        public void OneSelectionControlsInspectorAndMenuWithoutChangingOtherAvatars()
        {
            const string key = "jp.unisakistudio.posingsystem.language";
            var existed = EditorPrefs.HasKey(key); var previous = EditorPrefs.GetString(key);
            var first = new UnityEngine.GameObject("first"); var second = new UnityEngine.GameObject("second");
            try
            {
                var ps = first.AddComponent<jp.unisakistudio.posingsystem.PosingSystem>();
                var other = second.AddComponent<jp.unisakistudio.posingsystem.PosingSystem>(); other.menuLanguage = "fr";
                PosingSystemLocalization.Select("en");
                using (PosingSystemLocalization.ForInspector(ps))
                {
                    Assert.That(PosingSystemLocalization.Language, Is.EqualTo("en"));
                    Assert.That(PosingSystemLocalization.ApplyLanguageSelection(0, ps), Is.True);
                    Assert.That(ps.menuLanguage, Is.EqualTo("ja"));
                    Assert.That(PosingSystemLocalization.Language, Is.EqualTo("ja"));
                    Assert.That(PosingSystemLocalization.ResolveLanguage(ps), Is.EqualTo("ja"));
                    Assert.That(PosingSystemLocalization.Text("機能"), Is.EqualTo("機能"));
                }
                PosingSystemLocalization.Select("en");
                using (PosingSystemLocalization.ForInspector(ps)) Assert.That(PosingSystemLocalization.Language, Is.EqualTo("ja"));
                using (PosingSystemLocalization.ForInspector(other)) Assert.That(PosingSystemLocalization.Language, Is.EqualTo("fr"));
                Assert.That(PosingSystemLocalization.Language, Is.EqualTo("en"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(first); UnityEngine.Object.DestroyImmediate(second);
                if (existed) EditorPrefs.SetString(key, previous); else EditorPrefs.DeleteKey(key);
            }
        }

        [Test]
        public void PopupSelectionCanReturnToJapaneseAtIndexZero()
        {
            const string key = "jp.unisakistudio.posingsystem.language";
            var existed = EditorPrefs.HasKey(key); var previous = EditorPrefs.GetString(key);
            try
            {
                PosingSystemLocalization.Select("en");
                Assert.That(PosingSystemLocalization.ApplyLanguageSelection(0), Is.True);
                Assert.That(EditorPrefs.GetString(key), Is.EqualTo("ja"));
                Assert.That(PosingSystemLocalization.Text("機能"), Is.EqualTo("機能"));
                Assert.That(PosingSystemLocalization.ApplyLanguageSelection(1), Is.True);
                Assert.That(PosingSystemLocalization.Text("機能"), Is.EqualTo("Features"));
                Assert.That(PosingSystemLocalization.ApplyLanguageSelection(0), Is.True);
                Assert.That(PosingSystemLocalization.Text("アバター固有補正を使う"), Is.EqualTo("アバター固有補正を使う"));
                Assert.That(PosingSystemLocalization.ApplyLanguageSelection(0), Is.False);
                Assert.That(PosingSystemLocalization.ApplyLanguageSelection(-1), Is.False);
                Assert.That(EditorPrefs.GetString(key), Is.EqualTo("ja"));
            }
            finally { if (existed) EditorPrefs.SetString(key, previous); else EditorPrefs.DeleteKey(key); }
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
        public void TargetFormattedFailurePreservesNamesAndRestoresOuterLanguage()
        {
            const string preference = "jp.unisakistudio.posingsystem.language";
            const string template = "「{0}」オブジェクトにMAParametersコンポーネントがありません";
            const string customName = "Custom / 対象 {0} 😀";
            var existed = EditorPrefs.HasKey(preference); var previous = EditorPrefs.GetString(preference);
            var go = new UnityEngine.GameObject(customName);
            try
            {
                var ps = go.AddComponent<jp.unisakistudio.posingsystem.PosingSystem>();
                PosingSystemLocalization.Select("en");
                ps.menuLanguage = "fr";
                var french = PosingSystemLocalization.FormatFor(ps, template, customName);
                Assert.That(french, Is.EqualTo(string.Format(PosingSystemLocalization.Translate("fr", template, template), customName)));
                Assert.That(french, Does.Contain(customName));
                Assert.That(french, Does.Not.Contain("コンポーネントがありません"));
                Assert.That(PosingSystemLocalization.Language, Is.EqualTo("en"), "target scope must not leak into other tools");
                ps.menuLanguage = "ja";
                Assert.That(PosingSystemLocalization.FormatFor(ps, template, customName), Is.EqualTo(string.Format(template, customName)));
                Assert.That(PosingSystemLocalization.Language, Is.EqualTo("en"));
                Assert.That(go.name, Is.EqualTo(customName));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                if (existed) EditorPrefs.SetString(preference, previous); else EditorPrefs.DeleteKey(preference);
            }
        }

        [Test]
        public void FailureTranslatesFormattedSummaryAndBothRemedyParagraphs()
        {
            const string preference = "jp.unisakistudio.posingsystem.language";
            const string template = "ポーズ「{0} / {1}」の変換に失敗しました。";
            const string firstRemedy = "ConsoleにコンパイルエラーやMissing Scriptがないか確認してください。";
            const string customCategory = "Original / 対象";
            const string customPose = "My Pose {1}";
            var existed = EditorPrefs.HasKey(preference); var previous = EditorPrefs.GetString(preference);
            var go = new UnityEngine.GameObject("failure target");
            try
            {
                var ps = go.AddComponent<jp.unisakistudio.posingsystem.PosingSystem>();
                PosingSystemLocalization.Select("en");
                foreach (var language in new[] { "fr", "ja", "en" })
                {
                    ps.menuLanguage = language;
                    var exception = PosingSystemDiagnostics.Failure("PS035", string.Format(template, customCategory, customPose),
                        firstRemedy + "\n" + PosingSystemDiagnostics.RestoreTool, ps);
                    var expected = string.Format(PosingSystemLocalization.Translate(language, template, template), customCategory, customPose);
                    Assert.That(exception.Message, Does.Contain(expected), language);
                    Assert.That(exception.Message, Does.Contain(customCategory).And.Contain(customPose), language);
                    Assert.That(exception.Message, Does.Contain(PosingSystemLocalization.Translate(language, firstRemedy, firstRemedy)), language);
                    Assert.That(exception.Message, Does.Contain(PosingSystemLocalization.Translate(language, PosingSystemDiagnostics.RestoreTool, PosingSystemDiagnostics.RestoreTool)), language);
                    if (language != "ja") Assert.That(exception.Message, Does.Not.Contain("変換に失敗しました").And.Not.Contain("ないか確認してください"), language);
                    Assert.That(PosingSystemLocalization.Language, Is.EqualTo("en"));
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                if (existed) EditorPrefs.SetString(preference, previous); else EditorPrefs.DeleteKey(preference);
            }
        }

        [Test]
        public void ContextlessFailureInheritsTheEnclosingTargetLanguage()
        {
            const string preference = "jp.unisakistudio.posingsystem.language";
            const string summary = "ツールに同梱される空アニメーションが見つかりません。";
            var existed = EditorPrefs.HasKey(preference); var previous = EditorPrefs.GetString(preference);
            var go = new UnityEngine.GameObject("target scope");
            try
            {
                var ps = go.AddComponent<jp.unisakistudio.posingsystem.PosingSystem>();
                ps.menuLanguage = "fr";
                PosingSystemLocalization.Select("en");
                using (PosingSystemLocalization.ForInspector(ps))
                {
                    var failure = PosingSystemDiagnostics.Failure("PS031", summary, PosingSystemDiagnostics.RestoreTool, null);
                    Assert.That(failure.Message, Does.Contain(PosingSystemLocalization.Translate("fr", summary, summary)));
                    Assert.That(PosingSystemLocalization.Language, Is.EqualTo("fr"));
                }
                Assert.That(PosingSystemLocalization.Language, Is.EqualTo("en"));
                using (PosingSystemLocalization.ForInspector(null)) Assert.That(PosingSystemLocalization.Language, Is.EqualTo("en"));
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
                if (existed) EditorPrefs.SetString(preference, previous); else EditorPrefs.DeleteKey(preference);
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
