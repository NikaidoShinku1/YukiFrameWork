using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using YukiFrameWork.Pools;

namespace YukiFrameWork.States
{
    public class StateTransition
    {
        #region 字段
        private StateTransitionData transitionData;

        private IState stateManager;

        private StateBase toState;

        public string FromStateName => transitionData.fromStateName;

        public List<StateCondition> conditions;          
      
        #endregion

        public StateTransition(IState stateManager, StateTransitionData transitionData)
        {
            this.transitionData = transitionData;
            this.stateManager = stateManager;

            this.conditions = ListPools<StateCondition>.Get();

            foreach (var item in this.transitionData.conditions)
            {
                StateCondition condition = new StateCondition(item, this.stateManager);
                condition.onConditionMeet += this.CheckConditionIsMeet;
                this.conditions.Add(condition);
            }

            var targetState = stateManager.runTimeStatesDict.Values.Where(x => x.name == transitionData.toStateName).FirstOrDefault();

            if (targetState != null)
            {
                toState = targetState;
            }
        }

        /// <summary>
        /// 检测条件是否都满足
        /// </summary>
        public void CheckConditionIsMeet()
        {
            if (conditions.Count == 0)
            {
                return;
            }

            foreach (var item in conditions)
            {
                if (item.ConditionState == ConditionState.NotMeet) return;
            }

            if (toState == null)
            {
                Debug.LogError("查询目标状态失败！");
                return;
            }         

            if (!transitionData.fromStateName.Equals(stateManager.CurrentState?.name))
            {

                if (stateManager.CurrentState.name.Equals(transitionData.toStateName))
                    return;
            }                    

            stateManager.OnChangeState(toState);


        }
     
    }
}
