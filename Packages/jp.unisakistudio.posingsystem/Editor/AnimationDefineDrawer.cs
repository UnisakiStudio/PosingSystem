using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor
{
    [CustomPropertyDrawer(typeof(PosingSystem.AnimationDefine))]
    public class AnimationDefineDrawer : PropertyDrawer
    {
        // レイアウト用の定数を定義
        private const float ToggleWidth = 20f;
        private const float DefaultButtonWidth = 60f;
        private const float FieldIndentSpacing = 2f;
        private const float AnimationLabelWidth = 80f;
        private const float MotionTimeLabelWidth = 100f;
        private const float RotateLabelWidth = 100f;
        private const float IconLabelWidth = 100f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 各プロパティを取得
            var enabledProperty = property.FindPropertyRelative("enabled");
            var displayNameProperty = property.FindPropertyRelative("displayName");
            var initialSetProperty = property.FindPropertyRelative("initialSet");
            var previewImageProperty = property.FindPropertyRelative("previewImage");
            var animationClipProperty = property.FindPropertyRelative("animationClip");
            var adjustmentClipProperty = property.FindPropertyRelative("adjustmentClip");
            var initialProperty = property.FindPropertyRelative("initial");
            var isCustomIconProperty = property.FindPropertyRelative("isCustomIcon");
            var iconProperty = property.FindPropertyRelative("icon");
            var isMotionTimeProperty = property.FindPropertyRelative("isMotionTime");
            var motionTimeParamNameProperty = property.FindPropertyRelative("motionTimeParamName");
            var isRotateProperty = property.FindPropertyRelative("isRotate");
            var rotateProperty = property.FindPropertyRelative("rotate");

            // --- 変更検出 ---
            // このブロック内のUIが変更された場合、プレビュー画像をリセットする
            EditorGUI.BeginChangeCheck();

            // --- レイアウト ---
            // 1行目: 有効トグル、表示名、デフォルトボタン
            var lineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            var enabledRect = new Rect(lineRect.x, lineRect.y, ToggleWidth, lineRect.height);
            var buttonRect = new Rect(lineRect.xMax - DefaultButtonWidth, lineRect.y, DefaultButtonWidth, lineRect.height);
            var nameRect = new Rect(enabledRect.xMax, lineRect.y, buttonRect.x - enabledRect.xMax - FieldIndentSpacing, lineRect.height);

            enabledProperty.boolValue = EditorGUI.Toggle(enabledRect, enabledProperty.boolValue);
            displayNameProperty.stringValue = EditorGUI.TextField(nameRect, displayNameProperty.stringValue);

            using (new EditorGUI.DisabledScope(initialSetProperty.boolValue))
            {
                if (GUI.Button(buttonRect, "デフォルト"))
                {
                    initialSetProperty.boolValue = true;
                }
            }

            position.y += EditorGUIUtility.singleLineHeight;

            // 2行目以降: 左側にアイコン、右側に設定項目
            float iconSize = position.height - EditorGUIUtility.singleLineHeight - 2f; // -2fは僅かなパディング
            var iconRect = new Rect(position.x, position.y, iconSize, iconSize);
            var contentStartX = iconRect.xMax + EditorGUIUtility.standardVerticalSpacing;
            var contentWidth = position.xMax - contentStartX;
            var contentRect = new Rect(contentStartX, position.y, contentWidth, EditorGUIUtility.singleLineHeight);

            // --- アイコン背景描画 ---
            var prevColor = GUI.color;
            if (!enabledProperty.boolValue || animationClipProperty.objectReferenceValue == null)
            {
                GUI.color = new Color(0.3f, 0.3f, 0.3f, 1);
            }
            else if (initialProperty.boolValue)
            {
                GUI.color = new Color(0.3f, 0.3f, 0.8f, 1);
            }
            else
            {
                GUI.color = new Color(0.7f, 0.7f, 0.7f, 1);
            }
            GUI.DrawTexture(iconRect, EditorGUIUtility.whiteTexture, ScaleMode.ScaleToFit);
            GUI.color = prevColor;

            // --- アイコン描画 ---
            Texture2D iconToShow = null;
            if (isCustomIconProperty.boolValue)
            {
                iconToShow = (Texture2D)iconProperty.objectReferenceValue;
            }
            else
            {
                iconToShow = (Texture2D)previewImageProperty.objectReferenceValue;
            }
            if (iconToShow != null)
            {
                GUI.DrawTexture(iconRect, iconToShow, ScaleMode.ScaleToFit);
            }

            // --- 設定項目描画 ---
            // 2行目: アニメーション
            var animLabelRect = new Rect(contentRect.x, contentRect.y, AnimationLabelWidth, contentRect.height);
            var animFieldRect = new Rect(animLabelRect.xMax, contentRect.y, contentRect.width - AnimationLabelWidth, contentRect.height);
            EditorGUI.LabelField(animLabelRect, "アニメーション");
            animationClipProperty.objectReferenceValue = EditorGUI.ObjectField(animFieldRect, animationClipProperty.objectReferenceValue, typeof(Motion), false);
            contentRect.y += EditorGUIUtility.singleLineHeight;

            // 3行目: MotionTime
            var motionTimeToggleRect = new Rect(contentRect.x + FieldIndentSpacing, contentRect.y, ToggleWidth, contentRect.height);
            var motionTimeLabelRect = new Rect(motionTimeToggleRect.xMax, contentRect.y, MotionTimeLabelWidth, contentRect.height);
            var motionTimeFieldRect = new Rect(motionTimeLabelRect.xMax, contentRect.y, contentRect.width - (motionTimeLabelRect.xMax - contentRect.x), contentRect.height);
            isMotionTimeProperty.boolValue = EditorGUI.Toggle(motionTimeToggleRect, isMotionTimeProperty.boolValue);
            EditorGUI.LabelField(motionTimeLabelRect, "motionTime");
            if (isMotionTimeProperty.boolValue)
            {
                motionTimeParamNameProperty.stringValue = EditorGUI.TextField(motionTimeFieldRect, motionTimeParamNameProperty.stringValue);
            }
            contentRect.y += EditorGUIUtility.singleLineHeight;

            // 4行目: 回転
            var rotateToggleRect = new Rect(contentRect.x + FieldIndentSpacing, contentRect.y, ToggleWidth, contentRect.height);
            var rotateLabelRect = new Rect(rotateToggleRect.xMax, contentRect.y, RotateLabelWidth, contentRect.height);
            var rotateFieldRect = new Rect(rotateLabelRect.xMax, contentRect.y, contentRect.width - (rotateLabelRect.xMax - contentRect.x), contentRect.height);
            isRotateProperty.boolValue = EditorGUI.Toggle(rotateToggleRect, isRotateProperty.boolValue);
            EditorGUI.LabelField(rotateLabelRect, "回転角度(360度)");
            if (isRotateProperty.boolValue)
            {
                rotateProperty.intValue = EditorGUI.IntField(rotateFieldRect, rotateProperty.intValue);
            }
            contentRect.y += EditorGUIUtility.singleLineHeight;

            // 5行目: カスタムアイコン
            var customIconToggleRect = new Rect(contentRect.x + FieldIndentSpacing, contentRect.y, ToggleWidth, contentRect.height);
            var customIconLabelRect = new Rect(customIconToggleRect.xMax, contentRect.y, IconLabelWidth, contentRect.height);
            var customIconFieldRect = new Rect(customIconLabelRect.xMax, contentRect.y, contentRect.width - (customIconLabelRect.xMax - contentRect.x), contentRect.height);
            isCustomIconProperty.boolValue = EditorGUI.Toggle(customIconToggleRect, isCustomIconProperty.boolValue);
            EditorGUI.LabelField(customIconLabelRect, "アイコン画像");
            if (isCustomIconProperty.boolValue)
            {
                iconProperty.objectReferenceValue = EditorGUI.ObjectField(customIconFieldRect, iconProperty.objectReferenceValue, typeof(Texture2D), false);
            }
            contentRect.y += EditorGUIUtility.singleLineHeight;

            // 6行目: 調整アニメーション
            var adjustmentLabelRect = new Rect(contentRect.x, contentRect.y, AnimationLabelWidth, contentRect.height);
            var adjustmentFieldRect = new Rect(adjustmentLabelRect.xMax, contentRect.y, contentRect.width - AnimationLabelWidth, contentRect.height);
            EditorGUI.LabelField(adjustmentLabelRect, "調整クリップ");
            adjustmentClipProperty.objectReferenceValue = EditorGUI.ObjectField(adjustmentFieldRect, adjustmentClipProperty.objectReferenceValue, typeof(AnimationClip), false);
            contentRect.y += EditorGUIUtility.singleLineHeight;

            // --- 変更検出終了 ---
            if (EditorGUI.EndChangeCheck())
            {
                previewImageProperty.objectReferenceValue = null;
            }

        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 6;
        }

    }
}