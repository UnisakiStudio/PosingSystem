#region

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using jp.unisakistudio.posingsystem;
#if NDMF
using nadena.dev.ndmf;

#endif
#if MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

#endregion

#if NDMF && MODULAR_AVATAR

[assembly: ExportsPlugin(typeof(jp.unisakistudio.posingsystemeditor.PosingSystemConverter))]

namespace jp.unisakistudio.posingsystemeditor
{
    public class PosingSystemConverter : Plugin<PosingSystemConverter>
    {
        // NDMFの自パス実行中かの簡易フラグ（OnPreprocessAvatarの再帰呼び出しを避けるため）
        private static bool _isExecutingNdMfPass = false;
        [System.Serializable]
        private class DefineSerializeRoot
        {
            public System.Collections.Generic.List<PosingSystem.LayerDefine> list;
            public System.Collections.Generic.List<PosingSystem.OverrideAnimationDefine> overrides;
            public string version;
        }

        // DTOs for sanitized JSON (exclude previewImage fields entirely)
        [System.Serializable]
        private class BaseAnimationDefineDTO
        {
            public bool enabled;
            public bool isRotate;
            public int rotate;
            public bool isMotionTime;
            public string motionTimeParamName;
            public string animationClipGuid; // instanceIDの代わりにGUIDを使用
            public string adjustmentClipGuid;
            public string animationClipHash; // アニメーションの内容ハッシュ
            public string adjustmentClipHash; // 調整クリップの内容ハッシュ
        }

        [System.Serializable]
        private class AnimationDefineDTO : BaseAnimationDefineDTO
        {
            public string displayName;
            public bool initial;
            public bool initialSet;
            public bool isCustomIcon;
            public string iconGuid; // instanceIDの代わりにGUIDを使用
            public int typeParameterValue;
            public int syncdParameterValue;
        }

        [System.Serializable]
        private class OverrideAnimationDefineDTO : BaseAnimationDefineDTO
        {
            public PosingSystem.OverrideAnimationDefine.AnimationStateType stateType;
        }

        [System.Serializable]
        private class LayerDefineDTO
        {
            public string menuName;
            public string description;
            public string stateMachineName;
            public string paramName;
            public string iconGuid; // instanceIDの代わりにGUIDを使用
            public int locomotionTypeValue;
            public System.Collections.Generic.List<AnimationDefineDTO> animations;
        }

        [System.Serializable]
        private class DefineSerializeRootDTO
        {
            public System.Collections.Generic.List<LayerDefineDTO> list;
            public System.Collections.Generic.List<OverrideAnimationDefineDTO> overrides;
            public string version;
            public string savedInstanceId;
        }
        /// <summary>
        /// This name is used to identify the plugin internally, and can be used to declare BeforePlugin/AfterPlugin
        /// dependencies. If not set, the full type name will be used.
        /// </summary>
        public override string QualifiedName => "jp.unisakistudio.posingsystemeditor.posingsystemconverter";

        static string _versionName = null;
        public static string VersionName
        {
            get
            {
                if (_versionName == null)
                {
                    var request = UnityEditor.PackageManager.Client.List(true, true);
                    while (!request.IsCompleted) { }
                    if (request.Status == UnityEditor.PackageManager.StatusCode.Success)
                    {
                        _versionName = request.Result.FirstOrDefault(pkg => pkg.name == "jp.unisakistudio.posingsystem").version;
                    }
                }
                return _versionName != null ? _versionName : "";
            }
        }

        /// <summary>
        /// The plugin name shown in debug UIs. If not set, the qualified name will be shown.
        /// </summary>
        public override string DisplayName
        {
            get
            {
                return "ゆにさきポーズシステム" + VersionName;
            }
        }

        private static bool IsAndroidBuildTarget()
        {
            return UnityEditor.EditorUserBuildSettings.activeBuildTarget == UnityEditor.BuildTarget.Android;
        }

        delegate float ValueChanger(Keyframe value, int i);

        nadena.dev.ndmf.localization.Localizer errorLocalizer;

        protected override void Configure()
        {
            errorLocalizer = LocalizationAsset.ErrorLocalization();

            // AnimatorControllerのレイヤーを編集
            InPhase(BuildPhase.Resolving)
                .AfterPlugin("nadena.dev.modular-avatar")
                .BeforePlugin("jp.unisakistudio.posingsytemeditor.duplicate-eraser")
                .Run("Convert posing system animator controller", ctx =>
                {
                    if (ctx == null || ctx.AvatarRootObject == null)
                    {
                        return;
                    }

                    foreach (var posingSystem in ctx.AvatarRootObject.GetComponentsInChildren<PosingSystem>())
                    {
                        if (posingSystem.tag == "EditorOnly")
                        {
                            continue;
                        }
                        if (posingSystem.data != null && posingSystem.data != GetDefineSerializeJson(posingSystem, true))
                        {
                            ErrorReport.ReportError(errorLocalizer, ErrorSeverity.NonFatal, "オブジェクトの設定が更新されています。再度プレビルドを行ってください", posingSystem.name);
                        }

                        if (posingSystem.data == null || posingSystem.data.Length == 0 || posingSystem.data != GetDefineSerializeJson(posingSystem, true))
                        {
                            ConvertToModularAvatarComponents(posingSystem);
                        }
                    }
                });

            InPhase(BuildPhase.Optimizing)
                .BeforePlugin("com.anatawa12.avatar-optimizer")
                .AfterPlugin("nadena.dev.modular-avatar")
                .Run("Create posing system menu", ctx =>
                {
                    try
                    {
                        _isExecutingNdMfPass = true;

                        if (ctx == null || ctx.AvatarRootObject == null)
                        {
                            return;
                        }

                        if (IsAndroidBuildTarget())
                        {
                            // Androidビルドターゲットでは、アイコン撮影・メニューアイコン設定は行わない
                            return;
                        }

                        foreach (var posingSystem in ctx.AvatarRootObject.GetComponentsInChildren<PosingSystem>())
                        {
                            if (posingSystem.thumbnailPackObject == null)
                            {
                                TakeScreenshot(posingSystem, true, true);
                                SetMenuIcon(posingSystem);
                            }
                        }
                        foreach (var posingSystem in ctx.AvatarRootObject.GetComponentsInChildren<PosingSystem>())
                        {
                            GameObject.DestroyImmediate(posingSystem);
                        }
                    }
                    finally
                    {
                        _isExecutingNdMfPass = false;
                    }
                });
        }

        public static bool HasWarning(PosingSystem posingSystem)
        {
            var avatar = posingSystem.GetAvatar();
            if (IsPosingSystemDataUpdated(posingSystem))
            {
                return true;
            }
            if (avatar.autoFootsteps)
            {
                return true;
            }
            return false;
        }

        public static bool HasError(PosingSystem posingSystem)
        {
            var avatar = posingSystem.GetAvatar();
            if (avatar == null)
            {
                return true;
            }
            if (IsContainExistFolder(posingSystem))
            {
                return true;
            }
            if (IsContainExistProduct(posingSystem))
            {
                return true;
            }
            if (posingSystem.GetAvatar().GetComponent<Animator>().avatar == null)
            {
                return true;
            }
            if (!posingSystem.GetAvatar().GetComponent<Animator>().isHuman)
            {
                return true;
            }
            return false;
        }

        public static bool IsPosingSystemDataUpdated(PosingSystem posingSystem)
        {
            return posingSystem.data != GetDefineSerializeJson(posingSystem);
        }

        public static bool IsContainExistProduct(PosingSystem posingSystem)
        {
            var existProducts = PosingSystemEditor.CheckExistProduct(posingSystem.GetAvatar());
            return existProducts.Count > 0;
        }

        public static bool IsContainExistFolder(PosingSystem posingSystem)
        {
            var existFolders = PosingSystemEditor.CheckExistFolder();
            return existFolders.Count > 0;
        }

        public static List<(string name, ParameterSyncType type, AnimatorControllerParameterType aType, int value, bool sync)> SplitSyncParameters(int syncedParamValue, int maxSyncedParamValue)
        {
            var syncParameters = new List<(string name, ParameterSyncType type, AnimatorControllerParameterType aType, int value, bool sync)>();

            syncParameters.Add(("USSPS_Pose", ParameterSyncType.Int, AnimatorControllerParameterType.Int, syncedParamValue % 256, true));

            for (int i = 0; i < 8; i++)
            {
                bool sync = maxSyncedParamValue >> 8 >> i > 0;
                syncParameters.Add(("USSPS_Pose" + i.ToString(), ParameterSyncType.Bool, AnimatorControllerParameterType.Bool, (syncedParamValue >> 8 >> i) % 2, sync));
            }

            return syncParameters;
        }

        public static AnimatorCondition[] GetEntryTransitionConditions(List<(string name, ParameterSyncType type, AnimatorControllerParameterType aType, int value, bool sync)> parameters, bool vrmode)
        {
            var conditions = new List<AnimatorCondition>();
            conditions.Add(new AnimatorCondition()
            {
                mode = vrmode ? AnimatorConditionMode.NotEqual : AnimatorConditionMode.Equals,
                threshold = 0,
                parameter = "VRMode",
            });
            foreach (var parameter in parameters)
            {
                if (parameter.type == ParameterSyncType.Bool)
                {
                    conditions.Add(new AnimatorCondition()
                    {
                        mode = parameter.value != 0 ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot,
                        threshold = parameter.value,
                        parameter = parameter.name,
                    });
                }
                else if (parameter.type == ParameterSyncType.Int)
                {
                    conditions.Add(new AnimatorCondition()
                    {
                        mode = AnimatorConditionMode.Equals,
                        threshold = parameter.value,
                        parameter = parameter.name,
                    });
                }
                else
                {
                    conditions.Add(new AnimatorCondition()
                    {
                        mode = AnimatorConditionMode.Greater,
                        threshold = parameter.value - 0.01f,
                        parameter = parameter.name,
                    });
                    conditions.Add(new AnimatorCondition()
                    {
                        mode = AnimatorConditionMode.Less,
                        threshold = parameter.value + 0.01f,
                        parameter = parameter.name,
                    });
                }
            }
            return conditions.ToArray();
        }
        public static void CreateExitTransitions(List<(string name, ParameterSyncType type, AnimatorControllerParameterType aType, int value, bool sync)> parameters, AnimatorState state, bool vrmode)
        {
            foreach (var parameter in parameters)
            {
                if (parameter.type == ParameterSyncType.Bool)
                {
                    var exitTransition = state.AddExitTransition(false);
                    exitTransition.AddCondition(parameter.value != 0 ? AnimatorConditionMode.IfNot : AnimatorConditionMode.If, parameter.value, parameter.name);
                    exitTransition.duration = vrmode ? 0.5f : 0.0f;
                    exitTransition.interruptionSource = TransitionInterruptionSource.Destination;
                }
                else if (parameter.type == ParameterSyncType.Int)
                {
                    var exitTransition = state.AddExitTransition(false);
                    exitTransition.AddCondition(AnimatorConditionMode.NotEqual, parameter.value, parameter.name);
                    exitTransition.duration = vrmode ? 0.5f : 0.0f;
                    exitTransition.interruptionSource = TransitionInterruptionSource.Destination;
                }
                else
                {
                    var exitTransition1 = state.AddExitTransition(false);
                    exitTransition1.AddCondition(AnimatorConditionMode.Less, parameter.value - 0.01f, parameter.name);
                    exitTransition1.duration = vrmode ? 0.5f : 0.0f;
                    exitTransition1.interruptionSource = TransitionInterruptionSource.Destination;

                    var exitTransition2 = state.AddExitTransition(false);
                    exitTransition2.AddCondition(AnimatorConditionMode.Greater, parameter.value + 0.01f, parameter.name);
                    exitTransition2.duration = vrmode ? 0.5f : 0.0f;
                    exitTransition2.interruptionSource = TransitionInterruptionSource.Destination;
                }
            }
        }

