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
        public static IYieldExtension Start(this IEnumerator enumerator)
            => YieldExtensionCore.Get(enumerator);      

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
        private static readonly SimpleObjectPools<YieldExtensionCore> simpleObjectPools
            = new SimpleObjectPools<YieldExtensionCore>(() => new YieldExtensionCore(), null, 10);
  
        public object Current { get; set; }
        public bool IsPause { get;private set; }
        public bool IsRunning { get; private set; }

        private bool isRelease = true;
        public static YieldExtensionCore Get(IEnumerator enumerator)
        {
            var core = simpleObjectPools.Get();
            core.Init(enumerator);
            return core;
        }
        public YieldExtensionCore() { }
        public YieldExtensionCore(IEnumerator enumerator)
        {
            Init(enumerator);
        }

        private WaitUntil WaitUntil;

        public CustomYieldInstruction Request => WaitUntil;

        public void Init(IEnumerator enumerator)
        {
            this.enumerator = enumerator;           
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

        public IYieldExtension ExecuteAsync<T>(T callBack = null) where T : Delegate
        {
             return Execution(callBack).Start();
        }

        /// <summary>
        /// 等待当前拓展协程(异步)执行完毕,如果该执行没有启动或已经完成则直接跳过
        /// </summary>
        /// <returns></returns>
        protected virtual IEnumerator Execution<T>(T callBack = null) where T : Delegate
        {
            yield return Request;
            (callBack as Action)?.Invoke();
        }

        public void Cancel()
        {
            if (isRelease) return;
            
            IsRunning = false;                
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

    public static class CoroutineTool
    {
        struct WaitDefaultFrame : IEnumerator
        {
            public object Current => null;

            public bool MoveNext() => false;
           
            public void Reset()
            {
                
            }
        }      
        struct CustomWaitUntil : IEnumerator
        {
            private Func<bool> m_Predicate;

            public CustomWaitUntil(Func<bool> m_Predicate)
                => this.m_Predicate = m_Predicate;

            public bool keepWaiting => !m_Predicate();
            public object Current => null;

            public bool MoveNext()
            {
                return !m_Predicate();
            }

            public void Reset()
            {
                
            }
        }      
        struct CustomWaitWhile : IEnumerator
        {
            private Func<bool> m_Predicate;

            public CustomWaitWhile(Func<bool> m_Predicate)
                => this.m_Predicate = m_Predicate;

            public bool keepWaiting => !m_Predicate();
            public object Current => null;

            public bool MoveNext()
            {
                return m_Predicate();
            }

            public void Reset()
            {
                
            }
        }

        private static WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
        private static WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();

        public static WaitForFixedUpdate WaitForFixedUpdate()        
            => waitForFixedUpdate;        
        public static WaitForEndOfFrame WaitForEndOfFrame()
            => waitForEndOfFrame;

        public static IEnumerator WaitForSecondsRealtime(float time)
        {
            float timer = 0;
            while (timer < time)
            {
                timer += Time.unscaledDeltaTime;                
                yield return new WaitDefaultFrame();
            }
        }

        public static IEnumerator WaitForSeconds(float time)
        {
            float timer = 0;
            while (timer < time)
            {
                timer += Time.deltaTime;
                yield return new WaitDefaultFrame();
            }
        }

        public static IEnumerator WaitForFrames(int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                yield return WaitForFrame();
            }
        }

        public static IEnumerator WaitForFrame()
        {
            yield return new WaitDefaultFrame();
        }
       
        public static IEnumerator WaitUntil(Func<bool> m_Predicate)
        {
            yield return new CustomWaitUntil(m_Predicate);
        }

        public static IEnumerator WaitWhile(Func<bool> m_Predicate)
        {
            yield return new CustomWaitWhile(m_Predicate);
        }

    }
}