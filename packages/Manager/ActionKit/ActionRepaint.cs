///=====================================================
/// - FileName:      ActionRepaint.cs
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
    public interface IActionRepaint : IActionNode
    {
        public int RepaintCount { get; }
        event Action<IActionRepaint> EnquenceRepaint;
        void InitRepaint(int repaintCount);
        IActionNode Delay(float time, Action callBack);
        IActionNode StartTimer(float maxTime, Action<float> callBack, bool isConstraint = false, Action OnFinish = null);
        IActionNode ExcuteFrame(Func<bool> predicate, Action OnFinish = null);
    }
    public class ActionRepaint : IActionRepaint
    {
        public int RepaintCount { get; private set; }
        public bool IsLoop { get; private set; } = false;
        public bool IsEnter { get; private set; }       
        public ActionRepaint(int count)
        {
            InitRepaint(count);
        }

        public event Action<IActionRepaint> EnquenceRepaint;

        public void InitRepaint(int count)
        {
            RepaintCount = count;
            if (RepaintCount == -1) IsLoop = true;
        }

        public CancellationTokenSource CancellationToken { get; } = new CancellationTokenSource();

        public void AddTo<T>(T component, Action cancelCallBack = null) where T : Component
        {
            OnAddTo(component, cancelCallBack).Forget();
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
            while (IsLoop ? IsLoop : RepaintCount > 0 && !CancellationToken.IsCancellationRequested)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(time));
                callBack?.Invoke();
                if (!IsLoop) RepaintCount--;
            }
            IsEnter = true;
            await ToUniTask();
            Clear();
        }

        public IActionNode StartTimer(float maxTime, Action<float> callBack, bool isConstraint = false, Action OnFinish = null)
        {
            OnStartTimer(maxTime, callBack, isConstraint, OnFinish).Forget();
            return this;
        }

        private async UniTask OnStartTimer(float maxTime, Action<float> callBack, bool isConstraint = false, Action OnFinish = null)
        {         
            while (IsLoop ? IsLoop : RepaintCount > 0 && !CancellationToken.IsCancellationRequested)
            {              
                await ActionKit.StartTimer(maxTime, callBack, isConstraint, OnFinish).ToUniTask();
                if (!IsLoop) RepaintCount--;                
            }
            IsEnter = true;
            await ToUniTask();
            Clear();
        }

        public IActionNode ExcuteFrame(Func<bool> predicate, Action OnFinish = null)
        {
            OnExcuteFrame(predicate, OnFinish).Forget();
            return this;
        }

        private async UniTask OnExcuteFrame(Func<bool> predicate, Action onFinish)
        {
            while (IsLoop ? IsLoop : RepaintCount > 0 && !CancellationToken.IsCancellationRequested)
            {
                await ActionKit.ExcuteFrame(predicate, onFinish).ToUniTask();
                if (!IsLoop) RepaintCount--;
            }
            IsEnter = true;
            await ToUniTask();
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