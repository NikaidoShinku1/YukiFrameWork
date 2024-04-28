///=====================================================
/// - FileName:      YieldAwaitable.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/4 1:50:17
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using YukiFrameWork.Extension;
using YukiFrameWork.Pools;
namespace YukiFrameWork
{

    public interface ICoroutineCompletion : INotifyCompletion
    {
        public Coroutine Coroutine { get;internal set; }
    }
    /// <summary>
    /// 协程专属等待器
    /// </summary>
    [GUIDancePath("YukiFramework/YieldAwaitable")]
    [ClassAPI("协程专属等待器")]
    public class YieldAwaitable : ICoroutineCompletion
    {
        private bool _isDone;
        private System.Exception exception;
        private Action _continuation;
        public bool IsCompleted => _isDone;
        Coroutine ICoroutineCompletion.Coroutine { get; set; }
        
        public void Complete(Exception e)
        {
            this.Assert(!_isDone);

            _isDone = true;
            exception = e;

            if (_continuation != null)
            {
                YieldAwaitableExtension.RunOnUnityScheduler(_continuation);
            }
        }

        void INotifyCompletion.OnCompleted(Action continuation)
        {
            this.Assert(_continuation == null);
            this.Assert(!_isDone);

            _continuation = continuation;
        }

        public YieldAwaitable GetAwaiter() => this;

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
    /// 有返回值的协程等待器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class YieldAwaitable<T> : INotifyCompletion,ICoroutineCompletion
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
        void INotifyCompletion.OnCompleted(Action continuation)
        {
            this.Assert(_continuation == null);
            this.Assert(!_isDone);

            this._continuation = continuation;
        }

        public void Complete(Exception e,T result)
        {
            this.Assert(!_isDone);
        
            _isDone = true;
            exception = e;
            this.result = result;

            if (_continuation != null)
            {
                YieldAwaitableExtension.RunOnUnityScheduler(_continuation);
            }
        }

        public YieldAwaitable<T> GetAwaiter()
            => this;
    }

    public static class YieldAwaitableExtension
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

        public static YieldAwaitable GetAwaiter<T>(this T instruction) where T : YieldInstruction
        {
            return GetAwaiterReturnVoid(instruction);
        }  

        public static YieldAwaitable GetAwaiter(this IYieldExtension instruction)
        {
            return GetAwaiterReturnVoid(instruction.Request);
        }

        public static YieldAwaitable GetAwaiter(this WaitForSecondsRealtime instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }

        public static YieldAwaitable GetAwaiter(this WaitUntil instruction)
        {            
            return GetAwaiterReturnVoid(instruction);
        }

        public static YieldAwaitable GetAwaiter(this WaitWhile instruction)
        {
            return GetAwaiterReturnVoid(instruction);
        }
        
        public static YieldAwaitable<AsyncOperation> GetAwaiter(this AsyncOperation instruction)
        {
            return GetAwaiterReturnSelf(instruction);
        }

        public static YieldAwaitable<UnityEngine.Object> GetAwaiter(this ResourceRequest instruction)
        {
            return GetAwaiterReturnByResourcesRequest(instruction);
        }

        // Return itself so you can do things like (await new WWW(url)).bytes
        public static YieldAwaitable<WWW> GetAwaiter(this WWW instruction)
        {
            return GetAwaiterReturnSelf(instruction);
        }
        
        public static YieldAwaitable<AssetBundle> GetAwaiter(this AssetBundleCreateRequest instruction)
        {
            return GetAwaiterReturnByAssetBundleCreateRequest(instruction);
        }

        public static YieldAwaitable<UnityEngine.Object> GetAwaiter(this AssetBundleRequest instruction)
        {
            return GetAwaiterReturnByAssetBundleRequest(instruction);
        }

        public static YieldAwaitable<T> GetAwaiter<T>(this IEnumerator<T> coroutine)
        {
            var awaiter = new YieldAwaitable<T>();
            RunOnUnityScheduler(() => ((ICoroutineCompletion)awaiter).Coroutine = MonoHelper.Start(new CoroutineRunner<T>(coroutine, awaiter).Run()));
            return awaiter;
        }

        public static YieldAwaitable<T> GetAwaiter<K,T>(this K coroutine) where K : IEnumerator
        {
            var awaiter = new YieldAwaitable<T>();
            RunOnUnityScheduler(() => ((ICoroutineCompletion)awaiter).Coroutine = MonoHelper.Start(new CoroutineRunner<T>(coroutine, awaiter).Run()));
            return awaiter;
        }

        public static YieldAwaitable GetAwaiter(this IEnumerator coroutine)
        {
            return GetAwaiterReturnVoid(coroutine);
        }

        static YieldAwaitable GetAwaiterReturnVoid(object instruction)
        {
            var awaiter = new YieldAwaitable();
            RunOnUnityScheduler(() => ((ICoroutineCompletion)awaiter).Coroutine = MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return instruction;
                awaiter.Complete(null);
            }
            return awaiter;
        }

        static YieldAwaitable<T> GetAwaiterReturnSelf<T>(T instruction)
        {
            var awaiter = new YieldAwaitable<T>();
            RunOnUnityScheduler(() => ((ICoroutineCompletion)awaiter).Coroutine = MonoHelper.Start(NextVoid()));          
            IEnumerator NextVoid()
            {
                yield return instruction;
                awaiter.Complete(null,instruction);
            }
            return awaiter;
        }

        static YieldAwaitable<UnityEngine.Object> GetAwaiterReturnByAssetBundleRequest(AssetBundleRequest instruction)
        {
            var awaiter = new YieldAwaitable<UnityEngine.Object>();
            RunOnUnityScheduler(() => ((ICoroutineCompletion)awaiter).Coroutine = MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return instruction;
                awaiter.Complete(null, instruction.asset);
            }
            return awaiter;
        }

        static YieldAwaitable<UnityEngine.Object> GetAwaiterReturnByResourcesRequest(ResourceRequest instruction)
        {
            var awaiter = new YieldAwaitable<UnityEngine.Object>();
            RunOnUnityScheduler(() => ((ICoroutineCompletion)awaiter).Coroutine = MonoHelper.Start(NextVoid()));
            IEnumerator NextVoid()
            {
                yield return instruction;
                awaiter.Complete(null, instruction.asset);
            }
            return awaiter;
        }

        static YieldAwaitable<AssetBundle> GetAwaiterReturnByAssetBundleCreateRequest(AssetBundleCreateRequest instruction)
        {
            var awaiter = new YieldAwaitable<AssetBundle>();
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
