"""Product-owned translations kept in each product repository.

Text shared by every posing product stays in this repository's tables. A product's
name, its own pose names and its own captions live in
<product repo>/Tools/Localization/*.translations.json and are generated into
<product package>/Editor/Localization/translations.json, which the product registers
with PosingSystemLocalization.RegisterProductCatalog. Setup text that must work before
a product package is installed stays in Tools/InstallerLocalization.

Only sibling checkouts that exist are processed, so a core-only CI checkout still works.
"""
from pathlib import Path
import json
import uuid

ROOT = Path(__file__).resolve().parents[2]
PRODUCTS = {'KawaiiPosing': 'jp.unisakistudio.kawaiiposing', 'CoolPosing': 'jp.unisakistudio.coolposing'}
CATALOG = 'Editor/Localization/translations.json'


def _read(path):
    def unique(pairs):
        result = {}
        for key, value in pairs:
            if key in result:
                raise ValueError('Duplicate JSON key: ' + key)
            result[key] = value
        return result
    return json.loads(path.read_text(encoding='utf-8-sig'), object_pairs_hook=unique)


class Product:
    def __init__(self, name, repo):
        self.name = name
        self.repo = repo
        self.package = repo / 'Packages' / PRODUCTS[name]
        self.tables_folder = repo / 'Tools/Localization'
        self.catalog_path = self.package / CATALOG

    def tables(self):
        """Merged {language: {source: translation}} of this product's reviewed tables."""
        merged = {}
        for path in sorted(self.tables_folder.glob('*.translations.json')):
            for language, entries in _read(path).items():
                target = merged.setdefault(language, {})
                for key, value in entries.items():
                    if key in target and target[key] != value:
                        raise ValueError(f'Conflicting translation: {self.name}/{language}/{key}')
                    if not isinstance(value, str) or not value.strip():
                        raise ValueError(f'Empty translation: {self.name}/{language}/{key}')
                    target[key] = value
        return merged

    def _names(self):
        path = self.tables_folder / 'display-names.json'
        return _read(path) if path.exists() else {}

    def pose_names(self):
        """This product's own pose names."""
        return sorted(self._names().get('poses', []))

    def display_names(self):
        """Pose and menu names that may be translated in lists and generated in-game menus."""
        names = self._names()
        return sorted(set(names.get('poses', [])) | set(names.get('menus', [])))

    def render(self):
        tables = self.tables()
        data = dict(product=PRODUCTS[self.name], displayNames=self.display_names(),
                    languages=[dict(code=language, entries=[dict(key=key, value=value) for key, value in sorted(entries.items())])
                               for language, entries in sorted(tables.items())])
        return json.dumps(data, ensure_ascii=False, indent=2) + '\n'

    def generated(self):
        """(display names, {language: {source: translation}}) of the checked-in catalog, or None."""
        if not self.catalog_path.exists():
            return None
        data = _read(self.catalog_path)
        return (list(data.get('displayNames', [])),
                {item['code']: {entry['key']: entry['value'] for entry in item['entries']} for item in data.get('languages', [])})

    def write(self):
        folder = self.catalog_path.parent
        folder.mkdir(parents=True, exist_ok=True)
        self.catalog_path.write_text(self.render(), encoding='utf-8')
        for path, is_folder in ((folder, True), (self.catalog_path, False)):
            meta = Path(str(path) + '.meta')
            if not meta.exists():
                meta.write_text('fileFormatVersion: 2\nguid: ' + uuid.uuid4().hex + '\n' + ('folderAsset: yes\n' if is_folder else ''), encoding='utf-8')
        return self.catalog_path


def checkouts():
    """Products whose sibling checkout has translation tables."""
    result = []
    for name in PRODUCTS:
        product = Product(name, ROOT.parent / name)
        if product.tables_folder.is_dir() and any(product.tables_folder.glob('*.translations.json')):
            result.append(product)
    return result


def problems(core, product):
    """Structural errors of one product against the core inventory {language: {key: value}}."""
    errors = []
    tables = product.tables()
    english = set(tables.get('en', {}))
    for language in sorted(set(core) - {'ja'}):
        missing = english - set(tables.get(language, {}))
        extra = set(tables.get(language, {})) - english
        if missing or extra:
            errors.append(f'{product.name}/{language}: {len(missing)} missing, {len(extra)} unexpected product translations')
    # One owner per text: the same source in the core and in a product would be ambiguous.
    overlap = sorted(english & set(core.get('en', {})))
    if overlap:
        errors.append(f'{product.name}: also defined in the shared tables: ' + ', '.join(overlap[:5]))
    unknown = sorted(set(product.display_names()) - english)
    if unknown:
        errors.append(f'{product.name}: display names without translations: ' + ', '.join(unknown[:5]))
    generated = product.generated()
    if generated is None or generated != (product.display_names(), tables):
        errors.append(f'{product.name}: stale generated product catalog')
    return errors
