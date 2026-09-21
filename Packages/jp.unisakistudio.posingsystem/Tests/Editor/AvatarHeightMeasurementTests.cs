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

        [TestCase(0.5f, 0.5f, 1f)]
        [TestCase(1f, 1f, 1f)]
        [TestCase(0.7830368f, 1.78f, 1f)]
        [TestCase(2f, 2f, 1f)]
        [TestCase(0.7830368f, 1.78f, 1.3f)]
        public void GetHorizontalOffsetInRootUnits_IncludesUniformWorldScale(
            float humanScale, float rootScale, float parentScale)
        {
            var parent = new GameObject("ScaledParent");
            objectsToDestroy.Add(parent);
            parent.transform.localScale = Vector3.one * parentScale;
            parent.transform.SetPositionAndRotation(new Vector3(2, 3, -4), Quaternion.Euler(0, 37, 0));
            var avatarRoot = new GameObject("ScaledAvatar");
            avatarRoot.transform.SetParent(parent.transform, false);
            avatarRoot.transform.localScale = Vector3.one * rootScale;
            avatarRoot.transform.localRotation = Quaternion.Euler(0, 23, 0);
            var expected = new Vector3(0.35f, 0f, -0.6f);
            var worldPosition = avatarRoot.transform.TransformPoint(expected * humanScale + Vector3.up);

            var actual = GetHorizontalOffsetInRootUnits(avatarRoot.transform, worldPosition, humanScale);

            Assert.That(Vector3.Distance(actual, expected), Is.LessThan(0.00001f));
        }

        [TestCase(1f, 2f, 1f)]
        [TestCase(-1f, 1f, 1f)]
        [TestCase(0f, 0f, 0f)]
        [TestCase(0.0000001f, 0.0000001f, 0.0000001f)]
        [TestCase(0.00001f, 0.00002f, 0.00001f)]
        public void GetHorizontalOffsetInRootUnits_UnsupportedScaleKeepsExistingBehavior(
            float x, float y, float z)
        {
            var avatarRoot = new GameObject("UnsupportedScaleAvatar");
            objectsToDestroy.Add(avatarRoot);
            avatarRoot.transform.localScale = new Vector3(x, y, z);
            var worldPosition = new Vector3(0.2f, 0.8f, -0.4f);

            var actual = GetHorizontalOffsetInRootUnits(avatarRoot.transform, worldPosition, 0.5f);

            Assert.That(actual, Is.EqualTo(new Vector3(0.4f, 0f, -0.8f)));
        }

        private static IEnumerable<TestCaseData> HumanoidScaleCases()
        {
            foreach (var scale in new[] { 0.5f, 1f, 1.78f, 2f })
            foreach (var clip in new[] { "SitShallow_Ashikumi", "SitShallow_Maekagami", "SitSleepUp_Hirune" })
                yield return new TestCaseData(scale, clip);
        }

        [TestCaseSource(nameof(HumanoidScaleCases))]
        public void HorizontalCorrection_CentersRealHumanoidAtDifferentRootScales(float scale, string clipName)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Shinano/Prefab/Shinano.prefab");
            var source = AssetDatabase.LoadAssetAtPath<AnimationClip>(
                "Packages/jp.unisakistudio.kawaiiposing/Animations/" + clipName + ".anim");
            if (prefab == null || source == null)
                Assert.Ignore("Integration fixture requires Shinano and KawaiiPosing in this project.");
            Assert.That(AnimationMode.InAnimationMode(), Is.False, "Stop animation preview before running this test.");
            var avatar = Object.Instantiate(prefab);
            objectsToDestroy.Add(avatar);
            avatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            avatar.transform.localScale = Vector3.one * scale;
            var animator = avatar.GetComponent<Animator>();
            Assert.That(animator.isHuman, Is.True);
            var clip = Object.Instantiate(source);
            objectsToDestroy.Add(clip);

            try
            {
                SamplePose(avatar, source);
                var correction = GetHorizontalOffsetInRootUnits(avatar.transform,
                    animator.GetBoneTransform(HumanBodyBones.Head).position, animator.humanScale);
                AnimationMode.StopAnimationMode();
                avatar.SetActive(false);
                avatar.SetActive(true);
                foreach (var axis in new[] { "x", "z" })
                {
                    var binding = EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT." + axis);
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    Assert.That(curve, Is.Not.Null, "The fixture must contain horizontal RootT curves.");
                    var keys = curve.keys;
                    for (var i = 0; i < keys.Length; i++)
                        keys[i].value -= axis == "x" ? correction.x : correction.z;
                    curve.keys = keys;
                    AnimationUtility.SetEditorCurve(clip, binding, curve);
                }
                SamplePose(avatar, clip);
                var head = animator.GetBoneTransform(HumanBodyBones.Head).position;
                Assert.That(new Vector2(head.x, head.z).magnitude, Is.LessThan(0.0001f),
                    "Horizontal correction must stay within 0.1 mm at root scale " + scale);
            }
            finally
            {
                if (AnimationMode.InAnimationMode()) AnimationMode.StopAnimationMode();
            }
        }

        private static void SamplePose(GameObject avatar, AnimationClip clip)
        {
            AnimationMode.StartAnimationMode();
            AnimationMode.BeginSampling();
            try
            {
                AnimationMode.SampleAnimationClip(avatar, clip, 0);
                avatar.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
            }
            finally
            {
                AnimationMode.EndSampling();
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void PrebuildData_LegacyHorizontalConversionRequiresRegeneration(bool missingVersion)
        {
            var root = new GameObject("PrebuildVersionAvatar");
            objectsToDestroy.Add(root);
            var posing = root.AddComponent<jp.unisakistudio.posingsystem.PosingSystem>();
            posing.savedInstanceId = GlobalObjectId.GetGlobalObjectIdSlow(posing).ToString();
            var current = PosingSystemConverter.GetDefineSerializeJson(posing);
            StringAssert.Contains("\"horizontalRootTranslationVersion\":1", current);
            posing.data = missingVersion
                ? current.Replace("\"horizontalRootTranslationVersion\":1,", "")
                : current.Replace("\"horizontalRootTranslationVersion\":1", "\"horizontalRootTranslationVersion\":0");
            Assert.That(posing.data, Is.Not.EqualTo(current));
            Assert.That(PosingSystemConverter.IsPosingSystemDataUpdated(posing), Is.True,
                "Old prebuilds must be regenerated even when the package version is unchanged.");
            posing.data = current;
            Assert.That(PosingSystemConverter.IsPosingSystemDataUpdated(posing), Is.False,
                "A current prebuild must not be invalidated on every build.");
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
