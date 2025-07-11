using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;
using System;

namespace jp.unisakistudio.posingsystemeditor
{
    [InitializeOnLoad]
    public static class PosingSystemHierarchyBadge
    {
        static Texture2D warningIcon;
        static Texture2D errorIcon;

        static PosingSystemHierarchyBadge()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
        }

        static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null) return;

            // ここでVRCAvatarDescriptorがアタッチされているか判定
            if (obj.GetComponent<PosingSystem>() != null)
            {
                // Unityの標準の警告アイコンテクスチャを取得
                if (warningIcon == null)
                {
                    warningIcon = (Texture2D)EditorGUIUtility.IconContent("console.warnicon.sml").image;
                }
                if (errorIcon == null)
                {
                    errorIcon = (Texture2D)EditorGUIUtility.IconContent("console.erroricon.sml").image;
                }

                var posingSystem = obj.GetComponent<PosingSystem>();
                if (DateTime.Now - posingSystem.previousErrorCheckTime > TimeSpan.FromSeconds(3))
                {
                    posingSystem.isWarning = PosingSystemConverter.HasWarning(posingSystem);
                    posingSystem.isError = PosingSystemConverter.HasError(posingSystem);
                    posingSystem.previousErrorCheckTime = DateTime.Now;
                }

                // もしPosingSystemで警告がある場合は警告アイコンを表示
                if (posingSystem.isWarning)
                {
                    // 警告アイコンを表示
                    GUI.Label(selectionRect, warningIcon);
                }

                // もしPosingSystemでエラーがある場合はエラーアイコンを表示
                if (posingSystem.isError)
                {
                    // エラーアイコンを表示
                    GUI.Label(selectionRect, errorIcon);
                }
            }
        }
    }
}