"""Offline release gate for omissions, feature descriptions and glossary consistency.

Unlike the syntax validator, this compares every supported language with the full
English key inventory. --allow-incomplete is only for a diagnostic report; CI and
release gates must use the strict default. No network access or user preferences.
"""
from pathlib import Path
import argparse, collections, json, re, sys

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[1]
PACKAGE = ROOT / 'Packages/jp.unisakistudio.posingsystem'
JAPANESE = re.compile('[\u3040-\u30ff\u3400-\u9fff]')
STRING = r'"(?:\\.|[^"\\])*"'

def without_comments(text):
    # Keep strings and offsets intact: // may be part of a URL inside a literal.
    token = re.compile(r'@"(?:""|[^"])*"|' + STRING + r'|//[^\n]*|/\*.*?\*/', re.S)
    return token.sub(lambda m: ''.join('\n' if c == '\n' else ' ' for c in m[0])
                     if m[0].startswith(('//', '/*')) else m[0], text)

def call_arguments(text, opening):
    """Return top-level argument spans, ignoring nested calls, strings and commas."""
    spans, start, depth, index = [], opening + 1, 1, opening + 1
    while index < len(text):
        if text[index] == '"':
            match = re.match(STRING, text[index:])
            if not match: return [], index
            index += len(match[0]); continue
        char = text[index]
        if char in '([{': depth += 1
        elif char in ')]}':
            depth -= 1
            if depth == 0:
                spans.append((start,index)); return spans,index+1
        elif char == ',' and depth == 1:
            spans.append((start,index)); start=index+1
        index += 1
    return [],index

