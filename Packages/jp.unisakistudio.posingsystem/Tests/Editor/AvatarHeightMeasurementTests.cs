using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using jp.unisakistudio.posingsystemeditor;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class AvatarHeightSourceLifecycleProbe : MonoBehaviour
    {
        public int EnableCount { get; private set; }
        public int DisableCount { get; private set; }

        private void OnEnable()
        {
            EnableCount++;
        }

        private void OnDisable()
        {
            DisableCount++;
        }
    }

    public class AvatarHeightMeasurementTests
    {
        private struct TransformState
        {
            public Vector3 LocalPosition;
            public Quaternion LocalRotation;
            public Vector3 LocalScale;
            public bool ActiveSelf;
        }

        private readonly List<Object> objectsToDestroy = new();

        [TearDown]
        public void TearDown()
        {
            if (AnimationMode.InAnimationMode())
            {
                AnimationMode.StopAnimationMode();
            }

            foreach (var obj in objectsToDestroy.Where(obj => obj != null))
            {
                Object.DestroyImmediate(obj);
            }
            objectsToDestroy.Clear();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void MeasureAvatarHeight_DoesNotModifySourceAvatarOrOutfit(bool sourceIsActive)
        {
            var avatarRoot = CreateHumanoidAvatar();
            avatarRoot.transform.SetPositionAndRotation(new Vector3(2.5f, 1.25f, -3f), Quaternion.Euler(12f, 34f, 5f));

            // Merge Armature前の別Armatureを模した、Humanoidマッピング外の衣装階層。
            var outfitRoot = new GameObject("OutfitArmature");
            outfitRoot.transform.SetParent(avatarRoot.transform, false);
            outfitRoot.transform.localPosition = new Vector3(0.3f, -0.2f, 0.4f);
            outfitRoot.transform.localRotation = Quaternion.Euler(7f, 11f, 13f);
            var outfitSleeve = new GameObject("OutfitSleeve");
            outfitSleeve.transform.SetParent(outfitRoot.transform, false);
            outfitSleeve.transform.localPosition = new Vector3(0.5f, 0.6f, 0.7f);
            outfitSleeve.transform.localRotation = Quaternion.Euler(17f, 19f, 23f);

            avatarRoot.SetActive(sourceIsActive);
            var sourceState = CaptureHierarchy(avatarRoot);
            var animationModeWasActive = AnimationMode.InAnimationMode();

            MeasureAvatarHeight(avatarRoot, out var humanScale, out var headHeight);

            Assert.Greater(humanScale, 0f, "HumanoidのhumanScaleを取得できること");
            Assert.Greater(headHeight, 0f, "一時クローン上で頭の高さを計測できること");
            AssertHierarchyUnchanged(sourceState, avatarRoot);
            Assert.AreEqual(animationModeWasActive, AnimationMode.InAnimationMode(), "呼び出し元のAnimationMode状態を保持すること");
            AssertNoTemporaryObjects();
        }

        [Test]
        public void MeasureAvatarHeight_PreservesPreExistingAnimationMode()
        {
            var avatarRoot = CreateHumanoidAvatar();
            AnimationMode.StartAnimationMode();

            MeasureAvatarHeight(avatarRoot, out var humanScale, out var headHeight);

            Assert.Greater(humanScale, 0f);
            Assert.Greater(headHeight, 0f);
            Assert.IsTrue(AnimationMode.InAnimationMode(), "他処理が開始したAnimationModeを終了しないこと");
            AssertNoTemporaryObjects();
        }

        [Test]
        public void MeasureAvatarHeight_DoesNotToggleSourceAvatarLifecycle()
        {
            var avatarRoot = CreateHumanoidAvatar();
            var probe = avatarRoot.AddComponent<AvatarHeightSourceLifecycleProbe>();
            var enableCount = probe.EnableCount;
            var disableCount = probe.DisableCount;

            MeasureAvatarHeight(avatarRoot, out var humanScale, out var headHeight);

            Assert.Greater(humanScale, 0f);
            Assert.Greater(headHeight, 0f);
            Assert.AreEqual(enableCount, probe.EnableCount, "元アバターでOnEnableを再発火させないこと");
            Assert.AreEqual(disableCount, probe.DisableCount, "元アバターでOnDisableを発火させないこと");
            AssertNoTemporaryObjects();
        }

        [Test]
        public void MeasureAvatarHeight_InvalidAvatarReturnsSentinelWithoutTemporaryObjects()
        {
            var nonHumanoid = new GameObject("NonHumanoid");
            objectsToDestroy.Add(nonHumanoid);

            Assert.DoesNotThrow(() => MeasureAvatarHeight(null, out _, out _));
            MeasureAvatarHeight(nonHumanoid, out var humanScale, out var headHeight);

            Assert.AreEqual(-1f, humanScale);
            Assert.AreEqual(-1f, headHeight);
            AssertNoTemporaryObjects();
        }

        [Test]
        public void RecalibrateMotion_UpdatesNdmfTemporaryAssetsInPlace()
        {
            const string assetRoot = "Assets/__PosingSystemRecalibrationTests";
            AssetDatabase.DeleteAsset(assetRoot);

            var avatarRoot = new GameObject("RecalibrationTestAvatar");
            objectsToDestroy.Add(avatarRoot);
            var context = new nadena.dev.ndmf.BuildContext(avatarRoot, assetRoot, false);

            try
            {
                var clip = new AnimationClip { name = "_USSPS_Test_footheight_clip" };
                var binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootT.y");
                AnimationUtility.SetEditorCurve(clip, binding,
                    new AnimationCurve(new Keyframe(0f, 0.5f)));

                var tree = new BlendTree { name = "_USSPS_Test_footheight" };
                tree.children = new[]
                {
                    new ChildMotion { motion = clip, threshold = 0f, timeScale = 1f }
                };

                context.AssetSaver.SaveAsset(clip);
                context.AssetSaver.SaveAsset(tree);
                Assert.IsTrue(EditorUtility.IsPersistent(clip));
                Assert.IsTrue(EditorUtility.IsPersistent(tree));
                Assert.IsTrue(context.IsTemporaryAsset(clip));
                Assert.IsTrue(context.IsTemporaryAsset(tree));

                var method = typeof(PosingSystemConverter).GetMethod(
                    "RecalibrateMotion",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.IsNotNull(method);
                var converter = new PosingSystemConverter();
                var fixedMotions = new Dictionary<Motion, Motion>();

                var result = (Motion)method.Invoke(converter,
                    new object[] { context, tree, fixedMotions, 1f, 0.25f, 1.25f });

                Assert.AreSame(tree, result, "NDMF一時BlendTreeを再複製しないこと");
                Assert.AreSame(clip, tree.children[0].motion, "NDMF一時AnimationClipを再複製しないこと");
                var recalibratedCurve = AnimationUtility.GetEditorCurve(clip, binding);
                Assert.AreEqual(0.6f, recalibratedCurve.keys[0].value, 0.00001f);
            }
            finally
            {
                context.AssetSaver.Dispose();
                AssetDatabase.DeleteAsset(assetRoot);
            }
        }

        [Test]
        public void RecalibrateMotion_CopiesExternalPersistentSubAssetsWithoutChangingSource()
        {
            const string externalAssetRoot = "Assets/__PosingSystemExternalMotionTests";
            const string buildAssetRoot = "Assets/__PosingSystemExternalMotionBuild";
            AssetDatabase.DeleteAsset(externalAssetRoot);
            AssetDatabase.DeleteAsset(buildAssetRoot);
            AssetDatabase.CreateFolder("Assets", "__PosingSystemExternalMotionTests");

            var controller = AnimatorController.CreateAnimatorControllerAtPath(
                externalAssetRoot + "/External.controller");
            var sourceClip = new AnimationClip { name = "_USSPS_External_footheight_clip" };
            var binding = EditorCurveBinding.FloatCurve(string.Empty, typeof(Animator), "RootT.y");
            AnimationUtility.SetEditorCurve(sourceClip, binding,
                new AnimationCurve(new Keyframe(0f, 0.5f)));
            var sourceTree = new BlendTree
            {
                name = "_USSPS_External_footheight",
                children = new[]
                {
                    new ChildMotion { motion = sourceClip, threshold = 0f, timeScale = 1f }
                }
            };
            AssetDatabase.AddObjectToAsset(sourceClip, controller);
            AssetDatabase.AddObjectToAsset(sourceTree, controller);
            AssetDatabase.SaveAssets();

            var avatarRoot = new GameObject("ExternalMotionTestAvatar");
            objectsToDestroy.Add(avatarRoot);
            var context = new nadena.dev.ndmf.BuildContext(avatarRoot, buildAssetRoot, false);

            try
            {
                Assert.IsTrue(EditorUtility.IsPersistent(sourceClip));
                Assert.IsTrue(EditorUtility.IsPersistent(sourceTree));
                Assert.IsFalse(context.IsTemporaryAsset(sourceClip));
                Assert.IsFalse(context.IsTemporaryAsset(sourceTree));

                var method = typeof(PosingSystemConverter).GetMethod(
                    "RecalibrateMotion",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                Assert.IsNotNull(method);
                var resultTree = (BlendTree)method.Invoke(new PosingSystemConverter(), new object[]
                {
                    context,
                    sourceTree,
                    new Dictionary<Motion, Motion>(),
                    1f,
                    0.25f,
                    1.25f
                });
                var resultClip = (AnimationClip)resultTree.children[0].motion;

                Assert.AreNotSame(sourceTree, resultTree, "外部BlendTreeを直接変更しないこと");
                Assert.AreNotSame(sourceClip, resultClip, "外部AnimationClipを直接変更しないこと");
                Assert.AreEqual(AssetDatabase.GetAssetPath(context.AssetContainer), AssetDatabase.GetAssetPath(resultTree));
                Assert.AreEqual(AssetDatabase.GetAssetPath(context.AssetContainer), AssetDatabase.GetAssetPath(resultClip));
                Assert.AreEqual(0.5f, AnimationUtility.GetEditorCurve(sourceClip, binding).keys[0].value, 0.00001f,
                    "元AnimationClipのカーブを保持すること");
                Assert.AreEqual(0.6f, AnimationUtility.GetEditorCurve(resultClip, binding).keys[0].value, 0.00001f,
                    "複製したAnimationClipだけを補正すること");
            }
            finally
            {
                context.AssetSaver.Dispose();
                AssetDatabase.DeleteAsset(buildAssetRoot);
                AssetDatabase.DeleteAsset(externalAssetRoot);
            }
        }

        private GameObject CreateHumanoidAvatar()
        {
            var root = new GameObject("TestHumanoid");
            objectsToDestroy.Add(root);

            var hips = CreateBone("Hips", root.transform, new Vector3(0f, 1f, 0f));
            var spine = CreateBone("Spine", hips, new Vector3(0f, 0.25f, 0f));
            var chest = CreateBone("Chest", spine, new Vector3(0f, 0.25f, 0f));
            var neck = CreateBone("Neck", chest, new Vector3(0f, 0.2f, 0f));
            CreateBone("Head", neck, new Vector3(0f, 0.15f, 0f));

            var leftUpperArm = CreateBone("LeftUpperArm", chest, new Vector3(-0.2f, 0.15f, 0f));
            var leftLowerArm = CreateBone("LeftLowerArm", leftUpperArm, new Vector3(-0.3f, 0f, 0f));
            CreateBone("LeftHand", leftLowerArm, new Vector3(-0.25f, 0f, 0f));
            var rightUpperArm = CreateBone("RightUpperArm", chest, new Vector3(0.2f, 0.15f, 0f));
            var rightLowerArm = CreateBone("RightLowerArm", rightUpperArm, new Vector3(0.3f, 0f, 0f));
            CreateBone("RightHand", rightLowerArm, new Vector3(0.25f, 0f, 0f));

            var leftUpperLeg = CreateBone("LeftUpperLeg", hips, new Vector3(-0.1f, -0.1f, 0f));
            var leftLowerLeg = CreateBone("LeftLowerLeg", leftUpperLeg, new Vector3(0f, -0.45f, 0f));
            CreateBone("LeftFoot", leftLowerLeg, new Vector3(0f, -0.4f, 0.1f));
            var rightUpperLeg = CreateBone("RightUpperLeg", hips, new Vector3(0.1f, -0.1f, 0f));
            var rightLowerLeg = CreateBone("RightLowerLeg", rightUpperLeg, new Vector3(0f, -0.45f, 0f));
            CreateBone("RightFoot", rightLowerLeg, new Vector3(0f, -0.4f, 0.1f));

            var humanBones = new[]
            {
                HumanBone("Hips"), HumanBone("Spine"), HumanBone("Chest"), HumanBone("Neck"), HumanBone("Head"),
                HumanBone("LeftUpperArm"), HumanBone("LeftLowerArm"), HumanBone("LeftHand"),
                HumanBone("RightUpperArm"), HumanBone("RightLowerArm"), HumanBone("RightHand"),
                HumanBone("LeftUpperLeg"), HumanBone("LeftLowerLeg"), HumanBone("LeftFoot"),
                HumanBone("RightUpperLeg"), HumanBone("RightLowerLeg"), HumanBone("RightFoot")
            };
            var skeletonBones = root.GetComponentsInChildren<Transform>()
                .Select(transform => new SkeletonBone
                {
                    name = transform.name,
                    position = transform.localPosition,
                    rotation = transform.localRotation,
                    scale = transform.localScale
                })
                .ToArray();
            var description = new HumanDescription
            {
                human = humanBones,
                skeleton = skeletonBones,
                upperArmTwist = 0.5f,
                lowerArmTwist = 0.5f,
                upperLegTwist = 0.5f,
                lowerLegTwist = 0.5f,
                armStretch = 0.05f,
                legStretch = 0.05f,
                feetSpacing = 0f,
                hasTranslationDoF = false
            };
            var avatar = AvatarBuilder.BuildHumanAvatar(root, description);
            objectsToDestroy.Add(avatar);
            Assert.IsTrue(avatar.isValid, "テスト用Avatarが有効であること");
            Assert.IsTrue(avatar.isHuman, "テスト用AvatarがHumanoidであること");

            var animator = root.AddComponent<Animator>();
            animator.avatar = avatar;
            return root;
        }

        private static HumanBone HumanBone(string boneName)
        {
            return new HumanBone
            {
                boneName = boneName,
                humanName = boneName,
                limit = new HumanLimit { useDefaultValues = true }
            };
        }

        private static void MeasureAvatarHeight(GameObject avatarRoot, out float humanScale, out float headHeight)
        {
            var method = typeof(PosingSystemConverter).GetMethod(
                "MeasureAvatarHeight",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            Assert.IsNotNull(method, "高さ計測メソッドが存在すること");
            var arguments = new object[] { avatarRoot, null, null };
            method.Invoke(null, arguments);
            humanScale = (float)arguments[1];
            headHeight = (float)arguments[2];
        }

        private static Transform CreateBone(string name, Transform parent, Vector3 localPosition)
        {
            var bone = new GameObject(name).transform;
            bone.SetParent(parent, false);
            bone.localPosition = localPosition;
            return bone;
        }

        private static Dictionary<string, TransformState> CaptureHierarchy(GameObject root)
        {
            return root.GetComponentsInChildren<Transform>(true).ToDictionary(
                transform => AnimationUtility.CalculateTransformPath(transform, root.transform),
                transform => new TransformState
                {
                    LocalPosition = transform.localPosition,
                    LocalRotation = transform.localRotation,
                    LocalScale = transform.localScale,
                    ActiveSelf = transform.gameObject.activeSelf
                });
        }

        private static void AssertHierarchyUnchanged(Dictionary<string, TransformState> expected, GameObject root)
        {
            var actual = root.GetComponentsInChildren<Transform>(true).ToDictionary(
                transform => AnimationUtility.CalculateTransformPath(transform, root.transform));
            CollectionAssert.AreEquivalent(expected.Keys, actual.Keys, "元アバターの階層構造を変更しないこと");
            foreach (var pair in expected)
            {
                var transform = actual[pair.Key];
                Assert.AreEqual(pair.Value.LocalPosition, transform.localPosition, $"{pair.Key} の位置を変更しないこと");
                Assert.AreEqual(pair.Value.LocalRotation, transform.localRotation, $"{pair.Key} の回転を変更しないこと");
                Assert.AreEqual(pair.Value.LocalScale, transform.localScale, $"{pair.Key} のスケールを変更しないこと");
                Assert.AreEqual(pair.Value.ActiveSelf, transform.gameObject.activeSelf, $"{pair.Key} のactiveSelfを変更しないこと");
            }
        }

        private static void AssertNoTemporaryObjects()
        {
            Assert.IsFalse(Resources.FindObjectsOfTypeAll<GameObject>().Any(go => go.name == "_PosingSystem_TempMeasureAvatar"),
                "高さ計測用クローンを残さないこと");
            Assert.IsFalse(Resources.FindObjectsOfTypeAll<AnimationClip>().Any(clip => clip.name == "_PosingSystem_TempHeightClip"),
                "高さ計測用AnimationClipを残さないこと");
        }
    }
}
