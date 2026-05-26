///=====================================================
/// - FileName:      TimeRewinder.cs
/// - NameSpace:     YukiFrameWork.Time
/// - Description:   高级定制脚本生成
/// - Creation Time: 2026/5/23 15:57:33
/// -  (C) Copyright 2008 - 2026
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;

namespace YukiFrameWork.Rewinder
{
    public static class TimeRewinder
    {
        private static Dictionary<Type, IRewinder> rewinders = new Dictionary<Type, IRewinder>();
        
        /// <summary>
        /// 转换为一个倒带器实例.必定有值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T As<T>() where T : IRewinder,new()
        {
            if(rewinders.TryGetValue(typeof(T),out IRewinder rewinder))
            {
                return (T)rewinder;
            }
            else
            {
                rewinder = new T();
                rewinders.Add(typeof(T), rewinder);
                return rewinder.As<T>();
            }
        }

        /// <summary>
        /// 转换为倒带器实例,如存在缓存则返回True,否则False且值为空
        /// </summary>
        /// <param name="rewinder"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool TryAs<T>(out T rewinder) where T : IRewinder
        {
            if (rewinders.TryGetValue(typeof(T), out var item))
            {
                rewinder = (T)item;
                return true;
            }
            rewinder = default(T);
            return false;
        }

        /// <summary>
        /// 倒带器初始化
        /// </summary>
        /// <param name="recordTime"></param>
        /// <param name="param"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Initialize<T>(float recordTime,params object[] param)  where T : IRewinder,new()
        {
            T rewinder = As<T>();
            rewinder.Initialize(recordTime,param);
            return rewinder;
        }
        

        [Obsolete("过时的倒带器回放方法,请使用PlayBack方法进行回放!")]
        public static T BackFlow<T>(float seconds) where T : IRewinder, new()
        {
            return PlayBack<T>(seconds);
        }
        
        /// <summary>
        /// 倒带器回放到指定时刻
        /// <para>Tips:回放时间即为记录的时间,比如回放1.5s即为周期内的1.5s本身</para>
        /// </summary>
        /// <param name="seconds"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T PlayBack<T>(float seconds) where T : IRewinder
        {
            if(!TryAs<T>(out T rewinder))
                return default(T);
            rewinder.PlayBack(seconds);
            return rewinder;
        }
        
        /// <summary>
        /// 倒带器回放到记录时间的最后,如RecordTime为10,则回放到10秒前的状态
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T PlayBack<T>() where T : IRewinder
        {
            if(!TryAs<T>(out T rewinder))
                return default(T);
            rewinder.PlayBack(rewinder.RecordTime);
            return rewinder;
        }

        /// <summary>
        /// 完成倒带：停止继续记录，但保留缓冲区中的历史数据以供回放
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Complete<T>() where T : IRewinder
        {
            if (!TryAs<T>(out T rewinder))
                return default(T);
            rewinder.Complete();
            return rewinder;
        }

        /// <summary>
        /// 释放倒带器并从缓存中移除，之后需重新 Initialize 才能再次使用
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns>成功释放返回 true，实例不存在则返回 false</returns>
        public static bool Release<T>(bool clearCache = false) where T : IRewinder
        {
            if (!TryAs<T>(out T rewinder))
                return false;
            rewinder.Release();
            if (clearCache)
                rewinders.Remove(typeof(T));
            return true;
        }

        /// <summary>
        /// 尝试完成倒带：停止继续记录，但保留缓冲区中的历史数据以供回放
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="rewinder"></param>
        /// <returns>实例存在且成功完成倒带返回 true</returns>
        public static bool TryComplete<T>(out T rewinder) where T : IRewinder
        {
            if (!TryAs<T>(out rewinder))
                return false;
            rewinder.Complete();
            return true;
        }
        

        /// <summary>
        /// 回放到指定时刻后自动释放倒带器
        /// </summary>
        /// <param name="seconds"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T PlayBackAndRelease<T>(float seconds) where T : IRewinder
        {
            if (!TryAs<T>(out T rewinder))
                return default(T);
            rewinder.PlayBack(seconds);
            Release<T>();
            return rewinder;
        }

        /// <summary>
        /// 回放到记录窗口最旧的状态后自动释放倒带器
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T PlayBackAndRelease<T>() where T : IRewinder
        {
            if (!TryAs<T>(out T rewinder))
                return default(T);
            rewinder.PlayBack(rewinder.RecordTime);
            Release<T>();
            return rewinder;
        }
    }
}
