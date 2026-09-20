using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using jp.unisakistudio.posingsystem;
using jp.unisakistudio.posingsystemeditor;

public class PosingSystemAutoPoseTests
{
    [Test]
    public void NewRule_PreservesSourcePoseBendDirectionByDefault()
    {
        var rule = new PosingSystemAutoPoseLimbRule();

        Assert.That(
            rule.bendDirectionMode,
            Is.EqualTo(PosingSystemAutoPoseBendDirectionMode.SourcePose));
    }

    [Test]
    public void CatalogFind_PrefersStablePoseKey()
    {
        var catalog = ScriptableObject.CreateInstance<PosingSystemAutoPoseCatalog>();
        var clip = new AnimationClip();
        var keyed = new PosingSystemAutoPoseDefinition { poseKey = "座り_膝に手", sourceAnimation = clip };
        var fallback = new PosingSystemAutoPoseDefinition { poseKey = "別の姿勢", sourceAnimation = clip };
        catalog.poses.Add(keyed);
        catalog.poses.Add(fallback);

        Assert.That(catalog.Find("座り_膝に手", clip), Is.SameAs(keyed));
        Assert.That(catalog.Find("見つからない", clip), Is.SameAs(keyed));

        Object.DestroyImmediate(clip);
        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void ProductionCatalog_ActivatesOnlyAfterExplicitReadiness()
    {
        var root = new GameObject("PosingSystem");
        var posingSystem = root.AddComponent<PosingSystem>();
        var catalog = ScriptableObject.CreateInstance<PosingSystemAutoPoseCatalog>();
        posingSystem.autoPoseCatalog = catalog;

        Assert.That(posingSystem.UsesAutoPoseCatalog, Is.False);
        catalog.productionReady = true;
        Assert.That(posingSystem.UsesAutoPoseCatalog, Is.True);

        Object.DestroyImmediate(root);
        Object.DestroyImmediate(catalog);
    }

    [Test]
    public void TwoBoneSolver_ReachesTargetWithinLimbLength()
    {
        var success = PosingSystemTwoBoneIk.TrySolve(
            Vector3.zero,
            Vector3.right,
            Vector3.right * 2f,
            new Vector3(1f, 1f, 0f),
            Vector3.forward,
            out var solution);

        Assert.That(success, Is.True);
        Assert.That(Vector3.Distance(solution.TipPosition, new Vector3(1f, 1f, 0f)), Is.LessThan(0.0001f));
        Assert.That(Vector3.Distance(Vector3.zero, solution.JointPosition), Is.EqualTo(1f).Within(0.0001f));
        Assert.That(solution.TargetClamped, Is.False);
    }

    [Test]
    public void TwoBoneSolver_ClampsUnreachableTarget()
    {
        var success = PosingSystemTwoBoneIk.TrySolve(
            Vector3.zero,
            Vector3.right,
            Vector3.right * 2f,
            Vector3.right * 3f,
            Vector3.forward,
            out var solution);

        Assert.That(success, Is.True);
        Assert.That(solution.TipPosition.x, Is.EqualTo(2f).Within(0.0001f));
        Assert.That(solution.ReachError, Is.EqualTo(1f).Within(0.0001f));
        Assert.That(solution.TargetClamped, Is.True);
    }

    [Test]
    public void AvatarIndependentRotation_RemovesDifferentRigBoneAxes()
    {
        var intendedRotation = Quaternion.Euler(18f, -37f, 64f);
        var firstRigRestRotation = Quaternion.Euler(0f, 0f, 90f);
        var secondRigRestRotation = Quaternion.Euler(90f, 12f, -15f);
        var firstRigBoneRotation = intendedRotation * firstRigRestRotation;
        var secondRigBoneRotation = intendedRotation * secondRigRestRotation;

        var storedFromFirstRig = PosingSystemAutoPoseAdjustmentUtility
            .ToAvatarIndependentRotation(firstRigBoneRotation, firstRigRestRotation);
        var appliedToSecondRig = PosingSystemAutoPoseAdjustmentUtility
            .FromAvatarIndependentRotation(storedFromFirstRig, secondRigRestRotation);

        Assert.That(
            Quaternion.Angle(appliedToSecondRig, secondRigBoneRotation),
            Is.LessThan(0.0001f));
    }

    [Test]
    public void FindLeastUsedLimb_SelectsAnUnusedArmBeforeLegs()
    {
        var rules = new List<PosingSystemAutoPoseLimbRule>
        {
            new() { limb = PosingSystemAutoPoseLimb.RightArm },
        };

        Assert.That(
            PosingSystemAutoPoseAdjustmentUtility.FindLeastUsedLimb(rules),
            Is.EqualTo(PosingSystemAutoPoseLimb.LeftArm));

        rules.Add(new PosingSystemAutoPoseLimbRule
        {
            limb = PosingSystemAutoPoseLimb.LeftArm,
        });
        Assert.That(
            PosingSystemAutoPoseAdjustmentUtility.FindLeastUsedLimb(rules),
            Is.EqualTo(PosingSystemAutoPoseLimb.LeftLeg));
    }

    [Test]
    public void SupportedTargetBones_ExcludeFingersButKeepUsefulEndBones()
    {
        Assert.That(
            PosingSystemAutoPoseAdjustmentUtility.IsSupportedTargetBone(
                HumanBodyBones.LeftIndexProximal),
            Is.False);
        Assert.That(
            PosingSystemAutoPoseAdjustmentUtility.IsSupportedTargetBone(HumanBodyBones.Head),
            Is.True);
        Assert.That(
            PosingSystemAutoPoseAdjustmentUtility.IsSupportedTargetBone(HumanBodyBones.RightHand),
            Is.True);
        Assert.That(
            PosingSystemAutoPoseAdjustmentUtility.IsSupportedTargetBone(HumanBodyBones.LeftToes),
            Is.True);
    }
}
