"""Build complete offline documentation from reviewed local AI authoring files.

No network, translation service, publication, package installation or Unity state.
The same frozen paragraph is translated once and reused in its full-document context.
"""
from pathlib import Path
from collections import Counter
from html import escape
from html.parser import HTMLParser
from urllib.parse import urlsplit, unquote, quote
import argparse
import hashlib
import json
import os
import re
import sys
import uuid
import unicodedata

HERE = Path(__file__).resolve().parent
VPM = HERE.parents[2]
SLUGS = {'PosingSystem':'posingsystem','KawaiiPosing':'kawaiiposing','CoolPosing':'coolposing','VirtualLove':'virtuallove'}
# The frozen sources and translations of the three main products live here. A product
# that owns its documentation data (`--data <product repo>/Tools/Documentation`) is
# built from that folder instead, so its text never changes the shared digests.
DATA = None

def data():
    return DATA if DATA is not None else HERE
NAMES = dict(zip(['ja','en','fr','it','zh-Hans','zh-Hant','ko','th','de','es','pt','pt-BR','ru','uk','pl','nl','sv','da','fi','nb','cs','tr','id','vi','hi','bn','ta','te','ar','he'], ['日本語','English','Français','Italiano','简体中文','繁體中文','한국어','ไทย','Deutsch','Español','Português','Português (Brasil)','Русский','Українська','Polski','Nederlands','Svenska','Dansk','Suomi','Norsk','Čeština','Türkçe','Bahasa Indonesia','Tiếng Việt','हिन्दी','বাংলা','தமிழ்','తెలుగు','العربية','עברית']))
LINK = re.compile(r'(!?\[[^\]\n]*\]\()([^\n]*?)(\))')
NAV_START, NAV_END = '<!-- localization-navigation:start -->', '<!-- localization-navigation:end -->'
PROTECTED_IDENTIFIERS=('MotionTime','RootT','RootQ','USSPS_Locomotion','SleepDown','Standing','AnimatorOverrideController','VirtualAnimatorController.SetParameter','AddAssetToSameFile','MissingReferenceException','NullReferenceException','GeneratedResources','VRCContactSender/Receiver','VPMPackageAutoInstaller','SimpleJson','semver.net','anatawa12','FloorAdjuster','可愛いポーズツール.unitypackage','カッコいいポーズツール.unitypackage')
CSS = '''
:root { color-scheme: light dark; font-family: system-ui, -apple-system, "Segoe UI", sans-serif; line-height: 1.7; }
body { margin: auto; max-width: 70rem; padding: 1.5rem; overflow-wrap: anywhere; }
nav { display: flex; flex-wrap: wrap; gap: .6rem 1.2rem; padding-block: 1rem; border-block-end: 1px solid #8887; }
h1,h2,h3 { line-height: 1.4; } h2 { margin-block-start: 2.4rem; }
table { border-collapse: collapse; width: 100%; } th,td { text-align: start; vertical-align: top; padding: .5rem; border: 1px solid #8887; }
thead { background: #8882; } .table-scroll { overflow-x: auto; }
code,pre { direction: ltr; unicode-bidi: isolate; } code { background: #8882; padding: .1em .25em; } pre { white-space: pre-wrap; }
a { color: #2269b8; } a:visited { color: #7952ad; } li { margin-block: .35rem; }
@media(prefers-color-scheme:dark) { a { color: #8ec5ff; } a:visited { color: #cfacff; } }
@media print { nav { display: none; } body { max-width: none; padding: 0; } a { color: inherit; } }
'''.strip()

def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))

def repository(product):
    # The core checkout can have any folder name (including CI checkout paths).
    return HERE.parents[1] if product=='PosingSystem' else VPM/product

def source_path(document):
    return repository(document['product'])/Path(document['source']).relative_to(document['product'])

def digest(value):
    return hashlib.sha256(value.encode('utf-8')).hexdigest()

def canonical(text):
    text = text.replace('\r\n','\n')
    text = re.sub(r'\n'+re.escape(NAV_START)+r'.*?'+re.escape(NAV_END)+r'\n?', '', text, flags=re.S)
    return text.strip()

