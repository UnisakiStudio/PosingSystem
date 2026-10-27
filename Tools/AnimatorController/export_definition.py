"""One-time migration tool: emit a versioned C# definition from the owned Unity template.

The product generator does not load YAML or the template at runtime. PyYAML is only
required to run this developer migration tool. Unknown object kinds fail closed.
"""
import argparse
import hashlib
import json
import pathlib
import re

import yaml

COMMON_FIELDS = {'m_ObjectHideFlags', 'm_CorrespondingSourceObject', 'm_PrefabInstance', 'm_PrefabAsset', 'm_Name', 'serializedVersion'}
SCHEMA = {
    'AnimatorController': {'m_AnimatorParameters', 'm_AnimatorLayers'},
    'AnimatorStateMachine': {'m_AnyStatePosition', 'm_AnyStateTransitions', 'm_ChildStateMachines', 'm_ChildStates',
        'm_DefaultState', 'm_EntryPosition', 'm_EntryTransitions', 'm_ExitPosition', 'm_ParentStateMachinePosition',
        'm_StateMachineBehaviours', 'm_StateMachineTransitions'},
    'AnimatorState': {'m_CycleOffset', 'm_CycleOffsetParameter', 'm_CycleOffsetParameterActive', 'm_IKOnFeet',
        'm_Mirror', 'm_MirrorParameter', 'm_MirrorParameterActive', 'm_Motion', 'm_Position', 'm_Speed',
        'm_SpeedParameter', 'm_SpeedParameterActive', 'm_StateMachineBehaviours', 'm_Tag', 'm_TimeParameter',
        'm_TimeParameterActive', 'm_Transitions', 'm_WriteDefaultValues'},
    'AnimatorTransition': {'m_Conditions', 'm_DstState', 'm_DstStateMachine', 'm_IsExit', 'm_Mute', 'm_Solo'},
    'BlendTree': {'m_BlendParameter', 'm_BlendParameterY', 'm_BlendType', 'm_Childs', 'm_MaxThreshold', 'm_MinThreshold',
        'm_NormalizedBlendValues', 'm_UseAutomaticThresholds'},
}
SCHEMA['AnimatorStateTransition'] = SCHEMA['AnimatorTransition'] | {'m_CanTransitionToSelf', 'm_ExitTime',
    'm_HasExitTime', 'm_HasFixedDuration', 'm_InterruptionSource', 'm_OrderedInterruption', 'm_TransitionDuration', 'm_TransitionOffset'}
BEHAVIOURS = {'parameters': 'VRCAvatarParameterDriver', 'trackingHead': 'VRCAnimatorTrackingControl',
              'disableLocomotion': 'VRCAnimatorLocomotionControl', 'enterPoseSpace': 'VRCAnimatorTemporaryPoseSpace'}
BEHAVIOUR_COMMON = COMMON_FIELDS | {'m_GameObject', 'm_Enabled', 'm_EditorHideFlags', 'm_Script', 'm_EditorClassIdentifier', 'debugString'}


