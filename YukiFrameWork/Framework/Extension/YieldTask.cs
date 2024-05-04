///=====================================================
/// - FileName:      YieldTask.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/4 1:50:17
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using UnityEngine;
using YukiFrameWork.Extension;
using UnityEngine.Networking;   
namespace YukiFrameWork
{

    public interface ICoroutineCompletion : ICriticalNotifyCompletion
    {
        public Coroutine Coroutine { get;internal set; }
    }
    /// <summary>
    /// 协程专属等待器，返回值功能仅限2021以上版本使用
    /// </summary>
    [GUIDancePath("YukiFramework/YieldTask")]
    [ClassAPI("协程专属等待器")]
#if UNITY_2021_1_OR_NEWER
    [AsyncMethodBuilder(typeof(YieldBuilder))]
#endif
    public class YieldTask : ICoroutineCompletion
    {
        private bool _isDone;
        private System.Exception exception;
        private Action _continuation;
        public bool IsCompleted => _isDone;
        Coroutine ICoroutineCompletion.Coroutine { get; set; }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            ((ICriticalNotifyCompletion)this).UnsafeOnCompleted(continuation);
        }
        public void Complete(Exception e)
        {
            this.Assert(!_isDone);

            _isDone = true;
            exception = e;

            if (_continuation != null)
            {
                YieldTaskExtension.RunOnUnityScheduler(_continuation);
            }
        }
        void ICriticalNotifyCompletion.UnsafeOnCompleted(Action continuation)
        {
            this.Assert(_continuation == null);
            this.Assert(!_isDone);

            _continuation = continuation;
        }  

        public void SetException(Exception e)
        {           
            _isDone = true;
            exception = e;                     
        }

        public void SetResult()
        {           
            Complete(null);
        }

        public YieldTask GetAwaiter() => this;

        public void GetResult() 
        {
            this.Assert(_isDone);

            if (exception != null)
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }
        } 
    }

    /// <summary>
    /// 有返回值的协程等待器,返回值功能仅限2021以上版本使用
    /// </summary>
    /// <typeparam name="T"></typeparam>
#if UNITY_2021_1_OR_NEWER
    [AsyncMethodBuilder(typeof(YieldBuilder))]