def source_data():
    source = read(data()/'sources.json')
    editor=repository('PosingSystem')/'Packages/jp.unisakistudio.posingsystem/Editor/PosingSystemLocalization.cs'
    declared=re.search(r'Codes\s*=\s*\{([^}]+)\}',editor.read_text(encoding='utf-8-sig'))
    codes=re.findall(r'"([\w-]+)"',declared.group(1)) if declared else []
    if set(codes)!=set(source['locales']) or set(source['locales'])!=set(NAMES):
        raise ValueError('Documentation languages differ from the advertised Inspector languages')
    unsigned = {k:v for k,v in source.items() if k != 'source_sha256'}
    if digest(json.dumps(unsigned, ensure_ascii=False, sort_keys=True)) != source['source_sha256']:
        raise ValueError('Frozen source was edited without regenerating its digest')
    for key, block in source['blocks'].items():
        if digest(block['text'])[:16] != key:
            raise ValueError('Frozen paragraph digest mismatch: '+key)
    return source

def urls(text):
    return Counter(m.group(2) for m in LINK.finditer(text))

def codes(text):
    return Counter(re.findall(r'`([^`\n]+)`', text))

def release_versions(text):
    return re.findall(r'^## \[([^\]]+)\]', text, re.M)

def structure_errors(original, translated):
    errors = []
    if urls(original) != urls(translated): errors.append('link targets changed')
    if codes(original) != codes(translated): errors.append('inline code changed')
    if release_versions(original) != release_versions(translated): errors.append('release headings changed')
    dates = lambda t: Counter(re.findall(r'\b\d{4}-\d{2}-\d{2}\b', t))
    if dates(original) != dates(translated): errors.append('historical dates changed')
    def normalize_numbers(text):
        normalized=unicodedata.normalize('NFKC',text)
        return ''.join(str(unicodedata.decimal(c)) if c.isdecimal() else c for c in normalized)
    normalized_source=normalize_numbers(original)
    normalized_target=normalize_numbers(translated)
    numbers=lambda text: {token.replace(',','.') for token in re.findall(r'[0-9]+(?:[.,][0-9]+)*',text)}
    # Natural prose may spell out 'one', 'second', 'three-point' or 'one third'.
    # Guard concrete operating limits and package versions; contextual review
    # remains responsible for prose quantities, rather than forcing digit glyphs.
    limits={token.replace(',','.') for token in re.findall(r'([0-9]+(?:[.,][0-9]+)*)\s*(?:%|bits?\b|m\b)',normalized_source)}
    if limits-numbers(normalized_target): errors.append('numeric limits or values omitted')
    versions=lambda text:set(re.findall(r'(?<![0-9.])[0-9]+\.[0-9]+\.[0-9]+(?:[-.][A-Za-z0-9]+)*',text))
    if versions(normalized_source)-versions(normalized_target): errors.append('package versions omitted or changed')
    # Table dimensions and list item counts detect omitted rows and merged history.
    table = lambda t: [line.count('|') for line in t.splitlines() if line.startswith('|')]
    if table(original) != table(translated): errors.append('table structure changed')
    items = lambda t: sum(bool(re.match(r'^\s*(?:[-*] |\d+\. )', line)) for line in t.splitlines())
    if items(original) != items(translated): errors.append('list items changed')
    headings = lambda t: [len(m.group(1)) for m in re.finditer(r'^(#{1,6}) ', t, re.M)]
    if headings(original) != headings(translated): errors.append('heading structure changed')
    if '\ufffd' in translated: errors.append('replacement character')
    for identifier in PROTECTED_IDENTIFIERS:
        if identifier in original and identifier not in translated: errors.append('technical identifier changed: '+identifier)
    for name in ('可愛いポーズ','カッコいいポーズ'):
        if any(form in original for form in ('「'+name+'」','**'+name+'**',name+'Prefab')) and name not in translated:
            errors.append('saved object or Prefab name changed: '+name)
    for name in ('三点だいしゅき(男)','三点だいしゅき(女)'):
        if '「'+name+'」' in original and name not in translated:
            errors.append('saved object or Prefab name changed: '+name)
    return errors

