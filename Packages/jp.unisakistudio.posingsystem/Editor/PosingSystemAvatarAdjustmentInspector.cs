using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor
{
    public static class PosingSystemAvatarAdjustmentInspector
    {
        private static readonly HashSet<int> PendingGeneration = new HashSet<int>();

        public static void ScheduleAllInOpenScenes()
        {
            foreach (var posingSystem in Resources.FindObjectsOfTypeAll<PosingSystem>())
            {
                if (posingSystem != null && posingSystem.gameObject.scene.IsValid())
                {
                    EnsureUsageDefault(posingSystem);
                    ScheduleAutomaticGeneration(posingSystem);
                }
            }
        }

        public static void ScheduleAutomaticGeneration(PosingSystem posingSystem)
        {
            EnsureUsageDefault(posingSystem);
            if (posingSystem == null || Application.isPlaying ||
                !posingSystem.enableAvatarSpecificAdjustment ||
                !PosingSystemAvatarAdjustmentUtility.HasReferenceProfile(posingSystem))
            {
                return;
            }

            var instanceId = posingSystem.GetInstanceID();
            if (!PendingGeneration.Add(instanceId))
            {
                return;
            }

            EditorApplication.delayCall += () =>
            {
                PendingGeneration.Remove(instanceId);
                if (posingSystem == null || Application.isPlaying ||
                    !posingSystem.enableAvatarSpecificAdjustment ||
                    PosingSystemAvatarAdjustmentUtility.IsGeneratedClipCurrent(posingSystem))
                {
                    return;
                }
                if (!PosingSystemAvatarAdjustmentUtility.TryGenerate(posingSystem, out var message))
                {
                    Debug.LogWarning($"[PosingSystem] 自動補正を生成できませんでした: {message}", posingSystem);
                }
            };
        }

        public static bool EnsureUsageDefault(PosingSystem posingSystem)
        {
            if (posingSystem == null || posingSystem.avatarSpecificAdjustmentUsageInitialized)
            {
                return false;
            }

            // このフラグが存在しない旧バージョン導入済みオブジェクトは、既存の手動調整を尊重する。
            posingSystem.enableAvatarSpecificAdjustment = false;
            posingSystem.avatarSpecificAdjustmentUsageInitialized = true;
            EditorUtility.SetDirty(posingSystem);
            return true;
        }

        public static void ConfigureNewInstallation(PosingSystem posingSystem)
        {
            if (posingSystem == null)
            {
                return;
            }

            posingSystem.enableAvatarSpecificAdjustment = true;
            posingSystem.avatarSpecificAdjustmentUsageInitialized = true;
            EditorUtility.SetDirty(posingSystem);
            ScheduleAutomaticGeneration(posingSystem);
        }

        public static void ConfigurePresetApplied(PosingSystem posingSystem)
        {
            if (posingSystem == null)
            {
                return;
            }

            posingSystem.enableAvatarSpecificAdjustment = false;
            posingSystem.avatarSpecificAdjustmentUsageInitialized = true;
            EditorUtility.SetDirty(posingSystem);
        }

        public static void Draw(PosingSystem posingSystem)
        {
            if (!PosingSystemAvatarAdjustmentUtility.HasReferenceProfile(posingSystem))
            {
                return;
            }

            EnsureUsageDefault(posingSystem);

            EditorGUI.BeginChangeCheck();

            EditorGUILayout.BeginHorizontal();
            var enabled = EditorGUILayout.ToggleLeft(
                new GUIContent("アバター固有補正を使用", "基準アバターとの初期関節角度・ひねりの差を補正します。"),
                posingSystem.enableAvatarSpecificAdjustment,
                GUILayout.Width(190f));
            if (enabled)
            {
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField(
                        new GUIContent("自動生成された補正"),
                        posingSystem.avatarSpecificAdjustmentClip,
                        typeof(AnimationClip),
                        false);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            var kneeClamp = EditorGUILayout.ToggleLeft(
                new GUIContent("逆関節防止機能", "足IKで膝が逆向きに折れることを防止します。"),
                posingSystem.enableReverseKneeSafetyClamp,
                GUILayout.Width(190f));
            var safetyAngle = posingSystem.reverseKneeSafetyAngle;
            if (kneeClamp)
            {
                safetyAngle = EditorGUILayout.Slider(
                    new GUIContent("最低膝曲げ角度", "膝がこの角度より伸びる前にクランプします。"),
                    safetyAngle, 0f, 5f);
            }
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(posingSystem, "Change avatar-specific pose adjustment settings");
                posingSystem.enableAvatarSpecificAdjustment = enabled;
                posingSystem.avatarSpecificAdjustmentUsageInitialized = true;
                posingSystem.enableReverseKneeSafetyClamp = kneeClamp;
                posingSystem.reverseKneeSafetyAngle = safetyAngle;
                EditorUtility.SetDirty(posingSystem);
                if (enabled)
                {
                    ScheduleAutomaticGeneration(posingSystem);
                }
            }

            if (enabled && posingSystem.avatarSpecificAdjustmentClip != null &&
                !PosingSystemAvatarAdjustmentUtility.IsGeneratedClipCurrent(posingSystem))
            {
                EditorGUILayout.HelpBox(
                    "アバターの初期姿勢が変わったため、補正を自動更新します。",
                    MessageType.Warning);
                ScheduleAutomaticGeneration(posingSystem);
            }
        }
    }
}
