using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor
{
    public partial class PosingAnimationAdjustmentWindow
    {
        // Optional Editor extensions. Subscription and lifetime belong to the host project.
        public static event Action<PosingAnimationAdjustmentWindow> AdditionalControlsGUI;
        public static event Action<PosingAnimationAdjustmentWindow> AdditionalAnimationControlsGUI;
        public static event Action<PosingAnimationAdjustmentWindow, SceneView> AdditionalSceneGUI;
        public static event Func<PosingAnimationAdjustmentWindow, bool> AdditionalIkPreviewRequested;
        public static event Action<PosingAnimationAdjustmentWindow, string, AnimationClip, GUIContent> AdditionalAnimationListItem;
        private bool _scrollToSelectedAnimation;

        // Generic navigation for project extensions; no authoring-status policy belongs in the package.
        public bool SelectNextAnimation(Func<string, AnimationClip, bool> matches)
        {
            if (matches == null || _animationEntries.Count == 0) return false;
            for (var offset = 1; offset <= _animationEntries.Count; offset++)
            {
                var index = (_selectedAnimationIndex + offset) % _animationEntries.Count;
                if (index == _selectedAnimationIndex) continue;
                var entry = _animationEntries[index];
                if (entry.Animation.animationClip is not AnimationClip clip || !matches(GetAnimationKey(entry), clip)) continue;
                if (_adjustments != null) SaveCurrentAdjustmentData();
                _selectedAnimationIndex = index;
                _scrollToSelectedAnimation = true;
                _muscleScroll = Vector2.zero;
                LoadAdjustmentValues();
                return true;
            }
            return false;
        }

        private void ScrollToSelectedAnimation(Rect listRect)
        {
            if (!_scrollToSelectedAnimation || Event.current.type != EventType.Repaint || _animationEntries.Count == 0) return;
            _animationListScroll.y = Mathf.Max(0, (_selectedAnimationIndex - 1) * listRect.height / _animationEntries.Count);
            _scrollToSelectedAnimation = false;
            Repaint();
        }

        public PosingSystem PosingSystem => _posingSystem;
        public AnimationClip SelectedAnimationClip => GetSelectedEntry()?.Animation?.animationClip as AnimationClip;
        public string SelectedPoseKey => GetAnimationKey(GetSelectedEntry());
        public Animator PreviewAnimator => _previewAvatar;

        public void RefreshPreview()
        {
            if (!IsOpen) return;
            ApplyPreviewPose();
            Repaint();
            SceneView.RepaintAll();
        }

        private void RegisterAutoPoseSceneGui()
        {
            SceneView.duringSceneGui -= DrawAdditionalSceneGUI;
            SceneView.duringSceneGui += DrawAdditionalSceneGUI;
        }

        private void UnregisterAutoPoseSceneGui() => SceneView.duringSceneGui -= DrawAdditionalSceneGUI;

        private void DrawAdditionalSceneGUI(SceneView view)
        {
            if (IsOpen) AdditionalSceneGUI?.Invoke(this, view);
        }

        private void DrawAutoPoseCatalogPanel()
        {
            if (_posingSystem == null) return;
            if (_posingSystem.autoPoseCatalog != null && _posingSystem.autoPoseCatalog.productionReady)
            {
                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("体格に合わせた手足の自動調整", EditorStyles.boldLabel);
                    EditorGUILayout.LabelField("腕や脚の長さに合わせて、手足が自然な置き場所へ届くよう調整します。", EditorStyles.wordWrappedLabel);
                    EditorGUI.BeginChangeCheck();
                    var enabled = EditorGUILayout.ToggleLeft("手足の自動調整を使う", _posingSystem.enablePoseIk);
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RecordObject(_posingSystem, "Toggle automatic pose adjustment");
                        _posingSystem.enablePoseIk = enabled;
                        PosingSystemEditorChangeUtility.MarkChanged(_posingSystem);
                        ApplyPreviewPose();
                    }
                    EditorGUILayout.LabelField(enabled ? "調整結果をプレビューしています。" : "元の姿勢と手動調整をプレビューしています。", EditorStyles.wordWrappedMiniLabel);
                }
            }
            AdditionalControlsGUI?.Invoke(this);
        }

        private void DrawAutoPoseAuthoringPanel(AnimationEntry entry) => AdditionalAnimationControlsGUI?.Invoke(this);

        private bool UsesIkInPreview() => _posingSystem != null && GetSelectedEntry()?.Animation?.disablePoseIk == false &&
            (_posingSystem.UsesAutoPoseFor(GetSelectedEntry()?.Animation) || (AdditionalIkPreviewRequested != null &&
                AdditionalIkPreviewRequested.GetInvocationList()
                    .Cast<Func<PosingAnimationAdjustmentWindow, bool>>().Any(request => request(this))));

        private void DrawUserPoseIkPanel(AnimationEntry entry)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("このポーズの手足の自動調整（IK）", EditorStyles.boldLabel);
                var definition = PosingSystemAutoPoseAdjustmentUtility.FindDefinition(_posingSystem,
                    GetAnimationKey(entry), entry.Animation.animationClip as AnimationClip);
                var hasRules = definition?.limbRules?.Any(r => r != null && r.enabled) == true;
                if (!hasRules)
                {
                    EditorGUILayout.LabelField("このポーズにはIKによる自動調整が設定されていません。元の姿勢と手動調整を使用します。", EditorStyles.wordWrappedLabel);
                    return;
                }
                EditorGUILayout.LabelField("アバターに合わないときは、このポーズだけIKをオフにできます。ほかのポーズやアバターには影響しません。", EditorStyles.wordWrappedLabel);
                var enabled = !entry.Animation.disablePoseIk;
                var next = EditorGUILayout.ToggleLeft("このポーズのIKを使う", enabled);
                if (next != enabled)
                {
                    PosingSystemPoseIkUserUtility.SetEnabled(_posingSystem, entry.Animation, next);
                    ApplyPreviewPose();
                }
                if (next && !UsesIkInPreview())
                    EditorGUILayout.HelpBox("このアバターの手足の自動調整が無効になっています。", MessageType.Info);
                using (new EditorGUI.DisabledScope(!UsesIkInPreview()))
                    if (GUILayout.Button("IKを調整に反映", GUILayout.Height(26)))
                    {
                        if (HasAnyChanges())
                        {
                            if (!EditorUtility.DisplayDialog("未保存の手動調整があります",
                                "手動調整を保存してから、現在のIKの結果を別の調整アニメーションに保存します。既存のファイルは残ります。", "保存して続ける", "キャンセル")) return;
                            SaveAdjustmentClip();
                        }
                        EnsureAdjustmentFolderExists();
                        var path = GetAdjustmentFolderPath() + "/" + SanitizeFileName(_posingSystem.GetAvatar().name + "_" + entry.Animation.displayName) + "_IK.anim";
                        if (!PosingSystemPoseIkUserUtility.TryBake(_posingSystem, entry.Animation, GetAnimationKey(entry), path, out var message))
                            EditorUtility.DisplayDialog("IKを反映できませんでした", message, "閉じる");
                        else
                        {
                            ReloadCurrentAdjustmentFromFile();
                            UpdateOriginalDataAfterClipChange();
                            ApplyPreviewPose();
                            Repaint();
                        }
                    }
                EditorGUILayout.LabelField("IKの結果を新しい調整アニメーションに保存して、このポーズのIKをオフにします。その後は下の項目で手動調整できます。", EditorStyles.wordWrappedMiniLabel);
            }
        }

        private bool TryApplySelectedAutoPosePreview()
        {
            if (_previewAvatar == null || !UsesIkInPreview()) return false;
            var definition = PosingSystemAutoPoseAdjustmentUtility.FindDefinition(
                _posingSystem, SelectedPoseKey, SelectedAnimationClip);
            if (definition != null || _posingSystem.UsesAutoPoseCatalog)
                PosingSystemAutoPoseAdjustmentUtility.TryApplyDefinition(_previewAvatar, definition,
                    kneeSafety: PosingSystemAvatarAdjustmentUtility.GetKneeSafety(_posingSystem));

            // Production catalogs suppress legacy adjustment clips even for a missing definition.
            return definition != null || _posingSystem.UsesAutoPoseCatalog;
        }
    }
}
