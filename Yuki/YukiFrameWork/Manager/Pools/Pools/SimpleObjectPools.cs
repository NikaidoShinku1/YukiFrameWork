///=====================================================
/// - FileName:      SimpleObjectPools.cs
/// - NameSpace:     YukiFrameWork.Pools
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   对象池
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================


using System;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork.Pools
{  
    public class SimpleObjectPools<T> : AbstarctPools<T>
    {     
        private Action<T> recycleMethod;

        public SimpleObjectPools(Func<T> resetMethod, Action<T> recycleMethod,int initSize,int maxSize = 200)
        {
            InitPools(resetMethod, recycleMethod, initSize,maxSize);
        }

        public SimpleObjectPools(Func<T> resetMethod, int initSize,int maxSize = 200)
        {
            InitPools(resetMethod, null, initSize,maxSize);
        }     

        private void InitPools(Func<T> resetMethod, Action<T> recycleMethod, int initSize,int maxSize = 200)
        {
            this.recycleMethod = recycleMethod;
            this.maxSize = maxSize;
            fectoryPool = new CustomSimpleObjectPools<T>(resetMethod);

            for (int i = 0; i < initSize; i++)
            {
                var obj = fectoryPool.Create();
                Release(obj);
            }
        }

        public override bool Release(T obj)
        {
            recycleMethod?.Invoke(obj);

            if(tQueue.Count < maxSize)
                tQueue.Enqueue(obj);

            return true;
        }       
    }

    public class CustomSimpleObjectPools<T> : IFectoryPool<T>
    {
        private readonly Func<T> resetMethod;

        public CustomSimpleObjectPools(Func<T> resetMethod)
        {
            this.resetMethod = resetMethod;
        }

        public T Create()
            => resetMethod.Invoke();
    }
}
