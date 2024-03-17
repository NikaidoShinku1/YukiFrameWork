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

        private string layerName;
      
        #endregion

        public StateTransition(StateManager stateManager, StateTransitionData transitionData,string layerName, bool childMechine = false)
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

            StateBase targetState = null;
            if (!childMechine)
            {
                targetState = stateManager.runTimeSubStatePair["BaseLayer"].stateBases.Where(x => x.name == transitionData.toStateName).FirstOrDefault();
            }
            else
            {
                targetState = stateManager.runTimeSubStatePair[transitionData.layerName].stateBases.Where(x => x.name == transitionData.toStateName).FirstOrDefault();
            }

            if (targetState != null)
            {
                toState = targetState;
            }

            this.layerName = layerName;
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

            if (!transitionData.fromStateName.Equals(stateManager.runTimeSubStatePair[layerName].CurrentState?.name))
            {
                if (stateManager.runTimeSubStatePair[layerName].CurrentState.name.Equals(transitionData.toStateName))
                    return;
            }                    
            stateManager.OnChangeState(toState);

        }
     
    }
}
