using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using VRC.SDK3.Avatars.Components;
using VRC.SDKBase;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void FloorHeight_DerivesActivationOnBothClients_AndPhotoExitReleasesFeet(bool isLocal)
        {
            var ps = PersistedBuildSettings();
            PrebuildPersistedSettings(ps);
            var floating = (AnimatorController)ps.GetComponentsInChildren<ModularAvatarMergeAnimator>()
                .Single(m => m.name == "USSPS_FloatActivation").animator;
            var layerIndex = System.Array.FindIndex(floating.layers, l => l.name == "床の高さの有効化");
            var layer = floating.layers[layerIndex];
            var states = layer.stateMachine.states.Select(s => s.state).ToArray();
            var graph = PlayableGraph.Create("FloorHeightActivationTest");
            try
            {
                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                var playable = AnimatorControllerPlayable.Create(graph, floating);
                AnimationPlayableOutput.Create(graph, "Test", animator).SetSourcePlayable(playable);
                graph.Play();
                foreach (var height in new[] { 0f, 1f / 255, .5f, 1f, 0f, .25f, 0f })
                {
                    playable.SetFloat(PosingSystemFeatureUtility.FloatHeight, height);
                    for (var i = 0; i < 4; i++) graph.Evaluate(1f / 60);
                    var state = states.Single(s => playable.GetCurrentAnimatorStateInfo(layerIndex).shortNameHash == Animator.StringToHash(s.name));
                    var driver = state.behaviours.OfType<VRCAvatarParameterDriver>().Single();
                    Assert.That(driver.localOnly, Is.False, "Remote clients derive activation from synced height too");
                    var change = driver.parameters.Single();
                    Assert.That(change.name, Is.EqualTo(PosingSystemFeatureUtility.FloatActive));
                    Assert.That(change.type, Is.EqualTo(VRC_AvatarParameterDriver.ChangeType.Set));
                    // VRChat executes the parameter driver. Test its effect without requiring the client.
                    if (isLocal || !driver.localOnly) playable.SetBool(change.name, change.value != 0);
                    Assert.That(playable.GetBool(PosingSystemFeatureUtility.FloatActive), Is.EqualTo(height > 0), "height=" + height);
                }
            }
            finally { graph.Destroy(); }

            var controller = (AnimatorController)PosingSystemConverter.GetPrebuiltMergeAnimator(ps).animator;
            var reset = controller.layers.Single(l => l.name == "USSPS_PhotoReset").stateMachine.states
                .Select(s => s.state).Single(s => s.behaviours.OfType<VRCAvatarParameterDriver>().Any());
            var resetDriver = reset.behaviours.OfType<VRCAvatarParameterDriver>().Single();
            Assert.That(resetDriver.localOnly, Is.True);
            Assert.That(resetDriver.parameters.Single(p => p.name == "LocomotionLock").value, Is.Zero);
            Assert.That(resetDriver.parameters.Single(p => p.name == PosingSystemFeatureUtility.PhotoDistance).value, Is.Zero);
            Assert.That(resetDriver.parameters.Single(p => p.name == PosingSystemFeatureUtility.PhotoYaw).value, Is.EqualTo(.5f));
            Assert.That(resetDriver.parameters.Select(p => p.name), Does.Not.Contain("HeadTrackingLock"));
            Assert.That(resetDriver.parameters.Select(p => p.name), Does.Not.Contain("HandTrackingLock"));
            Assert.That(reset.transitions.Single().conditions.Single().parameter, Is.EqualTo("USSPS_PhotoReset"));
        }
    }
}
