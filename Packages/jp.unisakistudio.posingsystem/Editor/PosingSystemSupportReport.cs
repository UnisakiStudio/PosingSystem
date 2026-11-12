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
        private static List<string> ReadConsole(int mask, bool forDraft = false, bool whole = false)
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
                        var line = (string)message.GetValue(args[1]) ?? "";
                        if (!whole) line = line.Split('\n')[0].Trim();
                        if (whole) { }
                        else if (forDraft) line = SafeError(line);
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
            return errors.Count == 0 ? "" : "\n" + PosingSystemLocalization.Text("コンパイルエラー（先頭から最大5件）") + "\n" +
                string.Join("\n", errors.Select(s => LocalLine(s)));
        }

        private static string LocalLine(string text)
        {
            var line = text.Replace('\\','/');
            // Retain project-relative locations when the compiler supplies absolute paths.
            line = Regex.Replace(line, @"(?i)(?:[a-z]:/|//|/)[^\r\n]*?/(Assets/|Packages/|Library/PackageCache/)", "$1");
            line = Regex.Replace(line, @"(?i)(?:[a-z]:/|/(?:Users|home)/|//)[^\r\n]*?(?=\(\d+[,)]|$)", PosingSystemLocalization.Text("[パス]"));
            line = Regex.Replace(line, @"[\w.%+-]+@[\w.-]+\.[A-Za-z]{2,}", PosingSystemLocalization.Text("[メール]"));
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
                    : PosingSystemLocalization.Format("{0}（{1}行目）", package, source.Groups[3].Value) + " ";
            }
            if (code.Success)
            {
                var id = code.Groups[1].Value;
                if (id == "CS0618" && source.Groups[1].Value == "com.vrcfury.vrcfury" &&
                    source.Groups[2].Value == "VRCExpressionParameterExtensions.cs" &&
                    line.Contains("VRCExpressionParameters.TypeCost(") && line.Contains("Parameter.GetBitWidthByType()"))
                    return location + id + "（VRCExpressionParameters.TypeCost → Parameter.GetBitWidthByType）";
                return location + id + (id == "CS0618" || id == "CS0619" ? PosingSystemLocalization.Text("（廃止されたAPIの使用）") : "");
            }
            return exception.Success ? location + exception.Value : "";
        }

        // ---- Technical details for the developer -------------------------------------------
        // Language-neutral key=value lines: counts, switches, kinds of assets and code locations
        // inside this studio's packages. Never object, pose, avatar or file names chosen by the
        // user, and never arbitrary Console text.
        private static readonly Regex OwnFrame = new Regex(
            @"^\s*(?:at\s+)?jp\.unisakistudio\.[\w.]*?(\w+)[.:]([\w<>]+)\s*\(.*\(at [^\r\n]*?/([\w.-]+\.cs):(\d+)\)",
            RegexOptions.Multiline);

        // "NullReferenceException: ... @ Class.Method (File.cs:12) < Caller (File.cs:34)".
        // Only exceptions that passed through this studio's code; diagnostics are reported by code.
        internal static string ExceptionDetail(string entry)
        {
            if (string.IsNullOrEmpty(entry)) return "";
            var first = entry.Split('\n')[0].Trim();
            if (first.Contains("[PosingSystem")) return "";
            var type = Regex.Match(first, @"^(?:[\w.]+\.)?(\w*Exception)\b:?\s*(.*)$");
            if (!type.Success) return "";
            var frames = OwnFrame.Matches(entry).Cast<Match>().Take(3)
                .Select(m => m.Groups[1].Value + "." + m.Groups[2].Value + " (" + m.Groups[3].Value + ":" + m.Groups[4].Value + ")").ToArray();
            if (frames.Length == 0) return "";
            var message = LocalLine(type.Groups[2].Value);
            if (message.Length > 120) message = message.Substring(0, 120) + "…";
            return type.Groups[1].Value + (message.Length > 0 ? ": " + message : "") + " @ " + string.Join(" < ", frames);
        }

        private static string Kind(UnityEngine.Object motion)
        {
            if (motion == null) return "none";
            if (motion is UnityEditor.Animations.BlendTree) return "BlendTree";
            if (motion is AnimationClip clip)
                return clip.legacy ? "Clip:legacy" : PosingSystemRolloverGenerator.Supports(clip) ? "Clip:humanoid" : "Clip:generic";
            return motion.GetType().Name;
        }

        // The tool the report is about: the caller's, the selected one, or the only one in the scene.
        private static jp.unisakistudio.posingsystem.PosingSystem Resolve(jp.unisakistudio.posingsystem.PosingSystem ps)
        {
            if (ps != null) return ps;
            var selected = Selection.activeGameObject != null
                ? Selection.activeGameObject.GetComponentInParent<jp.unisakistudio.posingsystem.PosingSystem>(true) : null;
            if (selected != null) return selected;
            var all = UnityEngine.Object.FindObjectsOfType<jp.unisakistudio.posingsystem.PosingSystem>(true);
            return all.Length == 1 ? all[0] : null;
        }

        internal static List<string> ToolDetails(jp.unisakistudio.posingsystem.PosingSystem ps)
        {
            var lines = new List<string>();
            lines.Add("[env] os=" + Application.platform + " target=" + EditorUserBuildSettings.activeBuildTarget + " lang=" + PosingSystemLocalization.Language);
            if (ps == null) return lines;
            try
            {
                var layers = (ps.defines ?? new List<jp.unisakistudio.posingsystem.PosingSystem.LayerDefine>()).Where(d => d?.animations != null).ToList();
                var poses = layers.SelectMany(d => d.animations).Where(a => a != null).ToArray();
                var source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(ps.gameObject);
                var package = Regex.Match(source != null ? AssetDatabase.GetAssetPath(source) ?? "" : "", @"^Packages/(jp\.unisakistudio\.[a-z]+)/");
                string prebuild;
                try { prebuild = string.IsNullOrEmpty(ps.data) ? "none" : PosingSystemConverter.IsPosingSystemDataUpdated(ps) ? "stale" : "current"; }
                catch (Exception) { prebuild = "unknown"; }
                lines.Add("[tool] source=" + (package.Success ? package.Groups[1].Value : source != null ? "other-prefab" : "no-prefab") +
                    " poses=" + poses.Count(a => a.enabled) + "/" + poses.Length +
                    " custom=" + poses.Count(a => !PosingSystemLocalization.IsStandardDisplayName(a.displayName)) +
                    " imports=" + (ps.overrideDefines?.Count ?? 0) + " eightBit=" + (ps.isEightBitVersion ? 1 : 0) +
                    " prebuild=" + prebuild + " active=" + (ps.gameObject.activeInHierarchy ? 1 : 0));
                if (ps.features != null)
                    lines.Add("[features] " + string.Join(" ", ps.features.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)
                        .Where(f => f.FieldType == typeof(bool) || f.FieldType.IsEnum)
                        .Select(f => f.Name + "=" + (f.FieldType == typeof(bool) ? ((bool)f.GetValue(ps.features) ? "1" : "0") : f.GetValue(ps.features).ToString()))) +
                        " ik=" + (ps.enablePoseIk ? 1 : 0) + " avatarAdjust=" + (ps.enableAvatarSpecificAdjustment ? 1 : 0) +
                        " kneeClamp=" + (ps.enableReverseKneeSafetyClamp ? 1 : 0) + " autoImport=" + (ps.autoImportAvatarAnimations ? 1 : 0));
                var issues = PosingSystemDiagnostics.Collect(ps);
                if (issues.Count > 0)
                    lines.Add("[issues] " + string.Join(" ", issues.GroupBy(i => i.Code + "(" + (i.Severity == MessageType.Error ? "E" : i.Severity == MessageType.Warning ? "W" : "I") + ")")
                        .Select(g => g.Key + (g.Count() > 1 ? "x" + g.Count() : "")).Take(12)));
                if (issues.Any(i => i.Code == "PS010"))
                {
                    var avatar = ps.GetAvatar();
                    var markers = PosingSystemDiagnostics.CommonAnimatorMarkers(ps);
                    var recoverable = PosingSystemDiagnostics.CanRecoverCommonChild(ps, markers, out var orphan);
                    var blocked = avatar != null ? PosingSystemHierarchy.BlockingParent(markers.FirstOrDefault()?.transform ?? orphan?.transform ?? ps.transform, avatar.transform) : null;
                    lines.Add("[PS010] markers=" + markers.Length + " active=" + markers.Count(PosingSystemDiagnostics.ActiveCommonMarker) +
                        " recoverable=" + (recoverable ? 1 : 0) + " blocked=" + (blocked == null ? "none" : blocked.CompareTag("EditorOnly") ? "editorOnly" : "inactive") +
                        " prefab=" + PrefabUtility.GetPrefabInstanceStatus(ps.gameObject));
                }
                // Position and asset kinds identify the row without quoting its name.
                var rollover = new List<string>();
                for (var i = 0; i < layers.Count; i++)
                    for (var j = 0; j < layers[i].animations.Count; j++)
                    {
                        var pose = layers[i].animations[j];
                        if (pose == null || !pose.enabled || !pose.isRollover || pose.rolloverSideAnimation == null) continue;
                        if (pose.animationClip is AnimationClip center && PosingSystemRolloverGenerator.Supports(center) &&
                            PosingSystemRolloverGenerator.Supports(pose.rolloverSideAnimation)) continue;
                        rollover.Add((i + 1) + "-" + (j + 1) + " center=" + Kind(pose.animationClip) + " side=" + Kind(pose.rolloverSideAnimation) +
                            " standardName=" + (PosingSystemLocalization.IsStandardDisplayName(pose.displayName) ? 1 : 0));
                    }
                if (rollover.Count > 0) lines.Add("[PS070] " + string.Join(" ; ", rollover.Take(3)) + (rollover.Count > 3 ? " ; +" + (rollover.Count - 3) : ""));
            }
            catch (Exception exception)
            {
                // The report must still open when the tool is in a broken state.
                lines.Add("[tool] unavailable (" + exception.GetType().Name + ")");
            }
            return lines;
        }

        internal static string Draft(string failure, jp.unisakistudio.posingsystem.PosingSystem ps = null)
            => BuildDraft(failure, PosingSystemLocalization.Text("ポーズツールで次のエラーが出ました。対処方法を教えてください。"), ps);

        private static string BuildDraft(string failure, string question, jp.unisakistudio.posingsystem.PosingSystem ps = null)
        {
            var codes = Regex.Matches(failure ?? "", @"\bPS\d{3}\b").Cast<Match>().Select(m => m.Value).Distinct().Take(8).ToArray();
            var lines = new List<string> { question, PosingSystemLocalization.Text("行った操作・起きたこと："), "" };
            if (codes.Length != 0) lines.Add(PosingSystemLocalization.Text("診断 ") + string.Join(" ", codes));
            // Only package guidance is included; custom pose names remain excluded.
            if (codes.Contains("PS070")) lines.Add("PS070（" + PosingSystemLocalization.Text("寝返り") + "） " + PosingSystemLocalization.Text(PosingSystemDiagnostics.RolloverClipAction));
            lines.Add("Unity " + Application.unityVersion);
            foreach (var p in PackageInfo.GetAllRegisteredPackages().Where(p => tools.Contains(p.name)).OrderBy(p=>p.name,StringComparer.Ordinal))
            {
                var version = Regex.Match(p.version ?? "", @"^\d+\.\d+\.\d+(?:-(?:alpha|beta|rc|pre|preview)[.-]?\d+)?", RegexOptions.IgnoreCase);
                if (version.Success) lines.Add(p.name + " " + version.Value);
            }
            var compiled = EditorUtility.scriptCompilationFailed ? compiler.Select(SafeError) : Enumerable.Empty<string>();
            var errors = compiled.Append(SafeError(failure)).Concat(ReadConsole(CompileError | OtherErrors, true))
                .Where(s=>s.Length>0).Distinct().Take(Limit).ToArray();
            if (errors.Length > 0) lines.Add(PosingSystemLocalization.Text("Consoleのエラー\n") + string.Join("\n",errors));
            lines.AddRange(ToolDetails(Resolve(ps)));
            lines.AddRange(ReadConsole(OtherErrors, whole: true).Select(ExceptionDetail).Where(s => s.Length > 0).Distinct().Take(3).Select(s => "[exception] " + s));
            return string.Join("\n",lines);
        }

        internal static void Open(string failure, jp.unisakistudio.posingsystem.PosingSystem ps = null)
        {
            var draft = Draft(failure, ps);
            Application.OpenURL(PosingSystemPackageInfo.SupportUrl + "#unity-draft=" + Uri.EscapeDataString(draft));
        }

        internal static void OpenPrebuild(string paths)
        {
            var draft = BuildDraft(null, PosingSystemLocalization.Text("プレビルドの更新について相談したいです。") + "\n" + PosingSystemLocalization.Text("保存時と異なる項目（最大12件）") + "\n" + paths);
            Application.OpenURL(PosingSystemPackageInfo.SupportUrl + "#unity-draft=" + Uri.EscapeDataString(draft));
        }
    }
}
