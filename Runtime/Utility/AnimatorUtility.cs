using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MischievousByte.Masquerade.Utility
{
    public static class AnimatorUtility
    {
        [Serializable]
        public struct BoneData
        {
            public HumanBodyBones bone;
            public string name;
            public Matrix4x4 matrix;
        }

        public static void TPose(this Animator animator)
        {
            if (!animator.avatar.isHuman)
                throw new ArgumentException();

            var matrices = animator.avatar.ExtractLocalTPoseMatrices();

            foreach(Transform child in animator.avatarRoot.GetComponentsInChildren<Transform>())
            {
                var matches = matrices.Where(bd => bd.name == child.name);

                if (matches.Count() == 0)
                    continue;

                BoneData bd = matches.First();

                child.localPosition = bd.matrix.GetPosition();
                child.localRotation = bd.matrix.rotation;
                child.localScale = bd.matrix.lossyScale;
            }
        }


        public static void ToTree(this Animator animator, (Vector3 eyes, Vector3 headTop, Vector3 leftPalm, Vector3 rightPalm) extra, out BodyTree<Matrix4x4> tree)
        {
            if (!animator.avatar.isHuman)
                throw new ArgumentException();

            tree = new();

            foreach (var node in BodyNode.All.Enumerate())
                tree[node] = Matrix4x4.identity;

            var boneDataCache = new Dictionary<string, BoneData>();

            foreach (var x in animator.avatar.ExtractLocalTPoseMatrices())
                boneDataCache.Add(x.name, x);

            Dictionary<string, Matrix4x4> matrices = new();
            matrices[animator.avatarRoot.name] = Matrix4x4.identity;

            var transforms = animator.GetComponentsInChildren<Transform>().Where(t => boneDataCache.ContainsKey(t.name) && t != animator.avatarRoot);

            foreach (var transform in transforms)
                matrices[transform.name] = matrices[transform.parent.name] * boneDataCache[transform.name].matrix;

            foreach (var node in BodyNode.All.Enumerate())
            {
                HumanBodyBones hbb = node.ToHuman();

                if (hbb == HumanBodyBones.LastBone)
                    continue;

                tree[node] = Matrix4x4.Translate(matrices[boneDataCache.Where(pair => pair.Value.bone == hbb).First().Value.name].GetPosition());
            }
            

            tree[BodyNode.LeftHand] = Matrix4x4.Translate(extra.leftPalm);
            tree[BodyNode.RightHand] = Matrix4x4.Translate(extra.rightPalm);
            tree[BodyNode.Eyes] = Matrix4x4.Translate(extra.eyes);
            tree[BodyNode.HeadTop] = Matrix4x4.Translate(extra.headTop);

            tree.ChangeSpace(Space.Self, out tree);
        }

        public static IEnumerable<BoneData> ExtractLocalTPoseMatrices(this Avatar avatar)
        {
            if (!avatar.isHuman)
                throw new ArgumentException();

            Dictionary<string, HumanBone> humanBoneCache = new();

            foreach(HumanBone hb in avatar.humanDescription.human)
                humanBoneCache.Add(hb.boneName, hb);

            foreach (SkeletonBone sb in avatar.humanDescription.skeleton)
            {
                if (!humanBoneCache.ContainsKey(sb.name))
                    continue;

                yield return new()
                {
                    bone = Enum.Parse<HumanBodyBones>(humanBoneCache[sb.name].humanName.Replace(" ", "")),
                    name = sb.name,
                    matrix = Matrix4x4.TRS(sb.position, sb.rotation, sb.scale)
                };
            }
        }
    }
}
