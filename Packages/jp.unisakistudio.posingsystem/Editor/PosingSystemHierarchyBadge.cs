using System;
using System.Collections.Generic;
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

            internal StatusSignature(PosingSystem posingSystem, int currentHierarchyVersion, int currentProjectVersion)
            {
                var avatar = posingSystem.GetAvatar();
                var animator = avatar != null ? avatar.GetComponent<Animator>() : null;
                var animatorAvatar = animator != null ? animator.avatar : null;
                var baseController = avatar != null
                    ? avatar.baseAnimationLayers[(int)VRC.SDK3.Avatars.Components.VRCAvatarDescriptor.AnimLayerType.Base].animatorController
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
                    && projectVersion == other.projectVersion;
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
                    return (hash * 397) ^ projectVersion;
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

            internal BadgeStatus(StatusSignature signature, bool isWarning, bool isError)
            {
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

        internal static int StatusEvaluationCount { get; private set; }

        static PosingSystemHierarchyBadge()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyGUI;
            EditorApplication.hierarchyChanged += OnHierarchyChanged;
            EditorApplication.projectChanged += OnProjectChanged;
            Undo.undoRedoPerformed += InvalidateAll;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnHierarchyGUI(int instanceID, Rect selectionRect)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (obj == null || !obj.TryGetComponent<PosingSystem>(out var posingSystem))
            {
                statusByInstanceId.Remove(instanceID);
                return;
            }

            EnsureIcons();
            var status = GetStatus(posingSystem);

            if (status.IsWarning)
            {
                GUI.Label(selectionRect, warningIcon);
            }

            if (status.IsError)
            {
                GUI.Label(selectionRect, errorIcon);
            }
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
                && cachedStatus.Signature.Equals(signature))
            {
                return (cachedStatus.IsWarning, cachedStatus.IsError);
            }

            var isWarning = PosingSystemConverter.HasWarning(posingSystem);
            var isError = PosingSystemConverter.HasError(posingSystem);
            StatusEvaluationCount++;

            posingSystem.isWarning = isWarning;
            posingSystem.isError = isError;
            posingSystem.previousErrorCheckTime = DateTime.Now;
            statusByInstanceId[instanceId] = new BadgeStatus(signature, isWarning, isError);
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
            statusByInstanceId.Clear();
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
