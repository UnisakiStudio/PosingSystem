using NUnit.Framework;
using UnisakiStudio.Setup;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemSetupLanguageTests
    {
        [TestCase("en-US", "Japanese", "ja")]
        [TestCase("zh-TW", "Chinese", "zh-Hant")]
        [TestCase("zh-HK", "Chinese", "zh-Hant")]
        [TestCase("zh-CN", "ChineseSimplified", "zh-Hans")]
        [TestCase("en-US", "ChineseTraditional", "zh-Hant")]
        [TestCase("pt-BR", "Portuguese", "pt-BR")]
        [TestCase("pt-PT", "Portuguese", "pt")]
        [TestCase("nn-NO", "Norwegian", "nb")]
        [TestCase("ta-IN", "Unknown", "ta")]
        [TestCase("bn-BD", "Unknown", "bn")]
        [TestCase("te-IN", "Unknown", "te")]
        [TestCase("fr-FR", "French", "fr")]
        [TestCase("", "Unknown", "en")]
        [TestCase("xx-XX", "Unknown", "en")]
        public void OperatingSystemLanguageMapping(string culture, string system, string expected)
            => Assert.That(Language.Detect(culture, system), Is.EqualTo(expected));

        [Test]
        public void UserChoiceWinsAcrossDifferentProjectLanguages()
        {
            Assert.That(Language.Resolve("fr", "ja-JP", "Japanese"), Is.EqualTo("fr"));
            Assert.That(Language.Resolve("fr", "en-US", "English"), Is.EqualTo("fr"));
            Assert.That(Language.Resolve("", "ja-JP", "Japanese"), Is.EqualTo("ja"));
            Assert.That(Language.Resolve("invalid", "de-DE", "German"), Is.EqualTo("de"));
            Assert.That(Language.PreferenceKey, Is.EqualTo("jp.unisakistudio.posingsystem.language"));
        }
        [Test]
        public void BootstrapMessagesWorkWithoutAnyProductOrLicenseState()
        {
            Assert.That(Language.Translate("ja", "License installation"), Is.EqualTo("ライセンスインストール"));
            Assert.That(Language.Translate("ja", "All Packages are Installed!"), Is.EqualTo("必要なパッケージはすべて導入済みです。"));
            Assert.That(Language.Translate("en", "キャンセル"), Is.EqualTo("Cancel"));
            Assert.That(Language.Translate("fr", "Cancel"), Is.EqualTo("Annuler"));
            Assert.That(Language.Translate("ja", "https://example.com/package.json"), Is.EqualTo("https://example.com/package.json"));
        }
        [Test]
        public void SetupLanguageCanChangeWhileKeepingMetadataUntouched()
        {
            Assert.That(Language.Translate("en", "保存先: HKEY_CURRENT_USER"), Is.EqualTo("Storage: HKEY_CURRENT_USER"));
            Assert.That(Language.Translate("fr", "KawaiiPosing: Enabled"), Is.EqualTo("KawaiiPosing: Activé"));
            Assert.That(Language.Translate("en", "対象: 有効"), Is.EqualTo("Target: 有効"));
            Assert.That(Language.Translate("ja", "Version Unity requise : 2022.3+"), Is.EqualTo("必要なUnityバージョン: 2022.3+"));
            Assert.That(Language.Translate("fr", "Prebuild now?\nPrebuilding makes avatar builds and uploads faster."), Is.EqualTo("Lancer la précompilation ?"));
        }


        [Test]
        public void NativeMenuRegistrationUsesCurrentLanguageWithoutRunningCommands()
        {
            UnisakiStudio.Setup.LocalizedMenus.Rebuild();
            var path = PosingSystemLocalizedMenus.PathFor("GameObject/ゆにさきスタジオ/可愛いポーズツール追加", Language.Current);
            var exists = typeof(UnityEditor.Menu).GetMethod("MenuItemExists", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
            Assert.That(exists, Is.Not.Null, "Unity 2022.3 dynamic menu API");
            Assert.That((bool)exists.Invoke(null, new object[] { path }), Is.True, path);
        }

        [Test]
        public void EditorMenuPathPreservesUnityRootAndLocalizesStudioCommands()
        {
            const string source = "GameObject/ゆにさきスタジオ/可愛いポーズツール追加";
            Assert.That(PosingSystemLocalizedMenus.PathFor(source, "en"), Is.EqualTo("GameObject/Unisaki Studio/Add Kawaii Posing"));
            Assert.That(PosingSystemLocalizedMenus.PathFor(source, "ja"), Is.EqualTo(source));
        }
    }
}
