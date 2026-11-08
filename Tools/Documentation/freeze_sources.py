"""Freeze locally authored documentation for contextual AI translation (no network)."""
from pathlib import Path
import hashlib
import json
import re
from build_documents import canonical, repository

HERE = Path(__file__).resolve().parent
VPM = HERE.parents[2]
PRODUCTS = {'PosingSystem': ('posingsystem', '3.2.0'), 'KawaiiPosing': ('kawaiiposing', '3.2.0'), 'CoolPosing': ('coolposing', '1.2.0')}
LOCALES = ['ja','en','fr','it','zh-Hans','zh-Hant','ko','th','de','es','pt','pt-BR','ru','uk','pl','nl','sv','da','fi','nb','cs','tr','id','vi','hi','bn','ta','te','ar','he']

def sha(text):
    return hashlib.sha256(text.encode('utf-8')).hexdigest()

def freeze():
    docs, blocks = {}, {}
    for product, (slug, version) in PRODUCTS.items():
        repo = repository(product)
        package = repo / 'Packages' / ('jp.unisakistudio.' + slug)
        for kind, path, language in [('repository', repo / 'README.md', 'ja'), ('readme', package / 'README.md', 'ja'), ('changelog', package / 'CHANGELOG.md', 'ja'), ('guide', package / 'Documentation~' / 'UserGuide.en.md', 'en')]:
            source = canonical(path.read_text(encoding='utf-8-sig'))
            ids = []
            for paragraph in re.split(r'\n\s*\n', source):
                paragraph = paragraph.strip()
                if not paragraph:
                    continue
                key = sha(paragraph)[:16]
                if key in blocks and blocks[key]['text'] != paragraph:
                    raise ValueError('Digest collision')
                invariant = bool(re.fullmatch(r'https?://\S+', paragraph) or re.fullmatch(r'## \[[\w.\-]+\] - [\d\-/]+', paragraph))
                entry = blocks.setdefault(key, {'language': language, 'text': paragraph, 'invariant': invariant, 'contexts': []})
                entry['contexts'].append(product + '/' + kind)
                ids.append(key)
            docs[product + '/' + kind] = {'product': product, 'kind': kind, 'source': product+'/'+str(path.relative_to(repo)).replace('\\','/'), 'source_sha256': sha(source), 'blocks': ids}
    payload = {'schema': 1, 'locales': LOCALES, 'documents': docs, 'blocks': blocks}
    payload['source_sha256'] = sha(json.dumps(payload, ensure_ascii=False, sort_keys=True))
    HERE.mkdir(parents=True, exist_ok=True)
    (HERE / 'sources.json').write_text(json.dumps(payload, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'source_sha256': payload['source_sha256'], 'documents': len(docs), 'blocks': len(blocks), 'characters': sum(len(b['text']) for b in blocks.values()), 'invariant': sum(b['invariant'] for b in blocks.values())}))

if __name__ == '__main__':
    freeze()
