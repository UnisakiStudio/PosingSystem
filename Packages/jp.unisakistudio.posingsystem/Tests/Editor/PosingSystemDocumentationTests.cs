using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemDocumentationTests : global::jp.unisakistudio.posingsystemeditor.tests.SharedLanguagePreferenceTestBase
    {
        [Test]
        public void ManualAndChangelogFollowEverySelectedLanguage()
        {
            WithTemporaryPackage((root, info) =>
            {
                foreach (var language in PosingSystemLocalization.Codes)
                    WriteDocumentation(root, language, "## [3.2.0] - " + language);
                foreach (var language in PosingSystemLocalization.Codes)
                    using (new DocumentationLanguage(language))
                    {
                        Assert.That(new Uri(info.ManualUrl).LocalPath,
                            Is.EqualTo(Path.Combine(root, "Documentation~", "Localization", language, "UserGuide.html")), language);
                        Assert.That(PosingSystemPackageInfo.GetChangelog(info), Does.EndWith(language), language);
                    }
            });
        }

        [Test]
        public void ReusingPackageInfoDoesNotLeakChangelogLanguageAndReturnsToJapanese()
        {
            WithTemporaryPackage((root, info) =>
            {
                WriteDocumentation(root, "ja", "# 変更履歴\n\n## [3.2.0] - 未リリース\n\n日本語の変更");
                WriteDocumentation(root, "fr", "# Historique\n\n## [3.2.0] - Non publié\n\nModification française");
                foreach (var language in new[] { "ja", "fr", "ja", "fr", "ja" })
                    using (new DocumentationLanguage(language))
                    {
                        var text = PosingSystemPackageInfo.GetChangelog(info);
                        Assert.That(text, Does.StartWith("## [3.2.0]"));
                        Assert.That(text, Does.Contain(language == "ja" ? "日本語の変更" : "Modification française"));
                        Assert.That(text, Does.Not.Contain(language == "ja" ? "Modification française" : "日本語の変更"));
                    }
            });
        }

        [Test]
        public void PartialOlderInstallFallsBackToEnglishAndLegacyJapaneseHistory()
        {
            WithTemporaryPackage((root, info) =>
            {
                WriteDocumentation(root, "en", "## [3.2.0] - Unreleased");
                using (new DocumentationLanguage("th"))
                {
                    Assert.That(new Uri(info.ManualUrl).LocalPath, Does.Contain(Path.Combine("en", "UserGuide.html")));
                    Assert.That(PosingSystemPackageInfo.GetChangelog(info), Does.Contain("Unreleased"));
                }
                File.WriteAllText(Path.Combine(root, "CHANGELOG.md"), "# 変更履歴\n\n## [3.1.2]\n\n旧版");
                Directory.Delete(Path.Combine(root, "Documentation~"), true);
                // An uncached selection models an older package without localized files.
                using (new DocumentationLanguage("ja"))
                    Assert.That(PosingSystemPackageInfo.GetChangelog(info), Does.Contain("旧版"));
            });
        }

        [Test]
        public void MissingDocumentationDoesNotBecomeAPermanentCachedFailure()
        {
            WithTemporaryPackage((root, info) =>
            {
                using (new DocumentationLanguage("fr"))
                {
                    Assert.That(PosingSystemPackageInfo.GetChangelog(info), Is.Not.Empty);
                    WriteDocumentation(root, "fr", "## [3.2.0] - Non publié");
                    Assert.That(PosingSystemPackageInfo.GetChangelog(info), Does.Contain("Non publié"));
                }
            });
        }

        [Test]
        public void InstalledCoreIncludesCompleteDocsForEveryAdvertisedLanguage()
        {
            var info = PosingSystemPackageInfo.Get(typeof(jp.unisakistudio.posingsystem.PosingSystem));
            foreach (var language in PosingSystemLocalization.Codes)
                foreach (var document in new[] { "README.md", "README.html", "CHANGELOG.md", "CHANGELOG.html", "UserGuide.md", "UserGuide.html" })
                    Assert.That(File.Exists(Path.Combine(info.RootPath, "Documentation~", "Localization", language, document)), Is.True, language + "/" + document);
        }

        private static void WriteDocumentation(string root, string language, string history)
        {
            var directory = Path.Combine(root, "Documentation~", "Localization", language);
            Directory.CreateDirectory(directory);
            File.WriteAllText(Path.Combine(directory, "UserGuide.html"), "<!doctype html><title>" + language + "</title>");
            File.WriteAllText(Path.Combine(directory, "CHANGELOG.md"), history);
        }

        private sealed class DocumentationLanguage : IDisposable
        {
            private readonly GameObject owner;
            private readonly IDisposable scope;
            internal DocumentationLanguage(string language)
            {
                owner = new GameObject("Documentation language test");
                var system = owner.AddComponent<jp.unisakistudio.posingsystem.PosingSystem>();
                PosingSystemLocalization.Select(language);
                scope = PosingSystemLocalization.ForInspector(system);
            }
            public void Dispose()
            {
                scope.Dispose();
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }

        private static void WithTemporaryPackage(Action<string, PosingSystemPackageInfo.Info> test)
        {
            var parent = Path.GetFullPath(Path.GetTempPath());
            var root = Path.GetFullPath(Path.Combine(parent, "UnisakiDocumentationTests-" + Guid.NewGuid().ToString("N")));
            Assert.That(root.StartsWith(parent, StringComparison.OrdinalIgnoreCase), Is.True);
            Directory.CreateDirectory(root);
            try { test(root, new PosingSystemPackageInfo.Info { name = "jp.unisakistudio.posingsystem", RootPath = root }); }
            finally { Directory.Delete(root, true); }
        }
    }
}
