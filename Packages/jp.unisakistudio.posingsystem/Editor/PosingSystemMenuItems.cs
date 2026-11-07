/*
 * PosingSystemMenuItems
 * ゆにさきポージングシステムの右クリックメニュー
 * 
 * Copyright(c) 2025 UniSakiStudio
 * Released under the MIT license
 * https://opensource.org/licenses/mit-license.php
 */

using System.Linq;
using UnityEngine;
using UnityEditor;
using VRC.SDK3.Avatars.Components;
using System.IO;
using BestHTTP.Extensions;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System;
using jp.unisakistudio.posingsystem;

#if UNITY_EDITOR_WIN
using Microsoft.Win32;
#endif

namespace jp.unisakistudio.posingsystemeditor
{
    public static class PosingSystemMenuItems
    {
        private const string REGKEY = @"SOFTWARE\UnisakiStudio";
        private const string POSINGSYSTEM_APPKEY = "posingsystem";
        private const string LICENSE_VALUE = "licensed";
        
        private static string _md5HashSeed = "_jp.unisakistudio.posingsystem.guihashmaker";

        public static string GetPosingSystemGUIDHash(string guid)
        {
            var md5Hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(guid + _md5HashSeed));
            var hash = BitConverter.ToString(md5Hash).Replace("-", "").ToLower();
            return hash;
        }

