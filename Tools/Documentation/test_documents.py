"""Regression checks for real documentation loss/staleness and offline link failures."""
from pathlib import Path
from unittest import TestCase, main
import tempfile
from unittest.mock import patch
import json
import build_documents as docs

class DocumentationTests(TestCase):
    def test_missing_translation_is_not_filled_with_source_prose(self):
        source={'source_sha256':'frozen','blocks':{'one':{'text':'A full explanation.','language':'en','invariant':False}}}
        with tempfile.TemporaryDirectory() as directory, patch.object(docs,'HERE',Path(directory)):
            values,missing,issues=docs.translations(source,'fr')
            self.assertEqual(values,{})
            self.assertEqual(missing,['one'])

    def test_old_translation_provenance_is_rejected(self):
        source={'source_sha256':'new','blocks':{'one':{'text':'A full explanation.','language':'en','invariant':False}}}
        with tempfile.TemporaryDirectory() as directory, patch.object(docs,'HERE',Path(directory)):
            folder=Path(directory)/'translations'; folder.mkdir()
            (folder/'fr.json').write_text(json.dumps({'source_sha256':'old','translations':{'one':'Une explication complète.'}}),encoding='utf-8')
            values,missing,issues=docs.translations(source,'fr')
            self.assertFalse(missing)
            self.assertTrue(any('stale' in e['issue'] for e in issues))

    def test_release_date_link_code_and_item_loss_are_detected(self):
        source='## [3.1.2] - 2026-11-09\n- [Manual](guide.html) uses `Parameter`.\n- Another fix.'
        changed='## [3.1.1] - 2026-11-08\n- [Guide](wrong.html) uses `Changed`.'
        errors=docs.structure_errors(source,changed)
        for expected in ['link targets changed','inline code changed','release headings changed','historical dates changed','list items changed']:
            self.assertIn(expected,errors)

    def test_lost_table_row_is_rejected(self):
        source='| Feature | Bits |\n| --- | --- |\n| One | 8 |\n| Two | 16 |'
        self.assertIn('table structure changed',docs.structure_errors(source,source.rsplit('\n',1)[0]))

    def test_numeric_limit_loss_is_detected_but_decimal_localization_is_allowed(self):
        self.assertIn('numeric limits or values omitted',docs.structure_errors('Neutral at 50%, range 3m.','Neutre au centre, plage 3m.'))
        self.assertNotIn('numeric limits or values omitted',docs.structure_errors('Position 0.5, 3m.','Position 0,5, 3m.'))
        self.assertIn('package versions omitted or changed',docs.structure_errors('Requires NDMF 1.14.0.','Nécessite NDMF 1.13.0.'))
        self.assertIn('technical identifier changed: SleepDown',docs.structure_errors('SleepDown returns to Standing.','La pose revient à Standing.'))

    def test_rtl_html_preserves_logical_unicode_and_isolates_code(self):
        page=docs.html_page('# دليل\n\nنص `Parameter` و **عنوان**.','ar','')
        self.assertIn('lang="ar" dir="rtl"',page)
        self.assertIn('نص <code dir="ltr">Parameter</code>',page)
        self.assertIn('<strong>عنوان</strong>',page)

    def test_html_rejects_executable_link_and_escapes_raw_markup(self):
        with self.assertRaises(ValueError): docs.inline('[click](javascript:alert)')
        self.assertIn('&lt;script&gt;',docs.inline('<script>'))

    def test_localized_document_links_keep_original_legal_asset(self):
        root=Path(tempfile.gettempdir())/'documentation-test-package'
        target=root/'Documentation~'/'Localization'/'fr'
        text=docs.remap_links('[history](CHANGELOG.md) [terms](規約.pdf) [web](https://example.com/README.md)',root,target,root,'fr')
        self.assertIn('[history](CHANGELOG.md)',text)
        self.assertIn('../../../%E8%A6%8F%E7%B4%84.pdf',text)
        self.assertIn('https://example.com/README.md',text)

    def test_generated_navigation_is_not_mistaken_for_a_source_change(self):
        original='# Original\n\nSource paragraph.'
        with_navigation=original+'\n'+docs.NAV_START+'\nGenerated links.\n'+docs.NAV_END+'\n'
        self.assertEqual(docs.canonical(with_navigation),original)

if __name__=='__main__': main()
