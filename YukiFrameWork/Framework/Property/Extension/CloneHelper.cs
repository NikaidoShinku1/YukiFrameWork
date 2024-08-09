///=====================================================
/// - FileName:      CloneHelper.cs
/// - NameSpace:     YukiFrameWork.Farm
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/8/8 14:59:37
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
namespace YukiFrameWork
{
	public class CloneHelper
	{
        /// <summary>
        /// 贱复制
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static T Copy<T>(object target) where T : class
        {
            var t = typeof(T);
            var obj = (T)Activator.CreateInstance(t);
            var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                field.SetValue(obj, field.GetValue(target));
            }
            return obj;
        }

        /// <summary>
        /// 深复制
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(object target) where T : class
        {
            return (T)DeepCopy(typeof(T), target);
        }

        /// <summary>
        /// 深复制
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="target"></param>
        /// <returns></returns>
        public static T DeepCopy<T>(object target, List<Type> copyTypes) where T : class
        {
            return (T)DeepCopy(typeof(T), target, copyTypes);
        }

        /// <summary>
        /// 深复制
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public static object DeepCopy(object target)
        {
            return DeepCopy(target.GetType(), target);
        }

        /// <summary>
        /// 深复制
        /// </summary>
        /// <param name="type"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static object DeepCopy(Type type, object target)
        {
            return DeepCopy(type, target, new List<Type>() { typeof(UnityEngine.Object) });
        }

        /// <summary>
        /// 深复制
        /// </summary>
        /// <param name="type"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public static object DeepCopy(Type type, object target, List<Type> copyTypes)
        {
            var obj = Activator.CreateInstance(type);
            var loops = new List<object>
            {
                target
            };
            DeepCopy(ref target, ref obj, loops, copyTypes);
            return obj;
        }

        private static void DeepCopy(ref object source, ref object value, List<object> loops, List<Type> copyTypes)
        {
            var t = value.GetType();
            var fields = t.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (field.FieldType.IsPointer)
                    continue;
                bool copy = false;
                foreach (var item in copyTypes)
                {
                    if (field.FieldType == item | field.FieldType.IsSubclassOf(item))
                    {
                        copy = true;
                        break;
                    }
                }
                var code = Type.GetTypeCode(field.FieldType);
                if (code != TypeCode.Object | copy)
                {
                    field.SetValue(value, field.GetValue(source));
                }
                else if (field.FieldType.IsArray)
                {
                    var itemType = field.FieldType.GetArrayItemType();
                    if (itemType.IsPointer)
                        continue;
                    Array list = (Array)field.GetValue(source);
                    if (list == null)
                        continue;
                    Array list1 = Array.CreateInstance(itemType, list.Length);
                    copy = false;
                    foreach (var item in copyTypes)
                    {
                        if (itemType == item | itemType.IsSubclassOf(item))
                        {
                            copy = true;
                            break;
                        }
                    }
                    if (Type.GetTypeCode(itemType) != TypeCode.Object | copy)
                    {
                        Array.Copy(list, list1, list.Length);
                    }
                    else
                    {
                        for (int i = 0; i < list.Length; i++)
                        {
                            var value1 = list.GetValue(i);
                            var value2 = Activator.CreateInstance(itemType);
                            if (value1 == null)
                                continue;
                            DeepCopy(ref value1, ref value2, loops, copyTypes);
                            list1.SetValue(value2, i);
                        }
                    }
                    field.SetValue(value, list1);
                }
                else if (field.FieldType.IsGenericType)
                {
                    var arguments = field.FieldType.GenericTypeArguments;
                    if (arguments.Length != 1)
                        continue;
                    Type itemType = arguments[0];
                    if (itemType.IsPointer)
                        continue;
                    IList list = (IList)field.GetValue(source);
                    if (list == null)
                        continue;
                    IList list1 = (IList)Activator.CreateInstance(field.FieldType);
                    copy = false;
                    foreach (var item in copyTypes)
                    {
                        if (itemType == item | itemType.IsSubclassOf(item))
                        {
                            copy = true;
                            break;
                        }
                    }
                    if (Type.GetTypeCode(itemType) != TypeCode.Object | copy)
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            list1.Add(list[i]);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < list.Count; i++)
                        {
                            var value1 = list[i];
                            var value2 = Activator.CreateInstance(itemType);
                            if (value1 == null)
                            {
                                list1.Add(value2);//如果列表=null 列表也要统一长度
                                continue;
                            }
                            DeepCopy(ref value1, ref value2, loops, copyTypes);
                            list1.Add(value2);
                        }
                    }
                    field.SetValue(value, list1);
                }
                else if (!field.IsNotSerialized)
                {
                    var value1 = field.GetValue(source);
                    var value2 = field.GetValue(value);
                    if (field.FieldType.IsValueType)
                    {
                        DeepCopy(ref value1, ref value2, loops, copyTypes);
                        field.SetValue(value, value1);
                    }
                    else
                    {
                        if (value1 == null)
                            continue;
                        var srType = value1.GetType();
                        copy = false;
                        foreach (var item in copyTypes)
                        {
                            if (srType == item | srType.IsSubclassOf(item))
                            {
                                copy = true;
                                break;
                            }
                        }
                        if (copy) //解决无限递归死循环, 相同引用的对象需要添加到types参数
                        {
                            field.SetValue(value, value1);
                            continue;
                        }
                        if (value2 == null)
                        {
                            var type = value1.GetType();
                            var ctr = type.GetConstructor(new Type[0]);
                            if (ctr == null)
                                continue;
                            value2 = ctr.Invoke(null);
                        }
                        if (loops.Contains(value2))
                            continue;
                        loops.Add(value2);
                        DeepCopy(ref value1, ref value2, loops, copyTypes);
                        field.SetValue(value, value2);
                    }
                }
            }
        }
    }
}
