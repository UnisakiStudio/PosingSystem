using UnityEngine;

namespace jp.unisakistudio.posingsystemeditor
{
    public readonly struct PosingSystemTwoBoneIkSolution
    {
        public PosingSystemTwoBoneIkSolution(
            Vector3 jointPosition,
            Vector3 tipPosition,
            float reachError,
            bool targetClamped)
        {
            JointPosition = jointPosition;
            TipPosition = tipPosition;
            ReachError = reachError;
            TargetClamped = targetClamped;
        }

        public Vector3 JointPosition { get; }
        public Vector3 TipPosition { get; }
        public float ReachError { get; }
        public bool TargetClamped { get; }
    }

    public static class PosingSystemTwoBoneIk
    {
        private const float Epsilon = 0.000001f;

        public static bool TrySolve(
            Vector3 rootPosition,
            Vector3 jointPosition,
            Vector3 tipPosition,
            Vector3 targetPosition,
            Vector3 polePosition,
            out PosingSystemTwoBoneIkSolution solution)
        {
            var upperLength = Vector3.Distance(rootPosition, jointPosition);
            var lowerLength = Vector3.Distance(jointPosition, tipPosition);
            if (upperLength <= Epsilon || lowerLength <= Epsilon)
            {
                solution = default;
                return false;
            }

            var targetVector = targetPosition - rootPosition;
            var targetDistance = targetVector.magnitude;
            var targetDirection = targetDistance > Epsilon
                ? targetVector / targetDistance
                : FindFallbackDirection(rootPosition, jointPosition, tipPosition);

            var solvedDistance = Mathf.Clamp(
                targetDistance,
                Mathf.Abs(upperLength - lowerLength),
                upperLength + lowerLength);
            var solvedTip = rootPosition + targetDirection * solvedDistance;

            var bendDirection = Vector3.ProjectOnPlane(
                polePosition - rootPosition,
                targetDirection);
            if (bendDirection.sqrMagnitude <= Epsilon * Epsilon)
            {
                bendDirection = Vector3.ProjectOnPlane(
                    jointPosition - rootPosition,
                    targetDirection);
            }
            if (bendDirection.sqrMagnitude <= Epsilon * Epsilon)
            {
                var fallbackAxis = Mathf.Abs(Vector3.Dot(targetDirection, Vector3.up)) < 0.99f
                    ? Vector3.up
                    : Vector3.right;
                bendDirection = Vector3.ProjectOnPlane(fallbackAxis, targetDirection);
            }
            bendDirection.Normalize();

            var alongTarget = solvedDistance <= Epsilon
                ? upperLength
                : (upperLength * upperLength + solvedDistance * solvedDistance - lowerLength * lowerLength) /
                  (2f * solvedDistance);
            var bendHeight = Mathf.Sqrt(Mathf.Max(
                0f,
                upperLength * upperLength - alongTarget * alongTarget));
            var solvedJoint = rootPosition
                              + targetDirection * alongTarget
                              + bendDirection * bendHeight;
            var reachError = Vector3.Distance(targetPosition, solvedTip);

            solution = new PosingSystemTwoBoneIkSolution(
                solvedJoint,
                solvedTip,
                reachError,
                reachError > Epsilon);
            return true;
        }

        public static bool TryApply(
            Transform root,
            Transform joint,
            Transform tip,
            Vector3 targetPosition,
            Vector3 polePosition,
            float weight,
            out PosingSystemTwoBoneIkSolution solution)
        {
            solution = default;
            if (root == null || joint == null || tip == null ||
                !TrySolve(
                    root.position,
                    joint.position,
                    tip.position,
                    targetPosition,
                    polePosition,
                    out solution))
            {
                return false;
            }

            weight = Mathf.Clamp01(weight);
            if (weight <= 0f)
            {
                return true;
            }

            var currentUpperDirection = joint.position - root.position;
            var solvedUpperDirection = solution.JointPosition - root.position;
            if (currentUpperDirection.sqrMagnitude > Epsilon * Epsilon &&
                solvedUpperDirection.sqrMagnitude > Epsilon * Epsilon)
            {
                var solvedRotation =
                    Quaternion.FromToRotation(currentUpperDirection, solvedUpperDirection) * root.rotation;
                root.rotation = Quaternion.Slerp(root.rotation, solvedRotation, weight);
            }

            var currentLowerDirection = tip.position - joint.position;
            var solvedLowerDirection = solution.TipPosition - joint.position;
            if (currentLowerDirection.sqrMagnitude > Epsilon * Epsilon &&
                solvedLowerDirection.sqrMagnitude > Epsilon * Epsilon)
            {
                var solvedRotation =
                    Quaternion.FromToRotation(currentLowerDirection, solvedLowerDirection) * joint.rotation;
                joint.rotation = Quaternion.Slerp(joint.rotation, solvedRotation, weight);
            }

            return true;
        }

        private static Vector3 FindFallbackDirection(
            Vector3 rootPosition,
            Vector3 jointPosition,
            Vector3 tipPosition)
        {
            var tipDirection = tipPosition - rootPosition;
            return tipDirection.sqrMagnitude > Epsilon * Epsilon
                ? tipDirection.normalized
                : (jointPosition - rootPosition).normalized;
        }
    }
}
