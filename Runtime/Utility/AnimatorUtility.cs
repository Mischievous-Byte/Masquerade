using PlasticGui.WorkspaceWindow.PendingChanges;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace MischievousByte.Masquerade.Utility
{
    public static class A
    {
        
    }
    
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

            tree[BodyNode.LeftHand] = Matrix4x4.Translate(matrices[boneDataCache.Where(p => p.Value.bone == HumanBodyBones.LeftHand).First().Key].MultiplyPoint(extra.leftPalm));
            tree[BodyNode.RightHand] = Matrix4x4.Translate(matrices[boneDataCache.Where(p => p.Value.bone == HumanBodyBones.RightHand).First().Key].MultiplyPoint(extra.rightPalm));
            tree[BodyNode.Eyes] = Matrix4x4.Translate(matrices[boneDataCache.Where(p => p.Value.bone == HumanBodyBones.Head).First().Key].MultiplyPoint(extra.eyes));
            tree[BodyNode.HeadTop] = Matrix4x4.Translate(matrices[boneDataCache.Where(p => p.Value.bone == HumanBodyBones.Head).First().Key].MultiplyPoint(extra.headTop));
            
            tree.ChangeSpace(Space.Self, out tree);
        }

        public static void CreateTree(this Animator animator, Dictionary<BodyNode, Vector3> overrides, out BodyTree<Matrix4x4> tree)
        {
            tree = new();

            var tPose = animator.GetReducedTPose();

            Dictionary<Transform, Matrix4x4> matrices = new();
            matrices[animator.avatarRoot] = Matrix4x4.identity;

            
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


        //TODO: Handle missing optional bones (EG. UpperChest)
        public static Dictionary<HumanBodyBones, Matrix4x4> GetReducedTPose(this Animator animator)
        {
            if (!animator.avatar.isHuman)
                throw new ArgumentException();

            Dictionary<HumanBodyBones, Matrix4x4> r = new();

            Dictionary<string, Matrix4x4> matrices = new();

            foreach (SkeletonBone sb in animator.avatar.humanDescription.skeleton)
                matrices.Add(sb.name, Matrix4x4.TRS(sb.position, sb.rotation, sb.scale));

            for(int i = 0; i < HumanTrait.BoneCount; i ++)
            {
                HumanBodyBones bone = (HumanBodyBones)i;

                if (bone == HumanBodyBones.Hips)
                    continue;

                Transform self = animator.GetBoneTransform(bone);

                if (self == null)
                    continue;

                Transform parent = animator.GetBoneTransform((HumanBodyBones)HumanTrait.GetParentBone((int)bone));

                if (!self.IsChildOf(parent))
                    throw new Exception("Child is not a child????");

                Matrix4x4 e = matrices[self.name];

                Transform c = self;
                while((c = c.parent) != parent)
                    e = matrices[c.name] * e;
                
                r.Add(bone, e);
            }

            Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);
            r.Add(HumanBodyBones.Hips,animator.avatarRoot.worldToLocalMatrix * hips.localToWorldMatrix);
            return r;
        }


        /// <summary>
        /// Transforms in between mecanim bones that aren't present in the avatar will result in an exception
        /// If a mecanim bone can't be found, it will be skipped. (both a entry and in the parent chain)
        /// </summary>
        /// <param name="animator"></param>
        /// <returns></returns>
        public static Dictionary<HumanBodyBones, Matrix4x4> GetReducedTPose2(this Animator animator)
        {
            Dictionary<HumanBodyBones, Matrix4x4> reducedTPose = new();

            Dictionary<string, Matrix4x4> defaultMatrices = new();

            foreach (SkeletonBone sb in animator.avatar.humanDescription.skeleton)
                defaultMatrices.Add(sb.name, Matrix4x4.TRS(sb.position, sb.rotation, sb.scale));

            for(int i = 0; i < HumanTrait.BoneCount; i ++)
            {
                HumanBodyBones bone = (HumanBodyBones)i;

                if (bone == HumanBodyBones.Hips)
                    continue;

                Transform self = animator.GetBoneTransform(bone);

                if (self == null)
                    continue;

                Transform parent = animator.GetBoneTransform(GetFirstParent(animator.avatar, bone));

                if (!self.IsChildOf(parent))
                    throw new ArgumentException();

                Matrix4x4 e = defaultMatrices[self.name];
                while ((self = self.parent) != parent)
                    e = defaultMatrices[self.name] * e;

                reducedTPose.Add(bone, e);

            }
            return reducedTPose;
        }

        public static HumanBodyBones GetFirstParent(this Avatar avatar, HumanBodyBones child)
        {
            if (child == HumanBodyBones.Hips)
                return HumanBodyBones.LastBone;

            IEnumerable<int> presentBones = 
                avatar.humanDescription.human.Select(
                    bone => 
                    HumanTrait.BoneName
                    .Select((name, index) => (name, index))
                    .Where(p => p.name == bone.humanName).
                    First().index);

            int c = (int) child;

            
            while(!presentBones.Contains(c = HumanTrait.GetParentBone(c)))
                if(c == -1)
                    return HumanBodyBones.LastBone;

            return (HumanBodyBones)c;
        }
    }
}
