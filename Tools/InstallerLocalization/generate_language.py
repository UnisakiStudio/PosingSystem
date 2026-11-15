"""Generate the language helper for the product package and for the setup programs.

Both come from the same templates and the same tables. Only the precompiled setup program
keeps its tables as C# literals (render_standalone). Everything Unity compiles reads them
from a data file (SetupLanguageData.bytes) on first use, one language at a time: the package
copy, and the copy the setup program leaves under Assets for the license scripts
(write_helper). As literals the tables were 7 MB of C#. They made every compilation of the
editor assembly about twenty seconds longer, and with the copy under Assets every change of
any script in the user's project about twelve seconds longer.
"""
from pathlib import Path
import json,re,struct,uuid
from bootstrap_embed import embed
HERE=Path(__file__).resolve().parent
REPO=HERE.parents[1]
def quoted(value):return json.dumps(value,ensure_ascii=False)
def dictionary(values):return 'new Dictionary<string, string> {\n'+''.join('    { '+quoted(k)+', '+quoted(v)+' },\n' for k,v in sorted(values.items()))+'}'
DATA_NAME='SetupLanguageData.bytes'
# Where the setup program puts the helper's data, and the GUID package.py gives that file.
ASSETS_NAMESPACE=uuid.UUID('80458ad0-046f-4f16-8e14-bf3ddd379a96')
ASSETS_DATA_PATH='Assets/UniSakiStudio/Setup/Editor/'+DATA_NAME
ASSETS_DATA_GUID=uuid.uuid5(ASSETS_NAMESPACE,ASSETS_DATA_PATH).hex
MAGIC=0x314C5355  # "USL1"
def _string(value):
    raw=value.encode('utf-8');length=len(raw);prefix=bytearray()
    while length>=0x80:prefix.append((length&0x7F)|0x80);length>>=7
    prefix.append(length)
    return bytes(prefix)+raw
def pack(sections):
    """Sections {name: {key: value}} in the layout read by SetupLanguageData (BinaryReader strings)."""
    bodies=[(name,b''.join(_string(k)+_string(v) for k,v in sorted(entries.items())),len(entries)) for name,entries in sorted(sections.items())]
    header=8+sum(len(_string(name))+12 for name,_,_ in bodies)
    out=bytearray(struct.pack('<ii',MAGIC,len(bodies)));offset=header
    for name,body,count in bodies:
        out+=_string(name)+struct.pack('<qi',offset,count);offset+=len(body)
    assert len(out)==header
    for _,body,_ in bodies:out+=body
    return bytes(out)
