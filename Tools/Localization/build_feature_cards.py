"""Import AI-authored feature-card batches without overwriting existing translations."""
from pathlib import Path
import json
from audit_coverage import inventory, feature_keys

HERE = Path(__file__).resolve().parent

def main():
    source = json.loads((HERE/'feature-cards.ai.json').read_text(encoding='utf-8'))
    features = list(feature_keys().values())
    assert len(features) == 19, 'Review batch order after feature additions'
    existing = inventory()
    output = {}
    for language, rows in source['languages'].items():
        assert len(rows['descriptions']) == 19 and len(rows['titles']) == 12, language
        entries = {}
        for i, card in enumerate(features):
            entries[card['description']] = rows['descriptions'][i]
            if i >= 7: entries[card['title']] = rows['titles'][i-7]
        output[language] = {key: value for key, value in entries.items() if key not in existing[language] or key in prior.get(language, {})}
    dest.write_text(json.dumps(output, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print('Feature translations:', {k:len(v) for k,v in output.items()})

dest = HERE/'feature-cards.translations.json'
prior = json.loads(dest.read_text(encoding='utf-8')) if dest.exists() else {}
if __name__ == '__main__': main()
