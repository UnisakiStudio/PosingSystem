import copy
import pathlib
import unittest

import export_definition as exporter


class DefinitionMigrationTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.root = pathlib.Path(__file__).resolve().parents[2]
        cls.objects, cls.identity, cls.digest = exporter.load(cls.root / 'Packages/jp.unisakistudio.posingsystem/Resources/PosingSystem_Locomotion.controller')

    def test_reference_inventory_is_frozen(self):
        self.assertEqual(len(self.objects), 749)
        self.assertEqual(len(exporter.reachable(self.objects, self.identity)), 375)
        self.assertEqual(self.digest, '462fa9b486999c6e53727dffa657bc24c80dff31fd4242c06efec92dc8bcd6c7')

    def test_migration_is_deterministic_and_matches_checked_source(self):
        first = exporter.emit(self.objects, self.identity, self.digest)
        second = exporter.emit(dict(reversed(list(self.objects.items()))), self.identity, self.digest)
        self.assertEqual(first, second)
        self.assertEqual(first, (self.root / 'Packages/jp.unisakistudio.posingsystem/Editor/PosingSystemControllerDefinition.Generated.cs').read_text(encoding='utf-8'))

    def test_unknown_native_property_is_not_silently_dropped(self):
        changed = copy.deepcopy(self.objects)
        changed[self.identity]['data']['m_UnexpectedFutureFeature'] = 1
        with self.assertRaises(AssertionError): exporter.emit(changed, self.identity, self.digest)

    def test_unknown_object_kind_is_not_silently_dropped(self):
        changed = copy.deepcopy(self.objects)
        changed[self.identity]['kind'] = 'UnexpectedNativeType'
        with self.assertRaises(AssertionError): exporter.emit(changed, self.identity, self.digest)

    def test_missing_local_reference_fails(self):
        changed = copy.deepcopy(self.objects)
        child = changed[self.identity]['data']['m_AnimatorLayers'][0]['m_StateMachine']['fileID']
        del changed[child]
        with self.assertRaises(AssertionError): exporter.reachable(changed, self.identity)

    def test_future_condition_mode_fails(self):
        changed = copy.deepcopy(self.objects)
        node = next(changed[i] for i in exporter.reachable(changed, self.identity)
                    if changed[i]['kind'] == 'AnimatorStateTransition' and changed[i]['data']['m_Conditions'])
        node['data']['m_Conditions'][0]['m_ConditionMode'] = 999
        with self.assertRaises(AssertionError): exporter.emit(changed, self.identity, self.digest)

    def test_unknown_behaviour_field_fails(self):
        changed = copy.deepcopy(self.objects)
        node = next(changed[i] for i in exporter.reachable(changed, self.identity) if changed[i]['kind'] == 'MonoBehaviour')
        node['data']['futureSdkBehaviourOption'] = 1
        with self.assertRaises(AssertionError): exporter.emit(changed, self.identity, self.digest)

    def test_different_script_with_same_fields_is_not_assumed_to_be_sdk(self):
        changed = copy.deepcopy(self.objects)
        node = next(changed[i] for i in exporter.reachable(changed, self.identity) if changed[i]['kind'] == 'MonoBehaviour')
        node['data']['m_Script']['guid'] = '0' * 32
        with self.assertRaises(AssertionError): exporter.emit(changed, self.identity, self.digest)

    def test_prefab_reference_is_not_silently_discarded(self):
        changed = copy.deepcopy(self.objects)
        changed[self.identity]['data']['m_PrefabAsset'] = {'fileID': 1, 'guid': '0' * 32, 'type': 3}
        with self.assertRaises(AssertionError): exporter.emit(changed, self.identity, self.digest)


if __name__ == '__main__': unittest.main()
