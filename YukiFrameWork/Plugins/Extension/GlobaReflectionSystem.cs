///=====================================================
/// - FileName:      GlobalEventSystem.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   全局事件中心(反射，慎用)
/// - Creation Time: 2023/12/23 17:50:03
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using YukiFrameWork.Extension;

namespace YukiFrameWork
{
    [ClassAPI("全局反射系统(慎用)")]
    public static class GlobalReflectionSystem
    {
        class GlobalMethodInfo
        {             
            /// <summary>
            /// 保存可能在相同名称下有重载的方法集合
            /// </summary>
            private List<MethodInfo> methodInfos = new List<MethodInfo>();

            public GlobalMethodInfo Add(MethodInfo info)
            {
                methodInfos.Add(info);
                return this;
            }

            public MethodInfo GetMethodInfo(Type[] types)
            {
                foreach (var info in methodInfos)
                {
                    var parameterInfos = info.GetParameters();
                    if (parameterInfos.Length != types.Length) continue;
                    bool IsContinue = false;
                    for (int i = 0; i < types.Length; i++)
                    {
                        if (parameterInfos[i].ParameterType != types[i])
                        {
                            IsContinue = true;
                            break;
                        }
                    }
                    if (IsContinue) continue;

                    return info;
                }
                return null;
            }
        }

        private static Dictionary<Type, Dictionary<string,GlobalMethodInfo>> methodDicts;
        private static Dictionary<Type, Dictionary<string, MemberInfo>> valueDicts;

        static GlobalReflectionSystem()
        {
            methodDicts = new Dictionary<Type, Dictionary<string, GlobalMethodInfo>>();
            valueDicts = new Dictionary<Type, Dictionary<string, MemberInfo>>();
        }
        /// <summary>
        /// 直接通过反射执行方法(性能开销极大，请谨慎使用)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="target">需要调用方法的对象</param>
        /// <param name="methodName">方法名</param>
        /// <param name="args">参数</param>
        [MethodAPI("直接通过反射执行方法(性能开销极大，请谨慎使用)")]     
        public static void InvokeMethod<T>(this T target,string methodName,params object[] args) where T : class
        {
            InvokeMethod(typeof(T), target, methodName, args);
        }

        /// <summary>
        /// 直接通过反射执行方法(性能开销极大，请谨慎使用)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="target">需要调用方法的对象</param>
        /// <param name="methodName">方法名</param>
        /// <param name="args">参数</param>
        [MethodAPI("直接通过反射执行方法(性能开销极大，请谨慎使用)")]
        public static void InvokeMethod(Type type,object target, string methodName, params object[] args)
        {
            Type[] types = new Type[args.Length];

            for (int i = 0; i < types.Length; i++)
            {
                types[i] = args[i].GetType();
            }

            var info = GetMethodInfos(type,methodName, types);

            if (info == null)
            {
                Debug.LogWarning($"无法发送{methodName}方法,请检查在{type}是否存在该方法!或者检查类型参数是否完全匹配!传递的的方法参数个数是{args.Length}");
                return;
            }

            MethodInfo method = info as MethodInfo;

            method.Invoke(target, args);
        }

        /// <summary>
        /// 直接通过反射获取目标字段,包括任何私有化,以及静态字段，如果是属性则必须Getter或者Setter其中之一是公开的
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="target">目标</param>
        /// <param name="parameterName">参数名</param>
        /// <param name="type">需要获取的Value的类型</param>
        /// <returns>直接返回这个类的字段(属性)</returns>
        [MethodAPI("直接通过反射获取目标字段,包括任何私有化,以及静态字段，如果是属性则必须Getter或者Setter其中之一是公开的")]
        public static object GetValue<T>(this T target, string parameterName)
        {
            return Get(target, parameterName);
        }

        public static object GetValue(Type type, object target, string parameterName)
        {
            return Get(type, target, parameterName);
        }

        private static object Get<T>(T target, string parameterName)
        {
            return Get(typeof(T), target, parameterName);
        }


        private static object Get(Type type,object target, string parameterName)
        {       
            var info = GetMemberInfo(type,parameterName);
            if (info == null) return null;

            if (info is FieldInfo field)
                return field.GetValue(target);
            else if (info is PropertyInfo property)
                return property.GetValue(target);

