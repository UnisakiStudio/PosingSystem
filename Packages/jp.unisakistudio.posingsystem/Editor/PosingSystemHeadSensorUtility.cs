using System;
using UnityEngine;
using VRC.Dynamics;
using VRC.SDK3.Dynamics.Contact.Components;
using VRC.SDK3.Dynamics.Constraint.Components;

namespace jp.unisakistudio.posingsystemeditor
{
    /// <summary>A head-relative point measured in a frame that follows head position, but not head tilt.</summary>
    public static class PosingSystemHeadSensorUtility
    {
        public const float Span = 2.2f;
        public const string SensorRoot = "USSPS_HeadDirectionSensor";
        public static float DecodeProjection(float proximity) => proximity <= .01f ? 0 :
            Mathf.Clamp((proximity - .5f) * Span, -1, 1);

        public static float RolloverTime(float proximity, float maximumAngle, float deadZone)
        {
            var angle = Mathf.Asin(DecodeProjection(proximity)) * Mathf.Rad2Deg;
            maximumAngle = Mathf.Clamp(maximumAngle, 15, 90);
            deadZone = Mathf.Clamp(deadZone, 0, Mathf.Min(15, maximumAngle - 1));
            var magnitude = Mathf.Max(0, Mathf.Abs(angle) - deadZone) / (maximumAngle - deadZone);
            return Mathf.Clamp01(.5f + .5f * Mathf.Sign(angle) * magnitude);
        }

        public static GameObject Create(Animator animator, Transform parent)
        {
            if (animator == null || !animator.isHuman) throw new ArgumentException("Humanoidが必要です。");
            var head = animator.GetBoneTransform(HumanBodyBones.Head);
            var frame = new GameObject(SensorRoot);
            // Calibrate in avatar space; the tool object may have its own offset/scale.
            frame.transform.SetParent(animator.transform, false);
            frame.transform.rotation = animator.transform.rotation;
            frame.transform.position = head.position;
            var position = frame.AddComponent<VRCPositionConstraint>();
            position.Sources.Add(new VRCConstraintSource(head, 1));
            position.IsActive = position.Locked = true;
            position.GlobalWeight = 1;
            position.AffectsPositionX = position.AffectsPositionY = position.AffectsPositionZ = true;

            // The anatomical forward axis differs between imported rigs. Calibrate in a neutral pose.
            Vector3 localForward;
            var clone = UnityEngine.Object.Instantiate(animator.gameObject);
            clone.hideFlags = HideFlags.HideAndDontSave;
            try
            {
                var copy = clone.GetComponent<Animator>();
                using var handler = new HumanPoseHandler(copy.avatar, copy.transform);
                var neutral = new HumanPose { muscles = new float[HumanTrait.MuscleCount], bodyRotation = Quaternion.identity };
                handler.SetHumanPose(ref neutral);
                localForward = copy.GetBoneTransform(HumanBodyBones.Head).InverseTransformDirection(copy.transform.forward);
            }
            finally { UnityEngine.Object.DestroyImmediate(clone); }
            var radius = .12f * animator.humanScale / Mathf.Max(.001f, animator.transform.lossyScale.x);
            var probe = new GameObject("USSPS_HeadDirectionProbe");
            probe.transform.SetParent(frame.transform, false);
            probe.transform.rotation = head.rotation;
            var orientation = probe.AddComponent<VRCRotationConstraint>();
            orientation.Sources.Add(new VRCConstraintSource(head, 1));
            orientation.IsActive = orientation.Locked = true;
            orientation.GlobalWeight = 1;
            orientation.AffectsRotationX = orientation.AffectsRotationY = orientation.AffectsRotationZ = true;
            var sender = probe.AddComponent<VRCContactSender>();
            sender.shapeType = ContactBase.ShapeType.Sphere;
            sender.radius = radius * .0001f;
            sender.position = localForward.normalized * radius;
            sender.localOnly = true;
            var tag = "USSPS_Head_" + Guid.NewGuid().ToString("N");
            sender.collisionTags.Add(tag);
            var obj = new GameObject(PosingSystemFeatureUtility.RollSensor);
            obj.transform.SetParent(frame.transform, false);
            var receiver = obj.AddComponent<VRCContactReceiver>();
            receiver.shapeType = ContactBase.ShapeType.Box;
            receiver.size = new Vector3(radius * 3, radius * 3, radius * Span);
            receiver.rotation = Quaternion.LookRotation(Vector3.right);
            receiver.receiverType = ContactReceiver.ReceiverType.Proximity;
            receiver.useFaceProximity = true;
            receiver.allowSelf = true;
            receiver.allowOthers = false;
            receiver.localOnly = true;
            receiver.parameter = PosingSystemFeatureUtility.RollSensor;
            receiver.collisionTags.Add(tag);
            // Keep the hierarchy tidy without changing the calibrated world geometry.
            frame.transform.SetParent(parent, true);
            return frame;
        }
    }
}
