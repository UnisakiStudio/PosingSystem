using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    [UnityEngine.TestTools.PrebuildSetup(typeof(ControllerCompilerBatchSetup))]
    [UnityEngine.TestTools.PostBuildCleanup(typeof(ControllerCompilerBatchSetup))]
    public class PosingSystemControllerSelectionTests
    {
        internal static AnimatorStateMachine Group(AnimatorController controller) =>
            controller.layers.Single(l => l.name == "USSPS_GroupSwitch").stateMachine;

        // Written independently of transition construction. Preserve the current asymmetry in
        // SitDeep (VirtualLove alone does not leave it); changing that policy is a separate fix.
        private static string Expected(string state, IReadOnlyDictionary<string, float> p)
        {
            var shallow = p["USSPS_IsShallow"]; var deep = p["USSPS_IsDeep"]; var sleep = p["USSPS_IsSleep"];
            var love = p["VirtualLoveType"] != 0; var fix = p["USSPS_TypeFix"] != 0; var locked = p["LocomotionLock"] != 0;
            var dispatch = state switch {
                "Entry" => true,
                "Standing" => shallow > 0 || love || fix,
                "SitShallow" => love || deep > 0 || shallow < 0 || fix,
                "SitDeep" => fix || sleep > 0 || deep < 0,
                "SitSleep" => love || sleep < 0 || fix,
                "VirtualLove" => !love,
                "TypeFix" => love || !fix,
                "LocomotionLock" => !locked,
                _ => throw new ArgumentException(state)
            };
            if (!dispatch) return state;
            if (locked) return "LocomotionLock";
            if (love) return "VirtualLove";
            if (fix) return "TypeFix";
            if (sleep > 0) return "SitSleep";
            if (deep > 0) return "SitDeep";
            if (shallow > 0) return "SitShallow";
            return shallow < 0 && p["TrackingType"] != 0 ? "Standing" : "Entry";
        }

        private static IEnumerable<Dictionary<string, float>> Cases()
        {
            foreach (var s in new[] {-1f, 0f, 1f})
            foreach (var d in new[] {-1f, 0f, 1f})
            foreach (var n in new[] {-1f, 0f, 1f})
            foreach (var locked in new[] {0f, 1f})
            foreach (var love in new[] {0f, 1f})
            foreach (var fix in new[] {0f, 1f})
            foreach (var tracking in new[] {0f, 3f})
                yield return new Dictionary<string, float> {
                    ["USSPS_IsShallow"] = s, ["USSPS_IsDeep"] = d, ["USSPS_IsSleep"] = n,
                    ["LocomotionLock"] = locked, ["VirtualLoveType"] = love, ["USSPS_TypeFix"] = fix, ["TrackingType"] = tracking
                };
        }

        internal static AnimatorState Step(AnimatorState state, IReadOnlyDictionary<string, float> input)
        {
            var transition = state.transitions.FirstOrDefault(t => !t.mute && t.conditions.All(c => {
                var v = input[c.parameter];
                return c.mode switch {
                    AnimatorConditionMode.If => v != 0, AnimatorConditionMode.IfNot => v == 0,
                    AnimatorConditionMode.Greater => v > c.threshold, AnimatorConditionMode.Less => v < c.threshold,
                    AnimatorConditionMode.Equals => v == c.threshold, AnimatorConditionMode.NotEqual => v != c.threshold,
                    _ => throw new ArgumentException(c.mode.ToString())
                };
            }));
            return transition == null ? state : transition.destinationState;
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SEL01_And_SEL02_GroupMatchesIndependentSpecificationFor3456Cases(bool optimized)
        {
            using var result = PosingSystemControllerCompiler.Create(optimized);
            var checkedCases = 0;
            foreach (var state in Group(result.Controller).states.Select(s => s.state))
            foreach (var p in Cases())
            {
                var actual = Step(state, p);
                if (!optimized && actual.name == "Entry") actual = Step(actual, p);
                Assert.That(actual.name, Is.EqualTo(Expected(state.name, p)), state.name + " " + string.Join(",", p));
                checkedCases++;
            }
            Assert.That(checkedCases, Is.EqualTo(3456));
        }

        [TestCase(30)] [TestCase(60)] [TestCase(90)] [TestCase(144)]
        public void SEL02_RealUnityEvaluationShortensStandingToCrouch(int fps)
        {
            using var original = new GroupRuntime(false);
            using var compiled = new GroupRuntime(true);
            var oldFrames = original.Reach(.6f, "SitShallow", fps);
            var newFrames = compiled.Reach(.6f, "SitShallow", fps);
            TestContext.WriteLine($"Group standing->crouch {fps}fps: reference={oldFrames}, compiled={newFrames} frames");
            Assert.That(newFrames, Is.LessThan(oldFrames));
            Assert.That(oldFrames - newFrames, Is.LessThanOrEqualTo(2));
        }

        [TestCase(30)] [TestCase(60)] [TestCase(90)] [TestCase(144)]
        public void SEL03_RealUnityRapidReversalAlwaysRecovers(int fps)
        {
            using var rig = new GroupRuntime(true);
            foreach (var interval in new[] {1, 2, 3})
            {
                for (var cycle = 0; cycle < 30; cycle++)
                {
                    rig.Tick(.6f, interval, fps);
                    rig.Tick(1f, interval, fps);
                }
                rig.Reach(.6f, "SitShallow", fps);
                rig.Reach(.35f, "SitDeep", fps);
                rig.Reach(.1f, "SitSleep", fps);
                rig.Reach(1f, "Standing", fps);
            }
            var random = new System.Random(21827);
            for (var i = 0; i < 500; i++) rig.Tick((float)random.NextDouble(), 1, fps);
            rig.Reach(.6f, "SitShallow", fps);
            rig.Reach(1f, "Standing", fps);
        }

        [Test]
        public void PERF02_ChangedLayersStayWithinEvaluationBudget()
        {
            using var original = new GroupRuntime(false);
            using var compiled = new GroupRuntime(true);
            var before = new List<double>(); var after = new List<double>();
            original.Tick(.6f, 200, 90); compiled.Tick(.6f, 200, 90);
            for (var repeat = 0; repeat < 9; repeat++)
            {
                if (repeat % 2 == 0) { before.Add(Measure(original)); after.Add(Measure(compiled)); }
                else { after.Add(Measure(compiled)); before.Add(Measure(original)); }
            }
            var medianBefore = before.OrderBy(x => x).ElementAt(4);
            var medianAfter = after.OrderBy(x => x).ElementAt(4);
            TestContext.WriteLine("Evaluation milliseconds/frame reference=" + string.Join(",", before.Select(x => x.ToString("F6"))) +
                "; compiled=" + string.Join(",", after.Select(x => x.ToString("F6"))));
            TestContext.WriteLine($"Evaluation median reference={medianBefore:F6}ms compiled={medianAfter:F6}ms");
            // Budget for this four-layer subsystem: <=35% plus a 5us noise/absolute floor.
            // This is an Editor CPU measurement, not a claim about headset or VRChat frame time.
            Assert.That(medianAfter, Is.LessThanOrEqualTo(medianBefore * 1.35 + .005));

            double Measure(GroupRuntime rig)
            {
                var timer = System.Diagnostics.Stopwatch.StartNew();
                for (var i = 0; i < 3000; i++) rig.Tick((i / 5) % 2 == 0 ? 1f : .6f, 1, 90);
                timer.Stop();
                return timer.Elapsed.TotalMilliseconds / 3000;
            }
        }

        private sealed class GroupRuntime : IDisposable
        {
            readonly PosingSystemControllerCompiler.Result result;
            readonly GameObject host;
            readonly PlayableGraph graph;
            readonly AnimatorControllerPlayable playable;
            internal GroupRuntime(bool optimize)
            {
                result = PosingSystemControllerCompiler.Create(optimize);
                // Isolate only threshold computation and group dispatch. This deliberately makes
                // no claim about complete avatar/VRChat latency or SDK behaviour scheduling.
                result.Controller.layers = result.Controller.layers.Take(4).ToArray();
                host = new GameObject("Compiler group evaluation");
                var animator = host.AddComponent<Animator>();
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                graph = PlayableGraph.Create("Compiler group evaluation");
                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                playable = AnimatorControllerPlayable.Create(graph, result.Controller);
                AnimationPlayableOutput.Create(graph, "Output", animator).SetSourcePlayable(playable);
                playable.SetInteger("TrackingType", 3);
                playable.SetBool("IsLocal", true);
                graph.Play();
                Tick(1f, 20, 60);
                Assert.That(playable.GetCurrentAnimatorStateInfo(3).shortNameHash, Is.EqualTo(Animator.StringToHash("Standing")));
            }
            internal void Tick(float upright, int frames, int fps)
            {
                playable.SetFloat("Upright", upright);
                for (var i = 0; i < frames; i++) graph.Evaluate(1f / fps);
            }
            internal int Reach(float upright, string expected, int fps)
            {
                for (var i = 1; i <= 20; i++)
                {
                    Tick(upright, 1, fps);
                    if (playable.GetCurrentAnimatorStateInfo(3).shortNameHash == Animator.StringToHash(expected) &&
                        !playable.IsInTransition(3)) return i;
                }
                Assert.Fail("Group did not recover to " + expected + ", Upright=" + upright);
                return -1;
            }
            public void Dispose()
            {
                if (graph.IsValid()) graph.Destroy();
                Object.DestroyImmediate(host); result.Dispose();
            }
        }
    }
}
