using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;
using VRC.SDK3.Avatars.Components;
using System.IO;

namespace jp.unisakistudio.posingsystemeditor
{
    public class PosingAnimationAdjustmentWindow : EditorWindow
    {
        private bool _isOpen = false;
        private const string DefaultAdjustmentFolderRoot = "Assets/UnisakiStudio/GeneratedResources";
        private static readonly char[] InvalidFileNameChars = System.IO.Path.GetInvalidFileNameChars();
        
        // マッスル名の日本語翻訳配列（HumanTrait.MuscleCountに対応）
        private static string[] MuscleNamesJapanese;
        private static List<int> muscleDisplayOrder = new();

        private readonly Dictionary<string, bool> _foldoutStates = new();
        private readonly List<MuscleGroup> _muscleGroups = new();

        private PosingSystem _posingSystem;
        private readonly List<AnimationEntry> _animationEntries = new();
        private string[] _animationLabels = Array.Empty<string>();
        private int _selectedAnimationIndex = -1;
        public class Adjustments : ScriptableObject
        {
            public float[] adjustments;
            public Vector3 rootQAdjustments = Vector3.zero; // Euler angles for rotation
            public float rootTYAdjustment = 0f; // Y-axis position adjustment (height)
        }

        [System.Serializable]
        public class LimitBreakStates : ScriptableObject
        {
            [System.Serializable]
            public class MuscleLimitBreakData
            {
                public List<int> limitBreakMuscles = new List<int>();
            }

            [System.Serializable]
            public class RootQLimitBreakData
            {
                public List<int> limitBreakAxes = new List<int>();
            }

            [System.Serializable]
            public class AnimationLimitBreakData
            {
                public string animationKey;
                public MuscleLimitBreakData muscleLimitBreaks = new MuscleLimitBreakData();
                public RootQLimitBreakData rootQLimitBreaks = new RootQLimitBreakData();
                public bool rootTYLimitBreak = false;
            }

            public List<AnimationLimitBreakData> animationLimitBreaks = new List<AnimationLimitBreakData>();

            public AnimationLimitBreakData GetOrCreateAnimationData(string key)
            {
                var data = animationLimitBreaks.FirstOrDefault(x => x.animationKey == key);
                if (data == null)
                {
                    data = new AnimationLimitBreakData { animationKey = key };
                    animationLimitBreaks.Add(data);
                }
                return data;
            }
        }
        private Adjustments _adjustments;
        private LimitBreakStates _limitBreakStates;
        private Vector2 _animationListScroll;
        private Vector2 _muscleScroll;

        private bool _previewActive;
        private Animator _previewAvatar;
        
        // プレビュー開始時の初期位置・回転を保存（累積回転問題の回避用）
        private Vector3 _initialAvatarPosition;
        private Quaternion _initialAvatarRotation;
        private bool _initialTransformSaved;
        
        // Undo検知用の前回の値を保存
        private float[] _previousAdjustmentValues;
        
        // 全ての姿勢の変更データを保持
        private readonly Dictionary<string, float[]> _allAdjustmentData = new();
        private readonly Dictionary<string, Vector3> _allRootQAdjustmentData = new();
        private readonly Dictionary<string, float> _allRootTYAdjustmentData = new();
        
        // 元の値を保持（変更検知用）
        private readonly Dictionary<string, float[]> _originalAdjustmentData = new();
        private readonly Dictionary<string, Vector3> _originalRootQAdjustmentData = new();
        private readonly Dictionary<string, float> _originalRootTYAdjustmentData = new();
        
        
        // RootQの限界突破状態を保持（軸インデックス別）
        private readonly Dictionary<string, Dictionary<int, bool>> _rootQLimitBreakStates = new();
        
        // RootTYの限界突破状態を保持
        private readonly Dictionary<string, bool> _rootTYLimitBreakStates = new();

        private class AnimationEntry
        {
            public PosingSystem.LayerDefine Layer;
            public PosingSystem.AnimationDefine Animation;
            public string Label;
        }

        private class MuscleGroup
        {
            public string Name;
            public int Order;
            public readonly List<int> Indexes = new();
        }

        public bool IsOpen
        {
            get { return _isOpen && _posingSystem != null; }
        }

        private string GetAnimationKey(AnimationEntry entry)
        {
            if (entry?.Layer?.menuName != null && entry?.Animation?.displayName != null)
            {
                return $"{entry.Layer.menuName}_{entry.Animation.displayName}";
            }
            return string.Empty;
        }

        private void SaveCurrentAdjustmentData()
        {
            var entry = GetSelectedEntry();
            if (entry == null) return;
            
            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return;
            
            if (!_allAdjustmentData.ContainsKey(key))
            {
                _allAdjustmentData[key] = new float[HumanTrait.MuscleCount];
            }
            
            Array.Copy(_adjustments.adjustments, _allAdjustmentData[key], HumanTrait.MuscleCount);
            
            // RootQの値も保存
            _allRootQAdjustmentData[key] = _adjustments.rootQAdjustments;
            
            // RootTYの値も保存
            _allRootTYAdjustmentData[key] = _adjustments.rootTYAdjustment;
        }

        private bool HasMuscleChanges(int muscleIndex)
        {
            var entry = GetSelectedEntry();
            if (entry == null) return false;
            
            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return false;
            
            if (!_originalAdjustmentData.TryGetValue(key, out var originalData))
                return false;
            
            if (muscleIndex < 0 || muscleIndex >= originalData.Length)
                return false;
            
            return Mathf.Abs(_adjustments.adjustments[muscleIndex] - originalData[muscleIndex]) > 0.0001f;
        }

        private bool HasAnyChanges()
        {
            var entry = GetSelectedEntry();
            if (entry == null) return false;
            
            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return false;
            
            // Muscleの変更をチェック
            if (_originalAdjustmentData.TryGetValue(key, out var originalData))
            {
                for (int i = 0; i < HumanTrait.MuscleCount; i++)
                {
                    if (Mathf.Abs(_adjustments.adjustments[i] - originalData[i]) > 0.0001f)
                        return true;
                }
            }
            
            // RootQの変更をチェック
            if (_originalRootQAdjustmentData.TryGetValue(key, out var originalRootQ))
            {
                if (Vector3.Distance(_adjustments.rootQAdjustments, originalRootQ) > 0.0001f)
                    return true;
            }
            else if (_adjustments.rootQAdjustments != Vector3.zero)
            {
                return true;
            }
            
            // RootTYの変更をチェック
            if (_originalRootTYAdjustmentData.TryGetValue(key, out var originalRootTY))
            {
                if (Mathf.Abs(_adjustments.rootTYAdjustment - originalRootTY) > 0.0001f)
                    return true;
            }
            else if (Mathf.Abs(_adjustments.rootTYAdjustment) > 0.0001f)
            {
                return true;
            }
            
            return false;
        }

        private bool HasAnyAdjustments()
        {
            var entry = GetSelectedEntry();
            if (entry == null) return false;
            
            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return false;
            
            if (!_originalAdjustmentData.TryGetValue(key, out var originalData))
                return false;
            
            for (int i = 0; i < HumanTrait.MuscleCount; i++)
            {
                if (_adjustments.adjustments[i] != 0f)
                    return true;
            }
            
            // RootQの調整値もチェック
            if (_adjustments.rootQAdjustments != Vector3.zero)
                return true;
            
            // RootTYの調整値もチェック
            if (_adjustments.rootTYAdjustment != 0f)
                return true;
            
            return false;
        }

        private bool HasAdjustmentClip()
        {
            var entry = GetSelectedEntry();
            return entry?.Animation?.adjustmentClip != null;
        }

        private bool GetLimitBreakState(int muscleIndex)
        {
            var entry = GetSelectedEntry();
            if (entry == null) return false;
            
            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return false;
            
            if (_limitBreakStates != null)
            {
                var animData = _limitBreakStates.GetOrCreateAnimationData(key);
                return animData.muscleLimitBreaks.limitBreakMuscles.Contains(muscleIndex);
            }
            return false;
        }

        private void SetLimitBreakState(int muscleIndex, bool isLimitBreak)
        {
            var entry = GetSelectedEntry();
            if (entry == null) return;

            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return;

            if (_limitBreakStates != null)
            {
                Undo.RecordObject(_limitBreakStates, $"Change muscle {muscleIndex} limit break");
                var animData = _limitBreakStates.GetOrCreateAnimationData(key);
                
                if (isLimitBreak)
                {
                    if (!animData.muscleLimitBreaks.limitBreakMuscles.Contains(muscleIndex))
                    {
                        animData.muscleLimitBreaks.limitBreakMuscles.Add(muscleIndex);
                    }
                }
                else
                {
                    animData.muscleLimitBreaks.limitBreakMuscles.Remove(muscleIndex);
                }
            }
        }

        private bool GetRootQLimitBreakState(int axisIndex)
        {
            var entry = GetSelectedEntry();
            if (entry == null) return false;
            
            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return false;
            
            if (_limitBreakStates != null)
            {
                var animData = _limitBreakStates.GetOrCreateAnimationData(key);
                return animData.rootQLimitBreaks.limitBreakAxes.Contains(axisIndex);
            }
            return false;
        }

        private void SetRootQLimitBreakState(int axisIndex, bool isLimitBreak)
        {
            var entry = GetSelectedEntry();
            if (entry == null) return;

            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return;

            if (_limitBreakStates != null)
            {
                Undo.RecordObject(_limitBreakStates, $"Change RootQ {axisIndex} limit break");
                var animData = _limitBreakStates.GetOrCreateAnimationData(key);
                
                if (isLimitBreak)
                {
                    if (!animData.rootQLimitBreaks.limitBreakAxes.Contains(axisIndex))
                    {
                        animData.rootQLimitBreaks.limitBreakAxes.Add(axisIndex);
                    }
                }
                else
                {
                    animData.rootQLimitBreaks.limitBreakAxes.Remove(axisIndex);
                }
            }
        }

        private bool GetRootTYLimitBreakState()
        {
            var entry = GetSelectedEntry();
            if (entry == null) return false;
            
            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return false;
            
            if (_limitBreakStates != null)
            {
                var animData = _limitBreakStates.GetOrCreateAnimationData(key);
                return animData.rootTYLimitBreak;
            }
            return false;
        }

        private void SetRootTYLimitBreakState(bool isLimitBreak)
        {
            var entry = GetSelectedEntry();
            if (entry == null) return;

            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return;

            if (_limitBreakStates != null)
            {
                Undo.RecordObject(_limitBreakStates, "Change RootTY limit break");
                var animData = _limitBreakStates.GetOrCreateAnimationData(key);
                animData.rootTYLimitBreak = isLimitBreak;
            }
        }
        
