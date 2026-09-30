using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    internal sealed class PosingSystemInspectorHeader
    {
        private bool showChangelog;
        private int changelogTab;
        private Vector2 changelogScroll;
        private GUIStyle cardStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle versionStyle;
        private GUIStyle releaseStyle;

        internal void Draw(PosingSystem target)
        {
            EnsureStyles();
            var product = PosingSystemPackageInfo.Get(target.GetType(), MonoScript.FromMonoBehaviour(target));
            var system = PosingSystemPackageInfo.Get(typeof(PosingSystem));
            var hasSeparateProduct = !string.IsNullOrEmpty(product.name) && product.name != system.name;
            var wasChanged = GUI.changed;

            EditorGUILayout.Space(4);
            using (new EditorGUILayout.VerticalScope(cardStyle))
            {
                var accent = EditorGUIUtility.isProSkin
                    ? new Color(0.55f, 0.72f, 0.95f) : new Color(0.22f, 0.42f, 0.70f);
                var accentRect = GUILayoutUtility.GetRect(0, 3, GUILayout.ExpandWidth(true));
                EditorGUI.DrawRect(accentRect, accent);
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("ゆにさきスタジオ", EditorStyles.miniLabel);
                EditorGUILayout.LabelField(string.IsNullOrEmpty(target.settingName) ? product.displayName : target.settingName, titleStyle);
                EditorGUILayout.Space(4);
                DrawVersion(hasSeparateProduct ? product.displayName : "ゆにさきポーズシステム", product.VersionLabel);
                if (hasSeparateProduct) DrawVersion("共通システム", system.VersionLabel);

                EditorGUILayout.Space(8);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button(new GUIContent("ヘルプ / 使い方", "公式オンラインマニュアルを開きます"), GUILayout.Height(28)))
                        Application.OpenURL(product.ManualUrl);
                    if (GUILayout.Button(new GUIContent("お問い合わせ", "ゆにさきスタジオのサポートページを開きます"), GUILayout.Height(28)))
                        Application.OpenURL(PosingSystemPackageInfo.SupportUrl);
                }
                EditorGUILayout.Space(6);

                // Foldouts, tabs and scroll positions are UI state, not avatar settings.
                showChangelog = EditorGUILayout.Foldout(showChangelog, "変更履歴", true, EditorStyles.foldoutHeader);
                if (showChangelog)
                {
                    EditorGUILayout.Space(6);
                    if (hasSeparateProduct)
                    {
                        var nextTab = GUILayout.Toolbar(changelogTab, new[] { "このツール", "共通システム" });
                        if (nextTab != changelogTab) changelogScroll = Vector2.zero;
                        changelogTab = nextTab;
                    }
                    var selected = hasSeparateProduct && changelogTab == 0 ? product : system;
                    EditorGUILayout.LabelField(selected.displayName + "  " + selected.VersionLabel, bodyStyle);
                    using (var scroll = new EditorGUILayout.ScrollViewScope(changelogScroll, GUILayout.Height(240)))
                    {
                        changelogScroll = scroll.scrollPosition;
                        DrawChangelog(PosingSystemPackageInfo.GetChangelog(selected));
                    }
                }
            }
            EditorGUILayout.Space(8);
            GUI.changed = wasChanged;
        }

        private void DrawVersion(string name, string version)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(name, bodyStyle);
                GUILayout.Label(version, versionStyle, GUILayout.ExpandWidth(false));
            }
        }

        private void DrawChangelog(string changelog)
        {
            foreach (var rawLine in changelog.Split('\n'))
            {
                var line = rawLine.TrimEnd();
                if (line.StartsWith("## "))
                {
                    EditorGUILayout.Space(8);
                    EditorGUILayout.LabelField(line.Substring(3).Replace("[", "").Replace("]", ""), releaseStyle);
                }
                else if (!string.IsNullOrWhiteSpace(line))
                {
                    if (line.StartsWith("- ")) line = "• " + line.Substring(2);
                    EditorGUILayout.LabelField(line.Replace("`", ""), bodyStyle);
                }
            }
        }

        private void EnsureStyles()
        {
            if (cardStyle != null) return;
            cardStyle = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(12, 12, 10, 10) };
            titleStyle = new GUIStyle(EditorStyles.boldLabel) { fontSize = 19, wordWrap = true };
            bodyStyle = new GUIStyle(EditorStyles.wordWrappedLabel) { richText = false };
            versionStyle = new GUIStyle(EditorStyles.miniButton)
            {
                fontStyle = FontStyle.Bold,
                padding = new RectOffset(8, 8, 3, 3),
                alignment = TextAnchor.MiddleCenter,
            };
            releaseStyle = new GUIStyle(EditorStyles.boldLabel) { wordWrap = true };
        }
    }
}