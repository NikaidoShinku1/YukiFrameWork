///=====================================================
/// - FileName:      ActionNode.cs
/// - NameSpace:     YukiFrameWork.Events
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是动作时序根脚本
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
    public interface IActionNode
    {
        /// <summary>
        /// Token手动取消异步
        /// </summary>
        CancellationTokenSource CancellationToken { get; }
        /// <summary>
        /// 绑定Mono生命周期
        /// </summary>
        /// <typeparam name="T">类型</typeparam>
        /// <param name="component">本体</param>
        /// <param name="cancelCallBack">回调</param>
        IActionNode AddTo<T>(T component, Action cancelCallBack = null) where T : Component;

        bool IsCompleted { get; }

        /// <summary>
        /// 清除
        /// </summary>
        void Clear();       

        /// <summary>
        /// 可等待，转换为UniTask
        /// </summary>
        /// <returns></returns>
        UniTask<IActionNode> ToUniTask();

        IEnumerator ToCoroutine();
    }

    public interface IActionDelay : IActionNode
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="time">时间</param>
        /// <param name="callBack">回调</param>
        void InitDelay(float time, Action callBack = null);
      
        event Action<IActionDelay> DelayEnqueue;       
       
    }

    public interface IActionExcuteFrame : IActionNode
    {
        /// <summary>
        /// 初始化
        ///</summary>>
        void InitExcuteTimer(float maxTime, Action<float> TimeTemp, bool isConstraint = false, Action OnFinish = null);

        /// <summary>
        /// 初始化
        ///</summary>>
        void InitExcutePredicate(Func<bool> predicate, Action OnFinish = null);

        event Action<IActionExcuteFrame> ExcuteFrameEnquene;      
      
    }

    public interface IActionNextFrame : IActionNode
    {
        /// <summary>
        /// 初始化
        /// </summary>  
        void InitNextFrame(Action callBack,MonoUpdateType type);
       
        event Action<IActionNextFrame> ActionNextEnquene;
    }
    public interface IActionDelayFrame : IActionNode
    {
        /// <summary>
        /// 初始化
        /// </summary>  
        void InitDelayFrame(Action callBack, MonoUpdateType type,int delayFrameCount);

        event Action<IActionDelayFrame> ActionNextEnquene;
    }

    /// <summary>
    /// 定时回调
    /// </summary>
    public class ActionDelay : IActionDelay
    {
        public CancellationTokenSource CancellationToken { get; private set; } = new CancellationTokenSource();

        public bool IsCompleted { get; private set; }

        private Action CallBack;
        private float currentTime;
        private bool isFirstTrigger;     
        public event Action<IActionDelay> DelayEnqueue;

        public ActionDelay(float currentTime, Action CallBack)
        {
            Debug.Log("Init初始化");
            InitDelay(currentTime, CallBack);
        }

        public IActionNode AddTo<T>(T mono, Action cancelCallBack = null) where T : Component
        {
            _ = _AddTo(mono, cancelCallBack);
            return this;
        }

        private async UniTaskVoid _AddTo<T>(T mono, Action cancelCallBack = null) where T : Component
        {
            await mono.gameObject.OnDestroyAsync();
            Clear();
            cancelCallBack?.Invoke();
           
        }

        private async UniTask Delay()
        {            
            await UniTask.Delay(TimeSpan.FromSeconds(currentTime), cancellationToken: CancellationToken.Token);           
            CallBack?.Invoke();
            if (!isFirstTrigger) isFirstTrigger = true;
            await ToUniTask();
            IsCompleted = true;
            Clear();           
        }    

        public void InitDelay(float time, Action callBack = null)
        {
            IsCompleted = false;
            CancellationToken = new CancellationTokenSource();
            this.currentTime = time;
            this.CallBack = callBack;           
            _ = Delay();
        }      

        public void Clear()
        {           
            DelayEnqueue?.Invoke(this);
            DelayEnqueue = null;
            CancellationToken.Cancel();
        }

        public async UniTask<IActionNode> ToUniTask()
        {
            await UniTask.WaitUntil(() => isFirstTrigger);
            return this;
        }

        public IEnumerator ToCoroutine()
        {
            yield return ToUniTask().ToCoroutine();
        }
    }

    public class ActionExcuteFrame : IActionExcuteFrame
    {    
        public CancellationTokenSource CancellationToken { get; private set; } = new CancellationTokenSource();

        public bool IsCompleted { get; private set; }

        private bool isFirstTrigger = false;       
        private float maxTime;
        private Action<float> TimeTemp;
        private bool isConstraint = false;
        private Func<bool> predicate;
        private Action OnFinish;

        public event Action<IActionExcuteFrame> ExcuteFrameEnquene;

        public ActionExcuteFrame(float maxTime, Action<float> TimeTemp, bool isConstraint = false, Action OnFinish = null)
        {
            InitExcuteTimer(maxTime, TimeTemp, isConstraint, OnFinish);
        }

        public ActionExcuteFrame(Func<bool> predicate, Action OnFinish = null)
        {
            InitExcutePredicate(predicate, OnFinish);
        }

        public ActionExcuteFrame() 
        {

        }
        public IActionNode AddTo<T>(T mono, Action cancelCallBack = null) where T : Component
        {
            _ = _AddTo(mono, cancelCallBack);
            return this;
        }

        private async UniTaskVoid _AddTo<T>(T mono, Action cancelCallBack = null) where T : Component
        {
            await mono.gameObject.OnDestroyAsync();
            Clear();
            cancelCallBack?.Invoke();
        }

        public void Clear()
        {
            CancellationToken.Cancel();
            maxTime = 0;
            TimeTemp = null;
            OnFinish = null;
            predicate = null;
            isConstraint = false;
            isFirstTrigger = false;
          
            ExcuteFrameEnquene?.Invoke(this);
            ExcuteFrameEnquene = null;
        }     

        public void InitExcuteTimer(float maxTime, Action<float> TimeTemp, bool isConstraint = false, Action OnFinish = null)
        {
            if(IsCompleted)
               IsCompleted = false;
            else CancellationToken.Cancel();          
            CancellationToken = new CancellationTokenSource();
            this.maxTime = maxTime;
            this.TimeTemp = TimeTemp;
            this.isConstraint = isConstraint;
            this.OnFinish = OnFinish;           
            _ = ExcuteTimer();
        }

        public void InitExcutePredicate(Func<bool> predicate, Action OnFinish = null)
        {
            if (IsCompleted)
                IsCompleted = false;
            else CancellationToken.Cancel();
            CancellationToken = new CancellationTokenSource();
            this.predicate = predicate;
            this.OnFinish = OnFinish;
           
            _ = ExcutePredicate();
        }

        private async UniTask ExcuteTimer()
        {
            if (IsCompleted) return;         
            float time = 0;
            while (time < maxTime && !CancellationToken.IsCancellationRequested)
            {                             
                await UniTask.Yield(PlayerLoopTiming.LastUpdate);
                time += Time.deltaTime;
                if (isConstraint) TimeTemp?.Invoke(time / maxTime);
                else TimeTemp?.Invoke(time);
            }
            if (!isFirstTrigger) isFirstTrigger = true;
            OnFinish?.Invoke();
            await ToUniTask();
            IsCompleted = true;
            Clear();
        }

        private async UniTask ExcutePredicate()
        {
            if (IsCompleted) return;
            await UniTask.WaitUntil(predicate,cancellationToken:CancellationToken.Token);          
            if (!isFirstTrigger) isFirstTrigger = true;
            OnFinish?.Invoke();
            await ToUniTask();
            IsCompleted = true;
            Clear();
        }

        public async UniTask<IActionNode> ToUniTask()
        {
            await UniTask.WaitUntil(() => isFirstTrigger);
            return this;
        }

        public IEnumerator ToCoroutine()
        {
            yield return ToUniTask().ToCoroutine();
        }
    }

    public class ActionDelayFrame : IActionDelayFrame
    {
        private MonoUpdateType updateType;
        private bool isFirstTrigger;
        public CancellationTokenSource CancellationToken { get; private set; } = new CancellationTokenSource();

        public bool IsCompleted { get; private set; }

        public event Action<IActionDelayFrame> ActionNextEnquene;
        public ActionDelayFrame(Action callBack, MonoUpdateType updateType,int delayFrameCount)
        {
            InitDelayFrame(callBack, updateType,delayFrameCount);
        }

        public void InitDelayFrame(Action callBack, MonoUpdateType updateType, int delayFrameCount)
        {
            if (IsCompleted)
                IsCompleted = false;
            else CancellationToken.Cancel();         
            CancellationToken = new CancellationTokenSource();
            this.updateType = updateType;
            DelayFrame(callBack,delayFrameCount);
        }

        public IActionDelayFrame DelayFrame(Action callBack,int delayFrameCount)
        {           
            _ = _DelayFrame(callBack,delayFrameCount);
            return this;
        }

        private async UniTask _DelayFrame(Action callBack, int delayFrameCount)
        {
            if (IsCompleted) return;
            switch (updateType)
            {
                case MonoUpdateType.Update:
                    await UniTask.DelayFrame(delayFrameCount,PlayerLoopTiming.LastUpdate);
                    break;
                case MonoUpdateType.FixedUpdate:
                    await UniTask.DelayFrame(delayFrameCount,PlayerLoopTiming.FixedUpdate);
                    break;
                case MonoUpdateType.LateUpdate:
                    await UniTask.DelayFrame(delayFrameCount,PlayerLoopTiming.PostLateUpdate);
                    break;
            }
            isFirstTrigger = true;
            callBack?.Invoke();
            await ToUniTask();
            IsCompleted = true;
            Clear();
        }

        public void Clear()
        {
            isFirstTrigger = false;
            ActionNextEnquene?.Invoke(this);
        }

        public async UniTask<IActionNode> ToUniTask()
        {
            await UniTask.WaitUntil(() => isFirstTrigger);
            return this;
        }

        public IActionNode AddTo<T>(T mono, Action cancelCallBack = null) where T : Component
        {
            _ = _AddTo(mono, cancelCallBack);
            return this;
        }

        private async UniTaskVoid _AddTo<T>(T mono, Action cancelCallBack = null) where T : Component
        {
            await mono.gameObject.OnDestroyAsync();
            Clear();
            cancelCallBack?.Invoke();
        }

        public IEnumerator ToCoroutine()
        {
            yield return ToUniTask().ToCoroutine();
        }
    }

    public class ActionNextFrame : IActionNextFrame
    {
        private MonoUpdateType updateType;
        private bool isFirstTrigger;
        public CancellationTokenSource CancellationToken { get; private set; } = new CancellationTokenSource();

        public bool IsCompleted { get; private set; }

        public event Action<IActionNextFrame> ActionNextEnquene;
        public ActionNextFrame(Action callBack,MonoUpdateType updateType)
        {
            InitNextFrame(callBack,updateType);
        }

        public void InitNextFrame(Action callBack,MonoUpdateType updateType)
        {
            if (IsCompleted)
                IsCompleted = false;
            else CancellationToken.Cancel();
            CancellationToken = new CancellationTokenSource();
            this.updateType = updateType;
            NextFrame(callBack);
        }

        public IActionNextFrame NextFrame(Action callBack)
        {
            _ = _NextFrame(callBack);
            return this;
        }

        private async UniTask _NextFrame(Action callBack)
        {
            switch (updateType)
            {
                case MonoUpdateType.Update:
                    await UniTask.NextFrame(PlayerLoopTiming.LastUpdate);
                    break;
                case MonoUpdateType.FixedUpdate:
                    await UniTask.NextFrame(PlayerLoopTiming.FixedUpdate);
                    break;
                case MonoUpdateType.LateUpdate:
                    await UniTask.NextFrame(PlayerLoopTiming.PostLateUpdate);
                    break;
            }            
            isFirstTrigger = true;
            callBack?.Invoke();
            await ToUniTask();
            IsCompleted = true;
            Clear();
        }

        public void Clear()
        {
            isFirstTrigger = false;
            ActionNextEnquene?.Invoke(this);
        }

        public async UniTask<IActionNode> ToUniTask()
        {
            await UniTask.WaitUntil(() => isFirstTrigger);          
            return this;
        }

        public IActionNode AddTo<T>(T mono, Action cancelCallBack = null) where T : Component
        {
            _ = _AddTo(mono, cancelCallBack);
            return this;
        }

        private async UniTaskVoid _AddTo<T>(T mono, Action cancelCallBack = null) where T : Component
        {
            await mono.gameObject.OnDestroyAsync();
            Clear();
            cancelCallBack?.Invoke();
        }

        public IEnumerator ToCoroutine()
        {
            yield return ToUniTask().ToCoroutine();
        }
    }


}