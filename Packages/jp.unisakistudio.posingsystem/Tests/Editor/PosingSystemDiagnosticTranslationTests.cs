using NUnit.Framework;
using UnityEngine;
using jp.unisakistudio.posingsystem;
using jp.unisakistudio.posingsystemeditor;

namespace UnisakiStudio.PosingSystem.Tests
{
    public class PosingSystemDiagnosticTranslationTests : global::jp.unisakistudio.posingsystemeditor.tests.SharedLanguagePreferenceTestBase
    {
        [Test]
        public void DuplicateRemovalPluginTitleFollowsLanguageWithoutChangingItsIdentity()
        {
            var go = new GameObject("NDMF plugin display language scope");
            try
            {
                var ps = go.AddComponent<jp.unisakistudio.posingsystem.PosingSystem>();
                var plugin = new DuplicateEraserConverter();
                const string source = "ゆにさきポーズシステム・重複オブジェクト削除機能";
                foreach (var language in PosingSystemLocalization.Codes)
                {
                    PosingSystemLocalization.Select(language);
                    using (PosingSystemLocalization.ForInspector(ps))
                    {
                        Assert.That(plugin.DisplayName, Is.EqualTo(PosingSystemLocalization.Text(source) + PosingSystemPackageInfo.SystemVersion), language);
                        Assert.That(plugin.QualifiedName, Is.EqualTo("jp.unisakistudio.posingsytemeditor.duplicate-eraser"));
                        if (language != "ja") Assert.That(plugin.DisplayName, Does.Not.Contain(source), language);
                    }
                    PosingSystemLocalization.Select("ja");
                    using (PosingSystemLocalization.ForInspector(ps))
                        Assert.That(plugin.DisplayName, Does.StartWith(source), language + " -> ja");
                }
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void ReleasedRolloverInquiryGuidanceUsesEveryLanguageAndReturnsToJapanese()
        {
            const string privateName = "private-pose-name-not-for-the-inquiry";
            var go = new GameObject("released inquiry language scope");
            try
            {
                var ps = go.AddComponent<jp.unisakistudio.posingsystem.PosingSystem>();
                foreach (var language in PosingSystemLocalization.Codes)
                {
                    PosingSystemLocalization.Select(language);
                    using (PosingSystemLocalization.ForInspector(ps))
                    {
                        var guidance = PosingSystemLocalization.Text(PosingSystemDiagnostics.RolloverClipAction);
                        var draft = PosingSystemSupportReport.Draft("[PS070] " + privateName);
                        Assert.That(draft, Does.Contain(guidance), language);
                        Assert.That(draft, Does.Contain(PosingSystemLocalization.Text("寝返り")), language);
                        Assert.That(draft, Does.Not.Contain(privateName), language);
                        if (language != "ja")
                            Assert.That(draft, Does.Not.Contain(PosingSystemDiagnostics.RolloverClipAction), language);
                    }
                    Assert.That(PosingSystemLocalization.Language, Is.EqualTo(language));
                    PosingSystemLocalization.Select("ja");
                    using (PosingSystemLocalization.ForInspector(ps))
                        Assert.That(PosingSystemSupportReport.Draft("PS070"), Does.Contain(PosingSystemDiagnostics.RolloverClipAction), language + " -> ja");
                }
            }
            finally { Object.DestroyImmediate(go); }
        }

        [Test]
        public void DirectUtilityExceptionIsLocalizedBeforeLeavingItsTargetScope()
        {
            const string source = "Humanoid Animatorが必要です。";
            var go = new GameObject("direct utility exception scope");
            try
            {
                var ps = go.AddComponent<jp.unisakistudio.posingsystem.PosingSystem>();
                foreach (var language in PosingSystemLocalization.Codes)
                {
                    PosingSystemLocalization.Select(language);
                    using (PosingSystemLocalization.ForInspector(ps))
                    {
                        var exception = Assert.Throws<System.ArgumentException>(() => PosingSystemHeadSensorUtility.Create(null, null));
                        Assert.That(exception.Message, Is.EqualTo(PosingSystemLocalization.Translate(language, source, source)), language);
                    }
                    Assert.That(PosingSystemLocalization.Language, Is.EqualTo(language));
                }
            }
            finally { Object.DestroyImmediate(go); }
        }

        [TestCase("標準Controllerの元姿勢が見つかりません: {0}")]
        [TestCase("標準の揺れの無効化にはビルド用Controllerの複製が必要です: {0}")]
        [TestCase("生成ファイルをごみ箱へ移動できません: {0}")]
        public void DeferredDiagnosticUsesDisplayScopeAndPreservesCustomValueOnRoundTrip(string template)
        {
            // Exceptions are created before the display scope. Names, paths and braces are opaque.
            const string custom = "Assets/Original 対象/Custom {0} 😀.controller";
            var raw = string.Format(template, custom);
            var go = new GameObject("diagnostic language scope");
            try
            {
                var ps = go.AddComponent<jp.unisakistudio.posingsystem.PosingSystem>();
                foreach (var language in PosingSystemLocalization.Codes)
                {
                    PosingSystemLocalization.Select(language);
                    string displayed;
                    using (PosingSystemLocalization.ForInspector(ps))
                    {
                        displayed = PosingSystemLocalization.TextMessage(raw);
                        var expected = string.Format(PosingSystemLocalization.Translate(language, template, template), custom);
                        Assert.That(displayed, Is.EqualTo(expected), language);
                        Assert.That(displayed, Does.Contain(custom), language);
                        if (language != "ja") Assert.That(displayed, Is.Not.EqualTo(raw), language);
                    }
                    Assert.That(PosingSystemLocalization.Language, Is.EqualTo(language));
                    PosingSystemLocalization.Select("ja");
                    using (PosingSystemLocalization.ForInspector(ps))
                        Assert.That(PosingSystemLocalization.TextMessage(displayed), Is.EqualTo(raw), language + " -> ja");
                }
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
