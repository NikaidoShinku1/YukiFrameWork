using System;
using UnityEngine;

namespace YukiFrameWork.ActionStates
{
    /// <summary>
    /// 状态连接组件  
    /// </summary>
    [Serializable]
    public sealed class Transition : StateBase
    {
        public int currStateID, nextStateID;
        /// <summary>
        /// 当前状态
        /// </summary>
		public State CurrState => stateMachine.States[currStateID];
        /// <summary>
        /// 下一个状态
        /// </summary>
		public State NextState => stateMachine.States[nextStateID];
        /// <summary>
        /// 连接控制模式
        /// </summary>
		public TransitionMode mode = TransitionMode.ScriptControl;
        /// <summary>
        /// 当前时间
        /// </summary>
		public float time;
        /// <summary>
        /// 结束时间
        /// </summary>
		public float exitTime = 1f;
        /// <summary>
        /// 是否进入下一个状态?
        /// </summary>
		public bool isEnterNextState;

        public Transition() { }

        public Transition(State state, int stateId, params TransitionBehaviour[] behaviours)
        {
            this.behaviours = new BehaviourBase[0];
            currStateID = state.ID;
            nextStateID = stateId;
            stateMachine = state.stateMachine;
            AddComponent(behaviours);
            ArrayExtend.Add(ref state.transitions, this);
            for (int i = 0; i < state.transitions.Length; i++)
                state.transitions[i].ID = i;
        }

        /// <summary>
        /// 创建连接实例
        /// </summary>
        /// <param name="state">连接的开始状态</param>
        /// <param name="nextState">连接的结束状态</param>
        /// <param name="transitionName">连接名称</param>
        /// <returns></returns>
		public static Transition CreateTransitionInstance(State state, State nextState, string transitionName = "New Transition")
        {
            var t = new Transition
            {
                name = transitionName,
                currStateID = state.ID,
                nextStateID = nextState.ID,
                stateMachine = state.stateMachine,
                behaviours = new BehaviourBase[0],
            };
            ArrayExtend.Add(ref state.transitions, t);
            for (int i = 0; i < state.transitions.Length; i++)
                state.transitions[i].ID = i;
            return t;
        }

        internal void Init(IStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = (TransitionBehaviour)behaviours[i].InitBehaviour(stateMachine);             
                behaviour.transitionID = i;
                behaviours[i] = behaviour;
                behaviour.OnInit();
            }
        }

        internal void Update(UpdateStatus updateStatus)
        {
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i] as TransitionBehaviour;
                if (behaviour.Active)
                {
                    switch (updateStatus)
                    {
                        case UpdateStatus.OnUpdate:
                            behaviour.OnUpdate(ref isEnterNextState);
                            break;
                        case UpdateStatus.OnFixedUpdate:
                            behaviour.OnFixedUpdate(ref isEnterNextState);
                            break;
                        case UpdateStatus.OnLateUpdate:
                            behaviour.OnLateUpdate(ref isEnterNextState);
                            break;                     
                    }
                }
            }
            if (mode == TransitionMode.ExitTime)
            {
                time += Time.deltaTime;
                if (time > exitTime)
                    isEnterNextState = true;
            }
            if (isEnterNextState)
            {
                time = 0;
                isEnterNextState = false;
                stateMachine.ChangeState(nextStateID, 0, true);
            }
        }
    }
}