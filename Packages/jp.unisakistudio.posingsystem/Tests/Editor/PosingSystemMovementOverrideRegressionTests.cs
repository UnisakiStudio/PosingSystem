using System.Collections.Generic;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemMovementOverrideRegressionTests
    {
        private readonly List<UnityEngine.Object> objectsToDestroy =
            new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var value in objectsToDestroy)
            {
                if (value != null)
                {
                    UnityEngine.Object.DestroyImmediate(value);
                }
            }
            objectsToDestroy.Clear();
        }

        [Test]
        public void MovementCategory_RejectsSingleIdleClipButAcceptsLocomotionBlendTree()
        {
            var method = typeof(PosingSystemConverter).GetMethod(
                "IsValidOverrideMotion",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            Assert.That(method, Is.Not.Null);

            var idle = Track(new AnimationClip { name = "Stand_still" });
            var locomotion = Track(new BlendTree { name = "Locomotion" });
            var standWalkRun =
                PosingSystem.OverrideAnimationDefine.AnimationStateType.StandWalkRun;
            var crouch = PosingSystem.OverrideAnimationDefine.AnimationStateType.Crouch;
            var prone = PosingSystem.OverrideAnimationDefine.AnimationStateType.Prone;
            var jump = PosingSystem.OverrideAnimationDefine.AnimationStateType.Jump;

            Assert.That((bool)method.Invoke(null, new object[] { standWalkRun, idle }), Is.False);
            Assert.That((bool)method.Invoke(null, new object[] { crouch, idle }), Is.False);
            Assert.That((bool)method.Invoke(null, new object[] { prone, idle }), Is.False);
            Assert.That((bool)method.Invoke(null, new object[] { standWalkRun, locomotion }), Is.True);
            Assert.That((bool)method.Invoke(null, new object[] { jump, idle }), Is.True);
            Assert.That((bool)method.Invoke(null, new object[] { jump, null }), Is.False);
        }

        private T Track<T>(T value) where T : UnityEngine.Object
        {
            objectsToDestroy.Add(value);
            return value;
        }
    }
}
