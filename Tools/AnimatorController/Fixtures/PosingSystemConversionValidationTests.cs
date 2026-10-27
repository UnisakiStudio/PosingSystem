using NUnit.Framework;
using System.Reflection;
using UnityEngine;
using UnityEngine.TestTools;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemConversionValidationTests
    {
        [Test]
        public void ConvertToModularAvatarComponents_OutsideAvatar_RejectsWithoutMutation()
        {
            var gameObject = new GameObject("PosingSystemOutsideAvatar");
            try
            {
                var posingSystem = gameObject.AddComponent<PosingSystem>();

                var before = UnityEditor.EditorJsonUtility.ToJson(posingSystem);
                var error = Assert.Throws<PosingSystemDiagnostics.DiagnosticException>(() =>
                    PosingSystemConverter.ConvertToModularAvatarComponents(posingSystem));
                Assert.That(error.Message, Does.Contain("PS002"));
                Assert.That(UnityEditor.EditorJsonUtility.ToJson(posingSystem), Is.EqualTo(before));
                Assert.That(gameObject.transform.childCount, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void CreateOriginalAnimatorController_OutsideAvatar_RejectsWithoutMutation()
        {
            var gameObject = new GameObject("PosingSystemOutsideAvatar");
            try
            {
                var posingSystem = gameObject.AddComponent<PosingSystem>();

                var before = UnityEditor.EditorJsonUtility.ToJson(posingSystem);
                var error = Assert.Throws<System.InvalidOperationException>(() =>
                    PosingSystemConverter.CreateOriginalAnimatorController(posingSystem));
                Assert.That(error.Message, Does.Contain("導入先アバター"));
                Assert.That(UnityEditor.EditorJsonUtility.ToJson(posingSystem), Is.EqualTo(before));
                Assert.That(gameObject.transform.childCount, Is.Zero);
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        [Test]
        public void HasValidHumanoidAnimator_AnimatorWithoutAvatarIsInvalid()
        {
            var gameObject = new GameObject("PreviewAvatarWithoutAvatarAsset");
            try
            {
                gameObject.AddComponent<Animator>().avatar = null;
                var method = typeof(PosingSystemConverter).GetMethod(
                    "HasValidHumanoidAnimator",
                    BindingFlags.NonPublic | BindingFlags.Static);

                Assert.IsNotNull(method);
                Assert.IsFalse((bool)method.Invoke(null, new object[] { gameObject }),
                    "Animatorコンポーネントが存在してもAvatarアセットが失効していれば再利用しないこと");
            }
            finally
            {
                Object.DestroyImmediate(gameObject);
            }
        }
    }
}
