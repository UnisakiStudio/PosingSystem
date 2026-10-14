using System;
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    public static class PosingSystemFeatureUtility
    {
        public const string PhotoActive = "USSPS_Photo";
        public const string PhotoDistance = "USSPS_PhotoDistance";
        public const string PhotoYaw = "USSPS_PhotoYaw";
        public const float PhotoDistanceThreshold = .001f;
        public const string PhotoMenuName = "撮影モード";
        public const string LegacyPhotoMenuName = "撮影用の移動・回転";
        private const string LegacyPhotoAssistanceMenuName = "撮影支援機能";
        public const string FloatActive = "USSPS_Float";
        public const string FloatHeight = "USSPS_FloatHeight";
        public const string FloatHeightAdjusting = "USSPS_FloatHeightAdjusting";
        public const string FloorHeightMenuName = "床の高さ";
        public const float FloatHeightThreshold = .000001f;
        public const string RollActive = "USSPS_Rollover";
        public const string RollSensor = "USSPS_RollSensor";
        public const string RollTime = "USSPS_RollTime";
        public const string FloatMenuName = "全身を浮かせる（試験機能）";
        public const string RollMenuName = "寝返り";
        private const string LegacyRollMenuName = "頭の傾きで寝返り（試験機能）";
        private const string SettingsPrefabGuid = "ecde0b1d67f9e854992715d64f778d8c";

        public static void Migrate(PosingSystem ps)
        {
            ps.features ??= new PosingSystemFeatureSettings();
            // Apply even to avatars which already completed the old 8-bit feature migration.
            ps.features.ApplyFinalizedMovementSettings();
            if (ps.featureSettingsMigrated) return;
            if (ps.isEightBitVersion)
            {
                ps.features.mirror = false;
                ps.features.footHeight = false;
                ps.features.breathing = false;
                ps.features.fingerGestureControl = false;
                if (ps.additiveBreathing != null) ps.additiveBreathing.enabled = true;
            }
            ps.isEightBitVersion = false;
            ps.featureSettingsMigrated = true;
        }

        public static bool UsesBreathing(PosingSystem ps) => ps != null && !ps.isEightBitVersion &&
            (ps.features?.breathing ?? true) && (ps.additiveBreathing?.enabled ?? false);

        public static bool UsesBreathingControl(PosingSystem ps) => UsesBreathing(ps) && (ps.features?.breathingControl ?? true);

        public static bool AvatarUsesBreathingControl(PosingSystem ps)
        {
            var avatar = ps.GetAvatar();
            return avatar == null ? UsesBreathingControl(ps) : avatar.GetComponentsInChildren<PosingSystem>()
                .Any(p => p.tag != "EditorOnly" && UsesBreathingControl(p));
        }

        public static int OptionalBits(PosingSystem ps)
        {
            var f = ps.features ?? new PosingSystemFeatureSettings();
            if (ps.isEightBitVersion) return 0;
            return (f.mirror ? 1 : 0) + (f.footHeight ? 8 : 0) + (UsesBreathingControl(ps) ? 1 : 0) + (f.fingerGestureControl ? 1 : 0) +
                   (f.faceForward || f.rollover ? 1 : 0) + (f.photography && f.photographySynced ? 16 : 0) +
                   (f.floating ? 8 : 0) + (f.rollover ? 9 : 0);
        }

        /// <summary>Bit difference between on and off, with all other feature selections held constant.</summary>
        public static int SharedHeadFeatureToggleBits(PosingSystem ps, bool rollover)
        {
            if (ps == null || ps.isEightBitVersion || ps.tag == "EditorOnly" || !ps.gameObject.activeInHierarchy) return 0;
            var f = ps.features ?? new PosingSystemFeatureSettings();
            var avatar = ps.GetAvatar();
            var others = avatar == null ? Array.Empty<PosingSystemFeatureSettings>() : avatar.GetComponentsInChildren<PosingSystem>()
                .Where(p => p != ps && p.tag != "EditorOnly" && !p.isEightBitVersion)
                .Select(p => p.features ?? new PosingSystemFeatureSettings()).ToArray();
            var headAlreadyUsed = (rollover ? f.faceForward : f.rollover) || others.Any(p => p.faceForward || p.rollover);
            var rolloverAlreadyUsed = others.Any(p => p.rollover);
            return (headAlreadyUsed ? 0 : 1) + (rollover && !rolloverAlreadyUsed ? 9 : 0);
        }

        public static int PoseBits(PosingSystem ps)
        {
            var avatar = ps.GetAvatar();
            var systems = avatar != null ? avatar.GetComponentsInChildren<PosingSystem>() : new[] { ps };
            // The allocation reserves four common poses. Match SplitSyncParameters, including expansion.
            var count = 3 + systems.Where(p => p.tag != "EditorOnly")
                .Sum(p => p.defines?.Sum(d => d.animations?.Count(a => a.enabled) ?? 0) ?? 0);
            return PosingSystemConverter.SplitSyncParameters(0, count)
                .Where(p => p.sync).Sum(p => p.type == ParameterSyncType.Bool ? 1 : 8);
        }

        public static int AvatarOptionalBits(PosingSystem ps)
        {
            var avatar = ps.GetAvatar();
            if (avatar == null) return OptionalBits(ps);
            var systems = avatar.GetComponentsInChildren<PosingSystem>().Where(p => p.tag != "EditorOnly" && !p.isEightBitVersion).ToArray();
            var features = systems.Select(p => p.features ?? new PosingSystemFeatureSettings()).ToArray();
            return (features.Any(f => f.mirror) ? 1 : 0) + (features.Any(f => f.footHeight) ? 8 : 0) +
                (systems.Any(UsesBreathingControl) ? 1 : 0) + (features.Any(f => f.fingerGestureControl) ? 1 : 0) +
                (features.Any(f => f.faceForward || f.rollover) ? 1 : 0) + (features.Any(f => f.photography && f.photographySynced) ? 16 : 0) +
                (features.Any(f => f.floating) ? 8 : 0) + (features.Any(f => f.rollover) ? 9 : 0);
        }

        public static void Synchronize(PosingSystem ps, bool recordUndo = false)
        {
            if (ps == null) return;
            if (recordUndo) Undo.RecordObject(ps, "Change posing features");
            Migrate(ps);
            var f = ps.features;
            var parameters = ps.GetComponent<ModularAvatarParameters>();
            if (parameters == null) return;
            if (recordUndo) Undo.RecordObject(parameters, "Update posing parameters");
            SetParameter(parameters, "USSPS_Mirror", ParameterSyncType.Bool, f.mirror, false, 0, true);
            SetParameter(parameters, "FootHeight", ParameterSyncType.Float, f.footHeight, false, .5f, true);
            SetParameter(parameters, "USSPS_Breathing", ParameterSyncType.Bool, UsesBreathingControl(ps), false, 1, true);
            // Existing local neck-lock control becomes synced only for the correction feature.
            var avatar = ps.GetAvatar();
            var syncHead = f.faceForward || f.rollover || (avatar != null && avatar.GetComponentsInChildren<PosingSystem>()
                .Any(p => p.tag != "EditorOnly" && p.features != null && (p.features.faceForward || p.features.rollover)));
            SetParameter(parameters, "HeadTrackingLock", ParameterSyncType.Bool, true, !syncHead, 0, true);
            SetParameter(parameters, PhotoActive, ParameterSyncType.Bool, f.photography, true, 0, false);
            SetParameter(parameters, PhotoDistance, ParameterSyncType.Float, f.photography, !f.photographySynced, 0, false);
            SetParameter(parameters, PhotoYaw, ParameterSyncType.Float, f.photography, !f.photographySynced, .5f, false);
            SetParameter(parameters, "USSPS_PhotoReset", ParameterSyncType.Bool, f.photography, true, 0, false);
            SetParameter(parameters, FloatActive, ParameterSyncType.Bool, f.floating, true, 0, false);
            SetParameter(parameters, FloatHeight, ParameterSyncType.Float, f.floating, false, 0, false);
            SetParameter(parameters, FloatHeightAdjusting, ParameterSyncType.Bool, f.floating, true, 0, false);
            SetParameter(parameters, RollActive, ParameterSyncType.Bool, f.rollover, false, 0, true);
            SetParameter(parameters, RollSensor, ParameterSyncType.Float, f.rollover, false, .5f, false);
            EditorUtility.SetDirty(parameters);
            PrefabUtility.RecordPrefabInstancePropertyModifications(parameters);
            SetMenuAvailable(ps, "USSPS_Mirror", f.mirror, recordUndo);
            SetMenuAvailable(ps, "FootHeight", f.footHeight, recordUndo);
            SetMenuAvailable(ps, "USSPS_Breathing", UsesBreathingControl(ps), recordUndo);
            foreach (var item in ps.GetComponentsInChildren<ModularAvatarMenuItem>(true).Where(m => UsesParameter(m, "USSPS_Breathing")))
                SetGeneratedMenuIcon(item, "Breathing", recordUndo);
            SetMenuAvailable(ps, "USSPS_FingerGestureDisable_Menu", f.fingerGestureControl, recordUndo);
            foreach (var fingerParameters in ps.GetComponentsInChildren<ModularAvatarParameters>(true)
                         .Where(p => p != parameters && p.parameters.Any(v => v.nameOrPrefix.StartsWith("USSPS_FingerGestureDisable_"))))
            {
                if (recordUndo) Undo.RecordObject(fingerParameters, "Update finger gesture parameters");
                SetParameter(fingerParameters, "USSPS_FingerGestureDisable_Sync", ParameterSyncType.Bool, f.fingerGestureControl, false, 0, false);
                SetIncluded(fingerParameters.gameObject, f.fingerGestureControl, recordUndo);
                EditorUtility.SetDirty(fingerParameters);
                PrefabUtility.RecordPrefabInstancePropertyModifications(fingerParameters);
            }
            foreach (var eraser in ps.GetComponentsInChildren<DuplicateEraser>(true)
                         .Where(e => e.ID == PosingSystemConverter.AdditiveBreathingDuplicateEraserId))
                SetIncluded(eraser.gameObject, UsesBreathing(ps), recordUndo);
            EnsurePhotoMenu(ps, f.photography, recordUndo);
            EnsureFloorHeightMenu(ps, f.floating, recordUndo);
            EnsureRolloverMenu(ps, f.rollover, recordUndo);
            if (recordUndo) ps.data = string.Empty;
            EditorUtility.SetDirty(ps);
            PrefabUtility.RecordPrefabInstancePropertyModifications(ps);
        }

        private static void SetParameter(ModularAvatarParameters target, string name,
            ParameterSyncType type, bool enabled, bool localOnly, float defaultValue, bool saved)
        {
            var index = target.parameters.FindIndex(p => p.nameOrPrefix == name);
            if (!enabled)
            {
                target.parameters.RemoveAll(p => p.nameOrPrefix == name);
                return;
            }
            var parameter = index >= 0 ? target.parameters[index] : new ParameterConfig { nameOrPrefix = name };
            parameter.syncType = type;
            parameter.localOnly = localOnly;
            parameter.defaultValue = defaultValue;
            parameter.saved = saved;
            parameter.hasExplicitDefaultValue = true;
            if (index >= 0) target.parameters[index] = parameter;
            else target.parameters.Add(parameter);
        }

        private static bool UsesParameter(ModularAvatarMenuItem item, string parameter) =>
            item.Control != null && (item.Control.parameter?.name == parameter ||
            (item.Control.subParameters?.Any(p => p.name == parameter) ?? false));

        private static void SetMenuAvailable(PosingSystem ps, string parameter, bool enabled, bool undo)
        {
            var items = ps.GetComponentsInChildren<ModularAvatarMenuItem>(true)
                .Where(m => UsesParameter(m, parameter)).ToArray();
            if (enabled && items.Length == 0)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(SettingsPrefabGuid));
                var template = prefab != null ? prefab.GetComponentsInChildren<ModularAvatarMenuItem>(true)
                    .FirstOrDefault(m => UsesParameter(m, parameter)) : null;
                if (template != null)
                {
                    var path = AnimationUtility.CalculateTransformPath(template.transform.parent, prefab.transform);
                    var parent = ps.transform.Find(path) ?? ps.transform;
                    var copy = Object.Instantiate(template.gameObject, parent);
                    copy.name = template.name;
                    if (undo) Undo.RegisterCreatedObjectUndo(copy, "Restore feature menu");
                    items = new[] { copy.GetComponent<ModularAvatarMenuItem>() };
                }
            }
            foreach (var item in items) SetIncluded(item.gameObject, enabled, undo);
        }

        private static void SetIncluded(GameObject obj, bool enabled, bool undo)
        {
            var tag = enabled ? "Untagged" : "EditorOnly";
            if (obj.tag == tag && obj.activeSelf == enabled) return;
            if (undo) Undo.RecordObject(obj, "Update feature menu visibility");
            obj.tag = tag;
            obj.SetActive(enabled);
            EditorUtility.SetDirty(obj);
            PrefabUtility.RecordPrefabInstancePropertyModifications(obj);
        }

        private static void EnsurePhotoMenu(PosingSystem ps, bool enabled, bool undo)
        {
            var menu = ps.GetComponentsInChildren<ModularAvatarMenuItem>(true)
                .FirstOrDefault(m => m.name == PhotoMenuName || m.name == LegacyPhotoMenuName || m.name == LegacyPhotoAssistanceMenuName);
            if (menu == null && enabled)
            {
                menu = NewMenuItem(SettingsMenuParent(ps), PhotoMenuName, undo);
                menu.MenuSource = SubmenuSource.Children;
                menu.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
            }
            if (menu == null) return;
            RenameMenuItem(menu, PhotoMenuName, undo);
            SetIncluded(menu.gameObject, enabled, undo);
            SetGeneratedMenuIcon(menu, "PhotoAssistance", undo);
            SetGeneratedMenuIcon(EnsureControl(menu.transform, "距離", PhotoDistance, true, undo), "PhotoDistance", undo);
            SetGeneratedMenuIcon(EnsureControl(menu.transform, "向き", PhotoYaw, true, undo), "PhotoDirection", undo);
            SetGeneratedMenuIcon(EnsureControl(menu.transform, "撮影モードを終了", "USSPS_PhotoReset", false, undo, true), "PhotoExit", undo);

            // Reference the original menu's children instead of duplicating generated pose toggles.
            var poseSource = ps.SubmenuRoot != null ? ps.SubmenuRoot.GetComponentInParent<ModularAvatarMenuItem>() : null;
            if (poseSource == null || poseSource.transform.IsChildOf(menu.transform))
                poseSource = ps.GetComponentsInChildren<ModularAvatarMenuItem>(true)
                    .FirstOrDefault(m => m.name == "姿勢選択" && !m.transform.IsChildOf(menu.transform));
            var poseShortcut = menu.transform.Cast<Transform>().Select(t => t.GetComponent<ModularAvatarMenuItem>())
                .FirstOrDefault(m => m != null && m.name == "姿勢選択");
            if (poseShortcut == null) poseShortcut = NewMenuItem(menu.transform, "姿勢選択", undo);
            if (undo) Undo.RecordObject(poseShortcut, "Link photography pose menu");
            poseShortcut.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
            poseShortcut.MenuSource = poseSource != null ? poseSource.MenuSource : SubmenuSource.Children;
            poseShortcut.menuSource_otherObjectChildren = poseSource != null
                ? poseSource.menuSource_otherObjectChildren != null ? poseSource.menuSource_otherObjectChildren : poseSource.gameObject
                : ps.SubmenuRoot != null ? ps.SubmenuRoot.gameObject : null;
            poseShortcut.Control.subMenu = poseSource != null ? poseSource.Control.subMenu : null;
            poseShortcut.Control.icon = poseSource != null ? poseSource.Control.icon : null;
            SetIncluded(poseShortcut.gameObject, poseSource != null || ps.SubmenuRoot != null, undo);
            SaveMenuItem(poseShortcut);

            var feet = EnsureControl(menu.transform, "足固定", "LocomotionLock", false, undo);
            var gesture = EnsureControl(menu.transform, "ジェスチャー無効", "USSPS_FingerGestureDisable_Menu", false, undo);
            foreach (var shortcut in new[] { feet, gesture })
            {
                var source = ps.GetComponentsInChildren<ModularAvatarMenuItem>(true)
                    .FirstOrDefault(m => !m.transform.IsChildOf(menu.transform) && UsesParameter(m, shortcut.Control.parameter.name));
                shortcut.Control.icon = source != null ? source.Control.icon : null;
                shortcut.Control.value = source != null ? source.Control.value : 1;
                shortcut.automaticValue = false;
                SaveMenuItem(shortcut);
            }
            SetIncluded(gesture.gameObject, ps.features.fingerGestureControl, undo);
            // Six items fit in one page. Put pose selection first and the exit action last.
            var order = new[] { poseShortcut, menu.GetComponentsInChildren<ModularAvatarMenuItem>(true).First(m => UsesParameter(m, PhotoDistance)),
                menu.GetComponentsInChildren<ModularAvatarMenuItem>(true).First(m => UsesParameter(m, PhotoYaw)), feet, gesture,
                menu.GetComponentsInChildren<ModularAvatarMenuItem>(true).First(m => UsesParameter(m, "USSPS_PhotoReset")) };
            for (var i = 0; i < order.Length; i++)
            {
                if (undo) Undo.RecordObject(order[i].transform, "Order photography controls");
                order[i].transform.SetSiblingIndex(i);
            }
        }

        private static Transform SettingsMenuParent(PosingSystem ps) => ps.GetComponentsInChildren<ModularAvatarMenuItem>(true)
            .FirstOrDefault(m => UsesParameter(m, "USSPS_Mirror"))?.transform.parent ?? ps.transform;

        private static void EnsureFloorHeightMenu(PosingSystem ps, bool enabled, bool undo)
        {
            var items = ps.GetComponentsInChildren<ModularAvatarMenuItem>(true);
            var radial = items.FirstOrDefault(m => m.Control.type == VRCExpressionsMenu.Control.ControlType.RadialPuppet && UsesParameter(m, FloatHeight));
            var parent = SettingsMenuParent(ps);
            if (radial == null && enabled) radial = EnsureControl(parent, FloorHeightMenuName, FloatHeight, true, undo);
            if (radial != null)
            {
                if (radial.transform.parent != parent)
                {
                    if (undo) Undo.SetTransformParent(radial.transform, parent, "Move floor height to settings");
                    else radial.transform.SetParent(parent, false);
                }
                RenameMenuItem(radial, FloorHeightMenuName, undo);
                if (undo) Undo.RecordObject(radial, "Update floor height control");
                radial.Control.parameter = new VRCExpressionsMenu.Control.Parameter { name = FloatHeightAdjusting };
                radial.Control.value = 1;
                SetIncluded(radial.gameObject, enabled, undo);
                SetGeneratedMenuIcon(radial, "FloorHeight", undo);
                SaveMenuItem(radial);
            }
            // Migrate the previous two-control submenu without leaving a toggle for the derived flag.
            foreach (var oldToggle in items.Where(m => m != null && m.Control.parameter?.name == FloatActive))
                RemoveMenuObject(oldToggle.gameObject, undo);
            foreach (var oldMenu in items.Where(m => m != null && (m.name == FloatMenuName || m.name == "全身を浮かせる（支援機能）")))
            {
                // Preserve any user-added controls when flattening this generated container.
                foreach (var child in oldMenu.transform.Cast<Transform>().ToArray())
                {
                    if (undo) Undo.SetTransformParent(child, parent, "Flatten floating menu");
                    else child.SetParent(parent, false);
                }
                RemoveMenuObject(oldMenu.gameObject, undo);
            }

            var footHeight = parent.Cast<Transform>().Select(t => t.GetComponent<ModularAvatarMenuItem>())
                .FirstOrDefault(m => m != null && UsesParameter(m, "FootHeight"));
            if (radial != null && footHeight != null)
            {
                var currentIndex = radial.transform.GetSiblingIndex();
                var footIndex = footHeight.transform.GetSiblingIndex();
                // Removing an item from before the anchor shifts the anchor left by one.
                var targetIndex = footIndex + (currentIndex < footIndex ? 0 : 1);
                if (currentIndex != targetIndex)
                {
                    if (undo) Undo.RegisterFullObjectHierarchyUndo(parent.gameObject, "Order floor height menu");
                    radial.transform.SetSiblingIndex(targetIndex);
                    EditorUtility.SetDirty(radial.transform);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(radial.transform);
                }
            }
        }

        private static void EnsureRolloverMenu(PosingSystem ps, bool enabled, bool undo)
        {
            var items = ps.GetComponentsInChildren<ModularAvatarMenuItem>(true);
            var parent = SettingsMenuParent(ps);
            var toggle = items.FirstOrDefault(m => m.Control.type == VRCExpressionsMenu.Control.ControlType.Toggle && UsesParameter(m, RollActive));
            if (toggle != null && toggle.transform.parent != parent)
            {
                if (undo) Undo.SetTransformParent(toggle.transform, parent, "Move rollover to settings");
                else toggle.transform.SetParent(parent, false);
            }
            if (toggle != null || enabled)
            {
                toggle = EnsureControl(parent, RollMenuName, RollActive, false, undo);
                SetIncluded(toggle.gameObject, enabled, undo);
                SetGeneratedMenuIcon(toggle, "Rollover", undo);
            }
            // Migrate the generated one-toggle submenu, including old empty containers.
            // Preserve unrelated controls which users may have added to it.
            foreach (var oldMenu in items.Where(m => m != null && m.Control.type == VRCExpressionsMenu.Control.ControlType.SubMenu &&
                         (m.name == LegacyRollMenuName || m.name == RollMenuName)))
            {
                foreach (var child in oldMenu.transform.Cast<Transform>().ToArray())
                {
                    if (undo) Undo.SetTransformParent(child, parent, "Flatten rollover menu");
                    else child.SetParent(parent, false);
                }
                RemoveMenuObject(oldMenu.gameObject, undo);
            }
        }

        private static void RemoveMenuObject(GameObject obj, bool undo)
        {
            if (undo) Undo.DestroyObjectImmediate(obj);
            else Object.DestroyImmediate(obj);
        }

        private static ModularAvatarMenuItem NewMenuItem(Transform parent, string label, bool undo)
        {
            var obj = new GameObject(label);
            obj.transform.SetParent(parent, false);
            var item = obj.AddComponent<ModularAvatarMenuItem>();
            item.Control.name = item.label = label;
            if (undo) Undo.RegisterCreatedObjectUndo(obj, "Add feature menu item");
            return item;
        }

        private static ModularAvatarMenuItem EnsureControl(Transform parent, string label, string parameter, bool radial, bool undo, bool button = false)
        {
            var item = parent.Cast<Transform>().Select(t => t.GetComponent<ModularAvatarMenuItem>())
                .FirstOrDefault(m => m != null && UsesParameter(m, parameter));
            if (item == null) item = NewMenuItem(parent, label, undo);
            RenameMenuItem(item, label, undo);
            if (undo) Undo.RecordObject(item, "Update feature control");
            item.Control.type = radial ? VRCExpressionsMenu.Control.ControlType.RadialPuppet :
                button ? VRCExpressionsMenu.Control.ControlType.Button : VRCExpressionsMenu.Control.ControlType.Toggle;
            item.automaticValue = false;
            if (radial) item.Control.subParameters = new[] { new VRCExpressionsMenu.Control.Parameter { name = parameter } };
            else { item.Control.parameter = new VRCExpressionsMenu.Control.Parameter { name = parameter }; item.Control.value = 1; }
            SaveMenuItem(item);
            return item;
        }

        private static void RenameMenuItem(ModularAvatarMenuItem item, string label, bool undo)
        {
            if (item.name == label && item.label == label && item.Control.name == label) return;
            if (undo) { Undo.RecordObject(item, "Rename feature menu"); Undo.RecordObject(item.gameObject, "Rename feature menu"); }
            item.name = item.label = item.Control.name = label;
            SaveMenuItem(item);
            PrefabUtility.RecordPrefabInstancePropertyModifications(item.gameObject);
        }

        public static bool IsMenuFeatureSetting(string setting) =>
            setting == "features.mirror" || setting == "features.fingerGestureControl" ||
            setting == "features.breathingControl" || setting == "features.rollover" ||
            setting == "features.footHeight" || setting == "features.floating" || setting == "features.photography";

        public static Texture2D GetFeatureMenuIcon(PosingSystem ps, string setting)
        {
            if (ps == null || !IsMenuFeatureSetting(setting)) return null;
            var parameter = setting switch {
                "features.mirror" => "USSPS_Mirror", "features.fingerGestureControl" => "USSPS_FingerGestureDisable_Menu",
                "features.breathingControl" => "USSPS_Breathing", "features.rollover" => RollActive,
                "features.footHeight" => "FootHeight", "features.floating" => FloatHeight, _ => null
            };
            // Read the prefab's existing menu reference, including disabled/EditorOnly items.
            // Never cache by setting name: Kawaii, Cool and customized objects can use different art.
            var item = ps.GetComponentsInChildren<ModularAvatarMenuItem>(true).FirstOrDefault(m =>
                parameter != null ? UsesParameter(m, parameter) :
                m.name == PhotoMenuName || m.name == LegacyPhotoMenuName || m.name == LegacyPhotoAssistanceMenuName);
            if (item != null) return item.Control?.icon;
            // These menus are created on demand. Use exactly the same default as generation.
            var fallback = setting switch {
                "features.breathingControl" => "Breathing", "features.rollover" => "Rollover",
                "features.floating" => "FloorHeight", "features.photography" => "PhotoAssistance", _ => null
            };
            return fallback == null ? null : GetGeneratedMenuIcon(ps, fallback);
        }

        private static Texture2D GetGeneratedMenuIcon(PosingSystem ps, string icon) =>
            ps?.featureMenuIcons?.Get(icon) ?? AssetDatabase.LoadAssetAtPath<Texture2D>(
                "Packages/jp.unisakistudio.posingsystem/ExpressionsMenuIcons/PosingSystem_" + icon + ".png");

        private static void SetGeneratedMenuIcon(ModularAvatarMenuItem item, string icon, bool undo)
        {
            // Upgrade the shared default to the product's art, but preserve custom art.
            var ps = item.GetComponentInParent<PosingSystem>(true);
            var texture = GetGeneratedMenuIcon(ps, icon);
            if (item.Control.icon != null)
            {
                if (ps?.featureMenuIcons?.Get(icon) == null) return;
                var sharedDefault = AssetDatabase.LoadAssetAtPath<Texture2D>(
                    "Packages/jp.unisakistudio.posingsystem/ExpressionsMenuIcons/PosingSystem_" + icon + ".png");
                if (item.Control.icon != sharedDefault) return;
            }
            if (texture == null) return;
            if (undo) Undo.RecordObject(item, "Set feature menu icon");
            item.Control.icon = texture;
            SaveMenuItem(item);
        }

        private static void SaveMenuItem(ModularAvatarMenuItem item)
        {
            EditorUtility.SetDirty(item);
            PrefabUtility.RecordPrefabInstancePropertyModifications(item);
        }

        private static ModularAvatarMenuItem EnsureSimpleMenu(PosingSystem ps, string name, bool enabled, bool undo,
            params (string label, string parameter, bool radial)[] controls)
        {
            var menu = ps.GetComponentsInChildren<ModularAvatarMenuItem>(true).FirstOrDefault(m => m.name == name);
            if (menu == null && enabled)
            {
                var sibling = ps.GetComponentsInChildren<ModularAvatarMenuItem>(true)
                    .FirstOrDefault(m => UsesParameter(m, "USSPS_Mirror"));
                var root = new GameObject(name);
                root.transform.SetParent(sibling != null ? sibling.transform.parent : ps.transform, false);
                menu = root.AddComponent<ModularAvatarMenuItem>();
                menu.MenuSource = SubmenuSource.Children;
                menu.Control.type = VRCExpressionsMenu.Control.ControlType.SubMenu;
                foreach (var control in controls) AddControl(root.transform, control.label, control.parameter, control.radial);
                if (undo) Undo.RegisterCreatedObjectUndo(root, "Add feature menu");
            }
            if (menu != null) SetIncluded(menu.gameObject, enabled, undo);
            return menu;
        }

        private static void AddControl(Transform parent, string label, string parameter, bool radial, bool button = false)
        {
            var obj = new GameObject(label);
            obj.transform.SetParent(parent, false);
            var item = obj.AddComponent<ModularAvatarMenuItem>();
            item.Control.name = label;
            item.Control.type = radial ? VRCExpressionsMenu.Control.ControlType.RadialPuppet :
                button ? VRCExpressionsMenu.Control.ControlType.Button : VRCExpressionsMenu.Control.ControlType.Toggle;
            if (radial) item.Control.subParameters = new[] { new VRCExpressionsMenu.Control.Parameter { name = parameter } };
            else { item.Control.parameter = new VRCExpressionsMenu.Control.Parameter { name = parameter }; item.Control.value = 1; }
        }

        public static void DrawInspector(SerializedObject serialized, PosingSystem ps)
        {
            PosingSystemSettingsInspector.Draw(serialized, ps);
        }
    }
}
