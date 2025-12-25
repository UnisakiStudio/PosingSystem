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

namespace jp.unisakistudio.posingsystemeditor
{
    public static class PosingSystemMenuItems
    {
        private static string _md5HashSeed = "_jp.unisakistudio.posingsystem.guihashmaker";

        public static string GetPosingSystemGUIDHash(string guid)
        {
            var md5Hash = MD5.Create().ComputeHash(Encoding.UTF8.GetBytes(guid + _md5HashSeed));
            var hash = BitConverter.ToString(md5Hash).Replace("-", "").ToLower();
            return hash;
        }

        static public void AddPrefab(string name)
        {
            if (Selection.activeGameObject)
            {
                var avatar = Selection.activeGameObject.GetComponent<VRCAvatarDescriptor>();

                // 導入するPosingSystemのPrefabsを探す
                var guids = AssetDatabase.FindAssets("t:prefab " + name)
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path => string.Equals(Path.GetFileNameWithoutExtension(path), name, System.StringComparison.CurrentCulture));
                if (guids.Count() == 0)
                {
                    EditorUtility.DisplayDialog("エラー", "Prefabsが見つかりません。ツールを再度インポートしなおしてください", "閉じる");
                    return;
                }

                // 導入するPosingSystemのPrefabsを読み込む
                var prefabs = AssetDatabase.LoadAssetAtPath<GameObject>(guids.First());

                // 導入するPosingSystemのPrefabsをアバターに配置する
                var instance = PrefabUtility.InstantiatePrefab(prefabs, avatar.transform);
                Undo.RegisterCreatedObjectUndo(instance, "Add Prefab");
                UnityEditor.EditorUtility.SetDirty(instance);

                // アバターのオブジェクトを取得
                var avatarObject = avatar.gameObject;

                // PrefabVariantのVariantParentを遡ってすべてのPrefabsのGUIDと名前を取得する
                var variantParents = new List<GameObject>() { avatarObject };
                var variantParent = PrefabUtility.GetCorrespondingObjectFromSource(avatarObject);
                while (variantParent != null)
                {
                    variantParents.Add(variantParent);
                    variantParent = PrefabUtility.GetCorrespondingObjectFromSource(variantParent);
                }
                var variantParentNames = variantParents
                    .Select(parent => parent.name)
                    .ToList();
                var variantParentGUIDHashes = variantParents
                    .Select((parent) =>
                    {
                        var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(parent));
                        return GetPosingSystemGUIDHash(guid);
                    })
                    .ToList();

                // PresetDefineを探す
                var presetDefines = AssetDatabase.FindAssets("t:PosingSystemPresetDefines")
                    .Select(AssetDatabase.GUIDToAssetPath)
                    .Select(path => AssetDatabase.LoadAssetAtPath<PosingSystemPresetDefines>(path))
                    .SelectMany(defines => defines.presetDefines);

                Debug.Log("[PosingSystem]" + presetDefines.Count() + "個のPresetDefineが見つかりました");
                Debug.Log("[PosingSystem]" + variantParentGUIDHashes.Count() + "個のGUIDが見つかりました:" + variantParentGUIDHashes.Aggregate((a, b) => a + ", " + b));
                Debug.Log("[PosingSystem]" + variantParentNames.Count() + "個の名前が見つかりました:" + variantParentNames.Aggregate((a, b) => a + ", " + b));

                // 配置するPrefabsに対応しているものだけにしぼる
                presetDefines = presetDefines.Where(presetDefine => presetDefine.prefabs.Contains(prefabs)).ToList();

                Debug.Log("[PosingSystem]" + presetDefines.Count() + "個のPresetDefineがPrefabsに適合しました");

                // 配置するPrefabsに対応しているPresetDefineを探す
                var presetDefine = presetDefines.FirstOrDefault(presetDefine => presetDefine.prefabsHashes.FindAll(variantParentGUIDHashes.Contains).Count() > 0);
                if (presetDefine == null)
                {
                    Debug.Log("[PosingSystem]PrefabsのGUIDに合ったPresetDefineはありませんでした");
                    Debug.Log("[PosingSystem]名前で探します");
                    presetDefine = presetDefines.FirstOrDefault(presetDefine => presetDefine.prefabsNames.FindAll(variantParentNames.Contains).Count() > 0);
                    if (presetDefine == null)
                    {
                        Debug.Log("[PosingSystem]名前に合ったPresetDefineはありませんでした");
                    }
                }
                else
                {
                    Debug.Log("[PosingSystem]PrefabsのGUIDでPresetDefineが見つかりました");
                }

                // PresetDefineを適用する
                if (presetDefine != null)
                {
                    if (EditorUtility.DisplayDialog("情報", "このアバターのために作成された「" + presetDefine.avatarName + "」向けのプリセットが見つかりました。\nこのプリセットを適用するとアバターごとの体型の違いに対応した調整データが自動で設定されます。\n\nプリセットを適用しますか？", "適用する", "スキップ"))
                    {
                        Debug.Log("[PosingSystem]" + presetDefine.avatarName + "を適用します");
                        presetDefine.preset.ApplyTo((instance as GameObject).GetComponent<PosingSystem>(), new string[] { "defines", "overrideDefines", });
                    }
                }

                // さっそくプレビルドをするかどうかユーザーに尋ねる
                if (EditorUtility.DisplayDialog("プレビルド", "プレビルドをしますか？\nプレビルドを行うとアバターのビルドとアップロードが速くなります。", "はい", "いいえ"))
                {
                    // プレビルドをする
                    PosingSystem posingSystem = (instance as GameObject).GetComponent<PosingSystem>();
                    PosingSystemEditor.Prebuild(posingSystem);
                    PosingSystemEditor.RenewAvatar(posingSystem);

                    EditorUtility.DisplayDialog("プレビルド", "プレビルドが完了しました", "OK");
                }
            }
       }
    }
}
