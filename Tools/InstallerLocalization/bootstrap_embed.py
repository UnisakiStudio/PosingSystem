"""Pure single-source embedding candidate for the standalone setup generator.

Reads reviewed fixed-caption assets; returns C# without writing product files.
The generated helper already lives under Editor and intentionally compiles in the
standalone builder even when UNITY_EDITOR itself is not defined.
"""
from pathlib import Path
import hashlib
import json
import re

LANGUAGES = ('ar','he','hi','bn','ta','te','th')
REMEMBER = 'This language is remembered for other Unity projects on this computer.'


def sha(value): return hashlib.sha256(value).hexdigest()


def dictionary(source, method):
    body=re.search(r'private static Dictionary<string, string> '+re.escape(method)+r'\(\) => new Dictionary<string, string> \{(.*?)\n\};',source,re.S)
    if not body: raise ValueError('Missing generated dictionary: '+method)
    literal=r'("(?:[^"\\]|\\.)*")'
    return {json.loads(m[1]):json.loads(m[2]) for m in re.finditer(r'\{ '+literal+r', '+literal+r' \}',body[1])}


def picker_span(source):
    start=source.index('        private sealed class LanguagePicker : EditorWindow')
    brace=source.index('{',start);end=brace+1;depth=1
    # This fixed class currently has no brace-containing string literals. Fail
    # review instead of silently patching a differently structured future class.
    while depth:
        if source[end]=='{':depth+=1
        if source[end]=='}':depth-=1
        end+=1
    if source[start:end].count('private void OnGUI()')!=1:
        raise ValueError('Unexpected LanguagePicker structure')
    return start,end


def validate_captions(helper, assets):
    assets=Path(assets)
    manifest=json.loads((assets/'caption-manifest.json').read_text(encoding='utf-8-sig'))
    for name,expected in manifest['files'].items():
        if Path(name).name!=name:raise ValueError('Bootstrap manifest paths must be simple filenames')
        if sha((assets/name).read_bytes())!=expected:raise ValueError('Reviewed bootstrap asset changed: '+name)
    captions=manifest['captions']
    if len(captions)!=21 or {r['locale'] for r in captions}!=set(LANGUAGES):
        raise ValueError('Expected exactly 21 reviewed captions in seven languages')
    codes=re.findall(r'"([\w-]+)"',re.search(r'Codes\s*=\s*\{([^}]+)\}',helper)[1])
    names=re.findall(r'"([^"]+)"',re.search(r'Names\s*=\s*\{([^}]+)\}',helper)[1])
    aliases=dictionary(helper,'BuildAliases')
    for code in LANGUAGES:
        rows=[r for r in captions if r['locale']==code]
        if {r['role'] for r in rows}!={'autonym','help','apply'}:raise ValueError('Caption roles incomplete: '+code)
        catalog=dictionary(helper,'Build_'+code)
        for row in rows:
            key=REMEMBER if row['role']=='help' else 'Apply'
            value=names[codes.index(code)] if row['role']=='autonym' else catalog.get(aliases.get(key,key),aliases.get(key,key))
            if value!=row['text'] or sha(value.encode('utf-8'))!=row['textSha256']:
                raise ValueError('Caption translation changed; regenerate and review before embedding: '+code+'/'+row['role'])
            if row['scale']!=2 or row['zeroGlyphs']!=0 or row['whiteFailures']!=0:
                raise ValueError('Unverified bootstrap raster: '+code+'/'+row['role'])
    if any(r['requiredHeight']>220 or r['minWidth']!=380 or r['helpWrapWidth']!=300 for r in manifest['layout']):
        raise ValueError('Bootstrap layout exceeds the reviewed picker budget')
    return manifest


def namespace_body(text):
    # C# using directives cannot be appended after an existing namespace. The
    # helper's common using block already includes System/collections/UnityEditor/
    # UnityEngine; the two new BCL references are fully qualified below.
    start=text.index('namespace UnisakiStudio.Setup')
    result=text[start:].rstrip()
    if result.endswith('#endif'):result=result[:-len('#endif')].rstrip()
    if re.search(r'^\s*#(?:if|elif|else|endif)',result,re.M):
        raise ValueError('Unexpected nested build guard in bootstrap source')
    return result


def embed(helper, assets):
    """Return the augmented helper; preserves all existing APIs/dialog/menu code."""
    assets=Path(assets);manifest=validate_captions(helper,assets)
    if 'internal static partial class BootstrapCaptionImages' in helper:
        raise ValueError('Expected the base generated helper, not an already embedded output')
    start,end=picker_span(helper)
    picker=(assets/'LanguagePicker.cs.in').read_text(encoding='utf-8-sig').rstrip()
    runtime=namespace_body((assets/'CaptionImages.cs.in').read_text(encoding='utf-8-sig'))
    runtime=re.sub(r'\bSHA256\.Create', 'System.Security.Cryptography.SHA256.Create',runtime)
    runtime=re.sub(r'\bEncoding\.UTF8', 'System.Text.Encoding.UTF8',runtime)
    runtime=re.sub(r'\bObject\.DestroyImmediate', 'UnityEngine.Object.DestroyImmediate',runtime)
    data=namespace_body((assets/'CaptionImages.Data.cs.in').read_text(encoding='utf-8-sig'))
    output=helper[:start]+picker+helper[end:]
    # Exact preservation outside the private picker includes Translate/Resolve/
    # Select/Choose, synchronous dialog loop, prefs and native menu registrar.
    if output[:start]!=helper[:start] or not output.endswith(helper[end:]):
        raise AssertionError('Existing helper behavior changed outside LanguagePicker')
    if any(name in runtime for name in ('SkiaSharp','HarfBuzzSharp','Topten.RichTextKit')):
        raise ValueError('Offline renderer dependency must not ship in bootstrap')
    attribution=(assets/'BootstrapFontNotice.txt').read_text(encoding='utf-8-sig').replace('*/','* /')
    output+='\n// BEGIN REVIEWED BOOTSTRAP CAPTION OVERLAY\n/*\n'+attribution+'\n*/\n'+runtime+'\n'+data+'\n// END REVIEWED BOOTSTRAP CAPTION OVERLAY\n'
    return output
