"""Generate the reference appendix; the human requirements remain in the master document."""
import pathlib
import export_definition as definition

ROOT = pathlib.Path(__file__).resolve().parents[2]
SOURCE = ROOT / 'Packages/jp.unisakistudio.posingsystem/Resources/PosingSystem_Locomotion.controller'


def generate():
    objects, root, digest = definition.load(SOURCE)
    controller = objects[root]['data']
    rows = ['# AnimatorController 現行参照付録', '',
            'この付録は移行元の参照グラフの機械的な記録。正しい仕様という認定ではない。要件・課題・変更方針は [マスター仕様](animator-controller-master.md) を参照。', '',
            f'基準SHA-256: `{digest}`。更新元: `Tools/AnimatorController/export_specification.py`。', '',
            '## パラメーター', '', '| 名前 | 型 | 初期値 |', '|---|---|---|']
    for p in controller['m_AnimatorParameters']:
        kind, key = {1: ('float', 'm_DefaultFloat'), 3: ('int', 'm_DefaultInt'), 4: ('bool', 'm_DefaultBool'), 9: ('trigger', 'm_DefaultBool')}[p['m_Type']]
        rows.append(f'| {p["m_Name"]} | {kind} | {p[key]} |')

    def target(data):
        for key in ['m_DstState', 'm_DstStateMachine']:
            identity = data.get(key, {}).get('fileID')
            if identity: return objects[identity]['data']['m_Name']
        return 'Exit' if data.get('m_IsExit') else '(none)'

    def conditions(data):
        return ' AND '.join(c['m_ConditionEvent'] + ' ' + {1: 'is true', 2: 'is false', 3: '>', 4: '<', 6: '==', 7: '!='}[c['m_ConditionMode']]
                           + ('' if c['m_ConditionMode'] in (1, 2) else ' ' + str(c['m_EventTreshold'])) for c in data['m_Conditions']) or '(条件なし)'

    def transition_rows(refs):
        for index, ref in enumerate(refs):
            t = objects[ref['fileID']]['data']
            detail = ''
            if 'm_TransitionDuration' in t:
                detail = f'; duration={t["m_TransitionDuration"]}, fixed={t["m_HasFixedDuration"]}, exitTime={t["m_HasExitTime"]}:{t["m_ExitTime"]}, interruption={t["m_InterruptionSource"]}'
            rows.append(f'{index + 1}. → **{target(t)}**: `{conditions(t)}`{detail}')

    def machine(identity, depth):
        m = objects[identity]['data']
        rows.extend(['', '#' * depth + ' ' + m['m_Name'], '', '初期State: `' + objects[m['m_DefaultState']['fileID']]['data']['m_Name'] + '`' if m['m_DefaultState']['fileID'] else '空の識別レイヤー。'])
        if m['m_EntryTransitions']:
            rows.extend(['', 'Entry遷移（順序を保持）:']); transition_rows(m['m_EntryTransitions'])
        if m['m_AnyStateTransitions']:
            rows.extend(['', 'AnyState遷移（順序を保持）:']); transition_rows(m['m_AnyStateTransitions'])
        for child in m['m_ChildStates']:
            s = objects[child['m_State']['fileID']]['data']
            rows.extend(['', '#' * (depth + 1) + ' ' + s['m_Name'], ''])
            motion = s['m_Motion']
            motion_description = ('external GUID ' + motion['guid'] if motion.get('guid') else
                                  objects[motion['fileID']]['data']['m_Name'] if motion['fileID'] else 'none')
            rows.append(f'Motion: `{motion_description}`; Write Defaults={s["m_WriteDefaultValues"]}; Foot IK={s["m_IKOnFeet"]}。')
            for ref in s['m_StateMachineBehaviours']:
                b = objects[ref['fileID']]['data']
                if 'parameters' in b:
                    rows.append('Driver (localOnly=' + str(b['localOnly']) + '): ' + ', '.join('`' + p['name'] + '=' + str(p['value']) + '` (' + str(p['type']) + ')' for p in b['parameters']))
                else:
                    keys = [k for k in b if k not in definition.BEHAVIOUR_COMMON]
                    rows.append('Behaviour: ' + ', '.join('`' + k + '=' + str(b[k]) + '`' for k in keys))
            rows.append(''); transition_rows(s['m_Transitions'])
        for child in m['m_ChildStateMachines']: machine(child['m_StateMachine']['fileID'], depth + 1)
        for pair in m['m_StateMachineTransitions']:
            rows.extend(['', '子StateMachine `' + objects[pair['first']['fileID']]['data']['m_Name'] + '` の出る遷移:'])
            transition_rows(pair['second'])

    for layer in controller['m_AnimatorLayers']:
        machine(layer['m_StateMachine']['fileID'], 2)
    rows.extend(['', '## BlendTreeによる閾値計算', '',
                 '3個の参照BlendTreeはUprightと設定閾値を使う。内部ClipのAnimator float曲線が判定値を書く。単純な比較式に置き換える場合、等値・初期値・加算レイヤーを別途検証する。', ''])
    for identity in sorted(definition.reachable(objects, root)):
        node = objects[identity]
        if node['kind'] != 'BlendTree': continue
        d = node['data']
        rows.append(f'- `{d["m_Name"]}`: type={d["m_BlendType"]}; x=`{d["m_BlendParameter"]}`, y=`{d["m_BlendParameterY"]}`; children={len(d["m_Childs"])}')
    return '\n'.join(rows) + '\n'


if __name__ == '__main__':
    path = ROOT / 'docs/animator-controller-reference.md'
    path.write_text(generate(), encoding='utf-8', newline='\n')
    print(path)
