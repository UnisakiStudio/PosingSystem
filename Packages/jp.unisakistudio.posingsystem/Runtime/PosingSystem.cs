
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
        [HideInInspector]
        public bool developmentMode = false;
        public string settingName;
        public bool isIconDisabled = false;
        public const int PreviewMask = 21;

        [HideInInspector]
        public GameObject previewAvatarObject;

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
        }

        [System.Serializable]
        public class AnimationDefine : BaseAnimationDefine
        {
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
