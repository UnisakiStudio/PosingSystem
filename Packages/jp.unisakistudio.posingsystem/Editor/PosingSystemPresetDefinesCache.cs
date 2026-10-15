using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    [InitializeOnLoad]
    internal static class PosingSystemPresetDefinesCache
    {
        // Old assets are only read once to retain their display preference. No catalog is saved.
        internal const string DefaultCatalogPath = "Assets/UnisakiStudio/Editor/PosingSystemPresetCatalog.asset";
        private static PosingSystemPresetCatalog catalog;
        private static string sourceKey;
        private static bool invalidated = true;
        private static readonly HashSet<string> definitionPaths = new(StringComparer.Ordinal);
        internal static string VisibilityPreferenceKey => "PosingSystem.Presets.HideBundled." + Hash128.Compute(Application.dataPath);
        internal static string LastError { get; private set; }
        internal static int AssetSearchRequestCount { get; private set; }
        internal static int AssetLoadRequestCount { get; private set; }
        internal static int PresetLoadRequestCount { get; private set; }

        static PosingSystemPresetDefinesCache()
        {
            PosingSystemPresetDefines.Changed += Invalidate;
            Undo.undoRedoPerformed += OnUndoRedo;
            AssemblyReloadEvents.beforeAssemblyReload += Release;
            EditorApplication.quitting += Release;
        }

        internal static void Invalidate() => invalidated = true;

        private static void OnUndoRedo()
        {
            if (catalog != null) EditorPrefs.SetBool(VisibilityPreferenceKey, catalog.hideBundledPresets);
            Invalidate();
        }

        private static void Release()
        {
            if (catalog != null) UnityEngine.Object.DestroyImmediate(catalog);
            catalog = null;
            sourceKey = null;
            definitionPaths.Clear();
            invalidated = true;
            LastError = null;
        }

        internal static PosingSystemPresetCatalog GetCatalog(PosingSystem posingSystem = null, bool forceReload = false)
        {
            EnsureVisibilityPreference(posingSystem);
            return RequestCatalog(null, forceReload);
        }

        // Only lightweight metadata is kept in memory, shared by all Inspectors for this domain.
        internal static PosingSystemPresetCatalog RequestCatalog(string[] searchFolders, bool forceReload)
        {
            var key = searchFolders == null ? "*" : string.Join("\n", searchFolders);
            if (!forceReload && !invalidated && sourceKey == key)
                return string.IsNullOrEmpty(LastError) ? catalog : null;
            try
            {
                AssetSearchRequestCount++;
                var guids = searchFolders == null ? AssetDatabase.FindAssets("t:PosingSystemPresetDefines")
                    : AssetDatabase.FindAssets("t:PosingSystemPresetDefines", searchFolders);
                Array.Sort(guids, StringComparer.Ordinal);
                var entries = new List<PosingSystemPresetCatalog.Entry>();
                foreach (var guid in guids) ReadEntry(guid, entries);
                if (catalog == null)
                {
                    catalog = ScriptableObject.CreateInstance<PosingSystemPresetCatalog>();
                    catalog.hideFlags = HideFlags.HideAndDontSave;
                    catalog.hideBundledPresets = EditorPrefs.GetBool(VisibilityPreferenceKey, false);
                }
                catalog.entries = entries;
                catalog.revision++;
                definitionPaths.Clear();
                foreach (var guid in guids) definitionPaths.Add(AssetDatabase.GUIDToAssetPath(guid));
                LastError = null;
            }
            catch (Exception ex)
            {
                LastError = ex.Message;
            }
            sourceKey = key;
            invalidated = false;
            return string.IsNullOrEmpty(LastError) ? catalog : null;
        }

        private static void EnsureVisibilityPreference(PosingSystem posingSystem)
        {
            if (EditorPrefs.HasKey(VisibilityPreferenceKey)) return;
            var legacy = posingSystem != null ? posingSystem.presetCatalogObject as PosingSystemPresetCatalog : null;
            if (legacy == null) legacy = AssetDatabase.LoadAssetAtPath<PosingSystemPresetCatalog>(DefaultCatalogPath);
            if (legacy == null)
            {
                // Existing users may have moved their old generated catalog.
                var guid = AssetDatabase.FindAssets("t:PosingSystemPresetCatalog", new[] { "Assets" })
                    .OrderBy(x => x, StringComparer.Ordinal).FirstOrDefault();
                if (guid != null) legacy = AssetDatabase.LoadAssetAtPath<PosingSystemPresetCatalog>(AssetDatabase.GUIDToAssetPath(guid));
            }
            EditorPrefs.SetBool(VisibilityPreferenceKey, legacy != null && legacy.hideBundledPresets);
            if (catalog != null) catalog.hideBundledPresets = EditorPrefs.GetBool(VisibilityPreferenceKey);
        }

        internal static void SetHideBundledPresets(bool value)
        {
            if (catalog != null && catalog.hideBundledPresets != value)
            {
                Undo.RecordObject(catalog, "Change Preset Visibility");
                catalog.hideBundledPresets = value;
            }
            EditorPrefs.SetBool(VisibilityPreferenceKey, value);
        }

        internal static void NotifyAssetsChanged(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            // Unrelated animation/texture imports do not cause another preset search.
            if (deleted.Concat(movedFrom).Any(path => definitionPaths.Contains(path) || path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase))
                || imported.Concat(moved).Any(path => path.EndsWith(".prefab", StringComparison.OrdinalIgnoreCase)
                    || (path.EndsWith(".asset", StringComparison.OrdinalIgnoreCase)
                        && AssetDatabase.GetMainAssetTypeAtPath(path) == typeof(PosingSystemPresetDefines))))
                Invalidate();
        }

        private static void ReadEntry(string guid, List<PosingSystemPresetCatalog.Entry> entries)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            AssetLoadRequestCount++;
            var asset = AssetDatabase.LoadAssetAtPath<PosingSystemPresetDefines>(path);
            if (asset == null || asset.presetDefines == null) return;
            for (var i = 0; i < asset.presetDefines.Count; i++)
            {
                var define = asset.presetDefines[i];
                if (define == null) continue;
                var entry = new PosingSystemPresetCatalog.Entry
                {
                    sourceGuid = guid, sourceIndex = i, sourcePath = path,
                    avatarName = define.avatarName, official = IsOfficialPath(path),
                    prefabsHashes = new List<string>(define.prefabsHashes ?? new List<string>()),
                    prefabsNames = new List<string>(define.prefabsNames ?? new List<string>())
                };
                if (define.preset.isSet)
                    AssetDatabase.TryGetGUIDAndLocalFileIdentifier(define.preset.instanceID, out entry.presetGuid, out entry.presetLocalId);
                foreach (var prefab in define.prefabs ?? new List<LazyLoadReference<GameObject>>())
                {
                    if (!prefab.isSet) continue;
                    var prefabPath = AssetDatabase.GetAssetPath(prefab.instanceID);
                    if (string.IsNullOrEmpty(prefabPath)) continue;
                    entry.toolGuids.Add(AssetDatabase.AssetPathToGUID(prefabPath));
                    // Unity prefab asset names follow their filename; do not dereference .asset/.name here.
                    entry.toolNames.Add(Path.GetFileNameWithoutExtension(prefabPath));
                }
                entries.Add(entry);
            }
        }

        internal static bool IsOfficialPath(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            var parts = path.Replace('\\', '/').Split('/');
            return parts.Length > 4 && parts[0] == "Packages"
                && parts[1].StartsWith("jp.unisakistudio.", StringComparison.Ordinal)
                && parts[2] == "Resources" && parts[3] == "Presets";
        }

        internal static List<PosingSystemPresetCatalog.Entry> GetMatchingEntries(
            PosingSystemPresetCatalog source, GameObject prefab, string fallbackName)
        {
            var guid = prefab != null ? AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab)) : null;
            var name = prefab != null ? prefab.name : fallbackName;
            return source.entries.Where(entry => entry.MatchesTool(guid, name))
                .OrderBy(entry => entry.official)
                .ThenBy(entry => string.IsNullOrWhiteSpace(entry.avatarName))
                .ThenBy(entry => entry.avatarName, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(entry => entry.Key, StringComparer.Ordinal).ToList();
        }

        internal static string GetAvatarMatchKey(GameObject avatar)
        {
            // Watch only this avatar's prefab chain; this never scans the project's presets.
            return string.Join("\n", GetAvatarSources(avatar).Select(source => source.GetInstanceID() + ":" + source.name));
        }

        internal static List<PosingSystemPresetCatalog.Entry> GetAvatarMatchingEntries(
            IEnumerable<PosingSystemPresetCatalog.Entry> entries, GameObject avatar)
        {
            var candidates = entries.ToList();
            var sources = GetAvatarSources(avatar).ToList();
            var names = new HashSet<string>(sources.Select(source => source.name), StringComparer.Ordinal);
            var hashes = new HashSet<string>(sources
                .Select(source => AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source)))
                .Where(guid => !string.IsNullOrEmpty(guid))
                .Select(PosingSystemMenuItems.GetPosingSystemGUIDHash), StringComparer.Ordinal);
            var matched = candidates.Where(entry => entry.prefabsHashes != null
                && entry.prefabsHashes.Any(hashes.Contains)).ToList();
            // Match the installation recommendation: use names only when no prefab GUID matches.
            return matched.Count > 0 ? matched : candidates.Where(entry => entry.prefabsNames != null
                && entry.prefabsNames.Any(names.Contains)).ToList();
        }

        private static IEnumerable<GameObject> GetAvatarSources(GameObject avatar)
        {
            for (var source = avatar; source != null; source = PrefabUtility.GetCorrespondingObjectFromSource(source))
                yield return source;
        }

        internal static Preset LoadPreset(PosingSystemPresetCatalog.Entry entry)
        {
            if (entry == null || string.IsNullOrEmpty(entry.presetGuid)) return null;
            var path = AssetDatabase.GUIDToAssetPath(entry.presetGuid);
            if (string.IsNullOrEmpty(path)) return null;
            PresetLoadRequestCount++;
            var preset = AssetDatabase.LoadAssetAtPath<Preset>(path);
            if (preset != null && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(preset, out string guid, out long localId)
                && localId == entry.presetLocalId) return preset;
            return AssetDatabase.LoadAllAssetsAtPath(path).OfType<Preset>().FirstOrDefault(candidate =>
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(candidate, out string candidateGuid, out long candidateId)
                && candidateId == entry.presetLocalId);
        }

        internal static bool ApplyPreset(PosingSystem posingSystem, PosingSystemPresetCatalog.Entry entry)
        {
            var preset = LoadPreset(entry);
            if (preset == null || !preset.CanBeAppliedTo(posingSystem)) return false;
            Undo.RecordObject(posingSystem, "Apply Pose Preset");
            var applied = preset.ApplyTo(posingSystem, new[] { "defines", "overrideDefines" });
            if (applied)
            {
                PosingSystemAvatarAdjustmentInspector.ConfigurePresetApplied(posingSystem);
                posingSystem.previousErrorCheckTime = DateTime.MinValue;
                EditorUtility.SetDirty(posingSystem);
                if (PrefabUtility.IsPartOfPrefabInstance(posingSystem))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(posingSystem);
            }
            return applied;
        }

        internal static void ResetForTests()
        {
            Release();
            AssetSearchRequestCount = AssetLoadRequestCount = PresetLoadRequestCount = 0;
        }
    }

    internal sealed class PosingSystemPresetAssetPostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
            => PosingSystemPresetDefinesCache.NotifyAssetsChanged(imported, deleted, moved, movedFrom);
    }
}
