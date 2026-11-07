"""Read activator sources from local distribution copies without importing a unitypackage."""
from pathlib import Path
import argparse
import hashlib
import json
import tarfile


def extract(packages, output):
    output.mkdir(parents=True, exist_ok=True)
    report = []
    for package in packages:
        with tarfile.open(package, 'r:gz') as archive:
            entries = {m.name: archive.extractfile(m).read() for m in archive if m.isfile()}
        for name, data in entries.items():
            if not name.endswith('/pathname'):
                continue
            path = data.decode('utf-8').strip('\x00\r\n')
            if not path.endswith('LicenseActivator.cs'):
                continue
            payload = entries[name.rsplit('/', 1)[0] + '/asset']
            target = output / Path(path).name
            if target.exists() and target.read_bytes() != payload:
                raise ValueError('Different source already exists: ' + target.name)
            target.write_bytes(payload)
            report.append(dict(package=package.name, source=path,
                               sha256=hashlib.sha256(payload).hexdigest()))
    (output / 'extracted-inputs.json').write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print('Extracted activator sources:', len(report))


if __name__ == '__main__':
    p = argparse.ArgumentParser(description=__doc__)
    p.add_argument('--output', type=Path, required=True)
    p.add_argument('packages', type=Path, nargs='+')
    args = p.parse_args()
    extract(args.packages, args.output)
