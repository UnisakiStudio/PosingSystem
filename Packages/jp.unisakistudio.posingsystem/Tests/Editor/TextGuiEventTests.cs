// Stage-only NUnit source. Parent copies this into the existing Editor test assembly.
#if UNITY_EDITOR && UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    public sealed class TextGuiEventTests
    {
        private const BindingFlags All = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;
        private const int Owner = 1796231;
        private static readonly MethodInfo EndEditing = typeof(EditorGUI).GetMethod("EndEditingActiveTextField", All, null, Type.EmptyTypes, null);
        private static readonly string[] Scripts = { "العربية", "עברית", "हिन्दी", "বাংলা", "தமிழ்", "తెలుగు", "ภาษาไทย" };
        private enum Control { Button, Toggle, ToggleLeft, EditorToggle, Foldout, Slider, TextField }
        private sealed class Value { internal bool Flag; internal float Number = .25f; internal string Text = "original_pose_42"; }
        private sealed class Trace
        {
            internal int Before, After, Hot, Keyboard;
            internal bool Changed, Flag;
            internal float Number;
            internal string Text;
            internal string EditorBefore, EditorAfter;
            internal EventType Event;
            internal Rect Rect;
        }

        // A genuine owned EditorWindow supplies GUIView.current/HostView.current.
        // BeginContainer alone cannot exercise cursor rectangles or recycled editors.
        private sealed class ProbeWindow : EditorWindow
        {
            internal Action Draw;
            internal EventType Requested;
            internal bool Received, First, Pending, InRequestedEvent, EndTextEditing, CleanupAfterDraw;
            internal Exception Failure;
            private void OnGUI()
            {
                if (Draw == null) return;
                bool requested = Pending && Event.current.rawType == Requested;
                if (requested) { Received = true; Pending = false; }
                InRequestedEvent = requested && !CleanupAfterDraw;
                try
                {
                    var viewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GUIView");
                    var hostType = typeof(EditorWindow).Assembly.GetType("UnityEditor.HostView");
                    var current = viewType?.GetProperty("current", All)?.GetValue(null);
                    Assert.NotNull(current, "Synthetic event must enter a real Editor GUIView");
                    Assert.AreSame(this, hostType?.GetProperty("actualView", All)?.GetValue(current),
                        "Synthetic event must run inside the owned HostView, not a bare GUI container");
                    if (First && requested)
                    {
                        if (EndTextEditing)
                        {
                            Assert.NotNull(EndEditing, "Installed Unity internal text-editor cleanup contract");
                            EndEditing.Invoke(null, null);
                        }
                        GUIUtility.hotControl = 0; GUIUtility.keyboardControl = 0;
                        EditorGUIUtility.editingTextField = false;
                    }
                    Draw();
                    if (requested && CleanupAfterDraw)
                    {
                        EndEditing.Invoke(null, null);
                        GUIUtility.hotControl = 0; GUIUtility.keyboardControl = 0;
                        EditorGUIUtility.editingTextField = false;
                    }
                }
                catch (Exception error) { if (Failure == null) Failure = error; }
                finally { InRequestedEvent = false; }
            }
        }

        private sealed class Container : IDisposable
        {
            private readonly ProbeWindow window;
            internal bool InRequestedEvent => window.InRequestedEvent;
            internal Container(bool endTextEditing = true)
            {
                window = ScriptableObject.CreateInstance<ProbeWindow>();
                window.EndTextEditing = endTextEditing;
                window.titleContent = new GUIContent("Posing Text GUI Test");
                window.position = new Rect(200,200,480,640);
                window.ShowUtility(); window.Focus();
            }

            internal void Frame(Event input, Action draw, bool first = false, float width = 480, bool cleanup = false)
            {
                if (window.Failure != null) ExceptionDispatchInfo.Capture(window.Failure).Throw();
                // Keep the complete tree installed between SendEvent calls. Unity
                // may generate its own Layout/Repaint in that interval; returning
                // an empty tree would replace the cache with zero controls.
                window.Draw = draw;
                if (Math.Abs(window.position.width - width) > .1f)
                    window.position = new Rect(window.position.x,window.position.y,width,640);
                window.Requested = input.type; window.Pending = true;
                window.Received = false; window.First = first;
                window.CleanupAfterDraw = cleanup;
                try
                {
                    window.SendEvent(input);
                    if (window.Failure != null) ExceptionDispatchInfo.Capture(window.Failure).Throw();
                    Assert.IsTrue(window.Received, "Owned EditorWindow did not synchronously receive " + window.Requested);
                }
                finally { window.Pending = false; }
            }
            internal void EndSequence()
            {
                if (!window.EndTextEditing || window.Draw == null) return;
                Frame(new Event { type = EventType.ExecuteCommand, commandName = "USSPS_TEST_END_TEXT_EDITING" },
                    window.Draw, false, window.position.width, true);
            }
            public void Dispose()
            {
                try { EndSequence(); }
                finally
                {
                    window.Draw = null; window.Close();
                    if (window != null) UnityEngine.Object.DestroyImmediate(window);
                }
            }
        }

        [SetUp] public void ResetBackend()
            => typeof(PosingSystemTextRenderer).GetMethod("Reset", All)?.Invoke(null, null);

        private static void Draw(Control control, bool adapter, GUIContent label, Value value)
        {
            switch (control)
            {
                case Control.Button:
                    value.Flag = adapter ? PosingSystemTextGUI.LayoutButton(label, GUILayout.Width(240), GUILayout.Height(30)) :
                        GUILayout.Button(label, GUILayout.Width(240), GUILayout.Height(30)); break;
                case Control.Toggle:
                    value.Flag = adapter ? PosingSystemTextGUI.LayoutToggle(value.Flag, label, GUILayout.Width(240), GUILayout.Height(30)) :
                        GUILayout.Toggle(value.Flag, label, GUILayout.Width(240), GUILayout.Height(30)); break;
                case Control.ToggleLeft:
                    value.Flag = adapter ? PosingSystemTextGUI.LayoutToggleLeft(label, value.Flag) : EditorGUILayout.ToggleLeft(label, value.Flag); break;
                case Control.EditorToggle:
                    value.Flag = adapter ? PosingSystemTextGUI.LayoutEditorToggle(label, value.Flag) : EditorGUILayout.Toggle(label, value.Flag); break;
                case Control.Foldout:
                    value.Flag = adapter ? PosingSystemTextGUI.LayoutFoldout(value.Flag, label, true) : EditorGUILayout.Foldout(value.Flag, label, true); break;
                case Control.Slider:
                    value.Number = adapter ? PosingSystemTextGUI.LayoutSlider(label, value.Number, 0, 1) : EditorGUILayout.Slider(label, value.Number, 0, 1); break;
                case Control.TextField:
                    value.Text = adapter ? PosingSystemTextGUI.LayoutTextField(label, value.Text) : EditorGUILayout.TextField(label, value.Text); break;
            }
        }

        private static List<Trace> Sequence(Container container, Control control, bool adapter, bool disabled, string text, bool establishCaret = true)
        {
            var value = new Value(); var label = new GUIContent(text, "العربية {0} tooltip");
            Rect rect = default;
            var events = new List<EventType> { EventType.Layout, EventType.Repaint, EventType.MouseDown, EventType.MouseDrag, EventType.MouseUp };
            bool normalizeTextCaret = control == Control.TextField && establishCaret;
            if (normalizeTextCaret)
                events.AddRange(new[] { EventType.KeyDown, EventType.KeyUp });
            events.AddRange(new[] { EventType.KeyDown, EventType.KeyUp, EventType.Repaint });
            var traces = new List<Trace>(); int acquired = 0;
            EventType requestedType = EventType.Layout;
            Action render = () =>
            {
                    EventType type = requestedType;
                    GUI.enabled = !disabled; GUI.changed = false;
                    if (container.InRequestedEvent && (type == EventType.KeyDown || type == EventType.KeyUp) && acquired != 0)
                        GUIUtility.keyboardControl = acquired;
                    int before = GUIUtility.GetControlID(91917, FocusType.Passive);
                    string editorBefore = control == Control.TextField ? TextEditorState() : null;
                    Color prefix = EditorStyles.label.normal.textColor, foldout = EditorStyles.foldout.normal.textColor;
                    using (PosingSystemTextGUI.BeginGUI(Owner, 0)) Draw(control, adapter, label, value);
                    if (container.InRequestedEvent && type == EventType.Repaint) rect = GUILayoutUtility.GetLastRect();
                    if (container.InRequestedEvent && type == EventType.MouseDown) acquired = GUIUtility.hotControl;
                    Assert.AreEqual(prefix, EditorStyles.label.normal.textColor, "Prefix ink restored after native control");
                    Assert.AreEqual(foldout, EditorStyles.foldout.normal.textColor, "Foldout ink restored after native control");
                    int after = GUIUtility.GetControlID(91918, FocusType.Passive);
                    // Marker IDs and the control tree also run for natural events;
                    // only explicit requested events contribute comparison traces.
                    if (container.InRequestedEvent) traces.Add(new Trace { Before = before, After = after,
                        Hot = GUIUtility.hotControl, Keyboard = GUIUtility.keyboardControl, Changed = GUI.changed,
                        Flag = value.Flag, Number = value.Number, Text = value.Text, Event = Event.current.type, Rect = rect,
                        EditorBefore = editorBefore, EditorAfter = control == Control.TextField ? TextEditorState() : null });
                    Assert.AreEqual(text, label.text); Assert.AreEqual("العربية {0} tooltip", label.tooltip);
            };
            for (int i = 0; i < events.Count; i++)
            {
                requestedType = events[i];
                var input = new Event { type = requestedType, button = 0,
                    mousePosition = new Vector2(rect.x + rect.width * .72f, rect.y + rect.height * .5f),
                    keyCode = KeyCode.Space, character = ' ' };
                if (normalizeTextCaret && (i == 5 || i == 6))
                {
                    input.keyCode = KeyCode.A; input.character = '\0';
                    input.modifiers = Application.platform == RuntimePlatform.OSXEditor ? EventModifiers.Command : EventModifiers.Control;
                }
                container.Frame(input, render, i == 0);
                // These are read-only observations of real keyboard handling. No
                // cursor/selection internals are assigned to make the test pass.
                if (normalizeTextCaret && !disabled && i == 5)
                {
                    var editor = RecycledEditor(); Assert.NotNull(editor);
                    Assert.AreEqual(value.Text.Length, Math.Abs(editor.cursorIndex - editor.selectIndex),
                        "Real Ctrl/Command+A must select all text before independent caret normalization");
                }
            }
            if (control == Control.TextField)
                for (int i = 0; i < traces.Count; i++)
                    Console.WriteLine("TextField " + (adapter ? "facade" : "native") + " disabled=" + disabled + " event=" + i +
                        " before{" + traces[i].EditorBefore + "} after{" + traces[i].EditorAfter + "} returned[" + traces[i].Text + "]");
            container.EndSequence();
            return traces;
        }

        private static string TextEditorState()
        {
            var editor = RecycledEditor();
            var viewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GUIView");
            var view = viewType?.GetProperty("current", All)?.GetValue(null);
            object focused = viewType?.GetProperty("hasFocus", All)?.GetValue(view);
            return (editor == null ? "editor=null" : "text[" + editor.text + "] cursor=" + editor.cursorIndex +
                " select=" + editor.selectIndex + " control=" + editor.controlID) +
                " focused=" + focused + " editing=" + EditorGUIUtility.editingTextField +
                " cursorAlpha=" + GUI.skin.settings.cursorColor.a +
                " drag=" + typeof(EditorGUI).GetField("s_DragToPosition", All)?.GetValue(null) +
                " dragged=" + typeof(EditorGUI).GetField("s_Dragged", All)?.GetValue(null) +
                " selectOnUp=" + typeof(EditorGUI).GetField("s_SelectAllOnMouseUp", All)?.GetValue(null);
        }

        private static TextEditor RecycledEditor()
            => typeof(EditorGUI).GetField("s_RecycledEditor", All)?.GetValue(null) as TextEditor;

        [TestCase("Button", false)] [TestCase("Button", true)]
        [TestCase("Toggle", false)] [TestCase("Toggle", true)]
        [TestCase("ToggleLeft", false)] [TestCase("ToggleLeft", true)]
        [TestCase("EditorToggle", false)] [TestCase("EditorToggle", true)]
        [TestCase("Foldout", false)] [TestCase("Foldout", true)]
        [TestCase("Slider", false)] [TestCase("Slider", true)]
        [TestCase("TextField", false)] [TestCase("TextField", true)]
        public void NativeAndFacade_KeepInputEventsIdsValuesAndChanged(string name, bool disabled)
        {
            var control = (Control)Enum.Parse(typeof(Control), name);
            using (var container = new Container())
            {
                var native = Sequence(container, control, false, disabled, "العربية {0}");
                var adapted = Sequence(container, control, true, disabled, "العربية {0}");
                Assert.AreEqual(native.Count, adapted.Count);
                for (int i = 0; i < native.Count; i++)
                {
                    var a = native[i]; var b = adapted[i]; string context = name + " event " + i;
                    Assert.AreEqual(a.Before, b.Before, context + " before ID");
                    Assert.AreEqual(a.After, b.After, context + " after ID: no extra facade control");
                    Assert.AreEqual(a.Hot, b.Hot, context + " hotControl");
                    Assert.AreEqual(a.Keyboard, b.Keyboard, context + " keyboardControl");
                    Assert.AreEqual(a.Event, b.Event, context + " event consumption");
                    Assert.AreEqual(a.Changed, b.Changed, context + " GUI.changed");
                    Assert.AreEqual(a.Flag, b.Flag, context + " bool value");
                    Assert.AreEqual(a.Number, b.Number, .0001, context + " slider value");
                    Assert.AreEqual(a.Text, b.Text, context + " editable logical text");
                    Assert.AreEqual(a.Rect, b.Rect, context + " control geometry");
                }
                // Native Foldout is structural navigation: disabled groups may still
                // expand/collapse. Its exact events/values were compared above.
                if (disabled && control != Control.Foldout)
                    Assert.IsTrue(adapted.TrueForAll(x => !x.Changed && x.Hot == 0), "Disabled value controls cannot acquire input or change values");
                if (!disabled && control == Control.Button) Assert.IsTrue(adapted.Exists(x => x.Flag), "Probe must actually click the native/facade button");
                if (!disabled && control == Control.TextField)
                {
                    Assert.AreEqual(" ", native[native.Count - 2].Text, "Native printable key replaces the selected logical value");
                    Assert.AreEqual(" ", adapted[adapted.Count - 2].Text, "Facade printable key preserves the same logical replacement");
                }
            }
            Assert.IsNull(typeof(PosingSystemTextRenderer).GetProperty("Failure", All)?.GetValue(null), "Complex input test must use working backend");
        }

        [TestCase(false, false)] [TestCase(true, false)]
        public void TextField_NativeControlControlAndReverseOrderAreIdentical(bool firstAdapter, bool secondAdapter)
        {
            using (var container = new Container())
            {
                var first = Sequence(container, Control.TextField, firstAdapter, false, "العربية {0}");
                var second = Sequence(container, Control.TextField, secondAdapter, false, "العربية {0}");
                for (int i = 0; i < first.Count; i++)
                {
                    string context = firstAdapter + "/" + secondAdapter + " text event " + i;
                    Assert.AreEqual(first[i].Text, second[i].Text, context);
                    Assert.AreEqual(first[i].Before, second[i].Before, context + " before ID");
                    Assert.AreEqual(first[i].After, second[i].After, context + " after ID");
                    Assert.AreEqual(first[i].Hot, second[i].Hot, context + " hotControl");
                    Assert.AreEqual(first[i].Keyboard, second[i].Keyboard, context + " keyboardControl");
                    Assert.AreEqual(first[i].Changed, second[i].Changed, context + " GUI.changed");
                    Assert.AreEqual(first[i].Event, second[i].Event, context + " consumed event");
                }
            }
        }

        [Test] public void TextField_ReportsUnisolatedNativeSelectionResidueAndChecksCleanup()
        {
            string rawFirst, rawSecond, cleanFirst, cleanSecond, normalizedFirst, normalizedSecond;
            string initialState;
            using (var unisolated = new Container(false))
            {
                var first = Sequence(unisolated, Control.TextField, false, false, "العربية {0}", false);
                initialState = first[0].EditorBefore;
                rawFirst = first[5].Text;
                rawSecond = Sequence(unisolated, Control.TextField, false, false, "العربية {0}", false)[5].Text;
            }
            using (var isolated = new Container())
            {
                cleanFirst = Sequence(isolated, Control.TextField, false, false, "العربية {0}", false)[5].Text;
                cleanSecond = Sequence(isolated, Control.TextField, false, false, "العربية {0}", false)[5].Text;
                var first = Sequence(isolated, Control.TextField, false, false, "العربية {0}");
                var second = Sequence(isolated, Control.TextField, false, false, "العربية {0}");
                normalizedFirst = first[first.Count - 2].Text;
                normalizedSecond = second[second.Count - 2].Text;
            }
            // Diagnostic baseline is deliberately native/native. If Unity changes
            // the unisolated behavior in a patch, that alone must not fail this test.
            Console.WriteLine("TextEditor native/native event5 without cleanup: [" + rawFirst + "] / [" + rawSecond +
                "]; with EndEditingActiveTextField only: [" + cleanFirst + "] / [" + cleanSecond +
                "]; real keyboard caret: [" + normalizedFirst + "] / [" + normalizedSecond + "]; initial{" + initialState + "}");
            bool cold = initialState.StartsWith("text[] cursor=0 select=0 control=0");
            Console.WriteLine(cold && rawFirst != rawSecond ?
                "Cold native/native selection discrepancy reproduced without any facade call." :
                "No cold native/native discrepancy in this warm domain; use a fresh-domain single-test run for the cold diagnostic.");
            // Ending editing does not reset selection indices in Unity2022.3.22f1.
            // Compare printable input only after real user keys establish equal state.
            Assert.AreEqual(" ", normalizedFirst);
            Assert.AreEqual(normalizedFirst, normalizedSecond, "Independent native sequences with equal caret must match");
        }

        [Test] public void OrdinaryPath_DoesNotInitializeOptionalNativeBackend()
        {
            var created = typeof(PosingSystemTextRenderer).GetField("created", All);
            Assert.NotNull(created);
            using (var container = new Container())
            {
                container.Frame(new Event { type = EventType.Repaint }, () =>
                {
                    using (PosingSystemTextGUI.BeginGUI(Owner))
                        PosingSystemTextGUI.Label(new Rect(0,0,300,30), new GUIContent("日本語 English français 中文 한국어", "plain tooltip"));
                    Assert.IsFalse((bool)created.GetValue(null));
                }, true);
            }
        }

        [TestCaseSource(nameof(Scripts))]
        public void Labels_SevenComplexScriptsKeepContentImagesAndGuiState(string text)
        {
            using (var container = new Container())
            {
                container.Frame(new Event { type = EventType.Repaint }, () =>
                {
                    var image = new Texture2D(16,16);
                    try
                    {
                        var content = new GUIContent(text + " Latin42 {0}", image, text + " tooltip");
                        var style = new GUIStyle(EditorStyles.label) { wordWrap = true, padding = new RectOffset(4,6,3,5) };
                        var color = new Color(.8f,.7f,.6f,.9f); GUI.color = color;
                        var contentColor = new Color(.7f,.8f,.9f,.8f); GUI.contentColor = contentColor;
                        var matrix = Matrix4x4.TRS(new Vector3(3,5,0), Quaternion.identity, new Vector3(1.2f,1.2f,1)); GUI.matrix = matrix;
                        GUI.enabled = false; GUI.changed = false;
                        using (PosingSystemTextGUI.BeginGUI(Owner))
                            PosingSystemTextGUI.Label(new Rect(0,0,240,80), content, style);
                        Assert.AreEqual(color, GUI.color); Assert.AreEqual(contentColor, GUI.contentColor);
                        Assert.AreEqual(matrix, GUI.matrix); Assert.IsFalse(GUI.enabled); Assert.IsFalse(GUI.changed);
                        Assert.AreEqual(text + " Latin42 {0}", content.text); Assert.AreSame(image, content.image);
                        Assert.AreEqual(text + " tooltip", content.tooltip);
                        Assert.IsNull(typeof(PosingSystemTextRenderer).GetProperty("Failure", All)?.GetValue(null));
                    }
                    finally { UnityEngine.Object.DestroyImmediate(image); }
                }, true);
            }
        }

        [Test] public void WrappingLayout_StabilizesAfterMeasuredWidthPass()
        {
            using (var container = new Container())
            {
                var style = new GUIStyle(EditorStyles.label) { wordWrap = true };
                string text = "العربية עברית हिन्दी বাংলা தமிழ் తెలుగు ภาษาไทย Latin42 {0} " +
                    "العربية עברית हिन्दी বাংলা தமிழ் తెలుగు ภาษาไทย Latin42 {0}";
                var content = new GUIContent(text); Rect previous = default, settled = default;
                Action render = () =>
                {
                    using (PosingSystemTextGUI.BeginGUI(Owner)) PosingSystemTextGUI.LayoutLabel(content, style, GUILayout.Width(220));
                    if (container.InRequestedEvent && Event.current.rawType == EventType.Repaint)
                    { previous = settled; settled = GUILayoutUtility.GetLastRect(); }
                };
                for (int pass = 0; pass < 3; pass++)
                {
                    container.Frame(new Event { type = EventType.Layout }, render, pass == 0, 220);
                    container.Frame(new Event { type = EventType.Repaint }, render, false, 220);
                }
                Assert.AreEqual(previous, settled, "Wrapping geometry must settle by third pass");
                Assert.AreEqual(220, settled.width, .1);
                Assert.GreaterOrEqual(settled.height + .5f, PosingSystemTextGUI.MeasureHeight(content, style, settled.width));
            }
        }
    }
}
#endif