def behaviour_lines(identity, data):
    kinds = [v for k, v in BEHAVIOURS.items() if k in data]
    assert len(kinds) == 1, ('Unknown behaviour', identity)
    kind = kinds[0]
    script_ids = {'VRCAvatarParameterDriver': -706344726, 'VRCAnimatorTrackingControl': -646210727,
                  'VRCAnimatorLocomotionControl': -453519674, 'VRCAnimatorTemporaryPoseSpace': 141706016}
    assert data['m_Script'] == {'fileID': script_ids[kind], 'guid': '67cc4cb7839cd3741b63733d5adf0442', 'type': 3}, ('Unknown SDK script', data['m_Script'])
    rows = [f'var o = ({kind})objects[{identity}L];', 'o.name = ' + cs_string(data['m_Name'] or '') + ';',
            'o.debugString = ' + cs_string(data['debugString'] or '') + ';']
    extra = set(data) - BEHAVIOUR_COMMON
    if kind == 'VRCAvatarParameterDriver':
        assert extra == {'parameters', 'localOnly'}, ('Unknown driver fields', extra)
        params = []
        for p in data['parameters']:
            expected = {'type', 'name', 'source', 'value', 'valueMin', 'valueMax', 'chance', 'convertRange', 'sourceMin', 'sourceMax', 'destMin', 'destMax'}
            assert set(p) == expected, ('Unknown parameter fields', set(p))
            fields = []
            for k, v in p.items():
                value = ('(VRC.SDKBase.VRC_AvatarParameterDriver.ChangeType)' + str(v) if k == 'type' else
                         cs_string(v or '') if k in {'name', 'source'} else
                         ('true' if v else 'false') if k == 'convertRange' else num(v))
                fields.append(k + ' = ' + value)
            params.append('new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter { ' + ', '.join(fields) + ' }')
        rows += ['o.parameters = new List<VRC.SDKBase.VRC_AvatarParameterDriver.Parameter> { ' + ', '.join(params) + ' };',
                 'o.localOnly = ' + ('true' if data['localOnly'] else 'false') + ';']
    elif kind == 'VRCAnimatorTrackingControl':
        expected = {'tracking' + p for p in ['Head', 'LeftHand', 'RightHand', 'Hip', 'LeftFoot', 'RightFoot', 'LeftFingers', 'RightFingers', 'Eyes', 'Mouth']}
        assert extra == expected, ('Unknown tracking fields', extra)
        rows += ['o.' + k + ' = (VRC.SDKBase.VRC_AnimatorTrackingControl.TrackingType)' + str(data[k]) + ';' for k in sorted(extra)]
    else:
        expected = {'disableLocomotion'} if kind == 'VRCAnimatorLocomotionControl' else {'enterPoseSpace', 'fixedDelay', 'delayTime'}
        assert extra == expected, ('Unknown control fields', extra)
        rows += ['o.' + k + ' = ' + (num(data[k]) if k == 'delayTime' else ('true' if data[k] else 'false')) + ';' for k in sorted(extra)]
    assert data['m_Enabled'] == 1 and data['m_EditorHideFlags'] == 0
    return rows


def load(path):
    raw = path.read_bytes()
    parts = re.split(r'^--- !u!(\d+) &(-?\d+)\s*$', raw.decode('utf-8-sig'), flags=re.M)
    objects = {}
    for i in range(1, len(parts), 3):
        record = yaml.safe_load(parts[i+2])
        assert len(record) == 1
        kind, data = next(iter(record.items()))
        identity = int(parts[i+1])
        assert identity not in objects
        objects[identity] = {'kind': kind, 'data': data}
    roots = [i for i, o in objects.items() if o['kind'] == 'AnimatorController']
    assert len(roots) == 1
    return objects, roots[0], hashlib.sha256(raw).hexdigest()


def references(value):
    if isinstance(value, dict):
        if 'fileID' in value:
            yield value
        else:
            for child in value.values(): yield from references(child)
    elif isinstance(value, list):
        for child in value: yield from references(child)


def reachable(objects, root):
    seen, pending = set(), [root]
    while pending:
        identity = pending.pop()
        if identity in seen: continue
        seen.add(identity)
        for ref in references(objects[identity]['data']):
            if ref['fileID'] and not ref.get('guid'):
                assert ref['fileID'] in objects, ('Dangling local reference', identity, ref)
                pending.append(ref['fileID'])
    return seen


def cs_string(value):
    return '@"' + value.replace('"', '""') + '"'


def num(value):
    return repr(float(value)) + 'f'


