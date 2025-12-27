using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Presets;
using System.IO;
using jp.unisakistudio.posingsystem;
using System.Linq;
using System;

namespace jp.unisakistudio.posingsystemeditor
{
    public class PosingPresetCreatorWindow : EditorWindow
    {
        private string avatarName = "";
        private List<GameObject> applicablePrefabs = new List<GameObject>();
        private List<GameObject> targetAvatarPrefabs = new List<GameObject>();
        private List<string> targetGameObjectNames = new List<string>();
        private PosingSystem sourcePosingSystem;
        private Vector2 scrollPosition;

        public static void ShowWindow(PosingSystem source)
        {
            var window = GetWindow<PosingPresetCreatorWindow>("プリセット作成");
            window.sourcePosingSystem = source;
            window.Initialize();
            window.Show();
        }

        private void Initialize()
        {
            if (sourcePosingSystem == null) return;

            var avatar = sourcePosingSystem.GetAvatar();
            if (avatar != null)
            {
                if (string.IsNullOrEmpty(avatarName))
                {
                    avatarName = avatar.name;
                }
                
                // 自動検出を実行
                AutoDetectAvatarPrefabsAndNames();
            }

            // 適用するPosingSystem Prefabの自動設定
            var psPrefab = PrefabUtility.GetCorrespondingObjectFromSource(sourcePosingSystem.gameObject);
            if (psPrefab != null && !applicablePrefabs.Contains(psPrefab))
            {
                applicablePrefabs.Add(psPrefab);
                
                // 同じフォルダ内のPrefabVariantも自動追加
                AutoDetectPosingSystemVariants(psPrefab);
            }
        }

        private void AutoDetectPosingSystemVariants(GameObject basePrefab)
        {
            // ベースPrefabのパスとフォルダを取得
            string basePrefabPath = AssetDatabase.GetAssetPath(basePrefab);
            if (string.IsNullOrEmpty(basePrefabPath)) return;

            string folderPath = Path.GetDirectoryName(basePrefabPath).Replace("\\", "/");

            // 同じフォルダ内のPrefabを検索
            var prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { folderPath });
            
            foreach (var guid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                
                // サブフォルダのPrefabは除外（同じ階層のみ）
                if (Path.GetDirectoryName(prefabPath).Replace("\\", "/") != folderPath)
                    continue;

                // ベースPrefab自身は除外
                if (prefabPath == basePrefabPath)
                    continue;

                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                if (prefab == null) continue;

                // このPrefabがベースPrefabのVariantかどうかをチェック
                if (IsPrefabVariantOf(prefab, basePrefab))
                {
                    if (!applicablePrefabs.Contains(prefab))
                    {
                        applicablePrefabs.Add(prefab);
                        Debug.Log($"[PosingSystem] PrefabVariantを自動検出: {prefab.name}");
                    }
                }
            }
        }

        private bool IsPrefabVariantOf(GameObject prefab, GameObject targetBase)
        {
            // PrefabのVariant親を辿って、targetBaseに到達するかチェック
            var current = PrefabUtility.GetCorrespondingObjectFromSource(prefab);
            
            while (current != null)
            {
                if (current == targetBase)
                {
                    return true;
                }
                current = PrefabUtility.GetCorrespondingObjectFromSource(current);
            }
            
            return false;
        }

