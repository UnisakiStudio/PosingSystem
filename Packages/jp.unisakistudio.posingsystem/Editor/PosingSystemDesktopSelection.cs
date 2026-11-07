#if NDMF && MODULAR_AVATAR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using VRC.SDK3.Avatars.Components;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    // Desktop playback may use the local group/menu inputs before their synchronized encoding
    // arrives. Sleep/fixed/special groups, VR blending, remote decoding and jump edges stay intact.
    internal static class PosingSystemDesktopSelection
    {
        const string Added = "USSPS.Compiler.Desktop.v1.add:";
        const string Saved = "USSPS.Compiler.Desktop.v1.original:";
        [Serializable] sealed class Backup { public string name; public Condition[] conditions; }
        [Serializable] sealed class Condition { public string parameter; public int mode; public float threshold; }
        static AnimatorCondition C(string p, AnimatorConditionMode m, float v = 0) =>
            new() { parameter = p, mode = m, threshold = v };
        static AnimatorCondition Eq(string p, int v) => C(p, AnimatorConditionMode.Equals, v);
        static AnimatorCondition Ne(string p, int v) => C(p, AnimatorConditionMode.NotEqual, v);
        static AnimatorCondition[] Outside => new[] { Ne("LocomotionTypeGroup", 0), Ne("LocomotionTypeGroup", 1), Ne("LocomotionTypeGroup", 2) };
        static AnimatorCondition Local => C("IsLocal", AnimatorConditionMode.If);
        static AnimatorCondition Remote => C("IsLocal", AnimatorConditionMode.IfNot);

        internal static bool Restore(AnimatorController controller)
        {
            var layer = controller.layers.FirstOrDefault(l => l.name == "USSPS_Locomotion");
            if (layer == null) return false;
            var machine = layer.stateMachine; var restored = false;
            bool Keep(AnimatorTransitionBase t)
            {
                if (t.name.StartsWith(Added, StringComparison.Ordinal)) { restored = true; return false; }
                if (!t.name.StartsWith(Saved, StringComparison.Ordinal)) return true;
                var backup = JsonUtility.FromJson<Backup>(t.name.Substring(Saved.Length));
                t.conditions = backup.conditions.Select(c => C(c.parameter, (AnimatorConditionMode)c.mode, c.threshold)).ToArray();
                t.name = backup.name; restored = true; return true;
            }
            machine.entryTransitions = machine.entryTransitions.Where(Keep).ToArray();
            foreach (var state in machine.states) state.state.transitions = state.state.transitions.Where(Keep).ToArray();
            return restored;
        }

        static void Remember(AnimatorTransitionBase t)
        {
            t.name = Saved + JsonUtility.ToJson(new Backup { name=t.name, conditions=t.conditions.Select(c =>
                new Condition { parameter=c.parameter, mode=(int)c.mode, threshold=c.threshold }).ToArray() });
        }

        internal static void Apply(AnimatorController controller, Action<Object> own)
        {
            Restore(controller);
            var selector = controller.layers.Single(l => l.name == "USSPS_LocomotionType").stateMachine;
            var playback = controller.layers.Single(l => l.name == "USSPS_Locomotion").stateMachine;
            var desktop = playback.entryTransitions.Where(t => t.conditions.Any(c => c.parameter == "VRMode" &&
                c.mode == AnimatorConditionMode.Equals && c.threshold == 0)).ToArray();
            var plans = new Dictionary<AnimatorState, List<AnimatorCondition[]>>();
            // Build the lookup once. Scanning every encoder for every pose is quadratic.
            var encoders = selector.entryTransitions.Where(t => t.destinationState != null)
                .GroupBy(t => EncodingKey(t.destinationState.behaviours.OfType<VRCAvatarParameterDriver>()
                    .SelectMany(d => d.parameters).GroupBy(p => p.name).ToDictionary(g => g.Key, g => g.Last().value)))
                .ToDictionary(g => g.Key, g => g.ToArray());
            var selections = new[] { "SitStand", "SitShallow", "SitDeep" };
            var selectedTypes = new[] { 1, 12, 22 };
            var validSelections = Enumerable.Range(0, 3).Select(group => selector.entryTransitions
                .Where(t => t.conditions.Any(c => c.parameter == "LocomotionType" && c.mode == AnimatorConditionMode.Equals && c.threshold == selectedTypes[group]))
                .SelectMany(t => t.conditions.Where(c => c.parameter == selections[group] && c.mode == AnimatorConditionMode.Equals))
                .Select(c => (int)c.threshold).Append(0).Distinct().OrderBy(v => v).ToArray()).ToArray();
            AnimatorState defaultDesktop = null;
            foreach (var entry in desktop)
            {
                if (entry.destinationState == null) throw new InvalidOperationException("Unsupported desktop playback entry.");
                var poseConditions = entry.conditions.Where(c => c.parameter != "VRMode").ToArray();
                var key = EncodingKey(poseConditions.ToDictionary(c => c.parameter, c => c.mode switch {
                    AnimatorConditionMode.Equals => c.threshold,
                    AnimatorConditionMode.If => 1f,
                    AnimatorConditionMode.IfNot => 0f,
                    _ => throw new InvalidOperationException("Unsupported pose encoding condition.") }));
                if (!encoders.TryGetValue(key, out var matches) || matches.Length != 1)
                    throw new InvalidOperationException("Desktop selection must map to exactly one encoder: " + entry.destinationState.name);
                var conditions = matches[0].conditions;
                var type = conditions.Single(c => c.parameter == "LocomotionType" && c.mode == AnimatorConditionMode.Equals);
                if (type.threshold == 0) defaultDesktop = entry.destinationState;
                var ordinary = OrdinaryType((int)type.threshold);
                if (ordinary == null) ordinary = Array.Empty<AnimatorCondition>();
                // No ordinary match means this sleep/special state exits when an ordinary group takes over.
                var terms = new List<AnimatorCondition[]>();
                if (ordinary.Length != 0)
                    terms.Add(ordinary.Concat(conditions.Where(c => c.parameter != "LocomotionType")).Distinct().ToArray());
                if (!plans.TryAdd(entry.destinationState, terms)) throw new InvalidOperationException("Aliased desktop playback entries are unsupported.");
            }
            if (defaultDesktop == null) throw new InvalidOperationException("Desktop default posture is missing.");
            // Validate before modifying. Pose exits must be side-effect-free instant exits; special
            // jump/select edges remain in their original order and are never replaced by AnyState.
            foreach (var state in plans.Keys)
                if (state.transitions.Where(t => t.isExit).Any(t => t.hasExitTime || t.duration != 0))
                    throw new InvalidOperationException("Desktop exit blending cannot be bypassed: " + state.name);

            var entries = new List<AnimatorTransition>();
            foreach (var entry in playback.entryTransitions)
            {
                if (!desktop.Contains(entry)) { entries.Add(entry); continue; }
                var original = entry.conditions;
                Remember(entry); entry.conditions = new[] { Remote }.Concat(original).ToArray(); entries.Add(entry);
                AddEntry(original.Concat(Outside).Append(Local).ToArray(), entry.destinationState);
                foreach (var term in plans[entry.destinationState]) AddEntry(term.Concat(new[] { Local, Eq("VRMode", 0) }).ToArray(), entry.destinationState);
            }
            // The encoder falls back to its default state for an unknown menu value. Make that
            // default explicit too, otherwise a stale synchronized pose can create an exit loop.
            for (var group = 0; group < 3; group++)
                AddEntry(new[] { Local, Eq("VRMode", 0), Eq("LocomotionTypeGroup", group) }
                    .Concat(validSelections[group].Select(v => Ne(selections[group], v))).ToArray(), defaultDesktop);
            // The original default only applies after every generated ordinary selection was tried.
            playback.entryTransitions = entries.OrderBy(t => t.name == PosingSystemConverter.AvatarSelectionFallback).ToArray();
            foreach (var pair in plans)
            {
                var state = pair.Key; var transitions = new List<AnimatorStateTransition>();
                foreach (var exit in state.transitions)
                {
                    if (!exit.isExit) { transitions.Add(exit); continue; }
                    var original = exit.conditions;
                    // Remote and non-desktop callers still use the original synchronized condition.
                    transitions.Add(Clone(exit, new[] { Local, Ne("VRMode", 0) }.Concat(original).ToArray()));
                    transitions.Add(Clone(exit, new[] { Local }.Concat(Outside).Concat(original).ToArray()));
                    Remember(exit); exit.conditions = new[] { Remote }.Concat(original).ToArray(); transitions.Add(exit);
                }
                // NOT(group AND menu-value) = group differs OR menu differs. There is only one
                // conjunction per ordinary pose, so this adds O(poses) edges, never all-pairs edges.
                var mismatches = pair.Value.Count == 0 ? new[] { Array.Empty<AnimatorCondition>() } :
                    pair.Value.Single().Select(c => new[] { Negate(c) }).ToArray();
                if (state == defaultDesktop)
                    mismatches = Enumerable.Range(0, 3).SelectMany(group => Ranges(validSelections[group]
                        .Where(v => group != 0 || v != 0).ToArray()).Select(range => new[] {
                            Eq("LocomotionTypeGroup", group),
                            C(selections[group], AnimatorConditionMode.Greater, range.first - 1),
                            C(selections[group], AnimatorConditionMode.Less, range.last + 1) })).ToArray();
                foreach (var mismatch in mismatches)
                {
                    var t = new AnimatorStateTransition { name=Added, isExit=true, hasExitTime=false,
                        duration=0, hasFixedDuration=true, interruptionSource=TransitionInterruptionSource.SourceThenDestination,
                        conditions=new[] { Local, Eq("VRMode", 0), C("LocomotionTypeGroup", AnimatorConditionMode.Greater, -1),
                            C("LocomotionTypeGroup", AnimatorConditionMode.Less, 3) }.Concat(mismatch).ToArray() };
                    own(t); transitions.Add(t);
                }
                state.transitions = transitions.ToArray();
            }
            void AddEntry(AnimatorCondition[] conditions, AnimatorState target)
            {
                var t = new AnimatorTransition { name=Added, destinationState=target, conditions=conditions };
                own(t); entries.Add(t);
            }
            AnimatorStateTransition Clone(AnimatorStateTransition source, AnimatorCondition[] conditions)
            {
                var t = new AnimatorStateTransition(); EditorUtility.CopySerialized(source, t);
                t.name=Added; t.conditions=conditions; own(t); return t;
            }
        }

        static IEnumerable<(int first, int last)> Ranges(int[] sorted)
        {
            if (sorted.Length == 0) yield break;
            var first = sorted[0]; var last = first;
            foreach (var value in sorted.Skip(1))
            {
                if (value == last + 1) last = value;
                else { yield return (first, last); first = last = value; }
            }
            yield return (first, last);
        }

        static AnimatorCondition Negate(AnimatorCondition c) => C(c.parameter, c.mode switch {
            AnimatorConditionMode.Equals => AnimatorConditionMode.NotEqual,
            AnimatorConditionMode.NotEqual => AnimatorConditionMode.Equals,
            _ => throw new InvalidOperationException("Only discrete ordinary selection inputs may be inverted.") }, c.threshold);

        static AnimatorCondition[] OrdinaryType(int type) => type switch {
            0 => new[] { Eq("LocomotionTypeGroup", 0), Eq("SitStand", 0) },
            1 => new[] { Eq("LocomotionTypeGroup", 0), Ne("SitStand", 0) },
            11 => new[] { Eq("LocomotionTypeGroup", 1), Eq("SitShallow", 0) },
            12 => new[] { Eq("LocomotionTypeGroup", 1), Ne("SitShallow", 0) },
            21 => new[] { Eq("LocomotionTypeGroup", 2), Eq("SitDeep", 0) },
            22 => new[] { Eq("LocomotionTypeGroup", 2), Ne("SitDeep", 0) },
            _ => null };

        static int EncodingKey(Dictionary<string,float> values)
        {
            if (!values.TryGetValue("USSPS_Pose",out var low) || low<0 || low>255 || low!=(int)low)
                throw new InvalidOperationException("Invalid low pose byte.");
            var key=(int)low;
            for(var bit=0;bit<8;bit++)
            {
                if(!values.TryGetValue("USSPS_Pose"+bit,out var value) || (value!=0 && value!=1))
                    throw new InvalidOperationException("Incomplete high pose encoding.");
                key|=(int)value<<(8+bit);
            }
            return key;
        }
    }
}
#endif
