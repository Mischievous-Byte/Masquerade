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
    public enum BodyNode
    {
        Sacrum,
        [BodyNodeProperties(parent: Sacrum)] L3,
        [BodyNodeProperties(parent: L3)] T12,
        [BodyNodeProperties(parent: T12)] T7,
        [BodyNodeProperties(parent: T7)] C7,
        [BodyNodeProperties(parent: C7)] Head,
        [BodyNodeProperties(parent: Head)] Eyes,
        [BodyNodeProperties(parent: T7)] LeftClavicle,
        [BodyNodeProperties(parent: LeftClavicle)] LeftScapula,
        [BodyNodeProperties(parent: LeftScapula)] LeftUpperArm,
        [BodyNodeProperties(parent: LeftUpperArm)] LeftForearm,
        [BodyNodeProperties(parent: LeftForearm)] LeftWrist,
        [BodyNodeProperties(parent: LeftWrist)] LeftHand,
        [BodyNodeProperties(parent: T7)] RightClavicle,
        [BodyNodeProperties(parent: RightClavicle)] RightScapula,
        [BodyNodeProperties(parent: RightScapula)] RightUpperArm,
        [BodyNodeProperties(parent: RightUpperArm)] RightForearm,
        [BodyNodeProperties(parent: RightForearm)] RightWrist,
        [BodyNodeProperties(parent: RightWrist)] RightHand,
        [BodyNodeProperties(parent: Sacrum)] LeftUpperLeg,
        [BodyNodeProperties(parent: LeftUpperLeg)] LeftLowerLeg,
        [BodyNodeProperties(parent: LeftLowerLeg)] LeftFoot,
        [BodyNodeProperties(parent: LeftFoot)] LeftToes,
        [BodyNodeProperties(parent: Sacrum)] RightUpperLeg,
        [BodyNodeProperties(parent: RightUpperLeg)] RightLowerLeg,
        [BodyNodeProperties(parent: RightLowerLeg)] RightFoot,
        [BodyNodeProperties(parent: RightFoot)] RightToes,
        Invalid
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

        public static readonly IReadOnlyCollection<BodyNode> All = new ReadOnlyCollection<BodyNode>(Enum.GetValues(typeof(BodyNode)).Cast<BodyNode>().Where(x => x != BodyNode.Invalid).ToArray());

        private static NodeProperties[] properties = new NodeProperties[(int)BodyNode.Invalid];

        static BodyNodeUtility()
        {
            Enum.GetValues(typeof(BodyNode)).Cast<BodyNode>().ToList().ForEach(n => properties[(int)n] = FetchProperties(n));

            //Safety to make sure that the sacrum is the only root node
            foreach (var node in All.Where(n => n != BodyNode.Sacrum))
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
