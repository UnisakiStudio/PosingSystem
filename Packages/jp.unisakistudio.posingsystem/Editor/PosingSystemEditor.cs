using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Linq;
using Microsoft.Win32;
using jp.unisakistudio.posingsystem;
using nadena.dev.modular_avatar.core;

namespace jp.unisakistudio.posingsystemeditor
{

    [CustomEditor(typeof(PosingSystem))]
    public class PosingSystemEditor : Editor
    {
        private List<ReorderableList> reorderableLists = new List<ReorderableList>();
        List<string> existProducts;
        Texture2D exMenuBackground = null;

        const string REGKEY = @"SOFTWARE\UnisakiStudio";
        const string APPKEY = "posingsystem";

        public override void OnInspectorGUI()
        {
            PosingSystem posingSystem = target as PosingSystem;

            /*
             * このコメント分を含むここから先の処理はゆにさきポーズシステムを含む商品をゆにさきスタジオから購入した場合に変更することを許可します。
             * つまり購入者はライセンスにまつわるこの先のソースコードを削除して再配布を行うことができます。
             * 逆に、購入をせずにGithubなどからソースコードを取得しただけの場合、このライセンスに関するソースコードに手を加えることは許可しません。
             */
            if (!posingSystem.isPosingSystemLicensed)
            {
                var regKey = Registry.CurrentUser.CreateSubKey(REGKEY);
                var regValue = (string)regKey.GetValue(APPKEY);

                if (regValue == "licensed")
                {
                    posingSystem.isPosingSystemLicensed = true;
                }
                else
                {
                    EditorGUILayout.LabelField("ゆにさきポーズシステム", new GUIStyle() { fontStyle = FontStyle.Bold, fontSize = 20, }, GUILayout.Height(30));

                    EditorGUILayout.HelpBox("このコンピュータにはゆにさきポーズシステムの使用が許諾されていません。Boothのショップから可愛いポーズツールを購入して、コンピュータにライセンスをインストールしてください", MessageType.Error);
                    if (EditorGUILayout.LinkButton("可愛いポーズツール(Booth)"))
                    {
                        Application.OpenURL("https://yunisaki.booth.pm/items/5479202");
                    }
                    return;
                }
            }
            /*
             * ライセンス処理ここまで
             */

            if (posingSystem.developmentMode)
            {
                base.OnInspectorGUI();
                return;
            }

            if (exMenuBackground == null)
            {
                exMenuBackground = AssetDatabase.LoadAssetAtPath<Texture2D>(AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets("ExMenuBackground t:Texture")[0]));
            }

            EditorGUILayout.LabelField(posingSystem.settingName, new GUIStyle() { fontStyle = FontStyle.Bold, fontSize = 20, }, GUILayout.Height(30));

            EditorGUILayout.HelpBox("ModularAvatarで服を着せているとポーズサムネイルの洋服が外れて見えますが、アバターアップロード時に再撮影されて正しい画像がメニューに設定されるのでご安心ください", MessageType.Info);

            if (posingSystem.developmentMode != EditorGUILayout.Toggle("開発モード", posingSystem.developmentMode))
            {
                reorderableLists.Clear();
                posingSystem.developmentMode = !posingSystem.developmentMode;
            }

            Transform avatar = posingSystem.transform;
            VRC.SDK3.Avatars.Components.VRCAvatarDescriptor avatarDescriptor = null;
            while (avatar != null)
            {
                if (avatar.TryGetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>(out avatarDescriptor))
                {
                    break;
                }
                avatar = avatar.parent;
            }
            if (avatarDescriptor == null)
            {
                EditorGUILayout.HelpBox("オブジェクトがVRC用のアバターオブジェクトの中に入っていません。このオブジェクトはVRCAvatarDescriptorコンポーネントの付いたオブジェクトの中に配置してください", MessageType.Error);
                return;
            }
            if (existProducts == null)
            {
                if (avatar != null) {
                    existProducts = CheckExistProduct(avatarDescriptor);
                }
            }
            if (existProducts != null)
            {
                foreach (var existProduct in existProducts)
                {
                    EditorGUILayout.HelpBox(existProduct + "の設定がアバターに残っています！不具合が発生する可能性があるので、自動で設定を取り除く機能を使用してください。また、アバター購入時にBaseLayerに設定されていたLocomotion用のAnimatorControllerがある場合は、恐れ入りますが手動で復仇してお使いください。", MessageType.Error);
                    if (GUILayout.Button(existProduct + "の設定を取り除く"))
                    {
                        RemoveExistProduct(avatarDescriptor, existProduct);
                        existProducts = null;
                    }
                }
            }

