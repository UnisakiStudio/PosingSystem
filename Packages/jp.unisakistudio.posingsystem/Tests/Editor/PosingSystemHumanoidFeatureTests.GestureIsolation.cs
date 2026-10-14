using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(false, false, true)] [TestCase(false, true, true)]
        [TestCase(true, true, true)] [TestCase(true, false, false)] [TestCase(true, false, true)]
        public void RolloverGesture_CreatedOnlyForEligibleLegacyPoses(bool legacyClip, bool perPose, bool enabled)
        {
            root.AddComponent<VRCAvatarDescriptor>();
            var obj = new GameObject("Poses"); obj.transform.SetParent(root.transform, false);
            var ps = obj.AddComponent<PosingSystem>();
            var source = Clip();
            ps.features = new PosingSystemFeatureSettings { rollover = true, rolloverAnimation = legacyClip ? source : null };
            ps.defines = new List<PosingSystem.LayerDefine> { new("Sleep", "Sleep", "Sleep", 30, "Sleep", new List<PosingSystem.AnimationDefine> {
                new(source, "Pose") { isRollover = perPose, enabled = enabled, syncdParameterValue = 4 } }) };
            try
            {
                typeof(PosingSystemConverter).GetMethod("BuildRollover", BindingFlags.Static | BindingFlags.NonPublic)
                    .Invoke(null, new object[] { ps, new[] { ps }, null });
                var merges = ps.GetComponentsInChildren<ModularAvatarMergeAnimator>();
                if (!legacyClip || perPose || !enabled) { Assert.That(merges, Is.Empty); return; }
                var merge = merges.Single();
                Assert.That(merge.layerType, Is.EqualTo(VRCAvatarDescriptor.AnimLayerType.Gesture));
                Assert.That(merge.matchAvatarWriteDefaults, Is.True);
                var controller = (AnimatorController)merge.animator;
                var mapping = controller.layers[0];
                for (var i = 0; i < (int)AvatarMaskBodyPart.LastBodyPart; i++)
                    Assert.That(mapping.avatarMask.GetHumanoidBodyPartActive((AvatarMaskBodyPart)i), Is.False);
                Assert.That(controller.layers[1].stateMachine.states.Count(s => s.state.timeParameterActive), Is.EqualTo(1));
                Assert.That(controller.layers[1].stateMachine.states.Single(s => s.state.timeParameterActive).state.motion, Is.SameAs(source));
            }
            finally
            {
                foreach (var merge in ps.GetComponentsInChildren<ModularAvatarMergeAnimator>())
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(merge.animator));
            }
        }
    }
}
