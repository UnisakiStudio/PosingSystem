using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using jp.unisakistudio.posingsystemeditor;
using Object = UnityEngine.Object;

// "アバターの姿勢をリセット" puts the pose back and leaves the shape and the place of the avatar alone.
// Until 3.2.0-alpha.5 it copied position, rotation and scale of every transform from the model, so a body
// shaped with bone scales, and the scale of the avatar itself, were lost (inquiry of 2026-11-12).
public sealed class PosingSystemResetAvatarPoseTests
{
    private readonly List<Object> assets = new();
    private readonly List<string> savedAssets = new();
    private Scene scene;

    [SetUp]
    public void SetUp() => scene = EditorSceneManager.NewPreviewScene();

    [TearDown]
    public void TearDown()
    {
        if (scene.IsValid()) EditorSceneManager.ClosePreviewScene(scene);
        foreach (var path in savedAssets) AssetDatabase.DeleteAsset(path);
        savedAssets.Clear();
        foreach (var asset in assets) if (asset != null) Object.DestroyImmediate(asset);
        assets.Clear();
    }

    [Test]
    public void ThePoseGoesBackToTheModel()
    {
        var model = CreateAvatar("Model");
        var avatar = CreateAvatar("Avatar");
        Pose(avatar);

        var restored = PosingSystemEditor.RestoreRestPose(avatar, model.gameObject);

        Assert.That(restored, Is.EqualTo(Bones.Length), "every humanoid bone of the model");
        foreach (var bone in Bones)
            AssertRotation(avatar.GetBoneTransform(bone), model.GetBoneTransform(bone).localRotation, bone.ToString());
        AssertVector(avatar.GetBoneTransform(HumanBodyBones.Hips).localPosition, model.GetBoneTransform(HumanBodyBones.Hips).localPosition, "hips position");
    }

    [Test]
    public void TheShapeOfTheBodyStays()
    {
        var model = CreateAvatar("Model");
        var avatar = CreateAvatar("Avatar");
        // A body shaped the usual ways: bones scaled and moved, the armature and the avatar itself scaled.
        var shaped = new Dictionary<Transform, (Vector3 position, Vector3 scale)>();
        void Shape(Transform t, Vector3 move, Vector3 scale)
        {
            t.localPosition += move; t.localScale = scale;
            shaped[t] = (t.localPosition, t.localScale);
        }
        Shape(avatar.transform, new Vector3(2, 0, -3), new Vector3(1.344f, 1.344f, 1.344f));
        Shape(avatar.transform.Find("Armature"), new Vector3(0, .03f, 0), new Vector3(1.1f, 1.1f, 1.1f));
        Shape(avatar.GetBoneTransform(HumanBodyBones.Chest), new Vector3(0, .02f, .01f), new Vector3(1.2f, 1.05f, 1.3f));
        Shape(avatar.GetBoneTransform(HumanBodyBones.Head), Vector3.zero, new Vector3(1.15f, 1.15f, 1.15f));
        Shape(avatar.GetBoneTransform(HumanBodyBones.LeftUpperLeg), new Vector3(-.01f, 0, 0), new Vector3(.9f, 1.1f, .9f));
        Shape(avatar.GetBoneTransform(HumanBodyBones.Hips), Vector3.zero, new Vector3(1.05f, 1, 1.05f));
        var rootRotation = avatar.transform.localRotation = Quaternion.Euler(0, 90, 0);
        // Parts of the model that are not humanoid bones keep their rotation as well (a tail, the armature).
        var tail = avatar.transform.Find("Armature/Hips/Tail");
        var tailRotation = tail.localRotation = Quaternion.Euler(35, 0, 10);
        Shape(tail, new Vector3(0, 0, -.05f), new Vector3(1, 1, 2));
        var armatureRotation = avatar.transform.Find("Armature").localRotation = Quaternion.Euler(0, 5, 0);
        var hipsPosition = avatar.GetBoneTransform(HumanBodyBones.Hips).localPosition;
        Pose(avatar);

        PosingSystemEditor.RestoreRestPose(avatar, model.gameObject);

        foreach (var entry in shaped)
        {
            AssertVector(entry.Key.localScale, entry.Value.scale, entry.Key.name + " scale");
            if (entry.Key != avatar.GetBoneTransform(HumanBodyBones.Hips))
                AssertVector(entry.Key.localPosition, entry.Value.position, entry.Key.name + " position");
        }
        AssertRotation(avatar.transform, rootRotation, "avatar rotation");
        AssertRotation(tail, tailRotation, "tail rotation");
        AssertRotation(avatar.transform.Find("Armature"), armatureRotation, "armature rotation");
        // The hips are where the pose put the body, so their position does go back to the model.
        Assert.That(Vector3.Distance(hipsPosition, model.GetBoneTransform(HumanBodyBones.Hips).localPosition), Is.LessThan(1e-5f));
        AssertVector(avatar.GetBoneTransform(HumanBodyBones.Hips).localPosition, model.GetBoneTransform(HumanBodyBones.Hips).localPosition, "hips position");
        AssertRotation(avatar.GetBoneTransform(HumanBodyBones.Chest), model.GetBoneTransform(HumanBodyBones.Chest).localRotation, "chest rotation");
    }

