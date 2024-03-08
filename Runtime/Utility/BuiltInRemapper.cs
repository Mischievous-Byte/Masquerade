using MischievousByte.Masquerade.Anatomy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Utility
{
    public static class BuiltInRemapper
    {
        [Remapper]
        public static void Remap(
            in BodyTree<Matrix4x4> source,
            in float sourceHeight,
            out BodyTree<Matrix4x4> destination,
            in float destinationHeight)
        {

            destination = new BodyTree<Matrix4x4>();
        }
    }
}
