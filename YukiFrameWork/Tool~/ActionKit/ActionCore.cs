///=====================================================
/// - FileName:      ActionCore.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   Action核心部分
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Collections;
using YukiFrameWork.Pools;
namespace YukiFrameWork
{
    public static class RepeatExtension
    {
        public static IActionNode Delay(this IRepeat repeat, float currentTime, Action callBack = null,bool isRealTime = false)
        {
            repeat.ActionNode = ActionKit.Delay(currentTime, callBack,isRealTime);
            return repeat;
        }

        public static IActionNode DelayFrame(this IRepeat repeat, int frameCount, Action callBack = null)
        {
            repeat.ActionNode = ActionKit.DelayFrame(frameCount, callBack);
            return repeat;
        }

        public static IActionNode NextFrame(this IRepeat repeat, Action callBack)
        {
            repeat.ActionNode = ActionKit.NextFrame(callBack);
            return repeat;
        }

        public static IActionNode StartTimer(this IRepeat repeat, float maxTime, Action<float> TimpTemp, Action callBack = null, bool isConstraint = false, bool isRealTime = false)
        {
            repeat.ActionNode = ActionKit.StartTimer(maxTime, TimpTemp, callBack, isConstraint,isRealTime);
            return repeat;
        }

        public static IActionNode ExecuteFrame(this IRepeat repeat, Func<bool> predicate, Action callBack = null)
        {
            repeat.ActionNode = ActionKit.ExecuteFrame(predicate, callBack);
            return repeat;
        }

        public static IActionNode Sequence(this IRepeat repeat, Action<ISequence> sequenceEvent)
        {
            var sequence = ActionKit.Sequence();
            repeat.ActionNode = CallBack<ISequence>.Get(sequence, sequenceEvent);
            return repeat;
        }

        public static IActionNode Parallel(this IRepeat repeat, Action<IParallel> parallelEvent)
        {
            var parallel = ActionKit.Parallel();
            repeat.ActionNode = YukiFrameWork.CallBack<IParallel>.Get(parallel, parallelEvent);
            return repeat;
        }
    }

    public static class ActionSequenceExtension
    {
        public static ISequence Delay(this ISequence sequence, float currentTime, Action onDelayFinish = null, bool isRealTime = false)
            => sequence.AddSequence(YukiFrameWork.Delay.Get(currentTime, onDelayFinish,isRealTime));

        public static ISequence CallBack(this ISequence sequence, Action callBack)
            => sequence.AddSequence(YukiFrameWork.CallBack.Get(callBack));

        public static ISequence Condition(this ISequence sequence, Func<bool> condition)
            => sequence.AddSequence(YukiFrameWork.Condition.Get(condition));

        public static ISequence ExecuteFrame(this ISequence sequence, Func<bool> predicate, Action CallBack = null)
            => sequence.AddSequence(YukiFrameWork.ExecuteFrame.Get(predicate, CallBack));

        public static ISequence Parallel(this ISequence sequence, Action<IParallel> parallelEvent)
        {
            var parallel = YukiFrameWork.Parallel.Get();
            sequence.AddSequence(CallBack<IParallel>.Get(parallel, parallelEvent)) ;
            return sequence;
        }

        public static ISequence Repeat(this ISequence sequence,int Count, Action<IRepeat> repeatEvent)
        {
            var repeat = YukiFrameWork.Repeat.Get(Count);
            sequence.AddSequence(CallBack<IRepeat>.Get(repeat, repeatEvent));
            return sequence;
        }

        public static ISequence StartTimer(this ISequence sequence, float maxTime, Action<float> TimpTemp, Action callBack = null, bool isConstraint = false, bool isRealTime = false)
            => sequence.AddSequence(Timer.Get(maxTime, TimpTemp, callBack, isConstraint,isRealTime));
       
        public static ISequence Sequence(this ISequence sequence, Action<ISequence> sequenceEvent)
        {
            var newSequence = YukiFrameWork.Sequence.Get();
            sequence.AddSequence(CallBack<ISequence>.Get(newSequence, sequenceEvent));
            return sequence;
        }
    }

    public static class ParallelExtension
    {
        public static IParallel Delay(this IParallel parallel, float currentTime, Action callBack = null, bool isRealTime = false)
            => parallel.AddParallel(YukiFrameWork.Delay.Get(currentTime, callBack,isRealTime));