def native_lines(identity, obj, objects):
    d = obj['data']
    kind = obj['kind']
    assert not (set(d) - COMMON_FIELDS - SCHEMA[kind]), ('Unhandled fields', kind, set(d) - COMMON_FIELDS - SCHEMA[kind])
    variable = 'o'
    def string(v): return cs_string(v or '')
    def boolean(v): return 'true' if v else 'false'
    def vector(v): return 'new Vector3(' + ', '.join(num(v[k]) for k in 'xyz') + ')'
    def vector2(v): return 'new Vector2(' + ', '.join(num(v[k]) for k in 'xy') + ')'
    def ref(r, typ):
        if not r['fileID']: return 'null'
        if r.get('guid'): return f'({typ})external({string(r["guid"])}, {r["fileID"]}L)'
        assert r['fileID'] in objects
        return f'({typ})objects[{r["fileID"]}L]'
    def refs(values, typ): return f'new {typ}[] {{ ' + ', '.join(ref(r, typ) for r in values) + ' }'
    lines = [f'var o = ({kind})objects[{identity}L];', 'o.name = ' + string(d['m_Name']) + ';']
    def assign(prop, val): lines.append('o.' + prop + ' = ' + val + ';')
    if kind == 'AnimatorController':
        parameters = []
        for p in d['m_AnimatorParameters']:
            assert p['m_Controller']['fileID'] == identity
            parameters.append('new AnimatorControllerParameter { name = ' + string(p['m_Name']) +
                ', type = (AnimatorControllerParameterType)' + str(p['m_Type']) +
                ', defaultFloat = ' + num(p['m_DefaultFloat']) + ', defaultInt = ' + str(p['m_DefaultInt']) +
                ', defaultBool = ' + boolean(p['m_DefaultBool']) + ' }')
        assign('parameters', 'new AnimatorControllerParameter[] { ' + ', '.join(parameters) + ' }')
        layers = []
        for l in d['m_AnimatorLayers']:
            assert not l['m_Motions'] and not l['m_Behaviours'], 'Layer override schema needs explicit support'
            layers.append('new AnimatorControllerLayer { name = ' + string(l['m_Name']) +
                ', stateMachine = ' + ref(l['m_StateMachine'], 'AnimatorStateMachine') +
                ', avatarMask = ' + ref(l['m_Mask'], 'AvatarMask') +
                ', blendingMode = (AnimatorLayerBlendingMode)' + str(l['m_BlendingMode']) +
                ', syncedLayerIndex = ' + str(l['m_SyncedLayerIndex']) +
                ', defaultWeight = ' + num(l['m_DefaultWeight']) + ', iKPass = ' + boolean(l['m_IKPass']) +
                ', syncedLayerAffectsTiming = ' + boolean(l['m_SyncedLayerAffectsTiming']) + ' }')
        assign('layers', 'new AnimatorControllerLayer[] { ' + ', '.join(layers) + ' }')
    elif kind == 'AnimatorStateMachine':
        assign('states', 'new ChildAnimatorState[] { ' + ', '.join('new ChildAnimatorState { state = ' +
            ref(c['m_State'], 'AnimatorState') + ', position = ' + vector(c['m_Position']) + ' }' for c in d['m_ChildStates']) + ' }')
        assign('stateMachines', 'new ChildAnimatorStateMachine[] { ' + ', '.join('new ChildAnimatorStateMachine { stateMachine = ' +
            ref(c['m_StateMachine'], 'AnimatorStateMachine') + ', position = ' + vector(c['m_Position']) + ' }' for c in d['m_ChildStateMachines']) + ' }')
        assign('defaultState', ref(d['m_DefaultState'], 'AnimatorState'))
        assign('anyStateTransitions', refs(d['m_AnyStateTransitions'], 'AnimatorStateTransition'))
        assign('entryTransitions', refs(d['m_EntryTransitions'], 'AnimatorTransition'))
        assign('behaviours', refs(d['m_StateMachineBehaviours'], 'StateMachineBehaviour'))
        for p in ['AnyStatePosition', 'EntryPosition', 'ExitPosition', 'ParentStateMachinePosition']:
            assign(p[0].lower() + p[1:], vector(d['m_' + p]))
    elif kind == 'AnimatorState':
        for p in ['Speed', 'CycleOffset']: assign(p[0].lower() + p[1:], num(d['m_' + p]))
        for p in ['Mirror', 'IKOnFeet', 'WriteDefaultValues', 'SpeedParameterActive', 'MirrorParameterActive', 'CycleOffsetParameterActive', 'TimeParameterActive']:
            assign('iKOnFeet' if p == 'IKOnFeet' else p[0].lower() + p[1:], boolean(d['m_' + p]))
        for p in ['Tag', 'SpeedParameter', 'MirrorParameter', 'CycleOffsetParameter', 'TimeParameter']:
            assign(p[0].lower() + p[1:], string(d['m_' + p]))
        assign('motion', ref(d['m_Motion'], 'Motion'))
        assign('transitions', refs(d['m_Transitions'], 'AnimatorStateTransition'))
        assign('behaviours', refs(d['m_StateMachineBehaviours'], 'StateMachineBehaviour'))
        lines.append('using (var serialized = new SerializedObject(o)) { var legacy = serialized.FindProperty("m_Position"); '
            + 'if (legacy != null) { legacy.vector3Value = ' + vector(d['m_Position'])
            + '; serialized.ApplyModifiedPropertiesWithoutUndo(); } }')
    elif kind in ['AnimatorTransition', 'AnimatorStateTransition']:
        assert all(c['m_ConditionMode'] in {1, 2, 3, 4, 6, 7} for c in d['m_Conditions']), 'Unknown condition mode'
        assign('destinationState', ref(d['m_DstState'], 'AnimatorState'))
        assign('destinationStateMachine', ref(d['m_DstStateMachine'], 'AnimatorStateMachine'))
        for p in ['IsExit', 'Solo', 'Mute']: assign(p[0].lower() + p[1:], boolean(d['m_' + p]))
        assign('conditions', 'new AnimatorCondition[] { ' + ', '.join('new AnimatorCondition { parameter = ' +
            string(c['m_ConditionEvent']) + ', mode = (AnimatorConditionMode)' + str(c['m_ConditionMode']) +
            ', threshold = ' + num(c['m_EventTreshold']) + ' }' for c in d['m_Conditions']) + ' }')
        if kind == 'AnimatorStateTransition':
            for source, prop in [('TransitionDuration', 'duration'), ('TransitionOffset', 'offset'), ('ExitTime', 'exitTime')]:
                assign(prop, num(d['m_' + source]))
            for p in ['HasExitTime', 'HasFixedDuration', 'OrderedInterruption', 'CanTransitionToSelf']:
                assign(p[0].lower() + p[1:], boolean(d['m_' + p]))
            assign('interruptionSource', '(TransitionInterruptionSource)' + str(d['m_InterruptionSource']))
    elif kind == 'BlendTree':
        assign('blendType', '(BlendTreeType)' + str(d['m_BlendType']))
        assign('blendParameter', string(d['m_BlendParameter']))
        assign('blendParameterY', string(d['m_BlendParameterY']))
        assign('useAutomaticThresholds', 'false')
        assign('children', 'new ChildMotion[] { ' + ', '.join('new ChildMotion { motion = ' + ref(c['m_Motion'], 'Motion') +
            ', threshold = ' + num(c['m_Threshold']) + ', position = ' + vector2(c['m_Position']) +
            ', timeScale = ' + num(c['m_TimeScale']) + ', cycleOffset = ' + num(c['m_CycleOffset']) +
            ', directBlendParameter = ' + string(c['m_DirectBlendParameter']) + ', mirror = ' + boolean(c['m_Mirror']) + ' }'
            for c in d['m_Childs']) + ' }')
        assign('minThreshold', num(d['m_MinThreshold']))
        assign('maxThreshold', num(d['m_MaxThreshold']))
        assign('useAutomaticThresholds', boolean(d['m_UseAutomaticThresholds']))
        lines.append('using (var serialized = new SerializedObject(o)) { serialized.FindProperty("m_NormalizedBlendValues").boolValue = ' +
            boolean(d['m_NormalizedBlendValues']) + '; serialized.ApplyModifiedPropertiesWithoutUndo(); }')
    else: raise ValueError(kind)
    return lines


