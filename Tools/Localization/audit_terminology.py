"""Review changed translations against established feature terminology, offline.

Grammar can require inflections or omit a repeated noun. Accept those by declaring
forms in AIReview/<locale>.json or a specific, reasoned exception; never rewrite
natural translations just to satisfy substring matching.
"""
from pathlib import Path
import argparse, json, re, sys, unicodedata
from audit_coverage import inventory, read

HERE = Path(__file__).resolve().parent

def normalized(value):
    return unicodedata.normalize('NFKC', value).casefold()

def audit():
    merged = inventory()
    exceptions_path = HERE/'terminology-exceptions.json'
    exceptions = read(exceptions_path) if exceptions_path.exists() else {}
    result = {'unreviewed_languages': [], 'issues': [], 'exceptions': [], 'checked': 0}
    for language, values in merged.items():
        if language == 'en': continue
        metadata_path = HERE/'AIReview'/(language+'.json')
        metadata = read(metadata_path) if metadata_path.exists() else {}
        forms = metadata.get('term_forms', {})
        if not forms: result['unreviewed_languages'].append(language)
        for source_term, targets in forms.items():
            assert isinstance(targets, list) and targets and all(isinstance(t, str) and t.strip() for t in targets)
            pattern = re.compile(r'(?<![A-Za-z])'+re.escape(source_term)+(r's?' if source_term.lower() == 'preset' else '')+r'(?![A-Za-z])', re.I)
            for key, english in merged['en'].items():
                if key not in values or not pattern.search(english): continue
                result['checked'] += 1
                value = values[key]
                if any(normalized(form) in normalized(value) for form in targets): continue
                exception = (metadata.get('term_exceptions', {}).get(key, {}).get(source_term)
                    or exceptions.get(language, {}).get(key, {}).get(source_term))
                if isinstance(exception, dict):
                    exception = exception.get('reason') if exception.get('source') == english and exception.get('value') == value else None
                item = dict(language=language, key=key, source=english, value=value, term=source_term, approved_forms=targets)
                if exception and isinstance(exception, str) and exception.strip():
                    item['reason'] = exception
                    result['exceptions'].append(item)
                else: result['issues'].append(item)
    return result

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--report', type=Path, required=True)
    parser.add_argument('--strict', action='store_true')
    args = parser.parse_args()
    result = audit()
    args.report.parent.mkdir(parents=True, exist_ok=True)
    args.report.write_text(json.dumps(result, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print(json.dumps(dict(checked=result['checked'], issues=len(result['issues']), unreviewed_languages=result['unreviewed_languages'], explained_exceptions=len(result['exceptions']))))
    return 1 if args.strict and (result['issues'] or result['unreviewed_languages']) else 0

if __name__ == '__main__': sys.exit(main())
