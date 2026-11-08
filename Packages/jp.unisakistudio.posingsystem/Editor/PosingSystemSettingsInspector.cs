using System;
using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    /// <summary>One settings surface for menu features, icons and build integration.</summary>
    internal static class PosingSystemSettingsInspector
    {
        internal sealed class Setting
        {
            internal readonly string Path, Title, Description;
            internal readonly int Bits;
            internal readonly bool Inverted, SharedHead;

            internal Setting(string path, string title, string description, int bits = 0,
                bool inverted = false, bool sharedHead = false)
            {
                Path = path; Title = title; Description = description; Bits = bits;
                Inverted = inverted; SharedHead = sharedHead;
            }
        }

        private sealed class SettingGroup
        {
            internal readonly string Title;
            internal readonly Setting[] Settings;

            internal SettingGroup(string title, params Setting[] settings)
            {
                Title = title;
                Settings = settings;
            }
        }

        private static readonly SettingGroup[] Groups =
        {
            new SettingGroup("メニュー追加",
            new Setting("features.mirror", "左右反転", "VRChatのメニューに切り替え項目を追加します。使いたいときに、ポーズの左右を反転できます。", 1),
            new Setting("features.fingerGestureControl", "ジェスチャー無効", "指の形を固定・解除する切り替えをメニューに追加します。固定中はコントローラーを握っても形を保てます。", 1),
            new Setting("features.breathingControl", "呼吸", "VRChatのメニューで呼吸をつけたり止めたりできます。ON/OFFは他の人にも反映されます。", 1),
            new Setting("features.rollover", "寝返り", "頭の向きに合わせて寝返りできます。一部の寝姿勢のみ対応しています。", 10, sharedHead: true),
            new Setting("features.footHeight", "足の高さ", "頭の位置を保ち、足側の高さを変えられます。足が床に埋まる・浮くときに調整できます。", 8),
            new Setting("features.floating", "床の高さ", "頭も含めた身体全体の高さを変えられます。VRChatの「床の高さ」で、50%を元の高さとして上下3mまで調整できます。", 8),
            new Setting("features.photography", "撮影モード", "メニューに撮影モードを追加します。前へ最大3m移動したり回転したりして、自分から見える位置を調整できます。")),

            new SettingGroup("便利機能",
            new Setting("disableSleepJumpSwitch", "ジャンプで寝姿勢を切り替える", "仰向けとうつ伏せの両方を選んでいるとき、その場でジャンプすると切り替わります。", inverted: true),
            new Setting("features.faceForward", "首の向きの違いによる姿勢崩れを抑制", "首固定を解除したとき、元のポーズと首の向きが異なることで起きる姿勢崩れを抑えます。", 1, sharedHead: true),
            new Setting("features.photographySynced", "撮影モードを同期する", "撮影モードで動かした位置と向きを他の人にも表示できます。自分の画面だけで撮影する場合は0 bitで使えます。", 16),
            new Setting("features.photographyKeepHeadVisible", "撮影モード中は頭を非表示にしない", "自分の頭が近くにあっても撮影できます。自分の画面だけに反映され、撮影終了後は通常の表示に戻ります。"),
            new Setting("additiveBreathing.disableDefaultIdle", "VRChat標準の揺れを止める", "立ち止まっているときに自動で入る、VRChat標準の身体の揺れを止められます。ポーズツールで設定した呼吸の動きはそのまま使えます。"),
            new Setting("features.breathing", "呼吸の動きをつける", "呼吸に合わせて胸などが動きます。姿勢ごとに強さを設定でき、同期bitを使わずに利用できます。")),

            new SettingGroup("メニューのアイコン画像",
            new Setting("isIconDisabled", "ポーズアイコン生成", "画像でポーズを見分けやすくできます。アバターのビルド時間は少し長くなります。", inverted: true),
            new Setting("isIconSmall", "アイコン画像の容量を減らす", "アバターの容量を抑えたいときに便利です。アイコン画像は少し粗くなります。")),

        };

        private static GUIStyle titleStyle, descriptionStyle, badgeStyle, summaryStyle, groupStyle;
        private static bool stylesAreDark;
        private static Color Accent => EditorGUIUtility.isProSkin
            ? new Color(.40f, .69f, 1f) : new Color(.13f, .36f, .64f);

        internal static void Draw(SerializedObject serialized, PosingSystem ps)
        {
            var open = false;
            Draw(serialized, ps, ref open, null);
        }

        internal static void Draw(SerializedObject serialized, PosingSystem ps, ref bool breathingOpen, Action drawBreathing)
        {
            PosingSystemFeatureUtility.Migrate(ps);
            serialized.Update();
            EnsureStyles();
            DrawSummary(ps);
            foreach (var group in Groups)
            {
                PosingSystemInspectorGUI.Section(PosingSystemLocalization.Text(group.Title));
                DrawCards(serialized, ps, group.Settings, setting => setting.Path == "features.breathing" ? "設定" : null,
                    setting => { }, null, ref breathingOpen, drawBreathing);
            }
            GUILayout.Label(PosingSystemLocalization.Text("※ 寝返りと姿勢崩れ抑制のbit表示は、今の状態でチェックを変更したときの増減量です。首固定の1 bitは共用します。"), descriptionStyle);
            if (PosingSystemInspectorGUI.Link(PosingSystemLocalization.Text("追加機能を外して、ExpressionParametersの使用量を減らす")))
            {
                foreach (var group in Groups)
                    foreach (var setting in group.Settings)
                        if (setting.Path.StartsWith("features.", StringComparison.Ordinal))
                            serialized.FindProperty(setting.Path).boolValue = false;
                Apply(serialized, ps);
            }
            DrawDetails(serialized, ps);
        }

        internal static void DrawCards(SerializedObject serialized, PosingSystem ps, Setting[] settings,
            Func<Setting, string> detailLabel, Action<Setting> onDetail, Action afterRow,
            ref bool breathingOpen, Action drawBreathing = null, bool disabled = false)
        {
            EnsureStyles();
            var columns = EditorGUIUtility.currentViewWidth >= 460 ? 2 : 1;
            var estimatedWidth = (EditorGUIUtility.currentViewWidth - 64 - (columns - 1) * 8) / columns;
            var android = PosingSystemEditor.IsAndroidBuildTarget();
            for (var first = 0; first < settings.Length; first += columns)
            {
                var height = 0f;
                for (var c = 0; c < columns && first + c < settings.Length; c++)
                    height = Mathf.Max(height, CardHeight(settings[first + c], estimatedWidth, DescriptionFor(ps, settings[first + c])));
                var row = GUILayoutUtility.GetRect(0, height, GUILayout.ExpandWidth(true));
                var width = (row.width - (columns - 1) * 8) / columns;
                var hasBreathing = false;
                EditorGUI.BeginChangeCheck();
                for (var c = 0; c < columns && first + c < settings.Length; c++)
                {
                    var setting = settings[first + c];
                    var forced = setting.Path == "mergeTrackingControl" && serialized.FindProperty("features.photography").boolValue;
                    var dependencyMissing = (setting.Path == "features.breathingControl" && !PosingSystemFeatureUtility.UsesBreathing(ps)) ||
                        ((setting.Path == "features.photographySynced" || setting.Path == "features.photographyKeepHeadVisible") && !ps.features.photography);
                    var unavailable = disabled || forced || dependencyMissing || (android && (setting.Path == "isIconDisabled" || setting.Path == "isIconSmall")) ||
                        (setting.Path == "isIconSmall" && serialized.FindProperty("isIconDisabled").boolValue);
                    var property = serialized.FindProperty(setting.Path);
                    var label = detailLabel?.Invoke(setting);
                    if (setting.Path == "features.breathing") label = breathingOpen ? "▼ 設定を閉じる" : "▶ 設定を開く";
                    var isBreathing = setting.Path == "features.breathing";
                    var before = property.boolValue;
                    var clicked = DrawCard(new Rect(row.x + c * (width + 8), row.y, width, row.height), property, setting,
                        unavailable, forced ? "撮影機能に必要なため、自動で選ばれています。" : DescriptionFor(ps, setting),
                        dependencyMissing ? (bool?)false : forced ? (bool?)true : android && setting.Path == "isIconDisabled" ? (bool?)false :
                        isBreathing ? before && serialized.FindProperty("additiveBreathing.enabled").boolValue : (bool?)null, label,
                        setting.SharedHead ? PosingSystemFeatureUtility.SharedHeadFeatureToggleBits(ps, setting.Path == "features.rollover") : setting.Bits, ps);
                    if (clicked) PosingSystemEditorChangeUtility.EndTextEditing();
                    if (isBreathing)
                    {
                        hasBreathing = true;
                        if (clicked) breathingOpen = !breathingOpen;
                        if (property.boolValue != before || GUI.changed)
                        {
                            // A click on this card replaces the two legacy master switches.
                            if (cardToggled) serialized.FindProperty("additiveBreathing.enabled").boolValue = property.boolValue;
                        }
                    }
                    else if (clicked) onDetail?.Invoke(setting);
                }
                if (EditorGUI.EndChangeCheck()) Apply(serialized, ps);
                GUILayout.Space(5);
                if (hasBreathing && breathingOpen) drawBreathing?.Invoke();
                afterRow?.Invoke();
            }
        }

        private static void Apply(SerializedObject serialized, PosingSystem ps)
        {
            if (PosingSystemEditorChangeUtility.Apply(serialized)) PosingSystemFeatureUtility.Synchronize(ps, true);
            serialized.Update();
        }

        private static void DrawSummary(PosingSystem ps)
        {
            var pose = PosingSystemFeatureUtility.PoseBits(ps);
            var optional = PosingSystemFeatureUtility.AvatarOptionalBits(ps);
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField("ExpressionParameters", summaryStyle);
                    GUILayout.Label((pose + optional) + " bit", PosingSystemInspectorGUI.Text(24, true), GUILayout.Width(95), GUILayout.Height(31));
                }
                EditorGUILayout.LabelField(PosingSystemLocalization.Format("ポーズ {0} bit ＋ 追加機能 {1} bit  ／ このアバターのポーズツール分", pose, optional), descriptionStyle);
            }
        }

        private const float CardIconSize = 56;
        private const float CardTextLeft = CardIconSize + 10;

        internal static string DescriptionFor(PosingSystem ps, Setting setting)
        {
            if (setting.Path == "features.breathingControl" && !PosingSystemFeatureUtility.UsesBreathing(ps))
                return PosingSystemLocalization.Text(setting.Description) + "\n" + PosingSystemLocalization.Text("使用するには便利機能の「呼吸の動きをつける」をオンにしてください。");
            if ((setting.Path == "features.photographySynced" || setting.Path == "features.photographyKeepHeadVisible") && !ps.features.photography)
                return PosingSystemLocalization.Text(setting.Description) + "\n" + PosingSystemLocalization.Text("使用するにはメニュー追加の「撮影モード」をオンにしてください。");
            return PosingSystemLocalization.Text(setting.Description);
        }

        private static float CardHeight(Setting setting, float width, string description)
        {
            var textWidth = Mathf.Max(90, width - CardTextLeft - 5);
            var titleHeight = Mathf.Max(16, titleStyle.CalcHeight(new GUIContent(PosingSystemLocalization.Text(setting.Title)), textWidth));
            var bodyHeight = descriptionStyle.CalcHeight(new GUIContent(description), textWidth);
            return 4 + Mathf.Max(CardIconSize, titleHeight + 2 + bodyHeight) + 24 + 4;
        }

        private static bool cardToggled;
        private static bool DrawCard(Rect rect, SerializedProperty property, Setting setting, bool disabled,
            string disabledReason, bool? displayedValue, string detailLabel, int displayedBits, PosingSystem ps)
        {
            cardToggled = false;
            var value = displayedValue ?? (setting.Inverted ? !property.boolValue : property.boolValue);
            var textWidth = rect.width - CardTextLeft - 5;
            var titleHeight = Mathf.Max(16, titleStyle.CalcHeight(new GUIContent(PosingSystemLocalization.Text(setting.Title)), textWidth));
            var bodyY = rect.y + 6 + titleHeight;
            var background = value ? (EditorGUIUtility.isProSkin ? new Color(.19f,.25f,.32f) : new Color(.92f,.96f,1f)) : PosingSystemInspectorGUI.Surface;
            EditorGUI.DrawRect(rect, value ? Accent : PosingSystemInspectorGUI.Border);
            EditorGUI.DrawRect(new Rect(rect.x + 1, rect.y + 1, rect.width - 2, rect.height - 2), background);
            var clicked = detailLabel != null && PosingSystemInspectorGUI.Link(new Rect(rect.x + 6, rect.yMax - 25, rect.width - 95, 21), detailLabel);
            using (new EditorGUI.DisabledScope(disabled))
            {
                EditorGUI.BeginProperty(rect, new GUIContent(PosingSystemLocalization.Text(setting.Title)), property);
                var next = GUI.Toggle(rect, value, GUIContent.none, GUIStyle.none);
                var check = new Rect(rect.xMax - 26, rect.yMax - 26, 22, 22);
                // The full setting surface handles clicks; the check only paints its state.
                PosingSystemInspectorGUI.Check(check, value, GUIContent.none, false);
                if (next != value)
                {
                    property.boolValue = setting.Inverted ? !next : next;
                    cardToggled = true;
                }
                EditorGUI.EndProperty();
                var bitDetails = setting.Path == "features.rollover"
                    ? "チェックを付けると増える量／外すと減る量です。\n寝返りON/OFF：1 bit、頭の傾き：8 bit、首固定：1 bit。\nほかの設定で使用中のパラメーターは重複して数えません。"
                    : setting.SharedHead ? "チェックを付けると増える量／外すと減る量です。\n首固定：1 bit（寝返りと共用）。\n寝返りなどで使用中なら0 bitです。" : null;
                GUI.Label(new Rect(check.x - 49, check.y, 45, check.height),
                    new GUIContent(displayedBits + " bit" + (setting.SharedHead ? " ※" : ""), PosingSystemLocalization.Text(bitDetails)), badgeStyle);
            }
            var iconRect = new Rect(rect.x + 4, rect.y + 4, CardIconSize, CardIconSize);
            PosingSystemSettingIcons.Draw(iconRect, setting.Path, ps);
            GUI.Label(new Rect(rect.x + CardTextLeft, rect.y + 4, textWidth, titleHeight), PosingSystemLocalization.Text(setting.Title), titleStyle);
            GUI.Label(new Rect(rect.x + CardTextLeft, bodyY, textWidth, rect.yMax - 27 - bodyY), PosingSystemLocalization.Text(disabledReason), descriptionStyle);
            return clicked;
        }

        private static void DrawDetails(SerializedObject serialized, PosingSystem ps)
        {
            var f = serialized.FindProperty("features");
            EditorGUI.BeginChangeCheck();
            if (f.FindPropertyRelative("rollover").boolValue)
            {
                EditorGUILayout.Space(8);
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField(PosingSystemLocalization.Text("寝返りの設定"), EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(PosingSystemLocalization.Text("「ポーズ一覧」で対応するポーズの詳細を開き、「寝返りを使う」をオンにしてください。"), descriptionStyle);
                    Draw("rolloverAngle", "横を向くまでの頭の傾き");
                    Draw("rolloverDeadZone", "小さな傾きを無視する範囲");
                    EditorGUILayout.LabelField(PosingSystemLocalization.Text("角度は度数で指定します。寝姿勢を選び、VRChatのメニューで首固定を解除して使ってください。"), descriptionStyle);
                    // Keep previously assigned shared clips editable without requiring one for new per-pose settings.
                    if (f.FindPropertyRelative("rolloverAnimation").objectReferenceValue != null)
                        Draw("rolloverAnimation", "共通アニメーション（従来設定）");
                }
            }
            if (EditorGUI.EndChangeCheck()) Apply(serialized, ps);
            void Draw(string name, string label) => EditorGUILayout.PropertyField(f.FindPropertyRelative(name), new GUIContent(PosingSystemLocalization.Text(label)));
        }

        private static void EnsureStyles()
        {
            if (titleStyle != null && stylesAreDark == EditorGUIUtility.isProSkin) return;
            stylesAreDark = EditorGUIUtility.isProSkin;
            var secondary = stylesAreDark ? new Color(.79f, .82f, .86f) : new Color(.29f, .33f, .39f);
            // Explicit fonts prevent prefab overrides from switching Unity's implicit default font.
            titleStyle = new GUIStyle(EditorStyles.boldLabel) { font = EditorStyles.boldFont, fontSize = 12, wordWrap = true, alignment = TextAnchor.MiddleLeft, padding = new RectOffset() };
            descriptionStyle = new GUIStyle(EditorStyles.wordWrappedMiniLabel) { font = EditorStyles.standardFont, fontSize = 11, padding = new RectOffset(0, 0, 0, 0) };
            descriptionStyle.normal.textColor = secondary;
            badgeStyle = new GUIStyle(EditorStyles.miniLabel) { font = EditorStyles.standardFont, fontSize = 10, fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleRight, padding = new RectOffset(0, 0, 0, 0) };
            badgeStyle.normal.textColor = secondary;
            summaryStyle = new GUIStyle(EditorStyles.label) { font = EditorStyles.standardFont, fontSize = 12, wordWrap = true };
            groupStyle = new GUIStyle(EditorStyles.boldLabel) { font = EditorStyles.boldFont, fontSize = 12, alignment = TextAnchor.MiddleLeft };
        }
    }
}
