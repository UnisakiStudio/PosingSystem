"""Check localization coverage and preservation of existing license storage operations."""
from pathlib import Path
import argparse
import collections
import json
import re

HERE = Path(__file__).resolve().parent


def verify(inputs, sources):
    data = json.loads((HERE / 'bootstrap.translations.json').read_text(encoding='utf-8'))
    placeholders = lambda text: collections.Counter(re.findall(r'\{\d+\}', text))
    for locale, entries in data['translations'].items():
        for key in data['setup_keys']:
            if not entries.get(key, '').strip():
                raise ValueError(f'Missing setup translation: {locale}/{key}')
            if placeholders(key) != placeholders(entries[key]):
                raise ValueError(f'Placeholder mismatch: {locale}/{key}')
    installer = (sources / 'VPMPackageAutoInstaller.cs').read_text(encoding='utf-8')
    if '() => InstallerError.Message(e, SetupLanguage.Current)' not in installer or 'Debug.LogError(e);' not in installer:
        raise ValueError('Installer error details, language redraw or full Console logging were lost')
    for locale, entries in data['translations'].items():
        for key in ['Error installing packages', 'Please see console for more details']:
            model = data.get('message_models', {}).get(key, '')
            if not entries.get(key, '').strip() and not (model and all(entries.get(line, '').strip() for line in model.split('\n') if line.strip())):
                raise ValueError(f'Missing installer diagnostic translation: {locale}/{key}')
    # Check sensitive behavior as text, without loading activators into a running Editor.
    critical = re.compile(r'const string |Registry\.CurrentUser\.|\.SetValue\(|\.DeleteValue\(|\.GetValue\(|File\.(WriteAllText|ReadAllText|Delete|Exists)\(|Directory\.(CreateDirectory|Delete)\(')
    for original in inputs.glob('*LicenseActivator.cs'):
        before = original.read_text(encoding='utf-8-sig')
        after = (sources / original.name).read_text(encoding='utf-8-sig')
        select = lambda text: [line.strip() for line in text.splitlines() if critical.search(line)]
        if select(before) != select(after):
            raise ValueError('License storage logic changed: ' + original.name)
        # Unix file path selection must remain identical, including HOME handling.
        paths = lambda text: re.search(r'private static string GetLicenseFilePath\([^)]*\).*?\n        }', text, re.S)[0]
        if paths(before) != paths(after):
            raise ValueError('License file paths changed: ' + original.name)
    print(json.dumps(dict(setup_locales=len(data['translations']) + 1,
                          setup_terms=len(data['setup_keys']), license_storage_preserved=True)))


if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--inputs', type=Path, required=True)
    p.add_argument('--sources', type=Path, required=True)
    args = p.parse_args()
    verify(args.inputs, args.sources)
