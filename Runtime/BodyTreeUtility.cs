using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MischievousByte.Masquerade
{
    public static class BodyTreeUtility
    {
        public static void ChangeSpace(this in BodyTree<Matrix4x4> source, Space target, out BodyTree<Matrix4x4> destination)
        {
            destination = new();
            destination[BodyNode.Sacrum] = source[BodyNode.Sacrum];

            var selection = (BodyNode.All ^ BodyNode.Sacrum).Enumerate();
            if (target == Space.World)
                foreach (var node in selection)
                    destination[node] = destination[node.Previous()] * source[node];
            else
                foreach (var node in selection)
                    destination[node] = destination[node.Previous()].inverse * source[node];
        }
    }
}
