// Shared, standalone setup UI. Generated catalogs are embedded at build time.
// This file does not inspect, register or remove any product license.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace UnisakiStudio.Setup
{
    internal static class Language
    {
        internal const string PreferenceKey = "jp.unisakistudio.posingsystem.language";
        internal static bool HasSelection => Array.IndexOf(Codes, EditorPrefs.GetString(PreferenceKey, "")) >= 0;
        // (language, exact source text) => translation or null. Null in the standalone setup program.
        internal static Func<string, string, string> External;
        internal static readonly string[] Codes = { "ja", "en", "fr", "it", "zh-Hans", "zh-Hant", "ko", "th", "de", "es", "pt", "pt-BR", "ru", "uk", "pl", "nl", "sv", "da", "fi", "nb", "cs", "tr", "id", "vi", "hi", "bn", "ta", "te", "ar", "he" };
        internal static readonly string[] Names = { "日本語", "English", "Français", "Italiano", "简体中文", "繁體中文", "한국어", "ไทย", "Deutsch", "Español", "Português", "Português (Brasil)", "Русский", "Українська", "Polski", "Nederlands", "Svenska", "Dansk", "Suomi", "Norsk", "Čeština", "Türkçe", "Bahasa Indonesia", "Tiếng Việt", "हिन्दी", "বাংলা", "தமிழ்", "తెలుగు", "العربية", "עברית" };
        // Preference reads must not initialize translation catalogs or diagnostic regexes.
        private static Dictionary<string, string> Aliases => TranslationData.Aliases;
        private static SetupLanguageData.Catalogs Catalogs => TranslationData.Catalogs;
        private static class TranslationData
        {
            internal static readonly Dictionary<string, string> Aliases = SetupLanguageData.Aliases();
            internal static readonly SetupLanguageData.Catalogs Catalogs = new SetupLanguageData.Catalogs();
            internal static readonly HashSet<string> Canonical = new HashSet<string>(Aliases.Values, StringComparer.Ordinal);
        }
        private static readonly Dictionary<string, int[]> LocalizedArguments = new Dictionary<string, int[]> { { "Applied {0} IK rules.\n{1}", new[] { 1 } }, { "{0} was stopped.\n{1}", new[] { 1 } }, { "Prefab to find: {0}\nThe purchased license installer alone does not complete product installation. In ALCOM / VCC, make sure the product and Unisaki Posing System are added to this project.\nIf already installed: {1}", new[] { 1 } }, { "The common posing Animator is unavailable. \"{0}\" {1}.", new[] { 1 } } };
        // The tables are read from a data file beside this script, one section at a time.
        // As C# literals they were 7 MB of source in the editor assembly.
        private static class SetupLanguageData
        {
            private const string FileName = "SetupLanguageData.bytes";
            private const int Magic = 827085653;
            internal static string PathOverride;
            private static Dictionary<string, KeyValuePair<long, int>> index;
            private static readonly List<string> names = new List<string>();
            private static readonly Dictionary<string, Dictionary<string, string>> sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            private static string file;
            private static string Locate()
            {
                if (!string.IsNullOrEmpty(PathOverride)) return PathOverride;
                var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(Language).Assembly);
                if (package != null) return System.IO.Path.Combine(package.resolvedPath, "Editor", "Setup", FileName);
                // The copy left under Assets by the setup program: its data has a fixed GUID,
                // so it is found even if the folder was moved.
                var asset = AssetDatabase.GUIDToAssetPath("4a5bbb23675c52f5a3d85e48a4d90c06");
                return System.IO.Path.GetFullPath(string.IsNullOrEmpty(asset) ? "Assets/UniSakiStudio/Setup/Editor/SetupLanguageData.bytes" : asset);
            }
            private static bool Open()
            {
                if (index != null) return index.Count > 0;
                index = new Dictionary<string, KeyValuePair<long, int>>(StringComparer.Ordinal);
                try
                {
                    file = Locate();
                    using (var stream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
                    using (var reader = new System.IO.BinaryReader(stream, System.Text.Encoding.UTF8))
                    {
                        if (reader.ReadInt32() != Magic) throw new System.IO.InvalidDataException("Unexpected header");
                        var count = reader.ReadInt32();
                        for (var i = 0; i < count; i++)
                        {
                            var name = reader.ReadString();
                            var offset = reader.ReadInt64();
                            index[name] = new KeyValuePair<long, int>(offset, reader.ReadInt32());
                            names.Add(name);
                        }
                    }
                }
                catch (Exception exception) when (exception is System.IO.IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
                {
                    index.Clear(); names.Clear();
                    // Without the tables every text stays in its source language; say so once.
                    Debug.LogWarning("[PosingSystem] " + FileName + " could not be read (" + exception.Message + "). Reinstall the package to restore translated text.");
                }
                return index.Count > 0;
            }
            private static Dictionary<string, string> Load(string section)
            {
                if (sections.TryGetValue(section, out var result)) return result;
                sections[section] = result = new Dictionary<string, string>(StringComparer.Ordinal);
                if (!Open() || !index.TryGetValue(section, out var place)) return result;
                try
                {
                    using (var stream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read, 1 << 16))
                    using (var reader = new System.IO.BinaryReader(stream, System.Text.Encoding.UTF8))
                    {
                        stream.Seek(place.Key, System.IO.SeekOrigin.Begin);
                        for (var i = 0; i < place.Value; i++)
                        {
                            var key = reader.ReadString();
                            result[key] = reader.ReadString();
                        }
                    }
                }
                catch (Exception exception) when (exception is System.IO.IOException || exception is UnauthorizedAccessException)
                {
                    result.Clear();
                    Debug.LogWarning("[PosingSystem] " + FileName + " could not be read (" + exception.Message + ").");
                }
                return result;
            }
            // A translation that is unambiguous backwards is also the alias "translated text -> key".
            // Those entries are stored once, in their own sections, and read backwards here.
            internal static Dictionary<string, string> Aliases()
            {
                var result = new Dictionary<string, string>(Load("aliases"), StringComparer.Ordinal);
                if (!Open()) return result;
                foreach (var name in names)
                {
                    if (!name.StartsWith("reversible:", StringComparison.Ordinal)) continue;
                    foreach (var pair in Load(name))
                        if (!result.ContainsKey(pair.Value)) result[pair.Value] = pair.Key;
                }
                return result;
            }
            // Same lookup as the dictionary of dictionaries it replaces; a language is read when first asked for.
            internal sealed class Catalogs
            {
                private readonly Dictionary<string, Dictionary<string, string>> loaded = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
                internal bool TryGetValue(string language, out Dictionary<string, string> catalog)
                {
                    if (language == null) { catalog = null; return false; }
                    if (!loaded.TryGetValue(language, out catalog))
                    {
                        catalog = null;
                        if (Open() && (index.ContainsKey("catalog:" + language) || index.ContainsKey("reversible:" + language)))
                        {
                            catalog = new Dictionary<string, string>(Load("catalog:" + language), StringComparer.Ordinal);
                            foreach (var pair in Load("reversible:" + language)) catalog[pair.Key] = pair.Value;
                        }
                        loaded[language] = catalog;
                    }
                    return catalog != null;
                }
            }
        }

        private sealed class Template
        {
            internal Regex Pattern;
            internal string Canonical;
            internal int[] Indices;
            internal int[] TextArguments;
            internal string Prefix, Suffix;
        }
        private static class DiagnosticData
        {
            internal static readonly List<Template> Templates = BuildTemplates();
            internal static readonly string[] Prefixes = Aliases.SelectMany(pair => new[] { pair.Key, pair.Value })
                .Where(value => value.EndsWith(":", StringComparison.Ordinal)).Distinct().ToArray();
        }
        private static List<Template> BuildTemplates()
        {
            var result = new List<Template>();
            var patterns = Aliases.Concat(Aliases.Values.Select(value => new KeyValuePair<string, string>(value, value)))
                .Where(pair => pair.Key.IndexOf('{') >= 0).GroupBy(pair => pair.Key).Select(group => group.First());
            foreach (var pair in patterns.OrderByDescending(pair => Regex.Replace(pair.Key, @"\{\d+\}", "").Length))
            {
                var matches = Regex.Matches(pair.Key, @"\{(\d+)\}");
                if (matches.Count == 0 || Regex.Replace(pair.Key, @"\{\d+\}", "").Length == 0) continue;
                var indices = new List<int>();
                var textArguments = LocalizedArguments.TryGetValue(pair.Value, out var localized) ? localized : Array.Empty<int>();
                var pattern = "^"; var position = 0;
                foreach (Match match in matches)
                {
                    var index = int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                    indices.Add(index);
                    // Metadata fields are single-line. Only designated explanatory text may
                    // span lines, so an inner warning cannot consume the enclosing message.
                    var capture = Array.IndexOf(textArguments, index) >= 0 ? ".*?" : @"[^\r\n]*?";
                    pattern += Regex.Escape(pair.Key.Substring(position, match.Index - position)) + "(?<p" + index + ">" + capture + ")";
                    position = match.Index + match.Length;
                }
                pattern += Regex.Escape(pair.Key.Substring(position)) + "$";
                var first = matches[0]; var last = matches[matches.Count - 1];
                result.Add(new Template { Pattern = new Regex(pattern, RegexOptions.Singleline | RegexOptions.CultureInvariant), Canonical = pair.Value, Indices = indices.ToArray(), TextArguments = textArguments,
                    Prefix = pair.Key.Substring(0, first.Index), Suffix = pair.Key.Substring(last.Index + last.Length) });
            }
            return result;
        }
        private static bool TryFormatted(string language, string text, out string translated)
        {
            foreach (var template in DiagnosticData.Templates)
            {
                if (!text.StartsWith(template.Prefix, StringComparison.Ordinal) || !text.EndsWith(template.Suffix, StringComparison.Ordinal)) continue;
                var match = template.Pattern.Match(text);
                if (!match.Success) continue;
                var maximum = 0;
                foreach (var index in template.Indices) maximum = Math.Max(maximum, index);
                var arguments = new object[maximum + 1];
                foreach (var index in template.Indices) arguments[index] = match.Groups["p" + index].Value;
                // A translation with fewer placeholders must fall back, not throw into the caller.
                foreach (var index in template.TextArguments) if (index < arguments.Length) arguments[index] = Translate(language, arguments[index] as string);
                var target = Catalogs.TryGetValue(language, out var catalog) && catalog.TryGetValue(template.Canonical, out var value) ? value : template.Canonical;
                try { translated = string.Format(CultureInfo.InvariantCulture, target, arguments); return true; }
                catch (FormatException) { }
            }
            translated = null; return false;
        }
        internal static string Current => Resolve(EditorPrefs.GetString(PreferenceKey, ""),
            CultureInfo.CurrentUICulture.Name, Application.systemLanguage.ToString());
        internal static bool IsSupported(string code) => Array.IndexOf(Codes, code) >= 0;
        internal static string Resolve(string saved, string culture, string systemLanguage)
            => IsSupported(saved) ? saved : Detect(culture, systemLanguage);
        internal static string Detect(string culture, string systemLanguage)
        {
            culture = (culture ?? "").Replace('_', '-').ToLowerInvariant();
            string regional = culture == "zh-tw" || culture == "zh-hk" || culture == "zh-mo" || culture.StartsWith("zh-hant")
                ? "zh-Hant" : culture.StartsWith("zh") ? "zh-Hans" : culture.StartsWith("pt-br") ? "pt-BR"
                : culture.StartsWith("nb") || culture.StartsWith("nn") || culture.StartsWith("no") ? "nb"
                : culture.Split('-')[0];
            string code;
            switch (systemLanguage)
            {
                case "Japanese": code = "ja"; break;
                case "ChineseSimplified": code = "zh-Hans"; break;
                case "ChineseTraditional": code = "zh-Hant"; break;
                case "Chinese": code = regional.StartsWith("zh-") ? regional : "zh-Hans"; break;
                case "Portuguese": code = regional == "pt-BR" ? "pt-BR" : "pt"; break;
                case "Norwegian": code = "nb"; break;
                case "English": code = "en"; break;
                case "French": code = "fr"; break;
                case "Italian": code = "it"; break;
                case "Korean": code = "ko"; break;
                case "Thai": code = "th"; break;
                case "German": code = "de"; break;
                case "Spanish": code = "es"; break;
                case "Russian": code = "ru"; break;
                case "Ukrainian": code = "uk"; break;
                case "Polish": code = "pl"; break;
                case "Dutch": code = "nl"; break;
                case "Swedish": code = "sv"; break;
                case "Danish": code = "da"; break;
                case "Finnish": code = "fi"; break;
                case "Czech": code = "cs"; break;
                case "Turkish": code = "tr"; break;
                case "Indonesian": code = "id"; break;
                case "Vietnamese": code = "vi"; break;
                case "Hindi": code = "hi"; break;
                case "Arabic": code = "ar"; break;
                case "Hebrew": code = "he"; break;
                default: code = regional; break;
            }
            return IsSupported(code) ? code : "en";
        }
        internal static void Select(string code)
        {
            if (!IsSupported(code)) throw new ArgumentException("Unsupported language", nameof(code));
            EditorPrefs.SetString(PreferenceKey, code);
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
        // Exact UI lookup never parses user content as a diagnostic template.
        internal static string TranslateExact(string language, string source)
        {
            if (string.IsNullOrEmpty(source)) return source;
            return TryExact(language, source, out var value) ? value : source;
        }
        private static bool TryExact(string language, string source, out string value)
        {
            var trimmed = source.Trim();
            var known = Aliases.TryGetValue(trimmed, out var canonical);
            if (!known) canonical = trimmed;
            value = null;
            bool translated = Catalogs.TryGetValue(language, out var catalog) && catalog.TryGetValue(canonical, out value);
            if (!translated) value = canonical;
            if (!known && !translated && !TranslationData.Canonical.Contains(canonical))
            {
                // Product packages keep their own catalogs; only the package assembly sets this.
                var external = External?.Invoke(language, trimmed);
                if (external == null) return false;
                value = external;
            }
            int start = source.IndexOf(trimmed, StringComparison.Ordinal);
            value = start == 0 && trimmed.Length == source.Length ? value : source.Substring(0, start) + value + source.Substring(start + trimmed.Length);
            return true;
        }
        private const int CacheCapacity = 1024, MaxCachedLength = 4096;
        private static readonly Dictionary<(string language, string source), string> MessageCache = new Dictionary<(string, string), string>();
        internal static string Translate(string language, string source)
        {
            if (string.IsNullOrEmpty(source)) return source;
            if (TryExact(language, source, out var exact)) return exact;
            var key = (language, source);
            if (MessageCache.TryGetValue(key, out var cached)) return cached;
            var result = TranslateMessage(language, source);
            if (source.Length <= MaxCachedLength && result.Length <= MaxCachedLength)
            {
                if (MessageCache.Count >= CacheCapacity) MessageCache.Clear();
                MessageCache[key] = result;
            }
            return result;
        }
        private static string TranslateMessage(string language, string source)
        {
            if (string.IsNullOrEmpty(source)) return source;
            var codePrefix = Regex.Match(source, @"^\[(?:PosingSystem / )?PS\d{3}\]\s+");
            if (codePrefix.Success) return codePrefix.Value + Translate(language, source.Substring(codePrefix.Length));
            var trimmed = source.Trim();
            if (trimmed.Length == 0) return source;
            var canonical = Aliases.TryGetValue(trimmed, out var english) ? english : trimmed;
            var value = Catalogs.TryGetValue(language, out var catalog) && catalog.TryGetValue(canonical, out var translated)
                ? translated : canonical;
            var start = source.IndexOf(trimmed, StringComparison.Ordinal);
            if (value == canonical && canonical == trimmed && TryFormatted(language, trimmed, out var formatted))
                return source.Substring(0, start) + formatted + source.Substring(start + trimmed.Length);
            if (value == canonical && canonical == trimmed)
            {
                var colon = trimmed.IndexOf(':');
                if (colon > 0)
                {
                    var left = trimmed.Substring(0, colon).Trim();
                    var right = trimmed.Substring(colon + 1);
                    var meaning = Aliases.TryGetValue(left, out var aliasedLeft) ? aliasedLeft : left;
                    if (meaning == "Storage")
                        return source.Substring(0, start) + Translate(language, left) + ":" + right + source.Substring(start + trimmed.Length);
                    if (left == "KawaiiPosing" || left == "CoolPosing" || left == "PosingSystem")
                        return source.Substring(0, start) + left + ":" + Translate(language, right) + source.Substring(start + trimmed.Length);
                }
            }
            if (value == canonical && canonical == trimmed)
            {
                foreach (var prefix in DiagnosticData.Prefixes)
                {
                        if (!prefix.EndsWith(":", StringComparison.Ordinal) || trimmed.Length <= prefix.Length || !trimmed.StartsWith(prefix, StringComparison.Ordinal)) continue;
                        // Translate the body only when it is an exact known sentence (for example the
                        // action after "Action:"); free text such as exception details stays untouched.
                        var rest = trimmed.Substring(prefix.Length);
                        if (TryExact(language, rest, out var exactRest)) rest = exactRest;
                        return source.Substring(0, start) + Translate(language, prefix) + rest + source.Substring(start + trimmed.Length);
                }
            }
            if (value == canonical && canonical == trimmed && source.IndexOf('\n') >= 0)
            {
                var lines = source.Split('\n');
                for (var i = 0; i < lines.Length; i++) lines[i] = Translate(language, lines[i]);
                return string.Join("\n", lines);
            }
            return source.Substring(0, start) + value + source.Substring(start + trimmed.Length);
        }
        internal static string T(string source) => Translate(Current, source);
        internal static string Format(string source, params object[] arguments)
        {
            // An imperfect translation must not replace the real message with a format error.
            try { return string.Format(T(source), arguments); }
            catch (FormatException) { return string.Format(source, arguments); }
        }
        internal static void Progress(string title, string message, float progress)
        {
            Debug.Log("[VPAI] " + message); // Preserve detailed stage text for diagnostics.
            EditorUtility.DisplayProgressBar(T(title), T(message), progress);
        }
        internal static string StorageNotice
        {
            get
            {
#if UNITY_EDITOR_WIN
                return T("Storage") + ": HKEY_CURRENT_USER\\Software\\UnisakiStudio";
#elif UNITY_EDITOR_OSX
                return T("Storage") + ": " + System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "UnisakiStudio");
#elif UNITY_EDITOR_LINUX
                return T("Storage") + ": " + System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share", "UnisakiStudio");
#else
                return T("Storage") + ": " + T("Current user");
#endif
            }
        }
        internal static bool Dialog(string title, string message, string ok, string cancel = "")
            => Dialog(title, () => T(message), ok, cancel);
        internal static bool CanOfferLanguage(string title, string language, bool hasSelection)
        {
            if (hasSelection) return false;
            var caption = Translate(language, title);
            return caption == Translate(language, "Confirm") || caption == Translate(language, "License installation");
        }
        internal static bool Dialog(string title, Func<string> message, string ok, string cancel = "")
        {
            // Offer language only on the first setup confirmation. Completion and
            // error dialogs always use ordinary buttons; Inspector handles later changes.
            while (true)
            {
                if (!CanOfferLanguage(title, Current, HasSelection))
                    return string.IsNullOrEmpty(cancel)
                        ? EditorUtility.DisplayDialog(T(title), message(), T(ok))
                        : EditorUtility.DisplayDialog(T(title), message(), T(ok), T(cancel));
                var choice = EditorUtility.DisplayDialogComplex(T(title), message(), T(ok),
                    string.IsNullOrEmpty(cancel) ? T("Close") : T(cancel), T("Language..."));
                if (choice == 2) { Choose(); continue; }
                if (choice == 0 && !HasSelection) Select(Current); // Accept the detected PC language once.
                return choice == 0;
            }
        }
        internal static void Choose()
        {
            var window = ScriptableObject.CreateInstance<LanguagePicker>();
            window.titleContent = new GUIContent("Language / 言語");
            window.minSize = new Vector2(380, 130);
            window.maxSize = new Vector2(700, 220);
            window.ShowModal();
        }
        private sealed class LanguagePicker : EditorWindow
        {
            private int selection;
            private void OnEnable() { selection = Array.IndexOf(Codes, Current); }
            private void OnDisable() { BootstrapCaptionImages.Reset(); }
            private void OnGUI()
            {
                EditorGUILayout.Space(8);
                selection = BootstrapCaptionImages.Popup("Language / 言語", selection, Names, Codes[selection]);
                var language = Codes[selection];
                var remembered = Translate(language, "This language is remembered for other Unity projects on this computer.");
                var apply = Translate(language, "Apply");
                var requiredHeight = BootstrapCaptionImages.RequiredHeight(language, remembered, apply);
                // The offline generation gate rejects any caption that needs more than 220pt.
                minSize = new Vector2(380, Mathf.Min(220, requiredHeight));
                BootstrapCaptionImages.HelpBox(language, remembered);
                if (BootstrapCaptionImages.Button(language, apply)) { Select(Codes[selection]); Close(); }
            }
        }
    }
}

