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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
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

        [Obsolete("新版本框架不再需要自行调用ToSIngleTask转换异步")]
        public static YieldTask ToSingleTask(this IYieldExtension extension)
        {
            return extension.GetAwaiter();
        }

        [Obsolete("新版本框架不再需要自行调用ToSIngleTask转换异步")]
        public static YieldTask ToSingleTask(this IEnumerator enumerator)
        {
            return enumerator.GetAwaiter();
        }          

        public static IEnumerator ToCoroutine(this YieldTask task)
        {
            return CoroutineTool.WaitUntil(() =>
            {
                return task.IsCompleted;
            });
        }

        public static IEnumerator ToCoroutine<T>(this YieldTask<T> task)
        {
            return CoroutineTool.WaitUntil(() =>
            {
                return task.IsCompleted;
            });
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
        Coroutine Root { get; }
        void OnPause();
        void OnResume();
        void Cancel();
    }

    public static class SyncContext
    {
        private static bool isInstall = false;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Install()
        {
            if (isInstall) return;
            UnitySynchronizationContext = SynchronizationContext.Current;
            UnityThreadId = Thread.CurrentThread.ManagedThreadId;
            isInstall = true;
        }

        private static SynchronizationContext synchronization;
        private static int mThreadId = -1;

        public static int UnityThreadId
        {
            get
            {
                if (mThreadId == -1)
                    Install();
                return mThreadId;
            }
            private set
            {
                mThreadId = value;
            }
        }

        public static SynchronizationContext UnitySynchronizationContext
        {
            get
            {
                if (synchronization == null)
                    Install();
                return synchronization;
            }
            private set
            {
                synchronization = value;
            }
        }
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
            if (!Application.isPlaying)
            {
                throw new Exception("IYieldExtension cannot be used in Editor!");
            }
            var core = simpleObjectPools.Get();
            core.Init(enumerator);
            return core;
        }
        public YieldExtensionCore() { }
        public YieldExtensionCore(IEnumerator enumerator)
        {
            Init(enumerator);
        }

        private Action queueEvent;

        private WaitUntil WaitUntil;

        public CustomYieldInstruction Request => WaitUntil;

        public void Init(IEnumerator enumerator)
        {
            this.enumerator = enumerator;           
            IsRunning = true;
            isRelease = false;
            WaitUntil = new WaitUntil(() => !IsRunning);
            Root = MonoHelper.Start(ExecuteAsync());       
        }

        public Coroutine Root { get; private set; }

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
            queueEvent += callBack;
            return this;
        }     

        public void Cancel()
        {
            if (isRelease) return;
            queueEvent?.Invoke();
            queueEvent = null;
            IsRunning = false;                
            simpleObjectPools.Release(this);
            isRelease = true;
            Root = null;
        }

        public void CancelWaitGameObjectDestroy<TComponent>(TComponent component) where TComponent : Component
        {
            if(!component.TryGetComponent<OnGameObjectTrigger>(out var trigger))
            {
                trigger = component.gameObject.AddComponent<OnGameObjectTrigger>();
            }

            trigger.PushFinishEvent(() => 
            {
                if (isRelease) return;
                if (Root == null) return;
                MonoHelper.Stop(Root);

            });
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

            public readonly bool keepWaiting => !m_Predicate();
            public object Current => null;

            public bool MoveNext()
            {
                return keepWaiting;
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

            public bool keepWaiting => m_Predicate();
            public object Current => null;

            public bool MoveNext()
            {
                return keepWaiting;
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

        /// <summary>
        /// 绑定生命周期销毁时终止异步等待器，同时终止该异步协程后面所有的等待逻辑
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="awaitable"></param>
        /// <param name="component"></param>
        /// <returns></returns>
        [Obsolete("请在await的代码后面拓展Token方法，而不建议拓展CancelWaitGameDestroy!")]
        public static CoroutineCompletion CancelWaitGameObjectDestroy<T,CoroutineCompletion>(this CoroutineCompletion awaitable, T component) 
            where T : Component where CoroutineCompletion : ICoroutineCompletion
        {
            if (!ReferenceEquals(component,null))
            {
                if (!component.TryGetComponent<OnGameObjectTrigger>(out var trigger))
                {
                    trigger = component.gameObject.AddComponent<OnGameObjectTrigger>();
                }
                trigger.PushFinishEvent(() =>
                {
                    if (awaitable?.Coroutine != null)
                    {
                        MonoHelper.Stop(awaitable.Coroutine);                        
                    }
                    awaitable.StopAllTask();
                });
            }
            else
            {
                if (awaitable?.Coroutine != null)
                {
                    MonoHelper.Stop(awaitable.Coroutine);                   
                }
                awaitable.StopAllTask();
            }
            return awaitable;
        }
        [Obsolete("请在await的代码后面拓展Token方法，而不建议拓展CancelWaitGameDestroy!")]
        public static YieldTask CancelWaitGameObjectDestroy<T>(this IEnumerator enumerator, T component) where T : Component
            => CancelWaitGameObjectDestroy(enumerator.GetAwaiter(), component);
        [Obsolete("请在await的代码后面拓展Token方法，而不建议拓展CancelWaitGameDestroy!")]
        public static YieldTask CancelWaitGameObjectDestroy<T>(this YieldInstruction enumerator, T component) where T : Component
            => CancelWaitGameObjectDestroy(enumerator.GetAwaiter(), component);
        [Obsolete("请在await的代码后面拓展Token方法，而不建议拓展CancelWaitGameDestroy!")]
        public static YieldTask<UnityEngine.Object> CancelWaitGameObjectDestroy<T>(this ResourceRequest enumerator, T component) where T : Component
            => CancelWaitGameObjectDestroy(enumerator.GetAwaiter(), component);
        [Obsolete("请在await的代码后面拓展Token方法，而不建议拓展CancelWaitGameDestroy!")]
        public static YieldTask<UnityEngine.Object> CancelWaitGameObjectDestroy<T>(this AssetBundleRequest enumerator, T component) where T : Component
            => CancelWaitGameObjectDestroy(enumerator.GetAwaiter(), component);
        [Obsolete("请在await的代码后面拓展Token方法，而不建议拓展CancelWaitGameDestroy!")]
        public static YieldTask<UnityEngine.AssetBundle> CancelWaitGameObjectDestroy<T>(this AssetBundleCreateRequest enumerator, T component) where T : Component
           => CancelWaitGameObjectDestroy(enumerator.GetAwaiter(), component);
        [Obsolete("请在await的代码后面拓展Token方法，而不建议拓展CancelWaitGameDestroy!")]
        public static YieldTask<AsyncOperation> CancelWaitGameObjectDestroy<T>(this AsyncOperation enumerator, T component) where T : Component
           => CancelWaitGameObjectDestroy(enumerator.GetAwaiter(), component);

        public static CoroutineCompletion Token<CoroutineCompletion>(this CoroutineCompletion awaitable, CoroutineToken Token)
           where CoroutineCompletion : ICoroutineCompletion
        {
            awaitable.Token = Token;
            if (Token == null) return awaitable;
            Token.Register(() =>
            {
                if (awaitable?.Coroutine != null)
                {
                    MonoHelper.Stop(awaitable.Coroutine);
                }
                awaitable.StopAllTask();
            });

            return awaitable;
        }

        public static YieldTask Token(this IEnumerator enumerator, CoroutineToken token)
            => Token(enumerator.GetAwaiter(), token);

        public static YieldTask Token(this YieldInstruction enumerator, CoroutineToken token)
            => Token(enumerator.GetAwaiter(), token);

        public static YieldTask<UnityEngine.Object> Token(this ResourceRequest enumerator, CoroutineToken token)
            => Token(enumerator.GetAwaiter(), token);

        public static YieldTask<UnityEngine.Object> Token(this AssetBundleRequest enumerator, CoroutineToken token)
            => Token(enumerator.GetAwaiter(), token);

        public static YieldTask<UnityEngine.AssetBundle> Token(this AssetBundleCreateRequest enumerator, CoroutineToken token)
            => Token(enumerator.GetAwaiter(), token);

        public static YieldTask<AsyncOperation> Token(this AsyncOperation enumerator, CoroutineToken token)
            => Token(enumerator.GetAwaiter(), token);
    }

    public struct CoroutineRunner<T>
    {
        readonly YieldTask<T> _awaiter;
        readonly Stack<IEnumerator> _processStack;

        public CoroutineRunner(
            IEnumerator coroutine, YieldTask<T> awaiter)
        {
            _processStack = new Stack<IEnumerator>();
            _processStack.Push(coroutine);
            _awaiter = awaiter;
        }

        public IEnumerator Run()
        {
            while (true)
            {
                var topWorker = _processStack.Peek();

                bool isDone;

                try
                {
                    isDone = !topWorker.MoveNext();                 
                }
                catch (Exception e)
                {                 
                    var objectTrace = GenerateObjectTrace(_processStack);

                    if (objectTrace.Any())
                    {
                        _awaiter.Complete(
                             new Exception(
                                GenerateObjectTraceMessage(objectTrace), e), default(T));
                    }
                    else
                    {
                        _awaiter.Complete(e,default(T));
                    }

                    yield break;
                }

                if (isDone)
                {
                    _processStack.Pop();

                    if (_processStack.Count == 0)
                    {
                        _awaiter.Complete(null,(T)topWorker.Current);
                        yield break;
                    }
                }
                
                if (topWorker.Current is IEnumerator)
                {
                    _processStack.Push((IEnumerator)topWorker.Current);
                }
                else
                {                  
                    yield return topWorker.Current;
                }
            }
        }
        string GenerateObjectTraceMessage(List<Type> objTrace)
        {
            var result = new StringBuilder();

            foreach (var objType in objTrace)
            {
                if (result.Length != 0)
                {
                    result.Append(" -> ");
                }

                result.Append(objType.ToString());
            }

            result.AppendLine();
            return "Unity Coroutine Object Trace: " + result.ToString();
        }

        static List<Type> GenerateObjectTrace(IEnumerable<IEnumerator> enumerators)
        {
            var objTrace = new List<Type>();

            foreach (var enumerator in enumerators)
            {               
                var field = enumerator.GetType().GetField("$this", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

                if (field == null)
                {
                    continue;
                }

                var obj = field.GetValue(enumerator);

                if (obj == null)
                {
                    continue;
                }

                var objType = obj.GetType();

                if (!objTrace.Any() || objType != objTrace.Last())
                {
                    objTrace.Add(objType);
                }
            }

            objTrace.Reverse();
            return objTrace;
        }
    }
}