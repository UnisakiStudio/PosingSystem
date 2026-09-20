using System.Linq;
using UnityEditor;
using UnityEngine;
using jp.unisakistudio.posingsystem;

namespace jp.unisakistudio.posingsystemeditor
{
    public partial class PosingAnimationAdjustmentWindow
    {
        private bool _previewAutoPoseAdjustment = true;
        private bool _showAutoPoseSceneGuides;
        private int _selectedAutoPoseRuleIndex;

        private void RegisterAutoPoseSceneGui()
        {
            SceneView.duringSceneGui -= DrawAutoPoseSceneGuides;
            SceneView.duringSceneGui += DrawAutoPoseSceneGuides;
        }

        private void UnregisterAutoPoseSceneGui()
        {
            SceneView.duringSceneGui -= DrawAutoPoseSceneGuides;
        }

        private void DrawAutoPoseCatalogPanel()
        {
            if (_posingSystem == null)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                EditorGUILayout.LabelField("IKによる自動姿勢調整（開発用）", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(
                    "基準アバターで記録した手足の置き場所と向きを、別のHumanoidアバターの体格に合わせてIK補正します。" +
                    "商品利用者のアバターでは、この設定から専用の差分Animationを自動生成します。",
                    MessageType.Info);

                var oldCatalog = _posingSystem.autoPoseCatalog;
                PosingSystemAutoPoseCatalog newCatalog;
                using (new EditorGUI.DisabledScope(!_posingSystem.developmentMode))
                {
                    newCatalog = EditorGUILayout.ObjectField(
                        "IK設定ファイル",
                        oldCatalog,
                        typeof(PosingSystemAutoPoseCatalog),
                        false) as PosingSystemAutoPoseCatalog;
                }
                if (newCatalog != oldCatalog)
                {
                    Undo.RegisterCompleteObjectUndo(_posingSystem, "Change auto pose catalog");
                    _posingSystem.autoPoseCatalog = newCatalog;
                    EditorUtility.SetDirty(_posingSystem);
                    _selectedAutoPoseRuleIndex = 0;
                    ApplyPreviewPose();
                }

                if (!_posingSystem.developmentMode)
                {
                    EditorGUILayout.LabelField(
                        oldCatalog == null
                            ? "IK自動調整データは設定されていません。"
                            : (oldCatalog.productionReady
                                ? "姿勢別IK自動調整を使用します。"
                                : "IK設定は開発中のため、従来の調整Animationを使用します。"),
                        EditorStyles.wordWrappedMiniLabel);
                    EditorGUILayout.HelpBox(
                        "IK設定の作成・編集は商品開発用の機能です。編集を始める場合は開発モードを有効にしてください。",
                        MessageType.Info);
                    if (GUILayout.Button("IK設定の開発モードを有効にする"))
                    {
                        Undo.RegisterCompleteObjectUndo(_posingSystem, "Enable auto pose authoring");
                        _posingSystem.developmentMode = true;
                        EditorUtility.SetDirty(_posingSystem);
                    }
                    else
                    {
                        return;
                    }
                }

                if (GUILayout.Button("新しいIK設定ファイルを作成..."))
                {
                    CreateAndAssignAutoPoseCatalog();
                }

                var catalog = _posingSystem.autoPoseCatalog;
                if (catalog == null)
                {
                    EditorGUILayout.HelpBox(
                        "IK設定ファイルを作成または割り当てると、姿勢ごとの手足ターゲットを記録できます。",
                        MessageType.Info);
                    return;
                }

                EditorGUI.BeginChangeCheck();
                var productionReady = EditorGUILayout.ToggleLeft(
                    "公開用として有効",
                    catalog.productionReady);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(catalog, "Change auto pose catalog readiness");
                    catalog.productionReady = productionReady;
                    EditorUtility.SetDirty(catalog);
                    ApplyPreviewPose();
                }

                EditorGUI.BeginChangeCheck();
                _previewAutoPoseAdjustment = EditorGUILayout.ToggleLeft(
                    "IK自動調整をプレビュー",
                    _previewAutoPoseAdjustment);
                _showAutoPoseSceneGuides = EditorGUILayout.ToggleLeft(
                    "シーンでIKターゲットを表示・編集",
                    _showAutoPoseSceneGuides);
                if (EditorGUI.EndChangeCheck())
                {
                    ApplyPreviewPose();
                    SceneView.RepaintAll();
                }
                if (_showAutoPoseSceneGuides)
                {
                    EditorGUILayout.LabelField(
                        "姿勢別IK設定でルール名を選ぶと、Scene Viewの移動・回転ハンドルで置き場所を直接調整できます。",
                        EditorStyles.wordWrappedMiniLabel);
                }

                EditorGUILayout.HelpBox(
                    catalog.productionReady
                        ? "公開時の動作です。各姿勢の従来の調整Animationとアバター固有補正を使わず、このIK設定から対象アバター専用の調整Animationを生成します。"
                        : "制作中の動作です。IK設定は開発プレビューだけで使用し、通常の出力では従来の調整Animationとアバター固有補正を使用します。",
                    catalog.productionReady ? MessageType.Warning : MessageType.None);
            }
        }

