using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork.States
{
    public enum ConditionState
    {
        //满足条件
        Meet,
        NotMeet,
    }
    public class StateCondition
    {
        #region 字段
        private StateConditionData conditionData;

        private StateParameterData parameterData;

        public event System.Action onConditionMeet;

        private static Dictionary<CompareType, IParamaterCompare> paramaterDict = new Dictionary<CompareType, IParamaterCompare>();
        #endregion

        #region 属性
        public ConditionState ConditionState { get; private set; } = ConditionState.NotMeet;

        private IParamaterCompare compare => GetCompare(conditionData.compareType);
        #endregion


        #region 方法

        public StateCondition(StateConditionData conditionData,IState manager)
        {
            this.conditionData = conditionData;

            if (manager.ParametersDicts.ContainsKey(conditionData.parameterName))
            {
                parameterData = manager.ParametersDicts[this.conditionData.parameterName];
            }

            if (parameterData != null)
            {
                parameterData.onValueChange += CheckParameterValueChange;
            }

            CheckParameterValueChange();
        }

        static StateCondition()
            => Init();

        public void CheckParameterValueChange()
        {
            //判断条件是否满足

            if (compare.IsMeetCondition(this.parameterData, conditionData.targetValue))
            {
                ConditionState = ConditionState.Meet;              
                onConditionMeet?.Invoke();
            }
            else ConditionState = ConditionState.NotMeet;
        }       

        private static void Init()
        {
            if (paramaterDict.Count == 0)
            {
                paramaterDict.Add(CompareType.Equal, new EqualCompare());
                paramaterDict.Add(CompareType.Greater, new GreaterCompare());
                paramaterDict.Add(CompareType.Less, new LessCompare());
                paramaterDict.Add(CompareType.NotEqual, new NotEqualCompare());
            }
        }

        public static IParamaterCompare GetCompare(CompareType type)
        {
            paramaterDict.TryGetValue(type, out var data);                       
            return data;
        }

        #endregion


    }
}