def display_path_issues(text, known, invariants, exceptions=None):
    """Check display sinks as well as translation lookups, without a C# dependency.

    This deliberately checks static literals and local popup-option assignments.
    Arbitrary interprocedural data flow still requires review and runtime tests.
    """
    text = without_comments(text)
    issues, protected = [], []
    exceptions = exceptions or {}
    translate = r'(?:PosingSystemLocalization\.(?:Text|TextMessage|Format|FormatFor|Dialog|DialogFormat)|(?:UnisakiStudio\.Setup\.)?Language\.(?:T|Translate|Format|Dialog))'
    if 'class PosingSystemLocalization' in text:
        translate += r'|(?<![\w.])(?:Text|TextMessage|Format|Dialog|DialogFormat)'
    translate_calls = list(re.finditer(r'(?:'+translate+r')\s*\(',text))
    def registered(value):
        if not JAPANESE.search(value): return True
        if value in known or value.strip() in known or value in invariants: return True
        lines=[line.strip() for line in value.split('\n') if line.strip()]
        return bool(lines) and all(not JAPANESE.search(line) or line in known or line in invariants for line in lines)
    def literals(start,end):
        for match in re.finditer(STRING,text[start:end]):
            try: value=json.loads(match[0])
            except ValueError: continue
            yield start+match.start(),value
    def constant_string(start,end):
        expression=text[start:end]
        if re.fullmatch(STRING+r'(?:\s*\+\s*'+STRING+r')*',expression.strip(),re.S):
            return ''.join(value for _,value in literals(start,end))
        return None
    def add(offset,value,kind):
        issues.append(dict(line=text.count('\n',0,offset)+1,key=value,kind=kind))
    for call in translate_calls:
        args,end=call_arguments(text,call.end()-1)
        if not args: continue
        protected.append((call.start(),end))
        name=call[0].split('(')[0].strip().rsplit('.',1)[-1]
        checked=args[1:2] if name=='FormatFor' else args[:4] if name in {'Dialog','DialogFormat'} else args[:1]
        for start,stop in checked:
            folded=constant_string(start,stop)
            if folded is not None:
                if not registered(folded): add(start,folded,'unregistered_dialog_argument' if name.startswith('Dialog') else 'unregistered_display_argument')
                continue
            for offset,value in literals(start,stop):
                if not registered(value): add(offset,value,'unregistered_dialog_argument' if name.startswith('Dialog') else 'unregistered_display_argument')
    def is_protected(offset): return any(start<=offset<end for start,end in protected)
    # NDMF displays plugin names in its build/debug UI. A registered Japanese
    # literal still bypasses translation unless the getter actually calls Text.
    for prop in re.finditer(r'\boverride\s+string\s+DisplayName\s*(=>|\{)', text):
        start = prop.end()
        if prop[1] == '=>':
            end = text.find(';', start)
            if end < 0: continue
        else:
            _, end = call_arguments(text, start - 1)
        for offset, value in literals(start, end):
            if JAPANESE.search(value) and value not in invariants and not is_protected(offset):
                add(offset, value, 'unlocalized_plugin_display_name')
    # The shaped-text facade remains a display sink. Moving a control through
    # it must never make an unlocalized literal or local popup array invisible.
    controls = r'(?:Label|LabelField|HelpBox|Button|ToggleLeft|EditorToggle|Toggle|Foldout|Popup|LinkButton|TextField|ObjectField|IntField|FloatField|EnumPopup|PropertyField|Slider|Vector3Field|SelectionGrid|Toolbar)'
    ui = r'(?:(?:EditorGUI|EditorGUILayout|GUI|GUILayout)\.' + controls + r'|PosingSystemTextGUI\.(?:Layout)?' + controls + r'|EditorUtility\.(?:DisplayDialog|DisplayDialogComplex|DisplayProgressBar)|new\s+GUIContent)'
    ui_calls=[]
    for call in re.finditer(ui+r'\s*\(',text):
        args,end=call_arguments(text,call.end()-1)
        ui_calls.append((call.start(),end,args))
        for start,stop in args:
            for offset,value in literals(start,stop):
                if JAPANESE.search(value) and value not in invariants and not is_protected(offset):
                    add(offset,value,'unlocalized_display_literal')
    # A popup's label may be translated while its options (e.g. i + "番") are not.
    for assignment in re.finditer(r'\b(?:var|string\[\]|GUIContent\[\])\s+(\w+)\s*=\s*([^;]+);',text):
        name=assignment[1]
        consumers=[]
        for a,b,args in ui_calls:
            if a<assignment.end(): continue
            for x,y in args:
                for use in re.finditer(r'\b'+re.escape(name)+r'\b',text[x:y]):
                    consumers.append((x+use.start(),is_protected(x+use.start())))
        if not consumers: continue
        for offset,value in literals(assignment.start(2),assignment.end(2)):
            if not JAPANESE.search(value) or value in invariants or is_protected(offset): continue
            if not registered(value): add(offset,value,'unregistered_display_data')
            elif any(not translated for _,translated in consumers):
                # A local array may be localized with a method-group projection.
                translated_projection=any(re.search(r'\b'+re.escape(name)+r'\s*\.\s*Select\s*\(\s*PosingSystemLocalization\.Text\s*\)',text[max(0,use-5):use+150]) for use,_ in consumers)
                if not translated_projection: add(offset,value,'unlocalized_popup_options')
    # Native menus do not pass through Text(). Every Japanese path component must
    # resolve before the reflection-based menu rebuilder can localize it.
    for attribute in re.finditer(r'\b(?:MenuItem|CreateAssetMenu)(?:Attribute)?\s*\(',text):
        args,end=call_arguments(text,attribute.end()-1)
        selected=args[:1] if attribute[0].startswith('MenuItem') else args
        for start,stop in selected:
            if attribute[0].startswith('CreateAssetMenu') and not re.match(r'\s*menuName\s*=',text[start:stop]): continue
            for offset,value in literals(start,stop):
                for part in value.split('/'):
                    if JAPANESE.search(part) and not registered(part): add(offset,part,'unregistered_native_menu_component')
    # Prebuild/build exceptions can reach both the Inspector dialog and NDMF's
    # error window. Inspect every literal in a concatenation, not just its first
    # quote, and register both summary and recovery instructions of diagnostics.
    diagnostic = r'(?:throw\s+new\s+[\w.]*Exception|(?:PosingSystemDiagnostics\.)?Failure|new\s+(?:PosingSystemDiagnostics\.)?Issue)'
    if 'class PosingSystemDiagnostics' in text:
        diagnostic += r'|(?<![\w.])Add'
    for call in re.finditer(r'(?:'+diagnostic+r')\s*\(',text):
        args,end=call_arguments(text,call.end()-1)
        is_exception=call[0].lstrip().startswith('throw')
        checked=args[:1] if is_exception else args[1:3]
        for start,stop in checked:
            folded=constant_string(start,stop)
            if folded is not None:
                if folded in exceptions: continue
                if not registered(folded): add(start,folded,'unregistered_diagnostic_argument')
                elif is_exception and JAPANESE.search(folded): add(start,folded,'unlocalized_exception_literal')
                continue
            for offset,value in literals(start,stop):
                if value in exceptions or is_protected(offset): continue
                if not registered(value): add(offset,value,'unregistered_diagnostic_argument')
                elif is_exception and JAPANESE.search(value) and value not in invariants:
                    add(offset,value,'unlocalized_exception_literal')
    unique={(i['line'],i['key'],i['kind']):i for i in issues}
    return list(unique.values())

