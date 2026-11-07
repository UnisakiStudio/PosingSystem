"""Generate identical standalone language helpers for the product and setup programs."""
from pathlib import Path
import json,re,uuid
HERE=Path(__file__).resolve().parent
REPO=HERE.parents[1]
def quoted(value):return json.dumps(value,ensure_ascii=False)
def dictionary(values):return 'new Dictionary<string, string> {\n'+''.join('    { '+quoted(k)+', '+quoted(v)+' },\n' for k,v in sorted(values.items()))+'}'
def render():
    ui=json.loads((REPO/'Tools/Localization/ui.translations.json').read_text(encoding='utf-8'))
    catalogs={p.stem:{e['key']:e['value'] for e in json.loads(p.read_text(encoding='utf-8'))['entries']} for p in (REPO/'Packages/jp.unisakistudio.posingsystem/Editor/Localization').glob('*.json')}
    source=(REPO/'Packages/jp.unisakistudio.posingsystem/Editor/PosingSystemLocalization.cs').read_text(encoding='utf-8')
    codes=re.search(r'Codes = (\{[^;]+\})',source)[1];names=re.search(r'Names = (\{[^;]+\})',source)[1]
    data=json.loads((HERE/'bootstrap.translations.json').read_text(encoding='utf-8'))
    aliases={key:value for key,value in ui['en'].items() if re.search('[\u3040-\u30ff\u4e00-\u9fff]',key)}
    translations={'ja':{v:k for k,v in aliases.items()}}
    for locale,catalog in catalogs.items():
        if locale=='en':continue
        translations[locale]={ui['en'][key]:value for key,value in catalog.items() if key in ui['en']}
    aliases.update(data['aliases'])
    for locale,entries in data['translations'].items():translations.setdefault(locale,{}).update(entries)
    for locale, entries in translations.items():
        for source, model in data.get('message_models', {}).items():
            # Preserve the full original Japanese wording where it already exists.
            if locale == 'ja' and source in entries: continue
            entries[source] = '\n'.join(entries.get(line, line) for line in model.split('\n'))
    for locale, entries in translations.items():
        if locale == 'ja': continue
        for product in ['Kawaii Posing', 'Cool Posing', 'Posing System']:
            entries['Install ' + product + ' license'] = entries['License installation'] + ' - ' + product
            entries[product + ' license status'] = entries['License status'] + ' - ' + product
            entries['Remove ' + product + ' license'] = entries['Remove licenses'] + ' - ' + product
            entries['Add ' + product] = entries['Add'] + ' ' + product
    # Permit a setup dialog to be redrawn after choosing a different language.
    for entries in translations.values():
        for key,value in entries.items():
            if value.strip():aliases.setdefault(value.strip(),key)
    rendered=(HERE/'SetupLanguage.cs.in').read_text(encoding='utf-8-sig').replace('__CODES__',codes).replace('__NAMES__',names).replace('__ALIASES__','BuildAliases()')
    factories='        private static Dictionary<string, string> BuildAliases() => '+dictionary(aliases)+';\n'
    literal='new Dictionary<string, Dictionary<string, string>> {\n'
    for locale, entries in sorted(translations.items()):
        method='Build_'+locale.replace('-','_')
        literal+='{ '+quoted(locale)+', '+method+'() },\n'
        factories+='        private static Dictionary<string, string> '+method+'() => '+dictionary(entries)+';\n'
    literal+='}'
    factories+='        private static Dictionary<string, Dictionary<string, string>> BuildCatalogs() => '+literal+';\n'
    rendered=rendered.replace('__CATALOGS__','BuildCatalogs()').replace('__FACTORIES__',factories)
    rendered+='\n'+(HERE/'SetupMenus.cs.in').read_text(encoding='utf-8')
    dest=REPO/'Packages/jp.unisakistudio.posingsystem/Editor/Setup/SetupLanguage.cs';dest.parent.mkdir(exist_ok=True);dest.write_text(rendered,encoding='utf-8')
    for path in [dest.parent,dest]:
        meta=Path(str(path)+'.meta')
        if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n'+('folderAsset: yes\n' if path.is_dir() else ''),encoding='utf-8')
    return dest
if __name__=='__main__':print(render())