            if (avatarDescriptor.autoFootsteps)
            {
                EditorGUILayout.HelpBox("アバター設定の「Use Auto-Footsteps for 3 and 4 point tracking」がオンになっています。この設定がオンだと、アバターがゲーム内で自動的に足踏みをしてしまい、姿勢が崩れる可能性があるため、オフにすることが推奨されます", MessageType.Warning);
                if (GUILayout.Button("「Use Auto-Footsteps for 3 and 4 point tracking」をオフにする"))
                {
                    Undo.RecordObject(avatarDescriptor, "Disable auto footsteps");
                    avatarDescriptor.autoFootsteps = false;
                    EditorUtility.SetDirty(avatarDescriptor);
                }
            }

            var avatarAnimator = avatar.GetComponent<Animator>();
            if (avatarAnimator == null)
            {
                EditorGUILayout.HelpBox("アバターにAnimatorが設定されていません。このアバターではこのツールは使えません", MessageType.Error);
                return;
            }

            if (avatarAnimator.avatar == null)
            {
                EditorGUILayout.HelpBox("アバターのAnimatorにAvatarが設定されていません。このアバターではこのツールは使えません", MessageType.Error);
                return;
            }

            if (!avatarAnimator.isHuman)
            {
                EditorGUILayout.HelpBox("このアバターは人型ではありません。アバターのAnimatorにHumanoid型のAvatarが設定されていない場合はこのツールは使えません。", MessageType.Error);
                return;
            }

            EditorGUILayout.BeginVertical(GUI.skin.box);
            EditorGUILayout.LabelField("設定", new GUIStyle() { fontStyle = FontStyle.Bold, });
            var isIconDisabled = EditorGUILayout.Toggle("姿勢アイコン無しモード（Quest等）", posingSystem.isIconDisabled);
            if (isIconDisabled != posingSystem.isIconDisabled)
            {
                Undo.RecordObject(posingSystem, "Disable icon");
                posingSystem.isIconDisabled = isIconDisabled;
                EditorUtility.SetDirty(posingSystem);
            }
            EditorGUILayout.EndVertical();

            TakeScreenshot(posingSystem);
            serializedObject.Update();

