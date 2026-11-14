using System.Reflection;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.TestTools;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    // PS050: whatever the Console shows, the user learns which tool fails to compile and what to do,
    // in every language. The web draft still carries no name the user chose.
    public class PosingSystemCompileErrorReportTests : SharedLanguagePreferenceTestBase
    {
        private const string OtherError = "Assets/TexTool/Editor/Adjuster.cs(12,34): error CS0426: The type name 'Mode' does not exist in the type 'Adjuster'";
        private const string OwnError = "Packages/jp.unisakistudio.kawaiiposing/Editor/KawaiiPosingEditor.cs(5,6): error CS0117: 'A' does not contain a definition for 'B'";
        private const string Unreadable = "コンパイルエラーの内容を読み取れませんでした。Consoleの検索欄を空にし、エラーの表示を有効にして、赤い行の先頭を確認してください。";
        private const string OwnAction = "ALCOM / VCCで商品本体と「ゆにさきポーズシステム」を最新版に更新し、再コンパイルが完了してから実行してください。直らない場合は、このメッセージを添えてお問い合わせください。";
        private const string OtherAction = "ポーズツール以外で起きているエラーです。発生元のツールを更新するか、配布元で対応状況をご確認ください。エラーが解消し、再コンパイルが完了してから実行してください。";
        private const string UnknownAction = "Consoleの最初のコンパイルエラーを修正し、再コンパイルが完了してから実行してください。";

        [Test]
        public void OriginIsThePackageOrTheTopFolderUnderAssets()
        {
            Assert.That(PosingSystemSupportReport.Origin(OtherError, out var own), Is.EqualTo("Assets/TexTool"));
            Assert.That(own, Is.False);
            Assert.That(PosingSystemSupportReport.Origin("Assets/Loose.cs(1,2): error CS0103: x", out own), Is.EqualTo("Assets"));
            Assert.That(own, Is.False);
            Assert.That(PosingSystemSupportReport.Origin("Assets/UnisakiStudio/KawaiiPosing/Editor/Old.cs(1,2): error CS0101: x", out own), Is.EqualTo("Assets/UnisakiStudio"));
            Assert.That(own, Is.True, "a copy imported into Assets before the package release");
            Assert.That(PosingSystemSupportReport.Origin(OwnError, out own), Does.Contain("jp.unisakistudio.kawaiiposing"));
            Assert.That(own, Is.True);
            // Not installed here: only the identifier is known.
            Assert.That(PosingSystemSupportReport.Origin("Library/PackageCache/net.example.tool@1.2.3/Editor/A.cs(1,2): error CS0117: x", out own), Is.EqualTo("Packages/net.example.tool"));
            Assert.That(own, Is.False);
            // Installed: the name and version a user recognizes, with the identifier.
            Assert.That(PosingSystemSupportReport.Origin("Packages/nadena.dev.ndmf/Editor/A.cs(1,2): error CS0117: x", out own), Does.Match(@" \(nadena\.dev\.ndmf\) \d+\.\d+\.\d+"));
            Assert.That(own, Is.False);
            Assert.That(PosingSystemSupportReport.Origin("error CS2001: Source file 'x' could not be found.", out own), Is.Null);
            Assert.That(PosingSystemSupportReport.Origin(null, out own), Is.Null);
        }

        [Test]
        public void MessageNamesTheOriginAndFittingActionInEveryLanguage()
        {
            foreach (var language in PosingSystemLocalization.Codes)
            {
                PosingSystemLocalization.Select(language);
                foreach (var source in new[] { "エラーの発生元: {0}", Unreadable, OwnAction, OtherAction, UnknownAction })
                    if (language != "ja") Assert.That(PosingSystemLocalization.Text(source), Is.Not.EqualTo(source), language + ": " + source);
                Assert.That(PosingSystemLocalization.Format("エラーの発生元: {0}", "Assets/TexTool"), Does.Contain("Assets/TexTool"), language);

                var details = PosingSystemSupportReport.CompilerDetails(new[] { OtherError }, out var own, out var other);
                Assert.That(own, Is.False, language);
                Assert.That(other, Is.True, language);
                var message = PosingSystemDiagnostics.CompilationFailure(null, details, own, other).Message;
                Assert.That(message, Does.Contain("PS050").And.Contain(OtherError), language);
                Assert.That(message, Does.Contain(PosingSystemLocalization.Format("エラーの発生元: {0}", "Assets/TexTool")), language);
                Assert.That(message, Does.Contain(PosingSystemLocalization.Text(OtherAction)), language);
                if (language != "ja") Assert.That(message, Does.Not.Contain("発生元").And.Not.Contain("ポーズツール"), language);

                details = PosingSystemSupportReport.CompilerDetails(new[] { OtherError, OwnError }, out own, out other);
                Assert.That(own && other, Is.True, language);
                message = PosingSystemDiagnostics.CompilationFailure(null, details, own, other).Message;
                Assert.That(message, Does.Contain("Assets/TexTool, ").And.Contain("jp.unisakistudio.kawaiiposing"), language);
                Assert.That(message, Does.Contain(PosingSystemLocalization.Text(OwnAction)), language);
                if (language != "ja") Assert.That(message, Does.Not.Contain("更新し").And.Not.Contain("ゆにさき"), language);

                details = PosingSystemSupportReport.CompilerDetails(new string[0], out own, out other);
                Assert.That(own || other, Is.False, language);
                message = PosingSystemDiagnostics.CompilationFailure(null, details, own, other).Message;
                Assert.That(message, Does.Contain(PosingSystemLocalization.Text(Unreadable)).And.Contain(PosingSystemLocalization.Text(UnknownAction)), language);
                if (language != "ja") Assert.That(message, Does.Not.Contain("読み取れません").And.Not.Contain("最初の"), language);
            }
        }

        [Test]
        public void DraftNamesOnlyPackagesFromAPublishedListing()
        {
            Assert.That(PosingSystemSupportReport.SafeError("C:/Users/secret-name/Project/Assets/PrivateFolder/Editor/A.cs(12,34): error CS0426: private text"), Is.EqualTo("Assets CS0426"));
            Assert.That(PosingSystemSupportReport.SafeError("Packages/com.private.owner-name/Editor/A.cs(1,2): error CS0117: private text"), Is.EqualTo("Packages CS0117"));
            Assert.That(PosingSystemSupportReport.SafeError("Packages/com.private.owner-name/Editor/A.cs(1,2): warning CS0618: private text"), Is.Empty);

            const string manifest = "{\"dependencies\":{\"net.example.direct\":{\"version\":\"1.0.0\"}},\"locked\":{\"net.example.tool\":{\"version\":\"1.2.3\",\"dependencies\":{\"com.private.dependency\":\"^1.0.0\"}}}}";
            Assert.That(PosingSystemSupportReport.Listed(manifest, "net.example.tool"), Is.True);
            Assert.That(PosingSystemSupportReport.Listed(manifest, "com.private.dependency"), Is.False, "named by another package only");
            Assert.That(PosingSystemSupportReport.Listed(manifest, "net.example"), Is.False);
            Assert.That(PosingSystemSupportReport.Listed("{\"dependencies\":{}}", "net.example.tool"), Is.False);
            Assert.That(PosingSystemSupportReport.Listed(null, "net.example.tool"), Is.False);
        }

        // The Console API returns only the rows its window shows. The report has to read errors
        // behind a search text or a switched-off error button, and leave the window as it was.
        [Test]
        public void ConsoleErrorsAreReadBehindItsFilterAndTheFilterIsKept()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var entries = typeof(Editor).Assembly.GetType("UnityEditor.LogEntries");
            var shown = entries?.GetProperty("consoleFlags", flags);
            var getFilter = entries?.GetMethod("GetFilteringText", flags);
            var setFilter = entries?.GetMethod("SetFilteringText", flags);
            var clear = entries?.GetMethod("Clear", flags);
            if (shown == null || getFilter == null || setFilter == null || clear == null) Assert.Ignore("This Unity version has no such Console API.");

            var savedFlags = (int)shown.GetValue(null);
            var savedFilter = (string)getFilter.Invoke(null, null);
            try
            {
                clear.Invoke(null, null); // earlier rows would use up the report's row limit
                LogAssert.Expect(LogType.Error, new Regex("PS987"));
                Debug.LogError("[PosingSystem] PS987 console filter test");
                shown.SetValue(null, savedFlags | (1 << 9));
                setFilter.Invoke(null, new object[] { "" });
                Assume.That(PosingSystemSupportReport.Draft(null), Does.Contain("PS987"), "the Console keeps rows in this run mode");

                const string search = "no-row-contains-this-text";
                var hidden = savedFlags & ~(1 << 9);
                shown.SetValue(null, hidden);
                setFilter.Invoke(null, new object[] { search });
                Assert.That(PosingSystemSupportReport.Draft(null), Does.Contain("PS987"));
                Assert.That((int)shown.GetValue(null), Is.EqualTo(hidden));
                Assert.That((string)getFilter.Invoke(null, null), Is.EqualTo(search));
            }
            finally
            {
                shown.SetValue(null, savedFlags);
                setFilter.Invoke(null, new object[] { savedFilter });
            }
        }
    }
}
