using System;
using System.Collections.Generic;
using System.IO;
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
        // StickyError: rows Unity keeps until their cause is fixed (compiler and assembly definition errors).
        private const int Blocking = CompileError | (1 << 13);
        private const int ShowErrors = 1 << 9; // ConsoleWindow.ConsoleFlags.LogLevelError
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
                // The Console API returns only the rows its window shows: a search text or a
                // switched-off error button hides every error, and after an Editor restart these
                // rows are the only source. Show them for this read, then put the user's view back.
                var shown = entries.GetProperty("consoleFlags", flags);
                var getFilter = entries.GetMethod("GetFilteringText", flags);
                var setFilter = entries.GetMethod("SetFilteringText", flags);
                object hiddenFlags = null;
                string search = null;
                var args = new object[] { 0, Activator.CreateInstance(entryType) };
                try
                {
                    if (shown != null && getFilter != null && setFilter != null)
                    {
                        var current = (int)shown.GetValue(null);
                        var text = (string)getFilter.Invoke(null, null);
                        if ((current & ShowErrors) == 0) { hiddenFlags = current; shown.SetValue(null, current | ShowErrors); }
                        if (!string.IsNullOrEmpty(text)) { search = text; setFilter.Invoke(null, new object[] { "" }); }
                    }
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
                finally
                {
                    if (search != null) setFilter.Invoke(null, new object[] { search });
                    if (hiddenFlags != null) shown.SetValue(null, hiddenFlags);
                }
            }
            catch (Exception ex) when (ex is MemberAccessException || ex is TargetInvocationException ||
                ex is InvalidCastException || ex is AmbiguousMatchException || ex is ArgumentException) { }
            return result;
        }

        // Compiler events exist only for compilations this Editor session has seen. After a restart
        // the failed compilation is over before this class loads, so the Console rows are read instead.
        private static List<string> CompilerErrors() => compiler.Count > 0 ? compiler : ReadConsole(Blocking);

        internal static string CompilerDetails(out bool own, out bool other)
            => CompilerDetails(CompilerErrors().Select(LocalLine).ToList(), out own, out other);

        // Shown locally (dialog and Console) only. "own": a file of this studio's tools fails;
        // "other": a file of another tool or of the project fails.
        internal static string CompilerDetails(IReadOnlyList<string> errors, out bool own, out bool other)
        {
            own = other = false;
            if (errors.Count == 0)
                return "\n" + PosingSystemLocalization.Text("コンパイルエラーの内容を読み取れませんでした。Consoleの検索欄を空にし、エラーの表示を有効にして、赤い行の先頭を確認してください。");
            var origins = new List<string>();
            foreach (var error in errors)
            {
                var origin = Origin(error, out var mine);
                if (origin == null) continue;
                if (mine) own = true; else other = true;
                if (!origins.Contains(origin)) origins.Add(origin);
            }
            return (origins.Count == 0 ? "" : "\n" + PosingSystemLocalization.Format("エラーの発生元: {0}", string.Join(", ", origins))) +
                "\n" + PosingSystemLocalization.Text("コンパイルエラー（先頭から最大5件）") + "\n" + string.Join("\n", errors);
        }

        private static readonly Regex OriginPath = new Regex(
            @"^(?:(?:Packages|Library/PackageCache)/([A-Za-z0-9._-]+?)(?:@[^/]+)?/|Assets/(?:([^/()]+)/)?)", RegexOptions.IgnoreCase);

        // The package, or the top folder under Assets, that contains the failing script.
        internal static string Origin(string error, out bool own)
        {
            own = false;
            var match = OriginPath.Match(error ?? "");
            if (!match.Success) return null;
            if (!match.Groups[1].Success)
            {
                var folder = match.Groups[2].Value;
                // Versions before the package release were imported into Assets/UnisakiStudio.
                own = folder.Replace(" ", "").Equals("UnisakiStudio", StringComparison.OrdinalIgnoreCase);
                return folder.Length == 0 ? "Assets" : "Assets/" + folder;
            }
            var id = match.Groups[1].Value;
            own = id.StartsWith("jp.unisakistudio.", StringComparison.OrdinalIgnoreCase);
            var info = PackageInfo.FindForAssetPath("Packages/" + id);
            if (info == null) return "Packages/" + id;
            return (string.IsNullOrEmpty(info.displayName) ? id : info.displayName + " (" + id + ")") + " " + info.version;
        }

        // The same place for the web draft: a public package identifier with its version, or only
        // the kind of place.
        internal static string SafeOrigin(string path)
        {
            var match = OriginPath.Match(path ?? "");
            if (!match.Success) return "";
            if (!match.Groups[1].Success)
                return match.Groups[2].Value.Replace(" ", "").Equals("UnisakiStudio", StringComparison.OrdinalIgnoreCase) ? "Assets/UnisakiStudio" : "Assets";
            var id = match.Groups[1].Value;
            if (!tools.Contains(id) && !InstalledFromListing(id)) return "Packages";
            var version = SafeVersion(PackageInfo.FindForAssetPath("Packages/" + id)?.version);
            return id.ToLowerInvariant() + (version.Length > 0 ? "@" + version : "");
        }

        private static string SafeVersion(string version)
        {
            var match = Regex.Match(version ?? "", @"^\d+\.\d+\.\d+(?:-(?:alpha|beta|rc|pre|preview)[.-]?\d+)?", RegexOptions.IgnoreCase);
            return match.Success ? match.Value : "";
        }

        // Packages installed through ALCOM / VCC come from a published listing, so their identifier
        // is public. Embedded packages outside that list can carry a name the user chose.
        private static bool InstalledFromListing(string id)
        {
            try
            {
                return Listed(File.ReadAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath) ?? "", "Packages/vpm-manifest.json")), id);
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException) { return false; }
        }

        // An installed package is an object under "locked"; the packages it depends on are plain strings.
        internal static bool Listed(string manifest, string id)
        {
            var locked = (manifest ?? "").IndexOf("\"locked\"", StringComparison.Ordinal);
            return locked >= 0 && Regex.IsMatch(manifest.Substring(locked), "\"" + Regex.Escape(id) + "\"\\s*:\\s*\\{");
        }

        internal static string LocalLine(string text)
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
            if (source.Success && !tools.Contains(source.Groups[1].Value))
            {
                // Another tool: its public identifier and version, or only the kind of place.
                var other = source.Groups[1].Value;
                location = "Packages ";
                if (InstalledFromListing(other))
                {
                    var version = SafeVersion(PackageInfo.FindForAssetPath("Packages/" + other)?.version);
                    location = PosingSystemLocalization.Format("{0}（{1}行目）", other + (version.Length > 0 ? " " + version : ""), source.Groups[3].Value) + " ";
                }
            }
            else if (!source.Success && Regex.IsMatch(line, @"(?:^|[\s/])Assets/[^()\r\n]*\.cs\(\d+", RegexOptions.IgnoreCase))
                location = "Assets ";
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
                lines.AddRange(IssueLines(ps, PosingSystemDiagnostics.Collect(ps)));
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

        // Which checks fail, and why the shared Animator counts as unusable.
        private static List<string> IssueLines(jp.unisakistudio.posingsystem.PosingSystem ps, List<PosingSystemDiagnostics.Issue> issues)
        {
            var lines = new List<string>();
            if (issues.Count > 0)
                lines.Add("[issues] " + string.Join(" ", issues.GroupBy(i => i.Code + "(" + (i.Severity == MessageType.Error ? "E" : i.Severity == MessageType.Warning ? "W" : "I") + ")")
                    .Select(g => g.Key + (g.Count() > 1 ? "x" + g.Count() : "")).Take(12)));
            if (issues.Any(i => i.Code == "PS010"))
            {
                var avatar = ps.GetAvatar();
                var markers = PosingSystemDiagnostics.CommonAnimatorMarkers(ps);
                var recoverable = PosingSystemDiagnostics.CanRecoverCommonChild(ps, markers, out var orphan);
                var blocked = avatar != null ? PosingSystemHierarchy.BlockingParent(markers.FirstOrDefault()?.transform ?? orphan?.transform ?? ps.transform, avatar.transform) : null;
                // "outside": the tag or the switched-off object is above the avatar, not inside it.
                lines.Add("[PS010] markers=" + markers.Length + " active=" + markers.Count(PosingSystemDiagnostics.ActiveCommonMarker) +
                    " recoverable=" + (recoverable ? 1 : 0) + " blocked=" + (blocked == null ? "none" : blocked.CompareTag("EditorOnly") ? "editorOnly" : "inactive") +
                    " outside=" + (blocked != null && !blocked.IsChildOf(avatar.transform) ? 1 : 0) +
                    " prefab=" + PrefabUtility.GetPrefabInstanceStatus(ps.gameObject));
            }
            return lines;
        }

        // ---- The state at the moment of a failure -------------------------------------------
        // The notice of a failure stays in the Inspector, and the draft is written when the user
        // presses its button: by then the scene can differ, and a failure inside a build happens on
        // a temporary copy that no longer exists. These lines keep what was true when it failed.
        private sealed class Moment
        {
            public int Target;
            public string Code;
            public double Time;
            public List<string> Lines;
        }

        private const int Moments = 8;
        private static readonly List<Moment> moments = new List<Moment>();

        internal static void Remember(UnityEngine.Object target, string code, IEnumerable<string> lines)
        {
            if (moments.Count == Moments) moments.RemoveAt(0);
            moments.Add(new Moment { Target = target != null ? target.GetInstanceID() : 0, Code = code,
                Time = EditorApplication.timeSinceStartup, Lines = lines.ToList() });
        }

        // Something that did not stop the operation but explains a later question (numbers only, no names).
        private const string NoteCode = "note";
        internal static void Note(string line) => Remember(null, NoteCode, new[] { line });

        internal static List<string> Notes() => moments.Where(m => m.Code == NoteCode)
            .SelectMany(m => m.Lines.Select(line => "[note " + (int)Math.Max(0, EditorApplication.timeSinceStartup - m.Time) + "s] " + line))
            .GroupBy(line => line.Substring(line.IndexOf(']') + 1)).Select(group => group.Last()).ToList();

        // The object the checks ran on: the tool in the scene, or a copy made for a build.
        internal static void RememberInvalid(jp.unisakistudio.posingsystem.PosingSystem ps, List<PosingSystemDiagnostics.Issue> issues)
        {
            try
            {
                var scene = ps.gameObject.scene;
                var place = !scene.IsValid() ? "none" : UnityEditor.SceneManagement.EditorSceneManager.IsPreviewScene(scene) ? "preview"
                    : string.IsNullOrEmpty(scene.path) ? "unsaved" : "saved";
                var lines = new List<string> { "[object] scene=" + place + " hidden=" + (ps.transform.root.gameObject.hideFlags != HideFlags.None ? 1 : 0) +
                    " active=" + (ps.gameObject.activeInHierarchy ? 1 : 0) + " playing=" + (EditorApplication.isPlayingOrWillChangePlaymode ? 1 : 0) };
                lines.AddRange(IssueLines(ps, issues));
                Remember(ps, "PS000", lines);
            }
            catch (Exception) { }
        }

        // What other objects (build copies) remembered since the given time.
        internal static List<string> RememberedSince(double time, UnityEngine.Object except) =>
            moments.Where(m => m.Time >= time && (except == null || m.Target != except.GetInstanceID())).SelectMany(m => m.Lines).Distinct().ToList();

        internal static List<string> AtFailure(string[] codes, UnityEngine.Object target)
        {
            var moment = target == null ? null : moments.LastOrDefault(m => m.Target == target.GetInstanceID() && codes.Contains(m.Code));
            if (moment == null) return new List<string>();
            var age = (int)Math.Max(0, EditorApplication.timeSinceStartup - moment.Time);
            return moment.Lines.Select(line => "[at-failure " + age + "s] " + line).ToList();
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
                var version = SafeVersion(p.version);
                if (version.Length > 0) lines.Add(p.name + " " + version);
            }
            // Compiler errors come first: they explain a failed compilation, and unrelated errors
            // logged earlier would otherwise use up the row limit.
            var failed = EditorUtility.scriptCompilationFailed;
            var compiled = failed ? compiler.Select(SafeError).Concat(ReadConsole(Blocking, true)) : Enumerable.Empty<string>();
            var errors = compiled.Append(SafeError(failure)).Concat(ReadConsole(CompileError | OtherErrors, true))
                .Where(s=>s.Length>0).Distinct().Take(Limit).ToArray();
            if (errors.Length > 0) lines.Add(PosingSystemLocalization.Text("Consoleのエラー\n") + string.Join("\n",errors));
            // Zero in both counts means the errors could not be read, not that there are none.
            if (failed) lines.Add("[compile] failed=1 events=" + compiler.Count + " console=" + ReadConsole(Blocking).Count);
            var tool = Resolve(ps);
            lines.AddRange(ToolDetails(tool));
            lines.AddRange(AtFailure(codes, tool));
            lines.AddRange(Notes());
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