            for (int i = 0; i < posingSystem.defines.Count; i++)
            {
                var define = posingSystem.defines[i];
                EditorGUILayout.BeginVertical();

                var animationsProperty = serializedObject.FindProperty("defines").GetArrayElementAtIndex(i).FindPropertyRelative("animations");
                ReorderableList reorderableList = null;
                if (reorderableLists.Count <= i)
                {
                    reorderableList = new ReorderableList(base.serializedObject, animationsProperty);
                    reorderableList.drawElementCallback += (Rect rect, int index, bool selected, bool focused) =>
                    {
                        SerializedProperty property = reorderableList.serializedProperty.GetArrayElementAtIndex(index);
                        EditorGUI.PropertyField(rect, property, GUIContent.none);
                    };
                    reorderableList.drawHeaderCallback += rect =>
                    {
                        if (define.icon)
                        {
                            GUI.Box(new Rect(rect.x - 6, rect.y, rect.width + 12, rect.height + 1), new GUIContent());
                            GUI.DrawTexture(new Rect(rect.x - 3, rect.y + 1, 36, 36), exMenuBackground, ScaleMode.ScaleToFit);
                            GUI.DrawTexture(new Rect(rect.x - 1, rect.y + 1, 32, 32), define.icon, ScaleMode.ScaleToFit);
                            EditorGUI.LabelField(new Rect(rect.x + 40, rect.y, rect.width - rect.x - 40, 20), define.menuName + define.description);
                        }
                        else
                        {
                            EditorGUI.LabelField(rect, define.menuName + define.description);
                        }
                    };
                    if (define.icon != null) {
                        reorderableList.headerHeight = 40;
                    }
                    else
                    {
                        reorderableList.headerHeight = EditorGUIUtility.singleLineHeight;
                    }
                    reorderableList.elementHeight = EditorGUIUtility.singleLineHeight * 5 + 5;
                    reorderableLists.Add(reorderableList);
                }
                else
                {
                    reorderableList = reorderableLists[i];
                }

                reorderableList.DoLayoutList();
                serializedObject.ApplyModifiedProperties();

                PosingSystem.AnimationDefine changedAnimation = null;
                foreach (var animation in define.animations)
                {
                    if (animation.initial != animation.initialSet)
                    {
                        changedAnimation = animation;
                    }
                }
                if (changedAnimation != null)
                {
                    foreach (var avatarPosingSystem in avatar.GetComponentsInChildren<PosingSystem>())
                    {
                        foreach (var posingDefine in avatarPosingSystem.defines)
                        {
                            if (posingDefine.paramName != define.paramName)
                            {
                                continue;
                            }
                            foreach (var animation in posingDefine.animations)
                            {
                                if (animation.initial && changedAnimation != animation)
                                {
                                    animation.initial = false;
                                    animation.initialSet = false;
                                    animation.previewImage = null;
                                }
                                else if (changedAnimation == animation)
                                {
                                    animation.initial = animation.initialSet;
                                    animation.previewImage = null;
                                }
                            }
                        }
                    }
                }

                EditorGUILayout.EndVertical();
            }


            serializedObject.ApplyModifiedProperties();

