using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRC.SDKBase;

namespace jp.unisakistudio.posingsystem
{
    public class PosingOverride : MonoBehaviour, IEditorOnly
    {

        [HideInInspector]
        public bool ビルド時自動実行 = false;
        [HideInInspector]
        public bool deleteExistingLayer = true;
        [HideInInspector]
        public bool mergeTrackingControl = true;
        [HideInInspector]
        public bool deleteExistingTrackingControl = false;
    }
}