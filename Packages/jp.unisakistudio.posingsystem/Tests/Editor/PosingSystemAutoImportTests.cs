using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;
using StateType = jp.unisakistudio.posingsystem.PosingSystem.OverrideAnimationDefine.AnimationStateType;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemAutoImportTests
    {
        PosingSystemHumanoidFeatureTests humanoid;
        GameObject root;
        VRCAvatarDescriptor avatar;
        PosingSystem ps;
        AnimatorController source;
        AnimatorState jump;
        AnimationClip clip;
        Scene scene;
        string folder;
        readonly HashSet<Object> owned = new();

        [SetUp]
        public void SetUp()
        {
            PosingSystemGeneratedCleanup.Suspend = true;
            folder = "Assets/USSPS_AutoImportTests_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folder.Substring(7));
            PosingSystemGeneratedControllers.OutputRootOverride = folder + "/Generated";
            humanoid = new PosingSystemHumanoidFeatureTests(); humanoid.SetUp();
            root = (GameObject)typeof(PosingSystemHumanoidFeatureTests).GetField("root", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(humanoid);
            scene = EditorSceneManager.NewPreviewScene(); SceneManager.MoveGameObjectToScene(root, scene);
            avatar = root.AddComponent<VRCAvatarDescriptor>();
            avatar.baseAnimationLayers = new[] { VRCAvatarDescriptor.AnimLayerType.Base, VRCAvatarDescriptor.AnimLayerType.Additive,
                VRCAvatarDescriptor.AnimLayerType.Gesture, VRCAvatarDescriptor.AnimLayerType.Action, VRCAvatarDescriptor.AnimLayerType.FX }
                .Select(type=>new VRCAvatarDescriptor.CustomAnimLayer {type=type,isDefault=true}).ToArray();
            avatar.customizeAnimationLayers = true;
            ps = Settings();
            source = AnimatorController.CreateAnimatorControllerAtPath(folder + "/Source.controller");
            clip = Clip("AutoJump", .2f);
            jump = source.layers[0].stateMachine.AddState("Jump"); jump.motion = clip;
            SetSource(source);
        }

        PosingSystem Settings()
        {
            var settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath("ecde0b1d67f9e854992715d64f778d8c")), root.transform);
            var system = settings.GetComponent<PosingSystem>();
            system.features = new PosingSystemFeatureSettings { mirror=false, footHeight=false, breathing=false, fingerGestureControl=false,
                faceForward=false, photography=false, floating=false, rollover=false };
            system.featureSettingsMigrated = true;
            system.autoImportAvatarAnimations = true; system.isIconDisabled = true;
            system.enableAvatarSpecificAdjustment = false;
            system.defines.Clear(); system.overrideDefines = new List<PosingSystem.OverrideAnimationDefine>();
            return system;
        }

        void SetSource(RuntimeAnimatorController controller, bool isDefault=false)
        {
            var layers=avatar.baseAnimationLayers ?? Array.Empty<VRCAvatarDescriptor.CustomAnimLayer>();
            var index=Array.FindIndex(layers,l=>l.type==VRCAvatarDescriptor.AnimLayerType.Base);
            var slot=new VRCAvatarDescriptor.CustomAnimLayer { type=VRCAvatarDescriptor.AnimLayerType.Base,
                isDefault=isDefault, animatorController=controller };
            if(index<0)layers=layers.Concat(new[]{slot}).ToArray();else layers[index]=slot;
            avatar.baseAnimationLayers=layers;
        }

        AnimationClip Clip(string name, float value)
        {
            var result = new AnimationClip {name=name};
            AnimationUtility.SetEditorCurve(result, EditorCurveBinding.FloatCurve("",typeof(Animator),"Spine Front-Back"),AnimationCurve.Constant(0,1,value));
            AssetDatabase.CreateAsset(result, AssetDatabase.GenerateUniqueAssetPath(folder + "/" + name + ".anim"));
            return result;
        }

        PosingSystem.OverrideAnimationDefine Row(Motion motion, StateType type=StateType.Jump, bool enabled=true)
            => new() { animationClip=motion, stateType=type, enabled=enabled };

        static IEnumerable<AnimatorState> States(AnimatorStateMachine machine)
            => machine.states.Select(s=>s.state).Concat(machine.stateMachines.SelectMany(m=>States(m.stateMachine)));

        AnimatorController Output(PosingSystem system=null)
            => (AnimatorController)PosingSystemConverter.GetCommonMergeAnimator(system ?? ps).animator;
        AnimatorState OutputState(string name, PosingSystem system=null)
            => States(Output(system).layers.Single(l=>l.name=="USSPS_Locomotion").stateMachine).Single(s=>s.name==name);
        void Prebuild() => PosingSystemEditor.Prebuild(ps);
        string Signature() => PosingSystemConverter.GetDefineSerializeJson(ps);
        string ManualJson(PosingSystem system) => JsonUtility.ToJson(new Rows { rows=system.overrideDefines });
        [Serializable] class Rows { public List<PosingSystem.OverrideAnimationDefine> rows; }

        [TearDown]
        public void TearDown()
        {
            foreach (var controller in root.GetComponentsInChildren<ModularAvatarMergeAnimator>(true).Select(m=>m.animator).OfType<AnimatorController>())
                owned.UnionWith(PosingSystemControllerGraph.Collect(new Object[]{controller},o=>PosingSystemControllerGraph.IsGraphObject(o)&&!EditorUtility.IsPersistent(o)));
            humanoid.TearDown();
            EditorSceneManager.ClosePreviewScene(scene);
            foreach(var obj in owned.Reverse()) if(obj!=null&&!EditorUtility.IsPersistent(obj)) Object.DestroyImmediate(obj);
            owned.Clear();
            foreach(var path in AssetDatabase.FindAssets("",new[]{folder}).Select(AssetDatabase.GUIDToAssetPath).Distinct().Where(p=>!AssetDatabase.IsValidFolder(p)))
                foreach(var obj in AssetDatabase.LoadAllAssetsAtPath(path)) if(obj!=null) Undo.ClearUndo(obj);
            AssetDatabase.DeleteAsset(folder);
            PosingSystemGeneratedControllers.OutputRootOverride = null;
            PosingSystemGeneratedCleanup.Suspend = false;
        }

        [TestCase(false)] [TestCase(true)]
        public void Detection_IsReadOnlyForEmptyOrNullSettings(bool nullList)
        {
            if(nullList) ps.overrideDefines=null;
            var original=ps.overrideDefines;var before=EditorJsonUtility.ToJson(ps);
            for(var i=0;i<2;i++)
            {
                var result=PosingSystemConverter.AutoImportAvatarAnimations(ps);
                Assert.That(result.Single().animationClip,Is.SameAs(clip));
                Assert.That(ps.overrideDefines,Is.SameAs(original));
                Assert.That(EditorJsonUtility.ToJson(ps),Is.EqualTo(before));
            }
        }

        [TestCase("redetect")] [TestCase("adjustment-disable")] [TestCase("adjustment-remove")] [TestCase("shared")]
        public void Compiler_IMP01_OptimizedPrebuildPreservesAutomaticAndAdjustedImports(string scenario)
        {
            using var scope=new PosingSystemControllerGeneration.Scope(root,true);
            switch(scenario)
            {
                case "redetect":Prebuild_RedetectsChangedSourceAndRestoresDefaultWhenDisabled();break;
                case "adjustment-disable":Prebuild_ManualDisableOrRemovalRestoresDefaultAndClearsMotionTime(false);break;
                case "adjustment-remove":Prebuild_ManualDisableOrRemovalRestoresDefaultAndClearsMotionTime(true);break;
                case "shared":RebuildingEitherProduct_PreservesSharedManualChoiceAndOneGeneratedFile();break;
            }
            Assert.That(scope.Created,Is.EqualTo(1),string.Join(";",scope.Decisions));
            Assert.That(Output().layers.Single(l=>l.name=="USSPS_Locomotion").stateMachine.entryTransitions
                .Any(t=>t.conditions.Any(c=>c.parameter=="LocomotionTypeGroup")),Is.True);
        }

        [TestCase("null")] [TestCase("empty")] [TestCase("missing")] [TestCase("default")] [TestCase("reordered")]
        public void Detection_HandlesMissingDefaultAndReorderedLayers(string kind)
        {
            if(kind=="null")avatar.baseAnimationLayers=null;
            if(kind=="empty")avatar.baseAnimationLayers=Array.Empty<VRCAvatarDescriptor.CustomAnimLayer>();
            if(kind=="missing")avatar.baseAnimationLayers=new[]{new VRCAvatarDescriptor.CustomAnimLayer{type=VRCAvatarDescriptor.AnimLayerType.FX,animatorController=source}};
            if(kind=="default")SetSource(source,true);
            if(kind=="reordered")avatar.baseAnimationLayers=new[]{new VRCAvatarDescriptor.CustomAnimLayer{type=VRCAvatarDescriptor.AnimLayerType.FX},avatar.baseAnimationLayers[0]};
            Assert.That(PosingSystemConverter.AutoImportAvatarAnimations(ps).Count,Is.EqualTo(kind=="reordered"?1:0));
        }

        [TestCase("STANDING",StateType.StandWalkRun)]
        [TestCase("Crouching",StateType.Crouch)]
        [TestCase("Prone",StateType.Prone)]
        public void Detection_FindsWholeLocomotionTreesWithoutChangingTheirChildren(string name,StateType type)
        {
            var tree=new BlendTree {name="Locomotion",blendParameter="Speed"};tree.AddChild(clip,0);tree.AddChild(clip,1);
            AssetDatabase.AddObjectToAsset(tree,source);
            var state=source.layers[0].stateMachine.AddState(name);state.motion=tree;
            var before=EditorJsonUtility.ToJson(tree);
            Assert.That(PosingSystemConverter.AutoImportAvatarAnimations(ps).Single(d=>d.stateType==type).animationClip,Is.SameAs(tree));
            Assert.That(EditorJsonUtility.ToJson(tree),Is.EqualTo(before));Assert.That(ps.overrideDefines,Is.Empty);
            var signature=Signature();tree.blendParameter="UpdatedSpeed";
            Assert.That(Signature(),Is.Not.EqualTo(signature));
        }

        [TestCase("enabled")] [TestCase("disabled")] [TestCase("empty")]
        public void ManualChoice_IsPreservedAndBlocksAutomaticChoice(string kind)
        {
            var row=Row(kind=="empty"?null:Clip("Manual",.4f),enabled:kind!="disabled");
            row.isRotate=true;row.rotate=25;row.adjustmentClip=Clip("Adjustment",.1f);
            ps.overrideDefines.Add(null);ps.overrideDefines.Add(row);
            var before=ManualJson(ps);
            Assert.That(PosingSystemConverter.ResolveImportedMotionDefinitions(ps).Single(d=>d.stateType==StateType.Jump),Is.SameAs(row));
            Assert.That(ManualJson(ps),Is.EqualTo(before));
        }

        [Test]
        public void LegacyDisabledAlternative_DoesNotHideTheInspectorsEnabledSelection()
        {
            var enabled=Row(clip);ps.overrideDefines.Add(enabled);ps.overrideDefines.Add(Row(Clip("Alternative",.3f),enabled:false));
            Assert.That(PosingSystemConverter.ResolveImportedMotionDefinitions(ps).Single(),Is.SameAs(enabled));
            Assert.That(PosingSystemMotionImportUtility.SelectedIndex(ps,StateType.Jump),Is.Zero);
        }

        [Test]
        public void ClearingManualChoice_ReturnsToAutoAndUndoRestoresAdjustments()
        {
            var row=Row(Clip("Manual",.5f));row.rotate=45;row.isRotate=true;
            var other=Row(clip,StateType.ShortFall);ps.overrideDefines.Add(row);ps.overrideDefines.Add(other);
            Undo.IncrementCurrentGroup();
            try
            {
                PosingSystemMotionImportUtility.ClearManualChoice(ps,StateType.Jump);
                Assert.That(ps.overrideDefines.Single(),Is.SameAs(other));
                Assert.That(PosingSystemConverter.ResolveImportedMotionDefinitions(ps).Single(d=>d.stateType==StateType.Jump).animationClip,Is.SameAs(clip));
                Undo.FlushUndoRecordObjects();Undo.PerformUndo();
                Assert.That(ps.overrideDefines.Count,Is.EqualTo(2));
                Assert.That(ps.overrideDefines[0].rotate,Is.EqualTo(45));
                Assert.That(ps.overrideDefines[0].animationClip,Is.SameAs(row.animationClip));
            }
            finally{Undo.ClearUndo(ps);}
        }

        [Test]
        public void WholeMotion_IsAppliedBeforeIdleOnlyRegardlessOfListOrder()
        {
            var tree=new BlendTree();owned.Add(tree);tree.AddChild(clip,0);
            var idle=Row(clip,StateType.Stand);var whole=Row(tree,StateType.StandWalkRun);
            ps.overrideDefines.Add(idle);ps.overrideDefines.Add(whole);
            var result=PosingSystemConverter.ResolveImportedMotionDefinitions(ps);
            Assert.That(result.IndexOf(whole),Is.LessThan(result.IndexOf(idle)));
        }

        [Test]
        public void SharedAvatar_MergesManualChoicesAndIgnoresInactiveAndNestedAvatars()
        {
            var other=Settings();ps.autoImportAvatarAnimations=false;
            var chosen=Row(Clip("OtherManual",.4f));other.overrideDefines.Add(chosen);
            Assert.That(PosingSystemConverter.ResolveImportedMotionDefinitions(ps).Single(),Is.SameAs(chosen));
            Assert.That(PosingSystemConverter.ResolveImportedMotionDefinitions(other).Single(),Is.SameAs(chosen));
            var prior=Signature();other.gameObject.SetActive(false);
            Assert.That(PosingSystemConverter.ResolveImportedMotionDefinitions(ps),Is.Empty);
            Assert.That(Signature(),Is.Not.EqualTo(prior));
            other.gameObject.SetActive(true);other.gameObject.AddComponent<VRCAvatarDescriptor>();
            Assert.That(PosingSystemConverter.ResolveImportedMotionDefinitions(ps),Is.Empty);
        }

        [TestCase("toggle")] [TestCase("controller")] [TestCase("motion")] [TestCase("curve")]
        [TestCase("state-name")] [TestCase("default")] [TestCase("adjustment")] [TestCase("shared-setting")]
        public void Signature_TracksChangesIncludingUnsavedMotionEdits(string change)
        {
            var other=Settings();var adjustment=Clip("Adjustment",.1f);
            ps.overrideDefines.Add(Row(clip));ps.overrideDefines[0].adjustmentClip=adjustment;
            var before=Signature();Assert.That(Signature(),Is.EqualTo(before));
            switch(change)
            {
                case "toggle":ps.autoImportAvatarAnimations=false;break;
                case "controller":SetSource(AnimatorController.CreateAnimatorControllerAtPath(folder+"/Other.controller"));break;
                case "motion":jump.motion=Clip("Changed",.4f);break;
                case "curve":AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("",typeof(Animator),"Spine Front-Back"),AnimationCurve.Constant(0,1,.8f));break;
                case "state-name":jump.name="Soft Landing";break;
                case "default":SetSource(source,true);break;
                case "adjustment":AnimationUtility.SetEditorCurve(adjustment,EditorCurveBinding.FloatCurve("",typeof(Animator),"Spine Front-Back"),AnimationCurve.Constant(0,1,.7f));break;
                case "shared-setting":other.autoImportAvatarAnimations=false;other.overrideDefines.Add(Row(clip,StateType.ShortFall));break;
            }
            Assert.That(Signature(),Is.Not.EqualTo(before));
        }

        [Test]
        public void Prebuild_RedetectsChangedSourceAndRestoresDefaultWhenDisabled()
        {
            var before=EditorJsonUtility.ToJson(source);var rows=ps.overrideDefines;
            Prebuild();var file=AssetDatabase.GetAssetPath(Output());
            Assert.That(OutputState("Jump").motion,Is.SameAs(clip));
            Assert.That(ps.data,Is.EqualTo(Signature()));
            var next=Clip("NextJump",.5f);jump.motion=next;
            Assert.That(ps.data,Is.Not.EqualTo(Signature()));
            Prebuild();Assert.That(OutputState("Jump").motion,Is.SameAs(next));
            ps.autoImportAvatarAnimations=false;Prebuild();
            Assert.That(OutputState("Jump").motion,Is.Not.SameAs(next));
            Assert.That(OutputState("Jump").motion.name,Does.Not.Contain("NextJump"));
            Assert.That(AssetDatabase.GetAssetPath(Output()),Is.EqualTo(file));
            Assert.That(ps.overrideDefines,Is.SameAs(rows));Assert.That(rows,Is.Empty);
            jump.motion=clip;Assert.That(EditorJsonUtility.ToJson(source),Is.EqualTo(before));
        }

        [Test]
        public void RebuildingEitherProduct_PreservesSharedManualChoiceAndOneGeneratedFile()
        {
            var other=Settings();other.autoImportAvatarAnimations=false;
            var manual=Clip("SecondProductSelection",.35f);other.overrideDefines.Add(Row(manual,StateType.AvatarSelect));
            var before=ManualJson(other);
            void AssertSelectionMotion(PosingSystem target)
            {
                var machine = Output(target).layers.Single(l => l.name == "USSPS_Locomotion").stateMachine;
                Assert.That(machine.defaultState.name, Is.EqualTo("AvatarSelecting"));
                var selections = States(machine).Where(s => s.name == "AvatarSelecting").ToArray();
                Assert.That(selections, Is.Not.Empty);
                Assert.That(selections.All(s => s.motion == manual), Is.True, "Preview and normal selection must share the manual clip");
            }
            Prebuild();
            AssertSelectionMotion(ps);
            var path=AssetDatabase.GetAssetPath(Output());
            PosingSystemEditor.Prebuild(other);
            AssertSelectionMotion(other);
            Assert.That(OutputState("Jump",other).motion,Is.SameAs(clip));
            Assert.That(AssetDatabase.GetAssetPath(Output(other)),Is.EqualTo(path));
            Assert.That(AssetDatabase.FindAssets("t:PosingSystemGeneratedAsset",new[]{folder}),Has.Length.EqualTo(1));
            Assert.That(ManualJson(other),Is.EqualTo(before));Assert.That(ps.overrideDefines,Is.Empty);
        }

        [TestCase(false)] [TestCase(true)]
        public void Prebuild_ManualDisableOrRemovalRestoresDefaultAndClearsMotionTime(bool remove)
        {
            ps.autoImportAvatarAnimations=false;
            var row=Row(Clip("ManualJump",.4f));row.isMotionTime=true;row.motionTimeParamName="ImportedTime";
            row.isRotate=true;row.rotate=30;ps.overrideDefines.Add(row);
            var before=ManualJson(ps);Prebuild();
            Assert.That(OutputState("Jump").timeParameterActive,Is.True);
            Assert.That(AnimationUtility.GetAnimationClipSettings((AnimationClip)OutputState("Jump").motion).orientationOffsetY,Is.EqualTo(30));
            Assert.That(ManualJson(ps),Is.EqualTo(before));
            if(remove)ps.overrideDefines.Clear();else row.enabled=false;
            Prebuild();Assert.That(OutputState("Jump").timeParameterActive,Is.False);
            Assert.That(OutputState("Jump").motion.name,Does.Not.Contain("ManualJump"));
            if(!remove)Assert.That(row.rotate,Is.EqualTo(30));
        }

        [TestCase(false, false, false)] [TestCase(true, false, false)]
        [TestCase(false, true, false)] [TestCase(true, true, false)]
        [TestCase(false, false, true)] [TestCase(true, false, true)]
        [TestCase(false, true, true)] [TestCase(true, true, true)]
        public void NdmfBuild_UsesAutomaticMotionWithoutChangingSourceOrManualSettings(bool prebuilt, bool defaultBase, bool manual)
        {
            if (defaultBase) SetSource(null, true);
            if (manual) ps.overrideDefines.Add(Row(Clip("ManualLanding", .3f), StateType.ShortLanding));
            if(prebuilt)Prebuild();
            var files=AssetDatabase.FindAssets("t:PosingSystemGeneratedAsset",new[]{folder}).OrderBy(x=>x).ToArray();
            var before=EditorJsonUtility.ToJson(source);var originalRows=ps.overrideDefines;
            var rowsBefore = ManualJson(ps);
            var originalComponents = root.GetComponentsInChildren<Component>(true);
            var settingsBefore = originalComponents.Where(c => c != null).ToDictionary(c => c, c => EditorJsonUtility.ToJson(c));
            var dirtyBefore = originalComponents.Where(c => c != null).ToDictionary(c => c, EditorUtility.IsDirty);
            var sourceControllers = root.GetComponentsInChildren<ModularAvatarMergeAnimator>(true)
                .Select(m => (Object)m.animator).Where(o => o != null).Concat(new Object[] { source });
            var sourceGraph = PosingSystemControllerGraph.Collect(sourceControllers, PosingSystemControllerGraph.IsGraphObject)
                .ToDictionary(o => o, o => EditorJsonUtility.ToJson(o));
            var clone=Object.Instantiate(root);owned.Add(clone);
            clone.name = root.name;
            var cloneAvatar=clone.GetComponent<VRCAvatarDescriptor>();
            cloneAvatar.customExpressions=true;
            var parameters=ScriptableObject.CreateInstance<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters>();owned.Add(parameters);
            parameters.parameters=Array.Empty<VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionParameters.Parameter>();
            cloneAvatar.expressionParameters=parameters;
            var context=new BuildContext(clone,null,false);
            var messages = new List<string>();
            void OnLog(string message, string stack, LogType type) => messages.Add(message);
            Application.logMessageReceived += OnLog;
            try
            {
                var method=typeof(AvatarProcessor).GetMethod("ProcessAvatar",BindingFlags.Static|BindingFlags.NonPublic,null,new[]{typeof(BuildContext),typeof(BuildPhase),typeof(BuildPhase)},null);
                try{method.Invoke(null,new object[]{context,BuildPhase.Resolving,BuildPhase.Transforming});}
                catch(TargetInvocationException ex){throw ex.InnerException??ex;}
                var output=(AnimatorController)cloneAvatar.baseAnimationLayers.Single(l=>l.type==VRCAvatarDescriptor.AnimLayerType.Base).animatorController;
                var imported=output.layers.Where(l=>l.name.Contains("USSPS_Locomotion")).SelectMany(l=>States(l.stateMachine))
                    .Where(s=>s.name=="Jump").Select(s=>s.motion).ToArray();
                Assert.That(context.Successful, Is.True);
                Assert.That(imported.Any(m=>m!=null&&m.name.Contains("AutoJump")),Is.EqualTo(!defaultBase));
                if (manual)
                    Assert.That(output.layers.Where(l => l.name.Contains("USSPS_Locomotion")).SelectMany(l => States(l.stateMachine))
                        .Any(s => s.motion != null && s.motion.name.Contains("ManualLanding")), Is.True);
                Assert.That(messages.Any(m => m.Contains("再度プレビルド")), Is.False, string.Join("\n", messages));
                Assert.That(ps.overrideDefines,Is.SameAs(originalRows));
                Assert.That(ManualJson(ps), Is.EqualTo(rowsBefore));
                Assert.That(EditorJsonUtility.ToJson(source),Is.EqualTo(before));
                Assert.That(root.GetComponentsInChildren<Component>(true), Is.EqualTo(originalComponents));
                foreach (var entry in settingsBefore) Assert.That(EditorJsonUtility.ToJson(entry.Key), Is.EqualTo(entry.Value), entry.Key.name);
                foreach (var entry in dirtyBefore) Assert.That(EditorUtility.IsDirty(entry.Key), Is.EqualTo(entry.Value), entry.Key.name);
                foreach (var entry in sourceGraph) Assert.That(EditorJsonUtility.ToJson(entry.Key), Is.EqualTo(entry.Value), entry.Key.name);
                Assert.That(AssetDatabase.FindAssets("t:PosingSystemGeneratedAsset",new[]{folder}).OrderBy(x=>x),Is.EqualTo(files));
                foreach(var layer in cloneAvatar.baseAnimationLayers)
                    if(layer.animatorController!=null)owned.UnionWith(PosingSystemControllerGraph.Collect(new Object[]{layer.animatorController},o=>PosingSystemControllerGraph.IsGraphObject(o)&&!EditorUtility.IsPersistent(o)));
            }
            finally{Application.logMessageReceived -= OnLog;context.DeactivateAllExtensionContexts();}
        }

        [TestCase(false)] [TestCase(true)]
        public void BuildInputSnapshot_UsesOriginalInputAndRestoresLiveLookup(bool defaultBase)
        {
            if (defaultBase) SetSource(null, true);
            Prebuild();
            var before = Signature();
            var rows = ManualJson(ps);
            var state = new PosingSystemConverter.AutoImportBuildState();
            state.Capture(root);
            var replacement = AnimatorController.CreateAnimatorControllerAtPath(folder + "/BuildOnly.controller");
            var replacementClip = Clip("BuildOnlyJump", .6f);
            replacement.layers[0].stateMachine.AddState("Jump").motion = replacementClip;
            SetSource(replacement);
            using (new PosingSystemConverter.AutoImportBuildScope(state))
            {
                Assert.That(Signature(), Is.EqualTo(before));
                Assert.That(PosingSystemConverter.ShouldReportPrebuildUpdateWarning(ps), Is.False);
                var detected = PosingSystemConverter.AutoImportAvatarAnimations(ps);
                if (defaultBase) Assert.That(detected, Is.Empty);
                else Assert.That(detected.Single().animationClip, Is.SameAs(clip));
                Assert.That(ManualJson(ps), Is.EqualTo(rows));
            }
            Assert.That(PosingSystemConverter.AutoImportAvatarAnimations(ps).Single().animationClip, Is.SameAs(replacementClip));
            Assert.That(PosingSystemConverter.ShouldReportPrebuildUpdateWarning(ps), Is.True);
        }

        [Test]
        public void BuildInputSnapshot_PreservesRealInputChangesAndDoesNotLeakOnException()
        {
            Prebuild();
            jump.motion = Clip("ChangedBeforeBuild", .8f);
            var state = new PosingSystemConverter.AutoImportBuildState();
            state.Capture(root);
            Assert.Throws<InvalidOperationException>(() =>
            {
                using var scope = new PosingSystemConverter.AutoImportBuildScope(state);
                Assert.That(PosingSystemConverter.ShouldReportPrebuildUpdateWarning(ps), Is.True);
                throw new InvalidOperationException("Simulated build failure");
            });
            SetSource(null, true);
            Assert.That(PosingSystemConverter.AutoImportAvatarAnimations(ps), Is.Empty);
            state.Capture(root);
            using (new PosingSystemConverter.AutoImportBuildScope(state))
                Assert.That(PosingSystemConverter.AutoImportAvatarAnimations(ps), Is.Empty);
        }
    }
}
