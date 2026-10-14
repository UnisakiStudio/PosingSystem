using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        private static object CenteringCall(string method, params object[] args) => typeof(PosingSystemConverter)
            .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static).Invoke(null, args);

        private AnimationClip MovingRollover(float duration = 1, bool rootXZ = true)
        {
            var clip = PosingSystemConverter.RootOffsetClip(new Vector3(.2f, .9f, -.3f), Quaternion.identity, "Rollover");
            assets.Add(clip); clip.frameRate = 30;
            var q = new[] { Quaternion.Euler(90, -75, 0), Quaternion.Euler(90, 0, 0), Quaternion.Euler(90, 75, 0) };
            for (var axis = 0; axis < 4; axis++)
                AnimationUtility.SetEditorCurve(clip, Binding("RootQ." + "xyzw"[axis]),
                    new AnimationCurve(new Keyframe(0, q[0][axis]), new Keyframe(duration / 2, q[1][axis]), new Keyframe(duration, q[2][axis])));
            AnimationUtility.SetEditorCurve(clip, Binding("RootT.y"), AnimationCurve.Linear(0, .85f, duration, 1.05f));
            AnimationUtility.SetEditorCurve(clip, Binding("RootT.x"), rootXZ ? AnimationCurve.Linear(0, -.3f, duration, .6f) : null);
            AnimationUtility.SetEditorCurve(clip, Binding("RootT.z"), rootXZ ? AnimationCurve.Linear(0, .7f, duration, -.3f) : null);
            AnimationUtility.SetEditorCurve(clip, Binding("Spine Front-Back"), AnimationCurve.Linear(0, -.2f, duration, .3f));
            AnimationUtility.SetEditorCurve(clip, Binding("Neck Nod Down-Up"), AnimationCurve.Linear(0, .15f, duration, -.1f));
            clip.EnsureQuaternionContinuity();
            return clip;
        }

        private static EditorCurveBinding Binding(string name) => EditorCurveBinding.FloatCurve("", typeof(Animator), name);

        [TestCase(.5f, 0f, false, 1f, false)]
        [TestCase(1f, 0f, false, 1f, false)]
        [TestCase(2f, 90f, false, 1f, false)]
        [TestCase(1f, 35f, true, 1f, true)]
        [TestCase(1f, 0f, false, .016666667f, false)]
        [TestCase(1f, 0f, false, 1f, true)]
        public void RolloverCentering_AllFramesAndMidpoints_PreserveHeightAndJointPose(
            float scale, float orientation, bool mirror, float duration, bool omitRootXZ)
        {
            var parent = new GameObject("Scaled parent"); assets.Add(parent);
            parent.transform.localScale = Vector3.one * 1.3f;
            root.transform.SetParent(parent.transform, false);
            root.transform.localScale = Vector3.one * scale;
            root.transform.SetPositionAndRotation(new Vector3(3, .4f, -2), Quaternion.Euler(0, 23, 0));
            var position = root.transform.position; var rotation = root.transform.rotation;
            var source = MovingRollover(duration, !omitRootXZ);
            var settings = AnimationUtility.GetAnimationClipSettings(source);
            settings.orientationOffsetY = orientation; settings.mirror = mirror;
            AnimationUtility.SetAnimationClipSettings(source, settings);
            var sourceJson = EditorJsonUtility.ToJson(source);
            var result = Object.Instantiate(source); assets.Add(result);
            CenteringCall("CenterRolloverHead", root, result);
            Assert.That(EditorJsonUtility.ToJson(source), Is.EqualTo(sourceJson), "Source animation is untouched");
            Assert.That(result.length, Is.EqualTo(source.length).Within(.000001f));
            Assert.That(root.transform.position, Is.EqualTo(position));
            Assert.That(root.transform.rotation, Is.EqualTo(rotation));
            foreach (var binding in AnimationUtility.GetCurveBindings(source).Where(b => b.propertyName != "RootT.x" && b.propertyName != "RootT.z"))
                Assert.That(AnimationUtility.GetEditorCurve(result, binding).keys, Is.EqualTo(AnimationUtility.GetEditorCurve(source, binding).keys), binding.propertyName);

            var bones = new[] { HumanBodyBones.Head, HumanBodyBones.Hips, HumanBodyBones.LeftFoot, HumanBodyBones.RightHand };
            float drift = 0, residual = 0;
            for (var i = 0; i <= 120; i++)
            {
                var time = duration * i / 120;
                Sample(source, time);
                var before = bones.Select(b => animator.GetBoneTransform(b).position).ToArray();
                var directions = bones.Select(b => animator.GetBoneTransform(b).rotation).ToArray();
                var beforeHead = Quaternion.Inverse(rotation) * (before[0] - position);
                drift = Mathf.Max(drift, new Vector2(beforeHead.x, beforeHead.z).magnitude);
                Sample(result, time);
                var afterHead = Quaternion.Inverse(rotation) * (animator.GetBoneTransform(bones[0]).position - position);
                residual = Mathf.Max(residual, new Vector2(afterHead.x, afterHead.z).magnitude);
                Assert.That(afterHead.y, Is.EqualTo(beforeHead.y).Within(.00005f), "Head Y at " + time);
                var shift = animator.GetBoneTransform(bones[0]).position - before[0];
                for (var b = 1; b < bones.Length; b++)
                {
                    Assert.That(Vector3.Distance(animator.GetBoneTransform(bones[b]).position - before[b], shift), Is.LessThan(.0001f), "Rigid translation: " + bones[b]);
                    Assert.That(Quaternion.Angle(animator.GetBoneTransform(bones[b]).rotation, directions[b]), Is.LessThan(.08f));
                }
            }
            Assert.That(drift, Is.GreaterThan(.15f), "Fixture must move the head");
            Assert.That(residual, Is.LessThan(.0015f), "Maximum horizontal error, including interpolated frames");
            void Sample(AnimationClip clip, float time) { clip.SampleAnimation(root, time); root.transform.SetPositionAndRotation(position, rotation); }
        }

        [Test] public void RolloverCentering_RepeatedBakeIsStable_AndSingleFrameWorks()
        {
            var clip = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.Euler(90, 30, 0), "Single frame"); assets.Add(clip);
            foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                AnimationUtility.SetEditorCurve(clip, binding, new AnimationCurve(new Keyframe(0, AnimationUtility.GetEditorCurve(clip, binding).Evaluate(0))));
            CenteringCall("CenterRolloverHead", root, clip);
            var x = AnimationUtility.GetEditorCurve(clip, Binding("RootT.x")).Evaluate(0);
            var z = AnimationUtility.GetEditorCurve(clip, Binding("RootT.z")).Evaluate(0);
            CenteringCall("CenterRolloverHead", root, clip);
            Assert.That(clip.length, Is.Zero);
            Assert.That(AnimationUtility.GetEditorCurve(clip, Binding("RootT.x")).Evaluate(0), Is.EqualTo(x).Within(.00001f));
            Assert.That(AnimationUtility.GetEditorCurve(clip, Binding("RootT.z")).Evaluate(0), Is.EqualTo(z).Within(.00001f));
        }

        [Test] public void RolloverCentering_RecalibrationAfterAvatarChange_CentersAllFramesAgain()
        {
            var clip = MovingRollover();
            clip.name = "_USSPS_Pose_Processed_USSPS_RolloverCentered_zero";
            CenteringCall("CenterRolloverHead", root, clip);
            var yBefore = AnimationUtility.GetEditorCurve(clip, Binding("RootT.y"));
            // Simulate a later avatar/height adjustment that changes the final Head location.
            animator.GetBoneTransform(HumanBodyBones.Neck).localScale = Vector3.one * 1.2f;
            var path = "Assets/USSPS_Recenter_" + Guid.NewGuid().ToString("N");
            var context = new BuildContext(root, path, false);
            try
            {
                var method = typeof(PosingSystemConverter).GetMethod("RecalibrateMotion", BindingFlags.NonPublic | BindingFlags.Instance);
                var result = (AnimationClip)method.Invoke(new PosingSystemConverter(), new object[] {
                    context, clip, new Dictionary<Motion, Motion>(), new Vector3(.8f, .7f, 1.2f), Vector3.one, root });
                for (var i = 0; i <= 60; i++)
                {
                    var t = i / 60f;
                    result.SampleAnimation(root, t); root.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    var head = animator.GetBoneTransform(HumanBodyBones.Head).position;
                    Assert.That(new Vector2(head.x, head.z).magnitude, Is.LessThan(.0015f), "Final-avatar sample " + t);
                    Assert.That(AnimationUtility.GetEditorCurve(result, Binding("RootT.y")).Evaluate(t), Is.EqualTo(yBefore.Evaluate(t) * .7f).Within(.00001f));
                }
            }
            finally { context.AssetSaver.Dispose(); AssetDatabase.DeleteAsset(path); }
        }
    }
}
