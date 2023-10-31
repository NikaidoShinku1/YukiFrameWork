///=====================================================
/// - FileName:      ActionUpdate.cs
/// - NameSpace:     YukiFrameWork.Project
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是Update管理脚本
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System.Threading;
using System.Collections;

namespace YukiFrameWork.Events
{
    public interface IActionUpdate : IActionNode
    {
        event Action UpdateConditionEnqueue;       
        IActionUpdate Register(Action<object> callBack);
        void Update(Func<bool> condition = null, Func<bool> predicate = null, bool isImposeCount = false, object data = null,Action OnError = null,Action OnFinish = null);
        void FixedUpdate(Func<bool> condition = null, Func<bool> predicate = null, bool isImposeCount = false, object data = null, Action OnError = null, Action OnFinish = null);
        void LateUpdate(Func<bool> condition = null, Func<bool> predicate = null, bool isImposeCount = false, object data = null, Action OnError = null, Action OnFinish = null);       
    }

    public class ActionUpdate : IActionUpdate
    {
        private Action<object> callBack;

        public CancellationTokenSource CancellationToken { get; private set; } = new CancellationTokenSource();

        public bool IsCompleted { get; private set; }

        private bool isFinish;

        public event Action UpdateConditionEnqueue;

        public IActionNode AddTo<T>(T mono, Action cancelCallBack = null) where T : Component
        {
            _ = ToAddTo(mono, cancelCallBack);  
            return this;
        }

        private async UniTaskVoid ToAddTo<T>(T mono, Action cancelCallBack = null) where T : Component
        {
            await mono.gameObject.OnDestroyAsync();
            Clear();
            cancelCallBack?.Invoke();
        }

        /// <summary>
        /// 回收
        /// </summary>
        public void Clear()
        {
            CancellationToken.Cancel();
            UpdateConditionEnqueue?.Invoke();
            UpdateConditionEnqueue = null;
            callBack = null;
            isFinish = false;
        }

        public IActionUpdate Register(Action<object> callBack)
        {
            this.callBack = callBack;
            IsCompleted = false;
            return this;
        }    
      
        public void Update(Func<bool> condition = null,Func<bool> predicate = null,bool isImposeCount = false,object data = null,Action OnError = null,Action OnFinish = null)
        {
            CancellationToken = new CancellationTokenSource();
            _ = ToUpdateFunction(PlayerLoopTiming.LastUpdate, condition, predicate,isImposeCount,data,OnError,OnFinish);
        }

        public void FixedUpdate(Func<bool> condition = null, Func<bool> predicate = null, bool isImposeCount = false,object data = null, Action OnError = null, Action OnFinish = null)
        {
            CancellationToken = new CancellationTokenSource();
            _ = ToUpdateFunction(PlayerLoopTiming.LastFixedUpdate, condition, predicate,isImposeCount,data, OnError, OnFinish);
        }

        public void LateUpdate(Func<bool> condition = null, Func<bool> predicate = null, bool isImposeCount = false,object data = null, Action OnError = null, Action OnFinish = null)
        {
            CancellationToken = new CancellationTokenSource();
            _ = ToUpdateFunction(PlayerLoopTiming.PostLateUpdate, condition, predicate,isImposeCount,data, OnError, OnFinish);
        }

        private async UniTaskVoid ToUpdateFunction(PlayerLoopTiming playerLoopTiming, Func<bool> condition = null, Func<bool> predicate = null, bool isImposeCount = false,object data = null, Action OnError = null, Action OnFinish = null)
        {
            if (IsCompleted) return;
            await UniTask.SwitchToMainThread();
            while (!CancellationToken.IsCancellationRequested)
            {             
                await UniTask.Yield(playerLoopTiming);
                if (condition != null)
                {
                    await UniTask.WaitUntil(condition);                    
                }

                if (predicate != null)
                {
                    if (predicate?.Invoke() == true)
                    {
                        CancellationToken.Cancel();
                    }
                }
                if (OnError != null)
                {
                    try
                    {
                        callBack?.Invoke(data);
                    }
                    catch
                    {
                        OnError?.Invoke();
                        isImposeCount = true;
                    }
                }
                else
                {
                    callBack?.Invoke(data);
                }
                if (isImposeCount) CancellationToken.Cancel();               

            }
            OnFinish?.Invoke();
            isFinish = true;
            await ToUniTask();
            IsCompleted = true;
            Clear();
        }
      
        public async UniTask<IActionNode> ToUniTask()
        {
            await UniTask.WaitUntil(() => isFinish);
            return this;
        }

        public IEnumerator ToCoroutine()
        {
            yield return ToUniTask().ToCoroutine();
        }
    }

}