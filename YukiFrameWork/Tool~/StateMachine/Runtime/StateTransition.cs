///=====================================================
/// - FileName:      StateTransition.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/12 1:29:44
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using StateConditionGroup = System.Collections.Generic.List<YukiFrameWork.Machine.StateCondition>;
using System.Collections.Generic;
using System.Linq;
namespace YukiFrameWork.Machine
{
    public class StateTransition 
    {
        private StateTransitionData stateTransition;
        private StateBase toState;
        private StateMachine stateMachine;
        public StateTransitionData TransitionData => stateTransition;

        private List<StateConditionGroup> stateConditions = new List<StateConditionGroup>();

        public StateBase To => toState;

        public StateTransition(StateMachine stateMachine,StateTransitionData stateTransitionData)
        {
            this.stateMachine = stateMachine;
            this.stateTransition = stateTransitionData;

            StateConditionGroup stateConditionGroup = new StateConditionGroup(stateTransition.conditions
                .Select(x => new StateCondition(x,stateMachine)));

            stateConditions.Add(stateConditionGroup);

            foreach (var item in stateTransition.conditionGroups)
            {
                StateConditionGroup itemGroup = new StateConditionGroup(item
               .Select(x => new StateCondition(x, stateMachine)));
                stateConditions.Add(itemGroup);
            }
           
            foreach (var item in stateConditions)
            {
                foreach (var condition in item)
                {                   
                    condition.onConditionMeet += delegate { CheckConditionAndSwitch(); };
                }
            }

            var state = stateMachine.GetState(this.stateTransition.toStateName);
            if (state != null)
                toState = state;
        }

        /// <summary>
        /// 判断条件是否满足
        /// </summary>
        public bool IsMeet
        {
            get
            {
                if (!ConditionMeets())
                    return false;

                if (toState == null)
                    throw new Exception("目标状态丢失!");

                StateBase from = stateMachine.GetState(stateTransition.fromStateName);

                //当前状态为空抛出异常
                if (from == null)
                    throw new NullReferenceException($"来自{stateTransition.ToString()}的过渡 当前状态查询失败，无法进行过渡!");

                //如果当前状态就是目标状态，则是不允许切换的
                if (from == toState)
                    return false;

                //如果目标状态就是正在执行的状态，也是不允许切换的
                if (toState == stateMachine.CurrentState)
                    return false;

                //如果当前状态不是正在运行的状态，也是不允许切换的ToDo
                if (from != stateMachine.CurrentState)
                {
                    if (from.Runtime_StateData.IsAnyState && from.Runtime_StateData.parentStateMachineName == stateMachine.Name)
                    {                       
                        return true;
                    }

                    return false;
                }

                return true;

                

            }
        }
        public bool CheckConditionAndSwitch()
        {
           //判断条件是否满足
            bool isMeet = IsMeet;
            if (isMeet)
            {
                //先重置一次Trigger
                ResetTrigger();

                //切换到目标状态
                stateMachine.SwitchState(toState,this);
            }
            return isMeet;
        }
        private void ResetTrigger()
        {
            foreach (var items in stateConditions)
            {
                foreach (var condition in items)
                {
                    condition.ResetTrigger();
                }
            }
        }
        public int ConditionCount => stateConditions == null ? 0 : stateConditions.Count;
        private bool ConditionMeets()
        {
            if (stateTransition.autoSwitch && stateTransition.Empty)
                return true;
            if (ConditionCount == 0) return false; 

            foreach (var items in stateConditions)
            {
                bool isMeet = true;
                foreach (var condition in items)
                {
                    if (!condition.IsMeet)
                    {
                        isMeet = false;
                        break;
                    }
                }
                if (!isMeet) return false;
            }
            return true;
        }

    }
}
