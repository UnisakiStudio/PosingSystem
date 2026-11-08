"""Read an inclusive range of frozen paragraph blocks without shell quoting."""
from pathlib import Path
import json, sys
source = json.loads((Path(__file__).parent/'sources.json').read_text(encoding='utf-8'))
start, end = map(int, sys.argv[1:3])
for i,(key, block) in enumerate(source['blocks'].items()):
    if start <= i <= end:
        print(f'[{i} {key} {block["language"]} {", ".join(block["contexts"])}]\n{block["text"]}\n')
