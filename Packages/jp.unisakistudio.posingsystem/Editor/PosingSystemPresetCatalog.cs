using System;
using System.Collections.Generic;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    // Store strings, not Preset/Prefab references: opening this asset must not load every pose.
    public sealed class PosingSystemPresetCatalog : ScriptableObject
    {
        public int schemaVersion;
        public string packageVersions;
        public int revision;
        public bool hideBundledPresets;
        public List<Entry> entries = new List<Entry>();

        [Serializable]
        public sealed class Entry
        {
            public string sourceGuid;
            public int sourceIndex;
            public string sourcePath;
            public string avatarName;
            public bool official;
            public string presetGuid;
            public long presetLocalId;
            public List<string> toolGuids = new List<string>();
            public List<string> toolNames = new List<string>();
            public List<string> prefabsHashes = new List<string>();
            public List<string> prefabsNames = new List<string>();

            public string Key => sourceGuid + ":" + sourceIndex;

            public bool MatchesTool(string guid, string name)
            {
                return (!string.IsNullOrEmpty(guid) && toolGuids.Contains(guid))
                    || (!string.IsNullOrEmpty(name) && toolNames.Contains(name));
            }
        }
    }
}
