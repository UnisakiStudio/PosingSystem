using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor
{
    public partial class PosingSystemEditor
    {
        private bool breathingOpen, kneeOpen, importOpen, presetManagementOpen, troubleshootingOpen;
        private readonly PosingSystemPoseBrowser poseBrowser = new PosingSystemPoseBrowser();
        private readonly PosingSystemMotionImportInspector importBrowser = new PosingSystemMotionImportInspector();
        private readonly PosingSystemDiagnosticsInspector diagnosticsInspector = new PosingSystemDiagnosticsInspector();
        private Texture2D tabBackground;
        private Color tabBackgroundColor;
        private bool drewInspectorWarning;

        private void DrawInspectorWarning(string message)
        {
            drewInspectorWarning = true;
            EditorGUILayout.HelpBox(message, MessageType.Warning);
        }

        private void DrawUserInspector(PosingSystem ps)
        {
            serializedObject.Update();
            diagnosticsInspector.Draw(serializedObject, ps);
            prebuildInspector.Draw(serializedObject, ps);
            DrawInspectorTabs();
            var color = PosingSystemInspectorGUI.Surface;
            if (tabBackground == null)
            {
                tabBackground = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
                // Hot reload can retain the cached color after Unity destroys the texture.
                tabBackgroundColor = Color.clear;
            }
            if (tabBackgroundColor != color)
            {
                tabBackgroundColor = color; tabBackground.SetPixel(0, 0, color); tabBackground.Apply();
            }
            var panel = new GUIStyle { padding = new RectOffset(8, 8, 8, 8), normal = { background = tabBackground } };
            using (new EditorGUILayout.VerticalScope(panel))
            {
                if (_selectedTab == InspectorTab.Settings)
                    PosingSystemSettingsInspector.Draw(serializedObject, ps, ref breathingOpen, () => DrawBreathing(ps));
                var valid = ValidateAvatar(ps);
                if (_selectedTab == InspectorTab.PoseAnimations)
                {
                    using (new EditorGUI.DisabledScope(!valid))
                        if (DrawAnimationAdjustmentLauncher())
                        {
                            PosingSystemEditorChangeUtility.Commit(serializedObject);
                            PosingAnimationAdjustmentWindow.ShowWindow(ps);
                            GUIUtility.ExitGUI();
                        }
                    poseBrowser.Draw(serializedObject, ps);
                    using (new EditorGUI.DisabledScope(!valid || IsAndroidBuildTarget() || Application.isPlaying))
                        if (GUILayout.Button("ポーズ画像を更新", GUILayout.Height(26)) && EditorUtility.DisplayDialog(
                            "ポーズ画像を更新", "すべてのポーズ画像を作り直します。ポーズの数によっては少し時間がかかります。\n更新中はUnityの操作をお待ちください。", "更新する", "キャンセル"))
                        {
                            PosingSystemEditorChangeUtility.Commit(serializedObject);
                            PosingSystemQuickPreview.Stop();
                            ClearThumbnailPack(ps); CreateThumbnailPack(ps);
                            PosingSystemConverter.TakeScreenshot(ps, true, false);
                            serializedObject.Update(); GUIUtility.ExitGUI();
                        }
                }
                if (_selectedTab == InspectorTab.AvatarSpecific) DrawAvatarSettings(ps, valid);
                if (_selectedTab == InspectorTab.Settings && valid)
                {
                    GUILayout.Space(8);
                    if (PosingSystemInspectorGUI.Link(troubleshootingOpen ? "トラブルシューティングを閉じる" : "トラブルシューティング")) troubleshootingOpen = !troubleshootingOpen;
                    if (troubleshootingOpen && GUILayout.Button("アバターの姿勢をリセット（Tポーズに戻す）")) ResetAvatarPose(ps.GetAvatar());
                }
            }
            if (PosingSystemEditorChangeUtility.Apply(serializedObject))
            {
                ps.previousErrorCheckTime = System.DateTime.MinValue;
                PosingSystemFeatureUtility.Synchronize(ps, true);
            }
        }

        private static bool DrawAnimationAdjustmentLauncher()
        {
            const string title = "アニメーション調整";
            const string description = "姿勢のプレビューや、細かな調整ができます。";
            var titleStyle = PosingSystemInspectorGUI.Text(13, true);
            var descriptionStyle = PosingSystemInspectorGUI.Text(11);
            var layoutStyle = new GUIStyle(descriptionStyle)
            {
                padding = new RectOffset(76, 64, 32, 10)
            };
            var rect = GUILayoutUtility.GetRect(new GUIContent(description), layoutStyle,
                GUILayout.MinHeight(74), GUILayout.ExpandWidth(true));
            var hovered = GUI.enabled && rect.Contains(Event.current.mousePosition);
            var accent = PosingSystemInspectorGUI.Accent;
            var background = Color.Lerp(PosingSystemInspectorGUI.Surface, accent, hovered ? .18f : .08f);
            EditorGUI.DrawRect(rect, hovered ? accent : PosingSystemInspectorGUI.Border);
            EditorGUI.DrawRect(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2), background);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, 3, rect.height), accent);

            // Keep one native button hit area, including the icon and the explanation.
            // Opening the editor is navigation, not a serialized setting change.
            var wasChanged = GUI.changed;
            var clicked = GUI.Button(rect, new GUIContent(string.Empty, title + "を開きます。" + description), GUIStyle.none);
            GUI.changed = wasChanged;
            if (GUI.enabled) EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);

            PosingSystemSettingIcons.Draw(new Rect(rect.x + 6, rect.center.y - 32, 64, 64), "animation-adjustment");
            var textWidth = Mathf.Max(1, rect.width - 140);
            GUI.Label(new Rect(rect.x + 76, rect.y + 10, textWidth, 19), title, titleStyle);
            GUI.Label(new Rect(rect.x + 76, rect.y + 32, textWidth, rect.height - 42), description, descriptionStyle);
            var openRect = new Rect(rect.xMax - 57, rect.center.y - 14, 48, 28);
            EditorGUI.DrawRect(openRect, accent);
            var openStyle = PosingSystemInspectorGUI.Text(11, true);
            openStyle.alignment = TextAnchor.MiddleCenter;
            openStyle.normal.textColor = Color.white;
            GUI.Label(openRect, "開く ›", openStyle);
            GUILayout.Space(6);
            return clicked;
        }

        private void DrawBreathing(PosingSystem ps)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("呼吸の設定", PosingSystemInspectorGUI.Text(12, true));
                EditorGUILayout.LabelField("弱・中・強はゆっくりした呼吸、急は速い呼吸です。姿勢ごとに選べます。", PosingSystemInspectorGUI.Text(11));
                using (new EditorGUI.DisabledScope(!serializedObject.FindProperty("features.breathing").boolValue))
                    foreach (var name in GetAdditiveBreathingDisplayNames(ps))
                        DrawAdditiveBreathingLevel(serializedObject.FindProperty("additiveBreathing"), name);
            }
            if (PosingSystemEditorChangeUtility.Apply(serializedObject)) PosingSystemFeatureUtility.Synchronize(ps, true);
            serializedObject.Update();
        }

        private void DrawAvatarSettings(PosingSystem ps, bool valid)
        {
            PosingSystemAvatarAdjustmentInspector.EnsureUsageDefault(ps);
            serializedObject.Update();
            PosingSystemInspectorGUI.Section("身体の補正");
            var unavailable = ps.UsesAutoPoseCatalog || !PosingSystemAvatarAdjustmentUtility.HasReferenceProfile(ps);
            PosingSystemSettingsInspector.DrawCards(serializedObject, ps, new[] {
                new PosingSystemSettingsInspector.Setting("enableAvatarSpecificAdjustment", "アバター固有補正を使う", "アバターごとの関節の向きやひねりの違いを自動で補正します。"),
                new PosingSystemSettingsInspector.Setting("enableReverseKneeSafetyClamp", "膝が逆向きに曲がるのを防ぐ", "脚を伸ばしたときに、膝が逆向きに折れるのを抑えます。") },
                setting => setting.Path == "enableReverseKneeSafetyClamp" ? (kneeOpen ? "▼ 設定を閉じる" : "▶ 設定を開く") : null,
                setting => kneeOpen = !kneeOpen, null, ref breathingOpen, disabled: unavailable);
            if (unavailable) EditorGUILayout.HelpBox(ps.UsesAutoPoseCatalog ? "姿勢別IK自動調整を使用中です。従来の身体の補正は使用しません。" : "このツールにはアバター固有補正用の基準データがありません。", MessageType.Info);
            if (kneeOpen)
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                using (new EditorGUI.DisabledScope(unavailable || !ps.enableReverseKneeSafetyClamp))
                {
                    EditorGUILayout.LabelField("膝の補正", PosingSystemInspectorGUI.Text(12, true));
                    EditorGUILayout.Slider(serializedObject.FindProperty("reverseKneeSafetyAngle"), 0, 5, "最低膝曲げ角度（°）");
                    EditorGUILayout.LabelField("膝が伸びきる手前で止める角度です。", PosingSystemInspectorGUI.Text(11));
                }
            if (PosingSystemEditorChangeUtility.Apply(serializedObject))
            {
                Undo.RecordObject(ps, "Initialize avatar adjustment settings");
                ps.avatarSpecificAdjustmentUsageInitialized = true;
                PosingSystemEditorChangeUtility.MarkChanged(ps);
            }
            PosingSystemAvatarAdjustmentInspector.ScheduleAutomaticGeneration(ps);
            serializedObject.Update();

            PosingSystemInspectorGUI.Section("アバターとの組み合わせ");
            PosingSystemSettingsInspector.DrawCards(serializedObject, ps, new[] {
                new PosingSystemSettingsInspector.Setting("autoImportAvatarAnimations", "アバターの元の立ち・しゃがみ・伏せ姿勢を使う", "生成時にアバターのBaseからモーションを検出します。取り込み設定の一覧には追加しません。手動設定がある種類は手動指定を優先します。"),
                new PosingSystemSettingsInspector.Setting("mergeTrackingControl", "各固定機能の他ギミックとの統合", "ほかのギミックとの併用によって固定機能がうまく動かない場合はオンにしてください。") },
                null, null, null, ref breathingOpen);

            PosingSystemInspectorGUI.Section("自分で用意したモーションを使う");
            EditorGUILayout.LabelField("種類ごとに1つのモーションを指定できます。上の「元の姿勢を使う」とは別に設定できます。", PosingSystemInspectorGUI.Text(11));
            if (PosingSystemInspectorGUI.Link(importOpen ? "▼ 取り込み設定を閉じる" : "▶ 取り込み設定を開く"))
            {
                PosingSystemEditorChangeUtility.Commit(serializedObject);
                importOpen = !importOpen;
                GUIUtility.ExitGUI();
            }
            if (importOpen)
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("取り込むアニメーションを個別に指定", PosingSystemInspectorGUI.Text(12, true));
                    EditorGUILayout.LabelField("指定した種類にはこのモーションを使います。未指定の種類は元の設定を使います。", PosingSystemInspectorGUI.Text(11));
                    using (new EditorGUI.DisabledScope(!valid))
                        if (GUILayout.Button("現在のアバターから読み込む"))
                        {
                            PosingSystemEditorChangeUtility.Commit(serializedObject);
                            Undo.RecordObject(ps, "Import avatar animations");
                            AutoDetectOverrideSettings();
                            PosingSystemPoseListUtility.MarkChanged(ps);
                            serializedObject.Update();
                        }
                    importBrowser.Draw(serializedObject, ps, DrawInspectorWarning);
                }
            PosingSystemEditorChangeUtility.Apply(serializedObject);
            PosingSystemInspectorGUI.Section("プリセット");
            EditorGUILayout.LabelField("使い始めるときの姿勢セットを変更できます。変更すると現在のポーズ設定が置き換わります。", PosingSystemInspectorGUI.Text(11));
            presetBrowser.Draw(ps, DrawInspectorWarning);
            serializedObject.Update();
            if (PosingSystemInspectorGUI.Link(presetManagementOpen ? "プリセットの保存・管理を閉じる" : "プリセットの保存・管理")) presetManagementOpen = !presetManagementOpen;
            if (presetManagementOpen && GUILayout.Button("現在の設定からプリセットを作成")) PosingPresetCreatorWindow.ShowWindow(ps);
        }

        private void DrawInspectorTabs()
        {
            GUILayout.Space(4);
            var row = GUILayoutUtility.GetRect(0, 31, GUILayout.ExpandWidth(true));
            var width = (row.width - 6) / InspectorTabLabels.Length;
            for (var i = 0; i < InspectorTabLabels.Length; i++)
            {
                var selected = (int)_selectedTab == i;
                var rect = new Rect(row.x + i * (width + 3), row.y + (selected ? 0 : 3), width, selected ? 31 : 28);
                if (GUI.Button(rect, GUIContent.none, GUIStyle.none))
                {
                    PosingSystemEditorChangeUtility.Commit(serializedObject);
                    _selectedTab = (InspectorTab)i; Repaint(); GUIUtility.ExitGUI();
                }
                EditorGUI.DrawRect(rect, PosingSystemInspectorGUI.Border);
                EditorGUI.DrawRect(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - (selected ? 1 : 2)),
                    selected ? PosingSystemInspectorGUI.Surface : EditorGUIUtility.isProSkin ? new Color(.15f,.16f,.18f) : new Color(.81f,.83f,.86f));
                if (selected) EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 3), PosingSystemInspectorGUI.Accent);
                var style = PosingSystemInspectorGUI.Text(12, selected); style.alignment = TextAnchor.MiddleCenter;
                if (selected) style.normal.textColor = PosingSystemInspectorGUI.Accent;
                GUI.Label(rect, InspectorTabLabels[i], style);
            }
            GUILayout.Space(-2);
        }
    }
}
