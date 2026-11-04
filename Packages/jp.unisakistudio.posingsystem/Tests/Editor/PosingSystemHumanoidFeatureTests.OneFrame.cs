using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(30, false)] [TestCase(60, false)] [TestCase(90, false)] [TestCase(144, false)]
        [TestCase(30, true)] [TestCase(60, true)] [TestCase(90, true)] [TestCase(144, true)]
        [TestCase(60,false,true)] [TestCase(60,true,true)]
        public void Compiler_ONE01_OrdinaryPosturesSettleInOneEvaluation(int fps, bool selected, bool flat=false, bool observedClientHeight=false)
        {
            var controller = CompileSettings(CompilerSettings(), true);
            var playback = controller.layers.Single(l => l.name == "USSPS_Locomotion").stateMachine;
            var output = new GameObject("OneFrameOutput").transform;
            output.SetParent(root.transform, false);
            var samples = new Dictionary<AnimatorState, float>();
            foreach (var child in playback.states)
            {
                var clip = Clip();
                var sample = samples.Count + 1;
                AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("OneFrameOutput", typeof(Transform), "m_LocalPosition.x"), AnimationCurve.Constant(0, 1, sample));
                child.state.motion = clip;
                samples.Add(child.state, sample);
            }
            PosingSystemOneFrameSelection.Apply(controller, assets.Add,flat);
            using var rig = new ControllerCompilerRuntimeProbe(animator, controller, true, false);
            foreach (var p in new[] { "SitStand", "SitShallow", "SitDeep" })
                rig.Set(p, selected ? CompilerBinding(controller, p).select : 0);
            rig.Tick(fps * 2, fps);
            var failures = new List<string>();
            {
                // Actual desktop OSCQuery sample on 2026-10-28: 0.599999964 (0x3f199999).
                var crouch = observedClientHeight ? BitConverter.ToSingle(BitConverter.GetBytes(0x3f199999), 0) : .6f;
                foreach (var step in new[] { (crouch, "SitShallow", 1), (.35f, "SitDeep", 2), (crouch, "SitShallow", 1), (1f, "SitStand", 0) })
                {
                    var sync = selected ? CompilerBinding(controller, step.Item2).sync : step.Item3;
                    var target = playback.entryTransitions.Where(t => t.conditions.Any(c => c.parameter == "USSPS_Pose" && c.threshold == sync && c.mode == AnimatorConditionMode.Equals) &&
                        t.conditions.Any(c => c.parameter == "VRMode" && c.mode == AnimatorConditionMode.Equals && c.threshold == 0)).Select(t => t.destinationState).Distinct().Single();
                    var layer = rig.Layer("USSPS_Locomotion");
                    rig.Set("Upright", step.Item1);
                    var reached = -1;
                    var trace = new List<string>();
                    for (var frame = 1; frame <= 5; frame++)
                    {
                        rig.Tick(1, fps);
                        trace.Add(frame + ":" + rig.Snapshot());
                        if (Mathf.Abs(output.localPosition.x - samples[target]) < .0001f && !rig.Playable.IsInTransition(layer))
                        { reached = frame; break; }
                    }
                    TestContext.WriteLine($"ONE01 fps={fps}, selected={selected}, target={step.Item2}, evaluations={reached}\n" + string.Join("\n", trace));
                    if (reached != 1) failures.Add(step.Item2 + ": expected 1 evaluation, actual " + reached);
                    rig.Tick(fps, fps);
                    var startTime = rig.Playable.GetCurrentAnimatorStateInfo(layer).normalizedTime;
                    rig.Tick(10, fps);
                    var endTime = rig.Playable.GetCurrentAnimatorStateInfo(layer).normalizedTime;
                    if (endTime <= startTime) failures.Add(step.Item2 + ": unchanged input restarts moving clip");
                }
            }
            Assert.That(failures, Is.Empty, string.Join("\n", failures));
        }

        static string OneFrameSemanticSnapshot(ControllerCompilerRuntimeProbe rig)
        {
            // Nesting changes full paths, but must preserve states outside accelerated playback,
            // synchronized output, controls, and the identity of the selected playback state.
            var snapshot=rig.Snapshot();
            return snapshot.Substring(0,snapshot.IndexOf(";states=",StringComparison.Ordinal))+
                ";states="+string.Join(",",Enumerable.Range(0,rig.Playable.GetLayerCount())
                    .Where(i=>!PosingSystemOneFrameSelection.IsClassifierOutputLayer(rig.Playable.GetLayerName(i))).Select(i=>
                    rig.Playable.GetCurrentAnimatorStateInfo(i).shortNameHash+":"+rig.Playable.IsInTransition(i)));
        }

        [TestCase(false,false)] [TestCase(false,true)] [TestCase(true,false)] [TestCase(true,true)]
        public void Compiler_ONE02_CandidatePreservesLocksSleepJumpModesAndRemote(bool vr,bool remote)
        {
            var ps=CompilerSettings();
            var reference=CompileSettings(ps,false);var candidate=CompileSettings(ps,true);
            PosingSystemOneFrameSelection.Apply(candidate,assets.Add);
            CollectionAssert.AreEqual(Run(reference),Run(candidate));
            List<string> Run(AnimatorController c)
            {
                using var rig=new ControllerCompilerRuntimeProbe(animator,c,!remote,vr);
                foreach(var p in ps.defines)rig.Set(p.paramName,CompilerBinding(c,p.paramName).select);
                if(remote)rig.Set("USSPS_Pose",CompilerBinding(c,"SitShallow").sync);
                var result=new List<string>();
                Hold("stand",1);Hold("crouch",.6f);Hold("sit",.35f);
                rig.Set("LocomotionLock",1);Hold("locked",1);rig.Set("LocomotionLock",0);Hold("unlocked",1);
                rig.Set("USSPS_TypeFix",1);rig.Set("USSPS_LastMenu",12);Hold("fixed-shallow",1);
                rig.Set("USSPS_LastMenu",22);Hold("fixed-deep",1);rig.Set("USSPS_LastMenu",33);Hold("fixed-prone",1);
                rig.Set("USSPS_TypeFix",0);Hold("sleep",.1f);rig.Set("KawaiiPosing_IsDown",1);Hold("prone",.1f);
                Hold("stand-from-sleep",1);rig.Set("Grounded",0);rig.Set("VelocityY",3);Hold("jump",1);
                rig.Set("VelocityY",-1);Hold("fall",1);rig.Set("Grounded",1);rig.Set("VelocityY",0);Hold("land",1);
                rig.Set("TrackingType",0);Hold("select-avatar",1);rig.Set("TrackingType",3);Hold("restore",1);
                rig.Set("VRMode",vr?0:1);Hold("mode-change",.6f);rig.Set("VRMode",vr?1:0);Hold("mode-return",.35f);
                foreach(var interval in new[]{1,2,3})
                {
                    for(var cycle=0;cycle<20;cycle++){rig.Set("Upright",.6f);rig.Tick(interval,90);rig.Set("Upright",1);rig.Tick(interval,90);}
                    Hold("rapid-recovery-"+interval,.35f);
                }
                foreach(var invalid in new[]{-1,999}){rig.Set("SitShallow",invalid);Hold("invalid-"+invalid,.6f);rig.Set("SitShallow",CompilerBinding(c,"SitShallow").select);Hold("valid",.6f);}
                return result;
                void Hold(string name,float upright){rig.Set("Upright",upright);rig.Tick(180,90);result.Add(name+":"+OneFrameSemanticSnapshot(rig));}
            }
        }

        [TestCase("USSPS_ShallowThreshold")] [TestCase("USSPS_DeepThreshold")] [TestCase("USSPS_SleepThreshold")]
        public void Compiler_ONE03_CandidatePreservesVariableThresholdsAndBoundaryHistory(string parameter)
        {
            var ps=CompilerSettings();var reference=CompileSettings(ps,false);var candidate=CompileSettings(ps,true);
            PosingSystemOneFrameSelection.Apply(candidate,assets.Add);
            CollectionAssert.AreEqual(Run(reference),Run(candidate));
            List<string> Run(AnimatorController c)
            {
                using var rig=new ControllerCompilerRuntimeProbe(animator,c,true,false);
                foreach(var p in ps.defines)rig.Set(p.paramName,CompilerBinding(c,p.paramName).select);
                var result=new List<string>();
                foreach(var h in new[]{.35f,.6f,1f})foreach(var threshold in new[]{h-.01f,PosingSystemOneFrameSelection.Adjacent(h,-1),h,PosingSystemOneFrameSelection.Adjacent(h,1),h+.01f})
                {
                    rig.Set(parameter,threshold);
                    foreach(var u in new[]{1f,.6f,.35f,h,PosingSystemOneFrameSelection.Adjacent(h,-1),PosingSystemOneFrameSelection.Adjacent(h,1),.47f,.1f,1f})
                    {rig.Set("Upright",u);rig.Tick(120,90);result.Add($"{parameter}={threshold:R}, u={u:R}:"+OneFrameSemanticSnapshot(rig));}
                }
                return result;
            }
        }

        [TestCase(30)] [TestCase(60)] [TestCase(90)] [TestCase(144)]
        [TestCase(60,true)] [TestCase(90,true)]
        public void Compiler_ONE04_SelectedHumanoidMusclesChangeInOneEvaluation(int fps,bool features=false,bool observedClientHeight=false)
        {
            var ps=CompilerSettings();
            if(features)ps.features=new jp.unisakistudio.posingsystem.PosingSystemFeatureSettings {footHeight=true,photography=true,floating=true,breathing=false,fingerGestureControl=false};
            var expected=new[]{.15f,.4f,-.3f};
            for(var group=0;group<3;group++)
            {
                var clip=Clip();AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Animator),"Spine Front-Back"),AnimationCurve.Constant(0,1,expected[group]));
                ps.defines[group].animations[0].animationClip=clip;
            }
            var controller=CompileSettings(ps,true);PosingSystemOneFrameSelection.Apply(controller,assets.Add);
            using var rig=new ControllerCompilerRuntimeProbe(animator,controller,true,false);
            if(features){rig.Set("USSPS_Photo",1);rig.Set("USSPS_PhotoDistance",.6f);rig.Set("USSPS_Float",1);rig.Set("USSPS_FloatHeight",.7f);rig.Set("FootHeight",.65f);}
            foreach(var p in ps.defines)rig.Set(p.paramName,CompilerBinding(controller,p.paramName).select);
            using var handler=new HumanPoseHandler(animator.avatar,root.transform);
            var pose=new HumanPose();var muscle=Array.IndexOf(HumanTrait.MuscleName,"Spine Front-Back");
            rig.Tick(180,fps);
            var crouch = observedClientHeight ? BitConverter.ToSingle(BitConverter.GetBytes(0x3f199999), 0) : .6f;
            foreach(var step in new[]{(crouch,1),(.35f,2),(1f,0),(crouch,1),(1f,0)})
            {
                rig.Set("Upright",step.Item1);rig.Tick(1,fps);handler.GetHumanPose(ref pose);
                TestContext.WriteLine($"Humanoid fps={fps}, u={step.Item1}, actual={pose.muscles[muscle]:R}, expected={expected[step.Item2]:R}");
                Assert.That(pose.muscles[muscle],Is.EqualTo(expected[step.Item2]).Within(.002f));
            }
        }

        [TestCase(false)] [TestCase(true)]
        public void Compiler_ONE25_ObservedClientCrouchSelectsInOneEvaluation(bool selected)
            => Compiler_ONE01_OrdinaryPosturesSettleInOneEvaluation(60, selected, false, true);

        [Test]
        public void Compiler_ONE26_ObservedClientCrouchMusclesChangeInOneEvaluation()
            => Compiler_ONE04_SelectedHumanoidMusclesChangeInOneEvaluation(60, true, true);

        [Test]
        public void Compiler_ONE27_FinalClassifierPassCannotResetPoseOrAddDriverWrites()
        {
            var ps=CompilerSettings();
            ps.features.footHeight=ps.features.photography=ps.features.floating=true;
            var c=CompileSettings(ps,true);
            PosingSystemOneFrameSelection.Apply(c,assets.Add);
            var last=c.layers.Skip(c.layers.Length-3).ToArray();
            CollectionAssert.AreEqual(new[]{"USSPS_FinalShallowThreshold","USSPS_FinalDeepThreshold","USSPS_FinalSleepThreshold"},last.Select(l=>l.name));
            foreach(var layer in last)
            {
                Assert.That(layer.defaultWeight,Is.EqualTo(1));
                Assert.That(layer.syncedLayerIndex,Is.EqualTo(-1));
                var state=layer.stateMachine.states.Single().state;
                Assert.That(state.writeDefaultValues,Is.False,"A final pass must not reset humanoid or feature curves");
                Assert.That(state.behaviours,Is.Empty,"Keep original public sync/driver ownership");
                var curves=PosingSystemControllerGraph.Collect(new UnityEngine.Object[]{state.motion},o=>true)
                    .OfType<AnimationClip>().SelectMany(AnimationUtility.GetCurveBindings).ToArray();
                Assert.That(curves,Is.Not.Empty);
                foreach(var curve in curves)
                {
                    Assert.That(curve.path,Is.Empty);
                    Assert.That(curve.type,Is.EqualTo(typeof(Animator)));
                    Assert.That(curve.propertyName,Is.EqualTo(layer.name.Replace("USSPS_Final","USSPS_Is").Replace("Threshold","")));
                }
            }
        }

        [TestCase(false)] [TestCase(true)]
        public void Compiler_ONE28_FinalClassifierPassResistsAdditiveDefaultContamination(bool selected)
        {
            var ps=CompilerSettings();
            ps.features.footHeight=ps.features.photography=ps.features.floating=true;
            var c=CompileSettings(ps,true);PosingSystemOneFrameSelection.Apply(c,assets.Add);
            // Explicit stress model, not a claim to emulate VRChat scheduling: nonzero
            // parameter defaults accumulate through the real synced additive feature layers.
            var names=new[]{"USSPS_IsShallow","USSPS_IsDeep","USSPS_IsSleep"};
            var parameters=c.parameters;
            foreach(var p in parameters.Where(p=>names.Contains(p.name)))p.defaultFloat=-1;
            c.parameters=parameters;
            using var rig=new ControllerCompilerRuntimeProbe(animator,c,true,false);
            foreach(var menu in new[]{"SitStand","SitShallow","SitDeep"})rig.Set(menu,selected?CompilerBinding(c,menu).select:0);
            foreach(var u in new[]{1f,.599999964f,1f,.35f,1f,.599999964f,.35f,1f})
            {
                rig.Set("Upright",u);rig.Tick(20,90);
                var group=u==1?0:u>.5?1:2;
                Assert.That(rig.Playable.GetInteger("LocomotionTypeGroup"),Is.EqualTo(group),"u="+u);
                for(var i=0;i<names.Length;i++)
                    Assert.That(Math.Sign(rig.Playable.GetFloat(names[i])),Is.EqualTo(i<group?1:-1),names[i]+" at "+u);
                if(u==1 && !selected)Assert.That(rig.Playable.GetInteger("USSPS_Pose"),Is.Zero,"Standing sync must recover");
            }
        }

        [TestCase(16)] [TestCase(128)]
        public void Compiler_ONE05_CompareCapacityAndCpu(int poses)
        {
            var ps=CompilerSettings(poses);
            var reference=CompileSettings(ps,false);var previous=CompileSettings(ps,true);
            var compact=CompileSettings(ps,true);PosingSystemOneFrameSelection.Apply(compact,assets.Add);
            var flat=CompileSettings(ps,true);PosingSystemOneFrameSelection.Apply(flat,assets.Add,true);
            var variants=new[]{("reference",reference),("previous3",previous),("submachine",compact),("flat",flat)};
            testFolder="Assets/USSPS_OneFrameStorage_"+Guid.NewGuid().ToString("N");AssetDatabase.CreateFolder("Assets",testFolder.Substring(7));
            var times=variants.ToDictionary(v=>v.Item1,v=>new List<double>());
            for(var round=0;round<7;round++)foreach(var v in round%2==0?variants:variants.Reverse())times[v.Item1].Add(Measure(v.Item2));
            var sizes=new Dictionary<string,long>();
            foreach(var v in variants)
            {
                var graph=PosingSystemControllerGraph.Collect(new[]{v.Item2},o=>!EditorUtility.IsPersistent(o));
                var copies=PosingSystemControllerGraph.Clone(new[]{v.Item2});assets.AddRange(copies.Values);
                var saved=(AnimatorController)copies[v.Item2];var path=testFolder+"/"+v.Item1+".controller";
                AssetDatabase.CreateAsset(saved,path);foreach(var obj in copies.Values.Where(o=>o!=saved))AssetDatabase.AddObjectToAsset(obj,saved);
                foreach(var obj in copies.Values)EditorUtility.SetDirty(obj);AssetDatabase.SaveAssetIfDirty(saved);
                sizes[v.Item1]=new System.IO.FileInfo(path).Length;
                times[v.Item1].Sort();
                TestContext.WriteLine($"ONE05 poses={poses}, variant={v.Item1}, bytes={sizes[v.Item1]}, states={graph.OfType<AnimatorState>().Count()}, transitions={graph.OfType<AnimatorTransitionBase>().Count()}, conditions={graph.OfType<AnimatorTransitionBase>().Sum(t=>t.conditions.Length)}, cpu_ms={times[v.Item1][3]:F6}");
            }
            Assert.That(sizes["submachine"],Is.LessThan(sizes["previous3"]));
            Assert.That(times["submachine"][3],Is.LessThanOrEqualTo(times["reference"][3]*1.35+.005));
            double Measure(AnimatorController c)
            {
                using var rig=new ControllerCompilerRuntimeProbe(animator,c,true,false);
                foreach(var d in ps.defines)rig.Set(d.paramName,d.animations.Last().typeParameterValue);
                rig.Tick(240,90);var watch=System.Diagnostics.Stopwatch.StartNew();
                for(var i=0;i<1500;i++){rig.Set("Upright",(i/17)%2==0?.6f:1f);rig.Tick(1,90);}
                return watch.Elapsed.TotalMilliseconds/1500;
            }
        }

        [TestCase(false)] [TestCase(true)]
        public void Compiler_ONE06_CandidatePreservesSyncedFeatureMotionOverrides(bool selected)
        {
            var ps=CompilerSettings();
            ps.features=new jp.unisakistudio.posingsystem.PosingSystemFeatureSettings {mirror=true,footHeight=true,breathing=false,
                fingerGestureControl=false,photography=true,floating=true,faceForward=true};
            var reference=CompileSettings(ps,false);var candidate=CompileSettings(ps,true);
            // Explicit initialization profile: the known ACO-05 reference issue (initial -1
            // accumulating through additive layers) otherwise prevents reference selection itself.
            // Both sides use the same neutral defaults; this is not an unmodified-client claim.
            foreach(var c in new[]{reference,candidate})
            {
                var parameters=c.parameters;
                foreach(var p in parameters.Where(p=>new[]{"USSPS_IsShallow","USSPS_IsDeep","USSPS_IsSleep"}.Contains(p.name)))p.defaultFloat=0;
                c.parameters=parameters;
            }
            PosingSystemOneFrameSelection.Apply(candidate,assets.Add);
            var semantics=new Dictionary<AnimatorController,List<string>>();
            var expected=Run(reference);var actual=Run(candidate);
            CollectionAssert.AreEqual(semantics[reference],semantics[candidate],"Public synchronization and feature controls must also settle identically");
            Assert.That(actual.Count,Is.EqualTo(expected.Count));
            var failures=new List<string>();
            for(var i=0;i<actual.Count;i++)
            {
                var delta=actual[i].Zip(expected[i],(a,b)=>Mathf.Abs(a-b)).Max();
                if(delta>.003f)failures.Add($"Feature step {i}: max body/muscle difference={delta:R}");
            }
            Assert.That(failures,Is.Empty,string.Join("\n",failures));
            List<float[]> Run(AnimatorController c)
            {
                using var rig=new ControllerCompilerRuntimeProbe(animator,c,true,false);
                using var handler=new HumanPoseHandler(animator.avatar,root.transform);
                foreach(var p in ps.defines)rig.Set(p.paramName,selected?CompilerBinding(c,p.paramName).select:0);
                var result=new List<float[]>();var pose=new HumanPose();
                semantics[c]=new List<string>();
                foreach(var u in new[]{1f,.6f,.35f})foreach(var photo in new[]{0,1})foreach(var floating in new[]{0,1})foreach(var mirror in new[]{0,1})
                {
                    rig.Set("Upright",u);rig.Set("USSPS_Photo",photo);rig.Set("USSPS_PhotoDistance",.6f);rig.Set("USSPS_PhotoYaw",.65f);
                    rig.Set("USSPS_Float",floating);rig.Set("USSPS_FloatHeight",.7f);rig.Set("FootHeight",.65f);rig.Set("USSPS_Mirror",mirror);
                    rig.Tick(180,90);handler.GetHumanPose(ref pose);
                    Assert.That(rig.Playable.GetInteger("LocomotionTypeGroup"),Is.EqualTo(u==1?0:u==.6f?1:2),"Reference/candidate must actually select the requested posture");
                    semantics[c].Add(OneFrameSemanticSnapshot(rig));
                    // q and -q represent the same rotation. Compare rotated axes, not raw signs.
                    var forward=pose.bodyRotation*Vector3.forward;var up=pose.bodyRotation*Vector3.up;
                    result.Add(new[]{pose.bodyPosition.x,pose.bodyPosition.y,pose.bodyPosition.z,forward.x,forward.y,forward.z,up.x,up.y,up.z}.Concat(pose.muscles).ToArray());
                }
                return result;
            }
        }

        [TestCase(false)] [TestCase(true)]
        public void Compiler_ONE07_RestorationPreservesOriginalGraphAndFeatureOverrides(bool features)
        {
            var ps=CompilerSettings();
            if(features)ps.features=new jp.unisakistudio.posingsystem.PosingSystemFeatureSettings {footHeight=true,photography=true,floating=true,breathing=false,fingerGestureControl=false};
            var c=CompileSettings(ps,false);
            var original=PosingSystemControllerGraph.Collect(new[]{c},o=>!EditorUtility.IsPersistent(o));
            var json=original.ToDictionary(o=>o,o=>EditorJsonUtility.ToJson(o));
            for(var round=0;round<3;round++)
            {
                PosingSystemOneFrameSelection.Apply(c,assets.Add);
                Assert.That(PosingSystemOneFrameSelection.Restore(c),Is.True);
                Assert.That(PosingSystemOneFrameSelection.Restore(c),Is.False,"Restore must be idempotent");
                var restored=PosingSystemControllerGraph.Collect(new[]{c},o=>!EditorUtility.IsPersistent(o));
                CollectionAssert.AreEquivalent(original,restored,"No cloned states may remain through synced overrides");
                foreach(var obj in original)Assert.That(EditorJsonUtility.ToJson(obj),Is.EqualTo(json[obj]),obj.name);
            }
        }

        [Test]
        public void Compiler_ONE08_SavedComparisonRebuildKeepsOneFrameGraphWithoutAccumulation()
        {
            var ps=CompilerSettings();
            ps.features.photography=ps.features.floating=ps.features.footHeight=true;
            var pipeline=root.AddComponent<VRC.Core.PipelineManager>();pipeline.blueprintId="avtr_original_keep";
            var originalMerge=PosingSystemConverter.GetCommonMergeAnimator(ps);
            var originalController=originalMerge.animator;var originalData=EditorJsonUtility.ToJson(ps);
            testFolder="Assets/USSPS_OneFrameComparison_"+Guid.NewGuid().ToString("N");AssetDatabase.CreateFolder("Assets",testFolder.Substring(7));
            PosingSystemGeneratedControllers.OutputRootOverride=testFolder+"/Generated";
            GameObject copy=null;
            try
            {
                copy=PosingSystemControllerGeneration.CreateComparisonCopy(root,true);
                var copied=copy.GetComponentInChildren<jp.unisakistudio.posingsystem.PosingSystem>();
                var merge=PosingSystemConverter.GetCommonMergeAnimator(copied);
                var controller=(AnimatorController)merge.animator;var path=AssetDatabase.GetAssetPath(controller);
                Assert.That(path,Does.StartWith(testFolder));
                Assert.That(originalMerge.animator,Is.SameAs(originalController));
                Assert.That(EditorJsonUtility.ToJson(ps),Is.EqualTo(originalData));
                Assert.That(pipeline.blueprintId,Is.EqualTo("avtr_original_keep"));
                Assert.That(copy.GetComponent<VRC.Core.PipelineManager>().blueprintId,Is.Empty);
                var counts=new List<int>();
                for(var i=0;i<3;i++)
                {
                    copied.data="";PosingSystemConverter.ConvertToModularAvatarComponents(copied);
                    controller=(AnimatorController)merge.animator;
                    Assert.That(AssetDatabase.GetAssetPath(controller),Is.EqualTo(path));
                    Assert.That(controller.parameters.Count(p=>p.name==PosingSystemOneFrameSelection.Guard),Is.EqualTo(1));
                    Assert.That(controller.layers.Single(l=>l.name=="USSPS_Locomotion").stateMachine.stateMachines.Length,Is.EqualTo(4));
                    // Old subassets intentionally survive for Undo. Check the reachable graph,
                    // which is the generated controller, rather than deleting Undo's old copies.
                    counts.Add(PosingSystemControllerGraph.Collect(new[]{controller},o=>AssetDatabase.GetAssetPath(o)==path).Count);
                    using var rig=new ControllerCompilerRuntimeProbe(copy.GetComponent<Animator>(),controller,true,false);
                    foreach(var d in copied.defines)rig.Set(d.paramName,CompilerBinding(controller,d.paramName).select);
                    rig.Tick(180,90);rig.Set("Upright",.6f);rig.Tick(1,90);
                    var wanted=controller.layers.Single(l=>l.name=="USSPS_Locomotion").stateMachine.stateMachines
                        .Single(m=>m.stateMachine.name=="DirectSitShallow").stateMachine.entryTransitions
                        .Single(t=>t.conditions.Any(c=>c.parameter=="SitShallow" && c.threshold==CompilerBinding(controller,"SitShallow").select)).destinationState;
                    Assert.That(rig.Playable.GetCurrentAnimatorStateInfo(rig.Layer("USSPS_Locomotion")).shortNameHash,Is.EqualTo(Animator.StringToHash(wanted.name)));
                }
                Assert.That(counts.Distinct().Count(),Is.EqualTo(1),string.Join(",",counts));
                Assert.That(AssetDatabase.FindAssets("t:AnimatorController",new[]{testFolder}).Length,Is.GreaterThanOrEqualTo(1));
            }
            finally {PosingSystemGeneratedControllers.OutputRootOverride=null;if(copy!=null)UnityEngine.Object.DestroyImmediate(copy);}
        }

        [TestCase(false,false)] [TestCase(true,false)] [TestCase(true,true)]
        public void Compiler_ONE09_PersistedNdmfBuildRetainsOneFrameAndFeatures(bool prebuild,bool regenerate)
        {
            persistedCompilerOptimization=true;persistedCompilerOneFrame=true;
            try {NdmfBuild_PersistedPackageAssets_SupportFreshCachedAndRegeneratedControllers(prebuild,regenerate);Assert.That(persistedCompilerCreated,Is.EqualTo(prebuild?0:1));}
            finally {persistedCompilerOptimization=null;persistedCompilerOneFrame=false;}
        }

        [Test]
        public void Compiler_ONE10_LiveThresholdChangesUseRawInputAndFastPath()
        {
            var c=CompileSettings(CompilerSettings(),true);var playback=c.layers.Single(l=>l.name=="USSPS_Locomotion").stateMachine;
            var targets=new[]{"SitStand","SitShallow","SitDeep"}.Select(p=>{
                var sync=CompilerBinding(c,p).sync;
                return playback.entryTransitions.First(t=>t.conditions.Any(v=>v.parameter=="USSPS_Pose" && v.mode==AnimatorConditionMode.Equals && v.threshold==sync) &&
                    t.conditions.Any(v=>v.parameter=="VRMode" && v.mode==AnimatorConditionMode.Equals && v.threshold==0)).destinationState;
            }).ToArray();
            PosingSystemOneFrameSelection.Apply(c,assets.Add);
            using var rig=new ControllerCompilerRuntimeProbe(animator,c,true,false);
            foreach(var menu in new[]{"SitStand","SitShallow","SitDeep"})rig.Set(menu,CompilerBinding(c,menu).select);
            rig.Tick(180,90);rig.Set("Upright",.6f);rig.Tick(20,90);
            foreach(var values in new[]{(.55f,.2f,.1f,0),(.7f,.65f,.1f,2),(.8f,.59f,.34f,1)})
            {
                rig.Set("USSPS_ShallowThreshold",values.Item1);rig.Set("USSPS_DeepThreshold",values.Item2);rig.Set("USSPS_SleepThreshold",values.Item3);rig.Tick(1,90);
                Assert.That(rig.Playable.GetCurrentAnimatorStateInfo(rig.Layer("USSPS_Locomotion")).shortNameHash,Is.EqualTo(Animator.StringToHash(targets[values.Item4].name)));
            }
        }

        [TestCase(false)] [TestCase(true)]
        public void Compiler_ONE11_MultiBytePoseEncodingRemainsDistinct(bool remote)
        {
            const int count=520;var ps=CompilerSettings(count);var shallow=ps.defines.Single(d=>d.paramName=="SitShallow");
            ps.defines.Remove(shallow);ps.defines.Add(shallow);
            var c=CompileSettings(ps,false);var pose=shallow.animations.Last();Assert.That(pose.syncdParameterValue,Is.GreaterThan(511));
            var encoding=PosingSystemConverter.SplitSyncParameters(pose.syncdParameterValue,count+3);
            var expected=c.layers.Single(l=>l.name=="USSPS_Locomotion").stateMachine.entryTransitions.Single(t=>
                t.conditions.Any(v=>v.parameter=="VRMode" && v.mode==AnimatorConditionMode.Equals && v.threshold==0) &&
                t.conditions.Where(v=>v.parameter!="VRMode").All(v=>encoding.Any(p=>p.name==v.parameter &&
                    (v.mode==AnimatorConditionMode.Equals?p.value==v.threshold:v.mode==AnimatorConditionMode.If?p.value!=0:p.value==0)))).destinationState;
            PosingSystemOneFrameSelection.Apply(c,assets.Add);
            using var rig=new ControllerCompilerRuntimeProbe(animator,c,!remote,false);
            if(remote)foreach(var p in encoding)rig.Set(p.name,p.value);else rig.Set("SitShallow",pose.typeParameterValue);
            rig.Tick(180,90);rig.Set("Upright",.6f);rig.Tick(1,90);
            Assert.That(rig.Playable.GetCurrentAnimatorStateInfo(rig.Layer("USSPS_Locomotion")).shortNameHash,Is.EqualTo(Animator.StringToHash(expected.name)));
            rig.Tick(180,90);
            foreach(var p in encoding)Assert.That(p.name=="USSPS_Pose"?rig.Playable.GetInteger(p.name):rig.Playable.GetBool(p.name)?1:0,Is.EqualTo((int)p.value));
        }

        [TestCase(false)] [TestCase(true)]
        public void Compiler_ONE12_JumpLandingAndModeTransitionsKeepFrameTiming(bool selected)
        {
            var ps=CompilerSettings();var reference=CompileSettings(ps,false);var candidate=CompileSettings(ps,true);
            PosingSystemOneFrameSelection.Apply(candidate,assets.Add);
            var expected=Run(reference);var actual=Run(candidate);
            var differences=Enumerable.Range(0,expected.Count).Where(i=>expected[i]!=actual[i]).Select(i=>$"{i}: expected {expected[i]}; actual {actual[i]}").ToArray();
            Assert.That(differences,Is.Empty,string.Join("\n",differences.Take(30)));
            List<string> Run(AnimatorController c)
            {
                using var rig=new ControllerCompilerRuntimeProbe(animator,c,true,false);
                if(selected)rig.Set("SitStand",CompilerBinding(c,"SitStand").select);
                rig.Tick(180,90);var result=new List<string>();var layer=rig.Layer("USSPS_Locomotion");
                var states=CompilerStates(c.layers[layer].stateMachine).GroupBy(s=>Animator.StringToHash(s.name)).ToDictionary(g=>g.Key,g=>g.First().name);
                rig.Set("Grounded",0);rig.Set("VelocityY",3);Trace("jump",12);
                rig.Set("VelocityY",-10);Trace("fall",18);
                rig.Set("Grounded",1);rig.Set("VelocityY",0);Trace("land",100);
                rig.Set("TrackingType",0);Trace("select-avatar",4);rig.Set("TrackingType",3);Trace("tracking",8);
                rig.Set("VRMode",1);Trace("vr",20);rig.Set("VRMode",0);Trace("desktop",20);
                return result;
                void Trace(string label,int frames)
                {
                    for(var frame=0;frame<frames;frame++)
                    {
                        rig.Tick(1,90);var current=rig.Playable.GetCurrentAnimatorStateInfo(layer);var next=rig.Playable.GetNextAnimatorStateInfo(layer);
                        result.Add(label+"/"+frame+":"+Name(current.shortNameHash)+"->"+Name(next.shortNameHash)+":"+rig.Playable.IsInTransition(layer));
                    }
                }
                string Name(int key)=>states.TryGetValue(key,out var name)?name:key.ToString();
            }
        }

        [TestCase(false)] [TestCase(true)]
        public void Compiler_ONE13_RuntimeProbeObservesSdkTrackingAndPoseSpace(bool oneFrame)
        {
            var c=CompileSettings(CompilerSettings(),oneFrame);if(oneFrame)PosingSystemOneFrameSelection.Apply(c,assets.Add);
            using var rig=new ControllerCompilerRuntimeProbe(animator,c,true,false);
            rig.Set("SitShallow",CompilerBinding(c,"SitShallow").select);rig.Tick(180,90);
            rig.Set("Upright",.6f);rig.Tick(180,90);
            TestContext.WriteLine(rig.Snapshot());
            Assert.That(rig.Snapshot(),Does.Contain("leftFoot=Animation"),"Matching two empty tracking records is not evidence of SDK compatibility");
            Assert.That(rig.PoseSpace,Is.True);
        }

        [Test]
        public void Compiler_ONE14_ReportControlLatencySeparatelyFromMotion()
        {
            var settings=CompilerSettings();
            var reference=Measure(false);var candidate=Measure(true);
            Assert.That(candidate.tracking,Is.InRange(1,reference.tracking),"Tracking controls must not be delayed by playback acceleration");
            Assert.That(candidate.poseSpace,Is.InRange(1,reference.poseSpace));
            (int tracking,int poseSpace) Measure(bool oneFrame)
            {
                var c=CompileSettings(settings,oneFrame);
                if(oneFrame)PosingSystemOneFrameSelection.Apply(c,assets.Add);
                using var rig=new ControllerCompilerRuntimeProbe(animator,c,true,false);
                rig.Set("SitShallow",CompilerBinding(c,"SitShallow").select);rig.Tick(180,90);
                Assert.That(rig.Snapshot(),Does.Not.Contain("leftFoot=Animation"));
                Assert.That(rig.PoseSpace,Is.False);
                rig.Set("Upright",.6f);int tracking=0,space=0;
                for(var frame=1;frame<=12;frame++)
                {
                    rig.Tick(1,90);
                    if(tracking==0 && rig.Snapshot().Contains("leftFoot=Animation"))tracking=frame;
                    if(space==0 && rig.PoseSpace)space=frame;
                }
                TestContext.WriteLine($"ONE14 oneFrame={oneFrame}, tracking_evaluations={tracking}, pose_space_evaluations={space}");
                Assert.That(tracking,Is.GreaterThan(0));Assert.That(space,Is.GreaterThan(0));
                return(tracking,space);
            }
        }
    }
}
