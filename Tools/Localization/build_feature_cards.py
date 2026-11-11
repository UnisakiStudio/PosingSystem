"""Import reviewed feature-card batches using frozen serialized-path bindings."""
from pathlib import Path
import json
from audit_coverage import feature_keys, read

HERE = Path(__file__).resolve().parent

def build_translations(source, actual):
    """Build a complete batch independently of discovery order or old output.

    Source additions, removals and Japanese edits require batch review before
    generating any output, even when the total number of cards stays unchanged.
    """
    if source.get('schema') != 1:
        raise ValueError('Feature-card batch requires schema 1 frozen bindings')
    features = source['features']
    frozen = {}
    for card in features:
        path = card['path']
        if not isinstance(path, str) or not path or path in frozen:
            raise ValueError('Duplicate or invalid frozen feature path: ' + repr(path))
        if any(not isinstance(card[field], str) or not card[field]
               for field in ('title', 'description')):
            raise ValueError('Empty frozen feature source: ' + path)
        frozen[path] = card
    if not frozen:
        raise ValueError('Feature-card batch has no frozen bindings')
    added, missing = set(actual) - set(frozen), set(frozen) - set(actual)
    if added or missing:
        raise ValueError('Review feature-card batch: new paths=' + repr(sorted(added))
                         + ', missing paths=' + repr(sorted(missing)))
    for path, card in frozen.items():
        for field in ('title', 'description'):
            if actual[path].get(field) != card[field]:
                raise ValueError('Review feature-card batch: changed ' + path + '/' + field)
    title_paths = source['title_paths']
    if (len(title_paths) != len(set(title_paths))
            or not set(title_paths).issubset(frozen)):
        raise ValueError('Duplicate or unknown feature-card title_paths')
    if not source['languages']:
        raise ValueError('Feature-card batch has no translated languages')
    output = {}
    for language, rows in source['languages'].items():
        if len(rows['descriptions']) != len(features) or len(rows['titles']) != len(title_paths):
            raise ValueError('Incomplete feature-card rows: ' + language)
        entries = {}

        def add(key, value):
            if not isinstance(value, str) or not value.strip():
                raise ValueError('Empty feature-card translation: ' + language + '/' + key)
            if key in entries:
                raise ValueError('Duplicate feature-card lookup key: ' + language + '/' + key)
            entries[key] = value

        # Rows belong to frozen authoring paths, never to C# discovery order.
        descriptions = dict(zip((card['path'] for card in features), rows['descriptions']))
        titles = dict(zip(title_paths, rows['titles']))
        for path, card in frozen.items():
            add(card['description'], descriptions[path])
            if path in titles:
                add(card['title'], titles[path])
        output[language] = entries
    return output


def main():
    output = build_translations(read(HERE / 'feature-cards.ai.json'), feature_keys())
    # Emit every reviewed row: filtering against merged inventory made results
    # depend on a previous generated file and silently dropped valid keys.
    dest = HERE / 'feature-cards.translations.json'
    dest.write_text(json.dumps(output, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print('Feature translations:', {key: len(value) for key, value in output.items()})


if __name__ == '__main__':
    main()
