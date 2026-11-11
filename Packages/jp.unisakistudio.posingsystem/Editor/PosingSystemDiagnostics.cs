using System;
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
#if NDMF && MODULAR_AVATAR
    internal sealed class PosingSystemCompilationPrecheck : VRC.SDKBase.Editor.BuildPipeline.IVRCSDKPreprocessAvatarCallback
    {
        public int callbackOrder => int.MinValue;
        public bool OnPreprocessAvatar(GameObject avatar)
        {
            if (!EditorUtility.scriptCompilationFailed || avatar.GetComponentInChildren<PosingSystem>(true) == null)
                return true;
            Debug.LogWarning(PosingSystemDiagnostics.CompilationFailure(avatar).Message, avatar);
            return false;
        }
    }
#endif

    /// <summary>Read-only checks shared by the Inspector and conversion entry point.</summary>
    public static class PosingSystemDiagnostics
    {
        public const string RestoreTool = "シーンを保存・バックアップしてから、ALCOM / VCCで商品本体と「ゆにさきポーズシステム」を再取得してください。入れ直しても直らない場合は、Inspector上部の「ヘルプ・変更履歴」から公式マニュアル・お問い合わせをご確認ください。調整済みのオブジェクトやGeneratedResourcesは削除しないでください。";

        internal const string CommonAnimatorId = "jp.unisakistudio.posingsystem_locomotion";
        internal const string RolloverClipAction = "中央と横向きに、Humanoid形式のクリップを指定してください。BlendTree・Legacy・身体カーブのないクリップは使用できません。";
        private const string CommonAnimatorPrefabGuid = "16f1d2fbf5ebb4d48b442583001516a2";
        internal static ModularAvatarMergeAnimator CommonAnimatorTemplate()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(CommonAnimatorPrefabGuid));
            return prefab != null ? prefab.GetComponent<ModularAvatarMergeAnimator>() : null;
        }

        internal static DuplicateEraser[] CommonAnimatorMarkers(PosingSystem ps) =>
            ps.GetComponentsInChildren<DuplicateEraser>(true).Where(d => d.ID == CommonAnimatorId &&
                d.GetComponentInParent<PosingSystem>(true) == ps && ps.GetAvatar() == d.GetComponentInParent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>(true)).ToArray();

        internal static bool ActiveCommonMarker(DuplicateEraser marker) =>
            PosingSystemHierarchy.BlockingParent(marker.transform,
                marker.GetComponentInParent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>(true)?.transform) == null;

        private static GameObject[] UnmarkedCommonChildren(PosingSystem ps, ModularAvatarMergeAnimator template) =>
            ps.GetComponentsInChildren<Transform>(true).Where(t => t != ps.transform &&
                t.GetComponentInParent<PosingSystem>(true) == ps && t.GetComponent<DuplicateEraser>() == null &&
                (PrefabUtility.GetCorrespondingObjectFromOriginalSource(t.gameObject) == template.gameObject ||
                 (t.name == template.name && t.GetComponent<ModularAvatarMergeAnimator>()?.layerType ==
                     VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base)))
                .Select(t => t.gameObject).ToArray();

        internal static bool CanRecoverCommonChild(PosingSystem ps, DuplicateEraser[] markers, out GameObject child)
        {
            child = null;
            var avatar = ps.GetAvatar()?.transform;
            if (markers.Length != 0 || PosingSystemHierarchy.BlockingParent(ps.transform, avatar) != null) return false;
            var template = CommonAnimatorTemplate();
            if (template == null || template.animator == null) return false;
            var candidates = UnmarkedCommonChildren(ps, template);
            if (candidates.Length == 0) return true;
            if (candidates.Length != 1) return false;
            child = candidates[0];
            return PosingSystemHierarchy.BlockingParent(child.transform, avatar) == null &&
                GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child) == 0;
        }

        public sealed class Issue
        {
            public readonly string Code, Summary, Action;
            public readonly MessageType Severity;
            public readonly UnityEngine.Object Context;
            public Issue(string code, string summary, string action, MessageType severity, UnityEngine.Object context)
            { Code = code; Summary = summary; Action = action; Severity = severity; Context = context; }
            public string Message => "[" + Code + "] " + PosingSystemLocalization.TextMessage(Summary) + "\n" + PosingSystemLocalization.Text("対処:") + " " + PosingSystemLocalization.TextMessage(Action);
        }

        public sealed class DiagnosticException : InvalidOperationException
        {
            public readonly string Code;
            public readonly UnityEngine.Object Context;
            public DiagnosticException(string code, string summary, string action, UnityEngine.Object context, Exception inner = null)
                : base(DescribeFailure(code, summary, action, context), inner) { Context = context; Code = code; }
        }

        private static string DescribeFailure(string code, string summary, string action, UnityEngine.Object context)
        {
            var system = context as PosingSystem;
            if (system == null && context is Component component) system = component.GetComponentInParent<PosingSystem>(true);
            using var languageScope = PosingSystemLocalization.ForInspector(system);
            return "[PosingSystem / " + code + "] " + PosingSystemLocalization.TextMessage(summary) + "\n" + PosingSystemLocalization.Text("対処:") + " " + PosingSystemLocalization.TextMessage(action);
        }

        public static DiagnosticException Failure(string code, string summary, string action, UnityEngine.Object context, Exception inner = null)
            => new DiagnosticException(code, summary, action, context, inner);

        public static List<Issue> Collect(PosingSystem ps)
        {
            var issues = new List<Issue>();
            void Add(string code, string summary, string action, MessageType severity = MessageType.Error, UnityEngine.Object context = null)
                => issues.Add(new Issue(code, summary, action, severity, context != null ? context : ps));
            if (ps == null) { Add("PS001", "操作対象のポーズツールが見つかりません。", "Hierarchyでアバター内のポーズツールを選択し直してください。"); return issues; }
            var avatar = ps.GetAvatar();
            if (avatar == null)
            {
                Add("PS002", "ツールの親にVRC Avatar Descriptorがありません。", "Prefab編集画面を閉じ、HierarchyでVRC Avatar Descriptorが付いたアバターの子にツールを配置してください。Project内のPrefab単体ではプレビルドできません。");
                return issues;
            }
            var animator = avatar.GetComponent<Animator>();
            if (animator == null)
                Add("PS003", "アバターのルートにAnimatorがありません。", "VRC Avatar Descriptorと同じオブジェクトのAnimatorを確認し、配布元のアバターPrefabの設定と比較してください。", context: avatar);
            else if (animator.avatar == null)
                Add("PS004", "AnimatorのAvatar欄がNoneまたはMissingです。", "アバターの元モデルに対応するHumanoid AvatarをAnimatorのAvatar欄に戻してください。別アバターのAvatarは指定しないでください。", context: animator);
            else if (!animator.avatar.isValid || !animator.isHuman)
                Add("PS005", "AnimatorのAvatarが有効なHumanoidではありません。", "元モデルのInspector > RigでHumanoid設定とConfigureの割り当てを確認してください。配布元が指定する設定を優先してください。", context: animator);
            else
            {
                if (animator.GetBoneTransform(HumanBodyBones.Head) == null)
                    Add("PS006", "頭のボーン（Head）を取得できません。姿勢の高さを計算できません。", "元モデルのRig > ConfigureでHeadを確認し、シーン内で頭のボーンが削除されていないか確認してください。", context: animator);
                if (animator.GetBoneTransform(HumanBodyBones.Chest) == null)
                    Add("PS007", "胸のボーン（Chest）が割り当てられていません。", "胸の姿勢が崩れる場合は元モデルのRig > Configure > BodyのChestを確認してください。Upper Chestとは別の項目です。対応するボーンがない場合は無理に割り当てず、配布元の設定を確認してください。", MessageType.Warning, animator);
            }

            var markers = CommonAnimatorMarkers(ps);
            var marker = markers.FirstOrDefault(ActiveCommonMarker);
            if (marker == null)
            {
                if (CanRecoverCommonChild(ps, markers, out var orphan))
                    Add("PS010", "共通ポージングAnimatorがありません。", "プレビルド・ビルド時に標準Prefabから再作成します。", MessageType.Warning);
                else
                {
                    var blocked = PosingSystemHierarchy.BlockingParent(markers.FirstOrDefault()?.transform ?? orphan?.transform ?? ps.transform, avatar.transform);
                    if (blocked != null)
                    {
                        var path = AnimationUtility.CalculateTransformPath(blocked, null);
                        var reason = blocked.CompareTag("EditorOnly") ? "にEditorOnlyタグが設定されています" : "が無効になっています";
                        Add("PS010", PosingSystemLocalization.Format("共通ポージングAnimatorを使用できません。「{0}」{1}。", path, PosingSystemLocalization.Text(reason)),
                            "このアバターでポーズを使用する場合は、「対象を選択」で該当オブジェクトのタグと有効状態を確認してください。", context: blocked.gameObject);
                    }
                    else Add("PS010", "有効な共通ポージングAnimatorが見つかりません。", "ツール内の共通ポージングAnimatorが1つだけ有効か、EditorOnlyタグ・Missing Scriptの有無と標準Prefabの導入状態を確認してください。");
                }
            }
            else
            {
                var merge = marker.GetComponent<ModularAvatarMergeAnimator>();
                if (merge == null)
                {
                    var recoverable = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(marker.gameObject) == 0 &&
                        CommonAnimatorTemplate()?.animator != null;
                    Add("PS011", "共通ポージングAnimatorにMA Merge Animatorがありません。",
                        recoverable ? "プレビルド・ビルド時に標準設定から再作成します。" : RestoreTool,
                        recoverable ? MessageType.Warning : MessageType.Error, marker);
                }
                else if (merge.animator == null)
                    Add("PS012", "共通ポージングAnimatorのAnimator参照が未設定です。", "プレビルド時に標準Controllerから復元を試みます。復元できない場合は、ゆにさきポーズシステムのパッケージを再取得してください。", MessageType.Warning, merge);
                else if (!(merge.animator is AnimatorController controller))
                    Add("PS013", "共通ポージングAnimatorに通常のAnimator Controller以外が設定されています。", "Animator Override Controllerはここでは使用できません。ツールの標準Controllerへ戻してください。" + RestoreTool, context: merge);
                else if (!controller.layers.Any(l => l.name == "USSPS_Locomotion" && l.stateMachine != null) ||
                         !controller.layers.Any(l => l.name == "USSPS_LocomotionType" && l.stateMachine != null))
                    Add("PS014", "共通Controllerに姿勢制御用のレイヤーがありません。", "共通ポージングAnimatorへ別のControllerを指定していないか確認してください。" + RestoreTool, context: merge);
            }
            if (ps.GetComponent<ModularAvatarParameters>() == null)
                Add("PS015", "ツールのルートにMA Parametersがありません。", RestoreTool);

            var hasMenuPoses = ps.defines != null && ps.defines.Any(d => d?.animations != null && d.animations.Any(a => a != null && a.enabled));
            if (hasMenuPoses && ps.SubmenuRoot == null && ps.GetActiveAvatarChildren<ModularAvatarMenuGroup>().Length == 0)
                Add("PS016", "姿勢メニューの生成先が見つかりません。", "ツール内の姿勢メニューが削除・無効化されていないか確認してください。" + RestoreTool);
            if (ps.SubmenuRoot != null && ps.SubmenuRoot != avatar.transform && !ps.SubmenuRoot.IsChildOf(avatar.transform))
                Add("PS017", "姿勢メニューの生成先が別のアバター、またはアバターの外にあります。", "Submenu Rootには、このアバター内のメニュー生成先を指定してください。", context: ps.SubmenuRoot);

            if (ps.defines == null)
                Add("PS020", "ポーズ一覧のデータがありません。", "プリセットを適用し直すか、新しく追加したツールのポーズ一覧と比較してください。" + RestoreTool);
            else
            {
                for (var i = 0; i < ps.defines.Count; i++)
                {
                    var layer = ps.defines[i];
                    if (layer?.animations == null) { Add("PS021", string.Format("ポーズ一覧のカテゴリ{0}のデータが欠けています。", i + 1), "ポーズ一覧で該当カテゴリを確認し、プリセットまたはバックアップから復元してください。"); continue; }
                    var category = string.IsNullOrEmpty(layer.menuName) ? PosingSystemLocalization.FormatFor(ps, "カテゴリ{0}", i + 1) : layer.menuName;
                    var enabled = layer.animations.Where(a => a != null && a.enabled).ToList();
                    if (enabled.Count == 0)
                        Add("PS022", string.Format("「{0}」に有効なポーズがありません。このカテゴリの選択メニューは生成されません。", category), "ポーズ一覧で使うポーズを有効にしてください。意図的に空にしている場合は対応不要です。", MessageType.Info);
                    if (enabled.Count(a => a.initial) > 1)
                        Add("PS023", string.Format("「{0}」で初期ポーズが複数選ばれています。", category), "ポーズ一覧で初期ポーズを1つにしてください。", MessageType.Warning);
                    for (var j = 0; j < layer.animations.Count; j++)
                    {
                        var pose = layer.animations[j];
                        if (pose == null) { Add("PS024", string.Format("「{0}」の{1}番目のポーズデータが欠けています。", category, j + 1), "該当する項目をポーズ一覧から削除し、必要なアニメーションを追加し直してください。"); continue; }
                        if (!pose.enabled) continue;
                        if (pose.animationClip == null)
                            Add("PS025", string.Format("「{0}」の{1}番目（{2}）にアニメーションがありません。", category, j + 1, pose.displayName), "ポーズ一覧でアニメーションを指定するか、その項目を無効にしてください。", MessageType.Warning);
                        else if (pose.animationClip is AnimationClip clip && clip.legacy)
                            Add("PS026", string.Format("「{0}」の{1}番目（{2}）はLegacy形式のアニメーションです。", category, j + 1, pose.displayName), "Mecanimで使用できるアニメーションを指定してください。元モデル由来ならRigの設定も確認してください。", MessageType.Error, clip);
                        if (pose.isRollover && pose.rolloverSideAnimation != null)
                        {
                            if (!(pose.animationClip is AnimationClip center) || !PosingSystemRolloverGenerator.Supports(center) ||
                                !PosingSystemRolloverGenerator.Supports(pose.rolloverSideAnimation))
                                Add("PS070", string.Format("「{0}」の{1}番目（{2}）の寝返りにはHumanoidのAnimationClipが必要です。", category, j + 1, pose.displayName), RolloverClipAction, MessageType.Error);
                            if (float.IsNaN(pose.rolloverContactStrength) || float.IsInfinity(pose.rolloverContactStrength) || pose.rolloverContactStrength < 0 || pose.rolloverContactStrength > 1)
                                Add("PS071", string.Format("「{0}」の{1}番目（{2}）の接地補正の強さが範囲外です。", category, j + 1, pose.displayName), "0〜1の値を指定してください。", MessageType.Error);
                        }
                        if (!pose.isRollover && pose.isMotionTime && string.IsNullOrWhiteSpace(pose.motionTimeParamName))
                            Add("PS027", string.Format("「{0}」の{1}番目（{2}）のMotion Time用パラメータ名が空です。", category, j + 1, pose.displayName), "Motion Timeのパラメータ名を設定するか、Motion Timeを無効にしてください。");
                        if (pose.isCustomIcon && pose.icon == null)
                            Add("PS028", string.Format("「{0}」の{1}番目（{2}）は独自アイコンが有効ですが画像がありません。", category, j + 1, pose.displayName), "アイコン画像を指定するか、独自アイコンを無効にして自動生成を使用してください。", MessageType.Warning);
                    }
                }
            }
#if NDMF && MODULAR_AVATAR
            var photographyHeadIssue = PosingSystemConverter.PhotographyHeadIssue(ps);
            if (photographyHeadIssue != null) issues.Add(photographyHeadIssue);
#endif
            return issues;
        }

        public static void ThrowIfInvalid(PosingSystem ps)
        {
            ThrowIfCompilationFailed(ps);
            var errors = Collect(ps).Where(i => i.Severity == MessageType.Error).ToList();
            if (errors.Count == 0) return;
            throw Failure("PS000", "設定に問題があるため、ポーズの生成を開始できません。\n" + string.Join("\n\n", errors.Select(i => i.Message)),
                "Inspectorの「設定の確認」で対象を選択し、表示された項目を直して再実行してください。", ps);
        }
        internal static void ThrowIfCompilationFailed(UnityEngine.Object context)
        {
            if (EditorUtility.scriptCompilationFailed) throw CompilationFailure(context);
        }

        internal static DiagnosticException CompilationFailure(UnityEngine.Object context) =>
            Failure("PS050", PosingSystemLocalization.Text("Unityでスクリプトのコンパイルに失敗したため、ポーズの生成を開始できません。") + PosingSystemSupportReport.CompilerDetails(),
                "Consoleの最初のコンパイルエラーを修正し、再コンパイルが完了してから実行してください。", context);
    }
}
