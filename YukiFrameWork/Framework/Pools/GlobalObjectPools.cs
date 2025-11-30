///=====================================================
/// - FileName:      GlobalObjectPools.cs
/// - NameSpace:     YukiFrameWork.Pools
/// - Description:   全局单例对象池
/// - Creation Time: 2024/4/23 3:37:23
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
using YukiFrameWork.Extension;
namespace YukiFrameWork.Pools
{
    public interface IGlobalSign
    {
        /// <summary>
        /// 对象是否处于闲置(回收)状态
        /// </summary>
        bool IsMarkIdle { get; set; }
        /// <summary>
        /// 当对象从全局池取出时调用的初始化方法
        /// </summary>
        void Init();
        /// <summary>
        /// 当对象通过全局池回收时触发的方法
        /// </summary>
        void Release();
    }
    public static class GlobalPoolsExtension
    {
        public static bool GlobalRelease<T>(this T sign) where T : IGlobalSign
            => GlobalObjectPools.GlobalRelease(sign);
    }

    public interface IPoolGenerator
    {
        Type Type { get; }
        IGlobalSign Create();
    }

    /// <summary>
    /// 对象池信息结构
    /// </summary>
    public struct PoolInfo
    {
        /// <summary>
        /// 池子最大数量
        /// </summary>
        public int maxCount;
        /// <summary>
        /// 池子当前物品数量
        /// </summary>
        public int recycleCount;
        /// <summary>
        /// 池子是否可以通过私有构造函数构建
        /// </summary>
        public bool isNoPublic;

        /// <summary>
        /// 自定义的生成器类型(如没有则为空)
        /// </summary>
        public Type generatorType;

        /// <summary>
        /// 池子的类型信息
        /// </summary>
        public Type type;
        /// <summary>
        /// 池子最后使用的时间
        /// </summary>
        public float lastTime;

        public override string ToString()
        {
            return $"池子最大数量:{maxCount}\n池子当前物品数量:{recycleCount}\n池子是否可以通过私有构造函数构建:{isNoPublic} <color=yellow>Tips:具有生成器时该属性恒定为False</color>\n自定义生成器类型:{generatorType}\n池子的类型信息:{type}\n池子最后使用时间:{lastTime}";
        }
    }
    /// <summary>
    /// 全局对象池,根据类型取出对象,对象池拥有独立的生命周期管理，每一分钟检查一次对象池是否超过五分钟未使用，如果超过五分钟未使用会将整个池清空释放,默认对象池容量为200
    /// </summary>
    public static class GlobalObjectPools 
    {
        internal const float RELEASEPOOL_TIMER = 60;
        internal const float MAXRELEASEPOOL_TIMER = 5 * 60;

        /// <summary>
        /// 判断全局对象池是否需要开启自动清理模式，开启后每隔五分钟会检测一次池子，如果有某个类型的池子超过五分钟未使用，则自动进行清理操作。
        /// </summary>
        public static bool IsAutomationClean { get; set; } = true;

        /// <summary>
        /// 设置默认初始化池子内生成的对象数量，默认为10，在池被创建时会自动添加指定数量的对象保存在池内。
        /// </summary>
        public static int InitializePoolCount { get; set; } = 10;
        internal class GlobalPool : IEnumerable<IGlobalSign>
        {
            public Queue<IGlobalSign> pools = new Queue<IGlobalSign>();
            public Type type;
            public IPoolGenerator generator;
            private int maxSize;

            internal bool IsNoPublic = false;
            public int MaxSize
            {
                get => maxSize;
                set
                {
                    maxSize = value;

                    if (pools != null)
                    {
                        if (maxSize > 0)
                        {
                            if (maxSize < Count)
                            {
                                int removeCount = Count - maxSize;
                                while (removeCount > 0)
                                {
                                    pools.Dequeue();
                                    --removeCount;
                                }
                            }
                        }
                    }
                }
            }
            //记录每次使用池子的时间
            public float lastTime;
            public int Count => pools.Count;
            public IGlobalSign Get()
            {
                var obj =  Count > 0 ? pools.Dequeue() : (generator != null ? generator.Create() : Activator.CreateInstance(type, IsNoPublic) as IGlobalSign);
                obj.IsMarkIdle = false;
                obj.Init();
                lastTime = Time.time;              
                return obj;
            }

            public bool Release(IGlobalSign obj)
            {
                lastTime = Time.time;
                if (pools.Count >= maxSize)
                    return false;
                obj.IsMarkIdle = true;
                obj.Release();              
                pools.Enqueue(obj);
                return true;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<IGlobalSign> GetEnumerator()
            {
                return pools.GetEnumerator();
            }
        }      
        static GlobalObjectPools()
        {           
            if (!Application.isPlaying) return;
            MonoHelper.Start(CheckPools());
        }

        private static IEnumerator CheckPools()
        {
            while (true)
            {
                yield return CoroutineTool.WaitForSeconds(RELEASEPOOL_TIMER);

                if (!IsAutomationClean) continue;

                foreach (var pool in pools.Values)
                {
                    if (Time.time - pool.lastTime >= MAXRELEASEPOOL_TIMER)              
                        releases.Add(pool.type);                   
                }

                if (releases.Count > 0)
                {
                    CleanType();
                }
            }
        }

        /// <summary>
        /// 清空指定类型的池子
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool RemovePools<T>()
        {
            return RemovePools(typeof(T));
        }

        /// <summary>
        /// 清空指定类型的池子
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool RemovePools(Type type)
        {
            return pools.Remove(type);
        }

        /// <summary>
        /// 清空所有依赖指定类型的池子
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static bool RemoveDependPools<T>()
        {
            return RemoveDependPools(typeof(T));
        }