            if (GUILayout.Button("サムネ更新"))
            {
                TakeScreenshot(posingSystem, true);
            }
        }
        public void TakeScreenshot(PosingSystem posingSystem, bool force = false)
        {
            var avatarTransform = posingSystem.transform;
            while (avatarTransform != null && avatarTransform.GetComponent<VRC.SDK3.Avatars.Components.VRCAvatarDescriptor>() == null)
            {
                avatarTransform = avatarTransform.parent;
            }
            if (avatarTransform == null)
            {
                return;
            }
            bool skip = true;
            for (int i = 0; i < posingSystem.defines.Count; i++)
            {
                for (int j = 0; j < posingSystem.defines[i].animations.Count; j++)
                {
                    if (force == false && posingSystem.defines[i].animations[j].previewImage != null)
                    {
                        continue;
                    }
                    if (force == false && posingSystem.defines[i].animations[j].isCustomIcon)
                    {
                        continue;
                    }
                    skip = false;
                }
            }
            if (skip)
            {
                return;
            }

            var clone = Instantiate(avatarTransform.gameObject);

            var cameraGameObject = new GameObject();
            var camera = cameraGameObject.AddComponent<Camera>();
            camera.fieldOfView = 30;
            camera.clearFlags = CameraClearFlags.Color;

            clone.transform.position = new Vector3(0, 0, 0);
            clone.transform.LookAt(new Vector3(0, 0, 1));
            for (int i = 0; i < posingSystem.defines.Count; i++)
            {
                for (int j = 0; j < posingSystem.defines[i].animations.Count; j++)
                {
                    var animation = posingSystem.defines[i].animations[j];
                    if (force == false && animation.previewImage != null)
                    {
                        continue;
                    }
                    if (force == false && posingSystem.defines[i].animations[j].isCustomIcon)
                    {
                        continue;
                    }

                    AnimationMode.StartAnimationMode();
                    if (animation.animationClip == null)
                    {
                        clone.transform.position = new Vector3(0, 0, 0);
                    }
                    else
                    {
                        AnimationMode.BeginSampling();
                        AnimationClip getFirstAnimationClip(Motion motion)
                        {
                            if (motion.GetType() == typeof(AnimationClip))
                            {
                                return (AnimationClip)motion;
                            }
                            if (motion.GetType() == typeof(UnityEditor.Animations.BlendTree))
                            {
                                foreach (var child in ((UnityEditor.Animations.BlendTree)motion).children)
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
                        AnimationMode.SampleAnimationClip(clone, getFirstAnimationClip(animation.animationClip), 0);

                        clone.transform.position = new Vector3(100, 0, 0);
                        clone.transform.LookAt(new Vector3(100, 0, 1));
                        if (animation.isRotate)
                        {
                            clone.transform.Rotate(0, animation.rotate, 0);
                        }
                        foreach (var mergeArmature in clone.GetComponentsInChildren<ModularAvatarMergeArmature>())
                        {
                            mergeArmature.transform.position = new Vector3(200, 0, 0);
                        }
                        AnimationMode.EndSampling();

                        var cameraHeight = clone.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position.y;
                        var cameraDepth = clone.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.LeftFoot).position.z;
                        var cameraDepth2 = clone.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Head).position.z;
                        var cameraBase = clone.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Hips).position.z;
                        var distance = Mathf.Max(Mathf.Abs(cameraDepth - cameraBase), cameraHeight) + 0.5f;
                        camera.transform.position = new Vector3(100 - distance, /*cameraHeight + 0.2f*/1, cameraBase + distance + (cameraDepth + cameraDepth2) * 0.5f / 2);
                        camera.transform.LookAt(new Vector3(100, cameraHeight * 0.5f, /*cameraDepth / 2*/(cameraDepth + cameraDepth2) * 0.5f));
                    }

                    if (!animation.enabled || animation.animationClip == null)
                    {
                        camera.backgroundColor = new Color(0.3f, 0.3f, 0.3f, 1);
                    }
                    else if (animation.initial)
                    {
                        camera.backgroundColor = new Color(0.3f, 0.3f, 0.8f, 1);
                    }
                    else
                    {
                        camera.backgroundColor = new Color(0.7f, 0.7f, 0.7f, 1);
                    }

                    camera.targetTexture = new RenderTexture(300, 300, 24);
                    camera.Render();
                    AnimationMode.StopAnimationMode();

                    clone.SetActive(false);
                    clone.SetActive(true);

                    Texture2D texture2D = new Texture2D(300, 300, TextureFormat.ARGB32, false);
                    texture2D.hideFlags = HideFlags.DontSaveInEditor;
                    RenderTexture.active = camera.targetTexture;
                    texture2D.ReadPixels(new Rect(0, 0, 300, 300), 0, 0);
                    texture2D.Apply();

                    animation.previewImage = texture2D;
                }
            }
            RenderTexture.active = null;
            Object.DestroyImmediate(cameraGameObject);
            Object.DestroyImmediate(clone.gameObject);
        }

        List<(string name, List<string> animatorControllerNames, List<string> checkExpressionParametersNames, List<string> expressionParametersNames, List<string> expressionsMenuNames, List<string> prefabsNames)> productDefines = new List<(string name, List<string> animatorControllerNames, List<string> checkExpressionParametersNames, List<string> expressionParametersNames, List<string> expressionsMenuNames, List<string> prefabsNames)>
        {
            (
                "可愛い座りツール",
                new List<string>
                {
                    "KawaiiSitting_Locomotion",
                    "SleepTogether_KawaiiSitting_Locomotion",
                    "VirtualLoveBoy_KawaiiSitting_Locomotion",
                    "VirtualLoveGirl_KawaiiSitting_Locomotion",
                    "SleepTogether_VirtualLoveBoy_KawaiiSitting_Locomotion",
                    "SleepTogether_VirtualLoveGirl_KawaiiSitting_Locomotion",
                },
                 new List<string>
                {
                    "SitShallow",
                    "SitDeep",
                },
                 new List<string>
                {
                    "SitShallow",
                    "SitDeep",
                    "SitSleep",
                    "LocomotionLock",
                    "HeadTrackingLock",
                    "HandTrackingLock",
                    "FootHeight",
                },
                new List<string>
                {
                    "KawaiiSitting_ExpressionsMenu",
                    "KawaiiSitting_FootHeight_ExpressionsMenu",
                },
                new List<string>
                {
                }
            ),

            (
                "三点だいしゅきツール",
                new List<string>
                {
                    "VirtualLoveBoy_Locomotion",
                    "VirtualLoveBoy_KawaiiSitting_Locomotion",
                    "VirtualLoveGirl_Locomotion",
                    "VirtualLoveGirl_KawaiiSitting_Locomotion",
                    "SleepTogether_VirtualLoveBoy_Locomotion",
                    "SleepTogether_VirtualLoveBoy_KawaiiSitting_Locomotion",
                    "SleepTogether_VirtualLoveGirl_Locomotion",
                    "SleepTogether_VirtualLoveGirl_KawaiiSitting_Locomotion",
                },
                 new List<string>
                {
                    "VirtualLoveType",
                },
                 new List<string>
                {
                    "VirtualLoveType",
                    "LocomotionLock",
                    "HeadTrackingLock",
                    "HandTrackingLock",
                    "FootHeight",
                },
                new List<string>
                {
                    "VirtualLove_ExpressionsMenu",
                    "VirtualLove_FootHeight_ExpressionsMenu",
                },
                new List<string>
                {
                }
            ),

            (
                "添い寝ツール",
                new List<string>
                {
                    "SleepTogether_Locomotion",
                    "SleepTogether_KawaiiSitting_Locomotion",
                    "SleepTogether_VirtualLoveBoy_Locomotion",
                    "SleepTogether_VirtualLoveBoy_KawaiiSitting_Locomotion",
                    "SleepTogether_VirtualLoveGirl_Locomotion",
                    "SleepTogether_VirtualLoveGirl_KawaiiSitting_Locomotion",
                },
                 new List<string>
                {
                    "SleepTogether",
                    "RightStationType",
                    "LeftStationType",
                    "BedHeight",
                },
                 new List<string>
                {
                    "SleepTogether",
                    "RightStationType",
                    "LeftStationType",
                    "BedHeight",
                    "SitSleep",
                    "LocomotionLock",
                    "HeadTrackingLock",
                    "HandTrackingLock",
                    "FootHeight",
                },
                new List<string>
                {
                    "SleepTogether_ExpressionsMenu",
                    "SleepTogether_ExpressionsMenu_Right",
                    "SleepTogether_ExpressionsMenu_Left",
                },
                new List<string>
                {
                    "SleepTogether",
                    "SleepTogether_Right",
                    "SleepTogether_Left",
                }
            ),

            (
                "ごろ寝システム",
                new List<string>
                {
                    "SupineLocomotion",
                    "SupineLocomotion_ex",
                },
                 new List<string>
                {
                    "VRCSupineExAdjust",
                    "VRCSupineExAdjusting",
                    "VRCFootAnchorHandSwitchable",
                    "RootHeight",
                    "SetRootHeight",
                },
                 new List<string>
                {
                    "VRCSupineExAdjust",
                    "VRCSupineExAdjusting",
                    "VRCFootAnchorHandSwitchable",
                    "RootHeight",
                    "SetRootHeight",
                },
                new List<string>
                {
                    "SupineMenu",
                    "SupineMenu_ex",
                },
                new List<string>
                {
                }
            ),
        };

        List<string> CheckExistProduct(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor avatar)
        {
            var existProducts = new List<string>();

            foreach (var productDefine in productDefines)
            {
                bool isExistProduct = false;
                // AnimatorControllerが商品のか調べる
                var animatorController = avatar.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base];
                if (animatorController.animatorController != null)
                {
                    foreach (var animatoControllerName in productDefine.animatorControllerNames)
                    {
                        if (animatorController.animatorController.name.Contains(animatoControllerName))
                        {
                            isExistProduct = true;
                        }
                    }
                }

                var productMenuGuids = new List<string>();
                foreach (var menuName in productDefine.expressionsMenuNames)
                {
                    productMenuGuids.AddRange(AssetDatabase.FindAssets(menuName));
                }
                // 再起で商品のメニューを使用しているか調べる
                bool isKawaiiSittingMenu(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu menu)
                {
                    if (menu == null)
                    {
                        return false;
                    }
                    foreach (var menuName in productDefine.expressionsMenuNames)
                    {
                        if (productMenuGuids.IndexOf(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(menu))) != -1)
                        {
                            return true;
                        }

                    }
                    foreach (var control in menu.controls)
                    {
                        if (control.type != VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu)
                        {
                            continue;
                        }
                        if (isKawaiiSittingMenu(control.subMenu))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                if (isKawaiiSittingMenu(avatar.expressionsMenu))
                {
                    isExistProduct = true;
                }

                // パラメータに商品のがあるか調べる
                if (avatar.expressionParameters)
                {
                    foreach (var parameter in avatar.expressionParameters.parameters)
                    {
                        if (productDefine.checkExpressionParametersNames.IndexOf(parameter.name) != -1)
                        {
                            isExistProduct = true;
                        }
                    }
                }
                foreach (var prefabsName in productDefine.prefabsNames)
                {
                    Transform prefabsTransform = avatar.transform.Find(prefabsName);
                    if (prefabsTransform != null)
                    {
                        isExistProduct = true;
                    }
                }

                if (isExistProduct)
                {
                    existProducts.Add(productDefine.name);
                }
            }

            return existProducts;
        }

        void RemoveExistProduct(VRC.SDK3.Avatars.Components.VRCAvatarDescriptor avatar, string productName)
        {
            var productDefine = productDefines.First(define => define.name == productName);

            // AnimatorControllerが商品のか調べる
            var animatorController = avatar.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base];
            if (animatorController.animatorController != null)
            {
                foreach (var animatoControllerName in productDefine.animatorControllerNames)
                {
                    if (AssetDatabase.FindAssets(animatoControllerName).ToList().IndexOf(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(animatorController.animatorController))) != -1)
                    {
                        avatar.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base].animatorController = null;
                        avatar.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base].isDefault = true;
                        avatar.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base].isEnabled = true;
                        EditorUtility.SetDirty(avatar);
                        break;
                    }
                }
            }

            // 再起で商品のメニューを使用しているか調べる
            void removeKawaiiSittingMenu(VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu menu)
            {
                if (menu == null)
                {
                    return;
                }
                for (var i=menu.controls.Count -1; i>=0; i--)
                {
                    var control = menu.controls[i];
                    if (control.type != VRC.SDK3.Avatars.ScriptableObjects.VRCExpressionsMenu.Control.ControlType.SubMenu)
                    {
                        continue;
                    }
                    bool isKawaiiSittingMenu = false;
                    foreach (var menuName in productDefine.expressionsMenuNames)
                    {
                        if (AssetDatabase.FindAssets(menuName).ToList().IndexOf(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(control.subMenu))) != -1)
                        {
                            menu.controls.RemoveAt(i);
                            isKawaiiSittingMenu = true;
                            EditorUtility.SetDirty(menu);
                            break;
                        }
                    }
                    if (!isKawaiiSittingMenu)
                    {
                        removeKawaiiSittingMenu(control.subMenu);
                    }
                }
            }
            removeKawaiiSittingMenu(avatar.expressionsMenu);

            // パラメータに商品のがあるか調べる
            avatar.expressionParameters.parameters = avatar.expressionParameters.parameters.Where((parameter) =>
            {
                return productDefine.expressionParametersNames.IndexOf(parameter.name) == -1;
            }).ToArray();
            EditorUtility.SetDirty(avatar.expressionParameters);

            // Prefabsを削除
            foreach (var prefabsName in productDefine.prefabsNames)
            {
                Transform prefabsTransform;
                while ((prefabsTransform = avatar.transform.Find(prefabsName)) != null) {
                    GameObject.DestroyImmediate(prefabsTransform.gameObject);
                }
            }

            AssetDatabase.SaveAssets();
        }
    }

}
