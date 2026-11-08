"""Regression checks for omissions that the former validator silently accepted."""
import copy
import unittest
from unittest.mock import patch
import audit_coverage as audit

class CoverageGateTests(unittest.TestCase):
    def test_missing_feature_description_is_reported_even_when_fallback_exists(self):
        data = copy.deepcopy(audit.inventory())
        description = next(iter(audit.feature_keys().values()))['description']
        data['fr'].pop(description, None)
        with patch.object(audit, 'inventory', return_value=data): result = audit.audit()
        self.assertIn(description, result['languages']['fr']['feature_missing'])
        self.assertIn(description, result['languages']['fr']['missing'])

    def test_new_english_string_requires_every_target_language(self):
        data = copy.deepcopy(audit.inventory())
        key = '翻訳追加漏れ検出用の新規機能'
        data['en'][key] = 'A new feature requiring translations'
        with patch.object(audit, 'inventory', return_value=data): result = audit.audit()
        for language, info in result['languages'].items():
            if language != 'en': self.assertIn(key, info['missing'], language)

    def test_deleting_generated_entry_is_detected_as_stale(self):
        original = audit.read
        def missing(path):
            value = original(path)
            if path.name == 'en.json': value['entries'] = value['entries'][1:]
            return value
        with patch.object(audit, 'read', side_effect=missing): result = audit.audit()
        self.assertIn('Stale generated catalog: en', result['errors'])

if __name__ == '__main__': unittest.main()
