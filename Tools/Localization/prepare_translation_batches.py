"""Prepare context-rich batches for AI authoring, without sending anything anywhere."""
from pathlib import Path
import argparse, json
from audit_coverage import inventory, feature_keys, audit

HERE = Path(__file__).resolve().parent

def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--out', type=Path, required=True)
    args = parser.parse_args()
    args.out.mkdir(parents=True, exist_ok=True)
    merged = inventory()
    poses = json.loads((HERE/'poses.translations.json').read_text(encoding='utf-8'))['en']
    features = feature_keys()
    coverage = audit()
    bootstrap = json.loads((HERE.parent/'InstallerLocalization/bootstrap.translations.json').read_text(encoding='utf-8'))
    instructions = [
        'Translate with the existing target-language terminology and feature context.',
        'Preserve placeholders, tags, numbers, axes, API/package names and error codes.',
        'Keep existing translations. Propose corrections separately with a reason.',
        'Translate ALL standard pose names together; maintain numbering and direction conventions.',
        'Do not translate custom names, asset paths, GUIDs or serialized identifiers.',
        'Review feature titles and ON/OFF descriptions as pairs, not unrelated sentences.',
        'Do not use an external translation service without explicit user permission.',
        'Mark AI-authored output and retain provenance; do not claim native-speaker review.'
    ]
    for language, values in merged.items():
        if language == 'en': continue
        data = dict(language=language, instructions=instructions, existing_translations=values,
            english_reference=merged['en'], feature_cards=features, complete_pose_reference=poses,
            missing={k:v for k,v in merged['en'].items() if k not in values},
            setup_existing=bootstrap['translations'].get(language, {}),
            setup_missing=coverage['languages'][language]['setup_missing'])
        (args.out/(language+'.json')).write_text(json.dumps(data, ensure_ascii=False, indent=2)+'\n', encoding='utf-8')
    print('Prepared language batches:', len(merged)-1)

if __name__ == '__main__': main()
