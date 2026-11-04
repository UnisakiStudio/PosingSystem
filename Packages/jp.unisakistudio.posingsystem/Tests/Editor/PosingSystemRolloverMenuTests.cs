using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemRolloverMenuTests
    {
        private GameObject root;
        private PosingSystem ps;
        private Transform settings;

        [SetUp] public void SetUp()
        {
            root = new GameObject("Rollover menu test");
            ps = root.AddComponent<PosingSystem>();
            root.AddComponent<ModularAvatarParameters>();
            ps.features = new PosingSystemFeatureSettings { mirror = false, footHeight = false, breathing = false, fingerGestureControl = false, rollover = true };
            settings = new GameObject("Settings").transform; settings.SetParent(root.transform, false);
            var mirror = Item(settings, "Mirror"); mirror.Control.parameter.name = "USSPS_Mirror";
        }
        [TearDown] public void TearDown() { Undo.ClearAll(); Object.DestroyImmediate(root); }

        private static ModularAvatarMenuItem Item(Transform parent, string name)
        {
            var obj = new GameObject(name); obj.transform.SetParent(parent, false);
            var item = obj.AddComponent<ModularAvatarMenuItem>();
            item.Control.type = VRCExpressionsMenu.Control.ControlType.Toggle;
            item.Control.parameter = new VRCExpressionsMenu.Control.Parameter();
            item.Control.name = item.label = name;
            return item;
        }

        [TestCase(false)] [TestCase(true)]
        public void RolloverMenu_NewToggle_IsDirectAndIdempotent(bool undo)
        {
            PosingSystemFeatureUtility.Synchronize(ps, undo);
            PosingSystemFeatureUtility.Synchronize(ps, undo);
            AssertDirect();
            ps.features.rollover = false;
            PosingSystemFeatureUtility.Synchronize(ps, undo);
            var toggle = FindToggle();
            Assert.That(toggle.gameObject.activeSelf, Is.False);
            Assert.That(toggle.tag, Is.EqualTo("EditorOnly"));
            ps.features.rollover = true;
            PosingSystemFeatureUtility.Synchronize(ps, undo);
            AssertDirect();
        }

        [TestCase(false)] [TestCase(true)]
        public void RolloverMenu_MigratesOldSubmenu_AndPreservesCustomChildren(bool empty)
        {
            var old = Item(settings, "頭の傾きで寝返り（試験機能）");
            old.MenuSource = SubmenuSource.Children;
            old.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
            if (!empty) Item(old.transform, "寝返りを有効にする").Control.parameter.name = PosingSystemFeatureUtility.RollActive;
            var custom = Item(old.transform, "User control"); custom.Control.parameter.name = "UserParameter";
            PosingSystemFeatureUtility.Synchronize(ps);
            AssertDirect();
            Assert.That(old == null, Is.True);
            Assert.That(custom.transform.parent, Is.EqualTo(settings));
            Assert.That(custom.Control.parameter.name, Is.EqualTo("UserParameter"));
        }

        [Test] public void RolloverMenu_MigrationUndoRedo_RestoresHierarchyAndToggle()
        {
            var old = Item(settings, "頭の傾きで寝返り（試験機能）");
            old.MenuSource = SubmenuSource.Children; old.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
            var toggle = Item(old.transform, "寝返りを有効にする"); toggle.Control.parameter.name = PosingSystemFeatureUtility.RollActive;
            Undo.IncrementCurrentGroup(); var group = Undo.GetCurrentGroup();
            PosingSystemFeatureUtility.Synchronize(ps, true);
            Undo.FlushUndoRecordObjects(); Undo.CollapseUndoOperations(group); Undo.IncrementCurrentGroup();
            AssertDirect();
            Undo.PerformUndo();
            Assert.That(FindToggle().transform.parent.name, Is.EqualTo("頭の傾きで寝返り（試験機能）"));
            Undo.PerformRedo();
            AssertDirect();
        }

        [Test] public void RolloverMenu_CreationUndoRedo_RestoresConfiguredControl()
        {
            Undo.IncrementCurrentGroup(); var group = Undo.GetCurrentGroup();
            PosingSystemFeatureUtility.Synchronize(ps, true);
            Undo.FlushUndoRecordObjects(); Undo.CollapseUndoOperations(group); Undo.IncrementCurrentGroup();
            AssertDirect();
            Undo.PerformUndo();
            Assert.That(root.GetComponentsInChildren<ModularAvatarMenuItem>(true)
                .Any(m => m.Control.parameter?.name == PosingSystemFeatureUtility.RollActive), Is.False);
            Undo.PerformRedo();
            AssertDirect();
        }

        private ModularAvatarMenuItem FindToggle() => root.GetComponentsInChildren<ModularAvatarMenuItem>(true)
            .Single(m => m.Control.parameter?.name == PosingSystemFeatureUtility.RollActive);
        private void AssertDirect()
        {
            var toggle = FindToggle();
            Assert.That(toggle.transform.parent, Is.EqualTo(settings));
            Assert.That(toggle.Control.type, Is.EqualTo(VRCExpressionsMenu.Control.ControlType.Toggle));
            Assert.That(toggle.Control.name, Is.EqualTo("寝返り"));
            Assert.That(toggle.name, Is.EqualTo("寝返り"));
            Assert.That(toggle.label, Is.EqualTo("寝返り"));
            Assert.That(toggle.Control.value, Is.EqualTo(1));
            Assert.That(toggle.automaticValue, Is.False);
            Assert.That(toggle.gameObject.activeSelf, Is.True);
            Assert.That(root.GetComponentsInChildren<ModularAvatarMenuItem>(true)
                .Any(m => m.Control.type == VRCExpressionsMenu.Control.ControlType.SubMenu && m.name.Contains("寝返り")), Is.False);
        }
    }
}
