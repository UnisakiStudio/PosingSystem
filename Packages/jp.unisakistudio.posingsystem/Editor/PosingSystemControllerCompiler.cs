#if NDMF && MODULAR_AVATAR
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    // Versioned, editor-only compiler. No template/YAML load is needed to construct the controller.
    // The generated definition contains serialized fields so rare properties cannot silently disappear.
    internal static partial class PosingSystemControllerCompiler
    {
        internal sealed class Result : IDisposable
        {
            internal readonly Dictionary<long, Object> Objects = new();
            internal AnimatorController Controller => (AnimatorController)Objects[RootId];
            internal int RemovedDuplicateTransitions;
            internal int AddedGroupShortcuts;
            long nextId = long.MaxValue;
            internal void Own(Object obj)
            {
                while (Objects.ContainsKey(nextId)) nextId--;
                Objects.Add(nextId--, obj);
            }
            internal IEnumerable<Object> ReachableObjects()
            {
                var owned = Objects.Values.ToHashSet();
                return PosingSystemControllerGraph.Collect(new[] {Controller}, owned.Contains);
            }
            public void Dispose()
            {
                foreach (var obj in Objects.Values.Reverse())
                    if (obj != null && !EditorUtility.IsPersistent(obj)) Object.DestroyImmediate(obj);
                Objects.Clear();
            }
        }

        internal static Result Create(bool optimize, Func<string, long, Object> resolve = null)
        {
            var result = new Result();
            var external = new Dictionary<(string, long), Object>();
            resolve ??= Resolve;
            try
            {
                foreach (var node in Definition)
                {
                    Object obj;
                    if (typeof(StateMachineBehaviour).IsAssignableFrom(node.Type)) obj = ScriptableObject.CreateInstance(node.Type);
                    else if (node.Type == typeof(AnimatorController)) obj = new AnimatorController();
                    else if (node.Type == typeof(AnimatorStateMachine)) obj = new AnimatorStateMachine();
                    else if (node.Type == typeof(AnimatorState)) obj = new AnimatorState();
                    else if (node.Type == typeof(AnimatorStateTransition)) obj = new AnimatorStateTransition();
                    else if (node.Type == typeof(AnimatorTransition)) obj = new AnimatorTransition();
                    else if (node.Type == typeof(BlendTree)) obj = new BlendTree();
                    else throw new NotSupportedException("Unsupported controller definition type: " + node.Type);
                    result.Objects.Add(node.Id, obj);
                    obj.hideFlags = HideFlags.None;
                }
                Object External(string guid, long id)
                {
                    if (external.TryGetValue((guid, id), out var known)) return known;
                    var loaded = resolve(guid, id);
                    if (loaded == null) throw new InvalidOperationException("Controller dependency is missing: " + guid + ":" + id);
                    external.Add((guid, id), loaded);
                    return loaded;
                }
                ConfigureNative(result.Objects, External);
                if (optimize)
                {
                    RemoveDuplicates(result);
                    InlineGroupDispatch(result);
                    RemoveDuplicates(result);
                }
                return result;
            }
            catch { result.Dispose(); throw; }
        }

        private static Object Resolve(string guid, long id)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(path)) return null;
            return AssetDatabase.LoadAllAssetsAtPath(path).FirstOrDefault(o => o != null &&
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(o, out string actualGuid, out long actualId) &&
                actualGuid == guid && actualId == id);
        }

        internal static void RemoveDuplicates(Result result)
        {
            foreach (var state in result.Objects.Values.OfType<AnimatorState>())
            {
                var kept = new List<AnimatorStateTransition>();
                foreach (var transition in state.transitions)
                {
                    // Native JSON omits nonpersistent destination identities. Compare typed fields,
                    // including actual object identity, so equal conditions with different targets survive.
                    if (!kept.Any(t => SameTransition(t, transition))) kept.Add(transition);
                    else result.RemovedDuplicateTransitions++;
                }
                state.transitions = kept.ToArray();
            }
        }

        private static bool SameTransition(AnimatorStateTransition a, AnimatorStateTransition b) =>
            a.name == b.name && a.destinationState == b.destinationState &&
            a.destinationStateMachine == b.destinationStateMachine && a.isExit == b.isExit &&
            a.hasExitTime == b.hasExitTime && a.exitTime == b.exitTime && a.duration == b.duration &&
            a.offset == b.offset && a.hasFixedDuration == b.hasFixedDuration &&
            a.interruptionSource == b.interruptionSource && a.orderedInterruption == b.orderedInterruption &&
            a.canTransitionToSelf == b.canTransitionToSelf && a.mute == b.mute && a.solo == b.solo &&
            a.conditions.SequenceEqual(b.conditions);

        private sealed class DefinitionNode
        {
            internal readonly long Id;
            internal readonly Type Type;
            internal DefinitionNode(long id, Type type) { Id = id; Type = type; }
        }
    }
}
#endif
