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
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    // A generated pose keeps the Head above the avatar's origin: VRChat holds the head at the
    // viewpoint, so a pose whose head is somewhere else gets its legs pulled straight.
    // (Inquiry of 2026-11-16: avatar scaled to 1.344, changed proportions, MA Floor Adjuster.)
    public partial class PosingSystemHumanoidFeatureTests
    {
        private const string HeadCenterPose = "HeadCenterSupine";

        [Flags]
        public enum HeadCase
        {
            Plain = 0,
            View = 1,            // the viewpoint is moved away from the head
            NoPrebuild = 2,      // the build generates the pose itself
            Unbaked = 4,         // the source clip keeps Unity's default root settings
            NoFootHeight = 8,
            BoneScale = 16,      // proportions changed on the bones, as with a body-shape edit
            Adjusted = 32,       // the pose has an adjustment clip (rotation, position and muscles)
            Rotated = 64,        // the pose is turned in the pose list
            Placed = 128,        // the avatar stands somewhere else in the scene, turned
            ScaleAdjuster = 256, // MA Scale Adjuster on the hips and legs
            Moving = 512,        // the pose is an animation whose body travels
            Stale = 1024,        // scale and proportions are changed after the prebuild
            HumanoidScale = 2048, // another tool changes the Humanoid's scale in the middle of the build
            Rollover = 4096,     // a second pose of the list rolls over
            HipsScale = 8192,    // the Hips bone itself is scaled
            ChestStretch = 16384, // the Chest is scaled differently along each axis
            BonesMoved = 32768,  // Hips, Chest and Neck are moved from where the model has them
            Armature = 65536,    // the object between the avatar root and the Hips is scaled and moved
            RootStretch = 131072, // the avatar root is scaled differently along each axis
        }

        private AnimationClip SupinePose(HeadCase options, string name = HeadCenterPose, float roll = 0)
        {
            var clip = PosingSystemConverter.RootOffsetClip(new Vector3(.25f, .22f, -.35f), Quaternion.Euler(-80, 25, roll), name);
            assets.Add(clip);
            foreach (var (muscle, value) in new[] { ("Left Upper Leg Front-Back", .5f), ("Right Upper Leg Front-Back", .3f),
                         ("Left Lower Leg Stretch", -.6f), ("Right Lower Leg Stretch", -.4f), ("Spine Front-Back", .2f), ("Neck Nod Down-Up", .2f) })
                AnimationUtility.SetEditorCurve(clip, Binding(muscle), AnimationCurve.Constant(0, 1, value));
            if (options.HasFlag(HeadCase.Moving))
            {
                AnimationUtility.SetEditorCurve(clip, Binding("RootT.x"), AnimationCurve.Linear(0, .25f, 1, .45f));
                AnimationUtility.SetEditorCurve(clip, Binding("RootT.z"), AnimationCurve.Linear(0, -.35f, 1, -.2f));
            }
            if (options.HasFlag(HeadCase.Unbaked)) AnimationUtility.SetAnimationClipSettings(clip, new AnimationClipSettings { stopTime = 1 });
            return clip;
        }

        // Modular Avatar rebuilds the Humanoid only when each bone of its description names its parent,
        // as every imported model does.
        private void UseImportedStyleSkeleton()
        {
            var parent = typeof(SkeletonBone).GetField("parentName", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            Assert.That(parent, Is.Not.Null);
            var description = animator.avatar.humanDescription;
            description.skeleton = root.GetComponentsInChildren<Transform>().Select(t =>
            {
                object bone = new SkeletonBone { name = t.name, position = t.localPosition, rotation = t.localRotation, scale = t.localScale };
                parent.SetValue(bone, t == root.transform ? "" : t.parent.name);
                return (SkeletonBone)bone;
            }).ToArray();
            var avatar = AvatarBuilder.BuildHumanAvatar(root, description);
            Assert.That(avatar.isValid && avatar.isHuman, Is.True);
            assets.Add(avatar);
            animator.avatar = avatar;
        }

        private PosingSystem HeadCenterSettings(HeadCase options)
        {
            var ps = DefaultIdleSettings(false, false);
            ps.enablePoseIk = false;
            ps.features.footHeight = !options.HasFlag(HeadCase.NoFootHeight);
            ps.features.footHeightBackend = PosingSystemFeatureSettings.FootHeightBackend.ClipLevel;
            var pose = new PosingSystem.AnimationDefine { enabled = true, displayName = "Sleep", animationClip = SupinePose(options),
                syncdParameterValue = 4, typeParameterValue = 1 };
            if (options.HasFlag(HeadCase.Adjusted))
            {
                var adjustment = PosingSystemConverter.RootOffsetClip(new Vector3(.1f, .05f, .1f), Quaternion.Euler(10, 0, 25), "HeadCenterAdjustment");
                assets.Add(adjustment);
                AnimationUtility.SetEditorCurve(adjustment, Binding("Spine Front-Back"), AnimationCurve.Constant(0, 1, .3f));
                AnimationUtility.SetEditorCurve(adjustment, Binding("Left Upper Leg Front-Back"), AnimationCurve.Constant(0, 1, -.4f));
                pose.adjustmentClip = adjustment;
            }
            if (options.HasFlag(HeadCase.Rotated)) { pose.isRotate = true; pose.rotate = 70; }
            var poses = new List<PosingSystem.AnimationDefine> { pose };
            if (options.HasFlag(HeadCase.Rollover))
            {
                ps.features.rollover = true;
                poses.Add(new PosingSystem.AnimationDefine { enabled = true, displayName = "Roll", animationClip = SupinePose(HeadCase.Plain, HeadCenterPose + "Roll"),
                    isRollover = true, rolloverSideAnimation = SupinePose(HeadCase.Plain, "HeadCenterSide", 75), syncdParameterValue = 5, typeParameterValue = 2 });
            }
            ps.defines.Add(new PosingSystem.LayerDefine("Sleep", "Sleep", "SitSleepUp", 30, "Sleep", poses));
            return ps;
        }

        // The head relative to the avatar root, in the root's axes and in metres, with the clip played
        // through an animation graph as the built controller plays it (root motion is not applied).
        private static Vector3 PlayedHead(GameObject avatar, AnimationClip clip, float time = 0)
        {
            var target = avatar.GetComponent<Animator>();
            var head = target.GetBoneTransform(HumanBodyBones.Head);
            var position = avatar.transform.position; var rotation = avatar.transform.rotation;
            var graph = PlayableGraph.Create("Head centering test");
            try
            {
                graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                var playable = AnimationClipPlayable.Create(graph, clip);
                playable.SetApplyFootIK(false);
                playable.SetTime(time);
                AnimationPlayableOutput.Create(graph, "Avatar", target).SetSourcePlayable(playable);
                target.applyRootMotion = false;
                graph.Evaluate(0);
            }
            finally { graph.Destroy(); }
            avatar.transform.SetPositionAndRotation(position, rotation);
            return Quaternion.Inverse(rotation) * (head.position - position);
        }

        private static Vector3 SampledHead(GameObject avatar, AnimationClip clip, float time = 0)
        {
            var head = avatar.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
            var position = avatar.transform.position; var rotation = avatar.transform.rotation;
            clip.SampleAnimation(avatar, time);
            avatar.transform.SetPositionAndRotation(position, rotation);
            return Quaternion.Inverse(rotation) * (head.position - position);
        }

        private static float Horizontal(Vector3 offset) => new Vector2(offset.x, offset.z).magnitude;

        // Every generated clip of the test poses on the built avatar: how far the head is from the origin,
        // sampled directly and played. A rollover is looked at through its whole motion, a pose at its start.
        private static List<(string name, float sampled, float played, float height)> BuiltHeadOffsets(GameObject built)
        {
            var descriptor = built.GetComponent<VRCAvatarDescriptor>();
            var result = new List<(string, float, float, float)>();
            foreach (var controller in descriptor.baseAnimationLayers.Select(l => l.animatorController).OfType<AnimatorController>().Distinct())
            foreach (var clip in controller.animationClips.Distinct().Where(c => c.name.Contains(HeadCenterPose) && !c.name.Contains("_offset_")))
            {
                var steps = clip.name.Contains("RolloverCentered") ? 8 : 0;
                float sampled = 0, played = 0;
                for (var i = 0; i <= steps; i++)
                {
                    var time = steps == 0 ? 0 : clip.length * i / steps;
                    sampled = Mathf.Max(sampled, Horizontal(SampledHead(built, clip, time)));
                    played = Mathf.Max(played, Horizontal(PlayedHead(built, clip, time)));
                }
                result.Add((clip.name, sampled, played, PlayedHead(built, clip).y));
            }
            return result;
        }

        private static void RunBuild(BuildContext context, BuildPhase first, BuildPhase last)
        {
            var method = typeof(AvatarProcessor).GetMethod("ProcessAvatar", BindingFlags.NonPublic | BindingFlags.Static,
                null, new[] { typeof(BuildContext), typeof(BuildPhase), typeof(BuildPhase) }, null);
            try { method.Invoke(null, new object[] { context, first, last }); }
            catch (TargetInvocationException exception) { throw exception.InnerException ?? exception; }
        }

        private void ChangeProportions()
        {
            foreach (var (bone, size) in new[] { (HumanBodyBones.Spine, .8765f), (HumanBodyBones.Chest, 1.14f), (HumanBodyBones.LeftUpperLeg, 1.18f),
                         (HumanBodyBones.RightUpperLeg, 1.18f), (HumanBodyBones.Neck, .9f) })
                animator.GetBoneTransform(bone).localScale = Vector3.one * size;
        }

        // The ways a body is resized by hand. Applied before the prebuild, or after it for a stale prebuild.
        private void ResizeBody(HeadCase options)
        {
            if (options.HasFlag(HeadCase.BoneScale)) ChangeProportions();
            if (options.HasFlag(HeadCase.HipsScale)) animator.GetBoneTransform(HumanBodyBones.Hips).localScale = Vector3.one * 1.27f;
            if (options.HasFlag(HeadCase.ChestStretch)) animator.GetBoneTransform(HumanBodyBones.Chest).localScale = new Vector3(1.25f, .85f, 1.4f);
            if (options.HasFlag(HeadCase.BonesMoved))
            {
                animator.GetBoneTransform(HumanBodyBones.Hips).localPosition += new Vector3(.02f, .11f, -.04f);
                animator.GetBoneTransform(HumanBodyBones.Chest).localPosition += new Vector3(0, .06f, .05f);
                animator.GetBoneTransform(HumanBodyBones.Neck).localPosition += new Vector3(0, -.03f, .02f);
            }
            if (options.HasFlag(HeadCase.Armature))
            {
                var armature = animator.GetBoneTransform(HumanBodyBones.Hips).parent;
                armature.localScale = new Vector3(1.18f, 1.18f, 1.18f);
                armature.localPosition = new Vector3(0, .04f, .03f);
            }
            if (options.HasFlag(HeadCase.RootStretch)) root.transform.localScale = Vector3.Scale(root.transform.localScale, new Vector3(1.1f, 1.3f, .9f));
        }

        // scale: avatar root; floor: an MA Floor Adjuster below (-) or above (+) the origin in metres, 0 = none.
        [TestCase(1f, 0f, HeadCase.Plain)]
        [TestCase(1.34405f, 0f, HeadCase.Plain)]
        [TestCase(.6f, 0f, HeadCase.Plain)]
        [TestCase(1f, -.12f, HeadCase.Plain)]
        [TestCase(1f, .08f, HeadCase.Plain)]
        [TestCase(1f, -.12f, HeadCase.NoFootHeight)]
        [TestCase(1f, 0f, HeadCase.View)]
        [TestCase(1f, 0f, HeadCase.BoneScale)]
        [TestCase(1f, 0f, HeadCase.Adjusted)]
        [TestCase(1f, 0f, HeadCase.Rotated)]
        [TestCase(1f, 0f, HeadCase.Placed)]
        [TestCase(1f, 0f, HeadCase.Unbaked)]
        [TestCase(1.34405f, 0f, HeadCase.Unbaked | HeadCase.NoPrebuild)]
        [TestCase(1f, 0f, HeadCase.Moving)]
        [TestCase(1f, 0f, HeadCase.ScaleAdjuster)]
        [TestCase(1f, 0f, HeadCase.NoFootHeight | HeadCase.NoPrebuild)]
        [TestCase(1f, 0f, HeadCase.Rollover)]
        [TestCase(1.34405f, -.12f, HeadCase.Rollover | HeadCase.BoneScale)]
        [TestCase(1f, 0f, HeadCase.Stale)]
        [TestCase(1f, 0f, HeadCase.Stale | HeadCase.NoFootHeight | HeadCase.Rollover)]
        [TestCase(1f, 0f, HeadCase.HipsScale)]
        [TestCase(1f, 0f, HeadCase.ChestStretch)]
        [TestCase(1f, 0f, HeadCase.BonesMoved)]
        [TestCase(1f, 0f, HeadCase.Armature)]
        [TestCase(1f, 0f, HeadCase.RootStretch)]
        [TestCase(1f, -.12f, HeadCase.HipsScale | HeadCase.BonesMoved)]
        [TestCase(1f, .08f, HeadCase.Armature | HeadCase.ChestStretch)]
        [TestCase(1.34405f, -.12f, HeadCase.HipsScale | HeadCase.ChestStretch | HeadCase.BonesMoved | HeadCase.Armature | HeadCase.RootStretch | HeadCase.BoneScale | HeadCase.Rollover)]
        [TestCase(.7f, 0f, HeadCase.Stale | HeadCase.HipsScale | HeadCase.ChestStretch | HeadCase.BonesMoved | HeadCase.Armature | HeadCase.RootStretch)]
        [TestCase(1f, 0f, HeadCase.HumanoidScale)]
        [TestCase(1.34405f, 0f, HeadCase.HumanoidScale | HeadCase.NoFootHeight | HeadCase.Rollover)]
        // The avatar of the inquiry: scaled root, changed proportions, MA Floor Adjuster, moved viewpoint.
        [TestCase(1.34405f, -.12f, HeadCase.View | HeadCase.BoneScale | HeadCase.ScaleAdjuster)]
        [TestCase(1.34405f, -.12f, HeadCase.View | HeadCase.BoneScale | HeadCase.ScaleAdjuster | HeadCase.Adjusted)]
        [TestCase(1.34405f, -.12f, HeadCase.View | HeadCase.BoneScale | HeadCase.NoPrebuild)]
        [TestCase(.6f, .08f, HeadCase.View | HeadCase.Unbaked | HeadCase.NoFootHeight | HeadCase.Adjusted | HeadCase.Rotated | HeadCase.Placed | HeadCase.Moving)]
        public void HeadCentering_BuiltPoseKeepsHeadAboveOrigin(float scale, float floor, HeadCase options)
        {
            if (options.HasFlag(HeadCase.Armature))
            {
                // Most models have an object between the avatar root and the Hips.
                var armature = new GameObject("Armature").transform;
                armature.SetParent(root.transform, false);
                animator.GetBoneTransform(HumanBodyBones.Hips).SetParent(armature, false);
            }
            UseImportedStyleSkeleton();
            root.transform.localScale = Vector3.one * scale;
            if (options.HasFlag(HeadCase.Placed)) root.transform.SetPositionAndRotation(new Vector3(3, .4f, -2), Quaternion.Euler(0, 23, 0));
            if (!options.HasFlag(HeadCase.Stale)) ResizeBody(options);
            if (options.HasFlag(HeadCase.ScaleAdjuster))
                foreach (var bone in new[] { HumanBodyBones.Hips, HumanBodyBones.LeftUpperLeg, HumanBodyBones.RightUpperLeg })
                    animator.GetBoneTransform(bone).gameObject.AddComponent<ModularAvatarScaleAdjuster>().Scale = new Vector3(.83f, 1, .83f);
            var ps = HeadCenterSettings(options);
            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            var headBone = animator.GetBoneTransform(HumanBodyBones.Head);
            descriptor.ViewPosition = headBone.position - root.transform.position +
                (options.HasFlag(HeadCase.View) ? new Vector3(.03f, .11f, .14f) : new Vector3(0, .06f, .08f)) * scale;
            if (floor != 0)
            {
                var adjuster = new GameObject("Floor");
                adjuster.transform.SetParent(root.transform, false);
                adjuster.transform.localPosition = new Vector3(0, floor, 0);
                // MA Floor Adjuster exists since Modular Avatar 1.17.0; the package also compiles with older versions,
                // and its tests are compiled in every project that has the package.
                var floorAdjuster = typeof(ModularAvatarScaleAdjuster).Assembly.GetType("nadena.dev.modular_avatar.core.ModularAvatarFloorAdjuster");
                if (floorAdjuster == null) Assert.Ignore("MA Floor Adjuster needs Modular Avatar 1.17.0 or later.");
                adjuster.AddComponent(floorAdjuster);
            }
            if (!options.HasFlag(HeadCase.NoPrebuild)) PrebuildPersistedSettings(ps);
            if (options.HasFlag(HeadCase.Stale))
            {
                root.transform.localScale *= 1.25f;
                descriptor.ViewPosition *= 1.25f;
                ResizeBody(options | HeadCase.BoneScale);
            }
            WithPersistedBuild(context =>
            {
                if (options.HasFlag(HeadCase.HumanoidScale))
                {
                    // What a floor adjuster that edits the Humanoid's scale does between the generation of the poses and the end of the build.
                    RunBuild(context, BuildPhase.Resolving, BuildPhase.Generating);
                    var target = context.AvatarRootObject.GetComponent<Animator>();
                    var changed = Object.Instantiate(target.avatar);
                    assets.Add(changed);
                    var serialized = new SerializedObject(changed);
                    serialized.FindProperty("m_Avatar.m_Human.data.m_Scale").floatValue += .13f;
                    serialized.ApplyModifiedPropertiesWithoutUndo();
                    target.avatar = changed;
                    RunBuild(context, BuildPhase.Transforming, BuildPhase.Optimizing);
                }
                else RunBuild(context, BuildPhase.Resolving, BuildPhase.Optimizing);
                context.DeactivateAllExtensionContexts();
                Assert.That(context.Successful, Is.True);
                var built = context.AvatarRootObject;
                var builtAnimator = built.GetComponent<Animator>();
                if (floor != 0 || options.HasFlag(HeadCase.HumanoidScale))
                    Assert.That(builtAnimator.humanScale, Is.Not.EqualTo(animator.humanScale).Within(.001f), "Fixture: the Humanoid must have changed during the build");
                var offsets = BuiltHeadOffsets(built);
                Assert.That(offsets.Count, Is.GreaterThanOrEqualTo(options.HasFlag(HeadCase.Rollover) ? 2 : 1), "The build must contain the generated poses");
                if (options.HasFlag(HeadCase.Rollover)) Assert.That(offsets.Any(o => o.name.Contains("RolloverCentered")), Is.True);
                TestContext.WriteLine($"HEAD humanScale source={animator.humanScale:F4} built={builtAnimator.humanScale:F4} rootScale={built.transform.lossyScale.x:F4}");
                foreach (var (name, sampled, played, height) in offsets)
                    TestContext.WriteLine($"HEAD {name}: played off={played:F4} m, sampled off={sampled:F4} m, height={height:F4} m");
                foreach (var (name, sampled, played, _) in offsets)
                {
                    // Played is what the built controller does. Sampling agrees with it only for a clip that is baked into the pose.
                    Assert.That(played, Is.LessThan(name.Contains("RolloverCentered") ? .0015f : .0005f), name + " played");
                    if (!options.HasFlag(HeadCase.Unbaked))
                        Assert.That(sampled, Is.LessThan(name.Contains("RolloverCentered") ? .0015f : .0005f), name + " sampled");
                }
            });
        }

        [TestCase(1f, HeadCase.Plain)]
        [TestCase(.5f, HeadCase.Placed)]
        [TestCase(2f, HeadCase.Moving | HeadCase.Placed)]
        [TestCase(1.3f, HeadCase.Unbaked)]
        [TestCase(1.3f, HeadCase.Unbaked | HeadCase.Moving | HeadCase.Rotated | HeadCase.Placed)]
        [TestCase(1f, HeadCase.Rotated)]
        public void HeadCentering_CenterHeadAtStart_MovesOnlyTheHorizontalRoot(float scale, HeadCase options)
        {
            var parent = new GameObject("Scaled parent"); assets.Add(parent);
            parent.transform.localScale = Vector3.one * 1.3f;
            root.transform.SetParent(parent.transform, false);
            root.transform.localScale = Vector3.one * scale;
            if (options.HasFlag(HeadCase.Placed)) root.transform.SetPositionAndRotation(new Vector3(3, .4f, -2), Quaternion.Euler(0, 23, 0));
            var source = SupinePose(options);
            if (options.HasFlag(HeadCase.Rotated))
            {
                var turned = AnimationUtility.GetAnimationClipSettings(source);
                turned.orientationOffsetY = 70;
                AnimationUtility.SetAnimationClipSettings(source, turned);
            }
            var result = Object.Instantiate(source); assets.Add(result);
            var transforms = root.GetComponentsInChildren<Transform>(true);
            var before = transforms.Select(t => (t.localPosition, t.localRotation, t.localScale)).ToArray();
            Assert.That(Horizontal(PlayedHead(root, source)), Is.GreaterThan(.1f * scale), "Fixture: the head starts away from the origin");
            for (var i = 0; i < transforms.Length; i++)
            { transforms[i].localPosition = before[i].localPosition; transforms[i].localRotation = before[i].localRotation; transforms[i].localScale = before[i].localScale; }

            Assert.That(CenteringCall("CenterHeadAtStart", root, result), Is.EqualTo(true));
            Assert.That(transforms.Select(t => (t.localPosition, t.localRotation, t.localScale)).ToArray(), Is.EqualTo(before), "The avatar is put back as it was");
            foreach (var binding in AnimationUtility.GetCurveBindings(source).Where(b => b.propertyName != "RootT.x" && b.propertyName != "RootT.z"))
                Assert.That(AnimationUtility.GetEditorCurve(result, binding).keys, Is.EqualTo(AnimationUtility.GetEditorCurve(source, binding).keys), binding.propertyName);
            foreach (var axis in new[] { "RootT.x", "RootT.z" })
            {
                var from = AnimationUtility.GetEditorCurve(source, Binding(axis)); var to = AnimationUtility.GetEditorCurve(result, Binding(axis));
                Assert.That(to.length, Is.EqualTo(from.length));
                for (var i = 0; i < from.length; i++)
                {
                    Assert.That(to[i].time, Is.EqualTo(from[i].time));
                    Assert.That(to[i].value - to[0].value, Is.EqualTo(from[i].value - from[0].value).Within(.00001f), "The motion of the pose is kept");
                    Assert.That((to[i].inTangent, to[i].outTangent), Is.EqualTo((from[i].inTangent, from[i].outTangent)));
                }
            }
            var settings = AnimationUtility.GetAnimationClipSettings(result);
            var sourceSettings = AnimationUtility.GetAnimationClipSettings(source);
            Assert.That(settings.loopBlendPositionXZ && settings.keepOriginalPositionXZ, Is.True, "Only a horizontal position that is part of the pose can be centred");
            Assert.That((settings.orientationOffsetY, settings.level, settings.loopBlendOrientation, settings.keepOriginalOrientation, settings.loopBlendPositionY, settings.keepOriginalPositionY),
                Is.EqualTo((sourceSettings.orientationOffsetY, sourceSettings.level, sourceSettings.loopBlendOrientation, sourceSettings.keepOriginalOrientation, sourceSettings.loopBlendPositionY, sourceSettings.keepOriginalPositionY)));

            var head = PlayedHead(root, result);
            TestContext.WriteLine($"HEAD centred: x={head.x:F5} z={head.z:F5} y={head.y:F5}");
            Assert.That(Horizontal(head), Is.LessThan(.0002f));
            Assert.That(head.y, Is.EqualTo(PlayedHead(root, source).y).Within(.0001f), "Height is not touched");

            var json = EditorJsonUtility.ToJson(result);
            Assert.That(CenteringCall("CenterHeadAtStart", root, result), Is.EqualTo(false), "A centred pose is left alone");
            Assert.That(EditorJsonUtility.ToJson(result), Is.EqualTo(json));
        }

        [Test]
        public void HeadCentering_ClipWithoutHorizontalRootCurves_GetsThem()
        {
            root.transform.localScale = Vector3.one * 1.34405f;
            var clip = SupinePose(HeadCase.Plain);
            AnimationUtility.SetEditorCurve(clip, Binding("RootT.x"), null);
            AnimationUtility.SetEditorCurve(clip, Binding("RootT.z"), null);
            Assert.That(Horizontal(PlayedHead(root, clip)), Is.GreaterThan(.1f));
            Assert.That(CenteringCall("CenterHeadAtStart", root, clip), Is.EqualTo(true));
            Assert.That(AnimationUtility.GetEditorCurve(clip, Binding("RootT.x")), Is.Not.Null);
            Assert.That(AnimationUtility.GetEditorCurve(clip, Binding("RootT.z")), Is.Not.Null);
            Assert.That(Horizontal(PlayedHead(root, clip)), Is.LessThan(.0002f));
        }

        // RootT is in the Humanoid's own units: making the whole avatar larger or smaller must not change
        // one generated curve. This catches any place that mixes metres of the scene into those units.
        // The movement offsets (photography distance, floating height) are the one intended exception:
        // they are distances in metres, so their RootT shrinks as the avatar grows.
        [TestCase(2f)]
        [TestCase(.45f)]
        public void HeadCentering_UniformAvatarScaleDoesNotChangeGeneratedCurves(float scale)
        {
            UseImportedStyleSkeleton();
            var ps = HeadCenterSettings(HeadCase.Adjusted | HeadCase.Rollover);
            ps.features.photography = ps.features.floating = true;
            ps.features.photographyBackend = ps.features.floatingBackend = PosingSystemFeatureSettings.MovementBackend.AdditiveOverride;
            ps.features.floatingPoseSpace = PosingSystemFeatureSettings.FloatingPoseSpaceMode.EnterEveryFrame;
            var descriptor = root.GetComponent<VRCAvatarDescriptor>();
            descriptor.ViewPosition = animator.GetBoneTransform(HumanBodyBones.Head).position + new Vector3(0, .06f, .08f);

            PrebuildPersistedSettings(ps);
            var reference = GeneratedCurves();
            Assert.That(reference.Count(c => c.key.Contains(HeadCenterPose)), Is.GreaterThan(10), "Fixture: the poses were generated");
            Assert.That(reference.Any(c => c.key.Contains("_offset_")), Is.True, "Fixture: height or movement offsets were generated");

            root.transform.localScale = Vector3.one * scale;
            descriptor.ViewPosition *= scale;
            var warnings = new List<string>();
            void Collect(string message, string stack, LogType type) { if (type != LogType.Log) warnings.Add(message); }
            Application.logMessageReceived += Collect;
            try { PrebuildPersistedSettings(ps); }
            finally { Application.logMessageReceived -= Collect; }
            Assert.That(warnings.Where(w => w.Contains("[PosingSystem]")), Is.Empty, "A scaled avatar is not an unusual rig");

            var scaled = GeneratedCurves();
            Assert.That(scaled.Select(c => c.key), Is.EqualTo(reference.Select(c => c.key)));
            float worst = 0; var where = "";
            for (var i = 0; i < reference.Count; i++)
            for (var t = 0; t < reference[i].values.Length; t++)
            {
                var metres = reference[i].key.Contains("_movement_offset_") && reference[i].key.Contains("/RootT.");
                var difference = Mathf.Abs(reference[i].values[t] - scaled[i].values[t] * (metres ? scale : 1));
                if (difference > worst) { worst = difference; where = reference[i].key; }
            }
            TestContext.WriteLine($"HEAD curves compared={reference.Count} worst difference={worst:F6} at {where}");
            Assert.That(worst, Is.LessThan(.0005f), where);

            List<(string key, float[] values)> GeneratedCurves()
            {
                var curves = new List<(string, float[])>();
                foreach (var merge in root.GetComponentsInChildren<ModularAvatarMergeAnimator>(true))
                {
                    if (merge.animator is not AnimatorController controller) continue;
                    foreach (var clip in controller.animationClips.Distinct().Where(c => c.name.StartsWith("_USSPS_")))
                    foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                    {
                        var curve = AnimationUtility.GetEditorCurve(clip, binding);
                        curves.Add((controller.name + "/" + clip.name + "/" + binding.path + "/" + binding.propertyName,
                            Enumerable.Range(0, 5).Select(i => curve.Evaluate(clip.length * i / 4)).ToArray()));
                    }
                }
                // Equal names (the same pose for desktop and VR) are told apart by their values; the sign survives the scale.
                return curves.OrderBy(c => c.Item1, StringComparer.Ordinal).ThenBy(c => Math.Sign(c.Item2[0])).ThenBy(c => Math.Sign(c.Item2[4])).ToList();
            }
        }
    }
}
