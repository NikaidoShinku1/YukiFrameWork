///=====================================================
/// - FileName:      StateCondition.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/12 1:30:01
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
namespace YukiFrameWork.Machine
{
    public class StateCondition  
    {
        private StateConditionData condition;
        private StateParameterData stateParameter;
        private StateMachine stateMachine;
        private IParameterCompare compare => GetParameterCompare(condition.compareType);
        private static Dictionary<CompareType, IParameterCompare> compares = new Dictionary<CompareType, IParameterCompare>();

        public StateCondition(StateConditionData stateCondition,StateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
            this.condition = stateCondition;
            
            stateParameter = stateMachine.StateMachineCore.GetStateParameterData(condition.parameterName);           
            stateParameter.Parameter.RegisterWithInitValue(value => 
            {              
                if (IsMeet)
                {
                    onConditionMeet?.Invoke();
                }
            });
        }
        public event Action onConditionMeet;
        
        static StateCondition()
        {
            compares.Add(CompareType.Equal, new EqualCompare());
            compares.Add(CompareType.Greater, new GreaterCompare());
            compares.Add(CompareType.Less, new LessCompare());
            compares.Add(CompareType.NotEqual, new NotEqualCompare());
        }
        
        public bool IsMeet
        {
            get
            {                               
                return compare.IsCondition(stateParameter,condition.targetValue);
            }
        }


        public static IParameterCompare GetParameterCompare(CompareType compareType)
        {
            if (compares.TryGetValue(compareType, out var item))
                return item;
            return null;
        }

        public void ResetTrigger()
        {
            if (this.stateParameter.parameterType != ParameterType.Trigger) return;

            stateMachine.StateMachineCore.ClearTrigger(StateManager.StringToHash(this.stateParameter.parameterName));
        }

    }
}
