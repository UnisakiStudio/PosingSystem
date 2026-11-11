using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void BuiltThumbnail_RendersGeneratedPoseOnDescriptorlessClone(bool rollover)
        {
            if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null)
                Assert.Ignore("Requires a graphics device for Camera.Render.");
            var ps = UserSettings(out var pose);
            pose.isRollover = rollover;
            pose.isRotate = true;
            pose.rotate = 90;
            var generated = Clip();
            UnityEditor.AnimationUtility.SetEditorCurve(generated,
                UnityEditor.EditorCurveBinding.FloatCurve("", typeof(Animator), "Right Arm Down-Up"),
                AnimationCurve.Linear(0, -.8f, 1, .8f));
            var clone = Object.Instantiate(root);
            Object.DestroyImmediate(clone.GetComponent<VRCAvatarDescriptor>());
            var camera = PosingSystemConverter.CreateIconCamera();
            var hand = clone.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.RightHand);
            Vector3? renderedHand = null;
            Quaternion renderedRotation = default;
            void OnRender(Camera rendering)
            {
                if (rendering != camera) return;
                renderedHand = hand.position;
                renderedRotation = hand.rotation;
            }
            try
            {
                generated.SampleAnimation(clone, rollover ? .5f : 0);
                var expectedHand = hand.position;
                var expectedRotation = hand.rotation;
                generated.SampleAnimation(clone, 1);
                Assert.That(Vector3.Distance(hand.position, expectedHand), Is.GreaterThan(.01f));
                Camera.onPreRender += OnRender;
                PosingSystemConverter.TakeIconScreenshot(pose, ps, camera, clone, true, generated);
                Assert.That(renderedHand.HasValue, Is.True);
                Assert.That(Vector3.Distance(renderedHand.Value, expectedHand), Is.LessThan(.001f));
                Assert.That(Quaternion.Angle(renderedRotation, expectedRotation), Is.LessThan(.1f));
                Assert.That(pose.previewImage, Is.Not.Null);
            }
            finally
            {
                Camera.onPreRender -= OnRender;
                if (pose.previewImage != null) Object.DestroyImmediate(pose.previewImage);
                Object.DestroyImmediate(camera.gameObject);
                Object.DestroyImmediate(clone);
            }
        }
    }

    public class PosingSystemPreviewCloneTests
    {
        [TearDown]
        public void TearDown()
        {
            PosingSystemEditor.CleanupAllPreviewAvatars();

            foreach (var gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (gameObject != null &&
                    (gameObject.name == "PreviewCloneTestAvatar" ||
                     gameObject.name.StartsWith("_PosingSystem_ProcessedPreview_")))
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
        }

        [Test]
        public void BuildPipelineClone_RemainsInValidScene()
        {
            var sourceAvatar = new GameObject("PreviewCloneTestAvatar");
            var posingObject = new GameObject("PosingSystem");
            posingObject.transform.SetParent(sourceAvatar.transform);
            var posingSystem = posingObject.AddComponent<PosingSystem>();

            var method = typeof(PosingSystemConverter).GetMethod(
                "CreateBuildPipelineAvatarClone",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(method);

            var clone = (GameObject)method.Invoke(null, new object[] { sourceAvatar, posingSystem });

            Assert.IsTrue(clone.scene.IsValid(), "NDMFが保持するSceneは有効であること");
            Assert.IsTrue(clone.scene.isLoaded);
            Assert.IsNull(clone.transform.parent);
            Assert.IsTrue(clone.activeSelf);
            Assert.AreEqual(HideFlags.HideInHierarchy, clone.hideFlags);
            // The build must generate the same motions it would for the source avatar.
            Assert.AreEqual(posingObject.tag, clone.GetComponentInChildren<PosingSystem>(true).gameObject.tag);
        }

        [Test]
        public void BuiltThumbnail_SelectsNeutralHeightAndDistinguishesDuplicatePosesAbove255()
        {
            var root = new GameObject("PreviewCloneTestAvatar");
            var descriptor = root.AddComponent<VRCAvatarDescriptor>();
            var controller = new AnimatorController();
            var machine = new AnimatorStateMachine();
            var wrapper = new AnimatorStateMachine();
            wrapper.stateMachines = new[] { new ChildAnimatorStateMachine { stateMachine = machine } };
            var neutral = new AnimationClip { name = "Shared" };
            var duplicate = new AnimationClip { name = "Shared" };
            var footHeight = new BlendTree { blendParameter = "FootHeight", useAutomaticThresholds = false };
            footHeight.AddChild(duplicate, 0);
            footHeight.AddChild(neutral, .5f);
            try
            {
                controller.layers = new[] { new AnimatorControllerLayer { name = "USSPS_Locomotion", stateMachine = wrapper } };
                descriptor.baseAnimationLayers = new[] { new VRCAvatarDescriptor.CustomAnimLayer {
                    type = VRCAvatarDescriptor.AnimLayerType.Base, animatorController = controller } };
                void AddPose(int id, Motion motion)
                {
                    var state = machine.AddState("Shared");
                    state.motion = motion;
                    machine.AddEntryTransition(state).conditions = PosingSystemConverter.GetEntryTransitionConditions(
                        PosingSystemConverter.SplitSyncParameters(id, 257), true);
                }
                AddPose(1, footHeight);
                AddPose(257, duplicate);
                var first = new PosingSystem.AnimationDefine(duplicate, "Shared") { enabled = true, syncdParameterValue = 1 };
                var second = new PosingSystem.AnimationDefine(duplicate, "Shared") { enabled = true, syncdParameterValue = 257 };
                var disabled = new PosingSystem.AnimationDefine(duplicate, "Shared") { enabled = false, syncdParameterValue = 1 };
                var result = new Dictionary<PosingSystem.AnimationDefine, AnimationClip>();
                PosingSystemConverter.CollectThumbnailMotions(descriptor, new[] { second, disabled, first }, result);
                Assert.That(result[first], Is.SameAs(neutral));
                Assert.That(result[second], Is.SameAs(duplicate));
                Assert.That(result.ContainsKey(disabled), Is.False);
            }
            finally
            {
                var entries = machine.entryTransitions;
                var states = machine.states;
                foreach (var entry in entries) Object.DestroyImmediate(entry);
                foreach (var state in states) Object.DestroyImmediate(state.state);
                Object.DestroyImmediate(footHeight);
                Object.DestroyImmediate(neutral);
                Object.DestroyImmediate(duplicate);
                Object.DestroyImmediate(machine);
                Object.DestroyImmediate(wrapper);
                Object.DestroyImmediate(controller);
            }
        }

        [Test]
        public void PreviewRoot_IsRecoveredAfterStaticCacheReset()
        {
            PosingSystemEditor.CleanupAllPreviewAvatars();
            var first = PosingSystemEditor.GetPreviewAvatarRoot();
            Assert.IsFalse(first.gameObject.scene.IsValid());

            var field = typeof(PosingSystemEditor).GetField(
                "previewAvatarRoot",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            field.SetValue(null, null);

            var recovered = PosingSystemEditor.GetPreviewAvatarRoot();

            Assert.AreSame(first.gameObject, recovered.gameObject);
            Assert.AreEqual(
                1,
                Resources.FindObjectsOfTypeAll<GameObject>().Count(
                    gameObject => gameObject != null && gameObject.name == "PreviewAvatarRoot"));
        }
    }
}
