using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using jp.unisakistudio.posingsystem;
using jp.unisakistudio.posingsystemeditor;
using Object = UnityEngine.Object;

public sealed class PosingSystemAutoPosePositionTests
{
    private readonly List<Object> assets = new();
    private Scene scene;
    private Animator animator;
    private string savedCatalogPath;

    [SetUp]
    public void SetUp()
    {
        scene = EditorSceneManager.NewPreviewScene();
        animator = CreateAvatar();
        animator.transform.SetPositionAndRotation(new Vector3(2, 3, -4), Quaternion.Euler(23, 51, -17));
        animator.GetBoneTransform(HumanBodyBones.LeftLowerArm).localRotation = Quaternion.Euler(0, 42, 0);
    }

    [TearDown]
    public void TearDown()
    {
        if (AnimationMode.InAnimationMode()) AnimationMode.StopAnimationMode();
        if (scene.IsValid()) EditorSceneManager.ClosePreviewScene(scene);
        if (!string.IsNullOrEmpty(savedCatalogPath)) AssetDatabase.DeleteAsset(savedCatalogPath);
        foreach (var asset in assets) if (asset != null) Object.DestroyImmediate(asset);
        assets.Clear();
    }

    private static PosingSystemAutoPoseLimbRule Rule() => new() {
        limb = PosingSystemAutoPoseLimb.LeftArm,
        targetSpace = PosingSystemAutoPoseTargetSpace.AvatarRoot,
        coordinateSpaceVersion = PosingSystemAutoPoseLimbRule.CurrentCoordinateSpaceVersion,
        matchEndRotation = false,
    };

    private static IEnumerable<TestCaseData> AxisCases()
    {
        for (var mask = 0; mask < 8; mask++)
            foreach (var weight in new[] { 1f, .4f })
                foreach (var world in new[] { false, true })
                    yield return new TestCaseData(mask, weight, world);
    }

    private PosingSystemAutoPoseLimbRule ViewRule(out Vector3 headLocalView)
    {
        var head = animator.GetBoneTransform(HumanBodyBones.Head);
        headLocalView = new Vector3(.01f, .075f, .09f);
        var descriptor = animator.gameObject.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
        descriptor.ViewPosition = head.TransformPoint(headLocalView) - animator.transform.position;
        var rule = Rule();
        rule.targetSpace = PosingSystemAutoPoseTargetSpace.BodyBone;
        rule.useViewPosition = true;
        rule.lengthReference = PosingSystemAutoPoseLengthReference.Forearm;
        return rule;
    }