            return null;
        }

        private static void Set<T>(T target,object value, string parameterName)
        {
            Set(typeof(T), target, value, parameterName);
        }

        private static void Set(Type type,object target, object value, string parameterName)
        {
            MemberInfo info = GetMemberInfo(type,parameterName);

            if (info == null) return;
            
            if (info is FieldInfo field)
                field.SetValue(target, value);
            else if (info is PropertyInfo property)
                property.SetValue(target, value);
        }

        public static MemberInfo GetMemberInfo<T>(string parameterName)
        {
            return GetMemberInfo(typeof(T), parameterName);
        }

        private static MemberInfo GetMemberInfoInValueDict(Type type, string parameterName)
        {
            if (valueDicts.ContainsKey(type))
            {
                if (valueDicts[type].ContainsKey(parameterName))
                {
                    return valueDicts[type][parameterName];
                }
            }
            return null;
        }

        private static void SetMemberInfoInValueDict(Type type, string parameterName,MemberInfo info)
        {
            if (valueDicts.ContainsKey(type))
            {
                valueDicts[type][parameterName] = info;
            }            
        }
        private static MemberInfo GetMemberInfo(Type type,string parameterName)
        {
            var info = GetMemberInfoInValueDict(type, parameterName);
            if (info != null) return info;

            MemberInfo[] infos = type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic
                | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public);
            for (int i = 0; i < infos.Length; i++)
            {
                if (infos[i].Name.Equals(parameterName))
                {
                    SetMemberInfoInValueDict(type, parameterName, infos[i]);
                    return infos[i];
                }
            }
            return null;
        }

        public static IEnumerable<MemberInfo> GetRuntimeMemberInfos(this Type type)
        {
            return type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic
                | BindingFlags.GetProperty | BindingFlags.SetProperty | BindingFlags.Public | BindingFlags.Default | BindingFlags.InvokeMethod | BindingFlags.PutDispProperty
                | BindingFlags.DeclaredOnly | BindingFlags.FlattenHierarchy);
        }

        private static MethodInfo GetMethodInfos<T>(string methodName,Type[] types)
        {
            return GetMethodInfos(typeof(T), methodName, types);           
        }

        private static MethodInfo GetMethodInfos(Type type,string methodName, Type[] types)
        {
            if (methodDicts.ContainsKey(type))
            {
                if (methodDicts[type].ContainsKey(methodName))
                {
                    MethodInfo info = methodDicts[type][methodName].GetMethodInfo(types);
                    if (info != null)
                        return info;
                }
            }
            IEnumerable<MethodInfo> infos = type.GetRuntimeMethods();

            ParameterInfo[] parameterInfos = null;
            foreach (var info in infos)
            {
                if (!info.Name.Equals(methodName)) continue;
                parameterInfos = info.GetParameters();

                if (parameterInfos.Length != types.Length) continue;


                bool IsContinue = false;
                for (int i = 0; i < types.Length; i++)
                {
                    if (parameterInfos[i].ParameterType != types[i])
                    {
                        IsContinue = true;
                        break;
                    }
                }
                if (IsContinue) continue;

                if (methodDicts.ContainsKey(type))
                    methodDicts[type].Add(methodName, new GlobalMethodInfo().Add(info));
                else methodDicts.Add(type, new Dictionary<string, GlobalMethodInfo> { {methodName, new GlobalMethodInfo().Add(info) } });
                return info;
            }
            return null;
        }

        /// <summary>
        /// 直接通过反射赋值目标字段(属性),包括私有化,但如果是属性则必须要有setter
        /// </summary>        
        [MethodAPI("直接通过反射赋值目标字段(属性),包括私有化,但如果是属性则必须要有setter")]
        public static void SetValue<T>(this T target, string parameterName,object value) where T : class
        {
            Set<T>(target, value,parameterName);
        }

        /// <summary>
        /// 直接通过反射赋值目标字段(属性),包括私有化,但如果是属性则必须要有setter
        /// </summary>        
        [MethodAPI("直接通过反射赋值目标字段(属性),包括私有化,但如果是属性则必须要有setter")]
        public static void SetValue(Type type,object target, string parameterName, object value)
        {
            Set(type,target, value, parameterName);
        }
    }
}