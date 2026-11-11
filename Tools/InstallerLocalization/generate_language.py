"""Generate identical standalone language helpers for the product and setup programs."""
from pathlib import Path
import json,re,uuid
from bootstrap_embed import embed
HERE=Path(__file__).resolve().parent
REPO=HERE.parents[1]
def quoted(value):return json.dumps(value,ensure_ascii=False)
def dictionary(values):return 'new Dictionary<string, string> {\n'+''.join('    { '+quoted(k)+', '+quoted(v)+' },\n' for k,v in sorted(values.items()))+'}'
def render():
    ui={'en':{}}
    for table in sorted((REPO/'Tools/Localization').glob('*.translations.json')):
        if table.name != 'poses.translations.json': ui['en'].update(json.loads(table.read_text(encoding='utf-8')).get('en',{}))
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
            # Reviewed complete phrases take precedence in EVERY language. These
            # models were originally a fallback while only basic setup terms were
            # translated. Overwriting completed license captions with a multiline
            # model also put newlines inside native menu paths.
            entries.setdefault(source, '\n'.join(entries.get(line, line) for line in model.split('\n')))
    for locale, entries in translations.items():
        if locale == 'ja': continue
        for product in ['Kawaii Posing', 'Cool Posing', 'Posing System']:
            # Prefer reviewed complete phrases; composition is a fallback for newly
            # added products. Keep native menus, Inspector and setup terminology equal.
            entries.setdefault('Install ' + product + ' license', entries['License installation'] + ' - ' + product)
            entries.setdefault(product + ' license status', entries['License status'] + ' - ' + product)
            entries.setdefault('Remove ' + product + ' license', entries['Remove licenses'] + ' - ' + product)
            entries.setdefault('Add ' + product, entries['Add'] + ' ' + product)
    # Permit a setup dialog to be redrawn after choosing a different language.
    for entries in translations.values():
        for key,value in entries.items():
            if value.strip():aliases.setdefault(value.strip(),key)
    # Older builds installed multiline native license captions and stored paths
    # with newline delimiters. Retain their exact captions as migration aliases so
    # the registrar can recognize and remove all old-language copies on upgrade.
    # Prefer the native status key over the synonymous plural dialog title.
    for product in ['Kawaii Posing', 'Cool Posing']:
        for caption in ['license status', 'license removal']:
            canonical = product + ' ' + caption
            model = data.get('message_models', {}).get(canonical)
            if not model: continue
            for entries in [dict(), *translations.values()]:
                old_caption = '\n'.join(entries.get(line, line) for line in model.split('\n'))
                aliases[old_caption] = canonical
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
    arguments='new Dictionary<string, int[]> { '+', '.join('{ '+quoted(key)+', new[] { '+', '.join(map(str,indices))+' } }' for key,indices in data.get('localized_arguments',{}).items())+' }'
    rendered=rendered.replace('__LOCALIZED_ARGUMENTS__',arguments)
    rendered+='\n'+(HERE/'SetupMenus.cs.in').read_text(encoding='utf-8')
    rendered=embed(rendered,HERE/'Bootstrap')
    dest=REPO/'Packages/jp.unisakistudio.posingsystem/Editor/Setup/SetupLanguage.cs';dest.parent.mkdir(exist_ok=True);dest.write_text(rendered,encoding='utf-8')
    for path in [dest.parent,dest]:
        meta=Path(str(path)+'.meta')
        if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n'+('folderAsset: yes\n' if path.is_dir() else ''),encoding='utf-8')
    return dest
if __name__=='__main__':print(render())
