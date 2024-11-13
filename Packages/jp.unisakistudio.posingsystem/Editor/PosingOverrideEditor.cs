using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Linq;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor
{

    [CustomEditor(typeof(PosingOverride))]
    public class PosingOverrideEditor : Editor
    {
        string autoDetectMessage = null;
        public override void OnInspectorGUI()
        {
            PosingOverride posingOverride = target as PosingOverride;

            if (posingOverride.deleteExistingLayer != EditorGUILayout.Toggle("アバターの元移動レイヤーを削除する", posingOverride.deleteExistingLayer))
            {
                posingOverride.deleteExistingLayer = !posingOverride.deleteExistingLayer;
                EditorUtility.SetDirty(posingOverride);
                AssetDatabase.SaveAssets();
            }
            if (posingOverride.deleteExistingTrackingControl != EditorGUILayout.Toggle("アバターの元トラッキング制御を無効にする", posingOverride.deleteExistingTrackingControl))
            {
                posingOverride.deleteExistingTrackingControl = !posingOverride.deleteExistingTrackingControl;
                EditorUtility.SetDirty(posingOverride);
                AssetDatabase.SaveAssets();
            }
            if (posingOverride.ビルド時自動実行 != EditorGUILayout.Toggle("ビルド時自動実行", posingOverride.ビルド時自動実行)) {
                posingOverride.ビルド時自動実行 = !posingOverride.ビルド時自動実行;
                EditorUtility.SetDirty(posingOverride);
                AssetDatabase.SaveAssets();
            }
            if (posingOverride.ビルド時自動実行)
            {
                EditorGUILayout.HelpBox("このコンポーネントがあるためアバターに既に設定されている独自のアニメーションはビルド時に自動で継承されます。", MessageType.Info);
                return;
            }

            Transform avatarTransform = posingOverride.transform;
            VRC.SDK3.Avatars.Components.VRCAvatarDescriptor avatar = null;
            while (avatarTransform != null)
            {
                if (avatarTransform.TryGetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>(out avatar))
                {
                    break;
                }
                avatarTransform = avatarTransform.parent;
            }

            if (avatar != null)
            {
                if (autoDetectMessage == null)
                {
                    autoDetectMessage = CheckAutoOverrideDetect(avatar);
                }
                if (autoDetectMessage != null && autoDetectMessage.Length > 0)
                {
                    EditorGUILayout.HelpBox("アバターから自動で自動置き換え設定をインポートできます\n" + autoDetectMessage, MessageType.Info);
                    if (GUILayout.Button("インポートする"))
                    {
                        DetecAutoOverride(posingOverride, avatar);
                        EditorUtility.SetDirty(posingOverride);
                        AssetDatabase.SaveAssets();
                        autoDetectMessage = null;
                    }
                }
            }

            base.OnInspectorGUI();
        }

        public static List<(PosingOverride.OverrideDefine.AnimationStateType type, string stateName, string defaultMotionName)> detectSettings = new List<(PosingOverride.OverrideDefine.AnimationStateType type, string stateName, string defaultMotionName)>()
        {
            (PosingOverride.OverrideDefine.AnimationStateType.StandWalkRun, "stand", "vrc_StandingLocomotion"),
            (PosingOverride.OverrideDefine.AnimationStateType.Crouch, "crouch", "vrc_CrouchingLocomotion"),
            (PosingOverride.OverrideDefine.AnimationStateType.Prone, "prone", "vrc_ProneLocomotion"),
            (PosingOverride.OverrideDefine.AnimationStateType.Jump, "jump", "proxy_fall_short"),
            (PosingOverride.OverrideDefine.AnimationStateType.ShortFall, "shortfall", "proxy_fall_short"),
            (PosingOverride.OverrideDefine.AnimationStateType.ShortFall, "softfall", "proxy_fall_short"),
            (PosingOverride.OverrideDefine.AnimationStateType.ShortFall, "quickfall", "proxy_fall_short"),
            (PosingOverride.OverrideDefine.AnimationStateType.ShortLanding, "shortland", "proxy_land_quick"),
            (PosingOverride.OverrideDefine.AnimationStateType.ShortLanding, "softland", "proxy_land_quick"),
            (PosingOverride.OverrideDefine.AnimationStateType.ShortLanding, "quickland", "proxy_land_quick"),
            (PosingOverride.OverrideDefine.AnimationStateType.LongFall, "longfall", "proxy_fall_long"),
            (PosingOverride.OverrideDefine.AnimationStateType.LongFall, "hardfall", "proxy_fall_long"),
            (PosingOverride.OverrideDefine.AnimationStateType.LongLanding, "longland", "proxy_landing"),
            (PosingOverride.OverrideDefine.AnimationStateType.LongLanding, "hardland", "proxy_landing"),
        };

        static string CheckAutoOverrideDetect(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor avatar)
        {
            var posingOverrides = avatar.GetComponentsInChildren<PosingOverride>().ToList();
            string returnString = "";

            var animatorController = avatar.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base];
            if (animatorController.animatorController == null)
            {
                return null;
            }

            foreach (var layer in ((AnimatorController)animatorController.animatorController).layers)
            {
                void searchOverrideFromStateMachine(AnimatorStateMachine stateMachine)
                {
                    foreach (var subStateMachine in stateMachine.stateMachines)
                    {
                        searchOverrideFromStateMachine(subStateMachine.stateMachine);
                    }
                    foreach (var state in stateMachine.states)
                    {
                        var searchName = state.state.name;
                        searchName = searchName.Replace("ing", "").Replace(" ", "").Replace("_", "").ToLower();
                        var detectSettingIndex = detectSettings.FindIndex(setting => setting.stateName == searchName);
                        if (detectSettingIndex == -1)
                        {
                            continue;
                        }
                        var detectSetting = detectSettings[detectSettingIndex];
                        if (posingOverrides.FindIndex(posingOverride => posingOverride.defines.FindIndex(define => define.type == detectSetting.type) != -1) != -1)
                        {
                            continue;
                        }

                        if (state.state.motion == null)
                        {
                            continue;
                        }
                        if (state.state.motion.name == detectSetting.defaultMotionName)
                        {
                            continue;
                        }

                        returnString += string.Format("\nレイヤー「{0}」のステート「{1}({2})」は自動置き換え設定がインポート可能です", layer.name, state.state.name, state.state.motion.name);
                    }
                }

                searchOverrideFromStateMachine(layer.stateMachine);
            }

            return returnString;
        }

        public static void DetecAutoOverride(PosingOverride posingOverride, VRC.SDK3.Avatars.Components.VRCAvatarDescriptor avatar)
        {
            var posingOverrides = avatar.GetComponentsInChildren<PosingOverride>().ToList();
            var animatorController = avatar.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base];
            if (animatorController.animatorController == null)
            {
                return;
            }

            void addOverrideFromStateMachine(AnimatorStateMachine stateMachine)
            {
                foreach (var subStateMachine in stateMachine.stateMachines)
                {
                    addOverrideFromStateMachine(subStateMachine.stateMachine);
                }
                foreach (var state in stateMachine.states)
                {
                    var searchName = state.state.name;
                    searchName = searchName.Replace("ing", "").Replace(" ", "").Replace("_", "").ToLower();
                    var detectSettingIndex = detectSettings.FindIndex(setting => setting.stateName == searchName);
                    if (detectSettingIndex == -1)
                    {
                        continue;
                    }
                    var detectSetting = detectSettings[detectSettingIndex];
                    if (posingOverrides.FindIndex(ov => ov.defines.FindIndex(def => def.type == detectSetting.type) != -1) != -1)
                    {
                        continue;
                    }

                    if (state.state.motion == null)
                    {
                        continue;
                    }
                    if (state.state.motion.name == detectSetting.defaultMotionName)
                    {
                        continue;
                    }

                    var define = new PosingOverride.OverrideDefine();
                    define.type = detectSetting.type;
                    define.animation = state.state.motion;
                    posingOverride.defines.Add(define);
                }
            }

            foreach (var layer in ((AnimatorController)animatorController.animatorController).layers)
            {
                addOverrideFromStateMachine(layer.stateMachine);
            }
        }
    }


}
