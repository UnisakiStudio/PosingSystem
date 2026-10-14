using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    public static class PosingSystemFaceForwardUtility
    {
        public static readonly int[] Muscles = Enumerable.Range(0, HumanTrait.MuscleCount)
            .Where(i => HumanTrait.MuscleName[i].StartsWith("Neck ", StringComparison.Ordinal) ||
                        HumanTrait.MuscleName[i].StartsWith("Head ", StringComparison.Ordinal)).ToArray();

        // Run only on a disposable conversion avatar, never the user's scene bones.
        public static AnimationClip Create(Animator animator, AnimationClip source)
        {
            if (animator == null || !animator.isHuman || source == null)
                throw new ArgumentException("顔の正面補正にはHumanoidと元のAnimationが必要です。");
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            var transform = animator.transform;
            var position = transform.position;
            var rotation = transform.rotation;
            var clip = new AnimationClip { name = source.name + "_FaceForward", frameRate = source.frameRate };
            var curves = Muscles.Select(_ => new AnimationCurve()).ToArray();
            using var handler = new HumanPoseHandler(animator.avatar, transform);
            var neutral = new HumanPose { muscles = new float[HumanTrait.MuscleCount], bodyRotation = Quaternion.identity };
            handler.SetHumanPose(ref neutral);
            var target = head.rotation;
            var times = new SortedSet<float> { 0, source.length };
            var animated = false;
            foreach (var binding in AnimationUtility.GetCurveBindings(source))
            {
                var keys = AnimationUtility.GetEditorCurve(source, binding).keys;
                foreach (var key in keys) times.Add(key.time);
                if (keys.Any(k => k.value != keys[0].value ||
                    (k.inTangent != 0 && !float.IsInfinity(k.inTangent)) ||
                    (k.outTangent != 0 && !float.IsInfinity(k.outTangent)))) animated = true;
            }
            // Include in-between samples: compensating rotations is nonlinear.
            if (animated)
                for (var frame = 1; frame < Mathf.CeilToInt(source.length * 30); frame++) times.Add(frame / 30f);
            try
            {
                foreach (var time in times)
                {
                    handler.SetHumanPose(ref neutral);
                    source.SampleAnimation(animator.gameObject, time);
                    transform.SetPositionAndRotation(position, rotation);
                    var pose = new HumanPose();
                    handler.GetHumanPose(ref pose);
                    Solve(handler, head, target, ref pose);
                    for (var i = 0; i < Muscles.Length; i++) curves[i].AddKey(time, pose.muscles[Muscles[i]]);
                }
                for (var i = 0; i < Muscles.Length; i++)
                {
                    for (var key = 0; key < curves[i].length; key++)
                    {
                        AnimationUtility.SetKeyLeftTangentMode(curves[i], key, AnimationUtility.TangentMode.Linear);
                        AnimationUtility.SetKeyRightTangentMode(curves[i], key, AnimationUtility.TangentMode.Linear);
                    }
                    AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator),
                        HumanTrait.MuscleName[Muscles[i]]), curves[i]);
                }
                var settings = AnimationUtility.GetAnimationClipSettings(source);
                settings.additiveReferencePoseClip = null;
                settings.hasAdditiveReferencePose = false;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                return clip;
            }
            catch { UnityEngine.Object.DestroyImmediate(clip); throw; }
            finally { transform.SetPositionAndRotation(position, rotation); }
        }

        private static Vector3 RotationError(Quaternion target, Quaternion actual)
        {
            var delta = target * Quaternion.Inverse(actual);
            if (delta.w < 0) delta = new Quaternion(-delta.x, -delta.y, -delta.z, -delta.w);
            delta.ToAngleAxis(out var degrees, out var axis);
            return degrees < .0001f ? Vector3.zero : axis * degrees * Mathf.Deg2Rad;
        }

        public static void Solve(HumanPoseHandler handler, Transform head, Quaternion target, ref HumanPose pose)
        {
            // Damped least squares, changing ONLY neck/head muscles. The remaining pose is preserved.
            for (var iteration = 0; iteration < 24; iteration++)
            {
                handler.SetHumanPose(ref pose);
                var rotation = head.rotation;
                var error = RotationError(target, rotation);
                if (error.magnitude < .003f) break;
                var columns = new Vector3[Muscles.Length];
                var normal = Matrix4x4.zero;
                for (var i = 0; i < Muscles.Length; i++)
                {
                    var index = Muscles[i];
                    var previous = pose.muscles[index];
                    pose.muscles[index] = previous + .002f;
                    handler.SetHumanPose(ref pose);
                    var column = RotationError(head.rotation, rotation) / .002f;
                    columns[i] = column;
                    pose.muscles[index] = previous;
                    for (var r = 0; r < 3; r++)
                        for (var c = 0; c < 3; c++) normal[r, c] += column[r] * column[c];
                }
                for (var i = 0; i < 3; i++) normal[i, i] += .01f;
                normal[3, 3] = 1;
                var step = normal.inverse.MultiplyVector(error);
                for (var i = 0; i < Muscles.Length; i++)
                    pose.muscles[Muscles[i]] = Mathf.Clamp(pose.muscles[Muscles[i]] +
                        Mathf.Clamp(Vector3.Dot(columns[i], step), -.25f, .25f), -1f, 1f);
            }
            handler.SetHumanPose(ref pose);
        }
    }
}