        public static IParallel Condition(this IParallel parallel, Func<bool> condition)
            => parallel.AddParallel(YukiFrameWork.Condition.Get(condition));

        public static IParallel CallBack(this IParallel parallel, Action callBack)
            => parallel.AddParallel(YukiFrameWork.CallBack.Get(callBack));

        public static IParallel Repeat(this IParallel parallel, int count, Action<IRepeat> repeatEvent)
        {
            var repeat = YukiFrameWork.Repeat.Get(count);
            parallel.AddParallel(CallBack<IRepeat>.Get(repeat, repeatEvent));
            return parallel;
        }

        public static IParallel ExecuteFrame(this IParallel parallel, Func<bool> predicate, Action CallBack = null)
            => parallel.AddParallel(YukiFrameWork.ExecuteFrame.Get(predicate, CallBack));

        public static IParallel Sequence(this IParallel parallel, Action<ISequence> sequenceEvent)
        {
            var sequence = YukiFrameWork.Sequence.Get();
            parallel.AddParallel(YukiFrameWork.CallBack<ISequence>.Get(sequence, sequenceEvent));
            return parallel;
        }    

        public static IParallel StartTimer(this IParallel parallel, float maxTime, Action<float> TimpTemp, Action callBack = null, bool isConstraint = false, bool isRealTime = false)
            => parallel.AddParallel(Timer.Get(maxTime, TimpTemp, callBack, isConstraint, isRealTime));

        public static IParallel Parallel(this IParallel parallel, Action<IParallel> parallelEvent)
        {
            var newParallel = YukiFrameWork.Parallel.Get();
            parallel.AddParallel(YukiFrameWork.CallBack<IParallel>.Get(newParallel, parallelEvent));
            return parallel;
        }
    }

    public class Condition : ActionNode
    {
        private static SimpleObjectPools<Condition> simpleObjectPools
            = new SimpleObjectPools<Condition>(() => new Condition(), null, 10);
        private Func<bool> condition;
        public Condition(Func<bool> condition)
        {
            OnReset(condition);
        }
        public void OnReset(Func<bool> condition)
        {
            this.condition = condition;
        }

        public Condition() 
        {

        }

        public static Condition Get(Func<bool> condition)
        {
            var c = simpleObjectPools.Get();
            c.OnReset(condition);
            return c;
        }

        public override bool OnExecute(float delta)
        {
            if (IsPaused) return false;
            return condition == null ? true : condition.Invoke();
        }

        public override void OnFinish()
        {
            IsCompleted = true;
            IsInit = false;                       
            condition = null;
            simpleObjectPools.Release(this);
        }

        public override void OnInit()
        {
            IsCompleted = false;
            IsInit = true;
        }

        public override IEnumerator ToCoroutine()
        {
            if (!IsInit) OnInit();
            if (condition == null) yield break;
            yield return new WaitUntil(condition);
            OnFinish();
        }
    }

    public class CallBack : ActionNode
    {
        private static SimpleObjectPools<CallBack> simpleObjectPools
             = new SimpleObjectPools<CallBack>(() => new CallBack(), null, 10);
        private Action callBack;
        public CallBack(Action callBack)
        {
            OnReset(callBack);
        }

        public CallBack() { }

        public static CallBack Get(Action callBack)
        {
            var c = simpleObjectPools.Get();
            c.OnReset(callBack);
            return c;
        }

        public void OnReset(Action callBack)
        {
            this.callBack = callBack;
        }

        public override bool OnExecute(float delta)
        {
            callBack?.Invoke();
            return true;
        }

        public override void OnFinish()
        {
            IsCompleted = true;
            IsInit = false;                     
            callBack = null;
            simpleObjectPools.Release(this);
        }

        public override void OnInit()
        {
            IsInit = true;
            IsCompleted = false;
        }

        public override IEnumerator ToCoroutine()
        {
            if (!IsInit) OnInit();
            yield return new WaitUntil(() => OnExecute(Time.deltaTime));
            OnFinish();
        }
    }
    public class CallBack<TNode> : ActionNode where TNode : IActionNode
    {
        private static SimpleObjectPools<CallBack<TNode>> simpleObjectPools
               = new SimpleObjectPools<CallBack<TNode>>(() => new CallBack<TNode>(), null, 10);
        private TNode callBack;
        private Action<TNode> onEvent;
        public CallBack(TNode TNode, Action<TNode> onEvent)
        {
            OnReset(TNode, onEvent);
        }

