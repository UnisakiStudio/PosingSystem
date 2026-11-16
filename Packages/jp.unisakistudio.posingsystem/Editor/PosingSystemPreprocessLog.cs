using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    // Pose images are captured from a copy of the avatar that went through the same build steps
    // as an upload, so every installed tool takes part. The step only answers "failed": the reason
    // is whatever was logged meanwhile. This keeps those errors, so the message can say which tool
    // they come from instead of only that the update was stopped.
    internal sealed class PosingSystemPreprocessLog : IDisposable
    {
        internal readonly struct Entry
        {
            public readonly string Condition, Stack;
            public readonly bool Exception;
            public Entry(string condition, string stack, bool exception)
            { Condition = condition ?? ""; Stack = stack ?? ""; Exception = exception; }
        }

        internal const string Summary = "アバターのビルド前処理が失敗したため、ポーズ画像を更新できません。";
        internal const string ListLabel = "ビルド前処理中のエラー（先頭から最大3件）";
        internal const string OwnAction = "ALCOM / VCCで商品本体と「ゆにさきポーズシステム」を最新版に更新してください。直らない場合は、「このエラーについて相談する」からお問い合わせください。";
        internal const string OtherAction = "ポーズツール以外で起きているエラーです。発生元のツールとVRChat SDKを最新版に更新してください。VRChat SDKの「Build & Test」でも同じエラーが出る場合は、発生元のツールの配布元で対応状況をご確認ください。";
        internal const string UnknownAction = "Window > General > Consoleで、この操作中に最初に出た赤いエラーを確認してください。VRChat SDKの「Build & Test」でも失敗する場合は、ポーズツール以外が原因です。";

        private const int Limit = 20;
        private readonly List<Entry> entries = new List<Entry>();
        private readonly double started = EditorApplication.timeSinceStartup;
        private PosingSystemDiagnostics.DiagnosticException diagnostic;
        private int total;

        internal PosingSystemPreprocessLog()
        {
            PosingSystemDiagnostics.LastCreated = null;
            Application.logMessageReceived += Receive;
        }

        public void Dispose() => Application.logMessageReceived -= Receive;

        private void Receive(string condition, string stack, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            total++;
            // One of this tool's own checks stopped the step: its message already says what to do.
            var created = PosingSystemDiagnostics.LastCreated;
            if (diagnostic == null && created != null && (condition ?? "").Contains(FirstLine(created.Message))) diagnostic = created;
            if (entries.Count < Limit) entries.Add(new Entry(condition, stack, type == LogType.Exception));
        }

        // Call when the step answered "failed".
        internal Exception Failure(UnityEngine.Object context)
        {
            var lines = new List<string> { Technical(entries, total) };
            lines.AddRange(PosingSystemSupportReport.RememberedSince(started, context));
            if (diagnostic != null)
            {
                PosingSystemSupportReport.Remember(context, diagnostic.Code, lines);
                return diagnostic;
            }
            PosingSystemSupportReport.Remember(context, "PS036", lines);
            return Describe(entries, context);
        }

        private static string FirstLine(string text) => (text ?? "").Split('\n')[0].Trim();

        private static readonly Regex ScriptPath = new Regex(
            @"(?:^|[\s(/])((?:Packages|Library/PackageCache|Assets)/[^:()\r\n]*?\.cs)(?=[:(])", RegexOptions.Multiline | RegexOptions.IgnoreCase);

        // The first script the error passed through. Frames from the caller of the build step on
        // belong to this tool, which only started the step.
        internal static string Script(Entry entry)
        {
            foreach (var part in new[] { entry.Condition, entry.Stack })
            {
                var text = part.Replace('\\', '/');
                var caller = text.IndexOf("VRCBuildPipelineCallbacks", StringComparison.Ordinal);
                var match = ScriptPath.Match(caller < 0 ? text : text.Substring(0, caller));
                if (match.Success) return match.Groups[1].Value;
            }
            return null;
        }

        private static readonly Regex Callback = new Regex(@"VRCSDKPreprocessAvatarCallback '([\w.+]+)'");
        private static readonly Regex ProjectPath = new Regex(@"(?:^|/)((?:Packages|Library/PackageCache|Assets)/.*)$", RegexOptions.IgnoreCase);

        // The SDK names the callback that stopped the step, also when that callback logged nothing.
        // Its place is the assembly definition (or the library) that holds the class.
        internal static string CallbackPlace(IReadOnlyList<Entry> entries)
        {
            foreach (var entry in entries)
            {
                var match = Callback.Match(entry.Condition);
                if (!match.Success) continue;
                var name = match.Groups[1].Value;
                foreach (var type in TypeCache.GetTypesDerivedFrom<VRC.SDKBase.Editor.BuildPipeline.IVRCSDKPreprocessAvatarCallback>())
                {
                    if (type.Name != name && type.FullName != name) continue;
                    var place = CompilationPipeline.GetAssemblyDefinitionFilePathFromAssemblyName(type.Assembly.GetName().Name);
                    if (string.IsNullOrEmpty(place))
                    {
                        // A precompiled library, or the scripts under Assets that have no assembly definition.
                        var library = ProjectPath.Match((type.Assembly.Location ?? "").Replace('\\', '/'));
                        place = library.Success && !library.Groups[1].Value.StartsWith("Library/ScriptAssemblies", StringComparison.OrdinalIgnoreCase)
                            ? library.Groups[1].Value : "Assets/";
                    }
                    return place.Replace('\\', '/');
                }
            }
            return null;
        }

        // Where the failure comes from. NDMF runs the steps of many tools inside its own callback,
        // so for it the trace of the error says more than the name of the callback.
        internal static string Source(IReadOnlyList<Entry> entries)
        {
            var callback = CallbackPlace(entries);
            if (callback != null && callback.IndexOf("nadena.dev.ndmf", StringComparison.OrdinalIgnoreCase) < 0) return callback;
            return Ordered(entries).Select(Script).FirstOrDefault(s => s != null) ?? callback;
        }

        // An exception says more about the cause than the rows logged around it.
        private static List<Entry> Ordered(IReadOnlyList<Entry> entries) =>
            entries.Where(e => e.Exception).Concat(entries.Where(e => !e.Exception)).ToList();

        internal static PosingSystemDiagnostics.DiagnosticException Describe(IReadOnlyList<Entry> entries, UnityEngine.Object context)
        {
            var origin = PosingSystemSupportReport.Origin(Source(entries), out var own);
            // In batch mode the SDK also logs the dialog it could not show.
            var rows = Ordered(entries).Select(e => PosingSystemSupportReport.LocalLine(FirstLine(e.Condition)))
                .Where(r => r.Length > 0 && !r.StartsWith("Cancelling DisplayDialog", StringComparison.Ordinal)).Distinct().Take(3).ToList();
            var details = (origin == null ? "" : "\n" + PosingSystemLocalization.Format("エラーの発生元: {0}", origin)) +
                (rows.Count == 0 ? "" : "\n" + PosingSystemLocalization.Text(ListLabel) + "\n" + string.Join("\n", rows));
            return PosingSystemDiagnostics.Failure("PS036", PosingSystemLocalization.Text(Summary) + details,
                origin == null ? UnknownAction : own ? OwnAction : OtherAction, context);
        }

        // For the web draft: counts, the public identifier of the source and the kind of exception.
        // Never the text of an error, which can hold names the user chose.
        internal static string Technical(IReadOnlyList<Entry> entries, int total)
        {
            var origin = PosingSystemSupportReport.SafeOrigin(Source(entries));
            var kind = entries.Where(e => e.Exception).Select(e => Regex.Match(FirstLine(e.Condition), @"^(?:[\w.]+\.)?(\w*Exception)\b"))
                .Where(m => m.Success).Select(m => m.Groups[1].Value).FirstOrDefault();
            // A type declared in the user's own scripts can carry a name they chose.
            var named = origin.Length > 0 && origin != "Assets" && origin != "Packages";
            return "[preprocess] errors=" + total + " exceptions=" + entries.Count(e => e.Exception) +
                " origin=" + (origin.Length > 0 ? origin : "unknown") + (kind != null && named ? " type=" + kind : "");
        }
    }
}
