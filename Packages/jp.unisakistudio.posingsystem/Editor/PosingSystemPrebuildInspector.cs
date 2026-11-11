using jp.unisakistudio.posingsystem;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    internal sealed class PosingSystemPrebuildInspector
    {
        internal enum BuildState { AvatarRequired, NotBuilt, Outdated, Ready }
        private bool showImages;
        private bool showDetails;

        // Called on request. Only schema paths from the current DTO can leave the editor,
        // never avatar, custom pose or asset names from either snapshot.
        internal static string ChangedInputs(PosingSystem ps)
        {
            try
            {
                var saved = JObject.Parse(ps.data);
                var current = JObject.Parse(PosingSystemConverter.GetDefineSerializeJson(ps, true));
                var paths = new List<string>();
                void Compare(JToken a, JToken b, string path)
                {
                    if (paths.Count >= 12 || JToken.DeepEquals(a, b)) return;
                    if (path == "featureSettings" && a?.Type == JTokenType.String && b.Type == JTokenType.String)
                    {
                        var left = a.Value<string>();
                        var right = b.Value<string>();
                        var leftStart = left.IndexOf('{');
                        var rightStart = right.IndexOf('{');
                        if (leftStart >= 0 && rightStart >= 0)
                        {
                            if (left.Substring(0, leftStart) != right.Substring(0, rightStart))
                                paths.Add(path + PosingSystemLocalization.Text("（生成方式）"));
                            Compare(JObject.Parse(left.Substring(leftStart)), JObject.Parse(right.Substring(rightStart)), path);
                            return;
                        }
                    }
                    if (b is JObject obj && a is JObject)
                    {
                        foreach (var p in obj.Properties())
                            Compare(a[p.Name], p.Value, path.Length == 0 ? p.Name : path + "." + p.Name);
                    }
                    else if (b is JArray rows && a is JArray oldRows && rows.Count == oldRows.Count)
                    {
                        for (int i = 0; i < rows.Count && paths.Count < 12; i++)
                            Compare(oldRows[i], rows[i], path + "[" + i + "]");
                    }
                    else paths.Add(path);
                }
                Compare(saved, current, "");
                return paths.Count == 0 ? PosingSystemLocalization.Text("保存された設定の形式・表記") : string.Join("\n", paths);
            }
            catch (JsonException) { return PosingSystemLocalization.Text("旧形式の保存データ（項目を比較できません）"); }
        }

        internal static BuildState GetState(PosingSystem ps)
        {
            var avatar = ps.GetAvatar();
            var animator = avatar == null ? null : avatar.GetComponent<Animator>();
            if (animator == null || animator.avatar == null || !animator.isHuman) return BuildState.AvatarRequired;
            if (string.IsNullOrEmpty(ps.data)) return BuildState.NotBuilt;
            return PosingSystemInspectionCache.IsUpdated(ps) ? BuildState.Outdated : BuildState.Ready;
        }

        internal void Draw(SerializedObject serialized, PosingSystem ps)
        {
            var state = GetState(ps);
            var note = new GUIStyle(EditorStyles.wordWrappedMiniLabel) { font = EditorStyles.standardFont, fontSize = 11 };
            var label = new GUIStyle(EditorStyles.boldLabel) { font = EditorStyles.boldFont, fontSize = 12, alignment = TextAnchor.MiddleLeft, wordWrap = true };
            var statusLabel = new GUIStyle(note) { alignment = TextAnchor.MiddleLeft, margin = new RectOffset(label.margin.left, label.margin.right, label.margin.top, label.margin.bottom) };
            var title = new GUIContent(PosingSystemLocalization.Text("プレビルド"));
            var status = state == BuildState.Ready ? "最新" : state == BuildState.Outdated ? "設定の更新あり" :
                state == BuildState.NotBuilt ? "未実行" : "アバターの設定が必要";
            var statusContent = new GUIContent(PosingSystemLocalization.Text(status));
            var update = new GUIContent(PosingSystemLocalization.Text("プレビルドを更新"));
            var updateStyle = new GUIStyle(GUI.skin.button) { wordWrap = true };
            var detailsLabel = PosingSystemLocalization.Text(showDetails ? "閉じる" : "詳細");
            var detailsStyle = PosingSystemInspectorGUI.Text(11);
            var titleWidth = Mathf.Max(76, PosingSystemTextGUI.MeasureWidth((title).text, label) + 4);
            var statusWidth = PosingSystemTextGUI.MeasureWidth((statusContent).text, statusLabel) + 4;
            var updateWidth = Mathf.Max(120, PosingSystemTextGUI.MeasureWidth((update).text, updateStyle));
            var detailsWidth = Mathf.Max(34, PosingSystemTextGUI.MeasureWidth((new GUIContent(detailsLabel)).text, detailsStyle) + 4);
            // currentViewWidth is stable during Layout/Repaint. Leave room for the
            // Inspector scrollbar, help-box padding and the surrounding margins.
            var availableWidth = Mathf.Max(80, EditorGUIUtility.currentViewWidth - 36 - EditorGUI.indentLevel * 15);
            var separateActions = titleWidth + statusWidth + updateWidth + detailsWidth + 24 > availableWidth;
            var changed = GUI.changed;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (separateActions)
                    {
                        var wrappedTitleWidth = Mathf.Min(titleWidth, availableWidth * .45f);
                        var wrappedStatusWidth = Mathf.Max(20, availableWidth - wrappedTitleWidth - 8);
                        var headerHeight = Mathf.Max(24, PosingSystemTextGUI.MeasureHeight((title).text, label, wrappedTitleWidth), PosingSystemTextGUI.MeasureHeight((statusContent).text, statusLabel, wrappedStatusWidth));
                        PosingSystemTextGUI.LayoutLabel(title, label, GUILayout.Width(wrappedTitleWidth), GUILayout.Height(headerHeight));
                        PosingSystemTextGUI.LayoutLabel(statusContent, statusLabel, GUILayout.ExpandWidth(true), GUILayout.Height(headerHeight));
                    }
                    else
                    {
                        PosingSystemTextGUI.LayoutLabel(title, label, GUILayout.Width(titleWidth), GUILayout.Height(24));
                        PosingSystemTextGUI.LayoutLabel(statusContent, statusLabel, GUILayout.MinWidth(statusWidth), GUILayout.ExpandWidth(true), GUILayout.Height(24));
                        DrawUpdate(updateWidth, 24);
                        DrawDetails(24);
                    }
                }
                if (separateActions)
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var wrappedUpdateWidth = Mathf.Min(updateWidth, Mathf.Max(40, availableWidth - detailsWidth - 8));
                        var actionHeight = Mathf.Max(24, PosingSystemTextGUI.MeasureHeight((update).text, updateStyle, wrappedUpdateWidth));
                        DrawUpdate(wrappedUpdateWidth, actionHeight);
                        GUILayout.FlexibleSpace();
                        DrawDetails(actionHeight);
                    }
                if (state == BuildState.AvatarRequired)
                    PosingSystemTextGUI.LayoutHelpBox(PosingSystemLocalization.Text("プレビルドを実行できません。Inspectorの「設定の確認」で、アバターの配置・Animator・Humanoidの設定を確認してください。"), MessageType.Info);
                if (showDetails)
                {
                if (state == BuildState.Outdated && PosingSystemTextGUI.LayoutButton(PosingSystemLocalization.Text("保存時から変わった項目を確認")))
                {
                    var paths = ChangedInputs(ps);
                    if (PosingSystemLocalization.Dialog("保存時と異なるプレビルドの設定項目", paths,
                        "この警告について相談する", "閉じる")) PosingSystemSupportReport.OpenPrebuild(paths);
                }
                PosingSystemTextGUI.LayoutLabel(PosingSystemLocalization.Text("設定を先に生成して、アバターのビルド時間を短縮します。"), note);
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(state != BuildState.Ready || ps.isIconDisabled || PosingSystemEditor.IsAndroidBuildTarget() || EditorApplication.isPlaying))
                    {
                        if (PosingSystemTextGUI.LayoutButton(new GUIContent(PosingSystemLocalization.Text("アイコンを撮り直す"), PosingSystemLocalization.Text("衣装や見た目を変えたときに使います。設定を変えたときはプレビルドしてください。")), GUILayout.Height(20)))
                        {
                            PosingSystemEditor.TryRenewAvatar(ps);
                            serialized.Update();
                            GUIUtility.ExitGUI();
                        }
                    }
                    if (PosingSystemTextGUI.LayoutButton(PosingSystemLocalization.Text(showImages ? "画像ファイル・補足を閉じる" : "画像ファイル・補足"), GUILayout.Height(20))) showImages = !showImages;
                }
                if (showImages)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(true))
                            PosingSystemTextGUI.LayoutObjectField(PosingSystemLocalization.Text("画像ファイル"), ps.thumbnailPackObject, typeof(PosingSystemThumbnailPack), false);
                        using (new EditorGUI.DisabledScope(ps.thumbnailPackObject == null))
                        {
                            if (PosingSystemTextGUI.LayoutButton(PosingSystemLocalization.Text("削除"), GUILayout.Width(44)) && PosingSystemLocalization.Dialog(PosingSystemLocalization.Text("アイコン画像ファイルを削除"),
                                    PosingSystemLocalization.Text("生成済みの画像ファイルを削除します。削除後は「アイコンを撮り直す」で再生成できます。"), "削除する", "キャンセル"))
                            {
                                Undo.RecordObject(ps, "Delete thumbnail pack");
                                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(ps.thumbnailPackObject));
                                ps.thumbnailPackObject = null;
                                EditorUtility.SetDirty(ps);
                                AssetDatabase.SaveAssets();
                                serialized.Update();
                            }
                        }
                    }
                    PosingSystemTextGUI.LayoutLabel(PosingSystemLocalization.Text("Modular Avatarで追加した衣装などは、アバターのビルド時に撮影し直したアイコンへ反映されます。"), note);
                }
            }
            }
            GUI.changed = changed;

            void DrawUpdate(float width, float height)
            {
                using (new EditorGUI.DisabledScope(state == BuildState.AvatarRequired || EditorApplication.isCompiling || EditorApplication.isPlaying))
                {
                    if (!PosingSystemTextGUI.LayoutButton(update, updateStyle, GUILayout.Width(width), GUILayout.Height(height))) return;
                    PosingSystemEditorChangeUtility.Commit(serialized);
                    PosingSystemEditor.TryPrebuildAndRenew(ps);
                    serialized.Update();
                    GUIUtility.ExitGUI();
                }
            }

            void DrawDetails(float height)
            {
                if (PosingSystemInspectorGUI.LinkLocalized(detailsLabel, GUILayout.Width(detailsWidth), GUILayout.Height(height))) showDetails = !showDetails;
            }
        }
    }
}
