using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace jp.unisakistudio.posingsystemeditor
{
    /// <summary>
    /// PosingSystemPresetDefines の GUID 目録とロード済みデータを、Editor のドメイン内で共有する。
    /// Inspector 生成時は GUID 目録だけを更新し、アセット本体はプリセット UI が必要になるまでロードしない。
    /// </summary>
    internal static class PosingSystemPresetDefinesCache
    {
        private static bool _catalogInitialized;
        private static string[] _catalogGuids = Array.Empty<string>();
        private static List<PosingSystemPresetDefines> _loadedAssets;
        private static List<PosingSystemPresetDefines.PresetDefine> _loadedPresetDefines;

        internal static int CatalogCount => _catalogGuids.Length;
        internal static int AssetLoadRequestCount { get; private set; }
        internal static bool HasLoadedData => _loadedPresetDefines != null;

        /// <summary>
        /// GUID だけで目録を更新する。目録が同一なら、ロード済みアセットをそのまま再利用する。
        /// </summary>
        internal static bool RefreshCatalog()
        {
            var guids = AssetDatabase.FindAssets("t:PosingSystemPresetDefines");
            Array.Sort(guids, StringComparer.Ordinal);

            if (_catalogInitialized && _catalogGuids.SequenceEqual(guids, StringComparer.Ordinal))
            {
                return false;
            }

            _catalogInitialized = true;
            _catalogGuids = guids;
            InvalidateLoadedData();
            return true;
        }

        /// <summary>
        /// プリセット定義が実際に必要になった時だけアセット本体をロードする。
        /// </summary>
        internal static IReadOnlyList<PosingSystemPresetDefines.PresetDefine> GetPresetDefines(bool forceReload = false)
        {
            RefreshCatalog();

            if (forceReload)
            {
                InvalidateLoadedData();
            }

            if (_loadedPresetDefines != null)
            {
                return _loadedPresetDefines;
            }

            _loadedAssets = new List<PosingSystemPresetDefines>(_catalogGuids.Length);
            _loadedPresetDefines = new List<PosingSystemPresetDefines.PresetDefine>();

            foreach (var guid in _catalogGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                AssetLoadRequestCount++;
                var presetDefines = AssetDatabase.LoadAssetAtPath<PosingSystemPresetDefines>(path);
                if (presetDefines == null)
                {
                    continue;
                }

                // オーナーの ScriptableObject も保持し、選択切替後に再ロードされないようにする。
                _loadedAssets.Add(presetDefines);
                if (presetDefines.presetDefines != null)
                {
                    _loadedPresetDefines.AddRange(presetDefines.presetDefines.Where(define => define != null));
                }
            }

            return _loadedPresetDefines;
        }

        private static void InvalidateLoadedData()
        {
            _loadedAssets = null;
            _loadedPresetDefines = null;
        }

        // EditMode テストと性能計測から、ドメインリロードなしでコールド状態を再現するために使用する。
        internal static void ResetForTests()
        {
            _catalogInitialized = false;
            _catalogGuids = Array.Empty<string>();
            _loadedAssets = null;
            _loadedPresetDefines = null;
            AssetLoadRequestCount = 0;
        }
    }
}
