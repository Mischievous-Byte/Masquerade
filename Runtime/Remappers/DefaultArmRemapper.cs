using MischievousByte.Masquerade.Anatomy;
using MischievousByte.Masquerade.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows;

namespace MischievousByte.Masquerade.Remappers
{
    /// <summary>
    /// This remapper doesn't use shoulders, as that would be to implementation specific
    /// </summary>
    public static class DefaultArmRemapper
    {
        [Remapper(CommonGroupings.LeftArm)]
        public static void RemapLeftArm(
            in BodyTree<Matrix4x4> source,
            ref BodyTree<Matrix4x4> destination)
        {
            RemapArm(source, ref destination, LeftRight.Left);
        }

        [Remapper(CommonGroupings.LeftArm)]
        public static void RemapRightArm(
            in BodyTree<Matrix4x4> source,
            ref BodyTree<Matrix4x4> destination)
        {
            RemapArm(source, ref destination, LeftRight.Right);
        }


        private static readonly Quaternion leftUpperArmCorrection = Quaternion.Euler(180f, 90f, 0);
        private static readonly Quaternion leftForearmCorrection = Quaternion.Euler(180f, 90f, 0);
        private static readonly Quaternion leftWristCorrection = Quaternion.Euler(-90f, 90f, 0);

        private static readonly Quaternion rightUpperArmCorrection = Quaternion.Euler(0, -90f, 0);
        private static readonly Quaternion rightForearmCorrection = Quaternion.Euler(0, -90f, 0);
        private static readonly Quaternion rightWristCorrection = Quaternion.Euler(-90f, -90f, 0);

        public static void RemapArm(in BodyTree<Matrix4x4> source, ref BodyTree<Matrix4x4> destination, LeftRight side)
        {
            BodyNode clavicle = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;

            //Copy shoulder rotation
            destination[clavicle] = Matrix4x4.TRS(destination[clavicle].GetPosition(), source[clavicle].rotation, destination[clavicle].lossyScale);

            //Calculate new wrist position
            Pose remappedPoint = CalculateRemappedWristLocation(in source, in destination, side);

            //Generate hint based on source
            Vector3 hint = GenerateHint(in source, in destination, remappedPoint.position, side);

            //Solve
            SolveArm(ref destination, remappedPoint, hint, side);
        }

        /// <summary></summary>
        /// <param name="source"></param>
        /// <param name="destination"></param>
        /// <param name="side"></param>
        /// <returns>A point in world space where the wrist should be located</returns>
        private static Pose CalculateRemappedWristLocation(in BodyTree<Matrix4x4> source, in BodyTree<Matrix4x4> destination, LeftRight side)
        {
            source.ChangeSpace(Space.World, out var worldSource);
            destination.ChangeSpace(Space.World, out var worldDestination);

            BodyNode wrist = side == LeftRight.Left ? BodyNode.LeftWrist : BodyNode.RightWrist;

            Bounds sourceBounds = GetReachBounds(in source, side);
            Bounds destinationBounds = GetReachBounds(in destination, side);


            Vector3 sourceWristLocationT7 = worldSource[BodyNode.T7].inverse.MultiplyPoint(worldSource[wrist].GetPosition());

            Vector3 normalizedWristLocation = new Vector3(
                Mathf.InverseLerp(sourceBounds.min.x, sourceBounds.max.x, sourceWristLocationT7.x),
                Mathf.InverseLerp(sourceBounds.min.y, sourceBounds.max.y, sourceWristLocationT7.y),
                Mathf.InverseLerp(sourceBounds.min.z, sourceBounds.max.z, sourceWristLocationT7.z));


            Vector3 destinationWristLocationT7 = new Vector3(
                Mathf.Lerp(destinationBounds.min.x, destinationBounds.max.x, normalizedWristLocation.x),
                Mathf.Lerp(destinationBounds.min.y, destinationBounds.max.y, normalizedWristLocation.y),
                Mathf.Lerp(destinationBounds.min.z, destinationBounds.max.z, normalizedWristLocation.z));

            Vector3 worldDestinationWrist = worldDestination[BodyNode.T7].MultiplyPoint(destinationWristLocationT7);

            return new(worldDestinationWrist, worldSource[wrist].rotation);
        }

        /// <summary>
        /// T7, No shoulder
        /// </summary>
        /// <returns></returns>
        /// 
        private static Bounds GetReachBounds(in BodyTree<Matrix4x4> tree, LeftRight side)
        {
            BodyNode clavicle = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;
            BodyNode scapula = clavicle.Next();
            BodyNode upperArm = scapula.Next();
            BodyNode forearm = upperArm.Next();
            BodyNode wrist = forearm.Next();

            tree.ChangeSpace(Space.World, out var worldTree);

            float dynamicLength =
                tree[wrist].GetPosition().magnitude
                + tree[forearm].GetPosition().magnitude;

            Vector3 anchor = (tree[clavicle] * tree[scapula] * tree[upperArm]).GetPosition();

            Vector3 forward = anchor + Vector3.forward * dynamicLength;
            Vector3 back = anchor + Vector3.back * dynamicLength;
            Vector3 left = anchor + Vector3.left * dynamicLength;
            Vector3 right = anchor + Vector3.right * dynamicLength;
            Vector3 up = anchor + Vector3.up * dynamicLength;
            Vector3 down = anchor + Vector3.down * dynamicLength;

            Bounds bounds = new();

            bounds.SetMinMax(
                new Vector3(left.x, down.y, back.z),
                new Vector3(right.x, up.y, forward.z));

            return bounds;
        }

