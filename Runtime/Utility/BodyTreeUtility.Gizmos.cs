using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Utility
{
    public static partial class BodyTreeUtility
    {
        
        public static void DrawGizmos(this BodyTree<Matrix4x4> tree)
        {
            tree.ChangeSpace(Space.World, out var worldTree);
            BodyNode selection = BodyNode.All ^ BodyNode.Sacrum;

            foreach (BodyNode node in selection.Enumerate())
                Gizmos.DrawLine(
                    worldTree[node.Previous()].GetPosition(), 
                    worldTree[node].GetPosition());
        }

        public static void DrawGizmos(this BodyTree<Transform> tree)
        {

        }

        public static void DrawGizmos(this BodyTree<Vector3> tree)
        {

        }
    }
}
