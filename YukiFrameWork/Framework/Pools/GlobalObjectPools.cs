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
namespace YukiFrameWork.Pools
{
    public interface IGlobalSign
    {
        bool IsMarkIdle { get; set; }
        void Init();
        void Release();
    }
  
    public class GlobalObjectPools<T> : AbstarctPools<T>,ISingletonKit,IDisposable where T : IGlobalSign, new()
    {
        public static GlobalObjectPools<T> Instance => SingletonProperty<GlobalObjectPools<T>>.GetInstance();

        internal GlobalObjectPools()
        {
            fectoryPool = new GlobalFectory<T>();
        }

        public static T GlobalAllocation()
        {
            var obj = Instance.Get();
            obj.IsMarkIdle = false;
            obj.Init();
            //LogKit.I("取出成功，全局对象池 Type:" + typeof(T) + "   当前对象池容量:" + Instance.cacheQueue.Count);
            return obj;
        }

        public static void Init(int initSize,int maxSize)
        {
            Instance.OnInit(initSize,maxSize);
        }

        public static bool GlobalRelease(T obj) => Instance.Release(obj);    

        public void OnInit(int initSize, int maxSize)
        {
            MaxSize = maxSize;

            if (maxSize > 0)
            {
                initSize = Math.Min(maxSize, initSize);
            }

            if (Count < initSize)
            {
                for (var i = Count; i < initSize; ++i)
                {
                    Release(new T());
                }
            }
        }

        public int MaxSize 
        { 
            get => maxSize;
            set
            {
                maxSize = value;

                if (cacheQueue != null)
                {
                    if (maxSize > 0)
                    {
                        if (maxSize < cacheQueue.Count)
                        {
                            int removeCount = cacheQueue.Count - maxSize;
                            while (removeCount > 0)
                            {
                                cacheQueue.Dequeue();
                                --removeCount;
                            }
                        }
                    }
                }
            }
        }

        ~GlobalObjectPools()
        {
            Dispose();
        }
        public void Dispose()
        {
            OnDestroy();
        }

        public void OnDestroy()
        {
            SingletonFectory.ReleaseInstance<GlobalObjectPools<T>>();
        }

        void ISingletonKit.OnInit()
        {
            OnInit(10, 1000);
        }

        public override bool Release(T obj)
        {
            if (obj == null && obj.IsMarkIdle) return false;
            obj.IsMarkIdle = true;
            obj.Release();
            if (cacheQueue.Count < maxSize)
            {
               
                cacheQueue.Enqueue(obj);              
                return true;
            }
            return false;
        }
    }

    public class GlobalFectory<T> : IFectoryPool<T> where T : new()
    {
        public T Create()
        {
            return new T();
        }
    }

    public static class GlobalPoolsExtension
    {
        public static bool GlobalRelease<T>(this T sign) where T : IGlobalSign,new()
            => GlobalObjectPools<T>.GlobalRelease(sign);      
    }

    /// <summary>
    /// 全局无泛型对象池，无大小限制，根据类型取出对象
    /// </summary>
    public class GlobalObjectPools : Singleton<GlobalObjectPools>, IPools<IGlobalSign>, IDisposable
    {
        private Dictionary<Type, Queue<IGlobalSign>> pools = new Dictionary<Type, Queue<IGlobalSign>>();

        public static object GlobalAllocation(Type type)
        {
            if (!Instance.pools.TryGetValue(type, out var pools))
            {
                pools = new Queue<IGlobalSign>();
                Instance.pools.Add(type, pools);
            }
            IGlobalSign sign = null;
            sign = pools.Count > 0 ? pools.Dequeue() : Activator.CreateInstance(type) as IGlobalSign;
            sign.IsMarkIdle = false;
            sign.Init();
            return sign;
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

        public static T GlobalAllocation<T>()
        {
            return (T)GlobalAllocation(typeof(T));
        }

        public static bool GlobalRelease(IGlobalSign obj) => Instance.Release(obj);

        void IPools<IGlobalSign>.Clear(Action<IGlobalSign> clearMethod)
        {
            
        }

        IGlobalSign IPools<IGlobalSign>.Get()
        {
            throw new Exception("无泛型全局对象池应该使用静态的GlobalAllocation进行物品取出");
        }

        public bool Release(IGlobalSign obj)
        {
            if (obj == null && obj.IsMarkIdle) return false;
            obj.IsMarkIdle = true;
            obj.Release();
            Type type = obj.GetType();
            if (!Instance.pools.TryGetValue(type, out var pools))
            {
                pools = new Queue<IGlobalSign>();
                Instance.pools.Add(type, pools);
            }
            pools.Enqueue(obj);
            return false;
        }
    }
}
