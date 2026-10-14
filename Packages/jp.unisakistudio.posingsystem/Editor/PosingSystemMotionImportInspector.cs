using System;
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEngine;
using StateType = jp.unisakistudio.posingsystem.PosingSystem.OverrideAnimationDefine.AnimationStateType;

namespace jp.unisakistudio.posingsystemeditor
{
    /// <summary>One active override per state type. Retain legacy alternatives for Undo and reselection.</summary>
    public static class PosingSystemMotionImportUtility
    {
        public static int SelectedIndex(PosingSystem ps, StateType type)
        {
            if (ps.overrideDefines == null) return -1;
            var indices = Enumerable.Range(0, ps.overrideDefines.Count)
                .Where(i => ps.overrideDefines[i] != null && ps.overrideDefines[i].stateType == type).ToArray();
            // Match the converter's last valid, enabled override when opening older settings.
            return indices.Where(i => ps.overrideDefines[i].enabled &&
                PosingSystemConverter.IsValidOverrideMotion(type, ps.overrideDefines[i].animationClip))
                .DefaultIfEmpty(indices.DefaultIfEmpty(-1).Last()).Last();
        }

        public static int SetMotion(PosingSystem ps, StateType type, Motion motion)
        {
            if (motion != null && !PosingSystemConverter.IsValidOverrideMotion(type, motion))
                throw new ArgumentException("この種類には移動を含むBlendTreeを指定してください。", nameof(motion));
            Undo.RegisterCompleteObjectUndo(ps, "Set imported motion");
            ps.overrideDefines ??= new List<PosingSystem.OverrideAnimationDefine>();
            var index = SelectedIndex(ps, type);
            if (index < 0)
            {
                index = ps.overrideDefines.Count;
                ps.overrideDefines.Add(new PosingSystem.OverrideAnimationDefine { stateType = type });
            }
            var definition = ps.overrideDefines[index];
            definition.animationClip = motion;
            definition.previewImage = null;
            SelectCore(ps, index, motion != null);
            PosingSystemPoseListUtility.MarkChanged(ps);
            return SelectedIndex(ps, type);
        }

        public static void Select(PosingSystem ps, int index, bool enabled = true)
        {
            if (ps.overrideDefines == null || index < 0 || index >= ps.overrideDefines.Count) return;
            Undo.RegisterCompleteObjectUndo(ps, "Select imported motion");
            SelectCore(ps, index, enabled);
            PosingSystemPoseListUtility.MarkChanged(ps);
        }

        private static void SelectCore(PosingSystem ps, int index, bool enabled)
        {
            var chosen = ps.overrideDefines[index];
            for (var i = 0; i < ps.overrideDefines.Count; i++)
                if (ps.overrideDefines[i] != null && ps.overrideDefines[i].stateType == chosen.stateType)
                    ps.overrideDefines[i].enabled = i == index && enabled && chosen.animationClip != null;
            // Keep the chosen alternative visible after disabling it and reopening the Inspector.
            var last = ps.overrideDefines.FindLastIndex(d => d != null && d.stateType == chosen.stateType);
            if (last != index)
            {
                ps.overrideDefines[index] = ps.overrideDefines[last];
                ps.overrideDefines[last] = chosen;
            }
        }
    }

    internal sealed class PosingSystemMotionImportInspector
    {
        private int expanded = -1;
        private Vector2 scroll;
        private string warning;
        private static readonly string[] Labels = { "立ち・歩き・走り", "立ちのみ", "しゃがみ（Crouching）", "伏せ（Prone）", "ジャンプ", "落下（短め）", "着地（短め）", "落下（長め）", "着地（長め）", "アバター選択" };
        private static readonly StateType[] DisplayOrder = new[] { StateType.AvatarSelect }
            .Concat(Enum.GetValues(typeof(StateType)).Cast<StateType>().Where(type => type != StateType.AvatarSelect)).ToArray();

