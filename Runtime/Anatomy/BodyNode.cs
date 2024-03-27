using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MischievousByte.Masquerade.Anatomy
{
    /// <summary>
    /// An enum containing all possible nodes for representing a hunanoid skeletal structure. Similar to <see cref="UnityEngine.HumanBodyBones">HumanBodyBones</see>
    /// </summary>
    [Flags]
    public enum BodyNode : uint
    {
        None = 0,
        /// <summary>
        /// Exluded BodyNode.Invalid
        /// </summary>
        All = uint.MaxValue >> (32 - 27),
        Sacrum = 1,
        [BodyNodeProperties(parent: Sacrum)] L3 = 1 << 1,
        [BodyNodeProperties(parent: L3)] T12 = 1 << 2,
        [BodyNodeProperties(parent: T12)] T7 = 1 << 3 ,
        [BodyNodeProperties(parent: T7)] C7 = 1 << 4,
        [BodyNodeProperties(parent: C7)] Head = 1 << 5,
        [BodyNodeProperties(parent: Head)] Eyes = 1 << 6,
        [BodyNodeProperties(parent: Head)] HeadTop = 1 << 7,
        [BodyNodeProperties(parent: T7)] LeftClavicle = 1 << 8,
        [BodyNodeProperties(parent: LeftClavicle)] LeftScapula = 1 << 9,
        [BodyNodeProperties(parent: LeftScapula)] LeftUpperArm = 1 << 10,
        [BodyNodeProperties(parent: LeftUpperArm)] LeftForearm = 1 << 11,
        [BodyNodeProperties(parent: LeftForearm)] LeftWrist = 1 << 12,
        [BodyNodeProperties(parent: LeftWrist)] LeftHand = 1 << 13,
        [BodyNodeProperties(parent: T7)] RightClavicle = 1 << 14,
        [BodyNodeProperties(parent: RightClavicle)] RightScapula = 1 << 15,
        [BodyNodeProperties(parent: RightScapula)] RightUpperArm = 1 << 16,
        [BodyNodeProperties(parent: RightUpperArm)] RightForearm = 1 << 17,
        [BodyNodeProperties(parent: RightForearm)] RightWrist = 1 << 18,
        [BodyNodeProperties(parent: RightWrist)] RightHand = 1 << 19,
        [BodyNodeProperties(parent: Sacrum)] LeftUpperLeg = 1 << 20,
        [BodyNodeProperties(parent: LeftUpperLeg)] LeftLowerLeg = 1 << 21,
        [BodyNodeProperties(parent: LeftLowerLeg)] LeftFoot = 1 << 22,
        [BodyNodeProperties(parent: LeftFoot)] LeftToes = 1 << 23,
        [BodyNodeProperties(parent: Sacrum)] RightUpperLeg = 1 << 24,
        [BodyNodeProperties(parent: RightUpperLeg)] RightLowerLeg = 1 << 25,
        [BodyNodeProperties(parent: RightLowerLeg)] RightFoot = 1 << 26,
        [BodyNodeProperties(parent: RightFoot)] RightToes = 1 << 27,
        Invalid = 1 << 28
    }


    /// <summary>
    /// This attribute is used to attach properties to the values of BodyNode
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal class BodyNodePropertiesAttribute : Attribute
    {
        public readonly BodyNode Parent;
        public BodyNodePropertiesAttribute(BodyNode parent = BodyNode.Invalid)
        {
            Parent = parent;
        }
    }


    public static class BodyNodeUtility
    {
        private struct NodeProperties
        {
            public BodyNode parent;
        }

        private static NodeProperties[] properties = new NodeProperties[(int)BodyNode.Invalid];

        
        static BodyNodeUtility()
        {
            BodyNode.All.Enumerate().ToList().ForEach(n => properties[(int)n] = FetchProperties(n));

            //Safety to make sure that the sacrum is the only root node
            foreach (var node in BodyNode.All.Enumerate().Where(n => n != BodyNode.Sacrum))
                if (node.Previous() == BodyNode.Invalid)
                    throw new FormatException("BodyNode tree has multiple roots");
        }


        private static NodeProperties FetchProperties(BodyNode node)
        {
            if (node == BodyNode.Invalid)
                throw new ArgumentOutOfRangeException();

            string name = Enum.GetName(typeof(BodyNode), node);

            var attr = typeof(BodyNode).GetMember(name).First().GetCustomAttribute<BodyNodePropertiesAttribute>();

            NodeProperties props = new NodeProperties()
            {
                parent = attr?.Parent ?? BodyNode.Invalid
            };

            return props;
        }

        public static IEnumerable<BodyNode> Enumerate(this BodyNode flags)
        {
            for(int i = 0; i < 28; i ++)
            {
                BodyNode node = (BodyNode) (1u << i);
                if ((flags & node) != 0)
                    yield return node;
            }
        }

        public static BodyNode Previous(this BodyNode node)
        {
            if (node == BodyNode.Invalid)
                throw new ArgumentOutOfRangeException();

            NodeProperties props = properties[(int)node];
            return props.parent;
        }

        public static BodyNode Next(this BodyNode node)
        {
            if (node == BodyNode.Invalid)
                throw new ArgumentOutOfRangeException();

            for (int i = 0; i < properties.Length; i++)
                if (properties[i].parent == node)
                    return (BodyNode)i;

            return BodyNode.Invalid;
        }

        public static BodyNode[] Span(BodyNode origin, BodyNode destination)
        {
            if (origin == BodyNode.Invalid || destination == BodyNode.Invalid)
                throw new ArgumentOutOfRangeException();

            if (origin == destination)
                return new BodyNode[] { origin };

            List<BodyNode> originChain = new() { origin };
            List<BodyNode> destinationChain = new() { destination };

            while ((origin = origin.Previous()) != BodyNode.Invalid)
                originChain.Add(origin);

            while ((destination = destination.Previous()) != BodyNode.Invalid)
                destinationChain.Add(destination);


            int originOverlap = 0;
            int destinationOverlap = 0;

            for (int i = 0; i < originChain.Count; i++)
            {
                if ((destinationOverlap = destinationChain.IndexOf(originChain[i])) == -1)
                    continue;

                originOverlap = i;
                break;
            }

            BodyNode[] span = new BodyNode[originOverlap + 1 + destinationOverlap];

            for (int i = 0; i < originOverlap; i++)
                span[i] = originChain[i];

            for (int i = destinationOverlap; i >= 0; i--)
            {
                span[originOverlap + (destinationOverlap - i)] = destinationChain[i];
            }


            return span;
        }
    }
}
