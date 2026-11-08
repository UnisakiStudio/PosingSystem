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
    known = expected | set(bootstrap['aliases']) | set(read(HERE/'invariant-labels.json'))
    for path in sorted((PACKAGE/'Editor').rglob('*.cs')):
        if path.name == 'SetupLanguage.cs': continue
        text = re.sub(r'//[^\n]*', '', path.read_text(encoding='utf-8-sig'))
        for m in re.finditer(r'PosingSystemLocalization\.(?:Text|Format)\s*\(\s*('+STRING+r')', text):
            key = json.loads(m[1])
            if key not in known: report['source_omissions'].append({'file':path.relative_to(ROOT).as_posix(), 'key':key})
    for key in sorted(feature_inventory-known): report['source_omissions'].append({'file':'Inspector feature cards', 'key':key})
    report['summary'] = dict(target_languages=len(codes)-1, keys=len(expected), feature_cards=len(features),
        missing=sum(len(v['missing']) for v in report['languages'].values()),
        feature_missing=sum(len(v['feature_missing']) for v in report['languages'].values()),
        setup_missing=sum(len(v['setup_missing']) for v in report['languages'].values()),
        source_omissions=len(report['source_omissions']), terminology_errors=len(report['terminology_errors']))
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
    failed = report['errors'] or report['source_omissions'] or report['terminology_errors'] or report['summary']['missing'] or report['summary']['setup_missing']
    if failed and not args.allow_incomplete:
        print('Localization release gate FAILED. Inspect the report; fallback is not translation.', file=sys.stderr)
        return 1
    return 0

if __name__ == '__main__': sys.exit(main())