        internal static GameObject FindInstallPrefab(string name, out string[] paths)
        {
            paths = AssetDatabase.FindAssets("t:Prefab " + name)
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => string.Equals(Path.GetFileNameWithoutExtension(path), name, StringComparison.Ordinal))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToArray();
            // 同名のバックアップや改変コピーより、商品パッケージの本体を優先する。
            var packaged = paths.Where(path => path.StartsWith("Packages/jp.unisakistudio.", StringComparison.Ordinal)).ToArray();
            foreach (var path in packaged.Length > 0 ? packaged : paths)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null && prefab.GetComponent<PosingSystem>() != null) return prefab;
            }
            return null;
        }

        static public void AddPrefab(string name)
        {
            if (Selection.activeGameObject == null)
            {
                EditorUtility.DisplayDialog("追加先を選択してください [PS040]", "HierarchyでVRC Avatar Descriptorの付いたアバター、またはその子オブジェクトを選択してから追加してください。", "閉じる");
                return;
            }
            if (Selection.activeGameObject)
            {
                var avatar = Selection.activeGameObject.GetComponentInParent<VRCAvatarDescriptor>(true);
                if (avatar == null || EditorUtility.IsPersistent(avatar) || UnityEditor.SceneManagement.PrefabStageUtility.GetPrefabStage(avatar.gameObject) != null)
                {
                    EditorUtility.DisplayDialog("追加先を確認してください [PS040]", "シーンのHierarchyに配置したVRC Avatar Descriptor付きアバターを選択してください。Project内のPrefabやPrefab編集画面には追加できません。", "閉じる");
                    return;
                }

                // 導入するPosingSystemのPrefabsを探す
                var prefabs = FindInstallPrefab(name, out var paths);
                if (paths.Length == 0)
                {
                    if (EditorUtility.DisplayDialog("ツール本体が見つかりません [PS041]", "探しているPrefab: " + name + "\nBOOTH同梱のライセンス用unitypackageだけでは、ツール本体の導入は完了しません。ALCOM / VCCでこのプロジェクトに商品本体と「ゆにさきポーズシステム」が追加されているか確認してください。\n導入済みの場合: " + PosingSystemDiagnostics.RestoreTool, "公式ヘルプを開く", "閉じる"))
                        Application.OpenURL(PosingSystemPackageInfo.SupportUrl);
                    return;
                }

                if (prefabs == null)
                {
                    if (EditorUtility.DisplayDialog("Prefabを読み込めません [PS042]", "対象: " + string.Join("\n", paths) + "\nPrefabが破損しているか、Posing Systemコンポーネントがありません。ConsoleのMissing Script・コンパイルエラーを確認してください。\n" + PosingSystemDiagnostics.RestoreTool, "公式ヘルプを開く", "閉じる"))
                        Application.OpenURL(PosingSystemPackageInfo.SupportUrl);
                    return;
                }
                // 導入するPosingSystemのPrefabsをアバターに配置する
                var instance = PrefabUtility.InstantiatePrefab(prefabs, avatar.transform) as GameObject;
                if (instance == null)
                {
                    if (EditorUtility.DisplayDialog("Prefabを配置できません [PS043]", "対象: " + name + "\nConsoleでMissing Prefabなどのインポートエラーを確認してください。配置先のシーンを開き直して再実行してください。", "公式ヘルプを開く", "閉じる"))
                        Application.OpenURL(PosingSystemPackageInfo.SupportUrl);
                    return;
                }
                Undo.RegisterCreatedObjectUndo(instance, "Add Prefab");
                UnityEditor.EditorUtility.SetDirty(instance);
                var installedPosingSystem = (instance as GameObject).GetComponent<PosingSystem>();
                PosingSystemAvatarAdjustmentInspector.ConfigureNewInstallation(installedPosingSystem);
                Selection.activeGameObject = instance;

                // Build only metadata; reference targets are loaded only after the user chooses a preset.
                var presetCatalog = PosingSystemPresetDefinesCache.GetCatalog(installedPosingSystem);
                if (presetCatalog == null)
                {
                    var message = "ツールを追加しました。プリセット一覧を読み込めませんでした。「アバター調整」の「再読込」で再試行できます。";
                    SceneView.lastActiveSceneView?.ShowNotification(new GUIContent(message));
                    Debug.Log("[PosingSystem] " + message, installedPosingSystem);
                    return;
                }
                var toolGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefabs));
                var presetDefines = PosingSystemPresetDefinesCache.GetMatchingEntries(presetCatalog, prefabs, prefabs.name)
                    .Where(entry => entry.toolGuids.Contains(toolGuid)).ToList();
                // Inspectorの「アバターに適合」と同じ判定を使用する。
                var presetDefine = PosingSystemPresetDefinesCache.GetAvatarMatchingEntries(presetDefines, avatar.gameObject)
                    .FirstOrDefault();

                // PresetDefineを適用する
                if (presetDefine != null)
                {
                    if (EditorUtility.DisplayDialog("情報", "このアバターのために作成された「" + presetDefine.avatarName + "」向けのプリセットが見つかりました。\nこのプリセットを適用するとアバターごとの体型の違いに対応した調整データが自動で設定されます。\n\nプリセットを適用しますか？", "適用する", "スキップ"))
                    {
                        Debug.Log("[PosingSystem]" + presetDefine.avatarName + "を適用します");
                        if (!PosingSystemPresetDefinesCache.ApplyPreset(installedPosingSystem, presetDefine))
                        {
                            EditorUtility.DisplayDialog("プリセットを読み込めません",
                                "プリセットが削除されたか、このツールに適用できません。Inspectorの「再読込」で一覧を更新してください。", "閉じる");
                        }
                    }
                }

                // さっそくプレビルドをするかどうかユーザーに尋ねる
                if (EditorUtility.DisplayDialog("プレビルド", "プレビルドをしますか？\nプレビルドを行うとアバターのビルドとアップロードが速くなります。", "はい", "いいえ"))
                {
                    // プレビルドをする
                    if (PosingSystemEditor.TryPrebuildAndRenew(installedPosingSystem))
                        EditorUtility.DisplayDialog("プレビルド", "プレビルドが完了しました", "OK");
                }
            }
       }
        
        // ========================================
        // ライセンス関連
        // ========================================
        
        /// <summary>
        /// Mac/Linux用の設定ファイルパス
        /// </summary>
        private static string GetLicenseFilePath()
        {
#if UNITY_EDITOR_OSX
            string appSupport = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appSupport, "UnisakiStudio", $"{POSINGSYSTEM_APPKEY}.lic");
#elif UNITY_EDITOR_LINUX
            string homeDir = System.Environment.GetEnvironmentVariable("HOME");
            return Path.Combine(homeDir, ".local", "share", "UnisakiStudio", $"{POSINGSYSTEM_APPKEY}.lic");
#else
            return null;
