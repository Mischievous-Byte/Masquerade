using MischievousByte.Masquerade.Utility;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Anatomy
{
    /// <summary>
    /// Simple implementation for BodyTreeComponent<Matrix4x4>
    /// </summary>
    public class MatrixTreeComponent : BodyTreeComponent<Matrix4x4>
    {

#if UNITY_EDITOR
        public bool draw = true;
        public Color color = Color.blue;

        private void OnDrawGizmos()
        {
            if (!draw)
                return;

            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = color;

            tree.DrawGizmos();
        }
#endif
    }
}
