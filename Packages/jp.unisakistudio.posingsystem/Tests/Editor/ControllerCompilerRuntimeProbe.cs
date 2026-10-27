using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using VRC.SDKBase;

namespace jp.unisakistudio.posingsystemeditor.tests
{
    // Unity evaluates the actual generated graph and invokes SDK behaviours. This small bridge
    // applies SDK Set operations to its playable and records controls. It is intentionally not
    // presented as VRChat's network, IK or client scheduling implementation.
    internal sealed class ControllerCompilerRuntimeProbe : IDisposable
    {
        readonly Animator animator;
        readonly PlayableGraph graph;
        internal readonly AnimatorControllerPlayable Playable;
        readonly Dictionary<string, AnimatorControllerParameterType> types;
        readonly HashSet<VRC_AnimatorTrackingControl> trackingHooks = new();
        readonly HashSet<VRC_AnimatorLocomotionControl> locomotionHooks = new();
        readonly HashSet<VRC_AnimatorTemporaryPoseSpace> spaceHooks = new();
        readonly Dictionary<string, VRC_AnimatorTrackingControl.TrackingType> tracking = new();
        internal int AppliedDrivers;
        internal bool LocomotionDisabled, PoseSpace;

        internal ControllerCompilerRuntimeProbe(Animator target, AnimatorController controller, bool local, bool vr)
        {
            animator = target;
            types = controller.parameters.ToDictionary(p => p.name, p => p.type);
            graph = PlayableGraph.Create("Compiler full graph probe");
            graph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
            VRC_AvatarParameterDriver.OnApplySettings += ApplyDriver;
            VRC_AnimatorTrackingControl.Initialize += InitTracking;
            VRC_AnimatorLocomotionControl.Initialize += InitLocomotion;
            VRC_AnimatorTemporaryPoseSpace.Initialize += InitSpace;
            Playable = AnimatorControllerPlayable.Create(graph, controller);
            AnimationPlayableOutput.Create(graph, "Avatar", animator).SetSourcePlayable(Playable);
            Set("IsLocal", local ? 1 : 0); Set("VRMode", vr ? 1 : 0); Set("TrackingType", 3);
            Set("Grounded", 1); Set("Upright", 1);
            graph.Play();
        }
        internal void Set(string name, float value)
        {
            switch (types[name])
            {
                case AnimatorControllerParameterType.Bool: Playable.SetBool(name, value != 0); break;
                case AnimatorControllerParameterType.Int: Playable.SetInteger(name, (int)value); break;
                case AnimatorControllerParameterType.Float: Playable.SetFloat(name, value); break;
                default: throw new ArgumentException(name);
            }
        }
        internal void Tick(int frames, int fps)
        {
            for (var i = 0; i < frames; i++) graph.Evaluate(1f / fps);
        }
        internal int Layer(string name)
        {
            for (var i = 0; i < Playable.GetLayerCount(); i++) if (Playable.GetLayerName(i) == name) return i;
            throw new ArgumentException(name);
        }
        internal string Snapshot()
        {
            return "group=" + Playable.GetInteger("LocomotionTypeGroup") + ";type=" + Playable.GetInteger("LocomotionType") +
                ";pose=" + Playable.GetInteger("USSPS_Pose") + ";locomotion=" + LocomotionDisabled + ";poseSpace=" + PoseSpace +
                ";tracking=" + string.Join(",", tracking.OrderBy(p => p.Key).Select(p => p.Key + "=" + p.Value)) +
                ";states=" + string.Join(",", Enumerable.Range(0, Playable.GetLayerCount()).Select(i =>
                    Playable.GetCurrentAnimatorStateInfo(i).fullPathHash + ":" + Playable.IsInTransition(i)));
        }
        void ApplyDriver(VRC_AvatarParameterDriver driver, Animator target)
        {
            if (target != animator || (driver.localOnly && !Playable.GetBool("IsLocal"))) return;
            foreach (var p in driver.parameters)
            {
                if (p.type != VRC_AvatarParameterDriver.ChangeType.Set) throw new InvalidOperationException("Untested driver operation: " + p.type);
                if (types.ContainsKey(p.name)) Set(p.name, p.value);
            }
            AppliedDrivers++;
        }
        void InitTracking(VRC_AnimatorTrackingControl c) { if (trackingHooks.Add(c)) c.ApplySettings += ApplyTracking; }
        void InitLocomotion(VRC_AnimatorLocomotionControl c) { if (locomotionHooks.Add(c)) c.ApplySettings += ApplyLocomotion; }
        void InitSpace(VRC_AnimatorTemporaryPoseSpace c) { if (spaceHooks.Add(c)) c.ApplySettings += ApplySpace; }
        void ApplyTracking(VRC_AnimatorTrackingControl c, Animator target)
        {
            if (target != animator) return;
            Update("head", c.trackingHead); Update("leftHand", c.trackingLeftHand); Update("rightHand", c.trackingRightHand);
            Update("hip", c.trackingHip); Update("leftFoot", c.trackingLeftFoot); Update("rightFoot", c.trackingRightFoot);
            Update("leftFingers", c.trackingLeftFingers); Update("rightFingers", c.trackingRightFingers);
            Update("eyes", c.trackingEyes); Update("mouth", c.trackingMouth);
            void Update(string key, VRC_AnimatorTrackingControl.TrackingType value)
            { if (value != VRC_AnimatorTrackingControl.TrackingType.NoChange) tracking[key] = value; }
        }
        void ApplyLocomotion(VRC_AnimatorLocomotionControl c, Animator target) { if (target == animator) LocomotionDisabled = c.disableLocomotion; }
        void ApplySpace(VRC_AnimatorTemporaryPoseSpace c, Animator target) { if (target == animator) PoseSpace = c.enterPoseSpace; }
        public void Dispose()
        {
            if (graph.IsValid()) graph.Destroy();
            VRC_AvatarParameterDriver.OnApplySettings -= ApplyDriver;
            VRC_AnimatorTrackingControl.Initialize -= InitTracking;
            VRC_AnimatorLocomotionControl.Initialize -= InitLocomotion;
            VRC_AnimatorTemporaryPoseSpace.Initialize -= InitSpace;
            foreach (var c in trackingHooks) if (c != null) c.ApplySettings -= ApplyTracking;
            foreach (var c in locomotionHooks) if (c != null) c.ApplySettings -= ApplyLocomotion;
            foreach (var c in spaceHooks) if (c != null) c.ApplySettings -= ApplySpace;
        }
    }
}
