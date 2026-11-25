using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using VRC.SDK3.Avatars.Components;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    // Inquiry of 2026-11-22 (3.2.0): with "keep the head visible in photography mode" on, the avatar's own menu
    // toggles stopped working and its gesture expressions did not go back. While a layer whose weight is above zero
    // is in a state that has Write Defaults off and a motion, Unity writes no default value in the whole controller.
    // The head layer always had Write Defaults off, and so had every FX layer of the tool on an avatar that Modular
    // Avatar judges mixed (it then matches nothing). These tests give the avatar an FX controller of its own, build
    // the avatar and play the built FX controller the way a menu and a hand gesture would drive it.
    public partial class PosingSystemHumanoidFeatureTests
    {
        public enum AvatarFx
        {
            /// <summary>The avatar brings no FX controller.</summary>
            None,
            /// <summary>Toggles that rely on Write Defaults: a clip for "on" only.</summary>
            On,
            /// <summary>Write Defaults off, a clip for either side.</summary>
            Off,
            /// <summary>As On, plus one state without motion whose Write Defaults is unticked: harmless, but "mixed".</summary>
            OnWithStrayOff,
            /// <summary>As Off, plus one state without motion whose Write Defaults is ticked (Unity's default).</summary>
            OffWithStrayOn
        }

        private const string AvatarFxPart = "AvatarPart", AvatarFxFace = "AvatarFace";
        private static readonly string[] AvatarFxLayers = { "Avatar Toggle", "Avatar Expression", "Avatar Idle" };

        [TestCase(AvatarFx.On, false, false)]
        [TestCase(AvatarFx.On, true, false)]
        [TestCase(AvatarFx.On, true, true)]
        [TestCase(AvatarFx.OnWithStrayOff, false, false)]
        [TestCase(AvatarFx.OnWithStrayOff, true, true)]
        [TestCase(AvatarFx.Off, true, true)]
        [TestCase(AvatarFx.OffWithStrayOn, true, true)]
        public void WriteDefaults_AvatarToggleAndExpressionComeBack(AvatarFx kind, bool photography, bool keepHeadVisible)
        {
            PhotographyHeadSettings(photography, keepHeadVisible);
            GiveAvatarFx(kind);
            BuildDefaultIdleAvatar((built, _) =>
            {
                Assert.That(BuiltFx(built).layers.Any(l => l.name.StartsWith("Merge_")), Is.True,
                    "The case needs FX layers of the tool on the avatar");
                var (partOn, partOff, faceOn, faceOff) = PlayAvatarFx(built);
                Assert.That(partOn, Is.True, "The avatar's toggle did not switch on");
                Assert.That(faceOn, Is.EqualTo(100).Within(.01f), "The avatar's expression did not show");
                Assert.That(partOff, Is.False, "The avatar's toggle stayed on");
                Assert.That(faceOff, Is.EqualTo(0).Within(.01f), "The avatar's expression did not go back");
            }, true);
        }

        // Without an FX controller of its own the avatar gets VRChat's default one, and Modular Avatar matches the
        // layers to that (null: whatever it decides, the head layer must agree with the layers it merged).
        [TestCase(AvatarFx.None, null)]
        [TestCase(AvatarFx.On, true)]
        [TestCase(AvatarFx.Off, false)]
        [TestCase(AvatarFx.OnWithStrayOff, true)]
        [TestCase(AvatarFx.OffWithStrayOn, true)]
        public void WriteDefaults_GeneratedFxLayersFollowTheAvatar(AvatarFx kind, bool? expected)
        {
            var ps = PhotographyHeadSettings(true, true);
            ps.features.floating = true;
            GiveAvatarFx(kind);
            var own = kind == AvatarFx.None ? null : AvatarFxWriteDefaults(
                (AnimatorController)root.GetComponent<VRCAvatarDescriptor>().baseAnimationLayers
                    .Single(l => l.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController);
            BuildDefaultIdleAvatar((built, _) =>
            {
                var fx = BuiltFx(built);
                var generated = fx.layers.Where(IsGeneratedFxLayer).ToArray();
                var names = generated.Select(l => l.name).ToArray();
                Assert.That(names, Does.Contain(PosingSystemConverter.PhotographyTrackingLayer));
                Assert.That(names, Does.Contain(PosingSystemConverter.PhotographyHeadLayer));
                Assert.That(names, Does.Contain("床の高さの有効化"));
                Assert.That(names.Any(n => n.StartsWith("Merge_tracking")), Is.True);
                Assert.That(names, Does.Contain("Merge_disableLocomotion"));
                var decided = expected ?? DefaultIdleTestStates(generated.Single(
                    l => l.name == PosingSystemConverter.PhotographyTrackingLayer).stateMachine).First().writeDefaultValues;
                foreach (var layer in generated)
                    foreach (var state in DefaultIdleTestStates(layer.stateMachine))
                        Assert.That(state.writeDefaultValues, Is.EqualTo(decided), layer.name + " / " + state.name);
                // What the avatar's author chose is never changed.
                if (own != null) Assert.That(AvatarFxWriteDefaults(fx), Is.EqualTo(own));
            }, true);
        }

        // A prebuild made with 3.2.0 or older holds FX controllers with Write Defaults off. The build corrects them:
        // nobody has to update a prebuild because of this.
        [Test]
        public void WriteDefaults_PrebuildFromBeforeTheRuleIsCorrectedInTheBuild()
        {
            var ps = PhotographyHeadSettings(true, true);
            GiveAvatarFx(AvatarFx.OnWithStrayOff);
            PosingSystemEditor.Prebuild(ps);
            Assert.That(ps.data, Is.EqualTo(PosingSystemConverter.GetDefineSerializeJson(ps, true)));
            var merge = root.GetComponentsInChildren<ModularAvatarMergeAnimator>(true).Single(m => m.name == "USSPS_PhotoTracking");
            var prebuilt = (AnimatorController)merge.animator;
            Assert.That(merge.matchAvatarWriteDefaults, Is.True);
            foreach (var state in prebuilt.layers.SelectMany(l => DefaultIdleTestStates(l.stateMachine)))
            {
                Assert.That(state.writeDefaultValues, Is.True, "A new prebuild is made with Write Defaults on");
                state.writeDefaultValues = false;
                EditorUtility.SetDirty(state);
            }
            BuildDefaultIdleAvatar((built, _) =>
            {
                var layer = BuiltFx(built).layers.Single(l => l.name == PosingSystemConverter.PhotographyTrackingLayer);
                foreach (var state in DefaultIdleTestStates(layer.stateMachine)) Assert.That(state.writeDefaultValues, Is.True);
                var (_, partOff, _, faceOff) = PlayAvatarFx(built);
                Assert.That(partOff, Is.False, "The avatar's toggle stayed on");
                Assert.That(faceOff, Is.EqualTo(0).Within(.01f), "The avatar's expression did not go back");
            }, true);
            Assert.That(prebuilt.layers.SelectMany(l => DefaultIdleTestStates(l.stateMachine)).All(s => !s.writeDefaultValues), Is.True,
                "The build corrects its own copy, not the prebuilt asset");
        }

        private static AnimatorController BuiltFx(VRCAvatarDescriptor built) =>
            (AnimatorController)built.baseAnimationLayers.Single(l => l.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController;

        // The FX layers the tool generates: the merged tracking and locomotion controls, photography mode, the floor
        // height and the head layer. (Modular Avatar adds layers of its own, which are not the tool's.)
        private static bool IsGeneratedFxLayer(AnimatorControllerLayer layer) =>
            layer.name.StartsWith("Merge_") || layer.name == PosingSystemConverter.PhotographyTrackingLayer ||
            layer.name == PosingSystemConverter.PhotographyHeadLayer || layer.name == "床の高さの有効化";

        private static string AvatarFxWriteDefaults(AnimatorController fx) => string.Join(";", fx.layers
            .Where(l => AvatarFxLayers.Contains(l.name)).SelectMany(l => DefaultIdleTestStates(l.stateMachine)
                .Select(s => l.name + "/" + s.name + "=" + s.writeDefaultValues)).OrderBy(text => text, System.StringComparer.Ordinal));

        // An FX controller made the way avatars are sold: a menu toggle of an object under the head and a gesture
        // expression on a face shape. Call after the posing system has been set up (it adds the avatar descriptor).
        private void GiveAvatarFx(AvatarFx kind)
        {
            var part = new GameObject(AvatarFxPart);
            part.transform.SetParent(animator.GetBoneTransform(HumanBodyBones.Head), false);
            part.SetActive(false);
            var face = new GameObject(AvatarFxFace);
            face.transform.SetParent(root.transform, false);
            var mesh = new Mesh { name = AvatarFxFace };
            mesh.vertices = new[] { Vector3.zero, new Vector3(.02f, 0, 0), new Vector3(0, .02f, 0) };
            mesh.normals = new[] { Vector3.forward, Vector3.forward, Vector3.forward };
            mesh.triangles = new[] { 0, 1, 2 };
            mesh.boneWeights = Enumerable.Repeat(new BoneWeight { boneIndex0 = 0, weight0 = 1 }, 3).ToArray();
            mesh.bindposes = new[] { Matrix4x4.identity };
            mesh.AddBlendShapeFrame("Expression", 100, new[] { new Vector3(0, .01f, 0), Vector3.zero, Vector3.zero }, null, null);
            assets.Add(mesh);
            var renderer = face.AddComponent<SkinnedMeshRenderer>();
            renderer.sharedMesh = mesh;
            renderer.bones = new[] { face.transform };
            renderer.rootBone = face.transform;
            if (kind == AvatarFx.None) return;

            EnsureDefaultIdleTestFolder();
            var relies = kind == AvatarFx.On || kind == AvatarFx.OnWithStrayOff;
            var fx = AnimatorController.CreateAnimatorControllerAtPath(testFolder + "/AvatarFx.controller");
            fx.AddParameter("AvatarToggle", AnimatorControllerParameterType.Bool);
            fx.AddParameter("GestureLeft", AnimatorControllerParameterType.Int);
            Layer(AvatarFxLayers[0], EditorCurveBinding.FloatCurve(AnimationUtility.CalculateTransformPath(part.transform, root.transform),
                typeof(GameObject), "m_IsActive"), 1, "AvatarToggle");
            Layer(AvatarFxLayers[1], EditorCurveBinding.FloatCurve(AvatarFxFace, typeof(SkinnedMeshRenderer), "blendShape.Expression"), 100, "GestureLeft");
            if (kind == AvatarFx.OnWithStrayOff || kind == AvatarFx.OffWithStrayOn)
            {
                var machine = Machine(AvatarFxLayers[2]);
                machine.defaultState = machine.AddState("Idle");
                machine.defaultState.writeDefaultValues = !relies;
                fx.AddLayer(new AnimatorControllerLayer { name = AvatarFxLayers[2], defaultWeight = 1, stateMachine = machine });
            }
            EditorUtility.SetDirty(fx);
            AssetDatabase.SaveAssets();

            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            descriptor.customizeAnimationLayers = true;
            descriptor.baseAnimationLayers = new[] { VRCAvatarDescriptor.AnimLayerType.Base, VRCAvatarDescriptor.AnimLayerType.Additive,
                    VRCAvatarDescriptor.AnimLayerType.Gesture, VRCAvatarDescriptor.AnimLayerType.Action, VRCAvatarDescriptor.AnimLayerType.FX }
                .Select(type => new VRCAvatarDescriptor.CustomAnimLayer { type = type,
                    isDefault = type != VRCAvatarDescriptor.AnimLayerType.FX,
                    animatorController = type == VRCAvatarDescriptor.AnimLayerType.FX ? fx : null }).ToArray();
            descriptor.specialAnimationLayers = new VRCAvatarDescriptor.CustomAnimLayer[0];

            AnimatorStateMachine Machine(string name)
            {
                var machine = new AnimatorStateMachine { name = name, hideFlags = HideFlags.HideInHierarchy };
                AssetDatabase.AddObjectToAsset(machine, fx);
                return machine;
            }
            AnimationClip Clip(string name, EditorCurveBinding binding, float value)
            {
                var clip = new AnimationClip { name = name };
                AnimationUtility.SetEditorCurve(clip, binding, AnimationCurve.Constant(0, 1f / 60, value));
                AssetDatabase.AddObjectToAsset(clip, fx);
                return clip;
            }
            void Layer(string name, EditorCurveBinding binding, float value, string parameter)
            {
                var machine = Machine(name);
                var off = machine.AddState("Off");
                var on = machine.AddState("On");
                // Relying on Write Defaults: the resting state has no clip. Otherwise it writes the resting value itself.
                off.motion = relies ? null : Clip(name + " Off", binding, 0);
                on.motion = Clip(name + " On", binding, value);
                off.writeDefaultValues = on.writeDefaultValues = relies;
                machine.defaultState = off;
                var enter = off.AddTransition(on);
                var leave = on.AddTransition(off);
                foreach (var transition in new[] { enter, leave }) { transition.hasExitTime = false; transition.hasFixedDuration = true; transition.duration = .1f; }
                if (parameter == "GestureLeft")
                {
                    enter.AddCondition(AnimatorConditionMode.Equals, 1, parameter);
                    leave.AddCondition(AnimatorConditionMode.NotEqual, 1, parameter);
                }
                else
                {
                    enter.AddCondition(AnimatorConditionMode.If, 0, parameter);
                    leave.AddCondition(AnimatorConditionMode.IfNot, 0, parameter);
                }
                fx.AddLayer(new AnimatorControllerLayer { name = name, defaultWeight = 1, stateMachine = machine });
            }
        }

        // The built FX controller on the built avatar: the toggle and the gesture on for half a second, then off.
        private static (bool partOn, bool partOff, float faceOn, float faceOff) PlayAvatarFx(VRCAvatarDescriptor built)
        {
            var fx = BuiltFx(built);
            var part = built.GetComponentsInChildren<Transform>(true).Single(t => t.name == AvatarFxPart).gameObject;
            var face = built.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single(r => r.name == AvatarFxFace);
            // The avatar is verified more than once: start each time from what the build made.
            part.SetActive(false);
            face.SetBlendShapeWeight(0, 0);
            var types = fx.parameters.GroupBy(p => p.name).ToDictionary(g => g.Key, g => g.First().type);
            var graph = PlayableGraph.Create("AvatarFxWriteDefaults");
            try
            {
                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                var playable = AnimatorControllerPlayable.Create(graph, fx);
                AnimationPlayableOutput.Create(graph, "FX", built.GetComponent<Animator>()).SetSourcePlayable(playable);
                graph.Play();
                Set("IsLocal", 1);
                Wait();
                Set("AvatarToggle", 1); Set("GestureLeft", 1);
                Wait();
                var partOn = part.activeSelf;
                var faceOn = face.GetBlendShapeWeight(0);
                Set("AvatarToggle", 0); Set("GestureLeft", 0);
                Wait();
                return (partOn, part.activeSelf, faceOn, face.GetBlendShapeWeight(0));

                void Wait() { for (var frame = 0; frame < 30; frame++) graph.Evaluate(1f / 60); }
                // Modular Avatar may have changed the type of a parameter that several controllers declare.
                void Set(string name, float value)
                {
                    if (!types.TryGetValue(name, out var type)) return;
                    if (type == AnimatorControllerParameterType.Bool) playable.SetBool(name, value != 0);
                    else if (type == AnimatorControllerParameterType.Int) playable.SetInteger(name, Mathf.RoundToInt(value));
                    else playable.SetFloat(name, value);
                }
            }
            finally { graph.Destroy(); }
        }
    }
}
