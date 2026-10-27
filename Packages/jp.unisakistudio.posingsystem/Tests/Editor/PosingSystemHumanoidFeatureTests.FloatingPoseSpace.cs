using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using jp.unisakistudio.posingsystem;
using nadena.dev.ndmf;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(PosingSystemFeatureSettings.FloatingPoseSpaceMode.Disabled, false)]
        [TestCase(PosingSystemFeatureSettings.FloatingPoseSpaceMode.Disabled, true)]
        [TestCase(PosingSystemFeatureSettings.FloatingPoseSpaceMode.EnterOnce, false)]
        [TestCase(PosingSystemFeatureSettings.FloatingPoseSpaceMode.EnterOnce, true)]
        [TestCase(PosingSystemFeatureSettings.FloatingPoseSpaceMode.ReenterOnChange, false)]
        [TestCase(PosingSystemFeatureSettings.FloatingPoseSpaceMode.ReenterOnChange, true)]
        [TestCase(PosingSystemFeatureSettings.FloatingPoseSpaceMode.ExitThenReenter, false)]
        [TestCase(PosingSystemFeatureSettings.FloatingPoseSpaceMode.ExitThenReenter, true)]
        [TestCase(PosingSystemFeatureSettings.FloatingPoseSpaceMode.EnterEveryFrame, false)]
        [TestCase(PosingSystemFeatureSettings.FloatingPoseSpaceMode.EnterEveryFrame, true)]
        [TestCase(PosingSystemFeatureSettings.FloatingPoseSpaceMode.ReenterOnChangeImmediately, false)]
        [TestCase(PosingSystemFeatureSettings.FloatingPoseSpaceMode.ReenterOnChangeImmediately, true)]
        [TestCase(PosingSystemFeatureSettings.FloatingPoseSpaceMode.ReenterHeightImmediately, false)]
        [TestCase(PosingSystemFeatureSettings.FloatingPoseSpaceMode.ReenterHeightImmediately, true)]
        public void FloatingPoseSpace_LegacyModesMigrateToMenuOnly(PosingSystemFeatureSettings.FloatingPoseSpaceMode mode, bool vr)
            => CheckFloatingPoseSpaceLifecycle(mode, vr, 60);

        [TestCase(false, 30)]
        [TestCase(false, 60)]
        [TestCase(false, 90)]
        [TestCase(false, 144)]
        [TestCase(true, 30)]
        [TestCase(true, 60)]
        [TestCase(true, 90)]
        [TestCase(true, 144)]
        public void FloatingPoseSpace_HeightMenuOnly_HoldsViewThroughPoseChangesAndMenuClosure(bool vr, int fps)
            => CheckFloatingPoseSpaceLifecycle(PosingSystemFeatureSettings.FloatingPoseSpaceMode.HeightMenuOnly, vr, fps);

        private void CheckFloatingPoseSpaceLifecycle(PosingSystemFeatureSettings.FloatingPoseSpaceMode mode, bool vr, int fps)
        {
            root.AddComponent<VRCAvatarDescriptor>();
            var settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("ecde0b1d67f9e854992715d64f778d8c")), root.transform);
            var ps = settings.GetComponent<PosingSystem>();
            ps.features = new PosingSystemFeatureSettings { floating = true, mirror = false, footHeight = false,
                breathing = false, fingerGestureControl = false, floatingBackend = PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive,
                floatingPoseSpace = mode };
            Assert.That(PosingSystemFeatureUtility.OptionalBits(ps), Is.EqualTo(8));
            testFolder = "Assets/USSPS_FeatureTests_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", testFolder.Substring(7));
            var controller = AnimatorController.CreateAnimatorControllerAtPath(testFolder + "/Base.controller");
            var layers = controller.layers; layers[0].name = "USSPS_Locomotion"; controller.layers = layers;
            controller.AddParameter("PoseChoice", AnimatorControllerParameterType.Int);
            controller.AddParameter("FootHeight", AnimatorControllerParameterType.Float);
            var main = controller.layers[0].stateMachine;
            var poseStates = new List<AnimatorState>();
            for (var i = 0; i < 3; i++)
            {
                var state = main.AddState("Pose_" + i); poseStates.Add(state);
                state.motion = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.Euler(i == 0 ? 90 : 0, 0, 0), i == 2 ? "proxy_Walk" : "Pose_" + i);
                assets.Add(state.motion);
                if (i != 2)
                {
                    var original = state.AddStateMachineBehaviour<VRCAnimatorTemporaryPoseSpace>();
                    original.enterPoseSpace = true;
                }
            }
            for (var i = 0; i < 3; i++) for (var j = 0; j < 3; j++)
                if (i != j)
                {
                    var transition = poseStates[i].AddTransition(poseStates[j]);
                    transition.duration = .3f; transition.hasFixedDuration = true; transition.hasExitTime = false;
                    transition.AddCondition(AnimatorConditionMode.Equals, j, "PoseChoice");
                }
            PosingSystemConverter.GetCommonMergeAnimator(ps).animator = controller;
            var context = new BuildContext(root, null, false);
            var graph = PlayableGraph.Create("FloatingPoseSpaceLifecycle");
            var activationGraph = PlayableGraph.Create("FloorHeightActivationLifecycle");
            var frameNumber = 0;
            try
            {
                PosingSystemConverter.ConvertOptionalFeatures(ps, context.AssetSaver);
                PosingSystemConverter.ConvertOptionalFeatures(ps, context.AssetSaver);
                Assert.That(ps.features.floatingPoseSpace, Is.EqualTo(PosingSystemFeatureSettings.FloatingPoseSpaceMode.HeightMenuOnly));
                var index = Array.FindIndex(controller.layers, l => l.name == PosingSystemConverter.FloatingPoseSpacePrefix + "Control");
                Assert.That(index, Is.GreaterThan(0));
                var control = controller.layers[index];
                Assert.That(controller.layers.Count(l => l.name == control.name), Is.EqualTo(1));
                Assert.That(control.stateMachine.defaultState.behaviours.OfType<VRCAnimatorTemporaryPoseSpace>(), Is.Empty, "Idle must not clear the existing desktop view");
                Assert.That(controller.layers.Where(l => l.name.StartsWith(PosingSystemConverter.FloatingPoseSpacePrefix))
                    .SelectMany(l => l.stateMachine.states).SelectMany(s => s.state.behaviours).OfType<VRCAnimatorTrackingControl>(), Is.Empty);
                var info = controller.layers.Single(l => l.name == PosingSystemConverter.FloatingPoseSpacePrefix + "Info");
                foreach (var state in poseStates) Assert.That(info.GetOverrideBehaviours(state), Is.Empty);
                var stateByHash = control.stateMachine.states.Select(s => s.state).ToDictionary(s => Animator.StringToHash(s.name));
                var enteredNames = new List<string>();
                var poseSpaceCalls = new List<(string name, bool enter)>();
                var requestFrames = new List<int>();
                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                var playable = AnimatorControllerPlayable.Create(graph, controller);
                playable.SetInteger("VRMode", vr ? 1 : 0);
                playable.SetFloat(PosingSystemFeatureUtility.FloatHeight, .5f);
                var activationController = (AnimatorController)ps.GetComponentsInChildren<nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator>()
                    .Single(m => m.name == "USSPS_FloatActivation").animator;
                activationGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                var activation = AnimatorControllerPlayable.Create(activationGraph, activationController);
                AnimationPlayableOutput.Create(activationGraph, "Activation", animator).SetSourcePlayable(activation);
                activationGraph.Play();
                var activationIndex = Array.FindIndex(activationController.layers, l => l.name == "床の高さの有効化");
                var activationStates = activationController.layers[activationIndex].stateMachine.states.Select(s => s.state)
                    .ToDictionary(s => Animator.StringToHash(s.name));
                AnimationPlayableOutput.Create(graph, "Avatar", animator).SetSourcePlayable(playable);
                graph.Play();
                var previous = 0;
                Tick(30);
                Assert.That(poseSpaceCalls, Is.Empty);
                playable.SetFloat(PosingSystemFeatureUtility.FloatHeight, .2f);
                Tick(30);
                Assert.That(poseSpaceCalls, Is.Empty, "Remote avatars must not execute viewpoint requests");
                playable.SetBool("IsLocal", true);
                Tick(30);
                var updates = 0;
                Assert.That(CountEnter(), Is.EqualTo(1));
                Assert.That(controller.layers.Any(l => l.name.EndsWith("KeyDelta")), Is.False, "Pose transitions must not request repeated viewpoint samples");
                playable.SetFloat(PosingSystemFeatureUtility.FloatHeight, .6f);
                playable.SetFloat("FootHeight", .9f);
                playable.SetInteger("PoseChoice", 1);
                Tick(fps * 2);
                Assert.That(CountEnter(), Is.EqualTo(1), "Closed radial: neither height nor a 300ms pose transition may refresh the view");
                playable.SetBool(PosingSystemFeatureUtility.FloatHeightAdjusting, true);
                Tick(fps);
                Assert.That(CountEnter(), Is.EqualTo(2), "The changed height may be sampled only once the height radial is open");
                Tick(fps);
                Assert.That(CountEnter(), Is.EqualTo(2), "Merely leaving the radial open must not continuously re-enter");
                playable.SetInteger("PoseChoice", 0);
                playable.SetFloat("FootHeight", .1f);
                Tick(fps);
                Assert.That(CountEnter(), Is.EqualTo(2), "Pose and foot-height changes are ignored even while the radial is open");
                var changedAt = frameNumber;
                playable.SetFloat(PosingSystemFeatureUtility.FloatHeight, .7f);
                Tick(30);
                Assert.That(CountEnter(), Is.EqualTo(3));
                CheckLatency(changedAt);
                var beforeDrag = CountEnter();
                for (var i = 0; i < 20; i++)
                {
                    playable.SetFloat(PosingSystemFeatureUtility.FloatHeight, .1f + i * .02f);
                    Tick(1);
                }
                Tick(30);
                Assert.That(CountEnter() - beforeDrag, Is.GreaterThanOrEqualTo(5));
                updates = CountEnter();
                // Closing before an outstanding height change is evaluated cancels the update.
                playable.SetFloat(PosingSystemFeatureUtility.FloatHeight, .9f);
                playable.SetBool(PosingSystemFeatureUtility.FloatHeightAdjusting, false);
                playable.SetInteger("PoseChoice", 2);
                Tick(fps * 2);
                playable.SetInteger("PoseChoice", 1);
                Tick(fps);
                Assert.That(CountEnter(), Is.EqualTo(updates), "Closing the radial and changing to/from walking must leave the view alone");
                Assert.That(poseSpaceCalls.All(c => c.enter), Is.True, "No transient Exit during pose changes or menu closure");
                playable.SetBool(PosingSystemFeatureUtility.FloatHeightAdjusting, true);
                Tick(30);
                Assert.That(CountEnter(), Is.EqualTo(++updates));
                playable.SetBool(PosingSystemFeatureUtility.FloatHeightAdjusting, false);
                Tick(30);
                playable.SetFloat(PosingSystemFeatureUtility.FloatHeight, .5f);
                Tick(40);
                Assert.That(enteredNames.Last(), Is.EqualTo("Idle"));
                Assert.That(poseSpaceCalls.Count(c => c.name == "Release" && !c.enter), Is.EqualTo(1));
                Assert.That(poseSpaceCalls.Count(c => c.name == "RestoreDesktop" && c.enter), Is.EqualTo(vr ? 0 : 1));
                playable.SetFloat(PosingSystemFeatureUtility.FloatHeight, .2f);
                Tick(30);
                Assert.That(CountEnter(), Is.EqualTo(updates + 1));
                playable.SetInteger("PoseChoice", 2);
                Tick(fps);
                Assert.That(CountEnter(), Is.EqualTo(updates + 1), "Once started, menu-only floating must not resample or release due to walking");
                Assert.That(poseSpaceCalls.Last().enter, Is.True);
                playable.SetFloat(PosingSystemFeatureUtility.FloatHeight, .5f);
                Tick(fps);
                Assert.That(poseSpaceCalls.Last().enter, Is.False, "Explicit disable must still release the view");
                graph.Destroy();
                ps.features.floating = false;
                PosingSystemConverter.ConvertOptionalFeatures(ps, context.AssetSaver);
                Assert.That(controller.layers.Any(l => l.name.StartsWith(PosingSystemConverter.FloatingPoseSpacePrefix)), Is.False);
                ps.features.floating = true;
                ps.features.floatingPoseSpace = PosingSystemFeatureSettings.FloatingPoseSpaceMode.Disabled;
                PosingSystemConverter.ConvertOptionalFeatures(ps, context.AssetSaver);
                Assert.That(ps.features.floatingPoseSpace, Is.EqualTo(PosingSystemFeatureSettings.FloatingPoseSpaceMode.HeightMenuOnly));
                Assert.That(controller.layers.Any(l => l.name.StartsWith(PosingSystemConverter.FloatingPoseSpacePrefix)), Is.True);

                int CountEnter() => poseSpaceCalls.Count(c => c.name == "Entered" && c.enter);
                void CheckLatency(int changedAt)
                {
                    var frames = requestFrames.First(f => f > changedAt) - changedAt;
                    Assert.That(frames, Is.LessThanOrEqualTo(4), "Only Animator propagation frames, with no fixed-time wait");
                }
                void Tick(int frames)
                {
                    for (var frame = 0; frame < frames; frame++)
                    {
                        frameNumber++;
                        activation.SetFloat(PosingSystemFeatureUtility.FloatHeight, playable.GetFloat(PosingSystemFeatureUtility.FloatHeight));
                        activationGraph.Evaluate(1f / fps);
                        var activationState = activationStates[activation.GetCurrentAnimatorStateInfo(activationIndex).shortNameHash];
                        var activeValue = activationState.behaviours.OfType<VRCAvatarParameterDriver>().Single().parameters.Single().value;
                        // Only the SDK driver effect is simulated; both Animator state machines are generated production code.
                        playable.SetBool(PosingSystemFeatureUtility.FloatActive, activeValue != 0);
                        graph.Evaluate(1f / fps);
                        var hash = playable.GetCurrentAnimatorStateInfo(index).shortNameHash;
                        if (hash == previous || !stateByHash.TryGetValue(hash, out var current)) continue;
                        previous = hash; enteredNames.Add(current.name);
                        foreach (var behaviour in current.behaviours.OfType<VRCAnimatorTemporaryPoseSpace>())
                        {
                            Assert.That(behaviour.fixedDelay && behaviour.delayTime == 0, Is.True);
                            poseSpaceCalls.Add((current.name, behaviour.enterPoseSpace));
                            if (behaviour.enterPoseSpace) requestFrames.Add(frameNumber);
                        }
                        // The VRChat client owns these effects. Simulate only the SDK Copy operation here;
                        // transitions/difference trees are the actual generated Animator, not a model of it.
                        foreach (var driver in current.behaviours.OfType<VRCAvatarParameterDriver>())
                        {
                            Assert.That(driver.localOnly, Is.True);
                            foreach (var parameter in driver.parameters)
                            {
                                Assert.That(parameter.type, Is.EqualTo(VRC_AvatarParameterDriver.ChangeType.Copy));
                                playable.SetFloat(parameter.name, playable.GetFloat(parameter.source));
                            }
                        }
                    }
                }
            }
            finally
            {
                if (graph.IsValid()) graph.Destroy();
                if (activationGraph.IsValid()) activationGraph.Destroy();
                context.DeactivateAllExtensionContexts();
            }
        }
    }
}
