"""Rebuild checked-in translation catalogs from reviewed batch tables (no network calls)."""
from pathlib import Path
import json, uuid
ROOT=Path(__file__).resolve().parents[2]
DEST=ROOT/'Packages/jp.unisakistudio.posingsystem/Editor/Localization'

def meta(p,folder=False):
    m=Path(str(p)+'.meta')
    if not m.exists():
        m.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n'+('folderAsset: yes\n' if folder else ''),encoding='utf-8')

def main():
    DEST.mkdir(exist_ok=True);meta(DEST,True)
    translations={}
    for file in sorted(Path(__file__).parent.glob('*.translations.json')):
        for language, entries in json.loads(file.read_text(encoding='utf-8')).items():
            catalog=translations.setdefault(language,{})
            for key,value in entries.items():
                if key in catalog and catalog[key]!=value:raise ValueError(f'Conflicting translation {language}/{key}')
                if not isinstance(value,str) or not value.strip():raise ValueError(f'Empty translation {language}/{key}')
                catalog[key]=value
    for language,entries in sorted(translations.items()):
        p=DEST/(language+'.json')
        p.write_text(json.dumps({'entries':[dict(key=k,value=v) for k,v in sorted(entries.items())]},ensure_ascii=False,indent=2)+'\n',encoding='utf-8');meta(p)
    print({language:len(entries) for language,entries in translations.items()})
if __name__=='__main__':main()
