using MischievousByte.Masquerade.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Remappers
{
    public static class DefaultRemapper
    {
        [Remapper(BodyNode.All)]
        public static void Remap(
            in BodyTree<Matrix4x4> source,
            in float sourceHeight,
            ref BodyTree<Matrix4x4> destination,
            in float destinationHeight)
        {
            DefaultSpineRemapper.Remap(in source, in sourceHeight, ref destination, in destinationHeight);
            DefaultArmRemapper.RemapArm(in source, ref destination, LeftRight.Left);
            DefaultArmRemapper.RemapArm(in source, ref destination, LeftRight.Right);
        }
    }
}
