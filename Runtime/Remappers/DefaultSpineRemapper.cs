using MischievousByte.Masquerade.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Remappers
{
    public static class DefaultSpineRemapper
    {
        [Remapper(CommonNodeGroupings.Spine)]
        public static void Remap(
            in BodyTree<Matrix4x4> source, 
            in float sourceHeight,
            ref BodyTree<Matrix4x4> destination, 
            in float destinationHeight)
        {
            Rotate(in source, ref destination);
            Translate(in source, in sourceHeight, ref destination, in destinationHeight);
        }

        private static void Rotate(in BodyTree<Matrix4x4> source, ref BodyTree<Matrix4x4> destination)
        {
            foreach(BodyNode node in CommonNodeGroupings.Spine.Enumerate())
                destination[node] = Matrix4x4.TRS(destination[node].GetPosition(), source[node].rotation, Vector3.one);
        }


        private static void Translate(
            in BodyTree<Matrix4x4> source,
            in float sourceHeight,
            ref BodyTree<Matrix4x4> destination,
            in float destinationHeight)
        {
            float sourceEyeHeight = 
                sourceHeight + source[BodyNode.Eyes].GetPosition().y - source[BodyNode.HeadTop].GetPosition().y;

            float destinationEyeHeight =
                destinationHeight + destination[BodyNode.Eyes].GetPosition().y - destination[BodyNode.HeadTop].GetPosition().y;


            float scale = destinationEyeHeight / sourceEyeHeight;

            source.ChangeSpace(Space.World, out var worldSource);
            destination.ChangeSpace(Space.World, out var worldDestination);

            Vector3 sourceEyeLocation = worldSource[BodyNode.Eyes].GetPosition();
            Vector3 targetEyeLocation = sourceEyeLocation * scale;

            Vector3 error = targetEyeLocation - worldDestination[BodyNode.Eyes].GetPosition();

            Matrix4x4 originalDestinationSacrum = destination[BodyNode.Sacrum];
            destination[BodyNode.Sacrum] = Matrix4x4.TRS(
                originalDestinationSacrum.GetPosition() + error,
                originalDestinationSacrum.rotation,
                Vector3.one);
        }
    }
}
