using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    internal sealed class PosingSystemDiagnosticsInspector
    {
        private List<PosingSystemDiagnostics.Issue> issues;
        private double nextCheck;
        private bool showNotes;
        private bool showPreferences;
        private int checkedRevision = -1;
        private int checkedTarget;
        private int checkedDirtyCount;
        private static readonly Dictionary<int, string> failures = new Dictionary<int, string>();

        private void DrawPreferences(SerializedObject serialized, PosingSystem ps)
        {
            showPreferences = DrawBoxFoldout(showPreferences, "警告の表示設定");
            if (!showPreferences) return;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.PropertyField(serialized.FindProperty("hideHierarchyWarnings"), new GUIContent("Hierarchyの警告マークを非表示"));
                if (PosingSystemEditorChangeUtility.Apply(serialized)) GUIUtility.ExitGUI();
                DrawWarningToggle(serialized, ps, PosingSystem.WarningType.All, "すべての警告・注意を非表示");
                DrawWarningToggle(serialized, ps, PosingSystem.WarningType.PrebuildNotRun, "プレビルド未実行の警告を非表示");
                DrawWarningToggle(serialized, ps, PosingSystem.WarningType.PrebuildOutOfDate, "プレビルド後の設定変更の警告を非表示");
                DrawWarningToggle(serialized, ps, PosingSystem.WarningType.Configuration, "ポーズ・アバター設定の注意を非表示");
                DrawWarningToggle(serialized, ps, PosingSystem.WarningType.AutoFootsteps, "自動足踏みに関する案内を非表示");
                EditorGUILayout.LabelField("このポーズツールごとに保存します。エラー表示と、ビルド時に必要な再生成は有効なままです。", EditorStyles.wordWrappedMiniLabel);
            }
        }

        private static void DrawWarningToggle(SerializedObject serialized, PosingSystem ps, PosingSystem.WarningType type, string label)
        {
            var before = ps.IsWarningIgnored(type);
            var after = EditorGUILayout.ToggleLeft(label, before);
            if (before == after) return;
            PosingSystemEditorChangeUtility.Commit(serialized);
            PosingSystemEditor.SetWarningIgnored(ps, type, after);
            serialized.Update();
        }

        internal List<PosingSystemDiagnostics.Issue> GetIssues(PosingSystem ps)
        {
            if (issues == null || checkedTarget != ps.GetInstanceID() || checkedDirtyCount != EditorUtility.GetDirtyCount(ps) ||
                checkedRevision != PosingSystemHierarchyBadge.Revision || EditorApplication.timeSinceStartup >= nextCheck)
            {
                issues = PosingSystemDiagnostics.Collect(ps);
                checkedTarget = ps.GetInstanceID();
                checkedDirtyCount = EditorUtility.GetDirtyCount(ps);
                checkedRevision = PosingSystemHierarchyBadge.Revision;
                nextCheck = EditorApplication.timeSinceStartup + 1;
            }
            return issues;
        }

        internal static void SetFailure(PosingSystem ps, string message)
        {
            if (ps == null) return;
            if (string.IsNullOrEmpty(message)) failures.Remove(ps.GetInstanceID());
            else failures[ps.GetInstanceID()] = message;
        }

        internal void Draw(SerializedObject serialized, PosingSystem ps)
        {
            var visibleIssues = GetIssues(ps).Where(i => i.Severity != MessageType.Warning ||
                !ps.IsWarningIgnored(PosingSystem.WarningType.Configuration)).ToList();
            var visibleWarnings = PosingSystemConverter.GetVisibleWarnings(ps);
            var changed = GUI.changed;
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                var errors = visibleIssues.Count(i => i.Severity == MessageType.Error);
                var warnings = visibleIssues.Count(i => i.Severity == MessageType.Warning);
                if ((visibleWarnings & (PosingSystem.WarningType.PrebuildNotRun | PosingSystem.WarningType.PrebuildOutOfDate)) != 0) warnings++;
                EditorGUILayout.LabelField("設定の確認", errors > 0 ? errors + "件のエラー" : warnings > 0 ? warnings + "件の注意" : "検出できる設定エラーはありません");
                foreach (var issue in visibleIssues.Where(i => i.Severity == MessageType.Error)) DrawIssue(ps, issue);
                if ((visibleWarnings & PosingSystem.WarningType.PrebuildNotRun) != 0)
                    DrawPrebuildWarning(ps, PosingSystem.WarningType.PrebuildNotRun, "まだプレビルドしていません。「プレビルドを更新」で姿勢データを生成できます。");
                if ((visibleWarnings & PosingSystem.WarningType.PrebuildOutOfDate) != 0)
                    DrawPrebuildWarning(ps, PosingSystem.WarningType.PrebuildOutOfDate, "生成後に設定が変わっています。「プレビルドを更新」で現在の設定を反映してください。");
                if (visibleIssues.Any(i => i.Severity != MessageType.Error))
                {
                    showNotes = DrawBoxFoldout(showNotes, "注意・補足を確認（" + visibleIssues.Count(i => i.Severity != MessageType.Error) + "件）");
                    if (showNotes) foreach (var issue in visibleIssues.Where(i => i.Severity != MessageType.Error)) DrawIssue(ps, issue);
                }
                if (failures.TryGetValue(ps.GetInstanceID(), out var failure))
                {
                    EditorGUILayout.HelpBox("前回の処理は完了していません。\n" + failure, MessageType.Error);
                    if (GUILayout.Button("前回の失敗内容をコピー")) EditorGUIUtility.systemCopyBuffer = failure;
                    if (GUILayout.Button("前回の失敗表示を閉じる")) SetFailure(ps, null);
                }
                if (GUILayout.Button("設定を再確認")) { issues = null; nextCheck = 0; PosingSystemHierarchyBadge.Invalidate(ps); }
                DrawPreferences(serialized, ps);
            }
            GUI.changed = changed;
        }

        private static bool DrawBoxFoldout(bool expanded, string label)
        {
            // Reserve space for the foldout arrow inside the surrounding help box.
            using (new EditorGUI.IndentLevelScope())
                return EditorGUILayout.Foldout(expanded, label, true);
        }

        private static void DrawPrebuildWarning(PosingSystem ps, PosingSystem.WarningType type, string message)
        {
            EditorGUILayout.HelpBox(message, MessageType.Warning);
            if (PosingSystemInspectorGUI.Link("この警告を今後表示しない"))
            {
                PosingSystemEditor.SetWarningIgnored(ps, type, true);
                GUIUtility.ExitGUI();
            }
        }

        private static void DrawIssue(PosingSystem ps, PosingSystemDiagnostics.Issue issue)
        {
            EditorGUILayout.HelpBox(issue.Message, issue.Severity);
            if (issue.Context != null && GUILayout.Button("対象を選択（" + issue.Code + "）"))
            {
                Selection.activeObject = issue.Context;
                EditorGUIUtility.PingObject(issue.Context);
                GUIUtility.ExitGUI();
            }
        }
    }
}
