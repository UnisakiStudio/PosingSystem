#if NDMF && MODULAR_AVATAR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    internal static partial class PosingSystemControllerCompiler
    {
        // Only the fixed-size, side-effect-free group dispatcher is flattened. The number of
        // shortcuts does not depend on the number of product/user poses. Stateful selectors,
        // remote pose decoding and every animation/VR blending transition remain intact.
        private static void InlineGroupDispatch(Result result)
        {
            var machine = result.Controller.layers.Single(l => l.name == "USSPS_GroupSwitch").stateMachine;
            var states = machine.states.Select(c => c.state).ToArray();
            var entry = states.Single(s => s.name == "Entry");
            if (states.Length != 8 || machine.stateMachines.Length != 0 || machine.behaviours.Length != 0 ||
                machine.anyStateTransitions.Length != 0 || machine.entryTransitions.Length != 0 ||
                entry.behaviours.Length != 0 || states.Any(s => s.motion != entry.motion || !DispatcherMotion(s.motion) ||
                    s.writeDefaultValues != entry.writeDefaultValues) ||
                states.SelectMany(s => s.transitions).Any(t => !Instant(t)))
                throw new InvalidOperationException("The group dispatcher no longer satisfies the optimizer's verified preconditions. " +
                    $"states={states.Length}, machines={machine.stateMachines.Length}, behaviours={machine.behaviours.Length}, " +
                    $"any={machine.anyStateTransitions.Length}, entry={machine.entryTransitions.Length}, entryBehaviours={entry.behaviours.Length}; " +
                    "unsupportedMotion=" + string.Join(",", states.Where(s => !DispatcherMotion(s.motion)).Select(s => s.name + ":" + s.motion?.name)) +
                    "; noninstant=" + states.SelectMany(s => s.transitions).Count(t => !Instant(t)));

            foreach (var state in states.Where(s => s != entry))
            {
                var transitions = new List<AnimatorStateTransition>();
                foreach (var incoming in state.transitions)
                {
                    if (incoming.destinationState == entry)
                    {
                        foreach (var outgoing in entry.transitions)
                        {
                            var conditions = incoming.conditions.Concat(outgoing.conditions).Distinct().ToArray();
                            if (Contradictory(conditions)) continue;
                            var shortcut = new AnimatorStateTransition();
                            result.Own(shortcut);
                            EditorUtility.CopySerialized(outgoing, shortcut);
                            shortcut.hideFlags = HideFlags.None;
                            shortcut.conditions = conditions;
                            transitions.Add(shortcut);
                            result.AddedGroupShortcuts++;
                        }
                    }
                    // No destination matches at exact zero boundaries or TrackingType=0.
                    // Preserve the original wait state instead of inventing a default posture.
                    transitions.Add(incoming);
                }
                state.transitions = transitions.ToArray();
            }
            if (result.AddedGroupShortcuts > 140)
                throw new InvalidOperationException("Group optimization exceeded its fixed transition budget.");
        }

        private static bool DispatcherMotion(Motion motion)
        {
            if (!(motion is AnimationClip clip) || AnimationUtility.GetObjectReferenceCurveBindings(clip).Length != 0 ||
                AnimationUtility.GetAnimationEvents(clip).Length != 0) return false;
            // The shipped Empty clip activates Dummy constantly. All eight states share it, so
            // skipping Entry does not change this write even if an avatar has that child object.
            return AnimationUtility.GetCurveBindings(clip).All(b => b.path == "Dummy" && b.type == typeof(GameObject) &&
                b.propertyName == "m_IsActive" && AnimationUtility.GetEditorCurve(clip, b).keys.All(k =>
                    k.value == 1 && (k.inTangent == 0 || float.IsInfinity(k.inTangent)) &&
                    (k.outTangent == 0 || float.IsInfinity(k.outTangent))));
        }

        private static bool Instant(AnimatorStateTransition t) =>
            !t.hasExitTime && t.duration == 0 && t.offset == 0 && t.hasFixedDuration &&
            !t.isExit && !t.mute && !t.solo && t.destinationState != null && t.destinationStateMachine == null &&
            t.canTransitionToSelf && t.interruptionSource == TransitionInterruptionSource.Destination;

        // Conservative contradictions only. No float >=/<= complement is synthesized, and no
        // hierarchy between shallow/deep/sleep flags is assumed. Zero remains a distinct case.
        private static bool Contradictory(AnimatorCondition[] conditions)
        {
            foreach (var a in conditions)
            foreach (var b in conditions)
            {
                if (a.parameter != b.parameter) continue;
                if (a.mode == AnimatorConditionMode.If && b.mode == AnimatorConditionMode.IfNot) return true;
                if (a.mode == AnimatorConditionMode.Equals && b.mode == AnimatorConditionMode.Equals && a.threshold != b.threshold) return true;
                if (a.mode == AnimatorConditionMode.Equals && b.mode == AnimatorConditionMode.NotEqual && a.threshold == b.threshold) return true;
                if (a.mode == AnimatorConditionMode.Greater && b.mode == AnimatorConditionMode.Less && a.threshold >= b.threshold) return true;
            }
            return false;
        }
    }
}
#endif
