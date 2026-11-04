using System;
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
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void PhotographyHead_OnlyOptInPhotographyBuildAddsHeadChop(bool photography, bool visible)
        {
            var ps = PhotographyHeadSettings(photography, visible);
            var cost = PosingSystemFeatureUtility.OptionalBits(ps);
            var sourceHead = animator.GetBoneTransform(HumanBodyBones.Head);
            var sourceScale = sourceHead.localScale;
            BuildDefaultIdleAvatar((built, _) =>
            {
                AssertPhotographyHeadBuild(built, photography && visible);
                Assert.That(built.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).localScale,
                    Is.EqualTo(sourceScale));
                Assert.That(built.GetComponentsInChildren<Transform>().Any(t => t.name.Contains("Constraint")), Is.False);
            });
            Assert.That(root.GetComponentsInChildren<VRCHeadChop>(true), Is.Empty);
            Assert.That(sourceHead.localScale, Is.EqualTo(sourceScale));
            ps.features.photographyKeepHeadVisible = !visible;
            Assert.That(PosingSystemFeatureUtility.OptionalBits(ps), Is.EqualTo(cost));
        }

        [Test]
        public void PhotographyHead_SettingsSaveReloadAndPrebuildOnOffOn()
        {
            Assert.That(new PosingSystemFeatureSettings().photographyKeepHeadVisible, Is.False);
            var ps = PhotographyHeadSettings(true, false);
            var offSignature = PosingSystemConverter.GetDefineSerializeJson(ps);
            ps.features.photographyKeepHeadVisible = true;
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps), Is.Not.EqualTo(offSignature));
            EnsureDefaultIdleTestFolder();
            var path = testFolder + "/PhotoSettings.prefab";
            foreach (var visible in new[] { true, false, true })
            {
                ps.features.photographyKeepHeadVisible = visible;
                PrefabUtility.SaveAsPrefabAsset(ps.gameObject, path);
                var loaded = PrefabUtility.LoadPrefabContents(path);
                try { Assert.That(loaded.GetComponent<PosingSystem>().features.photographyKeepHeadVisible, Is.EqualTo(visible)); }
                finally { PrefabUtility.UnloadPrefabContents(loaded); }
                PosingSystemEditor.Prebuild(ps);
                Assert.That(ps.data, Is.EqualTo(PosingSystemConverter.GetDefineSerializeJson(ps, true)));
                Assert.That(root.GetComponentsInChildren<VRCHeadChop>(true), Is.Empty,
                    "Prebuild must not install a live head override");
                BuildDefaultIdleAvatar((built, _) => AssertPhotographyHeadBuild(built, visible));
            }
        }

        [TestCase(false, false)]
        [TestCase(false, true)]
        [TestCase(true, false)]
        [TestCase(true, true)]
        public void PhotographyHead_LocalOnlyAndExitRestore_Repeatedly(bool oneFrame, bool optimize)
        {
            PhotographyHeadSettings(true, true);
            BuildDefaultIdleAvatar((built, _) =>
            {
                if (oneFrame)
                {
                    var locomotion = (AnimatorController)built.baseAnimationLayers.Single(
                        l => l.type == VRCAvatarDescriptor.AnimLayerType.Base).animatorController;
                    var direct = locomotion.layers.Single(l => l.name.EndsWith("USSPS_Locomotion"))
                        .stateMachine.stateMachines.Select(s => s.stateMachine.name).ToArray();
                    foreach (var category in new[] { "SitStand", "SitShallow", "SitDeep" })
                        Assert.That(direct.Count(n => n.EndsWith("Direct" + category)), Is.EqualTo(1),
                            "Exercise the actual one-evaluation playback graph: " + category);
                    foreach (var threshold in new[] { "Shallow", "Deep", "Sleep" })
                        Assert.That(locomotion.layers.Count(l => l.name.EndsWith("USSPS_Final" + threshold + "Threshold")),
                            Is.EqualTo(1), "Keep the final classification pass: " + threshold);
                    Assert.That(locomotion.parameters.Count(p => p.name.EndsWith(PosingSystemOneFrameSelection.Guard)), Is.EqualTo(1));
                    Assert.That(built.expressionParameters.parameters.Any(
                        p => p.name.EndsWith(PosingSystemOneFrameSelection.Guard)), Is.False);
                }
                var chop = AssertPhotographyHeadBuild(built, true);
                var fx = (AnimatorController)built.baseAnimationLayers.Single(l => l.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController;
                var graph = PlayableGraph.Create("PhotographyHeadLocalSwitchTest");
                try
                {
                    var playable = AnimatorControllerPlayable.Create(graph, fx);
                    var output = AnimationPlayableOutput.Create(graph, "FX", built.GetComponent<Animator>());
                    output.SetSourcePlayable(playable); graph.Play();
                    foreach (var sample in new[] {
                        (local: false, photo: false), (local: false, photo: true),
                        (local: true, photo: true), (local: true, photo: false),
                        (local: true, photo: true), (local: false, photo: true),
                        (local: true, photo: true), (local: true, photo: false)
                    })
                    {
                        playable.SetBool("IsLocal", sample.local);
                        playable.SetBool(PosingSystemFeatureUtility.PhotoActive, sample.photo);
                        for (var frame = 0; frame < 6; frame++) graph.Evaluate(1f / 60);
                        Assert.That(chop.enabled, Is.EqualTo(sample.local && sample.photo),
                            "IsLocal=" + sample.local + ", Photo=" + sample.photo);
                    }
                }
                finally { graph.Destroy(); }
            }, optimize, oneFrame);
        }

        [Test]
        public void PhotographyHead_MultipleSystemsGenerateOne_ExistingEarSettingsPreserved()
        {
            PhotographyHeadSettings(true, false);
            PhotographyHeadSettings(true, true);
            PhotographyHeadSettings(true, true);
            var ear = new GameObject("Ear");
            ear.transform.SetParent(animator.GetBoneTransform(HumanBodyBones.Head), false);
            var obj = new GameObject("UserHeadChop"); obj.transform.SetParent(root.transform, false);
            var source = obj.AddComponent<VRCHeadChop>(); source.enabled = false; source.globalScaleFactor = .7f;
            source.targetBones = new[] { new VRCHeadChop.HeadChopBone {
                transform = ear.transform, scaleFactor = .8f, applyCondition = VRCHeadChop.HeadChopBone.ApplyCondition.VrOnly } };
            var json = EditorJsonUtility.ToJson(source);
            BuildDefaultIdleAvatar((built, _) =>
            {
                AssertPhotographyHeadBuild(built, true);
                Assert.That(built.GetComponentsInChildren<VRCHeadChop>(true).Length, Is.EqualTo(2));
                var retained = built.transform.Find("UserHeadChop").GetComponent<VRCHeadChop>();
                Assert.That(retained.enabled, Is.False);
                Assert.That(retained.globalScaleFactor, Is.EqualTo(.7f));
                Assert.That(retained.targetBones.Single().scaleFactor, Is.EqualTo(.8f));
                Assert.That(retained.targetBones.Single().applyCondition, Is.EqualTo(VRCHeadChop.HeadChopBone.ApplyCondition.VrOnly));
                Assert.That(retained.targetBones.Single().transform.root, Is.EqualTo(built.transform));
            });
            Assert.That(EditorJsonUtility.ToJson(source), Is.EqualTo(json));
        }

        [Test]
        public void PhotographyHead_MaReplacementAndLaterAnimatorCommitKeepLayer()
        {
            PhotographyHeadSettings(true, true);
            var source = DefaultIdleCustomController(false);
            var json = EditorJsonUtility.ToJson(source);
            var obj = new GameObject("User FX replacement"); obj.transform.SetParent(root.transform, false);
            var merge = obj.AddComponent<ModularAvatarMergeAnimator>();
            merge.animator = source; merge.layerType = VRCAvatarDescriptor.AnimLayerType.FX;
            merge.mergeAnimatorMode = MergeAnimatorMode.Replace;
            merge.pathMode = MergeAnimatorPathMode.Absolute;
            BuildDefaultIdleAvatar((built, _) =>
            {
                AssertPhotographyHeadBuild(built, true);
                var fx = (AnimatorController)built.baseAnimationLayers.Single(l => l.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController;
                Assert.That(fx.animationClips.Any(c => c.name == "proxy_idle"), Is.True);
            }, true);
            Assert.That(EditorJsonUtility.ToJson(source), Is.EqualTo(json));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void PhotographyHead_EditorOnlyOrLegacyEightBitDoesNotOptIn(bool editorOnly)
        {
            PhotographyHeadSettings(true, false);
            var ps = PhotographyHeadSettings(true, true);
            if (editorOnly)
            {
                var parent = new GameObject("Editor only"); parent.tag = "EditorOnly";
                parent.transform.SetParent(root.transform); ps.transform.SetParent(parent.transform);
            }
            else ps.isEightBitVersion = true;
            BuildDefaultIdleAvatar((built, _) => AssertPhotographyHeadBuild(built, false));
        }

        [Test]
        public void PhotographyHead_ConflictingHeadTargetAndComponentLimitAreActionable()
        {
            var ps = PhotographyHeadSettings(true, true);
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            var source = root.AddComponent<VRCHeadChop>();
            source.targetBones = new[] { new VRCHeadChop.HeadChopBone { transform = head, scaleFactor = .2f } };
            var json = EditorJsonUtility.ToJson(source);
            Assert.That(PosingSystemDiagnostics.Collect(ps).Any(i => i.Code == "PS080"), Is.True);
            Assert.That(EditorJsonUtility.ToJson(source), Is.EqualTo(json));
            source.targetBones = Array.Empty<VRCHeadChop.HeadChopBone>();
            for (var i = 1; i < VRCHeadChop.MaxComponentCount; i++)
            {
                var obj = new GameObject("Existing" + i); obj.transform.SetParent(root.transform);
                obj.AddComponent<VRCHeadChop>();
            }
            Assert.That(PosingSystemDiagnostics.Collect(ps).Any(i => i.Code == "PS081"), Is.True);
            ps.features.photographyKeepHeadVisible = false;
            Assert.That(PosingSystemDiagnostics.Collect(ps).Any(i => i.Code == "PS080" || i.Code == "PS081"), Is.False);
        }

        private PosingSystem PhotographyHeadSettings(bool photography, bool visible)
        {
            var ps = DefaultIdleSettings(false, false);
            ps.features.photography = photography;
            ps.features.photographyKeepHeadVisible = visible;
            return ps;
        }

        private static VRCHeadChop AssertPhotographyHeadBuild(VRCAvatarDescriptor built, bool expected)
        {
            var generated = built.GetComponentsInChildren<VRCHeadChop>(true)
                .Where(c => c.name == PosingSystemConverter.PhotographyHeadObject).ToArray();
            var fx = (AnimatorController)built.baseAnimationLayers.Single(l => l.type == VRCAvatarDescriptor.AnimLayerType.FX).animatorController;
            var layers = fx.layers.Where(l => l.name == PosingSystemConverter.PhotographyHeadLayer).ToArray();
            Assert.That(generated.Length, Is.EqualTo(expected ? 1 : 0));
            Assert.That(layers.Length, Is.EqualTo(expected ? 1 : 0));
            if (!expected) return null;
            var chop = generated.Single();
            Assert.That(chop.transform.parent, Is.Not.EqualTo(built.transform));
            Assert.That(chop.enabled, Is.False);
            Assert.That(chop.globalScaleFactor, Is.EqualTo(1));
            Assert.That(chop.targetBones.Single().transform, Is.EqualTo(built.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head)));
            Assert.That(chop.targetBones.Single().scaleFactor, Is.EqualTo(1));
            Assert.That(chop.targetBones.Single().applyCondition, Is.EqualTo(VRCHeadChop.HeadChopBone.ApplyCondition.AlwaysApply));
            Assert.That(fx.parameters.Single(p => p.name == "IsLocal").type, Is.EqualTo(AnimatorControllerParameterType.Bool));
            foreach (var state in layers.Single().stateMachine.states)
            {
                Assert.That(state.state.writeDefaultValues, Is.False);
                var binding = AnimationUtility.GetCurveBindings((AnimationClip)state.state.motion).Single();
                Assert.That(binding.type, Is.EqualTo(typeof(VRCHeadChop)));
                Assert.That(binding.propertyName, Is.EqualTo("m_Enabled"));
                Assert.That(built.transform.Find(binding.path), Is.EqualTo(chop.transform));
            }
            return chop;
        }
    }
}
