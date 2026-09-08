using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemLocomotionControllerTests
    {
        private const string ControllerPath =
            "Packages/jp.unisakistudio.posingsystem/Resources/PosingSystem_Locomotion.controller";

        [Test]
        public void VrPoseTransitions_PrioritizeSourceBeforeDestination()
        {
            var layer = LoadLocomotionLayer();

            foreach (var stateName in new[] { "Standing", "Crouching", "Prone" })
            {
                var state = layer.stateMachine.states.Single(child => child.state.name == stateName).state;
                var poseTransition = state.transitions.Single(transition =>
                    transition.isExit &&
                    transition.conditions.Any(condition => condition.parameter == "USSPS_Pose"));

                Assert.That(
                    poseTransition.interruptionSource,
                    Is.EqualTo(TransitionInterruptionSource.SourceThenDestination),
                    stateName + " must evaluate its source transition before destination transitions.");
            }
        }

        [Test]
        public void RapidCrouchThenStand_DoesNotCreateStandingToStandingTransition()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            Assert.That(controller, Is.Not.Null);

            var gameObject = new GameObject("LocomotionTransitionTest");
            try
            {
                var animator = gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = controller;
                animator.Rebind();

                animator.SetInteger("VRMode", 1);
                animator.SetInteger("TrackingType", 3);
                animator.SetBool("Grounded", true);
                animator.SetInteger("USSPS_Pose", 0);
                animator.Update(0.01f);

                var layerIndex = animator.GetLayerIndex("USSPS_Locomotion");
                Assert.That(layerIndex, Is.GreaterThanOrEqualTo(0));
                Assert.That(
                    animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash,
                    Is.EqualTo(Animator.StringToHash("Standing")));

                animator.SetInteger("USSPS_Pose", 1);
                animator.Update(0.01f);
                Assert.That(animator.IsInTransition(layerIndex), Is.True);
                Assert.That(
                    animator.GetNextAnimatorStateInfo(layerIndex).shortNameHash,
                    Is.EqualTo(Animator.StringToHash("Crouching")));

                animator.Update(0.1f);
                animator.SetInteger("USSPS_Pose", 0);
                animator.Update(0.01f);

                Assert.That(animator.IsInTransition(layerIndex), Is.True);
                Assert.That(
                    animator.GetNextAnimatorStateInfo(layerIndex).shortNameHash,
                    Is.EqualTo(Animator.StringToHash("Crouching")),
                    "A rapid reversal must not interrupt into a Standing-to-Standing self-transition.");

                for (var i = 0; i < 20; i++)
                {
                    animator.Update(0.1f);
                }

                Assert.That(
                    animator.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash,
                    Is.EqualTo(Animator.StringToHash("Standing")),
                    "The animator must settle back to Standing after the reversal.");
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void GeneratedVrPoseTransitions_PrioritizeSourceBeforeDestination()
        {
            var state = new AnimatorState();

            try
            {
                PosingSystemConverter.CreateExitTransitions(
                    PosingSystemConverter.SplitSyncParameters(1, 255), state, true);

                Assert.That(state.transitions, Is.Not.Empty);
                Assert.That(
                    state.transitions.All(transition =>
                        transition.interruptionSource == TransitionInterruptionSource.SourceThenDestination),
                    Is.True);
            }
            finally
            {
                foreach (var transition in state.transitions)
                {
                    Object.DestroyImmediate(transition);
                }
                Object.DestroyImmediate(state);
            }
        }

        [Test]
        public void GetPrebuiltMergeAnimator_RestoresMissingCandidateAndUsesValidCustomController()
        {
            var avatarObject = new GameObject("MissingLocomotionControllerAvatar");
            var customController = new AnimatorController();

            try
            {
                avatarObject.AddComponent<VRCAvatarDescriptor>();
                var posingSystemObject = new GameObject("PosingSystem");
                posingSystemObject.transform.SetParent(avatarObject.transform, false);
                var posingSystem = posingSystemObject.AddComponent<PosingSystem>();

                var missingObject = new GameObject("MissingCommonAnimator");
                missingObject.transform.SetParent(posingSystemObject.transform, false);
                missingObject.AddComponent<DuplicateEraser>().ID =
                    "jp.unisakistudio.posingsystem_locomotion";
                var missingMergeAnimator = missingObject.AddComponent<ModularAvatarMergeAnimator>();
                missingMergeAnimator.animator = null;

                var customObject = new GameObject("ValidCustomAnimator");
                customObject.transform.SetParent(posingSystemObject.transform, false);
                customObject.AddComponent<DuplicateEraser>().ID =
                    "jp.unisakistudio.posingsystem_locomotion";
                var customMergeAnimator = customObject.AddComponent<ModularAvatarMergeAnimator>();
                customMergeAnimator.animator = customController;

                var result = PosingSystemConverter.GetPrebuiltMergeAnimator(posingSystem);
                var defaultController = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);

                Assert.That(defaultController, Is.Not.Null);
                Assert.That(missingMergeAnimator.animator, Is.SameAs(defaultController),
                    "MissingになったControllerを標準Locomotionへ復旧すること");
                Assert.That(result, Is.SameAs(customMergeAnimator),
                    "復旧した標準Controllerではなく、有効なカスタムControllerを再利用すること");
                Assert.That(customMergeAnimator.animator, Is.SameAs(customController),
                    "有効なカスタムControllerを上書きしないこと");
            }
            finally
            {
                Object.DestroyImmediate(avatarObject);
                Object.DestroyImmediate(customController);
            }
        }

        private static AnimatorControllerLayer LoadLocomotionLayer()
        {
            var controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            Assert.That(controller, Is.Not.Null);
            return controller.layers.Single(layer => layer.name == "USSPS_Locomotion");
        }
    }
}
