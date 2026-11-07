using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.Animations;
using System.Linq;
using jp.unisakistudio.posingsystem;
using System;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR_WIN
using Microsoft.Win32;

#endif


#if MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

namespace jp.unisakistudio.posingsystemeditor
{

    [InitializeOnLoad]
    [CustomEditor(typeof(PosingSystem))]
    public partial class PosingSystemEditor : Editor
    {
        private const string DefaultAdjustmentFolderRoot = "Assets/UnisakiStudio/GeneratedResources";
        private static readonly string[] BreathingLevelLabels =
        {
            "なし",
            "弱",
            "中",
            "強",
            "急",
            "カスタム",
        };

        private static readonly PosingSystem.BreathingLevel[] BreathingLevels =
        {
            PosingSystem.BreathingLevel.None,
            PosingSystem.BreathingLevel.Weak,
            PosingSystem.BreathingLevel.Medium,
            PosingSystem.BreathingLevel.Strong,
            PosingSystem.BreathingLevel.Rapid,
            PosingSystem.BreathingLevel.Custom,
        };
        private List<ReorderableList> reorderableLists = new List<ReorderableList>();
        List<string> existProducts;
        List<string> existFolders;
        private readonly PosingSystemInspectorHeader inspectorHeader = new PosingSystemInspectorHeader();
        private readonly PosingSystemPrebuildInspector prebuildInspector = new PosingSystemPrebuildInspector();

        private readonly PosingSystemPresetBrowser presetBrowser = new PosingSystemPresetBrowser();

        private enum InspectorTab
        {
            Settings,
            PoseAnimations,
            AvatarSpecific,
        }

        private static readonly string[] InspectorTabLabels =
        {
            "機能",
            "ポーズ一覧",
            "アバター調整",
        };

        private InspectorTab _selectedTab = InspectorTab.Settings;

        public delegate List<string> CheckFunction();
        protected static List<CheckFunction> checkFunctions = new List<CheckFunction>();

        const string REGKEY = @"SOFTWARE\UnisakiStudio";
        const string APPKEY = "posingsystem";
        private bool isPosingSystemLicensed = false;
        private static Transform previewAvatarRoot;

        static PosingSystemEditor()
        {
            AssemblyReloadEvents.beforeAssemblyReload += CleanupAllPreviewAvatars;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneSaving += (scene, _) => LegacyRolloverPoseMigration.ApplyToScene(scene);
            PrefabUtility.prefabInstanceUpdated += LegacyRolloverPoseMigration.ApplyToHierarchy;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

            // アップデート前のバージョンがシーンへ保存したPreviewAvatarRootも、
            // スクリプト再読込後に確実に回収する。
            EditorApplication.delayCall += CleanupAllPreviewAvatars;
            EditorApplication.delayCall += LegacyRolloverPoseMigration.ApplyToOpenScenes;
            EditorApplication.delayCall += PosingSystemAvatarAdjustmentInspector.ScheduleAllInOpenScenes;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            LegacyRolloverPoseMigration.ApplyToScene(scene);
            CleanupAllPreviewAvatars();
            EditorApplication.delayCall += PosingSystemAvatarAdjustmentInspector.ScheduleAllInOpenScenes;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode || state == PlayModeStateChange.EnteredEditMode)
            {
                CleanupAllPreviewAvatars();
            }
        }

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
            Undo.undoRedoPerformed += OnFeatureUndoRedo;
            existFolders = null;
            //            AnimationMode.StartAnimationMode();
            //            AnimationMode.BeginSampling();

            PosingSystem posingSystem = target as PosingSystem;
            posingSystem.previousErrorCheckTime = DateTime.MinValue;
            PosingSystemAvatarAdjustmentInspector.ScheduleAutomaticGeneration(posingSystem);

            // プリセット一覧はUIを開いたときに共有ファイルから取得する。選択時には全件検索しない。

