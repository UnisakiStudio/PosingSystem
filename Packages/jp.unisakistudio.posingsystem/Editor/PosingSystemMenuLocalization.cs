using System.Collections.Generic;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using nadena.dev.ndmf;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;

namespace jp.unisakistudio.posingsystemeditor
{
    // Call only on the NDMF build clone, after optional menus are synchronized and before MA.
    internal static class PosingSystemMenuLocalization
    {
        internal static void Apply(PosingSystem ps, IAssetSaver saver = null)
        {
            var language = PosingSystemLocalization.Normalize(ps.menuLanguage);
            if (language == "ja") return;
            var copies = new Dictionary<VRCExpressionsMenu, VRCExpressionsMenu>();
            foreach (var item in ps.GetComponentsInChildren<ModularAvatarMenuItem>(true))
            {
                if (item.GetComponentInParent<PosingSystem>(true) != ps || item.Control == null) continue;
                // Keep GameObject.name unchanged: generation, icon assignment and old-product
                // detection still use those identities. MA may prefer its separate label field.
                var displayName = string.IsNullOrEmpty(item.label) ? item.gameObject.name : item.label;
                item.label = Translate(language, displayName);
                item.Control.name = item.label;
                TranslateLabels(item.Control, language);
                item.Control.subMenu = CloneMenu(item.Control.subMenu, language, copies, saver);
            }
            foreach (var installer in ps.GetComponentsInChildren<ModularAvatarMenuInstaller>(true))
            {
                if (installer.GetComponentInParent<PosingSystem>(true) != ps) continue;
                installer.menuToAppend = CloneMenu(installer.menuToAppend, language, copies, saver);
            }
            foreach (var installer in ps.GetComponentsInChildren<ModularAvatarMenuInstaller>(true))
            {
                if (installer.GetComponentInParent<PosingSystem>(true) != ps) continue;
                if (installer.installTargetMenu != null && copies.TryGetValue(installer.installTargetMenu, out var target))
                    installer.installTargetMenu = target;
            }
        }

        private static void TranslateLabels(VRCExpressionsMenu.Control control, string language)
        {
            if (control.labels == null) return;
            for (var i = 0; i < control.labels.Length; i++)
            {
                var label = control.labels[i];
                label.name = Translate(language, label.name);
                control.labels[i] = label;
            }
        }

        private static string Translate(string language, string value)
            => PosingSystemLocalization.Translate(language, value, value);

        internal static VRCExpressionsMenu CloneMenu(VRCExpressionsMenu source, string language,
            Dictionary<VRCExpressionsMenu, VRCExpressionsMenu> copies, IAssetSaver saver = null)
        {
            if (source == null) return null;
            if (copies.TryGetValue(source, out var existing)) return existing;
            var copy = Object.Instantiate(source);
            copy.name = source.name;
            copies.Add(source, copy); // Register before recursion: expression menus can contain cycles.
            saver?.SaveAsset(copy);
            for (var controlIndex = 0; controlIndex < copy.controls.Count; controlIndex++)
            {
                var control = copy.controls[controlIndex];
                if (control == null) continue;
                control.name = Translate(language, control.name);
                TranslateLabels(control, language);
                // Instantiate remaps a self-reference to the clone. Traverse the original
                // reference graph so self-links and shared submenus use our cache.
                control.subMenu = CloneMenu(source.controls[controlIndex].subMenu, language, copies, saver);
            }
            return copy;
        }
    }
}
