"""Regression checks for omissions that the former validator silently accepted."""
import copy
import unittest
from unittest.mock import patch
import audit_coverage as audit

class CoverageGateTests(unittest.TestCase):
    def test_shaped_facade_keeps_direct_literals_in_strict_display_scan(self):
        for method in ['Label', 'LayoutLabelField', 'LayoutHelpBox', 'LayoutButton',
                       'LayoutEditorToggle', 'LayoutTextField', 'LayoutSlider', 'LayoutPropertyField']:
            with self.subTest(method=method):
                issues = audit.display_path_issues('PosingSystemTextGUI.' + method + '("登録済み");', {'登録済み'}, set())
                self.assertEqual([(i['kind'], i['key']) for i in issues], [('unlocalized_display_literal', '登録済み')])

    def test_shaped_popup_arrays_are_still_checked_for_missing_conversion(self):
        source = 'string[] labels={"弱","強"}; PosingSystemTextGUI.LayoutPopup("Level",index,labels);'
        issues = audit.display_path_issues(source, {'弱', '強'}, set())
        self.assertEqual({i['kind'] for i in issues}, {'unlocalized_popup_options'})

    def test_shaped_translated_sink_and_array_projection_remain_valid(self):
        source = 'PosingSystemTextGUI.LayoutButton(PosingSystemLocalization.Text("表示")); string[] labels={"弱","強"}; PosingSystemTextGUI.LayoutPopup("Level",index,labels.Select(PosingSystemLocalization.Text).ToArray());'
        self.assertEqual(audit.display_path_issues(source, {'表示', '弱', '強'}, set()), [])

    def test_ndmf_display_name_cannot_bypass_localization_with_a_registered_literal(self):
        for expression in ['=> "登録済み" + Version;', '{ get { return "登録済み" + Version; } }']:
            source='public override string DisplayName '+expression
            issues=audit.display_path_issues(source, {'登録済み'}, set())
            self.assertEqual([(i['kind'],i['key']) for i in issues], [('unlocalized_plugin_display_name','登録済み')])

    def test_ndmf_translated_display_name_and_internal_identity_are_distinct(self):
        source='public override string QualifiedName => "固定された識別子"; public override string DisplayName { get { return PosingSystemLocalization.Text("表示名") + Version; } }'
        self.assertEqual(audit.display_path_issues(source, {'表示名'}, set()), [])

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

    def test_dialog_message_and_button_are_checked_beyond_first_argument(self):
        source='PosingSystemLocalization.Dialog("確認", "新しい本文", "新しい実行ボタン", "キャンセル");'
        issues=audit.display_path_issues(source, {'確認','キャンセル'}, set())
        self.assertEqual({i['key'] for i in issues},{'新しい本文','新しい実行ボタン'})

    def test_registered_text_still_requires_localization_at_direct_gui_sink(self):
        issues=audit.display_path_issues('GUILayout.Button("登録済みだが未変換");',{'登録済みだが未変換'},set())
        self.assertEqual(issues[0]['kind'],'unlocalized_display_literal')

    def test_native_menu_components_are_checked_without_any_text_call(self):
        source='[MenuItem("Tools/ゆにさきスタジオ/翻訳されない新機能")] void Run() {}'
        issues=audit.display_path_issues(source,{'ゆにさきスタジオ'},set())
        self.assertEqual([(i['kind'],i['key']) for i in issues],[('unregistered_native_menu_component','翻訳されない新機能')])

    def test_popup_option_suffix_is_checked_through_local_array(self):
        source='var choices=Enumerable.Range(1,5).Select(i=>i+"番").ToArray(); EditorGUILayout.Popup("Order",index,choices);'
        issues=audit.display_path_issues(source,set(),set())
        self.assertEqual(issues[0]['kind'],'unregistered_display_data')
        self.assertEqual(issues[0]['key'],'番')

    def test_localized_fallback_and_comment_do_not_raise_false_positive(self):
        source='// GUILayout.Button("コメント");\nGUI.Label(rect,PosingSystemLocalization.Text("stable.key","日本語の既定値"));'
        self.assertEqual(audit.display_path_issues(source,{'stable.key'},set()),[])

    def test_comment_marker_inside_string_preserves_following_display_call(self):
        source='var url="https://example.com"; GUILayout.Label("未翻訳");'
        self.assertEqual(audit.display_path_issues(source,set(),set())[0]['key'],'未翻訳')

    def test_registered_local_array_is_checked_for_missing_conversion(self):
        source='string[] labels={"弱","強"}; EditorGUILayout.Popup("Level",index,labels);'
        issues=audit.display_path_issues(source,{'弱','強'},set())
        self.assertEqual({i['kind'] for i in issues},{'unlocalized_popup_options'})

    def test_local_array_projection_is_accepted_when_each_key_is_registered(self):
        source='string[] labels={"弱","強"}; EditorGUILayout.Popup("Level",index,labels.Select(PosingSystemLocalization.Text).ToArray());'
        self.assertEqual(audit.display_path_issues(source,{'弱','強'},set()),[])

    def test_wrapping_array_use_does_not_hide_missing_key(self):
        source='string[] labels={"未登録"}; GUILayout.Label(PosingSystemLocalization.Text(labels[index]));'
        self.assertEqual(audit.display_path_issues(source,set(),set())[0]['key'],'未登録')

    def test_constant_concatenation_uses_the_full_registered_message(self):
        source='GUI.Label(rect,PosingSystemLocalization.Text("前半"+"後半"));'
        self.assertEqual(audit.display_path_issues(source,{'前半後半'},set()),[])

    def test_create_asset_menu_checks_caption_but_preserves_filename(self):
        source='[CreateAssetMenu(fileName="保存名",menuName="ゆにさきスタジオ/未登録項目")] class Asset {}'
        self.assertEqual([i['key'] for i in audit.display_path_issues(source,{'ゆにさきスタジオ'},set())],['未登録項目'])

    def test_diagnostic_summary_and_action_are_both_registered(self):
        source='throw PosingSystemDiagnostics.Failure("PS001","未登録の概要","未登録の対処",avatar);'
        self.assertEqual({i['key'] for i in audit.display_path_issues(source,set(),set())},{'未登録の概要','未登録の対処'})

    def test_exception_concatenation_checks_japanese_after_the_open_quote(self):
        source='throw new InvalidOperationException("「"+avatar.name+"」に必要な設定がありません。");'
        self.assertIn('」に必要な設定がありません。',{i['key'] for i in audit.display_path_issues(source,set(),set())})

    def test_known_exception_message_still_needs_conversion(self):
        source='throw new InvalidOperationException("未変換の例外");'
        self.assertEqual(audit.display_path_issues(source,{'未変換の例外'},set())[0]['kind'],'unlocalized_exception_literal')

    def test_experimental_exception_requires_documented_exact_exclusion(self):
        source='throw new InvalidOperationException("開発専用の比較機能");'
        self.assertEqual(audit.display_path_issues(source,set(),set(),{'開発専用の比較機能':'Experimental comparison tool only'}),[])

    def test_localized_exception_preserves_identifier_argument(self):
        source='throw new ArgumentException(PosingSystemLocalization.Text("既存の説明"),nameof(motion));'
        self.assertEqual(audit.display_path_issues(source,{'既存の説明'},set()),[])

    def test_format_for_checks_second_argument_without_localizing_custom_names(self):
        source='throw new InvalidOperationException(PosingSystemLocalization.FormatFor(ps,"未登録の{0}","カスタム名"));'
        self.assertEqual([i['key'] for i in audit.display_path_issues(source,set(),set())],['未登録の{0}'])

if __name__ == '__main__': unittest.main()
