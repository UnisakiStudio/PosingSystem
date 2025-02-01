#region

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using jp.unisakistudio.posingsystem;
#if NDMF
using nadena.dev.ndmf;
#endif
#if MODULAR_AVATAR
using nadena.dev.modular_avatar.core;
#endif

#endregion

#if NDMF && MODULAR_AVATAR

[assembly: ExportsPlugin(typeof(jp.unisakistudio.posingsystemeditor.PosingSystemConverter))]

namespace jp.unisakistudio.posingsystemeditor
{
    public class PosingSystemConverter : Plugin<PosingSystemConverter>
    {
        /// <summary>
        /// This name is used to identify the plugin internally, and can be used to declare BeforePlugin/AfterPlugin
        /// dependencies. If not set, the full type name will be used.
        /// </summary>
        public override string QualifiedName => "jp.unisakistudio.posingsystemeditor.posingsystemconverter";

        /// <summary>
        /// The plugin name shown in debug UIs. If not set, the qualified name will be shown.
        /// </summary>
        public override string DisplayName => "ゆにさきポーズシステム";

        delegate float ValueChanger(Keyframe value, int i);

        nadena.dev.ndmf.localization.Localizer errorLocalizer;

        protected override void Configure()
        {
            errorLocalizer = new nadena.dev.ndmf.localization.Localizer("ja-jp", () =>
            {
                return new()
                {
                    AssetDatabase.LoadAssetAtPath<LocalizationAsset>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("PosingSystem_Localization_ja-jp")[0])),
                };
            });

            // メニューアイテムを作成
            InPhase(BuildPhase.Generating)
            .Run("Create posing system menu", ctx =>
            {
                if (ctx == null || ctx.AvatarRootObject == null)
                {
                    return;
                }
                var posingSystems = ctx.AvatarRootObject.GetComponentsInChildren<PosingSystem>();
                if (posingSystems.Length == 0)
                {
                    return;
                }
                var syncdParameterValue = 3;
                foreach (var posingSystem in posingSystems)
                {
                    foreach (var define in posingSystem.defines)
                    {
                        bool isSkip = define.animations.Count(animation => animation.enabled) == 0;
                        if (isSkip)
                        {
                            continue;
                        }
                        Transform submenuRoot = posingSystem.SubmenuRoot != null ? submenuRoot = posingSystem.SubmenuRoot : posingSystem.transform;
                        Transform parentSearch = submenuRoot.parent;
                        while (parentSearch != null)
                        {
                            PosingSystem parentPosingSystem = null;
                            if (parentSearch.TryGetComponent<PosingSystem>(out parentPosingSystem))
                            {
                                submenuRoot = parentPosingSystem.SubmenuRoot ? parentPosingSystem.SubmenuRoot : parentPosingSystem.transform;
                            }
                            parentSearch = parentSearch.parent;
                        }
                        if (submenuRoot != posingSystem.SubmenuRoot && submenuRoot != posingSystem.transform)
                        {
                            foreach (var menuGroup in posingSystem.GetComponentsInChildren<ModularAvatarMenuGroup>())
                            {
                                Object.DestroyImmediate(menuGroup);
                            }
                            Object.DestroyImmediate(posingSystem.GetComponent<ModularAvatarMenuInstaller>());
                            Object.DestroyImmediate(posingSystem.GetComponent<ModularAvatarMenuItem>());
                        }
                        var menu = submenuRoot.GetComponentsInChildren<ModularAvatarMenuItem>().FirstOrDefault((submenu) =>
                        {
                            return submenu.Control.type == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu
                                && submenu.name == define.menuName;
                        });
                        GameObject menuObject = null;
                        if (menu == null)
                        {
                            menuObject = new GameObject(define.menuName);
                            menu = menuObject.AddComponent<ModularAvatarMenuItem>();
                            menu.MenuSource = SubmenuSource.Children;
                            menu.Control.type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu;
                            menu.Control.name = define.menuName;
                            menu.Control.icon = define.icon;
                            menu.Control.parameter = new() { name = "USSPS_LastMenu" };
                            menu.Control.value = define.locomotionTypeValue;
                            menuObject.transform.parent = submenuRoot;
                        }
                        else
                        {
                            menuObject = menu.gameObject;
                        }

                        var exParamMax = 0;
                        var menuItems = ctx.AvatarRootObject.GetComponentsInChildren<ModularAvatarMenuItem>();
                        if (menuItems.Length > 0)
                        {
                            exParamMax = menuItems.Max(item => item.Control != null && item.Control.parameter != null && item.Control.parameter.name == define.paramName ? (int)item.Control.value : 0);
                        }
                        int animationValue = exParamMax + 1;
                        foreach (var animation in define.animations)
                        {
                            if (!animation.enabled)
                            {
                                continue;
                            }
                            var itemObject = new GameObject(animation.displayName);
                            itemObject.transform.parent = menuObject.transform;
                            var item = itemObject.AddComponent<ModularAvatarMenuItem>();
                            item.MenuSource = SubmenuSource.Children;
                            item.Control.type = VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.Toggle;
                            item.Control.name = animation.displayName;
                            item.Control.parameter = new VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.Parameter();
                            item.Control.parameter.name = define.paramName;
                            item.Control.value = animation.typeParameterValue = animationValue;
                            animation.syncdParameterValue = syncdParameterValue;

                            if (animation.isCustomIcon)
                            {
                                item.Control.icon = animation.icon;
                            }

                            animationValue++;
                            syncdParameterValue++;
                        }

                    }
                }
                for (var i = 1; i * 255 < syncdParameterValue; i++)
                {
                    var parameters = ctx.AvatarRootObject.GetComponentInChildren<ModularAvatarParameters>();
                    var param = new ParameterConfig();
                    param.syncType = ParameterSyncType.Int;
                    param.localOnly = false;
                    param.nameOrPrefix = "USSPS_Pose" + i.ToString();
                    param.defaultValue = 0;
                    param.saved = false;
                    parameters.parameters.Add(param);
                }
            });

