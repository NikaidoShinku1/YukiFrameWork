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

        private StateManager stateManager;

        private StateBase toState;

        public string FromStateName => transitionData.fromStateName;

        public List<StateCondition> conditions;

        public TransitionMode TransitionMode => transitionData.transitionMode;

        private float stateLoadTime = 0;
      
        #endregion

        public StateTransition(StateManager stateManager, StateTransitionData transitionData)
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
           // stateManager.isDrawLine = true;

            if (!transitionData.fromStateName.Equals(stateManager.CurrentState?.name))
            {

                if (stateManager.CurrentState.name.Equals(transitionData.toStateName))
                    return;
            }         

            //切换状态

            stateManager.OnChangeState(toState);


        }
        /// <summary>
        /// 动画检测(当前过渡条件设置为动画模式时使用)
        /// </summary>
        /// <param name="fromState"></param>
        public void CheckConditionOrAnimIsMeet(StateBase fromState)
        {
            if (transitionData.transitionMode != TransitionMode.动画剪辑模式) return;

            if (fromState == null) return;

            switch (fromState.animData.type)
            {                
                case StateAnimType.Animator:
                    {
                        if (fromState.animData.animator == null)
                        {
                            throw new System.NullReferenceException("当前没有为该状态正确添加animator组件！");
                        }

                        if (CheckOrAnimator(fromState.animData))
                        {
                            stateManager.OnChangeState(toState);
/*#if UNITY_EDITOR
                            stateManager.isDrawLine = true;
#endif*/
                        }
                    }
                    break;
                case StateAnimType.Animation:
                    {
                        if (fromState.animData.animation == null)
                        {
                            throw new System.NullReferenceException("当前没有为该状态正确添加animation组件！");                          
                        }
                        if (CheckOrAnimation(fromState.animData))
                        {
                            stateManager.OnChangeState(toState);
/*#if UNITY_EDITOR
                            stateManager.isDrawLine = true;
#endif*/
                        }
                    }
                    break;           
            }
        }

        /// <summary>
        /// 检查条件类型是否为定时并检查过度
        /// </summary>
        public void CheckConditionOrTimeIsMeet()
        {
            if (transitionData.transitionMode != TransitionMode.定时模式) return;

            if (!transitionData.fromStateName.Equals(stateManager.CurrentState.name)) return;

            stateLoadTime += Time.deltaTime;         
            if (stateLoadTime >= transitionData.stateLoadTime)
            {
                stateLoadTime = 0;
                stateManager.OnChangeState(toState);
            }
        }

        private bool CheckOrAnimator(StateAnim anim)
        {
            var info = anim.animator.GetCurrentAnimatorStateInfo(anim.layer);                   
            if (!info.loop && info.normalizedTime * 100 >= anim.animLength)
                return true;
            return false;
        }

        private bool CheckOrAnimation(StateAnim anim)
        {
            float normalTime = anim.animation[anim.clipName].normalizedTime;
            if (!anim.animation[anim.clipName].clip.isLooping && normalTime * 100 >= anim.animLength - 5)
            {
                anim.animation.Stop();
                return true;
            }
            return false;
        }

    }
}
