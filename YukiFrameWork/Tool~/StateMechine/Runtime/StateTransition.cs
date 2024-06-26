﻿using System.Collections;
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

        public string ToStateName => transitionData.toStateName;

        private string layerName;

        public List<GroupCondition> groupConditions = new List<GroupCondition>();
        #endregion

        public StateTransition(StateManager stateManager, StateTransitionData transitionData,string layerName, bool childMechine = false)
        {
            this.transitionData = transitionData;
            this.stateManager = stateManager;

            //this.conditions = ListPools<StateCondition>.Get();

            GroupCondition groupCondition = new GroupCondition(this.transitionData.conditions,stateManager);

            groupCondition.onConditionMeet += this.CheckCondition;
            groupConditions.Add(groupCondition);

            foreach (var item in transitionData.conditionDatas)
            {
                GroupCondition group = new GroupCondition(item.conditions, stateManager);
                group.onConditionMeet += this.CheckCondition;
                groupConditions.Add(group);
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
        internal bool CheckConditionIsMeet()
        {
            if (groupConditions.Count == 0)
            {
                return false;
            }

            if (!ConditionGroupMeet()) return false;

            if (toState == null)
            {
                Debug.LogError("查询目标状态失败！");
                return false;
            }
            StateBase currentState = stateManager.currents.Find(x => x.layerName == layerName);
            StateBase fromState = stateManager.runTimeSubStatePair[layerName].stateBases.Find(x => x.name == FromStateName);

            if (currentState == null || fromState == null) return false;
            if (!transitionData.fromStateName.Equals(currentState.name))
            {
                if (fromState?.IsAnyState == false) return false;

                if (currentState.name == FromStateName) return false;
            }

            return true;
        }

        public void CheckCondition()
        {
            CheckConditionInStateEnter();
        }

        private bool TransitionExecute()
        {
            bool toContains = false;
            try
            {
                toContains = stateManager.runTimeSubStatePair[layerName].CurrentState.name.Equals(transitionData.toStateName);
            }
            catch { toContains = true; }
            if (!FromStateName.Equals(StateConst.anyState))
            {
                if (!FromStateName.Equals(stateManager.runTimeSubStatePair[layerName].CurrentState?.name)
                    || toContains)
                {
                    return false;
                }
            }
            else
            {
                if (toContains) return false;
            }
            stateManager.OnChangeState(toState);
            return true;
        }

        internal bool CheckConditionInStateEnter()
        {
            bool isMeet = CheckConditionIsMeet();

            if (isMeet)
                return TransitionExecute();

            return isMeet;
        }

        private bool ConditionGroupMeet()
        {
            foreach (var item in groupConditions)
            {
                if (item.IsMeet) return true;
            }

            return false;
        }
     
    }
}
