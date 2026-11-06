using System.Collections.Generic;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemMenuLocalizationTests
    {
        [Test]
        public void MenuClonePreservesSourceParametersAndCycles()
        {
            var source = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            var copies = new Dictionary<VRCExpressionsMenu, VRCExpressionsMenu>();
            source.controls.Add(new VRCExpressionsMenu.Control
            {
                name = "寝返り", subMenu = source,
                parameter = new VRCExpressionsMenu.Control.Parameter { name = "USSPS_Rollover" }, value = 1,
                labels = new[] { new VRCExpressionsMenu.Control.Label { name = "足固定" } }
            });
            try
            {
                var copy = PosingSystemMenuLocalization.CloneMenu(source, "en", copies);
                Assert.That(copy, Is.Not.SameAs(source));
                Assert.That(copy.controls[0].name, Is.EqualTo("Roll Over"));
                Assert.That(copy.controls[0].subMenu, Is.SameAs(copy));
                Assert.That(copy.controls[0].parameter.name, Is.EqualTo("USSPS_Rollover"));
                Assert.That(copy.controls[0].value, Is.EqualTo(1));
                Assert.That(copy.controls[0].labels[0].name, Is.EqualTo("Lock Feet"));
                Assert.That(source.controls[0].name, Is.EqualTo("寝返り"));
                Assert.That(source.controls[0].labels[0].name, Is.EqualTo("足固定"));
                Assert.That(source.controls[0].subMenu, Is.SameAs(source));
            }
            finally
            {
                foreach (var copy in copies.Values) Object.DestroyImmediate(copy);
                Object.DestroyImmediate(source);
            }
        }

        [Test]
        public void BuildItemsTranslateLabelsButKeepHierarchyNamesAndNestedOwnership()
        {
            var root = new GameObject("test");
            try
            {
                var ps = root.AddComponent<PosingSystem>(); ps.menuLanguage = "en";
                var item = root.AddComponent<ModularAvatarMenuItem>();
                item.Control.name = item.label = "寝返り";
                var child = new GameObject("nested"); child.transform.parent = root.transform;
                var nested = child.AddComponent<PosingSystem>(); nested.menuLanguage = "ja";
                var nestedItem = child.AddComponent<ModularAvatarMenuItem>(); nestedItem.Control.name = "足固定";
                PosingSystemMenuLocalization.Apply(ps);
                Assert.That(root.name, Is.EqualTo("test"));
                Assert.That(item.Control.name, Is.EqualTo("Roll Over"));
                Assert.That(item.label, Is.EqualTo("Roll Over"));
                Assert.That(nestedItem.Control.name, Is.EqualTo("足固定"));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void EmptyMaLabelUsesTranslatedGameObjectName(bool active)
        {
            var root = new GameObject("寝返り");
            try
            {
                var ps = root.AddComponent<PosingSystem>(); ps.menuLanguage = "en";
                var child = new GameObject("寝返り"); child.transform.parent = root.transform;
                var item = child.AddComponent<ModularAvatarMenuItem>(); item.label = "";
                root.SetActive(active);
                PosingSystemMenuLocalization.Apply(ps);
                Assert.That(item.label, Is.EqualTo("Roll Over"));
                Assert.That(root.name, Is.EqualTo("寝返り"));
            }
            finally { Object.DestroyImmediate(root); }
        }

        [Test]
        public void InstallerTargetsFollowOwnedMenuCopies()
        {
            var root = new GameObject("test");
            var menu = ScriptableObject.CreateInstance<VRCExpressionsMenu>();
            VRCExpressionsMenu copy = null;
            try
            {
                var ps = root.AddComponent<PosingSystem>(); ps.menuLanguage = "en";
                var first = root.AddComponent<ModularAvatarMenuInstaller>(); first.menuToAppend = menu;
                var child = new GameObject("child"); child.transform.parent = root.transform;
                var second = child.AddComponent<ModularAvatarMenuInstaller>(); second.installTargetMenu = menu;
                PosingSystemMenuLocalization.Apply(ps);
                copy = first.menuToAppend;
                Assert.That(copy, Is.Not.SameAs(menu));
                Assert.That(second.installTargetMenu, Is.SameAs(copy));
            }
            finally
            {
                Object.DestroyImmediate(root);
                if (copy != null) Object.DestroyImmediate(copy);
                Object.DestroyImmediate(menu);
            }
        }
    }
}
