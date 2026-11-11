// Development stage only. Logical catalog strings are never rewritten.
using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using SkiaSharp;
using Topten.RichTextKit;
using TextAlignment = Topten.RichTextKit.TextAlignment;

namespace jp.unisakistudio.posingsystemeditor
{
    public static class PosingSystemTextRenderer
    {
        // The front boundary deliberately has no native objects or initialization.
        // Unity's own PluginImporter can load managed assemblies independently of us.
        public static Exception Failure { get; private set; }
        // Set by the product's Editor assembly; no product assembly reference is
        // required by this isolated backend. Values remain logical language codes.
        public static Func<string> LanguageProvider { get; set; }
        private static bool created;
        // Bad user content must not poison the optional renderer for unrelated
        // shipped labels. Retain fingerprints, never arbitrarily large names.
        private const int MaxContentFailures = 128;
        private static readonly Dictionary<string, LinkedListNode<string>> contentFailures = new Dictionary<string, LinkedListNode<string>>();
        private static readonly LinkedList<string> contentFailureOrder = new LinkedList<string>();
        private static readonly HashSet<string> contentDiagnostics = new HashSet<string>();

        public static void Reset() => Release();

        public static bool TryPrepare(string text, GUIStyle style, float width)
        {
            if (!Requires(text) || Failure != null || RejectedText(text)) return false;
            try { Get(text, style, Math.Max(1, width - style.padding.horizontal)); return true; }
            catch (Exception error) { Handle(error, text, null); return false; }
        }

