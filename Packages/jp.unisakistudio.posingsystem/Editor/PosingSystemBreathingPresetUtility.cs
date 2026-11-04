using System;
using jp.unisakistudio.posingsystem;
using UnityEditor;
using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    /// <summary>Authoring recipe for the bundled muscle-only additive breathing clips.</summary>
    public static class PosingSystemBreathingPresetUtility
    {
        // Explicit authoring only: never rewrite shipped or user assets on import/build.
        public static AnimationClip Create(PosingSystem.BreathingLevel level)
        {
            if (level == PosingSystem.BreathingLevel.None || level == PosingSystem.BreathingLevel.Custom)
                throw new ArgumentOutOfRangeException(nameof(level));

            const float weakScale = .55f;
            const float strongScale = 1.6f;
            var scale = level == PosingSystem.BreathingLevel.Strong ? strongScale
                : level == PosingSystem.BreathingLevel.Weak ? weakScale
                : level == PosingSystem.BreathingLevel.Rapid ? 1.35f
                : (weakScale + strongScale) * .5f;
            var period = level == PosingSystem.BreathingLevel.Rapid ? 1.8f : 5f;
            var clip = new AnimationClip { name = "USSPS_Breathing_" + level, frameRate = 60 };

            // Share the counter bend between back and neck to reduce head translation as well as tilt.
            Add("Spine Front-Back", -.0626667f);
            Add("Chest Front-Back", .054f);
            Add("UpperChest Front-Back", .084f);
            // Counter the chest's pitch; unrelated neck sway would move a resting head.
            Add("Neck Nod Down-Up", -.0333333f);
            foreach (var side in new[] { "Left ", "Right " })
            {
                Add(side + "Shoulder Front-Back", -.042f);
                // Small arm counter-rotation reduces hand drift across standing/seated/lying poses.
                // This is a portable preset, not an IK pin: residual motion depends on the avatar.
                Add(side + "Arm Front-Back", .0176f);
                Add(side + "Arm Down-Up", -.0047f);
                Add(side + "Arm Twist In-Out", -.00352f);
                Add(side + "Forearm Stretch", -.0038f);
                Add(side + "Upper Leg Front-Back", .002f, true);
                Add(side + "Lower Leg Stretch", -.0026667f, true);
                Add(side + "Foot Up-Down", .0013333f, true);
            }

            var settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = true;
            settings.startTime = 0;
            settings.stopTime = period;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            return clip;

            void Add(string muscle, float peak, bool keepSubtle = false)
            {
                // Larger breaths keep the same small leg motion to avoid sliding feet.
                var amplitude = peak * (keepSubtle ? Mathf.Min(scale, 1f) : scale);
                // Shorter inhale, longer exhale. Flat tangents keep the turnaround and loop smooth.
                var curve = new AnimationCurve(
                    new Keyframe(0, 0, 0, 0),
                    new Keyframe(period * .36f, amplitude, 0, 0),
                    new Keyframe(period, 0, 0, 0));
                AnimationUtility.SetEditorCurve(clip,
                    EditorCurveBinding.FloatCurve("", typeof(Animator), muscle), curve);
            }
        }
    }
}