        public static bool CompareTransitionConditions(AnimatorCondition[] conditions1, AnimatorCondition[] conditions2)
        {
            if (conditions1.Length != conditions2.Length)
            {
                return false;
            }
            for (int i = 0; i < conditions1.Length; i++)
            {
                if (conditions1[i].mode != conditions2[i].mode
                    || conditions1[i].threshold != conditions2[i].threshold
                    || conditions1[i].parameter != conditions2[i].parameter)
                {
                    return false;
                }
            }
            return true;

        }

        public static ModularAvatarMergeAnimator GetCommonMergeAnimator(PosingSystem posingSystem)
        {
            var avatar = posingSystem.GetAvatar();
            if (avatar == null)
            {
                return null;
            }
            var animatorDuplicateErase = posingSystem.GetComponentsInChildren<DuplicateEraser>()
                .Where(d => d.gameObject.tag != "EditorOnly")
                .Where(d => d.ID == "jp.unisakistudio.posingsystem_locomotion")
                .FirstOrDefault();
            if (!animatorDuplicateErase)
            {
                return null;
            }
            var maMergeAnimator = animatorDuplicateErase.GetComponent<ModularAvatarMergeAnimator>();
            if (!maMergeAnimator)
            {
                Debug.LogError("[PosingSystem]ポージングシステム用の共通ポージングMAMergeAnimatorが見つかりません");
                throw new System.Exception("ポージングシステム用の共通ポージングMAMergeAnimatorが見つかりません");
            }
            if (!maMergeAnimator.animator)
            {
                Debug.LogError("[PosingSystem]ポージングシステム用の共通ポージングMAMergeAnimatorにAnimatorが設定されていません");
                throw new System.Exception("ポージングシステム用の共通ポージングMAMergeAnimatorにAnimatorが設定されていません");
            }

            return maMergeAnimator;
        }

        public static ModularAvatarMergeAnimator GetPrebuiltMergeAnimator(PosingSystem posingSystem)
        {
            var avatar = posingSystem.GetAvatar();
            if (avatar == null)
            {
                return null;
            }
            // まずはプレビルド済みAnimatorを探す
            var animatorDuplicateErase = avatar.GetComponentsInChildren<DuplicateEraser>()
                .Where(d => d.gameObject.tag != "EditorOnly")
                .Where(d => d.ID == "jp.unisakistudio.posingsystem_locomotion")
                .Select(d => d.GetComponent<ModularAvatarMergeAnimator>())
                .Where(m => m != null && !((m.animator as AnimatorController).layers.Any(l => l.name == "USSPS_IsDefault")))
                .Select(m => m.GetComponent<DuplicateEraser>())
                .FirstOrDefault();

            if (!animatorDuplicateErase)
            {
                return null;
            }

            var maMergeAnimator = animatorDuplicateErase.GetComponent<ModularAvatarMergeAnimator>();
            if (!maMergeAnimator)
            {
                Debug.LogError("[PosingSystem]ポージングシステム用の共通ポージングMAMergeAnimatorが見つかりません");
                throw new System.Exception("ポージングシステム用の共通ポージングMAMergeAnimatorが見つかりません");
            }
            if (!maMergeAnimator.animator)
            {
                Debug.LogError("[PosingSystem]ポージングシステム用の共通ポージングMAMergeAnimatorにAnimatorが設定されていません");
                throw new System.Exception("ポージングシステム用の共通ポージングMAMergeAnimatorにAnimatorが設定されていません");
            }

            return maMergeAnimator;
        }

        public static void ConvertToModularAvatarComponents(PosingSystem posingSystem)
        {
            // 自動生成するメニュー項目を一度削除する
            DeletePosingMenuObjects(posingSystem);

            // definesのTypeParameterとSyncedParameterを再設定する
            ResetTypeAndSyncedParameter(posingSystem);

            // SyncedParameterが8bitを超えていたら拡張する
            ResetParametersWithSyncedParameter(posingSystem);

            // メニューを再作成する
            CreatePosingMenuObjects(posingSystem);

            // MergeAnimatorのAnimatorControllerがオリジナルじゃなかったらベースを複製して設定する
            CreateOriginalAnimatorController(posingSystem);

            // MergeAnimatorのAnimatorControllerのParametersをセットする
            ResetAnimatorControllerParameters(posingSystem);

            // AnimatorControllerを変更する
            ConvertAnimatorController(posingSystem);

            // アニメーションをOverrideする
            ConvertOverrideAnimations(posingSystem);

            // アイコン画像ファイルがあったらメニューに設定する
            if (posingSystem.thumbnailPackObject != null)
            {
                SetMenuIcon(posingSystem);
            }

            // 変更を保存
            posingSystem.data = GetDefineSerializeJson(posingSystem);

            // PosingSystemオブジェクトをDirtyにしてシーンの保存対象にする
            EditorUtility.SetDirty(posingSystem);
        }

        /// <summary>
        /// UnityオブジェクトのGUIDを取得（instanceIDに依存しない安定した識別子）
        /// </summary>
        private static string GetStableObjectIdentifier(UnityEngine.Object obj)
        {
            if (obj == null) return "";

            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(assetPath))
            {
                // GUIDを取得（アセットファイルの場合）
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (!string.IsNullOrEmpty(guid))
                {
                    return guid;
                }
            }

            // シーン内オブジェクトやランタイム生成オブジェクトの場合（フォールバック）
            return obj.name + "_" + obj.GetType().Name;
        }

        /// <summary>
        /// アセットの内容ハッシュを取得（アニメーションの中身が変わったことを検知するため）
        /// </summary>
        private static string GetAssetContentHash(UnityEngine.Object obj)
        {
            if (obj == null) return "";

            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (string.IsNullOrEmpty(assetPath))
            {
                return "";
            }

            // アセットの依存関係を含むハッシュを取得（内容が変わるとハッシュも変わる）
            Hash128 hash = AssetDatabase.GetAssetDependencyHash(assetPath);
            return hash.ToString();
        }

        public static string GetDefineSerializeJson(PosingSystem posingSystem, bool ignoreInstanceId = false)
        {
            // Map to DTOs that do NOT have previewImage fields
            var dto = new DefineSerializeRootDTO
            {
                list = new System.Collections.Generic.List<LayerDefineDTO>(),
                overrides = new System.Collections.Generic.List<OverrideAnimationDefineDTO>(),
                version = VersionName,
                savedInstanceId = ignoreInstanceId ? posingSystem.savedInstanceId : UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(posingSystem).ToString(),
            };

            if (posingSystem.defines != null)
            {
                foreach (var define in posingSystem.defines)
                {
                    var layerDTO = new LayerDefineDTO
                    {
                        menuName = define.menuName,
                        description = define.description,
                        stateMachineName = define.stateMachineName,
                        paramName = define.paramName,
                        iconGuid = GetStableObjectIdentifier(define.icon),
                        locomotionTypeValue = define.locomotionTypeValue,
                        animations = new System.Collections.Generic.List<AnimationDefineDTO>()
                    };
                    if (define.animations != null)
                    {
                        foreach (var a in define.animations)
                        {
                            var aDTO = new AnimationDefineDTO
                            {
                                enabled = a.enabled,
                                isRotate = a.isRotate,
                                rotate = a.rotate,
                                isMotionTime = a.isMotionTime,
                                motionTimeParamName = a.motionTimeParamName,
                                animationClipGuid = GetStableObjectIdentifier(a.animationClip),
                                adjustmentClipGuid = GetStableObjectIdentifier(a.adjustmentClip),
                                animationClipHash = GetAssetContentHash(a.animationClip),
                                adjustmentClipHash = GetAssetContentHash(a.adjustmentClip),
                                displayName = a.displayName,
                                initial = a.initial,
                                initialSet = a.initialSet,
                                isCustomIcon = a.isCustomIcon,
                                iconGuid = GetStableObjectIdentifier(a.icon),
                                typeParameterValue = a.typeParameterValue,
                                syncdParameterValue = a.syncdParameterValue
                            };
                            layerDTO.animations.Add(aDTO);
                        }
                    }
                    dto.list.Add(layerDTO);
                }
            }

            if (posingSystem.overrideDefines != null)
            {
                foreach (var ov in posingSystem.overrideDefines)
                {
                    var ovDTO = new OverrideAnimationDefineDTO
                    {
                        enabled = ov.enabled,
                        isRotate = ov.isRotate,
                        rotate = ov.rotate,
                        isMotionTime = ov.isMotionTime,
                        motionTimeParamName = ov.motionTimeParamName,
                        animationClipGuid = GetStableObjectIdentifier(ov.animationClip),
                        adjustmentClipGuid = GetStableObjectIdentifier(ov.adjustmentClip),
                        animationClipHash = GetAssetContentHash(ov.animationClip),
                        adjustmentClipHash = GetAssetContentHash(ov.adjustmentClip),
                        stateType = ov.stateType
                    };
                    dto.overrides.Add(ovDTO);
                }
            }

            return JsonUtility.ToJson(dto);
        }

        public static void DeletePosingMenuObjects(PosingSystem posingSystem)
        {
            if (posingSystem.SubmenuRoot == null)
            {
                return;
            }
            // SubmenuRoot内の姿勢グループのサブメニューを列挙
            foreach (var submenu in posingSystem.SubmenuRoot.GetComponentsInChildren<ModularAvatarMenuItem>().Where(menu => menu.Control != null && menu.Control.type == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu))
            {
                // 削除しちゃう
                Object.DestroyImmediate(submenu.gameObject);
            }
        }

