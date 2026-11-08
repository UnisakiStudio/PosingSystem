using System.Linq;
using UnityEngine;
using VRC.SDK3.Avatars.Components;

namespace jp.unisakistudio.posingsystemeditor
{
    internal static class PosingSystemHierarchy
    {
        // Hiding the avatar in the editing scene must not disable its pose settings.
        internal static Transform InactiveChild(Transform item, Transform avatar)
        {
            for (var current = item; current != null && current != avatar; current = current.parent)
                if (!current.gameObject.activeSelf) return current;
            return null;
        }

        internal static T[] GetActiveAvatarChildren<T>(this Component root) where T : Component
        {
            var avatar = root.GetComponentInParent<VRCAvatarDescriptor>(true);
            if (avatar == null || avatar.gameObject.activeInHierarchy) return root.GetComponentsInChildren<T>();
            return root.GetComponentsInChildren<T>(true)
                .Where(item => InactiveChild(item.transform, avatar.transform) == null).ToArray();
        }

        internal static Transform BlockingParent(Transform item, Transform avatar) =>
            item.GetComponentsInParent<Transform>(true).FirstOrDefault(t => t.CompareTag("EditorOnly")) ??
            InactiveChild(item, avatar);
    }
}
