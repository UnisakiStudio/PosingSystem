using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    // Unity's own word wrap breaks Japanese and Chinese text between any two characters, so a
    // line can begin with "。" or "、" or end with an opening bracket. For wrapped labels whose
    // width is known, the lines are broken here instead and handed to Unity with explicit line
    // breaks. Text is never changed, only where its lines end.
    internal static class PosingSystemTextKinsoku
    {
        internal const string NoLineStart = "、。，．・：；？！）］｝〕〉》」』】…‥ー,.!?:;)]}";
        internal const string NoLineEnd = "（［｛〔〈《「『【([{";
        private const int Capacity = 512;

        private readonly struct Key : IEquatable<Key>
        {
            private readonly string text;
            private readonly int font, size, width;
            private readonly FontStyle style;
            public Key(string text, GUIStyle style, int width)
            {
                this.text = text; this.width = width; size = style.fontSize; this.style = style.fontStyle;
                font = style.font != null ? style.font.GetInstanceID() : 0;
            }
            public bool Equals(Key other) => width == other.width && size == other.size && font == other.font && style == other.style &&
                string.Equals(text, other.text, StringComparison.Ordinal);
            public override bool Equals(object obj) => obj is Key other && Equals(other);
            public override int GetHashCode() => (text.GetHashCode() * 397) ^ (width * 31 + size * 7 + font);
        }

        private static readonly Dictionary<Key, string> cache = new Dictionary<Key, string>();
        private static readonly GUIContent probe = new GUIContent();

        private static bool Ideograph(char c) => (c >= 0x3040 && c <= 0x30FF) || (c >= 0x4E00 && c <= 0x9FFF);
        private static bool Word(char c) => c < 0x80 && (char.IsLetterOrDigit(c) || c == '_');

        // Only text that can go wrong: Japanese or Chinese characters together with a character
        // that must not begin a line. Everything else keeps Unity's wrapping untouched.
        internal static bool Applies(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            bool ideograph = false, sensitive = false;
            foreach (var c in text)
            {
                if (!ideograph && Ideograph(c)) ideograph = true;
                if (!sensitive && NoLineStart.IndexOf(c) >= 0) sensitive = true;
                if (ideograph && sensitive) return true;
            }
            return false;
        }

        internal static string Wrap(string text, GUIStyle style, float width)
        {
            if (style == null || !style.wordWrap || style.richText || width < 24 || !Applies(text)) return text;
            var key = new Key(text, style, Mathf.FloorToInt(width));
            if (cache.TryGetValue(key, out var cached)) return cached;
            var result = Break(text, style, Mathf.Floor(width));
            if (cache.Count >= Capacity) cache.Clear();
            // Keep the caller's string when nothing changed, so that it can tell cheaply.
            cache[key] = result = result == text ? text : result;
            return result;
        }

        private static bool Fits(string paragraph, int start, int end, GUIStyle style, float width)
        {
            probe.text = paragraph.Substring(start, end - start);
            // One pixel in hand: Unity must not wrap a line again that was measured to fit.
            return style.CalcSize(probe).x <= width - 1;
        }

        private static string Break(string text, GUIStyle style, float width)
        {
            var output = new StringBuilder(text.Length + 8);
            var paragraphs = text.Split('\n');
            for (var p = 0; p < paragraphs.Length; p++)
            {
                if (p > 0) output.Append('\n');
                var paragraph = paragraphs[p];
                var start = 0;
                while (start < paragraph.Length)
                {
                    if (Fits(paragraph, start, paragraph.Length, style, width)) { output.Append(paragraph, start, paragraph.Length - start); break; }
                    // The longest prefix that fits (at least one character, so that the loop ends).
                    int low = start + 1, high = paragraph.Length - 1;
                    while (low < high)
                    {
                        var middle = (low + high + 1) / 2;
                        if (Fits(paragraph, start, middle, style, width)) low = middle; else high = middle - 1;
                    }
                    var end = low;
                    // Keep a Latin word or a number in one piece when the line has an earlier break.
                    if (end < paragraph.Length && Word(paragraph[end - 1]) && Word(paragraph[end]))
                    {
                        var word = end;
                        while (word > start + 1 && Word(paragraph[word - 1])) word--;
                        if (word > start + 1) end = word;
                    }
                    // Move characters to the next line until neither rule is broken.
                    for (var pass = 0; pass < 4; pass++)
                    {
                        var before = end;
                        while (end > start + 1 && end < paragraph.Length && NoLineStart.IndexOf(paragraph[end]) >= 0) end--;
                        while (end > start + 1 && NoLineEnd.IndexOf(paragraph[end - 1]) >= 0) end--;
                        if (end == before) break;
                    }
                    var lineEnd = end;
                    while (lineEnd > start && paragraph[lineEnd - 1] == ' ') lineEnd--;
                    output.Append(paragraph, start, lineEnd - start).Append('\n');
                    start = end;
                    while (start < paragraph.Length && paragraph[start] == ' ') start++;
                    if (start >= paragraph.Length) output.Length--; // nothing follows the last break
                }
            }
            return output.ToString();
        }
    }
}
