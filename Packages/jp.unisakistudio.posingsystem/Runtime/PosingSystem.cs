
#region

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.PlayerLoop;
using VRC.SDKBase;
#endregion

namespace jp.unisakistudio.posingsystem
{
    public class PosingSystem : MonoBehaviour, IEditorOnly
    {
        public string settingName;
        public bool isIconDisabled = false;
        public bool isIconSmall = false;
        public const int PreviewMask = 21;

        [Serializable]
        public class FeatureMenuIcons
        {
            public Texture2D breathing, rollover, floorHeight;
            public Texture2D photoMode, photoDistance, photoDirection, photoExit;

            public Texture2D Get(string key) => key switch
            {
                "Breathing" => breathing, "Rollover" => rollover, "FloorHeight" => floorHeight,
                "PhotoAssistance" => photoMode, "PhotoDistance" => photoDistance,
                "PhotoDirection" => photoDirection, "PhotoExit" => photoExit, _ => null
            };
        }

        // Product prefab references also cover menus that have not been enabled yet.
        [HideInInspector] public FeatureMenuIcons featureMenuIcons = new();

        // トラッキング機能統合オプション
        [HideInInspector]
        public bool mergeTrackingControl = true;
        
        // アバターの姿勢設定自動インポートオプション
        [HideInInspector]
        public bool autoImportAvatarAnimations = true;

        [Flags]
        public enum WarningType
        {
            None = 0,
            AutoFootsteps = 1 << 0,
            PrebuildNotRun = 1 << 1,
            PrebuildOutOfDate = 1 << 2,
            Configuration = 1 << 3,
            All = AutoFootsteps | PrebuildNotRun | PrebuildOutOfDate | Configuration,
        }

        public enum WarningDisplayPosition { InspectorTop, InspectorBottom }

        // Keep the original field name/flag values so saved preferences from 3.0 are restored.
        // Display preferences are deliberately excluded from the generated animation settings DTO.
        [HideInInspector] public WarningType ignoredWarnings = WarningType.None;
        [HideInInspector] public WarningDisplayPosition warningDisplayPosition;
        [HideInInspector] public bool hideHierarchyWarnings;

        public bool IsWarningIgnored(WarningType warning)
            => warning != WarningType.None && (ignoredWarnings & warning) == warning;

        // エディタ内だけで使う一時プレビュー。Prefab overrideやシーンへ保存すると、
        // 参照だけ失われて非表示のアバターがシーンに残るためシリアライズしない。
        [NonSerialized, HideInInspector]
        public GameObject previewAvatarObject;

        [Header("アバター固有姿勢補正")]
        public bool enableAvatarSpecificAdjustment = true;
        [HideInInspector] public bool avatarSpecificAdjustmentUsageInitialized;
        public bool enableReverseKneeSafetyClamp = true;
        [Range(0f, 5f)] public float reverseKneeSafetyAngle = 0.5f;
        [HideInInspector] public AnimationClip avatarSpecificAdjustmentClip;
        [HideInInspector] public string avatarSpecificAdjustmentSignature;
        [HideInInspector] public float avatarSpecificLeftKneeBendDegrees;
        [HideInInspector] public float avatarSpecificRightKneeBendDegrees;
        public virtual string AvatarReferenceProfileResourceName => string.Empty;

        [Header("自動調整アニメーション制作")]
        [Tooltip("商品開発時に作成した姿勢別IK設定です。設定されている場合、従来の姿勢別 adjustmentClip は使用しません。")]
        public PosingSystemAutoPoseCatalog autoPoseCatalog;

        [HideInInspector] public bool enablePoseIk = true;
        public bool UsesAutoPoseCatalog => enablePoseIk && autoPoseCatalog != null && autoPoseCatalog.productionReady;
        public bool UsesAutoPoseFor(BaseAnimationDefine pose) => UsesAutoPoseCatalog && pose != null && !pose.disablePoseIk;