def read(p):
    def unique(pairs):
        result = {}
        for key, value in pairs:
            if key in result: raise ValueError('Duplicate JSON key: ' + key)
            result[key] = value
        return result
    return json.loads(p.read_text(encoding='utf-8-sig'), object_pairs_hook=unique)

def inventory():
    merged = {}
    for path in sorted(HERE.glob('*.translations.json')):
        for language, values in read(path).items():
            target = merged.setdefault(language, {})
            for key, value in values.items():
                if key in target and target[key] != value: raise ValueError('Conflicting translation: ' + language + '/' + key)
                target[key] = value
    return merged

def feature_keys():
    result = {}
    for name in ['PosingSystemSettingsInspector.cs', 'PosingSystemEditor.Inspector.cs']:
        source = (PACKAGE/'Editor'/name).read_text(encoding='utf-8-sig')
        for m in re.finditer(r'new\s+(?:PosingSystemSettingsInspector\.)?Setting\s*\(\s*('+STRING+r')\s*,\s*('+STRING+r')\s*,\s*('+STRING+r')', source):
            result[json.loads(m[1])] = {'title': json.loads(m[2]), 'description': json.loads(m[3])}
    return result

def audit():
    merged = inventory()
    source = (PACKAGE/'Editor/PosingSystemLocalization.cs').read_text(encoding='utf-8-sig')
    codes = re.findall(r'"([\w-]+)"', re.search(r'Codes\s*=\s*\{(.*?)\}', source, re.S)[1])
    expected = set(merged['en'])
    poses = set(read(HERE/'poses.translations.json')['en'])
    features = feature_keys()
    feature_inventory = {v for item in features.values() for v in item.values()}
    bootstrap = read(ROOT/'Tools/InstallerLocalization/bootstrap.translations.json')
    setup_expected = set(bootstrap['aliases'].values()) | set(bootstrap.get('message_models', {}))
    constraints_path = HERE/'terminology-constraints.json'
    constraints = read(constraints_path) if constraints_path.exists() else {}
    report = {'languages': {}, 'feature_cards': features, 'source_omissions': [], 'terminology_errors': [], 'errors': []}
    display_names = set()
    for table in ('poses.translations.json', 'menus.translations.json'):
        for entries in read(HERE/table).values(): display_names.update(entries)
    names_source = (PACKAGE/'Editor/PosingSystemDisplayNameKeys.cs').read_text(encoding='utf-8-sig')
    generated_names = {json.loads(value) for value in re.findall(STRING, names_source)}
    if generated_names != display_names: report['errors'].append('Stale generated standard display-name keys')
    for language in codes:
        if language == 'ja': continue
        values = merged.get(language, {})
        catalog_path = PACKAGE/'Editor/Localization'/(language+'.json')
        generated = {e['key']:e['value'] for e in read(catalog_path)['entries']} if catalog_path.exists() else {}
        if generated != values: report['errors'].append('Stale generated catalog: ' + language)
        effective_setup = {merged['en'][key] for key in values if key in merged['en'] and key not in poses}
        effective_setup.update(bootstrap['translations'].get(language, {}))
        # English is the canonical setup language; messages assembled from translated
        # constituent lines are valid only when every line has an effective entry.
        for key, model in bootstrap.get('message_models', {}).items():
            if all(not line.strip() or line in effective_setup for line in model.split('\n')): effective_setup.add(key)
        missing = sorted(expected-set(values))
        setup_missing = [] if language == 'en' else sorted(setup_expected-effective_setup)
        report['languages'][language] = dict(entries=len(values), expected=len(expected), missing=missing,
            feature_missing=sorted(feature_inventory-set(values)), pose_missing=sorted(poses-set(values)),
            setup_missing=setup_missing,
            english_copies=sorted(k for k in expected & set(values) if values[k] == merged['en'][k]) if language != 'en' else [])
        for key, required in constraints.get(language, {}).items():
            for term in required:
                if term not in values.get(key, ''): report['terminology_errors'].append({'language':language, 'key':key, 'term':term})
    invariants = set(read(HERE/'invariant-labels.json'))
    exception_path = HERE/'source-audit-exceptions.json'
    source_exceptions = read(exception_path) if exception_path.exists() else {}
    for filename, values in source_exceptions.items():
        for key, reason in values.items():
            if not key or not reason.strip(): report['errors'].append('Source audit exception needs a key and reason: '+filename)
    known = expected | set(bootstrap['aliases']) | invariants
    for path in sorted((PACKAGE/'Editor').rglob('*.cs')):
        if path.name == 'SetupLanguage.cs': continue
        text = without_comments(path.read_text(encoding='utf-8-sig'))
        for m in re.finditer(r'PosingSystemLocalization\.(?:Text|TextMessage|Format)\s*\(\s*('+STRING+r')', text):
            key = json.loads(m[1])
            if text[m.end():].lstrip().startswith('+') and any(k.startswith(key) for k in expected): continue
            if key not in known: report['source_omissions'].append({'file':path.relative_to(ROOT).as_posix(), 'key':key})
        for call in re.finditer(r'PosingSystemLocalization\.FormatFor\s*\(', text):
            args,end=call_arguments(text,call.end()-1)
            if len(args)<2: continue
            expression=text[args[1][0]:args[1][1]].strip()
            if re.fullmatch(STRING,expression):
                key=json.loads(expression)
                if key not in known: report['source_omissions'].append({'file':path.relative_to(ROOT).as_posix(), 'key':key})
    report['display_path_issues']=[]
    report['display_source_roots']=[]
    for product in ['PosingSystem','KawaiiPosing','CoolPosing']:
        product_repo=ROOT.parent/product
        package=product_repo/'Packages'/('jp.unisakistudio.'+product.lower())
        if not package.exists(): continue
        report['display_source_roots'].append(str(package))
        for folder in ['Editor','Runtime']:
            for path in sorted((package/folder).rglob('*.cs')):
                if path.name=='SetupLanguage.cs': continue
                for issue in display_path_issues(path.read_text(encoding='utf-8-sig'),known,invariants,source_exceptions.get(path.name)):
                    report['display_path_issues'].append(dict(product=product,file=path.relative_to(product_repo).as_posix(),**issue))
    for key in sorted(feature_inventory-known): report['source_omissions'].append({'file':'Inspector feature cards', 'key':key})
    bone_source = (PACKAGE/'Editor/PosingSystemAutoPoseCatalogEditor.cs').read_text(encoding='utf-8-sig')
    for bone in re.findall(r'HumanBodyBones\.(\w+)\s*=>', bone_source):
        if 'bone.'+bone not in expected: report['source_omissions'].append({'file':'Humanoid bone labels', 'key':'bone.'+bone})
    report['summary'] = dict(target_languages=len(codes)-1, keys=len(expected), feature_cards=len(features),
        missing=sum(len(v['missing']) for v in report['languages'].values()),
        feature_missing=sum(len(v['feature_missing']) for v in report['languages'].values()),
        setup_missing=sum(len(v['setup_missing']) for v in report['languages'].values()),
        source_omissions=len(report['source_omissions']), display_path_issues=len(report['display_path_issues']), terminology_errors=len(report['terminology_errors']))
    return report

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--report', type=Path)
    parser.add_argument('--allow-incomplete', action='store_true')
    args = parser.parse_args()
    report = audit()
    if args.report:
        args.report.parent.mkdir(parents=True, exist_ok=True)
        args.report.write_text(json.dumps(report, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print(json.dumps(report['summary']))
    failed = report['errors'] or report['source_omissions'] or report['display_path_issues'] or report['terminology_errors'] or report['summary']['missing'] or report['summary']['setup_missing']
    if failed and not args.allow_incomplete:
        print('Localization release gate FAILED. Inspect the report; fallback is not translation.', file=sys.stderr)
        return 1
    return 0

if __name__ == '__main__': sys.exit(main())