        public static void ResetTypeAndSyncedParameter(PosingSystem posingSystem)
        {
            var typeParamHash = new Dictionary<string, Dictionary<int, PosingSystem.AnimationDefine>>();
            var syncedParamHash = new Dictionary<int, PosingSystem.AnimationDefine>();

            // 親からアバターを探す
            var avatar = posingSystem.GetAvatar();

            // アバターに入ってないならこの処理はしない
            if (avatar == null)
            {
                return;
            }

            // アバターの全PosingSystem（今設定したものは除く）の使用しているtypeParameterを記録する
            foreach (var otherPosingSystem in avatar.GetComponentsInChildren<PosingSystem>())
            {
                if (otherPosingSystem == posingSystem)
                {
                    continue;
                }

                foreach (var define in otherPosingSystem.defines)
                {
                    if (define.animations == null)
                    {
                        continue;
                    }
                    foreach (var animationDefine in define.animations)
                    {
                        if (!animationDefine.enabled)
                        {
                            continue;
                        }
                        // アニメーションのメニューの値（保存される値）を記録
                        if (animationDefine.typeParameterValue > 0)
                        {
                            if (!typeParamHash.ContainsKey(define.paramName))
                            {
                                typeParamHash.Add(define.paramName, new());
                            }
                            typeParamHash[define.paramName].Add(animationDefine.typeParameterValue, animationDefine);
                        }
                        // アニメーションの再生される値（同期される値）を記録
                        if (animationDefine.syncdParameterValue > 0)
                        {
                            syncedParamHash.Add(animationDefine.syncdParameterValue, animationDefine);
                        }
                    }
                }
            }

            var typeParamValues = new Dictionary<string, int>();
            var syncedParamValue = 4;

            // 空いてる数値を振る
            foreach (var define in posingSystem.defines)
            {
                if (!typeParamValues.ContainsKey(define.paramName))
                {
                    typeParamValues.Add(define.paramName, 2);
                }
                if (!typeParamHash.ContainsKey(define.paramName))
                {
                    typeParamHash.Add(define.paramName, new());
                }

                foreach (var animationDefine in define.animations)
                {
                    if (!animationDefine.enabled)
                    {
                        continue;
                    }

                    while (typeParamHash[define.paramName].ContainsKey(typeParamValues[define.paramName]))
                    {
                        typeParamValues[define.paramName] = typeParamValues[define.paramName] + 1;
                    }
                    while (syncedParamHash.ContainsKey(syncedParamValue))
                    {
                        syncedParamValue++;
                    }

                    animationDefine.typeParameterValue = typeParamValues[define.paramName];
                    animationDefine.syncdParameterValue = syncedParamValue;

                    typeParamValues[define.paramName] = typeParamValues[define.paramName] + 1;
                    syncedParamValue++;
                }
            }

            Undo.RecordObject(posingSystem, "reset parameter");
            EditorUtility.SetDirty(posingSystem);
        }

        public static void ResetParametersWithSyncedParameter(PosingSystem posingSystem)
        {
            // アバターのPosingSystemの一番大きなSyncedParameterValueを探す
            var avatar = posingSystem.GetAvatar();
            int maxSyncedParamValue = avatar.GetComponentsInChildren<PosingSystem>().DefaultIfEmpty().Max(p => p.defines.DefaultIfEmpty().Max(d => d.animations.DefaultIfEmpty().Max(a => a.syncdParameterValue)));

            // MAパラメータオブジェクト探す            
            var maParameter = posingSystem.GetComponent<ModularAvatarParameters>();
            if (maParameter == null)
            {
                Debug.LogError("[PosingSystem]「" + posingSystem.name + "」オブジェクトにMAParametersコンポーネントがありません");
                throw new System.Exception("「" + posingSystem.name + "」オブジェクトにMAParametersコンポーネントがありません");
            }

            // 必要なパラメータを列挙
            var parameters = SplitSyncParameters(0, maxSyncedParamValue);

            // 不要なパラメータを削除
            maParameter.parameters.RemoveAll(param =>
            {
                // 姿勢用パラメータでなければスキップ
                if (param.nameOrPrefix.IndexOf("USSPS_Pose") == -1)
                {
                    return false;
                }
                // 必要なパラメータに含まれているかで残すかを決める
                return parameters.Where(p => p.name == param.nameOrPrefix).Count() == 0;
            });

            // 必要なパラメータを追加
            foreach (var param in parameters.Where(p => p.sync && maParameter.parameters.Where(param => p.name == param.nameOrPrefix).Count() == 0))
            {
                var paramConfig = new ParameterConfig();
                paramConfig.syncType = param.type;
                paramConfig.localOnly = false;
                paramConfig.nameOrPrefix = param.name;
                paramConfig.defaultValue = 0;
                paramConfig.saved = false;
                maParameter.parameters.Add(paramConfig);
            }

            // initialが設定されている姿勢があったら、その姿勢のパラメータの数値を記録
            var initialParamValues = new Dictionary<string, int>();
            foreach (var otherPosingSystem in avatar.GetComponentsInChildren<PosingSystem>())
            {
                foreach (var define in otherPosingSystem.defines)
                {
                    foreach (var animation in define.animations)
                    {
                        if (!initialParamValues.ContainsKey(define.paramName))
                        {
                            initialParamValues[define.paramName] = 0;
                        }
                        if (animation.initial)
                        {
                            initialParamValues[define.paramName] = animation.typeParameterValue;
                        }
                    }
                }
            }
            foreach (var otherPosingSystem in avatar.GetComponentsInChildren<PosingSystem>())
            {
                var otherMaParameter = otherPosingSystem.GetComponent<ModularAvatarParameters>();
                otherMaParameter.parameters = otherMaParameter.parameters.Select(param => {
                    if (initialParamValues.ContainsKey(param.nameOrPrefix))
                    {
                        param.defaultValue = initialParamValues[param.nameOrPrefix];
                    }
                    return param;
                }).ToList();
            }

            Undo.RecordObject(maParameter, "reset parameter");
            EditorUtility.SetDirty(maParameter);
        }

        public static void CreatePosingMenuObjects(PosingSystem posingSystem)
        {
            var avatar = posingSystem.GetAvatar();
            foreach (var define in posingSystem.defines)
            {
                // アニメーションが無かったらスキップ
                bool isSkip = define.animations.Count(animation => animation.enabled) == 0;
                if (isSkip)
                {
                    continue;
                }

                // 設定されている姿勢メニューグループオブジェクトを取得
                Transform submenuRoot = posingSystem.SubmenuRoot;

                // もし設定されていなかったら「姿勢メニュー」という名前のメニューグループオブジェクトを探す
                if (submenuRoot == null)
                {
                    submenuRoot = posingSystem.transform.GetComponentsInChildren<ModularAvatarMenuGroup>().FirstOrDefault(group => group.gameObject.name == "姿勢メニュー")?.transform;
                }
                // もしそれでもなかったらメニューグループオブジェクトならなんでもいい
                if (submenuRoot == null)
                {
                    submenuRoot = posingSystem.transform.GetComponentsInChildren<ModularAvatarMenuGroup>().FirstOrDefault()?.transform;
                }
                // 姿勢メニューオブジェクトが見つからなかったらエラー
                if (submenuRoot == null)
                {
                    Debug.LogError("[PosingSystem]「" + posingSystem.name + "」の姿勢メニューオブジェクトが見つかりません");
                    throw new System.Exception("「" + posingSystem.name + "」の姿勢メニューオブジェクトが見つかりません");
                }

                // 姿勢メニューオブジェクトの親のPosingSystemを調べる
                var outOfPosingSystem = false;
                Transform parentSearch = submenuRoot.parent;
                while (parentSearch != null)
                {
                    PosingSystem parentPosingSystem = null;
                    if (parentSearch != posingSystem.transform && parentSearch.TryGetComponent<PosingSystem>(out parentPosingSystem))
                    {
                        outOfPosingSystem = true;
                        break;
                    }
                    parentSearch = parentSearch.parent;
                }
                // 親のPosingSystemが別のオブジェクトだったら、このPosingSystemのExメニューは消しちゃう
                if (outOfPosingSystem)
                {
                    foreach (var menuGroup in posingSystem.GetComponentsInChildren<ModularAvatarMenuGroup>())
                    {
                        Object.DestroyImmediate(menuGroup);
                    }
                    Object.DestroyImmediate(posingSystem.GetComponent<ModularAvatarMenuInstaller>());
                    Object.DestroyImmediate(posingSystem.GetComponent<ModularAvatarMenuItem>());
                }
                // この姿勢グループ用のメニューアイテム（例えば「立ち姿勢用」など）を取得
                var menu = submenuRoot.GetComponentsInChildren<ModularAvatarMenuItem>().FirstOrDefault((submenu) =>
                {
                    return submenu.Control.type == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu
                        && submenu.name == define.menuName;
                });
                // もしまだ作成していなかったら、姿勢グループ用メニューアイテムを作成
                GameObject menuObject = null;
                if (menu == null)
                {
                    menuObject = new GameObject(define.menuName);
                    menu = menuObject.AddComponent<ModularAvatarMenuItem>();
                    menu.MenuSource = SubmenuSource.Children;
                    menu.Control.type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu;
                    menu.Control.name = define.menuName;
                    menu.Control.icon = define.icon;
                    menu.Control.parameter = new() { name = "USSPS_LastMenu" };
                    menu.Control.value = define.locomotionTypeValue;
                    menuObject.transform.parent = submenuRoot;
                }
                // 無い場合は、複数のdefineで同じ姿勢グループを使用しているパターン
                else
                {
                    menuObject = menu.gameObject;
                }

                foreach (var animation in define.animations)
                {
                    if (!animation.enabled)
                    {
                        continue;
                    }

                    // 各姿勢のメニューアイテムを作成
                    var itemObject = new GameObject(animation.displayName);
                    itemObject.transform.parent = menuObject.transform;
                    var item = itemObject.AddComponent<ModularAvatarMenuItem>();
                    item.MenuSource = SubmenuSource.Children;
                    item.Control.type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.Toggle;
                    item.Control.name = animation.displayName;
                    item.Control.parameter = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Parameter();
                    item.Control.parameter.name = define.paramName;
                    item.Control.value = animation.typeParameterValue;

                    // アイコンがあったら設定する
                    if (animation.isCustomIcon)
                    {
                        item.Control.icon = animation.icon;
                    }
                }

            }
        }

