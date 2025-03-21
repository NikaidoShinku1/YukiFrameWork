///=====================================================
/// - FileName:      PoolsKit.cs
/// - NameSpace:     YukiFrameWork.Pools
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的对象池管理套件
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.Pools
{
    public interface IPools
    {
        
    }
    public interface IPools<T> : IPools
    {
        T Get();

        abstract bool Release(T obj);

        void Clear(Action<T> clearMethod);
    }

    /// <summary>
    /// 对象工厂
    /// </summary>
    public interface IFectoryPool<T>
    {
        T Create();
    }

    /// <summary>
    /// 对象池基类
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    public abstract class AbstarctPools<T> : IPools<T>
    {
        protected readonly Queue<T> cacheQueue = new Queue<T>();

        protected int maxSize = 10;

        public int Max => maxSize;

        protected IFectoryPool<T> fectoryPool;

        protected Action<T> recycleMethod;

        public void SetFectoryPool(IFectoryPool<T> fectoryPool)
            => this.fectoryPool = fectoryPool;

        public void SetFectoryPool(Func<T> resetMethod)
            => fectoryPool = new CustomSimpleObjectPools<T>(resetMethod);

        public virtual T Get()
            => cacheQueue.Count == 0 ? fectoryPool.Create() : cacheQueue.Dequeue();

        public bool Contains(T t)
            => cacheQueue.Contains(t);        

        public abstract bool Release(T obj);

        public void Clear(Action<T> clearMethod = null)
        {
            foreach (var item in cacheQueue)
            {
                clearMethod?.Invoke(item);
            }

            cacheQueue.Clear();
        }

        public int Count => cacheQueue.Count;
    }   

    public static class ListPoolsExtension
    {
        public static void Release<T>(this List<T> values)
            => ListPools<T>.ReleaseList(values);      
    }
   
    public static class DictionaryExtension
    {
        public static void Release<TKey, TValue>(this Dictionary<TKey, TValue> keyValuePairs)
            => DictionaryPools<TKey,TValue>.ReleaseDict(keyValuePairs);
    }
}