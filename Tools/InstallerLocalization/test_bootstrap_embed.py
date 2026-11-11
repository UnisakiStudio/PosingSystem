"""Offline regressions for reproducible, SHA-bound bootstrap caption inputs."""
from pathlib import Path
import hashlib
import io
import json
import shutil
import subprocess
import tarfile
import tempfile
import unittest

from bootstrap_embed import REMEMBER, embed, picker_span, validate_captions

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[1]
ASSETS = HERE / 'Bootstrap'
HELPER = REPO / 'Packages/jp.unisakistudio.posingsystem/Editor/Setup/SetupLanguage.cs'


def base_helper():
    # Reconstruct only the original private picker from its authoritative
    # template. No generator writes, Unity, preferences or license operations.
    generated = HELPER.read_text('utf-8-sig')
    base = generated.split('\n// BEGIN REVIEWED BOOTSTRAP CAPTION OVERLAY\n', 1)[0]
    start, end = picker_span(base)
    template = (HERE / 'SetupLanguage.cs.in').read_text('utf-8-sig')
    tstart, tend = picker_span(template)
    return base[:start] + template[tstart:tend] + base[end:]


class BootstrapEmbedTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.base = base_helper()
        cls.expected = HELPER.read_text('utf-8-sig')
        cls.manifest = json.loads((ASSETS / 'caption-manifest.json').read_text('utf-8'))

    def test_manifest_raw_hashes_bind_canonical_lf_files(self):
        self.assertEqual(4, len(self.manifest['files']))
        for name, expected in self.manifest['files'].items():
            with self.subTest(name=name):
                data = (ASSETS / name).read_bytes()
                self.assertNotIn(b'\r', data)
                self.assertEqual(expected, hashlib.sha256(data).hexdigest())
        validate_captions(self.base, ASSETS)

    def test_windows_and_unix_checkout_attributes_force_lf(self):
        paths = ['Tools/InstallerLocalization/Bootstrap/' + name
                 for name in [*self.manifest['files'], 'caption-manifest.json']]
        # Per-file attributes take precedence over core.autocrlf. This is
        # read-only and deliberately also tests a hypothetical Windows setting.
        for mode in ('true', 'input', 'false'):
            with self.subTest(core_autocrlf=mode):
                result = subprocess.run(['git', '-c', 'core.autocrlf=' + mode,
                    'check-attr', 'eol', '--stdin'], cwd=REPO,
                    input=('\n'.join(paths)+'\n').encode('utf-8'), capture_output=True, check=True)
                lines = result.stdout.decode('utf-8').splitlines()
                self.assertEqual(len(paths), len(lines))
                self.assertTrue(all(line.endswith(': eol: lf') for line in lines), lines)

    def test_git_clean_lf_archive_round_trip_reproduces_generated_helper(self):
        archive = io.BytesIO()
        with tarfile.open(fileobj=archive, mode='w') as tar:
            for path in ASSETS.iterdir():
                if path.is_file():
                    # Git's clean/archive representation is LF. Do not trust
                    # a local CRLF working copy or copy its stale hash manifest.
                    data = path.read_bytes().replace(b'\r\n', b'\n')
                    member = tarfile.TarInfo(path.name)
                    member.size = len(data)
                    tar.addfile(member, io.BytesIO(data))
        with tempfile.TemporaryDirectory() as folder:
            restored = Path(folder)
            archive.seek(0)
            with tarfile.open(fileobj=archive, mode='r') as tar:
                for member in tar.getmembers():
                    self.assertEqual(member.name, Path(member.name).name)
                    (restored / member.name).write_bytes(tar.extractfile(member).read())
            validate_captions(self.base, restored)
            first = embed(self.base, restored)
            self.assertEqual(self.expected, first)
            self.assertEqual(first, embed(self.base, restored))

    def test_changed_logical_caption_is_rejected(self):
        caption = next(row for row in self.manifest['captions']
                       if row['locale'] == 'ar' and row['role'] == 'help')
        q = lambda value: json.dumps(value, ensure_ascii=False)
        pair = '{ ' + q(REMEMBER) + ', ' + q(caption['text']) + ' }'
        self.assertEqual(1, self.base.count(pair))
        changed = self.base.replace(pair, '{ ' + q(REMEMBER) + ', ' + q(caption['text'] + '!') + ' }')
        with self.assertRaisesRegex(ValueError, 'Caption translation changed'):
            embed(changed, ASSETS)

    def test_changed_inline_image_data_is_rejected(self):
        with tempfile.TemporaryDirectory() as folder:
            restored = Path(folder) / 'Bootstrap'
            shutil.copytree(ASSETS, restored)
            path = restored / 'CaptionImages.Data.cs.in'
            path.write_bytes(path.read_bytes() + b'// modified table\n')
            with self.assertRaisesRegex(ValueError, 'asset changed'):
                embed(self.base, restored)

    def test_double_embedding_is_rejected(self):
        with self.assertRaisesRegex(ValueError, 'already embedded'):
            embed(embed(self.base, ASSETS), ASSETS)


if __name__ == '__main__':
    unittest.main()
