using System.Linq;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(PosingSystem.BreathingLevel.Weak)]
        [TestCase(PosingSystem.BreathingLevel.Medium)]
        [TestCase(PosingSystem.BreathingLevel.Strong)]
        [TestCase(PosingSystem.BreathingLevel.Rapid)]
        public void Breathing_AdditivePlaybackLimitsDrift_AndReturnsToRest(PosingSystem.BreathingLevel level)
        {
            // A complete pose holds the body root, just like the locomotion pose clips.
            var source = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.identity, "BreathingBasePose");
            assets.Add(source);
            foreach (var muscle in HumanTrait.MuscleName)
                source.SetCurve("", typeof(Animator), muscle, AnimationCurve.Constant(0, 5, 0));
            var motion = PosingSystemConverter.ResolveAdditiveBreathingClip(
                new PosingSystem.AdditiveBreathingSettings { standingLevel = level }, PosingSystem.LocomotionTypeGroup.Standing);
            var uncorrected = Object.Instantiate(motion); assets.Add(uncorrected);
            foreach (var binding in AnimationUtility.GetCurveBindings(uncorrected))
                if (binding.propertyName.StartsWith("Spine") || binding.propertyName.StartsWith("Neck") ||
                    binding.propertyName.Contains("Arm ") || binding.propertyName.Contains("Forearm"))
                    AnimationUtility.SetEditorCurve(uncorrected, binding, null);

            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            var hand = animator.GetBoneTransform(HumanBodyBones.LeftHand);
            var foot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            var beforeHead = 0f; var beforeHand = 0f; var beforeFoot = 0f;
            EvaluateLayers(source, uncorrected, true, AvatarMaskBodyPart.Root, (mixer, graph) =>
            {
                mixer.SetInputWeight(1, 0); graph.Evaluate(0);
                var headPosition = head.position; var handPosition = hand.position; var footPosition = foot.position;
                mixer.SetInputWeight(1, 1); graph.Evaluate(0);
                beforeHead = Vector3.Distance(headPosition, head.position);
                beforeHand = Vector3.Distance(handPosition, hand.position);
                beforeFoot = Vector3.Distance(footPosition, foot.position);
            }, motion.length * .36);

            EvaluateLayers(source, motion, true, AvatarMaskBodyPart.Root, (mixer, graph) =>
            {
                mixer.SetInputWeight(1, 0); graph.Evaluate(0);
                var headPosition = head.position; var handPosition = hand.position; var footPosition = foot.position;
                var localFootPosition = hips.InverseTransformPoint(foot.position);
                var rootPosition = root.transform.position; var rootRotation = root.transform.rotation;
                mixer.SetInputWeight(1, 1); graph.Evaluate(0);
                Assert.That(Vector3.Distance(headPosition, head.position), Is.LessThan(beforeHead), "Head counter bend");
                Assert.That(Vector3.Distance(handPosition, hand.position), Is.LessThan(beforeHand), "Arm compensation");
                // Retargeting a torso bend also moves/rotates the hips on the no-UpperChest test rig.
                // Bound the leg articulation separately and require compensation to reduce world drift.
                Assert.That(Vector3.Distance(localFootPosition, hips.InverseTransformPoint(foot.position)),
                    Is.InRange(.00001f, .006f), "Subtle leg movement relative to hips");
                Assert.That(Vector3.Distance(footPosition, foot.position), Is.LessThan(beforeFoot), "Foot drift compensation");
                Assert.That(root.transform.position, Is.EqualTo(rootPosition));
                Assert.That(Quaternion.Angle(rootRotation, root.transform.rotation), Is.LessThan(.01f));
                mixer.SetInputWeight(1, 0); graph.Evaluate(0);
                Assert.That(Vector3.Distance(headPosition, head.position), Is.LessThan(.0001f));
                Assert.That(Vector3.Distance(handPosition, hand.position), Is.LessThan(.0001f));
                Assert.That(Vector3.Distance(footPosition, foot.position), Is.LessThan(.0001f));
            }, motion.length * .36);
        }
    }
}
