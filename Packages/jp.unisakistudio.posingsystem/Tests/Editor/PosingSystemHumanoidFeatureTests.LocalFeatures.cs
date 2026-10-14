using System.Linq;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor.Animations;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void LocalFeatures_NdmfBuildPreservesAnimationAndMenus_WithExpectedSyncCost(bool synced, bool prebuild)
        {
            var ps = DefaultIdleSettings(true, true);
            ps.features.photography = true;
            ps.features.photographySynced = synced;
            ps.features.breathingControl = synced;
            ps.features.photographyBackend = PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive;
            if (prebuild) PrebuildPersistedSettings(ps);
            BuildDefaultIdleAvatar((descriptor, additive) =>
            {
                var parameters = descriptor.expressionParameters.parameters;
                foreach (var name in new[] { PosingSystemFeatureUtility.PhotoDistance, PosingSystemFeatureUtility.PhotoYaw })
                    Assert.That(parameters.Single(p => p.name == name).networkSynced, Is.EqualTo(synced));
                Assert.That(parameters.Any(p => p.name == "USSPS_Breathing"), Is.EqualTo(synced));
                Assert.That(parameters.Where(p => p.networkSynced).Sum(p => p.valueType == VRCExpressionParameters.ValueType.Bool ? 1 : 8), Is.EqualTo(synced ? 25 : 8));
                var layer = additive.layers.Single(l => l.name == PosingSystemConverter.AdditiveBreathingLayerName);
                Assert.That(layer.defaultWeight, Is.EqualTo(1));
                Assert.That(layer.stateMachine.states.Any(s => s.state.motion != null && s.state.motion.name.Contains("Breathing")), Is.True);
                Assert.That(layer.stateMachine.anyStateTransitions.SelectMany(t => t.conditions).Any(c => c.parameter == "USSPS_Breathing"), Is.EqualTo(synced));
                var controls = FlattenMenu(descriptor.expressionsMenu).ToArray();
                Assert.That(controls.Any(c => c.subParameters?.Any(p => p.name == PosingSystemFeatureUtility.PhotoDistance) ?? false), Is.True);
                Assert.That(controls.Any(c => c.parameter?.name == "USSPS_Breathing"), Is.EqualTo(synced));
            });
        }

        [Test]
        public void LocalFeatures_MultipleSystems_OnlySyncsSharedParametersOnce()
        {
            var first = DefaultIdleSettings(false, true);
            var second = DefaultIdleSettings(false, true);
            first.features.photography = second.features.photography = true;
            first.features.photographySynced = first.features.breathingControl = false;
            second.features.photographySynced = second.features.breathingControl = true;
            Assert.That(PosingSystemFeatureUtility.AvatarOptionalBits(first), Is.EqualTo(17));
            BuildDefaultIdleAvatar((descriptor, additive) =>
            {
                var parameters = descriptor.expressionParameters.parameters;
                Assert.That(parameters.Single(p => p.name == PosingSystemFeatureUtility.PhotoDistance).networkSynced, Is.True);
                Assert.That(parameters.Single(p => p.name == "USSPS_Breathing").networkSynced, Is.True);
                Assert.That(additive.layers.Where(l => l.name == PosingSystemConverter.AdditiveBreathingLayerName)
                    .SelectMany(l => l.stateMachine.anyStateTransitions).SelectMany(t => t.conditions)
                    .Any(c => c.parameter == "USSPS_Breathing"), Is.True);
            });
        }

        [TestCase(true, false, false, 1)]
        [TestCase(true, false, true, 1)]
        [TestCase(false, true, false, 10)]
        [TestCase(false, true, true, 10)]
        [TestCase(true, true, false, 10)]
        [TestCase(true, true, true, 10)]
        public void RolloverAndFaceForward_NdmfBuildCountsOneSharedHeadLock(bool faceForward, bool rollover, bool prebuild, int optionalBits)
        {
            var ps = DefaultIdleSettings(true, false);
            ps.features.faceForward = faceForward;
            ps.features.rollover = rollover;
            if (prebuild) PrebuildPersistedSettings(ps);
            Assert.That(PosingSystemFeatureUtility.AvatarOptionalBits(ps), Is.EqualTo(optionalBits));
            BuildDefaultIdleAvatar((descriptor, additive) =>
            {
                var parameters = descriptor.expressionParameters.parameters.Where(p => p.networkSynced).ToArray();
                Assert.That(parameters.Sum(p => p.valueType == VRCExpressionParameters.ValueType.Bool ? 1 : 8), Is.EqualTo(8 + optionalBits));
                Assert.That(parameters.Count(p => p.name == "HeadTrackingLock"), Is.EqualTo(1));
                Assert.That(parameters.Any(p => p.name == PosingSystemFeatureUtility.RollActive && p.valueType == VRCExpressionParameters.ValueType.Bool), Is.EqualTo(rollover));
                Assert.That(parameters.Any(p => p.name == PosingSystemFeatureUtility.RollSensor && p.valueType == VRCExpressionParameters.ValueType.Float), Is.EqualTo(rollover));
            });
        }

        private static System.Collections.Generic.IEnumerable<VRCExpressionsMenu.Control> FlattenMenu(VRCExpressionsMenu menu)
        {
            if (menu == null) yield break;
            foreach (var control in menu.controls)
            {
                yield return control;
                if (control.subMenu != null) foreach (var child in FlattenMenu(control.subMenu)) yield return child;
            }
        }
    }
}
