using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor
{
    internal static class PosingSystemAutoPoseEditorGui
    {
        private static readonly HumanBodyBones[] TargetBones =
            PosingSystemAutoPoseAdjustmentUtility.SupportedTargetBones.ToArray();
        private static readonly GUIContent[] TargetBoneLabels = TargetBones
            .Select(bone => new GUIContent(GetJapaneseBoneName(bone)))
            .ToArray();

        public static HumanBodyBones TargetBonePopup(GUIContent label, HumanBodyBones current)
        {
            var index = Array.IndexOf(TargetBones, current);
            if (index < 0)
            {
                index = Math.Max(0, Array.IndexOf(TargetBones, HumanBodyBones.Hips));
            }
            var selected = EditorGUILayout.Popup(label, index, TargetBoneLabels);
            return TargetBones[Mathf.Clamp(selected, 0, TargetBones.Length - 1)];
        }

        private static string GetJapaneseBoneName(HumanBodyBones bone)
        {
            return bone switch
            {
                HumanBodyBones.Hips => "腰 (Hips)",
                HumanBodyBones.Spine => "背骨 (Spine)",
                HumanBodyBones.Chest => "胸 (Chest)",
                HumanBodyBones.UpperChest => "上胸 (Upper Chest)",
                HumanBodyBones.Neck => "首 (Neck)",
                HumanBodyBones.Head => "頭 (Head)",
                HumanBodyBones.LeftShoulder => "左肩",
                HumanBodyBones.LeftUpperArm => "左上腕",
                HumanBodyBones.LeftLowerArm => "左ひじ下",
                HumanBodyBones.LeftHand => "左手",
                HumanBodyBones.RightShoulder => "右肩",
                HumanBodyBones.RightUpperArm => "右上腕",
                HumanBodyBones.RightLowerArm => "右ひじ下",
                HumanBodyBones.RightHand => "右手",
                HumanBodyBones.LeftUpperLeg => "左太もも",
                HumanBodyBones.LeftLowerLeg => "左ひざ下",
                HumanBodyBones.LeftFoot => "左足",
                HumanBodyBones.LeftToes => "左つま先",
                HumanBodyBones.RightUpperLeg => "右太もも",
                HumanBodyBones.RightLowerLeg => "右ひざ下",
                HumanBodyBones.RightFoot => "右足",
                HumanBodyBones.RightToes => "右つま先",
                HumanBodyBones.LeftEye => "左目",
                HumanBodyBones.RightEye => "右目",
                HumanBodyBones.Jaw => "あご",
                _ => ObjectNames.NicifyVariableName(bone.ToString()),
            };
        }
    }

    [CustomEditor(typeof(PosingSystemAutoPoseCatalog))]
    public sealed class PosingSystemAutoPoseCatalogEditor : Editor
    {
        private SerializedProperty _formatVersion;
        private SerializedProperty _productName;
        private SerializedProperty _productionReady;
        private SerializedProperty _poses;

        private void OnEnable()
        {
            _formatVersion = serializedObject.FindProperty("formatVersion");
            _productName = serializedObject.FindProperty("productName");
            _productionReady = serializedObject.FindProperty("productionReady");
            _poses = serializedObject.FindProperty("poses");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "商品に収録する各姿勢について、手や足をどこへ置くかを記録する開発用データです。" +
                "アバターごとの設定ではなく、1つの商品につき1ファイルを作成します。",
                MessageType.Info);

            EditorGUILayout.PropertyField(_productName, new GUIContent(
                "商品名",
                "このIK設定を使用する商品名です。"));
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_formatVersion, new GUIContent(
                    "データ形式のバージョン",
                    "内部的な互換性管理に使います。通常は変更しません。"));
            }

            EditorGUILayout.PropertyField(_productionReady, new GUIContent(
                "公開用として有効",
                "有効にすると従来の調整Animationを無視し、このIK設定から自動調整Animationを生成します。"));

            if (_productionReady.boolValue)
            {
                EditorGUILayout.HelpBox(
                    "公開用として有効です。カタログを参照する商品では、従来の姿勢別調整Animationとアバター固有補正が使用されません。設定漏れの姿勢がないことを確認してください。",
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "現在は制作中です。商品Prefabから参照されていても、従来の調整Animationが引き続き使用されます。",
                    MessageType.None);
            }

            EditorGUILayout.Space();
            _poses.isExpanded = EditorGUILayout.Foldout(
                _poses.isExpanded,
                $"姿勢別IK設定一覧（{_poses.arraySize}件）",
                true);
            if (_poses.isExpanded)
            {
                EditorGUI.indentLevel++;
                for (var poseIndex = 0; poseIndex < _poses.arraySize; poseIndex++)
                {
                    DrawPose(_poses.GetArrayElementAtIndex(poseIndex), poseIndex);
                }

                if (GUILayout.Button("姿勢設定を追加"))
                {
                    AddPose();
                }
                EditorGUI.indentLevel--;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawPose(SerializedProperty pose, int poseIndex)
        {
            var poseKey = pose.FindPropertyRelative("poseKey");
            var sourceAnimation = pose.FindPropertyRelative("sourceAnimation");
            var solveIterations = pose.FindPropertyRelative("solveIterations");
            var limbRules = pose.FindPropertyRelative("limbRules");

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var title = string.IsNullOrEmpty(poseKey.stringValue)
                        ? $"姿勢 {poseIndex + 1}"
                        : poseKey.stringValue;
                    pose.isExpanded = EditorGUILayout.Foldout(pose.isExpanded, title, true);
                    if (GUILayout.Button("削除", GUILayout.Width(48f)))
                    {
                        _poses.DeleteArrayElementAtIndex(poseIndex);
                        GUIUtility.ExitGUI();
                    }
                }

                if (!pose.isExpanded)
                {
                    return;
                }

                EditorGUILayout.PropertyField(poseKey, new GUIContent(
                    "姿勢の識別キー",
                    "通常は「メニュー名_姿勢名」です。調整ウィンドウから追加すると自動入力されます。"));
                EditorGUILayout.PropertyField(sourceAnimation, new GUIContent(
                    "元の姿勢Animation",
                    "IKで補正する前の基準姿勢です。"));
                EditorGUILayout.PropertyField(solveIterations, new GUIContent(
                    "計算の反復回数",
                    "複数の手足を解き直す回数です。通常は2回です。"));

                limbRules.isExpanded = EditorGUILayout.Foldout(
                    limbRules.isExpanded,
                    $"手足のIKルール（{limbRules.arraySize}件）",
                    true);
                if (limbRules.isExpanded)
                {
                    EditorGUI.indentLevel++;
                    for (var ruleIndex = 0; ruleIndex < limbRules.arraySize; ruleIndex++)
                    {
                        DrawRule(limbRules, ruleIndex);
                    }
                    if (GUILayout.Button("手足のIKルールを追加"))
                    {
                        AddRule(limbRules);
                    }
                    EditorGUI.indentLevel--;
                }
            }
        }

        private static void DrawRule(SerializedProperty rules, int ruleIndex)
        {
            var rule = rules.GetArrayElementAtIndex(ruleIndex);
            var label = rule.FindPropertyRelative("label");
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var title = string.IsNullOrEmpty(label.stringValue)
                        ? $"IKルール {ruleIndex + 1}"
                        : label.stringValue;
                    rule.isExpanded = EditorGUILayout.Foldout(rule.isExpanded, title, true);
                    if (GUILayout.Button("削除", GUILayout.Width(48f)))
                    {
                        rules.DeleteArrayElementAtIndex(ruleIndex);
                        GUIUtility.ExitGUI();
                    }
                }

                if (!rule.isExpanded)
                {
                    return;
                }

                EditorGUILayout.PropertyField(rule.FindPropertyRelative("enabled"), new GUIContent("このルールを使用"));
                EditorGUILayout.PropertyField(label, new GUIContent("ルール名", "例: 右手を腰へ、左手を床へ"));
                EditorGUILayout.PropertyField(rule.FindPropertyRelative("limb"), new GUIContent("動かす手足"));
                EditorGUILayout.PropertyField(rule.FindPropertyRelative("solveOrder"), new GUIContent("計算する順番"));

                var targetSpace = rule.FindPropertyRelative("targetSpace");
                EditorGUILayout.PropertyField(targetSpace, new GUIContent(
                    "置き場所の基準",
                    "身体のボーン、アバター全体、床のいずれを基準にするか選びます。"));
                if ((PosingSystemAutoPoseTargetSpace)targetSpace.enumValueIndex ==
                    PosingSystemAutoPoseTargetSpace.BodyBone)
                {
                    var targetBone = rule.FindPropertyRelative("targetBone");
                    EditorGUI.BeginChangeCheck();
                    var selectedTargetBone = PosingSystemAutoPoseEditorGui.TargetBonePopup(
                        new GUIContent(
                            "基準にする身体ボーン",
                            "頭、腰、膝など、手足の置き場所を測る基準です。指ボーンは選択肢に含みません。"),
                        (HumanBodyBones)targetBone.intValue);
                    if (EditorGUI.EndChangeCheck())
                    {
                        targetBone.intValue = (int)selectedTargetBone;
                    }
                }

                EditorGUILayout.PropertyField(rule.FindPropertyRelative("normalizedTargetOffset"), new GUIContent(
                    "置き場所（身長に対する比率）",
                    "基準から見た位置を、アバター身長を1とした比率で保存します。"));

                var rotation = rule.FindPropertyRelative("targetRotationOffset");
                var coordinateSpaceVersion = rule.FindPropertyRelative("coordinateSpaceVersion");
                if (coordinateSpaceVersion.intValue <
                    PosingSystemAutoPoseLimbRule.CurrentCoordinateSpaceVersion)
                {
                    EditorGUILayout.HelpBox(
                        "旧座標形式です。調整アニメーションウィンドウで基準アバターを表示し、" +
                        "「現在の手足位置から取得」を一度実行してください。再取得までは元Animationの手足の向きを維持します。",
                        MessageType.Warning);
                }
                EditorGUI.BeginChangeCheck();
                var euler = EditorGUILayout.Vector3Field(
                    new GUIContent(
                        "見た目の手足の向き",
                        "Humanoidボーン軸の違いを除いた、基準から見た手首または足首の向きです。"),
                    rotation.quaternionValue.eulerAngles);
                if (EditorGUI.EndChangeCheck())
                {
                    rotation.quaternionValue = Quaternion.Euler(euler);
                }

                var matchRotation = rule.FindPropertyRelative("matchEndRotation");
                EditorGUILayout.PropertyField(matchRotation, new GUIContent("手足の向きも合わせる"));
                EditorGUILayout.PropertyField(rule.FindPropertyRelative("positionWeight"), new GUIContent(
                    "位置を合わせる強さ",
                    "通常は1です。小さくすると元の姿勢を多く残します。"));
                using (new EditorGUI.DisabledScope(!matchRotation.boolValue))
                {
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("rotationWeight"), new GUIContent(
                        "向きを合わせる強さ",
                        "0では回転せず、1で記録した向きへ完全に合わせます。"));
                }
                var bendDirectionMode = rule.FindPropertyRelative("bendDirectionMode");
                EditorGUILayout.PropertyField(bendDirectionMode, new GUIContent(
                    "肘・膝の曲げ方",
                    "通常は元の姿勢を維持します。対象アバターへ元Animationを適用した直後と同じ側へ曲げます。"));
                if ((PosingSystemAutoPoseBendDirectionMode)bendDirectionMode.enumValueIndex ==
                    PosingSystemAutoPoseBendDirectionMode.Manual)
                {
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("poleDirectionInAvatarSpace"), new GUIContent(
                        "肘・膝を曲げる方向",
                        "手動指定時だけ使用します。アバター正面を基準にした方向です。"));
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "元の姿勢Animationに含まれる肘・膝の曲がり方を、IKを解く前に自動取得します。",
                        MessageType.None);
                }
            }
        }

        private void AddPose()
        {
            var index = _poses.arraySize;
            _poses.InsertArrayElementAtIndex(index);
            var pose = _poses.GetArrayElementAtIndex(index);
            pose.FindPropertyRelative("poseKey").stringValue = string.Empty;
            pose.FindPropertyRelative("sourceAnimation").objectReferenceValue = null;
            pose.FindPropertyRelative("solveIterations").intValue = 2;
            pose.FindPropertyRelative("limbRules").arraySize = 0;
            pose.isExpanded = true;
        }

        private static void AddRule(SerializedProperty rules)
        {
            var index = rules.arraySize;
            rules.InsertArrayElementAtIndex(index);
            var rule = rules.GetArrayElementAtIndex(index);
            rule.FindPropertyRelative("enabled").boolValue = true;
            rule.FindPropertyRelative("label").stringValue = string.Empty;
            rule.FindPropertyRelative("solveOrder").intValue = index;
            rule.FindPropertyRelative("limb").enumValueIndex = 0;
            rule.FindPropertyRelative("targetSpace").enumValueIndex = 0;
            rule.FindPropertyRelative("targetBone").enumValueIndex = (int)HumanBodyBones.Hips;
            rule.FindPropertyRelative("normalizedTargetOffset").vector3Value = Vector3.zero;
            rule.FindPropertyRelative("targetRotationOffset").quaternionValue = Quaternion.identity;
            rule.FindPropertyRelative("coordinateSpaceVersion").intValue =
                PosingSystemAutoPoseLimbRule.CurrentCoordinateSpaceVersion;
            rule.FindPropertyRelative("matchEndRotation").boolValue = true;
            rule.FindPropertyRelative("positionWeight").floatValue = 1f;
            rule.FindPropertyRelative("rotationWeight").floatValue = 1f;
            rule.FindPropertyRelative("bendDirectionMode").enumValueIndex =
                (int)PosingSystemAutoPoseBendDirectionMode.SourcePose;
            rule.FindPropertyRelative("poleDirectionInAvatarSpace").vector3Value = Vector3.forward;
            rule.isExpanded = true;
        }
    }
}