#endif
    public class YieldTask<T> : INotifyCompletion,ICoroutineCompletion
    {
        private bool _isDone;
        private System.Exception exception;
        private Action _continuation;
        private T result;
        public bool IsCompleted => _isDone;
        public T Result => result;
        Coroutine ICoroutineCompletion.Coroutine { get; set; }
        public T GetResult()
        {
            this.Assert(_isDone);
            if (exception != null)
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }
            return result;
        }
        void ICriticalNotifyCompletion.UnsafeOnCompleted(Action continuation)
        {
            this.Assert(_continuation == null);
            this.Assert(!_isDone);

            _continuation = continuation;
        }
        void INotifyCompletion.OnCompleted(Action continuation)
        {
            ((ICriticalNotifyCompletion)this).UnsafeOnCompleted(continuation);
        }

        public void Complete(Exception e,T result)
        {
            this.Assert(!_isDone);
        
            _isDone = true;
            exception = e;
            this.result = result;

            if (_continuation != null)
            {
                YieldTaskExtension.RunOnUnityScheduler(_continuation);
            }
        }

        public YieldTask<T> GetAwaiter()
            => this;

        public void SetException(Exception e,T t)
        {
            _isDone = true;
            exception = e;
            this.result = t;
        }

        public void SetResult(T t)
        {
            Complete(null,t);
        }
    }

    public static class YieldTaskExtension
    {      
        public static void Assert(this INotifyCompletion notifyCompletion,bool condition)
        {
            if (!condition)
            {
                throw new Exception("Assert hit in UnityAsync package!");
            }
        }

        /// <summary>
        /// 涉及多线程时的同步模型
        /// </summary>
        /// <param name="action"></param>
        public static void RunOnUnityScheduler(Action action)
        {
            if (SynchronizationContext.Current == SyncContext.UnitySynchronizationContext)
            {
                action();
            }
            else
            {
                SyncContext.UnitySynchronizationContext.Post(_ => action(), null);
            }
        }

        public static YieldTask GetAwaiter<T>(this T instruction) where T : YieldInstruction
        {
            return GetAwaiterReturnVoid(instruction);
        }  

        public static YieldTask GetAwaiter(this IYieldExtension instruction)
        {
            return GetAwaiterReturnVoid(instruction.Request);
        }

        public static YieldTask GetAwaiter(this WaitForSecondsRealtime instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        public static YieldTask GetAwaiter(this WaitUntil instruction)
        {            
            return GetAwaiterReturnVoid(instruction);
        }

        public static YieldTask GetAwaiter(this WaitWhile instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }
        
        public static YieldTask<AsyncOperation> GetAwaiter(this AsyncOperation instruction)
        {
            return GetAwaiterReturnSelf(instruction);
        }

        public static YieldTask<UnityEngine.Object> GetAwaiter(this ResourceRequest instruction)
        {
            return GetAwaiterReturnByResourcesRequest(instruction);
        }

        [Obsolete]
        public static YieldTask<WWW> GetAwaiter(this WWW instruction)
        {
            return GetAwaiterReturnSelf(instruction);
        }

        public static YieldTask<UnityWebRequest> GetAwaiter(this UnityWebRequest instruction)
        {
            return GetAwaiterReturnSelf(instruction);
        }

        public static YieldTask<AssetBundle> GetAwaiter(this AssetBundleCreateRequest instruction)
        {
            return GetAwaiterReturnByAssetBundleCreateRequest(instruction);
        }

        public static YieldTask<UnityEngine.Object> GetAwaiter(this AssetBundleRequest instruction)
        {
            return GetAwaiterReturnByAssetBundleRequest(instruction);
        }

        public static YieldTask<T> GetAwaiter<T>(this IEnumerator<T> coroutine)
        {
            var awaiter = new YieldTask<T>();
            RunOnUnityScheduler(() => ((ICoroutineCompletion)awaiter).Coroutine = MonoHelper.Start(new CoroutineRunner<T>(coroutine, awaiter).Run()));
            return awaiter;
        }

        public static YieldTask<T> GetAwaiter<K,T>(this K coroutine) where K : IEnumerator
        {
            var awaiter = new YieldTask<T>();
            RunOnUnityScheduler(() => ((ICoroutineCompletion)awaiter).Coroutine = MonoHelper.Start(new CoroutineRunner<T>(coroutine, awaiter).Run()));
            return awaiter;
        }

        public static YieldTask GetAwaiter(this IEnumerator coroutine)
        {
            return GetAwaiterReturnVoid(coroutine);
        }

        static YieldTask GetAwaiterReturnVoid(object instruction)
        {
            var awaiter = new YieldTask();
            RunOnUnityScheduler(() => ((ICoroutineCompletion)awaiter).Coroutine = MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return instruction;
                awaiter.Complete(null);
            }
            return awaiter;
        }

        static YieldTask<T> GetAwaiterReturnSelf<T>(T instruction)
        {
            var awaiter = new YieldTask<T>();
            RunOnUnityScheduler(() => ((ICoroutineCompletion)awaiter).Coroutine = MonoHelper.Start(NextVoid()));          
            IEnumerator NextVoid()
            {
                yield return instruction;
                awaiter.Complete(null,instruction);
            }
            return awaiter;
        }

        static YieldTask<UnityEngine.Object> GetAwaiterReturnByAssetBundleRequest(AssetBundleRequest instruction)
        {
            var awaiter = new YieldTask<UnityEngine.Object>();
            RunOnUnityScheduler(() => ((ICoroutineCompletion)awaiter).Coroutine = MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return instruction;
                awaiter.Complete(null, instruction.asset);
            }
            return awaiter;
        }

        static YieldTask<UnityEngine.Object> GetAwaiterReturnByResourcesRequest(ResourceRequest instruction)
        {
            var awaiter = new YieldTask<UnityEngine.Object>();
            RunOnUnityScheduler(() => ((ICoroutineCompletion)awaiter).Coroutine = MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return instruction;
                awaiter.Complete(null, instruction.asset);
            }
            return awaiter;
        }

        static YieldTask<AssetBundle> GetAwaiterReturnByAssetBundleCreateRequest(AssetBundleCreateRequest instruction)
        {
            var awaiter = new YieldTask<AssetBundle>();
            RunOnUnityScheduler(() => ((ICoroutineCompletion)awaiter).Coroutine = MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return instruction;
                awaiter.Complete(null, instruction.assetBundle);
            }
            return awaiter;
        }

        public static void SetRunOnUnityScheduler(ICoroutineCompletion completion, Coroutine coroutine)
        {
            RunOnUnityScheduler(() => completion.Coroutine = coroutine);
        }
    }
}
