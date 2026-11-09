using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace jp.unisakistudio.posingsystemeditor
{
    [InitializeOnLoad]
    internal static class PosingSystemSupportReport
    {
        private const int Limit = 5;
        private const int CompileError = 1 << 11; // Unity 2022.3 ConsoleWindow.Mode.ScriptCompileError
        private const int OtherErrors = 1 | (1 << 4) | (1 << 6) | (1 << 8) | (1 << 17);
        private static readonly List<string> compiler = new List<string>();
        // Only public package identifiers are included in the web draft. Local/custom package
        // names, asset paths and arbitrary Console text can contain personal information.
        private static readonly HashSet<string> tools = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "jp.unisakistudio.posingsystem", "jp.unisakistudio.kawaiiposing", "jp.unisakistudio.coolposing",
            "jp.unisakistudio.virtuallove", "jp.unisakistudio.virtualbed", "com.vrchat.avatars", "com.vrchat.base",
            "nadena.dev.modular-avatar", "nadena.dev.ndmf", "com.vrcfury.vrcfury", "jp.suzuryg.face-emo",
            "com.anatawa12.avatar-optimizer", "io.github.azukimochi.light-limit-changer",
            "net.narazaka.vrchat.floor-adjuster", "vrchat.blackstartx.gesture-manager", "lyuma.av3emulator",
            "dev.limitex.avatar-compressor", "com.lemoneru.avatar-blink-fix", "jp.lilxyzw.avatar-utils", "jp.lilxyzw.liltoon"
        };

        static PosingSystemSupportReport()
        {
            CompilationPipeline.compilationStarted += _ => compiler.Clear();
            CompilationPipeline.assemblyCompilationFinished += (_, messages) =>
            {
                foreach (var m in messages.Where(m => m.type == CompilerMessageType.Error))
                {
                    if (compiler.Count == Limit) break;
                    var line = m.message.Split('\n')[0].Trim();
                    if (!string.IsNullOrEmpty(m.file) && !line.Contains(m.file))
                        line = $"{m.file}({m.line},{m.column}): {line}";
                    if (!compiler.Contains(line)) compiler.Add(line);
                }
            };
        }

        // Unity exposes live compiler events publicly, but not the existing Console rows.
        // When this internal API is unavailable the build check and manual Console hint still work.
        private static List<string> ReadConsole(int mask, bool forDraft = false)
        {
            var result = new List<string>();
            try
            {
                var assembly = typeof(Editor).Assembly;
                var entries = assembly.GetType("UnityEditor.LogEntries");
                var entryType = assembly.GetType("UnityEditor.LogEntry");
                const BindingFlags flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
                var start = entries?.GetMethod("StartGettingEntries", flags);
                var end = entries?.GetMethod("EndGettingEntries", flags);
                var get = entries?.GetMethod("GetEntryInternal", flags);
                var mode = entryType?.GetField("mode");
                var message = entryType?.GetField("message");
                if (start == null || end == null || get == null || mode == null || message == null) return result;
                var args = new object[] { 0, Activator.CreateInstance(entryType) };
                var count = (int)start.Invoke(null, null);
                try
                {
                    for (int i = 0; i < count && result.Count < Limit; i++)
                    {
                        args[0] = i;
                        if (!(bool)get.Invoke(null, args) || ((int)mode.GetValue(args[1]) & mask) == 0) continue;
                        var line = ((string)message.GetValue(args[1]) ?? "").Split('\n')[0].Trim();
                        if (forDraft) line = SafeError(line);
                        else if (line.Contains("[PosingSystem")) continue;
                        if (line.Length > 0 && !result.Contains(line)) result.Add(line);
                    }
                }
                finally { end.Invoke(null, null); }
            }
            catch (Exception ex) when (ex is MemberAccessException || ex is TargetInvocationException ||
                ex is InvalidCastException || ex is AmbiguousMatchException || ex is ArgumentException) { }
            return result;
        }

        private static List<string> CompilerErrors() => compiler.Count > 0 ? compiler : ReadConsole(CompileError);

        internal static string CompilerDetails()
        {
            var errors = CompilerErrors();
            return errors.Count == 0 ? "" : "\nコンパイルエラー（先頭から最大5件）\n" +
                string.Join("\n", errors.Select(s => LocalLine(s)));
        }

        private static string LocalLine(string text)
        {
            var line = text.Replace('\\','/');
            // Retain project-relative locations when the compiler supplies absolute paths.
            line = Regex.Replace(line, @"(?i)(?:[a-z]:/|//|/)[^\r\n]*?/(Assets/|Packages/|Library/PackageCache/)", "$1");
            line = Regex.Replace(line, @"(?i)(?:[a-z]:/|/(?:Users|home)/|//)[^\r\n]*?(?=\(\d+[,)]|$)", "[パス]");
            line = Regex.Replace(line, @"[\w.%+-]+@[\w.-]+\.[A-Za-z]{2,}", "[メール]");
            return line.Length > 500 ? line.Substring(0,500) + "…" : line;
        }

        internal static string SafeError(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "";
            var line = raw.Replace('\\','/');
            const string posingType = "[jp.unisakistudio.posingsystem]jp.unisakistudio.posingsystem.PosingSystem";
            if (line.Contains("Type '" + posingType + "' has an extra field 'legacyAutoHeadMenuChecked'") &&
                line.Contains("can't be serialized"))
                return "jp.unisakistudio.posingsystem.PosingSystem / legacyAutoHeadMenuChecked / can't be serialized";
            if (line.Contains("[PosingSystem"))
            {
                var diagnosis = Regex.Match(line, @"\bPS\d{3}\b");
                if (diagnosis.Success) return diagnosis.Value;
            }
            var code = Regex.Match(line, @"\berror\s+(CS\d{4})\b", RegexOptions.IgnoreCase);
            var exception = Regex.Match(line, @"\b(NullReferenceException|InvalidOperationException|ArgumentException|IOException|MissingReferenceException|FileNotFoundException|TypeLoadException)\b");
            var source = Regex.Match(line, @"(?:^|/)(?:Packages|Library/PackageCache)/([A-Za-z0-9.-]+)(?:@[^/\s]+)?/(?:[^/()\r\n]+/)*([A-Za-z0-9_.-]+\.cs)\((\d+)(?:,(\d+))?\)", RegexOptions.IgnoreCase);
            string location = "";
            if (source.Success && tools.Contains(source.Groups[1].Value))
            {
                // A locally edited package can contain a script named after its owner.
                var package = source.Groups[1].Value;
                var script = source.Groups[2].Value;
                location = package == "com.vrcfury.vrcfury" && script == "VRCExpressionParameterExtensions.cs"
                    ? package + "/" + script + ":" + source.Groups[3].Value + " "
                    : package + "（" + source.Groups[3].Value + "行目） ";
            }
            if (code.Success)
            {
                var id = code.Groups[1].Value;
                if (id == "CS0618" && source.Groups[1].Value == "com.vrcfury.vrcfury" &&
                    source.Groups[2].Value == "VRCExpressionParameterExtensions.cs" &&
                    line.Contains("VRCExpressionParameters.TypeCost(") && line.Contains("Parameter.GetBitWidthByType()"))
                    return location + id + "（VRCExpressionParameters.TypeCost → Parameter.GetBitWidthByType）";
                return location + id + (id == "CS0618" || id == "CS0619" ? "（廃止されたAPIの使用）" : "");
            }
            return exception.Success ? location + exception.Value : "";
        }

        internal static string Draft(string failure) => BuildDraft(failure, "ポーズツールで次のエラーが出ました。対処方法を教えてください。");

        private static string BuildDraft(string failure, string question)
        {
            var codes = Regex.Matches(failure ?? "", @"\bPS\d{3}\b").Cast<Match>().Select(m => m.Value).Distinct().Take(8).ToArray();
            var lines = new List<string> { question, "行った操作・起きたこと：", "" };
            if (codes.Length != 0) lines.Add("診断 " + string.Join(" ", codes));
            // Include the package's guidance, never the issue summary containing custom pose names.
            if (codes.Contains("PS070")) lines.Add("PS070（寝返り） " + PosingSystemDiagnostics.RolloverClipAction);
            lines.Add("Unity " + Application.unityVersion);
            foreach (var p in PackageInfo.GetAllRegisteredPackages().Where(p => tools.Contains(p.name)).OrderBy(p=>p.name,StringComparer.Ordinal))
            {
                var version = Regex.Match(p.version ?? "", @"^\d+\.\d+\.\d+(?:-(?:alpha|beta|rc|pre|preview)[.-]?\d+)?", RegexOptions.IgnoreCase);
                if (version.Success) lines.Add(p.name + " " + version.Value);
            }
            var compiled = EditorUtility.scriptCompilationFailed ? compiler.Select(SafeError) : Enumerable.Empty<string>();
            var errors = compiled.Append(SafeError(failure)).Concat(ReadConsole(CompileError | OtherErrors, true))
                .Where(s=>s.Length>0).Distinct().Take(Limit).ToArray();
            if (errors.Length > 0) lines.Add("Consoleのエラー\n" + string.Join("\n",errors));
            return string.Join("\n",lines);
        }

        internal static void Open(string failure)
        {
            var draft = Draft(failure);
            Application.OpenURL(PosingSystemPackageInfo.SupportUrl + "#unity-draft=" + Uri.EscapeDataString(draft));
        }

        internal static void OpenPrebuild(string paths)
        {
            var draft = BuildDraft(null, "プレビルドの更新について相談したいです。\n保存時と異なる項目（最大12件）\n" + paths);
            Application.OpenURL(PosingSystemPackageInfo.SupportUrl + "#unity-draft=" + Uri.EscapeDataString(draft));
        }
    }
}
