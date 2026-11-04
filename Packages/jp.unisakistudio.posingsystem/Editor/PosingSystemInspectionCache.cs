using System.Collections.Generic;
using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    // Display-only cache. Conversion/build decisions always use the uncached comparison.
    // Capture the actual inputs touched by serialization, including unsaved controller
    // subassets, clips, shared IK catalogs and the other tools on the same avatar.
    internal static class PosingSystemInspectionCache
    {
        private sealed class Entry
        {
            internal PosingSystem Target;
            internal string BuildData;
            internal int Revision;
            internal bool Updated;
            internal readonly Dictionary<Object, int> Dependencies = new Dictionary<Object, int>();

            internal bool IsCurrent(PosingSystem target)
            {
                if (Target != target || BuildData != target.data || Revision != PosingSystemHierarchyBadge.Revision)
                    return false;
                foreach (var pair in Dependencies)
                    if (pair.Key == null || EditorUtility.GetDirtyCount(pair.Key) != pair.Value) return false;
                return true;
            }
        }

        private static readonly Dictionary<int, Entry> entries = new Dictionary<int, Entry>();
        private static HashSet<Object> collecting;
        internal static int EvaluationCount { get; private set; }

        internal static void Track(Object value)
        {
            if (value != null) collecting?.Add(value);
        }

        internal static bool IsUpdated(PosingSystem target)
        {
            if (target == null) return true;
            var id = target.GetInstanceID();
            if (entries.TryGetValue(id, out var cached) && cached.IsCurrent(target)) return cached.Updated;
            var inputs = new HashSet<Object>();
            var previous = collecting;
            var entry = new Entry { Target = target, BuildData = target.data, Revision = PosingSystemHierarchyBadge.Revision };
            try
            {
                collecting = inputs;
                Track(target);
                var avatar = target.GetAvatar();
                Track(avatar);
                if (avatar != null) Track(avatar.GetComponent<Animator>());
                entry.Updated = PosingSystemConverter.IsPosingSystemDataUpdated(target);
                EvaluationCount++;
            }
            finally { collecting = previous; }
            foreach (var input in inputs)
                if (input != null) entry.Dependencies.Add(input, EditorUtility.GetDirtyCount(input));
            // Bound references held by an inspector session with many previously selected tools.
            if (entries.Count >= 64) entries.Clear();
            entries[id] = entry;
            return entry.Updated;
        }

        internal static void Clear() => entries.Clear();

        internal static bool HasChanged(PosingSystem target) => target != null &&
            entries.TryGetValue(target.GetInstanceID(), out var entry) && !entry.IsCurrent(target);
    }
}
