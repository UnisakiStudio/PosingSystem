using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using nadena.dev.ndmf;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDK3.Avatars.Components;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemThumbnailTests
    {
        private readonly List<Object> cleanup = new();
        private Scene scene;
        private string assetFolder;
        private PosingSystem ps;
        private PosingSystem.AnimationDefine pose;
        private VRCExpressionsMenu menu;

        [SetUp]
        public void SetUp()
        {
            scene = EditorSceneManager.NewPreviewScene();
            var avatar = Track(new GameObject("ThumbnailTestAvatar"));
            SceneManager.MoveGameObjectToScene(avatar, scene);
            var descriptor = avatar.AddComponent<VRCAvatarDescriptor>();
            var child = new GameObject("PosingSystem");
            child.transform.SetParent(avatar.transform);
            ps = child.AddComponent<PosingSystem>();
            ps.isIconSmall = true;
            pose = new PosingSystem.AnimationDefine { displayName = "New pose", typeParameterValue = 1 };
            ps.defines.Add(new PosingSystem.LayerDefine("", "", "Pose", 0, "Poses",
                new List<PosingSystem.AnimationDefine> { pose }));
            // Leave overrideDefines null to cover avatars with no motion overrides.
            menu = Track(ScriptableObject.CreateInstance<VRCExpressionsMenu>());
            menu.controls.Add(new VRCExpressionsMenu.Control
            {
                type = VRCExpressionsMenu.Control.ControlType.Toggle,
                parameter = new VRCExpressionsMenu.Control.Parameter { name = "Pose" },
                value = 1
            });
            descriptor.expressionsMenu = menu;
        }

        [TearDown]
        public void TearDown()
        {
            AnimationMode.StopAnimationMode();
            foreach (var image in ps.defines.SelectMany(d => d.animations).Select(a => a.previewImage).Distinct())
                if (image != null && !EditorUtility.IsPersistent(image) && !cleanup.Contains(image))
                    Object.DestroyImmediate(image);
            for (var i = cleanup.Count - 1; i >= 0; i--)
                if (cleanup[i] != null) Object.DestroyImmediate(cleanup[i]);
            cleanup.Clear();
            if (!string.IsNullOrEmpty(assetFolder)) AssetDatabase.DeleteAsset(assetFolder);
            EditorSceneManager.ClosePreviewScene(scene);
        }

        [Test]
        public void Build_ReplacesDryImageInExistingPack_WithoutChangingSourceAssets()
        {
            AddHumanoid();
            AddVisibleBody(Color.green);
            assetFolder = "Assets/__PosingThumbnailTest_" + Guid.NewGuid().ToString("N");
            AssetDatabase.CreateFolder("Assets", assetFolder.Substring("Assets/".Length));
            var pack = ScriptableObject.CreateInstance<PosingSystemThumbnailPack>();
            AssetDatabase.CreateAsset(pack, assetFolder + "/Icons.asset");
            var original = new Texture2D(64, 64, TextureFormat.RGBA32, false);
            original.SetPixels(Enumerable.Repeat(Color.red, 64 * 64).ToArray());
            original.Apply();
            AssetDatabase.AddObjectToAsset(original, pack);
            AssetDatabase.SaveAssets();
            ps.thumbnailPackObject = pack;
            pose.previewImage = original;
            pose.previewImageIsFinal = false;
            var originalPixels = original.GetPixels32();
            var savedBytes = System.IO.File.ReadAllBytes(assetFolder + "/Icons.asset");
            var saver = new RecordingAssetSaver();

            BuildIcons(saver);

            Assert.That(pose.previewImage, Is.Not.Null);
            Assert.That(pose.previewImage, Is.Not.SameAs(original));
            Assert.That(pose.previewImageIsFinal, Is.True);
            Assert.That(menu.controls[0].icon, Is.SameAs(pose.previewImage));
            CollectionAssert.Contains(saver.saved, pose.previewImage);
            Assert.That(AssetDatabase.Contains(pose.previewImage), Is.False);
            CollectionAssert.AreEqual(originalPixels, original.GetPixels32());
            CollectionAssert.AreEqual(savedBytes, System.IO.File.ReadAllBytes(assetFolder + "/Icons.asset"));
            Assert.That(AssetDatabase.LoadAllAssetRepresentationsAtPath(assetFolder + "/Icons.asset"), Has.Length.EqualTo(1));
            Assert.That(pose.previewImage.GetPixels().Count(c => c.g > c.r + 0.2f && c.a > 0.5f), Is.GreaterThan(20));
        }

        [Test]
        public void Build_UsesProcessedAvatarInsteadOfCachedEditorPreview()
        {
            AddHumanoid();
            var body = AddVisibleBody(Color.red);
            var stale = Track(Object.Instantiate(ps.GetAvatar().gameObject));
            SceneManager.MoveGameObjectToScene(stale, scene);
            stale.name = "StaleEditorPreview";
            stale.transform.position = new Vector3(30, 0, 0);
            ps.previewAvatarObject = stale;
            body.GetComponent<Renderer>().sharedMaterial = Track(new Material(Shader.Find("Unlit/Color")) { color = Color.green });

            BuildIcons(new RecordingAssetSaver());

            Assert.That(ps.previewAvatarObject, Is.SameAs(stale));
            Assert.That(stale.transform.position, Is.EqualTo(new Vector3(30, 0, 0)));
            Assert.That(pose.previewImageIsFinal, Is.True);
            Assert.That(pose.previewImage.GetPixels().Count(c => c.g > c.r + 0.2f && c.a > 0.5f), Is.GreaterThan(20),
                "The upload icon must show the processed green body, not the cached red preview.");
        }

        [Test]
        public void DryCapture_IsNotTrustedForUpload_AndFinalImageIsReused()
        {
            AddHumanoid();
            AddVisibleBody(Color.green);
            PosingSystemConverter.TakeScreenshot(ps);
            Assert.That(pose.previewImage, Is.Not.Null);
            Assert.That(pose.previewImageIsFinal, Is.False);
            var dry = Track(pose.previewImage);
            var saver = new RecordingAssetSaver();

            BuildIcons(saver);
            Assert.That(pose.previewImage, Is.Not.SameAs(dry));
            Assert.That(pose.previewImageIsFinal, Is.True);
            var final = pose.previewImage;
            var count = saver.saved.Count;
            BuildIcons(saver);
            Assert.That(pose.previewImage, Is.SameAs(final));
            Assert.That(saver.saved, Has.Count.EqualTo(count));
        }

        [Test]
        public void FinalImageProvenance_SurvivesSerialization_AndMissingImageStillRegenerates()
        {
            AddHumanoid();
            AddVisibleBody(Color.green);
            BuildIcons(new RecordingAssetSaver());
            var serialized = EditorJsonUtility.ToJson(ps);
            pose.previewImageIsFinal = false;
            EditorJsonUtility.FromJsonOverwrite(serialized, ps);
            pose = ps.defines[0].animations[0];
            Assert.That(pose.previewImageIsFinal, Is.True);

            Track(pose.previewImage);
            pose.previewImage = null;
            BuildIcons(new RecordingAssetSaver());
            Assert.That(pose.previewImage, Is.Not.Null);
            Assert.That(pose.previewImageIsFinal, Is.True);
        }

        [Test]
        public void CustomIcon_IsPreservedByForcedCaptureAndMenuUpdate()
        {
            var custom = Track(new Texture2D(4, 4));
            var stale = Track(new Texture2D(4, 4));
            pose.isCustomIcon = true;
            pose.icon = custom;
            pose.previewImage = stale;
            // No Animator: any accidental capture would log a warning or generate an image.
            PosingSystemConverter.TakeScreenshot(ps, true, true);
            BuildIcons(new RecordingAssetSaver());
            Assert.That(pose.previewImage, Is.SameAs(stale));
            Assert.That(menu.controls[0].icon, Is.SameAs(custom));
        }

        [Test]
        public void FinalCachedIcon_DoesNotRequireHumanoidOrRecapture()
        {
            var final = Track(new Texture2D(4, 4));
            pose.previewImage = final;
            pose.previewImageIsFinal = true;
            var saver = new RecordingAssetSaver();
            BuildIcons(saver);
            Assert.That(menu.controls[0].icon, Is.SameAs(final));
            Assert.That(saver.saved, Is.Empty);
        }

        [Test]
        public void EditorOnlyPreprocessClone_DoesNotCaptureRecursively()
        {
            ps.gameObject.tag = "EditorOnly";
            BuildIcons(new RecordingAssetSaver());
            Assert.That(pose.previewImage, Is.Null);
            Assert.That(menu.controls[0].icon, Is.Null);
        }

        [Test]
        public void DisabledIcons_DoNotCapture()
        {
            ps.isIconDisabled = true;
            BuildIcons(new RecordingAssetSaver());
            Assert.That(pose.previewImage, Is.Null);
        }

        [Test]
        public void UnmergedBodyPreview_IsReplacedAfterBuildProcessing()
        {
            AddHumanoid();
            var body = AddVisibleBody(Color.green);
            var unmerged = body.AddComponent<nadena.dev.modular_avatar.core.ModularAvatarMergeArmature>();
            var head = GameObject.CreatePrimitive(PrimitiveType.Cube);
            head.transform.SetParent(ps.GetAvatar().GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head), false);
            head.transform.localPosition = Vector3.zero;
            head.transform.localScale = Vector3.one * 0.25f;
            head.GetComponent<Renderer>().sharedMaterial =
                Track(new Material(Shader.Find("Unlit/Color")) { color = Color.blue });

            PosingSystemConverter.TakeScreenshot(ps);
            var dry = Track(pose.previewImage);
            Assert.That(dry.GetPixels().Count(c => c.b > c.g + 0.2f && c.a > 0.5f), Is.GreaterThan(0));
            Assert.That(dry.GetPixels().Count(c => c.g > c.b + 0.2f && c.a > 0.5f), Is.Zero,
                "The dry preview reproduces the head-only image when the unmerged body is hidden.");

            // The Optimizing pass receives the avatar after MA has consumed MergeArmature.
            Object.DestroyImmediate(unmerged);
            BuildIcons(new RecordingAssetSaver());

            Assert.That(pose.previewImageIsFinal, Is.True);
            Assert.That(pose.previewImage.GetPixels().Count(c => c.g > c.b + 0.2f && c.a > 0.5f), Is.GreaterThan(20),
                "The final uploaded icon must include the processed body.");
        }

        private void BuildIcons(IAssetSaver saver)
        {
            typeof(PosingSystemConverter).GetMethod("CreateBuildMenuIcons", BindingFlags.Static | BindingFlags.NonPublic)
                .Invoke(null, new object[] { ps, saver });
        }

        private T Track<T>(T value) where T : Object
        {
            cleanup.Add(value);
            return value;
        }

        private GameObject AddVisibleBody(Color color)
        {
            var body = GameObject.CreatePrimitive(PrimitiveType.Cube);
            body.name = "Body";
            body.transform.SetParent(ps.GetAvatar().transform, false);
            body.transform.localPosition = new Vector3(0, 0.9f, 0);
            body.transform.localScale = new Vector3(0.5f, 0.8f, 0.3f);
            body.GetComponent<Renderer>().sharedMaterial = Track(new Material(Shader.Find("Unlit/Color")) { color = color });
            return body;
        }

        private void AddHumanoid()
        {
            var root = ps.GetAvatar().gameObject;
            var human = new List<HumanBone>();
            Transform Bone(HumanBodyBones id, Transform parent, Vector3 position)
            {
                var bone = new GameObject(id.ToString()).transform;
                bone.SetParent(parent, false);
                bone.localPosition = position;
                human.Add(new HumanBone
                {
                    humanName = HumanTrait.BoneName[(int)id], boneName = bone.name,
                    limit = new HumanLimit { useDefaultValues = true }
                });
                return bone;
            }
            var hips = Bone(HumanBodyBones.Hips, root.transform, new Vector3(0, 1, 0));
            var spine = Bone(HumanBodyBones.Spine, hips, new Vector3(0, 0.2f, 0));
            var chest = Bone(HumanBodyBones.Chest, spine, new Vector3(0, 0.2f, 0));
            var neck = Bone(HumanBodyBones.Neck, chest, new Vector3(0, 0.2f, 0));
            Bone(HumanBodyBones.Head, neck, new Vector3(0, 0.15f, 0));
            var leftArm = Bone(HumanBodyBones.LeftUpperArm, chest, new Vector3(-0.2f, 0.1f, 0));
            var leftForearm = Bone(HumanBodyBones.LeftLowerArm, leftArm, new Vector3(-0.3f, 0, 0));
            Bone(HumanBodyBones.LeftHand, leftForearm, new Vector3(-0.2f, 0, 0));
            var rightArm = Bone(HumanBodyBones.RightUpperArm, chest, new Vector3(0.2f, 0.1f, 0));
            var rightForearm = Bone(HumanBodyBones.RightLowerArm, rightArm, new Vector3(0.3f, 0, 0));
            Bone(HumanBodyBones.RightHand, rightForearm, new Vector3(0.2f, 0, 0));
            var leftLeg = Bone(HumanBodyBones.LeftUpperLeg, hips, new Vector3(-0.1f, -0.05f, 0));
            var leftCalf = Bone(HumanBodyBones.LeftLowerLeg, leftLeg, new Vector3(0, -0.45f, 0));
            Bone(HumanBodyBones.LeftFoot, leftCalf, new Vector3(0, -0.45f, 0.05f));
            var rightLeg = Bone(HumanBodyBones.RightUpperLeg, hips, new Vector3(0.1f, -0.05f, 0));
            var rightCalf = Bone(HumanBodyBones.RightLowerLeg, rightLeg, new Vector3(0, -0.45f, 0));
            Bone(HumanBodyBones.RightFoot, rightCalf, new Vector3(0, -0.45f, 0.05f));
            var description = new HumanDescription
            {
                human = human.ToArray(),
                skeleton = root.GetComponentsInChildren<Transform>().Select(t => new SkeletonBone
                {
                    name = t.name, position = t.localPosition, rotation = t.localRotation, scale = t.localScale
                }).ToArray(),
                upperArmTwist = 0.5f, lowerArmTwist = 0.5f, upperLegTwist = 0.5f, lowerLegTwist = 0.5f,
                armStretch = 0.05f, legStretch = 0.05f
            };
            var humanoid = Track(AvatarBuilder.BuildHumanAvatar(root, description));
            Assert.That(humanoid.isValid && humanoid.isHuman, Is.True);
            root.AddComponent<Animator>().avatar = humanoid;
            pose.animationClip = Track(new AnimationClip());
        }

        private sealed class RecordingAssetSaver : IAssetSaver
        {
            public readonly List<Object> saved = new();
            public Object CurrentContainer => null;
            public void SaveAsset(Object asset) { saved.Add(asset); }
            public bool IsTemporaryAsset(Object asset) => !EditorUtility.IsPersistent(asset);
            public IEnumerable<Object> GetPersistedAssets() => saved;
            public void Dispose() { }
        }
    }
}