        internal void Draw(SerializedObject serialized, PosingSystem ps)
        {
            if (warning != null) EditorGUILayout.HelpBox(warning, MessageType.Warning);
            using (var viewport = new EditorGUILayout.ScrollViewScope(scroll, GUILayout.Height(340)))
            {
                scroll = viewport.scrollPosition;
                foreach (var type in DisplayOrder)
                {
                    var index = PosingSystemMotionImportUtility.SelectedIndex(ps, type);
                    var definition = index < 0 ? null : ps.overrideDefines[index];
                    using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                    {
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            GUILayout.Label(Labels[(int)type], PosingSystemInspectorGUI.Text(12, true));
                            if (definition != null && PosingSystemInspectorGUI.Link(expanded == (int)type ? "▼ 設定を閉じる" : "▶ 設定を開く", GUILayout.Width(100)))
                            {
                                PosingSystemEditorChangeUtility.Commit(serialized);
                                expanded = expanded == (int)type ? -1 : (int)type;
                                GUIUtility.ExitGUI();
                            }
                        }
                        using (new EditorGUILayout.HorizontalScope())
                        {
                            var preview = definition?.previewImage;
                            var rect = GUILayoutUtility.GetRect(32, 32, GUILayout.Width(32));
                            if (preview != null) GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit);
                            else PosingSystemSettingIcons.Draw(rect, "autoImportAvatarAnimations");
                            EditorGUI.BeginChangeCheck();
                            var motion = (Motion)EditorGUILayout.ObjectField(definition?.animationClip, typeof(Motion), false);
                            if (EditorGUI.EndChangeCheck())
                            {
                                if (motion != null && !PosingSystemConverter.IsValidOverrideMotion(type, motion))
                                    warning = "「立ち・歩き・走り」「しゃがみ（Crouching）」「伏せ（Prone）」には移動を含むBlendTreeを指定してください。単独の立ち姿勢は「立ちのみ」に指定できます。";
                                else
                                {
                                    PosingSystemEditorChangeUtility.Commit(serialized);
                                    PosingSystemMotionImportUtility.SetMotion(ps, type, motion); warning = null;
                                    serialized.Update(); GUIUtility.ExitGUI();
                                }
                            }
                            using (new EditorGUI.DisabledScope(definition?.animationClip == null))
                            {
                                var active = definition != null && definition.enabled;
                                var next = GUILayout.Toggle(active, new GUIContent("使用", "この種類のモーションを使う"), GUILayout.Width(47));
                                if (active != next) { PosingSystemEditorChangeUtility.Commit(serialized); PosingSystemMotionImportUtility.Select(ps, index, next); serialized.Update(); GUIUtility.ExitGUI(); }
                            }
                        }
                        if (ps.overrideDefines != null)
                        {
                            var alternatives = Enumerable.Range(0, ps.overrideDefines.Count)
                                .Where(i => ps.overrideDefines[i]?.stateType == type).ToArray();
                            if (alternatives.Length > 1)
                            {
                                EditorGUILayout.LabelField("以前の設定から、使う1つを選べます。", PosingSystemInspectorGUI.Text(11));
                                var selected = Array.IndexOf(alternatives, index);
                                var next = EditorGUILayout.Popup(selected, alternatives.Select(i => ps.overrideDefines[i].animationClip != null ? ps.overrideDefines[i].animationClip.name : "未指定").ToArray());
                                if (next != selected) { PosingSystemEditorChangeUtility.Commit(serialized); PosingSystemMotionImportUtility.Select(ps, alternatives[next]); serialized.Update(); GUIUtility.ExitGUI(); }
                            }
                        }
                        if (expanded == (int)type && index >= 0) DrawDetails(serialized.FindProperty("overrideDefines").GetArrayElementAtIndex(index));
                    }
                }
            }
        }

        private static void DrawDetails(SerializedProperty property)
        {
            var previous = EditorGUIUtility.labelWidth;
            try
            {
                EditorGUIUtility.labelWidth = 145;
                EditorGUILayout.PropertyField(property.FindPropertyRelative("isRotate"), new GUIContent("向きを回転させる"));
                if (property.FindPropertyRelative("isRotate").boolValue) EditorGUILayout.PropertyField(property.FindPropertyRelative("rotate"), new GUIContent("回転角度"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("isMotionTime"), new GUIContent("再生位置を指定する"));
                if (property.FindPropertyRelative("isMotionTime").boolValue) EditorGUILayout.PropertyField(property.FindPropertyRelative("motionTimeParamName"), new GUIContent("再生位置のパラメーター"));
                EditorGUILayout.PropertyField(property.FindPropertyRelative("adjustmentClip"), new GUIContent("調整アニメーション"));
            }
            finally { EditorGUIUtility.labelWidth = previous; }
        }
    }
}
