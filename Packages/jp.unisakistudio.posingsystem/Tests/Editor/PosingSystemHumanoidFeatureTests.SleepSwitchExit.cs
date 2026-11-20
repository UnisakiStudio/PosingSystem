using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    // While lying, a jump turns the avatar over. Up to 3.2.0-alpha.4 (and in 3.1.4) a user who left the lying
    // poses before landing stayed in the lying pose while standing, until lying down again. Found on 2026-11-20
    // by playing random head heights, jumps, AFK, emotes, stations and menu changes on the built avatar and
    // comparing with standing after everything was put back.
    public partial class PosingSystemHumanoidFeatureTests
    {
        private AnimatorController SleepSwitchController(bool withExit, out PosingSystem ps)
        {
            ps = CompilerSettings();
            var controller = CompileSettings(ps, true);
            var machine = controller.layers.Single(l => l.name == PosingSystemSleepSwitchExit.Layer).stateMachine;
            var waiting = machine.states.Select(s => s.state).Where(s => PosingSystemSleepSwitchExit.Waiting.Contains(s.name)).ToArray();
            Assert.That(waiting.Select(s => s.name), Is.EquivalentTo(PosingSystemSleepSwitchExit.Waiting), "Fixture: the four waiting states exist");
            // The controller as it was before the exit existed, whether or not this way of compiling already adds it.
            foreach (var state in waiting)
                state.transitions = state.transitions.Where(t => t.destinationState == null || t.destinationState.name != PosingSystemSleepSwitchExit.Idle).ToArray();
            if (withExit)
            {
                Assert.That(PosingSystemSleepSwitchExit.Apply(controller), Is.EqualTo(4), "Each waiting state gets the exit");
                Assert.That(PosingSystemSleepSwitchExit.Apply(controller), Is.Zero, "Applying it again changes nothing");
            }
            return controller;
        }

        private sealed class SleepRun : System.IDisposable
        {
            private readonly ControllerCompilerRuntimeProbe rig;
            private readonly int fps;
            internal SleepRun(Animator animator, AnimatorController controller, PosingSystem ps, int fps)
            {
                this.fps = fps;
                rig = new ControllerCompilerRuntimeProbe(animator, controller, true, true);
                foreach (var group in ps.defines) rig.Set(group.paramName, CompilerBinding(controller, group.paramName).select);
                Hold(3);
            }
            internal int Group => rig.Playable.GetInteger("LocomotionTypeGroup");
            internal int Type => rig.Playable.GetInteger("LocomotionType");
            internal int Pose => rig.Playable.GetInteger("USSPS_Pose");
            internal void Hold(float seconds) => rig.Tick(Mathf.Max(1, Mathf.CeilToInt(seconds * fps)), fps);
            internal void Height(float upright) => rig.Set("Upright", upright);
            internal void Air(bool airborne) { rig.Set("Grounded", airborne ? 0 : 1); rig.Set("VelocityY", airborne ? 2.5f : 0); }
            public void Dispose() => rig.Dispose();
        }

        [TestCase(90, .2f, .4f)]   // stands up while the layer waits for the landing
        [TestCase(90, .02f, .5f)]  // stands up while it is still deciding
        [TestCase(30, .2f, .4f)]
        [TestCase(144, .05f, .1f)]
        [TestCase(60, .5f, .02f)]  // stands up just before landing
        public void SleepSwitchExit_StandingUpDuringTheTurningJump_EndsStanding(int fps, float lyingInTheAir, float standingInTheAir)
        {
            var controller = SleepSwitchController(true, out var ps);
            using var run = new SleepRun(animator, controller, ps, fps);
            var standingType = run.Type; var standingPose = run.Pose;
            Assert.That(run.Group, Is.Zero, "Fixture: standing");
            run.Height(.2f); run.Hold(2);
            Assert.That(run.Group, Is.EqualTo(PosingSystemSleepSwitchExit.Lying), "Fixture: lying");
            Assert.That(run.Type, Is.EqualTo(32).Or.EqualTo(33), "Fixture: a chosen lying pose");
            run.Air(true); run.Hold(lyingInTheAir);
            run.Height(1); run.Hold(standingInTheAir);
            run.Air(false); run.Hold(4);
            Assert.That((run.Group, run.Type, run.Pose), Is.EqualTo((0, standingType, standingPose)), "The user stands, so does the avatar");
        }

        // The defect itself, on the controller without the exit.
        [Test]
        public void SleepSwitchExit_WithoutTheExit_TheAvatarStaysLying()
        {
            var controller = SleepSwitchController(false, out var ps);
            using var run = new SleepRun(animator, controller, ps, 90);
            var standingType = run.Type;
            run.Height(.2f); run.Hold(2);
            run.Air(true); run.Hold(.2f);
            run.Height(1); run.Hold(.4f);
            run.Air(false); run.Hold(8);
            TestContext.Out.WriteLine($"without the exit: group={run.Group} type={run.Type} (standing type {standingType})");
            Assert.That(run.Group, Is.Zero, "The pose group follows the user");
            Assert.That(run.Type, Is.EqualTo(32).Or.EqualTo(33), "But the pose type is the lying one: the avatar lies while the user stands");
        }

        [TestCase(90)] [TestCase(30)]
        public void SleepSwitchExit_JumpWhileLying_StillTurnsOver(int fps)
        {
            var controller = SleepSwitchController(true, out var ps);
            using var run = new SleepRun(animator, controller, ps, fps);
            run.Height(.2f); run.Hold(2);
            var first = run.Type;
            Assert.That(first, Is.EqualTo(32).Or.EqualTo(33));
            run.Air(true); run.Hold(.4f); run.Air(false); run.Hold(2);
            var second = run.Type;
            Assert.That(second, Is.EqualTo(first == 32 ? 33 : 32), "A jump turns the avatar over");
            run.Air(true); run.Hold(.4f); run.Air(false); run.Hold(2);
            Assert.That(run.Type, Is.EqualTo(first), "And the next one turns it back");
            Assert.That(run.Group, Is.EqualTo(PosingSystemSleepSwitchExit.Lying));
        }

        // Random head heights and jumps, then standing on the ground: the avatar stands, whatever happened.
        [TestCase(90, 1)] [TestCase(45, 2)] [TestCase(120, 3)]
        public void SleepSwitchExit_AnyHeightsAndJumps_EndStanding(int fps, int seed)
        {
            var controller = SleepSwitchController(true, out var ps);
            using var run = new SleepRun(animator, controller, ps, fps);
            var standing = (run.Group, run.Type, run.Pose);
            var random = new System.Random(seed);
            var failed = new List<string>();
            for (var round = 0; round < 30; round++)
            {
                for (var time = 0f; time < 4f;)
                {
                    var hold = .03f + (float)random.NextDouble() * .6f;
                    if (random.Next(3) == 0) run.Air(random.Next(2) == 0);
                    else run.Height(random.Next(3) == 0 ? .15f + (float)random.NextDouble() * .15f : .15f + (float)random.NextDouble() * .85f);
                    run.Hold(hold); time += hold;
                }
                run.Height(1); run.Air(false); run.Hold(4);
                if (!(run.Group, run.Type, run.Pose).Equals(standing)) failed.Add(round + ":" + (run.Group, run.Type, run.Pose));
            }
            Assert.That(failed, Is.Empty, "Rounds that did not end standing " + standing);
        }

        [Test]
        public void SleepSwitchExit_ChangesNothingElse()
        {
            var before = SleepSwitchController(false, out _);
            string Describe(AnimatorController controller) => string.Join("\n", controller.layers.SelectMany(l => l.stateMachine.states.Select(s => s.state)
                .SelectMany(s => s.transitions.Select(t => l.name + "|" + s.name + "->" + (t.destinationState != null ? t.destinationState.name : t.isExit ? "EXIT" : "machine") + "|" +
                    string.Join("&", t.conditions.Select(c => c.parameter + c.mode + c.threshold))))));
            var was = Describe(before).Split('\n');
            Assert.That(PosingSystemSleepSwitchExit.Apply(before), Is.EqualTo(4));
            var now = Describe(before).Split('\n');
            var added = now.Except(was).ToArray();
            Assert.That(now.Length, Is.EqualTo(was.Length + 4));
            Assert.That(added, Is.EquivalentTo(PosingSystemSleepSwitchExit.Waiting.Select(name =>
                PosingSystemSleepSwitchExit.Layer + "|" + name + "->" + PosingSystemSleepSwitchExit.Idle + "|" + PosingSystemSleepSwitchExit.Group + "NotEqual" + PosingSystemSleepSwitchExit.Lying)));
            var machine = before.layers.Single(l => l.name == PosingSystemSleepSwitchExit.Layer).stateMachine;
            foreach (var state in machine.states.Select(s => s.state).Where(s => PosingSystemSleepSwitchExit.Waiting.Contains(s.name)))
                Assert.That(state.transitions[0].destinationState.name, Is.EqualTo(PosingSystemSleepSwitchExit.Idle), state.name + ": the exit is checked first");
        }
    }
}