            // AnimatorControllerのレイヤーを編集
            InPhase(BuildPhase.Transforming)
                .AfterPlugin("nadena.dev.modular-avatar")
                .Run("Convert posing system animator controller", ctx =>
                {
                    if (ctx == null || ctx.AvatarRootObject == null)
                    {
                        return;
                    }

                    var waitAnimation = AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("PosingSystem_Empty")[0]));
                    var runtimeAnimatorController = ctx.AvatarDescriptor.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base].animatorController;
                    if (runtimeAnimatorController == null && runtimeAnimatorController.GetType() != typeof(AnimatorController))
                    {
                        Debug.Log("animatorController == null");
                        return;
                    }
                    var animatorController = (AnimatorController)runtimeAnimatorController;
                    var layer = animatorController.layers.FirstOrDefault(l => l.name == "USSPS_Locomotion");
                    if (layer == null)
                    {
                        return;
                    }
                    var stateMachine = layer.stateMachine;
                    var typeLayer = animatorController.layers.FirstOrDefault(l => l.name == "USSPS_LocomotionType");
                    if (typeLayer == null)
                    {
                        return;
                    }

                    bool isWriteDefaultsOff(AnimatorStateMachine searchStateMachine)
                    {
                        foreach (var state in searchStateMachine.states)
                        {
                            if (!state.state.writeDefaultValues)
                            {
                                return true;
                            }
                        }
                        foreach (var subStateMachine in searchStateMachine.stateMachines)
                        {
                            if (isWriteDefaultsOff(subStateMachine.stateMachine))
                            {
                                return true;
                            }
                        }
                        return false;
                    }
                    var writeDefaults = !animatorController.layers.Any(l => isWriteDefaultsOff(l.stateMachine));

                    var posingSystems = ctx.AvatarRootObject.GetComponentsInChildren<PosingSystem>();
                    if (posingSystems.Length == 0)
                    {
                        return;
                    }
                    var syncdParamValueMax = posingSystems.Max(posing => posing.defines.DefaultIfEmpty().Max(def => def == null ? 0 : def.animations.DefaultIfEmpty().Max(anim => anim == null ? 0 : anim.syncdParameterValue)));
                    var syncdParamNum = (syncdParamValueMax - 1) / 255 + 1;
                    for (var i = 1; i < syncdParamNum; i++)
                    {
                        animatorController.AddParameter("USSPS_Pose" + i.ToString(), AnimatorControllerParameterType.Int);
                        var standingEntryTransition = layer.stateMachine.entryTransitions.First(st => st.destinationState == layer.stateMachine.defaultState);
                        standingEntryTransition.AddCondition(AnimatorConditionMode.Equals, 0, "USSPS_Pose" + i.ToString());
                        var standingExitTransition = layer.stateMachine.defaultState.AddExitTransition(false);
                        standingExitTransition.AddCondition(AnimatorConditionMode.NotEqual, 0, "USSPS_Pose" + i.ToString());
                        standingExitTransition.interruptionSource = TransitionInterruptionSource.Destination;

                        foreach (var state in typeLayer.stateMachine.states)
                        {
                            var behaviour = state.state.behaviours.FirstOrDefault(be => be.GetType() == typeof(VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver));
                            if (behaviour != null)
                            {
                                var parameterDriver = (VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver)behaviour;
                                var driveParam = new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter();
                                driveParam.name = "USSPS_Pose" + i.ToString();
                                driveParam.value = 0;
                                parameterDriver.parameters.Add(driveParam);
                            }
                        }
                    }

                    foreach (var posingSystem in posingSystems)
                    {
                        float avatarHeightUnit = -1;
                        var baseAnimationClip = new AnimationClip();
                        var rootTyBinding = new EditorCurveBinding();
                        rootTyBinding.path = "";
                        rootTyBinding.type = typeof(Animator);
                        rootTyBinding.propertyName = "RootT.y";
                        AnimationCurve rootTyCurve = new AnimationCurve();
                        rootTyCurve.AddKey(0, 1);
                        AnimationUtility.SetEditorCurve(baseAnimationClip, rootTyBinding, rootTyCurve);
                        var clipInfo = new AnimationClipSettings();
                        clipInfo.keepOriginalOrientation = true;
                        clipInfo.keepOriginalPositionXZ = true;
                        clipInfo.keepOriginalPositionY = true;
                        AnimationUtility.SetAnimationClipSettings(baseAnimationClip, clipInfo);
                        AnimationMode.StartAnimationMode();
                        AnimationMode.BeginSampling();
                        AnimationMode.SampleAnimationClip(ctx.AvatarRootObject, baseAnimationClip, 0);
                        ctx.AvatarRootTransform.position = Vector3.zero;
                        ctx.AvatarRootTransform.rotation = Quaternion.identity;
                        AnimationMode.EndSampling();
                        avatarHeightUnit = ctx.AvatarRootObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips).position.y;
                        AnimationMode.StopAnimationMode();
                        ctx.AvatarRootObject.SetActive(false);
                        ctx.AvatarRootObject.SetActive(true);

                        foreach (var define in posingSystem.defines)
                        {
                            var typeStateMachine = typeLayer.stateMachine.stateMachines.First(s => s.stateMachine.name == define.stateMachineName).stateMachine;
                            var typeParameter = animatorController.parameters.FirstOrDefault(param => param.name == define.paramName);
                            bool isTypeParameterFloat = false;
                            if (typeParameter != null)
                            {
                                if (typeParameter.type == AnimatorControllerParameterType.Float)
                                {
                                    isTypeParameterFloat = true;
                                }
                            }
                            var vrmodeParameter = animatorController.parameters.FirstOrDefault(param => param.name == "VRMode");
                            bool isVRModeParameterFloat = false;
                            if (vrmodeParameter != null)
                            {
                                if (vrmodeParameter.type == AnimatorControllerParameterType.Float)
                                {
                                    isVRModeParameterFloat = true;
                                }
                            }
                            foreach (var animationDefine in define.animations)
                            {
                                if (!animationDefine.enabled)
                                {
                                    continue;
                                }
                                Motion motion = null;
                                if (animationDefine.animationClip.GetType() == typeof(AnimationClip))
                                {
                                    var animationClip = animationDefine.animationClip != null ? (AnimationClip)animationDefine.animationClip : waitAnimation;
                                    AnimationMode.StartAnimationMode();
                                    //                                ctx.AvatarRootTransform.position = new Vector3(ctx.AvatarDescriptor.ViewPosition.x, 0, ctx.AvatarDescriptor.ViewPosition.z);
                                    ctx.AvatarRootTransform.LookAt(new Vector3(0, 0, 1));
                                    var eyeObject = new GameObject();
                                    eyeObject.transform.parent = ctx.AvatarRootObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head);
                                    //eyeObject.transform.position = ctx.AvatarDescriptor.ViewPosition + new Vector3(0, 0.1f, 0);
                                    eyeObject.transform.localPosition = new Vector3(0, 0, 0);
                                    AnimationMode.BeginSampling();
                                    AnimationMode.SampleAnimationClip(ctx.AvatarRootObject, animationClip, 0);
                                    AnimationMode.EndSampling();
                                    //ctx.AvatarRootTransform.transform.rotation = Quaternion.Euler(0, animationDefine.rotate, 0) * ctx.AvatarRootTransform.transform.rotation;
                                    //var headPosision = ctx.AvatarRootObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position;
                                    var headPosision = eyeObject.transform.position;
                                    ctx.AvatarRootTransform.LookAt(new Vector3(0, 0, 1));

                                    animationClip = Object.Instantiate(animationClip);

                                    var srcClipSetting = AnimationUtility.GetAnimationClipSettings(animationClip);
                                    if (srcClipSetting.mirror)
                                    {
                                        srcClipSetting.mirror = false;
                                        AnimationUtility.SetAnimationClipSettings(animationClip, srcClipSetting);
                                    }

                                    var rootQxList = new List<float>();
                                    var rootQyList = new List<float>();
                                    var rootQzList = new List<float>();
                                    var rootQwList = new List<float>();
                                    var rootTxList = new List<float>();
                                    var rootTyList = new List<float>();
                                    var rootTzList = new List<float>();

                                    List<Quaternion> rootQs = new List<Quaternion>();
                                    foreach (var binding in AnimationUtility.GetCurveBindings(animationClip))
                                    {
                                        if (!binding.propertyName.StartsWith("RootQ."))
                                        {
                                            continue;
                                        }
                                        var curve = AnimationUtility.GetEditorCurve(animationClip, binding);
                                        for (int i = 0; i < curve.keys.Length; i++)
                                        {
                                            AnimationMode.BeginSampling();
                                            AnimationMode.SampleAnimationClip(ctx.AvatarRootObject, animationClip, curve.keys[i].time);
                                            AnimationMode.EndSampling();
                                            Quaternion rootQ = new Quaternion();
                                            var rootBinding = binding;
                                            rootBinding.propertyName = "RootQ.x";
                                            AnimationUtility.GetFloatValue(ctx.AvatarRootObject, rootBinding, out rootQ.x);
                                            rootBinding.propertyName = "RootQ.y";
                                            AnimationUtility.GetFloatValue(ctx.AvatarRootObject, rootBinding, out rootQ.y);
                                            rootBinding.propertyName = "RootQ.z";
                                            AnimationUtility.GetFloatValue(ctx.AvatarRootObject, rootBinding, out rootQ.z);
                                            rootBinding.propertyName = "RootQ.w";
                                            AnimationUtility.GetFloatValue(ctx.AvatarRootObject, rootBinding, out rootQ.w);

                                            //rootQ = Quaternion.Euler(0, animationDefine.rotate, 0) * rootQ;
                                            if (binding.propertyName == "RootQ.x")
                                                rootQxList.Add(rootQ.x);
                                            if (binding.propertyName == "RootQ.y")
                                                rootQyList.Add(rootQ.y);
                                            if (binding.propertyName == "RootQ.z")
                                                rootQzList.Add(rootQ.z);
                                            if (binding.propertyName == "RootQ.w")
                                                rootQwList.Add(rootQ.w);
                                        }
                                    }
                                    foreach (var binding in AnimationUtility.GetCurveBindings(animationClip))
                                    {
                                        if (!binding.propertyName.StartsWith("RootT."))
                                        {
                                            continue;
                                        }
                                        var curve = AnimationUtility.GetEditorCurve(animationClip, binding);
                                        for (int i = 0; i < curve.keys.Length; i++)
                                        {
                                            AnimationMode.BeginSampling();
                                            AnimationMode.SampleAnimationClip(ctx.AvatarRootObject, animationClip, curve.keys[i].time);
                                            AnimationMode.EndSampling();
                                            var rootT = new Vector3();
                                            var rootBinding = binding;
                                            rootBinding.propertyName = "RootT.x";
                                            AnimationUtility.GetFloatValue(ctx.AvatarRootObject, rootBinding, out rootT.x);
                                            rootBinding.propertyName = "RootT.y";
                                            AnimationUtility.GetFloatValue(ctx.AvatarRootObject, rootBinding, out rootT.y);
                                            rootBinding.propertyName = "RootT.z";
                                            AnimationUtility.GetFloatValue(ctx.AvatarRootObject, rootBinding, out rootT.z);

                                            rootT -= headPosision / avatarHeightUnit;
                                            //rootT = Quaternion.Euler(0, animationDefine.rotate, 0) * rootT;
                                            if (binding.propertyName == "RootT.x")
                                                rootTxList.Add(rootT.x);
                                            if (binding.propertyName == "RootT.y")
                                                rootTyList.Add(rootT.y);
                                            if (binding.propertyName == "RootT.z")
                                                rootTzList.Add(rootT.z);
                                        }
                                    }

                                    AnimationMode.StopAnimationMode();
                                    ctx.AvatarRootObject.SetActive(false);
                                    ctx.AvatarRootObject.SetActive(true);

                                    var newAnimationClip = Object.Instantiate(animationClip);
                                    foreach (var binding in AnimationUtility.GetCurveBindings(newAnimationClip))
                                    {
                                        void setCurve(string propertyName, ValueChanger changer)
                                        {
                                            if (binding.propertyName == propertyName)
                                            {
                                                var curve = AnimationUtility.GetEditorCurve(newAnimationClip, binding);
                                                for (int i = 0; i < curve.keys.Length; i++)
                                                {
                                                    var key = curve.keys[i];
                                                    key.value = changer(key, i);
                                                    curve.MoveKey(i, key);
                                                }
                                                AnimationUtility.SetEditorCurve(newAnimationClip, binding, curve);
                                            }
                                        };

                                        setCurve("RootT.x", (keyframe, i) => rootTxList[i]);
                                        setCurve("RootT.z", (keyframe, i) => rootTzList[i]);
                                        setCurve("RootQ.x", (keyframe, i) => rootQxList[i]);
                                        setCurve("RootQ.y", (keyframe, i) => rootQyList[i]);
                                        setCurve("RootQ.z", (keyframe, i) => rootQzList[i]);
                                        setCurve("RootQ.w", (keyframe, i) => rootQwList[i]);
                                    }
                                    if (animationDefine.isRotate)
                                    {
                                        var clipSetting = AnimationUtility.GetAnimationClipSettings(newAnimationClip);
                                        clipSetting.orientationOffsetY += animationDefine.rotate;
                                        AnimationUtility.SetAnimationClipSettings(newAnimationClip, clipSetting);
                                    }

                                    motion = newAnimationClip;
                                }
                                else if (animationDefine.animationClip.GetType() == typeof(BlendTree))
                                {
                                    motion = animationDefine.animationClip;
                                }

                                var syncdParamName = "USSPS_Pose";
                                var syncdParamValue = animationDefine.syncdParameterValue;
                                if (syncdParamValue > 255)
                                {
                                    var paramNum = (animationDefine.syncdParameterValue - 1) / 255;
                                    syncdParamName += paramNum.ToString();
                                    syncdParamValue = (animationDefine.syncdParameterValue - 1) % 255 + 1;
                                }
                                var animationName = animationDefine.animationClip != null ? animationDefine.animationClip.name : "Animation";

                                // デスクトップ用
                                {
                                    var state = stateMachine.AddState(animationName + "_Desktop", new Vector3(500, (animationDefine.syncdParameterValue - 1) * 120));

                                    state.motion = motion;
                                    state.writeDefaultValues = writeDefaults;
                                    if (animationDefine.isMotionTime)
                                    {
                                        if (animatorController.parameters.Where(param => param.name == animationDefine.motionTimeParamName).Count() == 0)
                                        {
                                            animatorController.AddParameter(animationDefine.motionTimeParamName, AnimatorControllerParameterType.Float);
                                        }
                                        state.timeParameterActive = true;
                                        state.timeParameter = animationDefine.motionTimeParamName;
                                    }
                                    state.mirrorParameterActive = true;
                                    state.mirrorParameter = "USSPS_Mirror";

                                    var poseSpace = state.AddStateMachineBehaviour<VRC.SDK3.Avatars.Components.VRCAnimatorTemporaryPoseSpace>();
                                    if (poseSpace == null)
                                    {
                                        ErrorReport.ReportError(errorLocalizer, ErrorSeverity.Error, "AddStateMachineBehaviourに失敗しました");
                                        return;
                                    }
                                    poseSpace.enterPoseSpace = true;
                                    poseSpace.fixedDelay = false;
                                    poseSpace.delayTime = 0.0f;

                                    var transition = stateMachine.AddEntryTransition(state);
                                    transition.AddCondition(AnimatorConditionMode.Equals, syncdParamValue, syncdParamName);
                                    if (isVRModeParameterFloat)
                                    {
                                        transition.AddCondition(AnimatorConditionMode.Less, 0.01f, "VRMode");
                                    }
                                    else
                                    {
                                        transition.AddCondition(AnimatorConditionMode.Equals, 0, "VRMode");
                                    }

                                    var exitTransition = state.AddExitTransition(false);
                                    exitTransition.AddCondition(AnimatorConditionMode.NotEqual, syncdParamValue, syncdParamName);
                                    exitTransition.duration = 0.0f;
                                    exitTransition.interruptionSource = TransitionInterruptionSource.Destination;
                                }

                                // ３点トラッキング用
                                {
                                    var state = stateMachine.AddState(animationName, new Vector3(500, (animationDefine.syncdParameterValue - 1) * 120 + 60));

                                    state.motion = motion;
                                    state.writeDefaultValues = writeDefaults;
                                    if (animationDefine.isMotionTime)
                                    {
                                        if (animatorController.parameters.Where(param => param.name == animationDefine.motionTimeParamName).Count() == 0)
                                        {
                                            animatorController.AddParameter(animationDefine.motionTimeParamName, AnimatorControllerParameterType.Float);
                                        }
                                        state.timeParameterActive = true;
                                        state.timeParameter = animationDefine.motionTimeParamName;
                                    }
                                    state.mirrorParameterActive = true;
                                    state.mirrorParameter = "USSPS_Mirror";

                                    var transition = stateMachine.AddEntryTransition(state);
                                    transition.AddCondition(AnimatorConditionMode.Equals, syncdParamValue, syncdParamName);
                                    if (isVRModeParameterFloat)
                                    {
                                        transition.AddCondition(AnimatorConditionMode.Greater, 0.01f, "VRMode");
                                    }
                                    else
                                    {
                                        transition.AddCondition(AnimatorConditionMode.Equals, 1, "VRMode");
                                    }

                                    var exitTransition = state.AddExitTransition(false);
                                    exitTransition.AddCondition(AnimatorConditionMode.NotEqual, syncdParamValue, syncdParamName);
                                    exitTransition.duration = 0.5f;
                                    exitTransition.interruptionSource = TransitionInterruptionSource.Destination;
                                }

                                // LocomotionType switch
                                var typeState = typeStateMachine.AddState(animationName, new Vector3(500, (animationDefine.typeParameterValue - 1) * 60));
                                typeState.writeDefaultValues = writeDefaults;
                                typeState.motion = waitAnimation;

                                if (isTypeParameterFloat)
                                {
                                    var typeEnterTransition = typeStateMachine.AddEntryTransition(typeState);
                                    typeEnterTransition.AddCondition(AnimatorConditionMode.Greater, animationDefine.typeParameterValue - 0.01f, define.paramName);
                                    typeEnterTransition.AddCondition(AnimatorConditionMode.Less, animationDefine.typeParameterValue + 0.01f, define.paramName);

                                    var typeExitTransitionU = typeState.AddExitTransition(false);
                                    typeExitTransitionU.AddCondition(AnimatorConditionMode.Less, animationDefine.typeParameterValue - 0.01f, define.paramName);
                                    typeExitTransitionU.duration = 0.5f;
                                    typeExitTransitionU.interruptionSource = TransitionInterruptionSource.Destination;

                                    var typeExitTransitionL = typeState.AddExitTransition(false);
                                    typeExitTransitionL.AddCondition(AnimatorConditionMode.Greater, animationDefine.typeParameterValue + 0.01f, define.paramName);
                                    typeExitTransitionL.duration = 0.5f;
                                    typeExitTransitionL.interruptionSource = TransitionInterruptionSource.Destination;
                                }
                                else
                                {
                                    var typeEnterTransition = typeStateMachine.AddEntryTransition(typeState);
                                    typeEnterTransition.AddCondition(AnimatorConditionMode.Equals, animationDefine.typeParameterValue, define.paramName);

                                    var typeExitTransition = typeState.AddExitTransition(false);
                                    typeExitTransition.AddCondition(AnimatorConditionMode.NotEqual, animationDefine.typeParameterValue, define.paramName);
                                    typeExitTransition.duration = 0.5f;
                                    typeExitTransition.interruptionSource = TransitionInterruptionSource.Destination;
                                }

                                var typeExitTransition1 = typeState.AddExitTransition(false);
                                typeExitTransition1.AddCondition(AnimatorConditionMode.NotEqual, define.locomotionTypeValue, "LocomotionType");
                                typeExitTransition1.duration = 0.5f;
                                typeExitTransition1.interruptionSource = TransitionInterruptionSource.Destination;

                                var typeParameterDriver = typeState.AddStateMachineBehaviour<VRC.SDK3.Avatars.Components.VRCAvatarParameterDriver>();
                                if (typeParameterDriver == null)
                                {
                                    ErrorReport.ReportError(errorLocalizer, ErrorSeverity.Error, "AddStateMachineBehaviourに失敗しました");
                                    return;
                                }
                                typeParameterDriver.isLocalPlayer = true;
                                for (var i = 0; i < syncdParamNum; i++)
                                {
                                    var setSyncdParamName = "USSPS_Pose";
                                    if (i != 0)
                                    {
                                        setSyncdParamName += i.ToString();
                                    }
                                    if (setSyncdParamName == syncdParamName)
                                    {
                                        var drivingParam = new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter();
                                        drivingParam.name = syncdParamName;
                                        drivingParam.value = syncdParamValue;
                                        typeParameterDriver.parameters.Add(drivingParam);
                                    }
                                    else
                                    {
                                        var drivingParam = new VRC.SDKBase.VRC_AvatarParameterDriver.Parameter();
                                        drivingParam.name = setSyncdParamName;
                                        drivingParam.value = 0;
                                        typeParameterDriver.parameters.Add(drivingParam);
                                    }
                                }

                                /*
                                foreach (var transiton in typeStateMachine.entryTransitions)
                                {
                                    var exTransition1 = typeState.AddTransition(transiton.destinationState);
                                    exTransition1.AddCondition(transiton.conditions[0].mode, transiton.conditions[0].threshold, transiton.conditions[0].parameter);
                                    exTransition1.duration = 0.1f;

                                    var exTransition2 = transiton.destinationState.AddTransition(typeState);
                                    exTransition2.AddCondition(AnimatorConditionMode.Equals, animationDefine.typeParameterValue, define.paramName);
                                    exTransition2.duration = 0.5f;
                                }
                                */

                            }
                        }
                    }
                });

            InPhase(BuildPhase.Optimizing)
                .BeforePlugin("com.anatawa12.avatar-optimizer")
                .AfterPlugin("nadena.dev.modular-avatar")
                .Run("Create posing system menu", ctx =>
                {
                    if (ctx == null || ctx.AvatarRootObject == null)
                    {
                        return;
                    }

                    var posingSystems = ctx.AvatarRootObject.GetComponentsInChildren<PosingSystem>();
                    if (posingSystems.Length == 0)
                    {
                        return;
                    }

                    var cameraGameObject = new GameObject();
                    var camera = cameraGameObject.AddComponent<Camera>();
                    camera.fieldOfView = 30;
                    camera.clearFlags = CameraClearFlags.Color;
                    camera.backgroundColor = new Color(0, 0, 0, 0);

                    var defineParamNames = posingSystems.SelectMany(posingSystem => posingSystem.defines.Select(define => define.paramName));

                    // メニューから本システムのメニューアイテムを探す
                    void takeIconForControl(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control control)
                    {
                        if (control.type == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu)
                        {
                            if (control.subMenu == null)
                            {
                                return;
                            }
                            foreach (var subControl in control.subMenu.controls)
                            {
                                takeIconForControl(subControl);
                            }
                            return;
                        }

                        // Toggleならパラメータ名でチェック
                        if (control.type == VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.Toggle)
                        {
                            if (control.parameter == null)
                            {
                                return;
                            }
                            if (!defineParamNames.Contains(control.parameter.name))
                            {
                                return;
                            }


                            // パラメータ名がヒットしたのでdefineを探す
                            foreach (var posingSystem in posingSystems)
                            {
                                foreach (var define in posingSystem.defines)
                                {
                                    if (define.paramName != control.parameter.name)
                                    {
                                        continue;
                                    }
                                    foreach (var animation in define.animations)
                                    {
                                        if (animation.typeParameterValue != control.value)
                                        {
                                            continue;
                                        }

                                        // アイコンの撮影
                                        if (!animation.enabled)
                                        {
                                            continue;
                                        }
                                        if (animation.icon != null)
                                        {
                                            continue;
                                        }
                                        if (posingSystem.isIconDisabled)
                                        {
                                            continue;
                                        }
                                        AnimationMode.StartAnimationMode();
                                        if (animation.animationClip == null)
                                        {
                                            ctx.AvatarRootTransform.transform.position = new Vector3(0, 0, 0);
                                        }
                                        else
                                        {
                                            AnimationMode.BeginSampling();
                                            AnimationClip getFirstAnimationClip(Motion motion)
                                            {
                                                if (motion == null)
                                                {
                                                    return null;
                                                }
                                                if (motion.GetType() == typeof(AnimationClip))
                                                {
                                                    return (AnimationClip)motion;
                                                }
                                                if (motion.GetType() == typeof(BlendTree))
                                                {
                                                    foreach (var child in ((BlendTree)motion).children)
                                                    {
                                                        var clip = getFirstAnimationClip(child.motion);
                                                        if (clip)
                                                        {
                                                            return clip;
                                                        }
                                                    }
                                                }
                                                return null;
                                            }
                                            var firstAnimationClip = getFirstAnimationClip(animation.animationClip);
                                            if (firstAnimationClip)
                                            {
                                                AnimationMode.SampleAnimationClip(ctx.AvatarRootObject, firstAnimationClip, 0);
                                                AnimationMode.EndSampling();
                                            }

                                            ctx.AvatarRootTransform.position = new Vector3(100, 0, 0);
                                            ctx.AvatarRootTransform.LookAt(new Vector3(100, 0, 1));
                                            if (animation.isRotate)
                                            {
                                                ctx.AvatarRootTransform.transform.Rotate(0, animation.rotate, 0);
                                            }

                                            var cameraHeight = ctx.AvatarRootObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position.y;
                                            var cameraDepth = ctx.AvatarRootObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftFoot).position.z;
                                            var cameraDepth2 = ctx.AvatarRootObject.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position.z;
                                            var distance = Mathf.Max(Mathf.Abs(cameraDepth), cameraHeight) + 0.5f;
                                            camera.transform.position = new Vector3(100 - distance, /*cameraHeight + 0.2f*/1, distance + cameraDepth / 2);
                                            camera.transform.LookAt(new Vector3(100, cameraHeight * 0.5f, /*cameraDepth / 2*/(cameraDepth + cameraDepth2) * 0.5f));
                                        }

                                        camera.targetTexture = new RenderTexture(256, 256, 24);
                                        camera.Render();
                                        AnimationMode.StopAnimationMode();

                                        ctx.AvatarRootObject.SetActive(false);
                                        ctx.AvatarRootObject.SetActive(true);

                                        RenderTexture.active = camera.targetTexture;
                                        control.icon = new Texture2D(256, 256, TextureFormat.ARGB32, false);
                                        control.icon.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
                                        control.icon.Apply();
                                    }
                                }
                            }
                        }
                    };

                    if (ctx.AvatarDescriptor.expressionsMenu)
                    {
                        foreach (var control in ctx.AvatarDescriptor.expressionsMenu.controls)
                        {
                            takeIconForControl(control);
                        }
                    }


                    foreach (var posingSystem in posingSystems)
                    {
                        Object.DestroyImmediate(posingSystem);
                    }

                    RenderTexture.active = null;
                    Object.DestroyImmediate(cameraGameObject);

                    // メニューアイテムを作成
                    var runtimeAnimatorController = ctx.AvatarDescriptor.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base].animatorController;
                    if (runtimeAnimatorController == null && runtimeAnimatorController.GetType() != typeof(AnimatorController))
                    {
                        Debug.Log("animatorController == null");
                        return;
                    }
                    var animatorController = (AnimatorController)runtimeAnimatorController;

                    var layer = animatorController.layers.FirstOrDefault(search => search.name == "USSPS_Locomotion");
                    if (layer == null)
                    {
                        Debug.Log("layer == null");
                        return;
                    }

                    var skipBlendTree = new List<BlendTree>();
                    void addFootHeightBlendtree(AnimatorStateMachine stateMachine)
                    {
                        foreach (var childStateMachine in stateMachine.stateMachines)
                        {
                            addFootHeightBlendtree(childStateMachine.stateMachine);
                        }

                        BlendTree getFootHeightBlendtree(BlendTree blendtree, float level)
                        {
                            if (blendtree == null)
                            {
                                return null;
                            }

                            var blendTreeChildren = new List<ChildMotion>();
                            foreach (var childMotion in blendtree.children)
                            {
                                if (childMotion.motion == null)
                                {
                                    continue;
                                }
                                Motion newMotion;
                                if (childMotion.motion.GetType() == typeof(BlendTree))
                                {
                                    newMotion = getFootHeightBlendtree((BlendTree)childMotion.motion, level);
                                }
                                else
                                {
                                    if (childMotion.motion.name.IndexOf("proxy_") == 0)
                                    {
                                        newMotion = childMotion.motion;
                                    }
                                    else
                                    {
                                        var animationClip = (AnimationClip)childMotion.motion;
                                        var newAnimationClip = Object.Instantiate(animationClip);
                                        var clipSetting = AnimationUtility.GetAnimationClipSettings(newAnimationClip);
                                        clipSetting.level += level;
                                        AnimationUtility.SetAnimationClipSettings(newAnimationClip, clipSetting);

                                        newMotion = newAnimationClip;
                                    }
                                }
                                var newChild = childMotion;
                                newChild.motion = newMotion;
                                blendTreeChildren.Add(newChild);
                            }
                            var newBlendtree = new BlendTree
                            {
                                blendType = blendtree.blendType,
                                blendParameter = blendtree.blendParameter,
                                blendParameterY = blendtree.blendParameterY,
                                name = blendtree.name + "_" + level.ToString(),
                                children = blendTreeChildren.ToArray(),
                                useAutomaticThresholds = blendtree.useAutomaticThresholds
                            };
                            if (blendtree.useAutomaticThresholds)
                            {
                                newBlendtree.minThreshold = blendtree.minThreshold;
                                newBlendtree.maxThreshold = blendtree.maxThreshold;
                            }
                            return newBlendtree;
                        }

                        foreach (var state in stateMachine.states)
                        {
                            if (state.state.motion == null)
                            {
                                continue;
                            }
                            if (state.state.motion.GetType() == typeof(AnimationClip))
                            {
                                if (state.state.motion.name.IndexOf("proxy_") != 0)
                                {
                                    var animationClip = (AnimationClip)state.state.motion;
                                    var newAnimationClip = Object.Instantiate(animationClip);
                                    var clipSetting = AnimationUtility.GetAnimationClipSettings(newAnimationClip);
                                    clipSetting.level += 2;
                                    AnimationUtility.SetAnimationClipSettings(newAnimationClip, clipSetting);

                                    // FootHeight用アニメーション
                                    var heightAnimationClip = Object.Instantiate(newAnimationClip);
                                    clipSetting.level -= 4;
                                    AnimationUtility.SetAnimationClipSettings(heightAnimationClip, clipSetting);

                                    // FootHeight用BlendTree作成
                                    var footHeightBlendTree = new BlendTree();
                                    footHeightBlendTree.name = newAnimationClip.name + "_footheight";
                                    footHeightBlendTree.blendParameter = "FootHeight";

                                    footHeightBlendTree.AddChild(newAnimationClip, 0);
                                    footHeightBlendTree.AddChild(heightAnimationClip, 1);

                                    state.state.motion = footHeightBlendTree;
                                }
                            }
                            else
                            {
                                var blendTree = (BlendTree)state.state.motion;

                                var footHeightBlendtreeDown = getFootHeightBlendtree(blendTree, 2);
                                var footHeightBlendtreeUp = getFootHeightBlendtree(blendTree, -2);

                                var footHeightBlendTree = new BlendTree();
                                footHeightBlendTree.name = blendTree.name + "_footheight";
                                footHeightBlendTree.blendParameter = "FootHeight";

                                footHeightBlendTree.AddChild(footHeightBlendtreeDown, 0);
                                footHeightBlendTree.AddChild(footHeightBlendtreeUp, 1);

                                state.state.motion = footHeightBlendTree;
                            }
                        }
                    }
                    addFootHeightBlendtree(layer.stateMachine);

                    // 姿勢決定用パラメータの初期値を設定
                    foreach (var posingSystem in posingSystems)
                    {
                        foreach (var define in posingSystem.defines)
                        {
                            int animationValue = 1;
                            int initialValue = -1;
                            foreach (var animation in define.animations)
                            {
                                if (!animation.enabled)
                                {
                                    continue;
                                }
                                if (animation.initial)
                                {
                                    initialValue = animation.typeParameterValue;
                                }
                                animationValue++;
                            }

                            if (initialValue == -1)
                            {
                                continue;
                            }
                            foreach (var param in ctx.AvatarDescriptor.expressionParameters.parameters)
                            {
                                if (param.name == define.paramName)
                                {
                                    param.defaultValue = initialValue;
                                }
                            }
                        }
                    }

                    // 全てのAnimatorStateのTransitionConditionをチェックして、問題があるものがあれば修正する
                    foreach (var baseLayer in ctx.AvatarDescriptor.baseAnimationLayers)
                    {
                        if (baseLayer.animatorController == null)
                        {
                            continue;
                        }

                        if (baseLayer.animatorController.GetType() != typeof(AnimatorController))
                        {
                            continue;
                        }
                        var baseLayerAnimatorController = (AnimatorController)baseLayer.animatorController;

                        var paramDic = new Dictionary<string, AnimatorControllerParameterType>();
                        foreach (var param in baseLayerAnimatorController.parameters)
                        {
                            paramDic.Add(param.name, param.type);
                        }

                        foreach (var l in baseLayerAnimatorController.layers)
                        {
                            checkTransitionConditionValueType(l.stateMachine, paramDic, animatorController.name, layer.name);
                        }
                    }

                    foreach (var posingSystem in posingSystems)
                    {
                        Object.DestroyImmediate(posingSystem);
                    }
                });
        }

        void checkTransitionConditionValueType(AnimatorStateMachine stateMachine, Dictionary<string, AnimatorControllerParameterType> paramDic, string fileName, string layerName)
        {
            foreach (var transition in stateMachine.entryTransitions)
            {
                foreach (var condition in transition.conditions)
                {
                    if (!paramDic.ContainsKey(condition.parameter))
                    {
                        ErrorReport.ReportError(errorLocalizer, ErrorSeverity.NonFatal, "AnimatorにParameterとして登録されていないパラメータが条件式に使われています", fileName, layerName, condition.parameter);
                        continue;
                    }
                    switch (paramDic[condition.parameter])
                    {
                        case AnimatorControllerParameterType.Bool:
                            if (condition.mode != AnimatorConditionMode.If && condition.mode != AnimatorConditionMode.IfNot)
                            {
                                ErrorReport.ReportError(errorLocalizer, ErrorSeverity.NonFatal, "条件式が間違っているパラメータがあります", fileName, layerName, condition.parameter, typeof(AnimatorControllerParameterType).GetEnumName(paramDic[condition.parameter]), typeof(AnimatorConditionMode).GetEnumName(condition.mode));
                            }
                            break;
                        case AnimatorControllerParameterType.Int:
                            if (condition.mode != AnimatorConditionMode.Equals && condition.mode != AnimatorConditionMode.NotEqual && condition.mode != AnimatorConditionMode.Greater && condition.mode != AnimatorConditionMode.Less)
                            {
                                ErrorReport.ReportError(errorLocalizer, ErrorSeverity.NonFatal, "条件式が間違っているパラメータがあります", fileName, layerName, condition.parameter, typeof(AnimatorControllerParameterType).GetEnumName(paramDic[condition.parameter]), typeof(AnimatorConditionMode).GetEnumName(condition.mode));
                            }
                            break;
                        case AnimatorControllerParameterType.Float:
                            if (condition.mode != AnimatorConditionMode.Greater && condition.mode != AnimatorConditionMode.Less)
                            {
                                ErrorReport.ReportError(errorLocalizer, ErrorSeverity.NonFatal, "条件式が間違っているパラメータがあります", fileName, layerName, condition.parameter, typeof(AnimatorControllerParameterType).GetEnumName(paramDic[condition.parameter]), typeof(AnimatorConditionMode).GetEnumName(condition.mode));
                            }
                            break;
                        case AnimatorControllerParameterType.Trigger:
                            break;
                    }
                }
            }

            foreach (var transition in stateMachine.anyStateTransitions)
            {
                foreach (var condition in transition.conditions)
                {
                    if (!paramDic.ContainsKey(condition.parameter))
                    {
                        ErrorReport.ReportError(errorLocalizer, ErrorSeverity.NonFatal, "AnimatorにParameterとして登録されていないパラメータが条件式に使われています", fileName, layerName, condition.parameter);
                        continue;
                    }
                    switch (paramDic[condition.parameter])
                    {
                        case AnimatorControllerParameterType.Bool:
                            if (condition.mode != AnimatorConditionMode.If && condition.mode != AnimatorConditionMode.IfNot)
                            {
                                ErrorReport.ReportError(errorLocalizer, ErrorSeverity.NonFatal, "条件式が間違っているパラメータがあります", fileName, layerName, condition.parameter, typeof(AnimatorControllerParameterType).GetEnumName(paramDic[condition.parameter]), typeof(AnimatorConditionMode).GetEnumName(condition.mode));
                            }
                            break;
                        case AnimatorControllerParameterType.Int:
                            if (condition.mode != AnimatorConditionMode.Equals && condition.mode != AnimatorConditionMode.NotEqual && condition.mode != AnimatorConditionMode.Greater && condition.mode != AnimatorConditionMode.Less)
                            {
                                ErrorReport.ReportError(errorLocalizer, ErrorSeverity.NonFatal, "条件式が間違っているパラメータがあります", fileName, layerName, condition.parameter, typeof(AnimatorControllerParameterType).GetEnumName(paramDic[condition.parameter]), typeof(AnimatorConditionMode).GetEnumName(condition.mode));
                            }
                            break;
                        case AnimatorControllerParameterType.Float:
                            if (condition.mode != AnimatorConditionMode.Greater && condition.mode != AnimatorConditionMode.Less)
                            {
                                ErrorReport.ReportError(errorLocalizer, ErrorSeverity.NonFatal, "条件式が間違っているパラメータがあります", fileName, layerName, condition.parameter, typeof(AnimatorControllerParameterType).GetEnumName(paramDic[condition.parameter]), typeof(AnimatorConditionMode).GetEnumName(condition.mode));
                            }
                            break;
                        case AnimatorControllerParameterType.Trigger:
                            break;
                    }
                }
            }

            foreach (var state in stateMachine.states)
            {
                foreach (var transition in state.state.transitions)
                {
                    foreach (var condition in transition.conditions)
                    {
                        if (!paramDic.ContainsKey(condition.parameter))
                        {
                            ErrorReport.ReportError(errorLocalizer, ErrorSeverity.NonFatal, "AnimatorにParameterとして登録されていないパラメータが条件式に使われています", fileName, layerName, condition.parameter);
                            continue;
                        }
                        switch (paramDic[condition.parameter])
                        {
                            case AnimatorControllerParameterType.Bool:
                                if (condition.mode != AnimatorConditionMode.If && condition.mode != AnimatorConditionMode.IfNot)
                                {
                                    ErrorReport.ReportError(errorLocalizer, ErrorSeverity.NonFatal, "条件式が間違っているパラメータがあります", fileName, layerName, condition.parameter, typeof(AnimatorControllerParameterType).GetEnumName(paramDic[condition.parameter]), typeof(AnimatorConditionMode).GetEnumName(condition.mode));
                                }
                                break;
                            case AnimatorControllerParameterType.Int:
                                if (condition.mode != AnimatorConditionMode.Equals && condition.mode != AnimatorConditionMode.NotEqual && condition.mode != AnimatorConditionMode.Greater && condition.mode != AnimatorConditionMode.Less)
                                {
                                    ErrorReport.ReportError(errorLocalizer, ErrorSeverity.NonFatal, "条件式が間違っているパラメータがあります", fileName, layerName, condition.parameter, typeof(AnimatorControllerParameterType).GetEnumName(paramDic[condition.parameter]), typeof(AnimatorConditionMode).GetEnumName(condition.mode));
                                }
                                break;
                            case AnimatorControllerParameterType.Float:
                                if (condition.mode != AnimatorConditionMode.Greater && condition.mode != AnimatorConditionMode.Less)
                                {
                                    ErrorReport.ReportError(errorLocalizer, ErrorSeverity.NonFatal, "条件式が間違っているパラメータがあります", fileName, layerName, condition.parameter, typeof(AnimatorControllerParameterType).GetEnumName(paramDic[condition.parameter]), typeof(AnimatorConditionMode).GetEnumName(condition.mode));
                                }
                                break;
                            case AnimatorControllerParameterType.Trigger:
                                break;
                        }
                    }
                }
            }

            foreach (var subStateMachine in stateMachine.stateMachines)
            {
                checkTransitionConditionValueType(subStateMachine.stateMachine, paramDic, fileName, layerName);
            }
        }
    }
}

#endif
