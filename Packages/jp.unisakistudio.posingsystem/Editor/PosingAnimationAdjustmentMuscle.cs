using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    enum PosingAnimationAdjustmentMuscleGroupName
    {
        Ignored = -1,
        Unidentified = 0,
        Trunk,
        NeckAndHead,
        LeftLeg,
        RightLeg,
        LeftArm,
        RightArm,
        LeftHand,
        RightHand,
        LeftFingers,
        RightFingers,
        Others,
        COUNT
    }

    class PosingAnimationAdjustmentMuscleGroup
    {
        public PosingAnimationAdjustmentMuscleGroupName Name { get; private set; }
        public string DisplayName { get; private set; }
        public PosingAnimationAdjustmentMuscleGroupName OppositeName { get; private set; }
        public string OppositeDisplayName { get; private set; }

        public bool HasOpposite => OppositeName > 0;

        List<int> indices;
        public List<int> MuscleIndices
        {
            get
            {
                if (indices == null)
                {
                    indices = new List<int>();
                }
                return indices;
            }
        }

        public PosingAnimationAdjustmentMuscleGroup(PosingAnimationAdjustmentMuscleGroupName name, string[] groupDisplayNames)
        {
            Name = name;
            DisplayName = groupDisplayNames[(int)name];

            switch (name)
            {
                case PosingAnimationAdjustmentMuscleGroupName.LeftLeg:
                    OppositeName = PosingAnimationAdjustmentMuscleGroupName.RightLeg;
                    break;
                case PosingAnimationAdjustmentMuscleGroupName.RightLeg:
                    OppositeName = PosingAnimationAdjustmentMuscleGroupName.LeftLeg;
                    break;
                case PosingAnimationAdjustmentMuscleGroupName.LeftArm:
                    OppositeName = PosingAnimationAdjustmentMuscleGroupName.RightArm;
                    break;
                case PosingAnimationAdjustmentMuscleGroupName.RightArm:
                    OppositeName = PosingAnimationAdjustmentMuscleGroupName.LeftArm;
                    break;
                case PosingAnimationAdjustmentMuscleGroupName.LeftHand:
                    OppositeName = PosingAnimationAdjustmentMuscleGroupName.RightHand;
                    break;
                case PosingAnimationAdjustmentMuscleGroupName.RightHand:
                    OppositeName = PosingAnimationAdjustmentMuscleGroupName.LeftHand;
                    break;
                case PosingAnimationAdjustmentMuscleGroupName.LeftFingers:
                    OppositeName = PosingAnimationAdjustmentMuscleGroupName.RightFingers;
                    break;
                case PosingAnimationAdjustmentMuscleGroupName.RightFingers:
                    OppositeName = PosingAnimationAdjustmentMuscleGroupName.LeftFingers;
                    break;
                default:
                    OppositeName = PosingAnimationAdjustmentMuscleGroupName.Unidentified;
                    break;
            }

            if (HasOpposite)
            {
                OppositeDisplayName = groupDisplayNames[(int)OppositeName];
            }
        }
    }

    static class PosingAnimationAdjustmentMuscle
    {
        // マッスル名の日本語翻訳配列（HumanTrait.MuscleCountに対応）
        private static string[] _muscleNamesJapanese;
        private static readonly List<int> _muscleDisplayOrder = new();

        static string[] _groupNamesJapanese = {
            "全身", "体幹", "首・頭", "左脚", "右脚", "左腕", "右腕", "左手首", "右手首", "左手指", "右手指", "その他"
        };

        private static PosingAnimationAdjustmentMuscleGroup[] _muscleGroups = new PosingAnimationAdjustmentMuscleGroup[(int)PosingAnimationAdjustmentMuscleGroupName.COUNT];

        public static PosingAnimationAdjustmentMuscleGroup[] MuscleGroups
        {
            get
            {
                InitializeIfNeeded();
                return _muscleGroups;
            }
        }

        private static void InitializeIfNeeded()
        {
            if (_muscleNamesJapanese != null && _muscleDisplayOrder.Count > 0)
            {
                // すでに初期化されている場合はスキップ
                return;
            }

            _muscleNamesJapanese = new string[HumanTrait.MuscleCount];

            var boneNames = HumanTrait.BoneName;

            // 初期化時は英語名をそのまま使用（後で日本語に置き換え可能）
            for (int i = 0; i < HumanTrait.MuscleCount; i++)
            {
                _muscleNamesJapanese[i] = HumanTrait.MuscleName[i];
            }

            // 体幹・胸部
            SetMuscleNameJapanese(0, "腰 前後");
            SetMuscleNameJapanese(1, "腰 左右");
            SetMuscleNameJapanese(2, "腰 ひねり");
            SetMuscleNameJapanese(3, "胸 前後");
            SetMuscleNameJapanese(4, "胸 左右");
            SetMuscleNameJapanese(5, "胸 ひねり");
            SetMuscleNameJapanese(6, "上胸部 前後");
            SetMuscleNameJapanese(7, "上胸部 左右");
            SetMuscleNameJapanese(8, "上胸部 ひねり");

            // 首・頭部
            SetMuscleNameJapanese(9, "首 上下");
            SetMuscleNameJapanese(10, "首 左右");
            SetMuscleNameJapanese(11, "首 ひねり");
            SetMuscleNameJapanese(12, "頭 上下");
            SetMuscleNameJapanese(13, "頭 左右");
            SetMuscleNameJapanese(14, "頭 ひねり");

            // 目・顎は飛ばす（15-20）

            // 左脚
            SetMuscleNameJapanese(21, "脚 前後");
            SetMuscleNameJapanese(22, "脚 内外");
            SetMuscleNameJapanese(23, "脚 ひねり");
            SetMuscleNameJapanese(24, "ひざ 曲げ伸ばし");
            SetMuscleNameJapanese(25, "ひざ ひねり");
            SetMuscleNameJapanese(26, "足首 上下");
            SetMuscleNameJapanese(27, "足首 ひねり");
            SetMuscleNameJapanese(28, "つま先 上下");

            // 右脚
            SetMuscleNameJapanese(29, "脚 前後");
            SetMuscleNameJapanese(30, "脚 内外");
            SetMuscleNameJapanese(31, "脚 ひねり");
            SetMuscleNameJapanese(32, "ひざ 曲げ伸ばし");
            SetMuscleNameJapanese(33, "ひざ ひねり");
            SetMuscleNameJapanese(34, "足首 上下");
            SetMuscleNameJapanese(35, "足首 ひねり");
            SetMuscleNameJapanese(36, "つま先 上下");

            // 左腕
            SetMuscleNameJapanese(37, "肩 上下");
            SetMuscleNameJapanese(38, "肩 前後");
            SetMuscleNameJapanese(39, "腕 上下");
            SetMuscleNameJapanese(40, "腕 前後");
            SetMuscleNameJapanese(41, "腕 ひねり");
            SetMuscleNameJapanese(42, "ひじ 曲げ伸ばし");
            SetMuscleNameJapanese(43, "ひじ ひねり");
            SetMuscleNameJapanese(44, "手首 上下");
            SetMuscleNameJapanese(45, "手首 内外");

            // 右腕
            SetMuscleNameJapanese(46, "肩 上下");
            SetMuscleNameJapanese(47, "肩 前後");
            SetMuscleNameJapanese(48, "腕 上下");
            SetMuscleNameJapanese(49, "腕 前後");
            SetMuscleNameJapanese(50, "腕 ひねり");
            SetMuscleNameJapanese(51, "ひじ 曲げ伸ばし");
            SetMuscleNameJapanese(52, "ひじ ひねり");
            SetMuscleNameJapanese(53, "手首 上下");
            SetMuscleNameJapanese(54, "手首 内外");

            // 左手指
            SetMuscleNameJapanese(56, "親指 開き");
            SetMuscleNameJapanese(60, "人差し指 開き");
            SetMuscleNameJapanese(64, "中指 開き");
            SetMuscleNameJapanese(68, "薬指 開き");
            SetMuscleNameJapanese(72, "小指 開き");
            SetMuscleNameJapanese(55, "親指 第一関節");
            SetMuscleNameJapanese(57, "親指 第二関節");
            SetMuscleNameJapanese(58, "親指 第三関節");
            SetMuscleNameJapanese(59, "人差し指 第一関節");
            SetMuscleNameJapanese(61, "人差し指 第二関節");
            SetMuscleNameJapanese(62, "人差し指 第三関節");
            SetMuscleNameJapanese(63, "中指 第一関節");
            SetMuscleNameJapanese(65, "中指 第二関節");
            SetMuscleNameJapanese(66, "中指 第三関節");
            SetMuscleNameJapanese(67, "薬指 第一関節");
            SetMuscleNameJapanese(69, "薬指 第二関節");
            SetMuscleNameJapanese(70, "薬指 第三関節");
            SetMuscleNameJapanese(71, "小指 第一関節");
            SetMuscleNameJapanese(73, "小指 第二関節");
            SetMuscleNameJapanese(74, "小指 第三関節");

            // 右手指
            SetMuscleNameJapanese(76, "親指 開き");
            SetMuscleNameJapanese(80, "人差し指 開き");
            SetMuscleNameJapanese(84, "中指 開き");
            SetMuscleNameJapanese(88, "薬指 開き");
            SetMuscleNameJapanese(92, "小指 開き");
            SetMuscleNameJapanese(75, "親指 第一関節");
            SetMuscleNameJapanese(77, "親指 第二関節");
            SetMuscleNameJapanese(78, "親指 第三関節");
            SetMuscleNameJapanese(79, "人差し指 第一関節");
            SetMuscleNameJapanese(81, "人差し指 第二関節");
            SetMuscleNameJapanese(82, "人差し指 第三関節");
            SetMuscleNameJapanese(83, "中指 第一関節");
            SetMuscleNameJapanese(85, "中指 第二関節");
            SetMuscleNameJapanese(86, "中指 第三関節");
            SetMuscleNameJapanese(87, "薬指 第一関節");
            SetMuscleNameJapanese(89, "薬指 第二関節");
            SetMuscleNameJapanese(90, "薬指 第三関節");
            SetMuscleNameJapanese(91, "小指 第一関節");
            SetMuscleNameJapanese(93, "小指 第二関節");
            SetMuscleNameJapanese(94, "小指 第三関節");

            BuildMuscleGroups();
        }

        static void BuildMuscleGroups()
        {
            Array.Clear(_muscleGroups, 0, _muscleGroups.Length);

            foreach (int muscleIndex in _muscleDisplayOrder)
            {
                var groupName = ResolveMuscleGroupName(muscleIndex);
                if (groupName < 0)
                {
                    continue;
                }

                PosingAnimationAdjustmentMuscleGroup group;
                if (_muscleGroups[(int)groupName] == null)
                {
                    group = _muscleGroups[(int)groupName] = new PosingAnimationAdjustmentMuscleGroup(groupName, _groupNamesJapanese);
                }
                else
                {
                    group = _muscleGroups[(int)groupName];
                }

                group.MuscleIndices.Add(muscleIndex);
            }
        }

        /// <summary>
        /// 指定したマッスルインデックスに日本語名を設定します
        /// </summary>
        /// <param name="muscleIndex">マッスルインデックス</param>
        /// <param name="japaneseName">日本語名</param>
        static void SetMuscleNameJapanese(int muscleIndex, string japaneseName)
        {
            if (muscleIndex >= 0 && muscleIndex < _muscleNamesJapanese.Length)
            {
                _muscleNamesJapanese[muscleIndex] = japaneseName;
            }

            if (!_muscleDisplayOrder.Contains(muscleIndex))
            {
                _muscleDisplayOrder.Add(muscleIndex);
            }
        }

        public static string GetMuscleNameJapanese(int muscleIndex)
        {
            // 初期化されていない場合は初期化を実行
            InitializeIfNeeded();

            if (muscleIndex < 0 || muscleIndex >= _muscleNamesJapanese.Length)
            {
                return HumanTrait.MuscleName[muscleIndex];
            }

            // 日本語翻訳が設定されていない場合は英語名を返す
            var japaneseName = _muscleNamesJapanese[muscleIndex];
            return string.IsNullOrEmpty(japaneseName) ? HumanTrait.MuscleName[muscleIndex] : japaneseName;
        }

        private static bool IsRightBoneName(string boneName) => boneName.StartsWith("Right", StringComparison.OrdinalIgnoreCase);

        private static PosingAnimationAdjustmentMuscleGroupName ResolveMuscleGroupName(int muscleIndex)
        {
            var boneIndex = HumanTrait.BoneFromMuscle(muscleIndex);
            if (boneIndex < 0 || boneIndex >= HumanTrait.BoneCount)
            {
                return PosingAnimationAdjustmentMuscleGroupName.Unidentified;
            }

            var boneName = HumanTrait.BoneName[boneIndex];
            if (string.IsNullOrEmpty(boneName))
            {
                return PosingAnimationAdjustmentMuscleGroupName.Others;
            }

            if (boneName.Contains("Thumb") || boneName.Contains("Index") || boneName.Contains("Middle") || boneName.Contains("Ring") || boneName.Contains("Little"))
            {
                return IsRightBoneName(boneName) ? PosingAnimationAdjustmentMuscleGroupName.RightFingers : PosingAnimationAdjustmentMuscleGroupName.LeftFingers;
            }

            if (boneName.Contains("Hand"))
            {
                return IsRightBoneName(boneName) ? PosingAnimationAdjustmentMuscleGroupName.RightHand : PosingAnimationAdjustmentMuscleGroupName.LeftHand;
            }

            if (boneName.Contains("Arm") || boneName.Contains("Shoulder"))
            {
                return IsRightBoneName(boneName) ? PosingAnimationAdjustmentMuscleGroupName.RightArm : PosingAnimationAdjustmentMuscleGroupName.LeftArm;
            }

            if (boneName.Contains("Leg") || boneName.Contains("Foot") || boneName.Contains("Toe"))
            {
                return IsRightBoneName(boneName) ? PosingAnimationAdjustmentMuscleGroupName.RightLeg : PosingAnimationAdjustmentMuscleGroupName.LeftLeg;
            }

            if (boneName.Contains("Eye"))
            {
                return PosingAnimationAdjustmentMuscleGroupName.Ignored;
            }

            if (boneName.Contains("Jaw"))
            {
                return PosingAnimationAdjustmentMuscleGroupName.Ignored;
            }

            if (boneName.Contains("Head") || boneName.Contains("Neck"))
            {
                return PosingAnimationAdjustmentMuscleGroupName.NeckAndHead;
            }

            if (boneName.Contains("Spine") || boneName.Contains("Chest") || boneName.Contains("Hips") || boneName.Contains("UpperChest"))
            {
                return PosingAnimationAdjustmentMuscleGroupName.Trunk;
            }

            return PosingAnimationAdjustmentMuscleGroupName.Others;
        }
    }
}