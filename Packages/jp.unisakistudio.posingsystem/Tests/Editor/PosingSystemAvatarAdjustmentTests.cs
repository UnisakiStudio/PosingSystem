using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;
using jp.unisakistudio.posingsystemeditor;

public class PosingSystemAvatarAdjustmentTests
{
    [Test]
    public void IkKneeSafetyChanges_InvalidatePrebuiltClips()
    {
        var obj = new GameObject("IK cache test");
        var catalog = ScriptableObject.CreateInstance<PosingSystemAutoPoseCatalog>();
        try
        {
            var system = obj.AddComponent<AvatarAdjustmentTestPosingSystem>();
            system.autoPoseCatalog = catalog;
            catalog.productionReady = true;
            system.enableReverseKneeSafetyClamp = true;
            var initial = PosingSystemConverter.GetDefineSerializeJson(system, true);
            Assert.That(initial, Does.Contain("\"autoPoseLogicVersion\":2"));
            system.reverseKneeSafetyAngle += 5;
            var afterAngle = PosingSystemConverter.GetDefineSerializeJson(system, true);
            Assert.That(afterAngle, Is.Not.EqualTo(initial));
            system.avatarSpecificLeftKneeBendDegrees += 3;
            var afterBaseline = PosingSystemConverter.GetDefineSerializeJson(system, true);
            Assert.That(afterBaseline, Is.Not.EqualTo(afterAngle));
            system.enableReverseKneeSafetyClamp = false;
            Assert.That(PosingSystemConverter.GetDefineSerializeJson(system, true), Is.Not.EqualTo(afterBaseline));
        }
        finally { Object.DestroyImmediate(obj); Object.DestroyImmediate(catalog); }
    }

    [Test]
    public void KneeSafety_TargetBaselineIsIndependentAndDisabledSafetyDoesNotChangePose()
    {
        var obj = new GameObject("Knee safety test");
        try
        {
            var system = obj.AddComponent<AvatarAdjustmentTestPosingSystem>();
            system.enableReverseKneeSafetyClamp = true;
            system.reverseKneeSafetyAngle = 5;
            system.avatarSpecificLeftKneeBendDegrees = 80;
            system.avatarSpecificRightKneeBendDegrees = 80;
            var settingsBefore = EditorJsonUtility.ToJson(system);
            var pose = new HumanPose { muscles = new float[HumanTrait.MuscleCount] };
            pose.muscles[24] = pose.muscles[32] = 1.5f;
            pose.muscles[42] = .25f;
            var safety = PosingSystemAvatarAdjustmentUtility.GetKneeSafety(system, new Vector2(-5, 2));
            Assert.That(safety.Apply(ref pose), Is.True);
            Assert.That(pose.muscles[24], Is.LessThan(pose.muscles[32]));
            Assert.That(pose.muscles[24], Is.EqualTo(1 - 10f / HumanTrait.GetMuscleDefaultMax(24) - .0005f).Within(.00001f));
            Assert.That(pose.muscles[42], Is.EqualTo(.25f));
            Assert.That(safety.Apply(ref pose), Is.False, "Applying the same limit twice must not accumulate offsets");
            Assert.That(EditorJsonUtility.ToJson(system), Is.EqualTo(settingsBefore));
            system.enableReverseKneeSafetyClamp = false;
            pose.muscles[24] = 1.5f;
            Assert.That(PosingSystemAvatarAdjustmentUtility.GetKneeSafety(system).Apply(ref pose), Is.False);
            Assert.That(pose.muscles[24], Is.EqualTo(1.5f));
        }
        finally { Object.DestroyImmediate(obj); }
    }

