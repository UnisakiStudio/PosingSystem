// Isolated Editor tooltip overlay: logical strings, no controls or input consumption.
#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    internal static class PosingSystemTextTooltip
    {
        internal const double DelaySeconds = .6;
        internal const float MovementTolerance = 4;
        internal static int ClippedTooltips { get; private set; }
        internal static int UnavailableClipProbes { get; private set; }
        internal static Exception RepaintFailure { get; private set; }

        // Pure hover lifecycle, also exercised by the isolated runtime probe.
        internal sealed class HoverState
        {
            internal int Owner;
            internal string Text;
            internal Rect Target;
            internal Vector2 Anchor, Mouse;
            internal double Since, Seen;
            internal bool Visible(double now) => Text != null && now - Since >= DelaySeconds;
            internal void Clear() { Text = null; }
            internal void Observe(int owner, string text, Rect target, Vector2 mouse, double now)
            {
                float dx = mouse.x - Anchor.x, dy = mouse.y - Anchor.y;
                if (Text != text || Owner != owner || dx * dx + dy * dy > MovementTolerance * MovementTolerance ||
                    Math.Abs(Target.x - target.x) > .5f || Math.Abs(Target.y - target.y) > .5f ||
                    Math.Abs(Target.width - target.width) > .5f || Math.Abs(Target.height - target.height) > .5f)
                { Since = now; Anchor = mouse; }
                Owner = owner; Text = text; Target = target; Mouse = mouse; Seen = now;
            }
        }

        internal sealed class Scope : IDisposable
        {
            internal readonly int Owner;
            internal readonly Action Repaint;
            internal readonly bool AllowDisabled;
            internal readonly Rect Viewport;
            internal readonly EditorWindow Window;
            internal bool HasCandidate;
            internal string Text;
            internal Rect Target;
            internal Vector2 Mouse;
            private readonly Scope previous;
            internal Scope(int owner, Action repaint, Rect? viewport, bool allowDisabled)
            {
                previous = current; current = this;
                Owner = owner; Repaint = repaint; AllowDisabled = allowDisabled;
                Window = CurrentWindow();
                var nativeViewport = CurrentViewport(Window);
                Viewport = viewport.HasValue ? Intersection(viewport.Value, nativeViewport) : nativeViewport;
                if (Window == activeWindow && (Cancels(Event.current.rawType) || GUIUtility.hotControl != 0)) hover.Clear();
            }
            public void Dispose()
            {
                try
                {
                    // One overlay per top-level GUI invocation. Child groups have
                    // already been popped by the caller when the root scope ends.
                    if (previous != null)
                    {
                        if (HasCandidate)
                        {
                            previous.HasCandidate = true; previous.Text = Text;
                            previous.Target = Target; previous.Mouse = Mouse;
                        }
                    }
                    else Finish(this);
                }
                finally { current = previous; }
            }
        }

        private static Scope current;
        private static readonly HoverState hover = new HoverState();
        private static EditorWindow activeWindow;
        private static Action activeRepaint;
        private static bool ticking;
        private static double nextRepaint;
        // Unity2022 has no public visible-clip query. Read-only cached reflection
        // avoids registering controls hidden behind scroll-view clipping. If that
        // contract is unavailable, retain native tooltips instead of losing text.
        private static readonly PropertyInfo visibleRect = typeof(GUI).Assembly
            .GetType("UnityEngine.GUIClip")?.GetProperty("visibleRect", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly PropertyInfo currentView = typeof(EditorWindow).Assembly
            .GetType("UnityEditor.GUIView")?.GetProperty("current", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        private static readonly PropertyInfo actualWindow = typeof(EditorWindow).Assembly
            .GetType("UnityEditor.HostView")?.GetProperty("actualView", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        private static bool clipProbeFailed;
        internal static bool NativeBindingsAvailable => visibleRect != null && currentView != null && actualWindow != null;

        private static EditorWindow CurrentWindow()
        {
            try
            {
                var view = currentView?.GetValue(null);
                return view == null || actualWindow == null || !actualWindow.DeclaringType.IsInstanceOfType(view) ?
                    null : actualWindow.GetValue(view) as EditorWindow;
            }
            catch (Exception) { return null; }
        }

        internal static Scope Begin(int owner, Action repaint, Rect? viewport, bool allowDisabled)
            => new Scope(owner, repaint, viewport, allowDisabled);
        internal static bool ComplexTooltip(GUIContent content)
            => content != null && !string.IsNullOrEmpty(content.tooltip) && PosingSystemTextRenderer.Requires(content.tooltip);
        private static GUIStyle TooltipStyle()
            => new GUIStyle(GUI.skin.FindStyle("Tooltip") ?? EditorStyles.helpBox) {
                wordWrap = true, fixedWidth = 0, fixedHeight = 0,
                clipping = TextClipping.Clip, alignment = TextAnchor.UpperLeft };

        internal static GUIContent NativeContent(GUIContent content)
        {
            if (current == null || !ComplexTooltip(content) || current.Window == null ||
                current.Window != EditorWindow.mouseOverWindow || current.Viewport.width < 12 ||
                !TryVisibleRect(out _) || !PosingSystemTextRenderer.TryPrepare(content.tooltip, TooltipStyle(),
                    Mathf.Min(360, current.Viewport.width - 8))) return content;
            // GUIContent.hash uses text only in Unity2022.3. Copying image/text
            // and clearing only tooltip preserves native ID hints and layout.
            return new GUIContent(content) { tooltip = "" };
        }

        internal static GUIContent Native(Rect rect, GUIContent content)
        {
            Register(rect, content);
            return NativeContent(content);
        }
        internal static void RegisterLast(GUIContent content)
        {
            if (ComplexTooltip(content) && Event.current.type == EventType.Repaint)
                Register(GUILayoutUtility.GetLastRect(), content);
        }
        internal static void Register(Rect rect, GUIContent content)
        {
            if (current == null || current.Window == null || current.Window != EditorWindow.mouseOverWindow ||
                Event.current.type != EventType.Repaint || !ComplexTooltip(content) ||
                (!GUI.enabled && !current.AllowDisabled) || GUIUtility.hotControl != 0 ||
                !rect.Contains(Event.current.mousePosition) || !TryVisibleRect(out var clip) ||
                !clip.Contains(Event.current.mousePosition)) return;
            var mouse = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            if (!current.Viewport.Contains(mouse)) return;
            // Use the same local hit test as native IMGUI, then convert all corners so
            // scaled/rotated GUI.matrix affects the anchor bounds, not tooltip text order.
            var target = ScreenBounds(rect);
            // A GUILayout PropertyField registers its outer rect after a nested
            // drawer returned. Keep the child's more specific tooltip in that case.
            if (current.HasCandidate && KeepsChild(current.Target, target)) return;
            current.HasCandidate = true; current.Text = content.tooltip;
            current.Target = target;
            current.Mouse = mouse;
        }

        internal static bool KeepsChild(Rect existing, Rect candidate)
            => existing.width * existing.height < candidate.width * candidate.height &&
                candidate.xMin <= existing.xMin && candidate.yMin <= existing.yMin &&
                candidate.xMax >= existing.xMax && candidate.yMax >= existing.yMax;

        private static bool TryVisibleRect(out Rect rect)
        {
            rect = default;
            if (clipProbeFailed) return false;
            try
            {
                if (visibleRect != null) { rect = (Rect)visibleRect.GetValue(null); return true; }
            }
            catch (Exception) { /* Native fallback remains available. */ }
            clipProbeFailed = true; UnavailableClipProbes++; return false;
        }

        internal static bool Cancels(EventType type) => type == EventType.MouseDown || type == EventType.MouseUp ||
            type == EventType.MouseDrag || type == EventType.ScrollWheel || type == EventType.KeyDown ||
            type == EventType.KeyUp || type == EventType.ContextClick || type == EventType.MouseLeaveWindow ||
            type == EventType.DragUpdated || type == EventType.DragPerform || type == EventType.DragExited;

        private static Rect ScreenBounds(Rect rect)
        {
            Vector2 a = GUIUtility.GUIToScreenPoint(new Vector2(rect.xMin, rect.yMin));
            Vector2 b = GUIUtility.GUIToScreenPoint(new Vector2(rect.xMax, rect.yMin));
            Vector2 c = GUIUtility.GUIToScreenPoint(new Vector2(rect.xMin, rect.yMax));
            Vector2 d = GUIUtility.GUIToScreenPoint(new Vector2(rect.xMax, rect.yMax));
            return Rect.MinMaxRect(Mathf.Min(a.x,b.x,c.x,d.x), Mathf.Min(a.y,b.y,c.y,d.y),
                Mathf.Max(a.x,b.x,c.x,d.x), Mathf.Max(a.y,b.y,c.y,d.y));
        }

        private static Rect CurrentViewport(EditorWindow window)
        {
            if (window == null) return new Rect();
            var bounds = window.position;
            var origin = GUIUtility.GUIToScreenPoint(Vector2.zero);
            var viewport = Rect.MinMaxRect(Mathf.Max(bounds.xMin, origin.x), Mathf.Max(bounds.yMin, origin.y),
                bounds.xMax, bounds.yMax);
            // A CustomEditor can already be inside Unity's own Inspector scroll
            // clip before our root scope begins. Never place outside that clip.
            return TryVisibleRect(out var clip) ? Intersection(viewport, ScreenBounds(clip)) : viewport;
        }

        private static Rect Intersection(Rect a, Rect b)
            => Rect.MinMaxRect(Mathf.Max(a.xMin,b.xMin), Mathf.Max(a.yMin,b.yMin),
                Mathf.Max(Mathf.Max(a.xMin,b.xMin), Mathf.Min(a.xMax,b.xMax)),
                Mathf.Max(Mathf.Max(a.yMin,b.yMin), Mathf.Min(a.yMax,b.yMax)));

        // Coordinates here are absolute screen points in the same space as GUIToScreenPoint.
        internal static Rect Place(Rect viewport, Vector2 mouse, Rect target, float width, float height)
        {
            const float margin = 4;
            width = Math.Max(1, Math.Min(width, viewport.width - margin * 2));
            height = Math.Max(1, Math.Min(height, viewport.height - margin * 2));
            float x = Math.Max(viewport.xMin + margin, Math.Min(mouse.x + 12, viewport.xMax - margin - width));
            float y = mouse.y + 20;
            if (y + height > viewport.yMax - margin) y = target.yMin - height - 6;
            y = Math.Max(viewport.yMin + margin, Math.Min(y, viewport.yMax - margin - height));
            return new Rect(x, y, width, height);
        }

        private static void Finish(Scope scope)
        {
            if (Event.current.type != EventType.Repaint) return;
            // A normal child/native tooltip takes priority over an enclosing overlay.
            if (!scope.HasCandidate || !string.IsNullOrEmpty(GUI.tooltip) || scope.Viewport.width < 12 || scope.Viewport.height < 12)
            {
                if (hover.Owner == scope.Owner) hover.Clear();
                return;
            }
            double now = EditorApplication.timeSinceStartup;
            hover.Observe(scope.Owner, scope.Text, scope.Target, scope.Mouse, now);
            activeWindow = scope.Window; activeRepaint = scope.Repaint;
            if (!ticking) { EditorApplication.update += Tick; ticking = true; nextRepaint = now; }
            if (!hover.Visible(now)) return;

            var originalMatrix = GUI.matrix;
            var originalColor = GUI.color; var originalContentColor = GUI.contentColor;
            bool originalEnabled = GUI.enabled;
            try
            {
                // The scope must end outside child groups/scroll views. Draw upright in
                // root GUI coordinates; the source control's transform stays untouched.
                GUI.matrix = Matrix4x4.identity;
                GUI.color = Color.white; GUI.contentColor = Color.white; GUI.enabled = true;
                var style = TooltipStyle();
                float width = Mathf.Min(360, scope.Viewport.width - 8);
                width = Mathf.Max(1, Mathf.Min(width, PosingSystemTextRenderer.MeasureWidth(scope.Text, style)));
                float height = PosingSystemTextRenderer.MeasureHeight(scope.Text, style, width);
                Rect screen = Place(scope.Viewport, scope.Mouse, scope.Target, width, height);
                if (screen.height + .5f < height) ClippedTooltips++;
                Vector2 top = GUIUtility.ScreenToGUIPoint(screen.min), bottom = GUIUtility.ScreenToGUIPoint(screen.max);
                Rect rect = Rect.MinMaxRect(top.x, top.y, bottom.x, bottom.y);
                // GUIStyle.Draw draws the background directly; no GUI.Box/Label control ID.
                style.Draw(rect, GUIContent.none, false, false, false, false);
                PosingSystemTextRenderer.Draw(rect, new GUIContent(scope.Text), style);
            }
            finally
            {
                GUI.matrix = originalMatrix; GUI.color = originalColor;
                GUI.contentColor = originalContentColor; GUI.enabled = originalEnabled;
            }
        }

        private static void Tick()
        {
            double now = EditorApplication.timeSinceStartup;
            if (hover.Text == null || now - hover.Seen > 1 || activeWindow == null || EditorWindow.mouseOverWindow != activeWindow)
            {
                StopTick(true);
                return;
            }
            // Repaint validates current hit geometry even when the native tooltip was
            // deliberately suppressed and the window does not request mouse-move events.
            if (now < nextRepaint) return;
            nextRepaint = now + .1;
            try { if (activeRepaint != null) activeRepaint(); else activeWindow.Repaint(); }
            catch (Exception error) { RepaintFailure = error; StopTick(false); }
        }

        private static void StopTick(bool repaint)
        {
            var callback = activeRepaint;
            hover.Clear(); EditorApplication.update -= Tick; ticking = false;
            activeWindow = null; activeRepaint = null;
            if (!repaint || callback == null) return;
            try { callback(); }
            catch (Exception error) { RepaintFailure = error; }
        }
    }
}
#endif
