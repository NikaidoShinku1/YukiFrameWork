///=====================================================
/// - FileName:      YieldAwaitable.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/4 1:50:17
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using System;
using System.Runtime.CompilerServices;
using YukiFrameWork.Extension;
using YukiFrameWork.Pools;
namespace YukiFrameWork
{  
    /// <summary>
    /// 协程专属等待器
    /// </summary>
    [GUIDancePath("YukiFramework/YieldAwaitable")]
    [ClassAPI("协程专属等待器")]
    public class YieldAwaitable : INotifyCompletion
    {
        private static SimpleObjectPools<YieldAwaitable> awaitablePools
            = new SimpleObjectPools<YieldAwaitable>
            (() => new YieldAwaitable(), null, 50, 200);
        public bool IsCompleted { get; private set; }

        public IYieldExtension Extension { get; private set; }

        public event Action _continuation;
        public void OnCompleted(Action continuation)
        {         
            this._continuation = continuation;
        }

        private YieldAwaitable() { }

        public void GetResult() { }

        public YieldAwaitable GetAwaiter()
        {
            return this;
        }

        public static YieldAwaitable GetAwaitable(IYieldExtension extension)
        {
            var awaitable = awaitablePools.Get();
            awaitable.Extension = extension;
            return awaitable;
        }


        public static void OnFinish(YieldAwaitable awaitable)
        {
            awaitable.IsCompleted = true;
            awaitable._continuation?.Invoke();
            awaitable._continuation = null;
            awaitable.IsCompleted = false;
            awaitable.Extension = null;
            awaitablePools.Release(awaitable);
        }
    }
}
