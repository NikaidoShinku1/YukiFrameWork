using System;
using UnityEngine.Playables;

namespace YukiFrameWork.ActionStates
{
    /// <summary>
    /// ARPG状态动作
    /// </summary>
	[Serializable]
    public sealed class StateAction : StateBase
    {
        /// <summary>
        /// 动画剪辑名称
        /// </summary>
		public string clipName;
        /// <summary>
        /// 动画剪辑索引
        /// </summary>
		public int clipIndex;
        /// <summary>
        /// 可播放动画资源
        /// </summary>
        public PlayableAsset clipAsset;
        /// <summary>
        /// 当前动画时间
        /// </summary>
		public float animTime;
        /// <summary>
        /// 动画结束时间
        /// </summary>
		public float animTimeMax = 100f;
        /// <summary>
        /// 动画层， -1是播放所有层，其他值是指定层播放
        /// </summary>
        public int layer;
        private bool isStop;
        /// <summary>
        /// 动作是否完成?, 当动画播放结束后为True, 否则为false
        /// </summary>
        public bool IsComplete => animTime >= animTimeMax - 1;

        public StateAction() { }

        public StateAction(State state, string clipName, params ActionBehaviour[] behaviours)
        {
            ID = state.ID;
            SetAnimClip(clipName);
            if (behaviours != null)
                AddComponent(behaviours);
            else this.behaviours = new ActionBehaviour[0];
            ArrayExtend.Add(ref state.actions, this);
        }

        internal void Enter(State state)
        {
            isStop = false;
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i] as ActionBehaviour;
                if (behaviour.Active)
                    behaviour.OnEnter(this);
            }
            stateMachine.Handler.OnPlayAnimation(state, this);
        }

        internal void Exit()
        {
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i] as ActionBehaviour;
                if (behaviour.Active)
                    behaviour.OnExit(this);
            }
        }

        internal void Init(IStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i].InitBehaviour(stateMachine);               
                behaviours[i] = behaviour;
                behaviour.OnInit();
            }
        }

        internal void Update(State state,UpdateStatus updateStatus)
        {
            if (isStop)
                return;
            var isPlaying = stateMachine.Handler.OnAnimationUpdate(state, this,updateStatus);
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i] as ActionBehaviour;
                if (behaviour.Active)
                {
                    switch (updateStatus)
                    {
                        case UpdateStatus.OnUpdate:
                            behaviour.OnUpdate(this);
                            break;
                        case UpdateStatus.OnFixedUpdate:
                            behaviour.OnFixedUpdate(this);
                            break;
                        case UpdateStatus.OnLateUpdate:
                            behaviour.OnLateUpdate(this);
                            break;                     
                    }
                }
            }
            if (animTime >= animTimeMax | !isPlaying)
            {
                if (state.isExitState & state.transitions.Length > 0)
                {
                    state.transitions[state.DstStateID].isEnterNextState = true;
                    return;
                }
                if (state.animLoop)
                {
                    for (int i = 0; i < behaviours.Length; i++)
                    {
                        var behaviour = behaviours[i] as ActionBehaviour;
                        if (behaviour.Active)
                            behaviour.OnExit(this);
                    }
                    state.OnActionExit();
                    if (stateMachine.NextId == state.ID)//如果在动作行为里面有切换状态代码, 则不需要重载函数了, 否则重载当前状态
                        state.Enter(state.actionIndex);//重载进入函数
                    return;
                }
                else
                {
                    isStop = true;
                    for (int i = 0; i < behaviours.Length; i++)
                    {
                        var behaviour = behaviours[i] as ActionBehaviour;
                        if (behaviour.Active)
                            behaviour.OnStop(this);
                    }
                    state.OnActionStop();
                }
            }
        }

        internal void FixedUpdate()
        {
            if (isStop)
                return;
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i] as ActionBehaviour;
                if (behaviour.Active)
                    behaviour.OnFixedUpdate(this);
            }
        }

        internal void LateUpdate()
        {
            if (isStop)
                return;
            for (int i = 0; i < behaviours.Length; i++)
            {
                var behaviour = behaviours[i] as ActionBehaviour;
                if (behaviour.Active)
                    behaviour.OnLateUpdate(this);
            }
        }

        public void SetAnimClip(string clipName)
        {
            this.clipName = clipName;
            if (stateMachine.View == null)
                return;
            var clipNames = stateMachine.View.ClipNames;
            for (int i = 0; i < clipNames.Count; i++)
            {
                if (clipName == clipNames[i])
                {
                    clipIndex = i;
                    break;
                }
            }
        }
    }
}