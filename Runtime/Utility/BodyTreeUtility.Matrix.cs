using MischievousByte.Masquerade.Anatomy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MischievousByte.Masquerade.Utility
{
    public static partial class BodyTreeUtility
    {
        public static void DrawGizmos(this BodyTree<Matrix4x4> tree)
        {
            tree.ToWorld(out var worldTree);
            BodyNode selection = BodyNode.All ^ BodyNode.Sacrum;

            foreach (BodyNode node in selection.Enumerate())
                Gizmos.DrawLine(
                    worldTree[node.Previous()].GetPosition(),
                    worldTree[node].GetPosition());
        }

        public static void ToLocal(this in BodyTree<Matrix4x4> source, out BodyTree<Matrix4x4> destination)
        {
            var tmp = new BodyTree<Matrix4x4>();
            tmp[BodyNode.Sacrum] = source[BodyNode.Sacrum];

            var selection = (BodyNode.All ^ BodyNode.Sacrum).Enumerate();

            foreach (var node in selection)
                tmp[node] = source[node.Previous()].inverse * source[node];

            destination = tmp; 
        }

        public static void ToWorld(this in BodyTree<Matrix4x4> source, out BodyTree<Matrix4x4> destination)
        {
            var tmp = new BodyTree<Matrix4x4>();
            tmp[BodyNode.Sacrum] = source[BodyNode.Sacrum];

            var selection = (BodyNode.All ^ BodyNode.Sacrum).Enumerate();

            foreach (var node in selection)
                tmp[node] = tmp[node.Previous()] * source[node];

            destination = tmp;
        }

        [Obsolete]
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