        public enum LocomotionTypeGroup
        {
            [InspectorName("立ち")]
            Standing = 0,
            [InspectorName("浅座り")]
            SitShallow = 1,
            [InspectorName("深座り")]
            SitDeep = 2,
            [InspectorName("寝姿勢")]
            SitSleep = 3,
            [InspectorName("VirtualLove")]
            VirtualLove = 4,
            [InspectorName("姿勢補正中")]
            TypeFix = 5,
        }

        public enum BreathingLevel
        {
            Medium = 0,
            None = 1,
            Weak = 2,
            Strong = 3,
            Custom = 4,
            Rapid = 5,
        }

        [System.Serializable]
        public class AdditiveBreathingSettings
        {
            public bool enabled = true;
            // Independent of the optional breathing animation and its in-game parameter.
            public bool disableDefaultIdle = false;
            public BreathingLevel standingLevel = BreathingLevel.Medium;
            public BreathingLevel sitShallowLevel = BreathingLevel.Medium;
            public BreathingLevel sitDeepLevel = BreathingLevel.Medium;
            public BreathingLevel sitSleepLevel = BreathingLevel.Medium;
            public BreathingLevel virtualLoveLevel = BreathingLevel.Medium;
            public BreathingLevel typeFixLevel = BreathingLevel.Medium;

            [InspectorName("立ち")]
            public AnimationClip standing;
            [InspectorName("浅座り")]
            public AnimationClip sitShallow;
            [InspectorName("深座り")]
            public AnimationClip sitDeep;
            [InspectorName("寝姿勢")]
            public AnimationClip sitSleep;
            [InspectorName("VirtualLove")]
            public AnimationClip virtualLove;
            [InspectorName("姿勢補正中")]
            public AnimationClip typeFix;

            public BreathingLevel GetLevel(LocomotionTypeGroup group)
            {
                switch (group)
                {
                    case LocomotionTypeGroup.Standing:
                        return standingLevel;
                    case LocomotionTypeGroup.SitShallow:
                        return sitShallowLevel;
                    case LocomotionTypeGroup.SitDeep:
                        return sitDeepLevel;
                    case LocomotionTypeGroup.SitSleep:
                        return sitSleepLevel;
                    case LocomotionTypeGroup.VirtualLove:
                        return virtualLoveLevel;
                    case LocomotionTypeGroup.TypeFix:
                        return typeFixLevel;
                    default:
                        return BreathingLevel.Medium;
                }
            }

            public AnimationClip GetCustomClip(LocomotionTypeGroup group)
            {
                switch (group)
                {
                    case LocomotionTypeGroup.Standing:
                        return standing;
                    case LocomotionTypeGroup.SitShallow:
                        return sitShallow;
                    case LocomotionTypeGroup.SitDeep:
                        return sitDeep;
                    case LocomotionTypeGroup.SitSleep:
                        return sitSleep;
                    case LocomotionTypeGroup.VirtualLove:
                        return virtualLove;
                    case LocomotionTypeGroup.TypeFix:
                        return typeFix;
                    default:
                        return null;
                }
            }
        }

        [Header("呼吸機能")]
        public AdditiveBreathingSettings additiveBreathing = new();

        // 同期パラメーターを8bit以内に収める商品版では、呼吸用Boolも含めて完全に除外する。
        // 商品Prefab側に明示的に記録し、名前やアセットパスには依存しない。
        [HideInInspector]
        public bool isEightBitVersion = false;

        [HideInInspector] public PosingSystemFeatureSettings features = new();
        [HideInInspector] public bool featureSettingsMigrated;

        [System.Serializable]
        public class BaseAnimationDefine
        {
            public bool enabled = true;
            public bool isRotate = false;
            public int rotate = 0;
            public bool isMotionTime = false;
            public string motionTimeParamName = "";
            public Motion animationClip;
            public Texture2D previewImage;
            public AnimationClip adjustmentClip;
            // Per-avatar override; never changes the shared product IK catalog.
            public bool disablePoseIk;
        }

