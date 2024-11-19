
#region

using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;
#endregion

namespace jp.unisakistudio.posingsystem
{
    public class PosingSystem : MonoBehaviour, IEditorOnly
    {
        public bool developmentMode = false;
        [HideInInspector]
        public bool isPosingSystemLicensed = false;
        public string settingName;
        public bool isIconDisabled = false;
        [System.Serializable]
        public class AnimationDefine
        {
            public string displayName;
            public bool enabled = true;
            public bool initial = false;
            public bool initialSet = false;
            public bool isRotate = false;
            public int rotate = 0;
            public bool isMotionTime = false;
            public string motionTimeParamName = "";
            public bool isCustomIcon = false;
            public Texture2D icon = null;
            public Motion animationClip;
            public int typeParameterValue;
            public int syncdParameterValue;

            public Texture2D previewImage;
            public Texture2D menuImage;

            public AnimationDefine()
            {
                previewImage = null;
                enabled = true;
                animationClip = null;
                displayName = "";
            }
            public AnimationDefine(AnimationClip _animationClip, string _displayName)
            {
                previewImage = null;
                enabled = true;
                animationClip = _animationClip;
                displayName = _displayName;
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

        public List<LayerDefine> defines = new List<LayerDefine>();

        public Transform SubmenuRoot;

        public string data;
    }
}
