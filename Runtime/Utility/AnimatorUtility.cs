using MischievousByte.Masquerade.Anatomy;
using PlasticGui;
using PlasticGui.WorkspaceWindow.PendingChanges;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.Profiling;

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

            var tPose = GetReducedTPose(animator);

            foreach(var pair in tPose)
            {
                Transform parent;
                if (pair.Key == HumanBodyBones.Hips)
                    parent = animator.avatarRoot;
                else
                    parent = animator.GetBoneTransform(pair.Key.GetClosestAncestor(animator));

                Transform child = animator.GetBoneTransform(pair.Key);

                Matrix4x4 result = parent.localToWorldMatrix * pair.Value;

                child.position = result.GetPosition();
                child.rotation = result.rotation;
                child.localScale = pair.Value.lossyScale;
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

            tree[BodyNode.LeftHand] = Matrix4x4.Translate(matrices[boneDataCache.Where(p => p.Value.bone == HumanBodyBones.LeftHand).First().Key].MultiplyPoint(extra.leftPalm));
            tree[BodyNode.RightHand] = Matrix4x4.Translate(matrices[boneDataCache.Where(p => p.Value.bone == HumanBodyBones.RightHand).First().Key].MultiplyPoint(extra.rightPalm));
            tree[BodyNode.Eyes] = Matrix4x4.Translate(matrices[boneDataCache.Where(p => p.Value.bone == HumanBodyBones.Head).First().Key].MultiplyPoint(extra.eyes));
            tree[BodyNode.HeadTop] = Matrix4x4.Translate(matrices[boneDataCache.Where(p => p.Value.bone == HumanBodyBones.Head).First().Key].MultiplyPoint(extra.headTop));
            
            tree.ToLocal(out tree);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="overrides">Vector3 is in local space to previous node</param>
        /// <param name="tree"></param>
        public static void CreateTree(this Animator animator, IDictionary<BodyNode, Vector3> overrides, out BodyTree<Matrix4x4> tree)
        {
            tree = new();

            var tPose = animator.GetReducedTPose();

            Dictionary<HumanBodyBones, Matrix4x4> worldTPose = new() { { HumanBodyBones.Hips, tPose[HumanBodyBones.Hips] } };

            foreach (var node in BodyNode.All.Enumerate().Where(x => x != BodyNode.Sacrum))
            {
                HumanBodyBones bone = node.ToHuman();

                if (!tPose.ContainsKey(bone))
                    continue;

                HumanBodyBones parent = bone.GetClosestAncestor(animator);

                Matrix4x4 r = worldTPose[parent] * tPose[bone];
                worldTPose.Add(bone, r);
                tree[node] = Matrix4x4.Translate(r.GetPosition());
            }

            tree[BodyNode.Sacrum] = Matrix4x4.Translate(tPose[HumanBodyBones.Hips].GetPosition());

            
            foreach(var pair in overrides)
                if (pair.Key == BodyNode.Sacrum)
                    tree[pair.Key] = Matrix4x4.Translate(pair.Value);
                else
                    tree[pair.Key] = Matrix4x4.Translate(worldTPose[pair.Key.Previous().ToHuman()].MultiplyPoint(pair.Value));

            tree.ToLocal(out tree);
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

        /// <summary>
        /// Transforms in between mecanim bones that aren't present in the avatar will result in an exception
        /// If a mecanim bone can't be found, it will be skipped. (both entry and in the parent chain)
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        public static Dictionary<HumanBodyBones, Matrix4x4> GetReducedTPose(this Animator animator)
        {
            Dictionary<HumanBodyBones, Matrix4x4> reducedTPose = new();

            Dictionary<string, Matrix4x4> defaultMatrices = new();

            foreach (SkeletonBone sb in animator.avatar.humanDescription.skeleton)
                defaultMatrices.Add(sb.name, Matrix4x4.TRS(sb.position, sb.rotation, sb.scale));

            Profiler.BeginSample("Loop");

            for (int i = 0; i < HumanTrait.BoneCount; i ++)
            {
                HumanBodyBones bone = (HumanBodyBones)i;

                if (bone == HumanBodyBones.Hips)
                    continue;

                Transform self = animator.GetBoneTransform(bone);

                if (self == null)
                    continue;

                Transform parent = animator.GetBoneTransform(bone.GetClosestAncestor(animator));

                if (!self.IsChildOf(parent))
                    throw new ArgumentException();

                Matrix4x4 e = defaultMatrices[self.name];
                while ((self = self.parent) != parent)
                    e = defaultMatrices[self.name] * e;

                reducedTPose.Add(bone, e);

            }

            Profiler.EndSample();

            Transform s = animator.GetBoneTransform(HumanBodyBones.Hips);
            Matrix4x4 h = defaultMatrices[s.name];
            while ((s = s.parent) != animator.avatarRoot)
                h = defaultMatrices[s.name] * h;

            reducedTPose.Add(HumanBodyBones.Hips, h);
            return reducedTPose;
        }
    }
}