        private void DrawAutoPoseAuthoringPanel(AnimationEntry entry)
        {
            if (_posingSystem == null || !_posingSystem.developmentMode ||
                _posingSystem.autoPoseCatalog == null ||
                entry?.Animation?.animationClip is not AnimationClip baseClip)
            {
                return;
            }

            var catalog = _posingSystem.autoPoseCatalog;
            var poseKey = GetAnimationKey(entry);
            var definition = catalog.Find(poseKey, baseClip);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("姿勢別IK設定（開発用）", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                if (definition == null)
                {
                    EditorGUILayout.HelpBox("この姿勢にはIK設定がまだありません。", MessageType.Info);
                    if (GUILayout.Button("この姿勢のIK設定を追加"))
                    {
                        Undo.RegisterCompleteObjectUndo(catalog, "Add auto pose definition");
                        definition = new PosingSystemAutoPoseDefinition
                        {
                            poseKey = poseKey,
                            sourceAnimation = baseClip,
                        };
                        catalog.poses.Add(definition);
                        EditorUtility.SetDirty(catalog);
                    }
                    return;
                }

                EditorGUILayout.LabelField("識別キー", definition.poseKey);
                using (new EditorGUI.DisabledScope(true))
                {
                    EditorGUILayout.ObjectField("元の姿勢", definition.sourceAnimation, typeof(AnimationClip), false);
                }

                EditorGUI.BeginChangeCheck();
                var solveIterations = EditorGUILayout.IntSlider("反復回数", definition.solveIterations, 1, 8);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RegisterCompleteObjectUndo(catalog, "Change auto pose iterations");
                    definition.solveIterations = solveIterations;
                    EditorUtility.SetDirty(catalog);
                    ApplyPreviewPose();
                }

                for (var index = 0; index < definition.limbRules.Count; index++)
                {
                    DrawAutoPoseRule(catalog, definition, index);
                }

                if (GUILayout.Button("IKルールを追加"))
                {
                    Undo.RegisterCompleteObjectUndo(catalog, "Add auto pose rule");
                    var limb = PosingSystemAutoPoseAdjustmentUtility.FindLeastUsedLimb(
                        definition.limbRules);
                    definition.limbRules.Add(new PosingSystemAutoPoseLimbRule
                    {
                        enabled = false,
                        label = $"IK {definition.limbRules.Count + 1}",
                        limb = limb,
                        coordinateSpaceVersion = PosingSystemAutoPoseLimbRule.CurrentCoordinateSpaceVersion,
                    });
                    _selectedAutoPoseRuleIndex = definition.limbRules.Count - 1;
                    EditorUtility.SetDirty(catalog);
                }

                using (new EditorGUI.DisabledScope(definition.limbRules.All(rule => rule == null || !rule.enabled)))
                {
                    EditorGUILayout.HelpBox(
                        "検証用です。現在プレビュー中のアバターへIKを適用した差分Animationを、確認できる.animファイルとして保存します。" +
                        "公開用の自動生成では書き出す必要はありません。",
                        MessageType.None);
                    if (GUILayout.Button("検証用の自動調整Animationを書き出す..."))
                    {
                        ExportSelectedAutoPoseAdjustment(baseClip, definition);
                    }
                }
            }
        }

        private void DrawAutoPoseRule(
            PosingSystemAutoPoseCatalog catalog,
            PosingSystemAutoPoseDefinition definition,
            int index)
        {
            var rule = definition.limbRules[index];
            if (rule == null)
            {
                return;
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    var selected = GUILayout.Toggle(
                        _selectedAutoPoseRuleIndex == index,
                        string.IsNullOrEmpty(rule.label) ? $"IK {index + 1}" : rule.label,
                        "Button");
                    if (selected)
                    {
                        _selectedAutoPoseRuleIndex = index;
                    }
                    if (GUILayout.Button("削除", GUILayout.Width(50f)))
                    {
                        Undo.RegisterCompleteObjectUndo(catalog, "Remove auto pose rule");
                        definition.limbRules.RemoveAt(index);
                        _selectedAutoPoseRuleIndex = Mathf.Clamp(
                            _selectedAutoPoseRuleIndex,
                            0,
                            Mathf.Max(0, definition.limbRules.Count - 1));
                        EditorUtility.SetDirty(catalog);
                        ApplyPreviewPose();
                        GUIUtility.ExitGUI();
                    }
                }

                var enabled = EditorGUILayout.Toggle("有効", rule.enabled);
                var label = EditorGUILayout.TextField("名前", rule.label);
                var limb = (PosingSystemAutoPoseLimb)EditorGUILayout.EnumPopup("手足", rule.limb);
                var solveOrder = EditorGUILayout.IntField("解決順", rule.solveOrder);
                var targetSpace = (PosingSystemAutoPoseTargetSpace)EditorGUILayout.EnumPopup(
                    "ターゲット基準",
                    rule.targetSpace);
                var targetBone = rule.targetBone;
                if (targetSpace == PosingSystemAutoPoseTargetSpace.BodyBone)
                {
                    targetBone = PosingSystemAutoPoseEditorGui.TargetBonePopup(
                        new GUIContent(
                            "基準ボーン",
                            "指ボーンを除いたHumanoidボーンから、置き場所の基準を選びます。"),
                        targetBone);
                }
                var normalizedOffset = EditorGUILayout.Vector3Field(
                    "位置（身長比）",
                    rule.normalizedTargetOffset);
                var rotationEuler = EditorGUILayout.Vector3Field(
                    "見た目の手足の向き",
                    rule.targetRotationOffset.eulerAngles);
                var matchRotation = EditorGUILayout.Toggle("向きを合わせる", rule.matchEndRotation);
                var positionWeight = EditorGUILayout.Slider("位置の強さ", rule.positionWeight, 0f, 1f);
                var rotationWeight = EditorGUILayout.Slider("向きの強さ", rule.rotationWeight, 0f, 1f);
                var bendDirectionMode = (PosingSystemAutoPoseBendDirectionMode)EditorGUILayout.EnumPopup(
                    "肘・膝の曲げ方",
                    rule.bendDirectionMode);
                var poleDirection = rule.poleDirectionInAvatarSpace;
                if (bendDirectionMode == PosingSystemAutoPoseBendDirectionMode.Manual)
                {
                    poleDirection = EditorGUILayout.Vector3Field(
                        "肘・膝の曲げ方向",
                        poleDirection);
                }
                else
                {
                    EditorGUILayout.HelpBox(
                        "対象アバターへ元の姿勢Animationを適用した直後の曲げ方向を保存し、IK中も同じ側へ曲げます。",
                        MessageType.None);
                }

                if (rule.coordinateSpaceVersion < PosingSystemAutoPoseLimbRule.CurrentCoordinateSpaceVersion)
                {
                    EditorGUILayout.HelpBox(
                        "このルールは旧座標形式です。別アバターで手首・足首がねじれないよう、" +
                        "「現在の手足位置から取得」を一度実行してください。再取得までは向きを変更せず、元Animationの向きを維持します。",
                        MessageType.Warning);
                }

                var changed = enabled != rule.enabled || label != rule.label || limb != rule.limb ||
                              solveOrder != rule.solveOrder || targetSpace != rule.targetSpace ||
                              targetBone != rule.targetBone || normalizedOffset != rule.normalizedTargetOffset ||
                              Quaternion.Angle(Quaternion.Euler(rotationEuler), rule.targetRotationOffset) > 0.001f ||
                              matchRotation != rule.matchEndRotation ||
                              !Mathf.Approximately(positionWeight, rule.positionWeight) ||
                              !Mathf.Approximately(rotationWeight, rule.rotationWeight) ||
                              bendDirectionMode != rule.bendDirectionMode ||
                              poleDirection != rule.poleDirectionInAvatarSpace;
                if (changed)
                {
                    Undo.RegisterCompleteObjectUndo(catalog, "Edit auto pose rule");
                    rule.enabled = enabled;
                    rule.label = label;
                    rule.limb = limb;
                    rule.solveOrder = solveOrder;
                    rule.targetSpace = targetSpace;
                    rule.targetBone = targetBone;
                    rule.normalizedTargetOffset = normalizedOffset;
                    rule.targetRotationOffset = Quaternion.Euler(rotationEuler);
                    rule.matchEndRotation = matchRotation;
                    rule.positionWeight = positionWeight;
                    rule.rotationWeight = rotationWeight;
                    rule.bendDirectionMode = bendDirectionMode;
                    rule.poleDirectionInAvatarSpace = poleDirection;
                    EditorUtility.SetDirty(catalog);
                    ApplyPreviewPose();
                }

                if (GUILayout.Button("現在の手足位置から取得"))
                {
                    var animator = GetAutoPosePreviewAnimator();
                    Undo.RegisterCompleteObjectUndo(catalog, "Capture auto pose rule");
                    if (PosingSystemAutoPoseAdjustmentUtility.TryCaptureRuleFromCurrentPose(
                            animator,
                            rule,
                            out var message))
                    {
                        rule.enabled = true;
                        catalog.formatVersion = PosingSystemAutoPoseCatalog.CurrentFormatVersion;
                        EditorUtility.SetDirty(catalog);
                        ApplyPreviewPose();
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("IK設定", message, "OK");
                    }
                }
            }
        }

        private void CreateAndAssignAutoPoseCatalog()
        {
            var path = EditorUtility.SaveFilePanelInProject(
                "IK設定ファイルを作成",
                $"{SanitizeFileName(_posingSystem.name)}_AutoPoseCatalog",
                "asset",
                "保存先を指定してください。",
                "Assets");
            if (string.IsNullOrEmpty(path))
            {
                return;
            }

            var catalog = CreateInstance<PosingSystemAutoPoseCatalog>();
            catalog.productName = _posingSystem.name;
            AssetDatabase.CreateAsset(catalog, path);
            Undo.RegisterCompleteObjectUndo(_posingSystem, "Assign auto pose catalog");
            _posingSystem.autoPoseCatalog = catalog;
            EditorUtility.SetDirty(_posingSystem);
            AssetDatabase.SaveAssets();
            Selection.activeObject = catalog;
        }

        private void ExportSelectedAutoPoseAdjustment(
            AnimationClip baseClip,
            PosingSystemAutoPoseDefinition definition)
        {
            var animator = GetAutoPosePreviewAnimator();
            if (!PosingSystemAutoPoseAdjustmentUtility.TryCreateAdjustmentClip(
                    animator,
                    baseClip,
                    definition,
                    out var generated,
                    out _,
                    out var message))
            {
                EditorUtility.DisplayDialog("自動調整Animation", message, "OK");
                return;
            }

            try
            {
                var path = EditorUtility.SaveFilePanelInProject(
                    "自動調整Animationを書き出す",
                    $"{SanitizeFileName(definition.poseKey)}_AutoAdjustment",
                    "anim",
                    "保存先を指定してください。",
                    "Assets");
                if (!string.IsNullOrEmpty(path))
                {
                    var saved = PosingSystemAutoPoseAdjustmentUtility.SaveClip(generated, path);
                    Selection.activeObject = saved;
                    EditorUtility.DisplayDialog("自動調整Animation", message, "OK");
                }
            }
            finally
            {
                DestroyImmediate(generated);
                ApplyPreviewPose();
            }
        }

        private bool TryApplySelectedAutoPosePreview()
        {
            if (_posingSystem == null || _previewAvatar == null ||
                (!_posingSystem.UsesAutoPoseCatalog &&
                 (!_posingSystem.developmentMode || !_previewAutoPoseAdjustment ||
                  _posingSystem.autoPoseCatalog == null)))
            {
                return false;
            }

            var entry = GetSelectedEntry();
            var clip = entry?.Animation?.animationClip as AnimationClip;
            var definition = PosingSystemAutoPoseAdjustmentUtility.FindDefinition(
                _posingSystem,
                GetAnimationKey(entry),
                clip);
            if (definition != null)
            {
                PosingSystemAutoPoseAdjustmentUtility.TryApplyDefinition(_previewAvatar, definition);
            }

            // A production catalog intentionally suppresses legacy adjustment clips even if a
            // definition is missing; the unmodified base pose makes the missing entry obvious.
            return definition != null || _posingSystem.UsesAutoPoseCatalog;
        }

        private Animator GetAutoPosePreviewAnimator()
        {
            if (_previewAvatar != null)
            {
                return _previewAvatar;
            }
            if (_posingSystem?.previewAvatarObject != null)
            {
                return _posingSystem.previewAvatarObject.GetComponent<Animator>();
            }
            return _posingSystem?.GetAvatar()?.GetComponent<Animator>();
        }

        private void DrawAutoPoseSceneGuides(SceneView sceneView)
        {
            if (!_showAutoPoseSceneGuides || !IsOpen || _posingSystem == null ||
                _posingSystem.autoPoseCatalog == null)
            {
                return;
            }

            var entry = GetSelectedEntry();
            var baseClip = entry?.Animation?.animationClip as AnimationClip;
            var definition = _posingSystem.autoPoseCatalog.Find(GetAnimationKey(entry), baseClip);
            var animator = GetAutoPosePreviewAnimator();
            if (definition == null || animator == null)
            {
                return;
            }

            for (var index = 0; index < definition.limbRules.Count; index++)
            {
                var rule = definition.limbRules[index];
                if (rule == null ||
                    !PosingSystemAutoPoseAdjustmentUtility.TryGetLimbBones(
                        animator, rule.limb, out var root, out var joint, out var tip) ||
                    !PosingSystemAutoPoseAdjustmentUtility.TryGetTarget(
                        animator, rule, out var targetPosition, out var targetRotation, out _))
                {
                    continue;
                }

                var color = GetAutoPoseRuleColor(rule.limb);
                color.a = rule.enabled ? 1f : 0.35f;
                Handles.color = color;

                if (index == _selectedAutoPoseRuleIndex)
                {
                    EditorGUI.BeginChangeCheck();
                    var editedPosition = Handles.PositionHandle(targetPosition, Quaternion.identity);
                    var editedRotation = rule.matchEndRotation
                        ? Handles.RotationHandle(targetRotation, editedPosition)
                        : targetRotation;
                    if (EditorGUI.EndChangeCheck())
                    {
                        Undo.RegisterCompleteObjectUndo(
                            _posingSystem.autoPoseCatalog,
                            "Move auto pose IK target");
                        if (PosingSystemAutoPoseAdjustmentUtility.TrySetTargetFromWorld(
                                animator,
                                rule,
                                editedPosition,
                                editedRotation,
                                out _))
                        {
                            _posingSystem.autoPoseCatalog.formatVersion =
                                PosingSystemAutoPoseCatalog.CurrentFormatVersion;
                            EditorUtility.SetDirty(_posingSystem.autoPoseCatalog);
                            targetPosition = editedPosition;
                            targetRotation = editedRotation;
                            ApplyPreviewPose();
                            Repaint();
                        }
                    }
                }

                Handles.DrawAAPolyLine(4f, root.position, joint.position, tip.position);
                Handles.DrawDottedLine(tip.position, targetPosition, 4f);
                var size = HandleUtility.GetHandleSize(targetPosition) *
                           (index == _selectedAutoPoseRuleIndex ? 0.075f : 0.05f);
                Handles.SphereHandleCap(0, targetPosition, Quaternion.identity, size, EventType.Repaint);

                var pole = rule.bendDirectionMode == PosingSystemAutoPoseBendDirectionMode.SourcePose &&
                           PosingSystemAutoPoseAdjustmentUtility.TryGetCurrentBendDirection(
                               animator, rule, out var sourceBendDirection)
                    ? sourceBendDirection
                    : animator.transform.TransformDirection(rule.poleDirectionInAvatarSpace);
                if (pole.sqrMagnitude > 0.000001f)
                {
                    Handles.DrawDottedLine(root.position, root.position + pole.normalized * size * 5f, 3f);
                }
                Handles.Label(targetPosition + Vector3.up * size, string.IsNullOrEmpty(rule.label)
                    ? rule.limb.ToString()
                    : rule.label);
            }
        }

        private static Color GetAutoPoseRuleColor(PosingSystemAutoPoseLimb limb)
        {
            return limb switch
            {
                PosingSystemAutoPoseLimb.LeftArm => new Color(0.1f, 0.9f, 1f),
                PosingSystemAutoPoseLimb.RightArm => new Color(1f, 0.25f, 0.8f),
                PosingSystemAutoPoseLimb.LeftLeg => new Color(0.2f, 1f, 0.35f),
                _ => new Color(1f, 0.8f, 0.15f),
            };
        }
    }
}
