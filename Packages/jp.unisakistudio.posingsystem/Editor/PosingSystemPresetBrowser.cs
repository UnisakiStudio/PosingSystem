using System;
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    internal sealed class PosingSystemPresetBrowser
    {
        private PosingSystemPresetCatalog catalog;
        private int revision = -1;
        private string avatarMatchKey;
        private PosingSystem owner;
        private bool loadFailed;
        private List<PosingSystemPresetCatalog.Entry> entries = new List<PosingSystemPresetCatalog.Entry>();
        private PosingSystemPresetCatalog.Entry selected;
        private PresetTreeView list;
        private string search = "";
        private string error;
        private bool showBrowser;
        private string displayLanguage;

        internal void Draw(PosingSystem posingSystem, Action<string> drawWarning)
        {
            if (list == null) list = new PresetTreeView(entry => selected = entry);
            if (displayLanguage != PosingSystemLocalization.Language)
            {
                displayLanguage = PosingSystemLocalization.Language;
                list.Reload();
            }

            if (!loadFailed)
            {
                try
                {
                    var current = PosingSystemPresetDefinesCache.GetCatalog(posingSystem);
                    var avatar = posingSystem.GetAvatar();
                    var matchKey = PosingSystemPresetDefinesCache.GetAvatarMatchKey(avatar != null ? avatar.gameObject : null);
                    if (owner != posingSystem || catalog != current || revision != (current != null ? current.revision : -1) || avatarMatchKey != matchKey)
                        SetCatalog(posingSystem, current);
                    if (!string.IsNullOrEmpty(PosingSystemPresetDefinesCache.LastError))
                        error = "プリセット一覧を読み込めませんでした。「再読込」で再試行できます: " + PosingSystemPresetDefinesCache.LastError;
                }
                catch (Exception ex)
                {
                    loadFailed = true;
                    error = "プリセット一覧を読み込めませんでした。「再読込」で再試行できます: " + ex.Message;
                }
            }

            if (PosingSystemInspectorGUI.Link(showBrowser ? "プリセット一覧を閉じる" : "ほかのプリセットを探す")) showBrowser = !showBrowser;
            if (showBrowser)
            {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(PosingSystemLocalization.Text("プリセット選択"), EditorStyles.boldLabel);
            if (GUILayout.Button(PosingSystemLocalization.Text("再読込"), GUILayout.Width(72)))
            {
                try
                {
                    SetCatalog(posingSystem, PosingSystemPresetDefinesCache.GetCatalog(posingSystem, true));
                }
                catch (Exception ex)
                {
                    loadFailed = true;
                    error = "プリセット一覧を更新できませんでした: " + ex.Message;
                }
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(PosingSystemLocalization.Text("一覧は追加・変更時に更新されます。表示が変わらない場合は「再読込」を押してください。"), EditorStyles.wordWrappedMiniLabel);
            if (catalog != null)
            {
                EditorGUI.BeginChangeCheck();
                var hideBundled = EditorGUILayout.ToggleLeft(new GUIContent(PosingSystemLocalization.Text("同梱プリセットを表示しない"),
                    PosingSystemLocalization.Text("ツールに同梱された公式プリセットを一覧と検索結果に表示しません。同じプロジェクト内のアバターで共有する設定です。")),
                    catalog.hideBundledPresets);
                if (EditorGUI.EndChangeCheck())
                {
                    PosingSystemPresetDefinesCache.SetHideBundledPresets(hideBundled);
                }
            }
            search = EditorGUILayout.TextField(PosingSystemLocalization.Text("名前で検索"), search);
            list.SetFilter(search, catalog != null && catalog.hideBundledPresets);
            list.OnGUI(GUILayoutUtility.GetRect(0, 240, GUILayout.ExpandWidth(true)));

            if (catalog != null && list.MatchCount == 0)
                EditorGUILayout.HelpBox(PosingSystemLocalization.Text("該当するプリセットがありません。"), MessageType.Info);
            }
            if (selected != null) EditorGUILayout.LabelField(PosingSystemLocalization.Text(selected.avatarName), PosingSystemInspectorGUI.Text(12, true));
            if (!string.IsNullOrEmpty(error)) drawWarning(error);

            using (new EditorGUI.DisabledScope(selected == null))
            {
                if (GUILayout.Button(PosingSystemLocalization.Text(selected == null ? "プリセットを選択してください" : "このプリセットに変更する")))
                {
                    if (PosingSystemPresetDefinesCache.ApplyPreset(posingSystem, selected))
                    {
                        error = null;
                        GUI.changed = true;
                    }
                    else
                    {
                        error = "このプリセットは削除されたか、現在のツールに適用できません。「再読込」で一覧を更新してください。";
                    }
                }
            }
        }

        private void SetCatalog(PosingSystem posingSystem, PosingSystemPresetCatalog current)
        {
            owner = posingSystem;
            loadFailed = false;
            catalog = current;
            revision = current != null ? current.revision : -1;
            var prefab = PrefabUtility.GetCorrespondingObjectFromSource(posingSystem.gameObject);
            entries = current != null
                ? PosingSystemPresetDefinesCache.GetMatchingEntries(current, prefab, posingSystem.gameObject.name)
                : new List<PosingSystemPresetCatalog.Entry>();
            var descriptor = posingSystem.GetAvatar();
            var avatar = descriptor != null ? descriptor.gameObject : null;
            avatarMatchKey = PosingSystemPresetDefinesCache.GetAvatarMatchKey(avatar);
            list.SetEntries(entries, PosingSystemPresetDefinesCache.GetAvatarMatchingEntries(entries, avatar));
            selected = PosingSystemPresetDefinesCache.GetAvatarMatchingEntries(entries, avatar)
                .FirstOrDefault(entry => current != null && (!current.hideBundledPresets || !entry.official));
            error = null;
        }

        private sealed class PresetTreeView : TreeView
        {
            private const int AvatarGroup = 3;
            private const int UserGroup = 1;
            private const int OfficialGroup = 2;
            private readonly Action<PosingSystemPresetCatalog.Entry> onSelectionChanged;
            private readonly Dictionary<int, PosingSystemPresetCatalog.Entry> entryById = new();
            private List<PosingSystemPresetCatalog.Entry> items = new();
            private HashSet<PosingSystemPresetCatalog.Entry> avatarMatches = new();
            private string filter = "";
            private bool suppressExpansionSave;
            private bool hideBundled;
            internal int MatchCount { get; private set; }

            internal PresetTreeView(Action<PosingSystemPresetCatalog.Entry> onSelectionChanged)
                : base(new TreeViewState())
            {
                this.onSelectionChanged = onSelectionChanged;
                rowHeight = 18;
                showBorder = true;
                Rebuild();
            }

            internal void SetEntries(List<PosingSystemPresetCatalog.Entry> entries,
                IEnumerable<PosingSystemPresetCatalog.Entry> matches)
            {
                items = entries;
                avatarMatches = new HashSet<PosingSystemPresetCatalog.Entry>(matches);
                SetSelection(Array.Empty<int>());
                onSelectionChanged(null);
                Rebuild();
            }

            internal void SetFilter(string value, bool hideBundledPresets)
            {
                if (filter == value && hideBundled == hideBundledPresets) return;
                var previousSelection = GetSelection().ToArray();
                filter = value;
                hideBundled = hideBundledPresets;
                // Search reveals matches inside collapsed groups; clearing it restores their state.
                Rebuild();
                if (previousSelection.Any(id => !entryById.ContainsKey(id)))
                {
                    SetSelection(Array.Empty<int>());
                    onSelectionChanged(null);
                }
            }

            protected override TreeViewItem BuildRoot()
            {
                var root = new TreeViewItem(0, -1, "Presets");
                var compatible = new TreeViewItem(AvatarGroup, 0, PosingSystemLocalization.Text("アバターに適合"));
                var user = new TreeViewItem(UserGroup, 0, PosingSystemLocalization.Text("ユーザー作成"));
                var official = new TreeViewItem(OfficialGroup, 0, PosingSystemLocalization.Text("ゆにさきスタジオ公式"));
                root.AddChild(compatible);
                root.AddChild(user);
                if (!hideBundled) root.AddChild(official);
                entryById.Clear();
                MatchCount = 0;
                for (var i = 0; i < items.Count; i++)
                {
                    var entry = items[i];
                    if (hideBundled && entry.official) continue;
                    if (!string.IsNullOrEmpty(filter)
                        && (entry.avatarName ?? "").IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) < 0
                        && (PosingSystemLocalization.Text(entry.avatarName) ?? "").IndexOf(filter, StringComparison.CurrentCultureIgnoreCase) < 0)
                        continue;
                    // IDs stay stable when the search filters other rows out.
                    var id = i * 2 + 4;
                    entryById.Add(id, entry);
                    var label = PosingSystemLocalization.Text(string.IsNullOrWhiteSpace(entry.avatarName) ? "(名前なし)" : entry.avatarName);
                    (entry.official ? official : user).AddChild(new TreeViewItem(id, 1, label));
                    MatchCount++;
                    if (avatarMatches.Contains(entry))
                    {
                        entryById.Add(id + 1, entry);
                        compatible.AddChild(new TreeViewItem(id + 1, 1,
                            label + PosingSystemLocalization.Text(entry.official ? "（公式）" : "（ユーザー作成）")));
                    }
                }
                compatible.displayName += " (" + (compatible.children?.Count ?? 0) + ")";
                user.displayName += " (" + (user.children?.Count ?? 0) + ")";
                official.displayName += " (" + (official.children?.Count ?? 0) + ")";
                return root;
            }

            protected override bool CanMultiSelect(TreeViewItem item) => false;

            protected override void SelectionChanged(IList<int> selectedIds)
            {
                onSelectionChanged(selectedIds.Count == 1 && entryById.TryGetValue(selectedIds[0], out var entry)
                    ? entry : null);
            }

            protected override void ExpandedStateChanged()
            {
                if (suppressExpansionSave || !string.IsNullOrEmpty(filter)) return;
                SessionState.SetBool("PosingSystem.Presets.Avatar", IsExpanded(AvatarGroup));
                SessionState.SetBool("PosingSystem.Presets.User", IsExpanded(UserGroup));
                if (!hideBundled) SessionState.SetBool("PosingSystem.Presets.Official", IsExpanded(OfficialGroup));
            }

            protected override void RowGUI(RowGUIArgs args)
            {
                base.RowGUI(args);
                if (entryById.TryGetValue(args.item.id, out var entry))
                    GUI.Label(args.rowRect, new GUIContent("", entry.sourcePath), GUIStyle.none);
            }

            private void Rebuild()
            {
                suppressExpansionSave = true;
                try
                {
                    Reload();
                    RestoreExpansion();
                }
                finally { suppressExpansionSave = false; }
            }

            private void RestoreExpansion()
            {
                var searching = !string.IsNullOrEmpty(filter);
                var compatible = searching || SessionState.GetBool("PosingSystem.Presets.Avatar", true);
                var user = searching || SessionState.GetBool("PosingSystem.Presets.User", true);
                var official = searching || SessionState.GetBool("PosingSystem.Presets.Official", false);
                var expanded = new List<int>();
                if (compatible) expanded.Add(AvatarGroup);
                if (user) expanded.Add(UserGroup);
                if (official && !hideBundled) expanded.Add(OfficialGroup);
                SetExpanded(expanded);
            }
        }
    }
}