LOADER='''        // The tables are read from a data file beside this script, one section at a time.
        // As C# literals they were 7 MB of source in the editor assembly.
        private static class SetupLanguageData
        {
            private const string FileName = "__DATA_NAME__";
            private const int Magic = __MAGIC__;
            internal static string PathOverride;
            private static Dictionary<string, KeyValuePair<long, int>> index;
            private static readonly List<string> names = new List<string>();
            private static readonly Dictionary<string, Dictionary<string, string>> sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            private static string file;
            private static string Locate()
            {
                if (!string.IsNullOrEmpty(PathOverride)) return PathOverride;
                var package = UnityEditor.PackageManager.PackageInfo.FindForAssembly(typeof(Language).Assembly);
                if (package != null) return System.IO.Path.Combine(package.resolvedPath, "Editor", "Setup", FileName);
                // The copy left under Assets by the setup program: its data has a fixed GUID,
                // so it is found even if the folder was moved.
                var asset = AssetDatabase.GUIDToAssetPath("__ASSETS_DATA_GUID__");
                return System.IO.Path.GetFullPath(string.IsNullOrEmpty(asset) ? "__ASSETS_DATA_PATH__" : asset);
            }
            private static bool Open()
            {
                if (index != null) return index.Count > 0;
                index = new Dictionary<string, KeyValuePair<long, int>>(StringComparer.Ordinal);
                try
                {
                    file = Locate();
                    using (var stream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read))
                    using (var reader = new System.IO.BinaryReader(stream, System.Text.Encoding.UTF8))
                    {
                        if (reader.ReadInt32() != Magic) throw new System.IO.InvalidDataException("Unexpected header");
                        var count = reader.ReadInt32();
                        for (var i = 0; i < count; i++)
                        {
                            var name = reader.ReadString();
                            var offset = reader.ReadInt64();
                            index[name] = new KeyValuePair<long, int>(offset, reader.ReadInt32());
                            names.Add(name);
                        }
                    }
                }
                catch (Exception exception) when (exception is System.IO.IOException || exception is UnauthorizedAccessException || exception is ArgumentException)
                {
                    index.Clear(); names.Clear();
                    // Without the tables every text stays in its source language; say so once.
                    Debug.LogWarning("[PosingSystem] " + FileName + " could not be read (" + exception.Message + "). Reinstall the package to restore translated text.");
                }
                return index.Count > 0;
            }
            private static Dictionary<string, string> Load(string section)
            {
                if (sections.TryGetValue(section, out var result)) return result;
                sections[section] = result = new Dictionary<string, string>(StringComparer.Ordinal);
                if (!Open() || !index.TryGetValue(section, out var place)) return result;
                try
                {
                    using (var stream = new System.IO.FileStream(file, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.Read, 1 << 16))
                    using (var reader = new System.IO.BinaryReader(stream, System.Text.Encoding.UTF8))
                    {
                        stream.Seek(place.Key, System.IO.SeekOrigin.Begin);
                        for (var i = 0; i < place.Value; i++)
                        {
                            var key = reader.ReadString();
                            result[key] = reader.ReadString();
                        }
                    }
                }
                catch (Exception exception) when (exception is System.IO.IOException || exception is UnauthorizedAccessException)
                {
                    result.Clear();
                    Debug.LogWarning("[PosingSystem] " + FileName + " could not be read (" + exception.Message + ").");
                }
                return result;
            }
            // A translation that is unambiguous backwards is also the alias "translated text -> key".
            // Those entries are stored once, in their own sections, and read backwards here.
            internal static Dictionary<string, string> Aliases()
            {
                var result = new Dictionary<string, string>(Load("aliases"), StringComparer.Ordinal);
                if (!Open()) return result;
                foreach (var name in names)
                {
                    if (!name.StartsWith("reversible:", StringComparison.Ordinal)) continue;
                    foreach (var pair in Load(name))
                        if (!result.ContainsKey(pair.Value)) result[pair.Value] = pair.Key;
                }
                return result;
            }
            // Same lookup as the dictionary of dictionaries it replaces; a language is read when first asked for.
            internal sealed class Catalogs
            {
                private readonly Dictionary<string, Dictionary<string, string>> loaded = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
                internal bool TryGetValue(string language, out Dictionary<string, string> catalog)
                {
                    if (language == null) { catalog = null; return false; }
                    if (!loaded.TryGetValue(language, out catalog))
                    {
                        catalog = null;
                        if (Open() && (index.ContainsKey("catalog:" + language) || index.ContainsKey("reversible:" + language)))
                        {
                            catalog = new Dictionary<string, string>(Load("catalog:" + language), StringComparer.Ordinal);
                            foreach (var pair in Load("reversible:" + language)) catalog[pair.Key] = pair.Value;
                        }
                        loaded[language] = catalog;
                    }
                    return catalog != null;
                }
            }
        }
'''.replace('__DATA_NAME__',DATA_NAME).replace('__MAGIC__',str(MAGIC)).replace('__ASSETS_DATA_GUID__',ASSETS_DATA_GUID).replace('__ASSETS_DATA_PATH__',ASSETS_DATA_PATH)
def tables():
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
    for entries in translations.values():entries.setdefault('ok','OK')
    # Permit a setup dialog to be redrawn after choosing a different language.
    for entries in translations.values():
        for key,value in entries.items():
            # A case-only alias would turn every "OK" caption into the installer's lower-case key.
            if value.strip() and value.strip().lower()!=key.lower():aliases.setdefault(value.strip(),key)
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
    return codes,names,aliases,translations,data
def _finish(rendered,data,tables=None):
    arguments='new Dictionary<string, int[]> { '+', '.join('{ '+quoted(key)+', new[] { '+', '.join(map(str,indices))+' } }' for key,indices in data.get('localized_arguments',{}).items())+' }'
    rendered=rendered.replace('__LOCALIZED_ARGUMENTS__',arguments)
    rendered+='\n'+(HERE/'SetupMenus.cs.in').read_text(encoding='utf-8')
    return embed(rendered,HERE/'Bootstrap',tables)
