// Validation-only helper. No Unity or Skia references, no production rendering changes.
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

public static class GlyphClusterAudit
{
    const BindingFlags All=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Instance;
    public sealed class Issue
    {
        public int GlyphIndex,ClusterStart,ClusterEnd;
        public int[] CodePoints;
        public string RunKind;
        public bool AllowedControl;
        public override string ToString()
        {
            var values=Array.ConvertAll(CodePoints,cp=>"U+"+cp.ToString("X4"));
            return "GID0 glyph="+GlyphIndex+" absoluteUTF32=["+ClusterStart+","+ClusterEnd+") "+string.Join(" ",values)+" RunKind="+RunKind+" allowed="+AllowedControl;
        }
    }
    static object Get(object value,string name)=>value.GetType().GetField(name,All)?.GetValue(value)??value.GetType().GetProperty(name,All)?.GetValue(value);
    static int[] Ints(object value){var result=new List<int>();foreach(var item in (IEnumerable)value)result.Add(Convert.ToInt32(item));return result.ToArray();}
    static bool NonInkControl(int cp)=>cp==9||cp==10||cp==13||cp==0x85||cp==0x2028||cp==0x2029||
        cp==0x200b||cp==0x200c||cp==0x200d||cp==0x200e||cp==0x200f||cp>=0x202a&&cp<=0x202e||
        cp==0x2060||cp>=0x2066&&cp<=0x2069||cp==0xfeff;

    public static List<Issue> Inspect(object block)
    {
        var issues=new List<Issue>();
        foreach(var run in (IEnumerable)Get(block,"FontRuns"))
        {
            var start=(int)Get(run,"Start");var end=checked(start+(int)Get(run,"Length"));
            var cps=Ints(Get(run,"CodePoints"));var glyphs=Ints(Get(run,"Glyphs"));var clusters=Ints(Get(run,"Clusters"));
            if(glyphs.Length!=clusters.Length || cps.Length!=end-start)throw new InvalidOperationException("Glyph cluster audit schema mismatch");
            var boundaries=new SortedSet<int>(clusters);boundaries.Add(end);
            for(var g=0;g<glyphs.Length;g++)
            {
                if(glyphs[g]!=0)continue;
                var from=clusters[g];var to=end;
                // Clusters are absolute UTF32 offsets. Distinct sorted boundaries
                // handle descending RTL order and repeated ligature/mark clusters.
                foreach(var next in boundaries)if(next>from){to=next;break;}
                if(from<start || to>end || from>=to)throw new InvalidOperationException("GID0 has an invalid absolute UTF32 cluster");
                var source=new int[to-from];var controls=true;
                for(var cp=from;cp<to;cp++){source[cp-from]=cps[cp-start];controls&=NonInkControl(source[cp-from]);}
                var kind=Get(run,"RunKind").ToString();
                issues.Add(new Issue{GlyphIndex=g,ClusterStart=from,ClusterEnd=to,CodePoints=source,RunKind=kind,
                    // Pinned RichTextKit FontRun.Paint skips this run kind.
                    // A GID0 in a painted Normal run can draw .notdef even when
                    // its source is a control, so do not silently excuse it.
                    AllowedControl=controls && kind=="TrailingWhitespace"});
            }
        }
        return issues;
    }
}
