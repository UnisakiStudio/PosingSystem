"""Compare full-document feature terminology with the existing UI glossary.

Inflected forms may be approved in Reviews/<locale>.json. Specific exceptions
must bind both the current English pivot and translated paragraph by digest.
"""
from pathlib import Path
import argparse
import json
import re
import sys
import unicodedata
import build_documents as docs

HERE=Path(__file__).resolve().parent
sys.path.insert(0,str(HERE.parent/'Localization'))
import products

def normalized(text): return unicodedata.normalize('NFKC',text).casefold()

def audit(locales=None):
    source=docs.source_data(); english,missing,errors=docs.translations(source,'en')
    if missing or errors: raise ValueError('A complete reviewed English pivot is required')
    japanese,ja_missing,ja_errors=docs.translations(source,'ja')
    result={'source_sha256':source['source_sha256'],'translation_digests':{'en':docs.digest(json.dumps(english,ensure_ascii=False,sort_keys=True)),'ja':docs.digest(json.dumps(japanese,ensure_ascii=False,sort_keys=True))},'checked':0,'missing':{},'issues':[],'exceptions':[],'native_speaker_review':False}
    # Pose names shared by every product, plus each available product's own names.
    owners=products.checkouts()
    poses=sorted(set(docs.read(HERE.parent/'Localization/poses.translations.json')['en'])|{name for product in owners for name in product.pose_names()})
    product_tables=[product.tables() for product in owners]
    if ja_missing or ja_errors: raise ValueError('A complete Japanese guide is required')
    for locale in locales or source['locales']:
        values,missing,errors=docs.translations(source,locale)
        result['translation_digests'][locale]=docs.digest(json.dumps(values,ensure_ascii=False,sort_keys=True))
        if missing: result['missing'][locale]=missing
        metadata=HERE.parent/'Localization'/'AIReview'/(locale+'.json')
        metadata=docs.read(metadata) if metadata.exists() else {}
        review=HERE/'Reviews'/(locale+'.json')
        review=docs.read(review) if review.exists() else {}
        if locale=='ja': display={pose:pose for pose in poses}
        else:
            catalog=docs.read(docs.repository('PosingSystem')/'Packages/jp.unisakistudio.posingsystem/Editor/Localization'/(locale+'.json'))
            display={entry['key']:entry['value'] for entry in catalog['entries']}
            for tables in product_tables: display.update(tables.get(locale,{}))
        forms={term:list(options) for term,options in metadata.get('term_forms',{}).items()}
        for term,options in review.get('term_forms',{}).items(): forms.setdefault(term,[]).extend(options)
        if not forms and locale not in ('ja','en'): result['issues'].append({'locale':locale,'issue':'no established UI terminology metadata'}); continue
        for term,options in forms.items():
            pattern=re.compile(r'(?<![A-Za-z])'+re.escape(term).replace(r'\ ',r'[ -]')+(r's?' if term.lower() in ('preset','prebuild') else '')+r'(?![A-Za-z])',re.I)
            for key,pivot in english.items():
                prose=re.sub(r'`[^`]+`','',pivot)
                prose=re.sub(r'\]\([^)]*\)',']',prose)
                if key not in values or not pattern.search(prose): continue
                result['checked']+=1; value=values[key]
                if any(normalized(option) in normalized(value) for option in options): continue
                item={'locale':locale,'block':key,'term':term,'approved_forms':options,'english':pivot,'translation':value}
                exception=review.get('term_exceptions',{}).get(key,{}).get(term,{})
                if exception.get('reason') and exception.get('source_sha256')==docs.digest(pivot) and exception.get('value_sha256')==docs.digest(value):
                    item['reason']=exception['reason']; result['exceptions'].append(item)
                else: result['issues'].append(item)
        for key,block in source['blocks'].items():
            if key not in values: continue
            for pose in poses:
                original=block['text']
                if not ('「'+pose+'」' in original or '('+pose+')' in original or '（'+pose+'）' in original): continue
                expected=display[pose]; result['checked']+=1
                if normalized(expected) in normalized(values[key]): continue
                term='pose:'+pose
                item={'locale':locale,'block':key,'term':term,'approved_forms':[expected],'english':english[key],'translation':values[key]}
                exception=review.get('term_exceptions',{}).get(key,{}).get(term,{})
                if exception.get('reason') and exception.get('display')==expected and exception.get('source_sha256')==docs.digest(english[key]) and exception.get('value_sha256')==docs.digest(values[key]):
                    item['reason']=exception['reason']; result['exceptions'].append(item)
                else: result['issues'].append(item)
    return result

def main():
    p=argparse.ArgumentParser(description=__doc__); p.add_argument('--locales',nargs='+'); p.add_argument('--report',type=Path,required=True); p.add_argument('--strict',action='store_true'); args=p.parse_args()
    result=audit(args.locales); args.report.parent.mkdir(parents=True,exist_ok=True)
    args.report.write_text(json.dumps(result,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps({'checked':result['checked'],'issues':len(result['issues']),'missing_languages':len(result['missing']),'reviewed_exceptions':len(result['exceptions'])}))
    return int(args.strict and bool(result['issues'] or result['missing']))

if __name__=='__main__': sys.exit(main())
