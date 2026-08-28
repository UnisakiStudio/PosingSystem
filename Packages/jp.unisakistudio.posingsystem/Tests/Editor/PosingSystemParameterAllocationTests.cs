using System;
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemParameterAllocationTests
    {
        private GameObject avatarObject;

        [TearDown]
        public void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(avatarObject);
        }

        [Test]
        public void ResetTypeAndSyncedParameter_AllowsDuplicateValuesOnOtherSystems()
        {
            avatarObject = new GameObject("ParameterAllocationTestAvatar");
            var avatarDescriptorType = AppDomain.CurrentDomain.GetAssemblies()
                .Select(assembly => assembly.GetType("VRC.SDK3.Avatars.Components.VRCAvatarDescriptor"))
                .First(type => type != null);
            avatarObject.AddComponent(avatarDescriptorType);

            var target = CreatePosingSystem("Target", 0, 0, 2);
            CreatePosingSystem("Other1", 2, 4, 1);
            CreatePosingSystem("Other2", 2, 4, 1);

            Assert.DoesNotThrow(() => PosingSystemConverter.ResetTypeAndSyncedParameter(target));

            var animations = target.defines.Single().animations;
            CollectionAssert.AreEqual(new[] { 3, 4 }, animations.Select(x => x.typeParameterValue));
            CollectionAssert.AreEqual(new[] { 5, 6 }, animations.Select(x => x.syncdParameterValue));
        }

        private PosingSystem CreatePosingSystem(string name, int typeValue, int syncedValue, int animationCount)
        {
            var child = new GameObject(name);
            child.transform.SetParent(avatarObject.transform);
            var posingSystem = child.AddComponent<PosingSystem>();
            var animations = new List<PosingSystem.AnimationDefine>();
            for (var i = 0; i < animationCount; i++)
            {
                animations.Add(new PosingSystem.AnimationDefine
                {
                    displayName = name + i,
                    typeParameterValue = typeValue,
                    syncdParameterValue = syncedValue,
                });
            }

            posingSystem.defines = new List<PosingSystem.LayerDefine>
            {
                new PosingSystem.LayerDefine("", "", "TestParam", 0, "", animations),
            };
            return posingSystem;
        }
    }
}
