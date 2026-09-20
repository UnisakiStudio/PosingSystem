using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor
{
    public sealed class PosingSystemAutoPoseSolveReport
    {
        public int AppliedRuleCount { get; internal set; }
        public float MaximumReachError { get; internal set; }
        public List<string> Warnings { get; } = new();
    }

    public static class PosingSystemAutoPoseAdjustmentUtility
    {
        private const float CurveEpsilon = 0.00001f;
        private static readonly Dictionary<int, Dictionary<string, Quaternion>>
            RestRotationByAvatar = new();
        private static readonly PosingSystemAutoPoseLimb[] LimbSelectionPriority =
        {
            PosingSystemAutoPoseLimb.LeftArm,
            PosingSystemAutoPoseLimb.RightArm,
            PosingSystemAutoPoseLimb.LeftLeg,
            PosingSystemAutoPoseLimb.RightLeg,
        };
        private static readonly HumanBodyBones[] SupportedTargetBoneValues =
            Enum.GetValues(typeof(HumanBodyBones))
                .Cast<HumanBodyBones>()
                .Where(IsSupportedTargetBone)
                .ToArray();

        private readonly struct TargetFrame
        {
            public TargetFrame(Vector3 position, Quaternion rotation)
            {
                Position = position;
                Rotation = NormalizeQuaternion(rotation);
            }

            public Vector3 Position { get; }
            public Quaternion Rotation { get; }

            public Vector3 TransformPoint(Vector3 point)
            {
                return Position + Rotation * point;
            }

            public Vector3 InverseTransformPoint(Vector3 point)
            {
                return Quaternion.Inverse(Rotation) * (point - Position);
            }
        }

        public static PosingSystemAutoPoseDefinition FindDefinition(
            PosingSystem posingSystem,
            string poseKey,
            AnimationClip sourceAnimation)
        {
            return posingSystem != null && posingSystem.autoPoseCatalog != null
                ? posingSystem.autoPoseCatalog.Find(poseKey, sourceAnimation)
                : null;
        }

        public static IReadOnlyList<HumanBodyBones> SupportedTargetBones =>
            SupportedTargetBoneValues;

        public static bool IsSupportedTargetBone(HumanBodyBones bone)
        {
            if (bone == HumanBodyBones.LastBone)
            {
                return false;
            }

            var name = bone.ToString();
            return !name.Contains("Thumb") &&
                   !name.Contains("Index") &&
                   !name.Contains("Middle") &&
                   !name.Contains("Ring") &&
                   !name.Contains("Little");
        }

        public static PosingSystemAutoPoseLimb FindLeastUsedLimb(
            IEnumerable<PosingSystemAutoPoseLimbRule> rules)
        {
            var counts = LimbSelectionPriority.ToDictionary(limb => limb, _ => 0);
            if (rules != null)
            {
                foreach (var rule in rules)
                {
                    if (rule != null && counts.ContainsKey(rule.limb))
                    {
                        counts[rule.limb]++;
                    }
                }
            }

            return LimbSelectionPriority
                .OrderBy(limb => counts[limb])
                .First();
        }

        public static bool TryCreateAdjustmentClip(
            Animator animator,
            AnimationClip baseClip,
            PosingSystemAutoPoseDefinition definition,
            out AnimationClip adjustmentClip,
            out PosingSystemAutoPoseSolveReport report,
            out string message)
        {
            adjustmentClip = null;
            report = new PosingSystemAutoPoseSolveReport();
            message = string.Empty;
            if (!IsValidHumanoid(animator))
            {
                message = "有効なHumanoid Animatorがありません。";
                return false;
            }
            if (baseClip == null)
            {
                message = "元の姿勢AnimationClipがありません。";
                return false;
            }
            if (definition == null)
            {
                message = "この姿勢の自動調整データがありません。";
                return false;
            }

            var ownsAnimationMode = !AnimationMode.InAnimationMode();
            HumanPoseHandler handler = null;
            try
            {
                if (ownsAnimationMode)
                {
                    AnimationMode.StartAnimationMode();
                }

                Sample(animator.gameObject, baseClip);
                handler = new HumanPoseHandler(animator.avatar, animator.transform);
                var before = new HumanPose();
                handler.GetHumanPose(ref before);
                var beforeMuscles = (float[])before.muscles.Clone();

                if (!TryApplyDefinition(animator, definition, report))
                {
                    message = report.Warnings.Count > 0
                        ? string.Join("\n", report.Warnings)
                        : "IKルールを適用できませんでした。";
                    return false;
                }

                var after = new HumanPose();
                handler.GetHumanPose(ref after);
                adjustmentClip = CreateMuscleDifferenceClip(
                    beforeMuscles,
                    after.muscles,
                    $"AutoPoseAdjustment_{SanitizeFileName(definition.poseKey)}");
                message = report.Warnings.Count == 0
                    ? $"{report.AppliedRuleCount}件のIKルールから自動調整Animationを作成しました。"
                    : $"{report.AppliedRuleCount}件のIKルールを適用しました。\n{string.Join("\n", report.Warnings)}";
                return true;
            }
            catch (Exception exception)
            {
                message = $"自動調整Animationの作成に失敗しました: {exception.Message}";
                if (adjustmentClip != null)
                {
                    UnityEngine.Object.DestroyImmediate(adjustmentClip);
                    adjustmentClip = null;
                }
                return false;
            }
            finally
            {
                handler?.Dispose();
                // Direct Transform edits must not leak into the next pose preview/conversion.
                Sample(animator.gameObject, baseClip);
                if (ownsAnimationMode && AnimationMode.InAnimationMode())
                {
                    AnimationMode.StopAnimationMode();
                }
            }
        }

        public static bool TryApplyDefinition(
            Animator animator,
            PosingSystemAutoPoseDefinition definition,
            PosingSystemAutoPoseSolveReport report = null)
        {
            if (!IsValidHumanoid(animator) || definition == null)
            {
                return false;
            }

            report ??= new PosingSystemAutoPoseSolveReport();
            var rules = definition.limbRules
                .Where(rule => rule != null && rule.enabled)
                .OrderBy(rule => IsLeg(rule.limb) ? 0 : 1)
                .ThenBy(rule => rule.solveOrder)
                .ToList();
            if (rules.Count == 0)
            {
                report.Warnings.Add($"{definition.poseKey}: 有効なIKルールがありません。");
                return false;
            }

            // 元Animationをサンプルした直後の曲げ面を、IKを動かす前に保存する。
            // 反復中に現在の関節位置から取り直すと、わずかな誤差で肘・膝が反対側へ
            // 切り替わる可能性があるため、全反復で同じ方向を使用する。
            var sourceBendDirections = new Dictionary<PosingSystemAutoPoseLimbRule, Vector3>();
            var sourceEndRotations = new Dictionary<PosingSystemAutoPoseLimbRule, Quaternion>();
            foreach (var rule in rules)
            {
                if (TryGetLimbBones(animator, rule.limb, out _, out _, out var sourceTip))
                {
                    sourceEndRotations[rule] = sourceTip.rotation;
                }
                if (rule.bendDirectionMode == PosingSystemAutoPoseBendDirectionMode.SourcePose &&
                    TryGetCurrentBendDirection(animator, rule, out var bendDirection))
                {
                    sourceBendDirections[rule] = bendDirection;
                }
            }

            var iterations = Mathf.Clamp(definition.solveIterations, 1, 8);
            for (var iteration = 0; iteration < iterations; iteration++)
            {
                foreach (var rule in rules)
                {
                    sourceBendDirections.TryGetValue(rule, out var sourceBendDirection);
                    sourceEndRotations.TryGetValue(rule, out var sourceEndRotation);
                    ApplyRule(
                        animator,
                        rule,
                        sourceBendDirection,
                        sourceEndRotation,
                        report,
                        iteration == iterations - 1);
                }
            }
            return report.AppliedRuleCount > 0;
        }

        public static bool TryGetTarget(
            Animator animator,
            PosingSystemAutoPoseLimbRule rule,
            out Vector3 targetPosition,
            out Quaternion targetRotation,
            out string message)
        {
            return TryGetTarget(
                animator,
                rule,
                null,
                out targetPosition,
                out targetRotation,
                out message);
        }

        private static bool TryGetTarget(
            Animator animator,
            PosingSystemAutoPoseLimbRule rule,
            Quaternion? legacySourceRotation,
            out Vector3 targetPosition,
            out Quaternion targetRotation,
            out string message)
        {
            targetPosition = default;
            targetRotation = Quaternion.identity;
            message = string.Empty;
            if (!IsValidHumanoid(animator) || rule == null)
            {
                message = "Humanoid AnimatorまたはIKルールがありません。";
                return false;
            }

            var avatarHeight = MeasureAvatarHeight(animator);
            if (!TryGetTargetFrame(
                    animator,
                    rule,
                    rule.coordinateSpaceVersion >= PosingSystemAutoPoseLimbRule.CurrentCoordinateSpaceVersion,
                    out var frame,
                    out message) ||
                !TryGetLimbBones(animator, rule.limb, out _, out _, out var tip))
            {
                return false;
            }

            targetPosition = frame.TransformPoint(rule.normalizedTargetOffset * avatarHeight);
            if (rule.coordinateSpaceVersion < PosingSystemAutoPoseLimbRule.CurrentCoordinateSpaceVersion)
            {
                // The former representation mixed raw bone axes from two avatars. It cannot be
                // converted without the authoring avatar, so preserve this avatar's source-pose
                // end rotation until the rule is captured again.
                targetRotation = legacySourceRotation ?? tip.rotation;
            }
            else if (!TryGetRestRotationInAvatarSpace(animator, tip, out var tipRestRotation))
            {
                message = $"{tip.name} のHumanoid基準軸を取得できません。";
                return false;
            }
            else
            {
                var semanticTargetRotation =
                    frame.Rotation * NormalizeQuaternion(rule.targetRotationOffset);
                targetRotation = FromAvatarIndependentRotation(
                    semanticTargetRotation,
                    tipRestRotation);
            }
            return true;
        }

        public static bool TrySetTargetFromWorld(
            Animator animator,
            PosingSystemAutoPoseLimbRule rule,
            Vector3 targetPosition,
            Quaternion targetRotation,
            out string message)
        {
            message = string.Empty;
            if (!IsValidHumanoid(animator) || rule == null ||
                !TryGetTargetFrame(animator, rule, true, out var frame, out message) ||
                !TryGetLimbBones(animator, rule.limb, out _, out _, out var tip) ||
                !TryGetRestRotationInAvatarSpace(animator, tip, out var tipRestRotation))
            {
                return false;
            }

            var avatarHeight = MeasureAvatarHeight(animator);
            rule.normalizedTargetOffset = frame.InverseTransformPoint(targetPosition) / avatarHeight;
            var semanticTargetRotation = ToAvatarIndependentRotation(
                targetRotation,
                tipRestRotation);
            rule.targetRotationOffset = NormalizeQuaternion(
                Quaternion.Inverse(frame.Rotation) * semanticTargetRotation);
            rule.coordinateSpaceVersion = PosingSystemAutoPoseLimbRule.CurrentCoordinateSpaceVersion;
            return true;
        }

        public static bool TryGetCurrentBendDirection(
            Animator animator,
            PosingSystemAutoPoseLimbRule rule,
            out Vector3 worldDirection)
        {
            worldDirection = default;
            if (!IsValidHumanoid(animator) || rule == null ||
                !TryGetLimbBones(animator, rule.limb, out var root, out var joint, out var tip))
            {
                return false;
            }

            var limbDirection = tip.position - root.position;
            if (limbDirection.sqrMagnitude <= 0.000001f)
            {
                return false;
            }

            var bendDirection = Vector3.ProjectOnPlane(
                joint.position - root.position,
                limbDirection.normalized);
            if (bendDirection.sqrMagnitude <= 0.000001f)
            {
                // 完全に伸びた手足には曲げ面が存在しないため、手動値へフォールバックする。
                return false;
            }

            worldDirection = bendDirection.normalized;
            return true;
        }

        public static bool TryCaptureRuleFromCurrentPose(
            Animator animator,
            PosingSystemAutoPoseLimbRule rule,
            out string message)
        {
            message = string.Empty;
            if (!IsValidHumanoid(animator) || rule == null ||
                !TryGetLimbBones(animator, rule.limb, out var root, out var joint, out var tip))
            {
                message = "対象の腕または脚のHumanoidボーンが見つかりません。";
                return false;
            }

            if (!TryGetTargetFrame(animator, rule, true, out var frame, out message))
            {
                return false;
            }

            var avatarHeight = MeasureAvatarHeight(animator);
            if (!TryGetRestRotationInAvatarSpace(animator, tip, out var tipRestRotation))
            {
                message = $"{tip.name} のHumanoid基準軸を取得できません。";
                return false;
            }

            rule.normalizedTargetOffset = frame.InverseTransformPoint(tip.position) / avatarHeight;
            var semanticTipRotation = ToAvatarIndependentRotation(tip.rotation, tipRestRotation);
            rule.targetRotationOffset = NormalizeQuaternion(
                Quaternion.Inverse(frame.Rotation) * semanticTipRotation);
            rule.coordinateSpaceVersion = PosingSystemAutoPoseLimbRule.CurrentCoordinateSpaceVersion;

            var targetDirection = tip.position - root.position;
            var bendDirection = targetDirection.sqrMagnitude > 0.000001f
                ? Vector3.ProjectOnPlane(joint.position - root.position, targetDirection.normalized)
                : joint.position - root.position;
            if (bendDirection.sqrMagnitude <= 0.000001f)
            {
                bendDirection = joint.position - root.position;
            }
            rule.poleDirectionInAvatarSpace = animator.transform
                .InverseTransformDirection(bendDirection.normalized);
            message = "現在の手足位置・見た目の向き・曲げ方向をIKルールへ記録しました。";
            return true;
        }

        public static bool TryGetLimbBones(
            Animator animator,
            PosingSystemAutoPoseLimb limb,
            out Transform root,
            out Transform joint,
            out Transform tip)
        {
            root = null;
            joint = null;
            tip = null;
            if (animator == null)
            {
                return false;
            }

            HumanBodyBones rootBone;
            HumanBodyBones jointBone;
            HumanBodyBones tipBone;
            switch (limb)
            {
                case PosingSystemAutoPoseLimb.LeftArm:
                    rootBone = HumanBodyBones.LeftUpperArm;
                    jointBone = HumanBodyBones.LeftLowerArm;
                    tipBone = HumanBodyBones.LeftHand;
                    break;
                case PosingSystemAutoPoseLimb.RightArm:
                    rootBone = HumanBodyBones.RightUpperArm;
                    jointBone = HumanBodyBones.RightLowerArm;
                    tipBone = HumanBodyBones.RightHand;
                    break;
                case PosingSystemAutoPoseLimb.LeftLeg:
                    rootBone = HumanBodyBones.LeftUpperLeg;
                    jointBone = HumanBodyBones.LeftLowerLeg;
                    tipBone = HumanBodyBones.LeftFoot;
                    break;
                case PosingSystemAutoPoseLimb.RightLeg:
                    rootBone = HumanBodyBones.RightUpperLeg;
                    jointBone = HumanBodyBones.RightLowerLeg;
                    tipBone = HumanBodyBones.RightFoot;
                    break;
                default:
                    return false;
            }

            root = animator.GetBoneTransform(rootBone);
            joint = animator.GetBoneTransform(jointBone);
            tip = animator.GetBoneTransform(tipBone);
            return root != null && joint != null && tip != null;
        }

        public static float MeasureAvatarHeight(Animator animator)
        {
            if (animator == null)
            {
                return 1f;
            }

            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            var leftFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
            var rightFoot = animator.GetBoneTransform(HumanBodyBones.RightFoot);
            if (head != null && (leftFoot != null || rightFoot != null))
            {
                var footY = leftFoot != null && rightFoot != null
                    ? Mathf.Min(leftFoot.position.y, rightFoot.position.y)
                    : (leftFoot != null ? leftFoot.position.y : rightFoot.position.y);
                var measured = Mathf.Abs(head.position.y - footY);
                if (measured > 0.01f)
                {
                    return measured;
                }
            }

            return Mathf.Max(0.01f, animator.humanScale * 2f);
        }

        public static AnimationClip SaveClip(AnimationClip source, string assetPath)
        {
            if (source == null || string.IsNullOrEmpty(assetPath))
            {
                return null;
            }

            EnsureAssetFolder(Path.GetDirectoryName(assetPath)?.Replace('\\', '/'));
            var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
            if (existing == null)
            {
                existing = UnityEngine.Object.Instantiate(source);
                existing.name = Path.GetFileNameWithoutExtension(assetPath);
                AssetDatabase.CreateAsset(existing, assetPath);
            }
            else
            {
                foreach (var binding in AnimationUtility.GetCurveBindings(existing))
                {
                    AnimationUtility.SetEditorCurve(existing, binding, null);
                }
                foreach (var binding in AnimationUtility.GetCurveBindings(source))
                {
                    AnimationUtility.SetEditorCurve(
                        existing,
                        binding,
                        AnimationUtility.GetEditorCurve(source, binding));
                }
            }

            existing.frameRate = source.frameRate;
            EditorUtility.SetDirty(existing);
            AssetDatabase.SaveAssets();
            AssetDatabase.ImportAsset(assetPath);
            return AssetDatabase.LoadAssetAtPath<AnimationClip>(assetPath);
        }

        private static bool ApplyRule(
            Animator animator,
            PosingSystemAutoPoseLimbRule rule,
            Vector3 sourceBendDirection,
            Quaternion sourceEndRotation,
            PosingSystemAutoPoseSolveReport report,
            bool recordReport)
        {
            if (!TryGetLimbBones(animator, rule.limb, out var root, out var joint, out var tip))
            {
                if (recordReport)
                {
                    report.Warnings.Add($"{rule.label}: {rule.limb} のHumanoidボーンがありません。");
                }
                return false;
            }
            var legacySourceRotation =
                rule.coordinateSpaceVersion < PosingSystemAutoPoseLimbRule.CurrentCoordinateSpaceVersion
                    ? sourceEndRotation
                    : (Quaternion?)null;
            if (!TryGetTarget(
                    animator,
                    rule,
                    legacySourceRotation,
                    out var targetPosition,
                    out var targetRotation,
                    out var message))
            {
                if (recordReport)
                {
                    report.Warnings.Add($"{rule.label}: {message}");
                }
                return false;
            }

            var limbLength = Vector3.Distance(root.position, joint.position) +
                             Vector3.Distance(joint.position, tip.position);
            var poleDirection = sourceBendDirection.sqrMagnitude > 0.000001f
                ? sourceBendDirection
                : animator.transform.TransformDirection(rule.poleDirectionInAvatarSpace);
            if (poleDirection.sqrMagnitude <= 0.000001f)
            {
                poleDirection = joint.position - root.position;
            }
            var polePosition = root.position + poleDirection.normalized * Mathf.Max(0.01f, limbLength);
            if (!PosingSystemTwoBoneIk.TryApply(
                    root,
                    joint,
                    tip,
                    targetPosition,
                    polePosition,
                    rule.positionWeight,
                    out var solution))
            {
                if (recordReport)
                {
                    report.Warnings.Add($"{rule.label}: Two-Bone IKを解けませんでした。");
                }
                return false;
            }

            if (rule.matchEndRotation)
            {
                tip.rotation = Quaternion.Slerp(
                    tip.rotation,
                    targetRotation,
                    Mathf.Clamp01(rule.rotationWeight));
            }

            if (recordReport)
            {
                report.AppliedRuleCount++;
                report.MaximumReachError = Mathf.Max(report.MaximumReachError, solution.ReachError);
                if (solution.TargetClamped)
                {
                    report.Warnings.Add(
                        $"{rule.label}: ターゲットへ届かないため {solution.ReachError:F4}m 手前で制限しました。");
                }
            }
            return true;
        }

        private static bool TryGetTargetFrame(
            Animator animator,
            PosingSystemAutoPoseLimbRule rule,
            bool avatarIndependent,
            out TargetFrame frame,
            out string message)
        {
            frame = default;
            message = string.Empty;
            switch (rule.targetSpace)
            {
                case PosingSystemAutoPoseTargetSpace.BodyBone:
                    var targetBone = animator.GetBoneTransform(rule.targetBone);
                    if (targetBone == null)
                    {
                        message = $"ターゲットボーン {rule.targetBone} がありません。";
                        return false;
                    }
                    if (!avatarIndependent)
                    {
                        frame = new TargetFrame(targetBone.position, targetBone.rotation);
                        return true;
                    }
                    if (!TryGetRestRotationInAvatarSpace(
                            animator,
                            targetBone,
                            out var targetBoneRestRotation))
                    {
                        message = $"{targetBone.name} のHumanoid基準軸を取得できません。";
                        return false;
                    }
                    frame = new TargetFrame(
                        targetBone.position,
                        ToAvatarIndependentRotation(targetBone.rotation, targetBoneRestRotation));
                    return true;
                case PosingSystemAutoPoseTargetSpace.AvatarRoot:
                case PosingSystemAutoPoseTargetSpace.Ground:
                    frame = new TargetFrame(animator.transform.position, animator.transform.rotation);
                    return true;
                default:
                    message = $"未対応のターゲット空間です: {rule.targetSpace}";
                    return false;
            }
        }

        public static Quaternion ToAvatarIndependentRotation(
            Quaternion boneWorldRotation,
            Quaternion boneRestRotationInAvatarSpace)
        {
            return NormalizeQuaternion(
                boneWorldRotation * Quaternion.Inverse(boneRestRotationInAvatarSpace));
        }

        public static Quaternion FromAvatarIndependentRotation(
            Quaternion avatarIndependentRotation,
            Quaternion boneRestRotationInAvatarSpace)
        {
            return NormalizeQuaternion(
                avatarIndependentRotation * boneRestRotationInAvatarSpace);
        }

        private static bool TryGetRestRotationInAvatarSpace(
            Animator animator,
            Transform bone,
            out Quaternion restRotation)
        {
            restRotation = Quaternion.identity;
            if (!IsValidHumanoid(animator) || bone == null)
            {
                return false;
            }

            var chain = new List<Transform>();
            var current = bone;
            while (current != null && current != animator.transform)
            {
                chain.Add(current);
                current = current.parent;
            }
            if (current != animator.transform)
            {
                return false;
            }

            var avatarId = animator.avatar.GetInstanceID();
            if (!RestRotationByAvatar.TryGetValue(avatarId, out var rotationsByName))
            {
                rotationsByName = new Dictionary<string, Quaternion>();
                foreach (var skeletonBone in animator.avatar.humanDescription.skeleton)
                {
                    if (!rotationsByName.ContainsKey(skeletonBone.name))
                    {
                        rotationsByName.Add(
                            skeletonBone.name,
                            NormalizeQuaternion(skeletonBone.rotation));
                    }
                }
                RestRotationByAvatar.Add(avatarId, rotationsByName);
            }

            chain.Reverse();
            foreach (var transform in chain)
            {
                // Non-skeleton wrapper transforms are not animated by Humanoid. Their current
                // local rotation is therefore the best available rest-space continuation.
                restRotation *= rotationsByName.TryGetValue(transform.name, out var localRest)
                    ? localRest
                    : transform.localRotation;
            }
            restRotation = NormalizeQuaternion(restRotation);
            return true;
        }

        private static AnimationClip CreateMuscleDifferenceClip(
            IReadOnlyList<float> before,
            IReadOnlyList<float> after,
            string name)
        {
            var clip = new AnimationClip
            {
                name = name,
                frameRate = 60f,
                hideFlags = HideFlags.HideAndDontSave,
            };
            var count = Mathf.Min(before.Count, Mathf.Min(after.Count, HumanTrait.MuscleCount));
            for (var i = 0; i < count; i++)
            {
                var difference = after[i] - before[i];
                if (Mathf.Abs(difference) <= CurveEpsilon)
                {
                    continue;
                }

                var binding = EditorCurveBinding.FloatCurve(
                    string.Empty,
                    typeof(Animator),
                    HumanTrait.MuscleName[i]);
                AnimationUtility.SetEditorCurve(
                    clip,
                    binding,
                    AnimationCurve.Constant(0f, 1f / 60f, difference));
            }
            return clip;
        }

        private static void Sample(GameObject avatar, AnimationClip clip)
        {
            AnimationMode.BeginSampling();
            AnimationMode.SampleAnimationClip(avatar, clip, 0f);
            AnimationMode.EndSampling();
        }

        private static bool IsValidHumanoid(Animator animator)
        {
            return animator != null && animator.avatar != null && animator.avatar.isValid && animator.isHuman;
        }

        private static bool IsLeg(PosingSystemAutoPoseLimb limb)
        {
            return limb == PosingSystemAutoPoseLimb.LeftLeg ||
                   limb == PosingSystemAutoPoseLimb.RightLeg;
        }

        private static Quaternion NormalizeQuaternion(Quaternion value)
        {
            var magnitude = Mathf.Sqrt(
                value.x * value.x + value.y * value.y + value.z * value.z + value.w * value.w);
            if (magnitude <= 0.000001f)
            {
                return Quaternion.identity;
            }
            return new Quaternion(
                value.x / magnitude,
                value.y / magnitude,
                value.z / magnitude,
                value.w / magnitude);
        }

        private static void EnsureAssetFolder(string folder)
        {
            if (string.IsNullOrEmpty(folder) || AssetDatabase.IsValidFolder(folder))
            {
                return;
            }

            var parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            EnsureAssetFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(folder));
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Pose";
            }
            foreach (var invalid in Path.GetInvalidFileNameChars())
            {
                value = value.Replace(invalid, '_');
            }
            return value.Trim();
        }
    }
}
