using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    /// <summary>Build-only, avatar-specific two-pose roll. Source assets are never modified.</summary>
    public static class PosingSystemRolloverGenerator
    {
        private const int Steps = 240;
        private static readonly string[] RootNames = { "RootT.x", "RootT.y", "RootT.z", "RootQ.x", "RootQ.y", "RootQ.z", "RootQ.w" };
        private sealed class Pose
        {
            public Vector3 position, heading, face;
            public Quaternion rotation;
            public float[] muscles;
            public void Turn(Quaternion turn)
            {
                position = turn * position; rotation = turn * rotation;
                heading = turn * heading; face = turn * face;
            }
            public Pose Mirror()
            {
                var mirrored = new Pose { position = new Vector3(-position.x, position.y, position.z),
                    rotation = new Quaternion(rotation.x, -rotation.y, -rotation.z, rotation.w),
                    heading = new Vector3(-heading.x, heading.y, heading.z), face = new Vector3(-face.x, face.y, face.z),
                    muscles = new float[muscles.Length] };
                for (var i = 0; i < muscles.Length; i++)
                {
                    var name = HumanTrait.MuscleName[i];
                    var other = name.StartsWith("Left") ? "Right" + name.Substring(4) :
                        name.StartsWith("Right") ? "Left" + name.Substring(5) : name;
                    var index = Array.IndexOf(HumanTrait.MuscleName, other);
                    mirrored.muscles[index] = muscles[i] * (other == name && name.Contains("Left-Right") ? -1 : 1);
                }
                return mirrored;
            }
        }

        public static bool Supports(AnimationClip clip) => clip != null && !clip.legacy &&
            (clip.humanMotion || AnimationUtility.GetCurveBindings(clip).Any(b => b.type == typeof(Animator) &&
                b.path == "" && (b.propertyName.StartsWith("RootQ.") || AdditiveCurveUtility.IsMuscleBinding(b))));

        public static AnimationClip Create(GameObject avatar, AnimationClip center, AnimationClip side, float contactStrength = .25f)
        {
            if (!Supports(center) || !Supports(side)) throw new ArgumentException(PosingSystemLocalization.TextMessage("寝返りの中央と横向きにはHumanoid AnimationClipが必要です。"));
            if (float.IsNaN(contactStrength) || float.IsInfinity(contactStrength) || contactStrength < 0 || contactStrength > 1)
                throw new ArgumentOutOfRangeException(nameof(contactStrength));
            if (avatar == null || !avatar.TryGetComponent<Animator>(out var animator) || !animator.isHuman || !animator.avatar.isValid)
                throw new ArgumentException(PosingSystemLocalization.TextMessage("寝返りの生成には有効なHumanoidアバターが必要です。"));

            var clone = Object.Instantiate(avatar);
            clone.name = "_USSPS_RolloverBake"; clone.hideFlags = HideFlags.HideAndDontSave;
            clone.transform.SetParent(null, false);
            clone.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity); clone.transform.localScale = Vector3.one;
            foreach (var behaviour in clone.GetComponentsInChildren<Behaviour>(true))
                if (behaviour != null) behaviour.enabled = behaviour is Animator;
            clone.SetActive(true);
            var result = new AnimationClip { name = center.name + "_Rollover", frameRate = 60 };
            try
            {
                animator = clone.GetComponent<Animator>(); animator.runtimeAnimatorController = null;
                using var handler = new HumanPoseHandler(animator.avatar, animator.transform);
                var neutral = new HumanPose { bodyPosition = Vector3.up, bodyRotation = Quaternion.identity, muscles = new float[HumanTrait.MuscleCount] };
                handler.SetHumanPose(ref neutral);
                var transforms = clone.GetComponentsInChildren<Transform>(true);
                var positions = transforms.Select(t => t.localPosition).ToArray();
                var rotations = transforms.Select(t => t.localRotation).ToArray();
                var scales = transforms.Select(t => t.localScale).ToArray();
                void Restore()
                {
                    for (var i = 0; i < transforms.Length; i++)
                    { transforms[i].localPosition = positions[i]; transforms[i].localRotation = rotations[i]; transforms[i].localScale = scales[i]; }
                }
                Pose Read(AnimationClip source)
                {
                    Restore(); source.SampleAnimation(clone, 0);
                    clone.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                    var human = new HumanPose(); handler.GetHumanPose(ref human);
                    var head = animator.GetBoneTransform(HumanBodyBones.Head);
                    var heading = head.position - animator.GetBoneTransform(HumanBodyBones.Hips).position;
                    heading.y = 0;
                    return new Pose { position = human.bodyPosition, rotation = human.bodyRotation, muscles = human.muscles,
                        heading = heading.sqrMagnitude > .0001f ? heading.normalized : Vector3.back, face = head.forward };
                }
                var middle = Read(center);
                var left = Read(side);
                var toCanonical = Quaternion.FromToRotation(middle.heading, Vector3.back);
                middle.Turn(toCanonical);
                left.Turn(Quaternion.FromToRotation(left.heading, Vector3.back));
                if (left.face.x > 0) left = left.Mirror();
                var right = left.Mirror();
                var fromCanonical = Quaternion.Inverse(toCanonical);
                foreach (var pose in new[] { left, middle, right }) pose.Turn(fromCanonical);

                // Preserve non-body properties (facial expression, etc.) from the center pose.
                foreach (var binding in AnimationUtility.GetCurveBindings(center))
                    if (binding.type != typeof(Animator) || binding.path != "")
                        AnimationUtility.SetEditorCurve(result, binding, AnimationCurve.Constant(0, 1, AnimationUtility.GetEditorCurve(center, binding).Evaluate(0)));
                foreach (var binding in AnimationUtility.GetObjectReferenceCurveBindings(center))
                {
                    var keys = AnimationUtility.GetObjectReferenceCurve(center, binding);
                    if (keys.Length > 0) AnimationUtility.SetObjectReferenceCurve(result, binding,
                        new[] { new ObjectReferenceKeyframe { time = 0, value = keys[0].value } });
                }

                var muscleNames = HumanTrait.MuscleName.ToArray();
                foreach (var source in new[] { side, center })
                foreach (var binding in AnimationUtility.GetCurveBindings(source))
                {
                    var index = AdditiveCurveUtility.GetMuscleIndexFromBinding(binding);
                    if (index >= 0) muscleNames[index] = binding.propertyName;
                }
                var curves = Enumerable.Range(0, muscleNames.Length + 7).Select(_ => new List<Keyframe>()).ToArray();
                var previous = left.rotation;
                for (var frame = 0; frame <= Steps; frame++)
                {
                    var time = frame / (float)Steps;
                    var a = time <= .5f ? left : middle; var b = time <= .5f ? middle : right;
                    var weight = time <= .5f ? time * 2 : (time - .5f) * 2;
                    var p = Vector3.Lerp(a.position, b.position, weight);
                    var q = Quaternion.Slerp(a.rotation, b.rotation, weight);
                    if (Quaternion.Dot(previous, q) < 0) q = new Quaternion(-q.x, -q.y, -q.z, -q.w);
                    previous = q;
                    for (var i = 0; i < muscleNames.Length; i++) curves[i].Add(new Keyframe(time, Mathf.Lerp(a.muscles[i], b.muscles[i], weight)));
                    for (var i = 0; i < 3; i++) curves[muscleNames.Length + i].Add(new Keyframe(time, p[i]));
                    for (var i = 0; i < 4; i++) curves[muscleNames.Length + 3 + i].Add(new Keyframe(time, q[i]));
                }
                for (var i = 0; i < curves.Length; i++)
                    AnimationUtility.SetEditorCurve(result, Binding(i < muscleNames.Length ? muscleNames[i] : RootNames[i - muscleNames.Length]), Linear(curves[i]));
                var settings = AnimationUtility.GetAnimationClipSettings(result);
                settings.startTime = 0; settings.stopTime = 1; settings.loopTime = false;
                // These are Unity's "Bake Into Pose" switches, even when Loop Time is off.
                // Without them a sideways humanoid's changing projected body axis is extracted
                // as root yaw/translation, making MotionTime rotate the entire body on the floor.
                settings.loopBlendOrientation = settings.loopBlendPositionY = settings.loopBlendPositionXZ = true;
                settings.keepOriginalPositionXZ = settings.keepOriginalPositionY = settings.keepOriginalOrientation = true;
                AnimationUtility.SetAnimationClipSettings(result, settings);
                result.EnsureQuaternionContinuity();
                if (contactStrength > 0)
                {
                    // Use body vertices weighted to humanoid bones, excluding free hair/skirt bones.
                    // If a mesh is unavailable, bone positions provide a deterministic fallback.
                    using var support = new SupportSurface(animator);
                    float Minimum(float time)
                    {
                        Restore(); result.SampleAnimation(clone, time);
                        clone.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                        return support.Minimum();
                    }
                    var floors = new[] { Minimum(0), Minimum(.5f), Minimum(1) };
                    var raw = AnimationUtility.GetEditorCurve(result, Binding("RootT.y"));
                    var keys = new List<Keyframe>();
                    for (var frame = 0; frame <= Steps; frame++)
                    {
                        var time = frame / (float)Steps;
                        var target = time <= .5f ? Mathf.Lerp(floors[0], floors[1], time * 2) : Mathf.Lerp(floors[1], floors[2], (time - .5f) * 2);
                        var lift = (target - Minimum(time)) / animator.humanScale * contactStrength;
                        // Exactly preserve the authored center and endpoints, including their height.
                        keys.Add(new Keyframe(time, raw.Evaluate(time) + (frame == 0 || frame == Steps / 2 || frame == Steps ? 0 : lift)));
                    }
                    AnimationUtility.SetEditorCurve(result, Binding("RootT.y"), Linear(keys));
                }
                return result;
            }
            catch { Object.DestroyImmediate(result); throw; }
            finally { Object.DestroyImmediate(clone); }
        }

        private static EditorCurveBinding Binding(string name) => EditorCurveBinding.FloatCurve("", typeof(Animator), name);
        private static AnimationCurve Linear(List<Keyframe> keys)
        {
            var curve = new AnimationCurve(keys.ToArray());
            for (var i = 0; i < curve.length; i++)
            { AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear); AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear); }
            return curve;
        }
        private sealed class SupportSurface : IDisposable
        {
            private readonly List<(SkinnedMeshRenderer renderer, int[] vertices)> surfaces = new();
            private readonly Transform[] bones;
            private readonly Mesh mesh = new Mesh();
            public SupportSurface(Animator animator)
            {
                bones = Enumerable.Range(0, (int)HumanBodyBones.LastBone).Select(i => animator.GetBoneTransform((HumanBodyBones)i)).Where(b => b != null).ToArray();
                var human = new HashSet<Transform>(bones);
                foreach (var renderer in animator.GetComponentsInChildren<SkinnedMeshRenderer>())
                {
                    if (!renderer.enabled || renderer.sharedMesh == null) continue;
                    try
                    {
                        var mapped = renderer.bones.Select(human.Contains).ToArray();
                        bool IsHuman(int index) => index >= 0 && index < mapped.Length && mapped[index];
                        var weights = renderer.sharedMesh.boneWeights;
                        var vertices = Enumerable.Range(0, weights.Length).Where(i =>
                        {
                            var w = weights[i];
                            return (IsHuman(w.boneIndex0) ? w.weight0 : 0) + (IsHuman(w.boneIndex1) ? w.weight1 : 0) +
                                (IsHuman(w.boneIndex2) ? w.weight2 : 0) + (IsHuman(w.boneIndex3) ? w.weight3 : 0) >= .5f;
                        }).ToArray();
                        if (vertices.Length > 0) surfaces.Add((renderer, vertices));
                    }
                    catch (UnityException) { /* Non-readable external meshes use the bone fallback. */ }
                }
            }
            public float Minimum()
            {
                var min = float.PositiveInfinity;
                foreach (var surface in surfaces)
                {
                    surface.renderer.BakeMesh(mesh); var vertices = mesh.vertices;
                    foreach (var index in surface.vertices) min = Mathf.Min(min, surface.renderer.transform.TransformPoint(vertices[index]).y);
                }
                return float.IsInfinity(min) ? bones.Min(b => b.position.y) : min;
            }
            public void Dispose() => Object.DestroyImmediate(mesh);
        }
    }
}
