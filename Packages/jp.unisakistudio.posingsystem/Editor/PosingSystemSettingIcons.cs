using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    /// <summary>Illustrated settings icons. Editor-only assets are never included in avatars.</summary>
    internal static class PosingSystemSettingIcons
    {
        private const string Directory = "Packages/jp.unisakistudio.posingsystem/Editor/Icons/Settings/";
        private static readonly Dictionary<string, Texture2D> Textures = new Dictionary<string, Texture2D>();

        internal static void Draw(Rect rect, string setting, jp.unisakistudio.posingsystem.PosingSystem ps = null)
        {
            if (Event.current.type != EventType.Repaint) return;
            if (ps != null && PosingSystemFeatureUtility.IsMenuFeatureSetting(setting))
            {
                var menuIcon = PosingSystemFeatureUtility.GetFeatureMenuIcon(ps, setting);
                if (menuIcon != null)
                {
                    // Menu icons need a contrasting surface in both editor themes.
                    EditorGUI.DrawRect(rect, new Color(.20f, .24f, .29f, 1));
                    var previousColor = GUI.color;
                    try
                    {
                        GUI.color = Color.white;
                        GUI.DrawTexture(rect, menuIcon, ScaleMode.ScaleToFit, true);
                    }
                    finally { GUI.color = previousColor; }
                }
                return;
            }
            if (!Textures.TryGetValue(setting, out var texture) || texture == null)
            {
                var name = Filename(setting);
                if (name == null) return;
                texture = AssetDatabase.LoadAssetAtPath<Texture2D>(Directory + name + ".png");
                Textures[setting] = texture;
            }
            if (texture == null) return;

            // Keep the illustration legible on both editor themes, independently of toggle state.
            var previous = GUI.color;
            try
            {
                GUI.color = Color.white;
                GUI.DrawTexture(rect, texture, ScaleMode.ScaleToFit, true);
            }
            finally { GUI.color = previous; }
        }

        private static string Filename(string setting)
        {
            switch (setting)
            {
                case "animation-adjustment": return "animation-adjustment";
                case "features.mirror": return "mirror";
                case "features.footHeight": return "foot-height";
                case "features.breathingControl":
                case "features.breathing": return "breathing";
                case "features.fingerGestureControl": return "finger-lock";
                case "features.faceForward": return "face-forward";
                case "features.photographyKeepHeadVisible": return "photography-head-visible";
                case "features.photographySynced":
                case "features.photography": return "photography";
                case "features.floating": return "floating";
                case "features.rollover": return "rollover";
                case "isIconDisabled": return "pose-gallery";
                case "isIconSmall": return "small-images";
                case "mergeTrackingControl": return "gimmick-compatibility";
                case "autoImportAvatarAnimations": return "avatar-poses";
                case "additiveBreathing.disableDefaultIdle": return "idle-stop";
                case "enableAvatarSpecificAdjustment": return "avatar-adjustment";
                case "enableReverseKneeSafetyClamp": return "knee-safety";
                default: return null;
            }
        }
    }
}
