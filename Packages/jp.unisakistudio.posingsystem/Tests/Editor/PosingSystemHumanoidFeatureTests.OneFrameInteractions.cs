using System;
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(false, false, 30)] [TestCase(false, true, 30)]
        [TestCase(false, false, 60)] [TestCase(false, true, 60)]
        [TestCase(false, false, 90)] [TestCase(false, true, 90)]
        [TestCase(false, false, 144)] [TestCase(false, true, 144)]
        [TestCase(true, false, 30)] [TestCase(true, true, 30)]
        [TestCase(true, false, 60)] [TestCase(true, true, 60)]
        [TestCase(true, false, 90)] [TestCase(true, true, 90)]
        [TestCase(true, false, 144)] [TestCase(true, true, 144)]
        public void Compiler_ONE29_RapidHeightAndPhotoFloorInterruptionsRecoverLikeReference(bool vr, bool selected, int fps)
        {
            var settings = CompilerSettings();
            settings.features = new PosingSystemFeatureSettings {
                mirror = true, footHeight = true, photography = true, floating = true,
                faceForward = true, breathing = false, fingerGestureControl = false
            };
            var reference = CompileSettings(settings, false);
            var candidate = CompileSettings(settings, true);
            // Same explicit initialization as ONE06: the known additive initial -1
            // contamination must not make a non-selecting reference look compatible.
            foreach (var controller in new[] { reference, candidate })
            {
                var parameters = controller.parameters;
                foreach (var p in parameters.Where(p => new[] { "USSPS_IsShallow", "USSPS_IsDeep", "USSPS_IsSleep" }.Contains(p.name)))
                    p.defaultFloat = 0;
                controller.parameters = parameters;
            }
            PosingSystemOneFrameSelection.Apply(candidate, assets.Add);
            var expected = Run(reference);
            var actual = Run(candidate);
            CollectionAssert.AreEqual(expected.Select(x => x.semantic), actual.Select(x => x.semantic),
                "Pose selection, SDK controls and settled layer states must recover after interrupted input");
            for (var i = 0; i < actual.Count; i++)
                Assert.That(actual[i].body.Zip(expected[i].body, (a, b) => Mathf.Abs(a - b)).Max(), Is.LessThan(.003f),
                    "Settled Humanoid output differs at " + actual[i].label);

            List<(string label, string semantic, float[] body)> Run(AnimatorController controller)
            {
                using var rig = new ControllerCompilerRuntimeProbe(animator, controller, true, vr);
                using var handler = new HumanPoseHandler(animator.avatar, root.transform);
                foreach (var define in settings.defines)
                    rig.Set(define.paramName, selected ? CompilerBinding(controller, define.paramName).select : 0);
                rig.Set("USSPS_FloatHeight", .5f); rig.Set("FootHeight", .5f);
                rig.Set("USSPS_PhotoDistance", .15f); rig.Set("USSPS_PhotoYaw", .5f);
                var result = new List<(string label, string semantic, float[] body)>();
                Capture("initial", 1f, 0);
                foreach (var interval in new[] { 1, 2, 3 })
                {
                    rig.Set("USSPS_Photo", 1);
                    rig.Set("USSPS_FloatHeightAdjusting", 1);
                    // Float activation belongs to a separate FX controller in the client.
                    // This Base graph probe supplies that value explicitly, not as a claim
                    // of testing cross-playable-layer or VRChat IK scheduling.
                    rig.Set("USSPS_Float", 1);
                    for (var cycle = 0; cycle < 8; cycle++)
                    {
                        foreach (var height in vr ? new[] { .805f, .795f, .595f, .585f, .345f, .335f, 1f }
                                                 : new[] { .6f, 1f, .35f, 1f })
                        {
                            rig.Set("Upright", height);
                            rig.Set("USSPS_FloatHeight", cycle % 2 == 0 ? .47f : .53f);
                            rig.Set("USSPS_Mirror", cycle % 2);
                            rig.Tick(interval, fps);
                        }
                    }
                    // Short off/on pulse interrupts the photo exit's floor settle timer.
                    rig.Set("USSPS_Photo", 0); rig.Tick(1, fps);
                    rig.Set("USSPS_Photo", 1); rig.Tick(1, fps);
                    rig.Set("USSPS_FloatHeightAdjusting", 0);
                    rig.Set("USSPS_Photo", 0);
                    Capture("floor-held-" + interval, .6f, 1);
                    rig.Set("USSPS_FloatHeight", .5f); rig.Set("USSPS_Float", 0);
                    rig.Set("USSPS_Mirror", 0);
                    Capture("floor-released-" + interval, .35f, 2);
                    Capture("standing-restored-" + interval, 1f, 0);
                }
                return result;

                void Capture(string label, float upright, int group)
                {
                    rig.Set("Upright", upright); rig.Tick(fps * 2, fps);
                    Assert.That(rig.Playable.GetInteger("LocomotionTypeGroup"), Is.EqualTo(group), label);
                    Assert.That(rig.LocomotionDisabled, Is.False, label + ": movement must not remain disabled");
                    var pose = new HumanPose(); handler.GetHumanPose(ref pose);
                    var forward = pose.bodyRotation * Vector3.forward;
                    var up = pose.bodyRotation * Vector3.up;
                    var body = new[] { pose.bodyPosition.x, pose.bodyPosition.y, pose.bodyPosition.z,
                        forward.x, forward.y, forward.z, up.x, up.y, up.z }.Concat(pose.muscles).ToArray();
                    Assert.That(body.All(x => !float.IsNaN(x) && !float.IsInfinity(x)), Is.True, label);
                    result.Add((label, label + ":" + OneFrameSemanticSnapshot(rig), body));
                }
            }
        }
    }
}
