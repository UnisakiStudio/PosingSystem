using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor
{
    // Preferences belong to the user, not to an avatar or the Unity UI language.
    public static class PosingSystemLocalization
    {
        private const string PreferenceKey = "jp.unisakistudio.posingsystem.language";
        internal static readonly string[] Codes = { "ja", "en", "fr", "it", "zh-Hans", "zh-Hant", "ko", "th", "de", "es", "pt", "pt-BR", "ru", "uk", "pl", "nl", "sv", "da", "fi", "nb", "cs", "tr", "id", "vi", "hi", "bn", "ta", "te", "ar", "he" };
        internal static readonly string[] Names = { "日本語", "English", "Français", "Italiano", "简体中文", "繁體中文", "한국어", "ไทย", "Deutsch", "Español", "Português", "Português (Brasil)", "Русский", "Українська", "Polski", "Nederlands", "Svenska", "Dansk", "Suomi", "Norsk", "Čeština", "Türkçe", "Bahasa Indonesia", "Tiếng Việt", "हिन्दी", "বাংলা", "தமிழ்", "తెలుగు", "العربية", "עברית" };
        internal static bool HasSelection => Array.IndexOf(Codes, EditorPrefs.GetString(PreferenceKey, "")) >= 0;
        private static string PreferenceLanguage => UnisakiStudio.Setup.Language.Current;
        internal static string Language => PreferenceLanguage;
        internal static string ResolveLanguage(PosingSystem target)
            => PreferenceLanguage;
        // Retain the existing call surface while every inspector/build reads the
        // shared preference live. Legacy avatar fields never override it.
        internal static IDisposable ForInspector(PosingSystem target) => LanguageScope.Instance;
        private sealed class LanguageScope : IDisposable
        {
            internal static readonly LanguageScope Instance = new LanguageScope();
            public void Dispose() { }
        }
        internal static string Normalize(string code) => Array.IndexOf(Codes, code) >= 0 ? code : "ja";
        internal static void Select(string code)
        {
            UnisakiStudio.Setup.Language.Select(Normalize(code));
        }

        internal static bool ApplyLanguageSelection(int index, PosingSystem target = null)
        {
            if (index < 0 || index >= Codes.Length) return false;
            if (HasSelection && Language == Codes[index]) return false;
            Select(Codes[index]);
            return true;
        }

        [Serializable] internal sealed class Catalog { public Entry[] entries; }
        [Serializable] internal sealed class Entry { public string key; public string value; }
        private static readonly Dictionary<string, Dictionary<string, string>> Catalogs = new Dictionary<string, Dictionary<string, string>>();
        internal static void ClearCache() => Catalogs.Clear();

        // Keys may be stable semantic IDs or the original Japanese display text.
        // Never use this method for object names, parameter names or serialized IDs.
        public static bool Dialog(string title, string message, string ok, string cancel = "")
            => string.IsNullOrEmpty(cancel)
                ? EditorUtility.DisplayDialog(Text(title), TextMessage(message), Text(ok))
                : EditorUtility.DisplayDialog(Text(title), TextMessage(message), Text(ok), Text(cancel));
        public static bool DialogFormat(string title, string message, string ok, string cancel, params object[] arguments)
            => Dialog(title, Format(message, arguments), ok, cancel);
        public static string Format(string japanese, params object[] arguments) => string.Format(Text(japanese), arguments);
        public static string FormatFor(PosingSystem target, string japanese, params object[] arguments)
        {
            using (ForInspector(target)) return Format(japanese, arguments);
        }
        public static string Text(string japanese) => Text(japanese, japanese);
        public static string Text(string key, string japanese) => Translate(Language, key, japanese);
        // A cached error may already be formatted in a previously selected language.
        // Use reverse aliases for these messages, including when returning to Japanese.
        // Ordinary lookup/custom-name fallbacks must keep their original Japanese behavior.
        public static string TextMessage(string message) => UnisakiStudio.Setup.Language.Translate(Language, message);
        public static string Translate(string language, string key, string fallback)
        {
            language = Normalize(language);
            if (language == "ja" || string.IsNullOrEmpty(key)) return fallback;
            if (!Catalogs.TryGetValue(language, out var catalog))
            {
                var asset = AssetDatabase.LoadAssetAtPath<TextAsset>(
                    "Packages/jp.unisakistudio.posingsystem/Editor/Localization/" + language + ".json");
                catalog = ParseCatalog(asset != null ? asset.text : null);
                Catalogs[language] = catalog;
            }
            return catalog.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : UnisakiStudio.Setup.Language.TranslateExact(language, fallback);
        }
        // Only the reviewed standard pose/menu names are localizable display identities.
        // Unknown user names must never go through general UI aliases or diagnostics.
        internal static string DisplayName(string source) => TranslateDisplayName(Language, source);
        internal static string TranslateDisplayName(string language, string source)
            => source != null && PosingSystemDisplayNameKeys.Values.Contains(source) ? Translate(language, source, source) : source;
        internal static string PresetName(string source, bool official) => official ? Text(source) : source;
        internal static Dictionary<string, string> ParseCatalog(string json)
        {
            var result = new Dictionary<string, string>(StringComparer.Ordinal);
            if (string.IsNullOrWhiteSpace(json)) return result;
            try
            {
                var catalog = JsonUtility.FromJson<Catalog>(json);
                if (catalog?.entries == null) return result;
                foreach (var entry in catalog.entries)
                {
                    if (entry == null || string.IsNullOrEmpty(entry.key) || string.IsNullOrWhiteSpace(entry.value)) continue;
                    if (result.ContainsKey(entry.key)) throw new FormatException("Duplicate localization key: " + entry.key);
                    result.Add(entry.key, entry.value);
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[PosingSystem] Could not load translation catalog: " + exception.Message);
                result.Clear();
            }
            return result;
        }

        internal static void DrawLanguageSelector(PosingSystem target)
        {
            var changed = GUI.changed;
            var current = Array.IndexOf(Codes, Language);
            var selected = PosingSystemTextGUI.LayoutPopup(new GUIContent("Language / 言語", Text("画面とゲーム内メニューの言語。未選択時はOSの言語を使用します。")), current, Names);
            if (selected != current) ApplyLanguageSelection(selected, target);
            GUI.changed = changed;
        }

    }
}

namespace jp.unisakistudio.posingsystemeditor
{
    internal sealed class PosingSystemTranslationImporter : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            foreach (var paths in new[] { imported, deleted, moved, movedFrom })
                foreach (var path in paths)
                    if (path.StartsWith("Packages/jp.unisakistudio.posingsystem/Editor/Localization/", StringComparison.Ordinal))
                    {
                        PosingSystemLocalization.ClearCache();
                        return;
                    }
        }
    }
}
