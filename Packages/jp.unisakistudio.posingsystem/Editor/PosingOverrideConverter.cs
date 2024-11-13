#region

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDKBase;
using nadena.dev.ndmf;
using jp.unisakistudio.posingsystem;

#endregion

[assembly: ExportsPlugin(typeof(jp.unisakistudio.posingsystemeditor.PosingOverrideConverter))]

namespace jp.unisakistudio.posingsystemeditor
{
    public class PosingOverrideConverter : Plugin<PosingOverrideConverter>
    {
        /// <summary>
        /// This name is used to identify the plugin internally, and can be used to declare BeforePlugin/AfterPlugin
        /// dependencies. If not set, the full type name will be used.
        /// </summary>
        public override string QualifiedName => "jp.unisakistudio.posingsytemeditor.posingoverrideconverter";

        /// <summary>
        /// The plugin name shown in debug UIs. If not set, the qualified name will be shown.
        /// </summary>
        public override string DisplayName => "Override animation in animatorController.";

        List<(PosingOverride.OverrideDefine.AnimationStateType type, string layerName, string stateMachineName, string stateName, bool isBlendTree, float posX, float posY)> overrideSettings = new List<(PosingOverride.OverrideDefine.AnimationStateType type, string layerName, string stateMachineName, string stateName, bool isBlendTree, float posX, float posY)>
        {
            (PosingOverride.OverrideDefine.AnimationStateType.StandWalkRun, "USSPS_Locomotion", "", "Standing", false, 0, 0),
            (PosingOverride.OverrideDefine.AnimationStateType.Stand, "USSPS_Locomotion", "", "Standing", true, 0, 0),
            (PosingOverride.OverrideDefine.AnimationStateType.Crouch, "USSPS_Locomotion", "", "Crouching", false, 0, 0),
            (PosingOverride.OverrideDefine.AnimationStateType.Prone, "USSPS_Locomotion", "", "Prone", false, 0, 0),
            (PosingOverride.OverrideDefine.AnimationStateType.Jump, "USSPS_Locomotion", "Jump and Fall", "Jump", false, 0, 0),
            (PosingOverride.OverrideDefine.AnimationStateType.ShortFall, "USSPS_Locomotion", "Jump and Fall", "Short Fall", false, 0, 0),
            (PosingOverride.OverrideDefine.AnimationStateType.ShortLanding, "USSPS_Locomotion", "Jump and Fall", "Soft Landing", false, 0, 0),
            (PosingOverride.OverrideDefine.AnimationStateType.LongFall, "USSPS_Locomotion", "Jump and Fall", "Long Fall", false, 0, 0),
            (PosingOverride.OverrideDefine.AnimationStateType.LongLanding, "USSPS_Locomotion", "Jump and Fall", "Hard Landing", false, 0, 0),
        };

        protected override void Configure()
        {
            InPhase(BuildPhase.Generating)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run("Add auto detect override setting", ctx =>
                {
                    if (ctx == null || ctx.AvatarRootObject == null)
                    {
                        return;
                    }
                    var posingOverrides = ctx.AvatarRootObject.GetComponentsInChildren<PosingOverride>();
                    foreach (var posingOverride in posingOverrides)
                    {
                        if (posingOverride.ビルド時自動実行)
                        {
                            posingOverride.defines.Clear();
                            PosingOverrideEditor.DetecAutoOverride(posingOverride, ctx.AvatarDescriptor);
                        }
                    }
                });

            // AnimatorControllerのレイヤーを編集
            InPhase(BuildPhase.Optimizing)
                .AfterPlugin("nadena.dev.modular-avatar")
                .BeforePlugin("com.anatawa12.avatar-optimizer")
                .Run("Override animation", ctx =>
                {
                    if (ctx == null || ctx.AvatarRootObject == null)
                    {
                        return;
                    }
                    var posingOverrides = ctx.AvatarRootObject.GetComponentsInChildren<PosingOverride>();
                    var waitAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("PosingSystem_Empty")[0]));
                    var runtimeAnimatorController = ctx.AvatarDescriptor.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base].animatorController;
                    if (runtimeAnimatorController == null && runtimeAnimatorController.GetType() != typeof(AnimatorController))
                    {
                        Debug.Log("animatorController == null");
                        return;
                    }
                    var animatorController = (AnimatorController)runtimeAnimatorController;