def translations(source, locale):
    path = data()/'translations'/(locale+'.json')
    payload = read(path) if path.exists() else {}
    issues = []
    if payload and payload.get('source_sha256') != source['source_sha256']:
        issues.append({'locale':locale,'issue':'stale translation source digest'})
    authored = payload.get('translations', {})
    for key in authored.keys() - source['blocks'].keys():
        issues.append({'locale':locale,'block':key,'issue':'unknown paragraph'})
    values, missing = {}, []
    for key, block in source['blocks'].items():
        if block['invariant']:
            value = block['text']
        else:
            value = authored.get(key, block['text'] if block['language'] == locale else None)
        if not isinstance(value,str) or not value.strip():
            missing.append(key)
            continue
        values[key] = value.strip()
        for issue in structure_errors(block['text'], value):
            issues.append({'locale':locale,'block':key,'issue':issue})
        if '**Language...**' in block['text'] and 'confirmation' in block['text']:
            bootstrap=read(repository('PosingSystem')/'Tools/InstallerLocalization/bootstrap.translations.json')
            caption=bootstrap.get('translations',{}).get(locale,{}).get('Language...','Language...' if locale=='en' else None)
            if not caption or caption not in value:
                issues.append({'locale':locale,'block':key,'issue':'installer language button differs from actual caption','expected':caption})
        only_links = not LINK.sub('', value).strip('-* \n\t')
        if value.strip() == block['text'] and not block['invariant'] and block['language'] != locale and len(value)>100 and not only_links:
            issues.append({'locale':locale,'block':key,'issue':'unchanged source prose'})
    return values, missing, issues

def document_text(source, name, values):
    return '\n\n'.join(values[k] for k in source['documents'][name]['blocks'])+'\n'

def remap_links(text, source_base, destination, package, locale):
    """Keep legal/original assets; route documentation to complete sibling pages."""
    def replace(match):
        target = match.group(2)
        split = urlsplit(target)
        if split.scheme or split.netloc or target.startswith('#'): return match.group(0)
        resolved = (source_base/unquote(split.path)).resolve()
        if resolved == (package/'README.md').resolve():
            resolved = package/'Documentation~'/'Localization'/locale/'README.md'
        elif resolved == (package/'CHANGELOG.md').resolve():
            resolved = package/'Documentation~'/'Localization'/locale/'CHANGELOG.md'
        elif resolved == (package/'Documentation~'/'UserGuide.en.html').resolve():
            resolved = package/'Documentation~'/'Localization'/locale/'UserGuide.html'
        relative = Path(os.path.relpath(resolved, destination)).as_posix()
        target = quote(relative, safe='/~._-')
        if split.query: target += '?'+split.query
        if split.fragment: target += '#'+split.fragment
        return match.group(1)+target+match.group(3)
    return LINK.sub(replace,text)

def inline(text):
    tokens = []
    def hold(value):
        tokens.append(value)
        return '\x00'+str(len(tokens)-1)+'\x00'
    text = re.sub(r'`([^`\n]+)`',lambda m:hold('<code dir="ltr">'+escape(m.group(1))+'</code>'), text)
    def link(m):
        target = m.group(2)
        # Source is local authoring; still reject executable URLs in rendered HTML.
        if urlsplit(target).scheme.lower() not in ('','https','http','mailto'):
            raise ValueError('Unsupported documentation URL scheme: '+target)
        return hold('<a href="'+escape(target,quote=True)+'">'+escape(m.group(1))+'</a>')
    text = re.sub(r'\[([^\]\n]*)\]\(([^\n]*?)\)',link,text)
    text = escape(text)
    text = re.sub(r'\*\*(.+?)\*\*',r'<strong>\1</strong>',text)
    text = re.sub(r'\*(.+?)\*',r'<em>\1</em>',text)
    text = re.sub(r'(?<![\w\"=])(https?://[^\s<>]+)',lambda m:hold('<a href="'+escape(m.group(1),quote=True)+'">'+m.group(1)+'</a>'),text)
    # A code token may be nested inside a link label; expand until stable.
    for _ in range(3):
        text = re.sub(r'\x00(\d+)\x00',lambda m:tokens[int(m.group(1))],text)
    return text

