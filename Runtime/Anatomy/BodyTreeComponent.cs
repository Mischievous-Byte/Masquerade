using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Anatomy
{
    public abstract class BodyTreeComponent<T> : MonoBehaviour
    {
        public BodyTree<T> tree;
    }
}
