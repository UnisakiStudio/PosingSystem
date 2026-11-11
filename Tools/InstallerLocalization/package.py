"""Create review copies of BOOTH setup packages; never overwrite an input or publish."""
from pathlib import Path
import argparse
import gzip
import hashlib
import io
import json
import tarfile
import uuid

NAMESPACE = uuid.UUID('80458ad0-046f-4f16-8e14-bf3ddd379a96')
HELPER_PATH = 'Assets/UniSakiStudio/Setup/Editor/UnisakiSetupLanguage.cs'
NOTICE_PATH = 'Assets/UniSakiStudio/Setup/VPMPackageAutoInstaller-LICENSE.txt'
UNITY_TAR_FILENAME = 'archtemp.tar'


def digest(data):
    return hashlib.sha256(data).hexdigest()


def normalize_gzip_header(data):
    """Preserve TAR bytes, but give Unity's two-stage extractor an ASCII name.

    An unnamed gzip inherits the outer unitypackage basename. Unity 2022.3 on
    Windows then fails its TAR extraction for Japanese outer filenames.
    """
    tar_bytes = gzip.decompress(data)
    output = io.BytesIO()
    with gzip.GzipFile(fileobj=output, mode='wb', mtime=0, filename=UNITY_TAR_FILENAME) as zipped:
        zipped.write(tar_bytes)
    return output.getvalue()


def package(original, build, output, dependencies):
    if original.resolve() == output.resolve() or output.exists():
        raise ValueError('Output must be a new file separate from the distribution input')
    with tarfile.open(original, 'r:gz') as archive:
        entries = {m.name: archive.extractfile(m).read() for m in archive if m.isfile()}
    paths = {name.rsplit('/', 1)[0]: data.decode('utf-8').strip('\x00\r\n')
             for name, data in entries.items() if name.endswith('/pathname')}
    changes = []
    config_found = False
    license_found = False
    binary_found = False
    for guid, path in paths.items():
        asset = guid + '/asset'
        replacement = None
        if path.endswith('LicenseActivator.cs'):
            replacement = (build / 'Sources' / Path(path).name).read_bytes()
            license_found = True
        elif path.endswith('/com.anatawa12.vpm-package-auto-installer.dll'):
            replacement = (build / 'Build/bin/Release/netstandard2.1/com.anatawa12.vpm-package-auto-installer.dll').read_bytes()
            binary_found = True
        elif path.endswith('/config.json'):
            config_found = True
            config = json.loads(entries[asset])
            for name, version in dependencies.items():
                if name not in config['vpmDependencies']:
                    raise ValueError('Dependency absent from original config: ' + name)
                config['vpmDependencies'][name] = version
            if dependencies:
                replacement = (json.dumps(config, ensure_ascii=False, indent=2) + '\n').encode('utf-8')
        if replacement is not None:
            changes.append(dict(path=path, before=digest(entries[asset]), after=digest(replacement)))
            entries[asset] = replacement
    if not all([license_found, binary_found, config_found]):
        raise ValueError('Input is not a complete recognized license/setup package')
    # Shared helper GUIDs stay stable across the two products and differ from the package copy.
    # Keep the helper outside the VPAI folder, which upstream removes after completion.
    additions = {
        HELPER_PATH: (build / 'Sources/SetupLanguage.cs').read_bytes(),
        NOTICE_PATH: (build / 'Sources/LICENSE-VPMPackageAutoInstaller.txt').read_bytes(),
    }
    folders = ['Assets/UniSakiStudio/Setup', 'Assets/UniSakiStudio/Setup/Editor']
    for path in folders + list(additions):
        guid = uuid.uuid5(NAMESPACE, path).hex
        if guid in paths and paths[guid] != path:
            raise ValueError('GUID collision: ' + guid)
        entries[guid + '/pathname'] = path.encode('utf-8')
        entries[guid + '/asset.meta'] = ('fileFormatVersion: 2\nguid: ' + guid + '\n' +
                                       ('folderAsset: yes\nDefaultImporter:\n  userData:\n  assetBundleName:\n  assetBundleVariant:\n' if path in folders else '')).encode('utf-8')
        if path in additions:
            entries[guid + '/asset'] = additions[path]
        changes.append(dict(path=path, added=True))
    output.parent.mkdir(parents=True, exist_ok=True)
    # Fixed timestamps and ordered entries make the same inputs reproducible.
    with output.open('wb') as raw:
        with gzip.GzipFile(fileobj=raw, mode='wb', mtime=0, filename=UNITY_TAR_FILENAME) as zipped:
            with tarfile.open(fileobj=zipped, mode='w') as archive:
                for name, data in sorted(entries.items()):
                    info = tarfile.TarInfo(name)
                    info.size = len(data)
                    info.mode = 0o644
                    archive.addfile(info, io.BytesIO(data))
    with tarfile.open(output, 'r:gz') as archive:
        actual = {m.name: archive.extractfile(m).read() for m in archive if m.isfile()}
    if actual != entries:
        raise RuntimeError('Package round-trip verification failed')
    report = dict(input=str(original), input_sha256=digest(original.read_bytes()),
                  output=str(output), output_sha256=digest(output.read_bytes()), changes=changes,
                  dependency_overrides=dependencies,
                  dependency_note='Original dependency ranges retained' if not dependencies else 'Explicit overrides applied',
                  imported_into_unity=False, published=False)
    output.with_suffix('.manifest.json').write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(output)


if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__)
    for name in ['input', 'build', 'output']:
        p.add_argument('--' + name, type=Path, required=True)
    p.add_argument('--dependency', action='append', default=[], metavar='NAME=RANGE')
    args = p.parse_args()
    dependencies = dict(value.split('=', 1) for value in args.dependency)
    package(args.input.resolve(), args.build.resolve(), args.output.resolve(), dependencies)