        private bool HasAnyChangesInAllAnimations()
        {
            // Muscleの変更をチェック
            foreach (var kvp in _allAdjustmentData)
            {
                var key = kvp.Key;
                var currentData = kvp.Value;

                if (_originalAdjustmentData.TryGetValue(key, out var originalData))
                {
                    for (int i = 0; i < HumanTrait.MuscleCount; i++)
                    {
                        if (Mathf.Abs(currentData[i] - originalData[i]) > 0.0001f)
                            return true;
                    }
                }
            }
            
            // RootQの変更をチェック
            foreach (var kvp in _allRootQAdjustmentData)
            {
                var key = kvp.Key;
                var currentRootQ = kvp.Value;

                if (_originalRootQAdjustmentData.TryGetValue(key, out var originalRootQ))
                {
                    if (Vector3.Distance(currentRootQ, originalRootQ) > 0.0001f)
                        return true;
                }
                else if (currentRootQ != Vector3.zero)
                {
                    return true;
                }
            }
            
            // RootTYの変更をチェック
            foreach (var kvp in _allRootTYAdjustmentData)
            {
                var key = kvp.Key;
                var currentRootTY = kvp.Value;

                if (_originalRootTYAdjustmentData.TryGetValue(key, out var originalRootTY))
                {
                    if (Mathf.Abs(currentRootTY - originalRootTY) > 0.0001f)
                        return true;
                }
                else if (Mathf.Abs(currentRootTY) > 0.0001f)
                {
                    return true;
                }
            }

            return false;
        }

        private bool HasAnyAdjustmentsInAllAnimations()
        {
            // Muscleの調整値をチェック
            foreach (var kvp in _allAdjustmentData)
            {
                var key = kvp.Key;
                var currentData = kvp.Value;
                if (currentData.Any(x => x != 0f))
                    return true;
            }
            
            // RootQの調整値をチェック
            foreach (var kvp in _allRootQAdjustmentData)
            {
                var currentRootQ = kvp.Value;
                if (currentRootQ != Vector3.zero)
                    return true;
            }
            
            // RootTYの調整値をチェック
            foreach (var kvp in _allRootTYAdjustmentData)
            {
                var currentRootTY = kvp.Value;
                if (Mathf.Abs(currentRootTY) > 0.0001f)
                    return true;
            }
            
            return false;
        }

        private string[] GetDynamicAnimationLabels()
        {
            var labels = new string[_animationEntries.Count];
            for (int i = 0; i < _animationEntries.Count; i++)
            {
                var entry = _animationEntries[i];
                var key = GetAnimationKey(entry);
                var hasChanges = HasAnimationChanges(key);
                
                if (hasChanges)
                {
                    labels[i] = $"{entry.Label} ※";
                }
                else
                {
                    labels[i] = entry.Label;
                }
            }
            return labels;
        }

        private bool HasAnimationChanges(string key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            
            // Muscleの変更をチェック
            if (_allAdjustmentData.TryGetValue(key, out var currentData) &&
                _originalAdjustmentData.TryGetValue(key, out var originalData))
            {
                for (int i = 0; i < HumanTrait.MuscleCount; i++)
                {
                    if (Mathf.Abs(currentData[i] - originalData[i]) > 0.0001f)
                        return true;
                }
            }
            
            // RootQの変更をチェック
            if (_allRootQAdjustmentData.TryGetValue(key, out var currentRootQ))
            {
                if (_originalRootQAdjustmentData.TryGetValue(key, out var originalRootQ))
                {
                    if (Vector3.Distance(currentRootQ, originalRootQ) > 0.0001f)
                        return true;
                }
                else if (currentRootQ != Vector3.zero)
                {
                    return true;
                }
            }
            
            // RootTYの変更をチェック
            if (_allRootTYAdjustmentData.TryGetValue(key, out var currentRootTY))
            {
                if (_originalRootTYAdjustmentData.TryGetValue(key, out var originalRootTY))
                {
                    if (Mathf.Abs(currentRootTY - originalRootTY) > 0.0001f)
                        return true;
                }
                else if (Mathf.Abs(currentRootTY) > 0.0001f)
                {
                    return true;
                }
            }
            
            return false;
        }


        private static void InitializeMuscleNamesJapanese()
        {
            MuscleNamesJapanese = new string[HumanTrait.MuscleCount];

            // 初期化時は英語名をそのまま使用（後で日本語に置き換え可能）
            for (int i = 0; i < HumanTrait.MuscleCount; i++)
            {
                MuscleNamesJapanese[i] = HumanTrait.MuscleName[i];
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
        }

        private static string GetMuscleNameJapanese(int muscleIndex)
        {
            // 初期化されていない場合は初期化を実行
            if (MuscleNamesJapanese == null || muscleDisplayOrder.Count == 0)
            {
                InitializeMuscleNamesJapanese();
            }
            
            if (muscleIndex < 0 || muscleIndex >= MuscleNamesJapanese.Length)
            {
                return HumanTrait.MuscleName[muscleIndex];
            }
            
            // 日本語翻訳が設定されていない場合は英語名を返す
            var japaneseName = MuscleNamesJapanese[muscleIndex];
            return string.IsNullOrEmpty(japaneseName) ? HumanTrait.MuscleName[muscleIndex] : japaneseName;
        }

        /// <summary>
        /// 指定したマッスルインデックスに日本語名を設定します
        /// </summary>
        /// <param name="muscleIndex">マッスルインデックス</param>
        /// <param name="japaneseName">日本語名</param>
        public static void SetMuscleNameJapanese(int muscleIndex, string japaneseName)
        {
            if (muscleIndex >= 0 && muscleIndex < MuscleNamesJapanese.Length)
            {
                MuscleNamesJapanese[muscleIndex] = japaneseName;
            }

            if (!muscleDisplayOrder.Contains(muscleIndex))
            {
                muscleDisplayOrder.Add(muscleIndex);
            }
            else{
            }
        }

        /// <summary>
        /// 全てのマッスル名とそのインデックスを取得します（翻訳設定用）
        /// </summary>
        /// <returns>マッスルインデックスと英語名のペア</returns>
        public static (int index, string englishName)[] GetAllMuscleInfo()
        {
            // 初期化されていない場合は初期化を実行
            if (MuscleNamesJapanese == null || muscleDisplayOrder.Count == 0)
            {
                InitializeMuscleNamesJapanese();
            }
            
            var result = new (int, string)[HumanTrait.MuscleCount];
            for (int i = 0; i < HumanTrait.MuscleCount; i++)
            {
                result[i] = (i, HumanTrait.MuscleName[i]);
            }
            return result;
        }

        public static void ShowWindow(PosingSystem posingSystem)
        {
            if (posingSystem == null)
            {
                return;
            }

            PosingAnimationAdjustmentWindow window = null;
            foreach (var existWindow in Resources.FindObjectsOfTypeAll<PosingAnimationAdjustmentWindow>())
            {
                window = existWindow;
                existWindow.CleanupPreview();
            }

            if (!window)
            {
                window = GetWindow<PosingAnimationAdjustmentWindow>();
                // ウィンドウが最初は小さすぎるので初期値を設定する
                window.minSize = new Vector2(400, 300);
            }
            window.titleContent = new GUIContent("アニメーション調整");
            window.Initialize(posingSystem);
            window.ShowUtility();

        }

        private void Initialize(PosingSystem posingSystem)
        {
            _isOpen = true;

            _posingSystem = posingSystem;
            BuildMuscleGroups();
            RebuildAnimationEntries();
            
            // 全ての姿勢の調整値を事前に読み込み
            PreloadAllAdjustmentData();
            
            if (_animationEntries.Count > 0)
            {
                _selectedAnimationIndex = Mathf.Clamp(_selectedAnimationIndex, 0, _animationEntries.Count - 1);
                LoadAdjustmentValues();
            }
            else
            {
                _selectedAnimationIndex = -1;
                _adjustments = null;
            }
            Repaint();
        }

        public void Close()
        {
            _isOpen = false;
            CleanupPreview();
            _posingSystem = null;
            _animationEntries.Clear();
            _animationLabels = Array.Empty<string>();
            _selectedAnimationIndex = -1;
            _adjustments = null;
            Repaint();
        }

        private void OnEnable()
        {
            if (!IsOpen)
            {
                return;
            }
            // Undoイベントを購読
            Undo.undoRedoPerformed += OnUndoRedoPerformed;
            // 選択変更イベントを購読
            Selection.selectionChanged += OnSelectionChanged;
            BuildMuscleGroups();
            RebuildAnimationEntries();

            // 全ての姿勢の調整値を事前に読み込み
            PreloadAllAdjustmentData();

            if (_animationEntries.Count > 0)
            {
                _selectedAnimationIndex = Mathf.Clamp(_selectedAnimationIndex, 0, _animationEntries.Count - 1);
                LoadAdjustmentValues();
            }
            else
            {
                _selectedAnimationIndex = -1;
                _adjustments = null;
            }
            Repaint();
        }

        private void OnDisable()
        {
            // Undoイベントの購読解除
            Undo.undoRedoPerformed -= OnUndoRedoPerformed;
            // 選択変更イベントの購読解除
            Selection.selectionChanged -= OnSelectionChanged;
            CleanupPreview();
        }

        private void OnUndoRedoPerformed()
        {
            if (!IsOpen)
            {
                return;
            }
            // Undo/Redo操作が実行されたときに姿勢を更新
            if (_adjustments != null)
            {
                ApplyPreviewPose();
                UpdatePreviousAdjustmentValues();
                Repaint();
            }
        }

        private void OnSelectionChanged()
        {
            if (!IsOpen)
            {
                return;
            }

            // 選択されているオブジェクトがない場合は何もしない
            if (Selection.activeGameObject == null)
            {
                return;
            }

            // 現在プレビュー中のアバターを取得
            GameObject currentPreviewAvatar = null;
            if (_previewAvatar != null)
            {
                currentPreviewAvatar = _previewAvatar.gameObject;
            }
            else if (_posingSystem != null)
            {
                var avatar = _posingSystem.GetAvatar();
                if (avatar != null)
                {
                    currentPreviewAvatar = avatar.gameObject;
                }
            }

            // プレビュー中のアバターが存在し、選択されたオブジェクトが異なる場合
            if (currentPreviewAvatar != null && Selection.activeGameObject != currentPreviewAvatar)
            {
                // プレビューを終了
                CleanupPreview();
                Repaint();
            }
        }

        private void UpdatePreviousAdjustmentValues()
        {
            if (_adjustments?.adjustments != null)
            {
                _previousAdjustmentValues = new float[_adjustments.adjustments.Length];
                Array.Copy(_adjustments.adjustments, _previousAdjustmentValues, _adjustments.adjustments.Length);
            }
        }

        private void CheckForUndoChanges()
        {
            if (_adjustments?.adjustments != null && _previousAdjustmentValues != null)
            {
                // 配列の長さが異なる場合は値が変更されたとみなす
                if (_adjustments.adjustments.Length != _previousAdjustmentValues.Length)
                {
                    ApplyPreviewPose();
                    UpdatePreviousAdjustmentValues();
                    return;
                }

                // 各値をチェックして変更があるかを確認
                for (int i = 0; i < _adjustments.adjustments.Length; i++)
                {
                    if (Mathf.Abs(_adjustments.adjustments[i] - _previousAdjustmentValues[i]) > 0.0001f)
                    {
                        // 値が変更されている場合は姿勢を更新
                        ApplyPreviewPose();
                        UpdatePreviousAdjustmentValues();
                        break;
                    }
                }
            }
        }

        private void OnGUI()
        {
            if (!IsOpen)
            {
                EditorGUILayout.HelpBox("アニメーションの調整は現在停止中です", MessageType.Warning);

                return;
            }

            // Undo操作による値変更をチェック
            CheckForUndoChanges();
            
            if (_posingSystem == null)
            {
                EditorGUILayout.HelpBox("PosingSystem が見つかりません。対象のコンポーネントを選択した状態で再度開いてください。", MessageType.Warning);
                if (GUILayout.Button("閉じる"))
                {
                    Close();
                }
                return;
            }

            // 左右分割レイアウト
            using (new EditorGUILayout.HorizontalScope())
            {
                // 左側パネル（対象、アニメーション、元アニメーション）
                using (new EditorGUILayout.VerticalScope(GUILayout.Width(200)))
                {
                    DrawLeftPanel();
                }

                // 分割線
                GUILayout.Box("", GUILayout.Width(1), GUILayout.ExpandHeight(true));

                // 右側パネル（調整コントロール）
                using (new EditorGUILayout.VerticalScope())
                {
                    DrawRightPanel();
                }
            }
        }

        private void DrawLeftPanel()
        {
            EditorGUILayout.LabelField("アニメーション調整", EditorStyles.boldLabel);

            // グローバルボタン
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("一括操作", EditorStyles.boldLabel);

                var hasAnyChangesGlobal = HasAnyChangesInAllAnimations();
                var hasAnyAdjustmentsGlobal = HasAnyAdjustmentsInAllAnimations();

                // 全姿勢初期化（調整値がある時のみ有効）
                using (new EditorGUI.DisabledScope(!hasAnyAdjustmentsGlobal))
                {
                    if (GUILayout.Button(new GUIContent("全姿勢初期化", "全ての調整値を0にリセットします。")))
                    {
                        ResetAllAnimationsAdjustments();
                    }
                }

                // 全姿勢再読み込み（変更がある時のみ有効）
                using (new EditorGUI.DisabledScope(!hasAnyChangesGlobal))
                {
                    if (GUILayout.Button(new GUIContent("全姿勢再読込", "全ての調整池をファイルに保存されている値に戻します。")))
                    {
                        ReloadAllAnimationsAdjustments();
                    }
                }

                // 全差分保存（変更がある時のみ有効）
                using (new EditorGUI.DisabledScope(!hasAnyChangesGlobal))
                {
                    if (GUILayout.Button(new GUIContent("全差分保存", "変更のあるアニメーションの調整値を保存します。")))
                    {
                        SaveAllAdjustmentClips();
                    }
                }

                // 全調整データ複製保存（変更がある時のみ有効）
                using (new EditorGUI.DisabledScope(!hasAnyChangesGlobal))
                {
                    if (GUILayout.Button(new GUIContent("全調整データ複製保存", "変更のあるアニメーションの調整値を複製して保存します。元々設定されていたファイルは変更されません")))
                    {
                        DuplicateAndSaveAllAdjustmentClips();
                    }
                }
            }

            EditorGUILayout.LabelField("アニメーション");
            using (var scrollView = new EditorGUILayout.ScrollViewScope(_animationListScroll, GUILayout.ExpandHeight(true)))
            {
                _animationListScroll = scrollView.scrollPosition;
                if (_animationEntries.Count == 0)
                {
                    EditorGUILayout.HelpBox("調整可能なアニメーションが見つかりません。PosingSystem の設定を確認してください。", MessageType.Info);
                }
                else
                {
                    EditorGUI.BeginChangeCheck();
                    var dynamicLabels = GetDynamicAnimationLabels();
                    _selectedAnimationIndex = GUILayout.SelectionGrid(_selectedAnimationIndex, dynamicLabels, 1, GUILayout.ExpandHeight(true));
                    if (EditorGUI.EndChangeCheck())
                    {
                        LoadAdjustmentValues();
                    }
                }
            }
        }