    [Test]
    public void BonesOfAnOutfitWithTheSameNamesStay()
    {
        var model = CreateAvatar("Model");
        var avatar = CreateAvatar("Avatar");
        // An outfit brings its own Armature / Hips / Spine; they are not in the model of the avatar.
        var outfitHips = new GameObject("Hips").transform;
        var outfitArmature = new GameObject("Armature").transform;
        var outfit = new GameObject("Outfit").transform;
        outfit.SetParent(avatar.transform, false); outfitArmature.SetParent(outfit, false); outfitHips.SetParent(outfitArmature, false);
        var accessory = new GameObject("Ribbon").transform;
        accessory.SetParent(avatar.GetBoneTransform(HumanBodyBones.Head), false);
        var rotation = Quaternion.Euler(10, 20, 30);
        outfitHips.localRotation = accessory.localRotation = rotation;
        outfitHips.localPosition = accessory.localPosition = new Vector3(.1f, .2f, .3f);
        outfitHips.localScale = accessory.localScale = new Vector3(2, 2, 2);
        Pose(avatar);

        PosingSystemEditor.RestoreRestPose(avatar, model.gameObject);

        foreach (var t in new[] { outfitHips, accessory })
        {
            AssertRotation(t, rotation, t.name);
            AssertVector(t.localPosition, new Vector3(.1f, .2f, .3f), t.name + " position");
            AssertVector(t.localScale, new Vector3(2, 2, 2), t.name + " scale");
        }
        AssertRotation(avatar.GetBoneTransform(HumanBodyBones.LeftUpperArm), model.GetBoneTransform(HumanBodyBones.LeftUpperArm).localRotation, "the avatar's own arm");
    }

    [Test]
    public void AnAvatarThatIsSwitchedOffIsRestoredToo()
    {
        var model = CreateAvatar("Model");
        var avatar = CreateAvatar("Avatar");
        var arm = avatar.GetBoneTransform(HumanBodyBones.RightLowerArm);
        var hips = avatar.GetBoneTransform(HumanBodyBones.Hips);
        var rest = model.GetBoneTransform(HumanBodyBones.RightLowerArm).localRotation;
        var restHips = model.GetBoneTransform(HumanBodyBones.Hips).localPosition;
        Pose(avatar);
        avatar.gameObject.SetActive(false);

        var restored = PosingSystemEditor.RestoreRestPose(avatar, model.gameObject);

        Assert.That(restored, Is.EqualTo(Bones.Length));
        AssertRotation(arm, rest, "arm");
        AssertVector(hips.localPosition, restHips, "hips position");
    }

    [Test]
    public void ARigThatIsNotHumanoidGetsTheRotationsOfItsModel()
    {
        var model = CreateAvatar("Model");
        var avatar = CreateAvatar("Avatar");
        var generic = AvatarBuilder.BuildGenericAvatar(avatar.gameObject, "");
        assets.Add(generic);
        var tail = avatar.transform.Find("Armature/Hips/Tail");
        var chest = avatar.transform.Find("Armature/Hips/Spine/Chest");
        Pose(avatar);
        avatar.avatar = generic;
        tail.localRotation = Quaternion.Euler(50, 0, 0);
        chest.localScale = new Vector3(1.3f, 1.3f, 1.3f);
        chest.localPosition += new Vector3(0, .05f, 0);
        var chestPosition = chest.localPosition;

        PosingSystemEditor.RestoreRestPose(avatar, model.gameObject);

        AssertRotation(tail, Quaternion.identity, "tail");
        AssertRotation(chest, Quaternion.identity, "chest");
        AssertVector(chest.localScale, new Vector3(1.3f, 1.3f, 1.3f), "chest scale");
        AssertVector(chest.localPosition, chestPosition, "chest position");
    }

