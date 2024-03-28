using Codice.Client.Common.TreeGrouper;
using MischievousByte.Masquerade.Anatomy;
using MischievousByte.Masquerade.Utility;
using PlasticGui.WorkspaceWindow;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Remappers
{
    public static class DefaultArmRemapper
    {
        [Remapper(CommonNodeGroupings.LeftArm)]
        public static void RemapLeftArm(
            in BodyTree<Matrix4x4> source,
            ref BodyTree<Matrix4x4> destination)
        {
            
        }

        [Remapper(CommonNodeGroupings.LeftArm)]
        public static void RemapRightArm(
            in BodyTree<Matrix4x4> source,
            ref BodyTree<Matrix4x4> destination)
        {

        }


        public static void RemapArm(in BodyTree<Matrix4x4> source, ref BodyTree<Matrix4x4> destination, LeftRight side)
        {
            BodyNode upperArm = side == LeftRight.Left ? BodyNode.LeftUpperArm : BodyNode.RightUpperArm;

            Pose remappedPoint = CalculateRemappedWristLocation(in source, in destination, side);

            //SolveArm(side, ref matrices, Pose.identity, Vector3.zero);

        }


        private static Pose CalculateRemappedWristLocation(in BodyTree<Matrix4x4> source, in BodyTree<Matrix4x4> destination, LeftRight side)
        {
            BodyNode clavicle = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;
            BodyNode scapula = clavicle.Next();
            BodyNode upperArm = scapula.Next();
            BodyNode forearm = upperArm.Next();
            BodyNode wrist = forearm.Next();
            BodyNode palm = wrist.Next();


            return new Pose();
        }


        /// <summary>
        /// Local to matrix
        /// </summary>
        /// <returns></returns>
        public static Bounds GetReachBounds(in BodyTree<Matrix4x4> tree, LeftRight side, Matrix4x4 space)
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

            Vector3 v = tree[scapula].GetPosition() + tree[upperArm].GetPosition();

            Matrix4x4 c = space.inverse * worldTree[clavicle];

            Vector3 offset = c.GetPosition() + space.rotation * 
            space.rotation * 
            //Vector3 fixedOffset = tree[clavicle]
        }


        /*
        public static Bounds GetReachBounds(in BodyTree<Matrix4x4> tree, LeftRight side, Matrix4x4 matrix, float maxShoulderYAngle, float maxShoulderZAngle)
        {
            BodyNode clavicleNode = side == LeftRight.Left ? BodyNode.LeftClavicle : BodyNode.RightClavicle;
            BodyNode upperArmNode = clavicleNode + 1;
            BodyNode forearmNode = upperArmNode + 1;
            BodyNode wristNode = forearmNode + 1;

            float armLength = tree[forearmNode].GetPosition().magnitude + tree[wristNode].GetPosition().magnitude;

            Vector3 GetUpperArmPosition(in BodyTree<Matrix4x4> tree, Vector3 direction)
            {
                var angles = tree.CalculateOptimalShoulderAngles(side, direction, maxShoulderYAngle, maxShoulderZAngle);
                Matrix4x4 rotatedClavicle = Matrix4x4.TRS(tree[clavicleNode].GetPosition(), Quaternion.Euler(0, angles.y, angles.z), Vector3.one);


                return rotatedClavicle.MultiplyPoint(tree[upperArmNode].GetPosition());
            }

            float xMin = GetUpperArmPosition(tree, matrix.MultiplyVector(Vector3.left)).x - armLength;
            float xMax = GetUpperArmPosition(tree, matrix.MultiplyVector(Vector3.right)).x + armLength;
            float yMin = GetUpperArmPosition(tree, matrix.MultiplyVector(Vector3.down)).y - armLength;
            float yMax = GetUpperArmPosition(tree, matrix.MultiplyVector(Vector3.up)).y + armLength;
            float zMin = GetUpperArmPosition(tree, matrix.MultiplyVector(Vector3.back)).z - armLength;
            float zMax = GetUpperArmPosition(tree, matrix.MultiplyVector(Vector3.forward)).z + armLength;

            Bounds bounds = new Bounds();

            bounds.SetMinMax(
                new Vector3(xMin, yMin, zMin),
                new Vector3(xMax, yMax, zMax));

            return bounds;
        }*/

    }
}
