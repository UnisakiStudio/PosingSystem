using System;
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemGeneratedControllerTests
    {
        string folder;
        Scene originalScene, scene;
        bool savedEmptyRunnerScene;
        readonly List<Scene> scenes = new();
        readonly HashSet<Object> memory = new();
        AnimatorController template;
        AnimationClip clip;
        GameObject avatar;
        PosingSystem ps;
        ModularAvatarMergeAnimator merge;

        [SetUp]
        public void SetUp()
        {
            PosingSystemGeneratedCleanup.Suspend = true;
            folder = "Assets/USSPS_ManagementTests_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", folder.Substring(7));
            PosingSystemGeneratedControllers.OutputRootOverride = folder + "/Generated";
            originalScene = SceneManager.GetActiveScene();
            // Test Framework creates an empty untitled additive scene. Unity rejects another
            // additive scene until it has a path. Only persist this provably empty test context.
            savedEmptyRunnerScene = false;
            if (string.IsNullOrEmpty(originalScene.path))
            {
                Assert.That(originalScene.GetRootGameObjects().All(r =>
                    r.name == "nadena.dev.ndmf__Activator" && r.transform.childCount == 0 &&
                    r.GetComponents<Component>().All(c => c is Transform ||
                        (c != null && c.GetType().FullName == "nadena.dev.ndmf.runtime.ApplyOnPlayGlobalActivator"))),
                    Is.True, "Do not save an untitled scene containing user objects");
                Assert.That(EditorSceneManager.SaveScene(originalScene, folder + "/RunnerContext.unity"), Is.True);
                savedEmptyRunnerScene = true;
            }
            scene = NewScene("A");
            clip = new AnimationClip { name = "Source" };
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.x"), AnimationCurve.Constant(0, 1, .25f));
            AssetDatabase.CreateAsset(clip, folder + "/Source.anim");
            template = AnimatorController.CreateAnimatorControllerAtPath(folder + "/Template.controller");
            template.layers[0].stateMachine.AddState("Idle").motion = clip;
            template.AddParameter(new AnimatorControllerParameter { name="Input", type=AnimatorControllerParameterType.Float, defaultFloat=.37f });
            template.AddLayer("USSPS_IsDefault");
            AssetDatabase.SaveAssetIfDirty(template);
            avatar = NewAvatar("Avatar", out ps, out merge);
            EditorSceneManager.SaveScene(scene);
        }

        Scene NewScene(string name)
        {
            var result = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            scenes.Add(result); SceneManager.SetActiveScene(result);
            new GameObject("Test Camera").AddComponent<Camera>();
            new GameObject("Test Light").AddComponent<Light>().type = LightType.Directional;
            EditorSceneManager.SaveScene(result, folder + "/" + name + ".unity");
            return result;
        }

        GameObject NewAvatar(string name, out PosingSystem system, out ModularAvatarMergeAnimator animatorMerge)
        {
            var root = new GameObject(name); root.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
            system = root.AddComponent<PosingSystem>();
            system.features = new PosingSystemFeatureSettings { breathing=false };
            system.autoImportAvatarAnimations = false;
            root.AddComponent<DuplicateEraser>().ID = "jp.unisakistudio.posingsystem_locomotion";
            animatorMerge = root.AddComponent<ModularAvatarMergeAnimator>(); animatorMerge.animator = template;
            return root;
        }

        [TearDown]
        public void TearDown()
        {
            PosingSystemGeneratedControllers.OutputRootOverride = null;
            foreach (var path in AssetDatabase.FindAssets("", new[] { folder }).Select(AssetDatabase.GUIDToAssetPath).Distinct().Where(p => !p.EndsWith(".unity") && !AssetDatabase.IsValidFolder(p)))
                foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path)) if (obj != null) Undo.ClearUndo(obj);
            foreach (var current in scenes.AsEnumerable().Reverse())
                if (current.IsValid() && current.isLoaded) EditorSceneManager.CloseScene(current, true);
            scenes.Clear();
            if (savedEmptyRunnerScene && originalScene.IsValid() && originalScene.isLoaded)
            {
                var replacement = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
                EditorSceneManager.CloseScene(originalScene, true);
                SceneManager.SetActiveScene(replacement);
            }
            else if (originalScene.IsValid() && originalScene.isLoaded) SceneManager.SetActiveScene(originalScene);
            savedEmptyRunnerScene = false;
            foreach (var obj in memory.Reverse()) if (obj != null && !EditorUtility.IsPersistent(obj)) Object.DestroyImmediate(obj);
            memory.Clear();
            if (AssetDatabase.IsValidFolder(folder)) AssetDatabase.DeleteAsset(folder);
            PosingSystemGeneratedCleanup.Suspend = false;
        }

        AnimatorController Generate(PosingSystem system = null, string marker = null, IAssetSaver saver = null)
        {
            system ??= ps;
            PosingSystemGeneratedControllers.Convert(system, saver, output => {
                PosingSystemConverter.CreateOriginalAnimatorController(system);
                var controller = (AnimatorController)PosingSystemConverter.GetCommonMergeAnimator(system).animator;
                if (marker != null) controller.AddParameter(marker, AnimatorControllerParameterType.Bool);
            });
            return (AnimatorController)PosingSystemConverter.GetCommonMergeAnimator(system).animator;
        }

        string[] GeneratedFiles() => AssetDatabase.FindAssets("t:PosingSystemGeneratedAsset", new[] { folder })
            .Select(AssetDatabase.GUIDToAssetPath).Distinct().ToArray();

        [Test]
        public void FirstAndRepeatedGeneration_UsesOneSceneOwnedFileAndStableMainReference()
        {
            var first = Generate(marker:"First"); var path = AssetDatabase.GetAssetPath(first);
            Assert.That(path, Does.Contain("/Scenes/" + AssetDatabase.AssetPathToGUID(scene.path) + "/"));
            Assert.That(first.parameters.Single(p=>p.name=="Input").defaultFloat, Is.EqualTo(.37f));
            Assert.That(first.layers.Any(l=>l.name=="USSPS_IsDefault"), Is.False);
            Assert.That(Generate(marker:"Second"), Is.SameAs(first));
            Assert.That(Generate(), Is.SameAs(first));
            Assert.That(GeneratedFiles(), Has.Length.EqualTo(1));
            Assert.That(first.parameters.Any(p=>p.name=="First"), Is.True);
            Assert.That(first.parameters.Any(p=>p.name=="Second"), Is.True);
            Assert.That(template.parameters.Any(p=>p.name=="First"), Is.False);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DuplicateAvatar_DetachesBeforeEitherOwnerIsEdited(bool editOriginal)
        {
            var old = Generate();
            var duplicate = Object.Instantiate(avatar); duplicate.name = avatar.name;
            var duplicatePs = duplicate.GetComponent<PosingSystem>();
            var edited = Generate(editOriginal ? ps : duplicatePs, "Independent");
            var untouched = (AnimatorController)(editOriginal ? duplicate.GetComponent<ModularAvatarMergeAnimator>() : merge).animator;
            Assert.That(edited, Is.Not.SameAs(untouched));
            Assert.That(untouched, Is.SameAs(old));
            Assert.That(untouched.parameters.Any(p=>p.name=="Independent"), Is.False);
            Assert.That(GeneratedFiles(), Has.Length.EqualTo(2));
        }

        [Test]
        public void SameNamesAndMultipleProducts_AreSeparatedPerAvatarAndSharedWithinAvatar()
        {
            var first = Generate();
            NewAvatar("Avatar", out var other, out _); var second = Generate(other);
            Assert.That(second, Is.Not.SameAs(first));
            var child = new GameObject("Other product"); child.transform.SetParent(avatar.transform);
            var childPs = child.AddComponent<PosingSystem>(); childPs.features = new PosingSystemFeatureSettings {breathing=false};
            child.AddComponent<DuplicateEraser>().ID = "jp.unisakistudio.posingsystem_locomotion";
            var childMerge = child.AddComponent<ModularAvatarMergeAnimator>(); childMerge.animator=template;
            Assert.That(Generate(childPs), Is.SameAs(first));
            Assert.That(childMerge.animator, Is.SameAs(merge.animator));
            Assert.That(GeneratedFiles(), Has.Length.EqualTo(2));
        }

        [TestCase("avatar-rename")]
        [TestCase("avatar-reparent")]
        [TestCase("scene-rename")]
        [TestCase("scene-move")]
        public void RenamingOrMovingWithoutChangingSceneGuid_ReusesOwner(string operation)
        {
            var first=Generate(); var guid=AssetDatabase.AssetPathToGUID(scene.path);
            if(operation=="avatar-rename") avatar.name="Renamed";
            if(operation=="avatar-reparent") avatar.transform.SetParent(new GameObject("Parent").transform);
            if(operation=="scene-rename") Assert.That(AssetDatabase.MoveAsset(scene.path,folder+"/Renamed.unity"),Is.Empty);
            if(operation=="scene-move") {
                AssetDatabase.CreateFolder(folder,"Moved");
                Assert.That(AssetDatabase.MoveAsset(scene.path,folder+"/Moved/A.unity"),Is.Empty);
            }
            Assert.That(Generate(),Is.SameAs(first));
            Assert.That(PosingSystemGeneratedControllers.Metadata(first).ownerGuid,Is.EqualTo(guid));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SceneSaveAsOrCopy_AllocatesIndependentOwner(bool fileCopy)
        {
            var first=Generate(); EditorSceneManager.SaveScene(scene);
            var oldPath=scene.path; var newPath=folder+"/Copy.unity";
            if(fileCopy) {
                Assert.That(AssetDatabase.CopyAsset(oldPath,newPath),Is.True);
                var copyScene=EditorSceneManager.OpenScene(newPath,OpenSceneMode.Additive);scenes.Add(copyScene);
                var copyPs=copyScene.GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<PosingSystem>()).Single();
                Assert.That(Generate(copyPs,"Copy"),Is.Not.SameAs(first));
            } else {
                EditorSceneManager.SaveScene(scene,newPath);
                Assert.That(Generate(marker:"Copy"),Is.Not.SameAs(first));
            }
            Assert.That(first.parameters.Any(p=>p.name=="Copy"),Is.False);
            Assert.That(AssetDatabase.LoadAssetAtPath<SceneAsset>(oldPath),Is.Not.Null);
        }

        [Test]
        public void MoveAvatarBetweenLoadedScenes_DetachesWithoutChangingOldFile()
        {
            var first=Generate();EditorSceneManager.SaveScene(scene);
            var secondScene=NewScene("B"); SceneManager.MoveGameObjectToScene(avatar,secondScene);
            var second=Generate(marker:"Moved");
            Assert.That(second,Is.Not.SameAs(first));
            Assert.That(AssetDatabase.GetAssetPath(second),Does.Contain(AssetDatabase.AssetPathToGUID(secondScene.path)));
            Assert.That(first.parameters.Any(p=>p.name=="Moved"),Is.False);
        }

        [Test]
        public void UnsavedScene_CanGenerateAndRetainOwnerWithinSession()
        {
            var unsaved=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);scenes.Add(unsaved);
            SceneManager.MoveGameObjectToScene(avatar,unsaved);
            var first=Generate();Assert.That(AssetDatabase.GetAssetPath(first),Does.Contain("/Unsaved/"));
            Assert.That(Generate(),Is.SameAs(first));
            Assert.That(PosingSystemGeneratedCleanup.GuardReason(),Is.Not.Null);
        }

        [Test]
        public void FirstSceneSave_MovesFileAndPreservesAllObjectReferences()
        {
            var unsaved=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);scenes.Add(unsaved);
            SceneManager.MoveGameObjectToScene(avatar,unsaved);var first=Generate();
            var guid=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(first));
            // Exercise the actual sceneSaved handler while automatic destructive cleanup remains blocked by Undo/dirty scenes.
            PosingSystemGeneratedCleanup.Suspend=false;
            try { EditorSceneManager.SaveScene(unsaved,folder+"/FirstSave.unity"); }
            finally { PosingSystemGeneratedCleanup.Suspend=true; }
            Assert.That(AssetDatabase.GetAssetPath(first),Does.Contain("/Scenes/"));
            Assert.That(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(first)),Is.EqualTo(guid));
            Assert.That(Generate(),Is.SameAs(first));
        }

        [TestCase("normal")]
        [TestCase("variant")]
        [TestCase("nested")]
        public void PrefabInstances_DoNotWriteToPrefabOrOtherInstance(string kind)
        {
            Generate();EditorSceneManager.SaveScene(scene);
            var path=folder+"/Avatar.prefab";PrefabUtility.SaveAsPrefabAsset(avatar,path);
            if(kind=="variant") {
                var variant=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path),scene);
                path=folder+"/Variant.prefab";PrefabUtility.SaveAsPrefabAsset(variant,path);Object.DestroyImmediate(variant);
            }
            if(kind=="nested") {
                var wrapper=new GameObject("Wrapper");
                var nested=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path),wrapper.transform);
                path=folder+"/Nested.prefab";PrefabUtility.SaveAsPrefabAsset(wrapper,path);Object.DestroyImmediate(wrapper);
            }
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
            var instancePs=instance.GetComponentInChildren<PosingSystem>();
            var source=prefab.GetComponentInChildren<ModularAvatarMergeAnimator>().animator;
            var generated=Generate(instancePs,"InstanceOnly");
            Assert.That(generated,Is.Not.SameAs(source));
            Assert.That(prefab.GetComponentInChildren<ModularAvatarMergeAnimator>().animator,Is.SameAs(source));
            EditorSceneManager.SaveScene(scene);
            Assert.That(PrefabUtility.HasPrefabInstanceAnyOverrides(instance,false),Is.True);
            Assert.That(((AnimatorController)source).parameters.Any(p=>p.name=="InstanceOnly"),Is.False);
        }

        [TestCase(PrefabUnpackMode.OutermostRoot)]
        [TestCase(PrefabUnpackMode.Completely)]
        public void PrefabUnpack_PreservesInstanceController(PrefabUnpackMode mode)
        {
            var path=folder+"/Avatar.prefab";PrefabUtility.SaveAsPrefabAsset(avatar,path);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(path),scene);
            var instancePs=instance.GetComponent<PosingSystem>();var first=Generate(instancePs);
            PrefabUtility.UnpackPrefabInstance(instance,mode,InteractionMode.AutomatedAction);
            Assert.That(Generate(instancePs),Is.SameAs(first));
        }

        [Test]
        public void FailedConversion_PreservesPreviousControllerAndAllocatesNoFile()
        {
            var first=Generate();var before=EditorJsonUtility.ToJson(first);var count=GeneratedFiles().Length;
            Assert.Throws<InvalidOperationException>(()=>PosingSystemGeneratedControllers.Convert(ps,null,_=> {
                ((AnimatorController)merge.animator).AddParameter("NeverCommit",AnimatorControllerParameterType.Bool);
                throw new InvalidOperationException("test failure");
            }));
            Assert.That(merge.animator,Is.SameAs(first));
            Assert.That(EditorJsonUtility.ToJson(first),Is.EqualTo(before));
            Assert.That(GeneratedFiles(),Has.Length.EqualTo(count));
            Assert.That(PosingSystemGeneratedControllers.Current,Is.Null);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void BuildWithOrWithoutPrebuild_DoesNotCreatePermanentFileOrMutateSource(bool prebuilt)
        {
            if(prebuilt)Generate();var source=(AnimatorController)merge.animator;
            var before=EditorJsonUtility.ToJson(source);var paths=GeneratedFiles();
            var clone=Object.Instantiate(avatar);memory.Add(clone);
            var output=Generate(clone.GetComponent<PosingSystem>(),"BuildOnly",new NullAssetSaver());
            memory.UnionWith(PosingSystemControllerGraph.Collect(new[]{output},o=>PosingSystemControllerGraph.IsGraphObject(o)&&!EditorUtility.IsPersistent(o)));
            Assert.That(output,Is.Not.SameAs(source));Assert.That(output,Is.Not.Null);
            Assert.That(output.parameters.Any(p=>p.name=="BuildOnly"),Is.True);
            Assert.That(EditorJsonUtility.ToJson(source),Is.EqualTo(before));
            Assert.That(GeneratedFiles(),Is.EquivalentTo(paths));
        }

        [Test]
        public void FeatureAndBreathingControllers_AreInOneFile()
        {
            ps.features.breathing=true;
            PosingSystemGeneratedControllers.Convert(ps,null,saver=> {
                PosingSystemConverter.CreateOriginalAnimatorController(ps);
                PosingSystemConverter.CreateOriginalAdditiveBreathingAnimatorController(ps);
                var child=new GameObject("Extra feature");child.transform.SetParent(avatar.transform);
                var extra=new AnimatorController {name="Extra"};extra.AddLayer("Extra");saver.SaveAsset(extra);
                child.AddComponent<ModularAvatarMergeAnimator>().animator=extra;
            });
            var controllers=avatar.GetComponentsInChildren<ModularAvatarMergeAnimator>().Select(m=>m.animator).ToArray();
            Assert.That(controllers.Length,Is.GreaterThanOrEqualTo(3));
            Assert.That(controllers.Select(AssetDatabase.GetAssetPath).Distinct().Count(),Is.EqualTo(1));
            Assert.That(GeneratedFiles(),Has.Length.EqualTo(1));
            var path=AssetDatabase.GetAssetPath(merge.animator);AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
            Assert.That(controllers.All(c=>c!=null),Is.True);
        }

        [TestCase(false)] [TestCase(true)]
        public void ReenabledBreathing_ReusesExcludedComponentAndClonesSourceBeforeConversion(bool inactive)
        {
            ps.features.breathing = true;
            avatar.AddComponent<ModularAvatarParameters>();
            var source = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(
                PosingSystemConverter.DefaultAdditiveBreathingControllerGuid));
            var sourceBefore = EditorJsonUtility.ToJson(source);
            var child = new GameObject("Disabled breathing"); child.transform.SetParent(avatar.transform, false);
            child.AddComponent<DuplicateEraser>().ID = PosingSystemConverter.AdditiveBreathingDuplicateEraserId;
            var breathing = child.AddComponent<ModularAvatarMergeAnimator>(); breathing.animator = source;
            child.tag = "EditorOnly"; child.SetActive(!inactive);
            PosingSystemGeneratedControllers.Convert(ps, null, saver =>
            {
                PosingSystemFeatureUtility.Synchronize(ps);
                PosingSystemConverter.CreateOriginalAdditiveBreathingAnimatorController(ps);
                var actual = PosingSystemConverter.GetAdditiveBreathingMergeAnimator(ps);
                Assert.That(actual, Is.SameAs(breathing), "Reuse the tool's existing excluded component");
                Assert.That(saver.IsTemporaryAsset(actual.animator), Is.True);
                Assert.That(actual.animator, Is.Not.SameAs(source));
            });
            Assert.That(ps.GetComponentsInChildren<DuplicateEraser>(true).Count(e =>
                e.ID == PosingSystemConverter.AdditiveBreathingDuplicateEraserId), Is.EqualTo(1));
            Assert.That(EditorJsonUtility.ToJson(source), Is.EqualTo(sourceBefore));
            Assert.That(AssetDatabase.GetAssetPath(breathing.animator), Is.EqualTo(AssetDatabase.GetAssetPath(merge.animator)));
        }

        [Test]
        public void Clone_PreservesTransitionsSyncedOverridesAndSharedNestedTrees()
        {
            var main=template.layers[0].stateMachine;
            var nested=new BlendTree {name="Nested",blendParameter="Input"};nested.AddChild(clip,0);AssetDatabase.AddObjectToAsset(nested,template);
            var idle=main.states[0].state;idle.motion=nested;
            var child=main.AddStateMachine("Child");var target=child.AddState("Target");target.motion=nested;
            var transition=idle.AddTransition(target);transition.duration=.23f;transition.hasExitTime=false;
            var layers=template.layers;layers[1].syncedLayerIndex=0;layers[1].syncedLayerAffectsTiming=true;
            layers[1].SetOverrideMotion(idle,clip);template.layers=layers;
            var map=PosingSystemControllerGraph.Clone(new[]{template});memory.UnionWith(map.Values);
            var copy=(AnimatorController)map[template];var copiedIdle=(AnimatorState)map[idle];
            Assert.That(copiedIdle.transitions[0].destinationState,Is.SameAs(map[target]));
            Assert.That(copiedIdle.transitions[0].duration,Is.EqualTo(.23f));
            Assert.That(copiedIdle.motion,Is.SameAs(((AnimatorState)map[target]).motion));
            Assert.That(copiedIdle.motion,Is.Not.SameAs(nested));
            Assert.That(copy.layers[1].syncedLayerAffectsTiming,Is.True);
            Assert.That(copy.layers[1].GetOverrideMotion(copiedIdle),Is.SameAs(clip));
        }

        [Test]
        public void Cleanup_ProtectsUndoAndDirtyScenes_AndNeverIncludesLegacyFiles()
        {
            Generate();Assert.That(PosingSystemGeneratedCleanup.GuardReason(),Is.Not.Null);
            var candidates=PosingSystemGeneratedCleanup.Candidates(AssetDatabase.AssetPathToGUID(scene.path));
            Assert.That(candidates,Has.Length.EqualTo(1));
            Assert.That(candidates,Does.Not.Contain(AssetDatabase.GetAssetPath(template)));
        }

        [Test]
        public void Cleanup_DoesNotInspectControllerContentsFromOtherScenes()
        {
            var controller = Generate();
            var path = AssetDatabase.GetAssetPath(controller);
            // Duplicate metadata makes inspecting this file fail. Only its owner should inspect it.
            var duplicate = ScriptableObject.CreateInstance<PosingSystemGeneratedAsset>();
            AssetDatabase.AddObjectToAsset(duplicate, path);
            AssetDatabase.SaveAssetIfDirty(duplicate);
            var ownerGuid = AssetDatabase.AssetPathToGUID(scene.path);
            Assert.Throws<InvalidOperationException>(() => PosingSystemGeneratedCleanup.Candidates(ownerGuid));

            var other = NewScene("Other");
            Assert.That(PosingSystemGeneratedCleanup.Candidates(AssetDatabase.AssetPathToGUID(other.path)), Is.Empty);
        }

        [TestCase("scene")]
        [TestCase("prefab")]
        [TestCase("inactive")]
        [TestCase("unsaved-copy")]
        public void Cleanup_PreservesEveryKindOfExternalReference(string kind)
        {
            var first=Generate();var path=AssetDatabase.GetAssetPath(first);
            EditorSceneManager.SaveScene(scene);
            if(kind=="scene") {
                var other=NewScene("External");var copy=Object.Instantiate(avatar);SceneManager.MoveGameObjectToScene(copy,other);
                EditorSceneManager.SaveScene(other);EditorSceneManager.CloseScene(other,true);
                Object.DestroyImmediate(avatar);EditorSceneManager.SaveScene(scene);
            }
            if(kind=="prefab") {PrefabUtility.SaveAsPrefabAsset(avatar,folder+"/External.prefab");Object.DestroyImmediate(avatar);EditorSceneManager.SaveScene(scene);}
            if(kind=="inactive") {avatar.SetActive(false);EditorSceneManager.SaveScene(scene);}
            if(kind=="unsaved-copy") {var copy=Object.Instantiate(avatar);Object.DestroyImmediate(avatar);EditorSceneManager.SaveScene(scene);copy.name="Unsaved copy";}
            var decision=PosingSystemGeneratedCleanup.Analyze(scene.path,new[]{path}).Single();
            Assert.That(decision.DeleteFile,Is.False);
            Assert.That(AssetDatabase.LoadMainAssetAtPath(path),Is.Not.Null);
        }

        [Test]
        public void Cleanup_UnusedOwnedFileIsIdentified_AndMovedToTrashOnlyByExplicitTestApply()
        {
            var first=Generate();var path=AssetDatabase.GetAssetPath(first);
            Object.DestroyImmediate(avatar);EditorSceneManager.SaveScene(scene);
            var decision=PosingSystemGeneratedCleanup.Analyze(scene.path,new[]{path}).Single();
            Assert.That(decision.DeleteFile,Is.True);
            Assert.That(path,Does.StartWith(folder+"/"));
            // Only this test's file. The real scheduler additionally requires empty global Undo/Redo.
            PosingSystemGeneratedCleanup.Apply(decision);
            Assert.That(AssetDatabase.LoadMainAssetAtPath(path),Is.Null);
        }

        [Test]
        public void Cleanup_InvalidOwnerOrUnmanagedPathIsRejected()
        {
            Assert.Throws<InvalidOperationException>(()=>PosingSystemGeneratedCleanup.Analyze(scene.path,new[]{AssetDatabase.GetAssetPath(template)}));
            var first=Generate();var data=PosingSystemGeneratedControllers.Metadata(first);data.ownerGuid="unknown";
            Assert.That(PosingSystemGeneratedCleanup.Candidates(AssetDatabase.AssetPathToGUID(scene.path)),Is.Empty);
        }

        [Test]
        public void SavedSceneReload_RetainsOwnershipAndControllerReference()
        {
            var first=Generate();var assetPath=AssetDatabase.GetAssetPath(first);var scenePath=scene.path;
            EditorSceneManager.SaveScene(scene);EditorSceneManager.CloseScene(scene,true);
            scene=EditorSceneManager.OpenScene(scenePath,OpenSceneMode.Additive);scenes.Add(scene);
            avatar=scene.GetRootGameObjects().Single(r=>r.GetComponent<PosingSystem>()!=null);
            ps=avatar.GetComponent<PosingSystem>();merge=avatar.GetComponent<ModularAvatarMergeAnimator>();
            Assert.That(AssetDatabase.GetAssetPath(Generate()),Is.EqualTo(assetPath));
            Assert.That(GeneratedFiles(),Has.Length.EqualTo(1));
        }

        [Test]
        public void PrefabApplyThenRevert_ProtectsAppliedAssetAndRestoresValidReference()
        {
            var path=folder+"/Apply.prefab";PrefabUtility.SaveAsPrefabAsset(avatar,path);
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
            var instancePs=instance.GetComponent<PosingSystem>();var applied=Generate(instancePs,"Applied");
            PrefabUtility.ApplyPrefabInstance(instance,InteractionMode.AutomatedAction);
            Assert.That(prefab.GetComponent<ModularAvatarMergeAnimator>().animator,Is.SameAs(applied));
            var changed=Generate(instancePs,"AfterApply");
            Assert.That(changed,Is.Not.SameAs(applied));
            Assert.That(applied.parameters.Any(p=>p.name=="AfterApply"),Is.False);
            PrefabUtility.RevertPrefabInstance(instance,InteractionMode.AutomatedAction);
            instancePs=instance.GetComponent<PosingSystem>();
            Assert.That(instance.GetComponent<ModularAvatarMergeAnimator>().animator,Is.SameAs(applied));
            Assert.That(Generate(instancePs),Is.Not.SameAs(applied));
        }

        [Test]
        public void DeleteAvatarUndoRedo_RestoresValidControllerAndCleanupRemainsBlocked()
        {
            var controller=Generate();EditorSceneManager.SaveScene(scene);
            Undo.IncrementCurrentGroup();Undo.DestroyObjectImmediate(avatar);Undo.FlushUndoRecordObjects();
            Assert.That(PosingSystemGeneratedCleanup.GuardReason(),Is.Not.Null);
            Undo.PerformUndo();
            avatar=scene.GetRootGameObjects().Single(r=>r.GetComponent<PosingSystem>()!=null);
            Assert.That(avatar.GetComponent<ModularAvatarMergeAnimator>().animator,Is.SameAs(controller));
            Undo.PerformRedo();
            Assert.That(controller,Is.Not.Null);Assert.That(PosingSystemGeneratedCleanup.GuardReason(),Is.Not.Null);
            Undo.PerformUndo();
        }

        [Test]
        public void NonMaAnimatorReference_AlsoForcesCopyBeforeWrite()
        {
            var first=Generate();var external=new GameObject("Manual user").AddComponent<Animator>();
            external.runtimeAnimatorController=first;
            Assert.That(Generate(marker:"Independent"),Is.Not.SameAs(first));
            Assert.That(external.runtimeAnimatorController,Is.SameAs(first));
            Assert.That(first.parameters.Any(p=>p.name=="Independent"),Is.False);
        }

        [Test]
        public void PersistentBuildSaver_ContainsCompleteGraphWithoutEditorGeneratedFile()
        {
            var container=new AvatarMask();AssetDatabase.CreateAsset(container,folder+"/Build.asset");
            using(var saver=new SingleAssetSaver(container))
            {
                var result=Generate(marker:"Build",saver:saver);
                Assert.That(AssetDatabase.GetAssetPath(result),Is.EqualTo(folder+"/Build.asset"));
                Assert.That(AssetDatabase.GetAssetPath(result.layers[0].stateMachine),Is.EqualTo(folder+"/Build.asset"));
                Assert.That(GeneratedFiles(),Is.Empty);
            }
        }

        [Test]
        public void OwnedClipsAndStateBehaviours_AreDeepCopied()
        {
            var ownClip=new AnimationClip {name="Owned"};AssetDatabase.AddObjectToAsset(ownClip,template);
            var state=template.layers[0].stateMachine.states[0].state;state.motion=ownClip;
            var behaviour=state.AddStateMachineBehaviour<VRC.SDK3.Avatars.Components.VRCAnimatorTrackingControl>();
            behaviour.trackingHead=VRC.SDK3.Avatars.Components.VRCAnimatorTrackingControl.TrackingType.Animation;
            var map=PosingSystemControllerGraph.Clone(new[]{template});memory.UnionWith(map.Values);
            var result=(AnimatorState)map[state];
            Assert.That(result.motion,Is.Not.SameAs(ownClip));
            Assert.That(result.behaviours.Single(),Is.Not.SameAs(behaviour));
            Assert.That(((VRC.SDK3.Avatars.Components.VRCAnimatorTrackingControl)result.behaviours.Single()).trackingHead,
                Is.EqualTo(VRC.SDK3.Avatars.Components.VRCAnimatorTrackingControl.TrackingType.Animation));
        }

        [Test]
        public void Cleanup_PreservesManuallyAddedSubassetsEvenWithoutReferences()
        {
            var first=Generate();var path=AssetDatabase.GetAssetPath(first);
            AssetDatabase.AddObjectToAsset(new AnimationClip {name="User data"},first);AssetDatabase.SaveAssetIfDirty(first);
            Object.DestroyImmediate(avatar);EditorSceneManager.SaveScene(scene);
            var decision=PosingSystemGeneratedCleanup.Analyze(scene.path,new[]{path}).Single();
            Assert.That(decision.DeleteFile,Is.False);Assert.That(decision.Unused,Is.Empty);
            Assert.That(decision.Reason,Does.Contain("追加データ"));
        }

        [Test]
        public void Cleanup_PrunesOnlyUnreachableGeneratedObjectsAcrossAllControllers()
        {
            ps.features.breathing=true;var main=Generate();Generate(marker:"Again");
            var path=AssetDatabase.GetAssetPath(main);EditorSceneManager.SaveScene(scene);
            var data=PosingSystemGeneratedControllers.Metadata(main);
            var reachable=PosingSystemControllerGraph.Collect(data.controllers,o=>AssetDatabase.GetAssetPath(o)==path);
            var decision=PosingSystemGeneratedCleanup.Analyze(scene.path,new[]{path}).Single();
            Assert.That(decision.DeleteFile,Is.False);Assert.That(decision.Unused.Length,Is.GreaterThan(0));
            Assert.That(decision.Unused.Any(reachable.Contains),Is.False);
            // Apply only to this fixture's graph. Production also waits for empty global Undo/Redo.
            PosingSystemGeneratedCleanup.Apply(decision);
            Assert.That(reachable.All(o=>o!=null),Is.True);
            Assert.That(data.controllers.Length,Is.EqualTo(2));
            var after=PosingSystemGeneratedCleanup.Analyze(scene.path,new[]{path}).Single();Assert.That(after.Unused,Is.Empty);
        }

        [Test]
        public void PrefabStage_IsNotTreatedAsAnOrdinarySceneForCleanup()
        {
            var path=folder+"/Stage.prefab";PrefabUtility.SaveAsPrefabAsset(avatar,path);
            try
            {
                var stage=PrefabStageUtility.OpenPrefab(path);
                var root=stage.prefabContentsRoot;
                var owner=new PosingSystemGeneratedControllers.Owner(root);
                Assert.That(owner.Kind,Is.EqualTo("Prefabs"));
                Assert.That(owner.Guid,Is.EqualTo(AssetDatabase.AssetPathToGUID(path)));
                Assert.That(PosingSystemGeneratedCleanup.GuardReason(),Is.Not.Null);
                Assert.That(PosingSystemGeneratedCleanup.Candidates(owner.Guid),Is.Empty);
            }
            finally { StageUtility.GoToMainStage(); }
        }

        [Test]
        public void PersistentPrefabAsset_IsRejectedBeforeAnyMutation()
        {
            var path=folder+"/Direct.prefab";PrefabUtility.SaveAsPrefabAsset(avatar,path);
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var before=EditorJsonUtility.ToJson(source.GetComponent<ModularAvatarMergeAnimator>());
            Assert.Throws<InvalidOperationException>(()=>Generate(source.GetComponent<PosingSystem>()));
            Assert.That(EditorJsonUtility.ToJson(source.GetComponent<ModularAvatarMergeAnimator>()),Is.EqualTo(before));
            Assert.That(GeneratedFiles(),Is.Empty);
        }

        [Test]
        public void NestedAvatarDescriptors_AreDistinctOwners()
        {
            var nested=NewAvatar("Nested avatar",out var nestedPs,out var nestedMerge);
            nested.transform.SetParent(avatar.transform);
            var first=Generate();Assert.That(nestedMerge.animator,Is.SameAs(template));
            var second=Generate(nestedPs,"Nested");
            Assert.That(second,Is.Not.SameAs(first));Assert.That(merge.animator,Is.SameAs(first));
            Assert.That(first.parameters.Any(p=>p.name=="Nested"),Is.False);
            Assert.That(GeneratedFiles(),Has.Length.EqualTo(2));
        }

        [Test]
        public void ExternalBreathingResetClip_IsCopiedBeforeItCanBeRewritten()
        {
            clip.name=PosingSystemConverter.AdditiveBreathingResetClipName;
            var before=EditorJsonUtility.ToJson(clip);
            var map=PosingSystemControllerGraph.Clone(new[]{template});memory.UnionWith(map.Values);
            var copy=(AnimatorController)map[template];
            Assert.That(copy.layers[0].stateMachine.states[0].state.motion,Is.Not.SameAs(clip));
            Assert.That(EditorJsonUtility.ToJson(clip),Is.EqualTo(before));
        }
    }
}
