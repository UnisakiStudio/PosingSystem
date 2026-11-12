"""Regression checks for Unity's Japanese-filename package extraction failure."""
import gzip
import io
import json
from pathlib import Path
import struct
import tarfile
import tempfile
import unittest
from package import package, normalize_gzip_header


def gzip_filename(raw):
    if not raw[3] & 8:
        return None
    offset = 10
    if raw[3] & 4:
        offset += 2 + struct.unpack('<H', raw[offset:offset + 2])[0]
    return raw[offset:raw.index(b'\0', offset)].decode('ascii')


class PackageTests(unittest.TestCase):
    def test_container_fix_preserves_every_tar_byte_and_has_ascii_internal_name(self):
        source = gzip.compress(b'unchanged TAR bytes' + bytes(range(256)), mtime=0)
        fixed = normalize_gzip_header(source)
        self.assertEqual(gzip.decompress(source), gzip.decompress(fixed))
        self.assertEqual(gzip_filename(fixed), 'archtemp.tar')
        self.assertEqual(fixed, normalize_gzip_header(source))
        self.assertEqual(fixed, normalize_gzip_header(fixed))

    def test_corrupt_input_is_rejected(self):
        with self.assertRaises((OSError, EOFError)):
            normalize_gzip_header(b'not a gzip package')

    def test_package_writer_and_header_repair_produce_same_compatible_container(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            original = root / 'original.unitypackage'
            build = root / 'build'
            (build / 'Sources').mkdir(parents=True)
            (build / 'Sources/TestLicenseActivator.cs').write_bytes(b'license fixture')
            (build / 'Sources/SetupLanguage.cs').write_bytes(b'language fixture')
            (build / 'Sources/LICENSE-VPMPackageAutoInstaller.txt').write_bytes(b'notice fixture')
            binary = build / 'Build/bin/Release/netstandard2.1/com.anatawa12.vpm-package-auto-installer.dll'
            binary.parent.mkdir(parents=True)
            binary.write_bytes(b'binary fixture')
            assets = [('a' * 32, 'Assets/TestLicenseActivator.cs', b'old license'),
                      ('b' * 32, 'Assets/Setup/com.anatawa12.vpm-package-auto-installer.dll', b'old binary'),
                      ('c' * 32, 'Assets/Setup/config.json', json.dumps({'vpmDependencies': {'example': '3.x.x'}}).encode())]
            with tarfile.open(original, 'w:gz') as archive:
                for guid, path, raw in assets:
                    for entry, value in [('pathname', path.encode()), ('asset', raw)]:
                        info = tarfile.TarInfo(guid + '/' + entry)
                        info.size = len(value)
                        archive.addfile(info, io.BytesIO(value))
            output = root / '日本語のツール.unitypackage'
            package(original, build, output, {})
            raw = output.read_bytes()
            self.assertEqual(gzip_filename(raw), 'archtemp.tar')
            self.assertEqual(raw, normalize_gzip_header(raw))
            with tarfile.open(output, 'r:gz') as archive:
                self.assertEqual(archive.extractfile('a' * 32 + '/asset').read(), b'license fixture')
                self.assertEqual(archive.extractfile('b' * 32 + '/asset').read(), b'binary fixture')
                config = json.load(archive.extractfile('c' * 32 + '/asset'))
                self.assertEqual(config, {'vpmDependencies': {'example': '3.x.x'}})
            with self.assertRaises(ValueError):
                package(original, build, output, {})
            # Pre-release bundles opt in explicitly; stable bundles never carry the flag.
            alpha = root / 'alpha.unitypackage'
            package(original, build, alpha, {'example': '>=3.2.0-alpha.1'}, include_prerelease=True)
            with tarfile.open(alpha, 'r:gz') as archive:
                self.assertEqual(json.load(archive.extractfile('c' * 32 + '/asset')),
                                 {'vpmDependencies': {'example': '>=3.2.0-alpha.1'}, 'includePrerelease': True})


if __name__ == '__main__':
    unittest.main()
