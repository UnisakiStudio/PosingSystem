using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using jp.unisakistudio.posingsystemeditor;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class AvatarHeightMeasurementTests
    {
        private readonly List<Object> objectsToDestroy = new();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in objectsToDestroy)
            {
                if (obj != null)
                {
                    Object.DestroyImmediate(obj);
                }
            }
            objectsToDestroy.Clear();
        }

        [TestCase(0.5f)]
        [TestCase(1f)]
        [TestCase(2f)]
        public void GetHorizontalOffsetInRootUnits_NormalizesAvatarSpaceOffsetByHumanScale(float humanScale)
        {
            var avatarRoot = new GameObject("HorizontalOffsetTestAvatar");
            objectsToDestroy.Add(avatarRoot);
            avatarRoot.transform.SetPositionAndRotation(
                new Vector3(2.5f, 1.25f, -3f),
                Quaternion.Euler(0f, 37f, 0f));

            var expectedOffset = new Vector3(0.35f, 0f, -0.6f);
            var worldPosition = avatarRoot.transform.position
                + avatarRoot.transform.rotation * (expectedOffset * humanScale + Vector3.up * 0.8f);

            var actualOffset = GetHorizontalOffsetInRootUnits(
                avatarRoot.transform,
                worldPosition,
                humanScale);

            Assert.AreEqual(expectedOffset.x, actualOffset.x, 0.00001f);
            Assert.AreEqual(0f, actualOffset.y, 0.00001f);
            Assert.AreEqual(expectedOffset.z, actualOffset.z, 0.00001f);
        }

        [Test]
        public void RecalibrateMotion_ScalesEachRootTranslationAxisAndFootHeight()
        {
            var assetRoot = "Assets/__PosingSystemRootTranslationTests_"
                + System.Guid.NewGuid().ToString("N");
            var avatarRoot = new GameObject("RootTranslationTestAvatar");
            objectsToDestroy.Add(avatarRoot);
            var context = new nadena.dev.ndmf.BuildContext(avatarRoot, assetRoot, false);

            try
            {
                var clip = new AnimationClip { name = "_USSPS_Test_footheight_clip" };
                var expectedValues = new Dictionary<string, float>
                {
                    { "RootT.x", 0.4f },
                    { "RootT.y", 0.5f },
                    { "RootT.z", 1.8f }
                };
                var sourceValues = new Dictionary<string, float>
                {
                    { "RootT.x", 0.5f },
                    { "RootT.y", 1f },
                    { "RootT.z", 1.5f }
                };
                foreach (var pair in sourceValues)
                {
                    var binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), pair.Key);
                    AnimationUtility.SetEditorCurve(clip, binding,
                        new AnimationCurve(new Keyframe(0f, pair.Value)));
                }
                var settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.level = 2f;
                AnimationUtility.SetAnimationClipSettings(clip, settings);

                var tree = new BlendTree
                {
                    name = "_USSPS_Test_footheight",
                    children = new[]
                    {
                        new ChildMotion { motion = clip, threshold = 0f, timeScale = 1f }
                    }
                };
                context.AssetSaver.SaveAsset(clip);
                context.AssetSaver.SaveAsset(tree);

                var method = typeof(PosingSystemConverter).GetMethod(
                    "RecalibrateMotion",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.IsNotNull(method);
                var result = (Motion)method.Invoke(new PosingSystemConverter(), new object[]
                {
                    context,
                    tree,
                    new Dictionary<Motion, Motion>(),
                    new Vector3(0.8f, 0.5f, 1.2f),
                    Vector3.one,
                    null
                });

                Assert.AreSame(tree, result);
                Assert.AreSame(clip, tree.children[0].motion);
                foreach (var pair in expectedValues)
                {
                    var binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), pair.Key);
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    Assert.AreEqual(pair.Value, curve.keys[0].value, 0.00001f,
                        pair.Key + " must preserve its world-space distance.");
                }
                Assert.AreEqual(1f, AnimationUtility.GetAnimationClipSettings(clip).level, 0.00001f);
            }
            finally
            {
                context.AssetSaver.Dispose();
                AssetDatabase.DeleteAsset(assetRoot);
            }
        }

        private static Vector3 GetHorizontalOffsetInRootUnits(
            Transform avatarRoot,
            Vector3 worldPosition,
            float humanScale)
        {
            var method = typeof(PosingSystemConverter).GetMethod(
                "GetHorizontalOffsetInRootUnits",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(method, "The horizontal RootT correction method must exist.");
            return (Vector3)method.Invoke(null, new object[] { avatarRoot, worldPosition, humanScale });
        }
    }
}
