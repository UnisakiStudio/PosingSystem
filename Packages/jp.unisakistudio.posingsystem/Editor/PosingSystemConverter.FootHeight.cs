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
        public const string FootHeightAdditiveLayer = "USSPS_FootHeightAdditive";
        private const string FootHeightOffsetPrefix = "_USSPS_footheight_offset_";

        private static bool UsesAdditiveFootHeight(IEnumerable<PosingSystem> systems) => systems.Any(p =>
            p.tag != "EditorOnly" && p.features != null && p.features.footHeight &&
            p.features.footHeightBackend == PosingSystemFeatureSettings.FootHeightBackend.PoseSpaceAdditive);

        private static void BuildAdditiveFootHeight(AnimatorController controller, AnimatorControllerLayer main, IAssetSaver saver)
        {
            FeatureParameter(controller, "FootHeight", AnimatorControllerParameterType.Float, .5f);
            var zero = RootOffsetClip(Vector3.zero, Quaternion.identity, FootHeightOffsetPrefix + "Zero");
            SaveGeneratedAsset(zero, controller, saver);
            var layer = FeatureLayer(controller, FootHeightAdditiveLayer, AvatarMaskBodyPart.Root, true, saver);
            layer.syncedLayerIndex = Array.FindIndex(controller.layers, l => l.name == main.name);
            layer.syncedLayerAffectsTiming = false;
            var cache = new Dictionary<Motion, Motion>();
            foreach (var state in AllFeatureStates(main.stateMachine))
            {
                Motion correction = zero;
                if (state.motion is BlendTree height && height.blendParameter == "FootHeight" &&
                    WithoutFootHeight(height) is AnimationClip neutral && !neutral.name.StartsWith("proxy_"))
                {
                    // Also compacts older prebuilds when they are converted to the additive height correction.
                    height.children = new[] { new ChildMotion { motion = neutral, threshold = .5f, timeScale = 1 } };
                    correction = Project(neutral);
                }
                layer.SetOverrideMotion(state, correction);
                // Locomotion's tracking controls/drivers must execute only once.
                layer.SetOverrideBehaviours(state, Array.Empty<StateMachineBehaviour>());
            }
            var layers = controller.layers;
            layers[layers.Length - 1] = layer;
            controller.layers = layers;

            Motion Project(Motion source)
            {
                if (source == null || source.name.StartsWith("proxy_")) return zero;
                if (cache.TryGetValue(source, out var cached)) return cached;
                // Inverse rotations cannot be linearly blended to undo an arbitrary pose blend.
                // Keep existing nested BlendTree motions on ClipLevel until separately validated.
                if (source is not AnimationClip clip) return zero;
                var down = CreateFootHeightOffsetClip(clip, -2, zero);
                var up = CreateFootHeightOffsetClip(clip, 2, zero);
                SaveGeneratedAsset(down, controller, saver);
                SaveGeneratedAsset(up, controller, saver);
                var blend = new BlendTree { name = FootHeightOffsetPrefix + clip.name,
                    blendType = BlendTreeType.Simple1D, blendParameter = "FootHeight", useAutomaticThresholds = false };
                blend.AddChild(down, 0);
                blend.AddChild(zero, .5f);
                blend.AddChild(up, 1);
                SaveGeneratedAsset(blend, controller, saver);
                cache[source] = blend;
                return blend;
            }
        }

        /// <summary>Height in the legacy clip-level units. Humanoid additive translation is in body space.</summary>
        public static AnimationClip CreateFootHeightOffsetClip(AnimationClip source, float height, AnimationClip reference)
        {
            var result = RootOffsetClip(Vector3.zero, Quaternion.identity, FootHeightOffsetPrefix + source.name + (height < 0 ? "_Down" : "_Up"));
            var duration = Mathf.Max(source.length, 1f / 60);
            var rotation = "xyzw".Select(axis => AnimationUtility.GetEditorCurve(source,
                EditorCurveBinding.FloatCurve("", typeof(Animator), "RootQ." + axis))).ToArray();
            var times = new SortedSet<float> { 0, duration };
            // Static poses only need two keys. Moving poses follow the changing body orientation.
            if (rotation.Any(c => c != null && c.keys.Any(k => !Mathf.Approximately(k.value, c.keys[0].value) ||
                    Mathf.Abs(k.inTangent) > .000001f || Mathf.Abs(k.outTangent) > .000001f)))
            {
                var frames = Mathf.CeilToInt(duration * Mathf.Max(60, source.frameRate));
                for (var frame = 1; frame < frames; frame++) times.Add(duration * frame / frames);
                foreach (var curve in rotation.Where(c => c != null))
                foreach (var key in curve.keys)
                    if (key.time > 0 && key.time < duration) times.Add(key.time);
            }
            var samples = times.Select(time =>
            {
                var q = new Quaternion(rotation[0]?.Evaluate(time) ?? 0, rotation[1]?.Evaluate(time) ?? 0,
                    rotation[2]?.Evaluate(time) ?? 0, rotation[3]?.Evaluate(time) ?? 1).normalized;
                return Quaternion.Inverse(q) * Vector3.up * height;
            }).ToArray();
            var sampleTimes = times.ToArray();
            for (var axis = 0; axis < 3; axis++)
            {
                var curve = new AnimationCurve(sampleTimes.Select((t, i) => new Keyframe(t, samples[i][axis])).ToArray());
                for (var key = 0; key < curve.length; key++)
                {
                    AnimationUtility.SetKeyLeftTangentMode(curve, key, AnimationUtility.TangentMode.Linear);
                    AnimationUtility.SetKeyRightTangentMode(curve, key, AnimationUtility.TangentMode.Linear);
                }
                AnimationUtility.SetEditorCurve(result, EditorCurveBinding.FloatCurve("", typeof(Animator), "RootT." + "xyz"[axis]), curve);
            }
            // Match source duration so Motion Time, speed, cycle offsets and nested blend trees stay aligned.
            foreach (var binding in AnimationUtility.GetCurveBindings(result).Where(b => !b.propertyName.StartsWith("RootT.")))
            {
                var value = AnimationUtility.GetEditorCurve(result, binding).Evaluate(0);
                AnimationUtility.SetEditorCurve(result, binding, AnimationCurve.Constant(0, duration, value));
            }
            result.frameRate = source.frameRate;
            var settings = AnimationUtility.GetAnimationClipSettings(result);
            settings.loopTime = AnimationUtility.GetAnimationClipSettings(source).loopTime;
            AnimationUtility.SetAnimationClipSettings(result, settings);
            AnimationUtility.SetAdditiveReferencePose(result, reference, 0);
            return result;
        }
    }
}
#endif
