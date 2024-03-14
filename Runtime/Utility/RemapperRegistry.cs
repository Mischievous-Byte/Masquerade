using JetBrains.Annotations;
using MischievousByte.Masquerade.Anatomy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Video;

namespace MischievousByte.Masquerade.Utility
{
    public static class RemapperRegistry
    {

#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
#endif
        private static void OnLoad() { } //Empty method to call static constructor

        
        static RemapperRegistry()
        {
            float a = 10f;
            BodyTree<Matrix4x4> input = new BodyTree<Matrix4x4>();
            BodyTree<Matrix4x4> output = new BodyTree<Matrix4x4>();
            
            FindFlaggedMethods();

            //Find<float>()(in input, in a, out output, in a);
        }



        private static void FindFlaggedMethods()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                foreach (var type in assembly.GetTypes())
                    foreach (var method in type.GetMethods())
                        if (method.GetCustomAttribute<RemapperAttribute>() != null)
                            try
                            {
                                ProcessRemappingMethodInfo(method);
                            } catch(Exception ex)
                            {
                                Debug.LogException(ex);
                            }
        }


        private static void ProcessRemappingMethodInfo(MethodInfo info)
        {
            if (!info.IsStatic)
                throw new ArgumentException();

            var parameters = info.GetParameters();

            

            bool IsSimple()
            {
                if (parameters.Length != 4)
                    return false;

                if (!parameters[0].IsIn || parameters[0].ParameterType != typeof(BodyTree<Matrix4x4>).MakeByRefType())
                    return false;

                if (!parameters[2].ParameterType.IsByRef || parameters[2].ParameterType != typeof(BodyTree<Matrix4x4>).MakeByRefType())
                    return false;

                if (!parameters[1].IsIn || !parameters[3].IsIn)
                    return false;

                if (parameters[1].GetType() != parameters[3].GetType())
                    return false;

                return true;
            }

            bool IsComplex()
            {
                if (parameters.Length != 5)
                    return false;

                if (!parameters[0].IsIn || parameters[0].ParameterType != typeof(BodyTree<Matrix4x4>))
                    return false;

                if (!parameters[2].ParameterType.IsByRef || parameters[2].ParameterType != typeof(BodyTree<Matrix4x4>))
                    return false;

                if (!parameters[1].IsIn || !parameters[3].IsIn)
                    return false;

                if (parameters[1].GetType() != parameters[3].GetType())
                    return false;

                if (!parameters[4].IsIn)
                    return false;

                return true;
            }

            Type GetBase(Type t)
            {
                var u = t.GetElementType();

                return u == null ? t : u;
            }

            if (IsSimple())
                remappers.Add(Delegate.CreateDelegate(
                    typeof(RemapDelegate<>).MakeGenericType(GetBase(parameters[1].ParameterType)),
                    info));
            else if (IsComplex())
                remappers.Add(Delegate.CreateDelegate(
                    typeof(RemapDelegate<>).MakeGenericType(
                        GetBase(parameters[1].ParameterType), 
                        GetBase(parameters[4].ParameterType)),
                    info));
            else
                throw new ArgumentException();
        }


        public delegate void RemapDelegate<TUnique>(in BodyTree<Matrix4x4> source, in TUnique sourceData, out BodyTree<Matrix4x4> destination, in TUnique destinationData);
        public delegate void RemapDelegate<TUnique, TSettings>(in BodyTree<Matrix4x4> source, in TUnique sourceData, out BodyTree<Matrix4x4> destination, in TUnique destinationData, in TSettings settings);

        private static List<Delegate> remappers = new List<Delegate>();

        public static void Register<TUnique>(RemapDelegate<TUnique> method)
        {
            if (method == null)
                throw new ArgumentNullException();

            if (remappers.Contains(method))
                return;

            remappers.Add(method);
        }

        public static void Register<TUnique, TSettings>(RemapDelegate<TUnique, TSettings> method)
        {
            if (method == null)
                throw new ArgumentNullException();

            if (remappers.Contains(method))
                return;

            remappers.Add(method);
        }


        public static RemapDelegate<TUnique> Find<TUnique>()
        {
            RemapDelegate<TUnique> bestMatch = null;

            foreach(var r in remappers)
            {
                //Find first options available
                if (r is RemapDelegate<TUnique> x && bestMatch == null)
                    bestMatch = x;

                //Keep looking for an exact match
                Type[] generics = r.GetType().GetGenericArguments();

                if (generics.Length != 1)
                    continue;

                if (r.GetType().GetGenericArguments()[0] == typeof(TUnique))
                    return r as RemapDelegate<TUnique>;
            }
            
            return bestMatch;
        }

        public static RemapDelegate<TUnique, TSettings> Find<TUnique, TSettings>()
        {
            RemapDelegate<TUnique, TSettings> bestMatch = null;

            foreach (var r in remappers)
            {
                //Find first options available
                if (r is RemapDelegate<TUnique, TSettings> x && bestMatch == null)
                    bestMatch = x;

                //Keep looking for an exact match
                Type[] generics = r.GetType().GetGenericArguments();

                if (generics.Length != 2)
                    continue;

                if (generics[0] == typeof(TUnique) && generics[2] == typeof(TSettings))
                    return r as RemapDelegate<TUnique, TSettings>;
            }

            return bestMatch;
        }
    }
}
