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
            /*destination = new();
            destination[BodyNode.Sacrum] = source[BodyNode.Sacrum];

            if (target == Space.World)
                foreach (var node in BodyNodeUtility.All.Where(x => x != BodyNode.Sacrum))
                    destination[node] = destination[node.Previous()] * source[node];
            else
                foreach (var node in BodyNodeUtility.All.Where(x => x != BodyNode.Sacrum))
                    destination[node] = destination[node.Previous()].inverse * source[node];*/
        }
    }
}
