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
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // 共通インデント
            var indent = (position.height - 1) - EditorGUIUtility.singleLineHeight;

            // 有効無効
            var enabledProperty = property.FindPropertyRelative("enabled");
            var enabledOld = enabledProperty.boolValue;
            enabledProperty.boolValue = EditorGUI.Toggle(new Rect(position.x + 2, position.y, 20, EditorGUIUtility.singleLineHeight), enabledProperty.boolValue);
            if (enabledOld != enabledProperty.boolValue)
            {
                property.FindPropertyRelative("previewImage").objectReferenceValue = null;
            }

            // 表示名
            var displayNameProperty = property.FindPropertyRelative("displayName");
            displayNameProperty.stringValue = EditorGUI.TextField(new Rect(position.x + 20, position.y, position.width - 20 - 60 - 1, EditorGUIUtility.singleLineHeight), displayNameProperty.stringValue);

            // 標準化
            var initialSetProperty = property.FindPropertyRelative("initialSet");
            GUI.enabled = !initialSetProperty.boolValue;
            if (GUI.Button(new Rect(position.x + position.width - 60, position.y, 60, 20 - 1), "デフォルト"))
            {
                initialSetProperty.boolValue = true;
            }
            GUI.enabled = true;
            position.y += EditorGUIUtility.singleLineHeight;

            // アイコン
            var iconProperty = property.FindPropertyRelative("icon");
            var previewImageProperty = property.FindPropertyRelative("previewImage");
            if (property.FindPropertyRelative("isCustomIcon").boolValue)
            {
                if (iconProperty.objectReferenceValue != null)
                {
                    GUI.DrawTexture(new Rect(position.x, position.y + 1, (position.height - 2 - EditorGUIUtility.singleLineHeight), (position.height - 2 - EditorGUIUtility.singleLineHeight)), (Texture2D)iconProperty.objectReferenceValue, ScaleMode.ScaleToFit);
                }
            }
            else if (previewImageProperty.objectReferenceValue != null)
            {
                GUI.DrawTexture(new Rect(position.x, position.y + 1, (position.height - 2 - EditorGUIUtility.singleLineHeight), (position.height - 2 - EditorGUIUtility.singleLineHeight)), (Texture2D)previewImageProperty.objectReferenceValue, ScaleMode.ScaleToFit);
            }

            // アニメーション
            var animationClipProperty = property.FindPropertyRelative("animationClip");
            var animationClipOld = animationClipProperty.objectReferenceValue;
            EditorGUI.LabelField(new Rect(position.x + indent, position.y, 80, EditorGUIUtility.singleLineHeight), "アニメーション");
            animationClipProperty.objectReferenceValue = EditorGUI.ObjectField(new Rect(position.x + 80 + indent, position.y, position.width - 80 - indent, EditorGUIUtility.singleLineHeight), property.FindPropertyRelative("animationClip").objectReferenceValue, typeof(Motion), false);
            if (animationClipOld != animationClipProperty.objectReferenceValue)
            {
                property.FindPropertyRelative("previewImage").objectReferenceValue = null;
            }
            position.y += EditorGUIUtility.singleLineHeight;

            // MotionTime
            var isMotionTimeProperty = property.FindPropertyRelative("isMotionTime");
            isMotionTimeProperty.boolValue = EditorGUI.Toggle(new Rect(position.x + 2 + indent, position.y, 20, EditorGUIUtility.singleLineHeight), isMotionTimeProperty.boolValue);
            EditorGUI.LabelField(new Rect(position.x + 22 + indent, position.y, 100, EditorGUIUtility.singleLineHeight), "motionTime");
            if (isMotionTimeProperty.boolValue)
            {
                var motionTimeParamNameProperty = property.FindPropertyRelative("motionTimeParamName");
                motionTimeParamNameProperty.stringValue = EditorGUI.TextField(new Rect(position.x + indent + 122, position.y, position.width - indent - 122, EditorGUIUtility.singleLineHeight), motionTimeParamNameProperty.stringValue);
            }
            position.y += EditorGUIUtility.singleLineHeight;

            float mirrorWidth = 0;

            // 回転
            var isRotateProperty = property.FindPropertyRelative("isRotate");
            if(isRotateProperty.boolValue != EditorGUI.Toggle(new Rect(position.x + 2 + indent + mirrorWidth, position.y, 20, EditorGUIUtility.singleLineHeight), isRotateProperty.boolValue))
            {
                isRotateProperty.boolValue = !isRotateProperty.boolValue;
                property.FindPropertyRelative("previewImage").objectReferenceValue = null;
            }
            EditorGUI.LabelField(new Rect(position.x + 22 + indent + mirrorWidth, position.y, 100, EditorGUIUtility.singleLineHeight), "回転角度(360度)");
            if (isRotateProperty.boolValue)
            {
                var rotateProperty = property.FindPropertyRelative("rotate");
                var rotateOld = rotateProperty.intValue;
                rotateProperty.intValue = EditorGUI.IntField(new Rect(position.x + indent + 122 + mirrorWidth, position.y, position.width - indent - 122 - mirrorWidth, EditorGUIUtility.singleLineHeight), rotateProperty.intValue);
                if (rotateOld != rotateProperty.intValue)
                {
                    property.FindPropertyRelative("previewImage").objectReferenceValue = null;
                }
            }
            position.y += EditorGUIUtility.singleLineHeight;

            // アイコン
            var isCustomIconProperty = property.FindPropertyRelative("isCustomIcon");
            isCustomIconProperty.boolValue = EditorGUI.Toggle(new Rect(position.x + 2 + indent, position.y, 20, EditorGUIUtility.singleLineHeight), isCustomIconProperty.boolValue);
            EditorGUI.LabelField(new Rect(position.x + 22 + indent, position.y, 100, EditorGUIUtility.singleLineHeight), "アイコン画像");
            if (isCustomIconProperty.boolValue)
            {
                iconProperty.objectReferenceValue = EditorGUI.ObjectField(new Rect(position.x + indent + 122, position.y, position.width - indent - 122, EditorGUIUtility.singleLineHeight), iconProperty.objectReferenceValue, typeof(Texture2D), false);
            }
            position.y += EditorGUIUtility.singleLineHeight;
        }
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * 5;
        }

    }
}