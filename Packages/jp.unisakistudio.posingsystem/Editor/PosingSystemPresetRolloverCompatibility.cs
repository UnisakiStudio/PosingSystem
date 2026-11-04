using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEditor.Presets;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    // A pre-rollover Preset replaces the pose arrays without knowing these newer fields.
    // Copy only missing rollover settings, matched by pose identity rather than array index.
    internal sealed class PosingSystemPresetRolloverCompatibility
    {
        private readonly HashSet<string> propertyPaths;
        private readonly List<PoseSettings> current;
        private readonly List<PoseSettings> defaults;

        internal PosingSystemPresetRolloverCompatibility(PosingSystem target, Preset preset)
        {
            propertyPaths = ReadSavedPropertyPaths(preset);
            current = Capture(target);
            // Product defaults only fill poses absent from the current configuration.
            // Do not search/load unrelated products or every preset in the project.
            defaults = Capture(PrefabUtility.GetCorrespondingObjectFromSource(target));
        }

        private static HashSet<string> ReadSavedPropertyPaths(Preset preset)
        {
            // Unity adds current-schema defaults to both PropertyModifications and SerializedObject
            // while loading a Preset. Only the saved text distinguishes "absent" from explicit OFF.
            // Read just the selected preset on application; never scan preset files during listing.
            var path = AssetDatabase.GetAssetPath(preset);
            if (!EditorUtility.IsDirty(preset) && !string.IsNullOrEmpty(path)
                && AssetDatabase.TryGetGUIDAndLocalFileIdentifier(preset, out string guid, out long localId))
            {
                try
                {
                    using var reader = new StreamReader(path);
                    if (reader.ReadLine()?.StartsWith("%YAML ", StringComparison.Ordinal) == true)
                    {
                        var header = "--- !u!181963792 &" + localId;
                        var found = false;
                        var saved = new HashSet<string>(StringComparer.Ordinal);
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.StartsWith("--- !u!", StringComparison.Ordinal))
                            {
                                if (found) return saved;
                                found = line.TrimEnd() == header;
                            }
                            if (!found) continue;
                            var trimmed = line.TrimStart();
                            const string propertyPrefix = "propertyPath: ";
                            if (trimmed.StartsWith(propertyPrefix, StringComparison.Ordinal))
                                saved.Add(trimmed.Substring(propertyPrefix.Length).Trim());
                        }
                        if (found) return saved;
                    }
                }
                catch (IOException) { }
                catch (UnauthorizedAccessException) { }
            }
            // Unsaved edits and non-text presets cannot be classified as legacy safely.
            return new HashSet<string>(preset.PropertyModifications.Select(p => p.propertyPath), StringComparer.Ordinal);
        }

        internal void RestoreMissingSettings(PosingSystem target)
        {
            if (target.defines == null) return;
            for (var layerIndex = 0; layerIndex < target.defines.Count; layerIndex++)
            {
                var layer = target.defines[layerIndex];
                if (layer?.animations == null) continue;
                for (var poseIndex = 0; poseIndex < layer.animations.Count; poseIndex++)
                {
                    var pose = layer.animations[poseIndex];
                    if (pose == null || pose.animationClip == null || pose.isMotionTime) continue;
                    var path = $"defines.Array.data[{layerIndex}].animations.Array.data[{poseIndex}].";
                    // Any recorded rollover field identifies a newer preset. Its OFF/null values
                    // and the older complete-animation rollover format must remain authoritative.
                    if (propertyPaths.Contains(path + "isRollover")
                        || propertyPaths.Contains(path + "rolloverSideAnimation")
                        || propertyPaths.Contains(path + "rolloverContactStrength")) continue;

                    var source = Match(current, layer, pose);
                    if (source == null) source = Match(defaults, layer, pose);
                    if (source == null) continue;
                    pose.isRollover = source.enabled;
                    pose.rolloverSideAnimation = source.side;
                    pose.rolloverContactStrength = source.strength;
                }
            }
        }

        private static List<PoseSettings> Capture(PosingSystem target)
        {
            var result = new List<PoseSettings>();
            if (target == null || target.defines == null) return result;
            foreach (var layer in target.defines)
            {
                if (layer?.animations == null) continue;
                foreach (var pose in layer.animations)
                    if (pose != null && pose.animationClip != null)
                        result.Add(new PoseSettings(layer, pose));
            }
            return result;
        }

        private static PoseSettings Match(List<PoseSettings> candidates, PosingSystem.LayerDefine layer,
            PosingSystem.AnimationDefine pose)
        {
            var matches = candidates.Where(p => p.clip == pose.animationClip
                && p.stateMachine == layer.stateMachineName && p.parameter == layer.paramName).ToList();
            if (matches.Count == 1) return matches[0];
            // Reused animations can have different settings. Only use a unique name match;
            // never choose the first of several indistinguishable poses.
            var named = matches.Where(p => p.name == pose.displayName).ToList();
            return named.Count == 1 ? named[0] : null;
        }

        private sealed class PoseSettings
        {
            internal readonly string stateMachine, parameter, name;
            internal readonly Motion clip;
            internal readonly bool enabled;
            internal readonly AnimationClip side;
            internal readonly float strength;

            internal PoseSettings(PosingSystem.LayerDefine layer, PosingSystem.AnimationDefine pose)
            {
                stateMachine = layer.stateMachineName;
                parameter = layer.paramName;
                name = pose.displayName;
                clip = pose.animationClip;
                enabled = pose.isRollover;
                side = pose.rolloverSideAnimation;
                strength = pose.rolloverContactStrength;
            }
        }
    }
}
