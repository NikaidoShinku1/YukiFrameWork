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
namespace YukiFrameWork.Pools
{
    public interface IGlobalSign
    {
        bool IsMarkIdle { get; set; }
        void Init();
        void Release();
    }
    public static class GlobalPoolsExtension
    {
        public static bool GlobalRelease<T>(this T sign) where T : IGlobalSign
            => GlobalObjectPools.GlobalRelease(sign);
    }
    /// <summary>
    /// 全局对象池,根据类型取出对象,对象池拥有独立的生命周期管理，每一分钟检查一次对象池是否超过五分钟未使用，如果超过五分钟未使用会将整个池清空释放,默认对象池容量为200
    /// </summary>
    public class GlobalObjectPools : Singleton<GlobalObjectPools>, IPools<IGlobalSign>, IDisposable
    {
        internal const float RELEASEPOOL_TIMER = 60;
        internal const float MAXRELEASEPOOL_TIMER = 5 * 60;
  
        internal class GlobalPool : IEnumerable<IGlobalSign>
        {
            public Queue<IGlobalSign> pools = new Queue<IGlobalSign>();
            public Type type;
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
                var obj =  Count > 0 ? pools.Dequeue() : Activator.CreateInstance(type,IsNoPublic) as IGlobalSign;
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

        private GlobalObjectPools()
        {           
            if (!Application.isPlaying) return;
            MonoHelper.Start(CheckPools());
        }

        private IEnumerator CheckPools()
        {
            while (true)
            {
                yield return CoroutineTool.WaitForSeconds(RELEASEPOOL_TIMER);
                
                foreach (var pool in pools.Values)
                {
                    if (Time.time - pool.lastTime >= MAXRELEASEPOOL_TIMER)              
                        releases.Add(pool.type);                   
                }

                if (releases.Count > 0)
                {
                    for (int i = 0; i < releases.Count; i++)
                    {
                        pools.Remove(releases[i]);
                    }

                    releases.Clear();
                }
            }
        }
      
        private Dictionary<Type, GlobalPool> pools = new Dictionary<Type, GlobalPool>();

        private List<Type> releases = new List<Type>();

        public static object GlobalAllocation(Type type, bool noPublic = false)
        {
            if (!typeof(IGlobalSign).IsAssignableFrom(type))
            {
                throw new Exception("对象没有继承IGlobalSign接口，无法使用对象池");
            }
            if (!Instance.pools.TryGetValue(type, out var pool))
            {
                pool = SetGlobalPoolsBySize_Internal(200, type);
            }
            pool.IsNoPublic = noPublic;
            IGlobalSign sign = pool.Get();          
            return sign;
        }

        /// <summary>
        /// 设置池子的大小
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="maxSize"></param>
        public static void SetGlobalPoolsBySize<T>(int maxSize) where T : IGlobalSign
        {
            SetGlobalPoolsBySize(maxSize, typeof(T));
        }

        /// <summary>
        /// 设置池子的大小
        /// </summary>
        /// <param name="maxSize"></param>
        /// <param name="type"></param>
        public static void SetGlobalPoolsBySize(int maxSize,Type type)
        {
            SetGlobalPoolsBySize_Internal(maxSize, type);
        }

        internal static GlobalPool SetGlobalPoolsBySize_Internal(int maxSize, Type type)
        {
            if (!Instance.pools.TryGetValue(type, out var pool))
            {
                pool = new GlobalPool()
                {
                    MaxSize = 200,
                    type = type,
                    lastTime = Time.time,
                };
                Instance.pools.Add(type, pool);
            }
            else
            {
                pool.lastTime = Time.time;
            }
            pool.MaxSize = maxSize;
            return pool;
        }

        ~GlobalObjectPools()
        {
            Dispose();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();           
            pools.Clear();
        }

        public static T GlobalAllocation<T>(bool noPublic = false)
        {
            return (T)GlobalAllocation(typeof(T),noPublic);
        }

        public static bool GlobalRelease(IGlobalSign obj) => Instance.Release(obj);

        void IPools<IGlobalSign>.Clear(Action<IGlobalSign> clearMethod)
        {
            foreach (var pool in pools.Values)
            {
                foreach (var obj in pool)
                {
                    clearMethod?.Invoke(obj);
                }
            }

            pools.Clear();
        }

        IGlobalSign IPools<IGlobalSign>.Get()
        {
            throw new Exception("无泛型全局对象池应该使用静态的GlobalAllocation进行物品取出");
        }

        public bool Release(IGlobalSign obj)
        {
            if (obj == null || obj.IsMarkIdle) return false;            
            Type type = obj.GetType();
            if (!Instance.pools.TryGetValue(type, out var pool))
            {
                pool = SetGlobalPoolsBySize_Internal(200, type);
            }
            return pool.Release(obj);
        }
    }
}
