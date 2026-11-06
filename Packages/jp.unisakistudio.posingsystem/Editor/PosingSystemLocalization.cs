using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    // Preferences belong to the user, not to an avatar or the Unity UI language.
    public static class PosingSystemLocalization
    {
        private const string PreferenceKey = "jp.unisakistudio.posingsystem.language";
        internal static readonly string[] Codes = { "ja", "en", "fr", "it", "zh-Hans", "zh-Hant", "ko", "th", "de", "es", "pt", "pt-BR", "ru", "uk", "pl", "nl", "sv", "da", "fi", "nb", "cs", "tr", "id", "vi", "hi", "bn", "ta", "te", "ar", "he" };
        internal static readonly string[] Names = { "日本語", "English", "Français", "Italiano", "简体中文", "繁體中文", "한국어", "ไทย", "Deutsch", "Español", "Português", "Português (Brasil)", "Русский", "Українська", "Polski", "Nederlands", "Svenska", "Dansk", "Suomi", "Norsk", "Čeština", "Türkçe", "Bahasa Indonesia", "Tiếng Việt", "हिन्दी", "বাংলা", "தமிழ்", "తెలుగు", "العربية", "עברית" };
        internal static bool HasSelection => EditorPrefs.HasKey(PreferenceKey);
        internal static string Language => Normalize(EditorPrefs.GetString(PreferenceKey, "ja"));
        internal static string Normalize(string code) => Array.IndexOf(Codes, code) >= 0 ? code : "ja";
        internal static void Select(string code)
        {
            EditorPrefs.SetString(PreferenceKey, Normalize(code));
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        [Serializable] internal sealed class Catalog { public Entry[] entries; }
        [Serializable] internal sealed class Entry { public string key; public string value; }
        private static readonly Dictionary<string, Dictionary<string, string>> Catalogs = new Dictionary<string, Dictionary<string, string>>();
        internal static void ClearCache() => Catalogs.Clear();

        // Keys may be stable semantic IDs or the original Japanese display text.
        // Never use this method for object names, parameter names or serialized IDs.
        public static string Format(string japanese, params object[] arguments) => string.Format(Text(japanese), arguments);
        public static string Text(string japanese) => Text(japanese, japanese);
        public static string Text(string key, string japanese) => Translate(Language, key, japanese);
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
            return catalog.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value) ? value : fallback;
        }
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

        internal static void DrawLanguageSelector()
        {
            var changed = GUI.changed;
            if (!HasSelection)
            {
                EditorGUILayout.HelpBox("言語を選択してください / Choose your language. 未翻訳の項目は日本語で表示されます / Untranslated text uses Japanese.", MessageType.Info);
                // An explicit button records even the default Japanese choice.
                var selected = EditorGUILayout.Popup("Language / 言語", pendingSelection, Names);
                pendingSelection = selected;
                if (GUILayout.Button("選択 / Continue")) Select(Codes[pendingSelection]);
            }
            else
            {
                EditorGUI.BeginChangeCheck();
                var selected = EditorGUILayout.Popup("Language / 言語", Array.IndexOf(Codes, Language), Names);
                if (EditorGUI.EndChangeCheck()) Select(Codes[selected]);
            }
            GUI.changed = changed;
        }
        private static int pendingSelection;
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
