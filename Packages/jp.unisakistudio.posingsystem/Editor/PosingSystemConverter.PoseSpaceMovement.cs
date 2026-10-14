#if NDMF && MODULAR_AVATAR
using System;
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    public partial class PosingSystemConverter
    {
        public const string PoseSpaceMovementPrefix = "USSPS_PoseSpaceMovement";
        private const string MovementOffsetPrefix = "_USSPS_movement_offset_";
        private const string MovementDistance = "USSPS_MovementDistance";
        private const string MovementHeight = "USSPS_MovementHeight";
        private const string MovementYaw = "USSPS_MovementYaw";
        private const string MovementMirroredYaw = "USSPS_MovementMirroredYaw";
        private static readonly float[] MovementYawSamples = { -.5f, -.25f, 0f, .25f, .5f, .75f, 1f, 1.25f, 1.5f };

        private static void BuildPoseSpaceMovement(PosingSystem ps, AnimatorController controller,
            AnimatorControllerLayer main, PosingSystem photo, PosingSystem floating, IAssetSaver saver)
        {
            var usePhoto = photo != null && photo.features.photographyBackend == PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive;
            var useFloat = floating != null && floating.features.floatingBackend == PosingSystemFeatureSettings.MovementBackend.PoseSpaceAdditive;
            if (!usePhoto && !useFloat) return;
            MeasureRootTranslationUnits(ps.GetAvatar().gameObject, out var units);
            var zero = RootOffsetClip(Vector3.zero, Quaternion.identity, MovementOffsetPrefix + "Zero");
            SaveGeneratedAsset(zero, controller, saver);
            if (usePhoto) BuildPhotographyInputs(controller, saver, true);
            if (useFloat)
            {
                FeatureParameter(controller, MovementHeight, AnimatorControllerParameterType.Float);
                var gate = FeatureLayer(controller, PoseSpaceMovementPrefix + "_FloatInput", AvatarMaskBodyPart.Root, false, saver);
                var off = gate.stateMachine.AddState("浮遊オフ");
                var on = gate.stateMachine.AddState("浮遊");
                gate.stateMachine.defaultState = off;
                off.motion = HeightInput(0);
                var blend = Tree("FloatInput", PosingSystemFeatureUtility.FloatHeight);
                blend.AddChild(HeightInput(0), 0); blend.AddChild(HeightInput(1), 1);
                SaveGeneratedAsset(blend, controller, saver);
                on.motion = blend;
                off.writeDefaultValues = on.writeDefaultValues = false;
                FeatureTransition(off.AddTransition(on)).AddCondition(AnimatorConditionMode.If, 0, PosingSystemFeatureUtility.FloatActive);
                FeatureTransition(on.AddTransition(off)).AddCondition(AnimatorConditionMode.IfNot, 0, PosingSystemFeatureUtility.FloatActive);
                PersistFeatureStates(gate, controller, saver);
            }

            // Translation layers must precede yaw: their inverse is computed from the original pose's RootQ.
            if (useFloat) AddCorrection("Vertical", MovementHeight, Vector3.up * 3 / Mathf.Max(.001f, Mathf.Abs(units.y)), false);
            if (usePhoto)
            {
                AddCorrection("Forward", MovementDistance, Vector3.forward * 3 / Mathf.Max(.001f, Mathf.Abs(units.z)), false);
                AddCorrection("Yaw", MovementYaw, Vector3.zero, true);
            }

            void AddCorrection(string kind, string parameter, Vector3 offset, bool rotation)
            {
                var layer = FeatureLayer(controller, PoseSpaceMovementPrefix + "_" + kind, AvatarMaskBodyPart.Root, true, saver);
                layer.syncedLayerIndex = Array.FindIndex(controller.layers, l => l.name == main.name);
                layer.syncedLayerAffectsTiming = false;
                var cache = new Dictionary<(AnimationClip, string, bool), Motion>();
                foreach (var state in AllFeatureStates(main.stateMachine))
                {
                    Motion motion = zero;
                    // Blending inverse rotations does not invert a blended rotation. Do not move proxy locomotion
                    // or nested pose BlendTrees in this comparison backend, rather than produce a wrong direction.
                    if (WithoutFootHeight(state.motion) is AnimationClip source && !source.name.StartsWith("proxy_"))
                    {
                        var input = rotation && state.mirrorParameterActive && state.mirrorParameter == "USSPS_Mirror" ? MovementMirroredYaw : parameter;
                        var reverse = rotation && !state.mirrorParameterActive && state.mirror;
                        var key = (source, input, reverse);
                        if (!cache.TryGetValue(key, out motion))
                        {
                            var tree = Tree(MovementOffsetPrefix + kind + "_" + source.name, input);
                            foreach (var value in rotation ? MovementYawSamples : new[] { 0f, 1f })
                            {
                                if (rotation ? Mathf.Approximately(value, .5f) : value == 0) tree.AddChild(zero, value);
                                else
                                {
                                    var yaw = rotation ? Quaternion.Euler(0, (value - .5f) * 360 * (reverse ? -1 : 1), 0) : Quaternion.identity;
                                    var clip = CreatePoseSpaceOffsetClip(source, offset * value, yaw, zero);
                                    clip.name = MovementOffsetPrefix + kind + "_" + source.name + "_" + value;
                                    SaveGeneratedAsset(clip, controller, saver);
                                    tree.AddChild(clip, value);
                                }
                            }
                            SaveGeneratedAsset(tree, controller, saver);
                            cache[key] = motion = tree;
                        }
                    }
                    layer.SetOverrideMotion(state, motion);
                    layer.SetOverrideBehaviours(state, Array.Empty<StateMachineBehaviour>());
                }
                var layers = controller.layers;
                layers[layers.Length - 1] = layer;
                controller.layers = layers;
            }

            AnimationClip HeightInput(float value)
            {
                var clip = new AnimationClip { name = "FloatInput_" + value };
                SetInput(clip, MovementHeight, value);
                SaveGeneratedAsset(clip, controller, saver);
                return clip;
            }
            void SetInput(AnimationClip clip, string parameter, float value) => AnimationUtility.SetEditorCurve(clip,
                EditorCurveBinding.FloatCurve("", typeof(Animator), parameter), AnimationCurve.Constant(0, 1, value));
            BlendTree Tree(string name, string parameter) => new() { name = name, blendParameter = parameter,
                blendType = BlendTreeType.Simple1D, useAutomaticThresholds = false };
        }

        private static void BuildPhotographyInputs(AnimatorController controller, IAssetSaver saver, bool mirrorPoses)
        {
            FeatureParameter(controller, "USSPS_Mirror", AnimatorControllerParameterType.Bool);
            FeatureParameter(controller, MovementDistance, AnimatorControllerParameterType.Float);
            FeatureParameter(controller, MovementYaw, AnimatorControllerParameterType.Float, .5f);
            FeatureParameter(controller, MovementMirroredYaw, AnimatorControllerParameterType.Float, .5f);
            var gate = FeatureLayer(controller, PoseSpaceMovementPrefix + "_PhotoInputs", AvatarMaskBodyPart.Root, false, saver);
            var off = gate.stateMachine.AddState("撮影オフ");
            off.motion = InputClip(0, .5f, .5f);
            gate.stateMachine.defaultState = off;
            var active = new List<(AnimatorState state, bool mirror, bool facingViewer)>();
            foreach (var mirror in mirrorPoses ? new[] { false, true } : new[] { false })
                foreach (var facingViewer in new[] { false, true })
                {
                    var on = gate.stateMachine.AddState((facingViewer ? "こちら向き" : "元の向き") + (mirror ? "・左右反転" : ""));
                    active.Add((on, mirror, facingViewer));
                    var distance = Tree("PhotoInputs", PosingSystemFeatureUtility.PhotoDistance);
                    foreach (var d in new[] { 0f, 1f })
                    {
                        var yaw = Tree("YawInputs", PosingSystemFeatureUtility.PhotoYaw);
                        foreach (var y in new[] { 0f, 1f })
                        {
                            // Extend the internal range instead of wrapping 360 degrees inside a linear blend.
                            // The synced radial stays 0..1; its center adds 180 degrees only when displaced.
                            var value = y + (facingViewer ? .5f : 0);
                            yaw.AddChild(InputClip(d, value, mirror ? 1 - value : value), y);
                        }
                        SaveGeneratedAsset(yaw, controller, saver);
                        distance.AddChild(yaw, d);
                    }
                    SaveGeneratedAsset(distance, controller, saver);
                    on.motion = distance;
                    FeatureTransition(on.AddTransition(off)).AddCondition(AnimatorConditionMode.IfNot, 0, PosingSystemFeatureUtility.PhotoActive);
                }
            foreach (var from in active.Select(a => a.state).Prepend(off))
                foreach (var target in active.Where(a => a.state != from))
                {
                    var transition = FeatureTransition(from.AddTransition(target.state));
                    transition.AddCondition(AnimatorConditionMode.If, 0, PosingSystemFeatureUtility.PhotoActive);
                    if (mirrorPoses) transition.AddCondition(target.mirror ? AnimatorConditionMode.If : AnimatorConditionMode.IfNot, 0, "USSPS_Mirror");
                    transition.AddCondition(target.facingViewer ? AnimatorConditionMode.Greater : AnimatorConditionMode.Less,
                        PosingSystemFeatureUtility.PhotoDistanceThreshold, PosingSystemFeatureUtility.PhotoDistance);
                }
            foreach (var state in AllFeatureStates(gate.stateMachine)) state.writeDefaultValues = false;
            PersistFeatureStates(gate, controller, saver);

            AnimationClip InputClip(float distance, float yaw, float mirroredYaw)
            {
                var clip = new AnimationClip { name = "PhotoInput" };
                Set(MovementDistance, distance); Set(MovementYaw, yaw); Set(MovementMirroredYaw, mirroredYaw);
                SaveGeneratedAsset(clip, controller, saver);
                return clip;
                void Set(string parameter, float value) => AnimationUtility.SetEditorCurve(clip,
                    EditorCurveBinding.FloatCurve("", typeof(Animator), parameter), AnimationCurve.Constant(0, 1, value));
            }
            BlendTree Tree(string name, string parameter) => new() { name = name, blendParameter = parameter,
                blendType = BlendTreeType.Simple1D, useAutomaticThresholds = false };
        }

        /// <summary>Converts an avatar-space displacement and yaw to the additive body space of a pose.</summary>
        public static AnimationClip CreatePoseSpaceOffsetClip(AnimationClip source, Vector3 translation, Quaternion rotation, AnimationClip reference)
        {
            var result = RootOffsetClip(Vector3.zero, Quaternion.identity, MovementOffsetPrefix + source.name);
            var duration = Mathf.Max(source.length, 1f / 60);
            var curves = "xyzw".Select(axis => AnimationUtility.GetEditorCurve(source,
                EditorCurveBinding.FloatCurve("", typeof(Animator), "RootQ." + axis))).ToArray();
            var times = new SortedSet<float> { 0, duration };
            if (curves.Any(c => c != null && c.keys.Any(k => !Mathf.Approximately(k.value, c.keys[0].value) ||
                    Mathf.Abs(k.inTangent) > .000001f || Mathf.Abs(k.outTangent) > .000001f)))
            {
                var frames = Mathf.CeilToInt(duration * Mathf.Max(60, source.frameRate));
                for (var frame = 1; frame < frames; frame++) times.Add(duration * frame / frames);
                foreach (var c in curves.Where(c => c != null)) foreach (var key in c.keys)
                    if (key.time > 0 && key.time < duration) times.Add(key.time);
            }
            var sourceSettings = AnimationUtility.GetAnimationClipSettings(source);
            var samples = times.Select(time =>
            {
                var q = new Quaternion(curves[0]?.Evaluate(time) ?? 0, curves[1]?.Evaluate(time) ?? 0,
                    curves[2]?.Evaluate(time) ?? 0, curves[3]?.Evaluate(time) ?? 1).normalized;
                if (sourceSettings.mirror) q = new Quaternion(q.x, -q.y, -q.z, q.w);
                return (position: Quaternion.Inverse(q) * translation, rotation: Quaternion.Inverse(q) * rotation * q);
            }).ToArray();
            var sampleTimes = times.ToArray();
            for (var axis = 0; axis < 7; axis++)
            {
                var curve = new AnimationCurve(sampleTimes.Select((t, i) => new Keyframe(t,
                    axis < 3 ? samples[i].position[axis] : samples[i].rotation[axis - 3])).ToArray());
                for (var key = 0; key < curve.length; key++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, key, AnimationUtility.TangentMode.Linear);
                    AnimationUtility.SetKeyRightTangentMode(curve, key, AnimationUtility.TangentMode.Linear);
                }
                AnimationUtility.SetEditorCurve(result, EditorCurveBinding.FloatCurve("", typeof(Animator),
                    axis < 3 ? "RootT." + "xyz"[axis] : "RootQ." + "xyzw"[axis - 3]), curve);
            }
            AnimationUtility.SetEditorCurve(result, EditorCurveBinding.FloatCurve("", typeof(Animator), "Jaw Left-Right"), AnimationCurve.Constant(0, duration, 0));
            result.frameRate = source.frameRate;
            var settings = AnimationUtility.GetAnimationClipSettings(result);
            settings.loopTime = sourceSettings.loopTime;
            settings.hasAdditiveReferencePose = true;
            settings.additiveReferencePoseClip = reference;
            settings.additiveReferencePoseTime = 0;
            AnimationUtility.SetAnimationClipSettings(result, settings);
            AnimationUtility.SetAdditiveReferencePose(result, reference, 0);
            return result;
        }
    }
}
#endif
