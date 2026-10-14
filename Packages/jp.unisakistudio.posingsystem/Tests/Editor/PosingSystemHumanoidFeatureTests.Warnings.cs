using System.Reflection;
using jp.unisakistudio.posingsystem;
using NUnit.Framework;
using UnityEditor;

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
    }
}
