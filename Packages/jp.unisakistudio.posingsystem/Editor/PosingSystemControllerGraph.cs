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
    // Serialized references preserve synced-layer overrides, cross-machine transitions and SMB
    // references as well as properties not covered by a hand-written Animator copier.
    internal static class PosingSystemControllerGraph
    {
        internal static bool IsGraphObject(Object obj) => obj is AnimatorController ||
            obj is AnimatorStateMachine || obj is AnimatorState || obj is AnimatorTransitionBase ||
            obj is StateMachineBehaviour || obj is Motion || obj is AvatarMask;

        internal static IEnumerable<Object> References(Object obj)
        {
            using var serialized = new SerializedObject(obj);
            var it = serialized.GetIterator();
            while (it.Next(true))
                if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null)
                    yield return it.objectReferenceValue;
        }

        internal static HashSet<Object> Collect(IEnumerable<Object> roots, Func<Object, bool> include)
        {
            var result = new HashSet<Object>();
            var pending = new Stack<Object>(roots.Where(o => o != null));
            while (pending.Count > 0)
            {
                var obj = pending.Pop();
                if (obj == null || !include(obj) || !result.Add(obj)) continue;
                foreach (var reference in References(obj)) pending.Push(reference);
            }
            return result;
        }

        internal static void Remap(Object obj, IReadOnlyDictionary<Object, Object> map)
        {
            using var serialized = new SerializedObject(obj);
            var it = serialized.GetIterator();
            while (it.Next(true))
                if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null &&
                    map.TryGetValue(it.objectReferenceValue, out var replacement)) it.objectReferenceValue = replacement;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static Dictionary<Object, Object> Clone(IEnumerable<AnimatorController> roots)
        {
            var controllers = roots.Where(c => c != null).Distinct().ToArray();
            var ownedPaths = controllers.Select(AssetDatabase.GetAssetPath).Where(p => p.Length > 0).ToHashSet();
            var source = Collect(controllers, o => IsGraphObject(o) &&
                (!(o is AnimationClip || o is AvatarMask) || !EditorUtility.IsPersistent(o) ||
                 (o is AnimationClip && o.name == PosingSystemConverter.AdditiveBreathingResetClipName) ||
                 ownedPaths.Contains(AssetDatabase.GetAssetPath(o))));
            var map = new Dictionary<Object, Object>();
            try
            {
                foreach (var obj in source)
                {
                    Object copy = obj switch {
                        AnimatorController _ => new AnimatorController(),
                        AnimatorStateMachine _ => new AnimatorStateMachine(),
                        AnimatorState _ => new AnimatorState(),
                        AnimatorStateTransition _ => new AnimatorStateTransition(),
                        AnimatorTransition _ => new AnimatorTransition(),
                        BlendTree _ => new BlendTree(),
                        StateMachineBehaviour _ => ScriptableObject.CreateInstance(obj.GetType()),
                        AnimationClip _ => new AnimationClip(),
                        AvatarMask _ => new AvatarMask(),
                        _ => throw new InvalidOperationException("Unsupported animator object: " + obj.GetType())
                    };
                    map.Add(obj, copy);
                    EditorUtility.CopySerialized(obj, copy);
                    copy.hideFlags = HideFlags.None;
                }
                foreach (var obj in map.Values) Remap(obj, map);
                return map;
            }
            catch
            {
                foreach (var obj in map.Values.Reverse()) if (obj != null) Object.DestroyImmediate(obj);
                throw;
            }
        }
    }
}
#endif
