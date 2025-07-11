using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    }
}

