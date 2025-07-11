using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor
{
    [CustomPropertyDrawer(typeof(PosingSystem.OverrideAnimationDefine))]
    public class OverrideAnimationDefineDrawer : PropertyDrawer
    {
        // レイアウト用の定数を定義
        private const float FieldIndentSpacing = 2f;
        private const float ToggleWidth = 20f;
        private const float MotionTimeLabelWidth = 100f;
        private const float RotateLabelWidth = 100f;
        private const float AnimationLabelWidth = 80f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 各プロパティを取得
            var stateTypeProperty = property.FindPropertyRelative("stateType");
            var previewImageProperty = property.FindPropertyRelative("previewImage");
            var animationClipProperty = property.FindPropertyRelative("animationClip");
            var isMotionTimeProperty = property.FindPropertyRelative("isMotionTime");
            var motionTimeParamNameProperty = property.FindPropertyRelative("motionTimeParamName");
            var isRotateProperty = property.FindPropertyRelative("isRotate");
            var rotateProperty = property.FindPropertyRelative("rotate");

            // --- レイアウト計算 ---
            var lineRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            // 1行目: 種別を選択
            EditorGUI.PropertyField(lineRect, stateTypeProperty, GUIContent.none);
            position.y += EditorGUIUtility.singleLineHeight;

            // 2行目以降: 左側にアイコン、右側に設定項目
            float iconSize = position.height - EditorGUIUtility.singleLineHeight - 2f; // -2fは僅かなパディング
            var iconRect = new Rect(position.x, position.y, iconSize, iconSize);
            var contentStartX = iconRect.xMax + EditorGUIUtility.standardVerticalSpacing;
            var contentWidth = position.xMax - contentStartX;
            var contentRect = new Rect(contentStartX, position.y, contentWidth, EditorGUIUtility.singleLineHeight);

            // --- アイコン描画 ---
            var prevColor = GUI.color;
            GUI.color = new Color(0.7f, 0.7f, 0.7f, 1);
            GUI.DrawTexture(iconRect, EditorGUIUtility.whiteTexture, ScaleMode.ScaleToFit);
            GUI.color = prevColor;
            if (previewImageProperty.objectReferenceValue != null)
            {
                GUI.DrawTexture(iconRect, (Texture2D)previewImageProperty.objectReferenceValue, ScaleMode.ScaleToFit);
            }

            // --- 設定項目描画 ---
            EditorGUI.BeginChangeCheck();

            // 2行目: アニメーション
            var animLabelRect = new Rect(contentRect.x, contentRect.y, AnimationLabelWidth, contentRect.height);
            var animFieldRect = new Rect(animLabelRect.xMax, contentRect.y, contentRect.width - AnimationLabelWidth, contentRect.height);
            EditorGUI.LabelField(animLabelRect, "アニメーション");
            EditorGUI.ObjectField(animFieldRect, animationClipProperty, typeof(Motion), new GUIContent(""));
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

            // いずれかの値が変更されたら、プレビュー画像をリセット
            if (EditorGUI.EndChangeCheck())
            {
                previewImageProperty.objectReferenceValue = null;
            }
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 5;
        }

    }
}