        public static void CreateOriginalAnimatorController(PosingSystem posingSystem)
        {
            // 共通ポージングAnimatorを探す
            var maMergeAnimator = GetCommonMergeAnimator(posingSystem);
            
            if (maMergeAnimator == null || maMergeAnimator.animator == null)
            {
                Debug.LogError("[PosingSystem] MergeAnimator or its animator is null");
                return;
            }
            
            var animatorController = maMergeAnimator.animator as AnimatorController;
            if (animatorController == null)
            {
                Debug.LogError("[PosingSystem] Animator is not an AnimatorController");
                return;
            }

            // 設定されているAnimatorControllerが標準のものかどうかをチェックし、標準じゃないなら処理はもうしなくていい
            var isDefault = animatorController.layers.Any(l => l.name == "USSPS_IsDefault");
            if (!isDefault)
            {
                // 他のPosingSystemの共通ポージングAnimatorにも同じものを設定する
                var otherPosingSystems = posingSystem.GetAvatar().GetComponentsInChildren<PosingSystem>();
                foreach (var otherPosingSystem in otherPosingSystems)
                {
                    var otherMaMergeAnimator = GetCommonMergeAnimator(otherPosingSystem);
                    if (otherMaMergeAnimator)
                    {
                        otherMaMergeAnimator.animator = maMergeAnimator.animator;
                        Undo.RecordObject(otherMaMergeAnimator, "set animator controller");
                        EditorUtility.SetDirty(otherMaMergeAnimator);
                    }
                }

                // その先の処理は必要ない
                return;
            }

            // 別の共通ポージングAnimatorに設定されているものがないか調べる
            var prebuiltMergeAnimator = GetPrebuiltMergeAnimator(posingSystem);
            if (prebuiltMergeAnimator)
            {
                maMergeAnimator.animator = prebuiltMergeAnimator.animator;
                Undo.RecordObject(maMergeAnimator, "set animator controller");
                EditorUtility.SetDirty(maMergeAnimator);
                return;
            }
            var avatar = posingSystem.GetAvatar();

            var templeteAnimatorControllerPath = AssetDatabase.GetAssetPath(animatorController);

            // AnimatorControllerの保存先を決める
            var directoryPath = PosingSystemEditor.GetGeneratedFolderPath();
            var newAnimatorControllerPath = directoryPath + "/" + avatar.name + ".controller";
            newAnimatorControllerPath = AssetDatabase.GenerateUniqueAssetPath(newAnimatorControllerPath);

            // 複製を実行
            /*
            var newAnimatorController = new AnimatorController();
            EditorUtility.CopySerialized(maMergeAnimator.animator, newAnimatorController);
            AssetDatabase.CreateAsset(newAnimatorController, newAnimatorControllerPath);
            */
            var newAnimatorController = AnimatorControllerDeepCopy.CloneAnimatorController(animatorController, newAnimatorControllerPath);
//            AssetDatabase.CopyAsset(templeteAnimatorControllerPath, newAnimatorControllerPath);

            if (newAnimatorController == null)
            {
                return;
            }

            // オブジェクトに割り当てる
            maMergeAnimator.animator = newAnimatorController;

            // 新しいAnimatorControllerの「USSPS_IsDefault」レイヤーを削除する
            var isDefaultLayerIndex = newAnimatorController.layers.ToList().FindIndex(l => l.name == "USSPS_IsDefault");
            if (isDefaultLayerIndex != -1)
            {
                var layers = newAnimatorController.layers.ToList();
                layers.RemoveAt(isDefaultLayerIndex);
                newAnimatorController.layers = layers.ToArray();
                EditorUtility.SetDirty(newAnimatorController);
            }
            AssetDatabase.SaveAssets();

            // 変更を記録
            Undo.RecordObject(maMergeAnimator, "set animator controller");
            EditorUtility.SetDirty(maMergeAnimator);
        }

        public static void ResetAnimatorControllerParameters(PosingSystem posingSystem)
        {
            // 共通ポージングAnimatorを探す
            var maMergeAnimator = GetPrebuiltMergeAnimator(posingSystem);
            if (maMergeAnimator == null)
            {
                maMergeAnimator = GetCommonMergeAnimator(posingSystem);
            }

            // AnimatorControllerを取得
            var animatorController = maMergeAnimator.animator as AnimatorController;
            if (animatorController == null)
            {
                Debug.LogError("[PosingSystem]「" + posingSystem.name + "」の共通ポージングAnimatorに設定されているAnimatorControllerが不正です。AnimatorOverrideController等は使用できません");
                throw new System.Exception("「" + posingSystem.name + "」の共通ポージングAnimatorに設定されているAnimatorControllerが不正です。AnimatorOverrideController等は使用できません");
            }

            // アバターのPosingSystemの一番大きなSyncedParameterValueを探す
            var avatar = posingSystem.GetAvatar();
            int maxSyncedParamValue = avatar.GetComponentsInChildren<PosingSystem>().DefaultIfEmpty().Max(p => p.defines.DefaultIfEmpty().Max(d => d.animations.DefaultIfEmpty().Max(a => a.syncdParameterValue)));

            // 必要なパラメータを列挙
            var parameters = SplitSyncParameters(0, maxSyncedParamValue);

            // ないものがあったら追加する
            animatorController.parameters = animatorController.parameters.Concat(parameters.Where(param => animatorController.parameters.Where(p => param.name == p.name).Count() == 0).Select(param => new AnimatorControllerParameter() { name = param.name, type = param.aType, defaultBool = false, defaultInt = 0, })).ToArray();

            // 変更を記録
            Undo.RecordObject(animatorController, "set animator controller parameters");
            EditorUtility.SetDirty(animatorController);
        }

