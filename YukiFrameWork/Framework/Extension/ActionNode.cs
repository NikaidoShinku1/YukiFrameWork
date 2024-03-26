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

        bool IsCompleted { get; }

        IActionNode AddNode<TNode>(TNode node) where TNode : IActionNode;

        abstract void OnInit();

        abstract void OnFinish();

        void Cancel();

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
        IActionUpdateCondition Delay(float time);        
    }

    public interface IMonoActionNode<T> : IMonoActionNode where T : Delegate
    {
        void Init(T action,Mono mono);
    }

    public interface IMonoActionNode
    {
        public enum Mono
        {
            OnApplicationFocus,
            OnApplicationPause,
            OnApplicationQuit,
            OnGUI,
            OnDrawGizmos
        }
    }  
    public class MonoAction<T> : IMonoActionNode<T> where T : Delegate
    {      
        private static SimpleObjectPools<MonoAction<T>> simpleObjectPools
            = new SimpleObjectPools<MonoAction<T>>(() => new MonoAction<T>(),null,10);

        public T action { get; private set; }
        public IMonoActionNode.Mono m { get; private set; }
        public static MonoAction<T> Get(T action, IMonoActionNode.Mono m)
        {
            var mono = simpleObjectPools.Get();
            mono.Init(action,m);
            return mono;
        }
        public void Init(T action,IMonoActionNode.Mono m)
        {
            this.action = action;
            this.m = m;
        }

        public static bool Release(MonoAction<T> action) => simpleObjectPools.Release(action);
    }

    public abstract class ActionNode : IActionNode
    {      
        public abstract bool OnExecute(float delta);

        public Queue<IActionNode> actions { get; } = new Queue<IActionNode>();

        public virtual bool IsPaused { get; set; }     

        public virtual bool IsCompleted { get; protected set; }

        public bool IsInit { get; protected set; } = false;      
  
        public abstract IEnumerator ToCoroutine();

        public abstract void OnFinish();
    
        public abstract void OnInit();

        public IActionNode AddNode<TNode>(TNode node) where TNode : IActionNode
        {
            actions.Enqueue(node);
            return this;
        }

        public void Cancel()
        {
            IsCompleted = true;
        }
    }

    public class ActionUpdateNode : ActionNode, IActionUpdateNode
    {
        private static SimpleObjectPools<ActionUpdateNode> simpleObjectPools
            = new SimpleObjectPools<ActionUpdateNode>(() => new ActionUpdateNode(), null, 10);

        public UpdateStatus UpdateStatus { get; private set; }

        protected long data;
        private float current = 0;
        public float maxTime { get; set; }
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
                if (current < maxTime)
                {
                    current += delta;
                    return false;
                }
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
            catch(Exception ex)
            {
                if (onError != null)
                {
                    onError?.Invoke();
                    return true;
                }
                else
                {
                    throw ex;             
                }
            }
        }

        public override void OnFinish()
        {
            IsInit = false;
                     
            IsCompleted = true;
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
            IsCompleted = false;
        }
    }
}
