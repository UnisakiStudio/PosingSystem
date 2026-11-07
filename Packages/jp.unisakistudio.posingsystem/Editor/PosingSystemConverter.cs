#region

using System.Collections.Immutable;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using jp.unisakistudio.posingsystem;
using VRC.SDKBase;
using VRC.SDK3.Avatars.Components;
#if NDMF
using nadena.dev.ndmf;
using nadena.dev.ndmf.vrchat;
using nadena.dev.ndmf.animator;

#endif
#if MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

#endregion

#if NDMF && MODULAR_AVATAR

[assembly: ExportsPlugin(typeof(jp.unisakistudio.posingsystemeditor.PosingSystemConverter))]

namespace jp.unisakistudio.posingsystemeditor
{
    public partial class PosingSystemConverter : Plugin<PosingSystemConverter>
    {
        public const string AdditiveBreathingDuplicateEraserId =
            "jp.unisakistudio.posingsystem_additive_breathing";
        public const string DefaultAdditiveBreathingControllerGuid =
            "8e775531e66ee35469b6c4d6a6b741fd";
        public const string BreathingStrongClipGuid = "ef90febf8e7934b45a1030ad138e96e5";
        public const string BreathingMediumClipGuid = "a497bb916b6f9e9459b224ad7c50a228";
        public const string BreathingWeakClipGuid = "dd189cb98905b73498257854e1ad50c5";
        public const string BreathingRapidClipGuid = "ca474236197ac9f4f99d0da8925e90d5";
        public const string DefaultEmptyAnimationGuid =
            "52aff6f8eb70ac94ca2d631a92b39a80";

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
            public bool disablePoseIk;
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
            public bool isRollover;
            public string rolloverSideAnimationGuid;
            public string rolloverSideAnimationHash;
            public float rolloverContactStrength;
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
        private class AdditiveBreathingSettingsDTO
        {
            public bool enabled;
            public bool disableDefaultIdle;
            public PosingSystem.BreathingLevel standingLevel;
            public PosingSystem.BreathingLevel sitShallowLevel;
            public PosingSystem.BreathingLevel sitDeepLevel;
            public PosingSystem.BreathingLevel sitSleepLevel;
            public PosingSystem.BreathingLevel virtualLoveLevel;
            public PosingSystem.BreathingLevel typeFixLevel;
            public string standingGuid;
            public string standingHash;
            public string sitShallowGuid;
            public string sitShallowHash;
            public string sitDeepGuid;
            public string sitDeepHash;
            public string sitSleepGuid;
            public string sitSleepHash;
            public string virtualLoveGuid;
            public string virtualLoveHash;
            public string typeFixGuid;
            public string typeFixHash;
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
            public string featureSettings;
            public bool disableSleepJumpSwitch;
            public string rolloverClipGuid;
            public string rolloverClipHash;
            public bool enablePoseIk;
            public bool isIconDisabled;
            public bool isIconSmall;
            public int additiveBreathingLogicVersion;
            public int autoPoseLogicVersion;
            public int importedMotionLogicVersion;
            public int generatedAssetLogicVersion;
            public int controllerLogicVersion;
            public int floatingPhotographyLogicVersion;
            public bool autoImportAvatarAnimations;
            public string importedMotionInputs;
            public int rolloverCenteringLogicVersion;
            public int rolloverGenerationLogicVersion;
            public bool autoPoseKneeSafetyEnabled;
            public float autoPoseMinimumKneeAngle;
            public Vector2 autoPoseKneeBaseline;
            public string autoPoseCatalogGuid;
            public string autoPoseCatalogHash;
            public System.Collections.Generic.List<LayerDefineDTO> list;
            public System.Collections.Generic.List<OverrideAnimationDefineDTO> overrides;
            public AdditiveBreathingSettingsDTO additiveBreathing;
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

            // MAが同期パラメーターやメニューを処理する前に、選択した機能に同期する。
            InPhase(BuildPhase.Generating)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run("Synchronize optional posing features", ctx =>
                {
                    if (ctx == null || ctx.AvatarRootObject == null)
                    {
                        return;
                    }

                    foreach (var posingSystem in ctx.AvatarRootObject
                                 .GetComponentsInChildren<PosingSystem>(true))
                    {
                        PosingSystemFeatureUtility.Synchronize(posingSystem);
                    }
                });

            // Compile before MA remaps parameters. NDMF carries only this build's eligibility
            // to the later conversion pass; explicit reference/comparison scopes take precedence.
            InPhase(BuildPhase.Resolving)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run("Compile posing controller source", ctx =>
                {
                    ctx.GetState<AutoImportBuildState>().Capture(ctx.AvatarRootObject);
                    PosingSystemControllerGeneration.PrepareBeforeModularAvatar(ctx.AvatarRootObject, ctx.AssetSaver,
                        ctx.GetState<PosingSystemControllerGeneration.BuildState>());
                });

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

                    // Capture before DuplicateEraser removes shared settings objects.
                    using var imports = new AutoImportBuildScope(ctx.GetState<AutoImportBuildState>());
                    using var generation = PosingSystemControllerGeneration.BeginDefaultScope(ctx.AvatarRootObject,
                        ctx.GetState<PosingSystemControllerGeneration.BuildState>());
                    ctx.GetState<DefaultIdleSuppressionState>().enabled = ctx.AvatarRootObject
                        .GetComponentsInChildren<PosingSystem>()
                        .Any(ps => ps.additiveBreathing?.disableDefaultIdle == true &&
                            !ps.GetComponentsInParent<Transform>(true).Any(t => t.CompareTag("EditorOnly")));

                    foreach (var posingSystem in ctx.AvatarRootObject.GetComponentsInChildren<PosingSystem>())
                    {
                        if (posingSystem.tag == "EditorOnly")
                        {
                            continue;
                        }
                        PosingSystemDiagnostics.ThrowIfInvalid(posingSystem);
                        // 未プレビルド（null / 空文字）は直後に自動変換するため、
                        // 「設定変更済み」としてNDMFエラーを報告しない。
                        if (ShouldReportPrebuildUpdateWarning(posingSystem))
                        {
                            ErrorReport.ReportError(errorLocalizer, ErrorSeverity.NonFatal, "オブジェクトの設定が更新されています。再度プレビルドを行ってください", posingSystem.name);
                        }

                        if (posingSystem.data == null || posingSystem.data.Length == 0 || posingSystem.data != GetDefineSerializeJson(posingSystem, true))
                        {
                            try
                            {
                                ConvertToModularAvatarComponents(posingSystem, ctx.AssetSaver);
                            }
                            catch (AddStateMachineBehaviourCloneFailedException)
                            {
                                ErrorReport.ReportError(errorLocalizer, ErrorSeverity.Error, "AddStateMachineBehaviourに失敗しました");
                            }
                        }
                    }

                    // FloorAdjuster等、TransformingフェーズでHumanoidのRootT基準を変更するツールとの
                    // 整合をとるため、変換基準時点のRootT 1単位の実寸を記録する。
                    if (ctx.AvatarRootObject.GetComponentsInChildren<PosingSystem>().Any(ps => ps.tag != "EditorOnly"))
                    {
                        var calibration = ctx.GetState<RootTranslationCalibrationState>();
                        MeasureRootTranslationUnits(ctx.AvatarRootObject, out calibration.baseUnits);
                        calibration.measured = HasValidRootTranslationUnits(calibration.baseUnits);
                    }
                });

            // Narazaka FloorAdjusterの適用後に、ベイク済みの姿勢RootTカーブを補正する。
            // MA Floor Adjusterは自身でHumanoidを再構築するため、そのlate処理より前に固定して正常系を変えない。
            InPhase(BuildPhase.Transforming)
                .AfterPlugin("nadena.dev.modular-avatar")
                .AfterPlugin("net.narazaka.vrchat.floor_adjuster")
                .BeforePlugin("nadena.dev.modular-avatar.late-transform-stages")
                .Run("Recalibrate pose root translation", ctx =>
                {
                    RecalibratePoseRootTranslation(ctx);
                });

            // No AnimatorServicesContext compatibility: NDMF commits MA's virtual controllers
            // before this pass. Editing the committed build clone also covers MA Append/Replace.
            InPhase(BuildPhase.Transforming)
                .AfterPlugin("nadena.dev.modular-avatar")
                .BeforePlugin("nadena.dev.modular-avatar.late-transform-stages")
                .Run("Suppress VRChat default idle", SuppressDefaultIdle);

            InPhase(BuildPhase.Transforming)
                .AfterPlugin("nadena.dev.modular-avatar")
                .BeforePlugin("nadena.dev.modular-avatar.late-transform-stages")
                .Run("Keep local photography head visible", BuildPhotographyHeadVisibility);

