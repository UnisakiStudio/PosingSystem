using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Animations;
using System.Linq;
using jp.unisakistudio.posingsystem;
using System;
using System.IO;
#if UNITY_EDITOR_WIN
using Microsoft.Win32;

#endif


#if MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

namespace jp.unisakistudio.posingsystemeditor
{

    [CustomEditor(typeof(PosingSystem))]
    public class PosingSystemEditor : Editor
    {
        private const string DefaultAdjustmentFolderRoot = "Assets/UnisakiStudio/GeneratedResources";
        private List<ReorderableList> reorderableLists = new List<ReorderableList>();
        List<string> existProducts;
        List<string> existFolders;
        Texture2D exMenuBackground = null;

        // プリセット選択用の変数
        private List<PosingSystemPresetDefines.PresetDefine> _availablePresetDefines = new List<PosingSystemPresetDefines.PresetDefine>();
        private List<string> _presetDefineNames = new List<string>();
        private int _selectedPresetDefineIndex = 0;

        private bool foldoutOverride = false;
        private bool foldoutAnimationDefine = false;

        public delegate List<string> CheckFunction();
        protected static List<CheckFunction> checkFunctions = new List<CheckFunction>();

        const string REGKEY = @"SOFTWARE\UnisakiStudio";
        const string APPKEY = "posingsystem";
        private bool isPosingSystemLicensed = false;

        // Mac/Linux用の設定ファイルパス取得
        private static string GetLicenseFilePath()
        {
#if UNITY_EDITOR_OSX
            // Mac: ~/Library/Application Support/UnisakiStudio/
            string appSupport = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appSupport, "UnisakiStudio", "posingsystem.lic");
#elif UNITY_EDITOR_LINUX
            // Linux: ~/.local/share/UnisakiStudio/
            string homeDir = System.Environment.GetEnvironmentVariable("HOME");
            return Path.Combine(homeDir, ".local", "share", "UnisakiStudio", "posingsystem.lic");
#else
            return null;
#endif
        }
        private void OnEnable()
        {
            existFolders = null;
            //            AnimationMode.StartAnimationMode();
            //            AnimationMode.BeginSampling();

            PosingSystem posingSystem = target as PosingSystem;
            posingSystem.previousErrorCheckTime = DateTime.MinValue;

            // プリセット一覧を読み込み
            LoadAvailablePresetDefines();

            // 使われていないプレビュー用アバターを削除
            DeleteUnusedPreviewAvatar();
        }

        private void DeleteUnusedPreviewAvatar()
        {
            // シーンのすべてのPosingSystemのPreviewAvatarとPreviewAvatarRootの中身を比較して、使っていないものがあれば削除する
            var usingPreviewAvatars = GameObject.FindObjectsOfType<PosingSystem>().Select(posingSystem => posingSystem.previewAvatarObject).ToList();
            var deletePreviewAvatars = new List<GameObject>();
            foreach (Transform previewAvatarTransform in GetPreviewAvatarRoot())
            {
                if (!usingPreviewAvatars.Contains(previewAvatarTransform.gameObject))
                {
                    deletePreviewAvatars.Add(previewAvatarTransform.gameObject);
                }
            }
            while (deletePreviewAvatars.Count > 0)
            {
                var deletePreviewAvatar = deletePreviewAvatars.First();
                deletePreviewAvatars.Remove(deletePreviewAvatar);
                GameObject.DestroyImmediate(deletePreviewAvatar);
            }
            AssetDatabase.SaveAssets();
        }

        public static Transform GetPreviewAvatarRoot()
        {
            // シーンのHierarchyの一番上の階層にある「PreviewAvatarRoot」を取得する
            var previewAvatarRoot = GameObject.Find("PreviewAvatarRoot");
            if (previewAvatarRoot == null)
            {
                previewAvatarRoot = new GameObject("PreviewAvatarRoot");
                previewAvatarRoot.hideFlags = HideFlags.HideInHierarchy;
                EditorUtility.SetDirty(previewAvatarRoot);
            }
            return previewAvatarRoot.transform;
        }

        private static bool IsAndroidBuildTarget()
        {
            return UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android;
        }

        private void LoadAvailablePresetDefines()
        {
            var posingSystem = target as PosingSystem;

            _availablePresetDefines.Clear();
            _presetDefineNames.Clear();

            _presetDefineNames.Add("プリセットを選択");

            // プロジェクト内のすべてのPosingSystemPresetDefinesを検索
            var guids = AssetDatabase.FindAssets("t:PosingSystemPresetDefines");
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var presetDefines = AssetDatabase.LoadAssetAtPath<PosingSystemPresetDefines>(path);
                if (presetDefines != null)
                {
                    foreach (var presetDefine in presetDefines.presetDefines)
                    {
                        // PosingSystemがどのPrefabsを使っているか調べる
                        var prefabs = PrefabUtility.GetCorrespondingObjectFromSource<GameObject>(posingSystem.gameObject);
                        if (presetDefine.prefabs.Contains(prefabs))
                        {
                            _availablePresetDefines.Add(presetDefine);
                            _presetDefineNames.Add(presetDefine.avatarName);
                        }
                    }
                }
            }

