///=====================================================
/// - FileName:      YieldExtension.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   框架异步拓展
/// -  (C) Copyright 2008 - 2024,Yuki
/// -  All Rights Reserved.
///======================================================

using System;
using System.Collections;
using UnityEngine;
using YukiFrameWork.Pools;
namespace YukiFrameWork
{
    public static class YieldExtension
    {
        /// <summary>
        /// 启动协程
        /// </summary>
        /// <param name="enumerator">迭代器</param>
        /// <param name="callBack">回调(在迭代器结束运行时启动)</param>
        /// <returns></returns>
        public static IYieldExtension Start(this IEnumerator enumerator, Action callBack = null)
            => YieldExtensionCore.Get(enumerator, callBack);      

        /// <summary>
        /// 等待当前拓展协程结束，执行回调
        /// </summary>
        /// <param name="core">拓展异步本体</param>
        /// <param name="callBack">回调</param>
        public static IYieldExtension Request(this IYieldExtension core, Action callBack)        
            =>((YieldExtensionCore)core).ExecuteAsync(callBack);
        

        public static IYieldExtension CancelWaitGameObjectDestroy<TComponent>(this IYieldExtension core, TComponent component) where TComponent : Component
        {
            ((YieldExtensionCore)core).CancelWaitGameObjectDestroy(component);
            return core;
        }  
    }

    /// <summary>
    /// 拓展协程本体
    /// </summary>
    public interface IYieldExtension
    {
        object Current { get; set; }
        bool IsPause { get;}
        bool IsRunning { get; }    
        CustomYieldInstruction Request { get; }
        void OnPause();
        void OnResume();
        void Cancel();
    }

    public class YieldExtensionCore : IYieldExtension
    {
        private IEnumerator enumerator;
        private Action callBack;
        
        private static readonly SimpleObjectPools<YieldExtensionCore> simpleObjectPools
            = new SimpleObjectPools<YieldExtensionCore>(() => new YieldExtensionCore(), null, 10);
  
        public object Current { get; set; }
        public bool IsPause { get;private set; }
        public bool IsRunning { get; private set; }

        private bool isRelease = true;
        public static YieldExtensionCore Get(IEnumerator enumerator, Action callBack = null)
        {
            var core = simpleObjectPools.Get();
            core.Init(enumerator, callBack);
            return core;
        }
        public YieldExtensionCore() { }
        public YieldExtensionCore(IEnumerator enumerator, Action callBack = null)
        {
            Init(enumerator, callBack);
        }

        private WaitUntil WaitUntil;

        public CustomYieldInstruction Request => WaitUntil;

        public void Init(IEnumerator enumerator, Action callBack)
        {
            this.enumerator = enumerator;
            this.callBack = callBack;
            IsRunning = true;
            isRelease = false;
            WaitUntil = new WaitUntil(() => !IsRunning);
            MonoHelper.Start(ExecuteAsync());       
        }
        private IEnumerator ExecuteAsync()
        {
            while (IsRunning)
            {              
                if (IsPause)
                    yield return null;
                else
                {
                    if (enumerator?.MoveNext() == true)
                    {
                        Current = enumerator.Current;
                        yield return enumerator.Current;
                    }
                    else
                    {
                        IsRunning = false;
                        Cancel();
                    }

                }
            }                          
        }

        public IYieldExtension ExecuteAsync(Action callBack = null)
        {
             return Execution(callBack).Start();
        }

        /// <summary>
        /// 等待当前拓展协程(异步)执行完毕,如果该执行没有启动或已经完成则直接跳过
        /// </summary>
        /// <returns></returns>
        private IEnumerator Execution(Action callBack = null)
        {
            yield return Request;
            callBack?.Invoke();
        }

        public void Cancel()
        {
            if (isRelease) return;
            
            IsRunning = false;
            callBack?.Invoke();           
            simpleObjectPools.Release(this);
            isRelease = true;
        }

        public void CancelWaitGameObjectDestroy<TComponent>(TComponent component) where TComponent : Component
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
}