        private void DrawRightPanel()
        {
            var selectedEntry = GetSelectedEntry();
            if (selectedEntry == null)
            {
                EditorGUILayout.HelpBox("アニメーションを選択してください。", MessageType.Info);
                return;
            }

            var motion = selectedEntry.Animation.animationClip;
            if (motion == null)
            {
                EditorGUILayout.HelpBox("アニメーションが割り当てられていません。", MessageType.Warning);
                DrawAdjustmentControls(enabled: false);
                return;
            }

            if (motion is not AnimationClip baseClip)
            {
                EditorGUILayout.HelpBox("現在のモーションは AnimationClip ではありません。差分調整は AnimationClip のみ対応しています。", MessageType.Warning);
                DrawAdjustmentControls(enabled: false);
                return;
            }

            // アニメーション情報セクション
            DrawAnimationInfoSection(selectedEntry);
            
            DrawAdjustmentControls(enabled: true);
        }

        private void DrawAnimationInfoSection(AnimationEntry entry)
        {
            EditorGUILayout.LabelField("アニメーション情報", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                // アニメーション名表示
                EditorGUILayout.LabelField("名前", entry.Animation.displayName);
                
                // 調整アニメーションのanimationClipProperty表示・編集
                using (new EditorGUILayout.HorizontalScope())
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        EditorGUILayout.ObjectField("現在のアニメーション", entry.Animation.animationClip, typeof(AnimationClip), false);
                    }
                    using (new EditorGUI.DisabledScope(entry.Animation.adjustmentClip == null))
                    {
                        if (GUILayout.Button(new GUIContent("合成", "現在のアニメーションに調整アニメーションを合成したものを新しいアニメーションクリップとして保存します。"), GUILayout.Width(50)))
                        {
                            CombineAndSaveAdjustmentClip(entry);
                        }
                    }
                }

                // 現在の調整アニメーション表示
                EditorGUI.BeginChangeCheck();
                using (new EditorGUILayout.HorizontalScope())
                {
                    entry.Animation.adjustmentClip = EditorGUILayout.ObjectField("調整アニメーション", entry.Animation.adjustmentClip, typeof(AnimationClip), false) as AnimationClip;
                    if (entry.Animation.adjustmentClip != null)
                    {
                        // 複製ボタン
                        if (GUILayout.Button(new GUIContent("複製", "調整アニメーションを複製します。"), GUILayout.Width(50)))
                        {
                            DuplicateAdjustmentClip(entry);
                        }
                    }
                }
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_posingSystem, "Change Adjustment Clip");
                    EditorUtility.SetDirty(_posingSystem);
                    
                    // 調整アニメーションが変更されたので、データを再読込して元データも更新
                    ReloadCurrentAdjustmentFromFile();
                    
