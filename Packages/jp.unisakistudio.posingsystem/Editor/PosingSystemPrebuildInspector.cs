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
                                paths.Add(path + "（生成方式）");
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
                return paths.Count == 0 ? "保存された設定の形式・表記" : string.Join("\n", paths);
            }
            catch (JsonException) { return "旧形式の保存データ（項目を比較できません）"; }
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
            var label = new GUIStyle(EditorStyles.boldLabel) { font = EditorStyles.boldFont, fontSize = 12, alignment = TextAnchor.MiddleLeft };
            var statusLabel = new GUIStyle(note) { alignment = TextAnchor.MiddleLeft, margin = new RectOffset(label.margin.left, label.margin.right, label.margin.top, label.margin.bottom) };
            var changed = GUI.changed;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    GUILayout.Label("プレビルド", label, GUILayout.Width(76), GUILayout.Height(24));
                    var status = state == BuildState.Ready ? "最新" : state == BuildState.Outdated ? "設定の更新あり" :
                        state == BuildState.NotBuilt ? "未実行" : "アバターの設定が必要";
                    GUILayout.Label(status, statusLabel, GUILayout.Height(24));
                    using (new EditorGUI.DisabledScope(state == BuildState.AvatarRequired || EditorApplication.isCompiling || EditorApplication.isPlaying))
                    {
                        if (GUILayout.Button("プレビルドを更新", GUILayout.Width(120), GUILayout.Height(24)))
                        {
                            PosingSystemEditorChangeUtility.Commit(serialized);
                            PosingSystemEditor.TryPrebuildAndRenew(ps);
                            serialized.Update();
                            GUIUtility.ExitGUI();
                        }
                    }
                    if (PosingSystemInspectorGUI.Link(showDetails ? "閉じる" : "詳細", GUILayout.Width(34), GUILayout.Height(24))) showDetails = !showDetails;
                }
                if (state == BuildState.AvatarRequired)
                    EditorGUILayout.HelpBox("プレビルドを実行できません。Inspectorの「設定の確認」で、アバターの配置・Animator・Humanoidの設定を確認してください。", MessageType.Info);
                if (showDetails)
                {
                if (state == BuildState.Outdated && GUILayout.Button("保存時から変わった項目を確認"))
                {
                    var paths = ChangedInputs(ps);
                    if (EditorUtility.DisplayDialog("保存時と異なるプレビルドの設定項目", paths,
                        "この警告について相談する", "閉じる")) PosingSystemSupportReport.OpenPrebuild(paths);
                }
                GUILayout.Label("設定を先に生成して、アバターのビルド時間を短縮します。", note);
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(state != BuildState.Ready || ps.isIconDisabled || PosingSystemEditor.IsAndroidBuildTarget() || EditorApplication.isPlaying))
                    {
                        if (GUILayout.Button(new GUIContent("アイコンを撮り直す", "衣装や見た目を変えたときに使います。設定を変えたときはプレビルドしてください。"), GUILayout.Height(20)))
                        {
                            PosingSystemEditor.TryRenewAvatar(ps);
                            serialized.Update();
                            GUIUtility.ExitGUI();
                        }
                    }
                    if (GUILayout.Button(showImages ? "画像ファイル・補足を閉じる" : "画像ファイル・補足", GUILayout.Height(20))) showImages = !showImages;
                }
                if (showImages)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(true))
                            EditorGUILayout.ObjectField("画像ファイル", ps.thumbnailPackObject, typeof(PosingSystemThumbnailPack), false);
                        using (new EditorGUI.DisabledScope(ps.thumbnailPackObject == null))
                        {
                            if (GUILayout.Button("削除", GUILayout.Width(44)) && EditorUtility.DisplayDialog("アイコン画像ファイルを削除",
                                "生成済みの画像ファイルを削除します。削除後は「アイコンを撮り直す」で再生成できます。", "削除する", "キャンセル"))
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
                    GUILayout.Label("Modular Avatarで追加した衣装などは、アバターのビルド時に撮影し直したアイコンへ反映されます。", note);
                }
            }
            }
            GUI.changed = changed;
        }
    }
}
