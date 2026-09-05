using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemTrackingMergeRegressionTests
    {
        private readonly List<UnityEngine.Object> objectsToDestroy =
            new List<UnityEngine.Object>();
        private readonly List<string> assetPathsToDelete =
            new List<string>();

        [TearDown]
        public void TearDown()
        {
            foreach (var value in objectsToDestroy.AsEnumerable().Reverse())
            {
                if (value != null)
                {
                    UnityEngine.Object.DestroyImmediate(value);
                }
            }
            objectsToDestroy.Clear();
            foreach (var assetPath in assetPathsToDelete)
            {
                AssetDatabase.DeleteAsset(assetPath);
            }
            assetPathsToDelete.Clear();
        }

        [Test]
        public void PersistentControllerAsset_IsNeverMutatedByBuildConversion()
        {
            var assetPath = "Assets/__PosingSystemTrackingMergeRegression_" +
                            Guid.NewGuid().ToString("N") + ".controller";
            assetPathsToDelete.Add(assetPath);
            var source = AnimatorController.CreateAnimatorControllerAtPath(assetPath);
            var state = source.layers[0].stateMachine.AddState("PersistentControlled");
            var tracking = state.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
            tracking.trackingHead =
                VRC_AnimatorTrackingControl.TrackingType.Animation;
            var locomotion = state.AddStateMachineBehaviour<VRCAnimatorLocomotionControl>();
            locomotion.disableLocomotion = true;
            AssetDatabase.SaveAssets();

            var avatar = CreateAvatar("PersistentAssetAvatar");
            AddMergeAnimator(avatar.gameObject, "Merge", source);
            ProcessControls(avatar);

            var reloaded = AssetDatabase.LoadAssetAtPath<AnimatorController>(assetPath);
            var reloadedState = FindState(reloaded, "PersistentControlled");
            Assert.That(reloadedState.behaviours
                .OfType<VRCAnimatorTrackingControl>().Single().trackingHead,
                Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.Animation));
            Assert.That(reloadedState.behaviours
                .OfType<VRCAnimatorLocomotionControl>().Count(), Is.EqualTo(1));
            Assert.That(reloadedState.behaviours
                .OfType<VRCAvatarParameterDriver>(), Is.Empty);
        }

        [Test]
        public void SharedOverrideController_IsSeparatedAndSourceGraphRemainsUnchanged()
        {
            var baseClip = Track(new AnimationClip { name = "BaseClip" });
            var overrideClip = Track(new AnimationClip { name = "OverrideClip" });
            var source = Track(CreateControlController(baseClip));
            var sourceState = FindState(source, "Controlled");
            var sourceTracking = sourceState.behaviours
                .OfType<VRCAnimatorTrackingControl>().Single();

            var overrideController = Track(new AnimatorOverrideController(source));
            overrideController.name = "SharedOverride";
            overrideController.ApplyOverrides(
                new List<KeyValuePair<AnimationClip, AnimationClip>>
                {
                    new KeyValuePair<AnimationClip, AnimationClip>(baseClip, overrideClip)
                });

            var avatar = CreateAvatar("SharedOverrideAvatar");
            var first = AddMergeAnimator(avatar.gameObject, "First", overrideController);
            var second = AddMergeAnimator(avatar.gameObject, "Second", overrideController);

            ProcessControls(avatar);

            Assert.That(sourceState.motion, Is.SameAs(baseClip));
            Assert.That(sourceTracking.trackingHead,
                Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.Animation));
            Assert.That(sourceState.behaviours.OfType<VRCAnimatorLocomotionControl>().Count(),
                Is.EqualTo(1));
            Assert.That(sourceState.behaviours.OfType<VRCAvatarParameterDriver>().Count(),
                Is.Zero);

            Assert.That(first.animator, Is.Not.SameAs(overrideController));
            Assert.That(second.animator, Is.Not.SameAs(overrideController));
            Assert.That(first.animator, Is.Not.SameAs(second.animator));

            AssertConvertedController((AnimatorController)first.animator, overrideClip, "1");
            AssertConvertedController((AnimatorController)second.animator, overrideClip, "2");
        }

        [Test]
        public void VirtualControllerRoundTrip_PreservesComplexAnimatorGraph()
        {
            var baseClip = Track(new AnimationClip { name = "GraphBase" });
            var syncedClip = Track(new AnimationClip { name = "SyncedOverride" });
            var controller = Track(CreateComplexController(baseClip, syncedClip));
            var avatar = CreateAvatar("ComplexGraphAvatar");
            var merge = AddMergeAnimator(avatar.gameObject, "Complex", controller);

            RoundTripVirtualControllers(avatar.gameObject);

            var committed = merge.animator as AnimatorController;
            Assert.That(committed, Is.Not.Null);
            Assert.That(committed, Is.Not.SameAs(controller));
            Assert.That(controller.layers[0].stateMachine.stateMachines.Length, Is.EqualTo(2));

            var root = committed.layers[0].stateMachine;
            var leftMachine = root.stateMachines
                .Single(child => child.stateMachine.name == "Left").stateMachine;
            var rightMachine = root.stateMachines
                .Single(child => child.stateMachine.name == "Right").stateMachine;
            var left = leftMachine.states.Single().state;
            var right = rightMachine.states.Single().state;

            Assert.That(left.transitions.Single().destinationState, Is.SameAs(right));
            Assert.That(root.anyStateTransitions.Single().destinationState, Is.SameAs(right));
            Assert.That(root.GetStateMachineTransitions(leftMachine).Single()
                .destinationStateMachine, Is.SameAs(rightMachine));
            Assert.That(leftMachine.behaviours
                .OfType<VRCAnimatorTrackingControl>().Single().trackingHip,
                Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.Tracking));
            Assert.That(right.behaviours
                .OfType<VRCAvatarParameterDriver>().Single().parameters.Single().name,
                Is.EqualTo("probe-state"));

            Assert.That(committed.layers[1].syncedLayerIndex, Is.Zero);
            Assert.That(committed.layers[1].syncedLayerAffectsTiming, Is.True);
            Assert.That(committed.layers[1].GetOverrideMotion(left).name,
                Is.EqualTo(syncedClip.name));
            var syncedDriver = committed.layers[1].GetOverrideBehaviours(left)
                .OfType<VRCAvatarParameterDriver>().Single();
            Assert.That(syncedDriver.parameters.Single().name, Is.EqualTo("probe-sync"));
            Assert.That(syncedDriver.parameters.Single().value, Is.EqualTo(99));
        }

        [Test]
        public void BaseLayerController_UsesSuffixZeroWithoutMutatingSource()
        {
            var clip = Track(new AnimationClip { name = "BaseLayerClip" });
            var source = Track(CreateControlController(clip));
            var sourceState = FindState(source, "Controlled");
            var avatar = CreateAvatar("BaseLayerAvatar");
            avatar.customizeAnimationLayers = true;
            avatar.baseAnimationLayers = new[]
            {
                new VRCAvatarDescriptor.CustomAnimLayer
                {
                    type = VRCAvatarDescriptor.AnimLayerType.FX,
                    isDefault = false,
                    animatorController = source
                }
            };

            ProcessControls(avatar);

            var committed = avatar.baseAnimationLayers[0].animatorController as AnimatorController;
            Assert.That(committed, Is.Not.Null);
            Assert.That(committed, Is.Not.SameAs(source));
            AssertConvertedController(committed, clip, "0");
            Assert.That(sourceState.behaviours.OfType<VRCAnimatorTrackingControl>().Single()
                .trackingHead,
                Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.Animation));
            Assert.That(sourceState.behaviours.OfType<VRCAnimatorLocomotionControl>().Count(),
                Is.EqualTo(1));
        }

        [Test]
        public void NoControlBehaviours_DoesNotAllocateSuffixForMergeAnimator()
        {
            var controller = Track(new AnimatorController { name = "EmptyController" });
            controller.AddLayer(new AnimatorControllerLayer
            {
                name = "Base",
                defaultWeight = 1,
                stateMachine = new AnimatorStateMachine()
            });
            var avatar = CreateAvatar("NoControlsAvatar");
            var merge = AddMergeAnimator(avatar.gameObject, "Empty", controller);

            ProcessControls(avatar);

            var committed = (AnimatorController)merge.animator;
            Assert.That(committed.parameters.Any(parameter =>
                parameter.name.EndsWith("1", StringComparison.Ordinal)), Is.False);
            Assert.That(committed.layers
                .SelectMany(layer => AllStates(layer.stateMachine))
                .SelectMany(state => state.behaviours)
                .OfType<VRCAvatarParameterDriver>(), Is.Empty);
        }

        [Test]
        public void ConfiguredGeneratingPass_ActivatesVirtualControllerIsolation()
        {
            var clip = Track(new AnimationClip { name = "ConfiguredPassClip" });
            var source = Track(CreateControlController(clip));
            var avatar = CreateAvatar("ConfiguredPassAvatar");
            var posingSystem = avatar.gameObject.AddComponent<
                jp.unisakistudio.posingsystem.PosingSystem>();
            posingSystem.mergeTrackingControl = true;
            var merge = AddMergeAnimator(avatar.gameObject, "Merge", source);

            var context = new BuildContext(avatar.gameObject, null, false);
            var processMethod = typeof(AvatarProcessor).GetMethod(
                "ProcessAvatar",
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { typeof(BuildContext), typeof(BuildPhase), typeof(BuildPhase) },
                null);
            Assert.That(processMethod, Is.Not.Null);
            try
            {
                processMethod.Invoke(null, new object[]
                {
                    context,
                    BuildPhase.Generating,
                    BuildPhase.Generating
                });
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException ?? exception;
            }
            finally
            {
                context.DeactivateAllExtensionContexts();
            }

            Assert.That(FindState(source, "Controlled").behaviours
                .OfType<VRCAvatarParameterDriver>(), Is.Empty);
            Assert.That(merge.animator, Is.Not.SameAs(source));
            AssertConvertedController(
                (AnimatorController)merge.animator,
                clip,
                "1");
        }

        [Test]
        public void TwoBuildsFromSameSource_ProduceEquivalentOutputsWithoutSourceMutation()
        {
            var clip = Track(new AnimationClip { name = "RepeatClip" });
            var source = Track(CreateControlController(clip));

            var first = ProcessFreshAvatar(source, "RepeatFirst");
            var second = ProcessFreshAvatar(source, "RepeatSecond");

            CollectionAssert.AreEquivalent(DriverNames(first), DriverNames(second));
            Assert.That(FindState(source, "Controlled").behaviours
                .OfType<VRCAvatarParameterDriver>(), Is.Empty);
            Assert.That(FindState(source, "Controlled").behaviours
                .OfType<VRCAnimatorLocomotionControl>().Count(), Is.EqualTo(1));
        }

        private AnimatorController ProcessFreshAvatar(
            AnimatorController source,
            string name)
        {
            var avatar = CreateAvatar(name);
            var merge = AddMergeAnimator(avatar.gameObject, "Merge", source);
            ProcessControls(avatar);
            return (AnimatorController)merge.animator;
        }

        private void ProcessControls(VRCAvatarDescriptor avatar)
        {
            var context = new BuildContext(avatar.gameObject, null, false);
            context.ActivateExtensionContextRecursive<AnimatorServicesContext>();
            try
            {
                var virtualControllers =
                    context.Extension<AnimatorServicesContext>().ControllerContext;
                InvokeMerge("MergeTrackingControl", avatar, virtualControllers);
                InvokeMerge("MergeLocomotionControl", avatar, virtualControllers);
            }
            finally
            {
                context.DeactivateAllExtensionContexts();
            }
        }

        private void RoundTripVirtualControllers(GameObject avatar)
        {
            var context = new BuildContext(avatar, null, false);
            context.ActivateExtensionContextRecursive<AnimatorServicesContext>();
            context.DeactivateAllExtensionContexts();
        }

        private static void InvokeMerge(
            string methodName,
            VRCAvatarDescriptor avatar,
            VirtualControllerContext controllerContext)
        {
            var method = typeof(PosingSystemConverter).GetMethod(
                methodName,
                BindingFlags.Static | BindingFlags.NonPublic,
                null,
                new[] { typeof(VRCAvatarDescriptor), typeof(VirtualControllerContext) },
                null);
            Assert.That(method, Is.Not.Null);
            try
            {
                method.Invoke(null, new object[] { avatar, controllerContext });
            }
            catch (TargetInvocationException exception)
            {
                throw exception.InnerException ?? exception;
            }
        }

        private VRCAvatarDescriptor CreateAvatar(string name)
        {
            var avatarObject = Track(new GameObject(name));
            var descriptor = avatarObject.AddComponent<VRCAvatarDescriptor>();
            descriptor.baseAnimationLayers =
                Array.Empty<VRCAvatarDescriptor.CustomAnimLayer>();
            descriptor.specialAnimationLayers =
                Array.Empty<VRCAvatarDescriptor.CustomAnimLayer>();
            return descriptor;
        }

        private static ModularAvatarMergeAnimator AddMergeAnimator(
            GameObject avatar,
            string name,
            RuntimeAnimatorController controller)
        {
            var child = new GameObject(name);
            child.transform.SetParent(avatar.transform, false);
            var merge = child.AddComponent<ModularAvatarMergeAnimator>();
            merge.animator = controller;
            return merge;
        }

        private static AnimatorController CreateControlController(AnimationClip motion)
        {
            var controller = new AnimatorController { name = "ControlController" };
            var stateMachine = new AnimatorStateMachine { name = "Base" };
            var state = stateMachine.AddState("Controlled");
            state.motion = motion;

            var tracking = state.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
            tracking.trackingHead =
                VRC_AnimatorTrackingControl.TrackingType.Animation;
            tracking.trackingEyes =
                VRC_AnimatorTrackingControl.TrackingType.Tracking;
            tracking.trackingMouth =
                VRC_AnimatorTrackingControl.TrackingType.Animation;

            var locomotion = state.AddStateMachineBehaviour<VRCAnimatorLocomotionControl>();
            locomotion.disableLocomotion = true;

            controller.layers = new[]
            {
                new AnimatorControllerLayer
                {
                    name = "Base",
                    defaultWeight = 1,
                    stateMachine = stateMachine
                }
            };
            return controller;
        }

        private static AnimatorController CreateComplexController(
            AnimationClip baseClip,
            AnimationClip syncedClip)
        {
            var controller = new AnimatorController { name = "ComplexController" };
            var root = new AnimatorStateMachine { name = "Root" };
            var leftMachine = root.AddStateMachine("Left");
            var rightMachine = root.AddStateMachine("Right");
            var left = leftMachine.AddState("LeftState");
            var right = rightMachine.AddState("RightState");
            left.motion = baseClip;

            left.AddTransition(right);
            root.AddAnyStateTransition(right);
            root.AddStateMachineTransition(leftMachine, rightMachine);
            leftMachine.AddStateMachineBehaviour<VRCAnimatorTrackingControl>().trackingHip =
                VRC_AnimatorTrackingControl.TrackingType.Tracking;
            var stateDriver = right.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            stateDriver.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                name = "probe-state",
                value = 17,
                type = VRC_AvatarParameterDriver.ChangeType.Set
            });

            var layers = new[]
            {
                new AnimatorControllerLayer
                {
                    name = "Base",
                    defaultWeight = 1,
                    stateMachine = root
                },
                new AnimatorControllerLayer
                {
                    name = "Synced",
                    defaultWeight = 1,
                    stateMachine = new AnimatorStateMachine { name = "SyncedRoot" },
                    syncedLayerIndex = 0,
                    syncedLayerAffectsTiming = true
                }
            };
            controller.layers = layers;

            var syncedBehaviour =
                ScriptableObject.CreateInstance<VRCAvatarParameterDriver>();
            syncedBehaviour.parameters.Add(new VRC_AvatarParameterDriver.Parameter
            {
                name = "probe-sync",
                value = 99,
                type = VRC_AvatarParameterDriver.ChangeType.Set
            });
            layers[1].SetOverrideMotion(left, syncedClip);
            layers[1].SetOverrideBehaviours(left, new StateMachineBehaviour[]
            {
                syncedBehaviour
            });
            controller.layers = layers;
            return controller;
        }

        private static void AssertConvertedController(
            AnimatorController controller,
            AnimationClip expectedMotion,
            string suffix)
        {
            var state = FindState(controller, "Controlled");
            Assert.That(state.motion.name, Is.EqualTo(expectedMotion.name));

            var tracking = state.behaviours
                .OfType<VRCAnimatorTrackingControl>().Single();
            Assert.That(tracking.trackingHead,
                Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.NoChange));
            Assert.That(tracking.trackingEyes,
                Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.Tracking));
            Assert.That(tracking.trackingMouth,
                Is.EqualTo(VRC_AnimatorTrackingControl.TrackingType.Animation));
            Assert.That(state.behaviours.OfType<VRCAnimatorLocomotionControl>(),
                Is.Empty);

            var drivers = state.behaviours
                .OfType<VRCAvatarParameterDriver>()
                .SelectMany(driver => driver.parameters)
                .ToDictionary(parameter => parameter.name, parameter => parameter.value);
            Assert.That(drivers["trackingHead" + suffix], Is.Zero);
            Assert.That(drivers["disableLocomotion" + suffix], Is.Zero);
            Assert.That(controller.parameters.Any(parameter =>
                parameter.name == "trackingHead" + suffix &&
                parameter.type == AnimatorControllerParameterType.Bool &&
                parameter.defaultBool), Is.True);
            Assert.That(controller.parameters.Any(parameter =>
                parameter.name == "disableLocomotion" + suffix &&
                parameter.type == AnimatorControllerParameterType.Bool &&
                parameter.defaultBool), Is.True);
        }

        private static AnimatorState FindState(
            AnimatorController controller,
            string name)
        {
            return controller.layers
                .SelectMany(layer => AllStates(layer.stateMachine))
                .Single(state => state.name == name);
        }

        private static IEnumerable<AnimatorState> AllStates(
            AnimatorStateMachine stateMachine)
        {
            foreach (var childState in stateMachine.states)
            {
                yield return childState.state;
            }
            foreach (var childMachine in stateMachine.stateMachines)
            {
                foreach (var state in AllStates(childMachine.stateMachine))
                {
                    yield return state;
                }
            }
        }

        private static IEnumerable<string> DriverNames(
            AnimatorController controller)
        {
            return controller.layers
                .SelectMany(layer => AllStates(layer.stateMachine))
                .SelectMany(state => state.behaviours
                    .OfType<VRCAvatarParameterDriver>())
                .SelectMany(driver => driver.parameters)
                .Select(parameter => parameter.name)
                .OrderBy(name => name)
                .ToArray();
        }

        private T Track<T>(T value) where T : UnityEngine.Object
        {
            objectsToDestroy.Add(value);
            return value;
        }
    }
}