def markdown(text):
    lines, out, i = text.splitlines(), [], 0
    while i < len(lines):
        line = lines[i]
        if not line.strip(): i+=1; continue
        heading = re.match(r'^(#{1,6}) (.*)',line)
        if heading:
            level=len(heading.group(1)); out.append(f'<h{level}>'+inline(heading.group(2))+f'</h{level}>'); i+=1; continue
        if line.startswith('```'):
            i+=1; code=[]
            while i<len(lines) and not lines[i].startswith('```'): code.append(lines[i]); i+=1
            out.append('<pre dir="ltr"><code>'+escape('\n'.join(code))+'</code></pre>'); i+=1; continue
        if line.startswith('|'):
            rows=[]
            while i<len(lines) and lines[i].startswith('|'):
                cells=[c.strip() for c in lines[i].strip().strip('|').split('|')]
                if not all(re.fullmatch(r':?-+:?', c) for c in cells): rows.append(cells)
                i+=1
            out.append('<div class="table-scroll"><table><thead><tr>'+''.join('<th>'+inline(c)+'</th>' for c in rows[0])+'</tr></thead><tbody>')
            for row in rows[1:]: out.append('<tr>'+''.join('<td>'+inline(c)+'</td>' for c in row)+'</tr>')
            out.append('</tbody></table></div>'); continue
        item = re.match(r'^\s*(?:([-*]) |(\d+)\. )(.*)',line)
        if item:
            tag='ol' if item.group(2) else 'ul'; out.append('<'+tag+'>')
            while i<len(lines):
                entry=re.match(r'^\s*(?:[-*] |\d+\. )(.*)',lines[i])
                if not entry: break
                out.append('<li>'+inline(entry.group(1))+'</li>'); i+=1
            out.append('</'+tag+'>'); continue
        paragraph=[line]; i+=1
        while i<len(lines) and lines[i].strip() and not re.match(r'^(?:#|\||[-*] |\d+\. |```)',lines[i]): paragraph.append(lines[i]); i+=1
        out.append('<p>'+inline('\n'.join(paragraph))+'</p>')
    return '\n'.join(out)

def html_page(text, locale, navigation):
    title = re.search(r'^# (.*)',text,re.M)
    title = title.group(1) if title else 'Documentation'
    direction='rtl' if locale in ('ar','he') else 'ltr'
    return '<!doctype html>\n<html lang="'+locale+'" dir="'+direction+'"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>'+escape(title)+'</title><style>'+CSS+'</style></head><body>\n'+navigation+'\n<main>'+markdown(text)+'</main>\n</body></html>\n'

def output_files(source, locale, values, products):
    outputs={}
    for product in products:
        repo=repository(product); package=repo/'Packages'/('jp.unisakistudio.'+SLUGS[product])
        folder=package/'Documentation~'/'Localization'/locale
        changelog_label=values[next(k for k,b in source['blocks'].items() if b['text']=='# 変更履歴')].lstrip('# ')
        guide_label=document_text(source,product+'/guide',values).splitlines()[0].lstrip('# ')
        # A product without a separate guide uses its README as the guide, so the
        # Inspector's guide link still follows the language; the page is not listed twice.
        readme_is_guide=source['documents'][product+'/guide']['source']==source['documents'][product+'/readme']['source']
        guide_link='' if readme_is_guide else '<a href="UserGuide.html">'+escape(guide_label)+'</a>'
        nav='<nav aria-label="Documentation"><a href="../index.html">'+escape(NAMES[locale])+' · 🌐</a><a href="README.html">README</a><a href="CHANGELOG.html">'+escape(changelog_label)+'</a>'+guide_link+'</nav>'
        for kind, filename in [('readme','README'),('changelog','CHANGELOG'),('guide','UserGuide')]:
            original=source_path(source['documents'][product+'/'+kind])
            text=document_text(source,product+'/'+kind,values)
            text=remap_links(text, original.parent, folder, package, locale)
            outputs[folder/(filename+'.md')]=text
            def html_link(match):
                target=match.group(2)
                if not urlsplit(target).scheme:
                    target=re.sub(r'\.md(?=$|#)', '.html', target)
                return match.group(1)+target+match.group(3)
            htmltext=LINK.sub(html_link,text)
            outputs[folder/(filename+'.html')]=html_page(htmltext,locale,nav)
        # Conventional English entry points contain full content, not summaries.
        if locale=='en':
            for kind, filename in [('readme','README.en.md'),('changelog','CHANGELOG.en.md')]:
                original=source_path(source['documents'][product+'/'+kind])
                outputs[package/filename]=remap_links(document_text(source,product+'/'+kind,values),original.parent,package,package,locale)
                meta=package/(filename+'.meta')
                if meta.exists():
                    outputs[meta]=meta.read_text(encoding='utf-8-sig').replace('\r\n','\n')
                else:
                    guid=uuid.uuid5(uuid.NAMESPACE_URL,'https://unisakistudio.jp/'+SLUGS[product]+'/'+filename).hex
                    outputs[meta]='fileFormatVersion: 2\nguid: '+guid+'\n'
            if not readme_is_guide:
                original=source_path(source['documents'][product+'/guide'])
                legacytext=remap_links(document_text(source,product+'/guide',values),original.parent,original.parent,package,locale)
                legacynav='<nav><a href="Localization/index.html">🌐</a><a href="Localization/en/README.html">README</a><a href="Localization/en/CHANGELOG.html">CHANGELOG</a></nav>'
                outputs[package/'Documentation~'/'UserGuide.en.html']=html_page(legacytext,'en',legacynav)
        repository_text=document_text(source,product+'/repository',values)
        if locale!='ja': outputs[repo/('README.'+locale+'.md')]=remap_links(repository_text,repo,repo,package,locale)
        legacy=package/'Documentation~'/'Localization'/(locale+'.md')
        if legacy.exists():
            original=source_path(source['documents'][product+'/guide'])
            outputs[legacy]=remap_links(document_text(source,product+'/guide',values),original.parent,legacy.parent,package,locale)+'\n[🌐](index.md)\n'
    return outputs