        private static Vector3 GenerateHint(in BodyTree<Matrix4x4> source, in BodyTree<Matrix4x4> destination, Vector3 targetPosition, LeftRight side)
        {

            BodyNode clavicleNode = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;
            BodyNode upperArmNode = clavicleNode + 1;
            BodyNode forearmNode = upperArmNode + 1;
            BodyNode wristNode = forearmNode + 1;


            source.ChangeSpace(Space.World, out var worldSource);
            destination.ChangeSpace(Space.World, out var worldDestination);

            Vector3 GetPlaneNormal()
            {
                Vector3 root = worldSource[upperArmNode].GetPosition();
                Vector3 middle = worldSource[forearmNode].GetPosition();
                Vector3 end = worldSource[wristNode].GetPosition();

                Vector3 a = middle - root;
                Vector3 b = end - middle;

                Vector3 normal = Vector3.Cross(a, b).normalized;

                return normal;
            }

            Vector3 localRoot = worldDestination[upperArmNode].GetPosition();

            Vector3 localTarget = targetPosition;

            Vector3 localNormal = GetPlaneNormal();

            Vector3 localRootToTarget = localTarget - localRoot;

            Vector3 hintDirection = Vector3.Cross(localNormal, localRootToTarget).normalized;

            Vector3 halfway = Vector3.Lerp(localRoot, localTarget, 0.5f);

            return halfway - hintDirection;

        }


        private static void SolveArm(ref BodyTree<Matrix4x4> tree, Pose target, Vector3 hint, LeftRight side)
        {
            BodyNode clavicle = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;
            BodyNode scapula = clavicle.Next();
            BodyNode upperArm = scapula.Next();
            BodyNode forearm = upperArm.Next();
            BodyNode wrist = forearm.Next();

            tree.ChangeSpace(Space.World, out var worldTree);
            Matrix4x4 worldClavicle = worldTree[clavicle];

            float segmentALength = tree[forearm].GetPosition().magnitude;
            float segmentBLength = tree[wrist].GetPosition().magnitude;

            float totalLength = segmentALength + segmentBLength;

            Matrix4x4 worldUpperArm = worldClavicle * tree[upperArm];


            target.position = worldUpperArm.GetPosition() + Vector3.ClampMagnitude(target.position - worldUpperArm.GetPosition(), totalLength * 0.999f);

            float distance = (target.position - worldUpperArm.GetPosition()).magnitude;

            Vector3 normal = Vector3.Cross(worldUpperArm.GetPosition() - hint, target.position - hint).normalized;

            tree[upperArm] = Matrix4x4.TRS(
                tree[upperArm].GetPosition(),
                worldClavicle.inverse.rotation
                * Quaternion.LookRotation(target.position - worldUpperArm.GetPosition(), normal),
                Vector3.one);

            worldUpperArm = worldClavicle * tree[upperArm];

            float cos = (segmentBLength * segmentBLength - segmentALength * segmentALength - distance * distance) /
                    (-2 * distance * segmentALength);
            cos = Mathf.Clamp(cos, -1, 1); //Clamp because of float precision errors


            float angle = -Mathf.Acos(cos) * Mathf.Rad2Deg;

            tree[upperArm] = Matrix4x4.TRS(tree[upperArm].GetPosition(), worldClavicle.inverse.rotation * (Quaternion.AngleAxis(-angle, normal) * worldUpperArm.rotation), Vector3.one);
            tree[upperArm] = Matrix4x4.TRS(tree[upperArm].GetPosition(), tree[upperArm].rotation * (side == LeftRight.Left ? leftUpperArmCorrection : rightUpperArmCorrection), Vector3.one);

            worldUpperArm = worldClavicle * tree[upperArm];


            Matrix4x4 worldForearm = worldUpperArm * tree[forearm];
            tree[forearm] = Matrix4x4.TRS(tree[forearm].GetPosition(), worldUpperArm.inverse.rotation * Quaternion.LookRotation(target.position - worldForearm.GetPosition(), normal), Vector3.one);

            tree[forearm] = Matrix4x4.TRS(tree[forearm].GetPosition(), tree[forearm].rotation * (side == LeftRight.Left ? leftForearmCorrection : rightForearmCorrection), Vector3.one);

            worldForearm = worldUpperArm * tree[forearm];
            tree[wrist] = Matrix4x4.TRS(tree[wrist].GetPosition(), worldForearm.inverse.rotation * (target.rotation), Vector3.one);
        }
    }
}
