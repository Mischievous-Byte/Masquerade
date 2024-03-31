using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Utility
{
    public class BodyTreeAsset<T> : ScriptableObject
    {
        [SerializeField]
        private BodyTree<T> tree;

        public BodyTree<T> Tree
        {
            get => tree;
            set { tree = value; onChange?.Invoke(tree); } 
        }

        public event Action<BodyTree<T>> onChange;
    }
}