    [Test]
    public void CreateCombinedMuscleAdjustmentClip_AddsManualAndAvatarOffsets()
    {
        var gameObject = new GameObject("PosingSystemAvatarAdjustmentTests");
        var posingSystem = gameObject.AddComponent<AvatarAdjustmentTestPosingSystem>();
        var manual = CreateMuscleClip(PosingSystemAvatarAdjustmentUtility.LeftKneeMuscleIndex, 0.2f);
        var avatar = CreateMuscleClip(PosingSystemAvatarAdjustmentUtility.LeftKneeMuscleIndex, -0.03f);
        posingSystem.enableAvatarSpecificAdjustment = true;
        posingSystem.avatarSpecificAdjustmentClip = avatar;

        var combined = PosingSystemAvatarAdjustmentUtility.CreateCombinedMuscleAdjustmentClip(
            posingSystem, manual);

        try
        {
            var values = AdditiveCurveUtility.GetMuscleAdjustmentsAtTime(combined, 0f);
            Assert.That(values[PosingSystemAvatarAdjustmentUtility.LeftKneeMuscleIndex],
                Is.EqualTo(0.17f).Within(0.00001f));
        }
        finally
        {
            Object.DestroyImmediate(combined);
            Object.DestroyImmediate(manual);
            Object.DestroyImmediate(avatar);
            Object.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void ClampKnees_PreventsExtensionPastConfiguredPhysicalMargin()
    {
        var gameObject = new GameObject("PosingSystemAvatarAdjustmentTests");
        var posingSystem = gameObject.AddComponent<AvatarAdjustmentTestPosingSystem>();
        posingSystem.enableReverseKneeSafetyClamp = true;
        posingSystem.reverseKneeSafetyAngle = 0.5f;
        posingSystem.avatarSpecificLeftKneeBendDegrees = 1f;
        posingSystem.avatarSpecificRightKneeBendDegrees = 1f;

        var pose = new HumanPose { muscles = new float[HumanTrait.MuscleCount] };
        pose.muscles[PosingSystemAvatarAdjustmentUtility.LeftKneeMuscleIndex] = 1.2f;
        pose.muscles[PosingSystemAvatarAdjustmentUtility.RightKneeMuscleIndex] = 0.9f;

        PosingSystemAvatarAdjustmentUtility.ClampKnees(posingSystem, ref pose);

        var range = HumanTrait.GetMuscleDefaultMax(
            PosingSystemAvatarAdjustmentUtility.LeftKneeMuscleIndex);
        var expectedMaximum = 1f + 0.5f / range - 0.0005f;
        Assert.That(pose.muscles[PosingSystemAvatarAdjustmentUtility.LeftKneeMuscleIndex],
            Is.EqualTo(expectedMaximum).Within(0.00001f));
        Assert.That(pose.muscles[PosingSystemAvatarAdjustmentUtility.RightKneeMuscleIndex],
            Is.EqualTo(0.9f).Within(0.00001f));

        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void GetAffectedMuscleIndices_AlwaysIncludesKneesWhenSafetyClampIsEnabled()
    {
        var gameObject = new GameObject("PosingSystemAvatarAdjustmentTests");
        var posingSystem = gameObject.AddComponent<AvatarAdjustmentTestPosingSystem>();
        posingSystem.enableAvatarSpecificAdjustment = false;
        posingSystem.enableReverseKneeSafetyClamp = true;

        var indices = PosingSystemAvatarAdjustmentUtility.GetAffectedMuscleIndices(posingSystem);

        Assert.That(indices, Does.Contain(PosingSystemAvatarAdjustmentUtility.LeftKneeMuscleIndex));
        Assert.That(indices, Does.Contain(PosingSystemAvatarAdjustmentUtility.RightKneeMuscleIndex));
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void ApplyToPose_DebugOverrideControlsOnlyAvatarCorrection()
    {
        var gameObject = new GameObject("PosingSystemAvatarAdjustmentTests");
        var posingSystem = gameObject.AddComponent<AvatarAdjustmentTestPosingSystem>();
        var avatar = CreateMuscleClip(0, 0.2f);
        posingSystem.enableAvatarSpecificAdjustment = true;
        posingSystem.enableReverseKneeSafetyClamp = false;
        posingSystem.avatarSpecificAdjustmentClip = avatar;

        var disabledPose = new HumanPose { muscles = new float[HumanTrait.MuscleCount] };
        PosingSystemAvatarAdjustmentUtility.ApplyToPose(
            posingSystem, ref disabledPose, 0f, false);
        Assert.That(disabledPose.muscles[0], Is.EqualTo(0f).Within(0.00001f));

        var enabledPose = new HumanPose { muscles = new float[HumanTrait.MuscleCount] };
        PosingSystemAvatarAdjustmentUtility.ApplyToPose(
            posingSystem, ref enabledPose, 0f, true);
        Assert.That(enabledPose.muscles[0], Is.EqualTo(0.2f).Within(0.00001f));

        Object.DestroyImmediate(avatar);
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void EnsureUsageDefault_DisablesCorrectionForLegacyInstallation()
    {
        var gameObject = new GameObject("PosingSystemAvatarAdjustmentTests");
        var posingSystem = gameObject.AddComponent<AvatarAdjustmentTestPosingSystem>();
        posingSystem.enableAvatarSpecificAdjustment = true;
        posingSystem.avatarSpecificAdjustmentUsageInitialized = false;

        var changed = PosingSystemAvatarAdjustmentInspector.EnsureUsageDefault(posingSystem);

        Assert.That(changed, Is.True);
        Assert.That(posingSystem.enableAvatarSpecificAdjustment, Is.False);
        Assert.That(posingSystem.avatarSpecificAdjustmentUsageInitialized, Is.True);
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void ConfigurePresetApplied_DisablesCorrectionButKeepsKneeProtectionEnabled()
    {
        var gameObject = new GameObject("PosingSystemAvatarAdjustmentTests");
        var posingSystem = gameObject.AddComponent<AvatarAdjustmentTestPosingSystem>();
        posingSystem.enableAvatarSpecificAdjustment = true;
        posingSystem.enableReverseKneeSafetyClamp = true;

        PosingSystemAvatarAdjustmentInspector.ConfigurePresetApplied(posingSystem);

        Assert.That(posingSystem.enableAvatarSpecificAdjustment, Is.False);
        Assert.That(posingSystem.avatarSpecificAdjustmentUsageInitialized, Is.True);
        Assert.That(posingSystem.enableReverseKneeSafetyClamp, Is.True);
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void ConfigureNewInstallation_EnablesCorrectionByDefault()
    {
        var gameObject = new GameObject("PosingSystemAvatarAdjustmentTests");
        var posingSystem = gameObject.AddComponent<AvatarAdjustmentTestPosingSystem>();
        posingSystem.enableAvatarSpecificAdjustment = false;

        PosingSystemAvatarAdjustmentInspector.ConfigureNewInstallation(posingSystem);

        Assert.That(posingSystem.enableAvatarSpecificAdjustment, Is.True);
        Assert.That(posingSystem.avatarSpecificAdjustmentUsageInitialized, Is.True);
        Object.DestroyImmediate(gameObject);
    }

    [Test]
    public void EnsureUsageDefault_DoesNotOverrideUserChoiceAfterInitialization()
    {
        var gameObject = new GameObject("PosingSystemAvatarAdjustmentTests");
        var posingSystem = gameObject.AddComponent<AvatarAdjustmentTestPosingSystem>();
        posingSystem.enableAvatarSpecificAdjustment = true;
        posingSystem.avatarSpecificAdjustmentUsageInitialized = true;

        var changed = PosingSystemAvatarAdjustmentInspector.EnsureUsageDefault(posingSystem);

        Assert.That(changed, Is.False);
        Assert.That(posingSystem.enableAvatarSpecificAdjustment, Is.True);
        Object.DestroyImmediate(gameObject);
    }

    private static AnimationClip CreateMuscleClip(int muscleIndex, float value)
    {
        var clip = new AnimationClip();
        var binding = EditorCurveBinding.FloatCurve(
            string.Empty,
            typeof(Animator),
            HumanTrait.MuscleName[muscleIndex]);
        AnimationUtility.SetEditorCurve(clip, binding, AnimationCurve.Constant(0f, 1f / 60f, value));
        return clip;
    }
}

public class AvatarAdjustmentTestPosingSystem : PosingSystem
{
    public override string AvatarReferenceProfileResourceName => "TestReferenceProfile";
}
