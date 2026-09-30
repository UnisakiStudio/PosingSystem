using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace jp.unisakistudio.posingsystemeditor
{
    [InitializeOnLoad]
    internal static class PosingSystemPresetDefinesCache
    {
        internal const string DefaultCatalogPath = "Assets/UnisakiStudio/Editor/PosingSystemPresetCatalog.asset";
        private const int SchemaVersion = 1;
        private static PosingSystemPresetCatalog catalog;
        private static string versionSignature;

        internal static int AssetSearchRequestCount { get; private set; }
        internal static int AssetLoadRequestCount { get; private set; }
        internal static int PresetLoadRequestCount { get; private set; }

        static PosingSystemPresetDefinesCache()
        {
            // Recheck only package metadata. Adding/editing presets never invalidates the catalog.
            EditorApplication.projectChanged += () => versionSignature = null;
        }

        internal static PosingSystemPresetCatalog GetCatalog(PosingSystem posingSystem = null, bool forceReload = false)
        {
            var path = DefaultCatalogPath;
            if (catalog != null && EditorUtility.IsPersistent(catalog))
                path = AssetDatabase.GetAssetPath(catalog);
            else if (posingSystem != null && posingSystem.presetCatalogObject is PosingSystemPresetCatalog referenced
                && EditorUtility.IsPersistent(referenced))
                path = AssetDatabase.GetAssetPath(referenced);
            else if (!File.Exists(path))
            {
                // Recover a moved catalog once, rather than making one for each avatar.
                var existing = AssetDatabase.FindAssets("t:PosingSystemPresetCatalog", new[] { "Assets" });
                if (existing.Length > 0) path = AssetDatabase.GUIDToAssetPath(existing.OrderBy(g => g).First());
            }

            var result = LoadCatalog(path, GetVersionSignature(), forceReload, null, true);
            if (posingSystem != null && posingSystem.presetCatalogObject != result)
            {
                posingSystem.presetCatalogObject = result;
                EditorUtility.SetDirty(posingSystem);
                if (PrefabUtility.IsPartOfPrefabInstance(posingSystem))
                    PrefabUtility.RecordPrefabInstancePropertyModifications(posingSystem);
            }
            return result;
        }

        // The path and version inputs also let tests exercise real persistence in an isolated folder.
        internal static PosingSystemPresetCatalog LoadCatalog(
            string path, string versions, bool forceReload, string[] searchFolders, bool showProgress)
        {
            if (catalog == null || AssetDatabase.GetAssetPath(catalog) != path)
                catalog = AssetDatabase.LoadAssetAtPath<PosingSystemPresetCatalog>(path);

            if (!forceReload && catalog != null && catalog.schemaVersion == SchemaVersion
                && catalog.packageVersions == versions) return catalog;

            var entries = ReadEntries(searchFolders, showProgress);
            if (catalog == null)
            {
                if (AssetDatabase.LoadMainAssetAtPath(path) != null || File.Exists(path))
                    throw new InvalidOperationException("プリセット一覧の保存先に別のファイルがあります: " + path);
                PosingSystemEditor.EnsureAssetFolderExists(Path.GetDirectoryName(path).Replace('\\', '/'));
                catalog = ScriptableObject.CreateInstance<PosingSystemPresetCatalog>();
                AssetDatabase.CreateAsset(catalog, path);
            }
            // Commit the new catalog only after all definitions have been read successfully.
            catalog.entries = entries;
            catalog.packageVersions = versions;
            catalog.schemaVersion = SchemaVersion;
            catalog.revision++;
            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssetIfDirty(catalog);
            return catalog;
        }

        private static List<PosingSystemPresetCatalog.Entry> ReadEntries(string[] searchFolders, bool showProgress)
        {
            AssetSearchRequestCount++;
            var guids = searchFolders == null
                ? AssetDatabase.FindAssets("t:PosingSystemPresetDefines")
                : AssetDatabase.FindAssets("t:PosingSystemPresetDefines", searchFolders);
            Array.Sort(guids, StringComparer.Ordinal);
            var entries = new List<PosingSystemPresetCatalog.Entry>();
            try
            {
                for (var index = 0; index < guids.Length; index++)
                {
                    var path = AssetDatabase.GUIDToAssetPath(guids[index]);
                    if (showProgress)
                        EditorUtility.DisplayProgressBar("プリセット一覧を更新", Path.GetFileNameWithoutExtension(path),
                            (float)index / Math.Max(1, guids.Length));
                    AssetLoadRequestCount++;
                    var asset = AssetDatabase.LoadAssetAtPath<PosingSystemPresetDefines>(path);
                    if (asset == null || asset.presetDefines == null) continue;
                    for (var i = 0; i < asset.presetDefines.Count; i++)
                    {
                        var define = asset.presetDefines[i];
                        if (define == null) continue;
                        var entry = new PosingSystemPresetCatalog.Entry
                        {
                            sourceGuid = guids[index], sourceIndex = i, sourcePath = path,
                            avatarName = define.avatarName, official = IsOfficialPath(path),
                            prefabsHashes = new List<string>(define.prefabsHashes ?? new List<string>()),
                            prefabsNames = new List<string>(define.prefabsNames ?? new List<string>())
                        };
                        if (define.preset != null)
                            AssetDatabase.TryGetGUIDAndLocalFileIdentifier(define.preset, out entry.presetGuid, out entry.presetLocalId);
                        foreach (var prefab in define.prefabs ?? new List<GameObject>())
                        {
                            if (prefab == null) continue;
                            entry.toolGuids.Add(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(prefab)));
                            entry.toolNames.Add(prefab.name);
                        }
                        entries.Add(entry);
                    }
                }
            }
            finally
            {
                if (showProgress) EditorUtility.ClearProgressBar();
            }
            return entries;
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

        [Serializable]
        private sealed class Manifest { public string version; }

        private static string GetVersionSignature()
        {
            if (versionSignature != null) return versionSignature;
            var versions = new List<string>();
            foreach (var package in PackageInfo.GetAllRegisteredPackages()
                .Where(p => p.name.StartsWith("jp.unisakistudio.", StringComparison.Ordinal)).OrderBy(p => p.name))
            {
                var version = package.version;
                try
                {
                    var path = Path.Combine(package.resolvedPath, "package.json");
                    if (File.Exists(path)) version = JsonUtility.FromJson<Manifest>(File.ReadAllText(path))?.version ?? version;
                }
                catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException || ex is ArgumentException)
                {
                    // The registered version remains usable if the manifest is temporarily unavailable.
                }
                versions.Add(package.name + "@" + version);
            }
            return versionSignature = string.Join("\n", versions);
        }

        internal static void ResetForTests()
        {
            catalog = null;
            versionSignature = null;
            AssetSearchRequestCount = AssetLoadRequestCount = PresetLoadRequestCount = 0;
        }
    }
}
