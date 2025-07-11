using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Presets;

namespace jp.unisakistudio.posingsystemeditor
{
    [CreateAssetMenu(fileName = "PosingSystemPresetDefines", menuName = "UnisakiStudio/PosingSystem Preset Defines")]
    public class PosingSystemPresetDefines : ScriptableObject
    {
        [System.Serializable]
        public class PresetDefine
        {
            public List<GameObject> prefabs;
            public string avatarName;
            public Preset preset;
            public List<string> prefabsHashes;
            public List<string> prefabsNames;
        }
        [SerializeField]
        public List<PresetDefine> presetDefines = new List<PresetDefine>();
    }
}
