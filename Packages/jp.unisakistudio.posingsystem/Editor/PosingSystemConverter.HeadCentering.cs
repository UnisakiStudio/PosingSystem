#if NDMF && MODULAR_AVATAR
using System;
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.ndmf;
using nadena.dev.ndmf.vrchat;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    public partial class PosingSystemConverter
    {
        // A tenth of a millimetre: below this the head counts as standing above the origin.
        private const float HeadCenterTolerance = .0001f;

        private static readonly EditorCurveBinding RootXBinding = EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.x");
        private static readonly EditorCurveBinding RootZBinding = EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT.z");

        /// <summary>
        /// Moves a finished pose sideways so that at its first frame the Head stands above the avatar's
        /// origin. VRChat holds the head at the viewpoint: a pose whose head is somewhere else has its
        /// body dragged there and its legs pulled straight.
        ///
        /// Nothing is assumed about units. Where the head is, and how far it moves per unit of RootT, are
        /// both measured by playing the clip on this avatar. That includes the avatar's scale and
        /// proportions, a Humanoid rebuilt by another tool, the clip's own root settings and every
        /// adjustment already in the clip. Every key moves by the same amount, so an animated pose keeps
        /// its motion.
        /// </summary>
        /// <returns>True when the clip was changed.</returns>
        private static bool CenterHeadAtStart(GameObject workingAvatar, AnimationClip clip)
        {
            using var probe = new PoseHeadProbe(workingAvatar);
            return CenterHead(probe, clip);
        }

        private static bool CenterHead(PoseHeadProbe probe, AnimationClip clip)
        {
            if (!probe.Valid || clip == null) return false;
            var changed = false;
            AnimationClip trial = null;
            try
            {
                // The response is linear, so the first step lands on the origin; the second look confirms it.
                for (var attempt = 0; attempt < 3; attempt++)
                {
                    var head = probe.Head(clip);
                    if (new Vector2(head.x, head.z).magnitude < HeadCenterTolerance) break;
                    if (trial == null) trial = new AnimationClip { hideFlags = HideFlags.HideAndDontSave };
                    if (!TryMeasureRootResponse(probe, clip, trial, head, out var xAxis, out var zAxis, out var determinant))
                    {
                        // Not baked into the pose: Unity keeps the body where the root motion says and
                        // RootT moves nothing, so the head stays wherever the pose puts it. Only a
                        // horizontal position that is part of the pose can be moved.
                        var settings = AnimationUtility.GetAnimationClipSettings(clip);
                        if (settings.loopBlendPositionXZ && settings.keepOriginalPositionXZ) break;
                        settings.loopBlendPositionXZ = settings.keepOriginalPositionXZ = true;
                        AnimationUtility.SetAnimationClipSettings(clip, settings);
                        changed = true;
                        continue;
                    }
                    var dx = (head.x * zAxis.z - head.z * zAxis.x) / determinant;
                    var dz = (xAxis.x * head.z - xAxis.z * head.x) / determinant;
                    ShiftRootCurve(clip, RootXBinding, -dx);
                    ShiftRootCurve(clip, RootZBinding, -dz);
                    changed = true;
                }
            }
            finally { if (trial != null) Object.DestroyImmediate(trial); }
            return changed;
        }

        // How far the head moves, in the avatar's axes and in metres, when RootT.x or RootT.z grows by one.
        private static bool TryMeasureRootResponse(PoseHeadProbe probe, AnimationClip clip, AnimationClip trial, Vector3 head,
            out Vector3 xAxis, out Vector3 zAxis, out float determinant)
        {
            EditorUtility.CopySerialized(clip, trial);
            var sourceX = AnimationUtility.GetEditorCurve(clip, RootXBinding);
            var sourceZ = AnimationUtility.GetEditorCurve(clip, RootZBinding);
            AnimationUtility.SetEditorCurve(trial, RootXBinding, Shifted(sourceX, 1));
            xAxis = probe.Head(trial) - head;
            AnimationUtility.SetEditorCurve(trial, RootXBinding, sourceX);
            AnimationUtility.SetEditorCurve(trial, RootZBinding, Shifted(sourceZ, 1));
            zAxis = probe.Head(trial) - head;
            determinant = xAxis.x * zAxis.z - xAxis.z * zAxis.x;
            return !float.IsNaN(determinant) && !float.IsInfinity(determinant) && Mathf.Abs(determinant) > .00000001f;
        }

        private static void ShiftRootCurve(AnimationClip clip, EditorCurveBinding binding, float amount) =>
            AnimationUtility.SetEditorCurve(clip, binding, Shifted(AnimationUtility.GetEditorCurve(clip, binding), amount));

        // The same curve moved by a constant: key times, tangents and wrap modes stay. A clip without the curve gets one.
        private static AnimationCurve Shifted(AnimationCurve source, float amount)
        {
            if (source == null || source.length == 0) return new AnimationCurve(new Keyframe(0, amount));
            var curve = new AnimationCurve(source.keys) { preWrapMode = source.preWrapMode, postWrapMode = source.postWrapMode };
            for (var i = 0; i < curve.length; i++)
            {
                var key = curve[i];
                key.value += amount;
                curve.MoveKey(i, key);
            }
            return curve;
        }

        /// <summary>
        /// Plays a clip on an avatar the way the built controller does: through an animation graph, with
        /// root motion left unapplied. Sampling a clip directly ignores the clip's root settings and shows
        /// a clip that is not baked into the pose in a place it never has in VRChat.
        /// The avatar is a working copy; its transforms are put back when the probe is disposed.
        /// </summary>
        private sealed class PoseHeadProbe : IDisposable
        {
            private readonly Animator animator;
            private readonly Transform root, head, hips;
            private readonly Transform[] transforms;
            private readonly Vector3[] positions, scales;
            private readonly Quaternion[] rotations;
            private readonly Vector3 rootPosition;
            private readonly Quaternion rootRotation;
            // A place no pose puts the hips: if they are still there after playing, nothing was played.
            private static readonly Vector3 Unplayed = new Vector3(-7919.25f, 6007.5f, 4999.75f);

            public PoseHeadProbe(GameObject avatar)
            {
                animator = avatar != null ? avatar.GetComponent<Animator>() : null;
                if (animator == null || !animator.isHuman || animator.avatar == null) return;
                head = animator.GetBoneTransform(HumanBodyBones.Head);
                hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                root = avatar.transform;
                rootPosition = root.position; rootRotation = root.rotation;
                transforms = avatar.GetComponentsInChildren<Transform>(true);
                positions = transforms.Select(t => t.localPosition).ToArray();
                rotations = transforms.Select(t => t.localRotation).ToArray();
                scales = transforms.Select(t => t.localScale).ToArray();
            }

            public bool Valid => head != null && hips != null;

            /// <summary>The head at the first frame of the clip, relative to the avatar root, in the root's axes and in metres.</summary>
            public Vector3 Head(AnimationClip clip)
            {
                var hipsPosition = hips.localPosition;
                var applyRootMotion = animator.applyRootMotion; var culling = animator.cullingMode; var enabled = animator.enabled;
                var graph = PlayableGraph.Create("PosingSystem head probe");
                try
                {
                    hips.localPosition = Unplayed;
                    if (!enabled) animator.enabled = true;
                    animator.applyRootMotion = false;
                    animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                    graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                    var playable = AnimationClipPlayable.Create(graph, clip);
                    playable.SetApplyFootIK(false);
                    playable.SetApplyPlayableIK(false);
                    AnimationPlayableOutput.Create(graph, "Pose", animator).SetSourcePlayable(playable);
                    graph.Evaluate(0);
                }
                finally
                {
                    graph.Destroy();
                    animator.applyRootMotion = applyRootMotion; animator.cullingMode = culling;
                    if (!enabled) animator.enabled = false;
                }
                // An animator that cannot play here (an inactive copy, for example) leaves the hips where they
                // were put. Sampling then still answers for a clip that is baked into the pose.
                if (hips.localPosition == Unplayed)
                {
                    hips.localPosition = hipsPosition;
                    clip.SampleAnimation(root.gameObject, 0);
                }
                root.SetPositionAndRotation(rootPosition, rootRotation);
                return Quaternion.Inverse(rootRotation) * (head.position - rootPosition);
            }

            public void Dispose()
            {
                if (transforms == null) return;
                for (var i = 0; i < transforms.Length; i++)
                {
                    if (transforms[i] == null) continue;
                    transforms[i].localPosition = positions[i];
                    transforms[i].localRotation = rotations[i];
                    transforms[i].localScale = scales[i];
                }
            }
        }

        /// <summary>
        /// The last word on where the head is. Poses are generated early in the build, or before it by a
        /// prebuild; after that other tools still change the avatar (MA Floor Adjuster rebuilds the Humanoid
        /// in its late stage) and a prebuild can be older than a change of scale or proportions. Every
        /// generated pose is played on the avatar as it is now and moved only if its head is off the origin.
        /// Heights are not touched.
        /// </summary>
        private static void CenterPoseHeadsOnFinalAvatar(BuildContext ctx)
        {
            if (ctx == null || ctx.AvatarRootObject == null) return;
            if (!ctx.AvatarRootObject.GetComponentsInChildren<PosingSystem>().Any(ps => ps.tag != "EditorOnly")) return;
            var descriptor = ctx.VRChatAvatarDescriptor();
            if (descriptor == null) return;

            GameObject workingAvatar = null;
            try
            {
                workingAvatar = Object.Instantiate(ctx.AvatarRootObject);
                workingAvatar.name = "_PosingSystem_TempHeadCenterAvatar";
                workingAvatar.hideFlags = HideFlags.HideAndDontSave;
                workingAvatar.SetActive(true);
                using var probe = new PoseHeadProbe(workingAvatar);
                if (!probe.Valid) return;

                var done = new Dictionary<Motion, Motion>();
                var moved = 0;
                var controllers = new HashSet<AnimatorController>();
                foreach (var customLayer in descriptor.baseAnimationLayers.Concat(descriptor.specialAnimationLayers))
                {
                    if (customLayer.animatorController is not AnimatorController controller || !controllers.Add(controller)) continue;
                    foreach (var layer in controller.layers)
                        if (layer.stateMachine != null) Visit(layer.stateMachine);
                }
                if (moved > 0)
                    Debug.Log($"[PosingSystem] ビルド後のアバターで頭の位置がずれていた姿勢を{moved}個補正しました。");

                void Visit(AnimatorStateMachine stateMachine)
                {
                    foreach (var child in stateMachine.stateMachines)
                        if (child.stateMachine != null) Visit(child.stateMachine);
                    foreach (var state in stateMachine.states)
                        if (IsGeneratedPoseMotion(state.state.motion)) state.state.motion = Center(state.state.motion);
                }

                Motion Center(Motion motion)
                {
                    if (motion == null) return null;
                    if (done.TryGetValue(motion, out var known)) return known;
                    var result = motion;
                    if (motion is AnimationClip clip)
                    {
                        // A rollover keeps the head above the origin through its whole motion.
                        var rollover = clip.name.Contains(RolloverCenteredMarker);
                        if (IsCenteredPoseClip(clip) && (rollover ? RolloverHeadIsOff(workingAvatar, clip) : HeadIsOff(probe, clip)))
                        {
                            var target = WritableBuildCopy(ctx, clip);
                            if (rollover) CenterRolloverHead(workingAvatar, target);
                            else CenterHead(probe, target);
                            result = target;
                            moved++;
                        }
                    }
                    else if (motion is BlendTree tree)
                    {
                        var children = tree.children;
                        var changed = false;
                        for (var i = 0; i < children.Length; i++)
                        {
                            var centered = Center(children[i].motion);
                            if (centered == children[i].motion) continue;
                            children[i].motion = centered;
                            changed = true;
                        }
                        if (changed)
                        {
                            var target = WritableBuildCopy(ctx, tree);
                            target.children = children;
                            result = target;
                        }
                    }
                    return done[motion] = result;
                }
            }
            finally
            {
                if (workingAvatar != null) Object.DestroyImmediate(workingAvatar);
            }
        }

        /// <summary>
        /// A state motion that holds poses this tool generated. With the FootHeight feature on it is the
        /// "_footheight" blend tree around the pose; with the feature off it is the pose clip itself.
        /// </summary>
        private static bool IsGeneratedPoseMotion(Motion motion) =>
            motion != null && motion.name.IndexOf("_USSPS_") == 0 &&
            (motion.name.IndexOf("_footheight") != -1 || motion.name.IndexOf(ProcessedPoseMarker) != -1);

        // In the name of every clip generated from a pose of the list, and of its height variants.
        private const string ProcessedPoseMarker = "_Processed";

        /// <summary>
        /// A clip generated from a pose of the list, whose head belongs above the origin. Not the height and
        /// movement offsets (differences that another layer adds), not VRChat's own clips, and not the
        /// children of a blend tree the user supplied as a pose: those are used as they are, walking included.
        /// </summary>
        private static bool IsCenteredPoseClip(AnimationClip clip) =>
            clip != null && clip.name.IndexOf("_USSPS_") == 0 && clip.name.IndexOf(ProcessedPoseMarker) != -1 &&
            !clip.name.StartsWith(FootHeightOffsetPrefix) && !clip.name.StartsWith(MovementOffsetPrefix) &&
            new[] { "RootT.x", "RootT.y", "RootT.z" }.Any(name =>
                AnimationUtility.GetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), name)) != null);

        private static bool HeadIsOff(PoseHeadProbe probe, AnimationClip clip)
        {
            var head = probe.Head(clip);
            return new Vector2(head.x, head.z).magnitude >= HeadCenterTolerance;
        }

        private static bool RolloverHeadIsOff(GameObject workingAvatar, AnimationClip clip)
        {
            var animator = workingAvatar.GetComponent<Animator>();
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            var root = workingAvatar.transform;
            var position = root.position; var rotation = root.rotation;
            var off = false;
            for (var i = 0; i <= 8 && !off; i++)
            {
                clip.SampleAnimation(workingAvatar, clip.length * i / 8);
                root.SetPositionAndRotation(position, rotation);
                var offset = Quaternion.Inverse(rotation) * (head.position - position);
                off = new Vector2(offset.x, offset.z).magnitude > .0005f;
            }
            return off;
        }

        // An asset of the project is never written to. What NDMF made for this build is already a safe copy
        // and is edited in place (copying a motion inside its temporary container breaks the references).
        private static T WritableBuildCopy<T>(BuildContext ctx, T asset) where T : Motion, new()
        {
            if (!EditorUtility.IsPersistent(asset) || ctx.IsTemporaryAsset(asset)) return asset;
            var copy = new T();
            EditorUtility.CopySerialized(asset, copy);
            copy.name = asset.name;
            AssetDatabase.AddObjectToAsset(copy, ctx.AssetContainer);
            return copy;
        }
    }
}
#endif
