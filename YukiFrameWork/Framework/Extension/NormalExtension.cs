﻿///=====================================================
/// - FileName:      NormalExtension.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/14 16:07:15
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace YukiFrameWork
{
	public static class NormalExtension
	{
		public static bool IsNullOrEmpty(this string actor)
			=> string.IsNullOrEmpty(actor);

        private const int kb = 1024;
        private const int mb = kb * 1024;
        private const int gb = mb * 1024;
        private const long tb = gb * (long)1024;

        /// <summary>
        /// 格式化字节 例如: 1024 = 1kb
        /// </summary>
        /// <param name="length">字节长度</param>
        public static string ToByteString(this long length)
        {
            if (length < kb)
                return string.Format("{0}b", length);
            else if (length >= kb && length < mb)
                return string.Format("{0:N2}kb", length / 1024.0f);
            else if (length >= mb && length < gb)
                return string.Format("{0:N2}mb", length / 1024.0f / 1024.0f);
            else if (length >= gb && length < tb)
                return string.Format("{0:N2}gb", length / 1024.0f / 1024.0f / 1024.0f);

            return "";
        }

        public static T Add<T>(this T obj, IList<T> list)
        {
            list.Add(obj);
            return obj;
        }       

        public static T Remove<T>(this T obj, IList<T> list)
        {
            list.Remove(obj);
            return obj;
        }

        public static T IndexOf<T>(this T obj, IList<T> list,out int index)
        {
            index = list.IndexOf(obj);
            return obj;
        }

        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> enumerable, Action<T> each)
        {
            foreach (var item in enumerable)
            {
                each?.Invoke(item);
            }           
            return enumerable;
        }

        public static IList<T> For<T>(this IList<T> list, Action<T> each)
        {
            for (int i = 0; i < list.Count; i++)
            {
                each?.Invoke(list[i]);
            }
            return list;
        }

        public static IList<T> For<T>(this IList<T> list, Func<T, bool> condition , Action<T> each)
        {
            for (int i = 0; condition?.Invoke(list[i]) == true; i++)
            {
                each?.Invoke(list[i]);
            }

            return list;
        }

        public static HashSet<T> AddRange<T>(this HashSet<T> core, IEnumerable<T> enumerable)
        {
            foreach (var item in enumerable)
                core.Add(item);
            return core;
        }

        public static string Color(this string content, Color color) => $"<color=#{ColorUtility.ToHtmlStringRGB(color)}>{content}</color>";
        /// <summary>
        /// 把 0 -1 转换成 0% - 100%
        /// </summary>
        /// <returns></returns>
        public static string ToProgressString(this float progress)
        {
            return string.Format("{0}%", Mathf.Lerp(0, 100, progress));
        }

        /// <summary>
        /// 判断是否是空的，如果是则返回True
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsNull(this object obj)
            => ReferenceEquals(obj, null);

        /// <summary>
        /// 判断UObject是不是空的，是则返回True
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static bool IsMonoNull(this UnityEngine.Object obj)
            => obj.IsDestroy();

        public static void Sort<T>(this IList<T> list)
        {
            if (list is List<T>)
            {
                ((List<T>)list).Sort();
                return;
            }
            List<T> current = new List<T>(list);
            current.Sort();


            for (int i = 0; i < list.Count; i++)
            {
                list[i] = current[i];
            }
        }

        public static object CreateInstance(this Type type)
            => Activator.CreateInstance(type);

        public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
        {

            if (list is List<T>)
            {
                ((List<T>)list).Sort(comparison);
                return;
            }
            List<T> current = new List<T>(list);
            current.Sort(comparison);


            for (int i = 0; i < list.Count; i++)
            {
                list[i] = current[i];
            }
        }
    }
}
