#if NDMF && MODULAR_AVATAR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    public partial class PosingSystemConverter
    {
        // Retained by generated height wrappers, persisted prebuilds and NDMF copies.
        private const string RolloverCenteredMarker = "_USSPS_RolloverCentered";

        /// <summary>Bake horizontal head centering into the final pose, leaving Y and joint motion intact.</summary>
        private static void CenterRolloverHead(GameObject workingAvatar, AnimationClip clip)
        {
            var animator = workingAvatar != null ? workingAvatar.GetComponent<Animator>() : null;
            var head = animator != null && animator.isHuman ? animator.GetBoneTransform(HumanBodyBones.Head) : null;
            if (head == null || clip == null)
                throw new InvalidOperationException(PosingSystemLocalization.TextMessage("寝返りの頭位置補正には、有効なHumanoidとHeadボーンが必要です。"));

            var transforms = workingAvatar.GetComponentsInChildren<Transform>(true);
            var positions = transforms.Select(t => t.localPosition).ToArray();
            var rotations = transforms.Select(t => t.localRotation).ToArray();
            var scales = transforms.Select(t => t.localScale).ToArray();
            var root = workingAvatar.transform;
            var rootPosition = root.position;
            var rootRotation = root.rotation;
            var xBinding = EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.x");
            var zBinding = EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.z");
            var sourceX = AnimationUtility.GetEditorCurve(clip, xBinding);
            var sourceZ = AnimationUtility.GetEditorCurve(clip, zBinding);
            var duration = clip.length;
            var times = new SortedSet<float> { 0, duration };
            if (duration > 0)
            {
                // MotionTime also traverses very short clips continuously. Sample at least 60 intervals
                // across the whole motion, as well as every authored frame and off-frame key.
                var frames = Mathf.Max(60, Mathf.CeilToInt(duration * Mathf.Max(60, clip.frameRate)));
                for (var i = 1; i < frames; i++) times.Add(duration * i / frames);
                foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                foreach (var key in AnimationUtility.GetEditorCurve(clip, binding).keys)
                    if (key.time >= 0 && key.time <= duration) times.Add(key.time);
            }

            AnimationClip probe = null;
            try
            {
                // Measure the actual root-axis response on this avatar. This includes scale, modified
                // humanoid proportions and clip orientation settings, without assuming humanScale == meters.
                var origin = Sample(clip, 0);
                probe = new AnimationClip { hideFlags = HideFlags.HideAndDontSave };
                EditorUtility.CopySerialized(clip, probe);
                AnimationUtility.SetEditorCurve(probe, xBinding, AnimationCurve.Constant(0, duration, (sourceX?.Evaluate(0) ?? 0) + 1));
                var xAxis = Sample(probe, 0) - origin;
                AnimationUtility.SetEditorCurve(probe, xBinding, sourceX);
                AnimationUtility.SetEditorCurve(probe, zBinding, AnimationCurve.Constant(0, duration, (sourceZ?.Evaluate(0) ?? 0) + 1));
                var zAxis = Sample(probe, 0) - origin;
                var determinant = xAxis.x * zAxis.z - xAxis.z * zAxis.x;
                if (float.IsNaN(determinant) || float.IsInfinity(determinant) || Mathf.Abs(determinant) < .00000001f)
                    throw new InvalidOperationException(PosingSystemLocalization.TextMessage("寝返りの頭位置補正で、RootTの移動量を計測できませんでした。"));

                var values = new Dictionary<float, Vector2>();
                Vector2 Evaluate(float time)
                {
                    if (values.TryGetValue(time, out var cached)) return cached;
                    var offset = Sample(clip, time);
                    var dx = (offset.x * zAxis.z - offset.z * zAxis.x) / determinant;
                    var dz = (xAxis.x * offset.z - xAxis.z * offset.x) / determinant;
                    return values[time] = new Vector2((sourceX?.Evaluate(time) ?? 0) - dx, (sourceZ?.Evaluate(time) ?? 0) - dz);
                }

                // MotionTime may lie between authored frames. Subdivide nonlinear stretches whose
                // quarter-point interpolation error exceeds 0.25 mm, including short/fast rolls.
                void Refine(float from, float to, int depth)
                {
                    if (depth >= 10 || to - from < .000001f) return;
                    var a = Evaluate(from); var b = Evaluate(to);
                    var refine = false;
                    for (var quarter = 1; quarter <= 3; quarter++)
                    {
                        var fraction = quarter / 4f;
                        var error = Evaluate(Mathf.Lerp(from, to, fraction)) - Vector2.Lerp(a, b, fraction);
                        var displacement = xAxis * error.x + zAxis * error.y;
                        if (new Vector2(displacement.x, displacement.z).sqrMagnitude > .00025f * .00025f) refine = true;
                    }
                    if (!refine) return;
                    var middle = (from + to) * .5f;
                    times.Add(middle);
                    Refine(from, middle, depth + 1); Refine(middle, to, depth + 1);
                }
                var initialTimes = times.ToArray();
                for (var i = 1; i < initialTimes.Length; i++) Refine(initialTimes[i - 1], initialTimes[i], 0);
                var xKeys = new List<Keyframe>(); var zKeys = new List<Keyframe>();
                foreach (var time in times)
                {
                    var value = Evaluate(time);
                    xKeys.Add(new Keyframe(time, value.x)); zKeys.Add(new Keyframe(time, value.y));
                }
                AnimationUtility.SetEditorCurve(clip, xBinding, LinearCurve(xKeys));
                AnimationUtility.SetEditorCurve(clip, zBinding, LinearCurve(zKeys));
            }
            finally
            {
                Restore();
                if (probe != null) Object.DestroyImmediate(probe);
            }

            Vector3 Sample(AnimationClip source, float time)
            {
                Restore();
                source.SampleAnimation(workingAvatar, time);
                // Root motion extraction must not move the coordinate system being measured.
                root.SetPositionAndRotation(rootPosition, rootRotation);
                return Quaternion.Inverse(rootRotation) * (head.position - rootPosition);
            }

            void Restore()
            {
                for (var i = 0; i < transforms.Length; i++)
                {
                    transforms[i].localPosition = positions[i];
                    transforms[i].localRotation = rotations[i];
                    transforms[i].localScale = scales[i];
                }
            }

            AnimationCurve LinearCurve(List<Keyframe> keys)
            {
                var curve = new AnimationCurve(keys.ToArray());
                for (var i = 0; i < curve.length; i++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                    AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                }
                return curve;
            }
        }
    }
}
#endif
