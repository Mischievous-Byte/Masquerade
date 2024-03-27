using JetBrains.Annotations;
using MischievousByte.Masquerade.Anatomy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.Video;
using static PlasticGui.WorkspaceWindow.Diff.GetRestorePathData;

namespace MischievousByte.Masquerade.Utility
{
    public static class RemapperRegistry
    {
        public delegate void RemapDelegate(in BodyTree<Matrix4x4> source, ref BodyTree<Matrix4x4> destination);
        public delegate void RemapDelegate<TData>(in BodyTree<Matrix4x4> source, ref BodyTree<Matrix4x4> destination, in TData data);


        private struct Entry
        {
            public Delegate action;
            public BodyNode flags;
        }

        public static bool FindAlternativesByInheritance = true;

        private static List<Entry> entries = new();
        

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        private static void OnLoad() { } //Empty method to call static constructor

        

        static RemapperRegistry()
        {
            FindFlaggedMethods();
        }

        private static void FindFlaggedMethods()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (var type in assembly.GetTypes())
                    foreach (var method in type.GetMethods())
                    {
                        RemapperAttribute attribute = method.GetCustomAttribute<RemapperAttribute>();
                        if (attribute != null)
                            try
                            {
                                ProcessRemappingMethodInfo(method, attribute.Target);
                            }
                            catch (Exception ex)
                            {
                                Debug.LogException(ex);
                            }
                    }
                        
        }


        private static void ProcessRemappingMethodInfo(MethodInfo info, BodyNode target)
        {
            if (!info.IsStatic)
                throw new ArgumentException();

            var parameters = info.GetParameters();

            bool IsSimple()
            {
                if (parameters.Length != 2)
                    return false;

                if (!parameters[0].IsIn || parameters[0].ParameterType != typeof(BodyTree<Matrix4x4>).MakeByRefType())
                    return false;

                if (!parameters[1].ParameterType.IsByRef || parameters[1].ParameterType != typeof(BodyTree<Matrix4x4>).MakeByRefType())
                    return false;

                return true;
            }

            bool IsComplex()
            {
                if (parameters.Length != 3)
                    return false;

                if (!parameters[0].IsIn || parameters[0].ParameterType != typeof(BodyTree<Matrix4x4>).MakeByRefType())
                    return false;

                if (!parameters[1].ParameterType.IsByRef || parameters[1].ParameterType != typeof(BodyTree<Matrix4x4>).MakeByRefType())
                    return false;

                if (!parameters[2].IsIn)
                    return false;

                return true;
            }

            Type GetBase(Type t)
            {
                var u = t.GetElementType();

                return u == null ? t : u;
            }

            Delegate del;
            if (IsSimple())
                del = Delegate.CreateDelegate(
                    typeof(RemapDelegate),
                    info);
            else if (IsComplex())
                del = Delegate.CreateDelegate(
                    typeof(RemapDelegate<>).MakeGenericType(
                        GetBase(parameters[2].ParameterType)),
                    info);
            else
                throw new ArgumentException();

            entries.Add(new Entry()
            {
                action = del,
                flags = target
            });
        }

        

        public static RemapDelegate Find(BodyNode target) => 
            entries.Where(e => e.flags == target && e.action is RemapDelegate).First().action as RemapDelegate;

        public static RemapDelegate<TData> Find<TData>(BodyNode target)
        {
            var r = entries
                .Where(e => e.flags == target && !(e.action is RemapDelegate))
                .Where(e => e.action.GetType().GetGenericArguments()[0].IsAssignableFrom(typeof(TData)))
                .Select(entry =>
            {
                Type child = typeof(TData);
                Type parent = entry.action.GetType().GetGenericArguments()[0];

                int distance = 0;
                while (true)
                {
                    if (child == parent)
                        break;


                    child = child.BaseType;
                    distance++;
                }

                return (entry, distance);
            }).OrderBy(p => p.distance).FirstOrDefault();


            if (r.distance == 0)
                return r.entry.action as RemapDelegate<TData>;

            if (!FindAlternativesByInheritance)
                return null;

            RemapDelegate<TData> wrapper = (in BodyTree<Matrix4x4> source, ref BodyTree<Matrix4x4> destination, in TData data) => 
            {
                object[] p = { source, destination, data };
                r.entry.action.DynamicInvoke(p);

                destination = (BodyTree<Matrix4x4>) p[1];
            };


            return wrapper;
        }
    }
}