            // トラッキング機能の統合（プレビルドでは実行しない、NDMFビルド時のみ、MAの前）
            var mergeControlSequence = InPhase(BuildPhase.Generating)
                .BeforePlugin("nadena.dev.modular-avatar");
            mergeControlSequence.WithRequiredExtension(typeof(AnimatorServicesContext), sequence =>
            {
                sequence.Run("Merge tracking control", ctx =>
                {
                    if (ctx == null || ctx.AvatarRootObject == null)
                    {
                        return;
                    }

                    bool mergeTrackingControl = ctx.AvatarRootObject
                        .GetComponentsInChildren<jp.unisakistudio.posingsystem.PosingSystem>()
                        .Any(posingSystem => posingSystem.tag != "EditorOnly" &&
                            (posingSystem.mergeTrackingControl || (posingSystem.features?.photography ?? false)));

                    if (!mergeTrackingControl)
                    {
                        return;
                    }

                    try
                    {
                        // NDMF標準の仮想Controllerだけを書き換える。
                        // 元アセットや共有Controllerを直接変更しない。
                        var controllerContext = ctx.Extension<AnimatorServicesContext>().ControllerContext;
                        MergeTrackingControl(ctx.VRChatAvatarDescriptor(), controllerContext);
                        MergeLocomotionControl(ctx.VRChatAvatarDescriptor(), controllerContext);
                    }
                    catch (AddStateMachineBehaviourFailedException)
                    {
                        ErrorReport.ReportError(errorLocalizer, ErrorSeverity.Error, "AddStateMachineBehaviourに失敗しました");
                    }
                });
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


        // 変換基準時点（Resolvingフェーズ）のRootT実寸。Transformingフェーズの再校正パスと共有する
        private class RootTranslationCalibrationState
        {
            public bool measured;
            public Vector3 baseUnits;
        }

        // RootT=0と各軸=1のクリップをサンプリングし、各RootT軸の1単位が実寸で何mかを計測する。
        // humanScaleや頭の高さから推定せず、FloorAdjuster適用前後の実際のHumanoid変換をそのまま比較する。
        private static void MeasureRootTranslationUnits(GameObject avatarRoot, out Vector3 units)
        {
            units = Vector3.zero;
            if (avatarRoot == null)
            {
                return;
            }
            var animator = avatarRoot.GetComponent<Animator>();
            if (animator == null || !animator.isHuman
                || animator.GetBoneTransform(HumanBodyBones.Head) == null)
            {
                return;
            }

            var workingAvatars = new GameObject[4];
            var clips = new AnimationClip[4];
            try
            {
                var headPositions = new Vector3[4];
                var unitAxes = new[] { -1, 0, 1, 2 };
                for (int sampleIndex = 0; sampleIndex < unitAxes.Length; sampleIndex++)
                {
                    var workingAvatar = Object.Instantiate(avatarRoot);
                    workingAvatars[sampleIndex] = workingAvatar;
                    workingAvatar.name = "_PosingSystem_TempMeasureAvatar";
                    workingAvatar.hideFlags = HideFlags.HideAndDontSave;
                    workingAvatar.SetActive(true);

                    var workingAnimator = workingAvatar.GetComponent<Animator>();
                    var headBone = workingAnimator != null && workingAnimator.isHuman
                        ? workingAnimator.GetBoneTransform(HumanBodyBones.Head)
                        : null;
                    if (headBone == null)
                    {
                        return;
                    }

                    var clip = new AnimationClip
                    {
                        name = "_PosingSystem_TempRootTranslationClip",
                        hideFlags = HideFlags.HideAndDontSave
                    };
                    clips[sampleIndex] = clip;
                    var propertyNames = new[] { "RootT.x", "RootT.y", "RootT.z" };
                    for (int axis = 0; axis < propertyNames.Length; axis++)
                    {
                        var binding = EditorCurveBinding.FloatCurve(
                            string.Empty, typeof(Animator), propertyNames[axis]);
                        var value = unitAxes[sampleIndex] == axis ? 1f : 0f;
                        AnimationUtility.SetEditorCurve(
                            clip, binding, new AnimationCurve(new Keyframe(0f, value)));
                    }
                    AnimationUtility.SetAnimationClipSettings(clip, new AnimationClipSettings
                    {
                        keepOriginalOrientation = true,
                        keepOriginalPositionXZ = true,
                        keepOriginalPositionY = true
                    });

                    clip.SampleAnimation(workingAvatar, 0f);
                    workingAvatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    headPositions[sampleIndex] = headBone.position;
                }

                units = new Vector3(
                    Vector3.Distance(headPositions[0], headPositions[1]),
                    Vector3.Distance(headPositions[0], headPositions[2]),
                    Vector3.Distance(headPositions[0], headPositions[3]));
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PosingSystem] RootT基準の計測中にエラーが発生しました: {e.Message}\n{e.StackTrace}");
                units = Vector3.zero;
            }
            finally
            {
                foreach (var clip in clips)
                {
                    if (clip != null)
                    {
                        Object.DestroyImmediate(clip);
                    }
                }
                foreach (var workingAvatar in workingAvatars)
                {
                    if (workingAvatar != null)
                    {
                        Object.DestroyImmediate(workingAvatar);
                    }
                }
            }
        }

        private static bool HasValidRootTranslationUnits(Vector3 units)
        {
            return units.x > 0.0001f && units.y > 0.0001f && units.z > 0.0001f;
        }

        // Resolvingフェーズで記録したRootT各軸の実寸と、Transforming後の実寸を比較して
        // ベイク済みRootTカーブを補正する。FloorAdjusterの方式や実装に依存せず、
        // 変換前後で同じワールド距離になるよう各軸を baseUnit / finalUnit で換算する。
        private void RecalibratePoseRootTranslation(BuildContext ctx)
        {
            if (ctx == null || ctx.AvatarRootObject == null)
            {
                return;
            }
            // PosingSystemコンポーネントはOptimizingフェーズで破棄されるため、この時点では存在する
            if (!ctx.AvatarRootObject.GetComponentsInChildren<PosingSystem>().Any(ps => ps.tag != "EditorOnly"))
            {
                return;
            }

            var calibration = ctx.GetState<RootTranslationCalibrationState>();
            if (!calibration.measured)
            {
                return;
            }

            MeasureRootTranslationUnits(ctx.AvatarRootObject, out var finalUnits);
            if (!HasValidRootTranslationUnits(finalUnits))
            {
                return;
            }

            var unitScale = new Vector3(
                calibration.baseUnits.x / finalUnits.x,
                calibration.baseUnits.y / finalUnits.y,
                calibration.baseUnits.z / finalUnits.z);
            if (Mathf.Abs(unitScale.x - 1f) < 0.0005f
                && Mathf.Abs(unitScale.y - 1f) < 0.0005f
                && Mathf.Abs(unitScale.z - 1f) < 0.0005f)
            {
                return;
            }

            var descriptor = ctx.VRChatAvatarDescriptor();
            var fixedMotions = new Dictionary<Motion, Motion>();
            var processedControllers = new HashSet<AnimatorController>();
            GameObject workingAvatar = null;
            try
            {
                // 全ポーズで共有する計測用クローン。ポーズごとにアバター全体を複製しない。
                workingAvatar = Object.Instantiate(ctx.AvatarRootObject);
                workingAvatar.name = "_PosingSystem_TempRecenterAvatar";
                workingAvatar.hideFlags = HideFlags.HideAndDontSave;
                workingAvatar.SetActive(true);

                foreach (var customLayer in descriptor.baseAnimationLayers.Concat(descriptor.specialAnimationLayers))
                {
                    if (customLayer.animatorController is not AnimatorController animatorController
                        || !processedControllers.Add(animatorController))
                    {
                        continue;
                    }
                    var layers = animatorController.layers;
                    foreach (var layer in layers)
                    {
                        if (layer.stateMachine == null)
                        {
                            continue;
                        }
                        RecalibrateStateMachine(
                            ctx, layer.stateMachine, fixedMotions, unitScale, finalUnits, workingAvatar);
                        if (layer.syncedLayerIndex >= 0 && layer.syncedLayerIndex < layers.Length)
                        foreach (var state in AllFeatureStates(layers[layer.syncedLayerIndex].stateMachine))
                        {
                            var motion = layer.GetOverrideMotion(state);
                            if (motion == null || (!motion.name.StartsWith(FootHeightOffsetPrefix) && !motion.name.StartsWith(MovementOffsetPrefix))) continue;
                            layer.SetOverrideMotion(state, RecalibrateMotion(ctx, motion, fixedMotions, unitScale, finalUnits, workingAvatar));
                        }
                    }
                    animatorController.layers = layers;
                }
            }
            finally
            {
                if (workingAvatar != null)
                {
                    Object.DestroyImmediate(workingAvatar);
                }
            }

            Debug.Log(
                $"[PosingSystem] RootT基準の変化をビルド中に検出したため、姿勢を補正しました。" +
                $" X:{calibration.baseUnits.x:0.###}→{finalUnits.x:0.###}m" +
                $" Y:{calibration.baseUnits.y:0.###}→{finalUnits.y:0.###}m" +
                $" Z:{calibration.baseUnits.z:0.###}→{finalUnits.z:0.###}m");
        }

        private void RecalibrateStateMachine(
            BuildContext ctx, AnimatorStateMachine stateMachine,
            Dictionary<Motion, Motion> fixedMotions, Vector3 unitScale,
            Vector3 finalUnits, GameObject workingAvatar)
        {
            foreach (var childStateMachine in stateMachine.stateMachines)
            {
                if (childStateMachine.stateMachine != null)
                {
                    RecalibrateStateMachine(ctx, childStateMachine.stateMachine, fixedMotions, unitScale, finalUnits, workingAvatar);
                }
            }
            foreach (var state in stateMachine.states)
            {
                var motion = state.state.motion;
                // 本ツールが生成した姿勢モーション（FootHeight BlendTree配下）のみを対象にする
                if (motion == null || motion.name.IndexOf("_USSPS_") != 0 || motion.name.IndexOf("_footheight") == -1)
                {
                    continue;
                }
                state.state.motion = RecalibrateMotion(ctx, motion, fixedMotions, unitScale, finalUnits, workingAvatar);
            }
        }

        private Motion RecalibrateMotion(
            BuildContext ctx, Motion motion, Dictionary<Motion, Motion> fixedMotions,
            Vector3 unitScale, Vector3 finalUnits, GameObject workingAvatar)
        {
            if (motion == null)
            {
                return null;
            }
            if (fixedMotions.TryGetValue(motion, out var cached))
            {
                return cached;
            }

            if (motion is AnimationClip animationClip)
            {
                // VRChatのproxyアニメーションは触らない
                if (animationClip.name.IndexOf("proxy_") == 0)
                {
                    fixedMotions[motion] = animationClip;
                    return animationClip;
                }
                var rootTranslationBindings = new[] { "RootT.x", "RootT.y", "RootT.z" }
                    .Select(propertyName => EditorCurveBinding.FloatCurve(
                        string.Empty, typeof(Animator), propertyName))
                    .ToArray();
                if (!rootTranslationBindings.Any(binding =>
                        AnimationUtility.GetEditorCurve(animationClip, binding) != null))
                {
                    fixedMotions[motion] = animationClip;
                    return animationClip;
                }

                var targetClip = animationClip;
                // 外部の永続アセットは直接書き換えない。一方、MA/NDMF がこのビルド用に生成した
                // 一時アセットは既に安全なコピーなので、その場で補正する。
                // 一時AssetContainer内の入れ子Motionを Object.Instantiate すると、Unityが
                // kStrongPPtrMask のAssertionを発生させ、参照グラフを壊すことがある。
                if (EditorUtility.IsPersistent(animationClip) && !ctx.IsTemporaryAsset(animationClip))
                {
                    targetClip = new AnimationClip();
                    EditorUtility.CopySerialized(animationClip, targetClip);
                    targetClip.name = animationClip.name;
                    AssetDatabase.AddObjectToAsset(targetClip, ctx.AssetContainer);
                }

                for (int bindingIndex = 0; bindingIndex < rootTranslationBindings.Length; bindingIndex++)
                {
                    var binding = rootTranslationBindings[bindingIndex];
                    var axisScale = animationClip.name.StartsWith(FootHeightOffsetPrefix) || animationClip.name.StartsWith(MovementOffsetPrefix + "Vertical_") ? unitScale.y :
                        animationClip.name.StartsWith(MovementOffsetPrefix + "Forward_") ? unitScale.z :
                        bindingIndex == 0 ? unitScale.x : bindingIndex == 1 ? unitScale.y : unitScale.z;
                    var curve = AnimationUtility.GetEditorCurve(targetClip, binding);
                    if (curve == null)
                    {
                        continue;
                    }

                    var keys = curve.keys;
                    for (int i = 0; i < keys.Length; i++)
                    {
                        keys[i].value *= axisScale;
                        keys[i].inTangent *= axisScale;
                        keys[i].outTangent *= axisScale;
                    }
                    AnimationUtility.SetEditorCurve(targetClip, binding, new AnimationCurve(keys));
                }

                // FootHeight用のlevelオフセット（正規化単位）も実寸を保つよう縮尺を合わせる
                var clipSetting = AnimationUtility.GetAnimationClipSettings(targetClip);
                if (!Mathf.Approximately(clipSetting.level, 0))
                {
                    clipSetting.level *= unitScale.y;
                    AnimationUtility.SetAnimationClipSettings(targetClip, clipSetting);
                }

                // These curves are body-space vertical deltas, not absolute pose positions.
                if (!animationClip.name.StartsWith(FootHeightOffsetPrefix) && !animationClip.name.StartsWith(MovementOffsetPrefix))
                {
                    if (animationClip.name.Contains(RolloverCenteredMarker)) CenterRolloverHead(workingAvatar, targetClip);
                    else RecenterHorizontalRootTranslation(workingAvatar, targetClip, finalUnits);
                }

                fixedMotions[motion] = targetClip;
                return targetClip;
            }

            if (motion is BlendTree blendTree)
            {
                var children = blendTree.children;
                var changed = false;
                for (int i = 0; i < children.Length; i++)
                {
                    var fixedChild = RecalibrateMotion(ctx, children[i].motion, fixedMotions, unitScale, finalUnits, workingAvatar);
                    if (fixedChild != children[i].motion)
                    {
                        children[i].motion = fixedChild;
                        changed = true;
                    }
                }
                if (!changed)
                {
                    fixedMotions[motion] = blendTree;
                    return blendTree;
                }

                var targetTree = blendTree;
                if (EditorUtility.IsPersistent(blendTree) && !ctx.IsTemporaryAsset(blendTree))
                {
                    targetTree = new BlendTree();
                    EditorUtility.CopySerialized(blendTree, targetTree);
                    targetTree.name = blendTree.name;
                    AssetDatabase.AddObjectToAsset(targetTree, ctx.AssetContainer);
                }
                targetTree.children = children;
                fixedMotions[motion] = targetTree;
                return targetTree;
            }

            fixedMotions[motion] = motion;
            return motion;
        }

        // FloorAdjuster適用後の最終Humanoid上でポーズをサンプリングし、
        // 変換前にHead基準で作ったXZ中心を再確定する。元の生成処理と同じく時刻0の
        // Headオフセットを全キーへ一定量だけ反映するため、カーブの動き自体は変えない。
        private static void RecenterHorizontalRootTranslation(
            GameObject workingAvatar, AnimationClip clip, Vector3 finalUnits)
        {
            if (workingAvatar == null || clip == null || !HasValidRootTranslationUnits(finalUnits))
            {
                return;
            }

            try
            {
                var animator = workingAvatar.GetComponent<Animator>();
                var headBone = animator != null && animator.isHuman
                    ? animator.GetBoneTransform(HumanBodyBones.Head)
                    : null;
                if (headBone == null)
                {
                    return;
                }

                clip.SampleAnimation(workingAvatar, 0f);
                workingAvatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                var worldOffset = headBone.position - workingAvatar.transform.position;
                var avatarSpaceOffset =
                    Quaternion.Inverse(workingAvatar.transform.rotation) * worldOffset;
                var offsets = new[]
                {
                    avatarSpaceOffset.x / finalUnits.x,
                    avatarSpaceOffset.z / finalUnits.z
                };
                var propertyNames = new[] { "RootT.x", "RootT.z" };
                for (int axis = 0; axis < propertyNames.Length; axis++)
                {
                    var binding = EditorCurveBinding.FloatCurve(
                        string.Empty, typeof(Animator), propertyNames[axis]);
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    if (curve == null)
                    {
                        continue;
                    }

                    var keys = curve.keys;
                    for (int i = 0; i < keys.Length; i++)
                    {
                        keys[i].value -= offsets[axis];
                    }
                    AnimationUtility.SetEditorCurve(clip, binding, new AnimationCurve(keys));
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError(
                    $"[PosingSystem] 姿勢のHead基準XZ再計算中にエラーが発生しました: {e.Message}\n{e.StackTrace}");
            }
        }

        public static bool HasWarning(PosingSystem posingSystem)
        {
            return GetDiagnosticWarnings(posingSystem, false, null) != PosingSystem.WarningType.None ||
                PosingSystemHierarchyBadge.HasInspectorWarning(posingSystem);
        }

        public static bool ShouldReportPrebuildUpdateWarning(PosingSystem posingSystem)
            => posingSystem != null && !posingSystem.IsWarningIgnored(PosingSystem.WarningType.PrebuildOutOfDate)
                && !string.IsNullOrEmpty(posingSystem.data) && IsPosingSystemDataUpdated(posingSystem);

        public static PosingSystem.WarningType GetVisibleWarnings(PosingSystem posingSystem)
        {
            var warnings = GetDiagnosticWarnings(posingSystem, false, null);
            // This public API also includes the informational Auto-Footsteps troubleshooting note.
            if (posingSystem != null && !posingSystem.IsWarningIgnored(PosingSystem.WarningType.AutoFootsteps) &&
                posingSystem.GetAvatar() is { autoFootsteps: true })
                warnings |= PosingSystem.WarningType.AutoFootsteps;
            return warnings;
        }

        internal static PosingSystem.WarningType GetVisibleWarningsForInspector(PosingSystem posingSystem,
            List<PosingSystemDiagnostics.Issue> issues = null)
            => GetDiagnosticWarnings(posingSystem, true, issues);

        private static PosingSystem.WarningType GetDiagnosticWarnings(PosingSystem posingSystem, bool inspection,
            List<PosingSystemDiagnostics.Issue> issues)
        {
            if (posingSystem == null) return PosingSystem.WarningType.None;
            var warnings = PosingSystem.WarningType.None;
            var avatar = posingSystem.GetAvatar();
            if (avatar != null)
            {
                if (string.IsNullOrEmpty(posingSystem.data)) warnings |= PosingSystem.WarningType.PrebuildNotRun;
                else if (!posingSystem.IsWarningIgnored(PosingSystem.WarningType.PrebuildOutOfDate) &&
                    (inspection ? PosingSystemInspectionCache.IsUpdated(posingSystem) : IsPosingSystemDataUpdated(posingSystem)))
                    warnings |= PosingSystem.WarningType.PrebuildOutOfDate;
            }
            if (!posingSystem.IsWarningIgnored(PosingSystem.WarningType.Configuration) &&
                (issues ?? PosingSystemDiagnostics.Collect(posingSystem)).Any(issue => issue.Severity == MessageType.Warning))
                warnings |= PosingSystem.WarningType.Configuration;
            return warnings & ~posingSystem.ignoredWarnings;
        }

        public static bool HasError(PosingSystem posingSystem)
        {
            if (PosingSystemDiagnostics.Collect(posingSystem).Any(issue => issue.Severity == MessageType.Error)) return true;
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
            var animator = avatar.GetComponent<Animator>();
            if (animator == null || animator.avatar == null)
            {
                return true;
            }
            if (!animator.isHuman)
            {
                return true;
            }
            return false;
        }

        public static bool IsPosingSystemDataUpdated(PosingSystem posingSystem)
        {
            if (posingSystem == null || posingSystem.defines == null ||
                posingSystem.defines.Any(d => d?.animations == null || d.animations.Any(a => a == null))) return true;
            return posingSystem.data != GetDefineSerializeJson(posingSystem, true);
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
                    exitTransition.interruptionSource = TransitionInterruptionSource.SourceThenDestination;
                }
                else if (parameter.type == ParameterSyncType.Int)
                {
                    var exitTransition = state.AddExitTransition(false);
                    exitTransition.AddCondition(AnimatorConditionMode.NotEqual, parameter.value, parameter.name);
                    exitTransition.duration = vrmode ? 0.5f : 0.0f;
                    exitTransition.interruptionSource = TransitionInterruptionSource.SourceThenDestination;
                }
                else
                {
                    var exitTransition1 = state.AddExitTransition(false);
                    exitTransition1.AddCondition(AnimatorConditionMode.Less, parameter.value - 0.01f, parameter.name);
                    exitTransition1.duration = vrmode ? 0.5f : 0.0f;
                    exitTransition1.interruptionSource = TransitionInterruptionSource.SourceThenDestination;

                    var exitTransition2 = state.AddExitTransition(false);
                    exitTransition2.AddCondition(AnimatorConditionMode.Greater, parameter.value + 0.01f, parameter.name);
                    exitTransition2.duration = vrmode ? 0.5f : 0.0f;
                    exitTransition2.interruptionSource = TransitionInterruptionSource.SourceThenDestination;
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

        private static AnimatorController FindDefaultLocomotionController()
        {
            return AssetDatabase.FindAssets("l:USSPS_Locomotion")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<AnimatorController>)
                .FirstOrDefault();
        }

        private static AnimatorController RestoreMissingLocomotionController(ModularAvatarMergeAnimator maMergeAnimator)
        {
            if (!maMergeAnimator || maMergeAnimator.animator)
            {
                return maMergeAnimator != null ? maMergeAnimator.animator as AnimatorController : null;
            }

            var locomotionController = FindDefaultLocomotionController();
            if (locomotionController)
            {
                maMergeAnimator.animator = locomotionController;
                EditorUtility.SetDirty(maMergeAnimator);
            }
            return locomotionController;
        }
        public static ModularAvatarMergeAnimator GetCommonMergeAnimator(PosingSystem posingSystem, bool doRecover = false)
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
                throw PosingSystemDiagnostics.Failure("PS011", "共通ポージングAnimatorにMA Merge Animatorがありません。",
                    "ConsoleにコンパイルエラーやMissing Scriptがないか確認してください。" + PosingSystemDiagnostics.RestoreTool, animatorDuplicateErase);
            }
            if (!maMergeAnimator.animator)
            {
                if (doRecover)
                {
                    // PosingSystemLocomotion.controllerを探して復旧する
                    var locomotionController = RestoreMissingLocomotionController(maMergeAnimator);
                    if (locomotionController)
                    {
                        maMergeAnimator.animator = locomotionController;
                    }
                    else {
                        throw PosingSystemDiagnostics.Failure("PS030", "標準のPosingSystemLocomotion.controllerが見つからず、参照を復元できません。",
                            PosingSystemDiagnostics.RestoreTool, maMergeAnimator);
                    }
                }
                else
                {
                    throw PosingSystemDiagnostics.Failure("PS012", "共通ポージングAnimatorのAnimator参照が未設定です。",
                        "Inspectorの「プレビルドを更新」で標準Controllerからの復元を試してください。" + PosingSystemDiagnostics.RestoreTool, maMergeAnimator);
                }
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
            foreach (var animatorDuplicateErase in avatar.GetComponentsInChildren<DuplicateEraser>()
                         .Where(d => d.gameObject.tag != "EditorOnly")
                         .Where(d => d.ID == "jp.unisakistudio.posingsystem_locomotion"))
            {
                var maMergeAnimator = animatorDuplicateErase.GetComponent<ModularAvatarMergeAnimator>();
                if (!maMergeAnimator)
                {
                    continue;
                }

                // Generated内のControllerが削除されてMissingになった場合は、標準Controllerへ戻す。
                // 標準ControllerはUSSPS_IsDefaultレイヤーを持つため、プレビルド済み候補からは除外される。
                var animatorController = maMergeAnimator != null ? maMergeAnimator.animator as AnimatorController : null;
                if (animatorController == null && !maMergeAnimator.animator)
                {
                    animatorController = RestoreMissingLocomotionController(maMergeAnimator);
                }
                if (animatorController == null)
                {
                    continue;
                }
                if (!animatorController.layers.Any(l => l.name == "USSPS_IsDefault"))
                {
                    return maMergeAnimator;
                }
            }

            return null;
        }

        private static AnimatorController FindDefaultAdditiveBreathingController()
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(DefaultAdditiveBreathingControllerGuid);
            return string.IsNullOrEmpty(assetPath)
                ? null
                : AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
        }

        public static ModularAvatarMergeAnimator GetAdditiveBreathingMergeAnimator(
            PosingSystem posingSystem,
            bool doRecover = false)
        {
            if (posingSystem == null || posingSystem.GetAvatar() == null)
            {
                return null;
            }

            if (doRecover && posingSystem.isEightBitVersion)
            {
                return null;
            }

            var duplicateEraser = posingSystem.GetComponentsInChildren<DuplicateEraser>(true)
                .FirstOrDefault(component =>
                    component.gameObject.tag != "EditorOnly" &&
                    component.ID == AdditiveBreathingDuplicateEraserId);
            // Preparation happens before Synchronize re-enables excluded feature objects.
            // Reuse that same component so its controller is cloned before it becomes active.
            if (duplicateEraser == null && doRecover && PosingSystemFeatureUtility.UsesBreathing(posingSystem))
                duplicateEraser = posingSystem.GetComponentsInChildren<DuplicateEraser>(true)
                    .FirstOrDefault(component => component.ID == AdditiveBreathingDuplicateEraserId);
            ModularAvatarMergeAnimator mergeAnimator = null;
            if (duplicateEraser != null)
            {
                mergeAnimator = duplicateEraser.GetComponent<ModularAvatarMergeAnimator>();
            }
            else if (doRecover)
            {
                var mergeObject = new GameObject("Additive呼吸Animator");
                mergeObject.transform.SetParent(posingSystem.transform, false);
                duplicateEraser = mergeObject.AddComponent<DuplicateEraser>();
                duplicateEraser.ID = AdditiveBreathingDuplicateEraserId;
                mergeAnimator = mergeObject.AddComponent<ModularAvatarMergeAnimator>();
            }

            if (mergeAnimator == null)
            {
                return null;
            }

            if (!mergeAnimator.animator && doRecover)
            {
                mergeAnimator.animator = FindDefaultAdditiveBreathingController();
            }

            mergeAnimator.layerType = VRCAvatarDescriptor.AnimLayerType.Additive;
            mergeAnimator.mergeAnimatorMode = MergeAnimatorMode.Append;
            mergeAnimator.pathMode = MergeAnimatorPathMode.Absolute;
            mergeAnimator.matchAvatarWriteDefaults = true;
            mergeAnimator.deleteAttachedAnimator = false;
            EditorUtility.SetDirty(mergeAnimator);
            return mergeAnimator;
        }

        public static ModularAvatarMergeAnimator GetPrebuiltAdditiveBreathingMergeAnimator(
            PosingSystem posingSystem)
        {
            var avatar = posingSystem != null ? posingSystem.GetAvatar() : null;
            if (avatar == null)
            {
                return null;
            }

            foreach (var duplicateEraser in avatar.GetComponentsInChildren<DuplicateEraser>(true)
                         .Where(component =>
                             component.gameObject.tag != "EditorOnly" &&
                             component.ID == AdditiveBreathingDuplicateEraserId))
            {
                var mergeAnimator = duplicateEraser.GetComponent<ModularAvatarMergeAnimator>();
                var animatorController = mergeAnimator != null
                    ? mergeAnimator.animator as AnimatorController
                    : null;
                if (animatorController == null)
                {
                    continue;
                }

                var guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(animatorController));
                if (guid != DefaultAdditiveBreathingControllerGuid)
                {
                    return mergeAnimator;
                }
            }

