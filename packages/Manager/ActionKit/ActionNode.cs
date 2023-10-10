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
using System.Collections.Generic;

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
       /// <param name="mono">本体</param>
       /// <param name="cancelCallBack">回调</param>
        void AddTo<T>(T mono, Action cancelCallBack = null) where T : MonoBehaviour;
        /// <summary>
        /// 持续检测
        /// </summary>
        /// <param name="imposeCount">次数</param>
        IActionNode ToUpdate(int imposeCount = -1);
        /// <summary>
        /// 清除
        /// </summary>
        void Clear();
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

        UniTask<IActionDelay> ToUniTask();
    }

    public interface IActionSequence : IActionNode
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="predicate">判断条件</param>
        void InitSequence(Func<bool> predicate = null);
        event Action<IActionSequence> SequenceEnqueue;

        /// <summary>
        /// 开始执行队列
        /// </summary>
        IActionSequence Start_Sequence();
        /// <summary>
        /// 延迟时间
        /// </summary>
        /// <param name="time">秒数</param>     
        IActionSequence Delay(float time);

        /// <summary>
        /// 注册回调
        /// </summary>
        /// <param name="callBack">回调</param>
        IActionSequence CallBack(Action callBack);

        /// <summary>
        /// 转换为UniTask，可异步等待队列
        /// </summary>
        UniTask<IActionSequence> ToUniTask();
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

        UniTask<IActionExcuteFrame> ToUniTask();
    }

    public interface IActionNextFrame
    {
        /// <summary>
        /// 初始化
        /// </summary>  
        void InitNextFrame(Action callBack,MonoUpdateType type);
        CancellationTokenSource CancellationToken { get; }
        event Action<IActionNextFrame> ActionNextEnquene;
        /// <summary>
        /// (可等待)转换成UniTask异步
        /// </summary>      
        UniTask<IActionNextFrame> ToUniTask();
    }

    /// <summary>
    /// 定时回调
    /// </summary>
    public class ActionDelay : IActionDelay
    {
        public CancellationTokenSource CancellationToken { get; private set; } = new CancellationTokenSource();
        private Action CallBack;
        private float currentTime;
        private bool isFirstTrigger;
        private bool isUpdate = false;
        public event Action<IActionDelay> DelayEnqueue;

        public ActionDelay(float currentTime, Action CallBack)
        {
            InitDelay(currentTime, CallBack);
        }

        public void AddTo<T>(T mono, Action cancelCallBack = null) where T : MonoBehaviour
        {
            _ = _AddTo(mono, cancelCallBack);
        }

        private async UniTaskVoid _AddTo<T>(T mono, Action cancelCallBack = null) where T : MonoBehaviour
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
            if (!isUpdate)
            {
                Clear();
            }
        }    

        public void InitDelay(float time, Action callBack = null)
        {           
            CancellationToken = new CancellationTokenSource();
            this.currentTime = time;
            this.CallBack = callBack;           
            _ = Delay();
        }

        public IActionNode ToUpdate(int imposeCount = -1)
        {
            isUpdate = true;
            _ = _ToUpdate(imposeCount);
            return this;
        }

        private async UniTaskVoid _ToUpdate(int imposeCount)
        {
            await UniTask.WaitUntil(() => isFirstTrigger);
            while (!CancellationToken.IsCancellationRequested)
            {
                if (imposeCount != -1) imposeCount--;

                if (imposeCount == 0)
                {
                    CancellationToken.Cancel();
                    return;
                }

                await Delay();
            }
            isUpdate = false;
            Clear();
        }

        public void Clear()
        {
            CancellationToken.Cancel();
            DelayEnqueue?.Invoke(this);
            DelayEnqueue = null;
        }

        public async UniTask<IActionDelay> ToUniTask()
        {
            await UniTask.WaitUntil(() => isFirstTrigger);
            return this;
        }
    }

    public class ActionSequene : IActionSequence
    {
        public CancellationTokenSource CancellationToken { get; private set; } = new CancellationTokenSource();
        private List<Action> sequences = new List<Action>();
        private float currentTime = 0;

        /// <summary>
        /// 判断条件
        /// </summary>
        private Func<bool> predicate;

        /// <summary>
        /// 是否是第一次执行
        /// </summary>
        private bool isFirstTrigger = false;

        /// <summary>
        /// 是否开启Update
        /// </summary>
        private bool isUpdate = false;
     
        public event Action<IActionSequence> SequenceEnqueue;

        public ActionSequene(Func<bool> predicate = null)
        {
            CancellationToken = new CancellationTokenSource();
            isUpdate = false;
            this.predicate = predicate;
        }

        public void AddTo<T>(T mono, Action cancelCallBack = null) where T : MonoBehaviour
        {
            _ = _AddTo(mono, cancelCallBack);
        }

        private async UniTaskVoid _AddTo<T>(T mono, Action cancelCallBack = null) where T : MonoBehaviour
        {
            await mono.gameObject.OnDestroyAsync();
            Clear();
            cancelCallBack?.Invoke();
        }

        /// <summary>
        /// 注册回调
        /// </summary>
        /// <param name="callBack">回调</param>       
        public IActionSequence CallBack(Action callBack)
        {
            sequences.Add(callBack);
            return this;
        }

        /// <summary>
        /// 开始执行队列
        /// </summary>        
        public IActionSequence Start_Sequence()
        {
            _ = StartSequence();
            return this;
        }

        /// <summary>
        /// (可等待)将队列转换为UniTask异步
        /// </summary>      
        public async UniTask<IActionSequence> ToUniTask()
        {
            await UniTask.WaitUntil(() => isFirstTrigger == true);
            return this;
        }

        private async UniTask StartSequence()
        {
            await UniTask.Delay(TimeSpan.FromSeconds(currentTime), cancellationToken: CancellationToken.Token);
            foreach (var quence in sequences)
            {               
                if (predicate != null)
                    await UniTask.WaitUntil(predicate, cancellationToken: CancellationToken.Token);
                else await UniTask.Yield(cancellationToken: CancellationToken.Token);
                quence?.Invoke();
            }
            if (!isFirstTrigger) isFirstTrigger = true;
            await ToUniTask();
            if (!isUpdate)
            {
                Clear();
            }
        }

        public IActionNode ToUpdate(int imposeCount = -1)
        {
            isUpdate = true;
            _ = _ToUpdate(imposeCount);
            return this;
        }

        private async UniTaskVoid _ToUpdate(int imposeCount)
        {
            if (!isFirstTrigger) await StartSequence();
            else await UniTask.WaitUntil(() => isFirstTrigger == true);
            while (!CancellationToken.IsCancellationRequested)
            {
                if (imposeCount != -1) imposeCount--;

                if (imposeCount == 0)
                {
                    CancellationToken.Cancel();
                    return;
                }

                await StartSequence();
            }
            isUpdate = false;
            Clear();
        }

        public IActionSequence Delay(float time)
        {
            this.currentTime = time;
            return this;
        }

        public void InitSequence(Func<bool> predicate = null)
        {
            CancellationToken = new CancellationTokenSource();
            this.predicate = predicate;
        }

        public void Clear()
        {
            CancellationToken.Cancel();
            sequences.Clear();
            SequenceEnqueue?.Invoke(this);
            isUpdate = false;
            SequenceEnqueue = null;
        }
    }

    public class ActionExcuteFrame : IActionExcuteFrame
    {
        private enum ExcuteType
        {
            //计时
            Timer,
            //条件
            Predicate
        }
        private ExcuteType excuteType;
        public CancellationTokenSource CancellationToken { get; private set; } = new CancellationTokenSource();
        private bool isFirstTrigger = false;
        private bool isUpdate = false;
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
        public void AddTo<T>(T mono, Action cancelCallBack = null) where T : MonoBehaviour
        {
            _ = _AddTo(mono, cancelCallBack);
        }

        private async UniTaskVoid _AddTo<T>(T mono, Action cancelCallBack = null) where T : MonoBehaviour
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
            isUpdate = false;
            ExcuteFrameEnquene?.Invoke(this);
            ExcuteFrameEnquene = null;
        }  

        public IActionNode ToUpdate(int imposeCount = -1)
        {
            isUpdate = true;
            _ = _ToUpdate(imposeCount);
            return this;
        }

        private async UniTaskVoid _ToUpdate(int imposeCount)
        {          
            await UniTask.WaitUntil(() => isFirstTrigger, cancellationToken: CancellationToken.Token);
            while (!CancellationToken.IsCancellationRequested)
            {
                if (imposeCount != -1) imposeCount--;

                if (imposeCount == 0)
                {
                    CancellationToken.Cancel();
                    return;
                }
                switch (excuteType)
                {
                    case ExcuteType.Timer:
                        await ExcuteTimer();
                        break;
                    case ExcuteType.Predicate:
                        await ExcutePredicate();
                        break;             
                }
               
            }
            isUpdate = false;
            Clear();

        }

        public void InitExcuteTimer(float maxTime, Action<float> TimeTemp, bool isConstraint = false, Action OnFinish = null)
        {
            CancellationToken = new CancellationTokenSource();
            this.maxTime = maxTime;
            this.TimeTemp = TimeTemp;
            this.isConstraint = isConstraint;
            this.OnFinish = OnFinish;
            excuteType = ExcuteType.Timer;
            _ = ExcuteTimer();
        }

        public void InitExcutePredicate(Func<bool> predicate, Action OnFinish = null)
        {
            CancellationToken = new CancellationTokenSource();
            this.predicate = predicate;
            this.OnFinish = OnFinish;
            excuteType = ExcuteType.Predicate;
            _ = ExcutePredicate();
        }

        private async UniTask ExcuteTimer()
        {
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
            if (!isUpdate) Clear();
        }

        private async UniTask ExcutePredicate()
        {
            await UniTask.WaitUntil(predicate,cancellationToken:CancellationToken.Token);          
            if (!isFirstTrigger) isFirstTrigger = true;
            OnFinish?.Invoke();
            await ToUniTask();
            if (!isUpdate) Clear();
        }

        public async UniTask<IActionExcuteFrame> ToUniTask()
        {
            await UniTask.WaitUntil(() => isFirstTrigger);
            return this;
        }
    }

    public class ActionNextFrame : IActionNextFrame
    {
        private MonoUpdateType updateType;
        private bool isFirstTrigger;
        public CancellationTokenSource CancellationToken { get; private set; } = new CancellationTokenSource();
        public event Action<IActionNextFrame> ActionNextEnquene;
        public ActionNextFrame(Action callBack,MonoUpdateType updateType)
        {
            InitNextFrame(callBack,updateType);
        }

        public void InitNextFrame(Action callBack,MonoUpdateType updateType)
        {
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
            Clear();
        }

        private void Clear()
        {
            isFirstTrigger = false;
            ActionNextEnquene?.Invoke(this);
        }

        public async UniTask<IActionNextFrame> ToUniTask()
        {
            await UniTask.WaitUntil(() => isFirstTrigger);          
            return this;
        }

    }


}