        /// <summary>
        /// 清空所有依赖指定类型的池子
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool RemoveDependPools(Type type)
        {
            //如果跟自动冲突了，先打掉自动部分，确保清空正确有序
            releases.Clear();

            foreach (var item in pools.Values)
            {
                //如果池子的类型与指定类型没有联系直接跳过遍历
                if (!type.IsAssignableFrom(item.type)) continue;               
                releases.Add(item.type);
            }

            if (releases.Count == 0) return false;

            CleanType();
            
            return true;
        }

        /// <summary>
        /// 获取池子当前的活跃信息
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static PoolInfo GetPoolInfo<T>()
        {
            return GetPoolInfo(typeof(T));
        }

        /// <summary>
        /// 获取池子当前的活跃信息
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static PoolInfo GetPoolInfo(Type type)
        {
            if (pools.TryGetValue(type, out var pool))
            {
                PoolInfo info = new PoolInfo()
                {
                    maxCount = pool.MaxSize,
                    recycleCount = pool.Count,
                    type = pool.type,
                    isNoPublic = pool.IsNoPublic,
                    lastTime = pool.lastTime,
                    generatorType = (pool.generator == null) ? null : pool.generator.GetType()
                };
                return info;
            }
            return default;
        }

        /// <summary>
        /// 清空全局对象池
        /// </summary>
        public static void ClearAllPools()
        {
            releases.Clear();
            pools.Clear();
        }

        private static void CleanType()
        {
            for (int i = 0; i < releases.Count; i++)
            {
                pools.Remove(releases[i]);
            }

            releases.Clear();
        }
      
        private static Dictionary<Type, GlobalPool> pools = new Dictionary<Type, GlobalPool>();

        private static List<Type> releases = new List<Type>();
     
        public static object GlobalAllocation(Type type)
        {
            return GlobalAllocationInternal(type);
        }

        internal static object GlobalAllocationInternal(Type type)
        {
            if (!typeof(IGlobalSign).IsAssignableFrom(type))
            {
                throw new Exception("对象没有继承IGlobalSign接口，无法使用对象池");
            }
            if (!pools.TryGetValue(type, out var pool))
            {
                pool = SetGlobalPoolsBySize_Internal(200,false, type, null);
            }          
            IGlobalSign sign = pool.Get();
            return sign;
        }

        internal static IEnumerable<Type> GetAllPoolTypes()
            => pools.Keys;

        /// <summary>
        /// 设置池子的大小
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generator">池对象生成器(可以为空，则通过反射创建)</param>
        /// <param name="maxSize"></param>
        public static void SetGlobalPoolsBySize<T>(int maxSize,IPoolGenerator generator) where T : IGlobalSign
        {
            SetGlobalPoolsBySize(maxSize,typeof(T),generator);
        }

        /// <summary>
        /// 设置池子的大小
        /// </summary>
        /// <param name="maxSize"></param>
        /// <param name="type"></param>
        public static void SetGlobalPoolsBySize(int maxSize,Type type,IPoolGenerator generator)
        {
            SetGlobalPoolsBySize_Internal(maxSize,false, type, generator);
        }

        /// <summary>
        /// 设置池子的大小
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="generator">池对象生成器(可以为空，则通过反射创建)</param>
        /// <param name="maxSize"></param>
        public static void SetGlobalPoolsBySize<T>(int maxSize, bool isNoPublic = false) where T : IGlobalSign
        {
            SetGlobalPoolsBySize(maxSize, typeof(T), isNoPublic);
        }

        /// <summary>
        /// 设置池子的大小
        /// </summary>
        /// <param name="maxSize"></param>
        /// <param name="type"></param>
        public static void SetGlobalPoolsBySize(int maxSize, Type type, bool isNoPublic = false)
        {
            SetGlobalPoolsBySize_Internal(maxSize, isNoPublic, type, null);
        }

        internal static GlobalPool SetGlobalPoolsBySize_Internal(int maxSize,bool isNoPublic, Type type,IPoolGenerator generator)
        {
            if (generator != null && generator.Type != type)
            {
                throw new InvalidCastException($"池对象生成器类型并不是池类型，请检查后重试! Pool Type:{type} --- generator Type:{generator.Type}");
            }
            if (!pools.TryGetValue(type, out var pool))
            {
                pool = new GlobalPool()
                {
                    MaxSize = 200,
                    type = type,
                    lastTime = Time.time,
                    generator = generator,
                    IsNoPublic = isNoPublic
                };              
              
                if (InitializePoolCount > 0)
                {
                    if (generator != null)
                    {
                        for (int i = 0; i < (InitializePoolCount > maxSize ? maxSize : InitializePoolCount); i++)
                        {
                            pool.Release(generator.Create());
                        }
                    }
                    else
                    {
                        for (int i = 0; i < (InitializePoolCount > maxSize ? maxSize : InitializePoolCount); i++)
                        {
                            try
                            {
                                pool.Release(type.CreateInstance() as IGlobalSign);
                            }
                            catch
                            {
                                pool.Release(type.CreateInstance() as IGlobalSign);
                            }
                        }
                    }
                }
                pools.Add(type, pool);
            }
            else
            {
                pool.lastTime = Time.time;
                pool.generator = generator;
            }
            pool.MaxSize = maxSize;
            return pool;
        }       

        public static T GlobalAllocation<T>()
        {
            return (T)GlobalAllocation(typeof(T));
        } 

        public static bool GlobalRelease(IGlobalSign obj) => Release(obj);       

        internal static bool Release(IGlobalSign obj)
        {
            if (obj == null || obj.IsMarkIdle) return false;            
            Type type = obj.GetType();
            if (!pools.TryGetValue(type, out var pool))
            {
                pool = SetGlobalPoolsBySize_Internal(200,false, type,null);
            }
            return pool.Release(obj);
        }
    }
}
