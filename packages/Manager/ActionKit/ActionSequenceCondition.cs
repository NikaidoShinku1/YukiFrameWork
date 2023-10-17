///=====================================================
/// - FileName:      ActionSequenceCondition.cs
/// - NameSpace:     YukiFrameWork.Events
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是队列控制根脚本
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
using UnityEngine;

namespace YukiFrameWork.Events
{
    public interface IActionSequenceCondition : IActionNode
    {
        IActionSequenceExcute ActionSequenceExcute { get; set; }
        IActionSequenceCondition CallBack(Action callBack);

        IActionSequenceExcute Delay(float time);
        IActionSequenceExcute Condition(Func<bool> predicate);
        IActionNode Start(Action OnFinish = null);
        event Action<IActionSequenceCondition> EnquenceCondition;
        void InitSequenceCondition();      
    }

    public interface IActionSequenceExcute
    {
        IActionSequenceCondition CallBack(Action callBack);
        IActionSequenceExcute Condition(Func<bool> predicate);
        IActionSequenceCondition ActionSequenceCondition { get; set; }
    }

    public class ActionSequenceCondition : IActionSequenceCondition
    {
        private Queue<SequenceNode> sequenceQueue = new Queue<SequenceNode>();    
        public IActionSequenceExcute ActionSequenceExcute { get; set; } = new ActionSequenceExcute();

        public CancellationTokenSource CancellationToken { get; private set; } = new CancellationTokenSource();

        private float currentTime = 0;
        private Func<bool> predicate;
        private bool isFinish = false;
        public event Action<IActionSequenceCondition> EnquenceCondition;

        public ActionSequenceCondition()
        {
            InitSequenceCondition();
        }

        public void InitSequenceCondition()
        {
            currentTime = 0;
            predicate = null;
            CancellationToken = new CancellationTokenSource();
        }

        public IActionSequenceCondition CallBack(Action callBack)
        {
            CheckExcute();
            sequenceQueue.Enqueue( new SequenceNode(callBack, predicate,currentTime));
            currentTime = 0;
            return this;
        }

        private void CheckExcute()
        {
            if (ActionSequenceExcute.ActionSequenceCondition == null)
                ActionSequenceExcute.ActionSequenceCondition = this;            
        }

        /// <summary>
        /// 延迟执行回调
        /// </summary>
        /// <param name="time">时间</param>
        public IActionSequenceExcute Delay(float time)
        {
            CheckExcute();
            currentTime = time;
            return ActionSequenceExcute;
        }       

        /// <summary>
        /// 启动队列
        /// </summary>
        /// <param name="OnFinish">结束回调</param>        
        public IActionNode Start(Action OnFinish = null)
        {
            OnStart(OnFinish).Forget();
            return this;
        }

        private async UniTaskVoid OnStart(Action OnFinish)
        {
            while (sequenceQueue.Count > 0)
            {
                var sequence = sequenceQueue.Dequeue();
                if (sequence.predicate != null)
                {
                    await UniTask.WaitUntil(sequence.predicate, cancellationToken: CancellationToken.Token);
                }

                if (sequence.currentTime == 0)
                {
                    await UniTask.NextFrame();
                }
                else
                   await UniTask.Delay(TimeSpan.FromSeconds(sequence.currentTime),cancellationToken: CancellationToken.Token);             
                sequence.action?.Invoke();
            }
            isFinish = true;
            await ToUniTask();
            OnFinish?.Invoke();
            Clear();
        }

        public void Clear()
        {
            sequenceQueue.Clear();
            CancellationToken.Cancel();
            EnquenceCondition?.Invoke(this);
            EnquenceCondition = null;
            isFinish = false;
        }

        /// <summary>
        /// 队列当前回调执行条件事件判断
        /// </summary>
        /// <param name="predicate">事件</param>
        public IActionSequenceExcute Condition(Func<bool> predicate)
        {
            CheckExcute();
            this.predicate = predicate;
            return ActionSequenceExcute;
        }

        public void AddTo<T>(T mono, Action cancelCallBack = null) where T : Component
        {
            _ = ToAddTo(mono, cancelCallBack);
        }

        private async UniTaskVoid ToAddTo<T>(T mono, Action cancelCallBack = null) where T : Component
        {
            await mono.gameObject.OnDestroyAsync();
            Clear();           
            cancelCallBack?.Invoke();
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

    public class SequenceNode
    {
        public float currentTime { get; }
        public Action action { get; }
        public Func<bool> predicate { get; }

        public SequenceNode(Action action, Func<bool> predicate,float currentTime)
        {
            this.action = action;
            this.predicate = predicate;
            this.currentTime = currentTime;
        }
    }

    public class ActionSequenceExcute : IActionSequenceExcute
    {
        public IActionSequenceCondition ActionSequenceCondition { get; set; }
        public IActionSequenceCondition CallBack(Action callBack)
        {
            if (ActionSequenceCondition.ActionSequenceExcute == null)
                ActionSequenceCondition.ActionSequenceExcute = this;
            ActionSequenceCondition.CallBack(callBack);
            return ActionSequenceCondition;
        }

        public IActionSequenceExcute Condition(Func<bool> predicate)
        {
            if (ActionSequenceCondition.ActionSequenceExcute == null)
                ActionSequenceCondition.ActionSequenceExcute = this;
            ActionSequenceCondition.Condition(predicate);
            return this;
        }
    }
}