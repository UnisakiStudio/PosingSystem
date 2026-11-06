using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor
{
    public static class PosingSystemAutoPoseEditorGui
    {
        public static Vector3 PositionOffsetField(Vector3 offset, ref bool x, ref bool y, ref bool z,
            PosingSystemAutoPosePositionSpace space = PosingSystemAutoPosePositionSpace.ReferenceLocal)
        {
            EditorGUILayout.LabelField(PosingSystemLocalization.Text("位置（選んだ長さを1とする比率）"));
            Axis("X", ref x, ref offset.x);
            Axis("Y", ref y, ref offset.y);
            Axis("Z", ref z, ref offset.z);
            EditorGUILayout.LabelField(space == PosingSystemAutoPosePositionSpace.World
                ? "ワールドの軸に沿って移動します。オフの軸は元の姿勢の位置を保ちます。"
                : "基準の向きに沿って移動します。オフの軸は元の姿勢の位置を保ちます。", EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.LabelField(PosingSystemLocalization.Text("スライダーは−1〜1。範囲外の値も数値欄に入力できます。"), EditorStyles.wordWrappedMiniLabel);
            return offset;

            void Axis(string label, ref bool enabled, ref float value)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    enabled = EditorGUILayout.ToggleLeft(label, enabled, GUILayout.Width(36));
                    using (new EditorGUI.DisabledScope(!enabled))
                    {
                        // Only a slider interaction may clamp to its range. Repaints and edits
                        // on another axis must preserve values entered outside [-1, 1].
                        EditorGUI.BeginChangeCheck();
                        var slid = GUILayout.HorizontalSlider(Mathf.Clamp(value, -1, 1), -1, 1, GUILayout.MinWidth(60));
                        if (EditorGUI.EndChangeCheck()) value = slid;
                        value = EditorGUILayout.FloatField(value, GUILayout.Width(72));
                    }
                }
            }
        }

        private static readonly HumanBodyBones[] TargetBones =
            PosingSystemAutoPoseAdjustmentUtility.SupportedTargetBones.ToArray();
        private static readonly GUIContent[] TargetBoneLabels = TargetBones
            .Select(bone => new GUIContent(GetJapaneseBoneName(bone)))
            .ToArray();
        private static readonly GUIContent[] TargetAnchorLabels = TargetBoneLabels
            .Concat(new[] { new GUIContent(PosingSystemLocalization.Text("ViewPosition（目の位置）")) }).ToArray();

        public static HumanBodyBones TargetBonePopup(GUIContent label, HumanBodyBones current, ref bool useViewPosition)
        {
            var index = useViewPosition ? TargetBones.Length : Array.IndexOf(TargetBones, current);
            if (index < 0) index = Math.Max(0, Array.IndexOf(TargetBones, HumanBodyBones.Hips));
            var selected = EditorGUILayout.Popup(label, index, TargetAnchorLabels);
            useViewPosition = selected == TargetBones.Length;
            return useViewPosition ? current : TargetBones[Mathf.Clamp(selected, 0, TargetBones.Length - 1)];
        }

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
                PosingSystemLocalization.Text("商品名"),
                "このIK設定を使用する商品名です。"));
            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.PropertyField(_formatVersion, new GUIContent(
                    PosingSystemLocalization.Text("データ形式のバージョン"),
                    "内部的な互換性管理に使います。通常は変更しません。"));
            }

            EditorGUILayout.PropertyField(_productionReady, new GUIContent(
                PosingSystemLocalization.Text("公開用として有効"),
                "有効にすると従来の調整Animationを無視し、このIK設定から自動調整Animationを生成します。"));

            if (_productionReady.boolValue)
            {
                EditorGUILayout.HelpBox(
                    PosingSystemLocalization.Text("公開用として有効です。カタログを参照する商品では、従来の姿勢別調整Animationとアバター固有補正が使用されません。設定漏れの姿勢がないことを確認してください。"),
                    MessageType.Warning);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    PosingSystemLocalization.Text("現在は制作中です。商品Prefabから参照されていても、従来の調整Animationが引き続き使用されます。"),
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

                if (GUILayout.Button(PosingSystemLocalization.Text("姿勢設定を追加")))
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
                    if (GUILayout.Button(PosingSystemLocalization.Text("削除"), GUILayout.Width(48f)))
                    {
                        PosingSystemEditorChangeUtility.DeleteArrayElement(_poses, poseIndex);
                        GUIUtility.ExitGUI();
                    }
                }

                if (!pose.isExpanded)
                {
                    return;
                }

                EditorGUILayout.PropertyField(poseKey, new GUIContent(
                    PosingSystemLocalization.Text("姿勢の識別キー"),
                    "通常は「メニュー名_姿勢名」です。調整ウィンドウから追加すると自動入力されます。"));
                EditorGUILayout.PropertyField(sourceAnimation, new GUIContent(
                    PosingSystemLocalization.Text("元の姿勢Animation"),
                    "IKで補正する前の基準姿勢です。"));
                EditorGUILayout.PropertyField(solveIterations, new GUIContent(
                    PosingSystemLocalization.Text("計算の反復回数"),
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
                    if (GUILayout.Button(PosingSystemLocalization.Text("手足のIKルールを追加")))
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
                    if (GUILayout.Button(PosingSystemLocalization.Text("削除"), GUILayout.Width(48f)))
                    {
                        PosingSystemEditorChangeUtility.DeleteArrayElement(rules, ruleIndex);
                        GUIUtility.ExitGUI();
                    }
                }

                if (!rule.isExpanded)
                {
                    return;
                }

                EditorGUILayout.PropertyField(rule.FindPropertyRelative("enabled"), new GUIContent(PosingSystemLocalization.Text("このルールを使用")));
                EditorGUILayout.PropertyField(label, new GUIContent(PosingSystemLocalization.Text("ルール名"), "例: 右手を腰へ、左手を床へ"));
                EditorGUILayout.PropertyField(rule.FindPropertyRelative("limb"), new GUIContent(PosingSystemLocalization.Text("動かす手足")));
                EditorGUILayout.PropertyField(rule.FindPropertyRelative("solveOrder"), new GUIContent(PosingSystemLocalization.Text("計算する順番")));

                var targetSpace = rule.FindPropertyRelative("targetSpace");
                EditorGUILayout.PropertyField(targetSpace, new GUIContent(
                    PosingSystemLocalization.Text("置き場所の基準"),
                    "身体のボーン、アバター全体、床のいずれを基準にするか選びます。"));
                if ((PosingSystemAutoPoseTargetSpace)targetSpace.enumValueIndex ==
                    PosingSystemAutoPoseTargetSpace.BodyBone)
                {
                    var targetBone = rule.FindPropertyRelative("targetBone");
                    var viewPosition = rule.FindPropertyRelative("useViewPosition");
                    var useViewPosition = viewPosition.boolValue;
                    EditorGUI.BeginChangeCheck();
                    var selectedTargetBone = PosingSystemAutoPoseEditorGui.TargetBonePopup(
                        new GUIContent(
                            PosingSystemLocalization.Text("基準にする身体ボーン"),
                            "頭、腰、膝、ViewPosition（目の位置）など、手足の置き場所を測る基準です。ViewPositionは頭の動きに追従します。"),
                        (HumanBodyBones)targetBone.intValue, ref useViewPosition);
                    if (EditorGUI.EndChangeCheck())
                    {
                        targetBone.intValue = (int)selectedTargetBone;
                        viewPosition.boolValue = useViewPosition;
                    }
                }

                var positionSpace = rule.FindPropertyRelative("positionSpace");
                EditorGUILayout.PropertyField(positionSpace, new GUIContent(PosingSystemLocalization.Text("位置の座標系")));
                EditorGUILayout.PropertyField(rule.FindPropertyRelative("lengthReference"), new GUIContent(PosingSystemLocalization.Text("位置の長さの基準")));
                var positionX = rule.FindPropertyRelative("applyPositionX");
                var positionY = rule.FindPropertyRelative("applyPositionY");
                var positionZ = rule.FindPropertyRelative("applyPositionZ");
                var x = positionX.boolValue; var y = positionY.boolValue; var z = positionZ.boolValue;
                var offset = rule.FindPropertyRelative("normalizedTargetOffset");
                offset.vector3Value = PosingSystemAutoPoseEditorGui.PositionOffsetField(offset.vector3Value, ref x, ref y, ref z,
                    (PosingSystemAutoPosePositionSpace)positionSpace.enumValueIndex);
                positionX.boolValue = x; positionY.boolValue = y; positionZ.boolValue = z;

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
                        PosingSystemLocalization.Text("見た目の手足の向き"),
                        "Humanoidボーン軸の違いを除いた、基準から見た手首または足首の向きです。"),
                    rotation.quaternionValue.eulerAngles);
                if (EditorGUI.EndChangeCheck())
                {
                    rotation.quaternionValue = Quaternion.Euler(euler);
                }

                var matchRotation = rule.FindPropertyRelative("matchEndRotation");
                EditorGUILayout.PropertyField(matchRotation, new GUIContent(PosingSystemLocalization.Text("手足の向きも合わせる")));
                EditorGUILayout.PropertyField(rule.FindPropertyRelative("positionWeight"), new GUIContent(
                    PosingSystemLocalization.Text("位置を合わせる強さ"),
                    "通常は1です。小さくすると元の姿勢を多く残します。"));
                using (new EditorGUI.DisabledScope(!matchRotation.boolValue))
                {
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("rotationWeight"), new GUIContent(
                        PosingSystemLocalization.Text("向きを合わせる強さ"),
                        "0では回転せず、1で記録した向きへ完全に合わせます。"));
                }
                var bendDirectionMode = rule.FindPropertyRelative("bendDirectionMode");
                EditorGUILayout.PropertyField(bendDirectionMode, new GUIContent(
                    PosingSystemLocalization.Text("肘・膝の曲げ方"),
                    "通常は元の姿勢を維持します。対象アバターへ元Animationを適用した直後と同じ側へ曲げます。"));
                if ((PosingSystemAutoPoseBendDirectionMode)bendDirectionMode.enumValueIndex ==
                    PosingSystemAutoPoseBendDirectionMode.Manual)
                {
                    EditorGUILayout.PropertyField(rule.FindPropertyRelative("poleDirectionInAvatarSpace"), new GUIContent(
                        PosingSystemLocalization.Text("肘・膝を曲げる方向"),
                        "手動指定時だけ使用します。アバター正面を基準にした方向です。"));
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        PosingSystemLocalization.Text("元の姿勢Animationに含まれる肘・膝の曲がり方を、IKを解く前に自動取得します。"),
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
            rule.FindPropertyRelative("useViewPosition").boolValue = false;
            rule.FindPropertyRelative("normalizedTargetOffset").vector3Value = Vector3.zero;
            rule.FindPropertyRelative("lengthReference").enumValueIndex = 0;
            rule.FindPropertyRelative("positionSpace").enumValueIndex = 0;
            rule.FindPropertyRelative("applyPositionX").boolValue = true;
            rule.FindPropertyRelative("applyPositionY").boolValue = true;
            rule.FindPropertyRelative("applyPositionZ").boolValue = true;
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