    // The button itself, on an avatar that comes from a prefab, as avatars in a scene do.
    [Test]
    public void TheButtonKeepsTheShapeAndThePlaceOfAnAvatarFromAPrefab()
    {
        var model = CreateAvatar("Model");
        var human = model.avatar;
        var path = "Assets/__USSPS_ResetAvatarPoseTest_" + Guid.NewGuid().ToString("N") + ".prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(model.gameObject, path);
        savedAssets.Add(path);
        Assert.That(prefab, Is.Not.Null);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, scene);
        var avatar = instance.GetComponent<Animator>();
        avatar.avatar = human;
        var descriptor = instance.AddComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>();
        instance.transform.localPosition = new Vector3(4, 0, 1);
        instance.transform.localRotation = Quaternion.Euler(0, 180, 0);
        instance.transform.localScale = new Vector3(1.344f, 1.344f, 1.344f);
        var chest = instance.transform.Find("Armature/Hips/Spine/Chest");
        chest.localScale = new Vector3(1.2f, 1.1f, 1.3f);
        Pose(avatar);
        var arm = instance.transform.Find("Armature/Hips/Spine/Chest/LeftShoulder/LeftUpperArm");
        Assert.That(Quaternion.Angle(arm.localRotation, Quaternion.identity), Is.GreaterThan(10), "the test must start from a pose");

        PosingSystemEditor.ResetAvatarPose(descriptor);

