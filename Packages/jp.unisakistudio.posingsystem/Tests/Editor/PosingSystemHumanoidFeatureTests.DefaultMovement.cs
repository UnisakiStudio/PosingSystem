using System;
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;
using VRC.SDK3.Avatars.Components;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(0f,false,false)]
        [TestCase(.25f,false,false)]
        [TestCase(.5f,false,false)]
        [TestCase(.75f,false,false)]
        [TestCase(1f,false,false)]
        [TestCase(.5f,true,false)]
        [TestCase(0f,false,true)]
        [TestCase(.5f,false,true)]
        [TestCase(1f,false,true)]
        [TestCase(0f,false,true,"Standing")]
        [TestCase(0f,false,true,"Crouching")]
        [TestCase(0f,false,true,"Prone")]
        [TestCase(.5f,false,true,"Standing")]
        [TestCase(.5f,false,true,"Crouching")]
        [TestCase(.5f,false,true,"Prone")]
        [TestCase(-.5f,false,true,"Standing")]
        [TestCase(-.5f,false,true,"Crouching")]
        [TestCase(-.5f,false,true,"Prone")]
        [TestCase(.5f,true,true,"Standing")]
        [TestCase(.5f,true,true,"Crouching")]
        [TestCase(.5f,true,true,"Prone")]
        [TestCase(.5f,false,false,null,true)]
        [TestCase(.5f,true,false,null,true)]
        public void DefaultMovement_NestedLocomotionRetainsPoseAndMovesInAvatarAxes(float blend, bool mirror, bool sdkProxy,
            string templateState=null, bool animated=false)
        {
            root.AddComponent<VRCAvatarDescriptor>();
            var go=new GameObject("PosingSystem");go.transform.SetParent(root.transform,false);
            var ps=go.AddComponent<PosingSystem>();
            go.AddComponent<DuplicateEraser>().ID="jp.unisakistudio.posingsystem_locomotion";
            ps.features=new PosingSystemFeatureSettings {photography=true,floating=true,footHeight=false,breathing=false};
            ps.featureSettingsMigrated=true;
            testFolder="Assets/USSPS_DefaultMovement_"+Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets",testFolder.Substring(7));
            var controller=AnimatorController.CreateAnimatorControllerAtPath(testFolder+"/Base.controller");
            go.AddComponent<ModularAvatarMergeAnimator>().animator=controller;
            var layers=controller.layers;layers[0].name="USSPS_Locomotion";controller.layers=layers;
            controller.AddParameter("Blend",AnimatorControllerParameterType.Float);
            controller.AddParameter("Nested",AnimatorControllerParameterType.Float);
            controller.AddParameter("USSPS_Mirror",AnimatorControllerParameterType.Bool);
            var state=controller.layers[0].stateMachine.AddState("Default");state.iKOnFeet=false;
            state.mirrorParameter="USSPS_Mirror";state.mirrorParameterActive=true;
            AnimationClip first,second;
            if(sdkProxy)
            {
                const string dir="Packages/com.vrchat.avatars/Samples/AV3 Demo Assets/Animation/ProxyAnim/";
                first=AssetDatabase.LoadAssetAtPath<AnimationClip>(dir+"proxy_stand_still.anim");
                second=AssetDatabase.LoadAssetAtPath<AnimationClip>(dir+"proxy_low_crawl_still.anim");
            }
            else
            {
                first=PosingSystemConverter.RootOffsetClip(Vector3.up,Quaternion.Euler(25,35,15),"CustomStanding");
                second=PosingSystemConverter.RootOffsetClip(Vector3.up,Quaternion.Euler(110,-40,30),"CustomProne");
                assets.Add(first);assets.Add(second);
                if(animated)
                {
                    var from=Quaternion.Euler(110,-40,30);var to=Quaternion.Euler(45,60,-20);
                    for(var axis=0;axis<4;axis++)AnimationUtility.SetEditorCurve(second,
                        EditorCurveBinding.FloatCurve("",typeof(Animator),"RootQ."+"xyzw"[axis]),AnimationCurve.Linear(0,from[axis],1,to[axis]));
                    var clipSettings=AnimationUtility.GetAnimationClipSettings(second);clipSettings.loopTime=true;AnimationUtility.SetAnimationClipSettings(second,clipSettings);
                }
            }
            var inner=new BlendTree {name="Nested",blendParameter="Nested",useAutomaticThresholds=false};assets.Add(inner);
            inner.AddChild(first,0);inner.AddChild(second,1);
            var source=new BlendTree {name="Locomotion",blendParameter="Blend",useAutomaticThresholds=false};assets.Add(source);
            source.AddChild(first,0);source.AddChild(inner,1);state.motion=source;
            if(templateState!=null)
            {
                var template=AssetDatabase.LoadAssetAtPath<AnimatorController>("Packages/jp.unisakistudio.posingsystem/Resources/PosingSystem_Locomotion.controller");
                source=(BlendTree)template.layers.Single(l=>l.name=="USSPS_Locomotion").stateMachine.states.Single(s=>s.state.name==templateState).state.motion;
                state.motion=source;
                controller.parameters=controller.parameters.Concat(template.parameters.Where(p=>!controller.parameters.Any(existing=>existing.name==p.name))).ToArray();
            }
            var sourceBefore=EditorJsonUtility.ToJson(source);var innerBefore=EditorJsonUtility.ToJson(inner);
            var firstBefore=EditorJsonUtility.ToJson(first);var secondBefore=EditorJsonUtility.ToJson(second);
            var bones=new[]{HumanBodyBones.Hips,HumanBodyBones.Head,HumanBodyBones.LeftHand,HumanBodyBones.LeftFoot,HumanBodyBones.RightFoot};
            var original=Sample(false,false,.5f,0);
            var originalAfterRelease=Sample(false,false,.5f,0,true);
            var context=new BuildContext(root,null,false);
            try
            {
                PosingSystemConverter.ConvertOptionalFeatures(ps,context.AssetSaver);
                Assert.That(state.motion,Is.SameAs(source));
                foreach(var photo in new[]{false,true})foreach(var floating in new[]{false,true})
                foreach(var yaw in photo?new[]{.25f,.5f,.75f}:new[]{.5f})
                {
                    var actual=Sample(photo,floating,yaw,.5f);
                    var rotation=Quaternion.Euler(0,photo?(yaw-.5f)*360+180:0,0);
                    for(var i=0;i<bones.Length;i++)
                    {
                        Assert.That(Vector3.Distance(actual[i]-actual[0],rotation*(original[i]-original[0])),Is.LessThan(.006f),"Pose and yaw: "+bones[i]);
                        Assert.That(actual[i].y-original[i].y,Is.EqualTo(floating?1.2f:0).Within(.006f),"Vertical: "+bones[i]);
                    }
                    if(photo)
                    {
                        var yawAtOrigin=yaw<=.5f?yaw+.5f:yaw-.5f;
                        var baseline=Sample(true,floating,yawAtOrigin,0);
                        for(var i=0;i<bones.Length;i++)Assert.That(Vector3.Distance(actual[i]-baseline[i],Vector3.forward*1.5f),Is.LessThan(.006f),"Forward: "+bones[i]+" yaw="+yaw+" delta="+(actual[i]-baseline[i]).ToString("F5"));
                    }
                }
                var restored=Sample(true,true,.75f,.5f,true);
                for(var i=0;i<bones.Length;i++)Assert.That(Vector3.Distance(restored[i],originalAfterRelease[i]),Is.LessThan(.006f),"OFF restore: "+bones[i]);
                var info=controller.layers.Single(l=>l.name==PosingSystemConverter.FloatingPoseSpacePrefix+"Info");
                var clip=(AnimationClip)info.GetOverrideMotion(state);
                Assert.That(AnimationUtility.GetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Animator),PosingSystemConverter.FloatingPoseSpacePrefix+"Supported")).Evaluate(0),Is.EqualTo(1));
                Assert.That(EditorJsonUtility.ToJson(source),Is.EqualTo(sourceBefore));
                Assert.That(EditorJsonUtility.ToJson(inner),Is.EqualTo(innerBefore));
                Assert.That(EditorJsonUtility.ToJson(first),Is.EqualTo(firstBefore));
                Assert.That(EditorJsonUtility.ToJson(second),Is.EqualTo(secondBefore));
            }
            finally{context.DeactivateAllExtensionContexts();}

            Vector3[] Sample(bool photo,bool floating,float yaw,float distance,bool switchOff=false)
            {
                var graph=PlayableGraph.Create("DefaultMovementVerification");
                try
                {
                    graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                    var playable=AnimatorControllerPlayable.Create(graph,controller);
                    playable.SetFloat("Blend",blend);playable.SetFloat("Nested",.8f);playable.SetBool("USSPS_Mirror",mirror);
                    if(templateState!=null){playable.SetFloat("VelocityX",blend*.2f);playable.SetFloat("VelocityZ",blend);}
                    if(controller.parameters.Any(p=>p.name==PosingSystemFeatureUtility.PhotoActive))
                    {
                        playable.SetBool(PosingSystemFeatureUtility.PhotoActive,photo);playable.SetBool(PosingSystemFeatureUtility.FloatActive,floating);
                        playable.SetFloat(PosingSystemFeatureUtility.PhotoDistance,distance);playable.SetFloat(PosingSystemFeatureUtility.PhotoYaw,yaw);
                        playable.SetFloat(PosingSystemFeatureUtility.FloatHeight,.4f);
                    }
                    AnimationPlayableOutput.Create(graph,"Avatar",animator).SetSourcePlayable(playable);
                    graph.Play();for(var i=0;i<16;i++)graph.Evaluate(.02f);
                    if(switchOff){if(controller.parameters.Any(p=>p.name==PosingSystemFeatureUtility.PhotoActive)){playable.SetBool(PosingSystemFeatureUtility.PhotoActive,false);playable.SetBool(PosingSystemFeatureUtility.FloatActive,false);}for(var i=0;i<10;i++)graph.Evaluate(.02f);}
                    return bones.Select(b=>animator.GetBoneTransform(b).position).ToArray();
                }
                finally{graph.Destroy();}
            }
        }
    }
}