namespace UnisakiStudio.Setup
{
    // Unity 2022.3 exposes dynamic registration internally. Keep reflection contained,
    // retain static attribute menus as a fallback, and never execute an action on registration.
    [InitializeOnLoad]
    internal static class LocalizedMenus
    {
        private const string SessionKey = "jp.unisakistudio.posingsystem.localized-menu-paths";
        private static readonly BindingFlags Flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.DeclaredOnly;
        private static readonly MethodInfo Add = typeof(Menu).GetMethods(Flags).FirstOrDefault(m => m.Name == "AddMenuItem" && m.GetParameters().Length == 6);
        private static readonly MethodInfo Remove = typeof(Menu).GetMethods(Flags).FirstOrDefault(m => m.Name == "RemoveMenuItem" && m.GetParameters().Length == 1);
        private static readonly MethodInfo Exists = typeof(Menu).GetMethods(Flags).FirstOrDefault(m => m.Name == "MenuItemExists" && m.GetParameters().Length == 1);
        private static readonly MethodInfo Extract = typeof(Menu).GetMethods(Flags).FirstOrDefault(m => m.Name == "ExtractSubmenus" && m.GetParameters().Length == 1);
        private static readonly string Owner = typeof(Language).Assembly.FullName.Split(',')[0];
        private static string language;
        private static double nextCheck;
        static LocalizedMenus()
        {
            EditorApplication.delayCall += Rebuild;
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += Cleanup;
            EditorApplication.quitting += Cleanup;
        }
        private static bool IsOwner()
        {
            var core = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.FullName.Split(',')[0] == "jp.unisakistudio.posingsystemeditor");
            return core != null ? core == typeof(Language).Assembly : Owner == "Assembly-CSharp-Editor";
        }
        private static void Cleanup()
        {
            if (SessionState.GetString(SessionKey + ".owner", "") != Owner) return;
            if (Remove == null || Extract == null) return;
            RemoveRegistered(ReadPaths(), null, null, null);
            SessionState.SetString(SessionKey, "");
        }
        [Serializable] private sealed class PathList { public string[] paths; }
        private static string[] ReadPaths()
        {
            var saved = SessionState.GetString(SessionKey, "");
            if (saved.TrimStart().StartsWith("{", StringComparison.Ordinal))
            {
                try { return JsonUtility.FromJson<PathList>(saved)?.paths ?? Array.Empty<string>(); }
                catch (ArgumentException) { return Array.Empty<string>(); }
            }
            // Upgrade fallback. Old multiline captions cannot be reconstructed
            // from this list; Rebuild also sweeps known canonical menu commands.
            return saved.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
        private static void RemoveRegistered(IEnumerable<string> savedPaths, HashSet<string> knownPaths, HashSet<string> canonicalPaths, IEnumerable<string> sources)
        {
            var saved = new HashSet<string>(savedPaths, StringComparer.Ordinal);
            var roots = saved.Where(p => p.Contains("/")).Select(p => p.Split('/')[0]);
            var prefixes = new HashSet<string>(StringComparer.Ordinal);
            if (sources != null)
                foreach (var source in sources)
                {
                    var parts = source.Split('/');
                    var length = parts.Length > 2 && parts[0] == "Assets" && parts[1] == "Create" ? 3 : 2;
                    var studioRoot = string.Join("/", parts.Take(length));
                    foreach (var code in Language.Codes) prefixes.Add(PathFor(studioRoot, code) + "/");
                    roots = roots.Concat(new[] { parts[0] });
                }
            // Do not feed every unrelated Unity/plugin command through the
            // translation parser. Only our studio submenus can be migration
            // candidates; exact saved paths remain removable independently.
            var actual = roots.Distinct().SelectMany(root => (string[])Extract.Invoke(null, new object[] { root })).Distinct()
                .Where(path => saved.Contains(path) || prefixes.Any(prefix => path.StartsWith(prefix, StringComparison.Ordinal))).ToArray();
            foreach (var path in actual)
                if (saved.Contains(path) || knownPaths != null && knownPaths.Contains(path) || canonicalPaths != null && canonicalPaths.Contains(PathFor(path, "en")))
                    // Unity's Exists check can return false for paths containing
                    // trailing whitespace. The actual native inventory is the
                    // authority; remove its exact path without an Exists gate.
                    Remove.Invoke(null, new object[] { path });
        }
        private static string MenuPart(string value) => Regex.Replace(value, @"\s+", " ").Trim();
        private static void Update()
        {
            if (EditorApplication.timeSinceStartup < nextCheck) return;
            nextCheck = EditorApplication.timeSinceStartup + .5;
            if (!IsOwner()) return;
            if (language != Language.Current || SessionState.GetString(SessionKey + ".owner", "") != Owner) Rebuild();
        }
        internal static string PathFor(string source, string locale)
        {
            var parts = source.Split('/');
            var first = parts.Length > 1 && parts[0] == "Assets" && parts[1] == "Create" ? 2 : 1;
            for (var i = first; i < parts.Length; i++)
            {
                // Translate first, so legacy multiline captions can resolve via
                // migration aliases, then keep the native menu path single-line.
                parts[i] = MenuPart(locale == "ja" ? parts[i] : Language.Translate(locale, parts[i]));
            }
            return string.Join("/", parts);
        }
        private sealed class Entry
        {
            internal MethodInfo Execute, Validate;
            internal Action CreateAsset;
            internal bool HasAction => Execute != null || CreateAsset != null;
            internal int Priority = 1000;
        }
        internal static void Rebuild() => Rebuild(Language.Current);
        internal static void Rebuild(string locale)
        {
            if (!IsOwner()) return;
            language = locale;
            if (Add == null || Remove == null || Exists == null || Extract == null) return;
            var entries = new Dictionary<string, Entry>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                var name = assembly.FullName.Split(',')[0];
                if (!name.StartsWith("jp.unisakistudio.", StringComparison.Ordinal) && name != "Assembly-CSharp-Editor") continue;
                Type[] types;
                try { types = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException exception) { types = exception.Types.Where(t => t != null).ToArray(); }
                foreach (var type in types)
                {
                    if (typeof(ScriptableObject).IsAssignableFrom(type))
                        foreach (var attribute in type.GetCustomAttributesData())
                        {
                            if (attribute.AttributeType != typeof(CreateAssetMenuAttribute)) continue;
                            var menuName = attribute.NamedArguments.FirstOrDefault(a => a.MemberName == "menuName").TypedValue.Value as string;
                            if (menuName == null || !menuName.Contains("ゆにさきスタジオ/")) continue;
                            var filename = attribute.NamedArguments.FirstOrDefault(a => a.MemberName == "fileName").TypedValue.Value as string ?? type.Name;
                            if (!filename.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)) filename += ".asset";
                            var capturedType = type; var capturedFilename = filename;
                            var order = attribute.NamedArguments.FirstOrDefault(a => a.MemberName == "order").TypedValue.Value;
                            entries["Assets/Create/" + menuName] = new Entry
                            {
                                Priority = order is int priority ? priority : 1000,
                                CreateAsset = () => ProjectWindowUtil.CreateAsset(ScriptableObject.CreateInstance(capturedType), capturedFilename)
                            };
                        }
                    foreach (var method in type.GetMethods(Flags))
                        foreach (var attribute in method.GetCustomAttributesData())
                        {
                            if (attribute.AttributeType != typeof(MenuItem) || attribute.ConstructorArguments.Count == 0) continue;
                            var path = attribute.ConstructorArguments[0].Value as string;
                            if (path == null || !path.Contains("/ゆにさきスタジオ/")) continue;
                            if (!entries.TryGetValue(path, out var entry)) entries[path] = entry = new Entry();
                            var validate = attribute.ConstructorArguments.Count > 1 && (bool)attribute.ConstructorArguments[1].Value;
                            if (validate) entry.Validate = method;
                            else
                            {
                                entry.Execute = method;
                                if (attribute.ConstructorArguments.Count > 2) entry.Priority = (int)attribute.ConstructorArguments[2].Value;
                            }
                        }
                }
            }
            var sources = entries.Where(e => e.Value.HasAction).Select(e => e.Key).ToArray();
            var canonical = new HashSet<string>(sources.Select(source => PathFor(source, "en")), StringComparer.Ordinal);
            var known = new HashSet<string>(sources.SelectMany(source => Language.Codes.Select(code => PathFor(source, code))), StringComparer.Ordinal);
            // The native inventory sweep also recognizes captions left by a
            // previous version whose newline-delimited SessionState was broken.
            RemoveRegistered(ReadPaths(), known, canonical, sources);
            var installed = new List<string>();
            foreach (var pair in entries.Where(e => e.Value.HasAction))
            {
                var entry = pair.Value;
                var path = PathFor(pair.Key, language);
                if (installed.Contains(path)) continue;
                Action execute = entry.CreateAsset ?? (() => Invoke(entry.Execute));
                Func<bool> validate = entry.Validate == null ? (Func<bool>)(() => true) : () => (bool)Invoke(entry.Validate);
                Add.Invoke(null, new object[] { path, "", false, entry.Priority, execute, validate });
                installed.Add(path);
            }
            // A JSON array preserves each path as one item even if future caption
            // sources contain control characters before native normalization.
            SessionState.SetString(SessionKey, JsonUtility.ToJson(new PathList { paths = installed.ToArray() }));
            SessionState.SetString(SessionKey + ".owner", Owner);
        }
        private static object Invoke(MethodInfo method)
        {
            try { return method.Invoke(null, method.GetParameters().Length == 0 ? null : new object[] { new MenuCommand(Selection.activeObject) }); }
            catch (TargetInvocationException exception)
            {
                System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(exception.InnerException ?? exception).Throw();
                throw;
            }
        }
    }
}

