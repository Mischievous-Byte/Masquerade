using MischievousByte.Masquerade.Anatomy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using UnityEngine;

namespace MischievousByte.Masquerade.Utility
{
    public static class RemapperRegistry
    {
        public delegate void RemapDelegate(
            in BodyTree<Matrix4x4> source, 
            ref BodyTree<Matrix4x4> destination);

        public delegate void RemapDelegate<TSettings>(
            in BodyTree<Matrix4x4> source, 
            ref BodyTree<Matrix4x4> destination, 
            in TSettings data);

        public delegate void RemapDelegate<TUniqueS, TUniqueD>(
            in BodyTree<Matrix4x4> source,
            in TUniqueS sourceData,
            ref BodyTree<Matrix4x4> destination,
            in TUniqueD destinationData);

        public delegate void RemapDelegate<TUniqueS, TUniqueD, TSettings>(
            in BodyTree<Matrix4x4> source,
            in TUniqueS sourceData,
            ref BodyTree<Matrix4x4> destination,
            in TUniqueD destinationData,
            in TSettings data);


        private static class MethodInfoLoader
        {
            public static bool TryCreateDelegate(MethodInfo info, out Delegate del)
            {
                if (HandleSimple(info, out del)) return true;
                if (HandleSettings(info, out del)) return true;
                if (HandleUnique(info, out del)) return true;
                if (HandleHybrid(info, out del)) return true;
                return false;
            }

            private static Type GetBase(Type t)
            {
                var u = t.GetElementType();

                return u == null ? t : u;
            }

            private static bool HandleSimple(MethodInfo info, out Delegate del)
            {
                del = null;
                ParameterInfo[] parameters = info.GetParameters();
                
                if (parameters.Length != 2)
                    return false;

                if (!parameters[0].IsIn || parameters[0].ParameterType != typeof(BodyTree<Matrix4x4>).MakeByRefType())
                    return false;

                if (!parameters[1].ParameterType.IsByRef || parameters[1].ParameterType != typeof(BodyTree<Matrix4x4>).MakeByRefType())
                    return false;

                del = Delegate.CreateDelegate(
                    typeof(RemapDelegate),
                    info);

                return true;
            }

            private static bool HandleSettings(MethodInfo info, out Delegate del)
            {
                del = null;
                ParameterInfo[] parameters = info.GetParameters();

                if (parameters.Length != 3)
                    return false;

                if (!parameters[0].IsIn || parameters[0].ParameterType != typeof(BodyTree<Matrix4x4>).MakeByRefType())
                    return false;

                if (!parameters[1].ParameterType.IsByRef || parameters[1].ParameterType != typeof(BodyTree<Matrix4x4>).MakeByRefType())
                    return false;

                if (!parameters[2].IsIn)
                    return false;

                del = Delegate.CreateDelegate(
                    typeof(RemapDelegate<>).MakeGenericType(
                        GetBase(parameters[2].ParameterType)),
                    info);

                return true;
            }

            private static bool HandleUnique(MethodInfo info, out Delegate del)
            {
                del = null;
                ParameterInfo[] parameters = info.GetParameters();

                if (parameters.Length != 4)
                    return false;

                if (!parameters[0].IsIn || parameters[0].ParameterType != typeof(BodyTree<Matrix4x4>).MakeByRefType())
                    return false;

                if (!parameters[2].ParameterType.IsByRef || parameters[2].ParameterType != typeof(BodyTree<Matrix4x4>).MakeByRefType())
                    return false;

                if (!parameters[1].IsIn || !parameters[3].IsIn)
                    return false;

                del = Delegate.CreateDelegate(
                    typeof(RemapDelegate<,>).MakeGenericType(
                        GetBase(parameters[1].ParameterType),
                        GetBase(parameters[3].ParameterType)),
                    info);

                return true;
            }

            private static bool HandleHybrid(MethodInfo info, out Delegate del)
            {
                del = null;
                ParameterInfo[] parameters = info.GetParameters();

                if (parameters.Length != 5)
                    return false;

                if (!parameters[0].IsIn || parameters[0].ParameterType != typeof(BodyTree<Matrix4x4>).MakeByRefType())
                    return false;

                if (!parameters[2].ParameterType.IsByRef || parameters[2].ParameterType != typeof(BodyTree<Matrix4x4>).MakeByRefType())
                    return false;

                if (!parameters[1].IsIn || !parameters[3].IsIn || !parameters[4].IsIn)
                    return false;

                del = Delegate.CreateDelegate(
                    typeof(RemapDelegate<,,>).MakeGenericType(
                        GetBase(parameters[1].ParameterType),
                        GetBase(parameters[3].ParameterType),
                        GetBase(parameters[4].ParameterType)),
                    info);

                return true;
            }

        }

        private struct Entry
        {
            public Delegate action;
            public BodyNode flags;
        }


        /* 
        public enum FallbackFilter
        {
            None,
            Minimum
        }*/

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
            var pairs = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .SelectMany(type => type.GetMethods())
                .Where(method => method.IsStatic)
                .Select(method => (method, method.GetCustomAttribute<RemapperAttribute>()))
                .Where(pair => pair.Item2 != null);

