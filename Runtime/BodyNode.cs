using Codice.CM.Common.Tree.Partial;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MischievousByte.Masquerade
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
        All = uint.MaxValue >> (32 - 26),
        [BodyNodeProperties(reference: HumanBodyBones.Hips)] Sacrum = 1,
        [BodyNodeProperties(parent: Sacrum, reference: HumanBodyBones.Spine)] L3 = 1 << 1,
        [BodyNodeProperties(parent: L3, reference: HumanBodyBones.Chest)] T12 = 1 << 2,
        [BodyNodeProperties(parent: T12, reference: HumanBodyBones.UpperChest)] T7 = 1 << 3 ,
        [BodyNodeProperties(parent: T7, reference: HumanBodyBones.Neck)] C7 = 1 << 4,
        [BodyNodeProperties(parent: C7, reference: HumanBodyBones.Head)] Head = 1 << 5,
        [BodyNodeProperties(parent: Head)] Eyes = 1 << 6,
        [BodyNodeProperties(parent: Head)] HeadTop = 1 << 7,
        [BodyNodeProperties(parent: T7, reference: HumanBodyBones.LeftShoulder)] LeftClavicle = 1 << 8,
        //[BodyNodeProperties(parent: LeftClavicle)] LeftScapula = 1 << 9,
        [BodyNodeProperties(parent: LeftClavicle, reference: HumanBodyBones.LeftUpperArm)] LeftUpperArm = 1 << 9,
        [BodyNodeProperties(parent: LeftUpperArm, reference: HumanBodyBones.LeftLowerArm)] LeftForearm = 1 << 10,
        [BodyNodeProperties(parent: LeftForearm, reference: HumanBodyBones.LeftHand)] LeftWrist = 1 << 11,
        [BodyNodeProperties(parent: LeftWrist)] LeftHand = 1 << 12,
        [BodyNodeProperties(parent: T7, reference: HumanBodyBones.RightShoulder)] RightClavicle = 1 << 13,
        //[BodyNodeProperties(parent: RightClavicle)] RightScapula = 1 << 15,
        [BodyNodeProperties(parent: RightClavicle, reference: HumanBodyBones.RightUpperArm)] RightUpperArm = 1 << 14,
        [BodyNodeProperties(parent: RightUpperArm, reference: HumanBodyBones.RightLowerArm)] RightForearm = 1 << 15,
        [BodyNodeProperties(parent: RightForearm, reference: HumanBodyBones.RightHand)] RightWrist = 1 << 16,
        [BodyNodeProperties(parent: RightWrist)] RightHand = 1 << 17,
        [BodyNodeProperties(parent: Sacrum, reference: HumanBodyBones.LeftUpperLeg)] LeftUpperLeg = 1 << 18,
        [BodyNodeProperties(parent: LeftUpperLeg, reference: HumanBodyBones.LeftLowerLeg)] LeftLowerLeg = 1 << 19,
        [BodyNodeProperties(parent: LeftLowerLeg, reference: HumanBodyBones.LeftFoot)] LeftFoot = 1 << 20,
        [BodyNodeProperties(parent: LeftFoot, reference: HumanBodyBones.LeftToes)] LeftToes = 1 << 21,
        [BodyNodeProperties(parent: Sacrum, reference: HumanBodyBones.RightUpperLeg)] RightUpperLeg = 1 << 22,
        [BodyNodeProperties(parent: RightUpperLeg, reference: HumanBodyBones.RightLowerLeg)] RightLowerLeg = 1 << 23,
        [BodyNodeProperties(parent: RightLowerLeg, reference: HumanBodyBones.RightFoot)] RightFoot = 1 << 24,
        [BodyNodeProperties(parent: RightFoot, reference: HumanBodyBones.RightToes)] RightToes = 1 << 25,
        Invalid = 1 << 26
    }


    /// <summary>
    /// This attribute is used to attach properties to the values of BodyNode
    /// </summary>
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    internal class BodyNodePropertiesAttribute : Attribute
    {
        public readonly BodyNode Parent;
        public readonly HumanBodyBones? Reference;
        public BodyNodePropertiesAttribute(BodyNode parent = BodyNode.Invalid, HumanBodyBones reference = HumanBodyBones.LastBone)
        {
            Parent = parent;
            Reference = reference == HumanBodyBones.LastBone ? null : reference;
        }
    }


    public static class BodyNodeUtility
    {
        private struct NodeProperties
        {
            public BodyNode parent;
            public HumanBodyBones reference;
        }

        private static Dictionary<BodyNode, NodeProperties> properties = new();


        [RuntimeInitializeOnLoadMethod]
        private static void OnLoad() { }

        static BodyNodeUtility()
        {
            foreach (var node in BodyNode.All.Enumerate().ToList())
                properties.Add(node, FetchProperties(node));

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
                parent = attr?.Parent ?? BodyNode.Invalid,
                reference = attr?.Reference ?? HumanBodyBones.LastBone
            };

            return props;
        }

        public static IEnumerable<BodyNode> Enumerate(this BodyNode flags)
        {
            for(int i = 0; i < 29; i ++)
            {
                BodyNode node = (BodyNode) (1u << i);
                if ((flags & node) != 0)
                    yield return node;
            }
        }

        public static BodyNode Previous(this BodyNode node)
        {
            if(!properties.ContainsKey(node))
                throw new ArgumentOutOfRangeException();

            return properties[node].parent;
        }

        public static BodyNode Next(this BodyNode node)
        {
            if (!properties.ContainsKey(node))
                throw new ArgumentOutOfRangeException();

            foreach(var entry in properties)
                if (entry.Value.parent == node)
                    return entry.Key;

            return BodyNode.Invalid;
        }

        public static HumanBodyBones ToHuman(this BodyNode node)
        {
            if (!properties.ContainsKey(node))
                throw new ArgumentOutOfRangeException();

            return properties[node].reference;
        }
        public static BodyNode[] Span(BodyNode origin, BodyNode destination)
        {
            if (!properties.ContainsKey(origin) || !properties.ContainsKey(destination))
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
