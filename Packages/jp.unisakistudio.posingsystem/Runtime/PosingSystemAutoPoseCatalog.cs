using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace jp.unisakistudio.posingsystem
{
    /// <summary>
    /// Product-authored IK instructions used to create an avatar-specific adjustment animation.
    /// One catalog is normally referenced by one product prefab.
    /// </summary>
    [CreateAssetMenu(
        fileName = "自動姿勢IK設定",
        menuName = "ゆにさきスタジオ/ポーズシステム/自動姿勢IK設定")]
    public sealed class PosingSystemAutoPoseCatalog : ScriptableObject
    {
        public const int CurrentFormatVersion = 2;

        [InspectorName("データ形式のバージョン")]
        [Tooltip("内部的な互換性管理に使います。通常は変更しません。")]
        public int formatVersion = CurrentFormatVersion;

        [InspectorName("商品名")]
        [Tooltip("このIK設定を使用する商品名です。例: 可愛いポーズツール")]
        public string productName;

        [InspectorName("公開用として有効")]
        [Tooltip("有効にすると、このカタログを参照するポーズでは従来の調整Animationを無視してIK自動調整を使用します。")]
        public bool productionReady;

        [InspectorName("姿勢別IK設定一覧")]
        [Tooltip("商品に収録する姿勢ごとのIK設定です。アバターごとのプリセットではありません。")]
        public List<PosingSystemAutoPoseDefinition> poses = new();

        public PosingSystemAutoPoseDefinition Find(string poseKey, AnimationClip sourceAnimation = null)
        {
            if (!string.IsNullOrEmpty(poseKey))
            {
                var byKey = poses.FirstOrDefault(pose => pose != null && pose.poseKey == poseKey);
                if (byKey != null)
                {
                    return byKey;
                }
            }

            return sourceAnimation == null
                ? null
                : poses.FirstOrDefault(pose => pose != null && pose.sourceAnimation == sourceAnimation);
        }

        public static string BuildPoseKey(string menuName, string displayName)
        {
            return $"{menuName ?? string.Empty}_{displayName ?? string.Empty}";
        }
    }

    [Serializable]
    public sealed class PosingSystemAutoPoseDefinition
    {
        [InspectorName("姿勢の識別キー")]
        [Tooltip("通常は「メニュー名_姿勢名」の形式です。調整ウィンドウから追加すると自動入力されます。")]
        public string poseKey;

        [InspectorName("元の姿勢Animation")]
        [Tooltip("IKで補正する前の基準となるHumanoid AnimationClipです。")]
        public AnimationClip sourceAnimation;

        [InspectorName("計算の反復回数")]
        [Tooltip("複数の手足を順番に解き直す回数です。通常は2回で、接触が安定しない場合だけ増やします。")]
        [Range(1, 8)] public int solveIterations = 2;

        [InspectorName("手足のIKルール")]
        [Tooltip("この姿勢で位置を自動調整する腕・脚と、その置き場所を指定します。")]
        public List<PosingSystemAutoPoseLimbRule> limbRules = new();
    }

    public enum PosingSystemAutoPoseLimb
    {
        [InspectorName("左腕")]
        LeftArm,
        [InspectorName("右腕")]
        RightArm,
        [InspectorName("左脚")]
        LeftLeg,
        [InspectorName("右脚")]
        RightLeg,
    }

    public enum PosingSystemAutoPoseTargetSpace
    {
        /// <summary>Target is stored relative to a Humanoid body bone.</summary>
        [InspectorName("身体のボーンを基準")]
        BodyBone,
        /// <summary>Target is stored relative to the avatar root.</summary>
        [InspectorName("アバター全体を基準")]
        AvatarRoot,
        /// <summary>Target X/Z is relative to the avatar root and Y is relative to its ground plane.</summary>
        [InspectorName("床を基準")]
        Ground,
    }

    public enum PosingSystemAutoPoseBendDirectionMode
    {
        [InspectorName("元の姿勢の曲げ方向を維持")]
        SourcePose,
        [InspectorName("手動で方向を指定")]
        Manual,
    }

    public enum PosingSystemAutoPosePositionSpace
    {
        [InspectorName("ローカル（基準の向き）")]
        ReferenceLocal = 0,
        [InspectorName("ワールド（シーンの軸）")]
        World = 1,
    }

    public enum PosingSystemAutoPoseLengthReference
    {
        [InspectorName("身長")]
        AvatarHeight = 0,
        [InspectorName("上腕の長さ")]
        UpperArm = 1,
        [InspectorName("前腕の長さ")]
        Forearm = 2,
        [InspectorName("胸の長さ")]
        Chest = 3,
        [InspectorName("太ももの長さ")]
        Thigh = 4,
        [InspectorName("すねの長さ")]
        Shin = 5,
        [InspectorName("脚全体の長さ（太もも＋すね）")]
        Leg = 6,
    }

    [Serializable]
    public sealed class PosingSystemAutoPoseLimbRule
    {
        public const int CurrentCoordinateSpaceVersion = 1;

        /// <summary>
        /// 0 is the former raw-Transform representation. 1 stores both the target frame and
        /// end-effector rotation in avatar-independent Humanoid bone space.
        /// Kept per rule so unfinished catalogs can be recaptured one rule at a time.
        /// </summary>
        [HideInInspector]
        public int coordinateSpaceVersion;

        [InspectorName("このルールを使用")]
        [Tooltip("無効にすると、このルールを一時的に計算対象から外します。")]
        public bool enabled = true;

        [InspectorName("ルール名")]
        [Tooltip("例: 右手を腰へ、左手を床へ。計算には影響しない、開発者向けの名前です。")]
        public string label;

        [InspectorName("計算する順番")]
        [Tooltip("同じ腕・脚へ複数ルールがある場合などの順番です。小さい値から計算します。脚は腕より先に計算されます。")]
        public int solveOrder;

        [InspectorName("動かす手足")]
        [Tooltip("IKで動かす左腕・右腕・左脚・右脚を選びます。")]
        public PosingSystemAutoPoseLimb limb;

        [InspectorName("置き場所の基準")]
        [Tooltip("頭・腰・膝などへ置く場合は身体のボーン、顔の横などはアバター全体、床へ接触させる場合は床を選びます。")]
        public PosingSystemAutoPoseTargetSpace targetSpace;

        [InspectorName("基準にする身体ボーン")]
        [Tooltip("「身体のボーンを基準」を選んだ場合に、手足の置き場所の基準となるHumanoidボーンです。")]
        public HumanBodyBones targetBone = HumanBodyBones.Hips;

        // Separate from HumanBodyBones so existing serialized bone values remain unchanged.
        [Tooltip("身体のボーンの代わりに、頭に追従するAvatar DescriptorのView Positionを基準にします。")]
        public bool useViewPosition;

        [InspectorName("位置の座標系")]
        [Tooltip("ワールドでは基準位置からワールドX・Y・Z方向へ移動します。ボーンの向きに影響されません。手足の向きの指定は変わりません。")]
        public PosingSystemAutoPosePositionSpace positionSpace;

        [InspectorName("位置の長さの基準")]
        [Tooltip("位置の比率で1とする長さです。腕・脚は動かす手足と同じ側を測ります。脚全体は太ももとすねの合計です。胸はChestからNeckまで（UpperChestがあれば経由）を測ります。")]
        public PosingSystemAutoPoseLengthReference lengthReference;

        [InspectorName("X方向を合わせる")]
        public bool applyPositionX = true;
        [InspectorName("Y方向を合わせる")]
        public bool applyPositionY = true;
        [InspectorName("Z方向を合わせる")]
        public bool applyPositionZ = true;

        public bool HasPositionAxis => applyPositionX || applyPositionY || applyPositionZ;
        public bool HasAllPositionAxes => applyPositionX && applyPositionY && applyPositionZ;

        [InspectorName("置き場所（選んだ長さに対する比率）")]
        [Tooltip("基準から見た手または足の位置です。選んだ長さを1とした比率です。無効な軸は元のポーズの位置を残します。調整ウィンドウの「現在の手足位置から取得」で自動入力できます。")]
        public Vector3 normalizedTargetOffset;

        [InspectorName("手足の向き")]
        [Tooltip("基準から見た手または足の向きです。Humanoidボーンの軸が異なるアバター間でも同じ見た目の向きになる形式で保存します。")]
        public Quaternion targetRotationOffset = Quaternion.identity;

        [InspectorName("手足の向きも合わせる")]
        [Tooltip("有効にすると位置だけでなく、手首または足首の向きも記録した向きへ合わせます。")]
        public bool matchEndRotation = true;

        [InspectorName("位置を合わせる強さ")]
        [Tooltip("0では移動せず、1でターゲット位置へ完全に合わせます。通常は1です。")]
        [Range(0f, 1f)] public float positionWeight = 1f;

        [InspectorName("向きを合わせる強さ")]
        [Tooltip("0では回転せず、1で記録した向きへ完全に合わせます。")]
        [Range(0f, 1f)] public float rotationWeight = 1f;

        [InspectorName("肘・膝の曲げ方")]
        [Tooltip("通常は元の姿勢を選びます。対象アバターへ元Animationを適用した直後の肘・膝と同じ側へ曲げます。")]
        public PosingSystemAutoPoseBendDirectionMode bendDirectionMode =
            PosingSystemAutoPoseBendDirectionMode.SourcePose;

        [InspectorName("肘・膝を曲げる方向")]
        [Tooltip("「手動で方向を指定」を選んだ場合の方向です。通常は使用しません。調整ウィンドウから自動取得できます。")]
        public Vector3 poleDirectionInAvatarSpace = Vector3.forward;
    }
}