        [System.Serializable]
        public class AnimationDefine : BaseAnimationDefine
        {
            // The normal animation is the center pose; the side clip is sampled at its first frame.
            public bool isRollover = false;
            public AnimationClip rolloverSideAnimation;
            [Range(0, 1)] public float rolloverContactStrength = .25f;
            public string displayName;
            public bool initial = false;
            public bool initialSet = false;
            public bool isCustomIcon = false;
            public Texture2D icon = null;
            public int typeParameterValue;
            public int syncdParameterValue;

            public AnimationDefine()
            {
                previewImage = null;
                enabled = true;
                animationClip = null;
                adjustmentClip = null;
                displayName = "";
            }
            public AnimationDefine(AnimationClip _animationClip, string _displayName)
            {
                previewImage = null;
                enabled = true;
                animationClip = _animationClip;
                adjustmentClip = null;
                displayName = _displayName;
            }

        }

        [System.Serializable]
        public class OverrideAnimationDefine : BaseAnimationDefine
        {
            [System.Serializable]
            public enum AnimationStateType
            {
                [InspectorName("立ち・歩き・走り")]
                StandWalkRun,
                [InspectorName("立ちのみ")]
                Stand,
                [InspectorName("しゃがみ（Crouching）")]
                Crouch,
                [InspectorName("伏せ（Prone）")]
                Prone,
                [InspectorName("ジャンプ")]
                Jump,
                [InspectorName("落下（短め）")]
                ShortFall,
                [InspectorName("落下着地（短め）")]
                ShortLanding,
                [InspectorName("落下（長め）")]
                LongFall,
                [InspectorName("落下着地（長め）")]
                LongLanding,
                [InspectorName("アバター選択")]
                AvatarSelect,
            };
            public AnimationStateType stateType;

            public OverrideAnimationDefine()
            {
                previewImage = null;
                enabled = true;
                animationClip = null;
            }
            public OverrideAnimationDefine(AnimationClip _animationClip, string _displayName)
            {
                previewImage = null;
                enabled = true;
                animationClip = _animationClip;
            }

        }

        [System.Serializable]
        public class LayerDefine
        {
            public string menuName;
            public string description;
            public string stateMachineName;
            public string paramName;
            public Texture2D icon;
            public int locomotionTypeValue;
            public List<AnimationDefine> animations;
            public LayerDefine(string _description, string _stateMachineName, string _paramName, int _locomotionTypeValue, string _menuName, List<AnimationDefine> _animations)
            {
                description = _description;
                stateMachineName = _stateMachineName;
                paramName = _paramName;
                locomotionTypeValue = _locomotionTypeValue;
                menuName = _menuName;
                animations = _animations;
            }

        }

        public List<LayerDefine> defines = new();
        public List<OverrideAnimationDefine> overrideDefines = null;

        public Transform SubmenuRoot;

        public string data;

        public string savedInstanceId = "";
        public UnityEngine.Object thumbnailPackObject = null;

        // Project-wide editor catalog; all avatars reference the same persisted metadata.
        [HideInInspector]
        public UnityEngine.Object presetCatalogObject;

        [NonSerialized]
        public bool isWarning = false;
        [NonSerialized]
        public bool isError = false;
        [NonSerialized]
        public DateTime previousErrorCheckTime = DateTime.MinValue;

        public VRC.SDK3.Avatars.Components.VRCAvatarDescriptor GetAvatar()
        {
            VRC.SDK3.Avatars.Components.VRCAvatarDescriptor avatar = null;
            var parentSearch = transform;
            while (parentSearch != null)
            {
                if (parentSearch.TryGetComponent(out avatar))
                {
                    break;
                }
                parentSearch = parentSearch.parent;
            }
            return avatar;
        }
    }
}
