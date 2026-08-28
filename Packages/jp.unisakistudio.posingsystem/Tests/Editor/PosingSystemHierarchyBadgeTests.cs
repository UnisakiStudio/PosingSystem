using System;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemHierarchyBadgeTests
    {
        private Type badgeType;
        private GameObject gameObject;
        private PosingSystem posingSystem;

        [SetUp]
        public void SetUp()
        {
            badgeType = typeof(PosingSystemEditor).Assembly.GetType(
                "jp.unisakistudio.posingsystemeditor.PosingSystemHierarchyBadge");
            Assert.IsNotNull(badgeType);
            Invoke("ResetForTests");

            gameObject = new GameObject("PosingSystemHierarchyBadgeTests");
            posingSystem = gameObject.AddComponent<PosingSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            Invoke("ResetForTests");
            UnityEngine.Object.DestroyImmediate(gameObject);
        }

        [Test]
        public void UnchangedObject_ReusesCachedStatus()
        {
            Invoke("GetStatus", posingSystem);
            Invoke("GetStatus", posingSystem);

            Assert.AreEqual(1, GetProperty<int>("StatusEvaluationCount"));
        }

        [Test]
        public void DirtyObject_ReevaluatesStatus()
        {
            Invoke("GetStatus", posingSystem);
            EditorUtility.SetDirty(posingSystem);
            Invoke("GetStatus", posingSystem);

            Assert.AreEqual(2, GetProperty<int>("StatusEvaluationCount"));
        }

        [Test]
        public void ExplicitReset_ReevaluatesStatus()
        {
            Invoke("GetStatus", posingSystem);
            posingSystem.previousErrorCheckTime = DateTime.MinValue;
            Invoke("GetStatus", posingSystem);

            Assert.AreEqual(2, GetProperty<int>("StatusEvaluationCount"));
        }

        private object Invoke(string methodName, params object[] arguments)
        {
            var method = badgeType.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(method, methodName);
            return method.Invoke(null, arguments);
        }

        private T GetProperty<T>(string propertyName)
        {
            var property = badgeType.GetProperty(propertyName, BindingFlags.Static | BindingFlags.NonPublic);
            Assert.IsNotNull(property, propertyName);
            return (T)property.GetValue(null);
        }
    }
}
