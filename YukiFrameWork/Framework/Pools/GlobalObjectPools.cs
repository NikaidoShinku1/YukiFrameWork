///=====================================================
/// - FileName:      GlobalObjectPools.cs
/// - NameSpace:     YukiFrameWork.Pools
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/23 3:37:23
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
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

        private GlobalObjectPools()
        {
            fectoryPool = new GlobalFectory<T>();
        }

        public static T GlobalAllocation()
        {
            var obj = Instance.Get();
            obj.IsMarkIdle = false;
            obj.Init();
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
}
