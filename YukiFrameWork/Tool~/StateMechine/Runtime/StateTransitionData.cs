using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork.States
{   
    [Serializable]
    public class StateTransitionData
    {
        public string fromStateName;
        public string toStateName;
        public string layerName;
        public List<StateConditionData> conditions = new List<StateConditionData>();
        public List<GroupConditionDatas> conditionDatas = new List<GroupConditionDatas>();
    }

    [Serializable]
    public class GroupConditionDatas
    {
        public List<StateConditionData> conditions = new List<StateConditionData>();            
    }

    [Serializable]
    public class GroupCondition
    {
        public List<StateCondition> stateConditions = new List<StateCondition>();

        public GroupCondition(List<StateConditionData> conditions,IState manager)
        {
            for (int i = 0; i < conditions.Count; i++)
            {
                StateCondition condition = new StateCondition(conditions[i], manager);
                condition.onConditionMeet += CheckMeet;
                stateConditions.Add(condition);
            }
        }

        internal Action onConditionMeet;

        public bool IsMeet
        {
            get
            {            
                if (ConditionCount == 0)
                    return false;

                for (int i = 0; i < stateConditions.Count; i++)
                {
                    if (stateConditions[i].ConditionState == ConditionState.NotMeet) 
                        return false;
                }

                return true;
            }
        }

        private void CheckMeet()
        {
            if (IsMeet)
                onConditionMeet?.Invoke();
        }

        public int ConditionCount
        {
            get
            {
                if (stateConditions == null)
                    return 0;
                return stateConditions.Count;
            }
        }
    }
}