#endif
        }
        
        /// <summary>
        /// ライセンスがインストールされているかチェック
        /// </summary>
        private static bool IsLicensed()
        {
#if UNITY_EDITOR_WIN
            try
            {
                var regKey = Registry.CurrentUser.OpenSubKey(REGKEY);
                if (regKey != null)
                {
                    var value = (string)regKey.GetValue(POSINGSYSTEM_APPKEY);
                    regKey.Close();
                    return value == LICENSE_VALUE;
                }
            }
            catch (System.Exception)
            {
                // 例外は無視
            }
            return false;
#elif UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            try
            {
                string licenseFilePath = GetLicenseFilePath();
                if (File.Exists(licenseFilePath))
                {
                    string fileContent = File.ReadAllText(licenseFilePath);
                    return fileContent == LICENSE_VALUE;
                }
            }
            catch (System.Exception)
            {
                // 例外は無視
            }
            return false;
#else
            return false;
#endif
        }
        
        /// <summary>
        /// ライセンスを削除
        /// </summary>
        [MenuItem("Tools/ゆにさきスタジオ/ゆにさきポーズシステムライセンス削除", false, 201)]
        public static void UninstallLicense()
        {
            if (!IsLicensed())
            {
                EditorUtility.DisplayDialog(
                    "ライセンスの削除",
                    "ゆにさきポーズシステムのライセンスはインストールされていません。",
                    "OK"
                );
                return;
            }
            
            bool shouldUninstall = EditorUtility.DisplayDialog(
                "ライセンス削除",
                "ゆにさきポーズシステムのライセンスを削除しますか？\n\n" +
                "削除すると、ツールの機能が制限されます。\n" +
                "再度ライセンスを有効化するには、ライセンスインストーラーを再インポートする必要があります。",
                "削除",
                "キャンセル"
            );
            
            if (!shouldUninstall)
            {
                return;
            }
            
            string resultMessage = "";
            
#if UNITY_EDITOR_WIN
            try
            {
                var regKey = Registry.CurrentUser.OpenSubKey(REGKEY, true);
                if (regKey != null)
                {
                    // PosingSystemライセンスを削除
                    try
                    {
                        regKey.DeleteValue(POSINGSYSTEM_APPKEY, false);
                    }
                    catch (System.Exception) { }
                    
                    // レジストリキーが空になった場合は削除
                    if (regKey.ValueCount == 0 && regKey.SubKeyCount == 0)
                    {
                        regKey.Close();
                        Registry.CurrentUser.DeleteSubKey(REGKEY, false);
                        resultMessage = "ゆにさきポーズシステムのライセンスを削除しました。";
                    }
                    else
                    {
                        regKey.Close();
                        resultMessage = "ゆにさきポーズシステムのライセンスを削除しました。\n（他のゆにさきスタジオ商品のライセンスは保持されます）";
                    }
                    
                    Debug.Log($"[PosingSystem] {resultMessage}");
                }
                else
                {
                    resultMessage = "ライセンス情報は見つかりませんでした。";
                }
            }
            catch (System.Exception ex)
            {
                resultMessage = $"ライセンスの削除に失敗しました: {ex.Message}";
                Debug.LogError($"[PosingSystem] {resultMessage}");
            }
#endif
            
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
            try
            {
                string licenseFilePath = GetLicenseFilePath();
                
                // ライセンスファイルを削除
                if (File.Exists(licenseFilePath))
                {
                    File.Delete(licenseFilePath);
                    
                    // ディレクトリが空になった場合は削除
                    string directoryPath = Path.GetDirectoryName(licenseFilePath);
                    if (Directory.Exists(directoryPath) && 
                        Directory.GetFiles(directoryPath).Length == 0 && 
                        Directory.GetDirectories(directoryPath).Length == 0)
                    {
                        Directory.Delete(directoryPath);
                        resultMessage = "ゆにさきポーズシステムのライセンスを削除しました。";
                    }
                    else
                    {
                        resultMessage = "ゆにさきポーズシステムのライセンスを削除しました。\n（他のゆにさきスタジオ商品のライセンスは保持されます）";
                    }
                    
                    Debug.Log($"[PosingSystem] {resultMessage}");
                }
                else
                {
                    resultMessage = "ライセンス情報は見つかりませんでした。";
                }
            }
            catch (System.Exception ex)
            {
                resultMessage = $"ライセンスの削除に失敗しました: {ex.Message}";
                Debug.LogError($"[PosingSystem] {resultMessage}");
            }
#endif
            
            EditorUtility.DisplayDialog(
                "ライセンス削除",
                resultMessage,
                "OK"
            );
        }
    }
}
