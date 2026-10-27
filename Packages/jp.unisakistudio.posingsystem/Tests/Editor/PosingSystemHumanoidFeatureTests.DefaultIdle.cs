using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using nadena.dev.ndmf.animator;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(false, false, false, false)]
        [TestCase(true, false, false, false)]
        [TestCase(false, true, false, false)]
        [TestCase(true, true, false, false)]
        [TestCase(true, false, true, false)]
        [TestCase(false, true, true, false)]
        [TestCase(true, true, true, false)]
        [TestCase(true, false, false, true)]
        [TestCase(true, false, true, true)]
        public void DefaultIdle_NdmfBuild_PreservesBreathingAndAppendWithoutExtraBits(
            bool suppress, bool breathing, bool prebuild, bool eightBit)
        {
            var ps = DefaultIdleSettings(suppress, breathing);
            ps.isEightBitVersion = eightBit;
            ps.featureSettingsMigrated = !eightBit;
            // Verify independence from BOTH breathing switches.
            if (!breathing) ps.additiveBreathing.enabled = false;
            var userController = DefaultIdleCustomController(false);
            var append = new GameObject("Other Additive Tool");
            append.transform.SetParent(root.transform);
            var merge = append.AddComponent<ModularAvatarMergeAnimator>();
            merge.animator = userController;
            merge.layerType = VRCAvatarDescriptor.AnimLayerType.Additive;
            merge.pathMode = MergeAnimatorPathMode.Absolute;
            merge.mergeAnimatorMode = MergeAnimatorMode.Append;
            var sdkController = AssetDatabase.LoadAssetAtPath<AnimatorController>(AssetDatabase.GUIDToAssetPath(
                "573a1373059632b4d820876efe2d277f"));
            var sdkHash = EditorJsonUtility.ToJson(sdkController);
            var sourceHash = EditorJsonUtility.ToJson(userController);
            if (prebuild) PosingSystemEditor.Prebuild(ps);

            BuildDefaultIdleAvatar((descriptor, controller) =>
            {
                Assert.That(controller.animationClips.Contains(DefaultIdleProxy()), Is.EqualTo(!suppress));
                Assert.That(controller.animationClips.Any(c => c.name == PosingSystemConverter.SuppressedIdleClipName), Is.EqualTo(suppress));
                Assert.That(controller.layers.Any(l => l.name == PosingSystemConverter.AdditiveBreathingLayerName), Is.EqualTo(breathing));
                Assert.That(controller.animationClips.Any(c => c.name == "proxy_idle" &&
                    AnimationUtility.GetCurveBindings(c).Any(b => b.propertyName == "Left Arm Down-Up")), Is.True,
                    "A custom clip with the same name as the SDK clip must survive");
                var bits = descriptor.expressionParameters.parameters.Where(p => p.networkSynced).Sum(p =>
                    p.valueType == VRCExpressionParameters.ValueType.Bool ? 1 : 8);
                Assert.That(bits, Is.EqualTo(breathing ? 9 : 8));
                Assert.That(controller.layers.SelectMany(l => DefaultIdleTestStates(l.stateMachine))
                    .SelectMany(s => s.behaviours).OfType<VRCPlayableLayerControl>(), Is.Empty);
            }, optimize: true);
            Assert.That(EditorJsonUtility.ToJson(sdkController), Is.EqualTo(sdkHash));
            Assert.That(EditorJsonUtility.ToJson(userController), Is.EqualTo(sourceHash));
            Assert.That(sdkController.animationClips, Does.Contain(DefaultIdleProxy()));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void DefaultIdle_CustomController_NestedAndSyncedMotions_PreserveOtherLayers(bool replaceWithMa)
        {
            DefaultIdleSettings(true, false);
            var source = DefaultIdleCustomController(true);
            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            descriptor.customizeAnimationLayers = true;
            descriptor.baseAnimationLayers = new[] {
                new VRCAvatarDescriptor.CustomAnimLayer { type=VRCAvatarDescriptor.AnimLayerType.Base, isDefault=true },
                new VRCAvatarDescriptor.CustomAnimLayer { type=VRCAvatarDescriptor.AnimLayerType.Additive, isDefault=replaceWithMa,
                    animatorController=replaceWithMa ? null : source },
                new VRCAvatarDescriptor.CustomAnimLayer { type=VRCAvatarDescriptor.AnimLayerType.Gesture, animatorController=source },
                new VRCAvatarDescriptor.CustomAnimLayer { type=VRCAvatarDescriptor.AnimLayerType.Action, isDefault=true },
                new VRCAvatarDescriptor.CustomAnimLayer { type=VRCAvatarDescriptor.AnimLayerType.FX, isDefault=true }
            };
            if (replaceWithMa)
            {
                var obj = new GameObject("User Replacement"); obj.transform.SetParent(root.transform);
                var merge = obj.AddComponent<ModularAvatarMergeAnimator>();
                merge.animator = source;
                merge.layerType = VRCAvatarDescriptor.AnimLayerType.Additive;
                merge.mergeAnimatorMode = MergeAnimatorMode.Replace;
                merge.pathMode = MergeAnimatorPathMode.Absolute;
            }
            var sourceHash = EditorJsonUtility.ToJson(source);
            BuildDefaultIdleAvatar((built, controller) =>
            {
                Assert.That(controller.animationClips.Contains(DefaultIdleProxy()), Is.False);
                Assert.That(controller.layers.Length, Is.EqualTo(2));
                Assert.That(controller.layers[1].syncedLayerIndex, Is.EqualTo(0));
                var nested = controller.layers[0].stateMachine.stateMachines.Single().stateMachine.states.Single().state;
                var tree = (BlendTree)nested.motion;
                Assert.That(tree.children[0].motion.name, Is.EqualTo(PosingSystemConverter.SuppressedIdleClipName));
                Assert.That(tree.children[1].motion.name, Is.EqualTo("proxy_idle"));
                Assert.That(tree.children[1].threshold, Is.EqualTo(.75f));
                var idle = controller.layers[0].stateMachine.states.Single().state;
                Assert.That(controller.GetStateEffectiveMotion(idle, 1).name,
                    Is.EqualTo(PosingSystemConverter.SuppressedIdleClipName));
                Assert.That(idle.motion.name, Is.EqualTo("proxy_idle"));
                Assert.That(idle.transitions.Single().conditions.Single().parameter, Is.EqualTo("CustomBlend"));
                Assert.That(built.baseAnimationLayers.Single(l => l.type == VRCAvatarDescriptor.AnimLayerType.Gesture)
                    .animatorController.animationClips.Contains(DefaultIdleProxy()), Is.True,
                    "Even when shared with Additive, Gesture must stay unchanged");
            });
            Assert.That(EditorJsonUtility.ToJson(source), Is.EqualTo(sourceHash));
            Assert.That(source.animationClips, Does.Contain(DefaultIdleProxy()));
        }

        [Test]
        public void DefaultIdle_PrebuildOnOffOn_AndPrefabReload_RestoresOriginalIdle()
        {
            var ps = DefaultIdleSettings(false, false);
            Assert.That(new PosingSystem.AdditiveBreathingSettings().disableDefaultIdle, Is.False);
            var offSignature = PosingSystemConverter.GetDefineSerializeJson(ps);
            ps.additiveBreathing.disableDefaultIdle = true;
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps), Is.Not.EqualTo(offSignature));
            EnsureDefaultIdleTestFolder();
            var path = testFolder + "/Settings.prefab";
            PrefabUtility.SaveAsPrefabAsset(ps.gameObject, path);
            var saved = PrefabUtility.LoadPrefabContents(path);
            try { Assert.That(saved.GetComponent<PosingSystem>().additiveBreathing.disableDefaultIdle, Is.True); }
            finally { PrefabUtility.UnloadPrefabContents(saved); }
            foreach (var suppress in new[] { true, false, true })
            {
                ps.additiveBreathing.disableDefaultIdle = suppress;
                PosingSystemEditor.Prebuild(ps);
                Assert.That(ps.data, Is.EqualTo(PosingSystemConverter.GetDefineSerializeJson(ps, true)));
                BuildDefaultIdleAvatar((_, controller) =>
                    Assert.That(controller.animationClips.Contains(DefaultIdleProxy()), Is.EqualTo(!suppress)));
                Assert.That(root.GetComponent<VRCAvatarDescriptor>().baseAnimationLayers?
                    .All(l => l.isDefault || l.animatorController == null) ?? true, Is.True,
                    "Prebuild must not change the source avatar's playable layers");
            }
            ps.additiveBreathing.disableDefaultIdle = false;
            PrefabUtility.SaveAsPrefabAsset(ps.gameObject, path);
            saved = PrefabUtility.LoadPrefabContents(path);
            try { Assert.That(saved.GetComponent<PosingSystem>().additiveBreathing.disableDefaultIdle, Is.False); }
            finally { PrefabUtility.UnloadPrefabContents(saved); }
        }

        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void DefaultIdle_MultipleSystems_AnyEnabledWins(bool first, bool second)
        {
            DefaultIdleSettings(first, false);
            DefaultIdleSettings(second, false);
            BuildDefaultIdleAvatar((_, controller) => Assert.That(controller.animationClips.Contains(DefaultIdleProxy()), Is.False));
        }

        [Test]
        public void DefaultIdle_EditorOnlySetting_IsIgnored()
        {
            DefaultIdleSettings(false, false);
            var editorOnly = DefaultIdleSettings(true, false);
            var parent = new GameObject("Editor only settings");
            parent.transform.SetParent(root.transform); parent.tag = "EditorOnly";
            editorOnly.transform.SetParent(parent.transform);
            BuildDefaultIdleAvatar((_, controller) => Assert.That(controller.animationClips.Contains(DefaultIdleProxy()), Is.True));
        }

        private PosingSystem DefaultIdleSettings(bool suppress, bool breathing)
        {
            if (!root.TryGetComponent<VRCAvatarDescriptor>(out var descriptor))
            {
                descriptor = root.AddComponent<VRCAvatarDescriptor>();
                descriptor.customExpressions = true;
                descriptor.expressionParameters = ScriptableObject.CreateInstance<VRCExpressionParameters>();
                descriptor.expressionParameters.parameters = Array.Empty<VRCExpressionParameters.Parameter>();
                assets.Add(descriptor.expressionParameters);
            }
            var settings = Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(
                "ecde0b1d67f9e854992715d64f778d8c")), root.transform);
            var ps = settings.GetComponent<PosingSystem>();
            ps.features = new PosingSystemFeatureSettings { mirror=false, footHeight=false, fingerGestureControl=false, breathing=breathing };
            ps.featureSettingsMigrated = true;
            ps.additiveBreathing.disableDefaultIdle = suppress;
            ps.additiveBreathing.enabled = true;
            ps.defines.Clear(); ps.overrideDefines = new List<PosingSystem.OverrideAnimationDefine>();
            ps.autoImportAvatarAnimations = false; ps.isIconDisabled = true;
            return ps;
        }

        private static AnimationClip DefaultIdleProxy() => AssetDatabase.LoadAssetAtPath<AnimationClip>(
            AssetDatabase.GUIDToAssetPath(PosingSystemConverter.StandardIdleClipGuid));

        private void EnsureDefaultIdleTestFolder()
        {
            if (!string.IsNullOrEmpty(testFolder) && AssetDatabase.IsValidFolder(testFolder)) return;
            testFolder = "Assets/USSPS_DefaultIdleTests_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", testFolder.Substring(7));
        }

        private AnimatorController DefaultIdleCustomController(bool includeProxy)
        {
            EnsureDefaultIdleTestFolder();
            var source = AnimatorController.CreateAnimatorControllerAtPath(testFolder + "/Custom.controller");
            source.AddParameter("CustomBlend", AnimatorControllerParameterType.Float);
            var custom = new AnimationClip { name="proxy_idle" };
            AnimationUtility.SetEditorCurve(custom, EditorCurveBinding.FloatCurve("", typeof(Animator), "Left Arm Down-Up"),
                AnimationCurve.Linear(0, 0, 1, .2f));
            AssetDatabase.AddObjectToAsset(custom, source);
            var layers = source.layers;
            layers[0].name = "Idle";
            var machine = layers[0].stateMachine;
            var state = machine.AddState("Idle"); state.motion = custom;
            state.writeDefaultValues = true;
            if (includeProxy)
            {
                var sub = machine.AddStateMachine("Nested");
                var nested = sub.AddState("Upright Idle"); nested.writeDefaultValues = true;
                var tree = new BlendTree { name="Idle", blendType=BlendTreeType.Simple1D, blendParameter="CustomBlend",
                    useAutomaticThresholds=false, children=new[] {
                        new ChildMotion { motion=DefaultIdleProxy(), threshold=0, timeScale=1 },
                        new ChildMotion { motion=custom, threshold=.75f, timeScale=1 }
                    } };
                AssetDatabase.AddObjectToAsset(tree, source); nested.motion = tree;
                var transition = state.AddTransition(nested); transition.hasExitTime=false;
                transition.AddCondition(AnimatorConditionMode.Greater, .5f, "CustomBlend");
                var sync = new AnimatorControllerLayer { name="Custom Sync", syncedLayerIndex=0, defaultWeight=.4f,
                    stateMachine=new AnimatorStateMachine { name="Custom Sync" } };
                AssetDatabase.AddObjectToAsset(sync.stateMachine, source);
                sync.SetOverrideMotion(state, DefaultIdleProxy());
                layers = new[] { layers[0], sync };
            }
            source.layers = layers;
            AssetDatabase.SaveAssets();
            return source;
        }

        private bool? defaultIdleCompilerOptimization;
        private int defaultIdleCompilerCreated;
        private void BuildDefaultIdleAvatar(Action<VRCAvatarDescriptor, AnimatorController> verify, bool optimize = false)
        {
            var build = Object.Instantiate(root); build.name = root.name;
            var context = new BuildContext(build, null, false);
            using var compiler = defaultIdleCompilerOptimization.HasValue ?
                new PosingSystemControllerGeneration.Scope(build, defaultIdleCompilerOptimization.Value) : null;
            try
            {
                foreach (var ps in build.GetComponentsInChildren<PosingSystem>())
                    if (!string.IsNullOrEmpty(ps.data))
                        Assert.That(ps.data, Is.EqualTo(PosingSystemConverter.GetDefineSerializeJson(ps, true)),
                            "The prebuilt case must exercise cached output, not silently regenerate it");
                var method = typeof(AvatarProcessor).GetMethod("ProcessAvatar", BindingFlags.NonPublic | BindingFlags.Static,
                    null, new[] { typeof(BuildContext), typeof(BuildPhase), typeof(BuildPhase) }, null);
                try { method.Invoke(null, new object[] { context, BuildPhase.Resolving,
                    optimize ? BuildPhase.Optimizing : BuildPhase.Transforming }); }
                catch (TargetInvocationException exception) { throw exception.InnerException ?? exception; }
                context.DeactivateAllExtensionContexts();
                Verify();
                defaultIdleCompilerCreated = compiler?.Created ?? 0;
                // A later tool can reactivate animator services. Regression test LastCommit invalidation.
                context.ActivateExtensionContextRecursive<AnimatorServicesContext>();
                context.DeactivateAllExtensionContexts();
                Verify();
                void Verify()
                {
                    var descriptor = build.GetComponent<VRCAvatarDescriptor>();
                    var slot = descriptor.baseAnimationLayers.Single(l => l.type == VRCAvatarDescriptor.AnimLayerType.Additive);
                    Assert.That(slot.isDefault, Is.False);
                    Assert.That(slot.animatorController, Is.TypeOf<AnimatorController>());
                    verify(descriptor, (AnimatorController)slot.animatorController);
                }
            }
            finally { context.DeactivateAllExtensionContexts(); Object.DestroyImmediate(build); }
        }

        private static IEnumerable<AnimatorState> DefaultIdleTestStates(AnimatorStateMachine machine)
        {
            if (machine == null) yield break;
            foreach (var state in machine.states) yield return state.state;
            foreach (var sub in machine.stateMachines)
                foreach (var state in DefaultIdleTestStates(sub.stateMachine)) yield return state;
        }
    }
}
