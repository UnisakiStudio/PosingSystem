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
            public List<LazyLoadReference<GameObject>> prefabs;
            public string avatarName;
            public LazyLoadReference<Preset> preset;
            public List<string> prefabsHashes;
            public List<string> prefabsNames;
        }
        // Same serialized object-reference format as the former direct references.
        // Metadata readers must use isSet/instanceID, and only applying a preset uses asset.
        internal static event System.Action Changed;
        private void OnValidate() => Changed?.Invoke();

        [SerializeField]
        public List<PresetDefine> presetDefines = new List<PresetDefine>();
    }
}
