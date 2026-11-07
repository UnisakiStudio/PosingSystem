using System;
using System.Linq;
using System.Collections.Generic;
using nadena.dev.ndmf;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    [UnityEngine.TestTools.PrebuildSetup(typeof(ControllerCompilerBatchSetup))]
    [UnityEngine.TestTools.PostBuildCleanup(typeof(ControllerCompilerBatchSetup))]
    public partial class PosingSystemHumanoidFeatureTests
    {
        static IEnumerable<AnimatorState> CompilerStates(AnimatorStateMachine machine)
        {
            foreach (var child in machine.states) yield return child.state;
            foreach (var child in machine.stateMachines)
                foreach (var state in CompilerStates(child.stateMachine)) yield return state;
        }
        static (int select, int sync) CompilerBinding(AnimatorController controller, string parameter)
        {
            var entry = controller.layers.Single(l => l.name == "USSPS_LocomotionType").stateMachine.entryTransitions
                .Single(t => t.conditions.Any(c => c.parameter == parameter && c.mode == AnimatorConditionMode.Equals));
            var selection = (int)entry.conditions.Single(c => c.parameter == parameter).threshold;
            var sync = (int)entry.destinationState.behaviours.OfType<VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver>()
                .SelectMany(d => d.parameters).Single(p => p.name == "USSPS_Pose").value;
            Assert.That(selection, Is.GreaterThan(0));
            Assert.That(sync, Is.GreaterThan(2));
            return (selection, sync);
        }
        PosingSystem CompilerSettings(int poseCount = 5)
        {
            root.AddComponent<VRCAvatarDescriptor>();
            var settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("ecde0b1d67f9e854992715d64f778d8c")), root.transform);
            var ps = settings.GetComponent<PosingSystem>();
            ps.autoImportAvatarAnimations = false; ps.isIconDisabled = true; ps.enablePoseIk = false;
            ps.features = new PosingSystemFeatureSettings { mirror=false, footHeight=false, breathing=false, fingerGestureControl=false };
            ps.defines.Clear(); ps.overrideDefines.Clear();
            var categories = new[] {("SitStand", 1), ("SitShallow", 12), ("SitDeep", 22), ("SitSleepUp", 32), ("SitSleepDown", 33)};
            var clip = Clip(); clip.name = "CompilerPose";
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), "Spine Front-Back"), AnimationCurve.Constant(0, 1, .2f));
            for (var i = 0; i < poseCount; i++)
            {
                var category = categories[i % categories.Length];
                var layer = ps.defines.FirstOrDefault(d => d.paramName == category.Item1);
                if (layer == null) { layer = new PosingSystem.LayerDefine("", category.Item1, category.Item1, category.Item2, category.Item1, new()); ps.defines.Add(layer); }
                layer.animations.Add(new PosingSystem.AnimationDefine { enabled=true, displayName="CompilerPose" + i, animationClip=clip });
            }
            return ps;
        }

        AnimatorController CompileSettings(PosingSystem ps, bool optimize)
        {
            PosingSystemConverter.GetCommonMergeAnimator(ps).animator = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(PosingSystemControllerGeneration.TemplateGuid));
            ps.data = "";
            var context = new BuildContext(root, null, false);
            try
            {
                using (new PosingSystemControllerGeneration.Scope(root, optimize))
                    PosingSystemConverter.ConvertToModularAvatarComponents(ps, context.AssetSaver);
                var controller = (AnimatorController)PosingSystemConverter.GetCommonMergeAnimator(ps).animator;
                foreach (var obj in PosingSystemControllerGraph.Collect(new[] { controller }, o => PosingSystemControllerGraph.IsGraphObject(o) && !EditorUtility.IsPersistent(o)))
                    if (!assets.Contains(obj)) assets.Add(obj);
                return controller;
            }
            finally { context.DeactivateAllExtensionContexts(); }
        }

        [TestCase(false, false, false)]
        [TestCase(false, true, false)]
        [TestCase(false, false, true)]
        [TestCase(false, true, true)]
        [TestCase(true, false, true)]
        [TestCase(true, true, true)]
        public void SleepJumpSwitch_OnlyJumpsToASelectedPose(bool disabled, bool startProne, bool selectBoth)
        {
            var ps = CompilerSettings();
            ps.disableSleepJumpSwitch = disabled;
            var controller = CompileSettings(ps, true);
            var sleep = controller.layers.Single(l => l.name == "USSPS_SitSleepSwitch");
            // Drive the built selector independently of the head-height and face-direction sensors.
            controller.layers = new[] { sleep };
            using var rig = new ControllerCompilerRuntimeProbe(animator, controller, true, false);
            rig.Set("LocomotionTypeGroup", 3);
            rig.Set("KawaiiPosing_IsDown", startProne ? 1 : 0);
            rig.Set("SitSleepUp", !startProne || selectBoth ? 1 : 0);
            rig.Set("SitSleepDown", startProne || selectBoth ? 1 : 0);
            rig.Tick(10, 60);
            var prone = startProne;
            int Expected() => Animator.StringToHash(prone ? "SitSleepDown" : "SitSleepUp");
            int Current() => rig.Playable.GetCurrentAnimatorStateInfo(0).shortNameHash;
            Assert.That(Current(), Is.EqualTo(Expected()), "Selected starting pose");
            for (var jump = 0; jump < 2; jump++)
            {
                rig.Set("Grounded", 0);
                rig.Set("VelocityY", 3);
                for (var frame = 0; frame < 30; frame++)
                {
                    rig.Tick(1, 60);
                    if (disabled || !selectBoth)
                    {
                        Assert.That(Current(), Is.EqualTo(Expected()), "Airborne frame " + frame);
                        Assert.That(rig.Playable.IsInTransition(0), Is.False);
                    }
                }
                rig.Set("Grounded", 1);
                rig.Set("VelocityY", 0);
                rig.Tick(10, 60);
                if (!disabled && selectBoth) prone = !prone;
                Assert.That(Current(), Is.EqualTo(Expected()), "Landed jump " + jump);
            }
        }

        [Test]
        public void SleepJumpSwitch_PrebuildUpdatesOldGuardAndRestoresSwitching()
        {
            var ps = CompilerSettings();
            var controller = CompileSettings(ps, true);
            var sleep = controller.layers.Single(l => l.name == "USSPS_SitSleepSwitch").stateMachine;
            var jump = sleep.states.Single(s => s.state.name == "SitSleepDown").state.transitions
                .Single(t => t.destinationState.name == "SitSleepUp_Judge");
            var old = jump.conditions;
            var guard = Array.FindIndex(old, c => c.parameter == "SitSleepUp");
            old[guard].parameter = "SitSleepDown";
            jump.conditions = old;
            var before = PosingSystemConverter.GetDefineSerializeJson(ps);
            ps.disableSleepJumpSwitch = true;
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps), Is.Not.EqualTo(before));
            PosingSystemConverter.ConvertOptionalFeatures(ps);
            Assert.That(jump.mute, Is.True);
            Assert.That(jump.conditions.Count(c => c.parameter == "SitSleepUp"), Is.EqualTo(1));
            ps.disableSleepJumpSwitch = false;
            PosingSystemConverter.ConvertOptionalFeatures(ps);
            Assert.That(jump.mute, Is.False);
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps), Is.EqualTo(before));
        }

        [TestCase(1)] [TestCase(16)] [TestCase(64)] [TestCase(128)]
        public void Compiler_PERF01_ControllerGrowthIsLinearInPoseCount(int count)
        {
            var ps = CompilerSettings(count);
            var original = CompileSettings(ps, false);
            var compiled = CompileSettings(ps, true);
            var a = PosingSystemControllerGraph.Collect(new[] {original}, o => !EditorUtility.IsPersistent(o));
            var b = PosingSystemControllerGraph.Collect(new[] {compiled}, o => !EditorUtility.IsPersistent(o));
            var before = a.OfType<AnimatorTransitionBase>().Count(); var after = b.OfType<AnimatorTransitionBase>().Count();
            var conditions = b.OfType<AnimatorTransitionBase>().Sum(t => t.conditions.Length);
            Assert.That(b.OfType<AnimatorState>().Count(), Is.EqualTo(a.OfType<AnimatorState>().Count()));
            Assert.That(after - before, Is.InRange(0, 250 + 24 * (count + 3)), "At most two fallback edges per nine sync fields plus three direct exits and two entries per pose");
            Assert.That(after, Is.LessThan(650 + count * 50));
            Assert.That(conditions, Is.LessThan(2800 + count * 160));
            Assert.That(compiled.parameters.Select(p => p.name), Is.EqualTo(original.parameters.Select(p => p.name)));
            TestContext.WriteLine($"poses={count}: states={b.OfType<AnimatorState>().Count()}, transitions={before}->{after}, conditions={conditions}");
        }

        [TestCase(false, 30)] [TestCase(false, 60)] [TestCase(false, 90)] [TestCase(false, 144)]
        [TestCase(true, 30)] [TestCase(true, 90)] [TestCase(true, 90, 6)]
        public void Compiler_MEM_TRK_RealGraphMatchesAfterLocksSleepAndRapidInputs(bool vr, int fps, int trackingType = 3)
        {
            var ps = CompilerSettings();
            var original = CompileSettings(ps, false); var compiled = CompileSettings(ps, true);
            var baseline = Run(original); var actual = Run(compiled);
            CollectionAssert.AreEqual(baseline, actual);

            List<string> Run(AnimatorController controller)
            {
                using var rig = new ControllerCompilerRuntimeProbe(animator, controller, true, vr);
                rig.Set("TrackingType", trackingType);
                foreach (var layer in ps.defines) rig.Set(layer.paramName, CompilerBinding(controller, layer.paramName).select);
                var observations = new List<string>();
                Hold("standing", 1);
                Hold("crouch", .6f);
                Assert.That(rig.Playable.GetInteger("LocomotionTypeGroup"), Is.EqualTo(1));
                Assert.That(rig.Playable.GetInteger("LocomotionType"), Is.EqualTo(12));
                Assert.That(rig.Playable.GetInteger("USSPS_Pose"), Is.EqualTo(CompilerBinding(controller, "SitShallow").sync));
                rig.Set("LocomotionLock", 1); Hold("locked-standing-input", 1);
                rig.Set("LocomotionLock", 0); Hold("unlock", 1);
                rig.Set("USSPS_TypeFix", 1); rig.Set("USSPS_LastMenu", 12); Hold("fixed-shallow", 1);
                rig.Set("USSPS_LastMenu", 22); Hold("fixed-deep", 1);
                rig.Set("USSPS_LastMenu", 33); Hold("fixed-prone", 1);
                rig.Set("USSPS_TypeFix", 0); Hold("sleep-up", .1f);
                rig.Set("KawaiiPosing_IsDown", 1); Hold("sleep-down", .1f);
                rig.Set("Grounded", 0); rig.Set("VelocityY", 3); Hold("airborne", .1f);
                rig.Set("Grounded", 1); rig.Set("VelocityY", 0); Hold("landed", .1f);
                foreach (var interval in new[] {1, 2, 3})
                {
                    for (var cycle = 0; cycle < 20; cycle++) { rig.Set("Upright", .6f); rig.Tick(interval, fps); rig.Set("Upright", 1); rig.Tick(interval, fps); }
                    Hold("recover-c-" + interval, .6f); Hold("recover-z-" + interval, .35f); Hold("recover-stand-" + interval, 1);
                }
                Assert.That(rig.AppliedDrivers, Is.GreaterThan(20), "Actual SDK callbacks must execute in this probe");
                return observations;
                void Hold(string label, float upright) { rig.Set("Upright", upright); rig.Tick(fps * 2, fps); observations.Add(label + ":" + rig.Snapshot()); }
            }
        }

        [TestCase(false)] [TestCase(true)]
        public void Compiler_NET01_RemotePlaybackUsesReceivedPoseWhileLocalInputsChange(bool vr)
        {
            var ps = CompilerSettings();
            var controller = CompileSettings(ps, true);
            var selected = CompilerBinding(controller, "SitShallow");
            var playback = controller.layers.Single(l => l.name == "USSPS_Locomotion").stateMachine;
            var expected = playback.entryTransitions.Where(t => t.conditions.Any(c => c.parameter == "USSPS_Pose" &&
                c.threshold == selected.sync && c.mode == AnimatorConditionMode.Equals) && t.conditions.Any(c =>
                c.parameter == "VRMode" && c.mode == (vr ? AnimatorConditionMode.NotEqual : AnimatorConditionMode.Equals))).Select(t => t.destinationState).Distinct().Single();
            using var rig = new ControllerCompilerRuntimeProbe(animator, controller, false, vr);
            rig.Set("USSPS_Pose", selected.sync);
            foreach (var upright in new[] {1f, .6f, .35f, .1f, 1f})
            {
                rig.Set("Upright", upright); rig.Set("SitShallow", 0); rig.Set("USSPS_TypeFix", 1); rig.Set("LocomotionLock", 1);
                rig.Set("LocomotionType",22); rig.Set("SitDeep",CompilerBinding(controller,"SitDeep").select);
                rig.Tick(180, 90);
                Assert.That(rig.Playable.GetInteger("USSPS_Pose"), Is.EqualTo(selected.sync));
                Assert.That(rig.Playable.GetCurrentAnimatorStateInfo(rig.Layer("USSPS_Locomotion")).shortNameHash, Is.EqualTo(Animator.StringToHash(expected.name)));
            }
        }

        [TestCase(30)] [TestCase(60)] [TestCase(90)] [TestCase(144)]
        public void Compiler_SEL02_SelectedPosePlaybackStartsEarlier(int fps)
        {
            var ps = CompilerSettings();
            var original = CompileSettings(ps, false); var compiled = CompileSettings(ps, true);
            var baseline = Measure(original); var optimized = Measure(compiled);
            TestContext.WriteLine($"Full graph selected crouch at {fps}fps: reference={baseline}, compiled={optimized} frames");
            Assert.That(optimized, Is.LessThan(baseline));

            int Measure(AnimatorController controller)
            {
                var binding = CompilerBinding(controller, "SitShallow");
                var playback = controller.layers.Single(l => l.name == "USSPS_Locomotion").stateMachine;
                var target = playback.entryTransitions.Where(t => t.conditions.Any(c => c.parameter == "USSPS_Pose" && c.threshold == binding.sync) &&
                    t.conditions.Any(c => c.parameter == "VRMode" && c.mode == AnimatorConditionMode.Equals)).Select(t => t.destinationState).Distinct().Single();
                using var rig = new ControllerCompilerRuntimeProbe(animator, controller, true, false);
                rig.Set("SitShallow", binding.select); rig.Tick(fps * 2, fps);
                rig.Set("Upright", .6f);
                for (var i = 1; i <= fps; i++)
                {
                    rig.Tick(1, fps);
                    if (rig.Playable.GetCurrentAnimatorStateInfo(rig.Layer("USSPS_Locomotion")).shortNameHash == Animator.StringToHash(target.name)) return i;
                }
                Assert.Fail("Selected crouch never became the current playback state");
                return -1;
            }
        }

        [TestCase(30, false)] [TestCase(60, false)] [TestCase(90, false)] [TestCase(144, false)]
        [TestCase(30, true)] [TestCase(60, true)] [TestCase(90, true)] [TestCase(144, true)]
        public void Compiler_SEL05_OrdinaryPosturesSettleWithinThreeEvaluations(int fps, bool selected)
        {
            var ps = CompilerSettings();
            var controller = CompileSettings(ps, true);
            var playback = controller.layers.Single(l => l.name == "USSPS_Locomotion").stateMachine;
            using var rig = new ControllerCompilerRuntimeProbe(animator, controller, true, false);
            foreach (var parameter in new[] { "SitStand", "SitShallow", "SitDeep" })
                rig.Set(parameter, selected ? CompilerBinding(controller, parameter).select : 0);
            rig.Tick(fps * 2, fps);
            foreach (var step in new[] { (.6f, "SitShallow", 1), (.35f, "SitDeep", 2), (.6f, "SitShallow", 1), (1f, "SitStand", 0) })
            {
                var sync = selected ? CompilerBinding(controller, step.Item2).sync : step.Item3;
                var target = playback.entryTransitions.Where(t => t.conditions.Any(c => c.parameter == "USSPS_Pose" && c.threshold == sync && c.mode == AnimatorConditionMode.Equals) &&
                    t.conditions.Any(c => c.parameter == "VRMode" && c.mode == AnimatorConditionMode.Equals && c.threshold == 0)).Select(t => t.destinationState).Distinct().Single();
                var targetHash = Animator.StringToHash(target.name);
                var layer = rig.Layer("USSPS_Locomotion");
                var sourceHash = rig.Playable.GetCurrentAnimatorStateInfo(layer).shortNameHash;
                rig.Set("Upright", step.Item1);
                var reached = -1; var trace = new List<string>();
                for (var frame = 1; frame <= 15; frame++)
                {
                    rig.Tick(1, fps);
                    var current = rig.Playable.GetCurrentAnimatorStateInfo(layer).shortNameHash;
                    trace.Add(frame + ":" + rig.Snapshot());
                    Assert.That(current == sourceHash || current == targetHash, Is.True,
                        "Unexpected intermediate posture: " + string.Join("\n", trace));
                    if (current == targetHash && !rig.Playable.IsInTransition(layer)) { reached = frame; break; }
                }
                TestContext.WriteLine($"{fps}fps selected={selected} target={step.Item2} frames={reached}\n" + string.Join("\n", trace));
                Assert.That(reached, Is.InRange(1, 3), "A normal posture must not wait for the old Type/Pose driver chain");
                rig.Tick(fps, fps);
                Assert.That(rig.Playable.GetInteger("USSPS_Pose"), Is.EqualTo(sync));
            }
        }

        [TestCase(false)] [TestCase(true)]
        public void Compiler_BUILD02_FreshPersistedNdmfWithBreathing(bool optimized)
        {
            persistedCompilerOptimization = optimized;
            try
            {
                NdmfBuild_PersistedPackageAssets_SupportFreshCachedAndRegeneratedControllers(false, false);
                Assert.That(persistedCompilerCreated, Is.EqualTo(1));
            }
            finally { persistedCompilerOptimization = null; }
        }

        [Test]
        public void Compiler_GEN05_DesktopOptimizationRestoresExactGraphAndDoesNotAccumulate()
        {
            var ps = CompilerSettings(16);
            var controller = CompileSettings(ps, false);
            var graph = PosingSystemControllerGraph.Collect(new[] { controller }, o => !EditorUtility.IsPersistent(o));
            var before = graph.ToDictionary(o => o, EditorJsonUtility.ToJson);
            var entries = controller.layers.Single(l => l.name == "USSPS_Locomotion").stateMachine.entryTransitions;
            var counts = new List<int>();
            for (var i = 0; i < 3; i++)
            {
                PosingSystemDesktopSelection.Apply(controller, o => assets.Add(o));
                counts.Add(PosingSystemControllerGraph.Collect(new[] { controller }, o => !EditorUtility.IsPersistent(o)).Count);
                Assert.That(PosingSystemDesktopSelection.Restore(controller), Is.True);
                foreach (var pair in before) Assert.That(EditorJsonUtility.ToJson(pair.Key), Is.EqualTo(pair.Value), pair.Key.name);
                CollectionAssert.AreEqual(entries, controller.layers.Single(l => l.name == "USSPS_Locomotion").stateMachine.entryTransitions);
                Assert.That(PosingSystemDesktopSelection.Restore(controller), Is.False);
            }
            Assert.That(counts.Distinct().Count(), Is.EqualTo(1));
        }

        [TestCase(-1)] [TestCase(999)]
        public void Compiler_SEL06_UnknownSelectionRecoversToReferenceDefault(int invalid)
        {
            var ps = CompilerSettings();
            var reference = CompileSettings(ps, false); var optimized = CompileSettings(ps, true);
            var expected = Observe(reference); var actual = Observe(optimized);
            CollectionAssert.AreEqual(expected, actual);
            List<string> Observe(AnimatorController c)
            {
                using var rig = new ControllerCompilerRuntimeProbe(animator, c, true, false);
                var result = new List<string>();
                foreach (var step in new[] { (1f,"SitStand"), (.6f,"SitShallow"), (.35f,"SitDeep") })
                foreach (var selection in new[] { CompilerBinding(c,step.Item2).select, invalid, 0 })
                {
                    rig.Set(step.Item2, selection); rig.Set("Upright",step.Item1); rig.Tick(180,90);
                    var settled = rig.Snapshot(); rig.Tick(7,90);
                    Assert.That(rig.Snapshot(), Is.EqualTo(settled), "Must not cycle after settling: " + step.Item2 + "=" + selection);
                    result.Add(step.Item2 + "=" + selection + ":" + settled);
                }
                return result;
            }
        }

        [Test]
        public void Compiler_SEL07_OrdinaryEntryTruthTableHasExactlyOneCorrectDestination()
        {
            var ps=CompilerSettings(16);
            var reference=CompileSettings(ps,false); var optimized=CompileSettings(ps,true);
            var names=new[]{"SitStand","SitShallow","SitDeep"};
            var definitions=names.Select(n=>ps.defines.Single(d=>d.paramName==n)).ToArray();
            var choices=definitions.Select(d=>new[]{0,-1,999,d.animations.First().typeParameterValue,d.animations.Last().typeParameterValue}.Distinct().ToArray()).ToArray();
            var expectedEntries=reference.layers.Single(l=>l.name=="USSPS_Locomotion").stateMachine.entryTransitions;
            var entries=optimized.layers.Single(l=>l.name=="USSPS_Locomotion").stateMachine.entryTransitions;
            var checkedInputs=0;
            foreach(var a in choices[0]) foreach(var b in choices[1]) foreach(var c in choices[2])
            for(var group=0;group<3;group++)
            {
                var selected=new[]{a,b,c};
                var pose=definitions[group].animations.FirstOrDefault(p=>p.typeParameterValue==selected[group]);
                var sync=selected[group]==0 ? group : pose?.syncdParameterValue ?? 0;
                var inputs=optimized.parameters.ToDictionary(p=>p.name,p=>p.type==AnimatorControllerParameterType.Bool?(p.defaultBool?1f:0f):p.type==AnimatorControllerParameterType.Int?p.defaultInt:p.defaultFloat);
                inputs["IsLocal"]=1;inputs["VRMode"]=0;inputs["TrackingType"]=3;inputs["LocomotionTypeGroup"]=group;
                inputs["SitStand"]=a;inputs["SitShallow"]=b;inputs["SitDeep"]=c;
                foreach(var value in PosingSystemConverter.SplitSyncParameters(sync,19))inputs[value.name]=value.value;
                var expected=expectedEntries.First(t=>Matches(t.conditions,inputs)).destinationState.name;
                // Poison the delayed intermediate values: the direct path must use current selections.
                inputs["USSPS_Pose"]=223;inputs["USSPS_Pose0"]=1;inputs["LocomotionType"]=41;
                var actual=entries.Where(t=>Matches(t.conditions,inputs)).ToArray();
                Assert.That(actual.Count(t=>t.name!=PosingSystemConverter.AvatarSelectionFallback),Is.EqualTo(1),$"group={group}, selections={a}/{b}/{c}");
                Assert.That(actual[0].name,Is.Not.EqualTo(PosingSystemConverter.AvatarSelectionFallback));
                Assert.That(actual[0].destinationState.name,Is.EqualTo(expected));checkedInputs++;
            }
            TestContext.WriteLine("Independent ordinary entry combinations: "+checkedInputs);
            static bool Matches(AnimatorCondition[] conditions,Dictionary<string,float> inputs) => conditions.All(c=>c.mode switch {
                AnimatorConditionMode.If=>inputs[c.parameter]!=0, AnimatorConditionMode.IfNot=>inputs[c.parameter]==0,
                AnimatorConditionMode.Equals=>inputs[c.parameter]==c.threshold, AnimatorConditionMode.NotEqual=>inputs[c.parameter]!=c.threshold,
                AnimatorConditionMode.Greater=>inputs[c.parameter]>c.threshold, AnimatorConditionMode.Less=>inputs[c.parameter]<c.threshold,
                _=>throw new InvalidOperationException("Unsupported condition") });
        }

        [TestCase(260, false)] [TestCase(260, true)] [TestCase(520, false)] [TestCase(520, true)]
        public void Compiler_NET03_MultiBytePoseSelectionPreservesLocalAndRemotePlayback(int count, bool remote)
        {
            var ps = CompilerSettings(count);
            var shallow = ps.defines.Single(d => d.paramName == "SitShallow");
            ps.defines.Remove(shallow); ps.defines.Add(shallow); // Allocate this category beyond 255/511.
            var reference = CompileSettings(ps, false); var optimized = CompileSettings(ps, true);
            var definition = ps.defines.Single(d => d.paramName == "SitShallow");
            var pose = definition.animations.Last();
            Assert.That(pose.syncdParameterValue, Is.GreaterThan(255));
            var encoding = PosingSystemConverter.SplitSyncParameters(pose.syncdParameterValue, count + 3);
            var expected = reference.layers.Single(l => l.name == "USSPS_Locomotion").stateMachine.entryTransitions
                .Single(t => t.conditions.Any(c => c.parameter == "VRMode" && c.mode == AnimatorConditionMode.Equals && c.threshold == 0) &&
                    t.conditions.Where(c => c.parameter != "VRMode").All(c => encoding.Any(p => p.name == c.parameter &&
                        (c.mode == AnimatorConditionMode.Equals ? p.value == c.threshold : c.mode == AnimatorConditionMode.If ? p.value != 0 : p.value == 0)))).destinationState;
            foreach (var controller in new[] { reference, optimized })
            {
                using var rig = new ControllerCompilerRuntimeProbe(animator, controller, !remote, false);
                if (remote) foreach (var p in encoding) rig.Set(p.name, p.value);
                else { rig.Set("SitShallow", pose.typeParameterValue); rig.Set("Upright", .6f); }
                rig.Tick(180,90);
                Assert.That(rig.Playable.GetCurrentAnimatorStateInfo(rig.Layer("USSPS_Locomotion")).shortNameHash,
                    Is.EqualTo(Animator.StringToHash(expected.name)), "High synchronized bits must distinguish poses above 255");
                Assert.That(rig.Playable.GetBool("USSPS_Pose0"), Is.EqualTo((pose.syncdParameterValue & 256) != 0));
                Assert.That(rig.Playable.GetBool("USSPS_Pose1"), Is.EqualTo((pose.syncdParameterValue & 512) != 0));
            }
        }

        [TestCase(0,0)] [TestCase(11,1)] [TestCase(21,2)]
        public void Compiler_NET04_DefaultPosesClearAndCheckEveryHighSyncBit(int type, int lowByte)
        {
            var controller = CompileSettings(CompilerSettings(1), false);
            var selector = controller.layers.Single(l=>l.name=="USSPS_LocomotionType").stateMachine;
            var encoder = selector.entryTransitions.Single(t=>t.conditions.Any(c=>c.parameter=="LocomotionType" && c.threshold==type)).destinationState;
            var values = encoder.behaviours.OfType<VRCAvatarParameterDriver>().SelectMany(b=>b.parameters).ToArray();
            for(var bit=0;bit<8;bit++) Assert.That(values.Any(p=>p.name=="USSPS_Pose"+bit && p.value==0),Is.True,
                "Returning to a standard pose must clear stale high bits");
            var playback=controller.layers.Single(l=>l.name=="USSPS_Locomotion").stateMachine;
            foreach(var entry in playback.entryTransitions.Where(t=>t.conditions.Any(c=>c.parameter=="USSPS_Pose" && c.threshold==lowByte)))
            for(var bit=0;bit<8;bit++) Assert.That(entry.conditions.Any(c=>c.parameter=="USSPS_Pose"+bit && c.mode==AnimatorConditionMode.IfNot),Is.True,
                "Standard poses must not shadow selected poses 256/257/258 or 512/513/514");
        }

        [TestCase(false)] [TestCase(true)]
        public void Compiler_TRK03_JumpAvatarSelectionAndVrModeChangesRecover(bool initialVr)
        {
            var ps = CompilerSettings();
            var reference = CompileSettings(ps, false); var optimized = CompileSettings(ps, true);
            CollectionAssert.AreEqual(Observe(reference), Observe(optimized));
            List<string> Observe(AnimatorController controller)
            {
                using var rig = new ControllerCompilerRuntimeProbe(animator,controller,true,initialVr);
                var result = new List<string>();
                Hold("standing");
                rig.Set("Grounded",0); rig.Set("VelocityY",3); Hold("jump");
                rig.Set("VelocityY",-1); Hold("fall");
                rig.Set("Grounded",1); rig.Set("VelocityY",0); Hold("land");
                rig.Set("TrackingType",0); Hold("select-avatar");
                rig.Set("TrackingType",3); Hold("restore-tracking");
                rig.Set("SitShallow",CompilerBinding(controller,"SitShallow").select); rig.Set("Upright",.6f); Hold("crouch");
                rig.Set("VRMode",initialVr?0:1); Hold("change-mode");
                rig.Set("Upright",1); Hold("stand-after-mode-change");
                return result;
                void Hold(string name) { rig.Tick(180,90); result.Add(name+":"+rig.Snapshot()); }
            }
        }

        [Test]
        public void Compiler_BUILD03_MultipleSystemsShareOneCompiledController()
        {
            defaultIdleCompilerOptimization = true;
            try { LocalFeatures_MultipleSystems_OnlySyncsSharedParametersOnce(); Assert.That(defaultIdleCompilerCreated,Is.EqualTo(1)); }
            finally { defaultIdleCompilerOptimization = null; }
        }

        [TestCase(16)] [TestCase(128)]
        public void Compiler_PERF04_SavedGraphSizeIsBoundedAndGenerationTimeIsRecorded(int poses)
        {
            var ps = CompilerSettings(poses);
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var reference = CompileSettings(ps, false);
            var referenceMs = watch.Elapsed.TotalMilliseconds;
            watch.Restart();
            var optimized = CompileSettings(ps, true);
            var optimizedMs = watch.Elapsed.TotalMilliseconds;
            watch.Stop();
            testFolder = "Assets/USSPS_CompilerStorage_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", testFolder.Substring(7));
            var referenceBytes = PersistIndependentCopy(reference, "Reference");
            var optimizedBytes = PersistIndependentCopy(optimized, "Optimized");
            TestContext.WriteLine($"Saved graph {poses} poses: reference={referenceBytes} bytes, optimized={optimizedBytes} bytes; generation reference={referenceMs:F3}ms, optimized={optimizedMs:F3}ms");
            Assert.That(referenceBytes, Is.GreaterThan(0));
            Assert.That(optimizedBytes, Is.LessThanOrEqualTo(referenceBytes * 3 + 65536),
                "The direct path trades extra transitions for latency; bound its serialized footprint too");
            Assert.That(System.IO.Directory.GetFiles(testFolder, "*.controller").Length, Is.EqualTo(2),
                "Each independently saved avatar graph must occupy one controller file");

            long PersistIndependentCopy(AnimatorController controller, string name)
            {
                // Clone transient clips too, so saving the first graph cannot make the second
                // graph's shared inputs external and artificially reduce its measured size.
                var copies = PosingSystemControllerGraph.Clone(new[] { controller });
                assets.AddRange(copies.Values);
                var saved = (AnimatorController)copies[controller];
                var path = testFolder + "/" + name + ".controller";
                AssetDatabase.CreateAsset(saved, path);
                foreach (var obj in copies.Values.Where(o => o != saved)) AssetDatabase.AddObjectToAsset(obj, saved);
                foreach (var obj in copies.Values) EditorUtility.SetDirty(obj);
                AssetDatabase.SaveAssetIfDirty(saved);
                Assert.That(AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimatorController>().Count(), Is.EqualTo(1));
                return new System.IO.FileInfo(path).Length;
            }
        }

        [TestCase(16)] [TestCase(128)]
        public void Compiler_PERF03_FullGraphEvaluationCostRemainsBounded(int poses)
        {
            var ps = CompilerSettings(poses);
            var baseline = CompileSettings(ps, false); var optimized = CompileSettings(ps, true);
            var a = new List<double>(); var b = new List<double>();
            for (var round = 0; round < 9; round++)
            {
                if (round % 2 == 0) { a.Add(Measure(baseline)); b.Add(Measure(optimized)); }
                else { b.Add(Measure(optimized)); a.Add(Measure(baseline)); }
            }
            a.Sort(); b.Sort();
            TestContext.WriteLine($"Full graph {poses} poses: reference={a[4]:F6}ms, optimized={b[4]:F6}ms per evaluation; ratio={b[4]/a[4]:F3}");
            Assert.That(b[4], Is.LessThanOrEqualTo(a[4] * 1.35 + .005), "Measure the complete converted graph, including playback condition overhead");
            double Measure(AnimatorController c)
            {
                using var rig = new ControllerCompilerRuntimeProbe(animator, c, true, false);
                // Last pose in each category exercises late entry conditions, not only the first pose.
                foreach (var d in ps.defines) rig.Set(d.paramName, d.animations.Last().typeParameterValue);
                rig.Tick(240, 90);
                var watch = System.Diagnostics.Stopwatch.StartNew();
                for (var i=0; i<1500; i++) { rig.Set("Upright", (i / 17) % 2 == 0 ? .6f : 1f); rig.Tick(1, 90); }
                watch.Stop(); return watch.Elapsed.TotalMilliseconds / 1500;
            }
        }

        [TestCase(false, false)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void Compiler_OPT01_NET02_NdmfBuildPreservesFeaturesMenusAndSyncBits(bool features, bool additiveFootHeight)
        {
            using var scope = new PosingSystemControllerGeneration.Scope(root, true);
            NdmfBuild_ExpressionParametersAndMenusMatchSelectedFeatures(features, features ? 53 : 8,
                PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive, additiveFootHeight);
            Assert.That(scope.Created, Is.EqualTo(1), "Source generation decisions: " + string.Join(";", scope.Decisions));
            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            var controller = (AnimatorController)descriptor.baseAnimationLayers.Single(l => l.type == VRCAvatarDescriptor.AnimLayerType.Base).animatorController;
            var group = controller.layers.Single(l => l.name.EndsWith("USSPS_GroupSwitch")).stateMachine;
            Assert.That(group.states.Single(s => s.state.name == "Standing").state.transitions
                .Any(t => t.destinationState != null && t.destinationState.name == "SitShallow"), Is.True,
                "The NDMF build must actually contain the generated/optimized dispatcher");
            Assert.That(controller.layers.Single(l=>l.name.EndsWith("USSPS_Locomotion")).stateMachine.entryTransitions
                .Any(t=>t.conditions.Any(c=>c.parameter.EndsWith("LocomotionTypeGroup"))),Is.True,
                "The final NDMF controller must retain direct desktop selection");
        }

        [Test]
        public void Compiler_BUILD01_ComparisonCopyPreservesOriginalAndRepeatedPrebuild()
        {
            root.AddComponent<VRCAvatarDescriptor>();
            var pipeline = root.AddComponent<VRC.Core.PipelineManager>();
            pipeline.blueprintId = "avtr_test_original_unchanged";
            var settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("ecde0b1d67f9e854992715d64f778d8c")), root.transform);
            var ps = settings.GetComponent<PosingSystem>();
            ps.autoImportAvatarAnimations = false; ps.isIconDisabled = true;
            ps.features = new PosingSystemFeatureSettings { breathing = false, fingerGestureControl = false, footHeight = false };
            ps.defines.Clear(); ps.overrideDefines.Clear();
            var sourceMerge = PosingSystemConverter.GetCommonMergeAnimator(ps);
            var sourceController = sourceMerge.animator;
            var sourceData = EditorJsonUtility.ToJson(ps);
            testFolder = "Assets/USSPS_CompilerIntegration_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", testFolder.Substring(7));
            PosingSystemGeneratedControllers.OutputRootOverride = testFolder + "/Generated";
            GameObject copy = null;
            try
            {
                copy = PosingSystemControllerGeneration.CreateComparisonCopy(root);
                Assert.That(sourceMerge.animator, Is.SameAs(sourceController));
                Assert.That(EditorJsonUtility.ToJson(ps), Is.EqualTo(sourceData));
                Assert.That(pipeline.blueprintId, Is.EqualTo("avtr_test_original_unchanged"));
                Assert.That(copy.GetComponent<VRC.Core.PipelineManager>().blueprintId, Is.Empty);
                var copiedSystem = copy.GetComponentInChildren<PosingSystem>();
                var merge = PosingSystemConverter.GetCommonMergeAnimator(copiedSystem);
                var controller = (AnimatorController)merge.animator;
                var path = AssetDatabase.GetAssetPath(controller);
                Assert.That(path, Does.StartWith(testFolder));
                Assert.That(PosingSystemControllerSelectionTests.Group(controller).states.Single(s => s.state.name == "Standing")
                    .state.transitions.Any(t => t.destinationState.name == "SitShallow"), Is.True);
                controller.AddParameter("PreserveExistingPoseMarker", AnimatorControllerParameterType.Bool);
                using (new PosingSystemControllerGeneration.Scope(copy, true))
                    PosingSystemConverter.ConvertToModularAvatarComponents(copiedSystem);
                Assert.That(merge.animator, Is.SameAs(controller));
                Assert.That(controller.parameters.Any(p => p.name == "PreserveExistingPoseMarker"), Is.True);
                Assert.That(AssetDatabase.GetAssetPath(controller), Is.EqualTo(path));
                // A later ordinary prebuild upgrades the old three-evaluation comparison
                // to the current desktop default without accumulating branches or losing poses.
                var clip = Clip(); clip.name = "ChangedComparisonPose";
                copiedSystem.defines.Add(new PosingSystem.LayerDefine("", "Changed", "SitShallow", 12, "Changed", new()
                    { new PosingSystem.AnimationDefine { enabled=true, displayName="Changed", animationClip=clip } }));
                var counts = new List<int>();
                for (var i=0;i<2;i++)
                {
                    copiedSystem.data="";
                    PosingSystemConverter.ConvertToModularAvatarComponents(copiedSystem);
                    controller=(AnimatorController)merge.animator;
                    foreach(var state in controller.layers.Single(l=>l.name=="USSPS_LocomotionType").stateMachine.states)
                    {
                        var drivers=state.state.behaviours.OfType<VRCAvatarParameterDriver>().ToArray();
                        Assert.That(drivers.Length,Is.EqualTo(1),state.state.name);
                        Assert.That(drivers[0].localOnly,Is.True,state.state.name);
                    }
                    counts.Add(PosingSystemControllerGraph.Collect(new[] { controller }, o => AssetDatabase.GetAssetPath(o)==path).Count);
                    Assert.That(controller.parameters.Count(p=>p.name==PosingSystemOneFrameSelection.Guard),Is.EqualTo(1));
                    var direct = controller.layers.Single(l=>l.name=="USSPS_Locomotion").stateMachine.stateMachines
                        .Single(m=>m.stateMachine.name=="DirectSitShallow").stateMachine;
                    Assert.That(direct.entryTransitions.Any(t=>t.conditions.Any(c=>c.parameter=="SitShallow" && c.threshold>0)),Is.True);
                    Assert.That(controller.animationClips.Any(c=>c.name.Contains("ChangedComparisonPose")),Is.True,
                        "The newly added pose must remain reachable after the upgrade");
                    Assert.That(AssetDatabase.GetAssetPath(controller),Is.EqualTo(path));
                }
                Assert.That(counts[1],Is.EqualTo(counts[0]),"Changing settings then rebuilding must not accumulate branches");
            }
            finally
            {
                PosingSystemGeneratedControllers.OutputRootOverride = null;
                if (copy != null) Object.DestroyImmediate(copy);
            }
        }
    }
}
