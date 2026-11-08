using NUnit.Framework;
using UnityEngine;
using jp.unisakistudio.posingsystem;
using jp.unisakistudio.posingsystemeditor;

namespace UnisakiStudio.PosingSystem.Tests
{
    public class PosingSystemDiagnosticTranslationTests
    {
        [Test]
        public void DirectUtilityExceptionIsLocalizedBeforeLeavingItsTargetScope()
        {
            const string source = "Humanoid Animatorが必要です。";
            var go = new GameObject("direct utility exception scope");
            try
            {
                var ps = go.AddComponent<jp.unisakistudio.posingsystem.PosingSystem>();
                var outerLanguage = PosingSystemLocalization.Language;
                foreach (var language in PosingSystemLocalization.Codes)
                {
                    ps.menuLanguage = language;
                    using (PosingSystemLocalization.ForInspector(ps))
                    {
                        var exception = Assert.Throws<System.ArgumentException>(() => PosingSystemHeadSensorUtility.Create(null, null));
                        Assert.That(exception.Message, Is.EqualTo(PosingSystemLocalization.Translate(language, source, source)), language);
                    }
                    Assert.That(PosingSystemLocalization.Language, Is.EqualTo(outerLanguage));
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
                var outerLanguage = PosingSystemLocalization.Language;
                foreach (var language in PosingSystemLocalization.Codes)
                {
                    ps.menuLanguage = language;
                    string displayed;
                    using (PosingSystemLocalization.ForInspector(ps))
                    {
                        displayed = PosingSystemLocalization.TextMessage(raw);
                        var expected = string.Format(PosingSystemLocalization.Translate(language, template, template), custom);
                        Assert.That(displayed, Is.EqualTo(expected), language);
                        Assert.That(displayed, Does.Contain(custom), language);
                        if (language != "ja") Assert.That(displayed, Is.Not.EqualTo(raw), language);
                    }
                    Assert.That(PosingSystemLocalization.Language, Is.EqualTo(outerLanguage));
                    ps.menuLanguage = "ja";
                    using (PosingSystemLocalization.ForInspector(ps))
                        Assert.That(PosingSystemLocalization.TextMessage(displayed), Is.EqualTo(raw), language + " -> ja");
                }
            }
            finally { Object.DestroyImmediate(go); }
        }
    }
}
