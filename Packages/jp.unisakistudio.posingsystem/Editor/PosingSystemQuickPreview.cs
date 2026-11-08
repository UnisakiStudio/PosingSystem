using System;
using System.Linq;
using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    [InitializeOnLoad]
    public static class PosingSystemQuickPreview
    {
        private static PosingSystem owner;
        private static PosingSystem.BaseAnimationDefine pose;
        private static GameObject previewRoot;
        private static GameObject[] originallyVisible;
        internal static Animator PreviewAnimator { get; private set; }
        private static AnimationModeDriver driver;
        private static string label;
        public static bool IsActive => owner != null && driver != null && AnimationMode.InAnimationMode(driver);
        public static bool IsPreviewing(PosingSystem ps, PosingSystem.BaseAnimationDefine entry) =>
            IsActive && owner == ps && ReferenceEquals(pose, entry);

        static PosingSystemQuickPreview()
        {
            AssemblyReloadEvents.beforeAssemblyReload += Stop;
            EditorApplication.quitting += Stop;
            EditorApplication.playModeStateChanged += _ => Stop();
            Selection.selectionChanged += Stop;
            Undo.undoRedoPerformed += Stop;
            EditorSceneManager.sceneSaving += (_, _) => Stop();
            EditorSceneManager.sceneClosing += (_, _) => Stop();
            SceneView.duringSceneGui += DrawSceneControls;
            EditorApplication.update += CheckLifetime;
        }

        public static bool Start(PosingSystem ps, PosingSystem.BaseAnimationDefine entry, string key,
            out string message, bool showScene = true)
        {
            Stop();
            message = "HumanoidのアバターとAnimationClipを指定してください。";
            var animator = ps?.GetAvatar()?.GetComponent<Animator>();
            if (Application.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode || ps == null ||
                animator == null || animator.avatar == null || !animator.isHuman ||
                entry?.animationClip is not AnimationClip clip || clip.legacy) return false;
            if (AnimationMode.InAnimationMode())
            { message = "アニメーション調整画面など、ほかのプレビューを終了してからお試しください。"; return false; }
            owner = ps; pose = entry; label = key;
            driver = ScriptableObject.CreateInstance<AnimationModeDriver>();
            driver.hideFlags = HideFlags.HideAndDontSave;
            AnimationClip combined = null;
            try
            {
                // Keep transient poses out of scene saves, prefab overrides and avatar builds.
                previewRoot = new GameObject("_USSPS_QuickPreview") { hideFlags = HideFlags.HideAndDontSave, tag = "EditorOnly" };
                previewRoot.SetActive(false);
                SceneManager.MoveGameObjectToScene(previewRoot, animator.gameObject.scene);
                var clone = Object.Instantiate(animator.gameObject, previewRoot.transform);
                foreach (var child in clone.GetComponentsInChildren<Transform>(true))
                    child.gameObject.hideFlags = HideFlags.HideAndDontSave;
                PreviewAnimator = clone.GetComponent<Animator>();
                foreach (var behaviour in clone.GetComponentsInChildren<Behaviour>(true))
                    if (behaviour != null && behaviour != PreviewAnimator) behaviour.enabled = false;
                PreviewAnimator.enabled = true;
                PreviewAnimator.runtimeAnimatorController = null;
                PreviewAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                clone.transform.SetPositionAndRotation(animator.transform.position, animator.transform.rotation);
                clone.transform.localScale = animator.transform.lossyScale;
                var position = clone.transform.position;
                var rotation = clone.transform.rotation;
                var scale = clone.transform.localScale;
                clone.SetActive(true); previewRoot.SetActive(true);
                // Hide every mesh under the source, not just the root GameObject (which
                // usually has no renderer). Preserve individually hidden clothing as well.
                originallyVisible = animator.GetComponentsInChildren<Transform>(true)
                    .Select(t => t.gameObject).Where(go => !SceneVisibilityManager.instance.IsHidden(go)).ToArray();
                SceneVisibilityManager.instance.Hide(animator.gameObject, true);
                animator = PreviewAnimator;
                AnimationMode.StartAnimationMode(driver);
                var sample = clip;
                if (!ps.UsesAutoPoseFor(entry) && entry.adjustmentClip != null)
                {
                    combined = Object.Instantiate(clip); combined.hideFlags = HideFlags.HideAndDontSave;
                    AdditiveCurveUtility.MultiplyRootQCurves(clip, entry.adjustmentClip, combined);
                    foreach (var binding in AnimationUtility.GetCurveBindings(entry.adjustmentClip).Where(b => !AdditiveCurveUtility.IsRootQBinding(b)))
                        AnimationUtility.SetEditorCurve(combined, binding, AdditiveCurveUtility.AddCurve(
                            AnimationUtility.GetEditorCurve(clip, binding) ?? new AnimationCurve(),
                            AnimationUtility.GetEditorCurve(entry.adjustmentClip, binding)));
                    sample = combined;
                }
                AnimationMode.BeginSampling();
                try { AnimationMode.SampleAnimationClip(animator.gameObject, sample, 0); }
                finally { AnimationMode.EndSampling(); }
                // Keep each scene avatar at its own placement, even when a clip has root curves.
                animator.transform.SetPositionAndRotation(position, rotation);
                animator.transform.localScale = scale;
                if (entry.isRotate) animator.transform.rotation *= Quaternion.Euler(0, entry.rotate, 0);
                if (ps.UsesAutoPoseFor(entry))
                    PosingSystemAutoPoseAdjustmentUtility.TryApplyDefinition(animator,
                        PosingSystemAutoPoseAdjustmentUtility.FindDefinition(ps, key, clip),
                        kneeSafety: PosingSystemAvatarAdjustmentUtility.GetKneeSafety(ps));
                else PosingSystemAvatarAdjustmentUtility.ApplyToAnimator(ps, animator);
                if (showScene)
                {
                    var view = SceneView.lastActiveSceneView ?? EditorWindow.GetWindow<SceneView>();
                    view.Focus();
                    var bounds = new Bounds(animator.transform.position + Vector3.up, Vector3.one * 2);
                    var renderers = animator.GetComponentsInChildren<Renderer>().Where(r => r.enabled).ToArray();
                    if (renderers.Length > 0) { bounds = renderers[0].bounds; foreach (var renderer in renderers.Skip(1)) bounds.Encapsulate(renderer.bounds); }
                    view.Frame(bounds, false);
                }
                SceneView.RepaintAll();
                message = null;
                return true;
            }
            catch (Exception e) { message = "プレビューできませんでした。\n" + e.Message; Debug.LogException(e, ps); Stop(); return false; }
            finally { if (combined != null) Object.DestroyImmediate(combined); }
        }

        public static void Stop()
        {
            try { if (driver != null && AnimationMode.InAnimationMode(driver)) AnimationMode.StopAnimationMode(driver); }
            finally
            {
                if (originallyVisible != null)
                    SceneVisibilityManager.instance.Show(originallyVisible.Where(go => go != null).ToArray(), false);
                originallyVisible = null;
                if (previewRoot != null) Object.DestroyImmediate(previewRoot);
                previewRoot = null; PreviewAnimator = null; owner = null; pose = null;
                if (driver != null) Object.DestroyImmediate(driver);
                driver = null;
            }
            SceneView.RepaintAll();
        }

        private static void CheckLifetime()
        {
            if (driver != null && (owner == null || !owner.gameObject.scene.IsValid() || !IsActive || !owner.gameObject.activeInHierarchy)) Stop();
        }

        private static void DrawSceneControls(SceneView view)
        {
            if (!IsActive) return;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            { Stop(); Event.current.Use(); return; }
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(12, 12, 300, 65), EditorStyles.helpBox);
            GUILayout.Label(PosingSystemLocalization.Format("プレビュー中：{0}", label), EditorStyles.miniBoldLabel);
            if (GUILayout.Button(PosingSystemLocalization.Text("プレビューを終了（元の姿勢に戻す）"))) Stop();
            GUILayout.EndArea();
            Handles.EndGUI();
        }
    }
}
