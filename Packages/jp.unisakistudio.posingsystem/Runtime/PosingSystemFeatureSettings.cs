using System;
using UnityEngine;

namespace jp.unisakistudio.posingsystem
{
    /// <summary>Optional features. Existing prefabs retain their original behaviour.</summary>
    [Serializable]
    public sealed class PosingSystemFeatureSettings
    {
        public enum MovementBackend
        {
            [InspectorName("A: Base / 加算レイヤー")] BaseAdditive,
            [InspectorName("B: Additive / Overrideレイヤー")] AdditiveOverride,
            [InspectorName("C: Additive / 加算レイヤー")] AdditiveAdditive,
            [InspectorName("D: Gesture / 加算レイヤー")] GestureAdditive,
            [InspectorName("E: Base / 姿勢に合わせて移動方向を補正")] PoseSpaceAdditive
        }
        public enum FootHeightBackend
        {
            [InspectorName("従来方式：ポーズを高さ別に複製")] ClipLevel,
            [InspectorName("Base内のAdditiveで高さ補正")] PoseSpaceAdditive
        }
        public enum FloatingPoseSpaceMode
        {
            [InspectorName("なし（これまでの比較）")] Disabled,
            [InspectorName("開始時にEnterを1回")] EnterOnce,
            [InspectorName("高さ・姿勢変更時にEnter")] ReenterOnChange,
            [InspectorName("高さ・姿勢変更時にExit → Enter")] ExitThenReenter,
            [InspectorName("毎フレームEnter（比較用）")] EnterEveryFrame,
            [InspectorName("変更時にEnter（待機なし）")] ReenterOnChangeImmediately,
            [InspectorName("高さは待機なし・姿勢は80ms後にEnter")] ReenterHeightImmediately,
            [InspectorName("開始時・高さの操作中だけEnter")] HeightMenuOnly
        }
        public bool mirror = true;
        public bool footHeight = true;
        // Retain the serialized comparison fields and enum values for existing scenes/prefabs.
        // Synchronization and builds normalize them to the finalized implementations.
        [HideInInspector] public FootHeightBackend footHeightBackend = FootHeightBackend.PoseSpaceAdditive;
        public bool breathing = true;
        // Keep existing avatars' menu controls when loading settings saved before these options existed.
        public bool breathingControl = true;
        public bool fingerGestureControl = true;
        public bool faceForward;
        public bool photography;
        public bool photographySynced = true;
        public bool photographyKeepHeadVisible;
        public bool floating;
        public bool rollover;
        public AnimationClip rolloverAnimation;
        [Range(15, 90)] public float rolloverAngle = 60;
        [Range(0, 15)] public float rolloverDeadZone = 3;
        [HideInInspector] public MovementBackend photographyBackend = MovementBackend.PoseSpaceAdditive;
        [HideInInspector] public MovementBackend floatingBackend = MovementBackend.PoseSpaceAdditive;
        [HideInInspector] public FloatingPoseSpaceMode floatingPoseSpace = FloatingPoseSpaceMode.HeightMenuOnly;
        [HideInInspector] public bool movementWriteDefaults = true;

        public void ApplyFinalizedMovementSettings()
        {
            footHeightBackend = FootHeightBackend.PoseSpaceAdditive;
            photographyBackend = MovementBackend.PoseSpaceAdditive;
            floatingBackend = MovementBackend.PoseSpaceAdditive;
            floatingPoseSpace = FloatingPoseSpaceMode.HeightMenuOnly;
        }
    }
}
