using System;
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    // VR: the head height chooses the pose, and a pose change is a blend of half a second. Until 3.2.0-alpha.4
    // a blend that had started ran to its end, so a short dip of the head played the whole way down and
    // back while the user was standing again, and repeated squats ran out of step (a post on X of
    // 2026-11-18; inquiry 5be3b611 of 2026-09-03 is the defect the earlier lock had been made for).
    public partial class PosingSystemHumanoidFeatureTests
    {
        private const float StandSpine = .15f, CrouchSpine = .4f, DeepSpine = -.3f;

        private AnimatorController ResponsiveController(bool selected, bool oneFrame, out PosingSystem.LayerDefine[] groups)
        {
            var ps = CompilerSettings();
            var values = new[] { StandSpine, CrouchSpine, DeepSpine };
            for (var group = 0; group < 3; group++)
            {
                var clip = Clip();
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), "Spine Front-Back"), AnimationCurve.Constant(0, 1, values[group]));
                ps.defines[group].animations[0].animationClip = clip;
            }
            var controller = CompileSettings(ps, true);
            if (oneFrame)
            {
                // As at the end of a build: the desktop pass adds the final classifiers, then the pose changes may be interrupted.
                PosingSystemOneFrameSelection.Apply(controller, assets.Add);
                Assert.That(PosingSystemResponsivePoseSwitch.Apply(controller), Is.GreaterThan(0));
            }
            Assert.That(PosingSystemResponsivePoseSwitch.Apply(controller), Is.Zero, oneFrame ? "Applying it again changes nothing" : "Without the final classifiers nothing is changed");
            groups = selected ? ps.defines.ToArray() : new PosingSystem.LayerDefine[0];
            return controller;
        }

        private sealed class PoseRun : IDisposable
        {
            private readonly ControllerCompilerRuntimeProbe rig;
            private readonly HumanPoseHandler handler;
            private readonly int fps;
            private readonly Dictionary<string, AnimatorControllerParameterType> types;
            private readonly float[] standing;
            private float[] last;
            private HumanPose pose;
            /// <summary>How far the body is from the standing pose (largest muscle difference), and the largest change in one evaluation.</summary>
            internal float Value, MaxStep;

            internal PoseRun(Animator animator, Transform root, AnimatorController controller, PosingSystem.LayerDefine[] groups, int fps)
            {
                this.fps = fps;
                rig = new ControllerCompilerRuntimeProbe(animator, controller, true, true);
                foreach (var group in groups) rig.Set(group.paramName, CompilerBinding(controller, group.paramName).select);
                handler = new HumanPoseHandler(animator.avatar, root);
                types = controller.parameters.GroupBy(p => p.name).Where(g => g.Key != "Upright").ToDictionary(g => g.Key, g => g.First().type);
                Hold(1, 3);
                standing = (float[])last.Clone();
                Value = MaxStep = 0;
            }

            internal void Step(float upright)
            {
                rig.Set("Upright", upright); rig.Tick(1, fps); handler.GetHumanPose(ref pose);
                Value = 0;
                for (var m = 0; m < pose.muscles.Length; m++)
                {
                    if (last != null) MaxStep = Mathf.Max(MaxStep, Mathf.Abs(pose.muscles[m] - last[m]));
                    if (standing != null) Value = Mathf.Max(Value, Mathf.Abs(pose.muscles[m] - standing[m]));
                }
                last = (float[])pose.muscles.Clone();
            }
            internal void Hold(float upright, float seconds) { for (var i = 0; i < Mathf.CeilToInt(seconds * fps); i++) Step(upright); }
            internal Dictionary<string, float> Parameters() => types.ToDictionary(p => p.Key, p =>
                p.Value == AnimatorControllerParameterType.Bool ? (rig.Playable.GetBool(p.Key) ? 1f : 0f)
                : p.Value == AnimatorControllerParameterType.Int ? rig.Playable.GetInteger(p.Key)
                : p.Value == AnimatorControllerParameterType.Float ? rig.Playable.GetFloat(p.Key) : 0f);
            internal string Controls() => rig.Snapshot().Split(new[] { ";states" }, StringSplitOptions.None)[0].Split(new[] { ";locomotion" }, 2, StringSplitOptions.None)[1];
            public void Dispose() { handler.Dispose(); rig.Dispose(); }
        }

        [TestCase(90, false)] [TestCase(90, true)] [TestCase(72, true)] [TestCase(120, true)] [TestCase(30, true)] [TestCase(45, false)]
        public void ResponsivePose_ShortDipTurnsBackWhereItIs(int fps, bool selected)
        {
            var controller = ResponsiveController(selected, true, out var groups);
            using var run = new PoseRun(animator, root.transform, controller, groups, fps);
            run.Hold(.75f, 3);
            var crouch = run.Value;
            Assert.That(crouch, Is.GreaterThan(.1f), "Fixture: the pose at three quarters of the height differs from standing");
            run.Hold(1, 3);
            Assert.That(run.Value, Is.LessThan(.002f), "Fixture: the pose comes back");
            // A blend of half a second moves by this much per evaluation; an interruption must not add a jump.
            var smooth = crouch / (.5f * fps) * 1.6f + .002f;

            foreach (var hold in new[] { .05f, .1f, .2f })
            {
                run.MaxStep = 0;
                var furthest = 0f; var back = 0f;
                run.Hold(.75f, hold);
                for (var i = 0; i < 2 * fps; i++)
                {
                    run.Step(1);
                    furthest = Mathf.Max(furthest, run.Value);
                    if (run.Value > .002f) back = (i + 1f) / fps;
                }
                TestContext.Out.WriteLine($"fps={fps} selected={selected} dip of {hold}s: furthest={furthest:F3} of {crouch:F3}, standing again after {back:F2}s, largest step {run.MaxStep:F4} (limit {smooth:F4})");
                // Before: the whole pose for every length of dip, standing again after more than a second.
                Assert.That(furthest, Is.LessThan(crouch * (hold + .12f) / .5f + .01f), "The body goes only as far as the dip lasted");
                Assert.That(back, Is.LessThan(.75f), "And is standing again within the time of one blend");
                Assert.That(run.MaxStep, Is.LessThan(smooth), "Without a jump");
            }
        }

        // The defect of 3.0.12 (inquiry 5be3b611): after crouching and standing up at once the avatar stayed
        // crouched. An interrupted blend must never leave any state behind: after a rest every parameter and
        // every control is what it is while standing.
        [TestCase(90, false, 1)] [TestCase(90, true, 2)] [TestCase(45, true, 3)] [TestCase(144, true, 4)] [TestCase(30, false, 5)]
        public void ResponsivePose_AnyHeadMovement_EndsStanding(int fps, bool selected, int seed)
        {
            var controller = ResponsiveController(selected, true, out var groups);
            using var run = new PoseRun(animator, root.transform, controller, groups, fps);
            var parameters = run.Parameters(); var controls = run.Controls();
            var random = new System.Random(seed);
            for (var round = 0; round < 25; round++)
            {
                run.MaxStep = 0;
                for (var time = 0f; time < 3f;)
                {
                    var hold = .02f + (float)random.NextDouble() * .5f;
                    // Squats, short dips and heights right at the three limits (0.8, 0.59, 0.34).
                    var height = round % 5 == 4 ? new[] { .8f, .59f, .34f }[random.Next(3)] + ((float)random.NextDouble() - .5f) * .04f : .25f + (float)random.NextDouble() * .75f;
                    run.Hold(height, hold); time += hold;
                }
                run.Hold(1, 3);
                Assert.That(run.Value, Is.LessThan(.002f), "Round " + round + ": the pose is back");
                var now = run.Parameters();
                var differing = parameters.Keys.Where(n => Mathf.Abs(now[n] - parameters[n]) > .0001f).Select(n => n + ":" + parameters[n] + "->" + now[n]).ToArray();
                Assert.That(differing, Is.Empty, "Round " + round + ": every parameter is back");
                Assert.That(run.Controls(), Is.EqualTo(controls), "Round " + round + ": tracking, locomotion and pose space are back");
            }
        }

        [Test]
        public void ResponsivePose_ChangesOnlyTheBlendedPoseChanges()
        {
            var controller = ResponsiveController(true, true, out _);
            var kinds = new List<string>();
            void Visit(AnimatorStateMachine machine, string layer)
            {
                foreach (var child in machine.states)
                    foreach (var transition in child.state.transitions)
                        kinds.Add(layer + "|" + (transition.duration > 0 ? "blend" : "cut") + "|" + transition.interruptionSource + "|" +
                            (transition.conditions.Any(c => c.parameter.StartsWith("USSPS_Pose")) ? "pose" : "other"));
                foreach (var child in machine.stateMachines) Visit(child.stateMachine, layer);
            }
            foreach (var layer in controller.layers.Where(l => l.name == PosingSystemResponsivePoseSwitch.PoseLayer || l.name == PosingSystemResponsivePoseSwitch.TypeLayer))
                Visit(layer.stateMachine, layer.name);
            Assert.That(kinds.Where(k => k.StartsWith(PosingSystemResponsivePoseSwitch.TypeLayer)), Has.None.Contains("|blend|"), "The layer that only chooses the number does not wait");
            var blended = kinds.Where(k => k.StartsWith(PosingSystemResponsivePoseSwitch.PoseLayer) && k.Contains("|blend|") && k.EndsWith("|pose")).ToArray();
            Assert.That(blended, Is.Not.Empty);
            Assert.That(blended, Has.All.Contains("|Destination|"));
            // Jumping, falling, landing and the desktop cuts keep the timing they had.
            Assert.That(kinds.Where(k => k.EndsWith("|other") && k.Contains("|blend|")), Has.None.Contains("|Destination|"));
        }

        // The comparison builds (without the desktop one-evaluation pass) have no classifier layers after the
        // pose layer. There an interrupted blend can freeze the height comparison, so they keep the lock.
        [Test]
        public void ResponsivePose_ControllerWithoutFinalClassifiers_IsLeftAlone()
        {
            var controller = ResponsiveController(false, false, out _);
            Assert.That(controller.layers.Select(l => l.name), Has.None.StartsWith("USSPS_Final"));
            var blended = new List<AnimatorStateTransition>();
            void Collect(AnimatorStateMachine machine)
            {
                blended.AddRange(machine.states.SelectMany(s => s.state.transitions).Where(t => t.duration > 0 && t.conditions.Any(c => c.parameter.StartsWith("USSPS_Pose"))));
                foreach (var child in machine.stateMachines) Collect(child.stateMachine);
            }
            Collect(controller.layers.Single(l => l.name == PosingSystemResponsivePoseSwitch.PoseLayer).stateMachine);
            Assert.That(blended, Is.Not.Empty);
            Assert.That(blended.Select(t => t.interruptionSource), Has.All.EqualTo(TransitionInterruptionSource.SourceThenDestination));
        }

        // Why they keep it: the same change forced onto such a controller leaves the avatar in a low pose after
        // some head movements. This is the defect of 3.0.12 (inquiry 5be3b611), reproduced.
        [TestCase(90, 1)] [TestCase(30, 3)]
        public void ResponsivePose_WithoutFinalClassifiers_TheSameChangeFreezesThePose(int fps, int seed)
        {
            var controller = ResponsiveController(false, false, out var groups);
            void Force(AnimatorStateMachine machine, bool type)
            {
                foreach (var transition in machine.states.SelectMany(s => s.state.transitions))
                    if (transition.duration > 0)
                    {
                        if (type) transition.duration = 0;
                        else if (transition.conditions.Any(c => c.parameter.StartsWith("USSPS_Pose"))) transition.interruptionSource = TransitionInterruptionSource.Destination;
                    }
                foreach (var child in machine.stateMachines) Force(child.stateMachine, type);
            }
            Force(controller.layers.Single(l => l.name == PosingSystemResponsivePoseSwitch.PoseLayer).stateMachine, false);
            Force(controller.layers.Single(l => l.name == PosingSystemResponsivePoseSwitch.TypeLayer).stateMachine, true);
            using var run = new PoseRun(animator, root.transform, controller, groups, fps);
            var random = new System.Random(seed); var frozen = 0;
            for (var round = 0; round < 25; round++)
            {
                for (var time = 0f; time < 3f;)
                {
                    var hold = .02f + (float)random.NextDouble() * .5f;
                    run.Hold(.25f + (float)random.NextDouble() * .75f, hold); time += hold;
                }
                run.Hold(1, 3);
                if (run.Value > .002f) frozen++;
            }
            TestContext.Out.WriteLine($"fps={fps}: rounds that did not end standing: {frozen} of 25");
            Assert.That(frozen, Is.GreaterThan(0), "If this stops failing, the lock of the comparison builds can be reconsidered");
        }
    }
}