def standalone_source():
    """The helper with its tables as C# literals, as the setup program compiles it."""
    codes,names,aliases,translations,data=tables()
    rendered=(HERE/'SetupLanguage.cs.in').read_text(encoding='utf-8-sig').replace('__CODES__',codes).replace('__NAMES__',names).replace('__ALIASES__','BuildAliases()')
    rendered=rendered.replace('__CATALOG_TYPE__','Dictionary<string, Dictionary<string, string>>')
    factories='        private static Dictionary<string, string> BuildAliases() => '+dictionary(aliases)+';\n'
    literal='new Dictionary<string, Dictionary<string, string>> {\n'
    for locale, entries in sorted(translations.items()):
        method='Build_'+locale.replace('-','_')
        literal+='{ '+quoted(locale)+', '+method+'() },\n'
        factories+='        private static Dictionary<string, string> '+method+'() => '+dictionary(entries)+';\n'
    literal+='}'
    factories+='        private static Dictionary<string, Dictionary<string, string>> BuildCatalogs() => '+literal+';\n'
    rendered=rendered.replace('__CATALOGS__','BuildCatalogs()').replace('__FACTORIES__',factories)
    return _finish(rendered,data)
def render_standalone(dest):
    dest=Path(dest);dest.parent.mkdir(parents=True,exist_ok=True);dest.write_text(standalone_source(),encoding='utf-8')
    return dest
def helper():
    """(source, data) of the helper that reads its tables from the data file."""
    codes,names,aliases,translations,data=tables()
    rendered=(HERE/'SetupLanguage.cs.in').read_text(encoding='utf-8-sig').replace('__CODES__',codes).replace('__NAMES__',names)
    rendered=rendered.replace('__ALIASES__','SetupLanguageData.Aliases()').replace('__CATALOG_TYPE__','SetupLanguageData.Catalogs')
    rendered=rendered.replace('__CATALOGS__','new SetupLanguageData.Catalogs()').replace('__FACTORIES__',LOADER)
    return _finish(rendered,data,(aliases,translations)),pack(split(aliases,translations))
def split(aliases,translations):
    """Sections of the data file. A translation whose text is the alias back to its own key is
    stored once ("reversible:<locale>"); the helper reads those entries backwards to rebuild
    the alias table. Checked here by rebuilding it the way the helper does."""
    sections={};produced={}
    for locale,entries in translations.items():
        reversible={key:value for key,value in entries.items() if aliases.get(value)==key}
        sections['reversible:'+locale]=reversible
        sections['catalog:'+locale]={key:value for key,value in entries.items() if key not in reversible}
        for key,value in reversible.items():produced[value]=key
    sections['aliases']={key:value for key,value in aliases.items() if key not in produced}
    rebuilt=dict(sections['aliases'])
    for name in sorted(sections):
        if name.startswith('reversible:'):
            for key,value in sections[name].items():rebuilt.setdefault(value,key)
    if rebuilt!=aliases:raise ValueError('The alias table cannot be rebuilt from the data file')
    for locale,entries in translations.items():
        if {**sections['catalog:'+locale],**sections['reversible:'+locale]}!=entries:raise ValueError('Catalog lost entries: '+locale)
    return sections
def write_helper(folder):
    """The helper and its data as the setup program ships them under Assets (same files as in the package)."""
    folder=Path(folder);folder.mkdir(parents=True,exist_ok=True)
    source,binary=helper()
    (folder/'SetupLanguage.cs').write_text(source,encoding='utf-8');(folder/DATA_NAME).write_bytes(binary)
    return folder
def render():
    rendered,packed=helper()
    dest=REPO/'Packages/jp.unisakistudio.posingsystem/Editor/Setup/SetupLanguage.cs';dest.parent.mkdir(exist_ok=True);dest.write_text(rendered,encoding='utf-8')
    binary=dest.parent/DATA_NAME;binary.write_bytes(packed)
    for path in [dest.parent,dest,binary]:
        meta=Path(str(path)+'.meta')
        if not meta.exists():meta.write_text('fileFormatVersion: 2\nguid: '+uuid.uuid4().hex+'\n'+('folderAsset: yes\n' if path.is_dir() else ''),encoding='utf-8')
    return dest
if __name__=='__main__':print(render())
