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
   // [Flags]
    public enum UpdateStatus
    {
        OnUpdate = 1,
        OnFixedUpdate = 2,
        OnLateUpdate = 4,
    }

    public interface IActionUpdateNode
    {
        UpdateStatus UpdateStatus { get; }     
        IActionUpdateNode Register(Action<float> OnEvent);
        bool OnExecute(float delta);
        void OnFinish();
    }

    public interface IActionUpdateCondition
    {
        ActionUpdateNode Action { get; }
        IActionUpdateNode Register(Action<float> onEvent);
        IActionUpdateCondition Where(Func<bool> condition);        
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
            OnDrawGizmos,
            OnCanvasGroupChanged
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

        [DisableEnumeratorWarning]
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
        
        protected Action<float> onEvent;        

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
            foreach (var action in actions)
            {
                IsPaused = !action.OnExecute(delta);
            }          
            if (IsPaused) return false;          
            onEvent?.Invoke(delta);
            return false;
        }

        public override void OnFinish()
        {
            IsInit = false;                   
            IsCompleted = true;
           
            onEvent = null;
            IsPaused = false;
            simpleObjectPools.Release(this);
        }

        public IActionUpdateNode Register(Action<float> onEvent)
        {
            this.onEvent = onEvent;                     
            return this;
        }
        [DisableEnumeratorWarning]
        public override IEnumerator ToCoroutine()
        {
            if (!IsInit) OnInit();
            yield return CoroutineTool.WaitUntil(() => OnExecute(Time.deltaTime));
            OnFinish();
        }

        public override void OnInit()
        {
            IsInit = true;           
            IsCompleted = false;
        }
    }
}