        AssertRotation(arm, Quaternion.identity, "arm");
        AssertVector(instance.transform.Find("Armature/Hips").localPosition, new Vector3(0, 1, 0), "hips position");
        AssertVector(chest.localScale, new Vector3(1.2f, 1.1f, 1.3f), "chest scale");
        AssertVector(instance.transform.localScale, new Vector3(1.344f, 1.344f, 1.344f), "avatar scale");
        AssertVector(instance.transform.localPosition, new Vector3(4, 0, 1), "avatar position");
        AssertRotation(instance.transform, Quaternion.Euler(0, 180, 0), "avatar rotation");
        Assert.That(instance.activeSelf, Is.True);
    }

    private static readonly HumanBodyBones[] Bones =
    {
        HumanBodyBones.Hips, HumanBodyBones.Spine, HumanBodyBones.Chest, HumanBodyBones.Neck, HumanBodyBones.Head,
        HumanBodyBones.LeftUpperLeg, HumanBodyBones.LeftLowerLeg, HumanBodyBones.LeftFoot,
        HumanBodyBones.LeftShoulder, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftHand,
        HumanBodyBones.RightUpperLeg, HumanBodyBones.RightLowerLeg, HumanBodyBones.RightFoot,
        HumanBodyBones.RightShoulder, HumanBodyBones.RightUpperArm, HumanBodyBones.RightLowerArm, HumanBodyBones.RightHand,
    };

    // A sitting pose written straight onto the bones, as a preview that was never taken back leaves it.
    private static void Pose(Animator avatar)
    {
        var index = 0;
        foreach (var bone in Bones)
        {
            var t = avatar.transform.Find(PathOf(bone));
            t.localRotation = Quaternion.Euler(20 + 7 * index, 15 - 5 * index, 30 + 3 * index);
            index++;
        }
        avatar.transform.Find(PathOf(HumanBodyBones.Hips)).localPosition = new Vector3(.2f, .35f, -.1f);
    }

    private static string PathOf(HumanBodyBones bone)
    {
        var name = bone.ToString();
        return Parents.TryGetValue(bone, out var parent) ? PathOf(parent) + "/" + name : "Armature/" + name;
    }

    private static readonly Dictionary<HumanBodyBones, HumanBodyBones> Parents = new()
    {
        [HumanBodyBones.Spine] = HumanBodyBones.Hips, [HumanBodyBones.Chest] = HumanBodyBones.Spine,
        [HumanBodyBones.Neck] = HumanBodyBones.Chest, [HumanBodyBones.Head] = HumanBodyBones.Neck,
        [HumanBodyBones.LeftUpperLeg] = HumanBodyBones.Hips, [HumanBodyBones.LeftLowerLeg] = HumanBodyBones.LeftUpperLeg,
        [HumanBodyBones.LeftFoot] = HumanBodyBones.LeftLowerLeg, [HumanBodyBones.LeftShoulder] = HumanBodyBones.Chest,
        [HumanBodyBones.LeftUpperArm] = HumanBodyBones.LeftShoulder, [HumanBodyBones.LeftLowerArm] = HumanBodyBones.LeftUpperArm,
        [HumanBodyBones.LeftHand] = HumanBodyBones.LeftLowerArm,
        [HumanBodyBones.RightUpperLeg] = HumanBodyBones.Hips, [HumanBodyBones.RightLowerLeg] = HumanBodyBones.RightUpperLeg,
        [HumanBodyBones.RightFoot] = HumanBodyBones.RightLowerLeg, [HumanBodyBones.RightShoulder] = HumanBodyBones.Chest,
        [HumanBodyBones.RightUpperArm] = HumanBodyBones.RightShoulder, [HumanBodyBones.RightLowerArm] = HumanBodyBones.RightUpperArm,
        [HumanBodyBones.RightHand] = HumanBodyBones.RightLowerArm,
    };

    private static void AssertRotation(Transform transform, Quaternion expected, string what) =>
        Assert.That(Quaternion.Angle(transform.localRotation, expected), Is.LessThan(.01f), what);

    private static void AssertVector(Vector3 actual, Vector3 expected, string what) =>
        Assert.That(Vector3.Distance(actual, expected), Is.LessThan(1e-5f), what + ": " + actual.ToString("F5") + " / " + expected.ToString("F5"));

    private Animator CreateAvatar(string name)
    {
        var root = new GameObject(name);
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
            Bone(B("LowerLeg"), B("UpperLeg"), new Vector3(0, -.45f, .01f));
            Bone(B("Foot"), B("LowerLeg"), new Vector3(0, -.4f, -.01f));
            Bone(B("Shoulder"), HumanBodyBones.Chest, new Vector3(.1f * sign, .1f, 0));
            Bone(B("UpperArm"), B("Shoulder"), new Vector3(.1f * sign, 0, 0));
            Bone(B("LowerArm"), B("UpperArm"), new Vector3(.26f * sign, 0, -.01f));
            Bone(B("Hand"), B("LowerArm"), new Vector3(.23f * sign, 0, .01f));
        }
        Assert.That(bones.Keys.OrderBy(b => b), Is.EqualTo(Bones.OrderBy(b => b)));
        var avatar = AvatarBuilder.BuildHumanAvatar(root, new HumanDescription {
            human = bones.Select(b => new HumanBone { boneName = b.Value.name, humanName = HumanTrait.BoneName[(int)b.Key], limit = new HumanLimit { useDefaultValues = true } }).ToArray(),
            skeleton = root.GetComponentsInChildren<Transform>().Select(t => new SkeletonBone { name = t.name, position = t.localPosition, rotation = t.localRotation, scale = t.localScale }).ToArray(),
            upperArmTwist = .5f, lowerArmTwist = .5f, upperLegTwist = .5f, lowerLegTwist = .5f, armStretch = .05f, legStretch = .05f });
        assets.Add(avatar);
        Assert.That(avatar.isValid && avatar.isHuman, Is.True);
        // A part of the model that is not a humanoid bone.
        var tail = new GameObject("Tail").transform;
        tail.SetParent(bones[HumanBodyBones.Hips], false);
        tail.localPosition = new Vector3(0, 0, -.1f);
        var result = root.AddComponent<Animator>(); result.avatar = avatar; result.cullingMode = AnimatorCullingMode.AlwaysAnimate;
        return result;

        void Bone(HumanBodyBones bone, HumanBodyBones? parent, Vector3 localPosition)
        {
            var obj = new GameObject(bone.ToString()); obj.transform.SetParent(parent.HasValue ? bones[parent.Value] : armature, false);
            obj.transform.localPosition = localPosition; bones.Add(bone, obj.transform);
        }
    }
}
