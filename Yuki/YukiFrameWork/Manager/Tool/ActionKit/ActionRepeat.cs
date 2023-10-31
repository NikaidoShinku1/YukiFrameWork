///=====================================================
/// - FileName:      ActionRepeat.cs
/// - NameSpace:     YukiFrameWork.Events
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是动作循环脚本
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using System.Collections;

namespace YukiFrameWork.Events
{
    public interface IActionRepeat : IActionNode
    {
        public int RepaintCount { get; }
        public bool IsFinish { get; }
        event Action<IActionRepeat> EnquenceRepaint;
        void InitRepaint(int repaintCount);
        void InitRepaint(Func<bool> condition);
        void InitRepaint(int count, Func<bool> condition);
        IActionNode Delay(float time, Action callBack);
        IActionNode StartTimer(float maxTime, Action<float> callBack, bool isConstraint = false, Action OnFinish = null);
        IActionNode ExcuteFrame(Func<bool> predicate, Action OnFinish = null);
    }
    public class ActionRepeat : IActionRepeat
    {
        public int RepaintCount { get; private set; }
        public bool IsLoop { get; private set; } = false;
        public bool IsEnter { get; private set; }
        public bool IsFinish { get; private set; } = false;

        private Func<bool> Condition;
        public ActionRepeat(int count)
        {
            InitRepaint(count);
        }

        public ActionRepeat(Func<bool> condition)
        {
            InitRepaint(condition);
        }

        public ActionRepeat(int count, Func<bool> condition)
        {
            InitRepaint(count, condition);
        }

        public event Action<IActionRepeat> EnquenceRepaint;

        public void InitRepaint(int count)
        {
            if (IsCompleted)
                IsCompleted = false;
            else CancellationToken.Cancel();
            CancellationToken = new CancellationTokenSource();
            RepaintCount = count;
            if (RepaintCount == -1) IsLoop = true;
        }

        public void InitRepaint(Func<bool> condition)
        {
            if (IsCompleted)
                IsCompleted = false;
            else CancellationToken.Cancel();
            CancellationToken = new CancellationTokenSource();
            this.Condition = condition;
            IsLoop = true;
        }

        public void InitRepaint(int count, Func<bool> condition)
        {
            if (IsCompleted)
                IsCompleted = false;
            else CancellationToken.Cancel();
            CancellationToken = new CancellationTokenSource();
            RepaintCount = count;
            if (RepaintCount == -1) IsLoop = true;
            this.Condition = condition;
        }

        public CancellationTokenSource CancellationToken { get; private set; } = new CancellationTokenSource();

        public bool IsCompleted { get; private set; }

        public IActionNode AddTo<T>(T component, Action cancelCallBack = null) where T : Component
        {
            OnAddTo(component, cancelCallBack).Forget();
            return this;
        }

        private async UniTaskVoid OnAddTo<T>(T component, Action cancelCallBack = null) where T : Component
        {
            await component.OnDestroyAsync();
            cancelCallBack?.Invoke();            
            Clear();
        }

        public IActionNode Delay(float time, Action callBack)
        {
            OnDelay(time, callBack).Forget();
            return this;
        }

        private async UniTask OnDelay(float time, Action callBack)
        {
            Debug.Log(IsCompleted);
            Debug.Log(RepaintCount);
            Debug.Log(!CancellationToken.IsCancellationRequested);
            if (IsCompleted) return;
            while (IsLoop ? IsLoop : RepaintCount > 0 && !CancellationToken.IsCancellationRequested)
            {               
                if (Condition?.Invoke() == false)
                {
                    IsFinish = true;
                    break;
                }
                await UniTask.Delay(TimeSpan.FromSeconds(time));
                callBack?.Invoke();
                if (!IsLoop) RepaintCount--;               
            }
            IsEnter = true;
            await ToUniTask();
            IsCompleted = true;
            Clear();
        }

        public IActionNode StartTimer(float maxTime, Action<float> callBack, bool isConstraint = false, Action OnFinish = null)
        {
            OnStartTimer(maxTime, callBack, isConstraint, OnFinish).Forget();
            return this;
        }

        private async UniTask OnStartTimer(float maxTime, Action<float> callBack, bool isConstraint = false, Action OnFinish = null)
        {
            if (IsCompleted) return;
            while (IsLoop ? IsLoop : RepaintCount > 0 && !CancellationToken.IsCancellationRequested)
            {
                if (Condition?.Invoke() == false)
                {
                    IsFinish = true;
                    break;
                }
                await ActionKit.StartTimer(maxTime, callBack, isConstraint, OnFinish).ToUniTask();
                if (!IsLoop) RepaintCount--;             
            }
            IsEnter = true;
            await ToUniTask();
            IsCompleted = true;
            Clear();
        }

        public IActionNode ExcuteFrame(Func<bool> predicate, Action OnFinish = null)
        {
            OnExcuteFrame(predicate, OnFinish).Forget();
            return this;
        }

        private async UniTask OnExcuteFrame(Func<bool> predicate, Action onFinish)
        {
            if (IsCompleted) return;
            while (IsLoop ? IsLoop : RepaintCount > 0 && !IsFinish && !CancellationToken.IsCancellationRequested)
            {
                if (Condition?.Invoke() == false)
                {
                    IsFinish = true;
                    break;
                }
                await ActionKit.ExcuteFrame(predicate, onFinish).ToUniTask();
                if (!IsLoop) RepaintCount--;              
            }
            IsEnter = true;
            await ToUniTask();
            IsCompleted = true;
            Clear();
        }      

        public void Clear()
        {
            IsLoop = false;
            IsEnter = false;
            RepaintCount = -1;
            EnquenceRepaint?.Invoke(this);
            CancellationToken.Cancel();
        }

        public async UniTask<IActionNode> ToUniTask()
        {
            await UniTask.WaitUntil(() => IsEnter);
            return this;
        }

        public IEnumerator ToCoroutine()
        {
            yield return ToUniTask().ToCoroutine();
        }
    }
}