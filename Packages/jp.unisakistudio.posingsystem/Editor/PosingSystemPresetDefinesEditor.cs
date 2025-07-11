using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace jp.unisakistudio.posingsystemeditor
{
    [CustomEditor(typeof(PosingSystemPresetDefines))]
    public class PosingSystemPresetDefinesEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("一括追加機能", EditorStyles.boldLabel);
            
            var presetDefines = target as PosingSystemPresetDefines;
            
            for (int i = 0; i < presetDefines.presetDefines.Count; i++)
            {
                var presetDefine = presetDefines.presetDefines[i];
                
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.LabelField($"PresetDefine [{i}]", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("PrefabsHashesに\nクリップボードから追加", GUILayout.Height(40)))
                {
                    AddFromClipboard(presetDefine.prefabsHashes, "PrefabsHashes");
                }
                
                if (GUILayout.Button("PrefabsNamesに\nクリップボードから追加", GUILayout.Height(40)))
                {
                    AddFromClipboard(presetDefine.prefabsNames, "PrefabsNames");
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("PrefabsHashesを\nクリア", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("確認", "PrefabsHashesをすべてクリアしますか？", "はい", "いいえ"))
                    {
                        Undo.RecordObject(target, "Clear PrefabsHashes");
                        presetDefine.prefabsHashes.Clear();
                        EditorUtility.SetDirty(target);
                    }
                }
                
                if (GUILayout.Button("PrefabsNamesを\nクリア", GUILayout.Height(30)))
                {
                    if (EditorUtility.DisplayDialog("確認", "PrefabsNamesをすべてクリアしますか？", "はい", "いいえ"))
                    {
                        Undo.RecordObject(target, "Clear PrefabsNames");
                        presetDefine.prefabsNames.Clear();
                        EditorUtility.SetDirty(target);
                    }
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();
            }
            
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("使用方法:\n1. 追加したい文字列を改行区切りでクリップボードにコピー\n2. 対応するボタンをクリック\n3. 重複する項目は自動的に除外されます", MessageType.Info);
        }
        
        private void AddFromClipboard(List<string> targetList, string listName)
        {
            string clipboardText = EditorGUIUtility.systemCopyBuffer;
            
            if (string.IsNullOrEmpty(clipboardText))
            {
                EditorUtility.DisplayDialog("エラー", "クリップボードが空です。", "OK");
                return;
            }
            
            // 改行区切りで分割し、空の行や空白のみの行を除外
            var lines = clipboardText.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries)
                                   .Select(line => line.Trim())
                                   .Where(line => !string.IsNullOrEmpty(line))
                                   .ToList();
            
            if (lines.Count == 0)
            {
                EditorUtility.DisplayDialog("エラー", "有効な文字列が見つかりませんでした。", "OK");
                return;
            }
            
            Undo.RecordObject(target, $"Add to {listName} from clipboard");
            
            int addedCount = 0;
            foreach (var line in lines)
            {
                // 重複チェック
                if (!targetList.Contains(line))
                {
                    targetList.Add(line);
                    addedCount++;
                }
            }
            
            EditorUtility.SetDirty(target);
            
            if (addedCount > 0)
            {
                EditorUtility.DisplayDialog("完了", $"{listName}に{addedCount}個の項目を追加しました。\n（重複する項目は除外されました）", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("情報", "すべての項目が既に存在するため、追加されませんでした。", "OK");
            }
        }
    }
}
