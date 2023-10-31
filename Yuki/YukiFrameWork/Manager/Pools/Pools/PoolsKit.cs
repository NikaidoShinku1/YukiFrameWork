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
    public interface IPools<T>
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
        protected readonly Queue<T> tQueue = new Queue<T>();

        protected int maxSize;

        protected IFectoryPool<T> fectoryPool;

        public void SetFectoryPool(IFectoryPool<T> fectoryPool)
            => this.fectoryPool = fectoryPool;

        public void SetFectoryPool(Func<T> resetMethod)
            => fectoryPool = new CustomSimpleObjectPools<T>(resetMethod);

        public virtual T Get()
            => tQueue.Count == 0 || tQueue.Count == maxSize ? fectoryPool.Create() : tQueue.Dequeue();

        public bool Contains(T t)
            => tQueue.Contains(t);        

        public abstract bool Release(T obj);

        public void Clear(Action<T> clearMethod = null)
        {
            foreach (var item in tQueue)
            {
                clearMethod?.Invoke(item);
            }

            tQueue.Clear();
        }

        public int Count => tQueue.Count;
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