    [TestCase(.5f, false)]
    [TestCase(1f, false)]
    [TestCase(2f, false)]
    [TestCase(.5f, true)]
    [TestCase(2f, true)]
    public void ViewPosition_FollowsPosedHead_WithRootAndParentScale(float scale, bool parentScale)
    {
        var rule = ViewRule(out var eye);
        if (parentScale)
        {
            var parent = new GameObject("Scaled parent");
            SceneManager.MoveGameObjectToScene(parent, scene);
            animator.transform.SetParent(parent.transform, false);
            parent.transform.localScale = Vector3.one * scale;
            parent.transform.rotation = Quaternion.Euler(-21, 63, 17);
        }
        else animator.transform.localScale = Vector3.one * scale;
        var head = animator.GetBoneTransform(HumanBodyBones.Head);
        // Match a correctly positioned SDK viewpoint after resizing/rotating the avatar.
        animator.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>().ViewPosition =
            head.TransformPoint(eye) - animator.transform.position;
        var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        hips.localRotation = Quaternion.Euler(70, -31, 55);
        head.localRotation = Quaternion.Euler(14, 32, -45);
        hips.localPosition += new Vector3(.12f, -.38f, .05f);
        var before = head.localRotation;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out var target, out _, out var message), Is.True, message);
        Assert.That(Vector3.Distance(target, head.TransformPoint(eye)), Is.LessThan(.00001f));
        Assert.That(Quaternion.Angle(head.localRotation, before), Is.LessThan(.001f), "Reading must not reset the pose");
        var clone = Object.Instantiate(animator.gameObject);
        SceneManager.MoveGameObjectToScene(clone, scene);
        clone.transform.SetPositionAndRotation(animator.transform.position, animator.transform.rotation);
        clone.transform.localScale = animator.transform.lossyScale;
        var clonedAnimator = clone.GetComponent<Animator>();
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(clonedAnimator, rule, out var cloned, out _, out message), Is.True, message);
        Assert.That(Vector3.Distance(cloned, clonedAnimator.GetBoneTransform(HumanBodyBones.Head).TransformPoint(eye)), Is.LessThan(.00002f));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ViewPosition_CaptureAndCoordinateConversion_RoundTrip(bool world)
    {
        var rule = ViewRule(out _);
        animator.GetBoneTransform(HumanBodyBones.Head).localRotation = Quaternion.Euler(33, -24, 45);
        rule.positionSpace = world ? PosingSystemAutoPosePositionSpace.World : PosingSystemAutoPosePositionSpace.ReferenceLocal;
        var tip = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        rule.matchEndRotation = true;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryCaptureRuleFromCurrentPose(animator, rule, out var message), Is.True, message);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out var target, out var rotation, out message), Is.True, message);
        Assert.That(Vector3.Distance(target, tip.position), Is.LessThan(.00001f));
        Assert.That(Quaternion.Angle(rotation, tip.rotation), Is.LessThan(.05f));
        var nextSpace = world ? PosingSystemAutoPosePositionSpace.ReferenceLocal : PosingSystemAutoPosePositionSpace.World;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryConvertPositionSettings(animator, rule,
            PosingSystemAutoPoseLengthReference.Leg, nextSpace, out var converted, out message), Is.True, message);
        rule.normalizedTargetOffset = converted;
        rule.lengthReference = PosingSystemAutoPoseLengthReference.Leg;
        rule.positionSpace = nextSpace;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out target, out rotation, out message), Is.True, message);
        Assert.That(Vector3.Distance(target, tip.position), Is.LessThan(.00002f));
        Assert.That(Quaternion.Angle(rotation, tip.rotation), Is.LessThan(.05f));
    }

    [TestCase(.5f)]
    [TestCase(2f)]
    public void IkViewTargetAndSolvedRotations_AreInvariantUnderUniformScale(float scale)
    {
        var rule = ViewRule(out _);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryCaptureRuleFromCurrentPose(animator, rule, out _), Is.True);
        rule.normalizedTargetOffset += new Vector3(.05f, .08f, -.03f);
        var definition = new PosingSystemAutoPoseDefinition { limbRules = new() { rule } };
        var bones = animator.GetComponentsInChildren<Transform>();
        var before = bones.Select(b => b.localRotation).ToArray();
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryApplyDefinition(animator, definition), Is.True);
        var expected = bones.Select(b => b.localRotation).ToArray();
        for (var i = 0; i < bones.Length; i++) bones[i].localRotation = before[i];
        animator.transform.localScale = Vector3.one * scale;
        animator.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>().ViewPosition *= scale;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryApplyDefinition(animator, definition), Is.True);
        for (var i = 0; i < bones.Length; i++)
            Assert.That(Quaternion.Angle(expected[i], bones[i].localRotation), Is.LessThan(.08f), bones[i].name);
    }

    [TestCase(.5f)]
    [TestCase(2f)]
    public void HeightReference_WhenLyingFlat_ScalesWithAvatar(float scale)
    {
        animator.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        var head = animator.GetBoneTransform(HumanBodyBones.Head);
        var left = animator.GetBoneTransform(HumanBodyBones.LeftFoot);
        var right = animator.GetBoneTransform(HumanBodyBones.RightFoot);
        head.position = new Vector3(head.position.x, Mathf.Min(left.position.y, right.position.y), head.position.z);
        var length = PosingSystemAutoPoseAdjustmentUtility.MeasureAvatarHeight(animator);
        animator.transform.localScale = Vector3.one * scale;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.MeasureAvatarHeight(animator), Is.EqualTo(length * scale).Within(.00001f));
    }

    [Test]
    public void ViewPosition_MissingDescriptorReportsReason_WithoutChangingLegacyBoneTargets()
    {
        var rule = Rule(); rule.targetSpace = PosingSystemAutoPoseTargetSpace.BodyBone;
        rule.targetBone = HumanBodyBones.Head;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out var target, out _, out _), Is.True);
        Assert.That(target, Is.EqualTo(animator.GetBoneTransform(HumanBodyBones.Head).position));
        rule.useViewPosition = true;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out _, out _, out var message), Is.False);
        Assert.That(message, Does.Contain("Avatar Descriptor"));
        rule.targetSpace = PosingSystemAutoPoseTargetSpace.Ground;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out _, out _, out _), Is.True);
    }

    [TestCaseSource(nameof(AxisCases))]
    public void DisabledAxes_PreserveSourcePosition_InRotatedFrameAcrossIterations(int mask, float weight, bool world)
    {
        var rule = Rule();
        rule.positionSpace = world ? PosingSystemAutoPosePositionSpace.World : PosingSystemAutoPosePositionSpace.ReferenceLocal;
        var tip = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        var inverse = world ? Quaternion.identity : Quaternion.Inverse(animator.transform.rotation);
        var before = inverse * (tip.position - animator.transform.position);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryCaptureRuleFromCurrentPose(animator, rule, out _), Is.True);
        rule.normalizedTargetOffset += new Vector3(.025f, .025f, .025f);
        rule.applyPositionX = (mask & 1) != 0;
        rule.applyPositionY = (mask & 2) != 0;
        rule.applyPositionZ = (mask & 4) != 0;
        rule.positionWeight = weight;
        var definition = new PosingSystemAutoPoseDefinition { solveIterations = 8, limbRules = new() { rule } };
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryApplyDefinition(animator, definition), Is.True);
        var after = inverse * (tip.position - animator.transform.position);
        for (var axis = 0; axis < 3; axis++)
            if ((mask & (1 << axis)) == 0) Assert.That(after[axis], Is.EqualTo(before[axis]).Within(.0001f), "Axis " + axis);
        if (mask != 0) Assert.That(Vector3.Distance(before, after), Is.GreaterThan(.0001f));
    }

    [Test]
    public void UnreachableTarget_ClampsOnlyEnabledAxes()
    {
        var rule = Rule();
        rule.applyPositionX = rule.applyPositionZ = false;
        rule.normalizedTargetOffset = new Vector3(999, 999, 999);
        var tip = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        var before = animator.transform.InverseTransformPoint(tip.position);
        var report = new PosingSystemAutoPoseSolveReport();
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryApplyDefinition(animator,
            new PosingSystemAutoPoseDefinition { limbRules = new() { rule } }, report), Is.True);
        var after = animator.transform.InverseTransformPoint(tip.position);
        Assert.That(after.x, Is.EqualTo(before.x).Within(.0001f));
        Assert.That(after.z, Is.EqualTo(before.z).Within(.0001f));
        Assert.That(report.Warnings, Is.Not.Empty);
    }

    [Test]
    public void AllAxesDisabled_PreservesLimb_ButAllowsIndependentEndRotation()
    {
        var rule = Rule();
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryCaptureRuleFromCurrentPose(animator, rule, out _), Is.True);
        rule.applyPositionX = rule.applyPositionY = rule.applyPositionZ = false;
        rule.matchEndRotation = true;
        rule.targetRotationOffset *= Quaternion.Euler(20, 0, 0);
        rule.bendDirectionMode = PosingSystemAutoPoseBendDirectionMode.Manual;
        rule.poleDirectionInAvatarSpace = Vector3.back;
        var root = animator.GetBoneTransform(HumanBodyBones.LeftUpperArm);
        var joint = animator.GetBoneTransform(HumanBodyBones.LeftLowerArm);
        var tip = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        var rootRotation = root.rotation; var jointRotation = joint.rotation; var tipRotation = tip.rotation; var tipPosition = tip.position;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryApplyDefinition(animator,
            new PosingSystemAutoPoseDefinition { limbRules = new() { rule } }), Is.True);
        Assert.That(Quaternion.Angle(rootRotation, root.rotation), Is.LessThan(.01f));
        Assert.That(Quaternion.Angle(jointRotation, joint.rotation), Is.LessThan(.01f));
        Assert.That(Vector3.Distance(tipPosition, tip.position), Is.LessThan(.0001f));
        Assert.That(Quaternion.Angle(tipRotation, tip.rotation), Is.GreaterThan(10));
    }

    [Test]
    public void SceneHandle_PreservesDisabledSavedValues()
    {
        var rule = Rule(); rule.applyPositionY = false;
        rule.normalizedTargetOffset = new Vector3(1, 123, 3);
        var target = animator.transform.position + animator.transform.rotation * new Vector3(.1f, .2f, .3f);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TrySetTargetFromWorld(animator, rule, target, Quaternion.identity, out _), Is.True);
        Assert.That(rule.normalizedTargetOffset.y, Is.EqualTo(123));
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out var restored, out _, out _), Is.True);
        var offset = Quaternion.Inverse(animator.transform.rotation) * (restored - animator.transform.position);
        Assert.That(offset.x, Is.EqualTo(.1f).Within(.0001f));
        Assert.That(offset.z, Is.EqualTo(.3f).Within(.0001f));
    }

    [Test]
    public void EveryLengthReference_UsesMeasuredBoneLength_AndScalesWithAvatar()
    {
        foreach (var limb in new[] { PosingSystemAutoPoseLimb.LeftArm, PosingSystemAutoPoseLimb.RightLeg })
        {
            var left = limb == PosingSystemAutoPoseLimb.LeftArm;
            var pairs = new[] {
                (PosingSystemAutoPoseLengthReference.UpperArm, left ? HumanBodyBones.LeftUpperArm : HumanBodyBones.RightUpperArm, left ? HumanBodyBones.LeftLowerArm : HumanBodyBones.RightLowerArm),
                (PosingSystemAutoPoseLengthReference.Forearm, left ? HumanBodyBones.LeftLowerArm : HumanBodyBones.RightLowerArm, left ? HumanBodyBones.LeftHand : HumanBodyBones.RightHand),
                (PosingSystemAutoPoseLengthReference.Thigh, left ? HumanBodyBones.LeftUpperLeg : HumanBodyBones.RightUpperLeg, left ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg),
                (PosingSystemAutoPoseLengthReference.Shin, left ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg, left ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot),
                (PosingSystemAutoPoseLengthReference.Chest, HumanBodyBones.Chest, HumanBodyBones.Neck),
            };
            foreach (var pair in pairs)
            {
                animator.transform.localScale = Vector3.one;
                Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryMeasureReferenceLength(animator, pair.Item1, limb, out var length, out _), Is.True);
                var expected = Vector3.Distance(animator.GetBoneTransform(pair.Item2).position, animator.GetBoneTransform(pair.Item3).position);
                Assert.That(length, Is.EqualTo(expected).Within(.00001f), pair.Item1.ToString());
                animator.transform.localScale = Vector3.one * 1.7f;
                Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryMeasureReferenceLength(animator, pair.Item1, limb, out var scaled, out _), Is.True);
                Assert.That(scaled, Is.EqualTo(length * 1.7f).Within(.00001f));
            }
        }
    }

    [Test]
    public void ReferenceConversion_AndCaptureRoundTrip_PreservePositionForEveryReference()
    {
        ViewRule(out _);
        var rule = Rule();
        var tip = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryCaptureRuleFromCurrentPose(animator, rule, out _), Is.True);
        foreach (PosingSystemAutoPoseLengthReference reference in Enum.GetValues(typeof(PosingSystemAutoPoseLengthReference)))
        {
            Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryConvertLengthReference(animator, rule, reference, out var offset, out _), Is.True);
            rule.lengthReference = reference; rule.normalizedTargetOffset = offset;
            Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out var target, out _, out _), Is.True);
            Assert.That(Vector3.Distance(target, tip.position), Is.LessThan(.00001f), reference.ToString());
            Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryCaptureRuleFromCurrentPose(animator, rule, out _), Is.True);
            Assert.That(Vector3.Distance(rule.normalizedTargetOffset, offset), Is.LessThan(.00001f));
        }
    }

    [TestCase(.5f)]
    [TestCase(2f)]
    public void LengthReferences_ScaleOnceIncludingParentScale(float scale)
    {
        ViewRule(out _);
        var descriptor = animator.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
        var head = animator.GetBoneTransform(HumanBodyBones.Head);
        var root = animator.transform;
        // Lateral/depth offsets do not contribute to head height.
        descriptor.ViewPosition = head.position - root.position + root.TransformVector(new Vector3(.25f, -.12f, .35f));
        float Measure(PosingSystemAutoPoseLengthReference reference)
        {
            Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryMeasureReferenceLength(animator, reference,
                PosingSystemAutoPoseLimb.LeftArm, out var length, out var message), Is.True, message);
            return length;
        }
        animator.GetBoneTransform(HumanBodyBones.Hips).localRotation *= Quaternion.Euler(50, 20, -30);
        Assert.That(Measure(PosingSystemAutoPoseLengthReference.Fixed), Is.EqualTo(1f).Within(.00001f));
        Assert.That(Measure(PosingSystemAutoPoseLengthReference.HeadToViewpoint), Is.EqualTo(.12f).Within(.00001f));
        Assert.That(Measure(PosingSystemAutoPoseLengthReference.HeadToViewpointDepth), Is.EqualTo(.35f).Within(.00001f));
        var references = Enum.GetValues(typeof(PosingSystemAutoPoseLengthReference)).Cast<PosingSystemAutoPoseLengthReference>();
        var baseline = references.ToDictionary(reference => reference, Measure);
        var parent = new GameObject("Scaled avatar parent");
        SceneManager.MoveGameObjectToScene(parent, scene);
        root.SetParent(parent.transform, false);
        parent.transform.localScale = Vector3.one * scale;
        // Descriptor stores a world-distance offset for the resized avatar.
        descriptor.ViewPosition *= scale;
        foreach (var pair in baseline)
            Assert.That(Measure(pair.Key), Is.EqualTo(pair.Value * scale).Within(.00002f), pair.Key.ToString());
    }

    [TestCase(-.08f)]
    [TestCase(0f)]
    public void HeadDepth_UsesOnlyAvatarZ_WithNonuniformScale(float depth)
    {
        var root = animator.transform;
        root.localScale = new Vector3(.5f, 2f, 3f);
        ViewRule(out _);
        var head = animator.GetBoneTransform(HumanBodyBones.Head);
        animator.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>().ViewPosition =
            head.position - root.position + root.TransformVector(new Vector3(.1f, .2f, depth));
        var valid = PosingSystemAutoPoseAdjustmentUtility.TryMeasureReferenceLength(animator,
            PosingSystemAutoPoseLengthReference.HeadToViewpointDepth, PosingSystemAutoPoseLimb.LeftArm,
            out var length, out var message);
        Assert.That(valid, Is.EqualTo(depth != 0f), message);
        Assert.That(length, Is.EqualTo(Mathf.Abs(depth) * 3f).Within(.00001f));
        if (!valid) Assert.That(message, Does.Contain("前後差"));
    }

    [TestCase(PosingSystemAutoPoseLimb.LeftLeg)]
    [TestCase(PosingSystemAutoPoseLimb.RightLeg)]
    [TestCase(PosingSystemAutoPoseLimb.LeftArm)]
    [TestCase(PosingSystemAutoPoseLimb.RightArm)]
    public void WholeLegLength_SumsSameSideSegments_AndIsInvariantToKneeBend(PosingSystemAutoPoseLimb limb)
    {
        // Asymmetry detects accidentally measuring the other leg, including when used for an arm rule.
        animator.GetBoneTransform(HumanBodyBones.RightLowerLeg).localPosition *= 1.3f;
        animator.GetBoneTransform(HumanBodyBones.RightFoot).localPosition *= .8f;
        var left = limb == PosingSystemAutoPoseLimb.LeftLeg || limb == PosingSystemAutoPoseLimb.LeftArm;
        var hip = animator.GetBoneTransform(left ? HumanBodyBones.LeftUpperLeg : HumanBodyBones.RightUpperLeg);
        var knee = animator.GetBoneTransform(left ? HumanBodyBones.LeftLowerLeg : HumanBodyBones.RightLowerLeg);
        var ankle = animator.GetBoneTransform(left ? HumanBodyBones.LeftFoot : HumanBodyBones.RightFoot);
        var expected = Vector3.Distance(hip.position, knee.position) + Vector3.Distance(knee.position, ankle.position);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryMeasureReferenceLength(animator,
            PosingSystemAutoPoseLengthReference.Leg, limb, out var standing, out var message), Is.True, message);
        Assert.That(standing, Is.EqualTo(expected).Within(.00001f));
        knee.localRotation *= Quaternion.Euler(90, 0, 0);
        Assert.That(Vector3.Distance(hip.position, ankle.position), Is.LessThan(expected - .01f));
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryMeasureReferenceLength(animator,
            PosingSystemAutoPoseLengthReference.Leg, limb, out var bent, out message), Is.True, message);
        Assert.That(bent, Is.EqualTo(standing).Within(.00001f));
        animator.transform.localScale = Vector3.one * 1.7f;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryMeasureReferenceLength(animator,
            PosingSystemAutoPoseLengthReference.Leg, limb, out var scaled, out message), Is.True, message);
        Assert.That(scaled, Is.EqualTo(standing * 1.7f).Within(.00001f));
    }

    [TestCase(HumanBodyBones.LeftLowerLeg)]
    [TestCase(HumanBodyBones.LeftFoot)]
    public void WholeLegLength_RejectsEitherZeroLengthSegment(HumanBodyBones collapsed)
    {
        animator.GetBoneTransform(collapsed).localPosition = Vector3.zero;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryMeasureReferenceLength(animator,
            PosingSystemAutoPoseLengthReference.Leg, PosingSystemAutoPoseLimb.LeftLeg, out var length, out var message), Is.False);
        Assert.That(length, Is.Zero);
        Assert.That(message, Is.Not.Empty);
    }

    [Test]
    public void LengthReference_PreservesPreviouslySerializedEnumValues()
    {
        Assert.That(new[] { (int)PosingSystemAutoPoseLengthReference.AvatarHeight,
            (int)PosingSystemAutoPoseLengthReference.UpperArm, (int)PosingSystemAutoPoseLengthReference.Forearm,
            (int)PosingSystemAutoPoseLengthReference.Chest, (int)PosingSystemAutoPoseLengthReference.Thigh,
            (int)PosingSystemAutoPoseLengthReference.Shin }, Is.EqualTo(new[] { 0, 1, 2, 3, 4, 5 }));
        Assert.That((int)PosingSystemAutoPoseLengthReference.Leg, Is.EqualTo(6));
    }

    [Test]
    public void InvalidMeasuredLength_FailsExplicitlyInsteadOfDividingByZero()
    {
        animator.GetBoneTransform(HumanBodyBones.LeftHand).localPosition = Vector3.zero;
        var rule = Rule(); rule.lengthReference = PosingSystemAutoPoseLengthReference.Forearm;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out _, out _, out var message), Is.False);
        Assert.That(message, Does.Contain("有効な長さ"));
    }

    [TestCase(PosingSystemAutoPoseLengthReference.Forearm)]
    [TestCase(PosingSystemAutoPoseLengthReference.Leg)]
    public void LegacyCatalog_UsesAllAxesAndHeight_AndSavedSettingsInvalidatePrebuild(PosingSystemAutoPoseLengthReference reference)
    {
        var catalog = ScriptableObject.CreateInstance<PosingSystemAutoPoseCatalog>();
        assets.Add(catalog);
        EditorJsonUtility.FromJsonOverwrite("{\"MonoBehaviour\":{\"poses\":[{\"poseKey\":\"Legacy\",\"limbRules\":[{\"enabled\":true,\"normalizedTargetOffset\":{\"x\":0.1,\"y\":0.2,\"z\":0.3}}]}]}}", catalog);
        var rule = catalog.poses[0].limbRules[0];
        Assert.That(rule.HasAllPositionAxes, Is.True);
        Assert.That(rule.lengthReference, Is.EqualTo(PosingSystemAutoPoseLengthReference.AvatarHeight));
        Assert.That(rule.positionSpace, Is.EqualTo(PosingSystemAutoPosePositionSpace.ReferenceLocal));
        Assert.That(rule.useViewPosition, Is.False, "Old catalogs must continue using the selected bone");
        savedCatalogPath = "Assets/__IkAxesTest_" + Guid.NewGuid().ToString("N") + ".asset";
        AssetDatabase.CreateAsset(catalog, savedCatalogPath);
        var ps = animator.gameObject.AddComponent<PosingSystem>();
        ps.autoPoseCatalog = catalog;
        var before = PosingSystemConverter.GetDefineSerializeJson(ps, true);
        rule.applyPositionY = false;
        EditorUtility.SetDirty(catalog);
        var afterAxis = PosingSystemConverter.GetDefineSerializeJson(ps, true);
        Assert.That(afterAxis, Is.Not.EqualTo(before), "Unsaved axis edits must invalidate the prebuild");
        rule.lengthReference = reference;
        Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps, true), Is.Not.EqualTo(afterAxis),
            "Unsaved length reference edits must invalidate the prebuild");
        var afterLength = PosingSystemConverter.GetDefineSerializeJson(ps, true);
        rule.positionSpace = PosingSystemAutoPosePositionSpace.World;
        Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps, true), Is.Not.EqualTo(afterLength),
            "Unsaved coordinate system edits must invalidate the prebuild");
        var afterSpace = PosingSystemConverter.GetDefineSerializeJson(ps, true);
        rule.useViewPosition = true;
        Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps, true), Is.Not.EqualTo(afterSpace));
        EditorUtility.SetDirty(catalog); AssetDatabase.SaveAssetIfDirty(catalog);
        AssetDatabase.ImportAsset(savedCatalogPath, ImportAssetOptions.ForceUpdate);
        Assert.That(PosingSystemConverter.GetDefineSerializeJson(ps, true), Is.Not.EqualTo(before));
        var saved = AssetDatabase.LoadAssetAtPath<PosingSystemAutoPoseCatalog>(savedCatalogPath).poses[0].limbRules[0];
        Assert.That(saved.applyPositionY, Is.False);
        Assert.That(saved.applyPositionX && saved.applyPositionZ, Is.True);
        Assert.That(saved.lengthReference, Is.EqualTo(reference));
        Assert.That(saved.positionSpace, Is.EqualTo(PosingSystemAutoPosePositionSpace.World));
        Assert.That(saved.useViewPosition, Is.True);
    }

    [Test]
    public void DisabledAxes_FollowRotatedBodyBoneFrame()
    {
        var rule = Rule(); rule.targetSpace = PosingSystemAutoPoseTargetSpace.BodyBone;
        rule.targetBone = HumanBodyBones.Head;
        var head = animator.GetBoneTransform(HumanBodyBones.Head);
        head.localRotation = Quaternion.Euler(24, -31, 13);
        var tip = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        var before = Quaternion.Inverse(head.rotation) * (tip.position - head.position);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryCaptureRuleFromCurrentPose(animator, rule, out _), Is.True);
        rule.applyPositionY = rule.applyPositionZ = false;
        rule.normalizedTargetOffset.x += .01f;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryApplyDefinition(animator,
            new PosingSystemAutoPoseDefinition { limbRules = new() { rule } }), Is.True);
        var after = Quaternion.Inverse(head.rotation) * (tip.position - head.position);
        Assert.That(after.y, Is.EqualTo(before.y).Within(.0001f));
        Assert.That(after.z, Is.EqualTo(before.z).Within(.0001f));
        Assert.That(after.x, Is.Not.EqualTo(before.x).Within(.0001f));
    }

    [TestCase(false, PosingSystemAutoPoseLengthReference.UpperArm)]
    [TestCase(true, PosingSystemAutoPoseLengthReference.UpperArm)]
    [TestCase(false, PosingSystemAutoPoseLengthReference.Leg)]
    [TestCase(true, PosingSystemAutoPoseLengthReference.Leg)]
    [TestCase(false, PosingSystemAutoPoseLengthReference.Forearm, true)]
    [TestCase(true, PosingSystemAutoPoseLengthReference.Leg, true)]
    [TestCase(false, PosingSystemAutoPoseLengthReference.Fixed)]
    [TestCase(true, PosingSystemAutoPoseLengthReference.HeadToViewpoint, true)]
    [TestCase(true, PosingSystemAutoPoseLengthReference.HeadToViewpointDepth, true)]
    public void GeneratedClip_ReflectsMaskedIkAndSelectedReference(bool world, PosingSystemAutoPoseLengthReference reference, bool view = false)
    {
        animator.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        var rule = view ? ViewRule(out _) : Rule();
        var clip = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.identity, "IK Axis Pose");
        assets.Add(clip);
        AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), "Left Forearm Stretch"), AnimationCurve.Constant(0, 1, -.35f));
        clip.SampleAnimation(animator.gameObject, 0);
        rule.lengthReference = reference;
        rule.positionSpace = world ? PosingSystemAutoPosePositionSpace.World : PosingSystemAutoPosePositionSpace.ReferenceLocal;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryCaptureRuleFromCurrentPose(animator, rule, out _), Is.True);
        rule.applyPositionX = rule.applyPositionZ = false;
        rule.normalizedTargetOffset.y += .15f;
        var definition = new PosingSystemAutoPoseDefinition { sourceAnimation = clip, limbRules = new() { rule } };
        using var handler = new HumanPoseHandler(animator.avatar, animator.transform);
        var before = new HumanPose(); handler.GetHumanPose(ref before);
        var beforeMuscles = (float[])before.muscles.Clone();
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryApplyDefinition(animator, definition), Is.True);
        var after = new HumanPose(); handler.GetHumanPose(ref after);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryCreateAdjustmentClip(animator, clip, definition, out var generated, out _, out var message), Is.True, message);
        assets.Add(generated);
        var curves = AnimationUtility.GetCurveBindings(generated);
        Assert.That(curves, Is.Not.Empty);
        foreach (var binding in curves)
        {
            var index = Array.IndexOf(HumanTrait.MuscleName, binding.propertyName);
            Assert.That(index, Is.GreaterThanOrEqualTo(0));
            Assert.That(AnimationUtility.GetEditorCurve(generated, binding).Evaluate(0),
                Is.EqualTo(after.muscles[index] - beforeMuscles[index]).Within(.0002f), binding.propertyName);
        }
    }

    [TestCase(PosingSystemAutoPoseTargetSpace.BodyBone)]
    [TestCase(PosingSystemAutoPoseTargetSpace.AvatarRoot)]
    [TestCase(PosingSystemAutoPoseTargetSpace.Ground)]
    public void WorldPosition_UsesGlobalAxesAroundAnchor_WithoutChangingRotation(PosingSystemAutoPoseTargetSpace space)
    {
        var rule = Rule(); rule.targetSpace = space; rule.targetBone = HumanBodyBones.Head;
        rule.lengthReference = PosingSystemAutoPoseLengthReference.Forearm;
        rule.normalizedTargetOffset = new Vector3(.2f, -.1f, .3f);
        rule.targetRotationOffset = Quaternion.Euler(10, 20, 30);
        var head = animator.GetBoneTransform(HumanBodyBones.Head);
        head.localRotation = Quaternion.Euler(32, 17, -41);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out _, out var oldRotation, out _), Is.True);
        rule.positionSpace = PosingSystemAutoPosePositionSpace.World;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryMeasureReferenceLength(animator, rule.lengthReference, rule.limb, out var length, out _), Is.True);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out var first, out var rotation, out _), Is.True);
        var anchor = space == PosingSystemAutoPoseTargetSpace.BodyBone ? head.position : animator.transform.position;
        Assert.That(Vector3.Distance(first, anchor + rule.normalizedTargetOffset * length), Is.LessThan(.00001f));
        Assert.That(Quaternion.Angle(rotation, oldRotation), Is.LessThan(.01f));
        rule.normalizedTargetOffset.z += .1f;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out var second, out _, out _), Is.True);
        Assert.That(Vector3.Distance(second - first, Vector3.forward * (.1f * length)), Is.LessThan(.00001f));
        if (space == PosingSystemAutoPoseTargetSpace.BodyBone)
        {
            head.localRotation *= Quaternion.Euler(-38, 27, 64);
            Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out var third, out _, out _), Is.True);
            Assert.That(Vector3.Distance(second, third), Is.LessThan(.00001f), "Bone orientation must not rotate a world offset");
            head.position += new Vector3(.1f, -.02f, .03f);
            Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out var fourth, out _, out _), Is.True);
            Assert.That(Vector3.Distance(fourth - third, new Vector3(.1f, -.02f, .03f)), Is.LessThan(.00001f));
        }
    }

    [Test]
    public void CoordinateAndLengthSwitch_RoundTripsWithoutMovingTargetOrChangingRotation()
    {
        var rule = Rule(); rule.targetSpace = PosingSystemAutoPoseTargetSpace.BodyBone;
        rule.targetBone = HumanBodyBones.Head;
        animator.GetBoneTransform(HumanBodyBones.Head).localRotation = Quaternion.Euler(21, 32, 43);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryCaptureRuleFromCurrentPose(animator, rule, out _), Is.True);
        var originalOffset = rule.normalizedTargetOffset;
        var originalRotationOffset = rule.targetRotationOffset;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out var before, out var beforeRotation, out _), Is.True);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryConvertPositionSettings(animator, rule,
            PosingSystemAutoPoseLengthReference.Forearm, PosingSystemAutoPosePositionSpace.World, out var converted, out _), Is.True);
        rule.normalizedTargetOffset = converted; rule.lengthReference = PosingSystemAutoPoseLengthReference.Forearm;
        rule.positionSpace = PosingSystemAutoPosePositionSpace.World;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out var after, out var afterRotation, out _), Is.True);
        Assert.That(Vector3.Distance(before, after), Is.LessThan(.00001f));
        Assert.That(Quaternion.Angle(beforeRotation, afterRotation), Is.LessThan(.01f));
        Assert.That(rule.targetRotationOffset, Is.EqualTo(originalRotationOffset));
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryConvertPositionSettings(animator, rule,
            PosingSystemAutoPoseLengthReference.AvatarHeight, PosingSystemAutoPosePositionSpace.ReferenceLocal,
            out var restored, out _), Is.True);
        Assert.That(Vector3.Distance(originalOffset, restored), Is.LessThan(.00001f));
    }

    [Test]
    public void WorldCaptureAndSceneHandle_RoundTripAndPreserveDisabledValues()
    {
        var rule = Rule(); rule.positionSpace = PosingSystemAutoPosePositionSpace.World;
        rule.targetSpace = PosingSystemAutoPoseTargetSpace.BodyBone; rule.targetBone = HumanBodyBones.Head;
        var tip = animator.GetBoneTransform(HumanBodyBones.LeftHand);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryCaptureRuleFromCurrentPose(animator, rule, out _), Is.True);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out var target, out var rotation, out _), Is.True);
        Assert.That(Vector3.Distance(tip.position, target), Is.LessThan(.00001f));
        Assert.That(Quaternion.Angle(tip.rotation, rotation), Is.LessThan(.01f));
        rule.applyPositionX = false;
        var savedX = rule.normalizedTargetOffset.x;
        var moved = tip.position + new Vector3(.1f, .02f, .03f);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TrySetTargetFromWorld(animator, rule, moved, rotation, out _), Is.True);
        Assert.That(rule.normalizedTargetOffset.x, Is.EqualTo(savedX));
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(animator, rule, out target, out _, out _), Is.True);
        Assert.That(target.x, Is.EqualTo(tip.position.x).Within(.00001f));
        Assert.That(target.y, Is.EqualTo(moved.y).Within(.00001f));
        Assert.That(target.z, Is.EqualTo(moved.z).Within(.00001f));
    }

    [Test]
    public void KneeBaseline_UsesRestSkeletonWithoutChangingAnAlreadyPosedSource()
    {
        Assert.That(PosingSystemAvatarAdjustmentUtility.TryReadKneeBaseline(animator, out var baseline, out var message), Is.True, message);
        animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).localRotation *= Quaternion.Euler(50, 0, 0);
        var transforms = animator.GetComponentsInChildren<Transform>();
        var positions = transforms.Select(t => t.localPosition).ToArray();
        var rotations = transforms.Select(t => t.localRotation).ToArray();
        Assert.That(PosingSystemAvatarAdjustmentUtility.TryReadKneeBaseline(animator, out var posedBaseline, out message), Is.True, message);
        Assert.That(Vector2.Distance(baseline, posedBaseline), Is.LessThan(.001f));
        Assert.That(transforms.Select(t => t.localPosition), Is.EqualTo(positions));
        Assert.That(transforms.Select(t => t.localRotation), Is.EqualTo(rotations));
    }

    [Test]
    public void KneeSafety_RunsBeforeSourceBendCaptureAndAfterIk()
    {
        var system = animator.gameObject.AddComponent<AvatarAdjustmentTestPosingSystem>();
        system.enableReverseKneeSafetyClamp = true;
        system.reverseKneeSafetyAngle = 25;
        var safety = PosingSystemAvatarAdjustmentUtility.GetKneeSafety(system);
        using var handler = new HumanPoseHandler(animator.avatar, animator.transform);
        var source = new HumanPose(); handler.GetHumanPose(ref source);
        source.muscles[24] = source.muscles[32] = 1.15f;
        handler.SetHumanPose(ref source);
        var rule = Rule(); rule.limb = PosingSystemAutoPoseLimb.LeftLeg;
        rule.bendDirectionMode = PosingSystemAutoPoseBendDirectionMode.SourcePose;
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryCaptureRuleFromCurrentPose(animator, rule, out _), Is.True);
        rule.normalizedTargetOffset += new Vector3(0, .07f, .015f);
        var definition = new PosingSystemAutoPoseDefinition { limbRules = new() { rule } };

        // Explicit reference pipeline, including correction BEFORE the solver reads its bend plane.
        safety.ApplyToAnimator(animator);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryApplyDefinition(animator, definition), Is.True);
        safety.ApplyToAnimator(animator);
        var expectedKnee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position;
        var expectedFoot = animator.GetBoneTransform(HumanBodyBones.LeftFoot).position;
        handler.SetHumanPose(ref source);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryApplyDefinition(animator, definition, kneeSafety: safety), Is.True);
        Assert.That(Vector3.Distance(expectedKnee, animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg).position), Is.LessThan(.001f));
        Assert.That(Vector3.Distance(expectedFoot, animator.GetBoneTransform(HumanBodyBones.LeftFoot).position), Is.LessThan(.001f));

        // An unreachable target straightens the leg; final safety must enforce the physical margin again.
        rule.normalizedTargetOffset = new Vector3(0, -10, 0);
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryApplyDefinition(animator, definition, kneeSafety: safety), Is.True);
        var final = new HumanPose(); handler.GetHumanPose(ref final);
        Assert.That(final.muscles[24], Is.LessThanOrEqualTo(1 - 25f / HumanTrait.GetMuscleDefaultMax(24) + .002f));
    }

    [Test]
    public void GeneratedIkClip_ContainsKneeSafetyEvenWithOnlyArmRules()
    {
        var system = animator.gameObject.AddComponent<AvatarAdjustmentTestPosingSystem>();
        system.enableReverseKneeSafetyClamp = true;
        system.reverseKneeSafetyAngle = 15;
        var safety = PosingSystemAvatarAdjustmentUtility.GetKneeSafety(system);
        var clip = PosingSystemConverter.RootOffsetClip(Vector3.up, Quaternion.identity, "KneeSafetySource");
        assets.Add(clip);
        foreach (var index in new[] { 24, 32 })
            AnimationUtility.SetEditorCurve(clip, EditorCurveBinding.FloatCurve("", typeof(Animator), HumanTrait.MuscleName[index]),
                AnimationCurve.Constant(0, 1, 1.15f));
        clip.SampleAnimation(animator.gameObject, 0);
        var rule = Rule();
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryCaptureRuleFromCurrentPose(animator, rule, out _), Is.True);
        var definition = new PosingSystemAutoPoseDefinition { limbRules = new() { rule } };
        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryCreateAdjustmentClip(animator, clip, definition,
            out var generated, out _, out var message, safety), Is.True, message);
        assets.Add(generated);
        foreach (var index in new[] { 24, 32 })
        {
            var curve = AnimationUtility.GetEditorCurve(generated,
                EditorCurveBinding.FloatCurve("", typeof(Animator), HumanTrait.MuscleName[index]));
            Assert.That(curve, Is.Not.Null);
            Assert.That(curve.Evaluate(0), Is.LessThan(-.05f));
        }
    }

    [TestCase(.6f)]
    [TestCase(1f)]
    [TestCase(1.4f)]
    public void KneeSafety_WithoutIkRules_PreservesPlacementAndUnrelatedBones(float scale)
    {
        animator.transform.localScale = Vector3.one * scale;
        var system = animator.gameObject.AddComponent<AvatarAdjustmentTestPosingSystem>();
        system.enableReverseKneeSafetyClamp = true;
        system.reverseKneeSafetyAngle = 15;
        var safety = PosingSystemAvatarAdjustmentUtility.GetKneeSafety(system);
        using var handler = new HumanPoseHandler(animator.avatar, animator.transform);
        var pose = new HumanPose(); handler.GetHumanPose(ref pose);
        pose.muscles[24] = 1.15f;
        pose.muscles[32] = .2f; // The unaffected knee must remain unchanged too.
        handler.SetHumanPose(ref pose);
        var transforms = animator.GetComponentsInChildren<Transform>();
        var positions = transforms.Select(t => t.localPosition).ToArray();
        var rotations = transforms.Select(t => t.localRotation).ToArray();
        var hips = animator.GetBoneTransform(HumanBodyBones.Hips);
        var head = animator.GetBoneTransform(HumanBodyBones.Head);
        var leftKnee = animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg);
        var hipsPosition = hips.position;
        var headPosition = head.position;
        var kneeRotation = leftKnee.localRotation;

        Assert.That(PosingSystemAutoPoseAdjustmentUtility.TryApplyDefinition(animator,
            new PosingSystemAutoPoseDefinition(), kneeSafety: safety), Is.False);
        Assert.That(Vector3.Distance(hipsPosition, hips.position), Is.LessThan(.00001f), "Knee safety moved the hips in world space");
        Assert.That(Vector3.Distance(headPosition, head.position), Is.LessThan(.00001f), "Knee safety moved the head");
        for (var i = 0; i < transforms.Length; i++)
        {
            Assert.That(Vector3.Distance(positions[i], transforms[i].localPosition), Is.LessThan(.00001f), transforms[i].name);
            if (transforms[i] != leftKnee)
                Assert.That(Quaternion.Angle(rotations[i], transforms[i].localRotation), Is.LessThan(.01f), transforms[i].name);
        }
        Assert.That(Quaternion.Angle(kneeRotation, leftKnee.localRotation), Is.GreaterThan(1), "The knee still needs to be corrected");
        handler.GetHumanPose(ref pose);
        Assert.That(pose.muscles[24], Is.LessThanOrEqualTo(1 - 15f / HumanTrait.GetMuscleDefaultMax(24) + .002f));

        // Repeated Inspector updates must not accumulate a translation or rotate unrelated bones.
        for (var iteration = 0; iteration < 5; iteration++) safety.ApplyToAnimator(animator);
        Assert.That(Vector3.Distance(hipsPosition, hips.position), Is.LessThan(.00001f));
    }

    private Animator CreateAvatar()
    {
        var root = new GameObject("IK Axis Test");
        SceneManager.MoveGameObjectToScene(root, scene);
        var armature = new GameObject("Armature").transform;
        armature.SetParent(root.transform, false);
        var bones = new Dictionary<HumanBodyBones, Transform>();
        Bone(HumanBodyBones.Hips, null, new Vector3(0, 1, 0));
        Bone(HumanBodyBones.Spine, HumanBodyBones.Hips, new Vector3(0, .15f, 0));
        Bone(HumanBodyBones.Chest, HumanBodyBones.Spine, new Vector3(0, .15f, 0));
        Bone(HumanBodyBones.Neck, HumanBodyBones.Chest, new Vector3(0, .2f, 0));
        Bone(HumanBodyBones.Head, HumanBodyBones.Neck, new Vector3(0, .12f, 0));
        foreach (var left in new[] { true, false })
        {
            var side = left ? "Left" : "Right"; var sign = left ? -1 : 1;
            HumanBodyBones B(string suffix) => (HumanBodyBones)Enum.Parse(typeof(HumanBodyBones), side + suffix);
            Bone(B("UpperLeg"), HumanBodyBones.Hips, new Vector3(.1f * sign, -.05f, 0));
            Bone(B("LowerLeg"), B("UpperLeg"), new Vector3(0, left ? -.43f : -.46f, .01f));
            Bone(B("Foot"), B("LowerLeg"), new Vector3(0, -.4f, -.01f));
            Bone(B("Shoulder"), HumanBodyBones.Chest, new Vector3(.1f * sign, .1f, 0));
            Bone(B("UpperArm"), B("Shoulder"), new Vector3(.1f * sign, 0, 0));
            Bone(B("LowerArm"), B("UpperArm"), new Vector3((left ? .25f : .28f) * sign, 0, -.01f));
            Bone(B("Hand"), B("LowerArm"), new Vector3(.23f * sign, 0, .01f));
        }
        var avatar = AvatarBuilder.BuildHumanAvatar(root, new HumanDescription {
            human = bones.Select(b => new HumanBone { boneName = b.Value.name, humanName = HumanTrait.BoneName[(int)b.Key], limit = new HumanLimit { useDefaultValues = true } }).ToArray(),
            skeleton = root.GetComponentsInChildren<Transform>().Select(t => new SkeletonBone { name = t.name, position = t.localPosition, rotation = t.localRotation, scale = t.localScale }).ToArray(),
            upperArmTwist = .5f, lowerArmTwist = .5f, upperLegTwist = .5f, lowerLegTwist = .5f, armStretch = .05f, legStretch = .05f });
        assets.Add(avatar);
        Assert.That(avatar.isValid && avatar.isHuman, Is.True);
        var result = root.AddComponent<Animator>(); result.avatar = avatar; result.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        return result;

        void Bone(HumanBodyBones bone, HumanBodyBones? parent, Vector3 localPosition)
        {
            var obj = new GameObject(bone.ToString()); obj.transform.SetParent(parent.HasValue ? bones[parent.Value] : armature, false);
            obj.transform.localPosition = localPosition; bones.Add(bone, obj.transform);
        }
    }
}
