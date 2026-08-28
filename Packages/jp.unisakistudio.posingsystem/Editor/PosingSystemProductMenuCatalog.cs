using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;

namespace jp.unisakistudio.posingsystemeditor
{
    /// <summary>
    /// 既存商品チェックで参照する ExpressionsMenu の軽量な目録。
    /// AssetDatabase.FindAssets を商品・アバターごとに繰り返さないため、プロジェクト変更まで共有する。
    /// </summary>
    [InitializeOnLoad]
    internal static class PosingSystemProductMenuCatalog
    {
        private readonly struct MenuAssetEntry
        {
            internal readonly string Guid;
            internal readonly string Name;

            internal MenuAssetEntry(string guid, string name)
            {
                Guid = guid;
                Name = name;
            }
        }

        private static List<MenuAssetEntry> menuAssets;
        private static readonly Dictionary<string, HashSet<string>> matchingGuids =
            new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

        internal static int AssetSearchRequestCount { get; private set; }

        static PosingSystemProductMenuCatalog()
        {
            EditorApplication.projectChanged += Invalidate;
        }

        internal static IReadOnlyCollection<string> GetMatchingGuids(string menuName)
        {
            EnsureCatalog();

            if (string.IsNullOrEmpty(menuName))
            {
                return Array.Empty<string>();
            }

            if (matchingGuids.TryGetValue(menuName, out var cachedGuids))
            {
                return cachedGuids;
            }

            var guids = new HashSet<string>();
            foreach (var menuAsset in menuAssets)
            {
                if (menuAsset.Name.IndexOf(menuName, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    guids.Add(menuAsset.Guid);
                }
            }

            matchingGuids[menuName] = guids;
            return guids;
        }

        private static void EnsureCatalog()
        {
            if (menuAssets != null)
            {
                return;
            }

            AssetSearchRequestCount++;
            menuAssets = new List<MenuAssetEntry>();
            foreach (var guid in AssetDatabase.FindAssets("t:VRCExpressionsMenu"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                menuAssets.Add(new MenuAssetEntry(guid, Path.GetFileNameWithoutExtension(path)));
            }
        }

        internal static void Invalidate()
        {
            menuAssets = null;
            matchingGuids.Clear();
        }

        internal static void ResetForTests()
        {
            Invalidate();
            AssetSearchRequestCount = 0;
        }
    }
}
