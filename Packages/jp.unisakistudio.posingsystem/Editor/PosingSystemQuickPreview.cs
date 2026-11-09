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
    // The source stays active and unmodified. Only its Scene view visibility is borrowed.
    internal sealed class SceneAvatarPreview : IDisposable
    {
        private GameObject root;
        private GameObject[] originallyVisible;
        private AnimationModeDriver driver;
        public Animator Animator { get; private set; }
        public bool IsActive => Animator != null && driver != null && AnimationMode.InAnimationMode(driver);

        public void Start(Animator source, Animator model, string name)
        {
            root = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave, tag = "EditorOnly" };
            root.SetActive(false);
            SceneManager.MoveGameObjectToScene(root, source.gameObject.scene);
            var clone = Object.Instantiate(model.gameObject, root.transform);
            foreach (var child in clone.GetComponentsInChildren<Transform>(true))
                child.gameObject.hideFlags = HideFlags.HideAndDontSave;
            Animator = clone.GetComponent<Animator>();
            foreach (var behaviour in clone.GetComponentsInChildren<Behaviour>(true))
                if (behaviour != null && behaviour != Animator) behaviour.enabled = false;
            Animator.enabled = true;
            Animator.runtimeAnimatorController = null;
            Animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            clone.transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);
            clone.transform.localScale = source.transform.lossyScale;
            clone.SetActive(true);
            root.SetActive(true);

            originallyVisible = source.GetComponentsInChildren<Transform>(true)
                .Select(t => t.gameObject).Where(go => !SceneVisibilityManager.instance.IsHidden(go)).ToArray();
            SceneVisibilityManager.instance.Hide(source.gameObject, true);
            driver = ScriptableObject.CreateInstance<AnimationModeDriver>();
            driver.hideFlags = HideFlags.HideAndDontSave;
            AnimationMode.StartAnimationMode(driver);
        }

        public void Dispose()
        {
            try
            {
                if (driver != null && AnimationMode.InAnimationMode(driver))
                    AnimationMode.StopAnimationMode(driver);
            }
            finally
            {
                if (originallyVisible != null)
                    SceneVisibilityManager.instance.Show(originallyVisible.Where(go => go != null).ToArray(), false);
                originallyVisible = null;
                if (root != null) Object.DestroyImmediate(root);
                if (driver != null) Object.DestroyImmediate(driver);
                Animator = null;
            }
        }
    }

    [InitializeOnLoad]
    public static class PosingSystemQuickPreview
    {
        private static PosingSystem owner;
        private static PosingSystem.BaseAnimationDefine pose;
        private static SceneAvatarPreview preview;
        internal static Animator PreviewAnimator => preview?.Animator;
        private static string label;
        public static bool IsActive => owner != null && preview != null && preview.IsActive;
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
            AnimationClip combined = null;
            try
            {
                preview = new SceneAvatarPreview();
                preview.Start(animator, animator, "_USSPS_QuickPreview");
                animator = preview.Animator;
                var position = animator.transform.position;
                var rotation = animator.transform.rotation;
                var scale = animator.transform.localScale;
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
            preview?.Dispose();
            preview = null;
            owner = null;
            pose = null;
            SceneView.RepaintAll();
        }

        private static void CheckLifetime()
        {
            if (preview != null && (owner == null || !owner.gameObject.scene.IsValid() || !IsActive || !owner.gameObject.activeInHierarchy)) Stop();
        }

        private static void DrawSceneControls(SceneView view)
        {
            if (!IsActive) return;
            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Escape)
            { Stop(); Event.current.Use(); return; }
            Handles.BeginGUI();
            GUILayout.BeginArea(new Rect(12, 12, 300, 65), EditorStyles.helpBox);
            GUILayout.Label("プレビュー中：" + label, EditorStyles.miniBoldLabel);
            if (GUILayout.Button("プレビューを終了（元の姿勢に戻す）")) Stop();
            GUILayout.EndArea();
            Handles.EndGUI();
        }
    }
}