class Links(HTMLParser):
    def __init__(self): super().__init__(); self.targets=[]
    def handle_starttag(self,tag,attrs):
        if tag=='a': self.targets.extend(v for k,v in attrs if k=='href')

def validate_links(outputs):
    issues=[]; count=0
    for path, value in outputs.items():
        if path.suffix=='.html':
            parser=Links(); parser.feed(value); targets=parser.targets
        else: targets=list(urls(value).elements())
        for target in targets:
            split=urlsplit(target)
            if split.scheme or split.netloc or not split.path: continue
            count+=1; resolved=(path.parent/unquote(split.path)).resolve()
            exists=resolved in outputs or resolved.is_file()
            # VPM source checkouts are separate repos; their installed Packages
            # directories are siblings in Unity. Validate that virtual layout too.
            if not exists and 'Packages' in resolved.parts:
                parts=resolved.parts; index=len(parts)-1-list(reversed(parts)).index('Packages')
                if index+1<len(parts):
                    for product,slug in SLUGS.items():
                        if parts[index+1]=='jp.unisakistudio.'+slug:
                            actual=repository(product)/'Packages'/parts[index+1]/Path(*parts[index+2:])
                            exists=actual in outputs or actual.is_file()
            if not exists: issues.append({'file':str(path),'target':target})
    return count,issues