        public static void ConvertAnimatorController(PosingSystem posingSystem)
        {
            var avatar = posingSystem.GetAvatar();
            if (avatar == null)
            {
                return;
            }

            var waitAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("PosingSystem_Empty")[0]));

            // 共通ポージングAnimatorを探す
            var maMergeAnimator = GetPrebuiltMergeAnimator(posingSystem);
            if (maMergeAnimator == null)
            {
                maMergeAnimator = GetCommonMergeAnimator(posingSystem);
            }

            // AnimatorControllerを取得
            var animatorController = maMergeAnimator.animator as AnimatorController;
            if (animatorController == null)
            {
                Debug.LogError("[PosingSystem]「" + posingSystem.name + "」の共通ポージングAnimatorに設定されているAnimatorControllerが不正です。AnimatorOverrideController等は使用できません");
                throw new System.Exception("「" + posingSystem.name + "」の共通ポージングAnimatorに設定されているAnimatorControllerが不正です。AnimatorOverrideController等は使用できません");
            }

            var avatarAnimator = avatar.GetComponent<Animator>();
            if (avatarAnimator == null || avatarAnimator.avatar == null || !avatarAnimator.isHuman)
            {
                // ヒューマノイド前提の変換処理のため、環境が整っていない場合はスキップ
                return;
            }

            // アバターのPosingSystemの一番大きなSyncedParameterValueを探す
            int maxSyncedParamValue = avatar.GetComponentsInChildren<PosingSystem>().DefaultIfEmpty().Max(p => p.defines.DefaultIfEmpty().Max(d => d.animations.DefaultIfEmpty().Max(a => a.syncdParameterValue)));

            // LocomotionレイヤーとLocomotionTypeレイヤーを取得
            var layer = animatorController.layers.FirstOrDefault(l => l.name == "USSPS_Locomotion");
            if (layer == null)
            {
                return;
            }
            var stateMachine = layer.stateMachine;
            var typeLayer = animatorController.layers.FirstOrDefault(l => l.name == "USSPS_LocomotionType");
            if (typeLayer == null)
            {
                return;
            }

            // アバターの背の高さ係数を計算
            float avatarHeightUnit = -1;
            var baseAnimationClip = new AnimationClip();
            var rootTyBinding = new EditorCurveBinding
            {
                path = "",
                type = typeof(Animator),
                propertyName = "RootT.y"
            };
            AnimationCurve rootTyCurve = new();
            rootTyCurve.AddKey(0, 1);
            AnimationUtility.SetEditorCurve(baseAnimationClip, rootTyBinding, rootTyCurve);
            var clipInfo = new AnimationClipSettings
            {
                keepOriginalOrientation = true,
                keepOriginalPositionXZ = true,
                keepOriginalPositionY = true
            };
            AnimationUtility.SetAnimationClipSettings(baseAnimationClip, clipInfo);
            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(avatar.gameObject, baseAnimationClip, 0);
            avatar.transform.position = Vector3.zero;
            avatar.transform.rotation = Quaternion.identity;
            AnimationMode.EndSampling();
            avatarHeightUnit = avatar.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position.y / 2;
            AnimationMode.StopAnimationMode();
            avatar.gameObject.SetActive(false);
            avatar.gameObject.SetActive(true);

            foreach (var define in posingSystem.defines)
            {
                foreach (var animationDefine in define.animations)
                {
                    if (!animationDefine.enabled)
                    {
                        continue;
                    }

                    if (animatorController.parameters.Where(param => param.name == animationDefine.motionTimeParamName).Count() == 0)
                    {
                        animatorController.AddParameter(animationDefine.motionTimeParamName, AnimatorControllerParameterType.Float);
                    }

                    // アニメーションを作成
                    Motion motion = null;
                    if (animationDefine.animationClip is AnimationClip baseClip)
                    {
                        var animationClip = baseClip != null ? baseClip : waitAnimation;
                        AnimationMode.StartAnimationMode();
                        //                                ctx.AvatarRootTransform.position = new Vector3(ctx.AvatarDescriptor.ViewPosition.x, 0, ctx.AvatarDescriptor.ViewPosition.z);
                        AnimationMode.BeginSampling();
                        var eyeObject = new GameObject();
                        eyeObject.transform.parent = avatar.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
                        //eyeObject.transform.position = ctx.AvatarDescriptor.ViewPosition + new Vector3(0, 0.1f, 0);
                        eyeObject.transform.localPosition = new Vector3(0, 0, 0);
                        AnimationMode.SampleAnimationClip(avatar.gameObject, animationClip, 0);
                        avatar.transform.LookAt(new Vector3(0, 0, 1));
                        AnimationMode.EndSampling();
                        //avatar.transform.transform.rotation = Quaternion.Euler(0, animationDefine.rotate, 0) * avatar.transform.transform.rotation;
                        //var headPosision = avatar.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position;
                        var headPosision = eyeObject.transform.position;
                        GameObject.DestroyImmediate(eyeObject);

                        animationClip = Object.Instantiate(animationClip);

                        var srcClipSetting = AnimationUtility.GetAnimationClipSettings(animationClip);
                        if (srcClipSetting.mirror)
                        {
                            srcClipSetting.mirror = false;
                            AnimationUtility.SetAnimationClipSettings(animationClip, srcClipSetting);
                        }

                        // RootQの各プロパティのカーブを個別に処理
                        var rootQxCurve = AnimationUtility.GetEditorCurve(animationClip, EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.x"));
                        var rootQyCurve = AnimationUtility.GetEditorCurve(animationClip, EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.y"));
                        var rootQzCurve = AnimationUtility.GetEditorCurve(animationClip, EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.z"));
                        var rootQwCurve = AnimationUtility.GetEditorCurve(animationClip, EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.w"));

                        var rootTxList = new List<float>();
                        var rootTyList = new List<float>();
                        var rootTzList = new List<float>();
                        foreach (var binding in AnimationUtility.GetCurveBindings(animationClip))
                        {
                            if (!binding.propertyName.StartsWith("RootT."))
                            {
                                continue;
                            }
                            var curve = AnimationUtility.GetEditorCurve(animationClip, binding);
                            for (int i = 0; i < curve.keys.Length; i++)
                            {
                                AnimationMode.BeginSampling();
                                AnimationMode.SampleAnimationClip(avatar.gameObject, animationClip, curve.keys[i].time);
                                AnimationMode.EndSampling();
                                var rootT = new Vector3();
                                var rootBinding = binding;
                                rootBinding.propertyName = "RootT.x";
                                AnimationUtility.GetFloatValue(avatar.gameObject, rootBinding, out rootT.x);
                                rootBinding.propertyName = "RootT.y";
                                AnimationUtility.GetFloatValue(avatar.gameObject, rootBinding, out rootT.y);
                                rootBinding.propertyName = "RootT.z";
                                AnimationUtility.GetFloatValue(avatar.gameObject, rootBinding, out rootT.z);

                                rootT -= headPosision / avatarHeightUnit;
                                //rootT = Quaternion.Euler(0, animationDefine.rotate, 0) * rootT;
                                if (binding.propertyName == "RootT.x")
                                    rootTxList.Add(rootT.x);
                                if (binding.propertyName == "RootT.y")
                                    rootTyList.Add(rootT.y);
                                if (binding.propertyName == "RootT.z")
                                    rootTzList.Add(rootT.z);
                            }
                        }

                        AnimationMode.StopAnimationMode();
                        avatar.gameObject.SetActive(false);
                        avatar.gameObject.SetActive(true);

                        var newAnimationClip = Object.Instantiate(animationClip);
                        if (animationDefine.adjustmentClip != null)
                        {
                            var adjustmentClip = animationDefine.adjustmentClip;
                            foreach (var binding in AnimationUtility.GetCurveBindings(adjustmentClip))
                            {
                                var adjustmentCurve = AnimationUtility.GetEditorCurve(adjustmentClip, binding);
                                var baseCurve = AnimationUtility.GetEditorCurve(newAnimationClip, binding);
                                if (baseCurve == null)
                                {
                                    baseCurve = new AnimationCurve();
                                }
                                baseCurve = AdditiveCurveUtility.AddCurve(baseCurve, adjustmentCurve);
                                AnimationUtility.SetEditorCurve(newAnimationClip, binding, baseCurve);
                            }
                        }
                        foreach (var binding in AnimationUtility.GetCurveBindings(newAnimationClip))
                        {
                            void setCurve(string propertyName, ValueChanger changer)
                            {
                                if (binding.propertyName == propertyName)
                                {
                                    var curve = AnimationUtility.GetEditorCurve(newAnimationClip, binding);
                                    for (int i = 0; i < curve.keys.Length; i++)
                                    {
                                        var key = curve.keys[i];
                                        key.value = changer(key, i);
                                        curve.MoveKey(i, key);
                                    }
                                    AnimationUtility.SetEditorCurve(newAnimationClip, binding, curve);
                                }
                            }
                            ;

                            setCurve("RootT.x", (keyframe, i) => i < rootTxList.Count ? rootTxList[i] : keyframe.value);
                            setCurve("RootT.z", (keyframe, i) => i < rootTzList.Count ? rootTzList[i] : keyframe.value);

                            // RootQカーブが存在する場合のみ処理（調整アニメーション対応）
                            if (rootQxCurve != null)
                                setCurve("RootQ.x", (keyframe, i) => keyframe.value);
                            if (rootQyCurve != null)
                                setCurve("RootQ.y", (keyframe, i) => keyframe.value);
                            if (rootQzCurve != null)
                                setCurve("RootQ.z", (keyframe, i) => keyframe.value);
                            if (rootQwCurve != null)
                                setCurve("RootQ.w", (keyframe, i) => keyframe.value);
                        }
                        if (animationDefine.isRotate)
                        {
                            var clipSetting = AnimationUtility.GetAnimationClipSettings(newAnimationClip);
                            clipSetting.orientationOffsetY += animationDefine.rotate;
                            AnimationUtility.SetAnimationClipSettings(newAnimationClip, clipSetting);
                        }

                        // 生成したアニメーションクリップをAnimatorControllerのサブアセットとして永続化
                        newAnimationClip.name = "_USSPS_" + (animationDefine.animationClip != null ? animationDefine.animationClip.name : "Animation") + "_Processed";
                        if (!IsObjectAlreadyInAssetDatabase(newAnimationClip))
                        {
                            AssetDatabase.AddObjectToAsset(newAnimationClip, animatorController);
                        }
                        EditorUtility.SetDirty(animatorController);
                        motion = newAnimationClip;
                    }
                    else if (animationDefine.animationClip.GetType() == typeof(BlendTree))
                    {
                        motion = animationDefine.animationClip;
                    }

                    // 同期するパラメータを計算
                    var parameters = SplitSyncParameters(animationDefine.syncdParameterValue, maxSyncedParamValue);

                    var animationName = animationDefine.animationClip != null ? animationDefine.animationClip.name : "Animation";

                    // デスクトップ用
                    {
                        // 同じ条件の遷移を取得する
                        var entryTransitionContidions = GetEntryTransitionConditions(parameters, false);
                        var existTransition = stateMachine.entryTransitions.FirstOrDefault(t =>
                        {
                            return CompareTransitionConditions(t.conditions, entryTransitionContidions);
                        });

                        AnimatorState state = null;

                        // もしまだないならStateから作る
                        if (!existTransition)
                        {
                            state = stateMachine.AddState(animationName + "_Desktop", new Vector3(500, (animationDefine.syncdParameterValue - 1) * 120));
                            state.writeDefaultValues = false;
                            state.mirrorParameterActive = true;
                            state.mirrorParameter = "USSPS_Mirror";

                            var poseSpace = state.AddStateMachineBehaviour<VRC.SDK3.Avatars.Components.VRCAnimatorTemporaryPoseSpace>();
                            if (poseSpace == null)
                            {
                                Debug.LogError("[PosingSystem]AddStateMachineBehaviourに失敗しました");
                                throw new System.Exception("AddStateMachineBehaviourに失敗しました");
                            }
                            poseSpace.enterPoseSpace = true;
                            poseSpace.fixedDelay = false;
                            poseSpace.delayTime = 0.0f;

                            // EntryのTransitionを作成
                            var transition = stateMachine.AddEntryTransition(state);
                            transition.conditions = GetEntryTransitionConditions(parameters, false);

                            // ExitのTransitionを作成
                            CreateExitTransitions(parameters, state, false);
                        }
                        else
                        {
                            state = existTransition.destinationState;
                            var states = stateMachine.states;
                            states[states.ToList().FindIndex(s => s.state == state)].position = new Vector3(500, (animationDefine.syncdParameterValue - 1) * 120);
                            stateMachine.states = states;
                        }

                        if (animationDefine.isMotionTime)
                        {
                            state.timeParameterActive = true;
                            state.timeParameter = animationDefine.motionTimeParamName;
                        }

                        state.motion = motion;
                    }

                    // ３点トラッキング用
                    {
                        // 同じ条件の遷移を取得する
                        var entryTransitionContidions = GetEntryTransitionConditions(parameters, true);
                        var existTransition = stateMachine.entryTransitions.FirstOrDefault(t =>
                        {
                            return CompareTransitionConditions(t.conditions, entryTransitionContidions);
                        });

                        AnimatorState state = null;

                        // もしまだないならStateから作る
                        if (!existTransition)
                        {
                            state = stateMachine.AddState(animationName, new Vector3(500, (animationDefine.syncdParameterValue - 1) * 120 + 60));

                            state.writeDefaultValues = false;
                            state.mirrorParameterActive = true;
                            state.mirrorParameter = "USSPS_Mirror";

                            // EntryのTransitionを作成
                            var transition = stateMachine.AddEntryTransition(state);
                            transition.conditions = GetEntryTransitionConditions(parameters, true);

                            // ExitのTransitionを作成
                            CreateExitTransitions(parameters, state, true);
                        }
                        else
                        {
                            state = existTransition.destinationState;
                            var states = stateMachine.states;
                            states[states.ToList().FindIndex(s => s.state == state)].position = new Vector3(500, (animationDefine.syncdParameterValue - 1) * 120 + 60);
                            stateMachine.states = states;
                        }

                        if (animationDefine.isMotionTime)
                        {
                            state.timeParameterActive = true;
                            state.timeParameter = animationDefine.motionTimeParamName;
                        }

                        state.motion = motion;
                    }

                    // 姿勢決めConditionsを作成
                    var typeEntryConditions = new AnimatorCondition[]
                    {
                        new AnimatorCondition{ mode = AnimatorConditionMode.Equals, threshold = animationDefine.typeParameterValue, parameter = define.paramName, },
                        new AnimatorCondition{ mode = AnimatorConditionMode.Equals, threshold = define.locomotionTypeValue, parameter = "LocomotionType", },
                    };
                    var existTypeTransition = typeLayer.stateMachine.entryTransitions.FirstOrDefault(t =>
                    {
                        return CompareTransitionConditions(t.conditions, typeEntryConditions);
                    });

                    AnimatorState typeState = null;
                    if (!existTypeTransition)
                    {
                        // 姿勢を決めるLocomotionTypeレイヤーに置くStateを作成
                        typeState = typeLayer.stateMachine.AddState(animationName, new Vector3(500, (animationDefine.syncdParameterValue - 1) * 60));
                        typeState.writeDefaultValues = false;
                        typeState.motion = waitAnimation;

                        // 姿勢決定のEntryTransitionを作成
                        var typeEnterTransition = typeLayer.stateMachine.AddEntryTransition(typeState);
                        typeEnterTransition.conditions = typeEntryConditions;

                        // 姿勢が変わった時のExitTransitionを作成（指定してる姿勢が変わった場合）
                        var typeExitTransition = typeState.AddExitTransition(false);
                        typeExitTransition.AddCondition(AnimatorConditionMode.NotEqual, animationDefine.typeParameterValue, define.paramName);
                        typeExitTransition.duration = 0.5f;
                        typeExitTransition.interruptionSource = TransitionInterruptionSource.Destination;

                        // 姿勢が変わった時のExitTransitionを作成（姿勢グループが変わった場合）
                        var typeExitTransition1 = typeState.AddExitTransition(false);
                        typeExitTransition1.AddCondition(AnimatorConditionMode.NotEqual, define.locomotionTypeValue, "LocomotionType");
                        typeExitTransition1.duration = 0.5f;
                        typeExitTransition1.interruptionSource = TransitionInterruptionSource.Destination;
                    }
                    else
                    {
                        typeState = existTypeTransition.destinationState;
                        foreach (var behaviour in typeState.behaviours)
                        {
                            //                            Object.DestroyImmediate(behaviour);
                        }
                        var states = typeLayer.stateMachine.states;
                        states[states.ToList().FindIndex(s => s.state == typeState)].position = new Vector3(500, (animationDefine.syncdParameterValue - 1) * 60);
                        typeLayer.stateMachine.states = states;
                    }

                    // 再生する姿勢を同期するためのParameterDriverを作成
                    var typeParameterDriver = typeState.AddStateMachineBehaviour<VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver>();
                    if (typeParameterDriver == null)
                    {
                        Debug.LogError("[PosingSystem]AddStateMachineBehaviourに失敗しました");
                        throw new System.Exception("AddStateMachineBehaviourに失敗しました");
                    }
                    typeParameterDriver.isLocalPlayer = true;
                    foreach (var parameter in parameters)
                    {
                        var drivingParam = new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter
                        {
                            name = parameter.name,
                            type = VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType.Set,
                            value = parameter.value,
                        };
                        typeParameterDriver.parameters.Add(drivingParam);
                    }
                }
            }

            AssetDatabase.SaveAssets();

            // FootHeight用のBlendTreeを作る
            var skipBlendTree = new List<BlendTree>();
            void addFootHeightBlendtree(AnimatorStateMachine stateMachine)
            {
                foreach (var childStateMachine in stateMachine.stateMachines)
                {
                    addFootHeightBlendtree(childStateMachine.stateMachine);
                }

                BlendTree getFootHeightBlendtree(BlendTree blendtree, float level)
                {
                    if (blendtree == null)
                    {
                        return null;
                    }

                    var blendTreeChildren = new List<ChildMotion>();
                    foreach (var childMotion in blendtree.children)
                    {
                        if (childMotion.motion == null)
                        {
                            continue;
                        }
                        Motion newMotion;
                        if (childMotion.motion.GetType() == typeof(BlendTree))
                        {
                            newMotion = getFootHeightBlendtree((BlendTree)childMotion.motion, level);
                            // 再帰的生成でnullが返された場合のtype mismatch回避
                            if (newMotion == null)
                            {
                                Debug.LogWarning($"[PosingSystem]FootHeight BlendTree再帰生成でnull: {childMotion.motion.name}, level={level}");
                                continue; // この子はスキップ
                            }
                        }
                        else
                        {
                            if (childMotion.motion.name.IndexOf("proxy_") == 0)
                            {
                                newMotion = childMotion.motion;
                            }
                            else
                            {
                                var animationClip = (AnimationClip)childMotion.motion;
                                var newAnimationClip = Object.Instantiate(animationClip);
                                var clipSetting = AnimationUtility.GetAnimationClipSettings(newAnimationClip);
                                clipSetting.level += level;
                                AnimationUtility.SetAnimationClipSettings(newAnimationClip, clipSetting);

                                // SubAsset用に名前にUSSPS接頭辞を追加
                                newAnimationClip.name = "_USSPS_" + animationClip.name + "_level" + level;

                                newMotion = newAnimationClip;
                            }
                        }
                        var newChild = new ChildMotion
                        {
                            motion = newMotion,
                            threshold = childMotion.threshold,
                            position = childMotion.position,
                            timeScale = childMotion.timeScale,
                            cycleOffset = childMotion.cycleOffset,
                            directBlendParameter = childMotion.directBlendParameter,
                            mirror = childMotion.mirror
                        };
                        blendTreeChildren.Add(newChild);
                    }

                    // 子が一つもない場合はnullを返す（type mismatch回避）
                    if (blendTreeChildren.Count == 0)
                    {
                        Debug.LogWarning($"[PosingSystem]FootHeight BlendTree生成で子が0個: {blendtree.name}, level={level}");
                        return null;
                    }

                    var newBlendtree = new BlendTree
                    {
                        blendType = blendtree.blendType,
                        blendParameter = blendtree.blendParameter,
                        blendParameterY = blendtree.blendParameterY,
                        name = "_USSPS_" + blendtree.name + "_" + level.ToString(),
                        children = blendTreeChildren.ToArray(),
                        useAutomaticThresholds = blendtree.useAutomaticThresholds
                    };
                    if (blendtree.useAutomaticThresholds)
                    {
                        newBlendtree.minThreshold = blendtree.minThreshold;
                        newBlendtree.maxThreshold = blendtree.maxThreshold;
                    }
                    else
                    {
                        // 手動thresholdの場合、子のthreshold値が既に適切に設定されていることを確認
                        // （上記のChildMotion作成で元のthresholdをコピー済み）
                    }

                    // 生成したBlendTreeをAnimatorControllerのサブアセットとして永続化
                    if (!IsObjectAlreadyInAssetDatabase(newBlendtree))
                    {
                        AssetDatabase.AddObjectToAsset(newBlendtree, animatorController);
                    }
                    // 子のAnimationClipも永続化
                    foreach (var child in newBlendtree.children)
                    {
                        if (child.motion != null && child.motion.GetType() == typeof(AnimationClip) &&
                            child.motion.name.IndexOf("proxy_") != 0 &&
                            !IsObjectAlreadyInAssetDatabase(child.motion))
                        {
                            AssetDatabase.AddObjectToAsset(child.motion, animatorController);
                        }
                    }
                    EditorUtility.SetDirty(animatorController);

                    return newBlendtree;
                }

                foreach (var state in stateMachine.states)
                {
                    if (state.state.motion == null)
                    {
                        continue;
                    }
                    // 既にFootHeight用のBlendTreeがある場合はスキップ
                    if (state.state.motion.name.IndexOf("_USSPS_") == 0 && state.state.motion.name.IndexOf("_footheight") != -1)
                    {
                        continue;
                    }
                    if (state.state.motion.GetType() == typeof(AnimationClip))
                    {
                        if (state.state.motion.name.IndexOf("proxy_") != 0)
                        {
                            var animationClip = (AnimationClip)state.state.motion;
                            var newAnimationClip = Object.Instantiate(animationClip);
                            var clipSetting = AnimationUtility.GetAnimationClipSettings(newAnimationClip);
                            clipSetting.level += 2;
                            AnimationUtility.SetAnimationClipSettings(newAnimationClip, clipSetting);
                            newAnimationClip.name = "_USSPS_" + animationClip.name + "_up";

                            // FootHeight用アニメーション
                            var heightAnimationClipZero = Object.Instantiate(newAnimationClip);
                            clipSetting.level -= 2;
                            AnimationUtility.SetAnimationClipSettings(heightAnimationClipZero, clipSetting);
                            heightAnimationClipZero.name = "_USSPS_" + animationClip.name + "_zero";

                            // FootHeight用アニメーション
                            var heightAnimationClip = Object.Instantiate(newAnimationClip);
                            clipSetting.level -= 2;
                            AnimationUtility.SetAnimationClipSettings(heightAnimationClip, clipSetting);
                            heightAnimationClip.name = "_USSPS_" + animationClip.name + "_down";

                            // FootHeight用BlendTree作成
                            var footHeightBlendTree = new BlendTree();
                            footHeightBlendTree.name = "_USSPS_" + animationClip.name + "_footheight";
                            footHeightBlendTree.blendParameter = "FootHeight";
                            footHeightBlendTree.useAutomaticThresholds = false;

                            // サムネイル撮影のための意図的なthreshold順序設定
                            footHeightBlendTree.AddChild(heightAnimationClipZero, 0.5f);  // threshold 0.5: 中央（撮影用）
                            footHeightBlendTree.AddChild(newAnimationClip, 0);            // threshold 0: 元モーション
                            footHeightBlendTree.AddChild(heightAnimationClip, 1);         // threshold 1: 上

                            // FootHeight用BlendTreeをAnimatorControllerのサブアセットとして永続化
                            if (!IsObjectAlreadyInAssetDatabase(footHeightBlendTree))
                            {
                                AssetDatabase.AddObjectToAsset(footHeightBlendTree, animatorController);
                            }
                            if (!IsObjectAlreadyInAssetDatabase(heightAnimationClipZero))
                            {
                                AssetDatabase.AddObjectToAsset(heightAnimationClipZero, animatorController);
                            }
                            if (!IsObjectAlreadyInAssetDatabase(heightAnimationClip))
                            {
                                AssetDatabase.AddObjectToAsset(heightAnimationClip, animatorController);
                            }
                            if (!IsObjectAlreadyInAssetDatabase(newAnimationClip))
                            {
                                AssetDatabase.AddObjectToAsset(newAnimationClip, animatorController);
                            }
                            EditorUtility.SetDirty(animatorController);

                            state.state.motion = footHeightBlendTree;
                        }
                    }
                    else
                    {
                        var blendTree = (BlendTree)state.state.motion;

                        var footHeightBlendtreeDown = getFootHeightBlendtree(blendTree, 2);
                        var footHeightBlendtreeZero = getFootHeightBlendtree(blendTree, 0);
                        var footHeightBlendtreeUp = getFootHeightBlendtree(blendTree, -2);

                        // null チェック（type mismatch回避）
                        if (footHeightBlendtreeDown == null || footHeightBlendtreeZero == null || footHeightBlendtreeUp == null)
                        {
                            Debug.LogWarning($"[PosingSystem]FootHeight BlendTree生成でnullが発生: {blendTree.name}");
                            continue; // このstateはスキップ
                        }

                        var footHeightBlendTree = new BlendTree();
                        footHeightBlendTree.name = "_USSPS_" + blendTree.name + "_footheight";
                        footHeightBlendTree.blendParameter = "FootHeight";
                        footHeightBlendTree.useAutomaticThresholds = false;
                        // サムネイル撮影のための意図的なthreshold順序設定
                        footHeightBlendTree.AddChild(footHeightBlendtreeZero, 0.5f);  // threshold 0.5: 中央（撮影用）
                        footHeightBlendTree.AddChild(footHeightBlendtreeDown, 0);     // threshold 0: 下 (level +2)
                        footHeightBlendTree.AddChild(footHeightBlendtreeUp, 1);       // threshold 1: 上 (level -2)

                        // FootHeight用BlendTreeをAnimatorControllerのサブアセットとして永続化
                        if (!IsObjectAlreadyInAssetDatabase(footHeightBlendTree))
                        {
                            AssetDatabase.AddObjectToAsset(footHeightBlendTree, animatorController);
                        }
                        // 子BlendTreeも永続化
                        AddBlendTreeToAsset(footHeightBlendtreeZero, animatorController);
                        AddBlendTreeToAsset(footHeightBlendtreeDown, animatorController);
                        AddBlendTreeToAsset(footHeightBlendtreeUp, animatorController);
                        EditorUtility.SetDirty(animatorController);

                        state.state.motion = footHeightBlendTree;
                    }
                }
            }
            addFootHeightBlendtree(layer.stateMachine);

            // 古いSubAssetを削除（AnimatorController肥大化防止）
            RemoveOldSubAssets(animatorController);

        }

        /// <summary>
        /// BlendTreeとその子要素を再帰的にAnimatorControllerのサブアセットとして永続化
        /// </summary>
        private static void AddBlendTreeToAsset(BlendTree blendTree, AnimatorController animatorController)
        {
            if (blendTree == null) return;

            // 既にアセットファイルに追加されていない場合のみ追加
            if (!IsObjectAlreadyInAssetDatabase(blendTree))
            {
                AssetDatabase.AddObjectToAsset(blendTree, animatorController);
            }

            foreach (var child in blendTree.children)
            {
                if (child.motion == null) continue;

                if (child.motion.GetType() == typeof(BlendTree))
                {
                    AddBlendTreeToAsset((BlendTree)child.motion, animatorController);
                }
                else if (child.motion.GetType() == typeof(AnimationClip) &&
                         child.motion.name.IndexOf("proxy_") != 0 &&
                         !IsObjectAlreadyInAssetDatabase(child.motion))
                {
                    AssetDatabase.AddObjectToAsset(child.motion, animatorController);
                }
            }
        }

        /// <summary>
        /// オブジェクトが既にアセットデータベースに存在するかチェック
        /// </summary>
        private static bool IsObjectAlreadyInAssetDatabase(UnityEngine.Object obj)
        {
            if (obj == null) return true;

            // メインアセットまたはサブアセットとして既に存在するかチェック
            string assetPath = AssetDatabase.GetAssetPath(obj);
            return !string.IsNullOrEmpty(assetPath);
        }

        /// <summary>
        /// AnimatorControllerから古いPosingSystem関連のSubAssetを削除
        /// </summary>
        private static void RemoveOldSubAssets(AnimatorController animatorController)
        {
            if (animatorController == null) return;

            var allAssets = AssetDatabase.LoadAllAssetsAtPath(AssetDatabase.GetAssetPath(animatorController));
            var assetsToRemove = new List<UnityEngine.Object>();

            // まずはAnimatorController内で使われているAnimationClipとBlendTreeを列挙する
            var usedAnimationClips = new List<AnimationClip>();
            var usedBlendTrees = new List<BlendTree>();
            System.Action<BlendTree> checkBlendTreeAction = null;
            System.Action<AnimatorState> checkStateAction = null;
            System.Action<AnimatorStateMachine> checkStateMachine = null;

            checkBlendTreeAction = (BlendTree blendTree) =>
            {
                if (usedBlendTrees.Contains(blendTree)) return;

                usedBlendTrees.Add(blendTree);
                foreach (var child in blendTree.children)
                {
                    if (child.motion is BlendTree childBlendTree)
                    {
                        checkBlendTreeAction(childBlendTree);
                    }
                    if (child.motion is AnimationClip childClip)
                    {
                        if (usedAnimationClips.Contains(childClip)) continue;
                        usedAnimationClips.Add(childClip);
                    }
                }
            };
            checkStateAction = (AnimatorState state) =>
            {
                if (state.motion is AnimationClip clip)
                {
                    usedAnimationClips.Add(clip);
                }
                else if (state.motion is BlendTree blendTree)
                {
                    checkBlendTreeAction(blendTree);
                }
            };
            checkStateMachine = (AnimatorStateMachine stateMachine) =>
            {
                foreach (var state in stateMachine.states)
                {
                    checkStateAction(state.state);
                }
                foreach (var subStateMachine in stateMachine.stateMachines)
                {
                    checkStateMachine(subStateMachine.stateMachine);
                }
            };

            // 全てのレイヤーで使っているAnimationClipとBlendTreeを列挙
            foreach (var layer in animatorController.layers)
            {
                checkStateMachine(layer.stateMachine);
            }

            foreach (var asset in allAssets)
            {
                if (asset == animatorController) continue; // メインアセットはスキップ

                // PosingSystem関連のSubAssetを特定
                bool shouldRemove = false;

                if (asset is AnimationClip clip)
                {
                    // 使われていたら削除しない
                    if (usedAnimationClips.Contains(clip)) continue;

                    // USSPSで生成されたAnimationClip（確実に当ツールで生成されたもの）
                    if (clip.name.StartsWith("_USSPS_"))
                    {
                        shouldRemove = true;
                    }
                }
                else if (asset is BlendTree blendTree)
                {
                    // 使われていたら削除しない
                    if (usedBlendTrees.Contains(blendTree)) continue;

                    // USSPSで生成されたBlendTree（確実に当ツールで生成されたもの）
                    if (blendTree.name.StartsWith("_USSPS_"))
                    {
                        shouldRemove = true;
                    }
                }

                if (shouldRemove)
                {
                    assetsToRemove.Add(asset);
                }
            }

            // 削除実行
            foreach (var asset in assetsToRemove)
            {
                AssetDatabase.RemoveObjectFromAsset(asset);
                UnityEngine.Object.DestroyImmediate(asset, true);
            }

            if (assetsToRemove.Count > 0)
            {
                EditorUtility.SetDirty(animatorController);
                AssetDatabase.SaveAssets();
                Debug.Log($"[PosingSystem]AnimatorController SubAsset削除: {assetsToRemove.Count}個のアセットを削除しました");
            }
        }

        public static Camera CreateIconCamera()
        {
            var cameraGameObject = new GameObject();
            var camera = cameraGameObject.AddComponent<Camera>();
            camera.fieldOfView = 30;
            camera.clearFlags = CameraClearFlags.Color;
            camera.backgroundColor = new Color(0, 0, 0, 0);
            camera.cullingMask = 1 << PosingSystem.PreviewMask;

            return camera;
        }

        public static void SetCameraForAvatar(Camera camera, GameObject avatarObject)
        {
            var cameraHeight = avatarObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position.y - avatarObject.transform.position.y;
            var cameraDepth = avatarObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftFoot).position.z - -avatarObject.transform.position.z;
            var cameraDepth2 = avatarObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position.z - -avatarObject.transform.position.z;
            var distance = Mathf.Max(Mathf.Abs(cameraDepth), cameraHeight) + 0.5f;
            camera.transform.position = avatarObject.transform.position + new Vector3(-distance, /*cameraHeight + 0.2f*/1, distance + cameraDepth / 2);
            camera.transform.LookAt(avatarObject.transform.position + new Vector3(0, cameraHeight * 0.5f, /*cameraDepth / 2*/(cameraDepth + cameraDepth2) * 0.5f));
        }

        public static void TakeScreenshot(PosingSystem posingSystem, bool force = false, bool drypreview = true)
        {
            // NDMFのビルドパス内では OnPreprocessAvatar を起動しないため、preprocessを回避するドライプレビューのみ許可
            if (_isExecutingNdMfPass)
            {
                drypreview = true;
            }
            if (IsAndroidBuildTarget())
            {
                return;
            }
            if (posingSystem.isIconDisabled)
            {
                return;
            }

            bool skip = true;
            for (int i = 0; i < posingSystem.defines.Count; i++)
            {
                for (int j = 0; j < posingSystem.defines[i].animations.Count; j++)
                {
                    if (force == false && posingSystem.defines[i].animations[j].previewImage != null)
                    {
                        continue;
                    }
                    if (force == false && posingSystem.defines[i].animations[j].isCustomIcon)
                    {
                        continue;
                    }
                    skip = false;
                }
            }
            if (posingSystem.overrideDefines != null)
            {
                for (int j = 0; j < posingSystem.overrideDefines.Count; j++)
                {
                    if (force == false && posingSystem.overrideDefines[j].previewImage != null)
                    {
                        continue;
                    }
                    skip = false;
                }
            }
            if (skip)
            {
                return;
            }

            var srcAvatar = posingSystem.GetAvatar();
            if (srcAvatar == null)
            {
                // アバター外に配置されているなど、取得できない場合は何もしない
                return;
            }
            GameObject clone;
            if (posingSystem.previewAvatarObject)
            {
                clone = posingSystem.previewAvatarObject;
            }
            else
            {
                clone = GameObject.Instantiate(srcAvatar.gameObject, PosingSystemEditor.GetPreviewAvatarRoot());
                if (!drypreview)
                {
                    if (posingSystem.previewAvatarObject)
                    {
                        Object.DestroyImmediate(posingSystem.previewAvatarObject);
                    }
                    posingSystem.previewAvatarObject = clone;
                    foreach (var clonePosingSystem in clone.GetComponentsInChildren<PosingSystem>())
                    {
                        clonePosingSystem.gameObject.tag = "EditorOnly";
                    }
                    VRC.SDKBase.Editor.BuildPipeline.VRCBuildPipelineCallbacks.OnPreprocessAvatar(clone);
                    Object.DestroyImmediate(clone.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>());
                }
            }

            var camera = PosingSystemConverter.CreateIconCamera();

            clone.SetActive(true);
            clone.layer = PosingSystem.PreviewMask;
            foreach (var child in clone.GetComponentsInChildren<Transform>())
            {
                child.gameObject.layer = clone.layer;
            }

            try
            {
                for (int i = 0; i < posingSystem.defines.Count; i++)
                {
                    for (int j = 0; j < posingSystem.defines[i].animations.Count; j++)
                    {
                        var animation = posingSystem.defines[i].animations[j];

                        if (force == false && animation.previewImage != null)
                        {
                            continue;
                        }
                        if (force == false && animation.isCustomIcon)
                        {
                            continue;
                        }

                        TakeIconScreenshot(animation, clone, camera, force);
                        if (!AssetDatabase.IsSubAsset(animation.previewImage))
                        {
                            if (posingSystem.thumbnailPackObject != null)
                            {
                                AssetDatabase.AddObjectToAsset(animation.previewImage, posingSystem.thumbnailPackObject);
                            }
                        }
                    }
                }
                for (int j = 0; j < posingSystem.overrideDefines.Count; j++)
                {
                    var animation = posingSystem.overrideDefines[j];

                    if (force == false && animation.previewImage != null)
                    {
                        continue;
                    }

                    TakeIconScreenshot(animation, clone, camera, force);
                    if (!AssetDatabase.IsSubAsset(animation.previewImage))
                    {
                        if (posingSystem.thumbnailPackObject != null)
                        {
                            AssetDatabase.AddObjectToAsset(animation.previewImage, posingSystem.thumbnailPackObject);
                        }
                    }
                }
                AssetDatabase.SaveAssets();
            }
            finally
            {
                RenderTexture.active = null;
                Object.DestroyImmediate(camera.gameObject);

                clone.layer = 0;
                clone.hideFlags = HideFlags.HideInHierarchy;
                clone.tag = "EditorOnly";
                foreach (var child in clone.GetComponentsInChildren<Transform>())
                {
                    child.gameObject.layer = clone.layer;
                }
                clone.SetActive(false);
                if (posingSystem.previewAvatarObject == null)
                {
                    Object.DestroyImmediate(clone.gameObject);
                }
            }
        }

        public static void SetMenuIcon(PosingSystem posingSystem)
        {
            if (IsAndroidBuildTarget())
            {
                return;
            }
            var srcAvatar = posingSystem.GetAvatar();
            if (srcAvatar == null)
            {
                return;
            }

            var defineParamNames = posingSystem.defines.Select(define => define.paramName);
            var checkedSubMenus = new HashSet<int>();
            var controlHash = new Dictionary<(string paramName, int value), VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control>();

            // メニューから本システムのメニューアイテムを探す
            void deepSearchControl(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control control)
            {
                if (control.type == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu)
                {
                    if (control.subMenu == null)
                    {
                        return;
                    }
                    int subMenuInstanceId = control.subMenu.GetInstanceID();
                    if (checkedSubMenus.Contains(subMenuInstanceId))
                    {
                        return;
                    }
                    checkedSubMenus.Add(subMenuInstanceId);
                    foreach (var subControl in control.subMenu.controls)
                    {
                        deepSearchControl(subControl);
                    }
                    return;
                }

                // Toggleならパラメータ名でチェック
                if (control.type == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.Toggle)
                {
                    if (control.parameter == null)
                    {
                        return;
                    }
                    if (!defineParamNames.Contains(control.parameter.name))
                    {
                        return;
                    }

                    controlHash[(control.parameter.name, (int)control.value)] = control;
                }
            }

            if (srcAvatar.expressionsMenu)
            {
                foreach (var control in srcAvatar.expressionsMenu.controls)
                {
                    deepSearchControl(control);
                }
            }

            foreach (var menuItem in posingSystem.GetComponentsInChildren<ModularAvatarMenuItem>())
            {
                controlHash[(menuItem.Control.parameter.name, (int)menuItem.Control.value)] = menuItem.Control;
            }


            for (int i = 0; i < posingSystem.defines.Count; i++)
            {
                for (int j = 0; j < posingSystem.defines[i].animations.Count; j++)
                {
                    var animation = posingSystem.defines[i].animations[j];
                    if (controlHash.ContainsKey((posingSystem.defines[i].paramName, animation.typeParameterValue)))
                    {
                        controlHash[(posingSystem.defines[i].paramName, animation.typeParameterValue)].icon = animation.previewImage;
                    }
                }
            }
        }

        public static void TakeIconScreenshot(PosingSystem.BaseAnimationDefine animation, GameObject avatarObject, Camera camera, bool force)
        {
            var animationDefine = animation as PosingSystem.AnimationDefine;
            if (force == false && animation.previewImage != null)
            {
                return;
            }
            if (force == false && animationDefine != null && animationDefine.isCustomIcon)
            {
                return;
            }

            AnimationMode.StartAnimationMode();
            if (animation.animationClip == null)
            {
                avatarObject.transform.position = new Vector3(0, 0, 0);
            }
            else
            {
                AnimationClip getFirstAnimationClip(Motion motion)
                {
                    if (motion == null)
                    {
                        return null;
                    }
                    if (motion.GetType() == typeof(AnimationClip))
                    {
                        return (AnimationClip)motion;
                    }
                    if (motion.GetType() == typeof(UnityEditor.Animations.BlendTree))
                    {
                        foreach (var child in ((UnityEditor.Animations.BlendTree)motion).children)
                        {
                            var clip = getFirstAnimationClip(child.motion);
                            if (clip)
                            {
                                return clip;
                            }
                        }
                    }
                    return null;
                }
                var firstAnimationClip = getFirstAnimationClip(animation.animationClip);
                if (firstAnimationClip)
                {
                    // 調整アニメーションを適用したクリップを作成
                    var clipToSample = firstAnimationClip;
                    if (animationDefine != null && animationDefine.adjustmentClip != null)
                    {
                        clipToSample = Object.Instantiate(firstAnimationClip);
                        var adjustmentClip = animationDefine.adjustmentClip;
                        foreach (var binding in AnimationUtility.GetCurveBindings(adjustmentClip))
                        {
                            var adjustmentCurve = AnimationUtility.GetEditorCurve(adjustmentClip, binding);
                            var baseCurve = AnimationUtility.GetEditorCurve(clipToSample, binding);
                            if (baseCurve == null)
                            {
                                baseCurve = new AnimationCurve();
                            }
                            baseCurve = AdditiveCurveUtility.AddCurve(baseCurve, adjustmentCurve);
                            AnimationUtility.SetEditorCurve(clipToSample, binding, baseCurve);
                        }
                    }

                    avatarObject.transform.position = new Vector3(0, 0, 0);
                    avatarObject.transform.LookAt(new Vector3(0, 0, 1));
                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(avatarObject, clipToSample, 0);
                    AnimationMode.EndSampling();

                    // 一時的に作成したクリップを削除
                    if (clipToSample != firstAnimationClip)
                    {
                        Object.DestroyImmediate(clipToSample);
                    }


                    if (animation.isRotate)
                    {
                        avatarObject.transform.Rotate(0, animation.rotate, 0);
                    }
#if MODULAR_AVATAR
                    foreach (var mergeArmature in avatarObject.GetComponentsInChildren<ModularAvatarMergeArmature>())
                    {
                        mergeArmature.transform.position = new Vector3(200, 0, 0);
                    }
#endif
                }

                var cameraHeight = avatarObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position.y;
                var cameraDepth = avatarObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftFoot).position.z;
                var cameraDepth2 = avatarObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position.z;
                var cameraBase = avatarObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips).position.z;
                var distance = Mathf.Max(Mathf.Abs(cameraDepth - cameraBase), cameraHeight) + 0.5f;
                camera.transform.position = new Vector3(100 - distance, /*cameraHeight + 0.2f*/1, cameraBase + distance + (cameraDepth + cameraDepth2) * 0.5f / 2);
                camera.transform.LookAt(new Vector3(100, cameraHeight * 0.5f, /*cameraDepth / 2*/(cameraDepth + cameraDepth2) * 0.5f));

                PosingSystemConverter.SetCameraForAvatar(camera, avatarObject);
            }

            camera.backgroundColor = new Color(0, 0, 0, 0);

            avatarObject.SetActive(false);
            avatarObject.SetActive(true);

            camera.targetTexture = new RenderTexture(300, 300, 24);
            camera.Render();
            AnimationMode.StopAnimationMode();


            if (animation.previewImage == null)
            {
                animation.previewImage = new Texture2D(300, 300, TextureFormat.ARGB32, false);
            }
            RenderTexture.active = camera.targetTexture;
            if (animation as PosingSystem.AnimationDefine != null)
            {
                animation.previewImage.name = (animation as PosingSystem.AnimationDefine).displayName;
            }
            else if (animation as PosingSystem.OverrideAnimationDefine != null)
            {
                animation.previewImage.name = (animation as PosingSystem.OverrideAnimationDefine).stateType.ToString();
            }
            animation.previewImage.ReadPixels(new Rect(0, 0, 300, 300), 0, 0);
            animation.previewImage.Apply();
        }

        public static List<(PosingSystem.OverrideAnimationDefine.AnimationStateType stateType, string layerName, string stateMachineName, string stateName, bool isBlendTree, float posX, float posY)> OverrideSettings = new()
        {
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun, "USSPS_Locomotion", "", "Standing", false, 0, 0),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun, "USSPS_Locomotion", "", "Standing_Desktop", false, 0, 0),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.Stand, "USSPS_Locomotion", "", "Standing_Desktop", true, 0, 0),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.Stand, "USSPS_Locomotion", "", "Standing", true, 0, 0),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.Crouch, "USSPS_Locomotion", "", "Crouching_Desktop", false, 0, 0),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.Crouch, "USSPS_Locomotion", "", "Crouching", false, 0, 0),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.Prone, "USSPS_Locomotion", "", "Prone_Desktop", false, 0, 0),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.Prone, "USSPS_Locomotion", "", "Prone", false, 0, 0),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.Jump, "USSPS_Locomotion", "Jump and Fall", "Jump", false, 0, 0),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortFall, "USSPS_Locomotion", "Jump and Fall", "Short Fall", false, 0, 0),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortLanding, "USSPS_Locomotion", "Jump and Fall", "Soft Landing", false, 0, 0),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.LongFall, "USSPS_Locomotion", "Jump and Fall", "Long Fall", false, 0, 0),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.LongLanding, "USSPS_Locomotion", "Jump and Fall", "Hard Landing", false, 0, 0),
            (PosingSystem.OverrideAnimationDefine.AnimationStateType.AvatarSelect, "USSPS_Locomotion", "", "AvatarSelecting", false, 0, 0),
        };

        public static void ConvertOverrideAnimations(PosingSystem posingSystem)
        {
            // 共通ポージングAnimatorを探す
            var maMergeAnimator = GetPrebuiltMergeAnimator(posingSystem);
            if (maMergeAnimator == null)
            {
                maMergeAnimator = GetCommonMergeAnimator(posingSystem);
            }

            // AnimatorControllerを取得
            var animatorController = maMergeAnimator.animator as AnimatorController;
            if (animatorController == null)
            {
                Debug.LogError("[PosingSystem]「" + posingSystem.name + "」の共通ポージングAnimatorに設定されているAnimatorControllerが不正です。AnimatorOverrideController等は使用できません");
                throw new System.Exception("「" + posingSystem.name + "」の共通ポージングAnimatorに設定されているAnimatorControllerが不正です。AnimatorOverrideController等は使用できません");
            }

            foreach (var define in posingSystem.overrideDefines)
            {
                foreach (var overrideSetting in OverrideSettings.Where(setting => setting.stateType == define.stateType))
                {
                    var layerIndex = animatorController.layers.ToList().FindIndex(l => l.name == overrideSetting.layerName);
                    if (layerIndex == -1)
                    {
                        continue;
                    }
                    var layer = animatorController.layers[layerIndex];
                    AnimatorState animatorState = null;
                    if (overrideSetting.stateMachineName.Length > 0)
                    {
                        var stateMachine = layer.stateMachine.stateMachines.FirstOrDefault(s => s.stateMachine.name == overrideSetting.stateMachineName).stateMachine;
                        if (stateMachine != null)
                        {
                            animatorState = stateMachine.states.First(state => state.state.name == overrideSetting.stateName).state;
                        }
                    }
                    else
                    {
                        animatorState = layer.stateMachine.states.FirstOrDefault(state => state.state.name == overrideSetting.stateName).state;
                    }
                    if (animatorState == null)
                    {
                        continue;
                    }

                    if (overrideSetting.isBlendTree)
                    {
                        if (animatorState.motion.GetType() == typeof(BlendTree) || animatorState.motion.GetType().IsSubclassOf(typeof(BlendTree)))
                        {
                            BlendTree blendTree = (BlendTree)animatorState.motion;
                            var blendTreeIndex = blendTree.children.ToList().FindIndex(child => Mathf.Approximately(child.position.x, overrideSetting.posX) && Mathf.Approximately(child.position.y, overrideSetting.posY));
                            if (blendTreeIndex != -1)
                            {
                                blendTree.RemoveChild(blendTreeIndex);
                            }
                            blendTree.AddChild(define.animationClip, new Vector2(overrideSetting.posX, overrideSetting.posY));
                        }
                        else
                        {
                            Debug.LogError(string.Format("[PosingSystem]可愛いポーズツールのPosingOverride機能でBlendTreeのはずのAnimatorStateがBlendTreeではありませんでした。「立ちのみ」の変更を行う場合は「立ち・歩き・走り」にはBlendTreeを設定してください"));
                        }
                    }
                    else
                    {
                        animatorState.motion = define.animationClip;
                    }
                }
            }
        }

    }

}

#endif