                    // 元データを現在のデータで更新（変更検知をリセット）
                    UpdateOriginalDataAfterClipChange();
                }
            }
            
            EditorGUILayout.Space();
        }

        private void UpdateOriginalDataAfterClipChange()
        {
            var entry = GetSelectedEntry();
            if (entry == null) return;
            
            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return;

            // 現在の調整値を元データとして更新（変更検知をリセット）
            if (_adjustments != null)
            {
                // Muscleの元データを更新
                if (!_originalAdjustmentData.ContainsKey(key))
                {
                    _originalAdjustmentData[key] = new float[HumanTrait.MuscleCount];
                }
                Array.Copy(_adjustments.adjustments, _originalAdjustmentData[key], HumanTrait.MuscleCount);
                
                // RootQの元データを更新
                _originalRootQAdjustmentData[key] = _adjustments.rootQAdjustments;
                
                // RootTYの元データを更新
                _originalRootTYAdjustmentData[key] = _adjustments.rootTYAdjustment;
            }
            
            // UIを更新
            Repaint();
        }

        private void CombineAndSaveAdjustmentClip(AnimationEntry entry)
        {
            if (entry.Animation.adjustmentClip == null)
            {
                EditorUtility.DisplayDialog("エラー", "合成する調整アニメーションが存在しません。", "OK");
                return;
            }

            // 新しいファイル名を生成
            var originalPath = AssetDatabase.GetAssetPath(entry.Animation.animationClip);
            var newPath = AssetDatabase.GenerateUniqueAssetPath(originalPath);

            // アニメーションを合成
            var combinedClip = UnityEngine.Object.Instantiate(entry.Animation.animationClip as AnimationClip);
            combinedClip.name = System.IO.Path.GetFileNameWithoutExtension(newPath);

            // 調整アニメーションを合成
            var adjustmentClip = entry.Animation.adjustmentClip;
            
            // 調整アニメーションの全てのカーブを取得してベースアニメーションに適用
            var adjustmentBindings = AnimationUtility.GetCurveBindings(adjustmentClip);
            foreach (var binding in adjustmentBindings)
            {
                // RootQカーブはQuaternion乗算で処理するためスキップ
                if (AdditiveCurveUtility.IsRootQBinding(binding))
                {
                    continue;
                }

                var adjustmentCurve = AnimationUtility.GetEditorCurve(adjustmentClip, binding);
                if (adjustmentCurve == null) continue;

                // ベースアニメーションの対応するカーブを取得
                var baseCurve = AnimationUtility.GetEditorCurve(combinedClip, binding);
                
                if (baseCurve != null)
                {
                    // 既存のカーブがある場合は加算合成
                    for (int i = 0; i < baseCurve.keys.Length && i < adjustmentCurve.keys.Length; i++)
                    {
                        var baseKey = baseCurve.keys[i];
                        var adjustmentKey = adjustmentCurve.keys[i];
                        
                        // 時間が一致する場合のみ加算
                        if (Mathf.Approximately(baseKey.time, adjustmentKey.time))
                        {
                            baseKey.value += adjustmentKey.value;
                            baseCurve.MoveKey(i, baseKey);
                        }
                    }
                    AnimationUtility.SetEditorCurve(combinedClip, binding, baseCurve);
                }
                else
                {
                    // ベースにカーブがない場合は調整カーブをそのまま設定
                    AnimationUtility.SetEditorCurve(combinedClip, binding, adjustmentCurve);
                }
            }
            
            // RootQカーブはQuaternion乗算で合成（元の回転 × 調整の回転）
            AdditiveCurveUtility.MultiplyRootQCurves(entry.Animation.animationClip as AnimationClip, adjustmentClip, combinedClip);
            
            // 新しいアニメーションとして保存
            AssetDatabase.CreateAsset(combinedClip, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 新しいアニメーションを現在のアニメーションとして設定
            Undo.RecordObject(_posingSystem, "Combine and Save Animation");
            entry.Animation.animationClip = combinedClip;
            entry.Animation.adjustmentClip = null; // 調整アニメーションはクリア
            EditorUtility.SetDirty(_posingSystem);

            // データをリセット
            var key = GetAnimationKey(entry);
            if (!string.IsNullOrEmpty(key))
            {
                if (_allAdjustmentData.ContainsKey(key))
                {
                    for (int i = 0; i < _allAdjustmentData[key].Length; i++)
                    {
                        _allAdjustmentData[key][i] = 0f;
                    }
                }
                if (_allRootQAdjustmentData.ContainsKey(key))
                {
                    _allRootQAdjustmentData[key] = Vector3.zero;
                }
                if (_allRootTYAdjustmentData.ContainsKey(key))
                {
                    _allRootTYAdjustmentData[key] = 0f;
                }
            }

            // 現在の調整値もリセット
            if (_adjustments != null)
            {
                for (int i = 0; i < _adjustments.adjustments.Length; i++)
                {
                    _adjustments.adjustments[i] = 0f;
                }
                _adjustments.rootQAdjustments = Vector3.zero;
                _adjustments.rootTYAdjustment = 0f;
            }

            ApplyPreviewPose();
            Repaint();

            EditorUtility.DisplayDialog("完了", $"アニメーションを合成して保存しました。\n新しいファイル: {newPath}", "OK");
        }

        private void DuplicateAdjustmentClip(AnimationEntry entry)
        {
            if (entry.Animation.adjustmentClip == null)
            {
                EditorUtility.DisplayDialog("エラー", "複製する調整アニメーションが存在しません。", "OK");
                return;
            }

            // 新しいファイル名を生成
            var originalPath = AssetDatabase.GetAssetPath(entry.Animation.adjustmentClip);
            var newPath = AssetDatabase.GenerateUniqueAssetPath(originalPath);

            // アニメーションを複製
            var duplicatedClip = UnityEngine.Object.Instantiate(entry.Animation.adjustmentClip);

            AssetDatabase.CreateAsset(duplicatedClip, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // animationClipPropertyを新しいパスに更新
            Undo.RecordObject(_posingSystem, "Duplicate Adjustment Clip");
            entry.Animation.adjustmentClip = duplicatedClip;
            EditorUtility.SetDirty(_posingSystem);
        }

        private void DuplicateAndSaveAdjustmentClip()
        {
            var entry = GetSelectedEntry();
            if (entry == null || _adjustments == null)
            {
                return;
            }

            // 現在の調整アニメーションが存在しない場合は通常の保存を実行
            if (entry.Animation.adjustmentClip == null)
            {
                SaveAdjustmentClip();
                return;
            }

            // 調整アニメーションを複製
            var originalPath = AssetDatabase.GetAssetPath(entry.Animation.adjustmentClip);
            var newPath = AssetDatabase.GenerateUniqueAssetPath(originalPath);

            // アニメーションを複製
            var duplicatedClip = UnityEngine.Object.Instantiate(entry.Animation.adjustmentClip);
            AssetDatabase.CreateAsset(duplicatedClip, newPath);

            // 複製したクリップを現在の調整アニメーションとして設定
            Undo.RecordObject(_posingSystem, "Duplicate and Save Adjustment Clip");
            entry.Animation.adjustmentClip = duplicatedClip;
            EditorUtility.SetDirty(_posingSystem);

            // 複製したクリップに現在の調整値を保存
            var key = GetAnimationKey(entry);
            if (!string.IsNullOrEmpty(key))
            {
                var adjustmentData = _allAdjustmentData.TryGetValue(key, out var data) ? data : _adjustments.adjustments;
                var rootQAdjustments = _allRootQAdjustmentData.TryGetValue(key, out var rootQ) ? rootQ : _adjustments.rootQAdjustments;
                var rootTYAdjustment = _allRootTYAdjustmentData.TryGetValue(key, out var rootTY) ? rootTY : _adjustments.rootTYAdjustment;

                SaveAdjustmentClipForEntry(entry, adjustmentData, rootQAdjustments, rootTYAdjustment);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // 元データを更新（変更検知をリセット）
            UpdateOriginalDataAfterClipChange();

            EditorUtility.DisplayDialog("完了", $"調整アニメーションを複製して保存しました。\n新しいファイル: {newPath}", "OK");
        }

        private void DuplicateAndSaveAllAdjustmentClips()
        {
            if (!EditorUtility.DisplayDialog("確認", "全ての調整データを複製保存しますか？\n変更のあるアニメーションの調整アニメーションが複製され、変更が保存されます。", "はい", "いいえ"))
            {
                return;
            }

            int processedCount = 0;
            foreach (var entry in _animationEntries)
            {
                var key = GetAnimationKey(entry);
                if (string.IsNullOrEmpty(key)) continue;
                
                // 変更があるかチェック
                if (!HasAnimationChanges(key)) continue;
                
                // 調整アニメーションが存在しない場合は通常の保存
                if (entry.Animation.adjustmentClip == null)
                {
                    var adjustmentData = _allAdjustmentData.TryGetValue(key, out var data) ? data : new float[HumanTrait.MuscleCount];
                    var rootQAdjustments = _allRootQAdjustmentData.TryGetValue(key, out var rootQ) ? rootQ : Vector3.zero;
                    var rootTYAdjustment = _allRootTYAdjustmentData.TryGetValue(key, out var rootTY) ? rootTY : 0f;
                    
                    SaveAdjustmentClipForEntry(entry, adjustmentData, rootQAdjustments, rootTYAdjustment);
                    processedCount++;
                    continue;
                }
                
                // 調整アニメーションを複製して保存
                var originalPath = AssetDatabase.GetAssetPath(entry.Animation.adjustmentClip);
                var newPath = AssetDatabase.GenerateUniqueAssetPath(originalPath);

                // アニメーションを複製
                var duplicatedClip = UnityEngine.Object.Instantiate(entry.Animation.adjustmentClip);
                AssetDatabase.CreateAsset(duplicatedClip, newPath);

                // 複製したクリップを現在の調整アニメーションとして設定
                entry.Animation.adjustmentClip = duplicatedClip;

                // 複製したクリップに現在の調整値を保存
                var adjustmentData2 = _allAdjustmentData.TryGetValue(key, out var data2) ? data2 : new float[HumanTrait.MuscleCount];
                var rootQAdjustments2 = _allRootQAdjustmentData.TryGetValue(key, out var rootQ2) ? rootQ2 : Vector3.zero;
                var rootTYAdjustment2 = _allRootTYAdjustmentData.TryGetValue(key, out var rootTY2) ? rootTY2 : 0f;

                SaveAdjustmentClipForEntry(entry, adjustmentData2, rootQAdjustments2, rootTYAdjustment2);
                processedCount++;
            }

            if (processedCount > 0)
            {
                Undo.RecordObject(_posingSystem, "Duplicate and Save All Adjustment Clips");
                EditorUtility.SetDirty(_posingSystem);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                
                // 全ての元データを更新（変更検知をリセット）
                PreloadAllAdjustmentData();
                LoadAdjustmentValues();
                
                EditorUtility.DisplayDialog("完了", $"{processedCount}個の調整アニメーションを複製保存しました。", "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("情報", "複製保存する変更はありませんでした。", "OK");
            }
        }
        
        private void DrawAdjustmentControls(bool enabled)
        {
            using (new EditorGUI.DisabledScope(!enabled))
            {
                // 現在の姿勢のボタン
                using (new EditorGUILayout.HorizontalScope())
                {
                    var hasChanges = HasAnyChanges();
                    var hasAdjustmentClip = HasAdjustmentClip();
                    var hasAnyAdjustments = HasAnyAdjustments();

                    // 初期化（調整値がある時のみ有効）
                    using (new EditorGUI.DisabledScope(!hasAnyAdjustments))
                    {
                        if (GUILayout.Button(new GUIContent("初期化", "調整値をすべて0にリセットします。")))
                        {
                            ResetAllAdjustments();
                        }
                    }

                    // 再読み込み（adjustmentClipがあるか、変更がある時のみ有効）
                    using (new EditorGUI.DisabledScope(!hasAdjustmentClip || !hasChanges))
                    {
                        if (GUILayout.Button(new GUIContent("再読込", "ファイルに保存されている値に戻します。")))
                        {
                            ReloadCurrentAdjustmentFromFile();
                        }
                    }

                    // 保存（変更がある時のみ有効）
                    using (new EditorGUI.DisabledScope(!hasChanges))
                    {
                        if (GUILayout.Button(new GUIContent("保存", "調整値をファイルに保存します。")))
                        {
                            SaveAdjustmentClip();
                        }
                    }

                    // 複製して保存（変更がある時のみ有効）
                    using (new EditorGUI.DisabledScope(!hasChanges))
                    {
                        if (GUILayout.Button(new GUIContent("複製して保存", "調整アニメーションを複製して保存します。元々設定されていたファイルは変更されません")))
                        {
                            DuplicateAndSaveAdjustmentClip();
                        }
                    }
                }

                EditorGUILayout.Space(5);

                using (var scroll = new EditorGUILayout.ScrollViewScope(_muscleScroll))
                {
                    _muscleScroll = scroll.scrollPosition;

                    // Root Transform Position Y (RootTY) セクション
                    DrawRootTYSection();

                    EditorGUILayout.Space(10);

                    // Root Transform Rotation (RootQ) セクション
                    DrawRootTransformSection();

                    foreach (var group in _muscleGroups)
                    {
                        if (!_foldoutStates.TryGetValue(group.Name, out var expanded))
                        {
                            expanded = true;
                            _foldoutStates[group.Name] = true;
                        }

                        expanded = EditorGUILayout.Foldout(expanded, group.Name, true);
                        _foldoutStates[group.Name] = expanded;
                        if (!expanded)
                        {
                            continue;
                        }

                        EditorGUI.indentLevel++;
                        foreach (var muscleIndex in group.Indexes)
                        {
                            DrawMuscleSlider(muscleIndex);
                        }
                        EditorGUI.indentLevel--;
                        EditorGUILayout.Space(4f);
                    }
                }
            }
        }

        private void DrawRootTYSection()
        {
            // Root Transform Position Y (Height)
            if (!_foldoutStates.TryGetValue("Root Height", out var rootTYExpanded))
            {
                rootTYExpanded = true;
                _foldoutStates["Root Height"] = true;
            }

            rootTYExpanded = EditorGUILayout.Foldout(rootTYExpanded, "高さ", true);
            _foldoutStates["Root Height"] = rootTYExpanded;
            
            if (rootTYExpanded)
            {
                EditorGUI.indentLevel++;
                
                using (new EditorGUILayout.HorizontalScope())
                {
                    var currentValue = _adjustments.rootTYAdjustment;
                    var min = -0.2f;
                    var max = 0.2f;
                    
                    // 限界突破の状態を取得
                    var isLimitBreak = GetRootTYLimitBreakState();
                    if (isLimitBreak)
                    {
                        min = -1.0f;
                        max = 1.0f;
                    }
                    
                    EditorGUI.BeginChangeCheck();
                    var newValue = EditorGUILayout.Slider("高さ", currentValue, min, max);
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_adjustments, $"Change RootTY adjustment");
                        _adjustments.rootTYAdjustment = newValue;
                        SaveCurrentAdjustmentData();
                        ApplyPreviewPose();
                    }
                    
                    // 限界突破トグル
                    EditorGUI.BeginChangeCheck();
                    var newLimitBreak = EditorGUILayout.ToggleLeft(new GUIContent("限界突破", "限界突破をオンにするとパラメータの設定幅が５倍になります"), isLimitBreak, GUILayout.Width(80));
                    if (EditorGUI.EndChangeCheck())
                    {
                        SetRootTYLimitBreakState(newLimitBreak);
                        
                        // 現在の値が新しい範囲を超えている場合はクランプ
                        var newMin = newLimitBreak ? -1.0f : -0.2f;
                        var newMax = newLimitBreak ? 1.0f : 0.2f;
                        
                        if (currentValue < newMin || currentValue > newMax)
                        {
                            Undo.RecordObject(_adjustments, "Clamp RootTY value due to limit break change");
                            _adjustments.rootTYAdjustment = Mathf.Clamp(currentValue, newMin, newMax);
                            SaveCurrentAdjustmentData();
                            ApplyPreviewPose();
                        }
                    }
                    
                    // Uボタン（再読込）
                    if (GUILayout.Button(new GUIContent("U", "変更を破棄して保存されていた値に戻します"), GUILayout.Width(20)))
                    {
                        ReloadRootTYParameter();
                    }
                    
                    // ×ボタン（0リセット）
                    if (GUILayout.Button(new GUIContent("×", "0にリセットします"), GUILayout.Width(20)))
                    {
                        Undo.RecordObject(_adjustments, "Reset RootTY adjustment");
                        _adjustments.rootTYAdjustment = 0f;
                        SaveCurrentAdjustmentData();
                        ApplyPreviewPose();
                    }
                }
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }
        }

        private void DrawRootTransformSection()
        {
            // Root Transform Rotation (RootQ)
            if (!_foldoutStates.TryGetValue("Root Rotation", out var rootExpanded))
            {
                rootExpanded = true;
                _foldoutStates["Root Rotation"] = true;
            }

            rootExpanded = EditorGUILayout.Foldout(rootExpanded, "回転", true);
            _foldoutStates["Root Rotation"] = rootExpanded;
            
            if (rootExpanded)
            {
                EditorGUI.indentLevel++;
                
                // Y軸（回転）
                DrawRootQSlider("Y", "回転", 1, ref _adjustments.rootQAdjustments);
                
                // X軸（前後傾き）
                DrawRootQSlider("X", "前後傾き", 0, ref _adjustments.rootQAdjustments);
                
                // Z軸（左右傾き）
                DrawRootQSlider("Z", "左右傾き", 2, ref _adjustments.rootQAdjustments);
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space(5);
            }
        }

        private void DrawRootQSlider(string axisName, string displayName, int axisIndex, ref Vector3 rootQAdjustments)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var currentValue = rootQAdjustments[axisIndex];
                var min = -36f;
                var max = 36f;
                
                // 限界突破の状態を取得
                var isLimitBreak = GetRootQLimitBreakState(axisIndex);
                if (isLimitBreak)
                {
                    min = -180f;
                    max = 180f;
                }
                
                EditorGUI.BeginChangeCheck();
                var newValue = EditorGUILayout.Slider($"{axisName} ({displayName})", currentValue, min, max);
                
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(_adjustments, $"Change RootQ {displayName} adjustment");
                    var newRootQ = rootQAdjustments;
                    newRootQ[axisIndex] = newValue;
                    rootQAdjustments = newRootQ;
                    SaveCurrentAdjustmentData();
                    ApplyPreviewPose();
                }
                
                // 限界突破トグル
                EditorGUI.BeginChangeCheck();
                var newLimitBreak = EditorGUILayout.ToggleLeft(new GUIContent("限界突破", "限界突破をオンにするとパラメータの設定幅が５倍になります"), isLimitBreak, GUILayout.Width(80));
                if (EditorGUI.EndChangeCheck())
                {
                    SetRootQLimitBreakState(axisIndex, newLimitBreak);
                    
                    // 現在の値が新しい範囲を超えている場合はクランプ
                    var newMin = newLimitBreak ? -180f : -36f;
                    var newMax = newLimitBreak ? 180f : 36f;
                    
                    if (currentValue < newMin || currentValue > newMax)
                    {
                        Undo.RecordObject(_adjustments, $"Clamp RootQ {displayName} value due to limit break change");
                        var newRootQ = rootQAdjustments;
                        newRootQ[axisIndex] = Mathf.Clamp(currentValue, newMin, newMax);
                        rootQAdjustments = newRootQ;
                        SaveCurrentAdjustmentData();
                        ApplyPreviewPose();
                    }
                }
                
                // Uボタン（再読込）
                if (GUILayout.Button(new GUIContent("U", "変更を破棄して保存されていた値に戻します"), GUILayout.Width(20)))
                {
                    ReloadRootQParameter(axisIndex);
                }
                
                // ×ボタン（0リセット）
                if (GUILayout.Button(new GUIContent("×", "0にリセットします"), GUILayout.Width(20)))
                {
                    Undo.RecordObject(_adjustments, $"Reset RootQ {displayName} adjustment");
                    var newRootQ = rootQAdjustments;
                    newRootQ[axisIndex] = 0f;
                    rootQAdjustments = newRootQ;
                    SaveCurrentAdjustmentData();
                    ApplyPreviewPose();
                }
            }
        }

        private void DrawMuscleSlider(int muscleIndex)
        {
            var label = GetMuscleNameJapanese(muscleIndex);
            var defaultMin = HumanTrait.GetMuscleDefaultMin(muscleIndex);
            var defaultMax = HumanTrait.GetMuscleDefaultMax(muscleIndex);
            
            // 限界突破の状態を取得
            var isLimitBreak = GetLimitBreakState(muscleIndex);
            var actualMin = isLimitBreak ? defaultMin * 5f : defaultMin;
            var actualMax = isLimitBreak ? defaultMax * 5f : defaultMax;
            
            // 変更があるかチェック
            var hasChanges = HasMuscleChanges(muscleIndex);
            
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUI.BeginChangeCheck();
                
                // 変更がある場合は太文字スタイルでスライダーを描画
                if (hasChanges)
                {
                    var origFontStyle = EditorStyles.label.fontStyle;
                    EditorStyles.label.fontStyle = FontStyle.Bold;
                    var newValue = EditorGUILayout.Slider(new GUIContent(label), _adjustments.adjustments[muscleIndex], actualMin, actualMax);
                    EditorStyles.label.fontStyle = origFontStyle;

                    if (EditorGUI.EndChangeCheck())
                    {
                        // Undoが効くようにする
                        Undo.RecordObject(_adjustments, "Update muscle adjustment");
                        _adjustments.adjustments[muscleIndex] = newValue;
                        SaveCurrentAdjustmentData();
                        ApplyPreviewPose();
                    }
                }
                else
                {
                    var newValue = EditorGUILayout.Slider(label, _adjustments.adjustments[muscleIndex], actualMin, actualMax);
                    
                    if (EditorGUI.EndChangeCheck())
                    {
                        // Undoが効くようにする
                        Undo.RecordObject(_adjustments, "Update muscle adjustment");
                        _adjustments.adjustments[muscleIndex] = newValue;
                        SaveCurrentAdjustmentData();
                        ApplyPreviewPose();
                    }
                }
                
                // 限界突破チェックボックス
                EditorGUI.BeginChangeCheck();
                var newLimitBreak = EditorGUILayout.ToggleLeft("限界突破", isLimitBreak, GUILayout.Width(80));
                if (EditorGUI.EndChangeCheck())
                {
                    SetLimitBreakState(muscleIndex, newLimitBreak);
                    
                    // 現在の値が新しい範囲を超えている場合はクランプ
                    var currentValue = _adjustments.adjustments[muscleIndex];
                    var newActualMin = newLimitBreak ? defaultMin * 5f : defaultMin;
                    var newActualMax = newLimitBreak ? defaultMax * 5f : defaultMax;
                    
                    if (currentValue < newActualMin || currentValue > newActualMax)
                    {
                        Undo.RecordObject(_adjustments, "Clamp muscle adjustment for limit break");
                        _adjustments.adjustments[muscleIndex] = Mathf.Clamp(currentValue, newActualMin, newActualMax);
                        SaveCurrentAdjustmentData();
                        ApplyPreviewPose();
                    }
                }
                
                // Uボタン（再読込）
                if (GUILayout.Button(new GUIContent("U", "変更を破棄して保存されていた値に戻します"), GUILayout.Width(20), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    ReloadMuscleParameter(muscleIndex);
                }
                
                // リセットボタン
                if (GUILayout.Button(new GUIContent("×", "0にリセットします"), GUILayout.Width(20), GUILayout.Height(EditorGUIUtility.singleLineHeight)))
                {
                    // 値を0にリセット
                    Undo.RecordObject(_adjustments, "Reset muscle adjustment");
                    _adjustments.adjustments[muscleIndex] = 0f;
                    SaveCurrentAdjustmentData();
                    ApplyPreviewPose();
                }
            }
        }

        private void ReloadRootTYParameter()
        {
            var entry = GetSelectedEntry();
            if (entry == null || _adjustments == null) return;
            
            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return;

            // 元の値を取得
            var originalValue = _originalRootTYAdjustmentData.TryGetValue(key, out var original) ? original : 0f;
            
            _adjustments.rootTYAdjustment = originalValue;
            Undo.RecordObject(_adjustments, "Reload RootTY parameter");
            
            // 現在のデータも更新
            _allRootTYAdjustmentData[key] = originalValue;
            
            SaveCurrentAdjustmentData();
            ApplyPreviewPose();
            Repaint();
        }

        private void ReloadRootQParameter(int axisIndex)
        {
            var entry = GetSelectedEntry();
            if (entry == null || _adjustments == null) return;
            
            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return;

            // 元の値を取得
            var originalRootQ = _originalRootQAdjustmentData.TryGetValue(key, out var original) ? original : Vector3.zero;
            
            var newRootQ = _adjustments.rootQAdjustments;
            newRootQ[axisIndex] = originalRootQ[axisIndex];
            _adjustments.rootQAdjustments = newRootQ;
            Undo.RecordObject(_adjustments, $"Reload RootQ parameter {axisIndex}");
            
            // 現在のデータも更新
            _allRootQAdjustmentData[key] = _adjustments.rootQAdjustments;
            
            SaveCurrentAdjustmentData();
            ApplyPreviewPose();
            Repaint();
        }

        private void ReloadMuscleParameter(int muscleIndex)
        {
            var entry = GetSelectedEntry();
            if (entry == null || _adjustments == null) return;
            
            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return;

            // 元の値を取得
            var originalValue = 0f;
            if (_originalAdjustmentData.TryGetValue(key, out var originalData) && muscleIndex < originalData.Length)
            {
                originalValue = originalData[muscleIndex];
            }
            
            _adjustments.adjustments[muscleIndex] = originalValue;
            Undo.RecordObject(_adjustments, $"Reload muscle parameter {HumanTrait.MuscleName[muscleIndex]}");

            // 現在のデータも更新
            if (_allAdjustmentData.TryGetValue(key, out var currentData) && muscleIndex < currentData.Length)
            {
                currentData[muscleIndex] = originalValue;
            }
            
            SaveCurrentAdjustmentData();
            ApplyPreviewPose();
            Repaint();
        }

        private AnimationEntry GetSelectedEntry()
        {
            if (_selectedAnimationIndex < 0 || _selectedAnimationIndex >= _animationEntries.Count)
            {
                return null;
            }
            return _animationEntries[_selectedAnimationIndex];
        }

        private void RebuildAnimationEntries()
        {
            _animationEntries.Clear();
            _animationLabels = Array.Empty<string>();
            if (_posingSystem == null || _posingSystem.defines == null)
            {
                return;
            }

            foreach (var layer in _posingSystem.defines)
            {
                if (layer?.animations == null)
                {
                    continue;
                }
                foreach (var animation in layer.animations)
                {
                    if (animation == null)
                    {
                        continue;
                    }
                    var entry = new AnimationEntry
                    {
                        Layer = layer,
                        Animation = animation,
                        Label = $"{layer.menuName} / {animation.displayName}"
                    };
                    _animationEntries.Add(entry);
                }
            }
            _animationLabels = _animationEntries.Select(e => e.Label).ToArray();
        }

        private void LoadAdjustmentValues()
        {
            _adjustments = CreateInstance<Adjustments>();
            _adjustments.adjustments = new float[HumanTrait.MuscleCount];
            _adjustments.rootQAdjustments = Vector3.zero;
            _adjustments.rootTYAdjustment = 0f;

            if (_limitBreakStates == null)
            {
                _limitBreakStates = CreateInstance<LimitBreakStates>();
            }
            
            var entry = GetSelectedEntry();
            if (entry == null) return;
            
            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return;

            // 事前読み込みされたデータを使用
            if (_allAdjustmentData.TryGetValue(key, out var existingData))
            {
                Array.Copy(existingData, _adjustments.adjustments, HumanTrait.MuscleCount);
            }
            
            // RootQの値も読み込み
            if (_allRootQAdjustmentData.TryGetValue(key, out var existingRootQ))
            {
                _adjustments.rootQAdjustments = existingRootQ;
            }
            
            // RootTYの値も読み込み
            if (_allRootTYAdjustmentData.TryGetValue(key, out var existingRootTY))
            {
                _adjustments.rootTYAdjustment = existingRootTY;
            }

            // 前回の値を保存（Undo検知用）
            UpdatePreviousAdjustmentValues();
            
            ApplyPreviewPose();
            Repaint();
        }

        private void ReloadCurrentAdjustmentFromFile()
        {
            var entry = GetSelectedEntry();
            if (entry == null) return;
            
            var key = GetAnimationKey(entry);
            if (string.IsNullOrEmpty(key)) return;

            // ファイルから直接読み込み
            var fileData = new float[HumanTrait.MuscleCount];
            var fileRootQ = Vector3.zero;
            var fileRootTY = 0f;
            if (entry.Animation?.adjustmentClip != null)
            {
                for (int i = 0; i < HumanTrait.MuscleCount; i++)
                {
                    var binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), HumanTrait.MuscleName[i]);
                    var curve = AnimationUtility.GetEditorCurve(entry.Animation.adjustmentClip, binding);
                    if (curve != null && curve.keys.Length > 0)
                    {
                        var min = HumanTrait.GetMuscleDefaultMin(i);
                        var max = HumanTrait.GetMuscleDefaultMax(i);
                        var muscleValue = curve.keys[0].value;
                        if (muscleValue > 0)
                        {
                            fileData[i] = muscleValue * max;
                        }
                        else
                        {
                            fileData[i] = muscleValue * -min;
                        }
                        
                        // 限界突破の自動検出
                        if (fileData[i] > max || fileData[i] < min)
                        {
                            SetLimitBreakState(i, true);
                        }
                    }
                }
                
                // RootQの値を読み込み
                var bindingQX = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.x");
                var bindingQY = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.y");
                var bindingQZ = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.z");
                var bindingQW = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.w");
                
                var curveQX = AnimationUtility.GetEditorCurve(entry.Animation.adjustmentClip, bindingQX);
                var curveQY = AnimationUtility.GetEditorCurve(entry.Animation.adjustmentClip, bindingQY);
                var curveQZ = AnimationUtility.GetEditorCurve(entry.Animation.adjustmentClip, bindingQZ);
                var curveQW = AnimationUtility.GetEditorCurve(entry.Animation.adjustmentClip, bindingQW);
                
                if (curveQX != null || curveQY != null || curveQZ != null || curveQW != null)
                {
                    var qx = curveQX?.keys.Length > 0 ? curveQX.keys[0].value : 0f;
                    var qy = curveQY?.keys.Length > 0 ? curveQY.keys[0].value : 0f;
                    var qz = curveQZ?.keys.Length > 0 ? curveQZ.keys[0].value : 0f;
                    var qw = curveQW?.keys.Length > 0 ? curveQW.keys[0].value : 1f;
                    
                    var quaternion = new Quaternion(qx, qy, qz, qw);
                    fileRootQ = quaternion.eulerAngles;
                    
                    // 角度を-180〜180の範囲に正規化
                    if (fileRootQ.x > 180f) fileRootQ.x -= 360f;
                    if (fileRootQ.y > 180f) fileRootQ.y -= 360f;
                    if (fileRootQ.z > 180f) fileRootQ.z -= 360f;
                    
                    // RootQの限界突破自動検出
                    if (Mathf.Abs(fileRootQ.x) > 36f) SetRootQLimitBreakState(0, true);
                    if (Mathf.Abs(fileRootQ.y) > 36f) SetRootQLimitBreakState(1, true);
                    if (Mathf.Abs(fileRootQ.z) > 36f) SetRootQLimitBreakState(2, true);
                }
                
                // RootTYの値を読み込み
                var bindingTY = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootT.y");
                var curveTY = AnimationUtility.GetEditorCurve(entry.Animation.adjustmentClip, bindingTY);
                if (curveTY != null && curveTY.keys.Length > 0)
                {
                    fileRootTY = curveTY.keys[0].value;
                    
                    // RootTYの限界突破自動検出
                    if (Mathf.Abs(fileRootTY) > 0.2f)
                    {
                        SetRootTYLimitBreakState(true);
                    }
                }
            }

            // 現在の調整値を更新
            if (_adjustments != null)
            {
                Undo.RecordObject(_adjustments, "Reload adjustment from file");
                Array.Copy(fileData, _adjustments.adjustments, HumanTrait.MuscleCount);
                _adjustments.rootQAdjustments = fileRootQ;
                _adjustments.rootTYAdjustment = fileRootTY;
            }

            // 変更データも更新
            if (_allAdjustmentData.ContainsKey(key))
            {
                Array.Copy(fileData, _allAdjustmentData[key], HumanTrait.MuscleCount);
            }
            _allRootQAdjustmentData[key] = fileRootQ;
            _allRootTYAdjustmentData[key] = fileRootTY;

            // 前回の値を保存（Undo検知用）
            UpdatePreviousAdjustmentValues();
            
            ApplyPreviewPose();
            Repaint();
        }


        private void ResetAllAdjustments()
        {
            if (_adjustments == null)
            {
                return;
            }
            
            Undo.RecordObject(_adjustments, "Reset all adjustments");
            for (int i = 0; i < _adjustments.adjustments.Length; i++)
            {
                _adjustments.adjustments[i] = 0f;
            }
            _adjustments.rootQAdjustments = Vector3.zero;
            _adjustments.rootTYAdjustment = 0f;
            SaveCurrentAdjustmentData();
            ApplyPreviewPose();
            Repaint();
        }

        private void SaveAllAdjustmentClips()
        {
            foreach (var entry in _animationEntries)
            {
                var key = GetAnimationKey(entry);
                if (string.IsNullOrEmpty(key)) continue;
                
                if (!_allAdjustmentData.TryGetValue(key, out var adjustmentData)) continue;
                
                // 変更があるかチェック
                if (!HasAnimationChanges(key)) continue;
                
                // RootQとRootTYの値も取得
                var rootQAdjustments = _allRootQAdjustmentData.TryGetValue(key, out var rootQ) ? rootQ : Vector3.zero;
                var rootTYAdjustment = _allRootTYAdjustmentData.TryGetValue(key, out var rootTY) ? rootTY : 0f;
                
                SaveAdjustmentClipForEntry(entry, adjustmentData, rootQAdjustments, rootTYAdjustment);
                
                // 保存後に元データを更新（※マークを消すため）
                if (!string.IsNullOrEmpty(key))
                {
                    if (!_originalAdjustmentData.ContainsKey(key))
                    {
                        _originalAdjustmentData[key] = new float[HumanTrait.MuscleCount];
                    }
                    Array.Copy(adjustmentData, _originalAdjustmentData[key], HumanTrait.MuscleCount);
                    _originalRootQAdjustmentData[key] = rootQAdjustments;
                    _originalRootTYAdjustmentData[key] = rootTYAdjustment;
                }
            }
            
            EditorUtility.DisplayDialog("完了", "全ての変更された差分を保存しました。", "OK");
            PosingSystemConverter.TakeScreenshot(_posingSystem, false, true);
            CleanupPreview();
            ApplyPreviewPose();
            Repaint();
        }

        private void ResetAllAnimationsAdjustments()
        {
            if (EditorUtility.DisplayDialog("確認", "全ての姿勢の調整値をリセットしますか？", "はい", "いいえ"))
            {
                // 存在する全ての調整値を0に変更する
                foreach (var kvp in _allAdjustmentData)
                {
                    var key = kvp.Key;
                    var adjustmentData = kvp.Value;
                    for (int i = 0; i < HumanTrait.MuscleCount; i++)
                    {
                        adjustmentData[i] = 0f;
                    }
                }
                
                // 存在する全てのRootQ調整値も0に変更する
                foreach (var key in _allRootQAdjustmentData.Keys.ToList())
                {
                    _allRootQAdjustmentData[key] = Vector3.zero;
                }
                
                // 存在する全てのRootTY調整値も0に変更する
                foreach (var key in _allRootTYAdjustmentData.Keys.ToList())
                {
                    _allRootTYAdjustmentData[key] = 0f;
                }

                Undo.RecordObject(_adjustments, "Reset all adjustments");
                for (int i = 0; i < _adjustments.adjustments.Length; i++)
                {
                    _adjustments.adjustments[i] = 0f;
                }
                _adjustments.rootQAdjustments = Vector3.zero;
                _adjustments.rootTYAdjustment = 0f;

                SaveCurrentAdjustmentData();
                CleanupPreview();
                ApplyPreviewPose();
                Repaint();
            }
        }

        private void ReloadAllAnimationsAdjustments()
        {
            if (EditorUtility.DisplayDialog("確認", "全ての姿勢の調整値を再読み込みしますか？未保存の変更は失われます。", "はい", "いいえ"))
            {
                // 全ての姿勢の調整値を再読み込み
                PreloadAllAdjustmentData();
                LoadAdjustmentValues();
            }
        }

        private void SaveAdjustmentClip()
        {
            var entry = GetSelectedEntry();
            if (entry == null || _adjustments == null)
            {
                return;
            }

            SaveAdjustmentClipForEntry(entry, _adjustments.adjustments, _adjustments.rootQAdjustments, _adjustments.rootTYAdjustment);
            PosingSystemConverter.TakeScreenshot(_posingSystem, false, true);
            CleanupPreview();
            ApplyPreviewPose();
            Repaint();
        }

        private void SaveAdjustmentClipForEntry(AnimationEntry entry, float[] adjustmentData, Vector3 rootQAdjustments = default, float rootTYAdjustment = 0f)
        {
            Undo.RecordObject(_posingSystem, "Update adjustment clip");

            var clip = entry.Animation.adjustmentClip;
            if (clip == null)
            {
                clip = CreateAdjustmentClipAsset(entry);
                entry.Animation.adjustmentClip = clip;
                entry.Animation.previewImage = null;
            }

            if (clip == null)
            {
                return;
            }

            Undo.RegisterCompleteObjectUndo(clip, "Update adjustment clip curves");
            clip.ClearCurves();

            for (int i = 0; i < HumanTrait.MuscleCount; i++)
            {
                var min = HumanTrait.GetMuscleDefaultMin(i);
                var max = HumanTrait.GetMuscleDefaultMax(i);
                var value = adjustmentData[i];
                if (value > 0)
                {
                    value = value / max;
                }
                else
                {
                    value = value / -min;
                }
                var binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), HumanTrait.MuscleName[i]);
                if (Mathf.Abs(value) < 0.0001f)
                {
                    AnimationUtility.SetEditorCurve(clip, binding, null);
                    continue;
                }

                var key0 = new Keyframe(0f, value, 0f, 0f);
                var key1 = new Keyframe(1f / 60f, value, 0f, 0f);
                var curve = new AnimationCurve(key0, key1);
                AnimationUtility.SetEditorCurve(clip, binding, curve);
            }

            // RootTY (Root Transform Position Y) のカーブを追加
            if (Mathf.Abs(rootTYAdjustment) > 0.0001f)
            {
                var bindingTY = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootT.y");
                var key0 = new Keyframe(0f, rootTYAdjustment, 0f, 0f);
                var key1 = new Keyframe(1f / 60f, rootTYAdjustment, 0f, 0f);
                var curve = new AnimationCurve(key0, key1);
                AnimationUtility.SetEditorCurve(clip, bindingTY, curve);
            }

            // RootQ (Root Transform Rotation) のカーブを追加
            if (rootQAdjustments != Vector3.zero)
            {
                var bindingX = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.x");
                var bindingY = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.y");
                var bindingZ = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.z");
                var bindingW = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.w");
                
                var quaternion = Quaternion.Euler(rootQAdjustments);
                
                if (Mathf.Abs(quaternion.x) > 0.0001f)
                {
                    var key0 = new Keyframe(0f, quaternion.x, 0f, 0f);
                    var key1 = new Keyframe(1f / 60f, quaternion.x, 0f, 0f);
                    var curve = new AnimationCurve(key0, key1);
                    AnimationUtility.SetEditorCurve(clip, bindingX, curve);
                }
                
                if (Mathf.Abs(quaternion.y) > 0.0001f)
                {
                    var key0 = new Keyframe(0f, quaternion.y, 0f, 0f);
                    var key1 = new Keyframe(1f / 60f, quaternion.y, 0f, 0f);
                    var curve = new AnimationCurve(key0, key1);
                    AnimationUtility.SetEditorCurve(clip, bindingY, curve);
                }
                
                if (Mathf.Abs(quaternion.z) > 0.0001f)
                {
                    var key0 = new Keyframe(0f, quaternion.z, 0f, 0f);
                    var key1 = new Keyframe(1f / 60f, quaternion.z, 0f, 0f);
                    var curve = new AnimationCurve(key0, key1);
                    AnimationUtility.SetEditorCurve(clip, bindingZ, curve);
                }
                
                if (Mathf.Abs(quaternion.w - 1f) > 0.0001f) // wは通常1なので、1からの差分をチェック
                {
                    var key0 = new Keyframe(0f, quaternion.w, 0f, 0f);
                    var key1 = new Keyframe(1f / 60f, quaternion.w, 0f, 0f);
                    var curve = new AnimationCurve(key0, key1);
                    AnimationUtility.SetEditorCurve(clip, bindingW, curve);
                }
            }

            entry.Animation.previewImage = null;
            EditorUtility.SetDirty(clip);
            EditorUtility.SetDirty(_posingSystem);
            AssetDatabase.SaveAssets();
            
            // 保存した値を新しい元データとして更新
            var key = GetAnimationKey(entry);
            if (!string.IsNullOrEmpty(key))
            {
                if (!_originalAdjustmentData.ContainsKey(key))
                {
                    _originalAdjustmentData[key] = new float[HumanTrait.MuscleCount];
                }
                Array.Copy(adjustmentData, _originalAdjustmentData[key], HumanTrait.MuscleCount);
                
                // RootQとRootTYの元データも更新
                _originalRootQAdjustmentData[key] = rootQAdjustments;
                _originalRootTYAdjustmentData[key] = rootTYAdjustment;
            }
        }

        private AnimationClip CreateAdjustmentClipAsset(AnimationEntry entry)
        {
            EnsureAdjustmentFolderExists();
            var toolName = SanitizeFileName(_posingSystem.name);
            var avatarName = SanitizeFileName(_posingSystem.GetAvatar().gameObject.name);
            var animationName = SanitizeFileName(entry.Animation?.animationClip?.name);
            var clipName = $"{toolName}_{avatarName}_{animationName}";
            if (string.IsNullOrWhiteSpace(clipName))
            {
                clipName = "Adjustment";
            }
            var folder = GetAdjustmentFolderPath();
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            var path = AssetDatabase.GenerateUniqueAssetPath($"{folder}/{clipName}.anim");
            var clip = new AnimationClip
            {
                name = System.IO.Path.GetFileNameWithoutExtension(path)
            };
            AssetDatabase.CreateAsset(clip, path);
            AssetDatabase.SaveAssets();
            return clip;
        }

        private void EnsureAdjustmentFolderExists()
        {
            EnsureFolderRecursive(GetAdjustmentFolderPath());
        }

        private static void EnsureFolderRecursive(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            var separatorIndex = folderPath.LastIndexOf('/')
                                 >= 0 ? folderPath.LastIndexOf('/') : folderPath.LastIndexOf('\\');
            if (separatorIndex < 0)
            {
                return;
            }

            var parent = folderPath.Substring(0, separatorIndex);
            var child = folderPath.Substring(separatorIndex + 1);
            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolderRecursive(parent);
            }
            AssetDatabase.CreateFolder(parent, child);
        }

        private string GetAdjustmentFolderPath()
        {
            var label = "PosingSystemResourceFolder";
            var folder = AssetDatabase.FindAssets($"l:{label}")
                .Select(AssetDatabase.GUIDToAssetPath)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(folder))
            {
                folder = DefaultAdjustmentFolderRoot;
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                var folderObject = AssetDatabase.LoadMainAssetAtPath(folder);
                AssetDatabase.SetLabels(folderObject, new string[] { label });
            }
            var avatarName = _posingSystem.GetAvatar().gameObject.name;
            avatarName = SanitizeFileName(avatarName);
            if (string.IsNullOrEmpty(avatarName))
            {
                avatarName = "PosingSystem";
            }
            return $"{folder}/{avatarName}";
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var sanitizedChars = value
                .Select(c => InvalidFileNameChars.Contains(c) ? '_' : c)
                .ToArray();
            return new string(sanitizedChars);
        }

        private void ApplyPreviewPose()
        {
            var entry = GetSelectedEntry();
            if (entry == null)
            {
                return;
            }

            if (entry.Animation.animationClip is not AnimationClip baseClip)
            {
                return;
            }

            if (!_previewActive)
            {
                AnimationMode.StartAnimationMode();
                _previewActive = true;
            }

            if (_posingSystem.previewAvatarObject != null)
            {
                _previewAvatar = _posingSystem.previewAvatarObject.GetComponent<Animator>();
            }
            if (_previewAvatar == null)
            {
                _previewAvatar = _posingSystem.GetAvatar().GetComponent<Animator>();
            }
            if (_previewAvatar == null)
            {
                EditorUtility.DisplayDialog("情報", "Animator を持つアバターが見つかりません。", "OK");
                CleanupPreview();
                return;
            }

            // 初回のみ元のアバターの位置と回転を保存（累積回転問題の回避）
            if (!_initialTransformSaved && _posingSystem.GetAvatar() != null)
            {
                _initialAvatarPosition = _posingSystem.GetAvatar().transform.position;
                _initialAvatarRotation = _posingSystem.GetAvatar().transform.rotation;
                _initialTransformSaved = true;
            }
            
            // プレビューアバターが元のアバターと異なる場合のみ、元のアバターを非アクティブにする
            if (_posingSystem.GetAvatar() != null && _previewAvatar.gameObject != _posingSystem.GetAvatar().gameObject)
            {
                _posingSystem.GetAvatar().gameObject.SetActive(false);
                EditorUtility.SetDirty(_posingSystem.gameObject);
            }
            
            _previewAvatar.gameObject.SetActive(true);
            EditorUtility.SetDirty(_posingSystem.gameObject);

            HumanPoseHandler poseHandler = null;
            try
            {
                AnimationMode.BeginSampling();
                AnimationMode.SampleAnimationClip(_previewAvatar.gameObject, baseClip, 0f);
                AnimationMode.EndSampling();

                // 元のアニメーションのRootQを取得（サンプリング後のアバターの回転）
                var baseRotation = _previewAvatar.transform.rotation;
                var basePosition = _previewAvatar.transform.position;

                poseHandler = new HumanPoseHandler(_previewAvatar.avatar, _previewAvatar.transform);
                var pose = new HumanPose();
                poseHandler.GetHumanPose(ref pose);

                for (int i = 0; i < HumanTrait.MuscleCount; i++)
                {
                    var min = HumanTrait.GetMuscleDefaultMin(i);
                    var max = HumanTrait.GetMuscleDefaultMax(i);
                    var addDegree = _adjustments.adjustments[i];
                    var addMuscleValue = 0f;
                    if (addDegree > 0)
                    {
                        addMuscleValue = addDegree / max;
                    }
                    else
                    {
                        addMuscleValue = addDegree / -min;
                    }

                    var muscleValue = pose.muscles[i] + addMuscleValue;
                    pose.muscles[i] = muscleValue;
                }

                poseHandler.SetHumanPose(ref pose);
            
                // プレビューアバターの位置と回転を設定
                // 元のアニメーションのRootTに調整を加算
                _previewAvatar.transform.position = basePosition + new Vector3(0, _adjustments.rootTYAdjustment, 0);
                // 元のアニメーションのRootQに調整を乗算（調整 × 元の回転）
                _previewAvatar.transform.rotation = Quaternion.Euler(_adjustments.rootQAdjustments) * baseRotation;

                SceneView.RepaintAll();
            }
            catch
            {
                // Avatar が Humanoid でない場合などは無視
            }
            finally
            {
                poseHandler?.Dispose();
            }
        }

        private void CleanupPreview()
        {
            if (_previewActive)
            {
                AnimationMode.StopAnimationMode();
                _previewActive = false;
            }
            if (_previewAvatar != null)
            {
                // プレビューアバターが元のアバターと異なる場合のみ非アクティブにする
                if (_posingSystem.GetAvatar() != null && _previewAvatar.gameObject != _posingSystem.GetAvatar().gameObject)
                {
                    _previewAvatar.gameObject.SetActive(false);
                    EditorUtility.SetDirty(_posingSystem.gameObject);
                }
                // プレビューアバターが元のアバターと同じ場合、元の位置・回転に戻す
                else if (_initialTransformSaved && _posingSystem.GetAvatar() != null)
                {
                    _posingSystem.GetAvatar().transform.position = _initialAvatarPosition;
                    _posingSystem.GetAvatar().transform.rotation = _initialAvatarRotation;
                }
                _previewAvatar = null;
            }
            if (_posingSystem != null && _posingSystem.GetAvatar() != null)
            {
                _posingSystem.GetAvatar().gameObject.SetActive(true);
                EditorUtility.SetDirty(_posingSystem.gameObject);
            }
            // 初期位置・回転の保存状態をリセット
            _initialTransformSaved = false;
        }

        private void PreloadAllAdjustmentData()
        {
            // データをクリア
            _allAdjustmentData.Clear();
            _allRootQAdjustmentData.Clear();
            _allRootTYAdjustmentData.Clear();
            _originalAdjustmentData.Clear();
            _originalRootQAdjustmentData.Clear();
            _originalRootTYAdjustmentData.Clear();
            if (_limitBreakStates != null)
            {
                _limitBreakStates.animationLimitBreaks.Clear();
            }
            _rootQLimitBreakStates.Clear();
            _rootTYLimitBreakStates.Clear();
            
            // 全ての姿勢の調整値を読み込み
            foreach (var entry in _animationEntries)
            {
                var key = GetAnimationKey(entry);
                if (string.IsNullOrEmpty(key)) continue;
                
                // 元データを読み込み
                var originalData = new float[HumanTrait.MuscleCount];
                var originalRootQ = Vector3.zero;
                var originalRootTY = 0f;
                var limitBreaks = new Dictionary<int, bool>();
                var rootQLimitBreaks = new Dictionary<int, bool>();
                var rootTYLimitBreak = false;
                
                if (entry.Animation?.adjustmentClip != null)
                {
                    for (int i = 0; i < HumanTrait.MuscleCount; i++)
                    {
                        var binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), HumanTrait.MuscleName[i]);
                        var curve = AnimationUtility.GetEditorCurve(entry.Animation.adjustmentClip, binding);
                        if (curve != null && curve.keys.Length > 0)
                        {
                            var min = HumanTrait.GetMuscleDefaultMin(i);
                            var max = HumanTrait.GetMuscleDefaultMax(i);
                            var muscleValue = curve.keys[0].value;
                            
                            if (muscleValue > 0)
                            {
                                originalData[i] = muscleValue * max;
                            }
                            else
                            {
                                originalData[i] = muscleValue * -min;
                            }
                            
                            // 限界突破の自動検出
                            if (originalData[i] > max || originalData[i] < min)
                            {
                                limitBreaks[i] = true;
                            }
                        }
                    }
                    
                    // RootQの値を読み込み
                    var bindingQX = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.x");
                    var bindingQY = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.y");
                    var bindingQZ = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.z");
                    var bindingQW = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.w");
                    
                    var curveQX = AnimationUtility.GetEditorCurve(entry.Animation.adjustmentClip, bindingQX);
                    var curveQY = AnimationUtility.GetEditorCurve(entry.Animation.adjustmentClip, bindingQY);
                    var curveQZ = AnimationUtility.GetEditorCurve(entry.Animation.adjustmentClip, bindingQZ);
                    var curveQW = AnimationUtility.GetEditorCurve(entry.Animation.adjustmentClip, bindingQW);
                    
                    if (curveQX != null || curveQY != null || curveQZ != null || curveQW != null)
                    {
                        var qx = curveQX?.keys.Length > 0 ? curveQX.keys[0].value : 0f;
                        var qy = curveQY?.keys.Length > 0 ? curveQY.keys[0].value : 0f;
                        var qz = curveQZ?.keys.Length > 0 ? curveQZ.keys[0].value : 0f;
                        var qw = curveQW?.keys.Length > 0 ? curveQW.keys[0].value : 1f;
                        
                        var quaternion = new Quaternion(qx, qy, qz, qw);
                        originalRootQ = quaternion.eulerAngles;
                        
                        // 角度を-180〜180の範囲に正規化
                        if (originalRootQ.x > 180f) originalRootQ.x -= 360f;
                        if (originalRootQ.y > 180f) originalRootQ.y -= 360f;
                        if (originalRootQ.z > 180f) originalRootQ.z -= 360f;
                        
                        // RootQの限界突破自動検出（±36度を超えている場合）
                        if (Mathf.Abs(originalRootQ.x) > 36f) rootQLimitBreaks[0] = true;
                        if (Mathf.Abs(originalRootQ.y) > 36f) rootQLimitBreaks[1] = true;
                        if (Mathf.Abs(originalRootQ.z) > 36f) rootQLimitBreaks[2] = true;
                    }
                    
                    // RootTYの値を読み込み
                    var bindingTY = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootT.y");
                    var curveTY = AnimationUtility.GetEditorCurve(entry.Animation.adjustmentClip, bindingTY);
                    if (curveTY != null && curveTY.keys.Length > 0)
                    {
                        originalRootTY = curveTY.keys[0].value;
                        
                        // RootTYの限界突破自動検出（±0.2を超えている場合）
                        if (Mathf.Abs(originalRootTY) > 0.2f)
                        {
                            rootTYLimitBreak = true;
                        }
                    }
                }
                
                // 元データを保存
                _originalAdjustmentData[key] = originalData;
                _originalRootQAdjustmentData[key] = originalRootQ;
                _originalRootTYAdjustmentData[key] = originalRootTY;
                
                // 限界突破状態を保存
                if (_limitBreakStates != null && (limitBreaks.Count > 0 || rootQLimitBreaks.Count > 0 || rootTYLimitBreak))
                {
                    var animData = _limitBreakStates.GetOrCreateAnimationData(key);
                    
                    // Muscle限界突破状態を設定
                    animData.muscleLimitBreaks.limitBreakMuscles.Clear();
                    foreach (var muscleIndex in limitBreaks.Keys.Where(k => limitBreaks[k]))
                    {
                        animData.muscleLimitBreaks.limitBreakMuscles.Add(muscleIndex);
                    }
                    
                    // RootQ限界突破状態を設定
                    animData.rootQLimitBreaks.limitBreakAxes.Clear();
                    foreach (var axisIndex in rootQLimitBreaks.Keys.Where(k => rootQLimitBreaks[k]))
                    {
                        animData.rootQLimitBreaks.limitBreakAxes.Add(axisIndex);
                    }
                    
                    // RootTY限界突破状態を設定
                    animData.rootTYLimitBreak = rootTYLimitBreak;
                }
                
                // 変更データとして元データのコピーを保存
                var adjustmentData = new float[HumanTrait.MuscleCount];
                Array.Copy(originalData, adjustmentData, HumanTrait.MuscleCount);
                _allAdjustmentData[key] = adjustmentData;
                _allRootQAdjustmentData[key] = originalRootQ;
                _allRootTYAdjustmentData[key] = originalRootTY;
            }
        }

        private void BuildMuscleGroups()
        {
            if (MuscleNamesJapanese == null || muscleDisplayOrder.Count == 0)
            {
                InitializeMuscleNamesJapanese();
            }

            _muscleGroups.Clear();
            
            // SetMuscleNameJapaneseの呼び出し順番でMuscleグループを構築
            var groups = new Dictionary<string, MuscleGroup>();

            foreach (int muscleIndex in muscleDisplayOrder)
            {
                var (groupName, order) = ResolveMuscleGroupName(muscleIndex);
                if (string.IsNullOrEmpty(groupName))
                {
                    continue;
                }
                if (!groups.TryGetValue(groupName, out var group))
                {
                    group = new MuscleGroup { Name = groupName, Order = order };
                    groups.Add(groupName, group);
                }
                group.Indexes.Add(muscleIndex);
            }

            _muscleGroups.AddRange(groups.Values.OrderBy(g => g.Order));
        }

        private static (string groupName, int order) ResolveMuscleGroupName(int muscleIndex)
        {
            var boneIndex = HumanTrait.BoneFromMuscle(muscleIndex);
            if (boneIndex < 0 || boneIndex >= HumanTrait.BoneCount)
            {
                return ("全身", 0);
            }

            var boneName = HumanTrait.BoneName[boneIndex];
            if (string.IsNullOrEmpty(boneName))
            {
                return ("その他", 1);
            }

            if (boneName.Contains("Thumb") || boneName.Contains("Index") || boneName.Contains("Middle") || boneName.Contains("Ring") || boneName.Contains("Little"))
            {
                return boneName.StartsWith("Right", StringComparison.OrdinalIgnoreCase) ? ("右手指", 8) : ("左手指", 9);
            }

            if (boneName.Contains("Hand"))
            {
                return boneName.StartsWith("Right", StringComparison.OrdinalIgnoreCase) ? ("右手首", 6) : ("左手首", 7);
            }

            if (boneName.Contains("Arm") || boneName.Contains("Shoulder"))
            {
                return boneName.StartsWith("Right", StringComparison.OrdinalIgnoreCase) ? ("右腕", 4) : ("左腕", 5);
            }

            if (boneName.Contains("Leg") || boneName.Contains("Foot") || boneName.Contains("Toe"))
            {
                return boneName.StartsWith("Right", StringComparison.OrdinalIgnoreCase) ? ("右脚", 2) : ("左脚", 3);
            }

            if (boneName.Contains("Eye"))
            {
                return (null, 0);
            }

            if (boneName.Contains("Jaw"))
            {
                return (null, 0);
            }

            if (boneName.Contains("Head") || boneName.Contains("Neck"))
            {
                return ("首・頭", 1);
            }

            if (boneName.Contains("Spine") || boneName.Contains("Chest") || boneName.Contains("Hips") || boneName.Contains("UpperChest"))
            {
                return ("体幹", 0);
            }

            return ("その他", 14)   ;
        }
    }
}



