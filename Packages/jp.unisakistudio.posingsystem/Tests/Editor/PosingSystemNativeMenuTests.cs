using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnisakiStudio.Setup;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public sealed class PosingSystemNativeMenuTests
    {
        private const string SessionKey = "jp.unisakistudio.posingsystem.localized-menu-paths";
        private const BindingFlags Flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static MethodInfo Native(string name) => typeof(Menu).GetMethods(Flags).Single(m => m.Name == name);
        private static string[] Tools() => (string[])Native("ExtractSubmenus").Invoke(null, new object[] { "Tools" });
        private static void Add(string path, Action callback) => Native("AddMenuItem").Invoke(null,
            new object[] { path, "", false, 200, callback, (Func<bool>)(() => true) });
        private static void RemoveIfPresent(string path)
        {
            if (Tools().Contains(path)) Native("RemoveMenuItem").Invoke(null, new object[] { path });
        }

        [Test]
        public void CompletedLicenseCaptionsAreSingleLineInEveryLanguage()
        {
            foreach (var language in Language.Codes)
                foreach (var product in new[] { "Kawaii Posing", "Cool Posing" })
                    foreach (var caption in new[] { "license status", "license removal" })
                    {
                        var value = Language.Translate(language, product + " " + caption);
                        Assert.That(value, Does.Not.Contain("\n").And.Not.Contain("\r"), language + "/" + caption);
                        Assert.That(value, Is.EqualTo(value.Trim()), language + "/" + caption);
                    }
        }

        [Test]
        public void RebuildRemovesLegacyMultilinePathsEvenWhenSavedListWasSplit()
        {
            const string source = "Tools/ゆにさきスタジオ/可愛いポーズツールライセンス削除";
            var paths = Language.Codes.Where(code => code != "ja")
                .Select(code => "Tools/Unisaki Studio/" + Language.Translate(code, "Remove licenses") + "\n" + Language.Translate(code, "Kawaii Posing"))
                .Distinct().ToArray();
            var executions = 0;
            try
            {
                LocalizedMenus.Rebuild("ja");
                foreach (var path in paths) Add(path, () => executions++);
                SessionState.SetString(SessionKey, string.Join("\n", paths)); // old broken storage
                LocalizedMenus.Rebuild("en");
                var actual = Tools();
                foreach (var path in paths) Assert.That(actual, Does.Not.Contain(path), path);
                Assert.That(actual, Does.Contain(LocalizedMenus.PathFor(source, "en")));
                Assert.That(SessionState.GetString(SessionKey, ""), Does.StartWith("{"), "paths use JSON rather than newline delimiters");
                Assert.That(executions, Is.Zero, "menu registration/cleanup must never run a license command");
            }
            finally
            {
                foreach (var path in paths) RemoveIfPresent(path);
                LocalizedMenus.Rebuild();
            }
        }

        [Test]
        public void InventoryCleanupHandlesTrailingWhitespacePaths()
        {
            const string path = "Tools/Unisaki Studio/Kawaii Posing license removal ";
            var executions = 0;
            try
            {
                Add(path, () => executions++);
                Assert.That(Tools(), Does.Contain(path));
                SessionState.SetString(SessionKey, path);
                LocalizedMenus.Rebuild("en");
                Assert.That(Tools(), Does.Not.Contain(path));
                Assert.That(executions, Is.Zero);
            }
            finally { RemoveIfPresent(path); LocalizedMenus.Rebuild(); }
        }

        [Test]
        public void ThirtyLanguageSwitchesDoNotAccumulateNativeCommands()
        {
            var sources = new[] {
                "Tools/ゆにさきスタジオ/可愛いポーズツールライセンス削除",
                "Tools/ゆにさきスタジオ/カッコいいポーズツールライセンス削除",
                "Tools/ゆにさきスタジオ/ゆにさきポーズシステムライセンス削除",
                "Tools/ゆにさきスタジオ/Consoleのエラーについて相談する"
            };
            var known = new HashSet<string>(sources.SelectMany(source => Language.Codes.Select(code => LocalizedMenus.PathFor(source, code))));
            try
            {
                foreach (var code in Language.Codes.Concat(new[] { "en", "ja", "en" }))
                {
                    LocalizedMenus.Rebuild(code);
                    var actual = Tools();
                    foreach (var source in sources)
                        Assert.That(actual, Does.Contain(LocalizedMenus.PathFor(source, code)), code + "/" + source);
                    var own = actual.Where(known.Contains).ToArray();
                    Assert.That(own.Length, Is.EqualTo(sources.Length), code + ": old-language registrations must be removed");
                    foreach (var path in own) Assert.That(path, Does.Not.Contain("\n").And.Not.Contain("\r"));
                }
            }
            finally { LocalizedMenus.Rebuild(); }
        }

        [Test]
        public void NativePathsNormalizeWhitespaceAndConsoleHelpIsLocalized()
        {
            const string source = "Tools/ゆにさきスタジオ/Consoleのエラーについて相談する";
            foreach (var code in Language.Codes)
            {
                var path = LocalizedMenus.PathFor(source, code);
                Assert.That(path, Does.Contain("Console"), code);
                Assert.That(path, Does.Not.Contain("\n").And.Not.Contain("\r"));
                if (code != "ja") Assert.That(path, Does.Not.Contain("相談する"), code);
            }
            Assert.That(LocalizedMenus.PathFor("Tools/Unisaki Studio/  A\n\tB  ", "en"), Is.EqualTo("Tools/Unisaki Studio/A B"));
        }
    }
}
