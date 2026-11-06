"""Validate translation batches and generated catalogs; report missing text and name lengths."""
from pathlib import Path
import argparse,collections,json,re,sys
ROOT=Path(__file__).resolve().parents[2]
TABLES=Path(__file__).parent
CATALOGS=ROOT/'Packages/jp.unisakistudio.posingsystem/Editor/Localization'

def unique_pairs(pairs):
    result={}
    for key,value in pairs:
        if key in result:raise ValueError('Duplicate JSON key: '+key)
        result[key]=value
    return result

def read(path):return json.loads(path.read_text(encoding='utf-8-sig'),object_pairs_hook=unique_pairs)

def validate(max_name_length=None):
    merged={}
    for path in sorted(TABLES.glob('*.translations.json')):
        for language,entries in read(path).items():
            for key,value in entries.items():
                if not isinstance(value,str) or not value.strip():raise ValueError(f'Empty {language}/{key}')
                if any(ord(c)<32 and c not in '\n\t' for c in value):raise ValueError(f'Control character {language}/{key}')
                # Preserve numbered composite-format placeholders when the source has them.
                placeholders=lambda text:collections.Counter(re.findall(r'(?<!\{)\{\d+(?:[^{}]*)\}(?!\})',text))
                if placeholders(key)!=placeholders(value):raise ValueError(f'Placeholder mismatch {language}/{key}')
                target=merged.setdefault(language,{})
                if key in target and target[key]!=value:raise ValueError(f'Conflicting {language}/{key}')
                target[key]=value
    report={}
    poses=read(TABLES/'poses.translations.json')
    pose_keys=set(poses['en'])
    for language,entries in merged.items():
        actual={}
        for entry in read(CATALOGS/(language+'.json'))['entries']:
            if entry['key'] in actual:raise ValueError('Duplicate catalog entry')
            actual[entry['key']]=entry['value']
        if actual!=entries:raise ValueError('Generated catalog is stale: '+language)
        if language in poses and set(poses[language])!=pose_keys:raise ValueError('Incomplete pose batch: '+language)
        over=[{'source':key,'value':value,'length':len(value)} for key,value in poses.get(language,{}).items() if max_name_length is not None and len(value)>max_name_length]
        report[language]={'entries':len(entries),'pose_names':len(poses.get(language,{})),'longest_pose_name':max((len(v) for v in poses.get(language,{}).values()),default=0),'over_requested_name_limit':over}
    if max_name_length is not None and any(v['over_requested_name_limit'] for v in report.values()):raise ValueError(json.dumps(report,ensure_ascii=False))
    return report

if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--max-name-length',type=int);parser.add_argument('--report',type=Path);args=parser.parse_args()
    report=validate(args.max_name_length)
    if args.report:args.report.write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({'languages':len(report),'entries':sum(x['entries'] for x in report.values()),'complete_pose_languages':[k for k,v in report.items() if v['pose_names']]}))
