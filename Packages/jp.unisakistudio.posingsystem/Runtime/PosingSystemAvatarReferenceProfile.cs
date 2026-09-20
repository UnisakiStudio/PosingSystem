using UnityEngine;

namespace jp.unisakistudio.posingsystem
{
    public sealed class PosingSystemAvatarReferenceProfile : ScriptableObject
    {
        public int profileVersion = 1;
        public string sourceAvatarName;
        public float[] referenceMuscles;
        public float leftKneeBendDegrees;
        public float rightKneeBendDegrees;
        public float leftElbowBendDegrees;
        public float rightElbowBendDegrees;
        public bool IsValid => referenceMuscles != null && referenceMuscles.Length == HumanTrait.MuscleCount;
    }
}
