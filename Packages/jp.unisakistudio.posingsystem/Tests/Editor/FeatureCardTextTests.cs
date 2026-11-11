// Isolated integration tests. Copied into the product test assembly only for review.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;
using jp.unisakistudio.posingsystemeditor;
using Object = UnityEngine.Object;

namespace UnisakiStudio.PosingSystem.Tests
{
    public sealed class FeatureCardTextTests : global::jp.unisakistudio.posingsystemeditor.tests.SharedLanguagePreferenceTestBase
    {
        const BindingFlags All = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance;
        sealed class GuiScope : IDisposable
        {
            readonly object state;
            readonly Event previous;
            public GuiScope()
            {
                state = Activator.CreateInstance(typeof(GUIUtility).Assembly.GetType("UnityEngine.ObjectGUIState"), true);
                previous = Event.current;
                typeof(GUIUtility).GetMethod("BeginContainer", All).Invoke(null, new[] { state });
                Event.current = new Event { type = EventType.Repaint };
                typeof(GUIUtility).GetField("s_SkinMode", All).SetValue(null, 1);
                typeof(GUIUtility).GetField("s_OriginalID", All).SetValue(null, 859123);
                typeof(GUIUtility).GetMethod("ResetGlobalState", All).Invoke(null, null);
            }
            public void Dispose()
            {
                typeof(GUIUtility).GetMethod("EndContainer", All).Invoke(null, null);
                (state as IDisposable)?.Dispose();
                Event.current = previous;
            }
        }

        [Test]
        public void EveryAdvertisedLanguageFeatureCardMeasuresAndDrawsWithoutChangingSettings()
        {
            var go = new GameObject("complex text feature cards");
            try
            {
                var ps = go.AddComponent<jp.unisakistudio.posingsystem.PosingSystem>();
                var serialized = new SerializedObject(ps);
                var type = typeof(PosingSystemSettingsInspector);
                var cardHeight = type.GetMethod("CardHeight", All);
                var drawCard = type.GetMethod("DrawCard", All);
                var groups = (IEnumerable)type.GetField("Groups", All).GetValue(null);
                var settings = new List<PosingSystemSettingsInspector.Setting>();
                foreach (var group in groups)
                    settings.AddRange((PosingSystemSettingsInspector.Setting[])group.GetType().GetField("Settings", All).GetValue(group));
                settings.AddRange(PosingSystemSettingsInspector.AvatarCorrectionSettings);
                settings.AddRange(PosingSystemSettingsInspector.AvatarIntegrationSettings);
                using var gui = new GuiScope();
                type.GetMethod("EnsureStyles", All).Invoke(null, null);
                var titleStyle = (GUIStyle)type.GetField("titleStyle", All).GetValue(null);
                var bodyStyle = (GUIStyle)type.GetField("descriptionStyle", All).GetValue(null);
                var samples = 0;
                foreach (var language in PosingSystemLocalization.Codes)
                {
                    PosingSystemLocalization.Select(language);
                    serialized.Update();
                    var original = EditorJsonUtility.ToJson(ps);
                    using (PosingSystemLocalization.ForInspector(ps))
                    using (PosingSystemTextGUI.BeginGUI(859123))
                    {
                        foreach (var setting in settings)
                        {
                            var description = PosingSystemSettingsInspector.DescriptionFor(ps, setting);
                            foreach (var width in new[] { 180f, 240f, 360f })
                            {
                                var available = Mathf.Max(1, width - 66 - 5);
                                var title = PosingSystemLocalization.Text(setting.Title);
                                var titleHeight = Mathf.Max(16, PosingSystemTextGUI.MeasureHeight(title, titleStyle, available));
                                var bodyHeight = PosingSystemTextGUI.MeasureHeight(description, bodyStyle, available);
                                var height = (float)cardHeight.Invoke(null, new object[] { setting, width, description });
                                Assert.That(height, Is.EqualTo(4 + Mathf.Max(56, titleHeight + 2 + bodyHeight) + 24 + 4).Within(.01f), language + " " + setting.Path);
                                GUI.changed = false;
                                drawCard.Invoke(null, new object[] { new Rect(0, 0, width, height), serialized.FindProperty(setting.Path), setting, false, description, null, null, setting.Bits, ps });
                                Assert.That(GUI.changed, Is.False, language + " repaint changes GUI.changed");
                                Assert.That(PosingSystemTextRenderer.Failure, Is.Null, language + " " + setting.Path);
                                if (PosingSystemTextRenderer.Requires(title)) AssertNoMissingGlyphs(title, titleStyle, available);
                                if (PosingSystemTextRenderer.Requires(description)) AssertNoMissingGlyphs(description, bodyStyle, available);
                                samples++;
                            }
                        }
                    }
                    Assert.That(EditorJsonUtility.ToJson(ps), Is.EqualTo(original), language + " changed serialized settings");
                    Assert.That(PosingSystemLocalization.Language, Is.EqualTo(language));
                }
                Assert.That(samples, Is.EqualTo(30 * 19 * 3));
                TestContext.WriteLine("All-language card width samples: " + samples);
            }
            finally { PosingSystemTextRenderer.Reset(); Object.DestroyImmediate(go); }
        }

        static void AssertNoMissingGlyphs(string text, GUIStyle style, float width)
        {
            var backend = typeof(PosingSystemTextRenderer).GetNestedType("Backend", All);
            var entry = backend.GetMethod("Get", All).Invoke(null, new object[] { text, style, (float?)width });
            var block = entry.GetType().GetField("Block", All).GetValue(entry);
            foreach (var issue in GlyphClusterAudit.Inspect(block))
                Assert.That(issue.AllowedControl, Is.True, issue + " text=" + text);
        }
    }
}
