using System;
using System.IO;
using System.Linq;
using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase.Editor.BuildPipeline;

namespace jp.unisakistudio.posingsystemeditor
{
    internal sealed class LegacyRolloverPoseMigration : IVRCSDKBuildRequestedCallback
    {
        // The 3.0 Kawaii component also exists on fully unpacked avatars.
        private const string ProductGuid = "ebbb07b93f9f2a6409f9eb86f5f4a75c";

        public int callbackOrder => 0;
        public bool OnBuildRequested(VRCSDKRequestedBuildType type)
        {
            if (type == VRCSDKRequestedBuildType.Avatar) ApplyToOpenScenes();
            return true;
        }

        internal static void ApplyToOpenScenes()
        {
            for (var i = 0; i < SceneManager.sceneCount; i++) ApplyToScene(SceneManager.GetSceneAt(i));
        }

        internal static void ApplyToScene(Scene scene)
        {
            if (!scene.isLoaded || EditorSceneManager.IsPreviewScene(scene)) return;
            foreach (var root in scene.GetRootGameObjects()) ApplyToHierarchy(root);
        }

        internal static void ApplyToHierarchy(GameObject root)
        {
            foreach (var system in root.GetComponentsInChildren<PosingSystem>(true)) Apply(system);
        }

        internal static void Apply(PosingSystem system)
        {
            if (!system || system.legacyRolloverPosesMigrated || system.defines == null ||
                !system.gameObject.scene.IsValid() || EditorUtility.IsPersistent(system) ||
                EditorSceneManager.IsPreviewScene(system.gameObject.scene)) return;
            var path = AssetDatabase.GUIDToAssetPath(ProductGuid);
            if (string.IsNullOrEmpty(path)) return;
            var product = AssetDatabase.LoadAssetAtPath<GameObject>(path)?.GetComponentInChildren<PosingSystem>(true);
            if (!product || system.GetType() != product.GetType()) return;

            // Read overrides before cloning/NDMF strips prefab provenance. A saved instance
            // can have overrides at both the avatar-prefab level and the scene level.
            var source = PrefabUtility.GetCorrespondingObjectFromOriginalSource(system);
            if (source != product && !SavedWithoutRolloverFields(source ? source : system)) return;

            var changed = false;
            for (int g = 0; g < system.defines.Count; g++)
            {
                var group = system.defines[g];
                if (group?.animations == null) continue;
                bool SameGroup(PosingSystem.LayerDefine d) => d?.animations != null &&
                    d.paramName == group.paramName && d.stateMachineName == group.stateMachineName;
                var originals = product.defines.Where(SameGroup).SelectMany(d => d.animations).ToList();
                // Other groups had no rollover feature to migrate.
                if (!originals.Any(p => p != null && p.isRollover)) continue;
                var byClip = originals.Where(p => p?.animationClip != null).ToLookup(p => p.animationClip);
                var uses = system.defines.Where(SameGroup).SelectMany(d => d.animations)
                    .Where(p => p?.animationClip != null).ToLookup(p => p.animationClip);
                var slot = product.defines.ElementAtOrDefault(g);
                for (int i = 0; i < group.animations.Count; i++)
                {
                    var pose = group.animations[i];
                    if (pose == null) continue;
                    var matches = byClip[pose.animationClip];
                    // A custom row can reuse a stock clip. Never spread settings across ambiguous rows.
                    bool ambiguous = matches.Count() > 1 || uses[pose.animationClip].Count() > 1;
                    if (ambiguous && source == product && SameGroup(slot) &&
                        slot.animations?.ElementAtOrDefault(i)?.animationClip == pose.animationClip) continue;
                    var original = !ambiguous && !pose.isMotionTime ? matches.FirstOrDefault() : null;
                    bool enabled = original?.isRollover ?? false;
                    var side = original?.rolloverSideAnimation;
                    float strength = original?.rolloverContactStrength ?? .25f;
                    string prefix = $"defines.Array.data[{g}].animations.Array.data[{i}].";
                    bool Inherited(string field) => !HasOverride(system, product, prefix + field);
                    if (pose.isRollover != enabled && Inherited(nameof(pose.isRollover)))
                    {
                        pose.isRollover = enabled;
                        changed = true;
                    }
                    if (pose.rolloverSideAnimation != side && Inherited(nameof(pose.rolloverSideAnimation)))
                    {
                        pose.rolloverSideAnimation = side;
                        changed = true;
                    }
                    if (pose.rolloverContactStrength != strength && Inherited(nameof(pose.rolloverContactStrength)))
                    {
                        pose.rolloverContactStrength = strength;
                        changed = true;
                    }
                }
            }
            if (!changed) return;
            system.legacyRolloverPosesMigrated = true;
            PosingSystemEditorChangeUtility.MarkChanged(system);
        }

        private static bool HasOverride(PosingSystem instance, PosingSystem product, string path)
        {
            for (var owner = instance; owner && owner != product; owner = PrefabUtility.GetCorrespondingObjectFromSource(owner))
            {
                using var serialized = new SerializedObject(owner);
                if (serialized.FindProperty(path)?.prefabOverride == true) return true;
            }
            return false;
        }

        // OFF/null is also a valid modern configuration. A fully unpacked object has no
        // prefab overrides, so only migrate if its own saved MonoBehaviour predates the fields.
        private static bool SavedWithoutRolloverFields(PosingSystem source)
        {
            var id = GlobalObjectId.GetGlobalObjectIdSlow(source);
            string path = AssetDatabase.GUIDToAssetPath(id.assetGUID.ToString());
            if (id.targetObjectId == 0 || string.IsNullOrEmpty(path) || !File.Exists(path)) return false;
            try
            {
                using var reader = File.OpenText(path);
                if (reader.ReadLine()?.StartsWith("%YAML ", StringComparison.Ordinal) != true) return false;
                string header = "--- !u!114 &" + unchecked((long)id.targetObjectId);
                bool found = false, hasDefines = false;
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("--- !u!", StringComparison.Ordinal))
                    {
                        if (found) break;
                        found = line == header;
                    }
                    else if (found)
                    {
                        if (line == "  defines:") hasDefines = true;
                        if (line.TrimStart().StartsWith("isRollover:", StringComparison.Ordinal)) return false;
                    }
                }
                return found && hasDefines;
            }
            catch (IOException) { return false; }
            catch (UnauthorizedAccessException) { return false; }
        }
    }
}
