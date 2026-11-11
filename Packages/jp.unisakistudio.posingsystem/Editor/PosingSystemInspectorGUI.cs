using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    internal static class PosingSystemInspectorGUI
    {
        internal static Color Accent => EditorGUIUtility.isProSkin ? new Color(.40f, .69f, 1f) : new Color(.13f, .36f, .64f);
        internal static Color Surface => EditorGUIUtility.isProSkin ? new Color(.20f, .21f, .23f) : new Color(.94f, .95f, .97f);
        internal static Color Border => EditorGUIUtility.isProSkin ? new Color(.36f, .38f, .42f) : new Color(.68f, .71f, .75f);
        internal static GUIStyle Text(int size = 12, bool bold = false) => new GUIStyle(EditorStyles.label)
        {
            font = bold ? EditorStyles.boldFont : EditorStyles.standardFont, fontSize = size,
            fontStyle = bold ? FontStyle.Bold : FontStyle.Normal, wordWrap = true,
            padding = new RectOffset(), alignment = TextAnchor.MiddleLeft
        };
        internal static void Section(string title)
        {
            title = PosingSystemLocalization.Text(title);
            GUILayout.Space(6);
            var rect = GUILayoutUtility.GetRect(0, 24, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(new Rect(rect.x, rect.y + 5, 3, 14), Accent);
            PosingSystemTextGUI.Label(new Rect(rect.x + 9, rect.y, rect.width - 9, rect.height), title, Text(12, true));
        }
        internal static bool Link(string label, params GUILayoutOption[] options)
        {
            label = PosingSystemLocalization.Text(label);
            return LinkLocalized(label, options);
        }
        internal static bool LinkLocalized(string label, params GUILayoutOption[] options)
            => DrawLink(GUILayoutUtility.GetRect(new GUIContent(label), Text(11), options), label);

        internal static bool Link(Rect rect, string label)
        {
            label = PosingSystemLocalization.Text(label);
            return DrawLink(rect, label);
        }
        private static bool DrawLink(Rect rect, string label)
        {
            var changed = GUI.changed;
            var style = Text(11); style.normal.textColor = Accent;
            var clicked = PosingSystemTextGUI.Button(rect, label, style);
            var width = Mathf.Min(rect.width, PosingSystemTextGUI.MeasureWidth((new GUIContent(label)).text, style));
            EditorGUI.DrawRect(new Rect(rect.x, rect.center.y + 7, width, 1), Accent);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            GUI.changed = changed;
            return clicked;
        }
        internal static bool Check(Rect rect, bool value, GUIContent label, bool interactive = true)
        {
            var next = interactive ? PosingSystemTextGUI.Toggle(rect, value, label, GUIStyle.none) : value;
            EditorGUI.DrawRect(rect, value ? Accent : Border);
            if (!value) EditorGUI.DrawRect(new Rect(rect.x + 2, rect.y + 2, rect.width - 4, rect.height - 4), Surface);
            else
            {
                var previous = Handles.color;
                Handles.BeginGUI(); Handles.color = Color.white;
                Handles.DrawAAPolyLine(2.5f, new Vector3(rect.x + 4, rect.y + 11), new Vector3(rect.x + 9, rect.y + 16), new Vector3(rect.x + 18, rect.y + 6));
                Handles.color = previous; Handles.EndGUI();
            }
            return next;
        }
    }
}
