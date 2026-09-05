using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemEmptyStandingRegressionTests
    {
        private GameObject avatarObject;

        [TearDown]
        public void TearDown()
        {
            if (avatarObject != null)
            {
                UnityEngine.Object.DestroyImmediate(avatarObject);
            }
        }

        [Test]
        public void EmptyStandingList_DoesNotAbortParameterReset()
        {
            avatarObject = new GameObject("EmptyStandingListAvatar");
            var descriptorType = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Single(type => type.FullName == "VRC.SDK3.Avatars.Components.VRCAvatarDescriptor");
            var avatar = avatarObject.AddComponent(descriptorType);
            var posingObject = new GameObject("PosingSystem");
            posingObject.transform.SetParent(avatarObject.transform);
            var posingSystem = posingObject.AddComponent<PosingSystem>();
            posingObject.AddComponent<ModularAvatarParameters>();
            posingSystem.defines = new List<PosingSystem.LayerDefine>
            {
                new PosingSystem.LayerDefine(
                    "立ち姿勢", "", "USSPS_SitStand", 0, "",
                    new List<PosingSystem.AnimationDefine>())
            };

            var maxMethod = typeof(PosingSystemConverter).GetMethod(
                "GetMaxSyncedParameterValue",
                BindingFlags.Static | BindingFlags.Public);
            Assert.That(maxMethod, Is.Not.Null);
            Assert.That((int)maxMethod.Invoke(null, new object[] { avatar }), Is.Zero);
            Assert.DoesNotThrow(() =>
                PosingSystemConverter.ResetParametersWithSyncedParameter(posingSystem));
        }
    }
}
