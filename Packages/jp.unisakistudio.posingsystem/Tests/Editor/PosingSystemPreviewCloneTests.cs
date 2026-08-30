using System.Reflection;
using System.Linq;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemPreviewCloneTests
    {
        [TearDown]
        public void TearDown()
        {
            PosingSystemEditor.CleanupAllPreviewAvatars();

            foreach (var gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
            {
                if (gameObject != null &&
                    (gameObject.name == "PreviewCloneTestAvatar" ||
                     gameObject.name.StartsWith("_PosingSystem_ProcessedPreview_")))
                {
                    Object.DestroyImmediate(gameObject);
                }
            }
        }

        [Test]
        public void BuildPipelineClone_RemainsInValidScene()
        {
            var sourceAvatar = new GameObject("PreviewCloneTestAvatar");
            var posingObject = new GameObject("PosingSystem");
            posingObject.transform.SetParent(sourceAvatar.transform);
            var posingSystem = posingObject.AddComponent<PosingSystem>();

            var method = typeof(PosingSystemConverter).GetMethod(
                "CreateBuildPipelineAvatarClone",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(method);

            var clone = (GameObject)method.Invoke(null, new object[] { sourceAvatar, posingSystem });

            Assert.IsTrue(clone.scene.IsValid(), "NDMFが保持するSceneは有効であること");
            Assert.IsTrue(clone.scene.isLoaded);
            Assert.IsNull(clone.transform.parent);
            Assert.IsTrue(clone.activeSelf);
            Assert.AreEqual(HideFlags.HideInHierarchy, clone.hideFlags);
            Assert.AreEqual(
                "EditorOnly",
                clone.GetComponentInChildren<PosingSystem>(true).gameObject.tag);
        }

        [Test]
        public void PreviewRoot_IsRecoveredAfterStaticCacheReset()
        {
            PosingSystemEditor.CleanupAllPreviewAvatars();
            var first = PosingSystemEditor.GetPreviewAvatarRoot();
            Assert.IsFalse(first.gameObject.scene.IsValid());

            var field = typeof(PosingSystemEditor).GetField(
                "previewAvatarRoot",
                BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(field);
            field.SetValue(null, null);

            var recovered = PosingSystemEditor.GetPreviewAvatarRoot();

            Assert.AreSame(first.gameObject, recovered.gameObject);
            Assert.AreEqual(
                1,
                Resources.FindObjectsOfTypeAll<GameObject>().Count(
                    gameObject => gameObject != null && gameObject.name == "PreviewAvatarRoot"));
        }
    }
}
