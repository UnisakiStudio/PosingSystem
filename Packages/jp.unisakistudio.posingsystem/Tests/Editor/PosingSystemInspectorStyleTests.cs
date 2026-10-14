using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public class PosingSystemInspectorStyleTests
    {
        [Test]
        public void PrefabOverrideFont_DoesNotChangeSettingsTypography()
        {
            const BindingFlags flags = BindingFlags.Static | BindingFlags.NonPublic;
            var inspector = typeof(PosingSystemEditor).Assembly.GetType(
                "jp.unisakistudio.posingsystemeditor.PosingSystemSettingsInspector", true);
            inspector.GetMethod("EnsureStyles", flags).Invoke(null, null);
            var getBold = typeof(EditorGUIUtility).GetMethod("GetBoldDefaultFont", flags);
            var setBold = typeof(EditorGUIUtility).GetMethod("SetBoldDefaultFont", flags);
            Assert.That(getBold, Is.Not.Null);
            Assert.That(setBold, Is.Not.Null);
            var previous = (bool)getBold.Invoke(null, null);
            try
            {
                foreach (var name in new[] { "titleStyle", "descriptionStyle", "badgeStyle", "summaryStyle", "groupStyle" })
                {
                    var style = (GUIStyle)inspector.GetField(name, flags).GetValue(null);
                    Assert.That(style.font, Is.Not.Null, name + " must not use Unity's changing default font");
                    var content = new GUIContent("首の向きの違いによる姿勢崩れを抑制 16 bit");
                    setBold.Invoke(null, new object[] { false });
                    var size = style.CalcSize(content);
                    var height = style.CalcHeight(content, 150);
                    setBold.Invoke(null, new object[] { true });
                    Assert.That(style.CalcSize(content), Is.EqualTo(size), name);
                    Assert.That(style.CalcHeight(content, 150), Is.EqualTo(height), name);
                }
            }
            finally { setBold.Invoke(null, new object[] { previous }); }
        }
    }
}
