using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Entry = jp.unisakistudio.posingsystemeditor.PosingSystemPreprocessLog.Entry;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    // Stops the avatar build step the way a failing tool does, only while a test asks for it.
    internal sealed class FailingPreprocessForTests : VRC.SDKBase.Editor.BuildPipeline.IVRCSDKPreprocessAvatarCallback
    {
        internal static string Mode;
        public int callbackOrder => int.MinValue;
        public bool OnPreprocessAvatar(GameObject avatar)
        {
            if (Mode == "throw") throw new InvalidOperationException("preprocess test failure");
            if (Mode == "log") { Debug.LogError("preprocess test error"); return false; }
            if (Mode == "diagnostic") throw PosingSystemDiagnostics.Failure("PS987", "preprocess test diagnostic", "none", avatar);
            return Mode != "silent";
        }
    }

    // PS036: when the pose images cannot be captured because the avatar's build step fails, the
    // user learns which tool the error comes from and what to do, in every language. The web draft
    // carries no error text.
    public class PosingSystemPreprocessFailureTests : SharedLanguagePreferenceTestBase
    {
        // A tool throws inside its build callback (the shape Unity logs for an exception).
        private static readonly Entry OtherException = new Entry(
            "Exception: This feature requires a newer version of the SDK",
            "VF.Feature.Check () (at Packages/com.vrcfury.vrcfury/Editor/VF/Feature/Check.cs:12)\n" +
            "VF.Hooks.Run (UnityEngine.GameObject avatar) (at Packages/com.vrcfury.vrcfury/Editor/VF/Hooks/Run.cs:34)\n" +
            "VRC.SDKBase.Editor.BuildPipeline.VRCBuildPipelineCallbacks.OnPreprocessAvatar (UnityEngine.GameObject avatarGameObject) (at <00000000000000000000000000000000>:0)\n" +
            "jp.unisakistudio.posingsystemeditor.PosingSystemConverter.TakeScreenshot () (at Packages/jp.unisakistudio.posingsystem/Editor/PosingSystemConverter.cs:3660)\n", true);
        // A precompiled assembly logs an error: the only scripts in the trace are this tool's, which started the step.
        private static readonly Entry Unattributed = new Entry(
            "Avatar preprocessing was stopped",
            "UnityEngine.Debug:LogError (object)\n" +
            "VRC.SDKBase.Editor.BuildPipeline.VRCBuildPipelineCallbacks:OnPreprocessAvatar (UnityEngine.GameObject)\n" +
            "jp.unisakistudio.posingsystemeditor.PosingSystemConverter:TakeScreenshot () (at Packages/jp.unisakistudio.posingsystem/Editor/PosingSystemConverter.cs:3660)\n", false);
        private static readonly Entry OwnError = new Entry(
            "NullReferenceException: Object reference not set to an instance of an object",
            "jp.unisakistudio.posingsystemeditor.PosingSystemConverter.Convert () (at ./Library/PackageCache/jp.unisakistudio.posingsystem@3.2.0/Editor/PosingSystemConverter.cs:99)\n" +
            "nadena.dev.ndmf.BuildContext.RunPass () (at Packages/nadena.dev.ndmf/Editor/API/BuildContext.cs:340)\n", true);
        private static readonly Entry UserScript = new Entry(
            "C:\\Users\\secret-name\\Project\\Assets\\My Tool\\Editor\\Hook.cs(5,6): MySecretException: private text",
            "UnityEngine.Debug:LogError (object)\nMyHook:OnPreprocessAvatar (UnityEngine.GameObject) (at Assets/My Tool/Editor/Hook.cs:5)\n", false);

        [Test]
        public void TheSourceIsTheFirstScriptBeforeTheCallerOfTheBuildStep()
        {
            Assert.That(PosingSystemPreprocessLog.Script(OtherException), Is.EqualTo("Packages/com.vrcfury.vrcfury/Editor/VF/Feature/Check.cs"));
            Assert.That(PosingSystemPreprocessLog.Script(Unattributed), Is.Null, "this tool only started the step");
            Assert.That(PosingSystemPreprocessLog.Script(OwnError), Is.EqualTo("Library/PackageCache/jp.unisakistudio.posingsystem@3.2.0/Editor/PosingSystemConverter.cs"));
            Assert.That(PosingSystemPreprocessLog.Script(UserScript), Is.EqualTo("Assets/My Tool/Editor/Hook.cs"));
            // The text of a logged exception object carries its own frames.
            var logged = new Entry("System.Exception: failed\n  at Tool.Hook.Run () [0x00000] in C:\\Project\\Packages\\net.example.tool\\Editor\\Hook.cs:7 \n",
                "UnityEngine.Debug:LogError (object)\n", false);
            Assert.That(PosingSystemPreprocessLog.Script(logged), Is.EqualTo("Packages/net.example.tool/Editor/Hook.cs"));
            Assert.That(PosingSystemPreprocessLog.Script(new Entry(null, null, false)), Is.Null);
        }

        [Test]
        public void TheCallbackNamedByTheSdkIsTheSourceExceptForTheSharedBuildFramework()
        {
            Entry Aborted(string name) => new Entry("The VRCSDK build was aborted because the VRCSDKPreprocessAvatarCallback '" + name + "' reported a failure.", "", false);
            // A callback of this package (the check for compilation errors), although another tool logged an error earlier.
            var own = new[] { UserScript, Aborted("PosingSystemCompilationPrecheck") };
            Assert.That(PosingSystemPreprocessLog.CallbackPlace(own), Does.StartWith("Packages/jp.unisakistudio.posingsystem/").And.EndWith(".asmdef"));
            Assert.That(PosingSystemPreprocessLog.Source(own), Is.EqualTo(PosingSystemPreprocessLog.CallbackPlace(own)));
            // The framework runs the steps of many tools: the trace says which one failed.
            var framework = new[] { Aborted("BuildFrameworkPreprocessHook"), OtherException };
            Assert.That(PosingSystemPreprocessLog.CallbackPlace(framework), Does.Contain("nadena.dev.ndmf"));
            Assert.That(PosingSystemPreprocessLog.Source(framework), Is.EqualTo("Packages/com.vrcfury.vrcfury/Editor/VF/Feature/Check.cs"));
            Assert.That(PosingSystemPreprocessLog.Source(new[] { Aborted("BuildFrameworkPreprocessHook") }), Does.Contain("nadena.dev.ndmf"), "better than no source");
            // A name the SDK reports for a class that is not loaded here says nothing.
            Assert.That(PosingSystemPreprocessLog.CallbackPlace(new[] { Aborted("NoSuchCallbackClass") }), Is.Null);
            Assert.That(PosingSystemPreprocessLog.Source(new[] { Aborted("NoSuchCallbackClass"), UserScript }), Is.EqualTo("Assets/My Tool/Editor/Hook.cs"));
        }

        [Test]
        public void MessageNamesTheSourceAndFittingActionInEveryLanguage()
        {
            var sources = new[] { PosingSystemPreprocessLog.Summary, PosingSystemPreprocessLog.ListLabel, PosingSystemPreprocessLog.OwnAction,
                PosingSystemPreprocessLog.OtherAction, PosingSystemPreprocessLog.UnknownAction };
            foreach (var language in PosingSystemLocalization.Codes)
            {
                PosingSystemLocalization.Select(language);
                foreach (var source in sources)
                    if (language != "ja") Assert.That(PosingSystemLocalization.Text(source), Is.Not.EqualTo(source), language + ": " + source);

                // The rows around the exception come after it; the tool that threw is named.
                var message = PosingSystemPreprocessLog.Describe(new[] { Unattributed, OtherException }, null).Message;
                Assert.That(message, Does.Contain("PS036").And.Contain(PosingSystemLocalization.Text(PosingSystemPreprocessLog.Summary)), language);
                Assert.That(message, Does.Contain(PosingSystemLocalization.Format("エラーの発生元: {0}", "Packages/com.vrcfury.vrcfury")), language);
                Assert.That(message, Does.Contain(PosingSystemLocalization.Text(PosingSystemPreprocessLog.ListLabel) +
                    "\nException: This feature requires a newer version of the SDK\nAvatar preprocessing was stopped"), language);
                Assert.That(message, Does.Contain(PosingSystemLocalization.Text(PosingSystemPreprocessLog.OtherAction)), language);
                if (language != "ja") Assert.That(message, Does.Not.Contain("発生元").And.Not.Contain("ビルド前処理").And.Not.Contain("ポーズ"), language);

                message = PosingSystemPreprocessLog.Describe(new[] { OwnError }, null).Message;
                Assert.That(message, Does.Contain("jp.unisakistudio.posingsystem").And.Contain(PosingSystemLocalization.Text(PosingSystemPreprocessLog.OwnAction)), language);
                if (language != "ja") Assert.That(message, Does.Not.Contain("してください").And.Not.Contain("お問い合わせ"), language);

                // Nothing readable: no source is claimed.
                foreach (var entries in new[] { new Entry[0], new[] { Unattributed } })
                {
                    message = PosingSystemPreprocessLog.Describe(entries, null).Message;
                    Assert.That(message, Does.Contain("PS036").And.Contain(PosingSystemLocalization.Text(PosingSystemPreprocessLog.UnknownAction)), language);
                    Assert.That(message, Does.Not.Contain(PosingSystemLocalization.Format("エラーの発生元: {0}", "")), language);
                    if (language != "ja") Assert.That(message, Does.Not.Contain("最初に").And.Not.Contain("が原因"), language);
                }
            }
        }

        [Test]
        public void LocalMessageHidesTheUserFolderAndTheDraftLineCarriesNoErrorText()
        {
            PosingSystemLocalization.Select("ja");
            var message = PosingSystemPreprocessLog.Describe(new[] { UserScript }, null).Message;
            Assert.That(message, Does.Contain("Assets/My Tool").And.Not.Contain("secret-name"));

            Assert.That(PosingSystemPreprocessLog.Technical(new[] { UserScript }, 1), Is.EqualTo("[preprocess] errors=1 exceptions=0 origin=Assets"));
            var privatePackage = new Entry("OwnerNameException: private text", "A.B () (at Packages/com.private.owner-name/Editor/A.cs:1)\n", true);
            Assert.That(PosingSystemPreprocessLog.Technical(new[] { privatePackage }, 1), Is.EqualTo("[preprocess] errors=1 exceptions=1 origin=Packages"));
            Assert.That(PosingSystemPreprocessLog.Technical(new[] { Unattributed, OtherException }, 25),
                Does.Match(@"^\[preprocess\] errors=25 exceptions=1 origin=com\.vrcfury\.vrcfury(@\d+\.\d+\.\d+)? type=Exception$"));
            Assert.That(PosingSystemPreprocessLog.Technical(new[] { OwnError }, 1),
                Does.Match(@"^\[preprocess\] errors=1 exceptions=1 origin=jp\.unisakistudio\.posingsystem@\d+\.\d+\.\d+\S* type=NullReferenceException$"));
            Assert.That(PosingSystemPreprocessLog.Technical(new Entry[0], 0), Is.EqualTo("[preprocess] errors=0 exceptions=0 origin=unknown"));
        }

        // The real build step: what the SDK logs for a failing callback is enough to name its source.
        // "silent": the callback only answers "failed"; the SDK still names it.
        [TestCase("throw")]
        [TestCase("log")]
        [TestCase("silent")]
        public void AFailingCallbackOfTheRealBuildStepIsDescribed(string mode)
        {
            PosingSystemLocalization.Select("ja");
            var avatar = new GameObject("PreprocessFailureTest");
            var tool = new GameObject("Tool").AddComponent<jp.unisakistudio.posingsystem.PosingSystem>();
            var ignored = LogAssert.ignoreFailingMessages;
            try
            {
                LogAssert.ignoreFailingMessages = true;
                FailingPreprocessForTests.Mode = mode;
                Exception failure;
                using (var log = new PosingSystemPreprocessLog())
                {
                    Assert.That(VRC.SDKBase.Editor.BuildPipeline.VRCBuildPipelineCallbacks.OnPreprocessAvatar(avatar), Is.False);
                    failure = log.Failure(tool);
                }
                Assert.That(failure, Is.InstanceOf<PosingSystemDiagnostics.DiagnosticException>());
                Assert.That(failure.Message, Does.Contain("PS036").And.Contain(PosingSystemPreprocessLog.Summary));
                // The failing callback is a script of this package.
                Assert.That(failure.Message, Does.Contain("jp.unisakistudio.posingsystem").And.Contain(PosingSystemPreprocessLog.OwnAction));
                Assert.That(failure.Message, Does.Contain("FailingPreprocessForTests").And.Not.Contain("Cancelling DisplayDialog"));
                if (mode != "silent") Assert.That(failure.Message, Does.Contain(mode == "throw" ? "preprocess test failure" : "preprocess test error"));

                // The line added for the developer holds counts and the public identifier only.
                var draft = PosingSystemSupportReport.Draft(failure.Message, tool);
                Assert.That(draft, Does.Contain("PS036"));
                var line = System.Array.Find(draft.Split((char)10), l => l.StartsWith("[at-failure"));
                Assert.That(line, Does.Match(@"^\[at-failure \d+s\] \[preprocess\] errors=\d+ exceptions=\d+ origin=" +
                    @"jp\.unisakistudio\.posingsystem@\S+( type=\w+)?$"));
            }
            finally
            {
                FailingPreprocessForTests.Mode = null;
                LogAssert.ignoreFailingMessages = ignored;
                UnityEngine.Object.DestroyImmediate(avatar);
                UnityEngine.Object.DestroyImmediate(tool.gameObject);
            }
        }

        // One of this tool's own checks stopped the step: its message is shown as it is.
        [Test]
        public void ADiagnosticOfThisToolIsPassedOnUnchanged()
        {
            PosingSystemLocalization.Select("ja");
            var avatar = new GameObject("PreprocessFailureTest");
            var ignored = LogAssert.ignoreFailingMessages;
            try
            {
                LogAssert.ignoreFailingMessages = true;
                FailingPreprocessForTests.Mode = "diagnostic";
                Exception failure;
                using (var log = new PosingSystemPreprocessLog())
                {
                    Assert.That(VRC.SDKBase.Editor.BuildPipeline.VRCBuildPipelineCallbacks.OnPreprocessAvatar(avatar), Is.False);
                    failure = log.Failure(null);
                }
                Assert.That((failure as PosingSystemDiagnostics.DiagnosticException)?.Code, Is.EqualTo("PS987"));
                Assert.That(failure.Message, Does.Contain("preprocess test diagnostic").And.Not.Contain("PS036"));
            }
            finally
            {
                FailingPreprocessForTests.Mode = null;
                LogAssert.ignoreFailingMessages = ignored;
                UnityEngine.Object.DestroyImmediate(avatar);
            }
        }

        // The draft is written when the user presses the button of a notice that can be old: it
        // says what the checks found when the operation failed, next to what they find now.
        [Test]
        public void TheDraftKeepsWhatTheChecksFoundWhenTheOperationFailed()
        {
            PosingSystemLocalization.Select("ja");
            var root = new GameObject("Secret Avatar Name");
            var tool = new GameObject("Secret Tool Name").AddComponent<jp.unisakistudio.posingsystem.PosingSystem>();
            try
            {
                tool.transform.SetParent(root.transform);
                var failure = Assert.Throws<PosingSystemDiagnostics.DiagnosticException>(() => PosingSystemDiagnostics.ThrowIfInvalid(tool));
                Assert.That(failure.Code, Is.EqualTo("PS000"));
                Assert.That(failure.Message, Does.Contain("PS002"));

                var draft = PosingSystemSupportReport.Draft(failure.Message, tool);
                Assert.That(draft, Does.Match(@"\[at-failure \d+s\] \[object\] scene=\w+ hidden=0 active=1 playing=0"));
                Assert.That(draft, Does.Match(@"\[at-failure \d+s\] \[issues\] PS002\(E\)"));
                Assert.That(draft, Does.Not.Contain("Secret"));
                // A notice about something else, or about another tool, gets no such lines.
                Assert.That(PosingSystemSupportReport.Draft("PS041", tool), Does.Not.Contain("[at-failure"));
                var other = new GameObject("Other").AddComponent<jp.unisakistudio.posingsystem.PosingSystem>();
                try { Assert.That(PosingSystemSupportReport.Draft(failure.Message, other), Does.Not.Contain("[at-failure")); }
                finally { UnityEngine.Object.DestroyImmediate(other.gameObject); }
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
    }
}
