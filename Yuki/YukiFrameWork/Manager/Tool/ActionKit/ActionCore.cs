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
namespace YukiFrameWork
{
    public static class RepeatExtension
    {
        public static IActionNode Delay(this IRepeat repeat, float currentTime, Action callBack = null)
        {
            repeat.ActionNode = ActionKit.Delay(currentTime, callBack);
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

        public static IActionNode StartTimer(this IRepeat repeat, float maxTime, Action<float> TimpTemp, Action callBack = null, bool isConstraint = false)
        {
            repeat.ActionNode = ActionKit.StartTimer(maxTime, TimpTemp, callBack, isConstraint);
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
            repeat.ActionNode = new CallBack<ISequence>(sequence, sequenceEvent);
            return repeat;
        }

        public static IActionNode Parallel(this IRepeat repeat, Action<IParallel> parallelEvent)
        {
            var parallel = ActionKit.Parallel();
            repeat.ActionNode = new CallBack<IParallel>(parallel, parallelEvent);
            return repeat;
        }
    }

    public static class ActionSequenceExtension
    {
        public static ISequence Delay(this ISequence sequence, float currentTime, Action onDelayFinish = null)
            => sequence.AddSequence(new Delay(currentTime, onDelayFinish));

        public static ISequence CallBack(this ISequence sequence, Action callBack)
            => sequence.AddSequence(new CallBack(callBack));

        public static ISequence Condition(this ISequence sequence, Func<bool> condition)
            => sequence.AddSequence(new Condition(condition));

        public static ISequence Parallel(this ISequence sequence, Action<IParallel> onEvent)
        {
            var parallel = new Parallel();
            sequence.AddSequence(new CallBack<IParallel>(parallel, onEvent));
            return sequence;
        }

        public static ISequence Sequence(this ISequence sequence, Action<ISequence> onEvent)
        {
            var newSequence = new Sequence();
            sequence.AddSequence(new CallBack<ISequence>(newSequence, onEvent));
            return sequence;
        }
    }

    public static class ParallelExtension
    {
        public static IParallel Delay(this IParallel parallel, float currentTime, Action callBack = null)
            => parallel.AddParallel(new Delay(currentTime, callBack));

        public static IParallel Condition(this IParallel parallel, Func<bool> condition)
            => parallel.AddParallel(new Condition(condition));

        public static IParallel CallBack(this IParallel parallel, Action callBack)
            => parallel.AddParallel(new CallBack(callBack));

        public static IParallel Sequence(this IParallel parallel, Action<ISequence> sequenceEvent)
        {
            var sequence = new Sequence();
            parallel.AddParallel(new CallBack<ISequence>(sequence, sequenceEvent));
            return parallel;
        }

        public static IParallel Parallel(this IParallel parallel, Action<IParallel> parallelEvent)
        {
            var newParallel = new Parallel();
            parallel.AddParallel(new CallBack<IParallel>(newParallel, parallelEvent));
            return parallel;
        }
    }

    public class Condition : ActionNode
    {
        private Func<bool> condition;
        public Condition(Func<bool> condition)
        {
            OnReset(condition);
        }
        public void OnReset(Func<bool> condition)
        {
            this.condition = condition;
        }

        public override bool OnExecute(float delta)
        {
            if (IsPaused) return false;
            return condition == null ? true : condition.Invoke();
        }

        public override void OnFinish()
        {
            IsFinish = true;
            IsInit = false;
           
            
            condition = null;
        }

        public override void OnInit()
        {
            IsFinish = false;
            IsInit = true;
        }

        public override IEnumerator ToCoroutine()
        {
            if (!IsInit) OnInit();
            if (condition == null) yield break;
            yield return new WaitUntil(condition);
        }
    }

    public class CallBack : ActionNode
    {
        private Action callBack;
        public CallBack(Action callBack)
        {
            OnReset(callBack);
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
            IsFinish = true;
            IsInit = false;                     
            callBack = null;
        }

        public override void OnInit()
        {
            IsInit = true;
            IsFinish = false;
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
        private TNode callBack;
        private Action<TNode> onEvent;
        public CallBack(TNode TNode, Action<TNode> onEvent)
        {
            OnReset(TNode, onEvent);
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
            IsFinish = true;
        }

        public override void OnInit()
        {
            IsInit = true;
            IsFinish = false;
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
                objectTrigger.AddAction(obj);

                foreach (var p in obj.actions)
                {
                    if (p is IParallel parallel)
                    {
                        if (parallel.GetHashCode() != obj.GetHashCode())
                            parallel?.Start(component);
                    }
                    else if (p is ISequence sequence)
                    {
                        if (sequence.GetHashCode() != obj.GetHashCode())
                            sequence?.Start(component);
                    }
                }
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
            uC.AddUpdate(UpdateNode);
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
            Action.AddNode(new Condition(condition));
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
            Action.AddNode(new Condition(condition));
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