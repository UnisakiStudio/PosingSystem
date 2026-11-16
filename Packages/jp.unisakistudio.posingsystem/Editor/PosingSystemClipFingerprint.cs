using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    // What a clip holds, for deciding whether a prebuild is still current.
    //
    // Unity keeps two copies of a clip's curves: the ones that play and "editor curves" for the
    // Animation window. A clip whose file has no editor curves (the VRChat SDK's proxy_fall_long
    // and proxy_fall_short are such files) gets them in memory the first time anything reads its
    // curve list, and loses them again when it is loaded anew: after a restart, a reimport or an
    // unload. A fingerprint that includes them therefore depends on what happened to the clip in
    // this session, and a finished prebuild looked changed after every restart. The editor curves
    // repeat the playing curves, so leaving them out loses nothing. Other derived members behave
    // alike for a clip edited in Unity; see Derived.
    internal static class PosingSystemClipFingerprint
    {
        private static readonly Dictionary<int, KeyValuePair<int, string>> cache = new Dictionary<int, KeyValuePair<int, string>>();

        [InitializeOnLoadMethod]
        private static void Watch()
        {
            // A reimport replaces the contents of a clip without counting as an edit.
            EditorApplication.projectChanged += cache.Clear;
            Undo.undoRedoPerformed += cache.Clear;
        }

        // Saving may start the count of edits anew, so a count seen before a save says nothing after it.
        private sealed class SaveWatcher : AssetModificationProcessor
        {
            private static string[] OnWillSaveAssets(string[] paths)
            {
                cache.Clear();
                return paths;
            }
        }

        internal static void Forget() => cache.Clear();

        internal static string Of(AnimationClip clip)
        {
            if (clip == null) return "null";
            // Serializing a clip takes about a millisecond and a controller holds dozens of them.
            // Only saved clips are remembered: every edit Unity will save raises their dirty count.
            var saved = EditorUtility.IsPersistent(clip);
            var id = clip.GetInstanceID();
            var dirty = EditorUtility.GetDirtyCount(clip);
            if (saved && cache.TryGetValue(id, out var known) && known.Key == dirty) return known.Value;
            var value = Hash128.Compute(Authored(EditorJsonUtility.ToJson(clip))).ToString();
            if (saved) cache[id] = new KeyValuePair<int, string>(dirty, value);
            return value;
        }

        // Members of a clip that Unity derives from the others. They differ between a clip that
        // was edited in this session and the same clip saved and loaded again (the binding table,
        // the two "has ..." switches), or appear once something reads the curves (editor curves).
        private static readonly HashSet<string> Derived = new HashSet<string>
        {
            "m_EditorCurves", "m_EulerEditorCurves", "m_ClipBindingConstant", "m_HasGenericRootTransform", "m_HasMotionFloatCurves", "m_Bounds"
        };

        // The JSON of a clip, {"AnimationClip":{"name":value,...}}, reduced to the members an author
        // sets: curves, settings and events. Text in another form is returned as it is.
        internal static string Authored(string json)
        {
            var open = json.IndexOf(":{", System.StringComparison.Ordinal);
            if (open < 0 || !json.StartsWith("{\"", System.StringComparison.Ordinal)) return json;
            var result = new System.Text.StringBuilder(json.Length);
            var at = open + 2;
            while (true)
            {
                if (at >= json.Length || json[at] != '"') return json;
                var nameEnd = json.IndexOf('"', at + 1);
                if (nameEnd < 0 || nameEnd + 1 >= json.Length || json[nameEnd + 1] != ':') return json;
                // The value ends at the comma or the closing brace of the clip.
                var depth = 0;
                var text = false;
                var end = nameEnd + 2;
                for (; end < json.Length; end++)
                {
                    var c = json[end];
                    if (text)
                    {
                        if (c == '\\') end++;
                        else if (c == '"') text = false;
                    }
                    else if (c == '"') text = true;
                    else if (c == '[' || c == '{') depth++;
                    else if (c == ']' || c == '}') { if (depth == 0) break; depth--; }
                    else if (c == ',' && depth == 0) break;
                }
                if (end >= json.Length) return json;
                if (!Derived.Contains(json.Substring(at + 1, nameEnd - at - 1))) result.Append(json, at, end - at).Append(',');
                if (json[end] == '}') return result.ToString();
                at = end + 1;
            }
        }
    }
}
