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
using System.Linq;
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
        public static void InvokeMethod(this object target,string methodName,params object[] args)
        {
            InvokeMethod(target,target.GetType(), methodName, args);
        }

        /// <summary>
        /// 直接通过反射执行方法(性能开销极大，请谨慎使用)
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="target">需要调用方法的对象</param>
        /// <param name="methodName">方法名</param>
        /// <param name="args">参数</param>
        [MethodAPI("直接通过反射执行方法(性能开销极大，请谨慎使用)")]
        public static void InvokeMethod(this object target,Type type, string methodName, params object[] args)
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
        public static object GetValue(this object target, string parameterName)
        {
            return GetValue(target,target.GetType(), parameterName);
        }

        /// <summary>
        /// 直接通过反射获取目标字段,包括任何私有化,以及静态字段，如果是属性则必须Getter或者Setter其中之一是公开的
        /// </summary>
        /// <param name="target"></param>
        /// <param name="type"></param>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        [MethodAPI("直接通过反射获取目标字段,包括任何私有化,以及静态字段，如果是属性则必须Getter或者Setter其中之一是公开的")]
        public static object GetValue(this object target,Type type, string parameterName)
        {
            return Get(type, target, parameterName);
        }
 
        private static object Get(Type type,object target, string parameterName)
        {
            FieldInfo field = type.GetRealField(parameterName);
            if(field != null)
                return field.GetValue(target);
            PropertyInfo propertyInfo = type.GetRealProperty(parameterName);
             if(propertyInfo != null)
                return propertyInfo.GetValue(target);

            return null;
        }  

        private static void Set(Type type,object target, object value, string parameterName)
        {
            FieldInfo field = type.GetRealField(parameterName);
            if (field != null)
                field.SetValue(target, value);
            else
            {
                PropertyInfo propertyInfo = type.GetRealProperty(parameterName);
                if (propertyInfo != null)
                    propertyInfo.SetValue(target,value);
            }                              
        }
        [Obsolete("过时的获取元素信息方法，MemberInfo包含重载在内的元素信息。而非个体。不应该通过单一的方式获取，可自行使用type.GetMember进行获取", true)]
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

        [Obsolete("过时的获取元素信息方法，MemberInfo包含重载在内的元素信息。而非个体。不应该通过单一的方式获取，可自行使用type.GetMember进行获取",true)]
        public static MemberInfo GetMemberInfo(Type type,string parameterName)
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

        public static IEnumerable<MemberInfo> GetRuntimeMemberInfos(this Type type,bool IsAddMethod = false)
        {
            IEnumerable<FieldInfo> fieldInfos = type.GetRuntimeFields();
            IEnumerable<PropertyInfo> propertyInfos = type.GetRuntimeProperties();
            IEnumerable<MethodInfo> methodInfos = IsAddMethod ? type.GetRuntimeMethods() : default;
            List<MemberInfo> memberInfos = new List<MemberInfo>(fieldInfos.Count() + propertyInfos.Count() + (methodInfos == null ? 0 : methodInfos.Count()));
            memberInfos.AddRange(fieldInfos);
            memberInfos.AddRange(propertyInfos);
            if (IsAddMethod)
                memberInfos.AddRange(methodInfos);
            return memberInfos;
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
        public static void SetValue(this object target, string parameterName,object value)
        {
            SetValue(target,target.GetType(), parameterName,value);
        }

        /// <summary>
        /// 直接通过反射赋值目标字段(属性),包括私有化,但如果是属性则必须要有setter
        /// </summary>        
        [MethodAPI("直接通过反射赋值目标字段(属性),包括私有化,但如果是属性则必须要有setter")]
        public static void SetValue(this object target,Type type, string parameterName, object value)
        {         
            Set(type,target,value,parameterName);
        }    

        public static bool HasCustomAttribute<T>(this Type type, bool inherit, out T attribute) where T : Attribute
        {
            attribute = type.GetCustomAttribute<T>(inherit);
            return attribute != null;
        }

        public static bool HasCustomAttribute<T>(this Type type, bool inherit = false) where T : Attribute
        {
            return HasCustomAttribute<T>(type, inherit,out _);
        }

        public static bool HasCustomAttribute<T>(this MemberInfo info, bool inherit, out T attribute) where T : Attribute
        {
            attribute = info.GetCustomAttribute<T>(inherit);          
            return attribute != null;
        }

        public static bool HasCustomAttribute<T>(this MemberInfo info, bool inherit = false) where T : Attribute
        {           
            return HasCustomAttribute<T>(info, inherit, out _);
        }

        public static bool HasCustomAttribute<T>(this ParameterInfo info, bool inherit, out T attribute) where T : Attribute
        {
            attribute = info.GetCustomAttribute<T>(inherit);
            return attribute != null;
        }

        public static bool HasCustomAttribute<T>(this ParameterInfo info, bool inherit = false) where T : Attribute
        {
            return HasCustomAttribute<T>(info, inherit, out _);
        }

        /// <summary>
        ///  获取某个类型所有的字段数据
        ///  <para>与默认提供的反射API不同 该API会对于这个类型的基类字段进行添加并获取</para>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<FieldInfo> GetRealFields(this Type type)
        {
            if (type == null) return null;

            List<FieldInfo> fields = new List<FieldInfo>();

            fields.AddRange(type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            if (type.BaseType != null)
                fields.AddRange(GetRealFields(type.BaseType));
            return fields;
        }
        /// <summary>
        ///  获取某个类型所有的属性数据
        ///  <para>与默认提供的反射API不同 该API会对于这个类型的基类属性进行添加并获取</para>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static List<PropertyInfo> GetRealProperties(this Type type)
        {
            if (type == null) return null;

            List<PropertyInfo> properties = new List<PropertyInfo>();

            properties.AddRange(type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance));
            if (type.BaseType != null)
                properties.AddRange(GetRealProperties(type.BaseType));
            return properties;
        }


        /// <summary>
        ///  获取某个类型字段数据
        ///  <para>与默认提供的反射API不同 该API会对于这个类型的基类字段进行添加并获取</para>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static FieldInfo GetRealField(this Type type, string name)
        {
            if (type == null) return null;

            FieldInfo field = null;

            field = type.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (type.BaseType != null && field == null)
                field = GetRealField(type.BaseType, name);
            return field;
        }
        /// <summary>
        ///  获取某个类型的属性数据
        ///  <para>与默认提供的反射API不同 该API会对于这个类型的基类属性进行添加并获取</para>
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PropertyInfo GetRealProperty(this Type type, string name)
        {
            if (type == null) return null;

            PropertyInfo property = null;

            property = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (type.BaseType != null && property == null)
                property = GetRealProperty(type.BaseType, name);
            return property;
        }
    }
}