        public CallBack() { }

        public static CallBack<TNode> Get(TNode TNode, Action<TNode> onEvent)
        {
            var cT = simpleObjectPools.Get();
            cT.OnReset(TNode, onEvent);
            return cT;
        }

        public void OnReset(TNode TNode, Action<TNode> onEvent)
        {
            this.callBack = TNode;
            this.onEvent = onEvent;
        }

        public override bool OnExecute(float delta)
        {
            if (callBack.OnExecute(delta))
            {
                return true;
            }

            return false;
        }

        public override void OnFinish()
        {
            callBack = default;
            onEvent = null;
            IsInit = false;
            IsCompleted = true;
            simpleObjectPools.Release(this);
        }

        public override void OnInit()
        {
            IsInit = true;
            IsCompleted = false;
            onEvent?.Invoke(callBack);
        }

        public override IEnumerator ToCoroutine()
        {
            if (!IsInit) OnInit();
            yield return new WaitUntil(() => OnExecute(Time.deltaTime));
            OnFinish();
        }
    }


    public struct ActionController : IActionNodeController
    {
        public IActionNode Action { get; set; }
        private OnGameObjectTrigger objectTrigger;


        public IActionNodeController Start<T>(T component, Action callBack = null) where T : Component
        {
            
            if (!component.TryGetComponent(out objectTrigger))
            {
                objectTrigger = component.gameObject.AddComponent<OnGameObjectTrigger>();

            }
            foreach (var obj in Action.actions)
            {
                objectTrigger.AddAction(obj,this);           
            }

            objectTrigger.PushFinishEvent(callBack);
         
            return this;
        }

    }

    public struct ActionUpdateNodeController : IActionUpdateNodeController
    {
        public IActionUpdateNode UpdateNode { get; set; }

        public IActionNodeController Start<T>(T component, Action callBack = null) where T : Component
        {
            var uC = component.GetComponent<MonoUpdateExecute>();
            uC = uC == null ? component.gameObject.AddComponent<MonoUpdateExecute>() : uC;
            uC.AddUpdate(UpdateNode,this);
            uC.PushFinishEvent(callBack);
            return this;
        }
    }

    public static class BindGameObjectExtension
    {
        public static IActionNodeController Start<TComponent>(this IActionNode action, TComponent component, Action onFinish = null) where TComponent : Component
        {
            return new ActionController()
            {
                Action = action
            }.Start(component, onFinish);
        }

        public static IActionNodeController Start<TComponent>(this IActionUpdateNode action, TComponent component, Action onFinish = null) where TComponent : Component
        {
            return new ActionUpdateNodeController()
            {
                UpdateNode = action
            }.Start(component, onFinish);
        }
    }

    public struct ActionUpdateCondition : IActionUpdateCondition
    {
        public ActionUpdateNode Action { get; set; }

        public IActionUpdateCondition First(Func<bool> condition = null)
        {
            Action.IsFirstExecute = true;
            Action.AddNode(YukiFrameWork.Condition.Get(condition));
            return this;
        }

        public IActionUpdateNode Register(Action<long> OnEvent, Action OnError = null, Action OnFinish = null)
        {
            Action.Register(OnEvent, OnError, OnFinish);
            return Action;
        }

        public IActionUpdateCondition TakeWhile(Func<bool> onFinishCondition)
        {
            Action.OnFinishCondition = onFinishCondition;
            return this;
        }

        public IActionUpdateCondition Where(Func<bool> condition)
        {
            Action.AddNode(YukiFrameWork.Condition.Get(condition));
            return this;
        }
    }

    public static class BindMonoUpdateExecuteExtension
    {
        public static IActionUpdateCondition Where(this IActionUpdateNode action, Func<bool> condition)
        {
            return new ActionUpdateCondition()
            {
                Action = action as ActionUpdateNode
            }.Where(condition);
        }

        public static IActionUpdateCondition First(this IActionUpdateNode action, Func<bool> condition)
        {
            return new ActionUpdateCondition()
            {
                Action = action as ActionUpdateNode
            }.First(condition);
        }

        public static IActionUpdateCondition TakeWhile(this IActionUpdateNode action, Func<bool> condition)
        {
            return new ActionUpdateCondition()
            {
                Action = action as ActionUpdateNode
            }.TakeWhile(condition);
        }
    }


}