// BEGIN REVIEWED BOOTSTRAP CAPTION OVERLAY
/*
Standalone language-picker caption artwork

The seven fixed complex-script caption sets were rendered offline using Noto Sans,
Noto Sans Arabic, Noto Sans Hebrew, Noto Sans Devanagari, Noto Sans Bengali,
Noto Sans Tamil, Noto Sans Telugu and Noto Sans Thai. Only the resulting fixed PNG
artwork is embedded in this standalone helper; font software and the offline
Skia/HarfBuzz/RichTextKit rendering libraries are not embedded in the installer.
Font attribution and the unmodified font license text are retained below.
This notice does not change the product agreements or upstream installer licenses.

Pinned font inputs used for this artwork:

NotoSans-Regular.ttf
SHA256: b85c38ecea8a7cfb39c24e395a4007474fa5a4fc864f6ee33309eb4948d232d5
Source: https://raw.githubusercontent.com/notofonts/noto-fonts/ffebf8c1ee449e544955a7e813c54f9b73848eac/hinted/ttf/NotoSans/NotoSans-Regular.ttf

NotoSansArabic-Regular.ttf
SHA256: ceea25b464a656dc3b26849bab9356740401af62aedf1bfa8b7f0d9b75925b1b
Source: https://raw.githubusercontent.com/notofonts/noto-fonts/ffebf8c1ee449e544955a7e813c54f9b73848eac/hinted/ttf/NotoSansArabic/NotoSansArabic-Regular.ttf

NotoSansHebrew-Regular.ttf
SHA256: a7fa16fffb27bedb060a0866267c29e9859aeb9c21cc33f5b3aaf6eb062eca85
Source: https://raw.githubusercontent.com/notofonts/noto-fonts/ffebf8c1ee449e544955a7e813c54f9b73848eac/hinted/ttf/NotoSansHebrew/NotoSansHebrew-Regular.ttf

NotoSansDevanagari-Regular.ttf
SHA256: 385e78e6359a9d88a0f243d53b1209d7548361ba2194e2b9ec779bcaa7e8949d
Source: https://raw.githubusercontent.com/notofonts/noto-fonts/ffebf8c1ee449e544955a7e813c54f9b73848eac/hinted/ttf/NotoSansDevanagari/NotoSansDevanagari-Regular.ttf

NotoSansBengali-Regular.ttf
SHA256: 6300c5370cd688b0641343de4c786de6d412bb6c578d129dae75e93a0322dcab
Source: https://raw.githubusercontent.com/notofonts/noto-fonts/ffebf8c1ee449e544955a7e813c54f9b73848eac/hinted/ttf/NotoSansBengali/NotoSansBengali-Regular.ttf

NotoSansTamil-Regular.ttf
SHA256: 6532db33b8b264abe3a098a40619feb489b5ddf5ab1d2b46e72b51eeb548001b
Source: https://raw.githubusercontent.com/notofonts/noto-fonts/ffebf8c1ee449e544955a7e813c54f9b73848eac/hinted/ttf/NotoSansTamil/NotoSansTamil-Regular.ttf

NotoSansTelugu-Regular.ttf
SHA256: 2c05072e8018a9be1cb0582953d9edf9a0cf129cdfb74e611de763b09c411f7f
Source: https://raw.githubusercontent.com/notofonts/noto-fonts/ffebf8c1ee449e544955a7e813c54f9b73848eac/hinted/ttf/NotoSansTelugu/NotoSansTelugu-Regular.ttf

NotoSansThai-Regular.ttf
SHA256: 404ddfb5ed0aaa6b6ec8a85700d682978992062d67da93903967b56cbd9a4acc
Source: https://raw.githubusercontent.com/notofonts/noto-fonts/ffebf8c1ee449e544955a7e813c54f9b73848eac/hinted/ttf/NotoSansThai/NotoSansThai-Regular.ttf

Copyright 2018 The Noto Project Authors (github.com/googlei18n/noto-fonts)

This Font Software is licensed under the SIL Open Font License,
Version 1.1.

This license is copied below, and is also available with a FAQ at:
http://scripts.sil.org/OFL

-----------------------------------------------------------
SIL OPEN FONT LICENSE Version 1.1 - 26 February 2007
-----------------------------------------------------------

PREAMBLE
The goals of the Open Font License (OFL) are to stimulate worldwide
development of collaborative font projects, to support the font
creation efforts of academic and linguistic communities, and to
provide a free and open framework in which fonts may be shared and
improved in partnership with others.

The OFL allows the licensed fonts to be used, studied, modified and
redistributed freely as long as they are not sold by themselves. The
fonts, including any derivative works, can be bundled, embedded,
redistributed and/or sold with any software provided that any reserved
names are not used by derivative works. The fonts and derivatives,
however, cannot be released under any other type of license. The
requirement for fonts to remain under this license does not apply to
any document created using the fonts or their derivatives.

DEFINITIONS
"Font Software" refers to the set of files released by the Copyright
Holder(s) under this license and clearly marked as such. This may
include source files, build scripts and documentation.

"Reserved Font Name" refers to any names specified as such after the
copyright statement(s).

"Original Version" refers to the collection of Font Software
components as distributed by the Copyright Holder(s).

"Modified Version" refers to any derivative made by adding to,
deleting, or substituting -- in part or in whole -- any of the
components of the Original Version, by changing formats or by porting
the Font Software to a new environment.

"Author" refers to any designer, engineer, programmer, technical
writer or other person who contributed to the Font Software.

PERMISSION & CONDITIONS
Permission is hereby granted, free of charge, to any person obtaining
a copy of the Font Software, to use, study, copy, merge, embed,
modify, redistribute, and sell modified and unmodified copies of the
Font Software, subject to the following conditions:

1) Neither the Font Software nor any of its individual components, in
Original or Modified Versions, may be sold by itself.

2) Original or Modified Versions of the Font Software may be bundled,
redistributed and/or sold with any software, provided that each copy
contains the above copyright notice and this license. These can be
included either as stand-alone text files, human-readable headers or
in the appropriate machine-readable metadata fields within text or
binary files as long as those fields can be easily viewed by the user.

3) No Modified Version of the Font Software may use the Reserved Font
Name(s) unless explicit written permission is granted by the
corresponding Copyright Holder. This restriction only applies to the
primary font name as presented to the users.

4) The name(s) of the Copyright Holder(s) or the Author(s) of the Font
Software shall not be used to promote, endorse or advertise any
Modified Version, except to acknowledge the contribution(s) of the
Copyright Holder(s) and the Author(s) or with their explicit written
permission.

5) The Font Software, modified or unmodified, in part or in whole,
must be distributed entirely under this license, and must not be
distributed under any other license. The requirement for fonts to
remain under this license does not apply to any document created using
the Font Software.

TERMINATION
This license becomes null and void if any of the above conditions are
not met.

DISCLAIMER
THE FONT SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO ANY WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT
OF COPYRIGHT, PATENT, TRADEMARK, OR OTHER RIGHT. IN NO EVENT SHALL THE
COPYRIGHT HOLDER BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
INCLUDING ANY GENERAL, SPECIAL, INDIRECT, INCIDENTAL, OR CONSEQUENTIAL
DAMAGES, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF THE USE OR INABILITY TO USE THE FONT SOFTWARE OR FROM
OTHER DEALINGS IN THE FONT SOFTWARE.

*/
namespace UnisakiStudio.Setup
{
    internal static partial class BootstrapCaptionImages
    {
        private sealed class Caption
        {
            internal readonly string Text, TextSha, PngSha, Base64;
            internal readonly float Width, Height;
            internal Texture2D Texture;
            internal Caption(string text, string textSha, string pngSha, float width, float height, string base64)
            { Text = text; TextSha = textSha; PngSha = pngSha; Width = width; Height = height; Base64 = base64; }
        }
        private static readonly HashSet<string> warned = new HashSet<string>();
        private static int buttonPointer, buttonKeyboard;
        internal static int TexturesLoaded { get; private set; }
        internal static string LastFailure { get; private set; }

