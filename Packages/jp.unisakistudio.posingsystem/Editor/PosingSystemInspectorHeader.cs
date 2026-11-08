using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    internal sealed class PosingSystemInspectorHeader
    {
        private bool showChangelog;
        private bool showHelp;
        private int changelogTab;
        private Vector2 changelogScroll;
        private GUIStyle cardStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle releaseStyle;
        private bool stylesAreDark;

        internal void Draw(PosingSystem target)
        {
            EnsureStyles();
            PosingSystemLocalization.DrawLanguageSelector(target);
            var product = PosingSystemPackageInfo.Get(target.GetType(), MonoScript.FromMonoBehaviour(target));
            var system = PosingSystemPackageInfo.Get(typeof(PosingSystem));
            var hasSeparateProduct = !string.IsNullOrEmpty(product.name) && product.name != system.name;
            var wasChanged = GUI.changed;

            EditorGUILayout.Space(2);
            using (new EditorGUILayout.VerticalScope(cardStyle))
            {
                var title = string.IsNullOrEmpty(target.settingName) ? product.displayName : target.settingName;
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(new GUIContent(PosingSystemLocalization.Text(title), PosingSystemLocalization.Text("package", "パッケージID") + ": " + product.name), titleStyle, GUILayout.Height(20));
                    GUILayout.Label(PosingSystemLocalization.Text("version", "バージョン") + " " + (product.version ?? PosingSystemLocalization.Text("unknown", "不明")), bodyStyle, GUILayout.Width(116), GUILayout.Height(20));
                }
                if (hasSeparateProduct) DrawVersion("ゆにさきポーズシステム", system);

                if (PosingSystemInspectorGUI.Link(showHelp ? PosingSystemLocalization.Text("help.close", "ヘルプ・変更履歴を閉じる") : PosingSystemLocalization.Text("help", "ヘルプ・変更履歴"))) showHelp = !showHelp;
                if (showHelp)
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        if (PosingSystemInspectorGUI.Link(PosingSystemLocalization.Text("manual", "使い方"))) Application.OpenURL(product.ManualUrl);
                        if (PosingSystemInspectorGUI.Link(PosingSystemLocalization.Text("support", "お問い合わせ"))) Application.OpenURL(PosingSystemPackageInfo.SupportUrl);
                        if (PosingSystemInspectorGUI.Link(showChangelog ? PosingSystemLocalization.Text("changelog.close", "変更履歴を閉じる") : PosingSystemLocalization.Text("changelog", "変更履歴を見る"))) showChangelog = !showChangelog;
                    }

                // Foldouts, tabs and scroll positions are UI state, not avatar settings.
                if (showHelp && showChangelog)
                {
                    EditorGUILayout.Space(6);
                    if (hasSeparateProduct)
                    {
                        var nextTab = GUILayout.Toolbar(changelogTab, new[] { PosingSystemLocalization.Text(product.displayName), PosingSystemLocalization.Text("ゆにさきポーズシステム") }, GUILayout.Height(28));
                        if (nextTab != changelogTab) changelogScroll = Vector2.zero;
                        changelogTab = nextTab;
                    }
                    var selected = hasSeparateProduct && changelogTab == 0 ? product : system;
                    EditorGUILayout.LabelField(PosingSystemLocalization.Text(selected.displayName) + "  " + selected.VersionLabel, bodyStyle);
                    using (var scroll = new EditorGUILayout.ScrollViewScope(changelogScroll, GUILayout.Height(240)))
                    {
                        changelogScroll = scroll.scrollPosition;
                        DrawChangelog(PosingSystemPackageInfo.GetChangelog(selected));
                    }
                }
            }
            EditorGUILayout.Space(2);
            GUI.changed = wasChanged;
        }

        private void DrawVersion(string name, PosingSystemPackageInfo.Info package)
        {
            EditorGUILayout.LabelField(new GUIContent(PosingSystemLocalization.Text(name) + "  /  " + PosingSystemLocalization.Text("version", "バージョン") + " " + (package.version ?? PosingSystemLocalization.Text("unknown", "不明")),
                PosingSystemLocalization.Text("package", "パッケージID") + ": " + package.name), bodyStyle, GUILayout.Height(18));
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
                else if (line.StartsWith("### "))
                {
                    EditorGUILayout.Space(4);
                    EditorGUILayout.LabelField(line.Substring(4), releaseStyle);
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
            if (cardStyle != null && stylesAreDark == EditorGUIUtility.isProSkin) return;
            stylesAreDark = EditorGUIUtility.isProSkin;
            cardStyle = new GUIStyle(EditorStyles.helpBox) { padding = new RectOffset(6, 6, 4, 4) };
            titleStyle = new GUIStyle(EditorStyles.boldLabel) { font = EditorStyles.boldFont, fontSize = 14, wordWrap = false };
            bodyStyle = new GUIStyle(EditorStyles.wordWrappedLabel) { font = EditorStyles.standardFont, fontSize = 11, richText = false };
            releaseStyle = new GUIStyle(EditorStyles.boldLabel) { font = EditorStyles.boldFont, fontSize = 12, wordWrap = true };
        }
    }
}