        private void AutoDetectAvatarPrefabsAndNames()
        {
            var avatar = sourcePosingSystem.GetAvatar();
            if (avatar == null) return;

            var avatarObject = avatar.gameObject;
            
            // variantParentを遡ってすべてのPrefabパスを取得
            var variantPaths = new List<string>();
            var current = PrefabUtility.GetCorrespondingObjectFromSource(avatarObject);
            
            while (current != null)
            {
                string path = AssetDatabase.GetAssetPath(current);
                if (!string.IsNullOrEmpty(path))
                {
                    variantPaths.Add(path);
                }
                current = PrefabUtility.GetCorrespondingObjectFromSource(current);
            }

            if (variantPaths.Count == 0) return;

            // パスの共通部分を取得（アバターデータフォルダ）
            string commonPath = GetCommonPath(variantPaths);
            if (string.IsNullOrEmpty(commonPath)) return;

            Debug.Log($"[PosingSystem]検出されたアバターデータフォルダ: {commonPath}");

            // 共通フォルダ配下のすべてのPrefabとFBXを検索
            var allAssets = AssetDatabase.FindAssets("t:GameObject", new[] { commonPath });
            
            foreach (var guid in allAssets)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                GameObject asset = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
                
                if (asset == null) continue;

                bool isFBX = assetPath.ToLower().EndsWith(".fbx");
                
                if (isFBX)
                {
                    // FBXの場合：HumanoidAvatarかチェック
                    var importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
                    if (importer != null && importer.animationType == ModelImporterAnimationType.Human)
                    {
                        if (!targetAvatarPrefabs.Contains(asset))
                        {
                            targetAvatarPrefabs.Add(asset);
                            
                            // GameObject名も追加
                            if (!targetGameObjectNames.Contains(asset.name))
                            {
                                targetGameObjectNames.Add(asset.name);
                            }
                        }
                    }
                }
                else if (assetPath.ToLower().EndsWith(".prefab"))
                {
                    // Prefabの場合：ルートにVRCAvatarDescriptorがあるかチェック
                    if (asset.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>() != null)
                    {
                        if (!targetAvatarPrefabs.Contains(asset))
                        {
                            targetAvatarPrefabs.Add(asset);
                            
                            // GameObject名も追加
                            if (!targetGameObjectNames.Contains(asset.name))
                            {
                                targetGameObjectNames.Add(asset.name);
                            }
                        }
                    }
                }
            }

