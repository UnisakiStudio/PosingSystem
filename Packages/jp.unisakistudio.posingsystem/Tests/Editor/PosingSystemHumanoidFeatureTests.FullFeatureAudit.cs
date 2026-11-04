// Staged outside Assets while the baseline suite runs. Copy into the package's
// Editor tests after that suite ends; never alter a running test assembly.
using System;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEditor.Animations;
using UnityEditor;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(false,false)] [TestCase(false,true)]
        [TestCase(true,false)] [TestCase(true,true)]
        public void Audit_FingerGestureMenuSelectsDisableAndEnableInstructions(bool vr,bool remote)
        {
            var source=AssetDatabase.LoadAssetAtPath<AnimatorController>("Packages/jp.unisakistudio.posingsystem/Resources/PosingSystem_FingerGesture.controller");
            var before=EditorJsonUtility.ToJson(source);
            var copies=PosingSystemControllerGraph.Clone(new[]{source});
            foreach(var value in copies.Values)if(!assets.Contains(value))assets.Add(value);
            var controller=(AnimatorController)copies[source];
            // The shared SDK-driver probe requires these observation fields. The finger
            // controller never reads them; they do not select or simulate finger states.
            foreach(var p in new[]{("IsLocal",AnimatorControllerParameterType.Bool),
                ("TrackingType",AnimatorControllerParameterType.Int),("Grounded",AnimatorControllerParameterType.Bool),
                ("Upright",AnimatorControllerParameterType.Float),("LocomotionTypeGroup",AnimatorControllerParameterType.Int),
                ("LocomotionType",AnimatorControllerParameterType.Int),("USSPS_Pose",AnimatorControllerParameterType.Int)})
                if(!controller.parameters.Any(v=>v.name==p.Item1))controller.AddParameter(p.Item1,p.Item2);
            using var rig=new ControllerCompilerRuntimeProbe(animator,controller,!remote,vr);
            rig.Tick(180,60);
            var layer=rig.Layer("USSPS_FingerGestureDisable_Sync");
            foreach(var disabled in new[]{true,false,true,false})
            {
                rig.Set(remote?"USSPS_FingerGestureDisable_Sync":"USSPS_FingerGestureDisable_Menu",disabled?1:0);
                rig.Tick(120,60);
                Assert.That(rig.Playable.GetBool("USSPS_FingerGestureDisable_Sync"),Is.EqualTo(disabled));
                var hash=rig.Playable.GetCurrentAnimatorStateInfo(layer).shortNameHash;
                var active=CompilerStates(controller.layers[layer].stateMachine).Single(s=>Animator.StringToHash(s.name)==hash);
                var instructions=active.behaviours.Where(b=>b.GetType().GetField("goalWeight")!=null).ToArray();
                Assert.That(instructions,Has.Length.EqualTo(1),"Active state must contain the Gesture playable-weight instruction");
                var instruction=instructions[0];
                Assert.That(Convert.ToInt32(instruction.GetType().GetField("layer").GetValue(instruction)),Is.EqualTo(2),"Gesture playable only");
                Assert.That(Convert.ToSingle(instruction.GetType().GetField("goalWeight").GetValue(instruction)),Is.EqualTo(disabled?0f:1f));
            }
            Assert.That(EditorJsonUtility.ToJson(source),Is.EqualTo(before));
            // This checks selected SDK instructions; remote transport and actual finger
            // rendering still require separate VRChat tests, not a fabricated IK bridge.
        }

        [TestCase(false,false,"HeadTrackingLock","head")]
        [TestCase(false,true,"HeadTrackingLock","head")]
        [TestCase(true,false,"HeadTrackingLock","head")]
        [TestCase(true,true,"HeadTrackingLock","head")]
        [TestCase(false,false,"HandTrackingLock","leftHand")]
        [TestCase(false,true,"HandTrackingLock","leftHand")]
        [TestCase(true,false,"HandTrackingLock","leftHand")]
        [TestCase(true,true,"HandTrackingLock","leftHand")]
        public void Audit_ManualTrackingLocksApplyAndRelease(bool oneFrame,bool vr,string parameter,string body)
        {
            var controller=CompileSettings(CompilerSettings(),oneFrame);
            if(oneFrame)PosingSystemOneFrameSelection.Apply(controller,assets.Add);
            using var rig=new ControllerCompilerRuntimeProbe(animator,controller,true,vr);
            rig.Tick(90,90);
            // The oracle is the public toggle's meaning, not equality with an old graph.
            // Require non-empty actual SDK callback observations on every repetition.
            for(var repeat=0;repeat<3;repeat++)
            {
                rig.Set(parameter,1);rig.Tick(30,90);
                AssertTracking("Animation");
                rig.Set(parameter,0);rig.Tick(30,90);
                AssertTracking("Tracking");
            }
            void AssertTracking(string expected)
            {
                Assert.That(rig.Snapshot(),Does.Contain(body+"="+expected),parameter);
                if(body=="leftHand")Assert.That(rig.Snapshot(),Does.Contain("rightHand="+expected),parameter);
            }
        }

        [TestCase(false,false)] [TestCase(false,true)]
        [TestCase(true,false)] [TestCase(true,true)]
        public void Audit_LocomotionLockReleasesAfterChangingPosture(bool oneFrame,bool vr)
        {
            var controller=CompileSettings(CompilerSettings(),oneFrame);
            if(oneFrame)PosingSystemOneFrameSelection.Apply(controller,assets.Add);
            using var rig=new ControllerCompilerRuntimeProbe(animator,controller,true,vr);
            rig.Set("SitShallow",CompilerBinding(controller,"SitShallow").select);
            rig.Set("Upright",.6f);rig.Tick(90,90);
            for(var repeat=0;repeat<3;repeat++)
            {
                rig.Set("LocomotionLock",1);rig.Tick(30,90);
                Assert.That(rig.LocomotionDisabled,Is.True,"Lock must actually call the SDK control");
                rig.Set("Upright",1);rig.Tick(30,90);
                Assert.That(rig.LocomotionDisabled,Is.True,"Height changes must not unlock");
                rig.Set("LocomotionLock",0);rig.Tick(90,90);
                Assert.That(rig.LocomotionDisabled,Is.False,"Release must restore locomotion");
                Assert.That(rig.Playable.GetInteger("LocomotionTypeGroup"),Is.Zero,"Release follows real standing input");
                rig.Set("Upright",.6f);rig.Tick(90,90);
            }
        }

        [TestCase(false)] [TestCase(true)]
        public void Audit_PhotographyExitResetsPlacementAndFeetButPreservesHeadAndHands(bool oneFrame)
        {
            var settings=CompilerSettings();settings.features.photography=true;
            var controller=CompileSettings(settings,oneFrame);
            if(oneFrame)PosingSystemOneFrameSelection.Apply(controller,assets.Add);
            using var rig=new ControllerCompilerRuntimeProbe(animator,controller,true,false);
            rig.Tick(90,90);
            foreach(var locks in new[]{false,true})for(var repeat=0;repeat<3;repeat++)
            {
                rig.Set("HeadTrackingLock",locks?1:0);rig.Set("HandTrackingLock",locks?1:0);
                rig.Set("LocomotionLock",1);rig.Set("USSPS_PhotoDistance",.35f);rig.Set("USSPS_PhotoYaw",.7f);
                rig.Tick(30,90);rig.Set("USSPS_PhotoReset",1);rig.Tick(30,90);
                Assert.That(rig.Playable.GetFloat("USSPS_PhotoDistance"),Is.Zero);
                Assert.That(rig.Playable.GetFloat("USSPS_PhotoYaw"),Is.EqualTo(.5f));
                Assert.That(rig.Playable.GetBool("LocomotionLock"),Is.False);
                Assert.That(rig.Playable.GetBool("HeadTrackingLock"),Is.EqualTo(locks));
                Assert.That(rig.Playable.GetBool("HandTrackingLock"),Is.EqualTo(locks));
                rig.Set("USSPS_PhotoReset",0);rig.Tick(30,90);
            }
        }
    }
}
