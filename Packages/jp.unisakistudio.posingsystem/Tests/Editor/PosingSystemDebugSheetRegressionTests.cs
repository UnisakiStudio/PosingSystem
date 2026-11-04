using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.TestTools;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemDebugSheetRegressionTests
    {
        private GameObject root;
        private PosingSystem ps;
        private AnimatorController controller;
        private AnimatorStateMachine machine;
        private readonly List<Object> owned = new();
        private string folder;
        private UnityEngine.SceneManagement.Scene scene;

        [SetUp]
        public void SetUp()
        {
            scene = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
            root = new GameObject("DebugSheetRegression");
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root, scene);
            root.AddComponent<VRCAvatarDescriptor>();
            ps = root.AddComponent<PosingSystem>();
            root.AddComponent<DuplicateEraser>().ID = "jp.unisakistudio.posingsystem_locomotion";
            controller = Own(new AnimatorController());
            machine = Own(new AnimatorStateMachine());
            controller.layers = new[] { new AnimatorControllerLayer { name="USSPS_Locomotion", stateMachine=machine, defaultWeight=1 } };
            root.AddComponent<ModularAvatarMergeAnimator>().animator = controller;
        }

        [TearDown]
        public void TearDown()
        {
            UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(scene);
            foreach (var obj in owned.AsEnumerable().Reverse())
                if (obj != null && !EditorUtility.IsPersistent(obj)) Object.DestroyImmediate(obj);
            owned.Clear();
            if (!string.IsNullOrEmpty(folder)) AssetDatabase.DeleteAsset(folder);
        }

        private T Own<T>(T obj) where T : Object { owned.Add(obj); return obj; }
        private PosingSystem.LayerDefine Layer(params PosingSystem.AnimationDefine[] poses)
            => new("", "Category", "PoseType", 0, "Category", poses.ToList());

        [Test]
        public void StandOverride_DoesNotChangeSourceTreeOrNestedTrees()
        {
            var original=Own(new AnimationClip {name="Idle"});
            var replacement=Own(new AnimationClip {name="Replacement"});
            var nested=Own(new BlendTree {name="Nested",blendParameter="Speed"}); nested.AddChild(original,0);
            var tree=Own(new BlendTree {name="Shared",blendType=BlendTreeType.SimpleDirectional2D,blendParameter="X",blendParameterY="Y"});
            tree.AddChild(original,Vector2.zero); tree.AddChild(nested,Vector2.up);
            var before=EditorJsonUtility.ToJson(tree); var nestedBefore=EditorJsonUtility.ToJson(nested);
            var state=Own(machine.AddState("Standing"));
            Own(machine.AddState("Standing_Desktop"));
            ps.overrideDefines=new List<PosingSystem.OverrideAnimationDefine> {
                new() {stateType=PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun,animationClip=tree},
                new() {stateType=PosingSystem.OverrideAnimationDefine.AnimationStateType.Stand,animationClip=replacement}
            };
            PosingSystemConverter.ConvertOverrideAnimations(ps,new NullAssetSaver());
            Assert.That(EditorJsonUtility.ToJson(tree),Is.EqualTo(before));
            Assert.That(EditorJsonUtility.ToJson(nested),Is.EqualTo(nestedBefore));
            var result=(BlendTree)state.motion; Own(result);
            Assert.That(result,Is.Not.SameAs(tree));
            Assert.That(result.children.Single(c=>c.position==Vector2.zero).motion,Is.SameAs(replacement));
            var nestedResult=(BlendTree)result.children.Single(c=>c.position==Vector2.up).motion; Own(nestedResult);
            Assert.That(nestedResult,Is.Not.SameAs(nested));
            Assert.That(nestedResult.children[0].motion,Is.SameAs(original));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MissingOrDisabledIkRules_KeepManualAdjustmentInSignature(bool emptyDefinition)
        {
            var clip=Own(new AnimationClip {name="Pose"});
            var a=Own(new AnimationClip {name="ManualA"}); var b=Own(new AnimationClip {name="ManualB"});
            var pose=new PosingSystem.AnimationDefine(clip,"Pose") {adjustmentClip=a};
            ps.defines=new List<PosingSystem.LayerDefine> {Layer(pose)};
            ps.autoPoseCatalog=Own(ScriptableObject.CreateInstance<PosingSystemAutoPoseCatalog>());
            ps.autoPoseCatalog.productionReady=true;
            var definition=new PosingSystemAutoPoseDefinition {poseKey="Category_Pose",sourceAnimation=clip};
            if(emptyDefinition) ps.autoPoseCatalog.poses.Add(definition);
            Assert.That(ps.UsesAutoPoseFor(pose),Is.False);
            var before=PosingSystemConverter.GetDefineSerializeJson(ps);
            pose.adjustmentClip=b;
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps),Is.Not.EqualTo(before));
            if(!emptyDefinition)ps.autoPoseCatalog.poses.Add(definition);
            definition.limbRules.Add(new PosingSystemAutoPoseLimbRule {enabled=true});
            Assert.That(ps.UsesAutoPoseFor(pose),Is.True);
            pose.disablePoseIk=true;
            Assert.That(ps.UsesAutoPoseFor(pose),Is.False);
        }

        [Test]
        public void SameNamedPoses_HaveIndependentDraftsEvenWhenUsingSameClip()
        {
            var window=Own(ScriptableObject.CreateInstance<PosingAnimationAdjustmentWindow>());
            var type=typeof(PosingAnimationAdjustmentWindow);
            var entryType=type.GetNestedType("AnimationEntry",BindingFlags.NonPublic);
            var a=new PosingSystem.AnimationDefine(Own(new AnimationClip()),"Same");
            var b=new PosingSystem.AnimationDefine((AnimationClip)a.animationClip,"Same");
            var layer=Layer(a,b);
            object Entry(PosingSystem.AnimationDefine p) {
                var entry=System.Activator.CreateInstance(entryType,true);
                entryType.GetField("Layer").SetValue(entry,layer);entryType.GetField("Animation").SetValue(entry,p);return entry;
            }
            var first=Entry(a); var second=Entry(b);
            var flags=BindingFlags.Instance|BindingFlags.NonPublic;
            var keyMethod=type.GetMethod("GetAdjustmentKey",flags);
            var ka=(string)keyMethod.Invoke(window,new[] {first});var kb=(string)keyMethod.Invoke(window,new[] {second});
            Assert.That(ka,Is.Not.EqualTo(kb));
            var stateMethod=type.GetMethod("GetAdjustmentState",flags);
            var sa=stateMethod.Invoke(window,new object[]{ka});var sb=stateMethod.Invoke(window,new object[]{kb});
            ((float[])sa.GetType().GetField("adjustments").GetValue(sa))[0]=.65f;
            Assert.That(((float[])sb.GetType().GetField("adjustments").GetValue(sb))[0],Is.Zero);
            a.displayName="Renamed";layer.animations.Reverse();
            Assert.That(keyMethod.Invoke(window,new[]{Entry(a)}),Is.EqualTo(ka));
        }

        [Test]
        public void ChangingSameNamedSubasset_InvalidatesSignatureAndStaysStableAfterReload()
        {
            folder="Assets/USSPS_DebugSheet_"+Guid.NewGuid().ToString("N");AssetDatabase.CreateFolder("Assets",folder.Substring(7));
            var path=folder+"/Motions.controller";
            var asset=AnimatorController.CreateAnimatorControllerAtPath(path);
            var a=new AnimationClip {name="Same"};var b=new AnimationClip {name="Same"};
            AssetDatabase.AddObjectToAsset(a,asset);AssetDatabase.AddObjectToAsset(b,asset);AssetDatabase.SaveAssetIfDirty(asset);
            var pose=new PosingSystem.AnimationDefine(a,"Pose");ps.defines=new List<PosingSystem.LayerDefine> {Layer(pose)};
            var before=PosingSystemConverter.GetDefineSerializeJson(ps);
            pose.animationClip=b;var after=PosingSystemConverter.GetDefineSerializeJson(ps);
            Assert.That(after,Is.Not.EqualTo(before));
            AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps),Is.EqualTo(after));
        }

        [Test]
        public void DisablingInitialPose_DoesNotSelectPoseWithReusedNumber()
        {
            var parameters=root.AddComponent<ModularAvatarParameters>();
            parameters.parameters.Add(new ParameterConfig{nameOrPrefix="PoseType",syncType=ParameterSyncType.Int});
            var clip=Own(new AnimationClip());
            var a=new PosingSystem.AnimationDefine(clip,"A"){initial=true};var b=new PosingSystem.AnimationDefine(clip,"B");
            ps.defines=new List<PosingSystem.LayerDefine>{Layer(a,b)};
            PosingSystemConverter.ResetTypeAndSyncedParameter(ps);PosingSystemConverter.ResetParametersWithSyncedParameter(ps);
            a.enabled=false;
            PosingSystemConverter.ResetTypeAndSyncedParameter(ps);PosingSystemConverter.ResetParametersWithSyncedParameter(ps);
            Assert.That(b.typeParameterValue,Is.EqualTo(a.typeParameterValue),"The regression requires the reused number");
            Assert.That(parameters.parameters.Single(p=>p.nameOrPrefix=="PoseType").defaultValue,Is.Zero);
        }

        [Test]
        public void OverrideController_ReportsUnsupportedBaseWithoutThrowing()
        {
            var avatar=root.GetComponent<VRCAvatarDescriptor>();
            avatar.baseAnimationLayers=new[]{new VRCAvatarDescriptor.CustomAnimLayer {type=VRCAvatarDescriptor.AnimLayerType.Base,animatorController=Own(new AnimatorOverrideController(controller)),isDefault=false}};
            LogAssert.Expect(LogType.Warning,new Regex("元姿勢を取り込めません"));
            Assert.DoesNotThrow(()=>PosingSystemConverter.AutoImportAvatarAnimations(ps));
        }
    }
}
