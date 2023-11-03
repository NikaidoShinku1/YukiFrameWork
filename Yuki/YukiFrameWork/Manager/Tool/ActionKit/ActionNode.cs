///=====================================================
/// - FileName:      ActionNode.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   Action本体
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using YukiFrameWork.Pools;

namespace YukiFrameWork
{
    public interface IActionNode
    {
        Queue<IActionNode> actions { get; }       

        bool OnExecute(float delta);

        bool IsInit { get; }

        bool IsPaused { get; set; }

        bool IsResume { get; set; }

        bool IsFinish { get; }

        IActionNode AddNode<TNode>(TNode node) where TNode : IActionNode;

        abstract void OnInit();

        abstract void OnFinish();

        abstract IEnumerator ToCoroutine();
    }

    public interface ISequence : IActionNode
    {
        ISequence AddSequence(IActionNode node);
    }

    public interface IParallel : IActionNode
    {
        IParallel AddParallel(IActionNode node);
    }

    public interface IRepeat : IActionNode
    {
        int CurrentCount { get; }
        IActionNode ActionNode { get; set; }
    }

    public interface IActionNodeController
    {
        IActionNodeController Start<T>(T component, Action callBack = null) where T : Component;
    }

    public interface IActionUpdateNodeController : IActionNodeController
    {

    }
    public enum UpdateStatus
    {
        OnUpdate = 0,
        OnFixedUpdate,
        OnLateUpdate
    }

    public interface IActionUpdateNode
    {
        UpdateStatus UpdateStatus { get; }
        bool IsFirstExecute { get; set; }      
        IActionUpdateNode Register(Action<long> OnEvent, Action OnError = null, Action OnFinish = null);
        bool OnExecute(float delta);
        void OnFinish();
    }

    public interface IActionUpdateCondition
    {
        ActionUpdateNode Action { get; }
        IActionUpdateNode Register(Action<long> onEvent, Action OnError = null, Action OnFinish = null);
        IActionUpdateCondition Where(Func<bool> condition);
        IActionUpdateCondition First(Func<bool> condition = null);
        IActionUpdateCondition TakeWhile(Func<bool> onFinishCondition);
    }

    public abstract class ActionNode : IActionNode
    {      
        public abstract bool OnExecute(float delta);

        public Queue<IActionNode> actions { get; } = new Queue<IActionNode>();

        public bool IsPaused { get; set; }

        public bool IsResume
        {
            get => !IsPaused;
            set => IsPaused = !value;
        }

        public bool IsFinish { get; protected set; }

        public bool IsInit { get; protected set; } = false;

        public abstract IEnumerator ToCoroutine();

        public abstract void OnFinish();
    
        public abstract void OnInit();

        public IActionNode AddNode<TNode>(TNode node) where TNode : IActionNode
        {
            actions.Enqueue(node);
            return this;
        }
    }

    public class ActionUpdateNode : ActionNode, IActionUpdateNode
    {
        private static SimpleObjectPools<ActionUpdateNode> simpleObjectPools
            = new SimpleObjectPools<ActionUpdateNode>(() => new ActionUpdateNode(), null, 10);

        public UpdateStatus UpdateStatus { get; private set; }

        protected long data;
        protected Action<long> onEvent;

        protected Action onError;
        protected Action onFinish;

        public Func<bool> OnFinishCondition { get; set; }

        public bool IsFirstExecute { get; set; }

        public ActionUpdateNode(UpdateStatus updateStatus)
        {
            OnReset(updateStatus);
        }

        public static ActionUpdateNode Get(UpdateStatus updateStatus)
        {
            var updateNode = simpleObjectPools.Get();
            updateNode.OnReset(updateStatus);
            return updateNode;
        }

        public ActionUpdateNode() { }

        public void OnReset(UpdateStatus updateStatus)
        {
            UpdateStatus = updateStatus;
        }

        public override bool OnExecute(float delta)
        {
            try
            {
                foreach (var action in actions)
                {
                    IsPaused = !action.OnExecute(delta);
                }

                if (IsPaused) return false;
                data++;
                onEvent?.Invoke(data);
                if (IsFirstExecute)
                {
                    onFinish?.Invoke();
                    return true;
                }
                if (OnFinishCondition != null && OnFinishCondition?.Invoke() == true)
                {
                    onFinish?.Invoke();
                    return true;
                }
                return false;
            }
            catch
            {
                if (onError != null)
                {
                    onError?.Invoke();
                    return true;
                }
                else
                {
                    throw new Exception("当前Update内涵错误代码！The Update is OnError!");
                }
            }
        }

        public override void OnFinish()
        {
            IsInit = false;
           
            
            IsFinish = true;
            data = 0;
            onEvent = null;
            IsPaused = false;
            simpleObjectPools.Release(this);
        }

        public IActionUpdateNode Register(Action<long> onEvent, Action OnError = null, Action OnFinish = null)
        {
            this.onEvent = onEvent;
            this.onError = OnError;
            this.onFinish = OnFinish;
            return this;
        }

        public override IEnumerator ToCoroutine()
        {
            if (!IsInit) OnInit();
            yield return new WaitUntil(() => OnExecute(Time.deltaTime));
            OnFinish();
        }

        public override void OnInit()
        {
            IsInit = true;
            data = 0;
            IsFinish = false;
        }
    }
}
