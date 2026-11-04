using System;
using System.Collections.Generic;
using System.Linq;
using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace jp.unisakistudio.posingsystemeditor
{
    [InitializeOnLoad]
    public static class PosingSystemHierarchyBadge
    {
        private readonly struct StatusSignature : IEquatable<StatusSignature>
        {
            private readonly int posingSystemDirtyCount;
            private readonly int avatarInstanceId;
            private readonly int avatarDirtyCount;
            private readonly int animatorInstanceId;
            private readonly int animatorDirtyCount;
            private readonly int animatorAvatarInstanceId;
            private readonly int baseControllerInstanceId;
            private readonly int baseControllerDirtyCount;
            private readonly int expressionsMenuInstanceId;
            private readonly int expressionsMenuDirtyCount;
            private readonly int expressionParametersInstanceId;
            private readonly int expressionParametersDirtyCount;
            private readonly int hierarchyVersion;
            private readonly int projectVersion;
            private readonly int catalogInstanceId;
            private readonly int catalogDirtyCount;

            internal StatusSignature(PosingSystem posingSystem, int currentHierarchyVersion, int currentProjectVersion)
            {
                var avatar = posingSystem.GetAvatar();
                var animator = avatar != null ? avatar.GetComponent<Animator>() : null;
                var animatorAvatar = animator != null ? animator.avatar : null;
                var baseController = avatar != null && avatar.baseAnimationLayers != null
                    ? avatar.baseAnimationLayers.FirstOrDefault(l => l.type == VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base).animatorController
                    : null;
                var expressionsMenu = avatar != null ? avatar.expressionsMenu : null;
                var expressionParameters = avatar != null ? avatar.expressionParameters : null;

                posingSystemDirtyCount = EditorUtility.GetDirtyCount(posingSystem);
                avatarInstanceId = GetInstanceId(avatar);
                avatarDirtyCount = GetDirtyCount(avatar);
                animatorInstanceId = GetInstanceId(animator);
                animatorDirtyCount = GetDirtyCount(animator);
                animatorAvatarInstanceId = GetInstanceId(animatorAvatar);
                baseControllerInstanceId = GetInstanceId(baseController);
                baseControllerDirtyCount = GetDirtyCount(baseController);
                expressionsMenuInstanceId = GetInstanceId(expressionsMenu);
                expressionsMenuDirtyCount = GetDirtyCount(expressionsMenu);
                expressionParametersInstanceId = GetInstanceId(expressionParameters);
                expressionParametersDirtyCount = GetDirtyCount(expressionParameters);
                hierarchyVersion = currentHierarchyVersion;
                projectVersion = currentProjectVersion;
                catalogInstanceId = GetInstanceId(posingSystem.autoPoseCatalog);
                catalogDirtyCount = GetDirtyCount(posingSystem.autoPoseCatalog);
            }

            public bool Equals(StatusSignature other)
            {
                return posingSystemDirtyCount == other.posingSystemDirtyCount
                    && avatarInstanceId == other.avatarInstanceId
                    && avatarDirtyCount == other.avatarDirtyCount
                    && animatorInstanceId == other.animatorInstanceId
                    && animatorDirtyCount == other.animatorDirtyCount
                    && animatorAvatarInstanceId == other.animatorAvatarInstanceId
                    && baseControllerInstanceId == other.baseControllerInstanceId
                    && baseControllerDirtyCount == other.baseControllerDirtyCount
                    && expressionsMenuInstanceId == other.expressionsMenuInstanceId
                    && expressionsMenuDirtyCount == other.expressionsMenuDirtyCount
                    && expressionParametersInstanceId == other.expressionParametersInstanceId
                    && expressionParametersDirtyCount == other.expressionParametersDirtyCount
                    && hierarchyVersion == other.hierarchyVersion
                    && projectVersion == other.projectVersion
                    && catalogInstanceId == other.catalogInstanceId
                    && catalogDirtyCount == other.catalogDirtyCount;
            }

            public override bool Equals(object obj)
            {
                return obj is StatusSignature other && Equals(other);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hash = posingSystemDirtyCount;
                    hash = (hash * 397) ^ avatarInstanceId;
                    hash = (hash * 397) ^ avatarDirtyCount;
                    hash = (hash * 397) ^ animatorInstanceId;
                    hash = (hash * 397) ^ animatorDirtyCount;
                    hash = (hash * 397) ^ animatorAvatarInstanceId;
                    hash = (hash * 397) ^ baseControllerInstanceId;
                    hash = (hash * 397) ^ baseControllerDirtyCount;
                    hash = (hash * 397) ^ expressionsMenuInstanceId;
                    hash = (hash * 397) ^ expressionsMenuDirtyCount;
                    hash = (hash * 397) ^ expressionParametersInstanceId;
                    hash = (hash * 397) ^ expressionParametersDirtyCount;
                    hash = (hash * 397) ^ hierarchyVersion;
                    hash = (hash * 397) ^ projectVersion;
                    hash = (hash * 397) ^ catalogInstanceId;
                    return (hash * 397) ^ catalogDirtyCount;
                }
            }

            private static int GetInstanceId(UnityEngine.Object obj)
            {
                return obj != null ? obj.GetInstanceID() : 0;
            }

            private static int GetDirtyCount(UnityEngine.Object obj)
            {
                return obj != null ? EditorUtility.GetDirtyCount(obj) : 0;
            }
        }

        private readonly struct BadgeStatus
        {
            internal readonly StatusSignature Signature;
            internal readonly bool IsWarning;
            internal readonly bool IsError;
            internal readonly PosingSystem Target;

            internal BadgeStatus(PosingSystem target, StatusSignature signature, bool isWarning, bool isError)
            {
                Target = target;
                Signature = signature;
                IsWarning = isWarning;
                IsError = isError;
            }
        }

        private static readonly Dictionary<int, BadgeStatus> statusByInstanceId = new Dictionary<int, BadgeStatus>();
        private static Texture2D warningIcon;
        private static Texture2D errorIcon;
        private static int hierarchyVersion;
        private static int projectVersion;
        private static double nextDirtyCheck;

        internal static int StatusEvaluationCount { get; private set; }
        internal static int Revision { get; private set; }

        static PosingSystemHierarchyBadge()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.projectChanged += OnProjectChanged;
            Undo.undoRedoPerformed += InvalidateAll;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            ObjectChangeEvents.changesPublished += OnObjectChanges;
            EditorApplication.update += OnEditorUpdate;
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null || !obj.TryGetComponent<PosingSystem>(out var posingSystem))
            {
                return;
            }

            EnsureIcons();
            var status = GetDisplayStatus(posingSystem);
            var rect = new Rect(selectionRect.xMax - 18, selectionRect.y, 16, selectionRect.height);
            if (status.IsError) GUI.Label(rect, new GUIContent(errorIcon, "設定にエラーがあります。Inspectorの「設定の確認」を開いてください。"));
            else if (status.IsWarning) GUI.Label(rect, new GUIContent(warningIcon, "設定の確認、またはプレビルドの更新が必要です。Inspectorを確認してください。"));
        }

        internal static (bool IsWarning, bool IsError) GetDisplayStatus(PosingSystem posingSystem)
        {
            var status = GetStatus(posingSystem);
            return (status.IsWarning && posingSystem != null && !posingSystem.hideHierarchyWarnings, status.IsError);
        }

        internal static (bool IsWarning, bool IsError) GetStatus(PosingSystem posingSystem)
        {
            if (posingSystem == null)
            {
                return (false, false);
            }

            var instanceId = posingSystem.GetInstanceID();
            var signature = new StatusSignature(posingSystem, hierarchyVersion, projectVersion);
            var explicitlyInvalidated = posingSystem.previousErrorCheckTime == DateTime.MinValue;
            if (!explicitlyInvalidated
                && statusByInstanceId.TryGetValue(instanceId, out var cachedStatus)
                && cachedStatus.Signature.Equals(signature)
                && !PosingSystemInspectionCache.HasChanged(posingSystem))
            {
                return (cachedStatus.IsWarning, cachedStatus.IsError);
            }

            var isWarning = (PosingSystemConverter.GetVisibleWarningsForInspector(posingSystem) &
                (PosingSystem.WarningType.PrebuildOutOfDate | PosingSystem.WarningType.Configuration)) != 0;
            var isError = PosingSystemConverter.HasError(posingSystem);
            StatusEvaluationCount++;

            posingSystem.isWarning = isWarning;
            posingSystem.isError = isError;
            posingSystem.previousErrorCheckTime = DateTime.Now;
            statusByInstanceId[instanceId] = new BadgeStatus(posingSystem, signature, isWarning, isError);
            return (isWarning, isError);
        }

        private static void EnsureIcons()
        {
            if (warningIcon == null)
            {
                warningIcon = (Texture2D)EditorGUIUtility.IconContent("console.warnicon.sml").image;
            }

            if (errorIcon == null)
            {
                errorIcon = (Texture2D)EditorGUIUtility.IconContent("console.erroricon.sml").image;
            }
        }

        private static void OnHierarchyChanged()
        {
            hierarchyVersion++;
            InvalidateAll();
        }

        // Undo/SerializedObject edits and referenced asset changes need not change the hierarchy.
        private static void OnObjectChanges(ref ObjectChangeEventStream stream)
        {
            for (var i = 0; i < stream.length; i++)
            {
                var kind = stream.GetEventType(i);
                if (kind == ObjectChangeKind.ChangeAssetObjectProperties)
                {
                    OnProjectChanged();
                    return;
                }
                if (kind == ObjectChangeKind.ChangeGameObjectOrComponentProperties)
                {
                    stream.GetChangeGameObjectOrComponentPropertiesEvent(i, out var change);
                    // Sampling a preview changes many bones; don't repeatedly diagnose those.
                    var obj = EditorUtility.InstanceIDToObject(change.instanceId);
                    if (obj is PosingSystem || obj is Animator ||
                        obj is VRC.SDK3.Avatars.Components.VRCAvatarDescriptor ||
                        obj is nadena.dev.modular_avatar.core.ModularAvatarMergeAnimator)
                    {
                        InvalidateAll();
                        return;
                    }
                }
            }
        }

        private static void OnEditorUpdate()
        {
            if (EditorApplication.timeSinceStartup < nextDirtyCheck) return;
            nextDirtyCheck = EditorApplication.timeSinceStartup + .25;
            CheckTrackedChanges();
        }

        // Catch direct SetDirty callers too, without serializing settings on every editor tick.
        internal static void CheckTrackedChanges()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlaying) return;
            foreach (var entry in statusByInstanceId.Values)
            {
                if (entry.Target == null || entry.Target.previousErrorCheckTime == DateTime.MinValue ||
                    PosingSystemInspectionCache.HasChanged(entry.Target) ||
                    !entry.Signature.Equals(new StatusSignature(entry.Target, hierarchyVersion, projectVersion)))
                {
                    InvalidateAll();
                    return;
                }
            }
        }

        private static void OnProjectChanged()
        {
            projectVersion++;
            InvalidateAll();
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            InvalidateAll();
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            InvalidateAll();
        }

        internal static void InvalidateAll()
        {
            PosingSystemInspectionCache.Clear();
            statusByInstanceId.Clear();
            Revision++;
            EditorApplication.RepaintHierarchyWindow();
        }

        internal static void Invalidate(PosingSystem posingSystem)
        {
            if (posingSystem == null) return;
            posingSystem.previousErrorCheckTime = DateTime.MinValue;
            statusByInstanceId.Remove(posingSystem.GetInstanceID());
            Revision++;
            EditorApplication.RepaintHierarchyWindow();
        }

        internal static void ResetForTests()
        {
            statusByInstanceId.Clear();
            hierarchyVersion = 0;
            projectVersion = 0;
            StatusEvaluationCount = 0;
        }
    }
}
