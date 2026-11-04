using System.Reflection;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using nadena.dev.modular_avatar.core;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public partial class PosingSystemHumanoidFeatureTests
    {
        [TestCase(false)]
        [TestCase(true)]
        public void WarningAudit_PrebuildClearsStaleness_RegardlessOfDisplayPreference(bool ignore)
        {
            var ps = DefaultIdleSettings(false, false);
            if (ignore) ps.ignoredWarnings = PosingSystem.WarningType.All;
            var badge = typeof(PosingSystemHierarchyBadge).GetMethod("GetStatus", BindingFlags.NonPublic | BindingFlags.Static);
            var prebuild = typeof(PosingSystemEditor).Assembly.GetType("jp.unisakistudio.posingsystemeditor.PosingSystemPrebuildInspector")
                .GetMethod("GetState", BindingFlags.NonPublic | BindingFlags.Static);
            PosingSystemEditor.Prebuild(ps);
            Assert.That(prebuild.Invoke(null, new object[] { ps }).ToString(), Is.EqualTo("Ready"));
            Assert.That(((bool, bool))badge.Invoke(null, new object[] { ps }), Is.EqualTo((false, false)));
            using (var serialized = new SerializedObject(ps))
            {
                serialized.FindProperty("isIconSmall").boolValue = !ps.isIconSmall;
                PosingSystemEditorChangeUtility.Commit(serialized);
            }
            Assert.That(prebuild.Invoke(null, new object[] { ps }).ToString(), Is.EqualTo("Outdated"));
            Assert.That(((bool, bool))badge.Invoke(null, new object[] { ps }), Is.EqualTo((!ignore, false)));
            PosingSystemEditor.Prebuild(ps);
            Assert.That(prebuild.Invoke(null, new object[] { ps }).ToString(), Is.EqualTo("Ready"));
            Assert.That(((bool, bool))badge.Invoke(null, new object[] { ps }), Is.EqualTo((false, false)));
        }

        [Test]
        public void WarningAudit_PoseEditAndFeatureSync_PreserveBuildHistoryAndHierarchyWarning()
        {
            var ps = DefaultIdleSettings(false, false);
            if (ps.GetComponent<ModularAvatarParameters>() == null) ps.gameObject.AddComponent<ModularAvatarParameters>();
            var clip = new AnimationClip(); assets.Add(clip);
            ps.defines.Add(new PosingSystem.LayerDefine("", "Test", "Test", 0, "Test", new List<PosingSystem.AnimationDefine>
                { new PosingSystem.AnimationDefine(clip, "Original") }));
            PosingSystemEditor.Prebuild(ps);
            var snapshot = ps.data;
            var assembly = typeof(PosingSystemEditor).Assembly;
            var state = assembly.GetType("jp.unisakistudio.posingsystemeditor.PosingSystemPrebuildInspector")
                .GetMethod("GetState", BindingFlags.NonPublic | BindingFlags.Static);
            var badge = typeof(PosingSystemHierarchyBadge).GetMethod("GetDisplayStatus", BindingFlags.NonPublic | BindingFlags.Static);
            Undo.IncrementCurrentGroup();
            using (var serialized = new SerializedObject(ps))
            {
                serialized.FindProperty("defines").GetArrayElementAtIndex(0).FindPropertyRelative("animations")
                    .GetArrayElementAtIndex(0).FindPropertyRelative("displayName").stringValue = "Renamed";
                PosingSystemEditorChangeUtility.Apply(serialized);
                // Same sequence as DrawUserInspector after editing a pose row.
                PosingSystemFeatureUtility.Synchronize(ps, true);
            }
            Undo.FlushUndoRecordObjects();
            Assert.That(ps.data, Is.EqualTo(snapshot), "Editing must retain the successful build snapshot.");
            Assert.That(state.Invoke(null, new object[] { ps }).ToString(), Is.EqualTo("Outdated"));
            Assert.That(((bool, bool))badge.Invoke(null, new object[] { ps }), Is.EqualTo((true, false)));
            Assert.That(PosingSystemConverter.GetVisibleWarnings(ps) & PosingSystem.WarningType.PrebuildNotRun, Is.EqualTo(PosingSystem.WarningType.None));
            Undo.PerformUndo();
            Assert.That(state.Invoke(null, new object[] { ps }).ToString(), Is.EqualTo("Ready"));
            Undo.PerformRedo();
            Assert.That(state.Invoke(null, new object[] { ps }).ToString(), Is.EqualTo("Outdated"));
            PosingSystemEditor.Prebuild(ps);
            Assert.That(state.Invoke(null, new object[] { ps }).ToString(), Is.EqualTo("Ready"));
            Assert.That(((bool, bool))badge.Invoke(null, new object[] { ps }), Is.EqualTo((false, false)));
        }
    }
}
