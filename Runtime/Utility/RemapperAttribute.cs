using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MischievousByte.Masquerade.Utility
{
    /// <summary>
    /// Attribute used to signal the system during reflection phase
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class RemapperAttribute : Attribute
    {

    }
}
