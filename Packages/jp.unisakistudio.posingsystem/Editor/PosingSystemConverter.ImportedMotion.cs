#if NDMF && MODULAR_AVATAR
using System;
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.ndmf;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    public partial class PosingSystemConverter
    {
        // Shared by authored poses and imported motions. Locomotion deliberately does not use
        // pose head-centering or product-reference-avatar offsets: those would change walking.
        private static void ApplyClipAdjustments(PosingSystem ps, GameObject workingAvatar,
            AnimationClip source, AnimationClip result, AnimationClip adjustment, bool avatarAdjustment)
        {
            using var languageScope = PosingSystemLocalization.ForInspector(ps);
            if (adjustment != null)
            {
                AdditiveCurveUtility.MultiplyRootQCurves(source, adjustment, result);
                foreach (var binding in AnimationUtility.GetCurveBindings(adjustment))
                {
                    if (!AdditiveCurveUtility.IsRootTBinding(binding)) continue;
                    AnimationUtility.SetEditorCurve(result, binding, AdditiveCurveUtility.AddCurve(
                        AnimationUtility.GetEditorCurve(result, binding) ?? new AnimationCurve(),
                        AnimationUtility.GetEditorCurve(adjustment, binding)));
                }
            }
            var combined = avatarAdjustment
                ? PosingSystemAvatarAdjustmentUtility.CreateCombinedMuscleAdjustmentClip(ps, adjustment)
                : adjustment;
            try
            {
                var indices = AdditiveCurveUtility.GetAdjustedMuscleIndices(combined);
                if (avatarAdjustment)
                    indices.UnionWith(PosingSystemAvatarAdjustmentUtility.GetAffectedMuscleIndices(ps));
                if (indices.Count > 0)
                {
                    if (workingAvatar == null)
                        throw new InvalidOperationException(PosingSystemLocalization.TextMessage("取り込みモーションの調整に有効なHumanoidアバターが必要です。"));
                    ApplyMuscleAdjustmentsWithRebake(workingAvatar, source, combined, result, indices, ps);
                }
            }
            finally { if (avatarAdjustment && combined != null) Object.DestroyImmediate(combined); }
        }

        private static void ApplyClipRotation(AnimationClip clip, PosingSystem.BaseAnimationDefine definition)
        {
            if (!definition.isRotate) return;
            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.orientationOffsetY += definition.rotate;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
        }

        private static void ConfigureMotionTime(AnimatorController controller, AnimatorState state,
            string parameter, float defaultValue = 0)
        {
            state.timeParameterActive = !string.IsNullOrWhiteSpace(parameter);
            state.timeParameter = state.timeParameterActive ? parameter : "";
            if (state.timeParameterActive)
                FeatureParameter(controller, parameter, AnimatorControllerParameterType.Float, defaultValue);
        }

        private static Motion PrepareImportedMotion(PosingSystem ps, PosingSystem.OverrideAnimationDefine definition,
            AnimatorController controller, IAssetSaver saver)
        {
            using var languageScope = PosingSystemLocalization.ForInspector(ps);
            var idleTime = definition.stateType == PosingSystem.OverrideAnimationDefine.AnimationStateType.Stand &&
                definition.isMotionTime && !string.IsNullOrWhiteSpace(definition.motionTimeParamName);
            if (!definition.isRotate && definition.adjustmentClip == null && !idleTime)
                return definition.animationClip;
            var copies = new Dictionary<Motion, Motion>();
            GameObject workingAvatar = null;
            try
            {
                if (AdditiveCurveUtility.GetAdjustedMuscleIndices(definition.adjustmentClip).Count > 0)
                {
                    var avatar = ps.GetAvatar();
                    var animator = avatar != null ? avatar.GetComponent<Animator>() : null;
                    if (animator == null || animator.avatar == null || !animator.isHuman)
                        throw new InvalidOperationException(PosingSystemLocalization.TextMessage("取り込みモーションの調整に有効なHumanoidアバターが必要です。"));
                    workingAvatar = Object.Instantiate(avatar.gameObject, PosingSystemEditor.GetPreviewAvatarRoot());
                    workingAvatar.name = "_PosingSystem_ImportedMotionAdjustment";
                    workingAvatar.hideFlags = HideFlags.HideAndDontSave;
                    workingAvatar.SetActive(true);
                }
                return Process(definition.animationClip);
            }
            finally { if (workingAvatar != null) Object.DestroyImmediate(workingAvatar); }

            void Save(Object value)
            {
                if (saver != null || EditorUtility.IsPersistent(controller)) SaveGeneratedAsset(value, controller, saver);
            }

            Motion Process(Motion source)
            {
                if (source == null) return null;
                if (copies.TryGetValue(source, out var existing)) return existing;
                if (source is BlendTree tree)
                {
                    var copy = new BlendTree();
                    EditorUtility.CopySerialized(tree, copy);
                    copy.name = "_USSPS_Import_" + tree.name;
                    copies.Add(source, copy);
                    var children = copy.children;
                    for (var i = 0; i < children.Length; i++) children[i].motion = Process(children[i].motion);
                    copy.children = children;
                    Save(copy);
                    return copy;
                }
                if (source is not AnimationClip clip) return source;
                var processed = Object.Instantiate(clip);
                processed.name = "_USSPS_Import_" + clip.name;
                // Sample the unmirrored source when baking muscle offsets, then retain its mirror setting.
                var sampleSource = Object.Instantiate(clip);
                try
                {
                    var settings = AnimationUtility.GetAnimationClipSettings(sampleSource);
                    settings.mirror = false;
                    AnimationUtility.SetAnimationClipSettings(sampleSource, settings);
                    ApplyClipAdjustments(ps, workingAvatar, sampleSource, processed, definition.adjustmentClip, false);
                    ApplyClipRotation(processed, definition);
                    if (idleTime)
                    {
                        var timed = CreateImportedIdleTimeMotion(processed, definition.motionTimeParamName, controller, Save);
                        copies.Add(source, timed);
                        if (timed != processed) Object.DestroyImmediate(processed);
                        return timed;
                    }
                    copies.Add(source, processed);
                    Save(processed);
                    return processed;
                }
                catch { if (processed != null && !EditorUtility.IsPersistent(processed)) Object.DestroyImmediate(processed); throw; }
                finally { Object.DestroyImmediate(sampleSource); }
            }
        }

        // Motion Time belongs to an AnimatorState, not a BlendTree child. For an idle-only override,
        // sample just that child at its authored frame rate; never pause the surrounding walking tree.
        private static Motion CreateImportedIdleTimeMotion(AnimationClip clip, string parameter,
            AnimatorController controller, Action<Object> save)
        {
            FeatureParameter(controller, parameter, AnimatorControllerParameterType.Float);
            var tree = new BlendTree { name = clip.name + "_MotionTime", blendType = BlendTreeType.Simple1D,
                blendParameter = parameter, useAutomaticThresholds = false };
            var duration = clip.length;
            var frameCount = Math.Max(1, Mathf.CeilToInt(duration * Math.Max(1, clip.frameRate)));
            var curves = AnimationUtility.GetCurveBindings(clip).Select(binding =>
                (binding, curve: AnimationUtility.GetEditorCurve(clip, binding))).ToArray();
            var objectCurves = AnimationUtility.GetObjectReferenceCurveBindings(clip).Select(binding =>
                (binding, keys: AnimationUtility.GetObjectReferenceCurve(clip, binding))).ToArray();
            if (curves.All(item => item.curve.keys.All(key => key.value == item.curve.keys[0].value &&
                    (key.inTangent == 0 || float.IsInfinity(key.inTangent)) &&
                    (key.outTangent == 0 || float.IsInfinity(key.outTangent)))) &&
                objectCurves.All(item => item.keys.All(key => key.value == item.keys[0].value)))
            {
                // Static poses do not need one duplicate asset per frame.
                Object.DestroyImmediate(tree);
                AnimationUtility.SetAnimationEvents(clip, Array.Empty<AnimationEvent>());
                save(clip);
                return clip;
            }
            for (var frame = 0; frame <= frameCount; frame++)
            {
                var t = duration * frame / frameCount;
                var snapshot = Object.Instantiate(clip);
                snapshot.name = clip.name + "_Frame" + frame;
                snapshot.ClearCurves();
                AnimationUtility.SetAnimationEvents(snapshot, Array.Empty<AnimationEvent>());
                foreach (var item in curves)
                    AnimationUtility.SetEditorCurve(snapshot, item.binding,
                        AnimationCurve.Constant(0, 1, item.curve.Evaluate(t)));
                foreach (var item in objectCurves)
                {
                    if (item.keys.Length == 0) continue;
                    var key = item.keys[0];
                    foreach (var candidate in item.keys) { if (candidate.time > t) break; key = candidate; }
                    AnimationUtility.SetObjectReferenceCurve(snapshot, item.binding,
                        new[] { new ObjectReferenceKeyframe { time = 0, value = key.value } });
                }
                save(snapshot);
                tree.AddChild(snapshot, (float)frame / frameCount);
            }
            save(tree);
            return tree;
        }
    }
}
#endif
