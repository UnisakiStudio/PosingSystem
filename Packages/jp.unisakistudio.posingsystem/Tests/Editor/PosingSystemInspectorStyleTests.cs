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
            using var styles = new HeadlessEditorStyles();
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

        // A batch runner has no Inspector OnGUI to initialize EditorStyles. Supply the same
        // built-in styles (not substitutes) and restore the global cache after the assertion.
        sealed class HeadlessEditorStyles : System.IDisposable
        {
            readonly FieldInfo current = typeof(EditorStyles).GetField("s_Current", BindingFlags.Static | BindingFlags.NonPublic);
            readonly object previous;
            public HeadlessEditorStyles()
            {
                previous = current.GetValue(null);
                if (previous != null) return;
                var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
                var styles = new EditorStyles();
                foreach (var pair in new[] { ("m_BoldLabel", "BoldLabel"), ("m_WordWrappedMiniLabel", "WordWrappedMiniLabel"),
                    ("m_MiniLabel", "MiniLabel"), ("m_Label", "ControlLabel") })
                {
                    var style = skin.FindStyle(pair.Item2);
                    Assert.That(style, Is.Not.Null, pair.Item2);
                    typeof(EditorStyles).GetField(pair.Item1, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(styles, style);
                }
                current.SetValue(null, styles);
            }
            public void Dispose() => current.SetValue(null, previous);
        }
    }
}
