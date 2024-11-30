﻿#region

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using VRC.SDKBase;
using VRC.SDK3.Avatars.Components;
using nadena.dev.ndmf;
using nadena.dev.modular_avatar.core;
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
        public override string DisplayName => "ゆにさきポーズシステム・既存ポーズマージ機能";

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

        static string TMP_FOLDER_PATH = "Packages/jp.unisakistudio.posingsystem";
        static string TMP_FOLDER_NAME = "tmp";

        class AddStateMachineBehaviourFailedException : System.Exception
        {
        };

        protected override void Configure()
        {
            var errorLocalizer = new nadena.dev.ndmf.localization.Localizer("ja-jp", () =>
            {
                return new()
                {
                    AssetDatabase.LoadAssetAtPath<LocalizationAsset>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("PosingSystem_Localization_ja-jp")[0])),
                };
            });

            InPhase(BuildPhase.Generating)
                .BeforePlugin("nadena.dev.modular-avatar")
                .Run("Add auto detect override setting", ctx =>
                {
                    if (ctx == null || ctx.AvatarRootObject == null)
                    {
                        return;
                    }
                    var posingOverrides = ctx.AvatarRootObject.GetComponentsInChildren<PosingOverride>();
                    bool mergeTrackingControl = false;
                    foreach (var posingOverride in posingOverrides)
                    {
                        if (posingOverride.ビルド時自動実行)
                        {
                            posingOverride.defines.Clear();
                            PosingOverrideEditor.DetecAutoOverride(posingOverride, ctx.AvatarDescriptor);
                        }

                        // TrackingControlの統合を行う
                        if (posingOverride.mergeTrackingControl)
                        {
                            mergeTrackingControl = true;
                        }
                    }
                    if (mergeTrackingControl)
                    {
                        try
                        {
                            MergeTrackingControl(ctx.AvatarDescriptor);
                        }
                        catch (AddStateMachineBehaviourFailedException e)
                        {
                            ErrorReport.ReportError(errorLocalizer, ErrorSeverity.Error, "AddStateMachineBehaviourに失敗しました");
                            return;
                        }
                    }
                });

            // AnimatorControllerのレイヤーを編集
            InPhase(BuildPhase.Optimizing)
                .AfterPlugin("nadena.dev.modular-avatar")
                .BeforePlugin("com.anatawa12.avatar-optimizer")
                .Run("Override animation", ctx =>
                {
                    // 一時ファイルを削除する
                    if (AssetDatabase.IsValidFolder(TMP_FOLDER_PATH + "/" + TMP_FOLDER_NAME))
                    {
                        AssetDatabase.DeleteAsset(TMP_FOLDER_PATH + "/" + TMP_FOLDER_NAME);
                    }

                    if (ctx == null || ctx.AvatarRootObject == null)
                    {
                        return;
                    }
                    var posingOverrides = ctx.AvatarRootObject.GetComponentsInChildren<PosingOverride>();
                    var waitAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("PosingSystem_Empty")[0]));
                    var runtimeAnimatorController = ctx.AvatarDescriptor.baseAnimationLayers[(int)VRCAvatarDescriptor.AnimLayerType.Base].animatorController;
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
                                            if (behaviour.GetType() == typeof(VRCAnimatorTrackingControl) || behaviour.GetType() == typeof(VRC.SDKBase.VRC_AnimatorTrackingControl))
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

        void MergeTrackingControl(VRCAvatarDescriptor avatarDescriptor)
        {
            // アバターに最初から設定されているBaseLayerのAnimatorControllerは全て同じsuffixとしてTrackingControlをParameterDriverに置き換える
            var suffixIndex = 0;
            var trackingTypesList = new List<(string suffix, List<string> trackingTypes)>();
            var avatarTrackingTypes = new List<string>();
            for (var i=0; i<avatarDescriptor.baseAnimationLayers.Length; i++)
            {
                var animLayer = avatarDescriptor.baseAnimationLayers[i];
                if (animLayer.animatorController != null)
                {
                    avatarTrackingTypes = ReplaceTrackingControlToParameterDriver((AnimatorController)animLayer.animatorController, suffixIndex.ToString(), avatarTrackingTypes);
                }
            }
            trackingTypesList.Add(new(suffixIndex.ToString(), avatarTrackingTypes));

            // 全てのMAMergeAnimatorを調べてそれぞれsuffixを振ってTrackingControlをParameterDriverに置き換える
            foreach (var mergeAnimator in avatarDescriptor.GetComponentsInChildren<ModularAvatarMergeAnimator>())
            {
                suffixIndex++;
                if (mergeAnimator.animator == null)
                {
                    continue;
                }
                mergeAnimator.animator = CloneAnimatorController((AnimatorController)mergeAnimator.animator);
                var trackingTypes = ReplaceTrackingControlToParameterDriver((AnimatorController)mergeAnimator.animator, suffixIndex.ToString(), new());
                trackingTypesList.Add((suffixIndex.ToString(), trackingTypes));
            }

            // 各TrackingTypeにどのSuffixがあるかのハッシュに変換
            var trackingTypeHash = new Dictionary<string, List<string>>();
            foreach (var trackingTypePair in trackingTypesList)
            {
                foreach (var trackingType in trackingTypePair.trackingTypes) {
                    if (trackingTypeHash.GetValueOrDefault(trackingType) == null)
                    {
                        trackingTypeHash[trackingType] = new();
                    }
                    trackingTypeHash[trackingType].Add(trackingTypePair.suffix);
                }
            }

            // MAParameterを既存のオブジェクトにくっつけるとMAParameterが既についててエラーになることがあるので新規オブジェクトを作る
            var maObject = new GameObject("PosingOverrideMergeTrackingControl");
            maObject.transform.parent = avatarDescriptor.transform;

            // 実際にTrackingControlを行うAnimatorControllerをMergeするMAMergeAnimatorを準備
            var maMergeAnimator = maObject.AddComponent<ModularAvatarMergeAnimator>();
            AnimatorController animator = new AnimatorController();
            maMergeAnimator.animator = animator;

            var parameters = new List<AnimatorControllerParameter>();
            foreach (var trackingType in trackingTypeHash.Keys)
            {
                // Animatorにパラメータを追加
                foreach (var suffix in trackingTypeHash[trackingType])
                {
                    parameters.Add(new() { name = trackingType + suffix, type = AnimatorControllerParameterType.Bool, defaultBool = true, });
                }

                // 実際のTrackingControlを行うレイヤーを追加
                var layer = new AnimatorControllerLayer();
                layer.name = "Merge_" + trackingType;

                // TrackingControl用のステート
                layer.stateMachine = new();
                var trackingState = layer.stateMachine.AddState("Tracking", new Vector3(500, 0, 0));
                var animationState = layer.stateMachine.AddState("Animation", new Vector3(500, 60, 0));

                // 実際のTrackingControl
                var trackingBehaviour = trackingState.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();
                var animationBehaviour = animationState.AddStateMachineBehaviour<VRCAnimatorTrackingControl>();

                if (trackingBehaviour == null || animationBehaviour == null)
                {
                    throw new AddStateMachineBehaviourFailedException();
                }

                switch (trackingType)
                {
                    case "trackingHead": trackingBehaviour.trackingHead = VRC_AnimatorTrackingControl.TrackingType.Tracking; animationBehaviour.trackingHead = VRC_AnimatorTrackingControl.TrackingType.Animation; break;
                    case "trackingLeftHand": trackingBehaviour.trackingLeftHand = VRC_AnimatorTrackingControl.TrackingType.Tracking; animationBehaviour.trackingLeftHand = VRC_AnimatorTrackingControl.TrackingType.Animation; break;
                    case "trackingRightHand": trackingBehaviour.trackingRightHand = VRC_AnimatorTrackingControl.TrackingType.Tracking; animationBehaviour.trackingRightHand = VRC_AnimatorTrackingControl.TrackingType.Animation; break;
                    case "trackingLeftFingers": trackingBehaviour.trackingLeftFingers = VRC_AnimatorTrackingControl.TrackingType.Tracking; animationBehaviour.trackingLeftFingers = VRC_AnimatorTrackingControl.TrackingType.Animation; break;
                    case "trackingRightFingers": trackingBehaviour.trackingRightFingers = VRC_AnimatorTrackingControl.TrackingType.Tracking; animationBehaviour.trackingRightFingers = VRC_AnimatorTrackingControl.TrackingType.Animation; break;
                    case "trackingHip": trackingBehaviour.trackingHip = VRC_AnimatorTrackingControl.TrackingType.Tracking; animationBehaviour.trackingHip = VRC_AnimatorTrackingControl.TrackingType.Animation; break;
                    case "trackingLeftFoot": trackingBehaviour.trackingLeftFoot = VRC_AnimatorTrackingControl.TrackingType.Tracking; animationBehaviour.trackingLeftFoot = VRC_AnimatorTrackingControl.TrackingType.Animation; break;
                    case "trackingRightFoot": trackingBehaviour.trackingRightFoot = VRC_AnimatorTrackingControl.TrackingType.Tracking; animationBehaviour.trackingRightFoot = VRC_AnimatorTrackingControl.TrackingType.Animation; break;
                }
                /*
                Debug.Log(typeof(VRC_AnimatorTrackingControl).GetProperty(trackingType));
                typeof(VRC_AnimatorTrackingControl).GetProperty(trackingType).SetValue(trackingBehaviour, VRC_AnimatorTrackingControl.TrackingType.Tracking);
                typeof(VRC_AnimatorTrackingControl).GetProperty(trackingType).SetValue(animationBehaviour, VRC_AnimatorTrackingControl.TrackingType.Animation);
                */

                // trackingを行うのがデフォルト
                layer.stateMachine.AddEntryTransition(trackingState);

                // 1つでもtrackingがfalseだったらAnimationに遷移する（Lockする）
                foreach (var suffix in trackingTypeHash[trackingType])
                {
                    var toAnimationTransition = trackingState.AddTransition(animationState, false);
                    toAnimationTransition.duration = 0;
                    toAnimationTransition.hasExitTime = false;
                    toAnimationTransition.AddCondition(AnimatorConditionMode.IfNot, 0, trackingType + suffix);
                }

                // 全部のtrackingがtrueだったらtrackingに戻る（Lock解除する）
                var toTrackingTransition = animationState.AddTransition(trackingState, false);
                toTrackingTransition.duration = 0;
                toTrackingTransition.hasExitTime = false;
                foreach (var suffix in trackingTypeHash[trackingType])
                {
                    toTrackingTransition.AddCondition(AnimatorConditionMode.If, 0, trackingType + suffix);
                }

                animator.AddLayer(layer);
            }
            // Animatorにパラメータを設定
            animator.parameters = parameters.ToArray();

            // 同期しないExpressionParametersのためにMAParameterを設定
            var maParameter = maObject.AddComponent<ModularAvatarParameters>();
            foreach (var parameter in parameters)
            {
                maParameter.parameters.Add(new() { nameOrPrefix = parameter.name, saved = false, syncType = ParameterSyncType.NotSynced, defaultValue = 1, });
            }
        }

        List<string> ReplaceTrackingControlToParameterDriver(AnimatorController animatorController, string paramSuffix, List<string> trackingTypes)
        {
            // VRCAvatarTrackingControlによるトラッキングコントロールを変数に置き換える
            foreach (var layer in animatorController.layers)
            {
                trackingTypes = ReplaceTrackingControlToParameterDriver(layer.stateMachine, paramSuffix, trackingTypes);
            }

            // パラメータが必要になるので追加する
            foreach (var trackingType in trackingTypes)
            {
                var paramName = trackingType + paramSuffix;
                if (animatorController.parameters.Where(param => param.name == paramName).Count() == 0)
                {
                    animatorController.AddParameter(new() { name = paramName, type = AnimatorControllerParameterType.Bool, defaultBool = true });
                }
            }

            return trackingTypes;
        }

        List<string> ReplaceTrackingControlToParameterDriver(AnimatorStateMachine stateMachine, string paramSuffix, List<string> trackingTypes)
        {
            // StateについてるTrackingControlを列挙して置き換える
            foreach (var state in stateMachine.states)
            {
                foreach (var behaviour in state.state.behaviours.Where(behav => behav.GetType() == typeof(VRCAnimatorTrackingControl)))
                {
                    var trackingControl = (VRCAnimatorTrackingControl)behaviour;

                    // フェイストラッキングとかで使うEye、Mouthは怖いので触らない
                    var trackingTargets = new (string name, VRC_AnimatorTrackingControl.TrackingType type)[] {
                        ("trackingHead", trackingControl.trackingHead),
                        ("trackingLeftHand", trackingControl.trackingLeftHand),
                        ("trackingRightHand", trackingControl.trackingRightHand),
                        ("trackingLeftFingers", trackingControl.trackingLeftFingers),
                        ("trackingRightFingers", trackingControl.trackingRightFingers),
                        ("trackingHip", trackingControl.trackingHip),
                        ("trackingLeftFoot", trackingControl.trackingLeftFoot),
                        ("trackingRightFoot", trackingControl.trackingRightFoot),
                    };

                    foreach (var trackingTarget in trackingTargets)
                    {
                        // アニメーションに固定する場合
                        if (trackingTarget.type == VRC_AnimatorTrackingControl.TrackingType.Animation)
                        {
                            AddParameterDriverToState(state.state, trackingTarget.name + paramSuffix, false);
                            if (trackingTypes.IndexOf(trackingTarget.name) == -1)
                            {
                                trackingTypes.Add(trackingTarget.name);
                            }
                        }
                        // トラッキングを優先する場合
                        if (trackingTarget.type == VRC_AnimatorTrackingControl.TrackingType.Tracking)
                        {
                            AddParameterDriverToState(state.state, trackingTarget.name + paramSuffix, true);
                            if (trackingTypes.IndexOf(trackingTarget.name) == -1)
                            {
                                trackingTypes.Add(trackingTarget.name);
                            }
                        }
                    }

                    // パラメータに置き換えたので、EyeとMouth以外はすべてスルーする
                    trackingControl.trackingHead = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                    trackingControl.trackingLeftHand = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                    trackingControl.trackingRightHand = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                    trackingControl.trackingLeftFingers = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                    trackingControl.trackingRightFingers = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                    trackingControl.trackingHip = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                    trackingControl.trackingLeftFoot = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                    trackingControl.trackingRightFoot = VRC_AnimatorTrackingControl.TrackingType.NoChange;
                }
            }

            // SubStateMachineに潜る
            foreach (var subStateMachine in stateMachine.stateMachines)
            {
                trackingTypes = ReplaceTrackingControlToParameterDriver(subStateMachine.stateMachine, paramSuffix, trackingTypes);
            }
            return trackingTypes;
        }

        void AddParameterDriverToState(AnimatorState state, string paramName, bool value)
        {
            var parameterDriver = state.AddStateMachineBehaviour<VRCAvatarParameterDriver>();
            if (parameterDriver == null)
            {
                throw new AddStateMachineBehaviourFailedException();
            }
            parameterDriver.parameters.Add(new() { type = VRC_AvatarParameterDriver.ChangeType.Set, name = paramName, value = value ? 1 : 0, });
        }

        AnimatorController CloneAnimatorController(AnimatorController srcAnimatorControlloer)
        {
            if (!AssetDatabase.IsValidFolder(TMP_FOLDER_PATH + "/" + TMP_FOLDER_NAME))
            {
                AssetDatabase.CreateFolder(TMP_FOLDER_PATH, TMP_FOLDER_NAME);
            }
            var path = TMP_FOLDER_PATH + "/" + TMP_FOLDER_NAME + "/tmp.controller";
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            
            if (AssetDatabase.IsNativeAsset(srcAnimatorControlloer))
            {
                AssetDatabase.CopyAsset(AssetDatabase.GetAssetPath(srcAnimatorControlloer), path);
            }
            else
            {
                AssetDatabase.CreateAsset(srcAnimatorControlloer, path);
            }

            return AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
        }
    }
}