def main():
    parser=argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--locales',nargs='+',choices=list(NAMES))
    parser.add_argument('--products',nargs='+',choices=list(SLUGS))
    parser.add_argument('--check',action='store_true',help='Read-only comparison with generated files')
    parser.add_argument('--audit-only',action='store_true',help='Report completeness; never write generated files')
    parser.add_argument('--report',type=Path,required=True)
    parser.add_argument('--data',type=Path,help='Folder with sources.json and translations/ of a product that owns its documentation data')
    args=parser.parse_args()
    if args.data:
        global DATA
        DATA=args.data.resolve()
    source=source_data(); locales=args.locales or source['locales']
    if len(locales)!=len(set(locales)): parser.error('Duplicate language codes are not allowed')
    frozen={document['product'] for document in source['documents'].values()}
    if args.products and set(args.products)-frozen: parser.error('Not in this frozen source: '+', '.join(sorted(set(args.products)-frozen)))
    products=args.products or [p for p in SLUGS if p in frozen and repository(p).is_dir()]
    issues=[]; outputs={}; coverage={}; translation_digests={}
    for name,doc in source['documents'].items():
        if doc['product'] not in products: continue
        path=source_path(doc)
        if not path.is_file() or digest(canonical(path.read_text(encoding='utf-8-sig'))) != doc['source_sha256']:
            issues.append({'document':name,'issue':'source changed; re-freeze and review affected translations'})
    for locale in locales:
        values,missing,errors=translations(source,locale)
        translation_digests[locale]=digest(json.dumps(values,ensure_ascii=False,sort_keys=True))
        coverage[locale]={'total_blocks':len(source['blocks']),'missing':missing,'translated_or_original':len(values),'issues':len(errors)}
        issues.extend(errors)
        if missing: issues.append({'locale':locale,'issue':'missing translations','count':len(missing)})
        if not missing and not errors: outputs.update(output_files(source,locale,values,products))
    # A native-language index offers all three complete documents for every locale.
    for product in products:
        package=repository(product)/'Packages'/('jp.unisakistudio.'+SLUGS[product])
        rows='\n'.join('<li lang="'+l+'" dir="'+('rtl' if l in ('ar','he') else 'ltr')+'">'+escape(NAMES[l])+': <a href="'+l+'/README.html">README</a> · <a href="'+l+'/CHANGELOG.html">CHANGELOG</a> · <a href="'+l+'/UserGuide.html">📖</a></li>' for l in locales)
        outputs[package/'Documentation~'/'Localization'/'index.html']='<!doctype html>\n<html lang="en"><head><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Languages</title><style>'+CSS+'</style></head><body><h1>🌐</h1><ul>'+rows+'</ul></body></html>\n'
        outputs[package/'Documentation~'/'Localization'/'index.md']='# 🌐\n\n'+'\n'.join('- '+NAMES[l]+': [README]('+l+'/README.md) · [CHANGELOG]('+l+'/CHANGELOG.md) · [📖]('+l+'/UserGuide.md)' for l in locales)+'\n'
        repo=repository(product)
        for path in [repo/'README.md',package/'README.md',package/'CHANGELOG.md']:
            index=package/'Documentation~'/'Localization'/'index.md'
            href=Path(os.path.relpath(index,path.parent)).as_posix()
            label=' · '.join(NAMES.values())
            navigation='\n'+NAV_START+'\n\n🌐 ['+label+']('+href+')\n\n'+NAV_END+'\n'
            outputs[path]=canonical(path.read_text(encoding='utf-8-sig'))+navigation
        for path,value in list(outputs.items()):
            if path.parent in (repo,package) and path.suffix=='.md' and path.name not in ('README.md','CHANGELOG.md'):
                href=Path(os.path.relpath(package/'Documentation~'/'Localization'/'index.md',path.parent)).as_posix()
                outputs[path]=value.rstrip()+'\n\n🌐 ['+' · '.join(NAMES.values())+']('+href+')\n'
    for product in products:
        repo=repository(product); package=repo/'Packages'/('jp.unisakistudio.'+SLUGS[product])
        # Existing concise references retain their identity and become part of
        # the immutable documentation evidence after the separate UI refresh.
        for reference in (package/'Documentation~'/'QuickReference').glob('*'):
            if reference.is_file() and reference.suffix in ('.md','.html'):
                outputs[reference]=reference.read_text(encoding='utf-8-sig').replace('\r\n','\n')
    links,broken=validate_links(outputs)
    issues.extend({'issue':'broken local link',**b} for b in broken)
    for product in products:
        repo=repository(product); package=repo/'Packages'/('jp.unisakistudio.'+SLUGS[product])
        files={Path(os.path.relpath(p,repo)).as_posix():digest(value) for p,value in outputs.items() if p.is_relative_to(repo)}
        sources={str(Path(d['source']).relative_to(product)).replace('\\','/'):d['source_sha256'] for d in source['documents'].values() if d['product']==product}
        manifest={'schema':1,'source_sha256':source['source_sha256'],'translation_digests':translation_digests,'product':product,'package':'jp.unisakistudio.'+SLUGS[product],'locales':locales,'full_30_languages':len(locales)==30,'sources':sources,'files':files,'translation_method':'AI contextual authoring with existing UI terminology','native_speaker_review':False,'publication':False}
        outputs[package/'Documentation~'/'Localization'/'manifest.json']=json.dumps(manifest,ensure_ascii=False,sort_keys=True,indent=2)+'\n'
    differences=[]
    if not args.audit_only and not issues:
        for path,value in outputs.items():
            if not path.is_file() or path.read_text(encoding='utf-8-sig').replace('\r\n','\n')!=value:
                differences.append(str(path))
                if not args.check:
                    path.parent.mkdir(parents=True,exist_ok=True); path.write_text(value,encoding='utf-8')
    report={'schema':1,'source_sha256':source['source_sha256'],'translation_digests':translation_digests,'locales':coverage,'products':products,'full_30_languages':len(locales)==30,'issues':issues,'files':len(outputs),'local_links':links,'generated_differences':differences,'check':args.check,'audit_only':args.audit_only,'publication':False}
    args.report.parent.mkdir(parents=True,exist_ok=True)
    args.report.write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({'languages':len(locales),'missing':sum(len(c['missing']) for c in coverage.values()),'issues':len(issues),'files':len(outputs),'local_links':links,'differences':len(differences),'full_30_languages':report['full_30_languages']}))
    return int(bool(issues or (args.check and differences)))

if __name__=='__main__': sys.exit(main())