            foreach(var pair in pairs)
            {
                if (!MethodInfoLoader.TryCreateDelegate(pair.method, out var del))
                {
                    Debug.LogWarning($"{pair.method.Name} is flagged as remapper, but doesn't match any delegate.");
                    continue;
                }

                if (entries.Where(entry => entry.action == del).Count() > 0)
                {
                    Debug.LogWarning($"{pair.method.Name} is already registered during discovery");
                    continue;
                }

                Entry entry = new Entry()
                {
                    action = del,
                    flags = pair.Item2.Target
                };

                entries.Add(entry);
            }
        }

        public static RemapDelegate Find(BodyNode target) => 
            entries.Where(e => e.flags == target && e.action is RemapDelegate)
            .FirstOrDefault().action as RemapDelegate;

        public static RemapDelegate<TSettings> Find<TSettings>(BodyNode target = BodyNode.All) =>
            entries.Where(e => e.flags == target && e.action is RemapDelegate<TSettings>)
            .FirstOrDefault().action as RemapDelegate<TSettings>;

        public static RemapDelegate<TUniqueS, TUniqueD> Find<TUniqueS, TUniqueD>(BodyNode target = BodyNode.All) =>
            entries.Where(e => e.flags == target && e.action is RemapDelegate<TUniqueS, TUniqueD>)
            .FirstOrDefault().action as RemapDelegate<TUniqueS, TUniqueD>;

        public static RemapDelegate<TUniqueS, TUniqueD, TSettings> Find<TUniqueS, TUniqueD, TSettings>(
            BodyNode target = BodyNode.All) =>
            entries.Where(e => e.flags == target && e.action is RemapDelegate<TUniqueS, TUniqueD, TSettings>)
            .FirstOrDefault().action as RemapDelegate<TUniqueS, TUniqueD, TSettings>;
        
        
        /* These methods also allow you to find alternatives. However, this is super unstable and might break with interfaces
        
        private static int GetDistanceBetweenTypes(Type child, Type parent)
        {
            if (!parent.IsAssignableFrom(child))
                return -1;

            int distance = 0;
            while (true)
            {
                if (child == parent)
                    break;

                child = child.BaseType;
                distance++;
            }

            return distance;
        }
        
        public static RemapDelegate<TData> Find<TData>(BodyNode target, FallbackFilter filter = FallbackFilter.None)
        {
            var result = entries
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

            if (result.entry.action == null)
                return null;

            if (result.distance == 0)
                return result.entry.action as RemapDelegate<TData>;

            if (filter == FallbackFilter.None)
                return null;

            RemapDelegate<TData> wrapper = (in BodyTree<Matrix4x4> source, ref BodyTree<Matrix4x4> destination, in TData data) => 
            {
                object[] p = { source, destination, data };
                result.entry.action.DynamicInvoke(p);

                destination = (BodyTree<Matrix4x4>) p[1];
            };


            return wrapper;
        }

        public static RemapDelegate<TUniqueS, TUniqueD> Find<TUniqueS, TUniqueD>(BodyNode target, FallbackFilter filter = FallbackFilter.None)
        {
            var candidates = entries.Where(
                e => e.flags == target && e.action.GetType().GetGenericArguments().Length == 2);

            Debug.Log(candidates.Count());

            candidates = candidates.Where(entry =>
            {
                Type[] types = entry.action.GetType().GetGenericArguments();
                return types[0].IsAssignableFrom(typeof(TUniqueS)) && types[1].IsAssignableFrom(typeof(TUniqueD));
            });

            Debug.Log(candidates.Count());

            var c = candidates.Select(entry =>
            {
                Type[] types = entry.action.GetType().GetGenericArguments();
                int distanceS = GetDistanceBetweenTypes(typeof(TUniqueS), types[0]);
                int distanceD = GetDistanceBetweenTypes(typeof(TUniqueD), types[1]);

                return (entry, distanceS, distanceD);
            });

            Debug.Log(c.Count());
            if (filter == FallbackFilter.None)
                return c.Where(c => c.distanceS == 0 && c.distanceD == 0)
                    .Select(c => c.entry.action).FirstOrDefault() as RemapDelegate<TUniqueS, TUniqueD>;

            var y = c.Select(candidate =>
            {
                int distance = -1;
                switch (filter)
                {
                    case FallbackFilter.Minimum:
                        distance = Mathf.Min(candidate.distanceS, candidate.distanceD);
                        break;
                }

                return (candidate.entry, distance);
            }).Where(e => e.distance != -1).OrderBy(c => c.distance).Select(c => c.entry.action).FirstOrDefault();


            if(y != null)
            {
                RemapDelegate<TUniqueS, TUniqueD> wrapper = (in BodyTree<Matrix4x4> source, in TUniqueS sourceData, ref BodyTree<Matrix4x4> destination, in TUniqueD destinationData) =>
                {
                    object[] p = { source, sourceData, destination, destinationData };
                    y.DynamicInvoke(p);

                    destination = (BodyTree<Matrix4x4>)p[2];
                };

                return wrapper;
            }

            return null;
        }*/
    }
}
