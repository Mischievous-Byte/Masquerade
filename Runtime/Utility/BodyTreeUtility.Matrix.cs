using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MischievousByte.Masquerade.Utility
{
    public static partial class BodyTreeUtility
    {
        public static void ChangeSpace(this in BodyTree<Matrix4x4> source, Space target, out BodyTree<Matrix4x4> destination)
        {
            var tmp = new BodyTree<Matrix4x4>();
            tmp[BodyNode.Sacrum] = source[BodyNode.Sacrum];

            var selection = (BodyNode.All ^ BodyNode.Sacrum).Enumerate();
            if (target == Space.World)
                foreach (var node in selection)
                    tmp[node] = tmp[node.Previous()] * source[node];
            else
                foreach (var node in selection)
                    tmp[node] = source[node.Previous()].inverse * source[node];

            destination = tmp;
        }
    }
}