        static BootstrapCaptionImages()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Reset;
            EditorApplication.quitting += Reset;
        }

        private static bool Complex(string language) => language == "ar" || language == "he" || language == "hi" ||
            language == "bn" || language == "ta" || language == "te" || language == "th";

        private static string Sha(byte[] value)
        { using (var sha = System.Security.Cryptography.SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(value)).Replace("-", "").ToLowerInvariant(); }

        private static bool Get(string language, string role, string logical, out Caption caption)
        {
            caption = null;
            if (!Complex(language)) return false;
            if (Captions.TryGetValue(language + "/" + role, out var found) && found.Text == logical)
            { caption = found; return true; }
            Warn(language + "/" + role, "Fixed caption differs from the current logical translation; native fallback remains visible.");
            return false;
        }

        private static void Warn(string key, string message)
        {
            LastFailure = message;
            if (warned.Add(key)) Debug.LogWarning("USSPS_BOOTSTRAP_CAPTION: " + key + ": " + message);
        }

        private static bool Prepare(Caption caption)
        {
            if (caption.Texture != null) return true;
            try
            {
                var bytes = Convert.FromBase64String(caption.Base64);
                if (Sha(System.Text.Encoding.UTF8.GetBytes(caption.Text)) != caption.TextSha || Sha(bytes) != caption.PngSha)
                    throw new InvalidOperationException("Caption/pixel SHA mismatch");
                var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false) {
                    hideFlags = HideFlags.HideAndDontSave, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                try
                {
                    if (!ImageConversion.LoadImage(texture, bytes, true) || texture.width != Mathf.RoundToInt(caption.Width * 2) ||
                        texture.height != Mathf.RoundToInt(caption.Height * 2)) throw new InvalidOperationException("Caption PNG dimensions differ");
                    caption.Texture = texture; TexturesLoaded++; return true;
                }
                catch { UnityEngine.Object.DestroyImmediate(texture); throw; }
            }
            catch (Exception e) { Warn(caption.TextSha, e.Message); return false; }
        }

        private static bool Overlay(string language, string role, string logical, out Caption caption)
        {
            if (!Get(language, role, logical, out caption) || Event.current.type != EventType.Repaint) return false;
            return Prepare(caption); // Never hide native ink unless the overlay is ready.
        }

        private static GUIStyleState[] States(GUIStyle style) => new[] { style.normal, style.hover, style.active, style.focused,
            style.onNormal, style.onHover, style.onActive, style.onFocused };

        private sealed class Ink : IDisposable
        {
            private readonly GUIStyle style;
            private readonly Color[] saved;
            internal Ink(GUIStyle original, bool hide)
            {
                if (!hide || Event.current.type != EventType.Repaint) return;
                style = original; var states = States(style); saved = new Color[states.Length];
                for (int i = 0; i < states.Length; i++) { saved[i] = states[i].textColor; states[i].textColor = Color.clear; }
            }
            public void Dispose()
            {
                if (style == null) return;
                var states = States(style); for (int i = 0; i < states.Length; i++) states[i].textColor = saved[i];
            }
        }

        private static void Draw(Caption caption, Rect available, Color color, bool centered)
        {
            if (Event.current.type != EventType.Repaint || caption?.Texture == null) return;
            var destination = new Rect(available.x + (centered ? (available.width - caption.Width) * .5f : 0),
                available.y + (available.height - caption.Height) * .5f, caption.Width, caption.Height);
            var clipped = Rect.MinMaxRect(Mathf.Max(destination.xMin, available.xMin), Mathf.Max(destination.yMin, available.yMin),
                Mathf.Min(destination.xMax, available.xMax), Mathf.Min(destination.yMax, available.yMax));
            if (clipped.width <= 0 || clipped.height <= 0) return;
            var uv = new Rect((clipped.x - destination.x) / destination.width,
                1 - (clipped.yMax - destination.y) / destination.height,
                clipped.width / destination.width, clipped.height / destination.height);
            var oldColor = GUI.color; var oldContent = GUI.contentColor; var enabled = GUI.enabled;
            var tint = oldColor * oldContent * color;
            if (!enabled) tint.a *= .55f;
            try
            {
                GUI.color = tint; GUI.contentColor = Color.white; GUI.enabled = true;
                GUI.DrawTextureWithTexCoords(clipped, caption.Texture, uv, true);
            }
            finally { GUI.color = oldColor; GUI.contentColor = oldContent; GUI.enabled = enabled; }
        }

        internal static float RequiredHeight(string language, string message, string apply)
        {
            float help = Get(language, "help", message, out var h) ? Mathf.Max(38, h.Height + 12) : 48;
            float button = Get(language, "apply", apply, out var a) ? Mathf.Max(24, a.Height + 6) : 24;
            return Mathf.Max(130, 8 + 20 + 4 + help + 4 + button + 12);
        }

        internal static int Popup(string label, int selected, string[] names, string language)
        {
            var logical = names[selected];
            bool overlay = Overlay(language, "autonym", logical, out var caption);
            var style = EditorStyles.popup;
            int result;
            using (new Ink(style, overlay))
                result = EditorGUILayout.Popup(label, selected, names, style); // Original logical options, exactly once.
            if (overlay)
            {
                var rect = GUILayoutUtility.GetLastRect(); rect.xMin += EditorGUIUtility.labelWidth; rect.xMax -= 16;
                Draw(caption, rect, style.normal.textColor, false);
            }
            return result;
        }

        internal static void HelpBox(string language, string logical)
        {
            if (!Get(language, "help", logical, out var caption)) { EditorGUILayout.HelpBox(logical, MessageType.Info); return; }
            var rect = GUILayoutUtility.GetRect(0, Mathf.Max(38, caption.Height + 12), GUILayout.ExpandWidth(true));
            bool overlay = Overlay(language, "help", logical, out caption);
            var style = EditorStyles.helpBox;
            using (new Ink(style, overlay)) EditorGUI.HelpBox(rect, logical, MessageType.Info);
            if (overlay)
            {
                var text = new Rect(rect.x + 40, rect.y + 4, Mathf.Min(304, rect.width - 44), rect.height - 8);
                Draw(caption, text, style.normal.textColor, false);
            }
        }

        internal static bool Button(string language, string logical)
        {
            if (!Get(language, "apply", logical, out var caption)) return GUILayout.Button(logical);
            bool overlay = Overlay(language, "apply", logical, out caption);
            var style = GUI.skin.button;
            int hot = GUIUtility.hotControl, keyboard = GUIUtility.keyboardControl;
            bool result;
            using (new Ink(style, overlay)) result = GUILayout.Button(logical, style, GUILayout.Height(Mathf.Max(24, caption.Height + 6)));
            if (GUIUtility.hotControl != 0 && GUIUtility.hotControl != hot) buttonPointer = GUIUtility.hotControl;
            if (GUIUtility.keyboardControl != 0 && GUIUtility.keyboardControl != keyboard) buttonKeyboard = GUIUtility.keyboardControl;
            if (overlay)
            {
                var rect = GUILayoutUtility.GetLastRect();
                bool hover = GUI.enabled && rect.Contains(Event.current.mousePosition);
                bool active = hover && buttonPointer != 0 && GUIUtility.hotControl == buttonPointer;
                bool focus = GUI.enabled && buttonKeyboard != 0 && GUIUtility.keyboardControl == buttonKeyboard;
                var state = active ? style.active : focus ? style.focused : hover ? style.hover : style.normal;
                Draw(caption, style.padding.Remove(rect), state.textColor, true);
            }
            return result;
        }