                    foreach (var posingOverride in posingOverrides)
                    {
                        // 既存のLocomotionレイヤーを削除する
                        if (posingOverride.deleteExistingLayer)
                        {
                            bool isContainHumanoidAnimation(Motion motion)
                            {
                                if (motion == null)
                                {
                                    return false;
                                }
                                if (motion.GetType() == typeof(AnimationClip))
                                {
                                    if (AnimationUtility.GetCurveBindings((AnimationClip)motion).Any(bind => {
                                        Debug.Log(bind.propertyName);
                                        return HumanTrait.MuscleName.ToList().IndexOf(bind.propertyName) != -1;
                                    }))
                                    {
                                        return true;
                                    }
                                }
                                if (motion.GetType() == typeof(BlendTree))
                                {
                                    foreach (var child in ((BlendTree)motion).children)
                                    {
                                        if (isContainHumanoidAnimation(child.motion))
                                        {
                                            return true;
                                        }
                                    }
                                }
                                return false;
                            }
                            bool isContainHumanoidAnimationState(AnimatorStateMachine stateMachine)
                            {
                                foreach (var state in stateMachine.states)
                                {
                                    if (isContainHumanoidAnimation(state.state.motion))
                                    {
                                        return true;
                                    }
                                }
                                foreach (var subStateMachine in stateMachine.stateMachines)
                                {
                                    if (isContainHumanoidAnimationState(subStateMachine.stateMachine))
                                    {
                                        return true;
                                    }
                                }
                                return false;
                            }

                            var layer = animatorController.layers.FirstOrDefault(lay => lay.name != "USSPS_Locomotion" && lay.stateMachine.states.Any(sta =>
                            {
                                return isContainHumanoidAnimationState(lay.stateMachine);
                            }));
                            if (layer != null)
                            {
                                var layerIndex = animatorController.layers.ToList().FindIndex(lay => lay.name == layer.name);
                                Debug.Log(layer.name + " : " + layerIndex.ToString());
                                animatorController.RemoveLayer(layerIndex);
                            }
                        }

                        if (posingOverride.deleteExistingTrackingControl)
                        {
                            foreach (var layer in animatorController.layers)
                            {
                                if (layer.name.StartsWith("USSPS_"))
                                {
                                    continue;
                                }

                                void deleteTrackingControl(AnimatorStateMachine stateMachine)
                                {
                                    foreach (var state in stateMachine.states)
                                    {
                                        foreach (var behaviour in state.state.behaviours)
                                        {
                                            if (behaviour.GetType() == typeof(VRC.SDK3.Avatars.Components.VRCAnimatorTrackingControl) || behaviour.GetType() == typeof(VRC.SDKBase.VRC_AnimatorTrackingControl))
                                            {
                                                Object.DestroyImmediate(behaviour, true);
                                            }
                                        }
                                    }
                                    foreach (var subStateMachine in stateMachine.stateMachines)
                                    {
                                        deleteTrackingControl(subStateMachine.stateMachine);
                                    }
                                }

                                deleteTrackingControl(layer.stateMachine);
                            }
                        }

                        foreach (var define in posingOverride.defines)
                        {
                            var overrideSetting = overrideSettings.First(setting => setting.type == define.type);
                            var layerIndex = animatorController.layers.ToList().FindIndex(l => l.name == overrideSetting.layerName);
                            if (layerIndex == -1)
                            {
                                continue;
                            }
                            var layer = animatorController.layers[layerIndex];
                            AnimatorState animatorState = null;
                            if (overrideSetting.stateMachineName.Length > 0)
                            {
                                var stateMachine = layer.stateMachine.stateMachines.First(s => s.stateMachine.name == overrideSetting.stateMachineName).stateMachine;
                                animatorState = stateMachine.states.First(state => state.state.name == overrideSetting.stateName).state;
                            }
                            else
                            {
                                animatorState = layer.stateMachine.states.First(state => state.state.name == overrideSetting.stateName).state;
                            }

                            if (overrideSetting.isBlendTree)
                            {
                                if (animatorState.motion.GetType() == typeof(BlendTree) || animatorState.motion.GetType().IsSubclassOf(typeof(BlendTree)))
                                {
                                    BlendTree blendTree = (BlendTree)animatorState.motion;
                                    var blendTreeIndex = blendTree.children.ToList().FindIndex(child => Mathf.Approximately(child.position.x, overrideSetting.posX) && Mathf.Approximately(child.position.y, overrideSetting.posY));
                                    if (blendTreeIndex != -1)
                                    {
                                        blendTree.RemoveChild(blendTreeIndex);
                                    }
                                    blendTree.AddChild(define.animation, new Vector2(overrideSetting.posX, overrideSetting.posY));
                                }
                                else
                                {
                                    Debug.LogError(string.Format("可愛いポーズツールのPosingOverride機能でBlendTreeのはずのAnimatorStateがBlendTreeではありませんでした。「立ちのみ」の変更を行う場合は「立ち・歩き・走り」にはBlendTreeを設定してください"));
                                }
                            }
                            else
                            {
                                animatorState.motion = define.animation;
                            }
                        }
                        Object.DestroyImmediate(posingOverride);
                    }
                });
        }
    }
}