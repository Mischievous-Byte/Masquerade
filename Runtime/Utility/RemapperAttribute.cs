using MischievousByte.Masquerade.Anatomy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Utility
{
    /// <summary>
    /// Attribute used to signal <see cref="MischievousByte.Masquerade.Utility.RemapperRegistry"></see> during reflection phase
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RemapperAttribute : Attribute
    {
        public readonly BodyNode Target;

        public RemapperAttribute(BodyNode target)
        {
            Target = target;
        }
    }
}
