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

namespace YukiFrameWork.Events
{
    public interface IActionUpdate
    {
        event Action UpdateConditionEnqueue;       
        IActionUpdate Register(Action<object> callBack);
        void Update(Func<bool> condition = null, Func<bool> predicate = null, bool isImposeCount = false, object data = null);
        void FixedUpdate(Func<bool> condition = null, Func<bool> predicate = null, bool isImposeCount = false, object data = null);
        void LateUpdate(Func<bool> condition = null, Func<bool> predicate = null, bool isImposeCount = false, object data = null);
        void AddTo<T>(T mono, Action cancelCallBack = null) where T : Component;
    }

    public class ActionUpdate : IActionUpdate
    {
        private Action<object> callBack;     

        public CancellationTokenSource CancellationToken { get; private set; } = new CancellationTokenSource();

        public event Action UpdateConditionEnqueue;

        public void AddTo<T>(T mono, Action cancelCallBack = null) where T : Component
        {
            _ = ToAddTo(mono, cancelCallBack);  
        }

        private async UniTaskVoid ToAddTo<T>(T mono, Action cancelCallBack = null) where T : Component
        {
            await mono.gameObject.OnDestroyAsync();
            Recyle();
            cancelCallBack?.Invoke();
        }

        /// <summary>
        /// 回收
        /// </summary>
        private void Recyle()
        {
            CancellationToken.Cancel();
            UpdateConditionEnqueue?.Invoke();
            UpdateConditionEnqueue = null;
            callBack = null;
        }

        public IActionUpdate Register(Action<object> callBack)
        {
            this.callBack = callBack;                      
            return this;
        }    
      
        public void Update(Func<bool> condition = null,Func<bool> predicate = null,bool isImposeCount = false,object data = null)
        {
            CancellationToken = new CancellationTokenSource();
            _ = ToUpdateFunction(PlayerLoopTiming.LastUpdate, condition, predicate,isImposeCount,data);
        }

        public void FixedUpdate(Func<bool> condition = null, Func<bool> predicate = null, bool isImposeCount = false,object data = null)
        {
            CancellationToken = new CancellationTokenSource();
            _ = ToUpdateFunction(PlayerLoopTiming.LastFixedUpdate, condition, predicate,isImposeCount,data);
        }

        public void LateUpdate(Func<bool> condition = null, Func<bool> predicate = null, bool isImposeCount = false,object data = null)
        {
            CancellationToken = new CancellationTokenSource();
            _ = ToUpdateFunction(PlayerLoopTiming.PostLateUpdate, condition, predicate,isImposeCount,data);
        }

        private async UniTaskVoid ToUpdateFunction(PlayerLoopTiming playerLoopTiming, Func<bool> condition = null, Func<bool> predicate = null, bool isImposeCount = false,object data = null)
        {
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
                callBack?.Invoke(data);
                if (isImposeCount) CancellationToken.Cancel();               

            }

            Recyle();
        }
       
    }

}