// Isolated integration candidate. Not imported by any product or live Unity project.
// No string reversal, serialized mutations, extra IMGUI control IDs or input rewriting.
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    internal static class PosingSystemTextGUI
    {
        private sealed class Frame
        {
            internal int Owner, Ordinal;
            internal float Width;
            internal Action Repaint;
        }

        private sealed class FrameScope : IDisposable
        {
            private readonly Frame previous;
            private readonly IDisposable tooltip;
            internal FrameScope(int owner, float width, Action repaint, Rect? tooltipViewport, bool disabledTooltips)
            {
                previous = frame; frame = new Frame { Owner = owner, Width = width, Repaint = repaint };
                tooltip = PosingSystemTextTooltip.Begin(owner, repaint, tooltipViewport, disabledTooltips);
            }
            public void Dispose()
            {
                try { tooltip.Dispose(); }
                finally { frame = previous; }
            }
        }

        private static Frame frame;
        private static readonly Dictionary<long, float> widths = new Dictionary<long, float>();
        private static readonly Dictionary<long, int> pointerControls = new Dictionary<long, int>();
        private static readonly Dictionary<long, int> keyboardControls = new Dictionary<long, int>();
        internal static int UnsupportedPropertyDrawers { get; private set; }
        internal static int PendingWrapLayouts { get; private set; }
        internal static int UnavailableLayoutRectProbes { get; private set; }
        // Register only reviewed drawers whose own OnGUI uses this facade. The
        // adapter cannot safely infer arbitrary third-party drawer label geometry.
        internal static Func<SerializedProperty, bool> PropertyDrawerOwnsLabel { get; set; }

        // One scope per OnGUI invocation. Owner must be the Editor/EditorWindow instance ID.
        // Explicit width is the available outer control width, not the whole window width.
        internal static IDisposable BeginGUI(int owner, float width = 0, Action repaint = null,
            Rect? tooltipViewport = null, bool disabledTooltips = true)
            => new FrameScope(owner, width, repaint, tooltipViewport, disabledTooltips);

        private static GUIContent Native(Rect rect, GUIContent content) => PosingSystemTextTooltip.Native(rect, content);
        private static GUIContent Native(GUIContent content) => PosingSystemTextTooltip.NativeContent(content);
        private static void RegisterLast(GUIContent content) => PosingSystemTextTooltip.RegisterLast(content);

        // GetLastRect deliberately returns a dummy after a native control consumes
        // an event. Read its existing layout entry without changing the event or
        // allocating a control, so pressed/focused ink keeps the same geometry.
        private static readonly FieldInfo layoutCache = typeof(GUILayoutUtility).GetField("current", BindingFlags.Static | BindingFlags.NonPublic);
        private static readonly FieldInfo layoutGroup = layoutCache?.FieldType.GetField("topLevel", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static readonly MethodInfo lastLayoutRect = layoutGroup?.FieldType.GetMethod("GetLast", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        private static Rect LastRect()
        {
            if (Event.current.type != EventType.Used) return GUILayoutUtility.GetLastRect();
            try
            {
                if (lastLayoutRect != null) return (Rect)lastLayoutRect.Invoke(layoutGroup.GetValue(layoutCache.GetValue(null)), null);
            }
            catch (Exception) { }
            UnavailableLayoutRectProbes++; return GUILayoutUtility.GetLastRect();
        }

        private static bool Complex(GUIContent content)
            => content != null && PosingSystemTextRenderer.Requires(content.text ?? "");
        private static GUIContent Content(string text) => new GUIContent(text ?? "");
        private static GUIStyle Style(GUIStyle style, GUIStyle fallback) => style ?? fallback;

        private static GUIStyle Hidden(GUIStyle original)
        {
            var style = new GUIStyle(original);
            foreach (var state in States(style)) state.textColor = Color.clear;
            return style;
        }

        private static GUIStyleState[] States(GUIStyle style) => new[] {
            style.normal, style.hover, style.active, style.focused,
            style.onNormal, style.onHover, style.onActive, style.onFocused };

        private static long SurfaceKey(Rect rect) => ((long)(frame?.Owner ?? 0) << 32) ^ (uint)rect.GetHashCode();
        private static void RememberControl(Rect rect, int beforePointer, int beforeKeyboard)
        {
            long key = SurfaceKey(rect);
            if (pointerControls.Count > 1024) pointerControls.Clear();
            if (keyboardControls.Count > 1024) keyboardControls.Clear();
            if (GUIUtility.hotControl != 0 && GUIUtility.hotControl != beforePointer) pointerControls[key] = GUIUtility.hotControl;
            if (GUIUtility.keyboardControl != 0 && GUIUtility.keyboardControl != beforeKeyboard) keyboardControls[key] = GUIUtility.keyboardControl;
        }
        private static GUIStyle SurfaceInk(Rect rect, GUIStyle style, bool on = false)
        {
            long key = SurfaceKey(rect);
            bool hover = GUI.enabled && rect.Contains(Event.current.mousePosition);
            bool active = hover && pointerControls.TryGetValue(key, out var pointer) && pointer == GUIUtility.hotControl;
            bool focused = GUI.enabled && keyboardControls.TryGetValue(key, out var keyboard) && keyboard == GUIUtility.keyboardControl;
            var state = on ? active ? style.onActive : focused ? style.onFocused : hover ? style.onHover : style.onNormal :
                active ? style.active : focused ? style.focused : hover ? style.hover : style.normal;
            var ink = new GUIStyle(style); ink.normal.textColor = state.textColor;
            return ink;
        }

        // Preserve the original content and native control ID hints. Color never changes
        // layout; changing label text to empty would move fields or change property drawers.
        private sealed class PrefixInk : IDisposable
        {
            private readonly GUIStyle style;
            private readonly Color[] colors;
            internal PrefixInk(bool hide)
            {
                if (!hide || Event.current.type != EventType.Repaint) return;
                style = EditorStyles.label;
                var states = States(style); colors = new Color[states.Length];
                for (int i = 0; i < states.Length; i++) { colors[i] = states[i].textColor; states[i].textColor = Color.clear; }
            }
            public void Dispose()
            {
                if (style == null) return;
                var states = States(style);
                for (int i = 0; i < states.Length; i++) states[i].textColor = colors[i];
            }
        }

        private static Rect Prefix(Rect rect)
        {
            rect = EditorGUI.IndentedRect(rect);
            rect.width = Mathf.Min(rect.width, Mathf.Max(0, EditorGUIUtility.labelWidth - 15 * EditorGUI.indentLevel));
            return rect;
        }

        private static void DrawPrefix(Rect rect, GUIContent content)
        { if (Complex(content)) DrawText(Prefix(rect), content, EditorStyles.label); }

        private static void DrawText(Rect rect, GUIContent content, GUIStyle style, bool forceText = false)
        {
            if (Event.current.type != EventType.Repaint || (!forceText && !Complex(content))) return;
            if (style.imagePosition == ImagePosition.ImageOnly) return;
            // Native controls paint images/tooltips/backgrounds. The backend paints only
            // text; reserve its image slot here to avoid duplicate or overlapping images.
            if (content.image != null)
            {
                var inner = style.padding.Remove(rect);
                var icon = EditorGUIUtility.GetIconSize();
                float iw = icon.x > 0 ? icon.x : content.image.width;
                float ih = icon.y > 0 ? icon.y : content.image.height;
                if (style.imagePosition == ImagePosition.ImageAbove)
                    inner.yMin += Mathf.Min(inner.height, ih + 2);
                else
                {
                    float textWidth = PosingSystemTextRenderer.MeasureWidth(content.text, new GUIStyle(style) { padding = new RectOffset() });
                    float groupWidth = Mathf.Min(inner.width, iw + 4 + textWidth);
                    if (style.alignment == TextAnchor.MiddleCenter || style.alignment == TextAnchor.UpperCenter || style.alignment == TextAnchor.LowerCenter)
                        inner.x += (inner.width - groupWidth) * .5f;
                    else if (style.alignment == TextAnchor.MiddleRight || style.alignment == TextAnchor.UpperRight || style.alignment == TextAnchor.LowerRight)
                        inner.x += inner.width - groupWidth;
                    inner.width = groupWidth;
                    inner.xMin += Mathf.Min(inner.width, iw + 4);
                }
                rect = inner;
                style = new GUIStyle(style) { padding = new RectOffset() };
            }
            PosingSystemTextRenderer.Draw(rect, content, style);
        }

        // Paint-only text styles for a native multi-item control. Every item must
        // be repainted if the shared native style has its ink hidden, including
        // ordinary Latin items in a mixed-script list. Background/images stay native.
        private static GUIStyle TextOnly(GUIStyle style)
        {
            var ink = new GUIStyle(style);
            foreach (var state in States(ink)) { state.background = null; state.scaledBackgrounds = null; }
            return ink;
        }

        private sealed class StyleInk : IDisposable
        {
            private readonly List<GUIStyleState> states = new List<GUIStyleState>();
            private readonly List<Color> colors = new List<Color>();
            internal StyleInk(bool hide, params GUIStyle[] styles)
            {
                if (!hide || Event.current.type != EventType.Repaint) return;
                var seen = new HashSet<GUIStyle>();
                foreach (var style in styles)
                {
                    if (!seen.Add(style)) continue;
                    foreach (var state in States(style))
                    { states.Add(state); colors.Add(state.textColor); state.textColor = Color.clear; }
                }
            }
            public void Dispose() { for (int i = 0; i < states.Count; i++) states[i].textColor = colors[i]; }
        }

        internal static int LayoutSelectionGrid(int selected, string[] labels, int columns, params GUILayoutOption[] options)
            => LayoutSelectionGrid(selected, Array.ConvertAll(labels, Content), columns, GUI.skin.button, options);
        internal static int LayoutSelectionGrid(int selected, GUIContent[] labels, int columns, params GUILayoutOption[] options)
            => LayoutSelectionGrid(selected, labels, columns, GUI.skin.button, options);
        internal static int LayoutSelectionGrid(int selected, GUIContent[] labels, int columns, GUIStyle style, params GUILayoutOption[] options)
            => LayoutGrid(selected, labels, columns, style, false, options);
        internal static int LayoutToolbar(int selected, string[] labels, params GUILayoutOption[] options)
            => LayoutToolbar(selected, Array.ConvertAll(labels, Content), GUI.skin.button, options);
        internal static int LayoutToolbar(int selected, GUIContent[] labels, GUIStyle style, params GUILayoutOption[] options)
            => LayoutGrid(selected, labels, labels.Length, style, true, options);

        private static int LayoutGrid(int selected, GUIContent[] labels, int columns, GUIStyle style, bool toolbar, GUILayoutOption[] options)
        {
            style = Style(style, GUI.skin.button);
            bool complex = Array.Exists(labels, Complex);
            GUIStyle middle = toolbar ? GUI.skin.FindStyle(style.name + "mid") ?? style : style;
            GUIStyle first = toolbar ? GUI.skin.FindStyle(style.name + "left") ?? middle : style;
            GUIStyle last = toolbar ? GUI.skin.FindStyle(style.name + "right") ?? middle : style;
            var native = Array.ConvertAll(labels, Native);
            int pointer = GUIUtility.hotControl, keyboard = GUIUtility.keyboardControl;
            int result;
            // Native GUILayout/GUI owns all IDs, selected values, click/key events,
            // backgrounds and images. Only the text colors are hidden for Repaint.
            using (new StyleInk(complex, style, first, middle, last))
                result = toolbar ? GUILayout.Toolbar(selected, native, style, options) :
                    GUILayout.SelectionGrid(selected, native, columns, style, options);
            if (Event.current.type == EventType.Layout || labels.Length == 0 || columns <= 0) return result;
            Rect outer = LastRect();
            int rows = (labels.Length + columns - 1) / columns;
            float gap = Mathf.Max(style.margin.top, style.margin.bottom);
            float spacing = 0;
            for (int col = 0; col < columns - 1; col++)
            {
                var current = col == 0 ? first : middle;
                var next = col == columns - 2 ? last : middle;
                spacing += Mathf.Max(current.margin.right, next.margin.left);
            }
            float width = style.fixedWidth > 0 ? style.fixedWidth : (outer.width - spacing) / columns;
            float height = style.fixedHeight > 0 ? style.fixedHeight : (outer.height - gap * (rows - 1)) / rows;
            float x = outer.x, y = outer.y;
            for (int i = 0; i < labels.Length; i++)
            {
                int col = i % columns;
                var itemStyle = labels.Length == 1 ? style : col == 0 ? first :
                    i == labels.Length - 1 || col == columns - 1 ? last : middle;
                Rect rect = GUIUtility.AlignRectToDevice(new Rect(x, y, width, height));
                if (rect.Contains(Event.current.mousePosition) || i == result) RememberControl(rect, pointer, keyboard);
                if (complex) DrawText(rect, labels[i], TextOnly(SurfaceInk(rect, itemStyle, i == result)), true);
                PosingSystemTextTooltip.Register(rect, labels[i]);
                var next = col == columns - 2 ? last : middle;
                x = rect.xMax + Mathf.Max(itemStyle.margin.right, next.margin.left);
                if (col == columns - 1) { x = outer.x; y += height + gap; }
            }
            return result;
        }

        internal static float MeasureHeight(string text, GUIStyle style, float width)
            => PosingSystemTextRenderer.Requires(text ?? "") ? PosingSystemTextRenderer.MeasureHeight(text, style, width) : style.CalcHeight(Content(text), width);
        internal static float MeasureWidth(string text, GUIStyle style)
            => PosingSystemTextRenderer.Requires(text ?? "") ? PosingSystemTextRenderer.MeasureWidth(text, style) : style.CalcSize(Content(text)).x;

        // Use the same padding/image slots in measurement and DrawText. Strings have no
        // image; GUIContent overloads are required for help boxes and icon labels.
        internal static float MeasureHeight(GUIContent content, GUIStyle style, float width)
        {
            if (!Complex(content)) return style.CalcHeight(content, width);
            if (content.image == null) return MeasureHeight(content.text, style, width);
            var icon = EditorGUIUtility.GetIconSize();
            float iw = icon.x > 0 ? icon.x : content.image.width;
            float ih = icon.y > 0 ? icon.y : content.image.height;
            if (style.imagePosition == ImagePosition.ImageOnly) return ih + style.padding.vertical;
            var textStyle = new GUIStyle(style) { padding = new RectOffset() };
            float textWidth = Mathf.Max(1, width - style.padding.horizontal -
                (style.imagePosition == ImagePosition.ImageAbove ? 0 : iw + 4));
            float textHeight = MeasureHeight(content.text, textStyle, textWidth);
            return style.padding.vertical + (style.imagePosition == ImagePosition.ImageAbove ?
                ih + 2 + textHeight : Mathf.Max(ih, textHeight));
        }
        internal static float MeasureWidth(GUIContent content, GUIStyle style)
        {
            if (!Complex(content)) return style.CalcSize(content).x;
            if (content.image == null) return MeasureWidth(content.text, style);
            var icon = EditorGUIUtility.GetIconSize();
            float iw = icon.x > 0 ? icon.x : content.image.width;
            if (style.imagePosition == ImagePosition.ImageOnly) return iw + style.padding.horizontal;
            float text = MeasureWidth(content.text, new GUIStyle(style) { padding = new RectOffset() });
            return style.padding.horizontal + (style.imagePosition == ImagePosition.ImageAbove ? Mathf.Max(iw, text) : iw + 4 + text);
        }

        private static Rect TextLayout(GUIContent content, GUIStyle style, GUILayoutOption[] options)
        {
            long key = frame == null ? 0 : ((long)frame.Owner << 32) ^ (uint)++frame.Ordinal;
            float width = frame != null && frame.Width > 0 ? frame.Width :
                key != 0 && widths.TryGetValue(key, out var cached) ? cached : Mathf.Max(20, EditorGUIUtility.currentViewWidth - 40);
            float ideal = MeasureWidth(content, style);
            float height = style.wordWrap ? MeasureHeight(content, style, width) :
                Mathf.Max(style.CalcSize(content).y, MeasureHeight(content, style, Mathf.Max(ideal, 1)));
            // Explicit caller width/height options are applied last, just as in native GUI.
            var sized = new List<GUILayoutOption> { GUILayout.MinHeight(height), GUILayout.Height(height) };
            if (!style.wordWrap) sized.Add(GUILayout.MinWidth(ideal));
            if (options != null) sized.AddRange(options);
            Rect rect = GUILayoutUtility.GetRect(content, style, sized.ToArray());
            if (Event.current.type == EventType.Repaint && style.wordWrap && rect.width > 0 && Mathf.Abs(width - rect.width) > .5f)
            {
                PendingWrapLayouts++;
                if (key != 0)
                {
                    if (widths.Count > 1024) widths.Clear();
                    widths[key] = rect.width;
                    frame.Repaint?.Invoke();
                }
            }
            return rect;
        }

        internal static void Label(Rect rect, string text, GUIStyle style = null) => Label(rect, Content(text), style);
        internal static void Label(Rect rect, GUIContent content, GUIStyle style = null)
        {
            style = Style(style, GUI.skin.label);
            GUI.Label(rect, Native(rect, content), Complex(content) ? Hidden(style) : style);
            DrawText(rect, content, style);
        }

        internal static bool Button(Rect rect, string text, GUIStyle style = null) => Button(rect, Content(text), style);
        internal static bool Button(Rect rect, GUIContent content, GUIStyle style = null)
        {
            style = Style(style, GUI.skin.button);
            if (!Complex(content)) return GUI.Button(rect, Native(rect, content), style);
            int pointer = GUIUtility.hotControl, keyboard = GUIUtility.keyboardControl;
            bool result = GUI.Button(rect, Native(rect, content), Hidden(style));
            RememberControl(rect, pointer, keyboard);
            DrawText(rect, content, SurfaceInk(rect, style));
            return result;
        }

        internal static bool Toggle(Rect rect, bool value, GUIContent content, GUIStyle style)
        {
            if (!Complex(content)) return GUI.Toggle(rect, value, Native(rect, content), style);
            int pointer = GUIUtility.hotControl, keyboard = GUIUtility.keyboardControl;
            bool result = GUI.Toggle(rect, value, Native(rect, content), Hidden(style));
            RememberControl(rect, pointer, keyboard);
            DrawText(rect, content, SurfaceInk(rect, style, result));
            return result;
        }
        internal static bool Toggle(Rect rect, bool value, string label, GUIStyle style) => Toggle(rect, value, Content(label), style);

        internal static void LayoutLabel(string text, GUIStyle style, params GUILayoutOption[] options) => LayoutLabel(Content(text), style, options);
        internal static void LayoutLabel(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            if (!Complex(content)) { GUILayout.Label(Native(content), style, options); RegisterLast(content); return; }
            Label(TextLayout(content, style, options), content, style);
        }
        internal static void LayoutLabel(string text, params GUILayoutOption[] options) => LayoutLabel(Content(text), GUI.skin.label, options);

        internal static bool LayoutButton(string text, params GUILayoutOption[] options) => LayoutButton(Content(text), GUI.skin.button, options);
        internal static bool LayoutButton(GUIContent content, params GUILayoutOption[] options) => LayoutButton(content, GUI.skin.button, options);
        internal static bool LayoutButton(string text, GUIStyle style, params GUILayoutOption[] options) => LayoutButton(Content(text), style, options);
        internal static bool LayoutButton(GUIContent content, GUIStyle style, params GUILayoutOption[] options)
        {
            if (!Complex(content)) { bool value = GUILayout.Button(Native(content), style, options); RegisterLast(content); return value; }
            return Button(TextLayout(content, style, options), content, style);
        }

        internal static void LabelField(Rect rect, string label) => LabelField(rect, Content(label));
        internal static void LabelField(Rect rect, GUIContent label)
        {
            if (!Complex(label)) { EditorGUI.LabelField(rect, Native(rect, label)); return; }
            using (new PrefixInk(true)) EditorGUI.LabelField(rect, Native(rect, label));
            DrawText(EditorGUI.IndentedRect(rect), label, EditorStyles.label);
        }

        internal static void LayoutLabelField(string label, GUIStyle style, params GUILayoutOption[] options) => LayoutLabelField(Content(label), style, options);
        internal static void LayoutLabelField(string label, params GUILayoutOption[] options) => LayoutLabelField(Content(label), EditorStyles.label, options);
        internal static void LayoutLabelField(GUIContent label, GUIStyle style, params GUILayoutOption[] options)
        {
            if (!Complex(label)) { EditorGUILayout.LabelField(Native(label), style, options); RegisterLast(label); return; }
            Rect rect = TextLayout(label, style, options);
            EditorGUI.LabelField(rect, Native(rect, label), Hidden(style));
            DrawText(EditorGUI.IndentedRect(rect), label, style);
        }
        internal static void LayoutLabelField(string label, string value, params GUILayoutOption[] options)
        {
            var left = Content(label); var right = Content(value);
            if (!Complex(left) && !Complex(right)) { EditorGUILayout.LabelField(label, value, options); return; }
            Rect rect = EditorGUILayout.GetControlRect(true, EditorGUIUtility.singleLineHeight, options);
            using (new PrefixInk(Complex(left)))
                EditorGUI.LabelField(rect, left, right, Complex(right) ? Hidden(EditorStyles.label) : EditorStyles.label);
            DrawPrefix(rect, left);
            rect.xMin = Prefix(rect).xMax + 2;
            DrawText(rect, right, EditorStyles.label);
        }

        internal static void LayoutHelpBox(string message, MessageType type)
        {
            var content = Content(message);
            if (!Complex(content)) { EditorGUILayout.HelpBox(message, type); return; }
            if (type != MessageType.None)
                content.image = EditorGUIUtility.IconContent(type == MessageType.Error ? "console.erroricon" : type == MessageType.Warning ? "console.warnicon" : "console.infoicon").image;
            var style = new GUIStyle(EditorStyles.helpBox) { imagePosition = ImagePosition.ImageLeft };
            // Measure the actual text budget after reserving the icon; DrawText uses the
            // same image/padding policy. Explicit BeginGUI width gives first-pass accuracy.
            Rect rect = TextLayout(content, style, Array.Empty<GUILayoutOption>());
            Label(rect, content, style);
        }

        internal static int Popup(Rect rect, GUIContent label, int selected, string[] displayed, GUIStyle style = null)
            => Popup(rect, label, selected, Array.ConvertAll(displayed, Content), style);
        internal static int Popup(Rect rect, GUIContent label, int selected, GUIContent[] displayed, GUIStyle style = null)
        {
            style = Style(style, EditorStyles.popup);
            var selectedContent = selected >= 0 && selected < displayed.Length ? displayed[selected] : GUIContent.none;
            bool shapeSelection = Complex(selectedContent);
            int result;
            using (new PrefixInk(Complex(label)))
                result = EditorGUI.Popup(rect, Native(rect, label), selected, displayed, shapeSelection ? Hidden(style) : style);
            DrawPrefix(rect, label);
            if (label != null && (!string.IsNullOrEmpty(label.text) || label.image != null)) rect.xMin = Prefix(rect).xMax + 2;
            // Reserve the arrow once; popup styles already normally include this padding.
            var ink = new GUIStyle(style); ink.padding.right = Mathf.Max(14, ink.padding.right);
            DrawText(rect, selectedContent, ink);
            PosingSystemTextTooltip.Register(rect, selectedContent);
            return result;
        }
        internal static int LayoutPopup(int selected, string[] displayed, params GUILayoutOption[] options) => LayoutPopup(GUIContent.none, selected, displayed, options);
        internal static int LayoutPopup(string label, int selected, string[] displayed, params GUILayoutOption[] options) => LayoutPopup(Content(label), selected, displayed, options);
        internal static int LayoutPopup(GUIContent label, int selected, string[] displayed, params GUILayoutOption[] options)
        {
            bool complex = Complex(label) || selected >= 0 && selected < displayed.Length && PosingSystemTextRenderer.Requires(displayed[selected]);
            if (!complex) { int value = EditorGUILayout.Popup(Native(label), selected, displayed, options); RegisterLast(label); return value; }
            return Popup(EditorGUILayout.GetControlRect(label != GUIContent.none, EditorGUIUtility.singleLineHeight, options), label, selected, displayed);
        }
        internal static int LayoutPopup(GUIContent label, int selected, GUIContent[] displayed, params GUILayoutOption[] options)
        {
            bool complex = Complex(label) || selected >= 0 && selected < displayed.Length && Complex(displayed[selected]);
            if (!complex) { int value = EditorGUILayout.Popup(Native(label), selected, displayed, options); RegisterLast(label); return value; }
            return Popup(EditorGUILayout.GetControlRect(label != GUIContent.none, EditorGUIUtility.singleLineHeight, options), label, selected, displayed);
        }

        internal static bool Foldout(Rect rect, bool expanded, string label, bool toggleOnLabelClick) => Foldout(rect, expanded, Content(label), toggleOnLabelClick);
        internal static bool Foldout(Rect rect, bool expanded, GUIContent label, bool toggleOnLabelClick, GUIStyle style = null)
        {
            style = Style(style, EditorStyles.foldout);
            int pointer = GUIUtility.hotControl, keyboard = GUIUtility.keyboardControl;
            bool result = EditorGUI.Foldout(rect, expanded, Native(rect, label), toggleOnLabelClick, Complex(label) ? Hidden(style) : style);
            RememberControl(rect, pointer, keyboard);
            var ink = SurfaceInk(rect, style, result); ink.padding.left = Mathf.Max(14, ink.padding.left);
            DrawText(EditorGUI.IndentedRect(rect), label, ink);
            return result;
        }
        internal static bool LayoutFoldout(bool expanded, string label, bool toggleOnLabelClick) => LayoutFoldout(expanded, Content(label), toggleOnLabelClick);
        internal static bool LayoutFoldout(bool expanded, GUIContent label, bool toggleOnLabelClick)
        {
            if (!Complex(label)) { bool value = EditorGUILayout.Foldout(expanded, Native(label), toggleOnLabelClick); RegisterLast(label); return value; }
            int pointer = GUIUtility.hotControl, keyboard = GUIUtility.keyboardControl;
            bool result;
            using (new StyleInk(true, EditorStyles.foldout))
                result = EditorGUILayout.Foldout(expanded, Native(label), toggleOnLabelClick);
            Rect rect = LastRect(); RegisterLast(label);
            RememberControl(rect, pointer, keyboard);
            var ink = SurfaceInk(rect, EditorStyles.foldout, result); ink.padding.left = Mathf.Max(14, ink.padding.left);
            DrawText(EditorGUI.IndentedRect(rect), label, ink); return result;
        }

        internal static bool LayoutToggleLeft(string label, bool value, params GUILayoutOption[] options) => LayoutToggleLeft(Content(label), value, options);
        internal static bool LayoutToggleLeft(GUIContent label, bool value, params GUILayoutOption[] options)
        {
            if (!Complex(label)) { bool changed = EditorGUILayout.ToggleLeft(Native(label), value, options); RegisterLast(label); return changed; }
            bool result;
            using (new PrefixInk(true)) result = EditorGUILayout.ToggleLeft(Native(label), value, options);
            Rect rect = LastRect(); RegisterLast(label);
            rect = EditorGUI.IndentedRect(rect); rect.xMin += 14;
            DrawText(rect, label, EditorStyles.label);
            return result;
        }
        internal static bool LayoutToggle(bool value, GUIContent label, params GUILayoutOption[] options)
        {
            if (!Complex(label)) { bool changed = GUILayout.Toggle(value, Native(label), options); RegisterLast(label); return changed; }
            Rect rect = TextLayout(label, GUI.skin.toggle, options);
            return Toggle(rect, value, label, GUI.skin.toggle);
        }
        internal static bool LayoutEditorToggle(GUIContent label, bool value, params GUILayoutOption[] options)
        {
            if (!Complex(label)) { bool changed = EditorGUILayout.Toggle(Native(label), value, options); RegisterLast(label); return changed; }
            bool result;
            using (new PrefixInk(true)) result = EditorGUILayout.Toggle(Native(label), value, options);
            Rect rect = LastRect(); RegisterLast(label);
            DrawPrefix(rect, label); return result;
        }

        internal static float LayoutSlider(string label, float value, float min, float max) => LayoutSlider(Content(label), value, min, max);
        internal static float LayoutSlider(GUIContent label, float value, float min, float max)
        {
            if (!Complex(label)) { float changed = EditorGUILayout.Slider(Native(label), value, min, max); RegisterLast(label); return changed; }
            float result;
            using (new PrefixInk(true)) result = EditorGUILayout.Slider(Native(label), value, min, max);
            Rect rect = LastRect(); RegisterLast(label);
            DrawPrefix(rect, label); return result;
        }
        internal static void LayoutSlider(SerializedProperty property, float min, float max, string label, params GUILayoutOption[] options)
            => LayoutSlider(property, min, max, Content(label), options);
        internal static void LayoutSlider(SerializedProperty property, float min, float max, GUIContent label, params GUILayoutOption[] options)
        {
            if (!Complex(label)) { EditorGUILayout.Slider(property, min, max, Native(label), options); RegisterLast(label); return; }
            using (new PrefixInk(true)) EditorGUILayout.Slider(property, min, max, Native(label), options);
            Rect rect = LastRect(); RegisterLast(label);
            DrawPrefix(rect, label);
        }

        internal static bool PropertyField(Rect rect, SerializedProperty property, GUIContent label, bool includeChildren = false)
        {
            // Arbitrary custom drawers and child labels require an explicit adapter in
            // their own OnGUI. Do not hide all their text or alter their input handling.
            if (PropertyDrawerOwnsLabel?.Invoke(property) == true)
                return EditorGUI.PropertyField(rect, property, Native(rect, label), includeChildren);
            if (!Complex(label)) return EditorGUI.PropertyField(rect, property, Native(rect, label), includeChildren);
            if (includeChildren || property.propertyType == SerializedPropertyType.Generic || property.propertyType == SerializedPropertyType.Enum)
            { UnsupportedPropertyDrawers++; return EditorGUI.PropertyField(rect, property, Native(rect, label), includeChildren); }
            bool result;
            using (new PrefixInk(true)) result = EditorGUI.PropertyField(rect, property, Native(rect, label), false);
            DrawPrefix(rect, label); return result;
        }
        internal static bool LayoutPropertyField(SerializedProperty property, GUIContent label, bool includeChildren = false, params GUILayoutOption[] options)
        {
            if (!Complex(label)) { bool value = EditorGUILayout.PropertyField(property, Native(label), includeChildren, options); RegisterLast(label); return value; }
            bool owns = PropertyDrawerOwnsLabel?.Invoke(property) == true;
            bool unsupported = !owns && (includeChildren || property.propertyType == SerializedPropertyType.Generic || property.propertyType == SerializedPropertyType.Enum);
            if (unsupported) UnsupportedPropertyDrawers++;
            bool result;
            using (new PrefixInk(!owns && !unsupported))
                result = EditorGUILayout.PropertyField(property, Native(label), includeChildren, options);
            Rect rect = LastRect(); RegisterLast(label);
            if (!owns && !unsupported) DrawPrefix(rect, label);
            return result;
        }
        internal static bool LayoutPropertyField(SerializedProperty property, bool includeChildren, params GUILayoutOption[] options)
            => LayoutPropertyField(property, new GUIContent(property.displayName, property.tooltip), includeChildren, options);
        internal static bool LayoutPropertyField(SerializedProperty property, params GUILayoutOption[] options)
            => LayoutPropertyField(property, false, options);

        internal static Object LayoutObjectField(string label, Object value, Type type, bool allowSceneObjects, params GUILayoutOption[] options)
            => LayoutObjectField(Content(label), value, type, allowSceneObjects, options);
        internal static Object LayoutObjectField(GUIContent label, Object value, Type type, bool allowSceneObjects, params GUILayoutOption[] options)
        {
            if (!Complex(label)) { Object changed = EditorGUILayout.ObjectField(Native(label), value, type, allowSceneObjects, options); RegisterLast(label); return changed; }
            Object result;
            using (new PrefixInk(true)) result = EditorGUILayout.ObjectField(Native(label), value, type, allowSceneObjects, options);
            Rect rect = LastRect(); RegisterLast(label);
            DrawPrefix(rect, label); return result;
        }
        internal static Object LayoutObjectField(Object value, Type type, bool allowSceneObjects, params GUILayoutOption[] options)
            => EditorGUILayout.ObjectField(value, type, allowSceneObjects, options);

        internal static string LayoutTextField(string label, string value, params GUILayoutOption[] options) => LayoutTextField(Content(label), value, options);
        internal static string LayoutTextField(GUIContent label, string value, params GUILayoutOption[] options)
        {
            if (!Complex(label)) { string changed = EditorGUILayout.TextField(Native(label), value, options); RegisterLast(label); return changed; }
            string result;
            using (new PrefixInk(true)) result = EditorGUILayout.TextField(Native(label), value, options);
            Rect rect = LastRect(); RegisterLast(label);
            DrawPrefix(rect, label); return result;
        }
        internal static string LayoutTextField(string value, GUIStyle style, params GUILayoutOption[] options) => EditorGUILayout.TextField(value, style, options);
        internal static string LayoutTextField(string value, params GUILayoutOption[] options) => EditorGUILayout.TextField(value, options);
        internal static float LayoutFloatField(float value, params GUILayoutOption[] options) => EditorGUILayout.FloatField(value, options);

        internal static Vector3 LayoutVector3Field(GUIContent label, Vector3 value, params GUILayoutOption[] options)
        {
            if (!Complex(label)) { Vector3 changed = EditorGUILayout.Vector3Field(Native(label), value, options); RegisterLast(label); return changed; }
            Vector3 result;
            using (new PrefixInk(true)) result = EditorGUILayout.Vector3Field(Native(label), value, options);
            Rect rect = LastRect(); RegisterLast(label);
            if (EditorGUIUtility.wideMode) DrawPrefix(rect, label);
            else { rect = EditorGUI.IndentedRect(rect); rect.height = EditorGUIUtility.singleLineHeight; DrawText(rect, label, EditorStyles.label); }
            return result;
        }
        internal static bool LayoutLinkButton(string label, params GUILayoutOption[] options)
        {
            if (!PosingSystemTextRenderer.Requires(label ?? "")) return EditorGUILayout.LinkButton(label, options);
            var content = Content(label);
            Rect rect = TextLayout(content, EditorStyles.linkLabel, options);
            EditorGUIUtility.AddCursorRect(rect, MouseCursor.Link);
            return Button(rect, content, EditorStyles.linkLabel);
        }
    }
}
#endif