def emit(objects, root, digest):
    seen = reachable(objects, root)
    rows = ['// Generated from the owned reference template. See docs/animator-controller-master.md.',
            '// Regenerate deliberately with Tools/AnimatorController/export_definition.py; review semantic tests.',
            '#if NDMF && MODULAR_AVATAR', 'using System;', 'using System.Collections.Generic;',
            'using UnityEditor;', 'using UnityEditor.Animations;', 'using UnityEngine;', 'using Object = UnityEngine.Object;',
            'using VRC.SDK3.Avatars.Components;', 'namespace jp.unisakistudio.posingsystemeditor', '{',
            '    internal static partial class PosingSystemControllerCompiler', '    {',
            f'        internal const string ReferenceSha256 = "{digest}";',
            f'        internal const long RootId = {root}L;',
            '        private static readonly DefinitionNode[] Definition = new DefinitionNode[]', '        {']
    supported = {'AnimatorController', 'AnimatorStateMachine', 'AnimatorState', 'AnimatorTransition',
                 'AnimatorStateTransition', 'BlendTree'}
    native = []
    for identity in sorted(seen):
        obj = objects[identity]
        data = obj['data']
        kind = obj['kind']
        for field in ['m_CorrespondingSourceObject', 'm_PrefabInstance', 'm_PrefabAsset', 'm_GameObject']:
            if field in data: assert data[field] == {'fileID': 0}, ('Unsupported prefab-linked graph', identity, field)
        if kind == 'MonoBehaviour':
            types = [name for key, name in BEHAVIOURS.items() if key in data]
            assert len(types) == 1, ('Unknown behaviour', identity, data)
            kind = types[0]
        else: assert kind in supported, ('Unknown object', kind)
        native.append((identity, obj))
        name = (data.get('m_Name') or kind).replace('\n', ' ').replace('\r', ' ')
        rows += [f'            // {kind}: {name}',
                 f'            new DefinitionNode({identity}L, typeof({kind})),']
    rows += ['        };', '        private static void ConfigureNative(Dictionary<long, Object> objects, Func<string, long, Object> external)', '        {']
    chunks = [native[i:i+40] for i in range(0, len(native), 40)]
    rows += [f'            Configure{i}(objects, external);' for i in range(len(chunks))]
    # This Unity API discards transitions whose destinations are not configured yet.
    # All graph nodes (including positive file IDs) must be initialized first.
    for identity, obj in native:
        if obj['kind'] == 'AnimatorStateMachine':
            for pair in obj['data']['m_StateMachineTransitions']:
                transitions = ', '.join('(AnimatorTransition)objects[' + str(t['fileID']) + 'L]' for t in pair['second'])
                rows.append(f'            ((AnimatorStateMachine)objects[{identity}L]).SetStateMachineTransitions('
                            f'(AnimatorStateMachine)objects[{pair["first"]["fileID"]}L], new AnimatorTransition[] {{ {transitions} }});')
    rows += ['        }']
    for i, chunk in enumerate(chunks):
        rows += [f'        private static void Configure{i}(Dictionary<long, Object> objects, Func<string, long, Object> external)', '        {']
        for identity, obj in chunk:
            rows += ['            {']
            lines = behaviour_lines(identity, obj['data']) if obj['kind'] == 'MonoBehaviour' else native_lines(identity, obj, objects)
            rows += ['                ' + line for line in lines]
            rows += ['            }']
        rows += ['        }']
    rows += ['    }', '}', '#endif', '']
    return '\n'.join(rows)


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--template', type=pathlib.Path, required=True)
    parser.add_argument('--output', type=pathlib.Path, required=True)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    objects, root, digest = load(args.template)
    result = emit(objects, root, digest)
    if args.check:
        assert args.output.read_text(encoding='utf-8') == result, 'Generated definition differs'
    else:
        args.output.parent.mkdir(parents=True, exist_ok=True)
        args.output.write_text(result, encoding='utf-8', newline='\n')
    print(json.dumps({'source_sha256': digest, 'objects': len(objects), 'reachable': len(reachable(objects, root)),
                      'generated_bytes': len(result.encode('utf-8'))}))


if __name__ == '__main__': main()
