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

        // Object references sit only inside containers. Entering strings, curves, vectors and
        // arrays of plain values would visit every character and keyframe of a clip, which was
        // most of the time a prebuild took, without ever finding a reference there.
        private static readonly HashSet<string> PlainElements = new HashSet<string> {
            "int", "unsigned int", "float", "double", "bool", "char", "UInt8", "SInt8", "UInt16", "SInt16",
            "SInt64", "UInt64", "long long", "unsigned long long", "string" };

        internal static bool MayHoldReferences(SerializedProperty property)
        {
            switch (property.propertyType)
            {
                case SerializedPropertyType.Generic:
                    return !property.isArray || !PlainElements.Contains(property.arrayElementType);
                case SerializedPropertyType.ManagedReference:
                case SerializedPropertyType.ExposedReference:
                    return true;
                default:
                    return false;
            }
        }

        internal static IEnumerable<Object> References(Object obj)
        {
            using var serialized = new SerializedObject(obj);
            var it = serialized.GetIterator();
            var enter = true;
            while (it.Next(enter))
            {
                enter = MayHoldReferences(it);
                if (it.propertyType == SerializedPropertyType.ObjectReference && it.objectReferenceValue != null)
                    yield return it.objectReferenceValue;
            }
        }

        // A clip refers to other objects only through object-reference curves and event
        // parameters (its float curves name at most the script of the animated component).
        // Nearly every generated clip has neither, and a prebuild with all features holds
        // thousands of them: walking their curve bindings took longer than everything else
        // when a prebuild was repeated.
        internal static bool HasNoGraphReferences(Object obj) =>
            obj is AnimationClip clip && AnimationUtility.GetObjectReferenceCurveBindings(clip).Length == 0 &&
            AnimationUtility.GetAnimationEvents(clip).Length == 0;

        internal static HashSet<Object> Collect(IEnumerable<Object> roots, Func<Object, bool> include)
        {
            using var sample = PosingSystemProfiling.Sample("Posing: Graph.Collect");
            var result = new HashSet<Object>();
            var pending = new Stack<Object>(roots.Where(o => o != null));
            while (pending.Count > 0)
            {
                var obj = pending.Pop();
                if (obj == null || !include(obj) || !result.Add(obj)) continue;
                if (HasNoGraphReferences(obj)) continue;
                foreach (var reference in References(obj)) pending.Push(reference);
            }
            return result;
        }

        internal static void Remap(Object obj, IReadOnlyDictionary<Object, Object> map)
        {
            if (HasNoGraphReferences(obj)) return;
            using var serialized = new SerializedObject(obj);
            var it = serialized.GetIterator();
            var enter = true;
            var changed = false;
            while (it.Next(enter))
            {
                enter = MayHoldReferences(it);
                if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
                var value = it.objectReferenceValue;
                if (value == null || !map.TryGetValue(value, out var replacement) || replacement == value) continue;
                it.objectReferenceValue = replacement;
                changed = true;
            }
            if (changed) serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        internal static Dictionary<Object, Object> Clone(IEnumerable<AnimatorController> roots)
        {
            using var sample = PosingSystemProfiling.Sample("Posing: Graph.Clone");
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
