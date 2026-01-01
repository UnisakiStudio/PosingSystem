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
    }
}