            Debug.Log($"[PosingSystem]自動検出完了: {targetAvatarPrefabs.Count}個のPrefab/FBX, {targetGameObjectNames.Count}個の名前");
        }

        private string GetCommonPath(List<string> paths)
        {
            if (paths.Count == 0) return "";
            if (paths.Count == 1) return Path.GetDirectoryName(paths[0]).Replace("\\", "/");

            // すべてのパスをディレクトリパスに変換
            var directories = paths.Select(p => Path.GetDirectoryName(p).Replace("\\", "/")).ToList();
            
            // 最初のパスを基準に共通部分を探す
            string[] firstPathParts = directories[0].Split('/');
            int commonLength = firstPathParts.Length;

            for (int i = 1; i < directories.Count; i++)
            {
                string[] currentPathParts = directories[i].Split('/');
                int matchLength = 0;

                for (int j = 0; j < Math.Min(firstPathParts.Length, currentPathParts.Length); j++)
                {
                    if (firstPathParts[j] == currentPathParts[j])
                    {
                        matchLength++;
                    }
                    else
                    {
                        break;
                    }
                }

                commonLength = Math.Min(commonLength, matchLength);
            }

            if (commonLength == 0) return "";

            return string.Join("/", firstPathParts.Take(commonLength));
        }

        private void OnGUI()
        {
            if (sourcePosingSystem == null)
            {
                EditorGUILayout.HelpBox("ソースとなるPosingSystemが見つかりません。", MessageType.Error);
                return;
            }

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUILayout.LabelField("プリセット作成設定", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            EditorGUILayout.HelpBox("アニメーション調節機能を使用して姿勢をアバターに合わせたデータを、別のアバターデータでも使用したい場合は、プリセットを作成すると便利です。PrefabsやFBXを登録してプリセットを作成すると、そのアバターに" + sourcePosingSystem.name + "を追加した時に自動でプリセットが適用されるようになります。また、作成したプリセットは公開、配付、販売が可能です。", MessageType.None);
            EditorGUILayout.Space();

            // アバター名
            avatarName = EditorGUILayout.TextField("アバター名", avatarName);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("アバターデータフォルダの自動検出", EditorStyles.boldLabel);
            if (GUILayout.Button("自動検出"))
            {
                targetAvatarPrefabs.Clear();
                targetGameObjectNames.Clear();
                AutoDetectAvatarPrefabsAndNames();
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("適用するPrefabs (PosingSystem)", EditorStyles.boldLabel);
            DrawGameObjectList(applicablePrefabs);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("配置対象のアバターPrefabs/FBX", EditorStyles.boldLabel);
            if (GUILayout.Button("Projectで選択中のPrefabを追加"))
            {
                foreach (var obj in Selection.objects)
                {
                    if (obj is GameObject go)
                    {
                        if (!targetAvatarPrefabs.Contains(go))
                        {
                            targetAvatarPrefabs.Add(go);
                        }
                    }
                }
            }
            DrawGameObjectList(targetAvatarPrefabs);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("配置対象のアバターPrefab/FBXのファイル名", EditorStyles.boldLabel);
            if (GUILayout.Button("Projectで選択中のファイル名を追加"))
            {
                foreach (var obj in Selection.objects)
                {
                    if (!targetGameObjectNames.Contains(obj.name))
                    {
                        targetGameObjectNames.Add(obj.name);
                    }
                }
            }
            DrawStringList(targetGameObjectNames);

            EditorGUILayout.Space(20);

            if (GUILayout.Button("プリセットを作成", GUILayout.Height(40)))
            {
                CreatePreset();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawGameObjectList(List<GameObject> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                list[i] = (GameObject)EditorGUILayout.ObjectField(list[i], typeof(GameObject), false);
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    list.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                list.Add(null);
            }
        }

        private void DrawStringList(List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                list[i] = EditorGUILayout.TextField(list[i]);
                if (GUILayout.Button("-", GUILayout.Width(20)))
                {
                    list.RemoveAt(i);
                    i--;
                }
                EditorGUILayout.EndHorizontal();
            }
            if (GUILayout.Button("+", GUILayout.Width(20)))
            {
                list.Add("");
            }
        }

        private void CreatePreset()
        {
            if (string.IsNullOrEmpty(avatarName))
            {
                EditorUtility.DisplayDialog("エラー", "アバター名を入力してください。", "OK");
                return;
            }

            // PosingSystemResourceFolderを探す
            var generatedFolderPath = PosingSystemEditor.GetGeneratedFolderPath();

            // フォルダ作成 (名前重複回避)
            string targetFolderPath = Path.Combine(generatedFolderPath, avatarName).Replace("\\", "/");
            targetFolderPath = AssetDatabase.GenerateUniqueAssetPath(targetFolderPath);
            Directory.CreateDirectory(targetFolderPath);

            try
            {
                // PosingSystemをクローンして初期化
                GameObject tempObj = Instantiate(sourcePosingSystem.gameObject);
                PosingSystem tempPs = tempObj.GetComponent<PosingSystem>();

                // プロパティ初期化
                tempPs.developmentMode = false;
                tempPs.previewAvatarObject = null; // プレビューアバターは保存しない
                tempPs.isWarning = false;
                tempPs.isError = false;

                // 調整アニメーションをコピーして参照を更新
                CopyAndUpdateAdjustmentClips(tempPs, targetFolderPath);

                // Preset作成
                Preset preset = new Preset(tempPs);
                string presetPath = Path.Combine(targetFolderPath, avatarName + "_Preset.preset").Replace("\\", "/");
                AssetDatabase.CreateAsset(preset, presetPath);

                // 一時オブジェクト破棄
                DestroyImmediate(tempObj);

                // PosingSystemPresetDefines作成
                var defines = CreateInstance<PosingSystemPresetDefines>();
                var presetDefine = new PosingSystemPresetDefines.PresetDefine();

                presetDefine.avatarName = avatarName;
                presetDefine.prefabs = new List<GameObject>(applicablePrefabs.Where(x => x != null));
                presetDefine.preset = preset;
                presetDefine.prefabsNames = new List<string>(targetGameObjectNames.Where(x => !string.IsNullOrEmpty(x)));

                // Prefab GUIDのハッシュ化
                presetDefine.prefabsHashes = new List<string>();
                foreach (var prefab in targetAvatarPrefabs)
                {
                    if (prefab == null) continue;
                    string path = AssetDatabase.GetAssetPath(prefab);
                    string guid = AssetDatabase.AssetPathToGUID(path);
                    if (!string.IsNullOrEmpty(guid))
                    {
                        presetDefine.prefabsHashes.Add(PosingSystemMenuItems.GetPosingSystemGUIDHash(guid));
                    }
                }

                defines.presetDefines.Add(presetDefine);

                string definesPath = Path.Combine(targetFolderPath, avatarName + "_PresetDefines.asset").Replace("\\", "/");
                AssetDatabase.CreateAsset(defines, definesPath);

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                // 作成したフォルダを選択状態にする
                var folderObj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(targetFolderPath);
                Selection.activeObject = folderObj;
                EditorGUIUtility.PingObject(folderObj);

                EditorUtility.DisplayDialog("完了", "プリセットを作成しました。", "OK");
                Close();
            }
            catch (System.Exception e)
            {
                EditorUtility.DisplayDialog("エラー", "プリセット作成中にエラーが発生しました。\n" + e.Message, "OK");
                Debug.LogException(e);
            }
        }

        private void CopyAndUpdateAdjustmentClips(PosingSystem posingSystem, string targetFolderPath)
        {
            var copiedClips = new Dictionary<AnimationClip, AnimationClip>();
            string posingSystemObjectName = sourcePosingSystem.gameObject.name;

            // definesの調整アニメーションをコピー
            foreach (var define in posingSystem.defines)
            {
                foreach (var animation in define.animations)
                {
                    animation.previewImage = null;
                    if (animation.adjustmentClip != null)
                    {
                        string poseName = animation.displayName;
                        animation.adjustmentClip = CopyAnimationClip(animation.adjustmentClip, targetFolderPath, copiedClips, posingSystemObjectName, poseName);
                    }
                }
            }

            // overrideDefinesの調整アニメーションをコピー
            if (posingSystem.overrideDefines != null)
            {
                foreach (var overrideDefine in posingSystem.overrideDefines)
                {
                    overrideDefine.previewImage = null;
                    if (overrideDefine.adjustmentClip != null)
                    {
                        string poseName = overrideDefine.stateType.ToString();
                        overrideDefine.adjustmentClip = CopyAnimationClip(overrideDefine.adjustmentClip, targetFolderPath, copiedClips, posingSystemObjectName, poseName);
                    }
                }
            }

            AssetDatabase.SaveAssets();
        }

        private AnimationClip CopyAnimationClip(AnimationClip originalClip, string targetFolderPath, Dictionary<AnimationClip, AnimationClip> copiedClips, string posingSystemObjectName, string poseName)
        {
            // 既にコピー済みの場合はそれを返す
            if (copiedClips.ContainsKey(originalClip))
            {
                return copiedClips[originalClip];
            }

            // 新しいアニメーションクリップを作成
            AnimationClip newClip = new AnimationClip();
            EditorUtility.CopySerialized(originalClip, newClip);

            // ファイル名を生成: (PosingSystemオブジェクト名)_(アバター名)_(姿勢名).anim
            string fileName = $"{posingSystemObjectName}_{avatarName}_{poseName}.anim";
            
            // 同名ファイルがある場合は番号を付ける
            string newPath = Path.Combine(targetFolderPath, fileName).Replace("\\", "/");
            int counter = 1;
            while (AssetDatabase.LoadAssetAtPath<AnimationClip>(newPath) != null)
            {
                fileName = $"{posingSystemObjectName}_{avatarName}_{poseName}_{counter}.anim";
                newPath = Path.Combine(targetFolderPath, fileName).Replace("\\", "/");
                counter++;
            }

            // アセットとして保存
            AssetDatabase.CreateAsset(newClip, newPath);
            
            // キャッシュに追加
            copiedClips[originalClip] = newClip;

            return newClip;
        }
    }
}


