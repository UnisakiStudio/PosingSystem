using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    // A wrapped Japanese or Chinese line must not begin with "。" or "、" or end with an opening bracket.
    public class PosingSystemTextKinsokuTests
    {
        private static readonly string[] Texts =
        {
            "生成時にアバターのBaseからモーションを検出します。取り込み設定の一覧には追加しません。手動設定がある種類は手動指定を優先します。",
            "「撮影モード」（オプション）を使うと、床の高さ、位置、向きを変えられます。対応していないポーズ（例：寝返り）は除きます。",
            "生成时会从头像的Base中检测动作，不会添加到导入设置列表。已手动设置的类型优先使用手动指定。",
            "プレビルド後の変更は、VRChat SDK 3.10.4以降でも「プレビルドを更新」で反映できます。",
        };

        private static GUIStyle Style() => new GUIStyle(EditorStyles.wordWrappedMiniLabel) { font = EditorStyles.standardFont, fontSize = 11, padding = new RectOffset() };

        // Editor styles and text measurement need a GUI event, as in the Inspector.
        private const System.Reflection.BindingFlags All = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Instance;
        private sealed class GuiScope : System.IDisposable
        {
            private readonly object state;
            private readonly Event previous;
            public GuiScope()
            {
                state = System.Activator.CreateInstance(typeof(GUIUtility).Assembly.GetType("UnityEngine.ObjectGUIState"), true);
                previous = Event.current;
                typeof(GUIUtility).GetMethod("BeginContainer", All).Invoke(null, new[] { state });
                Event.current = new Event { type = EventType.Repaint };
                typeof(GUIUtility).GetField("s_SkinMode", All).SetValue(null, 1);
                typeof(GUIUtility).GetMethod("ResetGlobalState", All).Invoke(null, null);
            }
            public void Dispose()
            {
                typeof(GUIUtility).GetMethod("EndContainer", All).Invoke(null, null);
                (state as System.IDisposable)?.Dispose();
                Event.current = previous;
            }
        }

        [Test]
        public void LinesKeepTheTextAndTheLineBreakRulesAtEveryWidth()
        {
            using var gui = new GuiScope();
            var style = Style();
            var broken = 0;
            foreach (var text in Texts)
                for (var width = 60; width <= 640; width += 7)
                {
                    var wrapped = PosingSystemTextKinsoku.Wrap(text, style, width);
                    // A space at a line end is dropped, as Unity does; nothing else may change.
                    Assert.That(wrapped.Replace("\n", "").Replace(" ", ""), Is.EqualTo(text.Replace(" ", "")), "only line ends may change, width " + width);
                    var lines = wrapped.Split('\n');
                    if (lines.Length > 1) broken++;
                    for (var i = 0; i < lines.Length; i++)
                    {
                        Assert.That(lines[i], Is.Not.Empty, text + " @" + width);
                        if (i > 0 && lines[i - 1].Length > 1)
                            Assert.That(PosingSystemTextKinsoku.NoLineStart.IndexOf(lines[i][0]), Is.LessThan(0), "line starts with '" + lines[i][0] + "' at width " + width + ": " + wrapped);
                        if (i < lines.Length - 1 && lines[i].Length > 1)
                            Assert.That(PosingSystemTextKinsoku.NoLineEnd.IndexOf(lines[i][lines[i].Length - 1]), Is.LessThan(0), "line ends with an opening bracket at width " + width + ": " + wrapped);
                        if (lines[i].Length > 1)
                            Assert.That(style.CalcSize(new GUIContent(lines[i])).x, Is.LessThanOrEqualTo(width), "line wider than the label at width " + width);
                    }
                    // Unity draws exactly these lines: it adds no break of its own.
                    var expected = style.CalcHeight(new GUIContent(string.Join("\n", lines.Select(_ => "あ"))), width);
                    Assert.That(style.CalcHeight(new GUIContent(wrapped), width), Is.EqualTo(expected).Within(.5f), "Unity wrapped again at width " + width + ": " + wrapped);
                    Assert.That(PosingSystemTextGUI.MeasureHeight(text, style, width), Is.EqualTo(expected).Within(.5f), "measured height at width " + width);
                }
            Assert.That(broken, Is.GreaterThan(100), "the widths must actually wrap the texts");
        }

        [Test]
        public void UnitysOwnWrappingBreaksTheRuleThatThisPrevents()
        {
            using var gui = new GuiScope();
            // Documents the defect: without explicit breaks the same label needs fewer or equal lines
            // somewhere only because punctuation was allowed to start a line.
            var style = Style();
            var text = Texts[0];
            var differs = Enumerable.Range(0, 84).Select(i => 60 + i * 7).Count(width => !ReferenceEquals(PosingSystemTextKinsoku.Wrap(text, style, width), text));
            Assert.That(differs, Is.GreaterThan(0));
        }

        [Test]
        public void OtherTextIsLeftToUnity()
        {
            using var gui = new GuiScope();
            var style = Style();
            foreach (var text in new[] { "Detects motions from the avatar's Base layer. Manual settings take priority.", "아바타의 Base에서 모션을 감지합니다. 수동 설정이 우선합니다.", "", null, "漢字だけで句読点なし" })
                Assert.That(PosingSystemTextKinsoku.Wrap(text, style, 120), Is.SameAs(text), text);
            var japanese = Texts[0];
            Assert.That(PosingSystemTextKinsoku.Wrap(japanese, new GUIStyle(style) { wordWrap = false }, 120), Is.SameAs(japanese), "single-line labels");
            Assert.That(PosingSystemTextKinsoku.Wrap(japanese, new GUIStyle(style) { richText = true }, 120), Is.SameAs(japanese), "rich text keeps Unity's wrapping");
            Assert.That(PosingSystemTextKinsoku.Wrap(japanese, style, 4000), Is.SameAs(japanese), "a label wide enough for one line");
        }
    }
}
