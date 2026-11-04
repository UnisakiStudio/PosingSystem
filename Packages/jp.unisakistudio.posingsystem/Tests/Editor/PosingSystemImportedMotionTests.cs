using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;
using StateType = jp.unisakistudio.posingsystem.PosingSystem.OverrideAnimationDefine.AnimationStateType;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemImportedMotionTests
    {
        private PosingSystemHumanoidFeatureTests humanoid;
        private GameObject root;
        private PosingSystem ps;
        private AnimatorController controller;
        private AnimatorStateMachine machine;
        private readonly HashSet<Object> owned = new();
        private Scene scene;
        private string folder;

        [SetUp]
        public void SetUp()
        {
            humanoid = new PosingSystemHumanoidFeatureTests();
            humanoid.SetUp();
            root = (GameObject)typeof(PosingSystemHumanoidFeatureTests).GetField("root", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(humanoid);
            scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
            SceneManager.MoveGameObjectToScene(root, scene);
            root.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            ps = root.AddComponent<PosingSystem>();
            ps.autoImportAvatarAnimations = false;
            ps.enableAvatarSpecificAdjustment = false;
            root.AddComponent<DuplicateEraser>().ID = "jp.unisakistudio.posingsystem_locomotion";
            controller = Own(new AnimatorController());
            machine = Own(new AnimatorStateMachine());
            controller.layers = new[] { new AnimatorControllerLayer { name="USSPS_Locomotion", stateMachine=machine, defaultWeight=1 } };
            root.AddComponent<ModularAvatarMergeAnimator>().animator = controller;
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var state in machine.states) Collect(state.state.motion);
            humanoid.TearDown();
            UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
            foreach (var obj in owned.Reverse()) if (obj != null && !EditorUtility.IsPersistent(obj)) Object.DestroyImmediate(obj);
            owned.Clear();
            if (folder != null) AssetDatabase.DeleteAsset(folder);
        }

        private T Own<T>(T value) where T : Object { owned.Add(value); return value; }
        private void Collect(Motion motion)
        {
            if (motion == null || !owned.Add(motion)) return;
            if (motion is BlendTree tree) foreach (var child in tree.children) Collect(child.motion);
        }
        private AnimationClip Clip(string name, float rootY = 1)
        {
            var clip = Own(PosingSystemConverter.RootOffsetClip(new Vector3(.2f, rootY, .3f), Quaternion.identity, name));
            Curve(clip,"Spine Front-Back", .1f);
            return clip;
        }
        private static void Curve(AnimationClip clip, string name, float value)
            => AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("",typeof(Animator),name),AnimationCurve.Constant(0,1,value));
        private static float Value(AnimationClip clip, string name, float time=0)
            => AnimationUtility.GetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Animator),name)).Evaluate(time);
        private AnimatorState State(string name) => Own(machine.AddState(name));
        private PosingSystem.OverrideAnimationDefine Definition(Motion motion, StateType type=StateType.AvatarSelect)
        {
            var definition = new PosingSystem.OverrideAnimationDefine {animationClip=motion,stateType=type};
            ps.overrideDefines = new List<PosingSystem.OverrideAnimationDefine> {definition};
            return definition;
        }

        [Test]
        public void ClipRotationRootAndMuscleAdjustments_AreAppliedWithoutChangingInputs()
        {
            var source=Clip("Source"); var adjustment=Own(new AnimationClip());
            Curve(adjustment,"RootT.y",.25f); Curve(adjustment,"Spine Front-Back",.2f);
            var before=EditorJsonUtility.ToJson(source); var adjustmentBefore=EditorJsonUtility.ToJson(adjustment);
            var state=State("AvatarSelecting"); var definition=Definition(source);
            definition.adjustmentClip=adjustment; definition.isRotate=true; definition.rotate=45;
            var settingsBefore=EditorJsonUtility.ToJson(ps);
            PosingSystemConverter.ConvertOverrideAnimations(ps);
            var result=(AnimationClip)state.motion;
            Assert.That(result,Is.Not.SameAs(source));
            Assert.That(Value(result,"RootT.y"),Is.EqualTo(1.25f).Within(.0001));
            Assert.That(Value(result,"RootT.x"),Is.EqualTo(.2f).Within(.0001),"Imported movement must not be head-centered");
            Assert.That(Value(result,"Spine Front-Back"),Is.EqualTo(.3f).Within(.005));
            Assert.That(AnimationUtility.GetAnimationClipSettings(result).orientationOffsetY,Is.EqualTo(45));
            Assert.That(EditorJsonUtility.ToJson(source),Is.EqualTo(before));
            Assert.That(EditorJsonUtility.ToJson(adjustment),Is.EqualTo(adjustmentBefore));
            Assert.That(EditorJsonUtility.ToJson(ps),Is.EqualTo(settingsBefore));
            Collect(result);
            PosingSystemConverter.ConvertOverrideAnimations(ps);
            Assert.That(Value((AnimationClip)state.motion,"RootT.y"),Is.EqualTo(1.25f).Within(.0001),"Do not accumulate adjustments");
        }

        [TestCase(StateType.StandWalkRun,"Standing")]
        [TestCase(StateType.Crouch,"Crouching")]
        [TestCase(StateType.Prone,"Prone")]
        public void NestedTree_AdjustsEveryClipAndPreservesMovementMetadata(StateType type,string stateName)
        {
            var clip=Clip("Walk"); var nested=Own(new BlendTree {blendParameter="Speed",useAutomaticThresholds=false});
            nested.AddChild(clip,0); nested.AddChild(clip,2);
            var tree=Own(new BlendTree {blendType=BlendTreeType.FreeformDirectional2D,blendParameter="VelocityX",blendParameterY="VelocityZ"});
            tree.children=new[]{new ChildMotion{motion=nested,position=new Vector2(1,2),timeScale=.7f,cycleOffset=.3f,mirror=true},
                new ChildMotion{motion=nested,position=Vector2.zero,timeScale=1}};
            var before=EditorJsonUtility.ToJson(tree);var nestedBefore=EditorJsonUtility.ToJson(nested);
            var state=State(stateName); State(stateName+"_Desktop");var definition=Definition(tree,type);definition.isRotate=true;definition.rotate=30;
            PosingSystemConverter.ConvertOverrideAnimations(ps);
            var result=(BlendTree)state.motion;var child=result.children[0];var resultNested=(BlendTree)child.motion;
            Assert.That(result,Is.Not.SameAs(tree));Assert.That(resultNested,Is.Not.SameAs(nested));
            Assert.That(result.children[1].motion,Is.SameAs(resultNested));
            Assert.That(child.position,Is.EqualTo(new Vector2(1,2)));Assert.That(child.timeScale,Is.EqualTo(.7f));
            Assert.That(child.cycleOffset,Is.EqualTo(.3f));Assert.That(child.mirror,Is.True);
            Assert.That(resultNested.children[0].motion,Is.SameAs(resultNested.children[1].motion));
            Assert.That(AnimationUtility.GetAnimationClipSettings((AnimationClip)resultNested.children[0].motion).orientationOffsetY,Is.EqualTo(30));
            Assert.That(controller.parameters.Any(p=>p.name=="Speed"),Is.True);
            Assert.That(EditorJsonUtility.ToJson(tree),Is.EqualTo(before));Assert.That(EditorJsonUtility.ToJson(nested),Is.EqualTo(nestedBefore));
        }

        [Test]
        public void WholeStateMotionTime_UsesFloatParameterAndCanBeDisabled()
        {
            var state=State("AvatarSelecting");var definition=Definition(Clip("Select"));
            definition.isMotionTime=true; definition.motionTimeParamName="ImportedTime";
            PosingSystemConverter.ConvertOverrideAnimations(ps);
            Assert.That(state.timeParameterActive,Is.True);Assert.That(state.timeParameter,Is.EqualTo("ImportedTime"));
            Assert.That(controller.parameters.Single(p=>p.name=="ImportedTime").type,Is.EqualTo(AnimatorControllerParameterType.Float));
            definition.isMotionTime=false;PosingSystemConverter.ConvertOverrideAnimations(ps);
            Assert.That(state.timeParameterActive,Is.False);Assert.That(state.timeParameter,Is.Empty);
        }

        [Test]
        public void IdleOnlyMotionTime_DoesNotFreezeWalkOrChangeSourceTree()
        {
            var idle=Clip("Idle");var walk=Clip("Walk");
            AnimationUtility.SetEditorCurve(idle,EditorCurveBinding.FloatCurve("",typeof(Animator),"Spine Front-Back"),AnimationCurve.Linear(0,0,1,.8f));
            idle.frameRate=30;
            var tree=Own(new BlendTree {blendType=BlendTreeType.SimpleDirectional2D,blendParameter="VelocityX",blendParameterY="VelocityZ"});
            tree.AddChild(walk,Vector2.zero);tree.AddChild(walk,Vector2.up);
            // Non-collinear directions are required for a valid native 2D blend.
            tree.AddChild(walk,Vector2.right);tree.AddChild(walk,Vector2.left);tree.AddChild(walk,Vector2.down);
            var original=EditorJsonUtility.ToJson(tree);
            var state=State("Standing");state.motion=tree;State("Standing_Desktop").motion=tree;
            var definition=Definition(idle,StateType.Stand);definition.isMotionTime=true;definition.motionTimeParamName="IdleTime";
            PosingSystemConverter.ConvertOverrideAnimations(ps);
            var output=(BlendTree)state.motion;
            Assert.That(state.timeParameterActive,Is.False,"The parent walking state must keep advancing");
            Assert.That(output.children.Single(c=>c.position==Vector2.up).motion,Is.SameAs(walk));
            var timed=(BlendTree)output.children.Single(c=>c.position==Vector2.zero).motion;
            Assert.That(timed.blendParameter,Is.EqualTo("IdleTime"));Assert.That(timed.children.Length,Is.EqualTo(31));
            Assert.That(Value((AnimationClip)timed.children[15].motion,"Spine Front-Back"),Is.EqualTo(.4f).Within(.0001));
            Assert.That(EditorJsonUtility.ToJson(tree),Is.EqualTo(original));
            controller.AddParameter("VelocityX",AnimatorControllerParameterType.Float);
            controller.AddParameter("VelocityZ",AnimatorControllerParameterType.Float);
            machine.defaultState=state;
            var animator=root.GetComponent<Animator>();
            var graph=PlayableGraph.Create("ImportedIdleTimePlayback");
            try
            {
                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                var playable=AnimatorControllerPlayable.Create(graph,controller);
                AnimationPlayableOutput.Create(graph,"Avatar",animator).SetSourcePlayable(playable);
                graph.Play();
                using(var handler=new HumanPoseHandler(animator.avatar,root.transform))
                {
                    var muscle=Array.IndexOf(HumanTrait.MuscleName,"Spine Front-Back");
                    foreach(var time in new[]{0f,.5f,1f})
                    {
                        playable.SetFloat("IdleTime",time);graph.Evaluate(.02f);
                        var pose=new HumanPose();handler.GetHumanPose(ref pose);
                        Assert.That(pose.muscles[muscle],Is.EqualTo(.8f*time).Within(.01));
                    }
                    playable.SetFloat("VelocityZ",1);graph.Evaluate(.02f);
                    var walking=new HumanPose();handler.GetHumanPose(ref walking);
                    Assert.That(walking.muscles[muscle],Is.EqualTo(.1f).Within(.01));
                    Assert.That(playable.GetCurrentAnimatorClipInfo(0).Single().clip,Is.SameAs(walk));
                }
            }
            finally {graph.Destroy();}
        }

        [Test]
        public void StaticIdleMotionTime_DoesNotDuplicateAnUnchangingPosePerFrame()
        {
            var idle=Clip("StaticIdle");var tree=Own(new BlendTree());tree.AddChild(idle,Vector2.zero);
            var state=State("Standing");state.motion=tree;State("Standing_Desktop").motion=tree;
            var definition=Definition(idle,StateType.Stand);definition.isMotionTime=true;definition.motionTimeParamName="IdleTime";
            PosingSystemConverter.ConvertOverrideAnimations(ps);
            var result=((BlendTree)state.motion).children.Single().motion;
            Assert.That(result,Is.TypeOf<AnimationClip>());
            Assert.That(result,Is.Not.SameAs(idle));
            Assert.That(Value((AnimationClip)result,"Spine Front-Back"),Is.EqualTo(.1f));
        }

        [Test]
        public void NoAdjustment_PreservesSourceMotionAndAnimationSettings()
        {
            var source=Clip("Source");var state=State("AvatarSelecting");Definition(source);
            PosingSystemConverter.ConvertOverrideAnimations(ps);
            Assert.That(state.motion,Is.SameAs(source));
        }

        [Test]
        public void ProcessedClip_PersistsInsidePrebuildController()
        {
            folder="Assets/USSPS_ImportTest_"+Guid.NewGuid().ToString("N");AssetDatabase.CreateFolder("Assets",folder.Substring(7));
            var path=folder+"/Generated.controller";AssetDatabase.CreateAsset(controller,path);AssetDatabase.AddObjectToAsset(machine,controller);
            var state=State("AvatarSelecting");
            if (!AssetDatabase.Contains(state)) AssetDatabase.AddObjectToAsset(state,controller);
            var definition=Definition(Clip("Source"));definition.isRotate=true;definition.rotate=90;
            PosingSystemConverter.ConvertOverrideAnimations(ps);
            Assert.That(AssetDatabase.GetAssetPath(state.motion),Is.EqualTo(path));
            EditorUtility.SetDirty(controller);EditorUtility.SetDirty(state);AssetDatabase.SaveAssetIfDirty(controller);
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceSynchronousImport|ImportAssetOptions.ForceUpdate);
            var reloaded=AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            Assert.That(reloaded.animationClips.Any(c=>AnimationUtility.GetAnimationClipSettings(c).orientationOffsetY==90),Is.True);
        }

        [Test]
        public void BuildSaver_OwnsProcessedMotionsWithTransientController()
        {
            folder="Assets/USSPS_ImportTest_"+Guid.NewGuid().ToString("N");AssetDatabase.CreateFolder("Assets",folder.Substring(7));
            var path=folder+"/BuildAssets.asset";var container=new AvatarMask();AssetDatabase.CreateAsset(container,path);
            var state=State("Standing");State("Standing_Desktop");
            var tree=Own(new BlendTree{blendParameter="Speed"});tree.AddChild(Clip("Source"),0);
            var definition=Definition(tree,StateType.StandWalkRun);definition.isRotate=true;definition.rotate=90;
            using(var saver=new nadena.dev.ndmf.SingleAssetSaver(container))
                PosingSystemConverter.ConvertOverrideAnimations(ps,saver);
            var result=(BlendTree)state.motion;
            Assert.That(EditorUtility.IsPersistent(controller),Is.False);
            Assert.That(AssetDatabase.GetAssetPath(result),Is.EqualTo(path));
            Assert.That(AssetDatabase.GetAssetPath(result.children[0].motion),Is.EqualTo(path));
            Assert.That(EditorUtility.IsPersistent(tree),Is.False);
        }
    }
}
