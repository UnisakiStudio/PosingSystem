using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor
{
    /// <summary>
    /// Generates and applies a constant Humanoid-muscle offset which translates poses authored
    /// on the product's reference avatar to the avatar containing the PosingSystem component.
    /// </summary>
    public static class PosingSystemAvatarAdjustmentUtility
    {
        public const int LeftKneeMuscleIndex = 24;
        public const int RightKneeMuscleIndex = 32;
        public const int LeftElbowMuscleIndex = 42;
        public const int RightElbowMuscleIndex = 51;

        private const float CurveEpsilon = 0.00001f;
        private const string GeneratedRoot = "Assets/UnisakiStudio/GeneratedResources";

        /// <summary>Transient knee limits. Comparison previews supply the target avatar's baseline,
        /// rather than reusing the reference avatar's generated adjustment data.</summary>
        public readonly struct KneeSafety
        {
            public bool Enabled { get; }
            private readonly Vector2 baseline;
            private readonly float minimumAngle;

            internal KneeSafety(Vector2 baseline, float minimumAngle)
            {
                Enabled = true;
                this.baseline = baseline;
                this.minimumAngle = minimumAngle;
            }

            public bool Apply(ref HumanPose pose)
            {
                if (!Enabled || pose.muscles == null || pose.muscles.Length <= RightKneeMuscleIndex)
                    return false;
                var left = pose.muscles[LeftKneeMuscleIndex];
                var right = pose.muscles[RightKneeMuscleIndex];
                ClampKnee(pose.muscles, LeftKneeMuscleIndex, baseline.x, minimumAngle);
                ClampKnee(pose.muscles, RightKneeMuscleIndex, baseline.y, minimumAngle);
                return left != pose.muscles[LeftKneeMuscleIndex] || right != pose.muscles[RightKneeMuscleIndex];
            }

            public void ApplyToAnimator(Animator animator)
            {
                if (!Enabled || !IsValidHumanoid(animator)) return;
                using var handler = new HumanPoseHandler(animator.avatar, animator.transform);
                var pose = new HumanPose();
                handler.GetHumanPose(ref pose);
                var leftMuscle = pose.muscles[LeftKneeMuscleIndex];
                var rightMuscle = pose.muscles[RightKneeMuscleIndex];
                if (!Apply(ref pose)) return;

                // Get/SetHumanPose is not an identity round trip for a translated/scaled avatar:
                // writing its body position can move the hips by several metres. It also retargets
                // unrelated bones. Use it only to calculate the corrected knee rotations, restoring
                // the complete local pose before committing those rotations alone.
                var leftKnee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
                var rightKnee = animator.GetBoneTransform(HumanBodyBones.RightLowerLeg);
                var transforms = animator.GetComponentsInChildren<Transform>(true);
                var original = new LocalTransformPose[transforms.Length];
                for (var i = 0; i < transforms.Length; i++) original[i] = new LocalTransformPose(transforms[i]);
                Quaternion leftRotation, rightRotation;
                try
                {
                    handler.SetHumanPose(ref pose);
                    leftRotation = leftKnee != null ? leftKnee.localRotation : Quaternion.identity;
                    rightRotation = rightKnee != null ? rightKnee.localRotation : Quaternion.identity;
                }
                finally
                {
                    for (var i = 0; i < transforms.Length; i++) original[i].Restore(transforms[i]);
                }
                if (leftKnee != null && leftMuscle != pose.muscles[LeftKneeMuscleIndex]) leftKnee.localRotation = leftRotation;
                if (rightKnee != null && rightMuscle != pose.muscles[RightKneeMuscleIndex]) rightKnee.localRotation = rightRotation;
            }
        }

        private readonly struct LocalTransformPose
        {
            private readonly Vector3 position;
            private readonly Quaternion rotation;
            private readonly Vector3 scale;

            public LocalTransformPose(Transform transform)
            {
                position = transform.localPosition;
                rotation = transform.localRotation;
                scale = transform.localScale;
            }

            public void Restore(Transform transform)
            {
                transform.localPosition = position;
                transform.localRotation = rotation;
                transform.localScale = scale;
            }
        }

        public static KneeSafety GetKneeSafety(PosingSystem posingSystem, Vector2? targetBaseline = null)
        {
            if (posingSystem == null || !posingSystem.enableReverseKneeSafetyClamp ||
                string.IsNullOrEmpty(posingSystem.AvatarReferenceProfileResourceName)) return default;
            return new KneeSafety(targetBaseline ?? new Vector2(
                posingSystem.avatarSpecificLeftKneeBendDegrees, posingSystem.avatarSpecificRightKneeBendDegrees),
                posingSystem.reverseKneeSafetyAngle);
        }

        public static bool TryReadKneeBaseline(Animator animator, out Vector2 baseline, out string message)
        {
            baseline = default;
            message = string.Empty;
            if (!IsValidHumanoid(animator))
            {
                message = "膝の計測に必要なHumanoid Animatorがありません。";
                return false;
            }
            if (!TryReadAvatarBaselineOnTemporaryClone(animator, out _, out var bends, out message,
                    restoreImportedPose: true)) return false;
            baseline = new Vector2(bends.leftKnee, bends.rightKnee);
            return true;
        }

        public static bool HasReferenceProfile(PosingSystem posingSystem)
        {
            return TryGetReferenceProfile(posingSystem, out _);
        }

        public static bool TryGetReferenceProfile(
            PosingSystem posingSystem,
            out PosingSystemAvatarReferenceProfile profile)
        {
            profile = null;
            if (posingSystem == null || string.IsNullOrEmpty(posingSystem.AvatarReferenceProfileResourceName))
            {
                return false;
            }

            profile = Resources.Load<PosingSystemAvatarReferenceProfile>(
                posingSystem.AvatarReferenceProfileResourceName);
            return profile != null && profile.IsValid;
        }

        // Transient comparison data; no generated assets or source settings are changed.
        public static bool TryGetPreviewMuscleAdjustment(PosingSystem system, Animator animator,
            out float[] adjustments, out string message)
        {
            adjustments = null;
            message = string.Empty;
            if (!TryGetReferenceProfile(system, out var profile))
            {
                message = "この商品には基準データがありません。";
                return false;
            }
            if (!IsValidHumanoid(animator)) { message = "Humanoid Animatorが必要です。"; return false; }
            if (!TryReadAvatarBaselineOnTemporaryClone(animator, out var muscles, out var bends, out message)) return false;
            adjustments = new float[HumanTrait.MuscleCount];
            for (var i = 0; i < adjustments.Length; i++) adjustments[i] = muscles[i] - profile.referenceMuscles[i];
            SetHingeAdjustment(adjustments, LeftKneeMuscleIndex, bends.leftKnee - profile.leftKneeBendDegrees);
            SetHingeAdjustment(adjustments, RightKneeMuscleIndex, bends.rightKnee - profile.rightKneeBendDegrees);
            SetHingeAdjustment(adjustments, LeftElbowMuscleIndex, bends.leftElbow - profile.leftElbowBendDegrees);
            SetHingeAdjustment(adjustments, RightElbowMuscleIndex, bends.rightElbow - profile.rightElbowBendDegrees);
            return true;
        }
        public static bool TryGenerate(PosingSystem posingSystem, out string message)
        {
            message = string.Empty;
            if (!TryGetReferenceProfile(posingSystem, out var profile))
            {
                message = "この商品には基準アバター姿勢が登録されていません。";
                return false;
            }

            var avatarRoot = GetAvatarRoot(posingSystem);
            var animator = avatarRoot != null ? avatarRoot.GetComponent<Animator>() : null;
            if (!IsValidHumanoid(animator))
            {
                message = "Humanoid の Animator を持つアバターが見つかりません。";
                return false;
            }

            if (!TryReadAvatarBaselineOnTemporaryClone(
                    animator, out var targetMuscles, out var bends, out message))
            {
                return false;
            }

            var adjustments = new float[HumanTrait.MuscleCount];
            for (var i = 0; i < adjustments.Length; i++)
            {
                adjustments[i] = targetMuscles[i] - profile.referenceMuscles[i];
            }

            // Stretch muscle values are not comparable enough across avatars near a straight limb.
            // Physical signed bend is stable there, so use its geometric difference instead.
            SetHingeAdjustment(adjustments, LeftKneeMuscleIndex,
                bends.leftKnee - profile.leftKneeBendDegrees);
            SetHingeAdjustment(adjustments, RightKneeMuscleIndex,
                bends.rightKnee - profile.rightKneeBendDegrees);
            SetHingeAdjustment(adjustments, LeftElbowMuscleIndex,
                bends.leftElbow - profile.leftElbowBendDegrees);
            SetHingeAdjustment(adjustments, RightElbowMuscleIndex,
                bends.rightElbow - profile.rightElbowBendDegrees);

            var signature = BuildSignature(animator, profile);
            var clip = SaveGeneratedClip(posingSystem, animator, adjustments);
            if (clip == null)
            {
                message = "アバター固有補正アニメーションを保存できませんでした。";
                return false;
            }

            Undo.RecordObject(posingSystem, "Generate avatar-specific pose adjustment");
            posingSystem.avatarSpecificAdjustmentClip = clip;
            posingSystem.avatarSpecificAdjustmentSignature = signature;
            posingSystem.avatarSpecificLeftKneeBendDegrees = bends.leftKnee;
            posingSystem.avatarSpecificRightKneeBendDegrees = bends.rightKnee;
            EditorUtility.SetDirty(posingSystem);

            var adjustedCount = adjustments.Count(value => Mathf.Abs(value) > CurveEpsilon);
            message = $"{animator.gameObject.name} 用の補正を生成しました（{adjustedCount} Muscle）。";
            return true;
        }

        public static bool IsGeneratedClipCurrent(PosingSystem posingSystem)
        {
            if (posingSystem == null || posingSystem.avatarSpecificAdjustmentClip == null ||
                !TryGetReferenceProfile(posingSystem, out var profile))
            {
                return false;
            }

            var root = GetAvatarRoot(posingSystem);
            var animator = root != null ? root.GetComponent<Animator>() : null;
            if (!IsValidHumanoid(animator))
            {
                return false;
            }

            return string.Equals(
                posingSystem.avatarSpecificAdjustmentSignature,
                BuildSignature(animator, profile),
                StringComparison.Ordinal);
        }

        public static HashSet<int> GetAffectedMuscleIndices(PosingSystem posingSystem)
        {
            var result = new HashSet<int>();
            if (posingSystem == null || string.IsNullOrEmpty(posingSystem.AvatarReferenceProfileResourceName))
            {
                return result;
            }

            if (!posingSystem.UsesAutoPoseCatalog &&
                posingSystem.enableAvatarSpecificAdjustment && posingSystem.avatarSpecificAdjustmentClip != null)
            {
                result.UnionWith(AdditiveCurveUtility.GetAdjustedMuscleIndices(
                    posingSystem.avatarSpecificAdjustmentClip));
            }

            if (posingSystem.enableReverseKneeSafetyClamp)
            {
                result.Add(LeftKneeMuscleIndex);
                result.Add(RightKneeMuscleIndex);
            }

            return result;
        }

        public static AnimationClip CreateCombinedMuscleAdjustmentClip(
            PosingSystem posingSystem,
            AnimationClip manualAdjustmentClip)
        {
            var clips = new List<AnimationClip>();
            if (manualAdjustmentClip != null)
            {
                clips.Add(manualAdjustmentClip);
            }
            if (posingSystem != null && !posingSystem.UsesAutoPoseCatalog &&
                posingSystem.enableAvatarSpecificAdjustment &&
                posingSystem.avatarSpecificAdjustmentClip != null)
            {
                clips.Add(posingSystem.avatarSpecificAdjustmentClip);
            }

            var result = new AnimationClip
            {
                name = "CombinedPoseAdjustment",
                hideFlags = HideFlags.HideAndDontSave,
                frameRate = 60f
            };
            if (clips.Count == 0)
            {
                return result;
            }

            var times = new HashSet<float> { 0f };
            foreach (var clip in clips)
            {
                foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                {
                    if (!AdditiveCurveUtility.IsMuscleBinding(binding))
                    {
                        continue;
                    }
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    if (curve == null) continue;
                    foreach (var key in curve.keys) times.Add(key.time);
                }
            }

            var sortedTimes = times.OrderBy(value => value).ToArray();
            var curves = new Dictionary<int, AnimationCurve>();
            foreach (var time in sortedTimes)
            {
                var totals = new Dictionary<int, float>();
                foreach (var clip in clips)
                {
                    foreach (var pair in AdditiveCurveUtility.GetMuscleAdjustmentsAtTime(clip, time))
                    {
                        totals[pair.Key] = totals.TryGetValue(pair.Key, out var value)
                            ? value + pair.Value
                            : pair.Value;
                    }
                }
                foreach (var pair in totals)
                {
                    if (!curves.TryGetValue(pair.Key, out var curve))
                    {
                        curve = new AnimationCurve();
                        curves[pair.Key] = curve;
                    }
                    curve.AddKey(new Keyframe(time, pair.Value));
                }
            }

            foreach (var pair in curves)
            {
                if (pair.Key < 0 || pair.Key >= HumanTrait.MuscleCount) continue;
                var binding = EditorCurveBinding.FloatCurve(
                    string.Empty, typeof(Animator), HumanTrait.MuscleName[pair.Key]);
                AnimationUtility.SetEditorCurve(result, binding, pair.Value);
            }
            return result;
        }

        public static void ApplyToPose(
            PosingSystem posingSystem,
            ref HumanPose pose,
            float time = 0f,
            bool? avatarSpecificAdjustmentOverride = null)
        {
            if (posingSystem == null || pose.muscles == null)
            {
                return;
            }

            var applyAvatarSpecificAdjustment = !posingSystem.UsesAutoPoseCatalog &&
                                                (avatarSpecificAdjustmentOverride ??
                                                 posingSystem.enableAvatarSpecificAdjustment);
            if (applyAvatarSpecificAdjustment && posingSystem.avatarSpecificAdjustmentClip != null)
            {
                var values = AdditiveCurveUtility.GetMuscleAdjustmentsAtTime(
                    posingSystem.avatarSpecificAdjustmentClip, time);
                foreach (var pair in values)
                {
                    if (pair.Key >= 0 && pair.Key < pose.muscles.Length)
                    {
                        pose.muscles[pair.Key] += pair.Value;
                    }
                }
            }

            ClampKnees(posingSystem, ref pose);
        }

        public static void ClampKnees(PosingSystem posingSystem, ref HumanPose pose)
        {
            GetKneeSafety(posingSystem).Apply(ref pose);
        }

        public static void ApplyToAnimator(PosingSystem posingSystem, Animator animator, float time = 0f)
        {
            if (!IsValidHumanoid(animator) || posingSystem == null)
            {
                return;
            }

            // GetHumanPose includes the scene placement in bodyPosition, whereas
            // SetHumanPose applies it relative to the avatar root. Round-tripping at
            // a nonzero placement therefore applies that placement a second time.
            // Muscle adjustments do not depend on the scene frame: evaluate them
            // at the origin, then restore the original placement even on failure.
            var root = animator.transform;
            var position = root.position;
            var rotation = root.rotation;
            try
            {
                root.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                using var handler = new HumanPoseHandler(animator.avatar, root);
                var pose = new HumanPose();
                handler.GetHumanPose(ref pose);
                var originalMuscles = (float[])pose.muscles.Clone();
                ApplyToPose(posingSystem, ref pose, time);
                // A no-op must not retarget unrelated bones through Mecanim.
                if (!originalMuscles.SequenceEqual(pose.muscles))
                    handler.SetHumanPose(ref pose);
            }
            finally { root.SetPositionAndRotation(position, rotation); }
        }

        public static bool CaptureReferenceProfile(
            GameObject referenceAvatarRoot,
            string assetPath,
            out string message)
        {
            message = string.Empty;
            var animator = referenceAvatarRoot != null
                ? referenceAvatarRoot.GetComponent<Animator>()
                : null;
            if (!IsValidHumanoid(animator))
            {
                message = "基準アバターに有効な Humanoid Animator がありません。";
                return false;
            }

            if (!TryReadAvatarBaselineOnTemporaryClone(
                    animator, out var muscles, out var bends, out message))
            {
                return false;
            }

            EnsureAssetFolder(Path.GetDirectoryName(assetPath)?.Replace('\\', '/'));
            var profile = AssetDatabase.LoadAssetAtPath<PosingSystemAvatarReferenceProfile>(assetPath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<PosingSystemAvatarReferenceProfile>();
                AssetDatabase.CreateAsset(profile, assetPath);
            }

            profile.profileVersion = 1;
            profile.sourceAvatarName = animator.gameObject.name;
            profile.referenceMuscles = muscles;
            profile.leftKneeBendDegrees = bends.leftKnee;
            profile.rightKneeBendDegrees = bends.rightKnee;
            profile.leftElbowBendDegrees = bends.leftElbow;
            profile.rightElbowBendDegrees = bends.rightElbow;
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath);
            message = $"{animator.gameObject.name} の基準姿勢を保存しました。";
            return true;
        }

        private static void SetHingeAdjustment(float[] adjustments, int muscleIndex, float bendDifferenceDegrees)
        {
            var range = Mathf.Max(0.0001f, HumanTrait.GetMuscleDefaultMax(muscleIndex));
            adjustments[muscleIndex] = bendDifferenceDegrees / range;
        }

        private static void ClampKnee(float[] muscles, int index, float baselineBend, float safetyAngle)
        {
            if (index < 0 || index >= muscles.Length)
            {
                return;
            }

            var range = Mathf.Max(0.0001f, HumanTrait.GetMuscleDefaultMax(index));
            // HumanPoseHandlerの往復誤差分だけ内側へ寄せ、保存後も上限を越えないようにする。
            var maximum = 1f + (baselineBend - Mathf.Max(0f, safetyAngle)) / range - 0.0005f;
            muscles[index] = Mathf.Min(muscles[index], maximum);
        }

        private static AnimationClip SaveGeneratedClip(
            PosingSystem posingSystem,
            Animator animator,
            IReadOnlyList<float> adjustments)
        {
            var avatarFolder = SanitizeFileName(animator.gameObject.name);
            var productFolder = SanitizeFileName(posingSystem.GetType().Name);
            var folder = $"{GeneratedRoot}/{avatarFolder}/{productFolder}";
            EnsureAssetFolder(folder);
            var path = $"{folder}/AvatarSpecificPoseAdjustment.anim";

            var clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip { name = "AvatarSpecificPoseAdjustment" };
                AssetDatabase.CreateAsset(clip, path);
            }
            else
            {
                foreach (var binding in AnimationUtility.GetCurveBindings(clip))
                {
                    AnimationUtility.SetEditorCurve(clip, binding, null);
                }
            }

            for (var i = 0; i < adjustments.Count && i < HumanTrait.MuscleCount; i++)
            {
                if (Mathf.Abs(adjustments[i]) <= CurveEpsilon)
                {
                    continue;
                }

                var binding = EditorCurveBinding.FloatCurve(
                    string.Empty,
                    typeof(Animator),
                    HumanTrait.MuscleName[i]);
                AnimationUtility.SetEditorCurve(clip, binding,
                    AnimationCurve.Constant(0f, 1f / 60f, adjustments[i]));
            }

            clip.frameRate = 60f;
            EditorUtility.SetDirty(clip);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(path);
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
        }

        private static GameObject GetAvatarRoot(PosingSystem posingSystem)
        {
            var descriptor = posingSystem != null ? posingSystem.GetAvatar() : null;
            return descriptor != null ? descriptor.gameObject : null;
        }

        private static bool IsValidHumanoid(Animator animator)
        {
            return animator != null && animator.avatar != null && animator.avatar.isValid && animator.isHuman;
        }

        private static bool TryReadAvatarBaselineOnTemporaryClone(
            Animator sourceAnimator,
            out float[] muscles,
            out LimbBends bends,
            out string message,
            bool restoreImportedPose = false)
        {
            muscles = null;
            bends = default;
            message = string.Empty;

            var sourceObject = PrefabUtility.GetCorrespondingObjectFromOriginalSource(
                sourceAnimator.gameObject);
            var cloneSource = sourceObject != null ? sourceObject : sourceAnimator.gameObject;
            GameObject clone = null;
            try
            {
                clone = UnityEngine.Object.Instantiate(cloneSource);
                clone.name = "_PosingSystem_AvatarBaselineProbe";
                clone.hideFlags = HideFlags.HideAndDontSave;
                clone.transform.SetParent(null, false);
                clone.transform.position = Vector3.zero;
                clone.transform.rotation = Quaternion.identity;
                clone.SetActive(true);

                var cloneAnimator = clone.GetComponent<Animator>();
                if (!IsValidHumanoid(cloneAnimator))
                {
                    message = "計測用クローンに有効な Humanoid Animator がありません。";
                    return false;
                }

                cloneAnimator.runtimeAnimatorController = null;
                foreach (var behaviour in clone.GetComponentsInChildren<Behaviour>(true))
                    if (behaviour != null && behaviour != cloneAnimator) behaviour.enabled = false;
                if (restoreImportedPose)
                {
                    // A scene avatar (or a preview clone without a prefab source) may already be posed.
                    // Measure the imported skeleton on the disposable clone, never that current pose.
                    var skeleton = cloneAnimator.avatar.humanDescription.skeleton;
                    var transforms = clone.GetComponentsInChildren<Transform>(true)
                        .Where(t => t != clone.transform).ToLookup(t => t.name);
                    foreach (var bone in skeleton)
                    {
                        var matches = transforms[bone.name].ToArray();
                        if (matches.Length != 1) continue;
                        matches[0].localPosition = bone.position;
                        matches[0].localRotation = bone.rotation;
                        matches[0].localScale = bone.scale;
                    }
                }

                return TryReadAvatarBaseline(cloneAnimator, out muscles, out bends, out message);
            }
            finally
            {
                if (clone != null)
                {
                    UnityEngine.Object.DestroyImmediate(clone);
                }
            }
        }

        private static bool TryReadAvatarBaseline(
            Animator animator,
            out float[] muscles,
            out LimbBends bends,
            out string message)
        {
            muscles = null;
            bends = default;
            message = string.Empty;
            try
            {
                var handler = new HumanPoseHandler(animator.avatar, animator.transform);
                var originalPose = new HumanPose();
                handler.GetHumanPose(ref originalPose);
                muscles = (float[])originalPose.muscles.Clone();

                bends.leftKnee = MeasureSignedBend(animator, handler, originalPose,
                    HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg,
                    HumanBodyBones.LeftFoot, LeftKneeMuscleIndex);
                bends.rightKnee = MeasureSignedBend(animator, handler, originalPose,
                    HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg,
                    HumanBodyBones.RightFoot, RightKneeMuscleIndex);
                bends.leftElbow = MeasureSignedBend(animator, handler, originalPose,
                    HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm,
                    HumanBodyBones.LeftHand, LeftElbowMuscleIndex);
                bends.rightElbow = MeasureSignedBend(animator, handler, originalPose,
                    HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm,
                    HumanBodyBones.RightHand, RightElbowMuscleIndex);

                handler.SetHumanPose(ref originalPose);
                handler.Dispose();
                return true;
            }
            catch (Exception exception)
            {
                message = $"基準姿勢の読み取りに失敗しました: {exception.Message}";
                return false;
            }
        }

        private static float MeasureSignedBend(
            Animator animator,
            HumanPoseHandler handler,
            HumanPose originalPose,
            HumanBodyBones upperBone,
            HumanBodyBones jointBone,
            HumanBodyBones lowerBone,
            int stretchMuscleIndex)
        {
            var upper = animator.GetBoneTransform(upperBone);
            var joint = animator.GetBoneTransform(jointBone);
            var lower = animator.GetBoneTransform(lowerBone);
            if (upper == null || joint == null || lower == null)
            {
                return 0f;
            }

            var upperDirection = (joint.position - upper.position).normalized;
            var lowerDirection = (lower.position - joint.position).normalized;

            var probePose = originalPose;
            probePose.muscles = (float[])originalPose.muscles.Clone();
            probePose.muscles[stretchMuscleIndex] -= 0.25f;
            handler.SetHumanPose(ref probePose);
            var probeUpperDirection = (joint.position - upper.position).normalized;
            var probeLowerDirection = (lower.position - joint.position).normalized;
            var positiveAxis = Vector3.Cross(probeUpperDirection, probeLowerDirection).normalized;
            handler.SetHumanPose(ref originalPose);

            if (positiveAxis.sqrMagnitude < 0.000001f)
            {
                return Vector3.Angle(upperDirection, lowerDirection);
            }

            var cross = Vector3.Cross(upperDirection, lowerDirection);
            var sine = Vector3.Dot(positiveAxis, cross);
            var cosine = Mathf.Clamp(Vector3.Dot(upperDirection, lowerDirection), -1f, 1f);
            return Mathf.Atan2(sine, cosine) * Mathf.Rad2Deg;
        }

        private static string BuildSignature(
            Animator animator,
            PosingSystemAvatarReferenceProfile profile)
        {
            // Scene上のTransform値はHumanPoseHandlerの丸めで微小変化するため、
            // 再生成判定にはAvatar/Profileアセットの依存ハッシュを使う。
            unchecked
            {
                uint hash = 2166136261;
                void Add(string value)
                {
                    if (value == null) return;
                    foreach (var character in value)
                    {
                        hash ^= character;
                        hash *= 16777619;
                    }
                }

                Add(profile.profileVersion.ToString());
                var avatarPath = AssetDatabase.GetAssetPath(animator.avatar);
                if (!string.IsNullOrEmpty(avatarPath))
                {
                    Add(AssetDatabase.AssetPathToGUID(avatarPath));
                    Add(AssetDatabase.GetAssetDependencyHash(avatarPath).ToString());
                }
                else
                {
                    Add(animator.avatar.name);
                }

                var profilePath = AssetDatabase.GetAssetPath(profile);
                Add(AssetDatabase.AssetPathToGUID(profilePath));
                Add(AssetDatabase.GetAssetDependencyHash(profilePath).ToString());
                return hash.ToString("X8");
            }
        }

        private static void EnsureAssetFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            var name = Path.GetFileName(folder);
            EnsureAssetFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Avatar";
            }

            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }
            return value.Trim();
        }

        private struct LimbBends
        {
            public float leftKnee;
            public float rightKnee;
            public float leftElbow;
            public float rightElbow;
        }
    }
}
