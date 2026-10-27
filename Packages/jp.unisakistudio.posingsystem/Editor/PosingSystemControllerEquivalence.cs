#if NDMF && MODULAR_AVATAR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    // NDMF can clone the template before our conversion pass, losing its asset GUID.
    // Name/marker matching alone is unsafe: prove the complete reachable graph equivalent,
    // including aliasing, ordered transitions, behaviours and every serialized scalar.
    internal static class PosingSystemControllerEquivalence
    {
        internal static bool Matches(Object reference, Object candidate, out string reason)
        {
            var forward = new Dictionary<Object, Object>();
            var reverse = new Dictionary<Object, Object>();
            string mismatch = null;
            bool Fail(string path) { mismatch ??= path; return false; }
            bool Visit(Object a, Object b, string path)
            {
                if (a == null && b == null) return true;
                if (a == null || b == null || a.GetType() != b.GetType()) return Fail(path + ": type/null");
                if (!PosingSystemControllerGraph.IsGraphObject(a)) return a == b || Fail(path + ": external reference");
                if (forward.TryGetValue(a, out var paired)) return paired == b || Fail(path + ": shared reference");
                if (reverse.ContainsKey(b)) return Fail(path + ": merged references");
                forward.Add(a, b); reverse.Add(b, a);
                if (a == b) return true;
                using var sa = new SerializedObject(a);
                using var sb = new SerializedObject(b);
                var x = sa.GetIterator(); var y = sb.GetIterator();
                var descend = true;
                while (x.Next(descend))
                {
                    if (!y.Next(descend) || x.propertyPath != y.propertyPath || x.propertyType != y.propertyType)
                        return Fail(path + ": schema " + x.propertyPath);
                    descend = x.propertyType != SerializedPropertyType.ObjectReference;
                    if (x.propertyPath == "m_ObjectHideFlags") continue;
                    if (a is AnimatorStateMachine && x.propertyPath == "m_StateMachineTransitions")
                    { descend = false; continue; }
                    if (x.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        if (!Visit(x.objectReferenceValue, y.objectReferenceValue, path + "/" + x.propertyPath)) return false;
                    }
                    else if (x.propertyType != SerializedPropertyType.Generic && !SerializedProperty.DataEquals(x, y))
                        return Fail(path + "/" + x.propertyPath + ": value");
                }
                if (y.Next(descend)) return Fail(path + ": extra fields");
                if (a is AnimatorStateMachine)
                {
                    var dx = sa.FindProperty("m_StateMachineTransitions");
                    var dy = sb.FindProperty("m_StateMachineTransitions");
                    if (dx.arraySize != dy.arraySize) return Fail(path + ": machine transition count");
                    for (var i = 0; i < dx.arraySize; i++)
                    {
                        var left = dx.GetArrayElementAtIndex(i);
                        var key = left.FindPropertyRelative("first").objectReferenceValue;
                        // Child machines have already been visited through m_ChildStateMachines.
                        var pairedKey = forward.TryGetValue(key, out var mapped) ? mapped : key;
                        SerializedProperty right = null;
                        for (var j = 0; j < dy.arraySize; j++)
                        {
                            var item = dy.GetArrayElementAtIndex(j);
                            if (item.FindPropertyRelative("first").objectReferenceValue == pairedKey) { right = item; break; }
                        }
                        if (right == null) return Fail(path + ": missing source machine");
                        var tx = left.FindPropertyRelative("second"); var ty = right.FindPropertyRelative("second");
                        if (tx.arraySize != ty.arraySize) return Fail(path + ": outgoing machine transitions");
                        for (var k = 0; k < tx.arraySize; k++)
                            if (!Visit(tx.GetArrayElementAtIndex(k).objectReferenceValue, ty.GetArrayElementAtIndex(k).objectReferenceValue,
                                    path + "/machine-transition[" + i + "," + k + "]")) return false;
                    }
                }
                return true;
            }
            var matches = Visit(reference, candidate, "controller");
            reason = mismatch;
            return matches;
        }
    }
}
#endif