            return null;
        }

        public static void ConvertToModularAvatarComponents(PosingSystem posingSystem, IAssetSaver assetSaver = null)
        {
            PosingSystemDiagnostics.ThrowIfInvalid(posingSystem);
            PosingSystemGeneratedControllers.Convert(posingSystem, assetSaver,
                saver => ConvertToModularAvatarComponentsCore(posingSystem, saver));
        }

        private static void ConvertToModularAvatarComponentsCore(PosingSystem posingSystem, IAssetSaver assetSaver)
        {
            PosingSystemDiagnostics.ThrowIfInvalid(posingSystem);
            var avatar = posingSystem.GetAvatar();
            // A partially failed conversion must not retain a previous success marker.
            posingSystem.data = null;
            EditorUtility.SetDirty(posingSystem);

            PosingSystemFeatureUtility.Synchronize(posingSystem);

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

            // Additive呼吸専用MergeAnimatorのControllerも、アバターごとの編集用に複製する
            CreateOriginalAdditiveBreathingAnimatorController(posingSystem);

            // MergeAnimatorのAnimatorControllerのParametersをセットする
            ResetAnimatorControllerParameters(posingSystem);

            // AnimatorControllerを変更する
            ConvertAnimatorController(posingSystem, assetSaver);

            // 姿勢ごとの呼吸アニメーションを設定する
            ConvertAdditiveBreathingAnimations(posingSystem);

            // アニメーションをOverrideする
            ResetImportedMotionStates(posingSystem, assetSaver);
            ConvertOverrideAnimations(posingSystem, assetSaver);

            ConvertOptionalFeatures(posingSystem, assetSaver);

            // アイコン画像ファイルがあったらメニューに設定する
            if (posingSystem.thumbnailPackObject != null)
            {
                SetMenuIcon(posingSystem);
            }

            // 変更を保存
            posingSystem.data = GetDefineSerializeJson(posingSystem);

            PosingSystemHierarchyBadge.Invalidate(posingSystem);

            // PosingSystemオブジェクトをDirtyにしてシーンの保存対象にする
            EditorUtility.SetDirty(posingSystem);
        }

        /// <summary>
        /// UnityオブジェクトのGUIDを取得（instanceIDに依存しない安定した識別子）
        /// </summary>
        private static string GetStableObjectIdentifier(UnityEngine.Object obj)
        {
            PosingSystemInspectionCache.Track(obj);
            if (obj == null) return "";

            string assetPath = AssetDatabase.GetAssetPath(obj);
            if (!string.IsNullOrEmpty(assetPath))
            {
                // GUIDを取得（アセットファイルの場合）
                string guid = AssetDatabase.AssetPathToGUID(assetPath);
                if (!string.IsNullOrEmpty(guid))
                {
                    if (AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out string assetGuid, out long localId))
                        return assetGuid + ":" + localId.ToString(System.Globalization.CultureInfo.InvariantCulture);
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
            var breathing = posingSystem.additiveBreathing;
            var standingBreathing = ResolveAdditiveBreathingClip(
                breathing, PosingSystem.LocomotionTypeGroup.Standing);
            var sitShallowBreathing = ResolveAdditiveBreathingClip(
                breathing, PosingSystem.LocomotionTypeGroup.SitShallow);
            var sitDeepBreathing = ResolveAdditiveBreathingClip(
                breathing, PosingSystem.LocomotionTypeGroup.SitDeep);
            var sitSleepBreathing = ResolveAdditiveBreathingClip(
                breathing, PosingSystem.LocomotionTypeGroup.SitSleep);
            var virtualLoveBreathing = ResolveAdditiveBreathingClip(
                breathing, PosingSystem.LocomotionTypeGroup.VirtualLove);
            var typeFixBreathing = ResolveAdditiveBreathingClip(
                breathing, PosingSystem.LocomotionTypeGroup.TypeFix);


            // Map to DTOs that do NOT have previewImage fields
            var dto = new DefineSerializeRootDTO
            {
                isIconDisabled = posingSystem.isIconDisabled,
                isIconSmall = posingSystem.isIconSmall,
                additiveBreathingLogicVersion = 3,
                // Rebuild unsupported IK poses with their manual adjustment instead of discarding it.
                autoPoseLogicVersion = 4,
                importedMotionLogicVersion = 2,
                generatedAssetLogicVersion = 1,
                controllerLogicVersion = 2, // Desktop one-evaluation is now standard; invalidate alpha.3 prebuilds.
                floatingPhotographyLogicVersion = 1, // Do not reuse a prebuild which samples the displaced photography head.
                autoImportAvatarAnimations = posingSystem.autoImportAvatarAnimations,
                importedMotionInputs = ImportedMotionInputSignature(posingSystem),
                // Invalidate prebuilds generated before per-frame rollover head centering.
                rolloverCenteringLogicVersion = 1,
                rolloverGenerationLogicVersion = 1,
                autoPoseKneeSafetyEnabled = posingSystem.UsesAutoPoseCatalog &&
                    PosingSystemAvatarAdjustmentUtility.GetKneeSafety(posingSystem).Enabled,
                autoPoseMinimumKneeAngle = posingSystem.reverseKneeSafetyAngle,
                autoPoseKneeBaseline = new Vector2(posingSystem.avatarSpecificLeftKneeBendDegrees,
                    posingSystem.avatarSpecificRightKneeBendDegrees),
                featureSettings = FeatureSettingsSignature(posingSystem),
                disableSleepJumpSwitch = posingSystem.disableSleepJumpSwitch,
                rolloverClipGuid = GetStableObjectIdentifier(posingSystem.features?.rolloverAnimation),
                rolloverClipHash = GetAssetContentHash(posingSystem.features?.rolloverAnimation),
                enablePoseIk = posingSystem.enablePoseIk,
                list = new System.Collections.Generic.List<LayerDefineDTO>(),
                overrides = new System.Collections.Generic.List<OverrideAnimationDefineDTO>(),
                additiveBreathing = new AdditiveBreathingSettingsDTO
                {
                    enabled = breathing != null && breathing.enabled,
                    disableDefaultIdle = breathing?.disableDefaultIdle ?? false,
                    standingLevel = breathing?.GetLevel(PosingSystem.LocomotionTypeGroup.Standing)
                        ?? PosingSystem.BreathingLevel.Medium,
                    sitShallowLevel = breathing?.GetLevel(PosingSystem.LocomotionTypeGroup.SitShallow)
                        ?? PosingSystem.BreathingLevel.Medium,
                    sitDeepLevel = breathing?.GetLevel(PosingSystem.LocomotionTypeGroup.SitDeep)
                        ?? PosingSystem.BreathingLevel.Medium,
                    sitSleepLevel = breathing?.GetLevel(PosingSystem.LocomotionTypeGroup.SitSleep)
                        ?? PosingSystem.BreathingLevel.Medium,
                    virtualLoveLevel = breathing?.GetLevel(PosingSystem.LocomotionTypeGroup.VirtualLove)
                        ?? PosingSystem.BreathingLevel.Medium,
                    typeFixLevel = breathing?.GetLevel(PosingSystem.LocomotionTypeGroup.TypeFix)
                        ?? PosingSystem.BreathingLevel.Medium,
                    standingGuid = GetStableObjectIdentifier(standingBreathing),
                    standingHash = GetAssetContentHash(standingBreathing),
                    sitShallowGuid = GetStableObjectIdentifier(sitShallowBreathing),
                    sitShallowHash = GetAssetContentHash(sitShallowBreathing),
                    sitDeepGuid = GetStableObjectIdentifier(sitDeepBreathing),
                    sitDeepHash = GetAssetContentHash(sitDeepBreathing),
                    sitSleepGuid = GetStableObjectIdentifier(sitSleepBreathing),
                    sitSleepHash = GetAssetContentHash(sitSleepBreathing),
                    virtualLoveGuid = GetStableObjectIdentifier(virtualLoveBreathing),
                    virtualLoveHash = GetAssetContentHash(virtualLoveBreathing),
                    typeFixGuid = GetStableObjectIdentifier(typeFixBreathing),
                    typeFixHash = GetAssetContentHash(typeFixBreathing),
                },
                version = VersionName,
                savedInstanceId = ignoreInstanceId ? posingSystem.savedInstanceId : UnityEditor.GlobalObjectId.GetGlobalObjectIdSlow(posingSystem).ToString(),
                autoPoseCatalogGuid = GetStableObjectIdentifier(posingSystem.autoPoseCatalog),
                // Include edits still in memory: dependency hashes alone only see the saved asset.
                // EditorJsonUtility preserves persistent animation references as GUID/fileID.
                autoPoseCatalogHash = GetAssetContentHash(posingSystem.autoPoseCatalog) +
                    (posingSystem.autoPoseCatalog != null
                        ? Hash128.Compute(EditorJsonUtility.ToJson(posingSystem.autoPoseCatalog)).ToString()
                        : string.Empty),
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
                                disablePoseIk = a.disablePoseIk,
                                isRotate = a.isRotate,
                                rotate = a.rotate,
                                isMotionTime = a.isMotionTime,
                                isRollover = a.isRollover,
                                rolloverSideAnimationGuid = GetStableObjectIdentifier(a.rolloverSideAnimation),
                                rolloverSideAnimationHash = GetAssetContentHash(a.rolloverSideAnimation),
                                rolloverContactStrength = a.rolloverContactStrength,
                                motionTimeParamName = a.motionTimeParamName,
                                animationClipGuid = GetStableObjectIdentifier(a.animationClip),
                                adjustmentClipGuid = posingSystem.UsesAutoPoseFor(a)
                                    ? string.Empty
                                    : GetStableObjectIdentifier(a.adjustmentClip),
                                animationClipHash = GetAssetContentHash(a.animationClip),
                                adjustmentClipHash = posingSystem.UsesAutoPoseFor(a)
                                    ? string.Empty
                                    : GetAssetContentHash(a.adjustmentClip),
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
                    if (ov == null) continue;
                    var ovDTO = new OverrideAnimationDefineDTO
                    {
                        enabled = ov.enabled,
                        disablePoseIk = ov.disablePoseIk,
                        isRotate = ov.isRotate,
                        rotate = ov.rotate,
                        isMotionTime = ov.isMotionTime,
                        motionTimeParamName = ov.motionTimeParamName,
                        animationClipGuid = GetStableObjectIdentifier(ov.animationClip),
                        adjustmentClipGuid = posingSystem.UsesAutoPoseFor(ov)
                            ? string.Empty
                            : GetStableObjectIdentifier(ov.adjustmentClip),
                        animationClipHash = GetAssetContentHash(ov.animationClip),
                        adjustmentClipHash = posingSystem.UsesAutoPoseFor(ov)
                            ? string.Empty
                            : GetAssetContentHash(ov.adjustmentClip),
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
            // 他のPosingSystemですでに使われている値だけを記録する。
            // 複製直後などは複数オブジェクトが同じ割当値を持つため、一意キーのDictionaryへ
            // AnimationDefineをAddすると例外になる。重複を許容する集合として扱う。
            var usedTypeParamValues = new Dictionary<string, HashSet<int>>();
            var usedSyncedParamValues = new HashSet<int>();

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
                            if (!usedTypeParamValues.ContainsKey(define.paramName))
                            {
                                usedTypeParamValues.Add(define.paramName, new HashSet<int>());
                            }
                            usedTypeParamValues[define.paramName].Add(animationDefine.typeParameterValue);
                        }
                        // アニメーションの再生される値（同期される値）を記録
                        if (animationDefine.syncdParameterValue > 0)
                        {
                            usedSyncedParamValues.Add(animationDefine.syncdParameterValue);
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
                if (!usedTypeParamValues.ContainsKey(define.paramName))
                {
                    usedTypeParamValues.Add(define.paramName, new HashSet<int>());
                }

                foreach (var animationDefine in define.animations)
                {
                    if (!animationDefine.enabled)
                    {
                        continue;
                    }

                    while (usedTypeParamValues[define.paramName].Contains(typeParamValues[define.paramName]))
                    {
                        typeParamValues[define.paramName] = typeParamValues[define.paramName] + 1;
                    }
                    while (usedSyncedParamValues.Contains(syncedParamValue))
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

        public static int GetMaxSyncedParameterValue(VRCAvatarDescriptor avatar)
        {
            if (avatar == null)
            {
                return 0;
            }

            return avatar.GetComponentsInChildren<PosingSystem>(true)
                .Where(system => system != null && system.defines != null)
                .SelectMany(system => system.defines)
                .Where(define => define != null && define.animations != null)
                .SelectMany(define => define.animations)
                .Where(animation => animation != null && animation.enabled)
                .Select(animation => animation.syncdParameterValue)
                .DefaultIfEmpty(0)
                .Max();
        }

        public static void ResetParametersWithSyncedParameter(PosingSystem posingSystem)
        {
            // アバターのPosingSystemの一番大きなSyncedParameterValueを探す
            var avatar = posingSystem.GetAvatar();
            int maxSyncedParamValue = GetMaxSyncedParameterValue(avatar);

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
                        if (animation.enabled && animation.animationClip != null && animation.initial)
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
                        if (animation.isRollover && animation.previewImage == null)
                            animation.previewImage = PosingSystemRolloverIcon.CopyWithBadge(animation.icon);
                        item.Control.icon = animation.isRollover ? animation.previewImage : animation.icon;
                    }
                }

            }
        }

        public static void CreateOriginalAnimatorController(PosingSystem posingSystem)
            => PosingSystemGeneratedControllers.EnsureBase(posingSystem);

        public static void CreateOriginalAdditiveBreathingAnimatorController(PosingSystem posingSystem)
            => PosingSystemGeneratedControllers.EnsureBreathing(posingSystem);

        public static void ResetAnimatorControllerParameters(PosingSystem posingSystem)
        {
            // 共通ポージングAnimatorを探す
            var maMergeAnimator = GetPrebuiltMergeAnimator(posingSystem);
            if (maMergeAnimator == null)
            {
                maMergeAnimator = GetCommonMergeAnimator(posingSystem);
            }

            // AnimatorControllerを取得
            var animatorController = maMergeAnimator != null ? maMergeAnimator.animator as AnimatorController : null;
            if (animatorController == null)
            {
                Debug.LogError("[PosingSystem]「" + posingSystem.name + "」の共通ポージングAnimatorに設定されているAnimatorControllerが不正です。AnimatorOverrideController等は使用できません");
                throw new System.Exception("「" + posingSystem.name + "」の共通ポージングAnimatorに設定されているAnimatorControllerが不正です。AnimatorOverrideController等は使用できません");
            }

            // アバターのPosingSystemの一番大きなSyncedParameterValueを探す
            var avatar = posingSystem.GetAvatar();
            int maxSyncedParamValue = GetMaxSyncedParameterValue(avatar);

            // 必要なパラメータを列挙
            var parameters = SplitSyncParameters(0, maxSyncedParamValue);

            // ないものがあったら追加する
            animatorController.parameters = animatorController.parameters.Concat(parameters.Where(param => animatorController.parameters.Where(p => param.name == p.name).Count() == 0).Select(param => new AnimatorControllerParameter() { name = param.name, type = param.aType, defaultBool = false, defaultInt = 0, })).ToArray();

            // 変更を記録
            Undo.RecordObject(animatorController, "set animator controller parameters");
            EditorUtility.SetDirty(animatorController);
        }

        public static void ConvertAnimatorController(PosingSystem posingSystem, IAssetSaver assetSaver = null)
        {
            var avatar = posingSystem.GetAvatar();
            if (avatar == null)
            {
                return;
            }

            var waitAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(DefaultEmptyAnimationGuid));
            if (waitAnimation == null)
                throw PosingSystemDiagnostics.Failure("PS031", "ツールに同梱される空アニメーションが見つかりません。", PosingSystemDiagnostics.RestoreTool, posingSystem);

            // 共通ポージングAnimatorを探す
            var maMergeAnimator = GetPrebuiltMergeAnimator(posingSystem);
            if (maMergeAnimator == null)
            {
                maMergeAnimator = GetCommonMergeAnimator(posingSystem);
            }

            // AnimatorControllerを取得
            var animatorController = maMergeAnimator != null ? maMergeAnimator.animator as AnimatorController : null;
            if (animatorController == null)
            {
                Debug.LogError("[PosingSystem]「" + posingSystem.name + "」の共通ポージングAnimatorに設定されているAnimatorControllerが不正です。AnimatorOverrideController等は使用できません");
                throw new System.Exception("「" + posingSystem.name + "」の共通ポージングAnimatorに設定されているAnimatorControllerが不正です。AnimatorOverrideController等は使用できません");
            }

            var srcAvatarAnimator = avatar.GetComponent<Animator>();
            if (srcAvatarAnimator == null || srcAvatarAnimator.avatar == null || !srcAvatarAnimator.isHuman)
            {
                throw PosingSystemDiagnostics.Failure("PS005", "姿勢変換に必要なHumanoid Animatorを取得できません。",
                    "アバターのルートにあるAnimatorのAvatar欄と、元モデルのRig設定を確認してください。", posingSystem);
            }

            // アバターのPosingSystemの一番大きなSyncedParameterValueを探す
            int maxSyncedParamValue = GetMaxSyncedParameterValue(avatar);

            // LocomotionレイヤーとLocomotionTypeレイヤーを取得
            var layer = animatorController.layers.FirstOrDefault(l => l.name == "USSPS_Locomotion");
            if (layer == null || layer.stateMachine == null)
                throw PosingSystemDiagnostics.Failure("PS014", "共通ControllerにUSSPS_Locomotionレイヤーがありません。", PosingSystemDiagnostics.RestoreTool, maMergeAnimator);
            var stateMachine = layer.stateMachine;
            var typeLayer = animatorController.layers.FirstOrDefault(l => l.name == "USSPS_LocomotionType");
            if (typeLayer == null || typeLayer.stateMachine == null)
                throw PosingSystemDiagnostics.Failure("PS014", "共通ControllerにUSSPS_LocomotionTypeレイヤーがありません。", PosingSystemDiagnostics.RestoreTool, maMergeAnimator);
            NormalizeDefaultPoseEncoding(animatorController, assetSaver);

            // 変換は必ず、その時点の処理対象アバターから作った一時クローンで行う。
            // previewAvatarObjectはVRC/NDMFの前処理結果を表示するためのEditorキャッシュであり、
            // NDMFが所有する一時Avatar等を参照することがある。そのキャッシュをビルド入力にすると、
            // 一時アセットの削除後にAnimator.avatarだけMissingになった古いクローンを再利用してしまう。
            // ビルド処理とプレビューのライフサイクルを分離し、プレビューの状態に変換結果を依存させない。
            var workingAvatar = GameObject.Instantiate(avatar.gameObject, PosingSystemEditor.GetPreviewAvatarRoot());
            workingAvatar.name = "_PosingSystem_TempConvertAvatar";
            workingAvatar.hideFlags = HideFlags.HideAndDontSave;

            // 作業用アバターのAnimatorとボーンのnullチェック
            var workingAnimator = workingAvatar.GetComponent<Animator>();
            if (workingAnimator == null || workingAnimator.avatar == null || !workingAnimator.isHuman)
            {
                Object.DestroyImmediate(workingAvatar);
                throw PosingSystemDiagnostics.Failure("PS005", "変換用アバターに有効なHumanoid Animatorがありません。", "元アバターのAnimatorとAvatar参照を確認してください。", posingSystem);
            }
            var headBone = workingAnimator.GetBoneTransform(HumanBodyBones.Head);
            if (headBone == null)
            {
                Object.DestroyImmediate(workingAvatar);
                throw PosingSystemDiagnostics.Failure("PS006", "変換用アバターのHeadボーンが見つかりません。", "元モデルのRig > Configureと、シーンの頭ボーンを確認してください。", posingSystem);
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
            
            try
            {
                workingAvatar.SetActive(true);
                AnimationMode.StartAnimationMode();
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(workingAvatar, baseAnimationClip, 0);
                workingAvatar.transform.position = Vector3.zero;
                workingAvatar.transform.rotation = Quaternion.identity;
                AnimationMode.EndSampling();
                avatarHeightUnit = headBone.position.y / 2;
            }
            catch (System.Exception e)
            {
                Object.DestroyImmediate(workingAvatar);
                throw PosingSystemDiagnostics.Failure("PS034", "アバターの高さ計算に失敗しました。",
                    "アバターのHumanoid設定とHeadボーンを確認してください。詳細はConsoleの内部例外を確認してください。", posingSystem, e);
            }
            finally
            {
                AnimationMode.StopAnimationMode();
                if (workingAvatar != null)
                {
                    workingAvatar.SetActive(false);
                    workingAvatar.SetActive(true);
                }
            }

            // Hipsボーンが足元付近にあるリグやボーンスケール改変アバターでは、humanScale(Hips接地高)と
            // 実測の体格が大きく乖離し、RootTがhumanScale単位で解釈されるため姿勢の高さ・位置がズレる。
            // 乖離を検出した場合はユーザーに警告して高さ調整ツールでの調整を案内する。
            var humanScale = workingAnimator.humanScale;
            if (humanScale > 0 && avatarHeightUnit > 0)
            {
                // 標準的なリグではavatarHeightUnit(頭位置/2)とhumanScaleはおおよそ一致する(比率0.9前後)
                var heightRatio = avatarHeightUnit / humanScale;
                if (heightRatio < 0.6f || heightRatio > 1.5f)
                {
                    Debug.LogWarning($"[PosingSystem] このアバターはHipsボーンの位置またはスケールが標準的なリグと大きく異なります(体格比率: {heightRatio:F2})。姿勢の高さ・位置がズレる場合は、可愛いポーズツールのアニメーション調整機能で高さを一度調整してください。");
                }
            }

            try
            {
                foreach (var define in posingSystem.defines)
                {
                    foreach (var animationDefine in define.animations)
                    {
                        if (!animationDefine.enabled)
                        {
                            continue;
                        }

                        // アニメーションを作成
                        Motion motion = null;
                        if (animationDefine.animationClip is AnimationClip baseClip)
                        {
                            var animationClip = baseClip != null ? baseClip : waitAnimation;
                            if (animationClip == null)
                            {
                                Debug.LogWarning($"[PosingSystem] アニメーションクリップがnullです: {animationDefine.displayName}");
                                continue;
                            }

                            Vector3 headPosision = Vector3.zero;
                            var rootTxList = new List<float>();
                            var rootTyList = new List<float>();
                            var rootTzList = new List<float>();

                            // RootQの各プロパティのカーブを個別に処理（AnimationMode外で取得可能）
                            var tempClipForCurves = Object.Instantiate(animationClip);
                            var srcClipSetting = AnimationUtility.GetAnimationClipSettings(tempClipForCurves);
                            if (srcClipSetting.mirror)
                            {
                                srcClipSetting.mirror = false;
                                AnimationUtility.SetAnimationClipSettings(tempClipForCurves, srcClipSetting);
                            }
                            var rootQxCurve = AnimationUtility.GetEditorCurve(tempClipForCurves, EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.x"));
                            var rootQyCurve = AnimationUtility.GetEditorCurve(tempClipForCurves, EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.y"));
                            var rootQzCurve = AnimationUtility.GetEditorCurve(tempClipForCurves, EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.z"));
                            var rootQwCurve = AnimationUtility.GetEditorCurve(tempClipForCurves, EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.w"));

                            try
                            {
                                AnimationMode.StartAnimationMode();
                                AnimationMode.BeginSampling();
                                AnimationMode.SampleAnimationClip(workingAvatar, animationClip, 0);
                                workingAvatar.transform.LookAt(new Vector3(0, 0, 1));
                                AnimationMode.EndSampling();
                                headPosision = workingAnimator.GetBoneTransform(HumanBodyBones.Head).transform.position;

                                foreach (var binding in AnimationUtility.GetCurveBindings(tempClipForCurves))
                                {
                                    if (!binding.propertyName.StartsWith("RootT."))
                                    {
                                        continue;
                                    }
                                    var curve = AnimationUtility.GetEditorCurve(tempClipForCurves, binding);
                                    if (curve == null) continue;

                                    for (int i = 0; i < curve.keys.Length; i++)
                                    {
                                        AnimationMode.BeginSampling();
                                        AnimationMode.SampleAnimationClip(workingAvatar, animationClip, curve.keys[i].time);
                                        AnimationMode.EndSampling();
                                        var rootT = new Vector3();
                                        var rootBinding = binding;
                                        rootBinding.propertyName = "RootT.x";
                                        AnimationUtility.GetFloatValue(workingAvatar, rootBinding, out rootT.x);
                                        rootBinding.propertyName = "RootT.y";
                                        AnimationUtility.GetFloatValue(workingAvatar, rootBinding, out rootT.y);
                                        rootBinding.propertyName = "RootT.z";
                                        AnimationUtility.GetFloatValue(workingAvatar, rootBinding, out rootT.z);

                                        rootT -= GetHorizontalOffsetInRootUnits(
                                            workingAvatar.transform,
                                            headPosision,
                                            workingAnimator.humanScale);
                                        if (binding.propertyName == "RootT.x")
                                            rootTxList.Add(rootT.x);
                                        if (binding.propertyName == "RootT.y")
                                            rootTyList.Add(rootT.y);
                                        if (binding.propertyName == "RootT.z")
                                            rootTzList.Add(rootT.z);
                                    }
                                }
                            }
                            catch (System.Exception e)
                            {
                                throw PosingSystemDiagnostics.Failure("PS035", $"ポーズ「{define.menuName} / {animationDefine.displayName}」の変換に失敗しました。",
                                    "ポーズ一覧で該当アニメーションと調整クリップを確認してください。切り分ける場合はそのポーズを一時的に無効にして再実行してください。", posingSystem, e);
                            }
                            finally
                            {
                                AnimationMode.StopAnimationMode();
                                workingAvatar.SetActive(false);
                                workingAvatar.SetActive(true);
                            }

                            var newAnimationClip = Object.Instantiate(tempClipForCurves);
                            AnimationClip automaticAdjustmentClip = null;
                            var adjustmentClip = posingSystem.UsesAutoPoseFor(animationDefine)
                                ? null
                                : animationDefine.adjustmentClip;
                            if (posingSystem.UsesAutoPoseFor(animationDefine))
                            {
                                automaticAdjustmentClip = CreateAutomaticPoseAdjustmentClip(
                                    posingSystem,
                                    workingAnimator,
                                    tempClipForCurves,
                                    PosingSystemAutoPoseCatalog.BuildPoseKey(
                                        define.menuName,
                                        animationDefine.displayName));
                                adjustmentClip = automaticAdjustmentClip;
                            }
                            try
                            {
                                ApplyClipAdjustments(posingSystem, workingAvatar, tempClipForCurves,
                                    newAnimationClip, adjustmentClip, true);
                            }
                            finally
                            {
                                if (automaticAdjustmentClip != null) Object.DestroyImmediate(automaticAdjustmentClip);
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
                            ApplyClipRotation(newAnimationClip, animationDefine);

                            // 生成したアニメーションクリップをAnimatorControllerのサブアセットとして永続化
                            newAnimationClip.name = "_USSPS_" + (animationDefine.animationClip != null ? animationDefine.animationClip.name : "Animation") + "_Processed";
                            if (animationDefine.isRollover)
                            {
                                // Compose only after the center pose's own adjustments have been applied.
                                if (animationDefine.rolloverSideAnimation != null)
                                {
                                    var side = PrepareRolloverSide(posingSystem, animationDefine, workingAvatar);
                                    try
                                    {
                                        var generated = PosingSystemRolloverGenerator.Create(workingAvatar,
                                            newAnimationClip, side, animationDefine.rolloverContactStrength);
                                        generated.name = newAnimationClip.name;
                                        Object.DestroyImmediate(newAnimationClip);
                                        newAnimationClip = generated;
                                    }
                                    finally { Object.DestroyImmediate(side); }
                                }
                                // An empty side field preserves existing authored-roll settings.
                                CenterRolloverHead(workingAvatar, newAnimationClip);
                                newAnimationClip.name += RolloverCenteredMarker;
                            }
                            SaveGeneratedAsset(newAnimationClip, animatorController, assetSaver);
                            EditorUtility.SetDirty(animatorController);
                            EditorUtility.SetDirty(newAnimationClip);
                            motion = newAnimationClip;
                        }
                        else if (animationDefine.animationClip.GetType() == typeof(BlendTree))
                        {
                            motion = animationDefine.animationClip;
                            // モーション置き換えで使用されるBlendTree内のパラメータをLocomotion用AnimatorControllerに追加
                            AddBlendTreeParametersToAnimator(animatorController, (BlendTree)motion);
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

                            ConfigurePoseMotionTime(animatorController, state, animationDefine);

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

                            ConfigurePoseMotionTime(animatorController, state, animationDefine);

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
                            typeExitTransition.interruptionSource = TransitionInterruptionSource.SourceThenDestination;

                            // 姿勢が変わった時のExitTransitionを作成（姿勢グループが変わった場合）
                            var typeExitTransition1 = typeState.AddExitTransition(false);
                            typeExitTransition1.AddCondition(AnimatorConditionMode.NotEqual, define.locomotionTypeValue, "LocomotionType");
                            typeExitTransition1.duration = 0.5f;
                            typeExitTransition1.interruptionSource = TransitionInterruptionSource.SourceThenDestination;
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
                        // Rebuilds reuse selector states. Reuse our encoder too, instead of adding
                        // another identical callback on every prebuild. Preserve other behaviours.
                        var existingEncoders = typeState.behaviours.OfType<VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver>()
                            .Where(b => b.parameters.Count == parameters.Count && b.parameters.All(p =>
                                p.type == VRC_AvatarParameterDriver.ChangeType.Set && parameters.Any(v => v.name == p.name))).ToArray();
                        var typeParameterDriver = existingEncoders.FirstOrDefault() ?? typeState.AddStateMachineBehaviour<VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver>();
                        typeState.behaviours = typeState.behaviours.Where(b => b == typeParameterDriver || !existingEncoders.Contains(b)).ToArray();
                        if (typeParameterDriver == null)
                        {
                            Debug.LogError("[PosingSystem]AddStateMachineBehaviourに失敗しました");
                            throw new System.Exception("AddStateMachineBehaviourに失敗しました");
                        }
                        typeParameterDriver.isLocalPlayer = true;
                        typeParameterDriver.localOnly = true;
                        typeParameterDriver.parameters.Clear();
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

                // The comparison backend retains one pose and supplies height from a separate layer.
                var additiveFootHeight = UsesAdditiveFootHeight(avatar.GetComponentsInChildren<PosingSystem>());
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
                        SaveGeneratedAsset(newBlendtree, animatorController, assetSaver);
                        // 子のAnimationClipも永続化
                        foreach (var child in newBlendtree.children)
                        {
                            if (child.motion != null && child.motion.GetType() == typeof(AnimationClip) &&
                                child.motion.name.IndexOf("proxy_") != 0 &&
                                !IsObjectAlreadyInAssetDatabase(child.motion))
                            {
                                SaveGeneratedAsset(child.motion, animatorController, assetSaver);
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
                            // A one-child wrapper is the compact backend. Re-expand it when returning
                            // to ClipLevel, including imported locomotion states reused across prebuilds.
                            if (!additiveFootHeight && state.state.motion is BlendTree compact && compact.children.Length == 1)
                                state.state.motion = compact.children[0].motion;
                            else continue;
                        }
                        if (additiveFootHeight && state.state.motion is AnimationClip)
                        {
                            var source = state.state.motion;
                            if (source is AnimationClip proxy && proxy.name.StartsWith("proxy_")) continue;
                            Motion neutral = Object.Instantiate(source);
                            if (neutral == null) continue;
                            neutral.name = "_USSPS_" + source.name + "_zero";
                            SaveGeneratedAsset(neutral, animatorController, assetSaver);
                            var wrapper = new BlendTree { name = "_USSPS_" + source.name + "_footheight",
                                blendParameter = "FootHeight", useAutomaticThresholds = false };
                            wrapper.AddChild(neutral, .5f);
                            SaveGeneratedAsset(wrapper, animatorController, assetSaver);
                            state.state.motion = wrapper;
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
                                SaveGeneratedAsset(footHeightBlendTree, animatorController, assetSaver);
                                SaveGeneratedAsset(heightAnimationClipZero, animatorController, assetSaver);
                                SaveGeneratedAsset(heightAnimationClip, animatorController, assetSaver);
                                SaveGeneratedAsset(newAnimationClip, animatorController, assetSaver);
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
                            SaveGeneratedAsset(footHeightBlendTree, animatorController, assetSaver);
                            // 子BlendTreeも永続化
                            AddBlendTreeToAsset(footHeightBlendtreeZero, animatorController, assetSaver);
                            AddBlendTreeToAsset(footHeightBlendtreeDown, animatorController, assetSaver);
                            AddBlendTreeToAsset(footHeightBlendtreeUp, animatorController, assetSaver);
                            EditorUtility.SetDirty(animatorController);

                            state.state.motion = footHeightBlendTree;
                        }
                    }
                }
                addFootHeightBlendtree(layer.stateMachine);

                // 古いSubAssetを削除（AnimatorController肥大化防止）
                if (EditorUtility.IsPersistent(animatorController))
                {
                    RemoveOldSubAssets(animatorController);
                }
            }
            finally
            {
                if (workingAvatar != null)
                {
                    Object.DestroyImmediate(workingAvatar);
                }
            }
        }

        public const string AdditiveBreathingLayerName = "USSPS_AdditiveBreathing";
        public const string AdditiveBreathingEnabledParameterName = "USSPS_Breathing";
        public const string AdditiveBreathingPoseParameterName = "USSPS_Pose";
        public const string AdditiveBreathingDisabledStateName = "BreathingOff";
        public const string AdditiveBreathingResetClipName = "_USSPS_AdditiveBreathingReset";

        /// <summary>
        /// 8bit商品版から呼吸用の同期Bool、メニュー、Additive統合をまとめて取り除く。
        /// </summary>
        public static void DisableAdditiveBreathingForEightBit(PosingSystem posingSystem)
        {
            if (posingSystem == null || !posingSystem.isEightBitVersion)
            {
                return;
            }

            if (posingSystem.additiveBreathing != null)
            {
                posingSystem.additiveBreathing.enabled = false;
            }

            foreach (var parameters in posingSystem.GetComponentsInChildren<ModularAvatarParameters>(true))
            {
                parameters.parameters.RemoveAll(parameter =>
                    parameter.nameOrPrefix == AdditiveBreathingEnabledParameterName);
                EditorUtility.SetDirty(parameters);
            }

            foreach (var menuItem in posingSystem.GetComponentsInChildren<ModularAvatarMenuItem>(true)
                         .Where(item =>
                             item.Control != null &&
                             item.Control.parameter != null &&
                             item.Control.parameter.name == AdditiveBreathingEnabledParameterName)
                         .ToArray())
            {
                Object.DestroyImmediate(menuItem.gameObject);
            }

            foreach (var duplicateEraser in posingSystem.GetComponentsInChildren<DuplicateEraser>(true)
                         .Where(component => component.ID == AdditiveBreathingDuplicateEraserId)
                         .ToArray())
            {
                Object.DestroyImmediate(duplicateEraser.gameObject);
            }

            EditorUtility.SetDirty(posingSystem);
        }

        /// <summary>
        /// 同期済みの姿勢番号からLocomotionTypeGroupを復元して切り替わるAdditive呼吸レイヤーを構築する。
        /// 未設定のグループには空クリップを割り当て、前のグループの呼吸が残らないようにする。
        /// </summary>
        public static AnimationClip ResolveAdditiveBreathingClip(
            PosingSystem.AdditiveBreathingSettings settings,
            PosingSystem.LocomotionTypeGroup group)
        {
            if (settings == null)
            {
                return null;
            }

            string clipGuid;
            switch (settings.GetLevel(group))
            {
                case PosingSystem.BreathingLevel.None:
                    return null;
                case PosingSystem.BreathingLevel.Weak:
                    clipGuid = BreathingWeakClipGuid;
                    break;
                case PosingSystem.BreathingLevel.Strong:
                    clipGuid = BreathingStrongClipGuid;
                    break;
                case PosingSystem.BreathingLevel.Custom:
                    return settings.GetCustomClip(group);
                case PosingSystem.BreathingLevel.Rapid:
                    clipGuid = BreathingRapidClipGuid;
                    break;
                case PosingSystem.BreathingLevel.Medium:
                default:
                    clipGuid = BreathingMediumClipGuid;
                    break;
            }

            var clipPath = AssetDatabase.GUIDToAssetPath(clipGuid);
            return string.IsNullOrEmpty(clipPath)
                ? null
                : AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        }

        public static void ConvertAdditiveBreathingAnimations(PosingSystem posingSystem)
        {
            if (posingSystem == null)
            {
                return;
            }

            // 旧版でLocomotion用Controllerへ追加していた呼吸レイヤーは移行時に取り除く。
            var locomotionMergeAnimator = GetPrebuiltMergeAnimator(posingSystem);
            if (locomotionMergeAnimator == null)
            {
                locomotionMergeAnimator = GetCommonMergeAnimator(posingSystem);
            }
            var locomotionController = locomotionMergeAnimator != null
                ? locomotionMergeAnimator.animator as AnimatorController
                : null;
            if (locomotionController != null)
            {
                var locomotionLayers = locomotionController.layers.ToList();
                var legacyLayerIndex = locomotionLayers.FindIndex(
                    candidate => candidate.name == AdditiveBreathingLayerName);
                if (legacyLayerIndex >= 0)
                {
                    locomotionLayers.RemoveAt(legacyLayerIndex);
                    locomotionController.layers = locomotionLayers.ToArray();
                    EditorUtility.SetDirty(locomotionController);
                }
            }

            if (!PosingSystemFeatureUtility.UsesBreathing(posingSystem))
            {
                DisableAdditiveBreathingForEightBit(posingSystem);
                return;
            }

            var maMergeAnimator = GetAdditiveBreathingMergeAnimator(posingSystem, true);
            var animatorController = maMergeAnimator != null
                ? maMergeAnimator.animator as AnimatorController
                : null;
            if (animatorController == null)
            {
                return;
            }

            var layers = animatorController.layers.ToList();
            var layerIndex = layers.FindIndex(candidate => candidate.name == AdditiveBreathingLayerName);
            var settings = posingSystem.additiveBreathing;
            var hasClip = settings != null && System.Enum
                .GetValues(typeof(PosingSystem.LocomotionTypeGroup))
                .Cast<PosingSystem.LocomotionTypeGroup>()
                .Any(group => ResolveAdditiveBreathingClip(settings, group) != null);

            // 以前生成したレイヤーがある場合、無効化設定も生成済みControllerへ反映する。
            if (settings == null || !settings.enabled || !hasClip)
            {
                if (layerIndex >= 0)
                {
                    var disabledLayer = layers[layerIndex];
                    disabledLayer.defaultWeight = 0f;
                    animatorController.layers = new[] { disabledLayer };
                    EditorUtility.SetDirty(animatorController);
                }
                return;
            }

            var resetClip = GetOrCreateAdditiveBreathingResetClip(animatorController, settings);

            // Additive Playable Layerでも他クライアントと同じ呼吸になるよう、
            // 非同期のLocomotionType/LocomotionTypeGroupには依存しない。
            var obsoleteParameterNames = new HashSet<string>
            {
                "LocomotionTypeGroup",
                "LocomotionType",
                "USSPS_TypeFix",
            };
            for (var parameterIndex = animatorController.parameters.Length - 1;
                 parameterIndex >= 0;
                 parameterIndex--)
            {
                if (obsoleteParameterNames.Contains(animatorController.parameters[parameterIndex].name))
                {
                    animatorController.RemoveParameter(parameterIndex);
                }
            }

            var maxSyncedParameterValue = GetMaxSyncedParameterValue(posingSystem.GetAvatar());
            var synchronizedPoseParameters = SplitSyncParameters(0, maxSyncedParameterValue);
            foreach (var synchronizedPoseParameter in synchronizedPoseParameters)
            {
                if (!animatorController.parameters.Any(parameter =>
                        parameter.name == synchronizedPoseParameter.name))
                {
                    animatorController.AddParameter(
                        synchronizedPoseParameter.name,
                        synchronizedPoseParameter.aType);
                }
            }

            if (!animatorController.parameters.Any(parameter =>
                    parameter.name == AdditiveBreathingEnabledParameterName))
            {
                animatorController.AddParameter(
                    AdditiveBreathingEnabledParameterName,
                    AnimatorControllerParameterType.Bool);
            }


            // ゲーム内設定の呼吸トグルは初期状態をONにする。
            var controllerParameters = animatorController.parameters;
            var breathingEnabledParameter = controllerParameters.First(parameter =>
                parameter.name == AdditiveBreathingEnabledParameterName);
            breathingEnabledParameter.defaultBool = true;
            animatorController.parameters = controllerParameters;

            AnimatorControllerLayer breathingLayer;
            if (layerIndex < 0)
            {
                var stateMachine = new AnimatorStateMachine
                {
                    name = AdditiveBreathingLayerName,
                };
                if (EditorUtility.IsPersistent(animatorController))
                {
                    AssetDatabase.AddObjectToAsset(stateMachine, animatorController);
                }

                breathingLayer = new AnimatorControllerLayer
                {
                    name = AdditiveBreathingLayerName,
                    stateMachine = stateMachine,
                };
                animatorController.AddLayer(breathingLayer);
                layers = animatorController.layers.ToList();
                layerIndex = layers.FindIndex(candidate => candidate.name == AdditiveBreathingLayerName);
                breathingLayer = layers[layerIndex];
            }
            else
            {
                breathingLayer = layers[layerIndex];
            }

            breathingLayer.blendingMode = AnimatorLayerBlendingMode.Additive;
            breathingLayer.defaultWeight = 1f;
            breathingLayer.iKPass = false;

            var stateMachineToConfigure = breathingLayer.stateMachine;
            foreach (var transition in stateMachineToConfigure.anyStateTransitions.ToArray())
            {
                stateMachineToConfigure.RemoveAnyStateTransition(transition);
                if (EditorUtility.IsPersistent(transition))
                {
                    Object.DestroyImmediate(transition, true);
                }
            }

            var breathingDisabledState = stateMachineToConfigure.states
                .Select(childState => childState.state)
                .FirstOrDefault(candidate => candidate.name == AdditiveBreathingDisabledStateName);
            if (breathingDisabledState == null)
            {
                breathingDisabledState = stateMachineToConfigure.AddState(
                    AdditiveBreathingDisabledStateName,
                    new Vector3(800f, 0f));
                AddAnimatorSubAssetIfNeeded(breathingDisabledState, animatorController);
            }

            foreach (var oldTransition in breathingDisabledState.transitions.ToArray())
            {
                breathingDisabledState.RemoveTransition(oldTransition);
                if (EditorUtility.IsPersistent(oldTransition))
                {
                    Object.DestroyImmediate(oldTransition, true);
                }
            }

            breathingDisabledState.writeDefaultValues = false;
            breathingDisabledState.motion = resetClip;

            var breathingDisabledTransition =
                stateMachineToConfigure.AddAnyStateTransition(breathingDisabledState);
            AddAnimatorSubAssetIfNeeded(breathingDisabledTransition, animatorController);
            breathingDisabledTransition.hasExitTime = false;
            breathingDisabledTransition.duration = 0.35f;
            breathingDisabledTransition.canTransitionToSelf = false;
            breathingDisabledTransition.interruptionSource =
                TransitionInterruptionSource.SourceThenDestination;
            breathingDisabledTransition.AddCondition(
                AnimatorConditionMode.IfNot,
                0f,
                AdditiveBreathingEnabledParameterName);

            var groups = System.Enum.GetValues(typeof(PosingSystem.LocomotionTypeGroup))
                .Cast<PosingSystem.LocomotionTypeGroup>()
                .ToArray();
            AnimatorState standingState = null;
            var statesByGroup = new Dictionary<PosingSystem.LocomotionTypeGroup, AnimatorState>();
            for (var index = 0; index < groups.Length; index++)
            {
                var group = groups[index];
                var stateName = group.ToString();
                var state = stateMachineToConfigure.states
                    .Select(childState => childState.state)
                    .FirstOrDefault(candidate => candidate.name == stateName);
                if (state == null)
                {
                    state = stateMachineToConfigure.AddState(stateName, new Vector3(500f, index * 70f));
                    AddAnimatorSubAssetIfNeeded(state, animatorController);
                }

                foreach (var oldTransition in state.transitions.ToArray())
                {
                    state.RemoveTransition(oldTransition);
                    if (EditorUtility.IsPersistent(oldTransition))
                    {
                        Object.DestroyImmediate(oldTransition, true);
                    }
                }

                state.writeDefaultValues = false;
                state.motion = ResolveAdditiveBreathingClip(settings, group) ?? resetClip;
                statesByGroup[group] = state;

                if (group == PosingSystem.LocomotionTypeGroup.Standing)
                {
                    standingState = state;
                }
            }

            // USSPS_Poseの0～3は標準姿勢用の予約値。頭の高さ無視中に個別ポーズを
            // 選択していない場合も1（中腰）/2（伏せ）が同期されるため、呼吸側でも扱う。
            // 3は寝姿勢用の予約値として先に確保し、今後Locomotion側が出力しても正しく動かす。
            var defaultPoseGroups = new[]
            {
                new KeyValuePair<int, PosingSystem.LocomotionTypeGroup>(
                    0, PosingSystem.LocomotionTypeGroup.Standing),
                new KeyValuePair<int, PosingSystem.LocomotionTypeGroup>(
                    1, PosingSystem.LocomotionTypeGroup.SitShallow),
                new KeyValuePair<int, PosingSystem.LocomotionTypeGroup>(
                    2, PosingSystem.LocomotionTypeGroup.SitDeep),
                new KeyValuePair<int, PosingSystem.LocomotionTypeGroup>(
                    3, PosingSystem.LocomotionTypeGroup.SitSleep),
            };
            foreach (var defaultPoseGroup in defaultPoseGroups)
            {
                AddSynchronizedPoseBreathingTransition(
                    stateMachineToConfigure,
                    statesByGroup[defaultPoseGroup.Value],
                    defaultPoseGroup.Key,
                    maxSyncedParameterValue,
                    animatorController);
            }

            var configuredPoseValues = new HashSet<int>(defaultPoseGroups.Select(pair => pair.Key));
            if (posingSystem.defines != null)
            {
                foreach (var define in posingSystem.defines)
                {
                    if (define == null || define.animations == null)
                    {
                        continue;
                    }

                    var groupValue = define.locomotionTypeValue / 10;
                    if (!System.Enum.IsDefined(typeof(PosingSystem.LocomotionTypeGroup), groupValue))
                    {
                        continue;
                    }

                    var group = (PosingSystem.LocomotionTypeGroup)groupValue;
                    AnimatorState groupState;
                    if (!statesByGroup.TryGetValue(group, out groupState))
                    {
                        continue;
                    }

                    foreach (var animation in define.animations)
                    {
                        if (animation == null || !animation.enabled ||
                            animation.syncdParameterValue <= 0 ||
                            !configuredPoseValues.Add(animation.syncdParameterValue))
                        {
                            continue;
                        }

                        AddSynchronizedPoseBreathingTransition(
                            stateMachineToConfigure,
                            groupState,
                            animation.syncdParameterValue,
                            maxSyncedParameterValue,
                            animatorController);
                    }
                }
            }

            if (!PosingSystemFeatureUtility.AvatarUsesBreathingControl(posingSystem))
            {
                stateMachineToConfigure.RemoveAnyStateTransition(breathingDisabledTransition);
                if (EditorUtility.IsPersistent(breathingDisabledTransition)) Object.DestroyImmediate(breathingDisabledTransition, true);
                foreach (var transition in stateMachineToConfigure.anyStateTransitions)
                    transition.conditions = transition.conditions.Where(c => c.parameter != AdditiveBreathingEnabledParameterName).ToArray();
                stateMachineToConfigure.RemoveState(breathingDisabledState);
                animatorController.parameters = animatorController.parameters.Where(p => p.name != AdditiveBreathingEnabledParameterName).ToArray();
            }

            stateMachineToConfigure.defaultState = standingState;
            animatorController.layers = new[] { breathingLayer };
            EditorUtility.SetDirty(stateMachineToConfigure);
            EditorUtility.SetDirty(animatorController);
            if (EditorUtility.IsPersistent(animatorController))
            {
                AssetDatabase.SaveAssets();
            }
        }

        private static void AddSynchronizedPoseBreathingTransition(
            AnimatorStateMachine stateMachine,
            AnimatorState destinationState,
            int syncedPoseValue,
            int maxSyncedParameterValue,
            AnimatorController animatorController)
        {
            var transition = stateMachine.AddAnyStateTransition(destinationState);
            AddAnimatorSubAssetIfNeeded(transition, animatorController);
            transition.hasExitTime = false;
            transition.duration = 0.35f;
            transition.canTransitionToSelf = false;
            transition.interruptionSource = TransitionInterruptionSource.SourceThenDestination;
            transition.AddCondition(
                AnimatorConditionMode.If,
                0f,
                AdditiveBreathingEnabledParameterName);

            foreach (var parameter in SplitSyncParameters(syncedPoseValue, maxSyncedParameterValue))
            {
                transition.AddCondition(
                    parameter.type == ParameterSyncType.Bool
                        ? (parameter.value != 0
                            ? AnimatorConditionMode.If
                            : AnimatorConditionMode.IfNot)
                        : AnimatorConditionMode.Equals,
                    parameter.value,
                    parameter.name);
            }
        }

        private static void AddAnimatorSubAssetIfNeeded(
            Object subAsset,
            AnimatorController animatorController)
        {
            if (subAsset != null &&
                EditorUtility.IsPersistent(animatorController) &&
                string.IsNullOrEmpty(AssetDatabase.GetAssetPath(subAsset)))
            {
                AssetDatabase.AddObjectToAsset(subAsset, animatorController);
            }
        }

        private static AnimationClip GetOrCreateAdditiveBreathingResetClip(
            AnimatorController animatorController,
            PosingSystem.AdditiveBreathingSettings settings)
        {
            AnimationClip resetClip = null;
            var controllerPath = AssetDatabase.GetAssetPath(animatorController);
            if (!string.IsNullOrEmpty(controllerPath))
            {
                resetClip = AssetDatabase.LoadAllAssetsAtPath(controllerPath)
                    .OfType<AnimationClip>()
                    .FirstOrDefault(clip => clip.name == AdditiveBreathingResetClipName);
            }

            if (resetClip == null)
            {
                resetClip = animatorController.layers
                    .SelectMany(layer => layer.stateMachine.states)
                    .Select(child => child.state.motion as AnimationClip)
                    .FirstOrDefault(clip =>
                        clip != null && clip.name == AdditiveBreathingResetClipName);
            }

            if (resetClip == null)
            {
                resetClip = new AnimationClip
                {
                    name = AdditiveBreathingResetClipName,
                    frameRate = 60f,
                };
                AddAnimatorSubAssetIfNeeded(resetClip, animatorController);
            }

            foreach (var oldBinding in AnimationUtility.GetCurveBindings(resetClip))
            {
                AnimationUtility.SetEditorCurve(resetClip, oldBinding, null);
            }

            var groups = System.Enum.GetValues(typeof(PosingSystem.LocomotionTypeGroup))
                .Cast<PosingSystem.LocomotionTypeGroup>();
            var bindings = groups
                .Select(group => ResolveAdditiveBreathingClip(settings, group))
                .Where(clip => clip != null)
                .SelectMany(AnimationUtility.GetCurveBindings)
                .Distinct();
            foreach (var binding in bindings)
            {
                AnimationUtility.SetEditorCurve(
                    resetClip,
                    binding,
                    AnimationCurve.Constant(0f, 1f, 0f));
            }

            var clipSettings = AnimationUtility.GetAnimationClipSettings(resetClip);
            clipSettings.loopTime = true;
            AnimationUtility.SetAnimationClipSettings(resetClip, clipSettings);
            EditorUtility.SetDirty(resetClip);
            return resetClip;
        }

        private static Vector3 GetHorizontalOffsetInRootUnits(
            Transform avatarRoot,
            Vector3 worldPosition,
            float humanScale)
        {
            // RootT uses humanScale units. Rotate the world offset into avatar axes while
            // preserving its meter magnitude, then normalize it by the humanoid scale.
            var worldOffset = worldPosition - avatarRoot.position;
            var avatarSpaceOffset = Quaternion.Inverse(avatarRoot.rotation) * worldOffset;
            return new Vector3(avatarSpaceOffset.x, 0f, avatarSpaceOffset.z) / humanScale;
        }

        private static bool HasValidHumanoidAnimator(GameObject avatarObject)
        {
            if (avatarObject == null)
            {
                return false;
            }

            var animator = avatarObject.GetComponent<Animator>();
            return animator != null
                && animator.avatar != null
                && animator.avatar.isValid
                && animator.isHuman;
        }

        private static GameObject CreatePreviewAvatarClone(GameObject sourceAvatar, PosingSystem posingSystem, string purpose)
        {
            var clone = GameObject.Instantiate(sourceAvatar, PosingSystemEditor.GetPreviewAvatarRoot());
            // NDMFはアバター名から一時アセット保存先を決め、同名の保存先を次の処理時に削除する。
            // 同じアバター内に複数のPosingSystemがあっても保存先が衝突しない名前にする。
            clone.name = $"_PosingSystem_{purpose}_{sourceAvatar.GetInstanceID()}_{posingSystem.GetInstanceID()}";
            foreach (var clonePosingSystem in clone.GetComponentsInChildren<PosingSystem>(true))
            {
                clonePosingSystem.gameObject.tag = "EditorOnly";
            }
            PosingSystemEditor.SetPreviewAvatarVisibility(clone, false);
            return clone;
        }

        private static GameObject CreateBuildPipelineAvatarClone(GameObject sourceAvatar, PosingSystem posingSystem)
        {
            // NDMF 1.14.7以降はErrorReport生成時のSceneを保持し、報告画面を開く際にそのSceneを検索する。
            // HideAndDontSave配下のオブジェクトはscene.IsValid() == falseになるため、NDMFへ渡す一時クローンを
            // Editorプレビュー用ルートの下へ置いてはいけない。
            var clone = GameObject.Instantiate(sourceAvatar);
            clone.transform.SetParent(null, true);
            clone.name =
                $"_PosingSystem_ProcessedPreview_{sourceAvatar.GetInstanceID()}_{posingSystem.GetInstanceID()}";

            foreach (var clonePosingSystem in clone.GetComponentsInChildren<PosingSystem>(true))
            {
                clonePosingSystem.gameObject.tag = "EditorOnly";
            }

            // DontSaveは有効なSceneを維持する。HideAndDontSaveはSceneから切り離すため使用しない。
            PosingSystemEditor.SetPreviewAvatarVisibility(clone, true);
            // ルートにHideInHierarchyとDontSaveを同時指定すると、再び無効Sceneへ移動してしまう。
            // 同期処理のfinallyで必ず破棄するため、ルートはHideInHierarchyだけで一時的に隠す。
            clone.hideFlags = HideFlags.HideInHierarchy;
            var sourceScene = sourceAvatar.scene;
            if (sourceScene.IsValid() && sourceScene.isLoaded && clone.scene != sourceScene)
            {
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(clone, sourceScene);
            }

            if (!clone.scene.IsValid())
            {
                Object.DestroyImmediate(clone);
                throw new System.InvalidOperationException(
                    "[PosingSystem] NDMFプレビュー用アバターを有効なSceneに作成できませんでした。");
            }

            return clone;
        }

        /// <summary>
        /// BlendTreeとその子要素を再帰的にAnimatorControllerのサブアセットとして永続化
        /// </summary>
        private static void AddBlendTreeToAsset(BlendTree blendTree, AnimatorController animatorController, IAssetSaver assetSaver)
        {
            if (blendTree == null) return;

            // 既にアセットファイルに追加されていない場合のみ追加
            SaveGeneratedAsset(blendTree, animatorController, assetSaver);

            foreach (var child in blendTree.children)
            {
                if (child.motion == null) continue;

                if (child.motion.GetType() == typeof(BlendTree))
                {
                    AddBlendTreeToAsset((BlendTree)child.motion, animatorController, assetSaver);
                }
                else if (child.motion.GetType() == typeof(AnimationClip) &&
                         child.motion.name.IndexOf("proxy_") != 0 &&
                         !IsObjectAlreadyInAssetDatabase(child.motion))
                {
                    SaveGeneratedAsset(child.motion, animatorController, assetSaver);
                }
            }
        }

        /// <summary>
        /// 生成したMotionを保存する。NDMFビルド中はAnimatorController自体が一時オブジェクトに
        /// なり得るため、AssetDatabaseへ直接追加せずNDMFのAssetSaverへ委譲する。
        /// </summary>
        private static void SaveGeneratedAsset(
            UnityEngine.Object asset,
            AnimatorController animatorController,
            IAssetSaver assetSaver)
        {
            if (asset == null || IsObjectAlreadyInAssetDatabase(asset))
            {
                return;
            }

            if (assetSaver != null)
            {
                assetSaver.SaveAsset(asset);
                return;
            }

            if (animatorController == null || !EditorUtility.IsPersistent(animatorController))
            {
                throw new System.InvalidOperationException(
                    "[PosingSystem] 生成Motionの保存先AnimatorControllerが永続アセットではありません。");
            }

            AssetDatabase.AddObjectToAsset(asset, animatorController);
        }

        /// <summary>
        /// BlendTreeおよびその子BlendTreeで使用しているパラメータをAnimatorControllerに追加する
        /// （元アバターのモーション置き換え時に、LocomotionController側に存在しないパラメータが原因でエラーになるのを防ぐ）
        /// </summary>
        /// <param name="animatorController">パラメータを追加するAnimatorController</param>
        /// <param name="blendTree">参照元のBlendTree</param>
        private static void AddBlendTreeParametersToAnimator(AnimatorController animatorController, BlendTree blendTree)
        {
            if (animatorController == null || blendTree == null) return;

            void AddParameterIfNeeded(string paramName)
            {
                if (string.IsNullOrEmpty(paramName)) return;
                if (animatorController.parameters.Any(p => p.name == paramName)) return;

                animatorController.AddParameter(paramName, AnimatorControllerParameterType.Float);
            }

            // このBlendTree自体が使用しているパラメータを追加
            AddParameterIfNeeded(blendTree.blendParameter);
            AddParameterIfNeeded(blendTree.blendParameterY);

            // 子に含まれるBlendTreeも再帰的に処理
            foreach (var child in blendTree.children)
            {
                if (child.motion is BlendTree childBlendTree)
                {
                    AddBlendTreeParametersToAnimator(animatorController, childBlendTree);
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
        /// テクスチャフォーマットが圧縮フォーマットかどうかをチェック
        /// 圧縮フォーマットのテクスチャにはReadPixelsで書き込めない
        /// </summary>
        private static bool IsCompressedFormat(TextureFormat format)
        {
            switch (format)
            {
                case TextureFormat.DXT1:
                case TextureFormat.DXT1Crunched:
                case TextureFormat.DXT5:
                case TextureFormat.DXT5Crunched:
                case TextureFormat.BC4:
                case TextureFormat.BC5:
                case TextureFormat.BC6H:
                case TextureFormat.BC7:
                case TextureFormat.PVRTC_RGB2:
                case TextureFormat.PVRTC_RGBA2:
                case TextureFormat.PVRTC_RGB4:
                case TextureFormat.PVRTC_RGBA4:
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.ETC2_RGBA8:
                case TextureFormat.ETC_RGB4Crunched:
                case TextureFormat.ETC2_RGBA8Crunched:
                case TextureFormat.ASTC_4x4:
                case TextureFormat.ASTC_5x5:
                case TextureFormat.ASTC_6x6:
                case TextureFormat.ASTC_8x8:
                case TextureFormat.ASTC_10x10:
                case TextureFormat.ASTC_12x12:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// AnimatorControllerから古いPosingSystem関連のSubAssetを削除
        /// </summary>
        private static void RemoveOldSubAssets(AnimatorController animatorController)
        {
            // Only a standalone prebuilt controller owns all sub-assets in its file.
            // NDMF packs multiple controllers and their motions into shared containers;
            // pruning one controller's unused motions there can delete another one's clips.
            if (animatorController == null || !AssetDatabase.IsMainAsset(animatorController) ||
                PosingSystemGeneratedControllers.Metadata(animatorController) != null) return;

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
                if (layer.syncedLayerIndex < 0 || layer.syncedLayerIndex >= animatorController.layers.Length) continue;
                foreach (var state in AllFeatureStates(animatorController.layers[layer.syncedLayerIndex].stateMachine))
                {
                    var motion = layer.GetOverrideMotion(state);
                    if (motion is AnimationClip clip) usedAnimationClips.Add(clip);
                    else if (motion is BlendTree tree) checkBlendTreeAction(tree);
                }
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
            var usingEditorPreview = drypreview && HasValidHumanoidAnimator(posingSystem.previewAvatarObject);
            if (usingEditorPreview)
            {
                clone = posingSystem.previewAvatarObject;
            }
            else
            {
                // フル前処理ではキャッシュを再利用しない。NDMF所有の一時アセットを参照する
                // 前処理済みクローンは、このTakeScreenshot呼び出しの外へ持ち出さない。
                if (posingSystem.previewAvatarObject != null)
                {
                    Object.DestroyImmediate(posingSystem.previewAvatarObject);
                }
                posingSystem.previewAvatarObject = null;
                clone = drypreview
                    ? CreatePreviewAvatarClone(srcAvatar.gameObject, posingSystem, "DryPreview")
                    : CreateBuildPipelineAvatarClone(srcAvatar.gameObject, posingSystem);
                if (!drypreview)
                {
                    try
                    {
                        VRC.SDKBase.Editor.BuildPipeline.VRCBuildPipelineCallbacks.OnPreprocessAvatar(clone);
                        Object.DestroyImmediate(clone.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>());

                        if (!HasValidHumanoidAnimator(clone))
                        {
                            Debug.LogError("[PosingSystem] プレビュー生成後のアバターに有効なHumanoid Avatarがありません。");
                            Object.DestroyImmediate(clone);
                            return;
                        }
                    }
                    catch
                    {
                        if (clone != null)
                        {
                            Object.DestroyImmediate(clone);
                        }
                        posingSystem.previewAvatarObject = null;
                        throw;
                    }
                }
            }

            var camera = PosingSystemConverter.CreateIconCamera();
            var screenshotCompleted = false;

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
                        if (force == false && animation.isCustomIcon && !animation.isRollover)
                        {
                            continue;
                        }

                        TakeIconScreenshot(animation, posingSystem, camera, clone, force);
                        if (animation.previewImage != null && !AssetDatabase.IsSubAsset(animation.previewImage))
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

                    TakeIconScreenshot(animation, posingSystem, camera, clone, force);
                    if (animation.previewImage != null && !AssetDatabase.IsSubAsset(animation.previewImage))
                    {
                        if (posingSystem.thumbnailPackObject != null)
                        {
                            AssetDatabase.AddObjectToAsset(animation.previewImage, posingSystem.thumbnailPackObject);
                        }
                    }
                }
                AssetDatabase.SaveAssets();
                screenshotCompleted = true;
            }
            finally
            {
                RenderTexture.active = null;
                Object.DestroyImmediate(camera.gameObject);

                if (clone != null && usingEditorPreview)
                {
                    clone.layer = 0;
                    clone.tag = "EditorOnly";
                    foreach (var child in clone.GetComponentsInChildren<Transform>())
                    {
                        child.gameObject.layer = clone.layer;
                    }
                    PosingSystemEditor.SetPreviewAvatarVisibility(clone, false);
                }
                else if (clone != null)
                {
                    Object.DestroyImmediate(clone);
                }

                // フル前処理の結果は撮影だけに使い、調整ウィンドウ等が必要とするEditorプレビューは
                // NDMFの一時アセットを一切参照しない元アバターのクローンとして別に作る。
                if (!drypreview && screenshotCompleted && posingSystem != null && posingSystem.previewAvatarObject == null)
                {
                    var editorPreview = CreatePreviewAvatarClone(srcAvatar.gameObject, posingSystem, "EditorPreview");
                    if (HasValidHumanoidAnimator(editorPreview))
                    {
                        posingSystem.previewAvatarObject = editorPreview;
                    }
                    else
                    {
                        Object.DestroyImmediate(editorPreview);
                    }
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
                        controlHash[(posingSystem.defines[i].paramName, animation.typeParameterValue)].icon =
                            animation.isCustomIcon && !animation.isRollover ? animation.icon : animation.previewImage;
                    }
                }
            }
        }

        private static AnimationClip CreateAutomaticPoseAdjustmentClip(
            PosingSystem posingSystem,
            Animator animator,
            AnimationClip baseClip,
            string poseKey)
        {
            if (posingSystem == null || !posingSystem.UsesAutoPoseCatalog)
            {
                return null;
            }

            var definition = PosingSystemAutoPoseAdjustmentUtility.FindDefinition(
                posingSystem,
                poseKey,
                baseClip);
            if (definition == null)
            {
                Debug.LogWarning(
                    $"[PosingSystem] '{poseKey}' のIK自動調整データがありません。従来の調整Animationは使用せず、元の姿勢を出力します。",
                    posingSystem);
                return null;
            }

            if (PosingSystemAutoPoseAdjustmentUtility.TryCreateAdjustmentClip(
                    animator,
                    baseClip,
                    definition,
                    out var adjustmentClip,
                    out _,
                    out var message,
                    PosingSystemAvatarAdjustmentUtility.GetKneeSafety(posingSystem)))
            {
                return adjustmentClip;
            }

            Debug.LogWarning(
                $"[PosingSystem] '{poseKey}' のIK自動調整に失敗しました。従来の調整Animationは使用しません: {message}",
                posingSystem);
            return null;
        }

        private static string FindPoseKey(
            PosingSystem posingSystem,
            PosingSystem.AnimationDefine animation)
        {
            if (posingSystem?.defines != null && animation != null)
            {
                foreach (var layer in posingSystem.defines)
                {
                    if (layer?.animations != null && layer.animations.Any(item => ReferenceEquals(item, animation)))
                    {
                        return PosingSystemAutoPoseCatalog.BuildPoseKey(
                            layer.menuName,
                            animation.displayName);
                    }
                }
            }
            return animation?.displayName ?? string.Empty;
        }

        public static void TakeIconScreenshot(PosingSystem.BaseAnimationDefine animation, PosingSystem posingSystem, Camera camera, GameObject previewAvatarObject, bool force)
        {
            var animationDefine = animation as PosingSystem.AnimationDefine;
            if (force == false && animation.previewImage != null)
            {
                return;
            }
            if (animationDefine != null && animationDefine.isCustomIcon && animationDefine.isRollover)
            {
                animation.previewImage = PosingSystemRolloverIcon.CopyWithBadge(animationDefine.icon);
                return;
            }
            if (force == false && animationDefine != null && animationDefine.isCustomIcon) return;

            // 元のアバターを取得
            var srcAvatar = posingSystem.GetAvatar();
            if (srcAvatar == null)
            {
                Debug.LogWarning("[PosingSystem] スクリーンショット撮影: アバターが見つかりません。");
                return;
            }

            // previewAvatarObjectがあればそれを使用、なければクローンを作成
            GameObject workingAvatar = null;
            bool isTemporaryClone = false;
            
            if (HasValidHumanoidAnimator(previewAvatarObject))
            {
                workingAvatar = previewAvatarObject;
            }
            else
            {
                // 一時的なクローンを作成
                workingAvatar = GameObject.Instantiate(srcAvatar.gameObject, PosingSystemEditor.GetPreviewAvatarRoot());
                workingAvatar.name = "_PosingSystem_TempIconAvatar";
                PosingSystemEditor.SetPreviewAvatarVisibility(workingAvatar, false);
                isTemporaryClone = true;
            }

            // AnimatorとHumanoid Avatarの有効性チェック
            var avatarAnimator = workingAvatar.GetComponent<Animator>();
            if (!HasValidHumanoidAnimator(workingAvatar))
            {
                Debug.LogWarning("[PosingSystem] スクリーンショット撮影: 有効なHumanoid Animatorがありません。");
                if (isTemporaryClone)
                {
                    Object.DestroyImmediate(workingAvatar);
                }
                else if (workingAvatar)
                {
                    PosingSystemEditor.SetPreviewAvatarVisibility(workingAvatar, false);
                }
                return;
            }

            try
            {
                workingAvatar.SetActive(true);
                workingAvatar.layer = PosingSystem.PreviewMask;
                foreach (var child in workingAvatar.GetComponentsInChildren<Transform>())
                {
                    child.gameObject.layer = workingAvatar.layer;
                }

                AnimationMode.StartAnimationMode();
                if (animation.animationClip == null)
                {
                    workingAvatar.transform.position = new Vector3(0, 0, 0);
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
                        AnimationClip automaticAdjustmentClip = null;
                        var selectedAdjustmentClip = posingSystem.UsesAutoPoseFor(animationDefine)
                            ? null
                            : animationDefine?.adjustmentClip;
                        if (posingSystem.UsesAutoPoseFor(animationDefine))
                        {
                            automaticAdjustmentClip = CreateAutomaticPoseAdjustmentClip(
                                posingSystem,
                                avatarAnimator,
                                firstAnimationClip,
                                FindPoseKey(posingSystem, animationDefine));
                            selectedAdjustmentClip = automaticAdjustmentClip;
                        }
                        if (selectedAdjustmentClip != null)
                        {
                            clipToSample = Object.Instantiate(firstAnimationClip);
                            var adjustmentClip = selectedAdjustmentClip;

                            // RootQカーブはQuaternion乗算で合成（元の回転 × 調整の回転）
                            AdditiveCurveUtility.MultiplyRootQCurves(firstAnimationClip, adjustmentClip, clipToSample);

                            // RootQ以外のカーブは従来通り加算
                            foreach (var binding in AnimationUtility.GetCurveBindings(adjustmentClip))
                            {
                                // RootQカーブは既にMultiplyRootQCurvesで処理済みなのでスキップ
                                if (AdditiveCurveUtility.IsRootQBinding(binding))
                                {
                                    continue;
                                }

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

                        workingAvatar.transform.position = new Vector3(0, 0, 0);
                        workingAvatar.transform.LookAt(new Vector3(0, 0, 1));
                        AnimationMode.BeginSampling();
                        AnimationMode.SampleAnimationClip(workingAvatar, clipToSample, 0);
                        AnimationMode.EndSampling();

                        // サムネイルにも自動補正と安全クランプを反映する。
                        PosingSystemAvatarAdjustmentUtility.ApplyToAnimator(
                            posingSystem, avatarAnimator, 0f);

                        // 一時的に作成したクリップを削除
                        if (clipToSample != firstAnimationClip)
                        {
                            Object.DestroyImmediate(clipToSample);
                        }
                        if (automaticAdjustmentClip != null)
                        {
                            Object.DestroyImmediate(automaticAdjustmentClip);
                        }

                        if (animation.isRotate)
                        {
                            workingAvatar.transform.Rotate(0, animation.rotate, 0);
                        }
#if MODULAR_AVATAR
                        foreach (var mergeArmature in workingAvatar.GetComponentsInChildren<ModularAvatarMergeArmature>())
                        {
                            mergeArmature.transform.position = new Vector3(200, 0, 0);
                        }
#endif
                    }

                    // ボーンのnullチェック
                    var headBone = avatarAnimator.GetBoneTransform(HumanBodyBones.Head);
                    var leftFootBone = avatarAnimator.GetBoneTransform(HumanBodyBones.LeftFoot);
                    var hipsBone = avatarAnimator.GetBoneTransform(HumanBodyBones.Hips);

                    if (headBone != null && leftFootBone != null && hipsBone != null)
                    {
                        var cameraHeight = headBone.position.y;
                        var cameraDepth = leftFootBone.position.z;
                        var cameraDepth2 = headBone.position.z;
                        var cameraBase = hipsBone.position.z;
                        var distance = Mathf.Max(Mathf.Abs(cameraDepth - cameraBase), cameraHeight) + 0.5f;
                        camera.transform.position = new Vector3(100 - distance, 1, cameraBase + distance + (cameraDepth + cameraDepth2) * 0.5f / 2);
                        camera.transform.LookAt(new Vector3(100, cameraHeight * 0.5f, (cameraDepth + cameraDepth2) * 0.5f));

                        PosingSystemConverter.SetCameraForAvatar(camera, workingAvatar);
                    }
                    else
                    {
                        Debug.LogWarning("[PosingSystem] スクリーンショット撮影: 必要なボーンが見つかりません。デフォルトのカメラ位置を使用します。");
                    }
                }

                camera.backgroundColor = new Color(0, 0, 0, 0);

                workingAvatar.SetActive(false);
                workingAvatar.SetActive(true);

                var iconSize = posingSystem.isIconSmall ? 64 : 256;

                var renderTexture = new RenderTexture(iconSize, iconSize, 24, RenderTextureFormat.ARGB32);
                camera.targetTexture = renderTexture;
                camera.Render();

                // previewImageがnull、サイズが異なる、または圧縮フォーマットの場合は新規作成
                // （圧縮済みテクスチャにはReadPixelsで書き込めないため）
                bool needsNewTexture = animation.previewImage == null ||
                    animation.previewImage.width != iconSize ||
                    animation.previewImage.height != iconSize ||
                    IsCompressedFormat(animation.previewImage.format);

                if (needsNewTexture)
                {
                    animation.previewImage = new Texture2D(iconSize, iconSize, TextureFormat.ARGB32, false);
                }

                RenderTexture.active = renderTexture;

                // 名前を設定
                if (animation as PosingSystem.AnimationDefine != null)
                {
                    animation.previewImage.name = (animation as PosingSystem.AnimationDefine).displayName;
                }
                else if (animation as PosingSystem.OverrideAnimationDefine != null)
                {
                    animation.previewImage.name = (animation as PosingSystem.OverrideAnimationDefine).stateType.ToString();
                }

                animation.previewImage.ReadPixels(new Rect(0, 0, iconSize, iconSize), 0, 0);
                animation.previewImage.Apply();
                if (animationDefine != null && animationDefine.isRollover) PosingSystemRolloverIcon.Stamp(animation.previewImage);
                animation.previewImage.Compress(true);

                // RenderTextureを解放（順序重要：activeをnull→targetTextureをnull→Release→Destroy）
                RenderTexture.active = null;
                camera.targetTexture = null;
                renderTexture.Release();
                Object.DestroyImmediate(renderTexture);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PosingSystem] スクリーンショット撮影中にエラーが発生しました: {e.Message}\n{e.StackTrace}");
            }
            finally
            {
                AnimationMode.StopAnimationMode();

                // 一時クローンの場合は削除
                if (isTemporaryClone && workingAvatar != null)
                {
                    Object.DestroyImmediate(workingAvatar);
                }
                else if (workingAvatar)
                {
                    PosingSystemEditor.SetPreviewAvatarVisibility(workingAvatar, false);
                }
            }
        }

        /// <summary>
        /// HumanPoseHandlerを使用して、マッスル調整を適用しアニメーションカーブを再ベイクする。
        /// 元のアニメーションと調整アニメーションのバインディングパスの違いを吸収し、
        /// 「元のカーブがあったマッスル + 調整があるマッスル」のみをオプション3形式で保存する。
        /// </summary>
        /// <param name="workingAvatar">作業用アバター（サンプリングに使用）</param>
        /// <param name="baseClip">元のアニメーションクリップ</param>
        /// <param name="adjustmentClip">調整アニメーションクリップ</param>
        /// <param name="resultClip">結果を設定するクリップ（既にbaseClipからコピー済み）</param>
        /// <param name="adjustedMuscleIndices">調整があるマッスルインデックスのセット</param>
        private static void ApplyMuscleAdjustmentsWithRebake(
            GameObject workingAvatar,
            AnimationClip baseClip,
            AnimationClip adjustmentClip,
            AnimationClip resultClip,
            HashSet<int> adjustedMuscleIndices,
            PosingSystem posingSystem)
        {
            if (workingAvatar == null || baseClip == null || adjustmentClip == null || resultClip == null)
            {
                Debug.LogWarning("[PosingSystem] ApplyMuscleAdjustmentsWithRebake: 必要なパラメータがnullです");
                return;
            }

            var animator = workingAvatar.GetComponent<Animator>();
            if (animator == null || animator.avatar == null)
            {
                Debug.LogWarning("[PosingSystem] ApplyMuscleAdjustmentsWithRebake: Animatorまたはアバターがnullです");
                return;
            }

            // 元のアニメーションでマッスルカーブを持つインデックスのセットを取得
            var originalMuscleIndices = AdditiveCurveUtility.GetOriginalMuscleIndices(baseClip);
            
            // 保存対象のインデックスセット = 元のカーブがあるインデックス ∪ 調整があるインデックス
            var targetMuscleIndices = new HashSet<int>(originalMuscleIndices);
            targetMuscleIndices.UnionWith(adjustedMuscleIndices);

            if (targetMuscleIndices.Count == 0)
            {
                return;
            }

            // キーフレーム時刻を収集（元のアニメーションのマッスルカーブから）
            var allKeyTimes = new HashSet<float>();
            foreach (var binding in AnimationUtility.GetCurveBindings(baseClip))
            {
                // RootQ, RootTはスキップ
                if (AdditiveCurveUtility.IsRootQBinding(binding) || AdditiveCurveUtility.IsRootTBinding(binding))
                    continue;
                
                var curve = AnimationUtility.GetEditorCurve(baseClip, binding);
                if (curve != null)
                {
                    foreach (var key in curve.keys)
                    {
                        allKeyTimes.Add(key.time);
                    }
                }
            }

            // 調整クリップからもキーフレーム時刻を収集
            foreach (var binding in AnimationUtility.GetCurveBindings(adjustmentClip))
            {
                if (AdditiveCurveUtility.IsRootQBinding(binding) || AdditiveCurveUtility.IsRootTBinding(binding))
                    continue;
                
                var curve = AnimationUtility.GetEditorCurve(adjustmentClip, binding);
                if (curve != null)
                {
                    foreach (var key in curve.keys)
                    {
                        allKeyTimes.Add(key.time);
                    }
                }
            }

            if (allKeyTimes.Count == 0)
            {
                allKeyTimes.Add(0f); // 最低限time=0を追加
            }

            var sortedKeyTimes = allKeyTimes.OrderBy(t => t).ToList();

            // 各対象マッスルのカーブを作成
            var muscleCurves = new Dictionary<int, AnimationCurve>();
            foreach (var muscleIndex in targetMuscleIndices)
            {
                muscleCurves[muscleIndex] = new AnimationCurve();
            }

            // HumanPoseHandlerを作成
            var humanPoseHandler = new HumanPoseHandler(animator.avatar, animator.transform);
            var humanPose = new HumanPose();

            try
            {
                AnimationMode.StartAnimationMode();

                foreach (var time in sortedKeyTimes)
                {
                    // 元のアニメーションをサンプリング
                    AnimationMode.BeginSampling();
                    AnimationMode.SampleAnimationClip(workingAvatar, baseClip, time);
                    AnimationMode.EndSampling();

                    // HumanPoseを取得
                    humanPoseHandler.GetHumanPose(ref humanPose);

                    // 調整値を取得して加算
                    var adjustments = AdditiveCurveUtility.GetMuscleAdjustmentsAtTime(adjustmentClip, time);
                    foreach (var kvp in adjustments)
                    {
                        int muscleIndex = kvp.Key;
                        float adjustValue = kvp.Value;
                        if (muscleIndex >= 0 && muscleIndex < humanPose.muscles.Length)
                        {
                            humanPose.muscles[muscleIndex] += adjustValue;
                        }
                    }

                    // 全補正の合成後にのみ安全クランプする。
                    PosingSystemAvatarAdjustmentUtility.ClampKnees(posingSystem, ref humanPose);

                    // 対象マッスルの値をカーブに追加
                    foreach (var muscleIndex in targetMuscleIndices)
                    {
                        if (muscleIndex >= 0 && muscleIndex < humanPose.muscles.Length)
                        {
                            float muscleValue = humanPose.muscles[muscleIndex];
                            muscleCurves[muscleIndex].AddKey(new Keyframe(time, muscleValue));
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[PosingSystem] マッスル調整の再ベイク中にエラーが発生しました: {e.Message}\n{e.StackTrace}");
                return;
            }
            finally
            {
                AnimationMode.StopAnimationMode();
                if (workingAvatar != null)
                {
                    workingAvatar.SetActive(false);
                    workingAvatar.SetActive(true);
                }
                humanPoseHandler?.Dispose();
            }

            // 既存のマッスルカーブを削除（対象マッスルに対応するバインディングのみ）
            // CouldBeForTargetMusclesを使用して、非標準形式のバインディング名も認識する
            // 元のバインディング形式を記録しておく（再生時の互換性のため）
            var bindingsToRemove = new List<EditorCurveBinding>();
            var originalBindingNames = new Dictionary<int, string>(); // muscleIndex -> 元のpropertyName
            
            foreach (var binding in AnimationUtility.GetCurveBindings(resultClip))
            {
                // RootQ, RootTはスキップ
                if (AdditiveCurveUtility.IsRootQBinding(binding) || AdditiveCurveUtility.IsRootTBinding(binding))
                    continue;
                
                // マッスルカーブでなければスキップ
                if (!AdditiveCurveUtility.IsMuscleBinding(binding))
                    continue;
                
                // まず通常のマッチングを試みる
                int muscleIndex = AdditiveCurveUtility.GetMuscleIndexFromBinding(binding);
                if (muscleIndex >= 0)
                {
                    if (targetMuscleIndices.Contains(muscleIndex))
                    {
                        bindingsToRemove.Add(binding);
                        // 元のバインディング形式を記録（最初に見つかったものを優先）
                        if (!originalBindingNames.ContainsKey(muscleIndex))
                        {
                            originalBindingNames[muscleIndex] = binding.propertyName;
                        }
                    }
                }
                else
                {
                    // マッチしない場合、CouldBeForTargetMusclesで推測
                    // これにより非標準形式のバインディング（例：「RightHand.Thumb.1 Spread」）も削除対象となる
                    if (AdditiveCurveUtility.CouldBeForTargetMuscles(binding, targetMuscleIndices))
                    {
                        bindingsToRemove.Add(binding);
                        // この場合もマッスルインデックスを推測して記録を試みる
                        int guessedIndex = AdditiveCurveUtility.GuessMuslceIndexFromBindingKeywords(binding, targetMuscleIndices);
                        if (guessedIndex >= 0 && !originalBindingNames.ContainsKey(guessedIndex))
                        {
                            originalBindingNames[guessedIndex] = binding.propertyName;
                        }
                    }
                }
            }

            foreach (var binding in bindingsToRemove)
            {
                AnimationUtility.SetEditorCurve(resultClip, binding, null);
            }

            // 新しいマッスルカーブを設定（元のバインディング形式を保持、なければHumanTrait.MuscleName形式で）
            int setCount = 0;
            foreach (var kvp in muscleCurves)
            {
                int muscleIndex = kvp.Key;
                var curve = kvp.Value;
                
                if (curve.length > 0)
                {
                    // 元のバインディング形式があればそれを使用、なければHumanTrait.MuscleName
                    string propertyName;
                    if (originalBindingNames.TryGetValue(muscleIndex, out var originalName))
                    {
                        propertyName = originalName;
                    }
                    else
                    {
                        propertyName = HumanTrait.MuscleName[muscleIndex];
                    }
                    
                    var binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), propertyName);
                    AnimationUtility.SetEditorCurve(resultClip, binding, curve);
                    setCount++;
                }
            }
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

        public static void ConvertOverrideAnimations(PosingSystem posingSystem, IAssetSaver assetSaver = null)
        {
            // 共通ポージングAnimatorを探す
            var maMergeAnimator = GetPrebuiltMergeAnimator(posingSystem);
            if (maMergeAnimator == null)
            {
                maMergeAnimator = GetCommonMergeAnimator(posingSystem);
            }

            // AnimatorControllerを取得
            var animatorController = maMergeAnimator != null ? maMergeAnimator.animator as AnimatorController : null;
            if (animatorController == null)
            {
                Debug.LogError("[PosingSystem]「" + posingSystem.name + "」の共通ポージングAnimatorに設定されているAnimatorControllerが不正です。AnimatorOverrideController等は使用できません");
                throw new System.Exception("「" + posingSystem.name + "」の共通ポージングAnimatorに設定されているAnimatorControllerが不正です。AnimatorOverrideController等は使用できません");
            }

            foreach (var define in ResolveImportedMotionDefinitions(posingSystem))
            {
                if (define == null || !define.enabled || !IsValidOverrideMotion(define.stateType, define.animationClip))
                {
                    if (define != null && define.enabled && define.animationClip != null)
                    {
                        Debug.LogWarning($"[PosingSystem] 「{define.stateType}」には移動を含むBlendTreeが必要なため、単発AnimationClipによる上書きを適用しません: {define.animationClip.name}");
                    }
                    continue;
                }

                var importedMotion = PrepareImportedMotion(posingSystem, define, animatorController, assetSaver);

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
                        if (animatorState.motion is BlendTree)
                        {
                            // The whole-motion import and controller copy can still reference the source asset.
                            // Own the complete mutable tree before replacing an idle child.
                            var copies = new Dictionary<BlendTree, BlendTree>();
                            BlendTree CopyTree(BlendTree source)
                            {
                                if (copies.TryGetValue(source, out var existing)) return existing;
                                var copy = new BlendTree();
                                EditorUtility.CopySerialized(source, copy);
                                copy.name = "_USSPS_Override_" + source.name;
                                copies.Add(source, copy);
                                var children = copy.children;
                                for (var i = 0; i < children.Length; i++)
                                    if (children[i].motion is BlendTree nested) children[i].motion = CopyTree(nested);
                                copy.children = children;
                                if (assetSaver != null || EditorUtility.IsPersistent(animatorController))
                                    SaveGeneratedAsset(copy, animatorController, assetSaver);
                                return copy;
                            }
                            BlendTree blendTree = CopyTree((BlendTree)animatorState.motion);
                            animatorState.motion = blendTree;
                            var blendTreeIndex = blendTree.children.ToList().FindIndex(child => Mathf.Approximately(child.position.x, overrideSetting.posX) && Mathf.Approximately(child.position.y, overrideSetting.posY));
                            if (blendTreeIndex != -1)
                            {
                                blendTree.RemoveChild(blendTreeIndex);
                            }
                            blendTree.AddChild(importedMotion, new Vector2(overrideSetting.posX, overrideSetting.posY));

                            // override用に指定されたモーションがBlendTreeの場合、そのパラメータもAnimatorControllerに追加する
                            if (importedMotion is BlendTree overrideBlendTree)
                            {
                                AddBlendTreeParametersToAnimator(animatorController, overrideBlendTree);
                            }
                        }
                        else
                        {
                            Debug.LogError(string.Format("[PosingSystem]可愛いポーズツールのPosingOverride機能でBlendTreeのはずのAnimatorStateがBlendTreeではありませんでした。「立ちのみ」の変更を行う場合は「立ち・歩き・走り」にはBlendTreeを設定してください"));
                        }
                    }
                    else
                    {
                        animatorState.motion = importedMotion;
                        ConfigureMotionTime(animatorController, animatorState,
                            define.isMotionTime ? define.motionTimeParamName : "");

                        // 単純置き換えでも、元クリップがBlendTreeの場合はパラメータを追加
                        if (importedMotion is BlendTree overrideBlendTree)
                        {
                            AddBlendTreeParametersToAnimator(animatorController, overrideBlendTree);
                        }
                    }
                }
            }
        }

        internal static bool IsValidOverrideMotion(PosingSystem.OverrideAnimationDefine.AnimationStateType stateType, Motion motion)
        {
            if (motion == null)
            {
                return false;
            }

            switch (stateType)
            {
                case PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun:
                case PosingSystem.OverrideAnimationDefine.AnimationStateType.Crouch:
                case PosingSystem.OverrideAnimationDefine.AnimationStateType.Prone:
                    // これらはステート全体を置き換えるため、単発Clipでは歩行・旋回が失われる。
                    return motion is BlendTree;
                default:
                    return true;
            }
        }

        /// <summary>
        /// アバター固有のモーションを検出する。結果は生成用で、ユーザーの設定を書き換えない。
        /// </summary>
        public static List<PosingSystem.OverrideAnimationDefine> AutoImportAvatarAnimations(PosingSystem posingSystem)
        {
            var result = new List<PosingSystem.OverrideAnimationDefine>();
            var sourceController = GetAutoImportSource(posingSystem);
            if (sourceController == null) return result;
            if (sourceController is not AnimatorController animatorController)
            {
                Debug.LogWarning("[PosingSystem] 元姿勢を取り込めません。アバターのBaseに通常のAnimatorControllerを設定してください。AnimatorOverrideControllerには対応していません。", posingSystem);
                return result;
            }

            var detectSettings = new List<(PosingSystem.OverrideAnimationDefine.AnimationStateType type, string stateName)>()
            {
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun, "stand"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.Crouch, "crouch"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.Prone, "prone"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.Jump, "jump"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortFall, "shortfall"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortFall, "softfall"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortFall, "quickfall"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortLanding, "shortland"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortLanding, "softland"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortLanding, "quickland"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.LongFall, "longfall"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.LongFall, "hardfall"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.LongLanding, "longland"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.LongLanding, "hardland"),
            };

            // 標準名で検出できなかった場合に使う別名(非標準ステート名を持つアバター向けフォールバック)
            var fallbackDetectSettings = new List<(PosingSystem.OverrideAnimationDefine.AnimationStateType type, string stateName)>()
            {
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun, "locomotion"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun, "walkrun"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun, "walk"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun, "run"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun, "move"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun, "movement"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun, "idle"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun, "fly"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun, "float"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun, "hover"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortFall, "fall"),
                (PosingSystem.OverrideAnimationDefine.AnimationStateType.ShortLanding, "land"),
            };

            // Duplicate detections are suppressed in this result only, never in serialized settings.
            var existingTypes = new HashSet<PosingSystem.OverrideAnimationDefine.AnimationStateType>();

            string normalizeStateName(string name)
            {
                return name.ToLowerInvariant().Replace("ing", "").Replace(" ", "").Replace("_", "");
            }

            // 全レイヤーからステートを収集(サブステートマシン含む)
            var allStates = new List<AnimatorState>();
            var visitedMachines = new HashSet<AnimatorStateMachine>();
            void collectStates(AnimatorStateMachine stateMachine)
            {
                if (stateMachine == null || !visitedMachines.Add(stateMachine)) return;
                foreach (var subStateMachine in stateMachine.stateMachines)
                {
                    collectStates(subStateMachine.stateMachine);
                }
                foreach (var state in stateMachine.states)
                {
                    if (state.state != null) allStates.Add(state.state);
                }
            }
            foreach (var layer in animatorController.layers)
            {
                collectStates(layer.stateMachine);
            }

            var importedStates = new HashSet<AnimatorState>();
            void searchOverride(List<(PosingSystem.OverrideAnimationDefine.AnimationStateType type, string stateName)> settings)
            {
                foreach (var state in allStates)
                {
                    if (importedStates.Contains(state))
                    {
                        continue;
                    }
                    var searchName = normalizeStateName(state.name);
                    var detectSettingIndex = settings.FindIndex(setting => setting.stateName == searchName);
                    if (detectSettingIndex == -1)
                    {
                        continue;
                    }
                    var detectSetting = settings[detectSettingIndex];

                    // 既に設定されている種類はスキップ
                    if (existingTypes.Contains(detectSetting.type))
                    {
                        continue;
                    }

                    if (state.motion == null)
                    {
                        continue;
                    }
                    if (!IsValidOverrideMotion(detectSetting.type, state.motion))
                    {
                        continue;
                    }
                    // VRChat SDK同梱の標準モーション(proxy等)はアバター固有モーションではないのでスキップ
                    // ※名前ではなくアセットの場所で判定することで、標準proxy名のままカスタムされたモーションも取り込めるようにする
                    if (IsVRChatBundledMotion(state.motion))
                    {
                        continue;
                    }
                    if (!IsContainHumanoidAnimation(state.motion))
                    {
                        continue;
                    }

                    var define = new PosingSystem.OverrideAnimationDefine();
                    define.stateType = detectSetting.type;
                    define.animationClip = state.motion;
                    result.Add(define);
                    existingTypes.Add(detectSetting.type);
                    importedStates.Add(state);

                    Debug.Log($"[PosingSystem] アバターのモーションを検出: {detectSetting.type} ({state.motion.name})");
                }
            }

            // 1回目: 標準ステート名で検出、2回目: 別名でフォールバック検出
            searchOverride(detectSettings);
            searchOverride(fallbackDetectSettings);

            // アバター固有のモーションを持つのに名前から検出できなかったステートを警告する
            // (名前は既知だが上書き設定済みのためスキップしたステートは対象外)
            var knownStateNames = new HashSet<string>(detectSettings.Concat(fallbackDetectSettings).Select(setting => setting.stateName));
            var undetectedStates = allStates
                .Where(state => !importedStates.Contains(state))
                .Where(state => !knownStateNames.Contains(normalizeStateName(state.name)))
                .Where(state => state.motion != null)
                .Where(state => !IsVRChatBundledMotion(state.motion))
                .Where(state => IsContainHumanoidAnimation(state.motion))
                .ToList();
            if (undetectedStates.Count > 0)
            {
                var stateList = string.Join(", ", undetectedStates.Select(state => $"「{state.name}」({state.motion.name})"));
                Debug.LogWarning($"[PosingSystem] アバター専用モーションの可能性があるステートを自動検出できませんでした: {stateList}\n専用の歩行・ジャンプモーション等が失われる場合は、可愛いポーズツールの「元のアニメーションの上書き設定」で手動で割り当ててください。");
            }
            return result;
        }

        /// <summary>
        /// モーションがVRChat SDKに同梱されている標準モーション(proxyアニメーション等)かチェック
        /// </summary>
        private static bool IsVRChatBundledMotion(Motion motion)
        {
            if (motion == null)
            {
                return false;
            }
            var path = AssetDatabase.GetAssetPath(motion);
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }
            // VPM導入(Packages/com.vrchat.～)と旧unitypackage導入(Assets/VRCSDK/～)の両方を対象にする
            return path.StartsWith("Packages/com.vrchat.") || path.Contains("/VRCSDK/");
        }

        /// <summary>
        /// モーションがHumanoidアニメーションを含むかチェック
        /// </summary>
        private static bool IsContainHumanoidAnimation(Motion motion)
        {
            if (motion == null)
            {
                return false;
            }
            if (motion.GetType() == typeof(AnimationClip))
            {
                if (AnimationUtility.GetCurveBindings((AnimationClip)motion).Any(bind => {
                    return HumanTrait.MuscleName.ToList().IndexOf(bind.propertyName) != -1;
                }))
                {
                    return true;
                }
            }
            if (motion.GetType() == typeof(BlendTree))
            {
                foreach (var child in ((BlendTree)motion).children)
                {
                    if (IsContainHumanoidAnimation(child.motion))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        #region Tracking/Locomotion Control Merge

        class AddStateMachineBehaviourFailedException : System.Exception
        {
        }

        static void MergeTrackingControl(
            VRCAvatarDescriptor avatarDescriptor,
            VirtualControllerContext controllerContext)
        {
            var suffixIndex = 0;
            var trackingTypesList = new List<(string suffix, List<string> trackingTypes)>();
            var avatarTrackingTypes = new List<string>();
            var processedControllers = new HashSet<VirtualAnimatorController>();

            foreach (var animLayer in avatarDescriptor.baseAnimationLayers)
            {
                if (controllerContext.Controllers.TryGetValue(animLayer.type, out var controller) &&
                    controller != null &&
                    processedControllers.Add(controller))
                {
                    avatarTrackingTypes =
                        ReplaceTrackingControlToParameterDriver(controller, suffixIndex.ToString(), avatarTrackingTypes);
                }
            }
            trackingTypesList.Add((suffixIndex.ToString(), avatarTrackingTypes));

            foreach (var mergeAnimator in
                     avatarDescriptor.GetComponentsInChildren<ModularAvatarMergeAnimator>(true))
            {
                if (!controllerContext.Controllers.TryGetValue(mergeAnimator, out var controller) ||
                    controller == null ||
                    !IsContainBehaviour<VRCAnimatorTrackingControl>(controller))
                {
                    continue;
                }

                suffixIndex++;
                var trackingTypes =
                    ReplaceTrackingControlToParameterDriver(controller, suffixIndex.ToString(), new List<string>());
                trackingTypesList.Add((suffixIndex.ToString(), trackingTypes));
            }

            var trackingTypeHash = BuildTypeSuffixMap(trackingTypesList);
            if (trackingTypeHash.Count == 0)
            {
                return;
            }

            var maObject = new GameObject("PosingSystemMergeTrackingControl");
            maObject.transform.SetParent(avatarDescriptor.transform, false);

            var maMergeAnimator = maObject.AddComponent<ModularAvatarMergeAnimator>();
            var animator = new AnimatorController();
            maMergeAnimator.animator = animator;

            var parameters = new List<AnimatorControllerParameter>();
            foreach (var trackingType in trackingTypeHash.Keys)
            {
                foreach (var suffix in trackingTypeHash[trackingType])
                {
                    parameters.Add(new AnimatorControllerParameter
                    {
                        name = trackingType + suffix,
                        type = AnimatorControllerParameterType.Bool,
                        defaultBool = true
                    });
                }

                var layer = new AnimatorControllerLayer
                {
                    name = "Merge_" + trackingType,
                    stateMachine = new AnimatorStateMachine()
                };
                var trackingState = layer.stateMachine.AddState("Tracking", new Vector3(500, 0, 0));
                var animationState = layer.stateMachine.AddState("Animation", new Vector3(500, 60, 0));

                var trackingBehaviour = trackingState.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
                var animationBehaviour = animationState.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
                if (trackingBehaviour == null || animationBehaviour == null)
                {
                    throw new AddStateMachineBehaviourFailedException();
                }

                SetTrackingType(trackingBehaviour, trackingType,
                    VRC_AnimatorTrackingControl.TrackingType.Tracking);
                SetTrackingType(animationBehaviour, trackingType,
                    VRC_AnimatorTrackingControl.TrackingType.Animation);

                layer.stateMachine.AddEntryTransition(trackingState);
                foreach (var suffix in trackingTypeHash[trackingType])
                {
                    var toAnimationTransition = trackingState.AddTransition(animationState, false);
                    toAnimationTransition.duration = 0;
                    toAnimationTransition.hasExitTime = false;
                    toAnimationTransition.AddCondition(
                        AnimatorConditionMode.IfNot, 0, trackingType + suffix);
                }

                var toTrackingTransition = animationState.AddTransition(trackingState, false);
                toTrackingTransition.duration = 0;
                toTrackingTransition.hasExitTime = false;
                foreach (var suffix in trackingTypeHash[trackingType])
                {
                    toTrackingTransition.AddCondition(
                        AnimatorConditionMode.If, 0, trackingType + suffix);
                }

                animator.AddLayer(layer);
            }

            animator.parameters = parameters.ToArray();
            RegisterGeneratedController(controllerContext, maMergeAnimator, animator);
            AddModularAvatarParameters(maObject, parameters);
        }

        static List<string> ReplaceTrackingControlToParameterDriver(
            VirtualAnimatorController animatorController,
            string paramSuffix,
            List<string> trackingTypes)
        {
            foreach (var layer in animatorController.Layers)
            {
                if (layer.StateMachine == null)
                {
                    continue;
                }

                foreach (var state in layer.StateMachine.AllStates())
                {
                    foreach (var trackingControl in
                             state.Behaviours.OfType<VRCAnimatorTrackingControl>().ToArray())
                    {
                        var trackingTargets =
                            new (string name, VRC_AnimatorTrackingControl.TrackingType type)[]
                            {
                                ("trackingHead", trackingControl.trackingHead),
                                ("trackingLeftHand", trackingControl.trackingLeftHand),
                                ("trackingRightHand", trackingControl.trackingRightHand),
                                ("trackingLeftFingers", trackingControl.trackingLeftFingers),
                                ("trackingRightFingers", trackingControl.trackingRightFingers),
                                ("trackingHip", trackingControl.trackingHip),
                                ("trackingLeftFoot", trackingControl.trackingLeftFoot),
                                ("trackingRightFoot", trackingControl.trackingRightFoot)
                            };

                        foreach (var trackingTarget in trackingTargets)
                        {
                            if (trackingTarget.type == VRC_AnimatorTrackingControl.TrackingType.Animation)
                            {
                                AddParameterDriverToState(
                                    state, trackingTarget.name + paramSuffix, false);
                                AddUnique(trackingTypes, trackingTarget.name);
                            }
                            else if (trackingTarget.type == VRC_AnimatorTrackingControl.TrackingType.Tracking)
                            {
                                AddParameterDriverToState(
                                    state, trackingTarget.name + paramSuffix, true);
                                AddUnique(trackingTypes, trackingTarget.name);
                            }
                        }

                        // EyeとMouthは顔トラッキング用途があるため維持する。
                        trackingControl.trackingHead = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                        trackingControl.trackingLeftHand = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                        trackingControl.trackingRightHand = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                        trackingControl.trackingLeftFingers = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                        trackingControl.trackingRightFingers = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                        trackingControl.trackingHip = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                        trackingControl.trackingLeftFoot = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                        trackingControl.trackingRightFoot = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                    }
                }
            }

            AddBoolParameters(animatorController, trackingTypes, paramSuffix);
            return trackingTypes;
        }

        static void MergeLocomotionControl(
            VRCAvatarDescriptor avatarDescriptor,
            VirtualControllerContext controllerContext)
        {
            var suffixIndex = 0;
            var locomotionTypesList = new List<(string suffix, List<string> locomotionTypes)>();
            var avatarLocomotionTypes = new List<string>();
            var processedControllers = new HashSet<VirtualAnimatorController>();

            foreach (var animLayer in avatarDescriptor.baseAnimationLayers)
            {
                if (controllerContext.Controllers.TryGetValue(animLayer.type, out var controller) &&
                    controller != null &&
                    processedControllers.Add(controller))
                {
                    avatarLocomotionTypes =
                        ReplaceLocomotionControlToParameterDriver(
                            controller, suffixIndex.ToString(), avatarLocomotionTypes);
                }
            }
            locomotionTypesList.Add((suffixIndex.ToString(), avatarLocomotionTypes));

            foreach (var mergeAnimator in
                     avatarDescriptor.GetComponentsInChildren<ModularAvatarMergeAnimator>(true))
            {
                if (!controllerContext.Controllers.TryGetValue(mergeAnimator, out var controller) ||
                    controller == null ||
                    !IsContainBehaviour<VRCAnimatorLocomotionControl>(controller))
                {
                    continue;
                }

                suffixIndex++;
                var locomotionTypes =
                    ReplaceLocomotionControlToParameterDriver(
                        controller, suffixIndex.ToString(), new List<string>());
                locomotionTypesList.Add((suffixIndex.ToString(), locomotionTypes));
            }

            var locomotionTypeHash = BuildTypeSuffixMap(locomotionTypesList);
            if (locomotionTypeHash.Count == 0)
            {
                return;
            }

            var maObject = new GameObject("PosingSystemMergeLocomotionControl");
            maObject.transform.SetParent(avatarDescriptor.transform, false);

            var maMergeAnimator = maObject.AddComponent<ModularAvatarMergeAnimator>();
            var animator = new AnimatorController();
            maMergeAnimator.animator = animator;

            var parameters = new List<AnimatorControllerParameter>();
            foreach (var locomotionType in locomotionTypeHash.Keys)
            {
                foreach (var suffix in locomotionTypeHash[locomotionType])
                {
                    parameters.Add(new AnimatorControllerParameter
                    {
                        name = locomotionType + suffix,
                        type = AnimatorControllerParameterType.Bool,
                        defaultBool = true
                    });
                }

                var layer = new AnimatorControllerLayer
                {
                    name = "Merge_" + locomotionType,
                    stateMachine = new AnimatorStateMachine()
                };
                var locomotionEnableState =
                    layer.stateMachine.AddState("LocomotionEnable", new Vector3(500, 0, 0));
                var locomotionDisableState =
                    layer.stateMachine.AddState("LocomotionDisable", new Vector3(500, 60, 0));

                var locomotionEnableBehaviour =
                    locomotionEnableState.AddStateMachineBehaviour<VRCAnimatorLocomotionControl>();
                var locomotionDisableBehaviour =
                    locomotionDisableState.AddStateMachineBehaviour<VRCAnimatorLocomotionControl>();
                if (locomotionEnableBehaviour == null || locomotionDisableBehaviour == null)
                {
                    throw new AddStateMachineBehaviourFailedException();
                }

                locomotionEnableBehaviour.disableLocomotion = false;
                locomotionDisableBehaviour.disableLocomotion = true;

                layer.stateMachine.AddEntryTransition(locomotionEnableState);
                foreach (var suffix in locomotionTypeHash[locomotionType])
                {
                    var toDisableTransition =
                        locomotionEnableState.AddTransition(locomotionDisableState, false);
                    toDisableTransition.duration = 0;
                    toDisableTransition.hasExitTime = false;
                    toDisableTransition.AddCondition(
                        AnimatorConditionMode.IfNot, 0, locomotionType + suffix);
                }

                var toEnableTransition =
                    locomotionDisableState.AddTransition(locomotionEnableState, false);
                toEnableTransition.duration = 0;
                toEnableTransition.hasExitTime = false;
                foreach (var suffix in locomotionTypeHash[locomotionType])
                {
                    toEnableTransition.AddCondition(
                        AnimatorConditionMode.If, 0, locomotionType + suffix);
                }

                animator.AddLayer(layer);
            }

            animator.parameters = parameters.ToArray();
            RegisterGeneratedController(controllerContext, maMergeAnimator, animator);
            AddModularAvatarParameters(maObject, parameters);
        }

        static List<string> ReplaceLocomotionControlToParameterDriver(
            VirtualAnimatorController animatorController,
            string paramSuffix,
            List<string> locomotionTypes)
        {
            foreach (var layer in animatorController.Layers)
            {
                if (layer.StateMachine == null)
                {
                    continue;
                }

                foreach (var state in layer.StateMachine.AllStates())
                {
                    foreach (var locomotionControl in
                             state.Behaviours.OfType<VRCAnimatorLocomotionControl>().ToArray())
                    {
                        AddParameterDriverToState(
                            state, "disableLocomotion" + paramSuffix,
                            !locomotionControl.disableLocomotion);
                        AddUnique(locomotionTypes, "disableLocomotion");
                    }

                    state.Behaviours = state.Behaviours
                        .Where(behaviour => !(behaviour is VRCAnimatorLocomotionControl))
                        .ToImmutableList();
                }
            }

            AddBoolParameters(animatorController, locomotionTypes, paramSuffix);
            return locomotionTypes;
        }

        static void AddParameterDriverToState(
            VirtualState state,
            string paramName,
            bool value)
        {
            var parameterDriver = ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
            if (parameterDriver == null)
            {
                throw new AddStateMachineBehaviourFailedException();
            }

            parameterDriver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                type = VRC_AvatarParameterDriver.ChangeType.Set,
                name = paramName,
                value = value ? 1 : 0
            });
            state.Behaviours = state.Behaviours.Add(parameterDriver);
        }

        static void AddBoolParameters(
            VirtualAnimatorController animatorController,
            IEnumerable<string> typeNames,
            string suffix)
        {
            foreach (var typeName in typeNames)
            {
                var paramName = typeName + suffix;
                if (!animatorController.Parameters.ContainsKey(paramName))
                {
                    animatorController.SetParameter(paramName, new AnimatorControllerParameter
                    {
                        name = paramName,
                        type = AnimatorControllerParameterType.Bool,
                        defaultBool = true
                    });
                }
            }
        }

        static Dictionary<string, List<string>> BuildTypeSuffixMap(
            IEnumerable<(string suffix, List<string> types)> typeLists)
        {
            var result = new Dictionary<string, List<string>>();
            foreach (var typePair in typeLists)
            {
                foreach (var typeName in typePair.types)
                {
                    List<string> suffixes;
                    if (!result.TryGetValue(typeName, out suffixes))
                    {
                        suffixes = new List<string>();
                        result[typeName] = suffixes;
                    }
                    suffixes.Add(typePair.suffix);
                }
            }
            return result;
        }

        static void AddUnique(List<string> values, string value)
        {
            if (!values.Contains(value))
            {
                values.Add(value);
            }
        }

        static void RegisterGeneratedController(
            VirtualControllerContext controllerContext,
            ModularAvatarMergeAnimator mergeAnimator,
            AnimatorController controller)
        {
            controllerContext.Controllers[mergeAnimator] = controllerContext.Clone(controller);
        }

        static void AddModularAvatarParameters(
            GameObject target,
            IEnumerable<AnimatorControllerParameter> parameters)
        {
            var maParameter = target.AddComponent<ModularAvatarParameters>();
            foreach (var parameter in parameters)
            {
                maParameter.parameters.Add(new ParameterConfig
                {
                    nameOrPrefix = parameter.name,
                    saved = false,
                    syncType = ParameterSyncType.NotSynced,
                    defaultValue = 1
                });
            }
        }

        static void SetTrackingType(
            VRCAnimatorTrackingControl behaviour,
            string trackingType,
            VRC_AnimatorTrackingControl.TrackingType value)
        {
            switch (trackingType)
            {
                case "trackingHead": behaviour.trackingHead = value; break;
                case "trackingLeftHand": behaviour.trackingLeftHand = value; break;
                case "trackingRightHand": behaviour.trackingRightHand = value; break;
                case "trackingLeftFingers": behaviour.trackingLeftFingers = value; break;
                case "trackingRightFingers": behaviour.trackingRightFingers = value; break;
                case "trackingHip": behaviour.trackingHip = value; break;
                case "trackingLeftFoot": behaviour.trackingLeftFoot = value; break;
                case "trackingRightFoot": behaviour.trackingRightFoot = value; break;
            }
        }

        static bool IsContainBehaviour<BehaviourType>(VirtualAnimatorController animatorController)
        {
            return animatorController != null &&
                   animatorController.Layers.Any(layer =>
                       layer.StateMachine != null &&
                       layer.StateMachine.AllStates().Any(state =>
                           state.Behaviours.Any(behaviour => behaviour is BehaviourType)));
        }

        #endregion

    }

}

#endif
