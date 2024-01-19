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
    public enum ValueType
    {
        Property,
        Field
    }
    [ClassAPI("全局反射系统(慎用)")]  
    public static class GlobalReflectionSystem
    {
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
            InvokeMethod(typeof(T),target,methodName,args);
        }

        [MethodAPI("直接通过反射执行方法(性能开销极大，请谨慎使用)在同一时刻内会调用同类型所有的方法")]
        public static void InvokeMethod(Type type, object target, string methodName, params object[] args)
        {
            if (Invoke(methodName,type, out var info))
            {
                info.Invoke(target,args);
            }
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
        public static object GetValue<T>(this T target, string parameterName,ValueType type = ValueType.Field)
        {
            switch (type)
            {
                case ValueType.Property:
                    return GetOrSetPropertyValue(target, parameterName);
                 
                case ValueType.Field:
                    return GetOrSetFieldValue(target, parameterName);
                default:
                    return null;                  
            }           
        }
        /// <summary>
        /// 直接通过反射赋值目标字段(属性),包括私有化,但如果是属性则必须要有setter
        /// </summary>        
        [MethodAPI("直接通过反射赋值目标字段(属性),包括私有化,但如果是属性则必须要有setter")]
        public static void SetValue<T>(this T target, string parameterName,object value, ValueType type = ValueType.Field) where T : class
        {
            switch (type)
            {
                case ValueType.Property:
                    GetOrSetPropertyValue(target, parameterName, true, value);
                    break;
                case ValueType.Field:
                    GetOrSetFieldValue(target, parameterName, true, value);
                    break;               
            }
        }

        private static object GetOrSetFieldValue<T>(T target,string parameterName,bool setting = false,object arg = null)
        {
            try
            {
                foreach (var field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static
                    | BindingFlags.NonPublic))
                {
                    if (field.Name.Equals(parameterName))
                    {
                        if (setting)
                            field.SetValue(target, arg);
                        return field.GetValue(target);
                    }
                }           
            }
            catch(Exception ex)
            {
                Debug.LogError(ex + "查找字段失败,请检查参数名是否存在！");               
            }
            return null;
        }

        private static object GetOrSetPropertyValue<T>(T target, string parameterName, bool setting = false, object arg = null)
        {
            try
            {
                foreach (var property in typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.Static
                    | BindingFlags.NonPublic | BindingFlags.GetProperty | BindingFlags.SetProperty))
                {
                    if (property.Name.Equals(parameterName))
                    {
                        if (setting)                        
                            property.SetValue(target, arg);                                                  
                        return property.GetValue(target);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError(ex + "查找属性失败,请检查参数名是否存在或者该方法有没有公开Getter/Setter！");
            }
            return null;
        }

        private static bool Invoke(string methodName,Type type,out MethodInfo methodInfo)
        {
            methodInfo = null;
         
            foreach (var method in type.GetMethods(BindingFlags.NonPublic |
                BindingFlags.InvokeMethod|              
                BindingFlags.Instance))
            {
                foreach (var attribute in method.GetCustomAttributes())
                {                  
                    if (attribute is DynamicMethodAttribute && method.Name.Equals(methodName))
                    {
                        methodInfo = method;
                        return true;
                    } 
                }
            }

            foreach (var method in type.GetMethods(BindingFlags.Public |
                BindingFlags.InvokeMethod |
                BindingFlags.Instance))
            {
                if (method.Name.Equals(methodName))
                {
                    methodInfo = method;
                    return true;
                }
            }
            return false;
        }
    }
}