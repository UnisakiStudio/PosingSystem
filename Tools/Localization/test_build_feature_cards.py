"""Pure regression tests: do not write generated catalogs or require Unity."""
import copy
import hashlib
import json
from pathlib import Path
import unittest

from audit_coverage import feature_keys, read
from build_feature_cards import build_translations

HERE = Path(__file__).resolve().parent
# Reviewed 8 x 31 key/value pairs from commit 20c73be, before the bad rebuild.
ORIGINAL_HASHES = {
    'fr': 'fd1335ca747f98d22e8709cf59fe2d5ddce5a1f0a8d4bf6e83349717f906acc3',
    'it': '989ed5912ce1f0d912458f3be3faa87e8bfa4c4ea3b35a8be4c48481dcbd6054',
    'zh-Hans': '52b5e49fb241068f5f33089147052666a3c9b274d14007c13e1b9ae163b0b376',
    'zh-Hant': '8ea66da8ca52914953b712035a45149ce99ab308f3fb32efa2450ae77a64b699',
    'ko': 'af1faa25a2493671b83eaa2f78f8cfd54aefed9c37c8ad86fad29a67d33a6f34',
    'th': '3e76bacc920b7baddad92d009004ef6599c93ccb62db0b73cabeb9f92321cd92',
    'de': '9e4aab20dc93fa37d976018cbf7a28a8dcce184fd0e4c22c0a18436bc4b79d85',
    'es': '55c3ff1f84650f86ec4c59f67c19286477ad8f61b000748962aaf3f9c44ab60f',
}


class FeatureCardBatchTests(unittest.TestCase):
    def setUp(self):
        self.source = read(HERE / 'feature-cards.ai.json')
        self.actual = feature_keys()

    def test_discovery_reordering_preserves_every_original_translation(self):
        original = build_translations(self.source, self.actual)
        reversed_actual = dict(reversed(list(self.actual.items())))
        rebuilt = build_translations(self.source, reversed_actual)
        self.assertEqual(json.dumps(original, ensure_ascii=False),
                         json.dumps(rebuilt, ensure_ascii=False))
        self.assertEqual(set(original), set(ORIGINAL_HASHES))
        self.assertEqual(len(self.source['features']), 19)
        self.assertEqual(len(self.source['title_paths']), 12)
        for language, entries in original.items():
            with self.subTest(language=language):
                self.assertEqual(len(entries), 31)
                canonical = json.dumps(entries, ensure_ascii=False, sort_keys=True,
                                       separators=(',', ':')).encode('utf-8')
                self.assertEqual(hashlib.sha256(canonical).hexdigest(), ORIGINAL_HASHES[language])

    def test_new_missing_and_renamed_paths_require_review(self):
        first = next(iter(self.actual))
        added = copy.deepcopy(self.actual)
        added['features.new'] = {'title': 'new', 'description': 'new explanation'}
        removed = copy.deepcopy(self.actual)
        removed.pop(first)
        renamed = copy.deepcopy(self.actual)
        renamed['features.renamed'] = renamed.pop(first)
        for name, actual in [('added', added), ('removed', removed), ('renamed', renamed)]:
            with self.subTest(change=name), self.assertRaisesRegex(ValueError, 'Review feature-card batch'):
                build_translations(self.source, actual)

    def test_changed_japanese_titles_and_descriptions_require_review(self):
        # Same count and same path set must not conceal a source-key edit.
        for path in self.actual:
            for field in ('title', 'description'):
                actual = copy.deepcopy(self.actual)
                actual[path][field] += ' changed'
                with self.subTest(path=path, field=field), self.assertRaisesRegex(ValueError, 'changed'):
                    build_translations(self.source, actual)

    def test_incomplete_or_ambiguous_batches_fail_without_dropping_rows(self):
        variants = []
        for field in ('titles', 'descriptions'):
            source = copy.deepcopy(self.source)
            source['languages']['fr'][field].pop()
            variants.append(source)
        source = copy.deepcopy(self.source)
        source['languages']['fr']['descriptions'][0] = ''
        variants.append(source)
        source = copy.deepcopy(self.source)
        source['features'].append(copy.deepcopy(source['features'][0]))
        variants.append(source)
        for value in (self.source['title_paths'][0], 'features.unreviewed'):
            source = copy.deepcopy(self.source)
            source['title_paths'][-1] = value
            variants.append(source)
        for index, source in enumerate(variants):
            with self.subTest(variant=index), self.assertRaises(ValueError):
                build_translations(source, self.actual)


if __name__ == '__main__':
    unittest.main()