            // 使われていないプレビュー用アバターを削除
            DeleteUnusedPreviewAvatar();
        }

        private void DeleteUnusedPreviewAvatar()
        {
            // 非アクティブなPosingSystemも含め、参照されていない一時プレビューを回収する。
            var usingPreviewAvatars = Resources.FindObjectsOfTypeAll<PosingSystem>()
                .Where(posingSystem => posingSystem != null && posingSystem.gameObject.scene.IsValid())
                .Select(posingSystem => posingSystem.previewAvatarObject)
                .Where(preview => preview != null)
                .ToHashSet();
            var deletePreviewAvatars = new List<GameObject>();
            var root = FindPreviewAvatarRoot();
            if (root == null)
            {
                return;
            }
            foreach (Transform previewAvatarTransform in root)
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
        }

        public static Transform GetPreviewAvatarRoot()
        {
            if (previewAvatarRoot == null)
            {
                previewAvatarRoot = FindPreviewAvatarRoot();
            }
            if (previewAvatarRoot == null)
            {
                var rootObject = new GameObject("PreviewAvatarRoot");
                rootObject.hideFlags = HideFlags.HideAndDontSave;
                previewAvatarRoot = rootObject.transform;
            }
            return previewAvatarRoot;
        }

        private static Transform FindPreviewAvatarRoot()
        {
            return Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(gameObject => gameObject != null && !EditorUtility.IsPersistent(gameObject))
                .Where(gameObject => gameObject.name == "PreviewAvatarRoot")
                .Select(gameObject => gameObject.transform)
                .FirstOrDefault();
        }

        public static void SetPreviewAvatarVisibility(GameObject previewAvatar, bool visible)
        {
            if (previewAvatar == null)
            {
                return;
            }

            var flags = visible ? HideFlags.DontSave : HideFlags.HideAndDontSave;
            foreach (var child in previewAvatar.GetComponentsInChildren<Transform>(true))
            {
                child.gameObject.hideFlags = flags;
            }
            previewAvatar.SetActive(visible);
        }

        public static void CleanupAllPreviewAvatars()
        {
            foreach (var posingSystem in Resources.FindObjectsOfTypeAll<PosingSystem>()
                         .Where(posingSystem => posingSystem != null && posingSystem.gameObject.scene.IsValid()))
            {
                posingSystem.previewAvatarObject = null;
            }

            var roots = Resources.FindObjectsOfTypeAll<GameObject>()
                .Where(gameObject => gameObject != null && !EditorUtility.IsPersistent(gameObject))
                .Where(gameObject => gameObject.name == "PreviewAvatarRoot")
                .ToArray();
            foreach (var root in roots)
            {
                // 旧バージョンのPreviewAvatarRootはシーン保存対象だったため、削除をシーン変更として記録する。
                if ((root.hideFlags & HideFlags.DontSaveInEditor) == 0 && root.scene.IsValid())
                {
                    EditorSceneManager.MarkSceneDirty(root.scene);
                }
                UnityEngine.Object.DestroyImmediate(root);
            }
            previewAvatarRoot = null;
        }

        internal static bool IsAndroidBuildTarget()
        {
            return UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android;
        }

        private void OnDisable()
        {
            PosingSystemHierarchyBadge.SetInspectorWarning(GetInstanceID(), null);
            if (tabBackground != null) DestroyImmediate(tabBackground);
            Undo.undoRedoPerformed -= OnFeatureUndoRedo;
            //            AnimationMode.EndSampling();
            //            AnimationMode.StopAnimationMode();
        }

        private void OnFeatureUndoRedo()
        {
            // Generated menu objects/parameters are recorded in the same Undo group as settings.
            // Rebuilding here would mutate the just-restored hierarchy and can destroy Redo state.
            PosingSystemEditorChangeUtility.EndTextEditing();
            serializedObject.Update();
            if (target is PosingSystem ps) ps.previousErrorCheckTime = DateTime.MinValue;
            Repaint();
        }

        public override void OnInspectorGUI()
        {
            drewInspectorWarning = false;
            using (PosingSystemLocalization.ForInspector(target as PosingSystem))
                DrawPosingInspector();
            if (Event.current.type == EventType.Repaint)
                PosingSystemHierarchyBadge.SetInspectorWarning(GetInstanceID(), drewInspectorWarning ? target as PosingSystem : null);
        }

        private void DrawPosingInspector()
        {
            PosingSystem posingSystem = target as PosingSystem;
            inspectorHeader.Draw(posingSystem);

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
                    EditorGUILayout.LabelField(PosingSystemLocalization.Text("ゆにさきポーズシステム"), header1Label, GUILayout.Height(30));

                    EditorGUILayout.HelpBox(PosingSystemLocalization.Text("このコンピュータにはゆにさきポーズシステムの使用が許諾されていません。Boothのショップから可愛いポーズツールを購入して、コンピュータにライセンスをインストールしてください"), MessageType.Error);
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

            // 調整ウィンドウが表示されている時は何もできないようにして、ウィンドウを閉じるボタンを用意する
            if (Resources.FindObjectsOfTypeAll<PosingAnimationAdjustmentWindow>().Where(window => window.IsOpen).Count() > 0)
            {
                EditorGUILayout.HelpBox(PosingSystemLocalization.Text("アニメーション調整ウィンドウが表示されています。調整が終わったらウィンドウを閉じてください"), MessageType.Info);
                if (GUILayout.Button(PosingSystemLocalization.Text("アニメーション調整ウィンドウを閉じる")))
                {
                    foreach (var window in Resources.FindObjectsOfTypeAll<PosingAnimationAdjustmentWindow>().Where(window => window.IsOpen))
                    {
                        window.StopAdjustment();
                    }
                }
                else
                {
                    return;
                }
            }


            DrawUserInspector(posingSystem);
        }

        private bool ValidateAvatar(PosingSystem posingSystem)
        {
            var avatar = posingSystem.GetAvatar();
            if (avatar == null)
            {
                if (_selectedTab == InspectorTab.Settings)
                {
                EditorGUILayout.HelpBox(PosingSystemLocalization.Text("オブジェクトがVRC用のアバターオブジェクトの中に入っていません。このオブジェクトはVRCAvatarDescriptorコンポーネントの付いたオブジェクトの中に配置してください"), MessageType.Error);
                }
                return false;
            }
            if (_selectedTab == InspectorTab.Settings)
            {
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
                    EditorGUILayout.HelpBox(existProduct + "の設定がアバターに残っています！ 不具合が発生する可能性があるので、自動で設定を取り除く機能を使用してください。また、アバター購入時にBaseLayerに設定されていたLocomotion用のAnimatorControllerがある場合は、恐れ入りますが手動で復元してお使いください。", MessageType.Error);
                    if (GUILayout.Button(existProduct + "の設定を取り除く") &&
                        PosingSystemLocalization.Dialog(PosingSystemLocalization.Text("旧版のアバター設定を取り除きます"), existProduct + "の旧設定を取り除きます。ツールを通常アンインストールするための操作ではありません。調整内容を確認できるよう、先にプロジェクトをバックアップしてください。", "バックアップ済み・取り除く", "キャンセル"))
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
                    EditorGUILayout.HelpBox("旧unitypackage版の「" + existFolder + "」フォルダがプロジェクトに残っています。新版と競合する可能性があります。削除する前に移行手順とバックアップを確認してください。", MessageType.Error);
                    if (GUILayout.Button(existFolder + "のフォルダを取り除く") &&
                        PosingSystemLocalization.Dialog(PosingSystemLocalization.Text("旧版フォルダの削除を確認"), "削除対象: " + existFolder + "\nこの中のPrefabやアニメーションも削除されます。自作・調整済みデータがある場合はキャンセルし、移行手順に従ってください。バックアップなしでは元に戻せません。", "バックアップ済み・削除する", "キャンセル"))
                    {
                        RemoveExistFolder(existFolder);
                        existFolders = null;
                        posingSystem.previousErrorCheckTime = DateTime.MinValue;
                    }
                }
            }

            if (avatar.autoFootsteps && troubleshootingOpen && !posingSystem.IsWarningIgnored(PosingSystem.WarningType.AutoFootsteps))
            {
                EditorGUILayout.HelpBox(PosingSystemLocalization.Text("Auto-Footstepsはオンです。ポーズ中に意図せず足が動く場合は、下のボタンでオフにして比較できます。問題がなければ変更は不要です。"), MessageType.Info);
                if (GUILayout.Button(PosingSystemLocalization.Text("「Use Auto-Footsteps for 3 and 4 point tracking」をオフにする")))
                {
                    Undo.RecordObject(avatar, "Disable auto footsteps");
                    avatar.autoFootsteps = false;
                    EditorUtility.SetDirty(avatar);
                    posingSystem.previousErrorCheckTime = DateTime.MinValue;
                }
            }

            }
            var avatarAnimator = avatar.GetComponent<Animator>();
            if (avatarAnimator == null)
            {
                if (_selectedTab == InspectorTab.Settings)
                {
                EditorGUILayout.HelpBox(PosingSystemLocalization.Text("アバターのルートにAnimatorがありません。Inspectorの「設定の確認」の対処手順をご確認ください。"), MessageType.Error);
                }
                return false;
            }

            if (avatarAnimator.avatar == null)
            {
                if (_selectedTab == InspectorTab.Settings)
                {
                EditorGUILayout.HelpBox(PosingSystemLocalization.Text("AnimatorのAvatar欄が未設定です。Inspectorの「設定の確認」の対処手順をご確認ください。"), MessageType.Error);
                }
                return false;
            }

            if (!avatarAnimator.isHuman)
            {
                if (_selectedTab == InspectorTab.Settings)
                {
                EditorGUILayout.HelpBox(PosingSystemLocalization.Text("このアバターは人型ではありません。アバターのAnimatorにHumanoid型のAvatarが設定されていない場合はこのツールは使えません。"), MessageType.Error);
                }
                return false;
            }


            return true;
        }

        public static void Prebuild(PosingSystem posingSystem)
        {
            LegacyRolloverPoseMigration.Apply(posingSystem);
            PosingSystemDiagnostics.ThrowIfInvalid(posingSystem);
            // ビルド時のクローンでもプレビルド済みデータを同一と判定できるよう、
            // 元のPosingSystemを示す安定したIDをデータ生成より先に保存する。
            posingSystem.savedInstanceId = UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(posingSystem).ToString();
            PosingSystemConverter.ConvertToModularAvatarComponents(posingSystem);
            PosingSystemEditorChangeUtility.MarkChanged(posingSystem);
        }

        internal static void SetWarningIgnored(PosingSystem posingSystem, PosingSystem.WarningType warning, bool ignored)
        {
            var value = ignored ? posingSystem.ignoredWarnings | warning : posingSystem.ignoredWarnings & ~warning;
            if (value == posingSystem.ignoredWarnings) return;
            Undo.RecordObject(posingSystem, "警告の表示を変更");
            posingSystem.ignoredWarnings = value;
            PosingSystemEditorChangeUtility.MarkChanged(posingSystem);
        }

        // UI entry point: do not continue into image generation or display success after a failed conversion.
        public static bool TryPrebuildAndRenew(PosingSystem posingSystem)
            => TryBuildOrRenew(posingSystem, true);

        public static bool TryRenewAvatar(PosingSystem posingSystem)
            => TryBuildOrRenew(posingSystem, false);

        private static bool TryBuildOrRenew(PosingSystem posingSystem, bool prebuild)
        {
            var stage = prebuild ? "プレビルド" : "アイコンの更新";
            PosingSystemDiagnosticsInspector.SetFailure(posingSystem, null);
            try
            {
                if (prebuild) Prebuild(posingSystem);
                else PosingSystemDiagnostics.ThrowIfInvalid(posingSystem);
                stage = "アイコンの更新";
                RenewAvatar(posingSystem);
                return true;
            }
            catch (ExitGUIException) { throw; }
            catch (Exception ex)
            {
                var message = stage + "を中止しました。\n" + ex.Message;
                if (!(ex is PosingSystemDiagnostics.DiagnosticException))
                    message += "\n対処: Window > General > Consoleで、この操作中に最初に出た赤いエラーを確認してください。設定の確認で問題が解消しない場合は、このエラー番号・詳細とツールのバージョンをお問い合わせに添えてください。";
                PosingSystemDiagnosticsInspector.SetFailure(posingSystem, message);
                Debug.LogError("[PosingSystem] " + message, posingSystem);
                Debug.LogException(ex, posingSystem);
                if (PosingSystemLocalization.Dialog(stage + "を完了できませんでした", message, "公式ヘルプを開く", "閉じる"))
                    Application.OpenURL(PosingSystemPackageInfo.SupportUrl);
                return false;
            }
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

        private static void DrawAdditiveBreathingLevel(
            SerializedProperty breathingProperty,
            KeyValuePair<PosingSystem.LocomotionTypeGroup, string> displayName)
        {
            var levelPropertyName = GetAdditiveBreathingLevelPropertyName(displayName.Key);
            var levelProperty = breathingProperty.FindPropertyRelative(levelPropertyName);
            if (levelProperty == null)
            {
                return;
            }

            var currentLevel = (PosingSystem.BreathingLevel)levelProperty.intValue;
            var selectedIndex = Array.IndexOf(BreathingLevels, currentLevel);
            if (selectedIndex < 0)
            {
                selectedIndex = Array.IndexOf(
                    BreathingLevels,
                    PosingSystem.BreathingLevel.Medium);
            }

            selectedIndex = EditorGUILayout.Popup(
                new GUIContent(displayName.Value),
                selectedIndex,
                BreathingLevelLabels);
            levelProperty.intValue = (int)BreathingLevels[selectedIndex];

            if (BreathingLevels[selectedIndex] != PosingSystem.BreathingLevel.Custom)
            {
                return;
            }

            var clipPropertyName = GetAdditiveBreathingPropertyName(displayName.Key);
            var clipProperty = breathingProperty.FindPropertyRelative(clipPropertyName);
            if (clipProperty == null)
            {
                return;
            }

            using (new EditorGUI.IndentLevelScope())
            {
                EditorGUILayout.PropertyField(
                    clipProperty,
                    new GUIContent(PosingSystemLocalization.Text("カスタムアニメーション")));
            }
        }

        public static List<KeyValuePair<PosingSystem.LocomotionTypeGroup, string>>
            GetAdditiveBreathingDisplayNames(PosingSystem posingSystem)
        {
            var result = new List<KeyValuePair<PosingSystem.LocomotionTypeGroup, string>>();
            if (posingSystem?.defines == null)
            {
                return result;
            }

            var namesByGroup = new Dictionary<PosingSystem.LocomotionTypeGroup, List<string>>();
            foreach (var define in posingSystem.defines)
            {
                if (define == null ||
                    define.locomotionTypeValue < 0 ||
                    string.IsNullOrWhiteSpace(define.menuName))
                {
                    continue;
                }

                var groupValue = define.locomotionTypeValue / 10;
                if (!Enum.IsDefined(typeof(PosingSystem.LocomotionTypeGroup), groupValue))
                {
                    continue;
                }

                var group = (PosingSystem.LocomotionTypeGroup)groupValue;
                if (!namesByGroup.TryGetValue(group, out var names))
                {
                    names = new List<string>();
                    namesByGroup.Add(group, names);
                    result.Add(new KeyValuePair<PosingSystem.LocomotionTypeGroup, string>(group, null));
                }

                if (!names.Contains(define.menuName))
                {
                    names.Add(define.menuName);
                }
            }

            for (var index = 0; index < result.Count; index++)
            {
                var group = result[index].Key;
                var names = namesByGroup[group];
                var label = string.Join("／", names);
                result[index] = new KeyValuePair<PosingSystem.LocomotionTypeGroup, string>(group, label);
            }

            return result;
        }

        private static string GetAdditiveBreathingLevelPropertyName(
            PosingSystem.LocomotionTypeGroup group)
        {
            switch (group)
            {
                case PosingSystem.LocomotionTypeGroup.Standing:
                    return "standingLevel";
                case PosingSystem.LocomotionTypeGroup.SitShallow:
                    return "sitShallowLevel";
                case PosingSystem.LocomotionTypeGroup.SitDeep:
                    return "sitDeepLevel";
                case PosingSystem.LocomotionTypeGroup.SitSleep:
                    return "sitSleepLevel";
                case PosingSystem.LocomotionTypeGroup.VirtualLove:
                    return "virtualLoveLevel";
                case PosingSystem.LocomotionTypeGroup.TypeFix:
                    return "typeFixLevel";
                default:
                    throw new ArgumentOutOfRangeException(nameof(group), group, null);
            }
        }

        private static string GetAdditiveBreathingPropertyName(PosingSystem.LocomotionTypeGroup group)
        {
            switch (group)
            {
                case PosingSystem.LocomotionTypeGroup.Standing:
                    return "standing";
                case PosingSystem.LocomotionTypeGroup.SitShallow:
                    return "sitShallow";
                case PosingSystem.LocomotionTypeGroup.SitDeep:
                    return "sitDeep";
                case PosingSystem.LocomotionTypeGroup.SitSleep:
                    return "sitSleep";
                case PosingSystem.LocomotionTypeGroup.VirtualLove:
                    return "virtualLove";
                case PosingSystem.LocomotionTypeGroup.TypeFix:
                    return "typeFix";
                default:
                    throw new ArgumentOutOfRangeException(nameof(group), group, null);
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
            if (avatar == null) return existProducts;
            var baseController = avatar.baseAnimationLayers?.FirstOrDefault(layer =>
                layer.type == VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base).animatorController;

            foreach (var productDefine in productDefines)
            {
                bool isExistProduct = false;
                // AnimatorControllerが商品のか調べる
                if (baseController != null)
                {
                    foreach (var animatoControllerName in productDefine.animatorControllerNames)
                    {
                        if (baseController.name.Contains(animatoControllerName))
                        {
                            isExistProduct = true;
                        }
                    }
                }

                var productMenuGuids = new HashSet<string>();
                foreach (var menuName in productDefine.expressionsMenuNames)
                {
                    productMenuGuids.UnionWith(PosingSystemProductMenuCatalog.GetMatchingGuids(menuName));
                }

                // 再帰で商品のメニューを使用しているか調べる（循環防止のため訪問済みセットを利用）
                var visitedMenus = new HashSet<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
                bool isKawaiiSittingMenu(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu menu)
                {
                    if (menu == null)
                    {
                        return false;
                    }
                    if (!visitedMenus.Add(menu))
                    {
                        return false;
                    }
                    var menuGuid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(menu));
                    if (productMenuGuids.Contains(menuGuid))
                    {
                        return true;
                    }
                    if (menu.controls == null) return false;
                    foreach (var control in menu.controls)
                    {
                        if (control == null || control.type != VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu)
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
                if (avatar.expressionParameters != null && avatar.expressionParameters.parameters != null)
                {
                    foreach (var parameter in avatar.expressionParameters.parameters)
                    {
                        if (parameter != null && productDefine.checkExpressionParametersNames.IndexOf(parameter.name) != -1)
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

            // 再起で商品のメニューを使用しているか調べる（循環防止のため訪問済みセットを利用）
            var visitedMenus = new HashSet<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu>();
            void removeKawaiiSittingMenu(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu menu)
            {
                if (menu == null)
                {
                    return;
                }
                if (!visitedMenus.Add(menu))
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
            if (animatorController.animatorController is not UnityEditor.Animations.AnimatorController)
            {
                PosingSystemLocalization.Dialog(PosingSystemLocalization.Text("元姿勢の読み込み"), "アバターのBaseに通常のAnimatorControllerを設定してください。AnimatorOverrideControllerには対応していません。", "OK");
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
                    if (!PosingSystemConverter.IsValidOverrideMotion(
                            detectSetting.type,
                            state.state.motion))
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
            var avatarFileName = SanitizeAssetFileName(posingSystem.GetAvatar().name, "PosingSystem");
            var filePath = $"{generatedFolderPath}/{avatarFileName}.asset";
            filePath = AssetDatabase.GenerateUniqueAssetPath(filePath);
            if (string.IsNullOrEmpty(filePath))
            {
                posingSystem.thumbnailPackObject = null;
                Debug.LogError($"可愛いポーズツール: サムネイル保存先のパスを生成できませんでした。フォルダ「{generatedFolderPath}」がプロジェクトに認識されているか確認してください。");
                return;
            }
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
            // ラベル付きフォルダがエクスプローラ等で削除・移動されていると古いパスが返るため、実在するフォルダのみ採用する
            var folder = AssetDatabase.FindAssets($"l:{label}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault(path => path.StartsWith("Assets/", System.StringComparison.Ordinal) && AssetDatabase.IsValidFolder(path));
            if (string.IsNullOrEmpty(folder))
            {
                folder = DefaultAdjustmentFolderRoot;
                EnsureAssetFolderExists(folder);
                var folderObject = AssetDatabase.LoadMainAssetAtPath(folder);
                if (folderObject != null)
                {
                    AssetDatabase.SetLabels(folderObject, new string[] { label });
                }
            }
            return folder;
        }

        // AssetDatabase.CreateFolderで作成することで、AssetDatabaseに未認識のフォルダに対して
        // GenerateUniqueAssetPathが空文字を返す問題を防ぐ
        public static void EnsureAssetFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var separatorIndex = folderPath.LastIndexOf('/');
            if (separatorIndex < 0)
            {
                return;
            }

            var parent = folderPath.Substring(0, separatorIndex);
            var child = folderPath.Substring(separatorIndex + 1);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureAssetFolderExists(parent);
            }
            AssetDatabase.CreateFolder(parent, child);
        }

        // アセットパスに使用できない文字を含む名前(アバター名等)をファイル名として安全な形に変換する
        public static string SanitizeAssetFileName(string value, string fallback)
        {
            if (string.IsNullOrEmpty(value))
            {
                return fallback;
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(value.Select(c => invalidChars.Contains(c) || c == '/' || c == '\\' ? '_' : c).ToArray()).Trim();
            if (string.IsNullOrEmpty(sanitized) || sanitized.All(c => c == '.'))
            {
                return fallback;
            }
            return sanitized;
        }
    }

}
