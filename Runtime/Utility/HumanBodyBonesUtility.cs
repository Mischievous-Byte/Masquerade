using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PlasticGui.LaunchDiffParameters;

namespace MischievousByte.Masquerade.Utility
{
    public static class HumanBodyBonesUtility
    {
        public static HumanBodyBones GetClosestAncestor(this HumanBodyBones bone, Animator animator)
        {
            if (bone == HumanBodyBones.Hips)
                return HumanBodyBones.LastBone;

            int c = (int)bone;

            do
            {
                c = HumanTrait.GetParentBone(c);

                if (c == -1)
                    return HumanBodyBones.LastBone;
            } while (animator.GetBoneTransform((HumanBodyBones)c) == null);

            return (HumanBodyBones)c;
        }

        public static HumanBodyBones GetClosestAncestor(this HumanBodyBones bone, Avatar avatar)
        {
            if (bone == HumanBodyBones.Hips)
                return HumanBodyBones.LastBone;

            int c = (int)bone;

            /* The first iteration of this method used this:
             * 
             * avatar.humanDescription.human.Any(hb => hb.boneName == HumanTrait.BoneName[c])
             * 
             * Storing the strings inside a HashSet only uses a 4th of the time
            */

            HashSet<string> availableBones = new HashSet<string>();

            foreach (var hb in avatar.humanDescription.human)
                availableBones.Add(hb.humanName);

            do
            {
                c = HumanTrait.GetParentBone(c);

                if (c == -1)
                    return HumanBodyBones.LastBone;
            } while (!availableBones.Contains(HumanTrait.BoneName[c]));

            return (HumanBodyBones) c;
        }
    }
}
