///=====================================================
/// - FileName:      AsyncExtension.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   框架异步拓展
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using YukiFrameWork.Pools;
namespace YukiFrameWork
{
    public static class AsyncExtension
    {
        /// <summary>
        /// 启动协程
        /// </summary>
        /// <param name="enumerator">迭代器</param>
        /// <param name="callBack">回调(在迭代器结束运行时启动)</param>
        /// <returns></returns>
        public static IAsyncExtensionCore Start(this IEnumerator enumerator, Action callBack = null)
            => AsyncExtensionCore.Get(enumerator, callBack);      
        
        /// <summary>
        /// 等待当前拓展协程结束
        /// </summary>
        /// <param name="core">拓展异步本体</param>
        /// <returns></returns>
        public static IEnumerator OnExecuteAsync(this IAsyncExtensionCore core)
        {
            yield return new WaitUntil(() => !core.IsRunning);
        }

        /// <summary>
        /// 等待当前拓展协程结束，执行回调
        /// </summary>
        /// <param name="core">拓展异步本体</param>
        /// <param name="callBack">回调</param>
        public static IAsyncExtensionCore OnExecuteAsync(this IAsyncExtensionCore core, Action callBack)        
            =>((AsyncExtensionCore)core).ExecuteAsync(callBack);
        

        public static IAsyncExtensionCore CancelWaitGameObjectDestory<TComponent>(this IAsyncExtensionCore core, TComponent component) where TComponent : Component
        {
            ((AsyncExtensionCore)core).CancelWaitGameObjectDestory(component);
            return core;
        }  
    }

    /// <summary>
    /// 拓展协程本体
    /// </summary>
    public interface IAsyncExtensionCore
    {
        bool IsPause { get;}
        bool IsRunning { get; }     
        void OnPause();
        void OnResume();
        void Cancel();
    }

    public class AsyncExtensionCore : IAsyncExtensionCore
    {
        private IEnumerator enumerator;
        private Action callBack;
        
        private static readonly SimpleObjectPools<AsyncExtensionCore> simpleObjectPools
            = new SimpleObjectPools<AsyncExtensionCore>(() => new AsyncExtensionCore(), null, 10);
  
        public object Current { get; set; }
        public bool IsPause { get; set; }
        public bool IsRunning { get; private set; }      

        public static AsyncExtensionCore Get(IEnumerator enumerator, Action callBack = null)
        {
            var yuki = simpleObjectPools.Get();
            yuki.Init(enumerator, callBack);
            return yuki;
        }
        public AsyncExtensionCore() { }
        public AsyncExtensionCore(IEnumerator enumerator, Action callBack = null)
        {
            Init(enumerator, callBack);
        }

        public void Init(IEnumerator enumerator, Action callBack)
        {
            this.enumerator = enumerator;
            this.callBack = callBack;
            IsRunning = true;
            AsyncExecuteCore.I.EneuqueExecute(AsyncExecute());       
        }       

        public IEnumerator AsyncExecute()
        {
            while (IsRunning)
            {              
                if (IsPause)
                    yield return null;
                else
                {
                    if (enumerator.MoveNext())
                    {
                        yield return enumerator.Current;
                    }
                    else
                    {
                        IsRunning = false;
                    }

                }
            }               
            Cancel();
        }

        public IAsyncExtensionCore ExecuteAsync(Action callBack = null)
        {
             return ToAsyncFromExecute(callBack).Start();
        }

        /// <summary>
        /// 等待当前拓展协程(异步)执行完毕,如果该执行没有启动或已经完成则直接跳过
        /// </summary>
        /// <returns></returns>
        private IEnumerator ToAsyncFromExecute(Action callBack = null)
        {
            yield return new WaitUntil(() => !IsRunning);
            callBack?.Invoke();
        }

        public void Cancel()
        {            
            IsRunning = false;
            callBack?.Invoke();
            simpleObjectPools.Release(this);            
        }

        public void CancelWaitGameObjectDestory<TComponent>(TComponent component) where TComponent : Component
        {
            if(!component.TryGetComponent<OnGameObjectTrigger>(out var trigger))
            {
                trigger = component.gameObject.AddComponent<OnGameObjectTrigger>();
            }

            trigger.PushFinishEvent(Cancel);
        }       

        public void OnPause()
        {
            IsPause = true;
        }

        public void OnResume()
        {
            IsPause = false;
        }
    }

    public class AsyncExecuteCore : SingletonMono<AsyncExecuteCore>
    {
        private object _lock = new object();

        private Queue<Action> asyncCore = new Queue<Action>();

        public void EneuqueExecute(IEnumerator enumerator)
        {
            asyncCore.Enqueue(() => StartCoroutine(enumerator));
        }

        private void Update()
        {
            lock (_lock)
            {
                if (asyncCore.Count > 0)
                {
                    asyncCore.Dequeue()?.Invoke();
                }
            }
        }


    }

}