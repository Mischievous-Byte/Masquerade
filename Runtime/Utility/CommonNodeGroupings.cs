using MischievousByte.Masquerade.Anatomy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Utility
{
    public static class CommonNodeGroupings
    {
        public const BodyNode Spine = BodyNode.Sacrum | BodyNode.L3 | BodyNode.T12 | BodyNode.T7 | BodyNode.C7 | BodyNode.Head;
        public const BodyNode LeftArm = BodyNode.LeftClavicle | BodyNode.LeftUpperArm | BodyNode.LeftForearm | BodyNode.LeftWrist;
        public const BodyNode RightArm = BodyNode.RightClavicle | BodyNode.RightUpperArm | BodyNode.RightForearm | BodyNode.RightWrist;
    }
}
