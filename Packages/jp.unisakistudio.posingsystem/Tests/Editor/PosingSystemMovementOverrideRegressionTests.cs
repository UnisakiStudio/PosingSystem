using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Avatars.Components;
using StateType = jp.unisakistudio.posingsystem.PosingSystem.OverrideAnimationDefine.AnimationStateType;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemMovementOverrideRegressionTests
    {
        private readonly List<UnityEngine.Object> objectsToDestroy = new();
        private Scene previewScene;

        [SetUp]
        public void SetUp()
        {
            previewScene = EditorSceneManager.NewPreviewScene();
        }

        [TearDown]
        public void TearDown()
        {
            for (var i = objectsToDestroy.Count - 1; i >= 0; i--)
            {
                if (objectsToDestroy[i] != null)
                    UnityEngine.Object.DestroyImmediate(objectsToDestroy[i]);
            }
            objectsToDestroy.Clear();
            EditorSceneManager.ClosePreviewScene(previewScene);
        }

        [Test]
        public void OnlyStandWalkRun_RequiresBlendTree()
        {
            var method = typeof(PosingSystemConverter).GetMethod(
                "IsValidOverrideMotion", BindingFlags.Static | BindingFlags.NonPublic);
            var clip = CreateClip("SinglePose");
            var tree = CreateTree("Locomotion", clip);

            Assert.That(method.Invoke(null, new object[] { StateType.StandWalkRun, clip }), Is.False);
            foreach (var type in new[] { StateType.Stand, StateType.Crouch, StateType.Prone, StateType.Jump })
                Assert.That(method.Invoke(null, new object[] { type, clip }), Is.True, type.ToString());
            foreach (var type in new[] { StateType.StandWalkRun, StateType.Crouch, StateType.Prone })
            {
                Assert.That(method.Invoke(null, new object[] { type, tree }), Is.True, type.ToString());
                Assert.That(method.Invoke(null, new object[] { type, null }), Is.False, type.ToString());
            }
        }

        [TestCase(false, 0)]
        [TestCase(true, 0)]
        [TestCase(false, 1)]
        [TestCase(true, 1)]
        [TestCase(false, 2)]
        [TestCase(true, 2)]
        public void ImportPostures_AcceptsClipsAndPrefersValidTrees(bool inspectorButton, int treeMode)
        {
            var source = CreateController("First");
            var ps = CreatePosingSystem(source);
            var crouch = CreateClip("CrouchClip");
            var prone = CreateClip("ProneClip");
            AddState(source.layers[0].stateMachine, "Crouching", crouch);
            AddState(source.layers[0].stateMachine, "Prone", prone);
            // A single standing clip must still not replace the entire locomotion tree.
            AddState(source.layers[0].stateMachine, "Standing", CreateClip("Idle"));

            Motion expectedCrouch = crouch;
            Motion expectedProne = prone;
            if (treeMode != 0)
            {
                source.AddLayer("Later");
                Track(source.layers[1].stateMachine);
                var nested = Track(source.layers[1].stateMachine.AddStateMachine("Nested"));
                var crouchTree = CreateTree("CrouchTree", treeMode == 1 ? crouch : null);
                var proneTree = CreateTree("ProneTree", treeMode == 1 ? prone : null);
                AddState(nested, "Crouching", crouchTree);
                AddState(nested, "Prone", proneTree);
                if (treeMode == 1)
                {
                    expectedCrouch = crouchTree;
                    expectedProne = proneTree;
                }
            }

            Import(ps, inspectorButton);

            Assert.That(ps.overrideDefines, Has.Count.EqualTo(2));
            Assert.That(ps.overrideDefines.Single(d => d.stateType == StateType.Crouch).animationClip,
                Is.SameAs(expectedCrouch));
            Assert.That(ps.overrideDefines.Single(d => d.stateType == StateType.Prone).animationClip,
                Is.SameAs(expectedProne));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ImportPostures_PreservesExplicitClipEvenWhenTreeExists(bool inspectorButton)
        {
            var source = CreateController("Locomotion");
            var ps = CreatePosingSystem(source);
            var clip = CreateClip("ExplicitClip");
            foreach (var pair in new[] { (StateType.Crouch, "Crouching"), (StateType.Prone, "Prone") })
            {
                ps.overrideDefines.Add(new PosingSystem.OverrideAnimationDefine
                    { stateType = pair.Item1, animationClip = clip });
                AddState(source.layers[0].stateMachine, pair.Item2, CreateTree(pair.Item2, clip));
            }

            Import(ps, inspectorButton);

            Assert.That(ps.overrideDefines, Has.Count.EqualTo(2));
            Assert.That(ps.overrideDefines.All(d => d.animationClip == clip), Is.True);
        }

        [Test]
        public void ManualOverrides_ApplyPostureClipsAndStandingTreeToVrAndDesktop()
        {
            var ps = CreatePosingSystem(CreateController("Locomotion"));
            var output = CreateController("USSPS_Locomotion");
            var original = CreateClip("Original");
            foreach (var name in new[] { "Standing", "Crouching", "Prone" })
            foreach (var suffix in new[] { "", "_Desktop" })
                AddState(output.layers[0].stateMachine, name + suffix, original);
            ps.gameObject.AddComponent<DuplicateEraser>().ID = "jp.unisakistudio.posingsystem_locomotion";
            ps.gameObject.AddComponent<ModularAvatarMergeAnimator>().animator = output;

            var crouch = CreateClip("AlueLike_Crouching");
            var prone = CreateClip("AlueLike_Prone");
            var standing = CreateTree("AlueLike_Standing", CreateClip("StandingClip"));
            ps.overrideDefines.Add(new PosingSystem.OverrideAnimationDefine
                { stateType = StateType.Crouch, animationClip = crouch });
            ps.overrideDefines.Add(new PosingSystem.OverrideAnimationDefine
                { stateType = StateType.Prone, animationClip = prone });
            ps.overrideDefines.Add(new PosingSystem.OverrideAnimationDefine
                { stateType = StateType.StandWalkRun, animationClip = standing });
            ps.overrideDefines.Add(new PosingSystem.OverrideAnimationDefine
                { stateType = StateType.Crouch, animationClip = original, enabled = false });

            PosingSystemConverter.ConvertOverrideAnimations(ps);

            foreach (var child in output.layers[0].stateMachine.states)
            {
                Motion expected = child.state.name.StartsWith("Standing") ? standing :
                    child.state.name.StartsWith("Crouching") ? crouch : prone;
                Assert.That(child.state.motion, Is.SameAs(expected), child.state.name);
            }
        }

        [Test]
        public void PrebuildFromBeforePostureFix_IsDetectedAsOutdated()
        {
            var ps = CreatePosingSystem(CreateController("Locomotion"));
            var current = PosingSystemConverter.GetDefineSerializeJson(ps, true);
            ps.data = current;
            Assert.That(PosingSystemConverter.IsPosingSystemDataUpdated(ps), Is.False);
            StringAssert.Contains("\"postureOverrideVersion\":1,", current);
            ps.data = current.Replace("\"postureOverrideVersion\":1,", "");
            Assert.That(PosingSystemConverter.IsPosingSystemDataUpdated(ps), Is.True);
        }

        private void Import(PosingSystem ps, bool inspectorButton)
        {
            if (!inspectorButton)
            {
                PosingSystemConverter.AutoImportAvatarAnimations(ps);
                return;
            }
            var editor = Track(Editor.CreateEditor(ps, typeof(PosingSystemEditor)));
            typeof(PosingSystemEditor).GetMethod("AutoDetectOverrideSettings",
                BindingFlags.Instance | BindingFlags.NonPublic).Invoke(editor, null);
        }

        private PosingSystem CreatePosingSystem(AnimatorController source)
        {
            var go = Track(new GameObject("PostureOverrideRegression"));
            SceneManager.MoveGameObjectToScene(go, previewScene);
            var avatar = go.AddComponent<VRCAvatarDescriptor>();
            avatar.baseAnimationLayers = new[]
            {
                new VRCAvatarDescriptor.CustomAnimLayer
                {
                    type = VRCAvatarDescriptor.AnimLayerType.Base,
                    animatorController = source,
                    isDefault = false
                }
            };
            var ps = go.AddComponent<PosingSystem>();
            ps.overrideDefines = new List<PosingSystem.OverrideAnimationDefine>();
            return ps;
        }

        private AnimatorController CreateController(string layerName)
        {
            var controller = Track(new AnimatorController());
            controller.AddLayer(layerName);
            Track(controller.layers[0].stateMachine);
            return controller;
        }

        private void AddState(AnimatorStateMachine machine, string name, Motion motion)
        {
            Track(machine.AddState(name)).motion = motion;
        }

        private AnimationClip CreateClip(string name)
        {
            var clip = Track(new AnimationClip { name = name });
            AnimationUtility.SetEditorCurve(clip,
                EditorCurveBinding.FloatCurve("", typeof(Animator), "Spine Front-Back"),
                AnimationCurve.Constant(0, 1, 0.2f));
            return clip;
        }

        private BlendTree CreateTree(string name, AnimationClip clip)
        {
            var tree = Track(new BlendTree { name = name, blendParameter = "VelocityZ" });
            if (clip != null) tree.AddChild(clip, 0);
            return tree;
        }

        private T Track<T>(T value) where T : UnityEngine.Object
        {
            objectsToDestroy.Add(value);
            return value;
        }
    }
}
