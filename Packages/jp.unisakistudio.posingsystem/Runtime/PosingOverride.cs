using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace jp.unisakistudio.posingsystem
{
    public class PosingOverride : MonoBehaviour, IEditorOnly
    {
        [System.Serializable]
        public class OverrideDefine
        {
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
            };
            public AnimationStateType type;
            public Motion animation;
        }

        [HideInInspector]
        public bool ビルド時自動実行 = false;
        [HideInInspector]
        public bool deleteExistingLayer = true;
        [HideInInspector]
        public bool mergeTrackingControl = true;
        [HideInInspector]
        public bool deleteExistingTrackingControl = false;

        [SerializeField]
        public List<OverrideDefine> defines = new List<OverrideDefine>();
    }
}