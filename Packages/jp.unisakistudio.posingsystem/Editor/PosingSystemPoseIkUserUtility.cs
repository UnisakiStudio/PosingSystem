using System;
using System.Linq;
using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace jp.unisakistudio.posingsystemeditor
{
    public static class PosingSystemPoseIkUserUtility
    {
        public static void SetEnabled(PosingSystem ps, PosingSystem.BaseAnimationDefine pose, bool enabled)
        {
            if (ps == null || pose == null || pose.disablePoseIk == !enabled) return;
            Undo.RecordObject(ps, "このポーズのIKを切り替え");
            pose.disablePoseIk = !enabled;
            pose.previewImage = null;
            PosingSystemEditorChangeUtility.MarkChanged(ps);
        }

        // Save a new asset, leaving shared catalog data and any previous adjustment intact.
        public static bool TryBake(PosingSystem ps, PosingSystem.AnimationDefine pose, string key,
            string requestedPath, out string message)
        {
            message = "IKを反映するポーズとアバターを確認してください。";
            if (ps == null || pose?.animationClip is not AnimationClip source || pose.disablePoseIk) return false;
            var definition = PosingSystemAutoPoseAdjustmentUtility.FindDefinition(ps, key, source);
            if (definition?.limbRules?.Any(r => r != null && r.enabled) != true) return false;
            if (!requestedPath.StartsWith("Assets/", StringComparison.Ordinal) || !requestedPath.EndsWith(".anim", StringComparison.OrdinalIgnoreCase) ||
                !AssetDatabase.IsValidFolder(System.IO.Path.GetDirectoryName(requestedPath)?.Replace('\\', '/')))
            { message = "保存先にはAssets内のフォルダと.animファイルを指定してください。"; return false; }
            var avatar = ps.GetAvatar();
            if (avatar == null) return false;
            GameObject clone = null, parking = null;
            AnimationClip clip = null;
            try
            {
                parking = new GameObject("_USSPS_IkBake") { hideFlags = HideFlags.HideAndDontSave };
                parking.SetActive(false);
                clone = Object.Instantiate(avatar.gameObject, parking.transform);
                clone.hideFlags = HideFlags.HideAndDontSave;
                clone.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                var animator = clone.GetComponent<Animator>();
                foreach (var behaviour in clone.GetComponentsInChildren<Behaviour>(true))
                    if (behaviour != null && behaviour != animator) behaviour.enabled = false;
                if (animator == null || animator.avatar == null || !animator.isHuman) return false;
                animator.enabled = true;
                animator.runtimeAnimatorController = null;
                animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                clone.SetActive(true); parking.SetActive(true);
                if (!PosingSystemAutoPoseAdjustmentUtility.TryCreateAdjustmentClip(animator, source, definition,
                    out clip, out _, out message, PosingSystemAvatarAdjustmentUtility.GetKneeSafety(ps))) return false;
                var path = AssetDatabase.GenerateUniqueAssetPath(requestedPath);
                clip.hideFlags = HideFlags.None;
                clip.name = System.IO.Path.GetFileNameWithoutExtension(path);
                AssetDatabase.CreateAsset(clip, path);
                AssetDatabase.SaveAssetIfDirty(clip);
                Undo.RecordObject(ps, "IKを調整に反映");
                pose.adjustmentClip = clip;
                pose.disablePoseIk = true;
                pose.previewImage = null;
                PosingSystemEditorChangeUtility.MarkChanged(ps);
                message = path;
                return true;
            }
            catch (Exception e) { message = "IKの調整を保存できませんでした。\n" + e.Message; return false; }
            finally
            {
                if (clip != null && !EditorUtility.IsPersistent(clip)) Object.DestroyImmediate(clip);
                if (parking != null) Object.DestroyImmediate(parking);
                else if (clone != null) Object.DestroyImmediate(clone);
            }
        }
    }
}