        internal static void Reset()
        {
            foreach (var caption in Captions.Values)
            {
                if (caption.Texture != null) UnityEngine.Object.DestroyImmediate(caption.Texture);
                caption.Texture = null;
            }
            TexturesLoaded = 0; buttonPointer = buttonKeyboard = 0;
        }
    }
}
namespace UnisakiStudio.Setup {
    internal static partial class BootstrapCaptionImages {
        private static readonly Dictionary<string, Caption> Captions = new Dictionary<string, Caption> {
            { "ar/autonym", new Caption("العربية", "d274159863057eb5c633116c3b54e4f8659167f0b73fabbee256e881fa1ac8c8", "e0e20b2b3eacbfac19b179479b4a92ac78308efc911c26d773843f1a269608ae", 35f, 29.5f, "iVBORw0KGgoAAAANSUhEUgAAAEYAAAA7CAYAAADGkvybAAAABHNCSVQICAgIfAhkiAAAAxdJREFUeJztmjFrFEEUx/8RQdBACiHapZFoGQsR1M7AfQRbYQvzDe6wspIsfoFrUq6Q8srApTOF1bVnt4qCOZDcdZtCxsL/4DCZt3uzd3u3kveDYbJvZ+a9++/MvNklG8YYKFe5se4A2ooKI6DCCKgwAiqMgAojoMIIqDACKoyACiOgwgioMAIqjIAKI6DCCKgwAiqMgAojUCXMGEABIAWQATAABiuKba3EzJhN1ncaiqUJ7MMcenbDInKzYuBHi8e2Vr7X7Xhd9pgHsR2uizDRxAiTcK1mDcbTGmKE2QXwEsDzBuNZFV+rGlRtvpY+gKeLx/P/UDVjMgBTAG8A7C3oq8PxRksYI3dSbsExO4H23YrxeuIdY4xURuYfY2PMkH8XJX2qSsExeiVtOoK9Z4yZGplc6GdxbTltqRSHFNyZ4yypcBJTrNhDQZCcP96/l1DUgn1dYVMnrpCotYQJLaUEwDMAMy6ho4ipngA4D5w0LZ9Zh84VJ6y3hMx3i8tmCODQsXedzfQk0K8WIWF2Wf+s4egJgG0Ad4X7B6zvC/ePWe979iMApxTtnbc3JAB25sk0MYSEuWD9kC+MScR4diZ8K2kz4dMP0eVM3Q743QfwkX2tOB0A73n/+OpwCzDHxhtCWpvnVWvXaVNnH4IxJnOSgN2Ms5LxlrbHAMBjPp2JY5uxAMBr74l2OLu2ef1KSIW2zZeSZ/WL9Qt+7nB9pADuAbjkzNliTJteW9cfvN8BAD9K/P8lMqv0KmaScWaERFGRrvOK/pZRSfr+7V0PPB/26BGdrstKynNN4TguaEuZMkNBT2lPSsZOnLaZN8aUPgbeGH0eL3InJitMzvu+HyuMtFyxYdr1z4ljbvqngcy0Utr02WFEUWYAPqw7mGUJM3C+B6c8iOVemzPa+5495aFwj5vq4TIPanWZ9+06hgvBfts7v6TMXju8ngB4G3nSbox17jF9vnJcAvi07j3Fp4kZMy8HXt0q2paVWkObslKrUGEEVBgBFUZAhRFQYQRUGAEVRkCFEVBhBFQYARVGQIURUGEEVBiBPwg/C0ygPyV5AAAAAElFTkSuQmCC") },
            { "ar/help", new Caption("سيتم تذكّر اللغة على هذا الجهاز.", "ecb87cfeb209e1de5d9f01bc18d07f64ae3fc6425ea55c1d2bf91d644e88eb02", "cca26d820b8eea25c2d689be6b5533237abc02ce5d564a9d1ecfffe2c12de645", 304.5f, 27.5f, "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") },
            { "ar/apply", new Caption("تطبيق", "268974da5082e980b65982616ccf50f93c0d77f9916c8cf9ab74302bfe50b8fe", "d022058b5eb6c7a87309754512f8d9d44cb16102cf17968ba2bacb82e2dbea30", 36f, 29.5f, "iVBORw0KGgoAAAANSUhEUgAAAEgAAAA7CAYAAADYW8woAAAABHNCSVQICAgIfAhkiAAAAv1JREFUeJztmrFrFEEUh7+IcJrGTguF1BbBQCqxVLguKS3SCFtoYS37J3j/wTU2wtkHq8C1aycRBElhcankhOAFUqzV2PxWls3OvvXWmNv4PljmsrPzZvbLzpu55dZCCDhxrl32AFYdF2TgggxckIELMnBBBi7IwAUZuCADF2TgggxckIELMnBBBi7IwAUZuCADF2Tgggz6JGgGBGD0Lzvtk6BLoU+CflxGp30SdKLy3pLtx0AOZMAQWABzfY7SVdBcHSXqONdALpI7HdoOgPWS5IHV4HqHzgrzNyoDWFVe6Ch406ZRF0EHlf9mqw77Rp9y0N9kDKRtLrwKgkbAoXJhUF6cNiTfDHgOPG0TvKugtLSBWwCTjvH+hCFwBLwCtoBbOn8beAy8jbR7r/J+q15CCMseaQghD+eZNLSZhBBmIYRxpH4YQpiHEKY1dSPFP6r0vVDcYem63BjLQvWpdZ9dBM0rg9jX33lDm8wYeCHhsKFuURKQReKMG+K0GcfvY9kplupRPgb2dG4X+K6lPvZ96a7Kz5H6bZUnkXo0lQbAO+BR5JpTlTcj9TOV5p4qJihRkNiNPlT5tXK+uPFtzjMENvT5dSTupsp5w5iRnL1I3RB4ps8fjTgmMUEvdTN1NwrwTeVmy+UyqUjJdK7Mvp5KgCc1cXdVfqiRk2rpzkpxTiNPaqL4aLVrJjL3ijmeNCTTugRtkZcSZIzZEnGrfbQhlp/MJJ0qwNxonKiTNgNa6NpE8Y9q2pVXt2lpEbDiznRMS+0TJeKq7Fx9m8m5SVCxWsyWWNmKAZnLZ1+OplVsw3oVUJMHNjT3Y0m4fxg5qNU8rWzO6jaBxZQalfZLdZvBTNfFNpIr8wQdqNzScj+pWXUSnS+2+wPgU+WVQh1nDXXrq/bKZC3Ef0i+D+y0jPNTUndbXNsrmgShp2QHeFDa5BUc6z3xF30xPIjE6DWWoP+eq/A+6EJxQQYuyMAFGbggAxdk4IIMXJCBCzJwQQYuyMAFGbggAxdk4IIMfgHSDAGETFS09QAAAABJRU5ErkJggg==") },
            { "he/autonym", new Caption("עברית", "9b1919717ec3013e338e9172d374770da80a1d7f6d5d7c00f790d618fc94c0c3", "519357fdb724cad362ecb6529cd133852b69652bd12081c2334de78bd5588dee", 36.5f, 20.5f, "iVBORw0KGgoAAAANSUhEUgAAAEkAAAApCAYAAAB5hwSGAAAABHNCSVQICAgIfAhkiAAAAk1JREFUaIHtmjFrFFEQx38RQa5KLIxFCg0ErA6ttbTY2tZAmhT6DZLSztx3UAshYCOSwiKQVqwkhSR4ILiBCEkhukXIXfUsnIfL8t7buduNwb35wTLvdobZt//bnTfvuDnnHEaaK5c9gf8BE0mBiaTARFJgIikwkRSYSApMJAUmkgITSYGJpMBEUmAiKTCRFMyCSFnTBDGR9gCnOH5JbHUio0rcoOQ7TeTaD+Raj8SPgC/AZuQePkjcVuL+/X3mKZGuRs6fAUfAAjAPjIGTSoz3PQT6wM2Sz8feCuQ+l9yhXPeA18AasCu+G4H4HrAI3AGeA9eBjUpMDtwHllMCqHDOpY4994c84h+4v2wH/J5BzXV8rpHE7yjiM+fcaWJ+66XrZ1PeH865xjVpAxjK+HYLuT7J+K4ifhd4J+PQE/sSKGT8KJKjJ/Zn6kJtFO5jsUst5DqfML6o8X8T24/4/Zx/pJJ0fXU7FLvSJEnXRfosdjHSCiyI/ZpKctEiHcmRfOcvkC1ZmQnUpUxWVKZtAdqiaTFvg1xahWpdWhNb1PRSnX/dKK2+1YXlgdiDugSzINJHseU2YVs+j4FXdQnqRPKrwmj6OV46vi6NZYuzDzwW31vpp5LEalJe2ioA7LQ354lJFdVeJe576TVC9nV9+ZLngRdyvgDeAE81EwiJlJUezQJ4H9gX/UtC3XQszj/xm8Az4FolZihd/eokE5jr6L9KBsAT2WgP5deCqb/ororUKrOwujXGRFJgIikwkRSYSApMJAUmkgITSYGJpOA32Mlgn0o18hYAAAAASUVORK5CYII=") },
            { "he/help", new Caption("השפה תישמר במחשב זה.", "c788364f5dc693be8f3931b7ab290379e9a78800690aae35720f1976a94a8b5b", "94b32f8c05178401b2b317725429a29e45456956b94abe8bd9ca68fa5108abec", 304.5f, 19f, "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") },
            { "he/apply", new Caption("החלה", "c12e0a7c858002e1fe422d0d69f9099a8c7186e5b9b0c3ff65144f3da3145a9d", "eeb88ee5e69678429375070f759dca12d31ec29dd9a00a2c2c49540e4d58e1de", 34f, 20.5f, "iVBORw0KGgoAAAANSUhEUgAAAEQAAAApCAYAAACMeY82AAAABHNCSVQICAgIfAhkiAAAAbVJREFUaIHtmbFKA0EQQF/EQiuTRss0NiKCheAHKFwlfoSFv3C/kD9ILfgBlkLaFBYSywgWZ2mlVtqNhRNyGdzLJdmsCcyDZY+9md29x80dlzREBGfMxn9vYNVwIQYXYnAhBhdicCEGF2JwIQYXYnAhBhdiiC0kBwpgbT+QYgtpAe3IcybFS8bgQgwhIT19DlS1AuhWzD0tv69xnRqxArzpvrI51voABoHcCTYD49vAa0VeW9s1sA+c6/j7lDxKz5hC+x3Naerxp15AmSawC5wBR8Be6Vw+Zc3RvMfAHXAJ3AejRWTeNpBfvkUkq5lzJWNsTqHjeSD3tpTbnXGvue5TRKRXFbuIkKy0wU7NnJ7GD/84N6Iqv6hzUYHW19yiKm6Rh2r4tgtzov2jGe/UzH+ZY80RX3WCUr5lcq1lgJuE685ESEheepPE4kL75znvriqGut+6d1qQkJDWohP/waH2tlxisBVrolQlsxblQkIhyyyXqKQSssxyiUoKId11KRcSCTnV/mnVy4UEQjLgQI8flrxWFBr+7/8k/nuIwYUYXIjBhRhciMGFGFyIwYUYXIjBhRh+AB97A6XGFTHKAAAAAElFTkSuQmCC") },
            { "hi/autonym", new Caption("हिन्दी", "9ddb12af8457d635ec15773f905ca8587772ef64d5ecbe24d9aa4eab27a2e79b", "6fdd9a97ea1daa4e1b61a3a632a72511b9ce602714da1196f3ad2d41f1f2cd06", 28.5f, 20f, "iVBORw0KGgoAAAANSUhEUgAAADkAAAAoCAYAAABEklK7AAAABHNCSVQICAgIfAhkiAAAAthJREFUaIHtmbFvEzEUxn9FSBSQKAiEGJC6sbCUPwKywVgEYwa6MfbGSgyQbkgMWZBYAiPKWClrsqEMLAdTKlFEytDAQjKZ5Vlxrvbl7HMCHPmkJ1/su/f83Xt+fr6sKaWoOs786QksAyuSVcE8ki1gAChDBkAXqC9wXgmQGjZHQAeoBWlTSrmkr6YYKqUGImOjP1VK1XJ0hEjdsDESmyNjHt72XAMtUTpWSjUs4w0xru9JIpLsGy+wSH8wyVQUtuYoaAnJmET1y7O93Lwxp7jW5Lq0X+ZE+2NgT673ZC0tEs+k3fZ5yEXyRNqNAjpeAC+Bc0IyLDlMcZRj+zVwCGx6JT6Hi5sB8d+RZ/olwzURPYM5+aJbVOeaOl3Wueq8X8D5wm9vudgHdl2DZy19h9LekBDUvy8Ax3OMbQIT4Jv8vgT8dNx7TfQfWcYuS7j+kD1S60L0Ze2cWHRMUSDL+YRaXpj5iA79Tgw7Nk8WRaPEsy4kwBXgakylISQbwFMJtVhoAg8LZnNvhJDUBHvAR1k3JrLrow7cytF3F7gj159kbx5K+zlgfqcRsCZ1DVl0fZm1bh6ai1r7IZ6cANdl/XzPeMmWxrNhvW9c3wO2xIM7AXMphgBPto3CPIuxpd7tZ+6x6XJl0SieLHto7gFvRXrS90gSiUZi7K/ZfTaV9qZFd1cKk7R0qejpybrhMdu5zlUO1sRrtpOKjghzLFGzaC/Tk3r9fQAOLOPvpV3P9B8AD6SYz+KNtM+BPtCWvdLERc95zqBMMWCDJpFfZs1iR7ahbUlCW8B9Y/wYeFdmUr4k9b512zI2lKw7AV556t0VMffUDeAJ8FWOWOEIyK46W2aPVB3pq3tmyjzRtrJfArzWZIjhmhQDNqKxRSc6n+IjCkkb0dhf7Ezx+p4Tk6RJNPbXuuhi+zLgg5oU17at4a9BWZL/BFb/hVQFK5JVwYpkVbAiWRX8FyR/A3CR4Eshsu7aAAAAAElFTkSuQmCC") },
            { "hi/help", new Caption("इस कंप्यूटर पर भाषा याद रखी जाएगी।", "d07ad091c93b4a8793074f856e6a1ab48ab30e643661816cf9ac4a0232a0a701", "93088c7b017e92043feacc103704c875ed2162359ac1085dc9b929d6d9eda453", 304f, 20.5f, "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") },
            { "hi/apply", new Caption("लागू करें", "b9ec87d0202339e11096ff592e46644bd99716409692b426e82cc514876dc630", "5f9c7a643d8d3366142d7ac53b13c2dfa54e421c4a24a201e6f818cfe765dd5a", 40.5f, 22f, "iVBORw0KGgoAAAANSUhEUgAAAFEAAAAsCAYAAAAdM1QpAAAABHNCSVQICAgIfAhkiAAAA7lJREFUaIHtmjFoVEkYx3/KgXpX5ARJLAQtDhVBUBDU2AgW252FVS4gwjaHCJZJI9hpsBBBgikCIiRiuVwVyB3XpAyJWLiKxSp3HJETFURXLMbC/8Dw8ua92X2z7IvMH4bvZeab2e/995tvvm8224wxJFTD9mEb8D0gkRgBicQISCRGQCIxAhKJEZBIjIBEIiwAXcBIrgANjTXVt1G0QCIRnoiol5LjwJzGJoAdwCgw41tgW6pYNmENOAYsAg+AO8Ab4IxvQiJxMxpAS175c8iEtJ03YwlYBUaAeyETEon5+EPyZIhy2s5+dEO3dJ4nmorti6f/Y5/rLXts70Swtajt0Ja2f3d8JP6Q0/eyjPkM9uoDXwOfgN3AW32DI07/j3rGGXsPvMustz9jhy9H+7cH24reaQ/wf8B7+T/PGFO1dcw3zGT6lz397thyzphFTNv6mWttbJXp5nniVkUTOKjnt8DNiuu9AM4BP5UplpHoGvYcmM8Z21XN1spoqMLYn+k/UHHdX0IVi0hcUQnk4gpwF7iWY/Qw0FBVMaq49xj4IAKzB4HrEH8pH3TXuaznWckTkoV1M/hjoo0H7xQTWk58segYYxakExoTm8aYNWdO1Zi4IN12SUzM2t7VXKvXcsY21OxzqR0+TzwqOQdMOf0deeAz4LDTN1L6bX3DhOrSWDgi+XeJnrV5VV46DlyQFy9ldEclXwMXQ4zwVSx2IZfAho59gFshi+dgtc95/cLG60V96ZO6SFhX+mJJmlW6heQiMJZDcC58JNq8yr3+2Qc8lQHznnllmNL8WHgleTZnbM1xhsnM2BvJMcklneafgZ26HguGj8QVyUtO3zxwXK0KjgN/ans9rLjWrF78ENDWhUFLCfwxjeXBeugLp+8mcF8eeh2YDrYiIFFdG0CyXdR6TbannYPKPTjuFSTbVr9ZcFh1tXapDUWDDeeUKiJy2CS6ZM6oNTK2uXr2JO4EnPpBRBbliUsKvA+0NTb0d1CwHQJCK5Q2cAp4VKBjY+hvwA3gNHDep1xWsVgi7+hiYV+gof3ATYYHialM1uHDJPAPcFUJvB89bJdGD7q9rts2m9FVSBjU50Z77zpcytry0pZt/6luPeFcpd2OcKEwMNSBxKIb5GXdpHxWTKplPK4DiUbeNuYZbwG/lugMFXX4oWpdlYWbLLed/0I4L52Roh/Qh4k6eKLvPnAO+F3PTcl+y82Bog4kWkwrjSLnArjWqBOJWxZ1iIlbHonECEgkRkAiMQISiRGQSIyAr7hzCgd5x5M3AAAAAElFTkSuQmCC") },
            { "bn/autonym", new Caption("বাংলা", "3bdfce0a268d154828af6e9f5e4b9962d0ee04a148ca64d8994570eed9556afd", "d962a646501a88aa0647de56c0d9fc92c25e81791468744ec2c422d2ca095357", 34.5f, 20f, "iVBORw0KGgoAAAANSUhEUgAAAEUAAAAoCAYAAACo5zetAAAABHNCSVQICAgIfAhkiAAAAutJREFUaIHtmTFoFFEQhr+IECwkiBIsbKzOUiGdFmKEKwQtbNRrhCtiYSteKVjkIlgqZ2ETODuRFCkCZ8AiNioXsEkgRaw0NrkUYlKNRWZ1XOf2du/t4RXvg+Ut82bfm/z75r3Zy4SIEPmbI/87gHEkiuIQRXGIojhEURyiKA5RFIcoisMoRekA+yMcvwgCLOV1HvVKmQQaI54jL5W8jv1EWVB1h722gTM61ryxJ9SBDWPfSIkXMre99nXFJqIk9g4ZHO1j3wW+5BT2tK6IxP+UsQF8B34a/yrwFJgyfRXgEdAFVgrMbTkGTAN7QE/j+AHMav8B8E3vdzJHEpGQqyUiPTnE2usisq/2hVTfgto7xraktlZALN64iMia2rfzjtVvpWRRB24DM/q2PV6qz6zTlzyzZWxftT3u+FeBK3q/CzRNXwM4AaxmxHsxlboDKSJKHbgPnDe2deBshjgeH4A54JYR4Zq26T+uDdw0qQhwT59vmljmgHcFYsgmx3Kqi0hX/tDT5V7V/m0nfdBl7KUPItKWf2n3SQfRFGiZOHa03dDndoxvOn0oO306JgX2gGWgVsK7qAGLJi1WdYO1XNL2LXDV2JeA6xrPObVVdbzpEmIbKMqMtpsmgDJoaRo9zPA5qe1Wyn5Dj9qesa0AbzSNghlUvC1rWzFnfjVwzgZwF3im4vRjU9vLTt9rYC1l2wuM6zeDRFkEngDvVZRZXb65S2aHLvBJN8853Uw9nmttUdHirm76aiWlsU/BWqBt6o9uwEabHiu9ySZXw/j0hqxTCm+0Rb99aprTm3ocdgPeR43DKvYAuNNnxTR1vnXgY8BchRimeFvRTberwhwEzJ8UYvMqzOdUcZbMlz6ZRkrIV/IF/UaZzOGbRRN4pfeNEjbyYEJ/OnhcUhw1Tckp4EFJYw7NhIzPfwirwAs9akd3suRgnEQZG+JvtA5RFIcoikMUxSGK4hBFcYiiOERRHKIoDlEUhyiKQxTF4Rf/TEF7qoFocAAAAABJRU5ErkJggg==") },
            { "bn/help", new Caption("এই কম্পিউটারে ভাষা মনে রাখা হবে।", "25c2fa95d3366b41d6b902fc0d4ba9ca1f47d72ffe2593a4f6d173d8ebecaaa7", "2848eb25ccf340fa2449ce014501209e1b25d60de0067ee1f4933b7b0e800bbd", 304f, 20.5f, "iVBORw0KGgoAAAANSUhEUgAAAmAAAAApCAYAAACSo5VdAAAABHNCSVQICAgIfAhkiAAADQRJREFUeJztnV+oHFcdx78RIU1BUhHSUIRbMVhDEXKxD9L6IEYcQexFfNDmQhE20KCFPtlsBSEgtNm8CX3YQiNFuJY+SElB6JWbgNANCi17wT8kocINaHWvSjYvdi8+jA/9Hvh5/J2ZM7Ozm1vz/cCPvbszc87v/M6Zmd/8zpnfPVCWJYQQQgghxPL4yJ1WQAghhBDibkMOmBBCCCHEkpEDJoQQQgixZOSACSGEEEIsGTlgQgghhBBLRg6YEEIIIcSSWYQDtgWgBDBYQNldMQAwArBDXa3M+PsYwAaA3px1jVhuGx1LAMM56xdCCCHEPmOREbDDCyy7LQMAUwDPAngUwH0AbgK4CuAygG0Af+O+JwCcAvAygAmdsaJFnffys6kjF+z3sRZ1CiGEEGIfc2ABiVh7dFpuA/g1gGvR9rNdV5jJiE7XHoC3ALwK4GLNMX0A3wDwMB2iPQDvAPhpxrGBDTpyu4wO/tnZ5wqAzei3YMc9bovteKOBDkIIIYTYRyzCAQOdjm8BOOhs2wNwDsD5RVScYMyI1i6AH7Z0XAYAvg1ghd+3AbyYWdYWgJM1+/wcwHr02xDAdxN2DDqsZtQvhBBCiH3EohywQLwO7JN0zG4DuH+RFRtC5GubEa040tSUHoCnARwHsNagvALAl53fDwP4JoAjAE4nHDpvPd0agIcSjpsQQggh9jGLdsA8gkN0YEn1hSnAlwCc6bDcogNnLjAE8FRDZ6oA8KaiYEIIIcSHjzuRhuJ9fi7rLcl1LrT/TstF9Ck856ukw9eU1/nZJCq4yXZ9PKHHVgs99gtd6/9ht8e8DOZ8M3mHEnO321UIIVrTxgHrc03VjBfgKYBLZvuWk9rBSlgL9Sy/exf2QU0ZTWWFU31v8vu/M4+bGp0KRu9su7ccp+5UC/3e5LEnzW85Oq5Q7DF9p6wq+VcLfb0+Qwf91Eb/NuV5eClJ5pU2ti0TjlLdeZWSiXmjNj7ntpw0JwXP53hc54yzZdm1qXi2bPOgJIQQnfHRhvuH6TwAuA7gn3xD8HFeyM/wLcHA57i2aY/pHWbOW4ATp54bTAtRx0muJ3s7sf0YgHcBfIKL8G/y+6cB/Mn8DqaieN8ce4hTpVuUPm9kuwB+x7ad5OeTJiIW9Al1p/S+WbEdRsdjvPlt094pHqCtz9Hes4QehwB8ngv7dwH8A8BfK8qFYz+vz5DZZ031fwDAe8YO1xNvkgLAI/x8u6K8FCN+rtAu70bjIRD0uMw67o3s+wjHyW0Af6fN6gjHBPvdcPZ5x/nN42Q0Bu9j9BdR/xXc91h0/Hn29ReciOzllnYN5+BxjruqPozHa7D3Hh96ft/QBqntOf0ihBCLoyzLXBmWHzAry7Jvfu/x92vOMYOyLCflfzONjm8rQ1PeJeqR2nfAfbecbZcqtu2w/Bll4OgwYxsLlrNjthfcp4iOS9XnyRb3j+tOycjRw+oT+uNSZnn9sizHDXWeR1L6XzNjaFKW5Uaiv+LjquyRGgthnHt1hP7wjg92Gjt9XiU7FWXmSs/oH7c19Lntv4HR1fZ1OKdK035PvyZ2teXPWP6g5pwF6w/9XbdvnQ2sNNVbIpFIOpcmO08d5yuId0EbmovnBi+4I+PMtHHCeryJhBtEzE7CUalywAYJ/WFuAMFh8XQOjsE1p5xwsx469op16SecIs8B6/O711bPEQwSbqSeY+G1PXaecxywcXRTB8vx2jt2bFOlf7jBTs0+/ah98XFV5aXEPjjE+qUcsPB73PYcmccBG0TOadW5GPdBOBeHtGv8MDGu0K+pXfumnFhX77wK5+Ukw5nNsYGVpuNBIpFIOpfcHcPFcJTY7l3QwgUxvniGsryIWUp6JrpQ8gYcbngT3kDGvIGUjoPhOWA9x5GLn7KH5f9ib7BBJ3tj2XHqje3m6RJ0j50qzwGzN5upo7fXH6EtdQ7ChrHLjPtXRQljCY56lQ1sJG6aOZ5iseMh6OU5YLnlxRIcjEmiP+LxOUu0xUpwnGOHo40DNojG3TXTx3FbUw8g9gGjNM5OjgPW1K6hjBH1GZo+nDnXiTA+6iLbuTaYdzxIJBJJp5JahN83i2zBNUNgBvlcvPUzYCb8XeawqqPggv+XzRqkl7iu5SumnjNMxbDGsk9lvPH1BNfdbDP9w23WM+Z6tiHzc4Hrwy6w/hPMBRaSu24DeJCfXlv3nHU2Mc+bv7+aYZdXqM9ltuHpjGO+xM8XE9uHXGMT1vhdpj1XnSz8VfyB63zC4u5b/DzG3zYAvMZ1RDepf5MF0QMu7D5B297kmp4x17d1xSrH0pGMfR9nm3+Z2N6nbV/gQvgX+L3N//nss/3PAjjKcXcawGdbpFlZ57EXKPdzLWNIkvzHFvqlWGFfPcbzIpyzb9B234vaeIRt8/LidWkDIYS4MyQ8s2n05G+n6QaJKTVv6idV/iSKknhSmKjQJDHd5tUb1qSNHf1tBCCeHiz4W6izrChn4mxLTcmEp3z7JB+iJUPzBB/+ju1StwZs6tTp6VH11B/qmPHvVNQyJwIW1hF5kcLS1LNhbBZHQ1P9upPQ00Zy5o2AFc5U2cjU5UXA6iJYM1POIJpGHTWIgA2MTuNEdMhra07/9WnHHVN+Xfua2HVUse4wLqdqzLe1QVu9JRKJZCGSioAdjiJYZ/mUucKnzq9l+HapJKXh6TaVysAeH/4x9i1m0c8hPDF7+bEs4a2y0JZNRtXWTETgdJTkNOhwxEmAmmrvb/n5RLTvYSZfPcoI3Bm+/XWwxT/urov8hGjmXxLbgy1mLCvX1h7nGTk6YdqxGkVa7mH0ZZP73pNR7vPmLcg19lWw+Tpt2AXnTT0vMULzMFMz9BPHHGU7PAbs020T/VljFHebb9quJI6NucKo3x6/f6ZF+2IKpmp4gdHPQ2x318l9H2O7Y8IYuWV+C+PvirP/ImwghBBLp0kesFUAz/EG+lTG/vENq+BUU3h9/ZWMMl7jjfBB3hymFVOLPW4L6QRSzkbgLG/a8TTQJredjaY/CgBf599Nss+f4fTmI+a3NWPLNSf7fe5NpXCcZY/gjFZNC19lvzxqpmLbJq4NyTnt1OhFY1dLne6B3/DzIY6LUeSorneUWuAE+ytMZ63xf4Dexgdj13N2D1a0IzgW3gPBakPHcRPAL/hgcoIPQzsVjmFumcFuu7Tz6zXHdEUB4Af8+1fm9+CQew81i7CBEEIsn0RoLEw55L6pGIf0e2Wa1FtPdbJhpnLCVMbM1Ltl6ojfnGoyhVYl3ht+VXawupcZqR/Cq/12WqVqOiZsi186iPWIF1VXiU07MTULx5vab6dC71ivnCnIkNJjFL2duRXVOe8UZGrfIpqettsmNQvwJ6ad3pTZxCmzTnpmytymzPD0z+m/+C1lO1bnnYIMb5WOqcMW/7bTsHXnwbw2aDseJBKJZCGSioCFzOznGM0YJCQ1VXaREZ43oqmZq4yenU8cV8U6oxHXuej5kpmiBIBXncXEXbPacmpmnXYoap7UH2akxVt4HNMD8EXu/7OafTe536cyyj3PNoapwnOMBjXlx4yoPVMRtQzjICfJ6CYjUo+xfy+YBfg7Hf6bqV1OCXqR0Z8kjnmPkcjU+fBkzYsnuVFAy0VOw36ffXtqzuzuZ8004Vs8x8ZzlBfzPiNWJynH+cLGc6zXEpYneP+83tK1DYQQYnlUeGfDinxbFpuKIVVWL3rlvItIVJlYgO5JVxGwOqmyQ988qXtRoVSy01QKjVSeqpQeIVpWFYULfb4R6Txtab++iRqNTT44u9h73ohVSJExocxbnl3k7bU39JN3TF1qlVQ0sGkaipBCJUSOCvNiy6xlBCzVd+OOFuEHPW3fTxM56Qoz7nIT2tbZYB69JRKJpHPJ2ckm/YzlkrnB5lzQeiaH1WTOjPiTBvXuBwcMkUMy4Q3UTsV4N/AicmJsDrBUQlVPjyKa8hnQHrZOe2P0dG5jvziHm2WUuME2vUEOasZhm/Kmibx3qUSsNiHv0CSyzXEgmjpgdro9TNMVFedE2/FfRA9h89o11sk64fG1wDrWG+Yt4bop7Zzrwjx6SyQSSSfSRSH9Fhe0oblh5mRl96TXoN794oDBpLuwN7bUv9axNrbrnlJrier08LKRT6J0Dl6Eskki1qp2W+e9yjFpc4OsSsDZ9obr6ZhywIpEuo2ch4ymDlg/Ef1NnYvzjH/rgHdlVysbFQ6+lxam7nqRcz2SAyaRSO64HCjL8k7Nfvb4BtQzC1qvJdL0+VberZbr8USannmLNX7jU/j0APyI6TzORWOyMGvBruhaIYT4f+FOOmBCCGHp64FACHG3IAdMCCGEEGLJNEnEKoQQQgghOkAOmBBCCCHEkpEDJoQQQgixZOSACSGEEEIsGTlgQgghhBBLRg6YEEIIIcSSkQMmhBBCCLFk5IAJIYQQQiwZOWBCCCGEEEtGDpgQQgghxJL5D8j7tfJ/aHfTAAAAAElFTkSuQmCC") },
            { "bn/apply", new Caption("প্রয়োগ", "f70442a7a90caf7b70b9393a5cfa0c6a8dcb4660dcf5d5f94ab2af094baac738", "bce4107a07ed528803e8231f134eb9468bee0bbec98144c62c45158abd5dcd43", 37f, 20f, "iVBORw0KGgoAAAANSUhEUgAAAEoAAAAoCAYAAABZ7GwgAAAABHNCSVQICAgIfAhkiAAAA3ZJREFUaIHtmTFoFEEUhr+IoGmMacRKUwSthBypTOwiXKeFhaAQhDQBKxu90kpzip1FGhshguVpY2C1O61CLNO5gQg5GxNBTLAYC/+BcZ3ZnbtbjMr8MMztzJv33vwz8+bt3ogxhoRqHDpoB/4VJKIikYiKRCIqEomoSCSiIpGIikQiKhJ1EpUBrRr1xaANmCHs5kAvRrBOouaAqzXq6wfnhxh7AmhWCcUS1RHzRqUHrHjkphyZqvK9D1nf2B3gguxecvrajj+Zx+/MQ8wr9echAkYq3vUWgHtifR/YVvtJ4AjwCZgHVmXIlTkGfHF0jUoPktsHPgOn1baryftwHBiTvW/AuOyNecYuA+vA0xK/d4ElnYApR+9HYNbrgTEmVFrGmD3zE11jTLPQn6mvpz5jjMkDupqSM8aYdUfXgvkV62orjre22gWdudqzCFu2dJx59Tx6vSXU0TTG7DgktT0GXRlrMERUz9Hls7XiTHpPz1VEoeciUVZ2R7+XPf67m2AoooqK7ARanlXLHRkfUdbxjSpn5LAlf8WjI4YotJOKKC5Aqx+iDgdiwpLqRaALTAAzwA2139L5fw80FBOmArpswH0Y6LdoAtPAUT1PV8iXoaGUYdzxYQa4Alx35rjoxMhyRKyye3x6zop3CsfHt6OWI3dT5uxge2Sahf5+dpSvWP1uWz7sjvJhTDcGwDNnZdCt58Ok6q1A/wLwSLr3gRfA5T58ikUm3zcHVRDKo7rAhpNvdEpIisFaoH3LueLXZLNOZEob5rQQy4MqChE1A5x1dsop1YOQVIZVxTlr87Ym5ksKB8GoFmITuOnE3r4RImpXtXW2ATwYgKSvqssCcwMYkf63CuZzwPMa3h1npXcCeDKMohBRH1QvOWTdGUD/O9WTFXJW/6xi1GsRdj/wqtQPBvH7N4SIeqwzPaX4lDslU2mrlK36krb9ac+E3aPV0pHLdRwvAnf1anENODf8VIdEyZXY1LVrE8sy5E5dlryuK2XoKg2wKUA3kPw1C/aHSQ98pZb0YDVw7S8AZ/R7TKs9U6LHBtCWdqhNTHcVn1aBl4pj28Cbgg/zhVv3QFD19SAWHX3q2FTgDMH9BFJL7PhTqIsonG8/daYPfw3qJOq/RvpzIRKJqEgkoiKRiIpEIioSiahIJKIikYiKRCIqEomoSCSiIvEDer6Oar5N7yYAAAAASUVORK5CYII=") },
            { "ta/autonym", new Caption("தமிழ்", "540a59cf6fd8508ba43aa077a3128e234b5c486a919eabd6d8c05b9930bf2487", "d0e1fbb7908333d0dd418aff348436df6055610cd54bba5a9de8cf6962e55cdd", 38.5f, 19f, "iVBORw0KGgoAAAANSUhEUgAAAE0AAAAmCAYAAACBOhYpAAAABHNCSVQICAgIfAhkiAAAA2lJREFUaIHtmr9rFEEUxz8RwVgZmySgkBRCQAimiCDY2aSwCGJhIUjgCi0EK/XsrDTxD5AUCkE4SWMRK09OsDBWhnSSSIpLoSRpvCvESzU239Vx2dkfc3u3G7gvPGZuZu69t9+beW9m9oaMMQyQDceKduAoYkCaBwakeWBAmgfKTloNaAJG0gE2gaVCvTLGlFU2zT/sG2OaxpiO1Vbz0Fk1xrSkZ9nXt6KJccmaiOnoQaP6jDFmKaPehvXdpq9/ZV2el1S+ARZDffPAB9VvZNTbANrAIfDO27sSzKooCTDn6K+kGNMzKetMC3DW0f5SMwbgSh/9gRJnz12VF2PGtFSe6oM//yGJtCUr3aeVRoIeF+a0negAE2q7BVQTfDzXpa1g7FYKWwAcTzNIgXPP+nwSGFV9NzR2P6XOMF5J56F0jmgWPQUmgTueetPamkprK+3y3JOyQFasvsmQ3PR4iJr1EPPSMwJ8Vv+CZkce6NpWWWLamMoNoG61X1bAP6GlWgpbZSEtiEu/I/q+qDxfFltJpAUpfyIU7B9YY4Iz4Zamfl7LKMCGytM56/W2FZcINoEZ1ZMSwbgC6RRwVbv48E6+W5zMWZ+3LddMWxNhbeAZMJyQCIaBR8CBMl6q1J0RUcupV4i15SLtgspV4GHM99tWfVEBtC3iatl9jYTrVNALpLKVFNPajvZAeSvUXreC6Rj5IAjK33PS17UtF2k/VS4AlVBfFbiu+nqM7vBO3QdrVlz9lIO+XGy5EkHV2jW/AJ5ondsJYDthI7vjaG9GtI2rnLX6x7VnAnidECZc6IktF2l1xaf7wLRFFMqW6zGEzap0HacmHO0oFgYH8APgB/Dek7De2cr5rim4om4Vcc+V0cdGUfdpFd0qNJQUZrSnWwwdUfqNilZDU2LfvHxVOe29EY9htGqM2Qq9zEhCM+JOv99SifA5PKv2Qz7bkvjCJi4RPLaCY/j6x8YO8At4qxvVonFXfm8D9xwzfkwxdzQi7rkS2F+4SFuQ4QMlhCKXWlYE58aPMX4vi7C2roUywRXTAsMrR4wwrJlyTeTYqCi+3dbnVS8LjnXb9HyvWAaZC8WsKHQ8XzbHxrR1/WLfvH6JYlFXzFpWhjxj9XUU6553s4KGzOBPfZlRlpvbI4UBaR4YkOaBAWke+ANQtmA1dR9KigAAAABJRU5ErkJggg==") },
            { "ta/help", new Caption("இந்தக் கணினியில் மொழி நினைவில் வைக்கப்படும்.", "9d854a6125a2c6c644f72e0f99f8134d8461826f3bd86f0bea0e4e412158b3bb", "05766108b6ab345e28cb1c42b8d5cfa25cc6d52995688bf54de1906f19da2c22", 304f, 36f, "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") },
            { "ta/apply", new Caption("பயன்படுத்து", "2653db8ed06b54cf26cb94173b62983f07e8ea1d9f9ddbdfe2d99cba709487b0", "8202f8e62ffb85f12af8e4c640314a6edc808a34a5e44c602a73a45509d1d499", 84.5f, 19f, "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") },
            { "te/autonym", new Caption("తెలుగు", "887a3c7b8f2a012aee37944917f579686a1261d0908a1419fb79bb8f6e7a9883", "8be89e9ac2d30fa3d2e45c2360470f691cc49ce7694b6d8a5982363586eecab0", 38f, 20.5f, "iVBORw0KGgoAAAANSUhEUgAAAEwAAAApCAYAAACfrs/CAAAABHNCSVQICAgIfAhkiAAAAy1JREFUaIHtWb+L1UAQ/k7s7K2EsxDBztLaK1Jb2Qr5G+RSWprGTngWggjvD0jnSdpXSizfiUUEm9jcUxFyIozFzcI4ziaZXDjx2A+W5O2P2W+/ndmd3O0RERKm48q/JvC/IQnmRBLMiSSYE0kwJ5JgTiTBnBgTrARAE0oLoAGQXxDvuVgB6JjzLK5XFyKyz+U5gM8AjhayuyQaAHfF79uzrBDREiUjoi2doVrI5pKlZm49EZXnsbXUGXbEngUA1xayOQcVh1wm6lYADgCcAngC4PA8EywlWAbgBr//WMimtp9N6HcPwHUAj/l3CeARv78C8NQY4zvLZrhlSXHsODz1mEKELBFRy3Wyz4aImsj4msNpLNzXEV4tEeUDa9lE5v2rLC3YOiJWb/TtBclM9NkZYkqxmwivgsfG0BPRSo2pRHs3RbShRu0VnTFhbZDUorVGfaiTB/BaiabJV6Jdi5YJHtp75Tp6w9NKMXZUtCGxLK8gMWHM/aUQhViEXJwlGNh2mNcKP8lLbl5tzKNLEM3y0IzFIg5Pt2CtQWrDdTX/boyFVYrUakDUXWTusBGxxYc5OlEXFqtDWYs9NG8huEW9LGY8EA4Dc0EqhFYQLAiYibqwWLnzMnS3kUM42AkY4icXNtZfj/M4yh/FyvQLfu4DeKPavgJ4ze9vOXM+4E8NC++4/QTATdf1fZY3xfCJ+d1f+KviI9u9Fetg5WEyVzlm4l8AvAfwUBA8BPCCyZ/y85jbTvj5gZ93JuZRAPCAn98m9r9YRFwvhFbsCh+7KErDzadc2/K2+xVJU2TIBnudMa8upbAbW1ewEw3JKcR3fNDGDtRSCGyJnKt0YW0Il4vklIjou3jfqHxNn5MQF9LQBodxP8V4eY6Gy6Sfc+hDXf9TUUdsFWL3xrDluYdSm15tYC7arI2VN+CzES6WV08STE62YfH0AnqurwZuPVnWLIhOdjv2ACsva8S8PY+3RAmba+VvOt3JjKS7HQrFUPbo8vwjN3xEv5zZPgmXSbALQfqbvhNJMCeSYE4kwZxIgjmRBHMiCeZEEsyJJJgTSTAnkmBOJMGcSII5kQRz4jdMnkmlM3er4wAAAABJRU5ErkJggg==") },
            { "te/help", new Caption("ఈ కంప్యూటర్‌లో భాష గుర్తుంచుకోబడుతుంది.", "ddf50a6ece09f8d6e1053f610432b57bd6b7e05d5d8195cdadae94e943ef7cff", "cc64197eb9dd9c72121496f925dd95f02c3233de96f3e44a4687557c0bf35844", 304f, 21.5f, "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") },
            { "te/apply", new Caption("వర్తింపజేయి", "c8b2553a63e2c34408e427ef2c15c51141c3d1d682c978e4fbfbf782bdd518c1", "26528c22d6345a1214ded566b87a63b996ff830a5770a17d6fa8823f058018e1", 59.5f, 20.5f, "iVBORw0KGgoAAAANSUhEUgAAAHcAAAApCAYAAAAPvbYNAAAABHNCSVQICAgIfAhkiAAABNhJREFUeJztWj2P3DYQfRsYMJAUBzdu3aRI6cCd7cowoAABkn+QRunyAyJ3Lk/+Bdu4MaDa2OsOUErVSrnptkgRudpDisgV3czAg8GQlLTa00XmAwjdiR8zw+E8DrnaOOeQsE58tbQCCedDcu6KkZy7YiTnrhjJueNRAzgA6OlZLq2QD5uULY9CA+Cp8f4NgN8X0CeIFLnj8BTARwCvAGwA/EHvny+sl4lTnJsDaIme6kjbnaCyjvoVJ8gei4rkzkFT/wC4pL9fAvgBwLMZxp0fzrkppXDO9e4z6kDbxvlRTZQ/prRKZqx9HdBX4hixe/FyqmNr51wWaJtTu945V9K7TDk8vwXHdkL+XM5l3FkHhypLmhwZXRmtWOec2w0QUFLbxqjbU117ohEZ6XJQ73ciwkILcGrZ0vha7p0poT33OwCPAfwIIKPyDsAFgD8B/DyC/b823u3o+WDkTqJxTbo9EmNWAH6i5OeS2uQnytG4mXm8+RGJiN7Z6CnyqgFRwWN0ar9iypxj5fuotFJ6HALbQCnYhG1slX1DaTmj95bNhUd+F6H42tAnWGINCpqQEHqiKF//Y6S/jzZzMuYgHBPa42uxkA5qEkshrzcmuPTo5tTis6CTQp1sDulTibrOsFEGWj80f5gaKQXtaceAwlKhRkVMJvZEZ+zJ0liNY2D1h0ouFqp2MCd4rbKR9bcWFLPFXsnoxVjaZskwOiAqxXJaZq4CLergqc6VpfEIZNq1kintxF4Yk4vxZKRKFulO0Lc1xmAb9AI9Cv1qsSAyj31jbD4adZmg8n1E/z5G0XM4F579M7TqrQks1Vi+vYzb+7aCWLHGKIlJONJysf92kdwjP8Fmi4Fk9PsYihd58BgWmgB5+N9HqDATbWX0DYkwlsNRw4b7Ep/aMGwr+g25XOAxdISVivoOZBtTKu//nZEcTbHZt0Abw0atp/NEf9S5VhJlJSKyMJ2UQvgQQzUlykUSMqxR/2uEzs/cRzJNq/rG9vWMFj1PcDHBZt9dwZAzdGyezHNuQWfGG7o33QC4AnAfwOvAefE/8Tf/QvKQzqAhfEvPv+nJ50efnCdKHp+3r0jXXwF8oDN6E5HNKKn9DfX/Xtwfl3R3rn/au6Yz+gXNGbcfY/PeUz/PGTqwqjUlyETE2lP0fsPRH0oueIUeDTk+SmKG2Co5so1Myqz7653SzaJ6zWIWE3BdOcJmyTS+ueTI9rEl2zealgvRUWaqvVJK7olWQiWN8B2FeMzK02+n+rAcadTeaKt/LNgp+1guO4WdfVCTvRs4TqHmbojNnXiWyn451z31y8Q43Dd4deur8F1cVOrWRcJaZVUg05SToOWHzrlajm/P7SLjtGriQxnxNlCv99gxNutFqPWLzcOko5DOlvXqqtUZMJRN802TvPDgq7jQQVxfBx6NiwEuW6VPq1hHj2PRLx9/ejVOHhinCdyuNQNt1gHTKSYrPHpFryHTZzYrRvrMZsVYwrkZfWbjqHR3+QvC/zOWoOWWzpQSHwH8BuDtbSuzZiwRuY89FyQvFtBl1VjCuX/R85qe39Dz3wV0WTXunXHsGN/L+g8A3p9Rly8SS2fLN/Q91i8ikhNmwjkjd3PGsRMGYOnITTgjknNXjOTcFSM5d8VIzl0xknNXjOTcFeMT/9lA7/6mX4YAAAAASUVORK5CYII=") },
            { "th/autonym", new Caption("ไทย", "6d420ffde81c1303865d1e069ea6cb0062fdbb56bcac49265f8208900aae9884", "7b72b3e5581c8f887b4adcbbeaee1378354ea74b126a835a23c7f807eafb3775", 24f, 22.5f, "iVBORw0KGgoAAAANSUhEUgAAADAAAAAtCAYAAADoSujCAAAABHNCSVQICAgIfAhkiAAAAc5JREFUaIHtl79PwkAUxz8YE2RCFxYH3Fx1dDfp6K6JiyaOjuDiKnU2RgcS4+7AJon+BQ66ycjgACRGHEzoVJdHctYUr7TwwNwnIXe9n+979L3Xy4VhyDyzoG1AWpwAbZwAbZwAbUYJGABhzG9mWIxpPwA6xnMBKEk9mIJd1uQsM3EbKAOPwPbkzbLHxgeqYvznrBmPpYAVKfsTtmUs/nUUmgvSCmgDXalXgZYRagfAM+DFzPWk3wzXD5G1Q8CfpAAkvPrAGbAuzh4AeWADaMSIuJX+vDHnK+nmWb1Cx0APOASWgSXg3BByGhnvi/AAODHmXGoJANgH6sZzBWhKfTUytihlB6gZ7U0SkpWA15jNW1KWI+1PRvtVmo2zEvCecHwduJZ3/0ictSuBIBGaYbQdSY4lI2laE/cxNw2q4gsvwEXEf6zREuAbjryZZiHtTNwb0VewWUBbQCkmyXnG/eNj1AJJBFidiCUV42JUi4jwjNwQRPLEL2x8YHgCJYkafWBtPLt/cAfsyufEvYRUDN8AuPlrERsBNWBLTqaY4ZVyD3gDduRAhoYHEmIb8k+NxPZKObNoO3FqnABtnABtnABtnABtnABtnABtnABtvgEtgVzKQwpbiwAAAABJRU5ErkJggg==") },
            { "th/help", new Caption("จดจำภาษาบนคอมพิวเตอร์นี้", "738e478ac5804ce14d39eeeeef87cafd70979d39a389eab152c5853e20e340ae", "141f1c30c1cb20c98d5d51b0a2bd25d1f00160d07b57059b34476c844fa722a2", 304f, 21f, "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") },
            { "th/apply", new Caption("ใช้", "e3d9fc562eab33f5a5c7180517a563cb019da988932ba559b42769333a80ae1f", "d2740ea8af9bb61e758ec3e298d7a698df316716f9715e776bf273bb76a6cdf3", 15.5f, 22.5f, "iVBORw0KGgoAAAANSUhEUgAAAB8AAAAtCAYAAABWHLCfAAAABHNCSVQICAgIfAhkiAAAAZxJREFUWIXtl7FKA0EQhr+IRcDCgBDLWPgINqJlIFb6EHY2VkJKS/MGsbHT3lI47KzE3oDFWQQ8q6RLqrFwDpfNZm9zFwniDiwzuzO3//17M3N3NRFhVbK2MuQIHsH/PXgPyADRkQH9X0EXEXsk8i0jEZnIj/QcsX2NExFJRaTjiJk7XMwT4AhoAHVgoOuHBTxawFlV5vNOIgmIuV2E+XrA/e2pzhy+U+DYOJUDIFV7qPPSzHNGWYHfJWkRc5+zp5tMRKS7yHGGDp8zU/B7a/3JqAJXhvuqozDbc2mqfrXWt4CJ2i3gwvB1DPvR+7xLdrhzLcM7ne8avivVY+ChaKOQbLcl33Rb9VB1/kk0Nm7CK2V7e1fLawrcGKBoY9oJ2sWTEL7ESec0nqLSDE44H+uWsm5bvhfVTcd1M1IGfF/1R4lrK4NvVAWtAr40KQP+prrhiZkuC3zTmj8b64mx3gFO1E4JEU8ppEa5jaySs99mmWEHv4h8zPvAp9p1y9cGroF3nTf1qAfAZWiHq0n8UYzgETyCR/AIHsH/OvgXus/xlpqQqbMAAAAASUVORK5CYII=") },
        };
    }
}
// END REVIEWED BOOTSTRAP CAPTION OVERLAY