        public static bool Requires(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            for (var i = 0; i < text.Length; i++)
            {
                var c = char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]) ? char.ConvertToUtf32(text, i) : text[i];
                if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1])) i++;
                if (Script(c) != null) return true;
            }
            return false;
        }

        public static float MeasureHeight(string text, GUIStyle style, float width)
        {
            if (!Requires(text) || Failure != null || RejectedText(text)) return style.CalcHeight(new GUIContent(text), width);
            try { return Get(text, style, Math.Max(1, width - style.padding.horizontal)).Block.MeasuredHeight + style.padding.vertical; }
            catch (Exception error) { Handle(error, text, null); return style.CalcHeight(new GUIContent(text), width); }
        }

        public static float MeasureWidth(string text, GUIStyle style)
        {
            if (!Requires(text) || Failure != null || RejectedText(text)) return style.CalcSize(new GUIContent(text)).x;
            try { return Get(text, style, null).Block.MeasuredWidth + style.padding.horizontal; }
            catch (Exception error) { Handle(error, text, null); return style.CalcSize(new GUIContent(text)).x; }
        }

        public static void Draw(Rect rect, GUIContent content, GUIStyle style)
        {
            if (Event.current == null || Event.current.type != EventType.Repaint || string.IsNullOrEmpty(content?.text)) return;
            if (!Requires(content.text) || Failure != null || RejectedText(content.text)) { style.Draw(rect, new GUIContent(content.text), false, false, false, false); return; }
            var rasterFailure = contentFailures.Count == 0 ? null : RasterFailureKey(content.text, rect, style);
            if (rasterFailure != null && Rejected(rasterFailure)) { style.Draw(rect, new GUIContent(content.text), false, false, false, false); return; }
            try
            {
                var area = style.padding.Remove(rect);
                area.position += style.contentOffset;
                if (area.width <= 0 || area.height <= 0) return;
                var entry = Get(content.text, style, area.width);
                Backend.Draw(entry, area, style);
            }
            catch (Exception error)
            {
                Handle(error, content.text, rasterFailure ?? RasterFailureKey(content.text, rect, style));
                style.Draw(rect, new GUIContent(content.text), false, false, false, false);
            }
        }

        private static string Fingerprint(string text)
        {
            // Hash exact UTF16 code units in bounded chunks, including malformed
            // custom names; do not allocate a second copy of an over-limit string.
            using (var hash = SHA256.Create())
            {
                var bytes = new byte[2048];
                for (var offset = 0; offset < text.Length;)
                {
                    var count = Math.Min(1024, text.Length - offset);
                    for (var i = 0; i < count; i++)
                    {
                        var value = text[offset + i];
                        bytes[i * 2] = (byte)value; bytes[i * 2 + 1] = (byte)(value >> 8);
                    }
                    hash.TransformBlock(bytes, 0, count * 2, bytes, 0); offset += count;
                }
                hash.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                return BitConverter.ToString(hash.Hash).Replace("-", "");
            }
        }
        private static string TextFailureKey(string text) => "text/" + Fingerprint(text);
        private static string RasterFailureKey(string text, Rect rect, GUIStyle style)
            => "raster/" + Fingerprint(text) + "/" + rect.width.ToString("R", CultureInfo.InvariantCulture) + "/" + rect.height.ToString("R", CultureInfo.InvariantCulture)
                + "/" + style.padding.left + "/" + style.padding.right + "/" + style.padding.top + "/" + style.padding.bottom
                + "/" + style.fontSize + "/" + style.fontStyle + "/" + style.wordWrap + "/" + style.richText + "/" + style.alignment + "/" + style.clipping
                + "/" + style.font?.GetInstanceID() + "/" + EditorGUIUtility.pixelsPerPoint.ToString("R", CultureInfo.InvariantCulture);
        private static bool RejectedText(string text) => contentFailures.Count != 0 && Rejected(TextFailureKey(text));
        private static bool Rejected(string key)
        {
            if (!contentFailures.TryGetValue(key, out var node)) return false;
            contentFailureOrder.Remove(node); contentFailureOrder.AddLast(node); return true;
        }
        private static void Handle(Exception error, string text, string rasterKey)
        {
            var code = error is InvalidOperationException ? error.Message.Split(':')[0] : "";
            if (code != "USSPS_TEXT_GLYPH_UNAVAILABLE" && code != "USSPS_TEXT_LENGTH_LIMIT" && code != "USSPS_TEXT_RASTER_LIMIT" && code != "USSPS_TEXT_FONT_LIMIT")
            { Fail(error); return; }
            var key = code == "USSPS_TEXT_RASTER_LIMIT" && rasterKey != null ? rasterKey : TextFailureKey(text);
            if (!Rejected(key)) contentFailures.Add(key, contentFailureOrder.AddLast(key));
            while (contentFailures.Count > MaxContentFailures)
            {
                contentFailures.Remove(contentFailureOrder.First.Value); contentFailureOrder.RemoveFirst();
            }
            // A repeated or evicted bad name must never spam the console. These
            // four stable diagnostic categories are reset only with the backend.
            if (contentDiagnostics.Add(code)) Debug.LogWarning(error.Message + " | native text fallback for this content.");
        }

        private static void Fail(Exception error)
        {
            if (Failure != null) return;
            Failure = error is InvalidOperationException && error.Message.StartsWith("USSPS_TEXT_") ? error
                : new InvalidOperationException("USSPS_TEXT_RENDER_FAILED: " + error.Message, error);
            // A partially initialized dependency must not let cleanup replace the
            // original diagnostic or escape through the native-drawing fallback.
            if (created)
                try { Backend.Release(); }
                catch (Exception cleanup) { Failure.Data["Cleanup"] = cleanup.ToString(); }
            Debug.LogException(Failure); // One diagnostic per domain, never one per Repaint.
        }

        private static Backend.Entry Get(string text, GUIStyle style, float? width)
        {
            if (text.Length > 32768) throw new InvalidOperationException("USSPS_TEXT_LENGTH_LIMIT: " + text.Length);
            if (!created)
            {
                created = true;
                AssemblyReloadEvents.beforeAssemblyReload += Release;
                EditorApplication.quitting += Release;
            }
            return Backend.Get(text, style, width);
        }

        internal static void Release()
        {
            try { if (created) Backend.Release(); }
            finally
            {
                created = false; Failure = null;
                contentFailures.Clear(); contentFailureOrder.Clear(); contentDiagnostics.Clear();
                AssemblyReloadEvents.beforeAssemblyReload -= Release;
                EditorApplication.quitting -= Release;
            }
        }

        private static string Script(int c)
        {
            if (c >= 0x590 && c <= 0x5ff || c >= 0xfb1d && c <= 0xfb4f) return "NotoSansHebrew";
            if (c >= 0x600 && c <= 0x6ff || c >= 0x750 && c <= 0x77f || c >= 0x870 && c <= 0x8ff || c >= 0xfb50 && c <= 0xfdff || c >= 0xfe70 && c <= 0xfefc) return "NotoSansArabic";
            if (c >= 0x900 && c <= 0x97f || c >= 0xa8e0 && c <= 0xa8ff || c >= 0x11b00 && c <= 0x11b5f) return "NotoSansDevanagari";
            if (c >= 0x980 && c <= 0x9ff) return "NotoSansBengali";
            if (c >= 0xb80 && c <= 0xbff || c >= 0x11fc0 && c <= 0x11fff) return "NotoSansTamil";
            if (c >= 0xc00 && c <= 0xc7f) return "NotoSansTelugu";
            if (c >= 0xe00 && c <= 0xe7f) return "NotoSansThai";
            return null;
        }

        private static class Backend
        {
            private const int MaxEntries = 128, MaxFallbackFaces = 64, MaxPixels = 4194304;
            private const long MaxTextureBytes = 24 * 1024 * 1024;
            // Skia ReadPixels is top-down; Unity raw Texture2D rows start at
            // the bottom. Flip only sampling, leaving raster bytes/layout intact.
            private static readonly Rect TextureCoordinates = new Rect(0, 1, 1, -1);
            private static readonly Dictionary<string, Entry> entries = new Dictionary<string, Entry>();
            private static readonly LinkedList<string> lru = new LinkedList<string>();
            private static readonly Regex tags = new Regex(@"<\s*(/?)\s*(b|i|size|color|sup|sub|br|material|quad)\b([^>]*)>", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            private static Fonts fonts;
            private static long textureBytes;
            private static string environment;
            private static PropertyInfo language;
            // RichTextKit 0.6.170 has no public cache eviction API. These narrowly
            // scoped handles are version-pinned by the dependency SHA manifest.
            private static readonly FieldInfo shaperCache = typeof(TextBlock).Assembly.GetType("Topten.RichTextKit.TextShaper")?.GetField("_shapers", BindingFlags.NonPublic | BindingFlags.Static);
            private static readonly FieldInfo runFont = typeof(FontRun).GetField("_font", BindingFlags.NonPublic | BindingFlags.Instance);
            private static readonly FieldInfo runBlob = typeof(FontRun).GetField("_textBlob", BindingFlags.NonPublic | BindingFlags.Instance);

            [Serializable] private sealed class Manifest { public Dependency[] files; }
            [Serializable] private sealed class Dependency { public string file, sha256, os; }
            private sealed class Format
            {
                public bool Bold, Italic;
                public float Size;
                public Color Color;
                public FontVariant Variant;
                public Format Copy() => (Format)MemberwiseClone();
            }
            internal sealed class Entry
            {
                public TextBlock Block;
                public LinkedListNode<string> Node;
                public Texture2D Texture;
                public string RasterKey;
                public Vector2 RasterOffset, RasterSize;
                public long Bytes;
            }
            private sealed class Fonts : FontMapper, IDisposable
            {
                private readonly Dictionary<string, SKTypeface> faces = new Dictionary<string, SKTypeface>();
                private readonly Dictionary<string, string> matching = new Dictionary<string, string>();
                private readonly List<SKTypeface> fallback = new List<SKTypeface>();
                public Fonts(string root)
                {
                    try
                    {
                        foreach (var family in new[] { "NotoSans", "NotoSansArabic", "NotoSansHebrew", "NotoSansDevanagari", "NotoSansBengali", "NotoSansTamil", "NotoSansTelugu", "NotoSansThai" })
                            foreach (var weight in new[] { "Regular", "Bold" })
                            {
                                var path = Path.Combine(root, "Fonts", family + "-" + weight + ".ttf");
                                var face = SKTypeface.FromFile(path);
                                if (face == null) throw new InvalidOperationException("USSPS_TEXT_FONT_MISSING: " + path);
                                faces.Add(family + "/" + weight, face);
                            }
                    }
                    catch { Dispose(); throw; }
                }
                public override SKTypeface TypefaceFromStyle(IStyle style, bool ignoreFontVariants) => faces[style.FontFamily];
                public string Select(string family, int codepoint, bool bold, bool italic)
                {
                    var key = family + "/" + (bold ? "Bold" : "Regular");
                    // Controls/formatting marks belong to the surrounding run and need no visible glyph.
                    var category = CharUnicodeInfo.GetUnicodeCategory(char.ConvertFromUtf32(codepoint), 0);
                    if (category == UnicodeCategory.Control || category == UnicodeCategory.Format || category == UnicodeCategory.NonSpacingMark || category == UnicodeCategory.SpacingCombiningMark || category == UnicodeCategory.EnclosingMark) return key;
                    using (var font = new SKFont(faces[key])) if (font.GetGlyph(codepoint) != 0) return key;
                    var query = codepoint + "/" + bold + "/" + italic;
                    if (matching.TryGetValue(query, out var cached)) return cached;
                    var match = SKFontManager.Default.MatchCharacter(null, bold ? 700 : 400, 5, italic ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright, null, codepoint);
                    if (match == null) throw new InvalidOperationException("USSPS_TEXT_GLYPH_UNAVAILABLE: U+" + codepoint.ToString("X"));
                    var existing = fallback.FindIndex(x => x.Handle == match.Handle);
                    string mapped;
                    if (existing >= 0) { if (!ReferenceEquals(match, fallback[existing])) match.Dispose(); mapped = "fallback/" + existing; }
                    else
                    {
                        if (fallback.Count >= MaxFallbackFaces) { match.Dispose(); throw new InvalidOperationException("USSPS_TEXT_FONT_LIMIT: " + MaxFallbackFaces); }
                        mapped = "fallback/" + fallback.Count; fallback.Add(match); faces.Add(mapped, match);
                    }
                    if (matching.Count < 2048) matching[query] = mapped;
                    return mapped;
                }
                public void Dispose()
                {
                    // Remove only our faces, never another consumer's global cache.
                    var cache = shaperCache?.GetValue(null) as IDictionary;
                    if (cache != null) lock (cache)
                        foreach (var face in faces.Values)
                            if (cache.Contains(face)) { (cache[face] as IDisposable)?.Dispose(); cache.Remove(face); }
                    foreach (var face in faces.Values) face.Dispose();
                    faces.Clear(); fallback.Clear(); matching.Clear();
                }
            }

            private static string SourceFile([CallerFilePath] string source = "") => source;
            private static string Root => Path.GetDirectoryName(Path.GetFullPath(SourceFile()));
            private static string Hash(string path)
            {
                using (var stream = File.OpenRead(path)) using (var sha = SHA256.Create())
                    return BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
            }
            private static string Host => Application.platform == RuntimePlatform.WindowsEditor ? "Windows" : Application.platform == RuntimePlatform.OSXEditor ? "OSX" : "Linux";

            private static void Initialize()
            {
                if (fonts != null) return;
                if (shaperCache == null || runFont == null || runBlob == null) throw new InvalidOperationException("USSPS_TEXT_RESOURCE_SCHEMA: RichTextKit 0.6.170 expected");
                var manifestPath = Path.Combine(Root, "complex-text-dependencies.json");
                if (!File.Exists(manifestPath)) throw new InvalidOperationException("USSPS_TEXT_MANIFEST_MISSING: " + manifestPath);
                var manifest = JsonUtility.FromJson<Manifest>(File.ReadAllText(manifestPath));
                var expected = new Dictionary<string, Dependency>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in manifest.files.Where(x => string.IsNullOrEmpty(x.os) || x.os == Host))
                {
                    var path = Path.Combine(Root, item.file);
                    if (!File.Exists(path) || Hash(path) != item.sha256) throw new InvalidOperationException("USSPS_TEXT_DEPENDENCY_INVALID: " + path);
                    if (Path.GetExtension(path) == ".dll" || item.os == Host) expected[Path.GetFileName(path)] = item;
                }
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    var name = assembly.FullName.Split(',')[0] + ".dll";
                    if (!expected.TryGetValue(name, out var dependency)) continue;
                    string location;
                    try { location = assembly.Location; } catch (Exception error) { throw new InvalidOperationException("USSPS_TEXT_ASSEMBLY_LOCATION: " + name, error); }
                    if (string.IsNullOrEmpty(location) || !File.Exists(location) || Hash(location) != dependency.sha256)
                        throw new InvalidOperationException("USSPS_TEXT_ASSEMBLY_CONFLICT: " + name + " | " + location);
                }
                // Inspect enabled assets, not other-OS binaries. Never replace another tool's DLL.
                foreach (var asset in AssetDatabase.GetAllAssetPaths())
                {
                    if (!expected.TryGetValue(Path.GetFileName(asset), out var dependency)) continue;
                    var importer = AssetImporter.GetAtPath(asset) as PluginImporter;
                    if (importer == null || !importer.GetCompatibleWithEditor()) continue;
                    var os = importer.GetEditorData("OS");
                    if (!string.IsNullOrEmpty(os) && os != "AnyOS" && os != Host) continue;
                    var resolved = Path.GetFullPath(asset);
                    if (!File.Exists(resolved))
                    {
                        var package = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(asset);
                        if (package != null) resolved = Path.Combine(package.resolvedPath, asset.Substring(package.assetPath.Length).TrimStart('/'));
                    }
                    if (!File.Exists(resolved) || Hash(resolved) != dependency.sha256)
                        throw new InvalidOperationException("USSPS_TEXT_PLUGIN_CONFLICT: " + asset);
                }
                // A native module may already have been loaded outside AssetDatabase.
                using (var process = System.Diagnostics.Process.GetCurrentProcess())
                    foreach (System.Diagnostics.ProcessModule module in process.Modules)
                        if (expected.TryGetValue(Path.GetFileName(module.FileName), out var dependency) && !string.IsNullOrEmpty(dependency.os) && Hash(module.FileName) != dependency.sha256)
                            throw new InvalidOperationException("USSPS_TEXT_NATIVE_CONFLICT: " + module.FileName);
                if (SkiaSharpVersion.Native.ToString() != "153.0") throw new InvalidOperationException("USSPS_TEXT_NATIVE_VERSION: " + SkiaSharpVersion.Native);
                fonts = new Fonts(Root);
            }

            private static string Language()
            {
                if (LanguageProvider != null) return LanguageProvider() ?? "auto";
                // Compatibility for source-only proofs and integrations before
                // their explicit bridge is wired. Product and backend may use
                // different asmdefs, so search loaded assemblies rather than ours.
                if (language == null)
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        language = assembly.GetType("jp.unisakistudio.posingsystemeditor.PosingSystemLocalization")?.GetProperty("Language", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                        if (language != null) break;
                    }
                return language?.GetValue(null) as string ?? "auto";
            }
            private static float FontSize(GUIStyle style) => style.fontSize > 0 ? style.fontSize : style.font != null && style.font.fontSize > 0 ? style.font.fontSize : 12;
            private static string Number(float value) => value.ToString("R", CultureInfo.InvariantCulture);
            private static Color Tint => GUI.color * GUI.contentColor * new Color(1, 1, 1, GUI.enabled ? 1 : .5f);
            private static string ColorKey(Color color) => ColorUtility.ToHtmlStringRGBA(color);

            public static Entry Get(string text, GUIStyle style, float? width)
            {
                Initialize();
                if (text.Length > 32768) throw new InvalidOperationException("USSPS_TEXT_LENGTH_LIMIT: " + text.Length);
                var lang = Language();
                var env = lang + "/" + EditorGUIUtility.isProSkin + "/" + Number(EditorGUIUtility.pixelsPerPoint);
                if (environment != env) { ClearEntries(); environment = env; }
                var key = text + "\u001f" + (width.HasValue ? Number(width.Value) : "natural") + "/" + Number(FontSize(style)) + "/" + style.fontStyle + "/" + style.wordWrap + "/" + style.richText + "/" + style.alignment + "/" + ColorKey(style.normal.textColor) + "/" + style.font?.GetInstanceID();
                if (entries.TryGetValue(key, out var found)) { lru.Remove(found.Node); lru.AddLast(found.Node); return found; }
                var block = Build(text, style, width, lang);
                var entry = new Entry { Block = block, Node = lru.AddLast(key) };
                entries.Add(key, entry); Trim(entry); return entry;
            }

            private static TextBlock Build(string text, GUIStyle gui, float? width, string lang)
            {
                var horizontal = (int)gui.alignment % 3;
                var block = new TextBlock { FontMapper = fonts, MaxWidth = gui.wordWrap ? width : null,
                    RenderWidth = width, Alignment = horizontal == 1 ? TextAlignment.Center : horizontal == 2 ? TextAlignment.Right : TextAlignment.Left,
                    BaseDirection = lang == "ar" || lang == "he" ? TextDirection.RTL : TextDirection.Auto };
                var format = new Format { Size = FontSize(gui), Bold = gui.fontStyle == FontStyle.Bold || gui.fontStyle == FontStyle.BoldAndItalic,
                    Italic = gui.fontStyle == FontStyle.Italic || gui.fontStyle == FontStyle.BoldAndItalic, Color = gui.normal.textColor };
                var stack = new List<KeyValuePair<string, Format>>();
                var position = 0;
                try
                {
                    if (!gui.richText) { Add(block, text, format); block.Layout(); return block; }
                    foreach (Match match in tags.Matches(text))
                    {
                        Add(block, text.Substring(position, match.Index - position), format);
                        var tag = match.Groups[2].Value.ToLowerInvariant(); var closing = match.Groups[1].Length != 0;
                        var argument = match.Groups[3].Value.Trim().TrimStart('=').Trim().Trim('"', '\'');
                        if (tag == "br") Add(block, "\n", format);
                        else if (gui.richText && tag != "material" && tag != "quad")
                        {
                            if (closing)
                            {
                                var index = stack.FindLastIndex(x => x.Key == tag);
                                if (index >= 0) { format = stack[index].Value; stack.RemoveRange(index, stack.Count - index); }
                            }
                            else
                            {
                                stack.Add(new KeyValuePair<string, Format>(tag, format.Copy()));
                                if (tag == "b") format.Bold = true;
                                else if (tag == "i") format.Italic = true;
                                else if (tag == "size" && float.TryParse(argument, NumberStyles.Float, CultureInfo.InvariantCulture, out var size)) format.Size = Mathf.Clamp(size, 1, 128);
                                else if (tag == "color" && ColorUtility.TryParseHtmlString(argument, out var color)) format.Color = color;
                                else if (tag == "sup") format.Variant = FontVariant.SuperScript;
                                else if (tag == "sub") format.Variant = FontVariant.SubScript;
                            }
                        }
                        position = match.Index + match.Length;
                    }
                    Add(block, text.Substring(position), format); block.Layout(); return block;
                }
                catch { block.Clear(); throw; }
            }

            private static void Add(TextBlock block, string text, Format format)
            {
                string runFamily = null; var start = 0; var priorScript = "NotoSans";
                for (var i = 0; i < text.Length;)
                {
                    // Custom names can contain an isolated UTF-16 surrogate.
                    // Display a replacement glyph without disabling the entire
                    // renderer; the original catalog/name is never mutated.
                    if (char.IsSurrogate(text[i]) && !(char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1])))
                        text = text.Substring(0, i) + "\ufffd" + text.Substring(i + 1);
                    var cp = char.ConvertToUtf32(text, i); var count = cp > 0xffff ? 2 : 1;
                    var category = CharUnicodeInfo.GetUnicodeCategory(text, i);
                    var combining = category == UnicodeCategory.NonSpacingMark || category == UnicodeCategory.SpacingCombiningMark || category == UnicodeCategory.EnclosingMark || cp == 0x200c || cp == 0x200d;
                    var script = Script(cp) ?? (combining ? priorScript : "NotoSans");
                    var family = fonts.Select(script, cp, format.Bold, format.Italic);
                    if (runFamily != family)
                    {
                        if (i > start) block.AddText(text.Substring(start, i - start), Style(runFamily, format));
                        runFamily = family; start = i;
                    }
                    priorScript = script; i += count;
                }
                if (start < text.Length) block.AddText(text.Substring(start), Style(runFamily, format));
            }
            private static Style Style(string family, Format format) => new Style { FontFamily = family, FontSize = format.Size, FontWeight = format.Bold ? 700 : 400,
                FontItalic = format.Italic, FontVariant = format.Variant, TextColor = new SKColor((byte)Mathf.RoundToInt(format.Color.r * 255), (byte)Mathf.RoundToInt(format.Color.g * 255), (byte)Mathf.RoundToInt(format.Color.b * 255), (byte)Mathf.RoundToInt(format.Color.a * 255)) };

            public static void Draw(Entry entry, Rect area, GUIStyle style)
            {
                var dpi = Mathf.Max(1, EditorGUIUtility.pixelsPerPoint);
                var tint = Tint;
                var rasterKey = Number(area.height) + "/" + Number(dpi) + "/" + style.alignment + "/" + style.clipping + "/" + ColorKey(tint);
                if (entry.Texture == null || entry.RasterKey != rasterKey)
                {
                    DropTexture(entry);
                    var raw = Raster(entry.Block, area.size, style, dpi, tint);
                    entry.Texture = new Texture2D(raw.Width, raw.Height, TextureFormat.RGBA32, false, false) { hideFlags = HideFlags.HideAndDontSave, filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                    entry.Texture.LoadRawTextureData(raw.Bytes); entry.Texture.Apply(false, true);
                    entry.Bytes = raw.Bytes.LongLength; textureBytes += entry.Bytes;
                    entry.RasterOffset = raw.Offset; entry.RasterSize = new Vector2(raw.Width / dpi, raw.Height / dpi); entry.RasterKey = rasterKey;
                    Trim(entry);
                }
                var color = GUI.color; var content = GUI.contentColor; var enabled = GUI.enabled;
                try
                {
                    GUI.color = Color.white; GUI.contentColor = Color.white; GUI.enabled = true;
                    GUI.DrawTextureWithTexCoords(new Rect(area.position + entry.RasterOffset, entry.RasterSize), entry.Texture, TextureCoordinates, true);
                }
                finally { GUI.color = color; GUI.contentColor = content; GUI.enabled = enabled; }
            }

            internal sealed class RasterData { public byte[] Bytes; public int Width, Height; public Vector2 Offset; }
            private static float PrepareItalicFonts(TextBlock block)
            {
                var overhang = 0f;
                // The bundled families are upright Regular/Bold. RichTextKit
                // 0.6.170 does not synthesize an oblique face. Apply the same
                // skew to its cached SKFont for painting; advance widths and
                // wrapping remain the measured shaped layout.
                foreach (var run in block.FontRuns)
                {
                    if (!run.Style.FontItalic || run.Typeface.IsItalic) continue;
                    var font = runFont.GetValue(run) as SKFont;
                    if (font == null)
                    {
                        var scale = run.Style.FontVariant == FontVariant.SuperScript || run.Style.FontVariant == FontVariant.SubScript ? .65f : 1;
                        font = new SKFont(run.Typeface, run.Style.FontSize * scale);
                        runFont.SetValue(run, font);
                    }
                    font.SkewX = -.2f;
                    overhang = Math.Max(overhang, run.Style.FontSize * .25f);
                }
                return overhang;
            }
            private static RasterData Raster(TextBlock block, Vector2 size, GUIStyle style, float dpi, Color tint)
            {
                var clipped = style.clipping == TextClipping.Clip;
                var italicOverhang = PrepareItalicFonts(block);
                var overhang = block.MeasuredOverhang;
                var x = 0f; var y = 0f;
                var width = size.x; var height = size.y;
                var vertical = (int)style.alignment / 3;
                var textY = vertical == 1 ? (size.y - block.MeasuredHeight) / 2 : vertical == 2 ? size.y - block.MeasuredHeight : 0;
                if (!clipped)
                {
                    var align = (int)style.alignment % 3;
                    var overflow = Math.Max(0, block.MeasuredWidth - size.x);
                    x = -Math.Max(0, overhang.Left) - italicOverhang - (align == 2 ? overflow : align == 1 ? overflow / 2 : 0);
                    y = Math.Min(0, textY) - Math.Max(0, overhang.Top);
                    width = Math.Max(size.x, block.MeasuredWidth) + Math.Max(0, overhang.Left) + Math.Max(0, overhang.Right) + italicOverhang * 2;
                    height = Math.Max(size.y, Math.Max(0, textY) + block.MeasuredHeight) - y + Math.Max(0, overhang.Bottom);
                }
                // Check floating-point dimensions before narrowing to int; huge
                // values/Infinity must not wrap to a tiny accepted allocation.
                var physicalWidth = Math.Ceiling((double)width * dpi); var physicalHeight = Math.Ceiling((double)height * dpi);
                if (double.IsNaN(physicalWidth) || double.IsNaN(physicalHeight) || physicalWidth > 8192 || physicalHeight > 8192 || physicalWidth < 0 || physicalHeight < 0)
                    throw new InvalidOperationException("USSPS_TEXT_RASTER_LIMIT: " + physicalWidth + "x" + physicalHeight);
                var pixelsX = Math.Max(1, (int)physicalWidth); var pixelsY = Math.Max(1, (int)physicalHeight);
                if ((long)pixelsX * pixelsY > MaxPixels) throw new InvalidOperationException("USSPS_TEXT_RASTER_LIMIT: " + pixelsX + "x" + pixelsY);
                using (var surface = SKSurface.Create(new SKImageInfo(pixelsX, pixelsY, SKColorType.Rgba8888, SKAlphaType.Premul)))
                using (var bitmap = new SKBitmap(new SKImageInfo(pixelsX, pixelsY, SKColorType.Rgba8888, SKAlphaType.Unpremul)))
                {
                    if (surface == null) throw new InvalidOperationException("USSPS_TEXT_SURFACE_FAILED");
                    surface.Canvas.Clear(SKColors.Transparent); surface.Canvas.Scale(dpi); surface.Canvas.Translate(-x, -y + textY); block.Paint(surface.Canvas);
                    if (!surface.ReadPixels(bitmap.Info, bitmap.GetPixels(), bitmap.RowBytes, 0, 0)) throw new InvalidOperationException("USSPS_TEXT_PIXELS_FAILED");
                    var bytes = bitmap.Bytes;
                    // Skia stores premultiplied colors; ReadPixels above explicitly converts
                    // to straight RGBA so Unity's ordinary alpha blend does not darken edges.
                    for (var i = 0; i < bytes.Length; i += 4)
                    {
                        bytes[i] = (byte)Mathf.Clamp(Mathf.RoundToInt(bytes[i] * tint.r), 0, 255);
                        bytes[i + 1] = (byte)Mathf.Clamp(Mathf.RoundToInt(bytes[i + 1] * tint.g), 0, 255);
                        bytes[i + 2] = (byte)Mathf.Clamp(Mathf.RoundToInt(bytes[i + 2] * tint.b), 0, 255);
                        bytes[i + 3] = (byte)Mathf.Clamp(Mathf.RoundToInt(bytes[i + 3] * tint.a), 0, 255);
                    }
                    return new RasterData { Bytes = bytes, Width = pixelsX, Height = pixelsY, Offset = new Vector2(x, y) };
                }
            }

            private static void DropTexture(Entry entry)
            {
                if (entry.Texture != null) UnityEngine.Object.DestroyImmediate(entry.Texture);
                entry.Texture = null; textureBytes -= entry.Bytes; entry.Bytes = 0;
            }
            private static void Trim(Entry keep)
            {
                while (entries.Count > MaxEntries || textureBytes > MaxTextureBytes)
                {
                    var oldest = lru.First; if (oldest == null || oldest == keep.Node) break;
                    var entry = entries[oldest.Value]; DropTexture(entry); ClearBlock(entry.Block); entries.Remove(oldest.Value); lru.RemoveFirst();
                }
            }
            private static void ClearEntries()
            {
                foreach (var entry in entries.Values) { DropTexture(entry); ClearBlock(entry.Block); }
                entries.Clear(); lru.Clear(); textureBytes = 0;
            }
            private static void ClearBlock(TextBlock block)
            {
                // Clear() returns FontRun objects to a pool; explicitly dispose
                // their GPU/Skia paint resources before the pool nulls the fields.
                foreach (var run in block.FontRuns)
                {
                    (runFont.GetValue(run) as IDisposable)?.Dispose(); runFont.SetValue(run, null);
                    (runBlob.GetValue(run) as IDisposable)?.Dispose(); runBlob.SetValue(run, null);
                }
                block.Clear();
            }
            public static void Release()
            {
                try { ClearEntries(); }
                finally
                {
                    try { fonts?.Dispose(); }
                    finally { fonts = null; environment = null; language = null; }
                }
            }
        }
    }
}
