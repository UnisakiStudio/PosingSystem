using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace jp.unisakistudio.posingsystemeditor
{
    public static class AdditiveCurveUtility
    {
        private const float TimeTolerance = 0.0001f;

        public static AnimationCurve AddCurve(AnimationCurve baseCurve, AnimationCurve adjustmentCurve)
        {
            if (adjustmentCurve == null || adjustmentCurve.length == 0)
            {
                return CloneCurve(baseCurve);
            }

            var baseSource = baseCurve ?? new AnimationCurve();
            if (baseSource.length == 0)
            {
                return CloneCurve(adjustmentCurve);
            }

            var result = CloneCurve(baseSource);

            // 追加曲線側にのみ存在する時間にキーを追加
            foreach (var adjustmentKey in adjustmentCurve.keys)
            {
                if (!ContainsKeyAtTime(result, adjustmentKey.time))
                {
                    var baseValue = baseSource.Evaluate(adjustmentKey.time);
                    result.AddKey(new Keyframe(adjustmentKey.time, baseValue));
                }
            }

            // 値を加算
            var times = new List<float>(result.keys.Select(key => key.time));
            foreach (var time in times)
            {
                var baseValue = baseSource.Evaluate(time);
                var adjustmentValue = adjustmentCurve.Evaluate(time);
                var keyIndex = FindKeyIndex(result, time);
                if (keyIndex < 0)
                {
                    continue;
                }
                var key = result.keys[keyIndex];
                key.value = baseValue + adjustmentValue;
                result.MoveKey(keyIndex, key);
            }

            return result;
        }

        private static AnimationCurve CloneCurve(AnimationCurve source)
        {
            if (source == null)
            {
                return new AnimationCurve();
            }

            var keys = source.keys.Select(CopyKeyframe).ToArray();
            var clone = new AnimationCurve(keys)
            {
                preWrapMode = source.preWrapMode,
                postWrapMode = source.postWrapMode
            };
            return clone;
        }

        private static Keyframe CopyKeyframe(Keyframe source)
        {
            var keyframe = new Keyframe(source.time, source.value, source.inTangent, source.outTangent)
            {
#if UNITY_2018_1_OR_NEWER
                weightedMode = source.weightedMode,
                inWeight = source.inWeight,
                outWeight = source.outWeight,
#endif
            };
            return keyframe;
        }

        private static bool ContainsKeyAtTime(AnimationCurve curve, float time)
        {
            return FindKeyIndex(curve, time) >= 0;
        }

        private static int FindKeyIndex(AnimationCurve curve, float time)
        {
            for (var i = 0; i < curve.length; i++)
            {
                if (Mathf.Abs(curve.keys[i].time - time) <= TimeTolerance)
                {
                    return i;
                }
            }
            return -1;
        }

        /// <summary>
        /// 元クリップのRootQカーブと調整クリップのRootQカーブをQuaternion乗算で合成する。
        /// 元の回転を保持しつつ、調整の回転を追加する。
        /// </summary>
        /// <param name="baseClip">元のアニメーションクリップ</param>
        /// <param name="adjustmentClip">調整アニメーションクリップ</param>
        /// <param name="resultClip">結果を設定するクリップ</param>
        public static void MultiplyRootQCurves(AnimationClip baseClip, AnimationClip adjustmentClip, AnimationClip resultClip)
        {
            if (adjustmentClip == null || resultClip == null)
            {
                return;
            }

            // 調整クリップからRootQカーブを取得
            AnimationCurve adjQx = null, adjQy = null, adjQz = null, adjQw = null;
            foreach (var binding in AnimationUtility.GetCurveBindings(adjustmentClip))
            {
                if (binding.propertyName == "RootQ.x")
                    adjQx = AnimationUtility.GetEditorCurve(adjustmentClip, binding);
                else if (binding.propertyName == "RootQ.y")
                    adjQy = AnimationUtility.GetEditorCurve(adjustmentClip, binding);
                else if (binding.propertyName == "RootQ.z")
                    adjQz = AnimationUtility.GetEditorCurve(adjustmentClip, binding);
                else if (binding.propertyName == "RootQ.w")
                    adjQw = AnimationUtility.GetEditorCurve(adjustmentClip, binding);
            }

            // 調整クリップにRootQカーブがない場合は何もしない（元のRootQをそのまま維持）
            if (adjQx == null && adjQy == null && adjQz == null && adjQw == null)
            {
                return;
            }

            // 元クリップからRootQカーブを取得
            AnimationCurve baseQx = null, baseQy = null, baseQz = null, baseQw = null;
            if (baseClip != null)
            {
                foreach (var binding in AnimationUtility.GetCurveBindings(baseClip))
                {
                    if (binding.propertyName == "RootQ.x")
                        baseQx = AnimationUtility.GetEditorCurve(baseClip, binding);
                    else if (binding.propertyName == "RootQ.y")
                        baseQy = AnimationUtility.GetEditorCurve(baseClip, binding);
                    else if (binding.propertyName == "RootQ.z")
                        baseQz = AnimationUtility.GetEditorCurve(baseClip, binding);
                    else if (binding.propertyName == "RootQ.w")
                        baseQw = AnimationUtility.GetEditorCurve(baseClip, binding);
                }
            }

            // バインディングを定義
            var bindingX = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.x");
            var bindingY = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.y");
            var bindingZ = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.z");
            var bindingW = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootQ.w");

            // 全キーフレーム時刻を収集
            var allTimes = new HashSet<float>();
            if (adjQx != null) foreach (var key in adjQx.keys) allTimes.Add(key.time);
            if (adjQy != null) foreach (var key in adjQy.keys) allTimes.Add(key.time);
            if (adjQz != null) foreach (var key in adjQz.keys) allTimes.Add(key.time);
            if (adjQw != null) foreach (var key in adjQw.keys) allTimes.Add(key.time);
            if (baseQx != null) foreach (var key in baseQx.keys) allTimes.Add(key.time);
            if (baseQy != null) foreach (var key in baseQy.keys) allTimes.Add(key.time);
            if (baseQz != null) foreach (var key in baseQz.keys) allTimes.Add(key.time);
            if (baseQw != null) foreach (var key in baseQw.keys) allTimes.Add(key.time);

            if (allTimes.Count == 0)
            {
                return;
            }

            // 結果カーブを作成
            var resultQx = new AnimationCurve();
            var resultQy = new AnimationCurve();
            var resultQz = new AnimationCurve();
            var resultQw = new AnimationCurve();

            var sortedTimes = allTimes.OrderBy(t => t).ToList();

            foreach (var time in sortedTimes)
            {
                // 元のQuaternionを取得（カーブがない場合はIdentity）
                var baseQ = new Quaternion(
                    baseQx?.Evaluate(time) ?? 0f,
                    baseQy?.Evaluate(time) ?? 0f,
                    baseQz?.Evaluate(time) ?? 0f,
                    baseQw?.Evaluate(time) ?? 1f
                );

                // 調整Quaternionを取得（カーブがない場合はIdentity）
                var adjQ = new Quaternion(
                    adjQx?.Evaluate(time) ?? 0f,
                    adjQy?.Evaluate(time) ?? 0f,
                    adjQz?.Evaluate(time) ?? 0f,
                    adjQw?.Evaluate(time) ?? 1f
                );

                // Quaternion乗算で回転を合成（元の回転 × 調整の回転）
                var resultQ = baseQ * adjQ;

                resultQx.AddKey(new Keyframe(time, resultQ.x));
                resultQy.AddKey(new Keyframe(time, resultQ.y));
                resultQz.AddKey(new Keyframe(time, resultQ.z));
                resultQw.AddKey(new Keyframe(time, resultQ.w));
            }

            // 結果クリップにカーブを設定
            AnimationUtility.SetEditorCurve(resultClip, bindingX, resultQx);
            AnimationUtility.SetEditorCurve(resultClip, bindingY, resultQy);
            AnimationUtility.SetEditorCurve(resultClip, bindingZ, resultQz);
            AnimationUtility.SetEditorCurve(resultClip, bindingW, resultQw);
        }

        /// <summary>
        /// 指定のバインディングがRootQカーブかどうかを判定する
        /// </summary>
        public static bool IsRootQBinding(EditorCurveBinding binding)
        {
            return binding.propertyName == "RootQ.x" ||
                   binding.propertyName == "RootQ.y" ||
                   binding.propertyName == "RootQ.z" ||
                   binding.propertyName == "RootQ.w";
        }

        /// <summary>
        /// 指定のバインディングがRootTカーブかどうかを判定する
        /// </summary>
        public static bool IsRootTBinding(EditorCurveBinding binding)
        {
            return binding.propertyName == "RootT.x" ||
                   binding.propertyName == "RootT.y" ||
                   binding.propertyName == "RootT.z";
        }

        /// <summary>
        /// マッスル名からマッスルインデックスを取得する辞書を作成する
        /// </summary>
        private static Dictionary<string, int> _muscleNameToIndex;
        public static Dictionary<string, int> GetMuscleNameToIndexMap()
        {
            if (_muscleNameToIndex == null)
            {
                _muscleNameToIndex = new Dictionary<string, int>();
                for (int i = 0; i < HumanTrait.MuscleCount; i++)
                {
                    _muscleNameToIndex[HumanTrait.MuscleName[i]] = i;
                }
            }
            return _muscleNameToIndex;
        }

        /// <summary>
        /// バインディングのプロパティ名からマッスルインデックスを取得する
        /// バインディングのpropertyNameは以下の形式がある：
        /// - FBXインポート形式: "RightHand.Thumb.1 Stretched", "LeftHand.Index.Spread"
        /// - HumanTrait形式: "Right Thumb 1 Stretched", "Left Index Spread"
        /// </summary>
        public static int GetMuscleIndexFromBinding(EditorCurveBinding binding)
        {
            var propertyName = binding.propertyName;
            
            // Animatorタイプでない場合は-1を返す
            if (binding.type != typeof(Animator))
            {
                return -1;
            }
            
            var map = GetMuscleNameToIndexMap();
            
            // まずそのまま検索（HumanTrait形式の場合）
            if (map.TryGetValue(propertyName, out int index))
            {
                return index;
            }
            
            // FBXインポート形式をHumanTrait形式に変換して検索
            var convertedName = ConvertFbxMuscleNameToHumanTrait(propertyName);
            if (convertedName != propertyName && map.TryGetValue(convertedName, out int convertedIndex))
            {
                return convertedIndex;
            }
            
            // 正規化して検索（最後の手段）
            var normalizedInput = NormalizeMuscleName(propertyName);
            for (int i = 0; i < HumanTrait.MuscleCount; i++)
            {
                var standardName = HumanTrait.MuscleName[i];
                if (NormalizeMuscleName(standardName) == normalizedInput)
                {
                    return i;
                }
            }
            
            return -1;
        }

        /// <summary>
        /// FBXインポート形式のマッスル名をHumanTrait形式に変換する
        /// 例: "RightHand.Thumb.1 Stretched" → "Right Thumb 1 Stretched"
        ///     "LeftHand.Index.Spread" → "Left Index Spread"
        /// </summary>
        private static string ConvertFbxMuscleNameToHumanTrait(string fbxName)
        {
            if (string.IsNullOrEmpty(fbxName))
                return fbxName;
            
            var result = fbxName;
            
            // "LeftHand." → "Left "
            if (result.StartsWith("LeftHand."))
            {
                result = "Left " + result.Substring("LeftHand.".Length);
            }
            // "RightHand." → "Right "
            else if (result.StartsWith("RightHand."))
            {
                result = "Right " + result.Substring("RightHand.".Length);
            }
            // "LeftFoot." → "Left Toe "
            else if (result.StartsWith("LeftFoot."))
            {
                result = "Left Toe " + result.Substring("LeftFoot.".Length);
            }
            // "RightFoot." → "Right Toe "
            else if (result.StartsWith("RightFoot."))
            {
                result = "Right Toe " + result.Substring("RightFoot.".Length);
            }
            
            // 残りのピリオドをスペースに変換
            result = result.Replace(".", " ");
            
            return result;
        }

        /// <summary>
        /// マッスル名を正規化する（スペース、ピリオド、Hand/Footを除去して小文字化）
        /// </summary>
        private static string NormalizeMuscleName(string name)
        {
            return name
                .Replace("Hand", "")
                .Replace("Foot", "Toe")
                .Replace(" ", "")
                .Replace(".", "")
                .ToLower();
        }

        /// <summary>
        /// 調整クリップからマッスル調整があるインデックスのセットを取得する
        /// </summary>
        public static HashSet<int> GetAdjustedMuscleIndices(AnimationClip adjustmentClip)
        {
            var result = new HashSet<int>();
            if (adjustmentClip == null)
            {
                return result;
            }

            var allBindings = AnimationUtility.GetCurveBindings(adjustmentClip);

            foreach (var binding in allBindings)
            {
                // RootQ, RootTはスキップ
                if (IsRootQBinding(binding) || IsRootTBinding(binding))
                {
                    continue;
                }
                
                int muscleIndex = GetMuscleIndexFromBinding(binding);
                
                if (muscleIndex >= 0)
                {
                    var curve = AnimationUtility.GetEditorCurve(adjustmentClip, binding);
                    if (curve != null && curve.length > 0)
                    {
                        // 調整値が非ゼロのカーブのみを含める
                        bool hasNonZeroValue = false;
                        foreach (var key in curve.keys)
                        {
                            if (Mathf.Abs(key.value) > 0.0001f)
                            {
                                hasNonZeroValue = true;
                                break;
                            }
                        }
                        if (hasNonZeroValue)
                        {
                            result.Add(muscleIndex);
                        }
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// 元のアニメーションクリップからマッスルカーブがあるインデックスのセットを取得する
        /// </summary>
        public static HashSet<int> GetOriginalMuscleIndices(AnimationClip originalClip)
        {
            var result = new HashSet<int>();
            if (originalClip == null) return result;

            foreach (var binding in AnimationUtility.GetCurveBindings(originalClip))
            {
                // RootQ, RootTはスキップ
                if (IsRootQBinding(binding) || IsRootTBinding(binding)) continue;
                
                int muscleIndex = GetMuscleIndexFromBinding(binding);
                if (muscleIndex >= 0)
                {
                    result.Add(muscleIndex);
                }
            }
            
            return result;
        }

        /// <summary>
        /// 調整クリップの各時刻でのマッスル調整値を取得する
        /// </summary>
        public static Dictionary<int, float> GetMuscleAdjustmentsAtTime(AnimationClip adjustmentClip, float time)
        {
            var result = new Dictionary<int, float>();
            if (adjustmentClip == null) return result;

            foreach (var binding in AnimationUtility.GetCurveBindings(adjustmentClip))
            {
                // RootQ, RootTはスキップ
                if (IsRootQBinding(binding) || IsRootTBinding(binding)) continue;
                
                int muscleIndex = GetMuscleIndexFromBinding(binding);
                if (muscleIndex >= 0)
                {
                    var curve = AnimationUtility.GetEditorCurve(adjustmentClip, binding);
                    if (curve != null)
                    {
                        float adjustValue = curve.Evaluate(time);
                        result[muscleIndex] = adjustValue;
                    }
                }
            }
            
            return result;
        }

        /// <summary>
        /// バインディングが指定されたマッスルインデックスのセットに対応する可能性があるかどうかを判断する
        /// GetMuscleIndexFromBindingがマッチしない場合でも、バインディング名のキーワードから推測する
        /// これにより、非標準形式のバインディング（例：「RightHand.Thumb.1 Spread」）も正しく判定できる
        /// </summary>
        public static bool CouldBeForTargetMuscles(EditorCurveBinding binding, HashSet<int> targetMuscleIndices)
        {
            // まず通常のマッチングを試みる
            int muscleIndex = GetMuscleIndexFromBinding(binding);
            if (muscleIndex >= 0)
            {
                return targetMuscleIndices.Contains(muscleIndex);
            }
            
            // マッチしない場合、バインディング名からキーワードを抽出して対応するマッスルを推測
            var propertyName = binding.propertyName.ToLower();
            
            // 指のキーワードをチェック
            string[] fingerKeywords = { "thumb", "index", "middle", "ring", "little", "pinky" };
            string[] sideKeywords = { "left", "right" };
            
            string detectedSide = null;
            string detectedFinger = null;
            
            foreach (var side in sideKeywords)
            {
                if (propertyName.Contains(side))
                {
                    detectedSide = side;
                    break;
                }
            }
            
            foreach (var finger in fingerKeywords)
            {
                if (propertyName.Contains(finger))
                {
                    detectedFinger = finger;
                    break;
                }
            }
            
            // 指関連のカーブの場合、対応するマッスルがtargetMuscleIndicesに含まれているかチェック
            if (detectedSide != null && detectedFinger != null)
            {
                // HumanTrait.MuscleNameから対応するマッスルを検索
                for (int i = 0; i < HumanTrait.MuscleCount; i++)
                {
                    var muscleName = HumanTrait.MuscleName[i].ToLower();
                    if (muscleName.Contains(detectedSide) && muscleName.Contains(detectedFinger))
                    {
                        if (targetMuscleIndices.Contains(i))
                        {
                            return true;
                        }
                    }
                }
            }
            
            // その他のボディマッスルのキーワードをチェック
            string[] bodyKeywords = { "spine", "chest", "neck", "head", "shoulder", "arm", "forearm", "hand", 
                                      "upper", "lower", "leg", "foot", "toe", "jaw", "eye" };
            string[] actionKeywords = { "down", "up", "front", "back", "in", "out", "twist", "stretch", "spread" };
            
            foreach (var bodyPart in bodyKeywords)
            {
                if (propertyName.Contains(bodyPart))
                {
                    // このボディパーツに関連するマッスルを検索
                    for (int i = 0; i < HumanTrait.MuscleCount; i++)
                    {
                        var muscleName = HumanTrait.MuscleName[i].ToLower();
                        if (muscleName.Contains(bodyPart))
                        {
                            if (targetMuscleIndices.Contains(i))
                            {
                                // さらにアクションキーワードでの一致もチェック
                                foreach (var action in actionKeywords)
                                {
                                    if (propertyName.Contains(action) && muscleName.Contains(action))
                                    {
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return false;
        }

        /// <summary>
        /// バインディングのキーワードから、targetMuscleIndicesの中で最も可能性の高いマッスルインデックスを推測する
        /// マッチしない場合は-1を返す
        /// </summary>
        public static int GuessMuslceIndexFromBindingKeywords(EditorCurveBinding binding, HashSet<int> targetMuscleIndices)
        {
            var propertyName = binding.propertyName.ToLower();
            
            // 指のキーワードをチェック
            string[] fingerKeywords = { "thumb", "index", "middle", "ring", "little", "pinky" };
            string[] sideKeywords = { "left", "right" };
            string[] jointKeywords = { "1", "2", "3" }; // 関節番号
            string[] actionKeywords = { "stretched", "spread" };
            
            string detectedSide = null;
            string detectedFinger = null;
            string detectedJoint = null;
            string detectedAction = null;
            
            foreach (var side in sideKeywords)
            {
                if (propertyName.Contains(side))
                {
                    detectedSide = side;
                    break;
                }
            }
            
            foreach (var finger in fingerKeywords)
            {
                if (propertyName.Contains(finger))
                {
                    // pinkyはlittleに変換
                    detectedFinger = finger == "pinky" ? "little" : finger;
                    break;
                }
            }
            
            foreach (var joint in jointKeywords)
            {
                if (propertyName.Contains(joint))
                {
                    detectedJoint = joint;
                    break;
                }
            }
            
            foreach (var action in actionKeywords)
            {
                if (propertyName.Contains(action))
                {
                    detectedAction = action;
                    break;
                }
            }
            
            // 指関連のカーブの場合、対応するマッスルを検索
            if (detectedSide != null && detectedFinger != null)
            {
                int bestMatch = -1;
                int bestScore = 0;
                
                for (int i = 0; i < HumanTrait.MuscleCount; i++)
                {
                    if (!targetMuscleIndices.Contains(i))
                        continue;
                    
                    var muscleName = HumanTrait.MuscleName[i].ToLower();
                    if (!muscleName.Contains(detectedSide) || !muscleName.Contains(detectedFinger))
                        continue;
                    
                    int score = 2; // side + finger でベーススコア
                    
                    // 関節番号の一致
                    if (detectedJoint != null && muscleName.Contains(detectedJoint))
                    {
                        score += 2;
                    }
                    
                    // アクションの一致
                    if (detectedAction != null && muscleName.Contains(detectedAction))
                    {
                        score += 1;
                    }
                    
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestMatch = i;
                    }
                }
                
                return bestMatch;
            }
            
            return -1;
        }

        /// <summary>
        /// バインディングがマッスルカーブかどうかを判断する（RootT/RootQ以外のAnimatorタイプ）
        /// </summary>
        public static bool IsMuscleBinding(EditorCurveBinding binding)
        {
            if (binding.type != typeof(Animator))
                return false;
            
            if (IsRootQBinding(binding) || IsRootTBinding(binding))
                return false;
            
            return true;
        }
    }
}

