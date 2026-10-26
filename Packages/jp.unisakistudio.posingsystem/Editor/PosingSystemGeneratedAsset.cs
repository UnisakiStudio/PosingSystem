#if NDMF && MODULAR_AVATAR
using UnityEngine;
using UnityEditor.Animations;

namespace jp.unisakistudio.posingsystemeditor
{
    // Stored inside the controller file. Legacy/unmarked assets are never cleanup candidates.
    public sealed class PosingSystemGeneratedAsset : ScriptableObject
    {
        public int formatVersion = 1;
        public string ownerKind;
        public string ownerGuid;
        public string ownerGlobalId;
        public string editorSession;
        public int ownerInstanceId;
        public string ownerName;
        public string ownerPath;
        public string assetId;
        public string committedUtc;
        public AnimatorController[] controllers;
        public long[] generatedLocalIds;
    }
}
#endif
