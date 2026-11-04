using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor
{
    public static class PosingSystemPoseListUtility
    {
        public static List<AnimationClip> CollectClips(IEnumerable<UnityEngine.Object> objects)
        {
            var result = new List<AnimationClip>();
            foreach (var obj in objects ?? Array.Empty<UnityEngine.Object>())
            {
                if (obj == null) continue;
                if (obj is AnimationClip clip) Add(clip);
                else
                {
                    var path = AssetDatabase.GetAssetPath(obj);
                    if (!string.IsNullOrEmpty(path) && !AssetDatabase.IsValidFolder(path))
                        foreach (var sub in AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()) Add(sub);
                }
            }
            return result;
            void Add(AnimationClip clip)
            {
                if (EditorUtility.IsPersistent(clip) && !clip.name.StartsWith("__preview__", StringComparison.Ordinal) && !result.Contains(clip)) result.Add(clip);
            }
        }

        public static int AddClips(PosingSystem ps, int category, IEnumerable<AnimationClip> clips)
        {
            if (ps == null || ps.defines == null || category < 0 || category >= ps.defines.Count) return 0;
            var define = ps.defines[category];
            var existing = new HashSet<Motion>((define.animations ?? new List<PosingSystem.AnimationDefine>()).Select(a => a.animationClip));
            var pending = CollectClips(clips?.Cast<UnityEngine.Object>()).Where(clip => existing.Add(clip)).ToList();
            if (pending.Count == 0) return 0;
            Undo.IncrementCurrentGroup();
            var group = Undo.GetCurrentGroup();
            Undo.SetCurrentGroupName("Add pose animations");
            Undo.RegisterCompleteObjectUndo(ps, "Add pose animations");
            if (define.animations == null) define.animations = new List<PosingSystem.AnimationDefine>();
            foreach (var clip in pending) define.animations.Add(new PosingSystem.AnimationDefine(clip, clip.name));
            MarkChanged(ps);
            Undo.CollapseUndoOperations(group);
            return pending.Count;
        }

        public static void SetInitial(PosingSystem owner, int category, int index, bool value)
        {
            var define = owner.defines[category];
            var target = define.animations[index];
            var avatar = owner.GetAvatar();
            var systems = avatar != null ? avatar.GetComponentsInChildren<PosingSystem>(true) : new[] { owner };
            Undo.RecordObjects(systems, "Change initial pose");
            foreach (var ps in systems)
            {
                var changed = false;
                foreach (var layer in ps.defines)
                {
                    if (layer.paramName != define.paramName) continue;
                    foreach (var animation in layer.animations)
                    {
                        if (animation != target && (!value || !animation.initial)) continue;
                        animation.initial = animation.initialSet = animation == target && value;
                        animation.previewImage = null;
                        changed = true;
                    }
                }
                if (changed) MarkChanged(ps);
            }
        }

        public static void MarkChanged(PosingSystem ps)
        {
            PosingSystemEditorChangeUtility.MarkChanged(ps);
        }

        public static bool Matches(string query, string name, Motion motion)
        {
            query = (query ?? "").Trim();
            return query.Length == 0 || (name ?? "").IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                (motion != null && motion.name.IndexOf(query, StringComparison.CurrentCultureIgnoreCase) >= 0);
        }
    }
}
