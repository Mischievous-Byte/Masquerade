using MischievousByte.Masquerade.Anatomy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Utility
{
    public static class BuiltInRemapper
    {
        /*
        private static readonly BodyNode[] spine = new BodyNode[] { BodyNode.Sacrum, BodyNode.L3, BodyNode.T12, BodyNode.T7, BodyNode.C7, BodyNode.Head };

        //[Remapper(BodyNode.All)]
        public static void Remap(
            in BodyTree<Matrix4x4> source,
            in float sourceHeight,
            ref BodyTree<Matrix4x4> destination,
            in float destinationHeight)
        {
            RemapSpine(source, sourceHeight, ref destination, destinationHeight);
            
        }

        private static void RemapSpine(in BodyTree<Matrix4x4> source, float sourceHeight, ref BodyTree<Matrix4x4> destination, float destinationHeight)
        {
            //Rotate
            for (ushort i = 0; i < spine.Length; i++)
            {
                BodyNode node = spine[i];

                destination[node] = Matrix4x4.TRS(destination[node].GetPosition(), source[node].rotation, Vector3.one);
            }

            //Translate
            float sourceEyeHeight, destinationEyeHeight;
            {
                float top = source[BodyNode.HeadTop].GetPosition().y;
                float eyes = source[BodyNode.Eyes].GetPosition().y;

                float delta = eyes - top;

                sourceEyeHeight = sourceHeight + delta;
            }
            {
                float top = destination[BodyNode.HeadTop].GetPosition().y;
                float eyes = destination[BodyNode.Eyes].GetPosition().y;

                float delta = eyes - top;

                destinationEyeHeight = destinationHeight + delta;
            }

            float scale = destinationEyeHeight / sourceEyeHeight;

            BodyTree<Matrix4x4> worldSource, worldDestination;
            source.ChangeSpace(Space.World, out worldSource);
            destination.ChangeSpace(Space.World, out worldDestination);

            Vector3 sourceEyeLocation = worldSource[BodyNode.Eyes].GetPosition();
            Vector3 targetEyeLocation = sourceEyeLocation * scale;

            Vector3 error = targetEyeLocation - worldDestination[BodyNode.Eyes].GetPosition();

            Matrix4x4 originalDestinationSacrum = destination[BodyNode.Sacrum];
            destination[BodyNode.Sacrum] = Matrix4x4.TRS(
                originalDestinationSacrum.GetPosition() + error,
                originalDestinationSacrum.rotation,
                originalDestinationSacrum.lossyScale);
        }

        private struct ArmMatrices
        {
            public Matrix4x4 upperArm;
            public Matrix4x4 forearm;
            public Matrix4x4 wrist;
            public Matrix4x4 hand;
        }


        private static void RemapArm(in BodyTree<Matrix4x4> source, ref BodyTree<Matrix4x4> destination, LeftRight side)
        {
            BodyNode upperArm = side == LeftRight.Left ? BodyNode.LeftUpperArm : BodyNode.RightUpperArm;


            ArmMatrices matrices = new ArmMatrices()
            {
                upperArm = destination[upperArm],
                forearm = destination[upperArm+1],
                wrist = destination[upperArm+2],
                hand = destination[upperArm+3],
            };

            Pose remappedPoint = CalculateRemappedWristLocation(side, in source, in destination);

            SolveArm(side, ref matrices, Pose.identity, Vector3.zero);

            destination[upperArm] = matrices.upperArm;
            destination[upperArm+1] = matrices.forearm;
            destination[upperArm+2] = matrices.wrist;
            destination[upperArm+3] = matrices.hand;
        }

        private static Pose CalculateRemappedWristLocation(LeftRight side, in BodyTree<Matrix4x4> source, in BodyTree<Matrix4x4> destination)
        {
            return Pose.identity;
        }
        
        //Everything is in shoulder space
        private static void SolveArm(LeftRight side, ref ArmMatrices matrices, Pose target, Vector3 hint)
        {
            
        }*/
    }
}
