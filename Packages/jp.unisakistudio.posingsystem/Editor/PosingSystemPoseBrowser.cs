using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor
{
    internal sealed class PosingSystemPoseBrowser
    {
        private int category;
        private string search = "";
        private string expanded;
        private bool bulkOpen;
        private int destination;
        private readonly List<AnimationClip> pending = new List<AnimationClip>();
        private string notice;
        private Vector2 listScroll;
        private float expandedRowY;
        private float? pendingRowY;
        private static readonly string[] ImportLabels = { "立ち・歩き", "立ちのみ", "しゃがみ（Crouching）", "伏せ（Prone）", "ジャンプ", "短い落下", "着地（短）", "長い落下", "着地（長）", "選択時" };

        internal void Draw(SerializedObject serialized, PosingSystem ps, bool imports = false)
        {
            var names = (imports ? ImportLabels : ps.defines.Select(d => d.menuName).ToArray()).Select(PosingSystemLocalization.Text).ToArray();
            if (names.Length == 0) { EditorGUILayout.HelpBox(PosingSystemLocalization.Text("姿勢の分類が設定されていません。"), MessageType.Info); return; }
            category = Mathf.Clamp(category, -1, names.Length - 1);
            using (new EditorGUILayout.HorizontalScope(GUILayout.ExpandWidth(true)))
            {
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(72)))
                {
                    Tab(serialized, -1, PosingSystemLocalization.Text("検索"), EditorGUIUtility.IconContent("Search Icon").image);
                    for (var i = 0; i < names.Length; i++) Tab(serialized, i, names[i]);
                }
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(Mathf.Max(150, EditorGUIUtility.currentViewWidth - (imports ? 150 : 134)))))
                {
                    if (category < 0)
                    {
                        EditorGUILayout.LabelField(PosingSystemLocalization.Text("すべての姿勢から検索"), PosingSystemInspectorGUI.Text(12, true));
                        search = EditorGUILayout.TextField(search, EditorStyles.toolbarSearchField);
                    }
                    else EditorGUILayout.LabelField(names[category], PosingSystemInspectorGUI.Text(12, true));
                    var count = 0;
                    using (var scroll = new EditorGUILayout.ScrollViewScope(listScroll, GUILayout.Height(360)))
                    {
                    listScroll = scroll.scrollPosition;
                    if (imports)
                    {
                        for (var i = 0; i < (ps.overrideDefines?.Count ?? 0); i++)
                        {
                            var entry = ps.overrideDefines[i];
                            if (category >= 0 && category != (int)entry.stateType) continue;
                            var title = entry.animationClip != null ? entry.animationClip.name : "アニメーション未設定";
                            if (category < 0 && !PosingSystemPoseListUtility.Matches(search, title, entry.animationClip)) continue;
                            var label = names[Mathf.Clamp((int)entry.stateType, 0, names.Length - 1)];
                            DrawRow(serialized, ps, serialized.FindProperty("overrideDefines").GetArrayElementAtIndex(i), -1, i, title, label, entry, true);
                            count++;
                        }
                    }
                    else for (var layer = 0; layer < ps.defines.Count; layer++)
                    {
                        if (category >= 0 && category != layer) continue;
                        var define = ps.defines[layer];
                        for (var i = 0; i < define.animations.Count; i++)
                        {
                            var entry = define.animations[i];
                            if (category < 0 && !PosingSystemPoseListUtility.Matches(search, entry.displayName, entry.animationClip) && !PosingSystemPoseListUtility.Matches(search, PosingSystemLocalization.Text(entry.displayName), entry.animationClip)) continue;
                            DrawRow(serialized, ps, serialized.FindProperty("defines").GetArrayElementAtIndex(layer).FindPropertyRelative("animations").GetArrayElementAtIndex(i), layer, i,
                                string.IsNullOrWhiteSpace(entry.displayName) ? PosingSystemLocalization.Text("名前なし") : PosingSystemLocalization.Text(entry.displayName), PosingSystemLocalization.Text(define.menuName), entry, false);
                            count++;
                        }
                    }
                    if (count == 0) EditorGUILayout.HelpBox(category < 0 ? PosingSystemLocalization.Text("一致するアニメーションがありません。") : PosingSystemLocalization.Text("この分類にはアニメーションがありません。"), MessageType.None);
                    }
                    if (!imports)
                    {
                        GUILayout.Space(6);
                        if (PosingSystemInspectorGUI.Link(bulkOpen ? "一括追加を閉じる" : "＋ 複数のアニメーションを追加")) { bulkOpen = !bulkOpen; if (category >= 0) destination = category; }
                        if (bulkOpen) DrawBulk(serialized, ps, names);
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        using (new EditorGUI.DisabledScope(category < 0))
                            if (PosingSystemInspectorGUI.Link(PosingSystemLocalization.Text("＋ 1件追加")))
                            {
                                PosingSystemEditorChangeUtility.Commit(serialized); Undo.IncrementCurrentGroup(); Undo.RecordObject(ps, "Add animation");
                                if (imports)
                                {
                                    if (ps.overrideDefines == null) ps.overrideDefines = new List<PosingSystem.OverrideAnimationDefine>();
                                    ps.overrideDefines.Add(new PosingSystem.OverrideAnimationDefine { stateType = (PosingSystem.OverrideAnimationDefine.AnimationStateType)category });
                                }
                                else ps.defines[category].animations.Add(new PosingSystem.AnimationDefine(null, "新しいポーズ"));
                                PosingSystemPoseListUtility.MarkChanged(ps); serialized.Update(); GUIUtility.ExitGUI();
                            }
                    }
                    if (!string.IsNullOrEmpty(notice)) EditorGUILayout.LabelField(notice, PosingSystemInspectorGUI.Text(11));
                }
            }
        }

        private void Tab(SerializedObject serialized, int index, string label, Texture icon = null)
        {
            var rect = GUILayoutUtility.GetRect(72, index < 0 ? 37 : 46);
            var active = category == index;
            EditorGUI.DrawRect(rect, active ? (EditorGUIUtility.isProSkin ? new Color(.23f,.31f,.41f) : new Color(.85f,.91f,.98f)) : PosingSystemInspectorGUI.Surface);
            if (active) EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3, rect.height), PosingSystemInspectorGUI.Accent);
            var style = PosingSystemInspectorGUI.Text(11, active); style.alignment = TextAnchor.MiddleCenter;
            if (GUI.Button(rect, new GUIContent(label, icon), style)) { PosingSystemEditorChangeUtility.Commit(serialized); category = index; expanded = null; pendingRowY = null; listScroll = Vector2.zero; GUIUtility.ExitGUI(); }
            GUILayout.Space(2);
        }

        private void DrawRow(SerializedObject serialized, PosingSystem ps, SerializedProperty property, int layer, int index,
            string title, string categoryName, PosingSystem.BaseAnimationDefine entry, bool imports)
        {
            var key = layer + ":" + index;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var rect = GUILayoutUtility.GetRect(0, category < 0 ? 72 : 59, GUILayout.ExpandWidth(true));
                if (expanded == key && Event.current.type == EventType.Repaint) AnchorExpandedRow(rect.y);
                var preview = entry.previewImage;
                if (entry is PosingSystem.AnimationDefine pose && pose.isCustomIcon && pose.icon != null) preview = pose.icon;
                if (preview == null && entry.animationClip != null) preview = AssetPreview.GetAssetPreview(entry.animationClip) ?? AssetPreview.GetMiniThumbnail(entry.animationClip);
                var image = new Rect(rect.x + 2, rect.y + 6, 40, 40);
                EditorGUI.DrawRect(image, EditorGUIUtility.isProSkin ? new Color(.16f,.17f,.19f) : new Color(.86f,.88f,.91f));
                if (preview != null) GUI.DrawTexture(image, preview, ScaleMode.ScaleToFit);
                if (entry is PosingSystem.AnimationDefine rollPose && rollPose.isRollover)
                    PosingSystemRolloverIcon.Draw(image);
                var x = rect.x + 49;
                var textWidth = Mathf.Max(50, rect.width - 80);
                var initial = entry is PosingSystem.AnimationDefine p && p.initial;
                if (category < 0) GUI.Label(new Rect(x, rect.y + 1, textWidth, 16), categoryName + (initial ? "  · " + PosingSystemLocalization.Text("初期ポーズ") : ""), PosingSystemInspectorGUI.Text(10));
                GUI.Label(new Rect(x, rect.y + (category < 0 ? 19 : 3), textWidth, 30), title, PosingSystemInspectorGUI.Text(12, true));
                var enabled = property.FindPropertyRelative("enabled");
                var checkRect = new Rect(rect.xMax - 23, rect.y + 5, 21, 21);
                EditorGUI.BeginProperty(checkRect, new GUIContent(PosingSystemLocalization.Text("このポーズを使う")), enabled);
                enabled.boolValue = PosingSystemInspectorGUI.Check(checkRect, enabled.boolValue, new GUIContent("", "このポーズを使う"));
                EditorGUI.EndProperty();
                var previewing = PosingSystemQuickPreview.IsPreviewing(ps, entry);
                using (new EditorGUI.DisabledScope(!(entry.animationClip is AnimationClip) || ps.GetAvatar() == null || Application.isPlaying))
                    if (GUI.Button(new Rect(x, rect.yMax - 23, 80, 21), new GUIContent(previewing ? PosingSystemLocalization.Text("終了") : PosingSystemLocalization.Text("プレビュー"), PosingSystemLocalization.Text("最初の姿勢をSceneタブで確認できます。終了すると元の姿勢に戻ります。"))))
                    {
                        PosingSystemEditorChangeUtility.Commit(serialized);
                        if (previewing) PosingSystemQuickPreview.Stop();
                        else
                        {
                            var current = imports ? (PosingSystem.BaseAnimationDefine)ps.overrideDefines[index] : ps.defines[layer].animations[index];
                            var poseKey = imports ? current.animationClip.name : PosingSystemAutoPoseCatalog.BuildPoseKey(ps.defines[layer].menuName, ps.defines[layer].animations[index].displayName);
                            if (!PosingSystemQuickPreview.Start(ps, current, poseKey, out var message))
                                PosingSystemLocalization.Dialog(PosingSystemLocalization.Text("姿勢のプレビュー"), message, "閉じる");
                        }
                        GUIUtility.ExitGUI();
                    }
                if (category >= 0 && initial) GUI.Label(new Rect(x + 84, rect.yMax - 21, Mathf.Max(0, textWidth - 112), 18), PosingSystemLocalization.Text("初期ポーズ"), PosingSystemInspectorGUI.Text(10));
                if (PosingSystemInspectorGUI.Link(new Rect(rect.xMax - 29, rect.yMax - 20, 29, 18), expanded == key ? PosingSystemLocalization.Text("閉じる") : PosingSystemLocalization.Text("詳細")))
                {
                    SwitchDetail(serialized, key);
                    GUIUtility.ExitGUI();
                }
                if (expanded == key) DrawDetail(serialized, ps, property, layer, index, imports);
            }
            GUILayout.Space(3);
        }

        private void DrawDetail(SerializedObject serialized, PosingSystem ps, SerializedProperty property, int layer, int index, bool imports)
        {
            var oldLabelWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = Mathf.Min(125, EditorGUIUtility.currentViewWidth * .30f);
            try
            {
                EditorGUI.BeginChangeCheck();
                if (imports) Field("stateType", "姿勢の分類"); else Field("displayName", "ポーズ名");
                Field("animationClip", "アニメーション");
                if (!imports)
                {
                    Field("isCustomIcon", "アイコンを指定する");
                    if (property.FindPropertyRelative("isCustomIcon").boolValue) Field("icon", "アイコン画像");
                }
                if (!imports) Field("isRollover", "寝返りを使う");
                var rollover = !imports && property.FindPropertyRelative("isRollover").boolValue;
                using (new EditorGUI.DisabledScope(rollover))
                {
                    Field("isMotionTime", "MotionTimeを使う");
                    if (property.FindPropertyRelative("isMotionTime").boolValue) Field("motionTimeParamName", "操作パラメーター");
                }
                if (rollover)
                {
                    Field("rolloverSideAnimation", "寝返り時の横向き");
                    if (property.FindPropertyRelative("rolloverSideAnimation").objectReferenceValue != null)
                    {
                        Field("rolloverContactStrength", "接地補正の強さ");
                        EditorGUILayout.HelpBox(PosingSystemLocalization.Text("通常のアニメーションを中央にし、横向きの姿勢と左右反転をビルド時に合成します。各クリップの最初の姿勢を使用します。中央は0.5、左右端は0と1です。接地補正は0で無効、0.25で25％です。"), MessageType.None);
                    }
                    else
                        EditorGUILayout.HelpBox(PosingSystemLocalization.Text("横向きのAnimationClipを指定してください。未指定の場合は、従来どおり通常欄の完成済み寝返りアニメーションを使います。"), MessageType.Info);
                    if (!ps.features.rollover)
                        EditorGUILayout.HelpBox(PosingSystemLocalization.Text("ゲーム内で使うには「機能」タブの「寝返り」もオンにしてください。"), MessageType.Info);
                }
                Field("isRotate", "向きを調整する");
                if (property.FindPropertyRelative("isRotate").boolValue) Field("rotate", "回転角度");
                Field("adjustmentClip", "調整アニメーション");
                if (EditorGUI.EndChangeCheck()) property.FindPropertyRelative("previewImage").objectReferenceValue = null;
                if (!imports)
                {
                    var initial = property.FindPropertyRelative("initial").boolValue;
                    var next = EditorGUILayout.ToggleLeft(PosingSystemLocalization.Text("この分類の初期ポーズにする"), initial);
                    if (next != initial)
                    {
                        PosingSystemEditorChangeUtility.Commit(serialized); Undo.IncrementCurrentGroup(); PosingSystemPoseListUtility.SetInitial(ps, layer, index, next); serialized.Update();
                    }
                }
                var array = imports ? serialized.FindProperty("overrideDefines") : serialized.FindProperty("defines").GetArrayElementAtIndex(layer).FindPropertyRelative("animations");
                var positions = Enumerable.Range(1, array.arraySize).Select(i => i + "番").ToArray();
                var nextIndex = EditorGUILayout.Popup(new GUIContent(PosingSystemLocalization.Text("順番"), PosingSystemLocalization.Text("この分類の中で移動先の順番を選べます。")), index, positions);
                if (nextIndex != index) Move(nextIndex);
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(index == 0)) if (GUILayout.Button("↑", GUILayout.Width(28))) Move(index - 1);
                    using (new EditorGUI.DisabledScope(index == array.arraySize - 1)) if (GUILayout.Button("↓", GUILayout.Width(28))) Move(index + 1);
                    GUILayout.FlexibleSpace();
                    if (PosingSystemInspectorGUI.Link(PosingSystemLocalization.Text("一覧から削除"), GUILayout.Width(72))) { PosingSystemEditorChangeUtility.DeleteArrayElement(array, index); Finish(); }
                }
            }
            finally { EditorGUIUtility.labelWidth = oldLabelWidth; }
            void Field(string name, string label)
            {
                GUI.SetNextControlName(property.propertyPath + "." + name);
                EditorGUILayout.PropertyField(property.FindPropertyRelative(name), new GUIContent(PosingSystemLocalization.Text(label)));
            }
            void Move(int to) { MoveRow(serialized, ps, layer, index, to, imports); GUI.FocusControl(null); GUIUtility.ExitGUI(); }
            void Finish() { PosingSystemEditorChangeUtility.Commit(serialized); expanded = null; pendingRowY = null; serialized.Update(); GUIUtility.ExitGUI(); }
        }

        private bool MoveRow(SerializedObject serialized, PosingSystem ps, int layer, int from, int to, bool imports)
        {
            var array = imports ? serialized.FindProperty("overrideDefines") : serialized.FindProperty("defines").GetArrayElementAtIndex(layer).FindPropertyRelative("animations");
            if (from < 0 || to < 0 || from >= array.arraySize || to >= array.arraySize || from == to) return false;
            PosingSystemEditorChangeUtility.Commit(serialized);
            Undo.IncrementCurrentGroup();
            Undo.SetCurrentGroupName("ポーズの順番を変更");
            array.MoveArrayElement(from, to);
            serialized.ApplyModifiedProperties();
            PosingSystemPoseListUtility.MarkChanged(ps);
            expanded = layer + ":" + to;
            pendingRowY = expandedRowY;
            serialized.Update();
            return true;
        }

        private void SwitchDetail(SerializedObject serialized, string key)
        {
            PosingSystemEditorChangeUtility.Commit(serialized);
            expanded = expanded == key ? null : key;
            pendingRowY = null;
        }

        private void AnchorExpandedRow(float y)
        {
            // Keep the moved row at the same viewport position, so the arrow remains under the cursor.
            if (pendingRowY.HasValue)
            {
                listScroll.y = Mathf.Max(0, listScroll.y + y - pendingRowY.Value);
                pendingRowY = null;
                UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
            }
            expandedRowY = y;
        }

        private void DrawBulk(SerializedObject serialized, PosingSystem ps, string[] names)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                destination = Mathf.Clamp(destination, 0, names.Length - 1);
                EditorGUILayout.LabelField(PosingSystemLocalization.Text("追加先の姿勢"), PosingSystemInspectorGUI.Text(11));
                destination = EditorGUILayout.Popup(destination, names);
                var drop = GUILayoutUtility.GetRect(0, 52, GUILayout.ExpandWidth(true));
                GUI.Box(drop, GUIContent.none);
                var style = PosingSystemInspectorGUI.Text(11); style.alignment = TextAnchor.MiddleCenter;
                GUI.Label(drop, PosingSystemLocalization.Text("複数のアニメーションをここにドロップ"), style);
                var evt = Event.current;
                if (drop.Contains(evt.mousePosition) && (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform))
                {
                    var clips = PosingSystemPoseListUtility.CollectClips(DragAndDrop.objectReferences);
                    DragAndDrop.visualMode = clips.Count > 0 ? DragAndDropVisualMode.Copy : DragAndDropVisualMode.Rejected;
                    if (evt.type == EventType.DragPerform && clips.Count > 0) { DragAndDrop.AcceptDrag(); Stage(clips); }
                    evt.Use();
                }
                GUILayout.Label(PosingSystemLocalization.Text("または"), style);
                if (GUILayout.Button(PosingSystemLocalization.Text("Projectで選択中のクリップを読み込む"), new GUIStyle(GUI.skin.button) { wordWrap = true }, GUILayout.Height(36))) Stage(PosingSystemPoseListUtility.CollectClips(Selection.objects));
                EditorGUILayout.LabelField(PosingSystemLocalization.Text("Projectで選ぶときは、Inspector右上の鍵を閉じてから選択してください。"), PosingSystemInspectorGUI.Text(11));
                pending.RemoveAll(clip => clip == null);
                for (var i = 0; i < pending.Count; i++)
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        EditorGUILayout.ObjectField(pending[i], typeof(AnimationClip), false);
                        if (GUILayout.Button("×", GUILayout.Width(23))) { pending.RemoveAt(i); break; }
                    }
                var existing = new HashSet<Motion>(ps.defines[destination].animations.Select(a => a.animationClip));
                var count = pending.Count(clip => !existing.Contains(clip));
                using (new EditorGUI.DisabledScope(count == 0))
                    if (GUILayout.Button(PosingSystemLocalization.Format("{0}件を「{1}」に追加", count, names[destination]), GUILayout.Height(28)))
                    {
                        PosingSystemEditorChangeUtility.Commit(serialized);
                        var added = PosingSystemPoseListUtility.AddClips(ps, destination, pending);
                        pending.Clear(); category = destination; notice = PosingSystemLocalization.Format("{0}件追加しました。", added); serialized.Update(); GUIUtility.ExitGUI();
                    }
                if (pending.Count > count) EditorGUILayout.LabelField(PosingSystemLocalization.Text("追加先にあるクリップは重複して追加されません。"), PosingSystemInspectorGUI.Text(11));
            }
        }
        private void Stage(IEnumerable<AnimationClip> clips)
        {
            var added = 0;
            foreach (var clip in clips) if (!pending.Contains(clip)) { pending.Add(clip); added++; }
            notice = added == 0 ? "追加できる新しいアニメーションクリップがありません。" : "候補を確認してから追加できます。";
        }
    }
}