            // デフォルト選択をリセット
            _selectedPresetDefineIndex = 0;
        }

        private void ApplySelectedPreset()
        {
            // Popupは先頭にプレースホルダーを入れているため、実データはインデックス0ではなく1から始まる
            if (_availablePresetDefines.Count == 0 || _selectedPresetDefineIndex <= 0 || _selectedPresetDefineIndex > _availablePresetDefines.Count)
            {
                EditorUtility.DisplayDialog("エラー", "選択されたプリセットが無効です。", "OK");
                return;
            }

            // プレースホルダー分だけインデックスを補正
            var selectedPresetDefine = _availablePresetDefines[_selectedPresetDefineIndex - 1];
            var posingSystem = target as PosingSystem;

            // 適用可能なプリセットを検索
            if (selectedPresetDefine.preset != null)
            {
                // プリセットのdefinesプロパティのみを適用
                Undo.RecordObject(posingSystem, "Apply Preset Defines");
                selectedPresetDefine.preset.ApplyTo(posingSystem, new string[] { "defines", "overrideDefines", });
                EditorUtility.SetDirty(posingSystem);

                Debug.Log($"[PosingSystem]プリセット '{selectedPresetDefine.avatarName}' の設定を適用しました。");
                return;
            }

            EditorUtility.DisplayDialog("エラー", "選択されたプリセットに有効な設定が見つかりませんでした。", "OK");
        }

        private void OnDisable()
        {
            //            AnimationMode.EndSampling();
            //            AnimationMode.StopAnimationMode();
        }

        public override void OnInspectorGUI()
        {
            PosingSystem posingSystem = target as PosingSystem;

            var header1Label = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold, fontSize = 20, };
            var header2Label = new GUIStyle(EditorStyles.label) { fontStyle = FontStyle.Bold, fontSize = 16, };

            /*
             * このコメント分を含むここから先の処理はゆにさきポーズシステムを含む商品をゆにさきスタジオから購入した場合に変更することを許可します。
             * つまり購入者はライセンスにまつわるこの先のソースコードを削除して再配布を行うことができます。
             * 逆に、購入をせずにGitHubなどからソースコードを取得しただけの場合、このライセンスに関するソースコードに手を加えることは許可しません。
             */
            if (!isPosingSystemLicensed)
            {
                bool hasLicense = false;

                // Windows: レジストリをチェック
#if UNITY_EDITOR_WIN
                try
                {
                    var regKey = Registry.CurrentUser.CreateSubKey(REGKEY);
                    var regValue = (string)regKey.GetValue(APPKEY);
                    if (regValue == "licensed")
                    {
                        hasLicense = true;
                    }
                }
                catch (System.Exception)
                {
                    // レジストリアクセスに失敗した場合は次のチェックへ
                }
#endif

                // Mac/Linux: 設定ファイルをチェック
#if UNITY_EDITOR_OSX || UNITY_EDITOR_LINUX
                if (!hasLicense)
                {
                    try
                    {
                        string licenseFilePath = GetLicenseFilePath();
                        if (File.Exists(licenseFilePath))
                        {
                            string fileContent = File.ReadAllText(licenseFilePath);
                            if (fileContent == "licensed")
                            {
                                hasLicense = true;
                            }
                        }
                    }
                    catch (System.Exception)
                    {
                        // ファイルアクセスに失敗
                    }
                }
#endif

                if (hasLicense)
                {
                    isPosingSystemLicensed = true;
                }
                else
                {
                    EditorGUILayout.LabelField("ゆにさきポーズシステム", header1Label, GUILayout.Height(30));

                    EditorGUILayout.HelpBox("このコンピュータにはゆにさきポーズシステムの使用が許諾されていません。Boothのショップから可愛いポーズツールを購入して、コンピュータにライセンスをインストールしてください", MessageType.Error);
                    if (EditorGUILayout.LinkButton("可愛いポーズツール(Booth)"))
                    {
                        Application.OpenURL("https://yunisaki.booth.pm/items/5479202");
                    }
                    return;
                }
            }
            /*
             * ライセンス処理ここまで
             */

            if (posingSystem.developmentMode)
            {
                base.OnInspectorGUI();

                // ここに、非表示にしてあるpreviewAvatar等を表示するトグルを追加する
                if (posingSystem.previewAvatarObject != null)
                {
                    var previewAvatarToggle = EditorGUILayout.ToggleLeft("プレビューアバター表示", posingSystem.previewAvatarObject.activeSelf);
                    if (previewAvatarToggle != posingSystem.previewAvatarObject.activeSelf)
                    {
                        posingSystem.previewAvatarObject.SetActive(previewAvatarToggle);
                        posingSystem.previewAvatarObject.hideFlags = previewAvatarToggle ? HideFlags.None : HideFlags.HideInHierarchy;
                        EditorUtility.SetDirty(posingSystem);
                    }
                }

                // シーンのHierarchyの一番上の階層で非表示になっているオブジェクトをすべて表示するボタンを追加
                if (GUILayout.Button("Hierarchyで非表示になっているオブジェクトをすべて表示"))
                {
                    foreach (var obj in posingSystem.gameObject.scene.GetRootGameObjects())
                    {
                        if (obj.hideFlags == HideFlags.HideInHierarchy)
                        {
                            obj.hideFlags = HideFlags.None;
                            obj.SetActive(true);
                        }
                    }
                }

                if (posingSystem.developmentMode != EditorGUILayout.ToggleLeft("開発モード", posingSystem.developmentMode))
                {
                    Undo.RecordObject(posingSystem, "Toggle development mode");
                    reorderableLists.Clear();
                    posingSystem.developmentMode = !posingSystem.developmentMode;
                }

                return;
            }

            // 調整ウィンドウが表示されている時は何もできないようにして、ウィンドウを閉じるボタンを用意する
            if (Resources.FindObjectsOfTypeAll<PosingAnimationAdjustmentWindow>().Where(window => window.IsOpen).Count() > 0)
            {
                EditorGUILayout.HelpBox("アニメーション調整ウィンドウが表示されています。調整が終わったらウィンドウを閉じてください", MessageType.Info);
                if (GUILayout.Button("アニメーション調整ウィンドウを閉じる"))
                {
                    foreach (var window in Resources.FindObjectsOfTypeAll<PosingAnimationAdjustmentWindow>().Where(window => window.IsOpen))
                    {
                        window.Close();
                    }
                }
                else
                {
                    return;
                }
            }

            if (exMenuBackground == null)
            {
                exMenuBackground = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("ExMenuBackground t:Texture")[0]));
            }

            EditorGUILayout.LabelField(posingSystem.settingName, header1Label, GUILayout.Height(30));

            if (posingSystem.previewAvatarObject == null)
            {
                EditorGUILayout.HelpBox("ModularAvatarで服を着せているとポーズサムネイルの洋服が外れて見えますが、アバターアップロード時に再撮影されて正しい画像がメニューに設定されるのでご安心ください", MessageType.Info);
            }

            var avatar = posingSystem.GetAvatar();
            if (avatar == null)
            {
                EditorGUILayout.HelpBox("オブジェクトがVRC用のアバターオブジェクトの中に入っていません。このオブジェクトはVRCAvatarDescriptorコンポーネントの付いたオブジェクトの中に配置してください", MessageType.Error);
                return;
            }
            if (existProducts == null)
            {
                if (avatar != null)
                {
                    existProducts = CheckExistProduct(avatar);
                    posingSystem.previousErrorCheckTime = DateTime.MinValue;
                }
            }
            if (existProducts != null)
            {
                foreach (var existProduct in existProducts)
                {
                    EditorGUILayout.HelpBox(existProduct + "の設定がアバターに残っています！ 不具合が発生する可能性があるので、自動で設定を取り除く機能を使用してください。また、アバター購入時にBaseLayerに設定されていたLocomotion用のAnimatorControllerがある場合は、恐れ入りますが手動で復仇してお使いください。", MessageType.Error);
                    if (GUILayout.Button(existProduct + "の設定を取り除く"))
                    {
                        RemoveExistProduct(avatar, existProduct);
                        existProducts = null;
                        posingSystem.previousErrorCheckTime = DateTime.MinValue;
                    }
                }
            }

            // unitypackage版のAssets用フォルダがあったら警告を出す
            if (existFolders == null)
            {
                existFolders = CheckExistFolder();
            }
            if (existFolders != null)
            {
                foreach (var existFolder in existFolders)
                {
                    EditorGUILayout.HelpBox("unitypackage版の「" + existFolder + "」フォルダがプロジェクトに残っています！ 不具合が発生する可能性があるので、自動で設定を取り除く機能を使用してください。", MessageType.Error);
                    if (GUILayout.Button(existFolder + "のフォルダを取り除く"))
                    {
                        RemoveExistFolder(existFolder);
                        existFolders = null;
                        posingSystem.previousErrorCheckTime = DateTime.MinValue;
                    }
                }
            }

            if (avatar.autoFootsteps)
            {
                EditorGUILayout.HelpBox("アバター設定の「Use Auto-Footsteps for 3 and 4 point tracking」がオンになっています。この設定がオンだと、アバターがゲーム内で自動的に足踏みをしてしまい、姿勢が崩れる可能性があるため、オフにすることが推奨されます", MessageType.Warning);
                if (GUILayout.Button("「Use Auto-Footsteps for 3 and 4 point tracking」をオフにする"))
                {
                    Undo.RecordObject(avatar, "Disable auto footsteps");
                    avatar.autoFootsteps = false;
                    EditorUtility.SetDirty(avatar);
                    posingSystem.previousErrorCheckTime = DateTime.MinValue;
                }
            }

            var avatarAnimator = avatar.GetComponent<Animator>();
            if (avatarAnimator == null)
            {
                EditorGUILayout.HelpBox("アバターにAnimatorが設定されていません。このアバターではこのツールは使えません", MessageType.Error);
                return;
            }

            if (avatarAnimator.avatar == null)
            {
                EditorGUILayout.HelpBox("アバターのAnimatorにAvatarが設定されていません。このアバターではこのツールは使えません", MessageType.Error);
                return;
            }

            if (!avatarAnimator.isHuman)
            {
                EditorGUILayout.HelpBox("このアバターは人型ではありません。アバターのAnimatorにHumanoid型のAvatarが設定されていない場合はこのツールは使えません。", MessageType.Error);
                return;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("設定", header2Label);
            if (IsAndroidBuildTarget())
            {
                EditorGUILayout.HelpBox("Androidビルドターゲットでは姿勢アイコンは生成・設定されません（サイズ削減のため強制無効化）", MessageType.Info);
            }
            else
            {
                var isIconDisabled = EditorGUILayout.ToggleLeft("姿勢アイコン無しモード（Quest等）", posingSystem.isIconDisabled);
                if (isIconDisabled != posingSystem.isIconDisabled)
                {
                    Undo.RecordObject(posingSystem, "Disable icon");
                    posingSystem.isIconDisabled = isIconDisabled;
                    EditorUtility.SetDirty(posingSystem);
                }

                var isIconSmall = EditorGUILayout.ToggleLeft("姿勢アイコン小さいモード(256→64)", posingSystem.isIconSmall);
                if (isIconSmall != posingSystem.isIconSmall)
                {
                    Undo.RecordObject(posingSystem, "Small icon");
                    posingSystem.isIconSmall = isIconSmall;
                    EditorUtility.SetDirty(posingSystem);
                }
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("ビルド時オプション", EditorStyles.boldLabel);
            
            var mergeTrackingControl = EditorGUILayout.ToggleLeft("トラッキング機能をビルド時に統合する", posingSystem.mergeTrackingControl);
            if (mergeTrackingControl != posingSystem.mergeTrackingControl)
            {
                Undo.RecordObject(posingSystem, "Merge Tracking Control");
                posingSystem.mergeTrackingControl = mergeTrackingControl;
                EditorUtility.SetDirty(posingSystem);
            }
            
            var autoImportAvatarAnimations = EditorGUILayout.ToggleLeft("アバターの姿勢設定をビルド時に自動でインポート", posingSystem.autoImportAvatarAnimations);
            if (autoImportAvatarAnimations != posingSystem.autoImportAvatarAnimations)
            {
                Undo.RecordObject(posingSystem, "Auto Import Avatar Animations");
                posingSystem.autoImportAvatarAnimations = autoImportAvatarAnimations;
                EditorUtility.SetDirty(posingSystem);
            }
            
            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("トラブルシューティング", EditorStyles.boldLabel);
            if (GUILayout.Button("アバターの姿勢をリセット（Tポーズに戻す）"))
            {
                ResetAvatarPose(avatar);
            }
            EditorGUILayout.HelpBox("エディタ上でアバターの姿勢がおかしくなった場合、このボタンで元のFBXの姿勢に戻せます", MessageType.None);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("プレビルド関係", header2Label);
            // 警告表示を元の位置に戻し、常にHelpBoxを表示してGUI要素数を安定化
            if (posingSystem.data == null || posingSystem.data.Length == 0)
            {
                EditorGUILayout.HelpBox("プレビルドが実行されていません。プレビルドを行うことをお勧めします", MessageType.Warning);
            }
            else if (PosingSystemConverter.IsPosingSystemDataUpdated(posingSystem))
            {
                EditorGUILayout.HelpBox("オブジェクトの設定が更新されています。再度プレビルドを行ってください", MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox("設定は最新の状態です", MessageType.None);
            }
            // 大きなボタンスタイルを定義
            var bigButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Normal,
                fixedHeight = 40,
                padding = new RectOffset(10, 10, 10, 10)
            };

            var descriptionStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 11,
                wordWrap = true,
                margin = new RectOffset(0, 0, 1, 1) // 行間を狭く
            };

            var foldoutStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 16,
                fontStyle = FontStyle.Normal,
                fixedHeight = 40,
                padding = new RectOffset(10, 10, 10, 10)
            };

            // 更新ボタンとその説明
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(120));
                if (GUILayout.Button("プレビルド実行", bigButtonStyle))
                {
                    Prebuild(posingSystem);
                    RenewAvatar(posingSystem);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("ModularAvatarコンポーネントを生成・更新します", descriptionStyle);
                EditorGUILayout.LabelField("姿勢設定を変更した場合はこのボタンを押すとアバターのビルドが早くなります", descriptionStyle);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            // アイコン更新ボタンとその説明（Android以外）
            if (!IsAndroidBuildTarget())
            {
                GUI.enabled = posingSystem.data != null && posingSystem.data.Length > 0;

                EditorGUILayout.BeginHorizontal();
                {
                    EditorGUILayout.BeginVertical(GUILayout.Width(120));
                    if (GUILayout.Button("アイコンのみ更新", bigButtonStyle))
                    {
                        RenewAvatar(posingSystem);
                    }
                    EditorGUILayout.EndVertical();

                    EditorGUILayout.BeginVertical();
                    EditorGUILayout.LabelField("アイコン撮影用のアバターを更新してアイコンを撮影します", descriptionStyle);
                    EditorGUILayout.LabelField("アバターの見た目を変えたときはこのボタンを押してください", descriptionStyle);
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndHorizontal();

                GUI.enabled = true;

                // ThumbnailPackObjectを選択
                EditorGUILayout.BeginHorizontal();
                var thumbnailPackProperty = serializedObject.FindProperty("thumbnailPackObject");
                GUI.enabled = false;
                EditorGUILayout.PropertyField(thumbnailPackProperty, new GUIContent("アイコン画像ファイル"));
                GUI.enabled = posingSystem.thumbnailPackObject != null;
                if (GUILayout.Button("削除", new GUIStyle(GUI.skin.button) { fixedWidth = 40, }))
                {
                    // ダイアログで確認する
                    if (EditorUtility.DisplayDialog("確認", "アイコン画像ファイルを削除しますか？ この操作は元に戻せませんが、アバター更新で簡単に作り直せます。", "削除する", "キャンセル"))
                    {
                        Undo.RecordObject(posingSystem, "Delete thumbnail pack");
                        AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(posingSystem.thumbnailPackObject));
                        posingSystem.thumbnailPackObject = null;
                        EditorUtility.SetDirty(posingSystem);
                        AssetDatabase.SaveAssets();
                    }
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();

            serializedObject.Update();

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("モーション置き換え機能", header2Label);

            // ボタンでたたむ
            if (foldoutOverride)
            {
                if (GUILayout.Button("▲▲▲モーション置き換え機能を非表示▲▲▲", foldoutStyle))
                {
                    foldoutOverride = false;
                }
            }
            else
            {
                if (GUILayout.Button("▼▼▼モーション置き換え機能を表示▼▼▼", foldoutStyle))
                {
                    foldoutOverride = true;
                }
            }
            if (foldoutOverride)
            {
                // autoImportAvatarAnimationsがオンの場合はHelpBoxを表示
                if (posingSystem.autoImportAvatarAnimations)
                {
                    EditorGUILayout.HelpBox("アバターの固有モーションをビルド時に自動でインポートする設定がオンになっています", MessageType.Info);
                }

                if (GUILayout.Button("アバターから自動インポート") || posingSystem.overrideDefines == null)
                {
                    Undo.RecordObject(posingSystem, "Auto detect override settings");
                    AutoDetectOverrideSettings();
                    serializedObject.Update();
                    EditorUtility.SetDirty(posingSystem);
                    AssetDatabase.SaveAssets();
                }
                var definesProperty = serializedObject.FindProperty("overrideDefines");
                //            EditorGUILayout.PropertyField(definesProperty);
                // 無かったら「設定なし」と表示して、アバターから自動インポートする説明を表示する
                if (posingSystem.overrideDefines == null || posingSystem.overrideDefines?.Count == 0)
                {
                    EditorGUILayout.HelpBox("設定なし", MessageType.None);
                    EditorGUILayout.LabelField("既にアバターに固有の姿勢モーションがある場合はアバターから自動インポートできます", descriptionStyle);
                }
                int removeIndex = -1;
                for (int i = 0; i < posingSystem.overrideDefines.Count; i++)
                {
                    var overrideDefine = posingSystem.overrideDefines[i];
                    var defineProperty = serializedObject.FindProperty("overrideDefines").GetArrayElementAtIndex(i);
                    EditorGUILayout.BeginHorizontal();
                    {
                        EditorGUILayout.PropertyField(defineProperty);
                        // ボタンサイズは横幅30で縦は高さ20
                        if (GUILayout.Button("x", new GUIStyle(GUI.skin.button)
                        {
                            fontStyle = FontStyle.Normal,
                            fixedWidth = 30,
                            padding = new RectOffset(10, 10, 10, 10)
                        }))
                        {
                            removeIndex = i;
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                }

                if (removeIndex != -1)
                {
                    Undo.RecordObject(posingSystem, "Remove override define");
                    posingSystem.overrideDefines.RemoveAt(removeIndex);
                    EditorUtility.SetDirty(posingSystem);
                    AssetDatabase.SaveAssets();
                }

                EditorGUILayout.BeginHorizontal();
                // 追加ボタン
                if (GUILayout.Button("追加"))
                {
                    Undo.RecordObject(posingSystem, "Add override define");
                    posingSystem.overrideDefines.Add(new PosingSystem.OverrideAnimationDefine());
                    EditorUtility.SetDirty(posingSystem);
                    AssetDatabase.SaveAssets();
                }
                // 一つ以上あったらクリアボタンを有効
                GUI.enabled = posingSystem.overrideDefines.Count > 0;
                if (GUILayout.Button("全クリア"))
                {
                    Undo.RecordObject(posingSystem, "Clear all override defines");
                    posingSystem.overrideDefines.Clear();
                    EditorUtility.SetDirty(posingSystem);
                    AssetDatabase.SaveAssets();
                }
                GUI.enabled = true;
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();



            // definesセクション全体をBeginVerticalで囲んでObjectFieldのGUI順序を安定化
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("姿勢アニメーション設定", header2Label);

            // プリセット選択と適用
            EditorGUILayout.LabelField("プリセット選択", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            {
                if (_availablePresetDefines.Count > 0)
                {
                    _selectedPresetDefineIndex = EditorGUILayout.Popup(_selectedPresetDefineIndex, _presetDefineNames.ToArray());

                    using (new EditorGUI.DisabledScope(_selectedPresetDefineIndex <= 0))
                    {
                        if (GUILayout.Button("プリセットを適用"))
                        {
                            ApplySelectedPreset();
                        }
                    }
                }
                else
                {
                    EditorGUILayout.LabelField("プリセットが見つかりません", EditorStyles.helpBox);
                    if (GUILayout.Button("再読込"))
                    {
                        LoadAvailablePresetDefines();
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(160));
                if (GUILayout.Button("プリセットを作成", bigButtonStyle))
                {
                    PosingPresetCreatorWindow.ShowWindow(posingSystem);
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("あらかじめアバターに合わせてアニメーション調整したデータをまとめたプリセットを適用すると、アバターに合わせた姿勢を簡単に設定できます", descriptionStyle);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            // アニメーション調整ウィンドウを開くボタン
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical(GUILayout.Width(160));
                if (GUILayout.Button("アニメーションを調整", bigButtonStyle))
                {
                    PosingAnimationAdjustmentWindow.ShowWindow(posingSystem);
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("アニメーションを微調整するためのウィンドウを開きます", descriptionStyle);
                EditorGUILayout.LabelField("アバターの体型の違いにより姿勢に気になるところがある場合に使用します", descriptionStyle);
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.EndHorizontal();

            // ボタンでたたむ
            if (foldoutAnimationDefine)
            {
                if (GUILayout.Button("▲▲▲姿勢アニメーション設定を非表示▲▲▲", foldoutStyle))
                {
                    foldoutAnimationDefine = false;
                }
            }
            else
            {
                if (GUILayout.Button("▼▼▼姿勢アニメーション設定を表示▼▼▼", foldoutStyle))
                {
                    foldoutAnimationDefine = true;
                }
            }
            if (foldoutAnimationDefine)
            {
                if (!IsAndroidBuildTarget() && !Application.isPlaying)
                {
                    CheckAndDeleteThumbnailPack(posingSystem);
                    PosingSystemConverter.TakeScreenshot(posingSystem, false, true);
                }

                for (int i = 0; i < posingSystem.defines.Count; i++)
                {
                    var define = posingSystem.defines[i];
                    EditorGUILayout.BeginVertical();

                    var animationsProperty = serializedObject.FindProperty("defines").GetArrayElementAtIndex(i).FindPropertyRelative("animations");
                    ReorderableList reorderableList = null;
                    if (reorderableLists.Count <= i)
                    {
                        reorderableList = new ReorderableList(base.serializedObject, animationsProperty);
                        reorderableList.drawElementCallback += (Rect rect, int index, bool selected, bool focused) =>
                        {
                            SerializedProperty property = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                            EditorGUI.PropertyField(rect, property, GUIContent.none);
                        };
                        reorderableList.drawHeaderCallback += rect =>
                        {
                            if (define.icon)
                            {
                                GUI.Box(new Rect(rect.x - 6, rect.y, rect.width + 12, rect.height + 1), new GUIContent());
                                GUI.DrawTexture(new Rect(rect.x - 3, rect.y + 1, 36, 36), exMenuBackground, ScaleMode.ScaleToFit);
                                GUI.DrawTexture(new Rect(rect.x - 1, rect.y + 1, 32, 32), define.icon, ScaleMode.ScaleToFit);
                                EditorGUI.LabelField(new Rect(rect.x + 40, rect.y, rect.width - rect.x - 40, 20), define.menuName + define.description);
                            }
                            else
                            {
                                EditorGUI.LabelField(rect, define.menuName + define.description);
                            }
                        };
                        reorderableList.onSelectCallback += (ReorderableList list) =>
                        {
                            foreach (var otherList in reorderableLists)
                            {
                                if (reorderableList == otherList)
                                {
                                    continue;
                                }
                                otherList.index = -1;
                            }
                        };
                        if (define.icon != null)
                        {
                            reorderableList.headerHeight = 40;
                        }
                        else
                        {
                            reorderableList.headerHeight = EditorGUIUtility.singleLineHeight;
                        }
                        reorderableList.elementHeight = EditorGUIUtility.singleLineHeight * 6 + 5;
                        reorderableLists.Add(reorderableList);
                    }
                    else
                    {
                        reorderableList = reorderableLists[i];
                    }

                    reorderableList.DoLayoutList();
                    serializedObject.ApplyModifiedProperties();

                    PosingSystem.AnimationDefine changedAnimation = null;
                    foreach (var animation in define.animations)
                    {
                        if (animation.initial != animation.initialSet)
                        {
                            changedAnimation = animation;
                        }
                    }
                    if (changedAnimation != null)
                    {
                        foreach (var avatarPosingSystem in avatar.GetComponentsInChildren<PosingSystem>())
                        {
                            foreach (var posingDefine in avatarPosingSystem.defines)
                            {
                                if (posingDefine.paramName != define.paramName)
                                {
                                    continue;
                                }
                                foreach (var animation in posingDefine.animations)
                                {
                                    if (animation.initial && changedAnimation != animation)
                                    {
                                        animation.initial = false;
                                        animation.initialSet = false;
                                        animation.previewImage = null;
                                        posingSystem.previousErrorCheckTime = DateTime.MinValue;
                                        EditorUtility.SetDirty(avatarPosingSystem);
                                    }
                                    else if (changedAnimation == animation)
                                    {
                                        animation.initial = animation.initialSet;
                                        animation.previewImage = null;
                                        posingSystem.previousErrorCheckTime = DateTime.MinValue;
                                        EditorUtility.SetDirty(avatarPosingSystem);
                                    }
                                }
                            }
                        }
                    }

                    EditorGUILayout.EndVertical();
                }
            }
            if (foldoutAnimationDefine)
            {
                if (GUILayout.Button("姿勢アニメーション設定を非表示", foldoutStyle))
                {
                    foldoutAnimationDefine = false;
                }
            }
            EditorGUILayout.EndVertical(); // definesセクション全体の終了


            serializedObject.ApplyModifiedProperties();

            if (!IsAndroidBuildTarget())
            {
                if (GUILayout.Button("サムネ更新"))
                {
                    ClearThumbnailPack(posingSystem);
                    CreateThumbnailPack(posingSystem);
                    PosingSystemConverter.TakeScreenshot(posingSystem, true, false);
                }
            }

            if (posingSystem.developmentMode != EditorGUILayout.ToggleLeft("開発モード", posingSystem.developmentMode))
            {
                Undo.RecordObject(posingSystem, "Toggle development mode");
                reorderableLists.Clear();
                posingSystem.developmentMode = !posingSystem.developmentMode;
            }
        }

        public static void Prebuild(PosingSystem posingSystem)
        {
            PosingSystemConverter.ConvertToModularAvatarComponents(posingSystem);
            posingSystem.previousErrorCheckTime = DateTime.MinValue;
            EditorUtility.SetDirty(posingSystem);
        }

        /// <summary>
        /// アバターの姿勢をFBX内のモデルの姿勢（通常はTポーズ）にリセットします。
        /// エディタ上でアバターの姿勢がおかしくなった場合の復旧用です。
        /// </summary>
        public static void ResetAvatarPose(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor avatar)
        {
            if (avatar == null)
            {
                Debug.LogWarning("[PosingSystem] アバターが指定されていません。");
                return;
            }

            var animator = avatar.GetComponent<Animator>();
            if (animator == null)
            {
                Debug.LogWarning("[PosingSystem] アバターにAnimatorコンポーネントがありません。");
                return;
            }

            if (animator.avatar == null)
            {
                Debug.LogWarning("[PosingSystem] AnimatorにAvatarが設定されていません。");
                return;
            }

            try
            {
                // AnimationModeが動作中の場合は停止
                if (AnimationMode.InAnimationMode())
                {
                    AnimationMode.StopAnimationMode();
                }

                // 元のPrefabまたはFBXを探す
                GameObject sourcePrefab = FindOriginalPrefabOrFBX(avatar.gameObject);
                
                if (sourcePrefab != null)
                {
                    // 元のプレハブ/FBXからボーンのローカル回転をコピー
                    CopyBoneTransformsFromSource(avatar.gameObject, sourcePrefab);
                    Debug.Log("[PosingSystem] アバターの姿勢を元のFBX/Prefabの姿勢にリセットしました。");
                }
                else
                {
                    // 元のPrefabが見つからない場合はAnimator.Rebindを使用
                    Debug.LogWarning("[PosingSystem] 元のPrefab/FBXが見つかりませんでした。Animator.Rebindで代替リセットを行います。");
                    animator.Rebind();
                    animator.Update(0f);
                }

                // Transform位置と回転もリセット
                Undo.RecordObject(avatar.transform, "Reset Avatar Pose");
                avatar.transform.localPosition = Vector3.zero;
                avatar.transform.localRotation = Quaternion.identity;

                // GameObjectをリフレッシュ
                avatar.gameObject.SetActive(false);
                avatar.gameObject.SetActive(true);

                EditorUtility.SetDirty(avatar.gameObject);
                SceneView.RepaintAll();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PosingSystem] 姿勢リセット中にエラーが発生しました: {e.Message}\n{e.StackTrace}");
            }
        }

        /// <summary>
        /// 元のPrefabまたはFBXを探す
        /// </summary>
        private static GameObject FindOriginalPrefabOrFBX(GameObject avatarObject)
        {
            // PrefabVariantを遡って元のPrefab/FBXを探す
            var current = PrefabUtility.GetCorrespondingObjectFromSource(avatarObject);
            GameObject lastValid = current;
            
            while (current != null)
            {
                lastValid = current;
                current = PrefabUtility.GetCorrespondingObjectFromSource(current);
            }
            
            return lastValid;
        }

        /// <summary>
        /// 元のPrefab/FBXからボーンのローカル回転をコピー
        /// </summary>
        private static void CopyBoneTransformsFromSource(GameObject targetAvatar, GameObject sourcePrefab)
        {
            // ターゲットの全てのTransformを取得
            var targetTransforms = targetAvatar.GetComponentsInChildren<Transform>(true);
            
            // ソースの全てのTransformを辞書化
            var sourceTransformDict = new Dictionary<string, Transform>();
            foreach (var t in sourcePrefab.GetComponentsInChildren<Transform>(true))
            {
                // フルパスを使用して一意に識別
                string path = GetTransformPath(t, sourcePrefab.transform);
                if (!sourceTransformDict.ContainsKey(path))
                {
                    sourceTransformDict[path] = t;
                }
            }
            
            // 各ターゲットTransformにソースの回転をコピー
            foreach (var targetTransform in targetTransforms)
            {
                string path = GetTransformPath(targetTransform, targetAvatar.transform);
                
                if (sourceTransformDict.TryGetValue(path, out var sourceTransform))
                {
                    Undo.RecordObject(targetTransform, "Reset Bone Transform");
                    targetTransform.localRotation = sourceTransform.localRotation;
                    targetTransform.localPosition = sourceTransform.localPosition;
                    targetTransform.localScale = sourceTransform.localScale;
                }
            }
        }

        /// <summary>
        /// ルートからのTransformパスを取得
        /// </summary>
        private static string GetTransformPath(Transform transform, Transform root)
        {
            if (transform == root)
            {
                return "";
            }
            
            var path = transform.name;
            var current = transform.parent;
            
            while (current != null && current != root)
            {
                path = current.name + "/" + path;
                current = current.parent;
            }
            
            return path;
        }

        public static void RenewAvatar(PosingSystem posingSystem)
        {
            if (posingSystem.previewAvatarObject != null)
            {
                GameObject.DestroyImmediate(posingSystem.previewAvatarObject);
                posingSystem.previewAvatarObject = null;
            }
            ClearThumbnailPack(posingSystem);
            CreateThumbnailPack(posingSystem);
            PosingSystemConverter.TakeScreenshot(posingSystem, true, false);
            PosingSystemConverter.SetMenuIcon(posingSystem);

            posingSystem.savedInstanceId = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(posingSystem).ToString();
            posingSystem.previousErrorCheckTime = DateTime.MinValue;
            EditorUtility.SetDirty(posingSystem);
        }

        public static List<(string name, List<string> animatorControllerNames, List<string> checkExpressionParametersNames, List<string> expressionParametersNames, List<string> expressionsMenuNames, List<string> prefabsNames)> productDefines = new List<(string name, List<string> animatorControllerNames, List<string> checkExpressionParametersNames, List<string> expressionParametersNames, List<string> expressionsMenuNames, List<string> prefabsNames)>
        {
            (
                "可愛い座りツール",
                new List<string>
                {
                    "KawaiiSitting_Locomotion",
                    "SleepTogether_KawaiiSitting_Locomotion",
                    "VirtualLoveBoy_KawaiiSitting_Locomotion",
                    "VirtualLoveGirl_KawaiiSitting_Locomotion",
                    "SleepTogether_VirtualLoveBoy_KawaiiSitting_Locomotion",
                    "SleepTogether_VirtualLoveGirl_KawaiiSitting_Locomotion",
                },
                 new List<string>
                {
                    "SitShallow",
                    "SitDeep",
                },
                 new List<string>
                {
                    "SitShallow",
                    "SitDeep",
                    "SitSleep",
                    "LocomotionLock",
                    "HeadTrackingLock",
                    "HandTrackingLock",
                    "FootHeight",
                },
                new List<string>
                {
                    "KawaiiSitting_ExpressionsMenu",
                    "KawaiiSitting_FootHeight_ExpressionsMenu",
                },
                new List<string>
                {
                }
            ),

            (
                "三点だいしゅきツール",
                new List<string>
                {
                    "VirtualLoveBoy_Locomotion",
                    "VirtualLoveBoy_KawaiiSitting_Locomotion",
                    "VirtualLoveGirl_Locomotion",
                    "VirtualLoveGirl_KawaiiSitting_Locomotion",
                    "SleepTogether_VirtualLoveBoy_Locomotion",
                    "SleepTogether_VirtualLoveBoy_KawaiiSitting_Locomotion",
                    "SleepTogether_VirtualLoveGirl_Locomotion",
                    "SleepTogether_VirtualLoveGirl_KawaiiSitting_Locomotion",
                },
                 new List<string>
                {
                    "VirtualLoveType",
                },
                 new List<string>
                {
                    "VirtualLoveType",
                    "LocomotionLock",
                    "HeadTrackingLock",
                    "HandTrackingLock",
                    "FootHeight",
                },
                new List<string>
                {
                    "VirtualLove_ExpressionsMenu",
                    "VirtualLove_FootHeight_ExpressionsMenu",
                },
                new List<string>
                {
                }
            ),

            (
                "添い寝ツール",
                new List<string>
                {
                    "SleepTogether_Locomotion",
                    "SleepTogether_KawaiiSitting_Locomotion",
                    "SleepTogether_VirtualLoveBoy_Locomotion",
                    "SleepTogether_VirtualLoveBoy_KawaiiSitting_Locomotion",
                    "SleepTogether_VirtualLoveGirl_Locomotion",
                    "SleepTogether_VirtualLoveGirl_KawaiiSitting_Locomotion",
                },
                 new List<string>
                {
                    "SleepTogether",
                    "RightStationType",
                    "LeftStationType",
                    "BedHeight",
                },
                 new List<string>
                {
                    "SleepTogether",
                    "RightStationType",
                    "LeftStationType",
                    "BedHeight",
                    "SitSleep",
                    "LocomotionLock",
                    "HeadTrackingLock",
                    "HandTrackingLock",
                    "FootHeight",
                },
                new List<string>
                {
                    "SleepTogether_ExpressionsMenu",
                    "SleepTogether_ExpressionsMenu_Right",
                    "SleepTogether_ExpressionsMenu_Left",
                },
                new List<string>
                {
                    "SleepTogether",
                    "SleepTogether_Right",
                    "SleepTogether_Left",
                }
            ),

            (
                "ごろ寝システム",
                new List<string>
                {
                    "SupineLocomotion",
                    "SupineLocomotion_ex",
                },
                 new List<string>
                {
                    "VRCSupineExAdjust",
                    "VRCSupineExAdjusting",
                    "VRCFootAnchorHandSwitchable",
                    "RootHeight",
                    "SetRootHeight",
                },
                 new List<string>
                {
                    "VRCSupineExAdjust",
                    "VRCSupineExAdjusting",
                    "VRCFootAnchorHandSwitchable",
                    "RootHeight",
                    "SetRootHeight",
                },
                new List<string>
                {
                    "SupineMenu",
                    "SupineMenu_ex",
                },
                new List<string>
                {
                }
            ),
        };

        public static List<string> CheckExistProduct(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor avatar)
        {
            var existProducts = new List<string>();

            foreach (var productDefine in productDefines)
            {
                bool isExistProduct = false;
                // AnimatorControllerが商品のか調べる
                var animatorController = avatar.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base];
                if (animatorController.animatorController != null)
                {
                    foreach (var animatoControllerName in productDefine.animatorControllerNames)
                    {
                        if (animatorController.animatorController.name.Contains(animatoControllerName))
                        {
                            isExistProduct = true;
                        }
                    }
                }

                var productMenuGuids = new List<string>();
                foreach (var menuName in productDefine.expressionsMenuNames)
                {
                    productMenuGuids.AddRange(AssetDatabase.FindAssets(menuName));
                }
                // 再起で商品のメニューを使用しているか調べる
                bool isKawaiiSittingMenu(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu menu)
                {
                    if (menu == null)
                    {
                        return false;
                    }
                    foreach (var menuName in productDefine.expressionsMenuNames)
                    {
                        if (productMenuGuids.IndexOf(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(menu))) != -1)
                        {
                            return true;
                        }

                    }
                    foreach (var control in menu.controls)
                    {
                        if (control.type != VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu)
                        {
                            continue;
                        }
                        if (isKawaiiSittingMenu(control.subMenu))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                if (isKawaiiSittingMenu(avatar.expressionsMenu))
                {
                    isExistProduct = true;
                }

                // パラメータに商品のがあるか調べる
                if (avatar.expressionParameters)
                {
                    foreach (var parameter in avatar.expressionParameters.parameters)
                    {
                        if (productDefine.checkExpressionParametersNames.IndexOf(parameter.name) != -1)
                        {
                            isExistProduct = true;
                        }
                    }
                }
                foreach (var prefabsName in productDefine.prefabsNames)
                {
                    Transform prefabsTransform = avatar.transform.Find(prefabsName);
                    if (prefabsTransform != null)
                    {
                        isExistProduct = true;
                    }
                }

                if (isExistProduct)
                {
                    existProducts.Add(productDefine.name);
                }
            }

            return existProducts;
        }

        void RemoveExistProduct(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor avatar, string productName)
        {
            var productDefine = productDefines.FirstOrDefault(define => define.name == productName);
            if (productDefine == default)
            {
                return;
            }

            // AnimatorControllerが商品のか調べる
            var animatorController = avatar.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base];
            if (animatorController.animatorController != null)
            {
                foreach (var animatoControllerName in productDefine.animatorControllerNames)
                {
                    if (AssetDatabase.FindAssets(animatoControllerName).ToList().IndexOf(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(animatorController.animatorController))) != -1)
                    {
                        avatar.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base].animatorController = null;
                        avatar.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base].isDefault = true;
                        avatar.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base].isEnabled = true;
                        EditorUtility.SetDirty(avatar);
                        break;
                    }
                }
            }

            // 再起で商品のメニューを使用しているか調べる
            void removeKawaiiSittingMenu(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu menu)
            {
                if (menu == null)
                {
                    return;
                }
                for (var i = menu.controls.Count - 1; i >= 0; i--)
                {
                    var control = menu.controls[i];
                    if (control.type != VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu)
                    {
                        continue;
                    }
                    bool isKawaiiSittingMenu = false;
                    foreach (var menuName in productDefine.expressionsMenuNames)
                    {
                        if (AssetDatabase.FindAssets(menuName).ToList().IndexOf(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(control.subMenu))) != -1)
                        {
                            menu.controls.RemoveAt(i);
                            isKawaiiSittingMenu = true;
                            EditorUtility.SetDirty(menu);
                            break;
                        }
                    }
                    if (!isKawaiiSittingMenu)
                    {
                        removeKawaiiSittingMenu(control.subMenu);
                    }
                }
            }
            removeKawaiiSittingMenu(avatar.expressionsMenu);

            // パラメータに商品のがあるか調べる
            avatar.expressionParameters.parameters = avatar.expressionParameters.parameters.Where((parameter) =>
            {
                return productDefine.expressionParametersNames.IndexOf(parameter.name) == -1;
            }).ToArray();
            EditorUtility.SetDirty(avatar.expressionParameters);

            // Prefabsを削除
            foreach (var prefabsName in productDefine.prefabsNames)
            {
                Transform prefabsTransform;
                while ((prefabsTransform = avatar.transform.Find(prefabsName)) != null)
                {
                    GameObject.DestroyImmediate(prefabsTransform.gameObject);
                }
            }

            AssetDatabase.SaveAssets();
        }

        private static readonly List<string> folderDefines = new()
        {
            "Assets/UnisakiStudio/PosingSystem",
        };

        public static List<string> CheckExistFolder()
        {
            List<string> existFolders = new();
            foreach (var checkFunction in checkFunctions)
            {
                existFolders.AddRange(checkFunction());
            }
            foreach (var folderDefine in folderDefines)
            {
                if (AssetDatabase.IsValidFolder(folderDefine))
                {
                    existFolders.Add(folderDefine);
                }
            }
            return existFolders;
        }

        void RemoveExistFolder(string folder)
        {
            AssetDatabase.DeleteAsset(folder);
        }

        public static List<(PosingSystem.OverrideAnimationDefine.AnimationStateType type, string stateName, string defaultMotionName)> detectSettings = new List<(PosingSystem.OverrideAnimationDefine.AnimationStateType stateType, string stateName, string defaultMotionName)>()
        {
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun, "stand", "vrc_StandingLocomotion"),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.Crouch, "crouch", "vrc_CrouchingLocomotion"),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.Prone, "prone", "vrc_ProneLocomotion"),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.Jump, "jump", "proxy_fall_short"),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortFall, "shortfall", "proxy_fall_short"),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortFall, "softfall", "proxy_fall_short"),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortFall, "quickfall", "proxy_fall_short"),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortLanding, "shortland", "proxy_land_quick"),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortLanding, "softland", "proxy_land_quick"),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortLanding, "quickland", "proxy_land_quick"),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.LongFall, "longfall", "proxy_fall_long"),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.LongFall, "hardfall", "proxy_fall_long"),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.LongLanding, "longland", "proxy_landing"),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.LongLanding, "hardland", "proxy_landing"),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.AvatarSelect, "avatarselect", "vrc_StandingLocomotion"),
        };

        void AutoDetectOverrideSettings()
        {
            var posingSystem = target as PosingSystem;
            var avatar = posingSystem.GetAvatar();

            var posingSystems = avatar.GetComponentsInChildren<PosingSystem>().ToList();
            var animatorController = avatar.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base];
            if (animatorController.animatorController == null)
            {
                return;
            }

            void addOverrideFromStateMachine(AnimatorStateMachine stateMachine)
            {
                foreach (var subStateMachine in stateMachine.stateMachines)
                {
                    addOverrideFromStateMachine(subStateMachine.stateMachine);
                }
                foreach (var state in stateMachine.states)
                {
                    var searchName = state.state.name;
                    searchName = searchName.Replace("ing", "").Replace(" ", "").Replace("_", "").ToLower();
                    var detectSettingIndex = detectSettings.FindIndex(setting => setting.stateName == searchName);
                    if (detectSettingIndex == -1)
                    {
                        continue;
                    }
                    var detectSetting = detectSettings[detectSettingIndex];
                    if (posingSystems.FindIndex(ov => ov.overrideDefines.FindIndex(def => def.stateType == detectSetting.type) != -1) != -1)
                    {
                        continue;
                    }

                    if (state.state.motion == null)
                    {
                        continue;
                    }
                    if (state.state.motion.name == detectSetting.defaultMotionName)
                    {
                        continue;
                    }
                    if (!isContainHumanoidAnimation(state.state.motion))
                    {
                        continue;
                    }

                    var define = new PosingSystem.OverrideAnimationDefine();
                    define.stateType = detectSetting.type;
                    define.animationClip = state.state.motion;
                    posingSystem.overrideDefines.Add(define);
                }
            }

            foreach (var layer in ((UnityEditor.Animations.AnimatorController)animatorController.animatorController).layers)
            {
                addOverrideFromStateMachine(layer.stateMachine);
            }
        }

        static bool isContainHumanoidAnimation(Motion motion)
        {
            if (motion == null)
            {
                return false;
            }
            if (motion.GetType() == typeof(AnimationClip))
            {
                if (AnimationUtility.GetCurveBindings((AnimationClip)motion).Any(bind =>
                {
                    return HumanTrait.MuscleName.ToList().IndexOf(bind.propertyName) != -1;
                }))
                {
                    return true;
                }
            }
            if (motion.GetType() == typeof(UnityEditor.Animations.BlendTree))
            {
                foreach (var child in ((UnityEditor.Animations.BlendTree)motion).children)
                {
                    if (isContainHumanoidAnimation(child.motion))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        
        public static void ClearThumbnailPack(PosingSystem posingSystem)
        {
            // 既にあるなら一度すべてのアニメーションのプレビュー画像を削除            
            if (posingSystem.thumbnailPackObject != null)
            {
                var assetRepresentations = AssetDatabase.LoadAllAssetRepresentationsAtPath(AssetDatabase.GetAssetPath(posingSystem.thumbnailPackObject));
                foreach (var assetRepresentation in assetRepresentations)
                {
                    AssetDatabase.RemoveObjectFromAsset(assetRepresentation);
                }
            }
        }

        public static void CreateThumbnailPack(PosingSystem posingSystem)
        {
            CheckAndDeleteThumbnailPack(posingSystem);

            // 既にあるなら作成しない
            if (posingSystem.thumbnailPackObject != null)
            {
                return;
            }

            // 新規作成
            posingSystem.thumbnailPackObject = ScriptableObject.CreateInstance<jp.unisakistudio.posingsystem.PosingSystemThumbnailPack>();
            var generatedFolderPath = GetGeneratedFolderPath();
            var filePath = $"{generatedFolderPath}/{posingSystem.GetAvatar().name}.asset";
            filePath = AssetDatabase.GenerateUniqueAssetPath(filePath);
//            posingSystem.savedInstanceId = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(posingSystem).ToString();
            EditorUtility.SetDirty(posingSystem);
            AssetDatabase.CreateAsset(posingSystem.thumbnailPackObject, filePath);
            AssetDatabase.SaveAssets();
        }

        public static void CheckAndDeleteThumbnailPack(PosingSystem posingSystem)
        {
            if (posingSystem.thumbnailPackObject == null)
            {
                return;
            }

            // アイコン画像作成時のアバターと違うなら、アイコン画像は全部削除する
            var savedInstanceId = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(posingSystem).ToString();
            if (posingSystem.savedInstanceId != "" && savedInstanceId != posingSystem.savedInstanceId)
            {
                posingSystem.savedInstanceId = savedInstanceId;
                posingSystem.thumbnailPackObject = null;

                for (int index = 0; index < posingSystem.defines.Count; index++)
                {
                    var define = posingSystem.defines[index];
                    {
                        for (int animationIndex = 0; animationIndex < define.animations.Count; animationIndex++)
                        {
                            define.animations[animationIndex].previewImage = null;
                        }
                    }
                }
                for (int index = 0; index < posingSystem.overrideDefines.Count; index++)
                {
                    posingSystem.overrideDefines[index].previewImage = null;
                }
            }
        }

        public static string GetGeneratedFolderPath()
        {
            var label = "PosingSystemResourceFolder";
            var folder = AssetDatabase.FindAssets($"l:{label}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(folder))
            {
                folder = DefaultAdjustmentFolderRoot;
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                var folderObject = AssetDatabase.LoadMainAssetAtPath(folder);
                AssetDatabase.SetLabels(folderObject, new string[] { label });
            }
            return folder;
        }
    }

}
