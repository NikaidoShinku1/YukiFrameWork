using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;
namespace YukiFrameWork.ActionStates
{
    /// <summary>
    /// 状态机
    /// </summary>
    [Serializable]
    public class StateMachineCore : IStateMachine
    {
        [SerializeField] private string _name;
        public string name { get => _name; set => _name = value; }
        public int Id { get; set; }
        [SerializeField] private StateMachineView _view;
        public StateMachineView View { get => _view; set => _view = value; }
        public Transform transform => _view.Parent != null ? _view.Parent : _view.transform;
     
        public UpdateStatus UpdateStatus { get ; set; }

        /// <summary>
        /// 默认状态ID
        /// </summary>
        public int defaulId;
        /// <summary>
        /// 当前运行的状态索引
        /// </summary>
		public int stateId;
        /// <summary>
        /// 切换的状态id
        /// </summary>
        internal int nextId;
        /// <summary>
        /// 切换下一个状态的动作索引
        /// </summary>
        internal int nextActionId;
        /// <summary>
        /// 所有状态
        /// </summary>
#if UNITY_2020_1_OR_NEWER
        [NonReorderable]
#endif
        public State[] states;
#if UNITY_EDITOR
        /// <summary>
        /// 选中的状态,可以多选
        /// </summary>
        public List<int> selectStates;
#endif
        /// <summary>
        /// 以状态ID取出状态对象
        /// </summary>
        /// <param name="stateID"></param>
        /// <returns></returns>
        public State this[int stateID] => states[stateID];
        /// <summary>
        /// 获取 或 设置 默认状态
        /// </summary>
        public State DefaultState
        {
            get
            {
                if (defaulId < states.Length)
                    return states[defaulId];
                return null;
            }
            set { defaulId = value.ID; }
        }
        /// <summary>
        /// 当前状态
        /// </summary>
		public State CurrState => states[stateId];
#if UNITY_EDITOR    
        /// <summary>
        /// 选择的状态
        /// </summary>
        public State SelectState
        {
            get
            {
                selectStates ??= new List<int>();
                if (selectStates.Count > 0)
                    return states[selectStates[0]];
                return null;
            }
            set
            {
                if (!selectStates.Contains(value.ID))
                    selectStates.Add(value.ID);
            }
        }
#endif
        public State[] States { get => states ??= new State[0]; set => states = value; }

        public int NextId { get => nextId; set => nextId = value; }
#if UNITY_EDITOR
        public List<int> SelectStates { get => selectStates ??= new List<int>(); set => selectStates = value; }
#endif
        public int StateId { get => stateId; set => stateId = value; }
        public IAnimationHandler Handler { get; set; }
        public IStateMachine Parent { get; set; }
        private bool isInitialize;

        /// <summary>
        /// 添加状态
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public State AddState(string name, params StateBehaviour[] behaviours)
        {
            var state = new State(this)
            {
                name = name,
#if UNITY_EDITOR
                rect = new Rect(5000, 5000, 150, 30)
#endif
            };
            state.AddComponent(behaviours);
            return state;
        }

        public void UpdateStates()
        {
            for (int i = 0; i < states.Length; i++)
            {
                int id = states[i].ID;
                foreach (var state1 in states)
                {
                    foreach (var transition in state1.transitions)
                    {
                        if (transition.currStateID == id)
                            transition.currStateID = i;
                        if (transition.nextStateID == id)
                            transition.nextStateID = i;
                    }
                    foreach (var behaviour in state1.behaviours)
                        if (behaviour.ID == id)
                            behaviour.ID = i;
                    foreach (var action in state1.actions)
                        foreach (var behaviour in action.behaviours)
                            if (behaviour.ID == id)
                                behaviour.ID = i;
                }
                states[i].ID = i;
            }
        }

        public void Init()
        {
            if (isInitialize)
                return;
            isInitialize = true;
            Handler.OnInit();
            if (states.Length == 0)
                return;
            foreach (var state in states)
                state.Init(this);
            if (Parent == null)
            {
                DefaultState.Enter(0);               
            }         
            stateId = defaulId;
            nextId = defaulId;
        }

        public void Execute(UpdateStatus updateStatus)
        {
            if (!isInitialize)
                return;
            if (states.Length == 0)
                return;
            if (stateId != nextId || IsEnterState)
            {
                //不允许在非Update调用时切换
                if (updateStatus != UpdateStatus.OnUpdate)
                    return;
                IsEnterState = false;
                var currIdTemo = stateId;
                var nextIdTemp = nextId; //防止进入或退出行为又执行了EnterNextState切换了状态
                stateId = nextId;
                states[currIdTemo].Exit();
                states[nextIdTemp].Enter(nextActionId);
                return; //有时候你调用Play时，并没有直接更新动画时间，而是下一帧才会更新动画时间，如果Play后直接执行下面的Update计算动画时间会导致鬼畜现象的问题
            }         
            CurrState.Update(updateStatus);
        }

        /// <summary>
        /// 当进入下一个状态, 你也可以立即进入当前播放的状态, 如果不想进入当前播放的状态, 使用StatusEntry方法
        /// </summary>
        /// <param name="nextStateIndex">下一个状态的ID</param>
		public void EnterNextState(int nextStateIndex, int actionId = 0) => ChangeState(nextStateIndex, actionId, true);

        /// <summary>
        /// 进入下一个状态, 如果状态正在播放就不做任何处理, 如果想让动作立即播放可以使用 OnEnterNextState 方法
        /// </summary>
        /// <param name="stateID"></param>
        public void StatusEntry(int stateID, int actionId = 0) => ChangeState(stateID, actionId);

        private bool IsEnterState;
        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="stateId"></param>
        /// <param name="force"></param>
        public void ChangeState(int stateId, int actionId = 0, bool force = false)
        {
            if (force)
            {
                IsEnterState = true;
                nextId = stateId;
                nextActionId = actionId;
            }
            else if (nextId != stateId)
            {
                nextId = stateId;
                nextActionId = actionId;
            }
        }

        /// <summary>
        /// 切换子状态
        /// </summary>
        /// <param name="stateId"></param>
        /// <param name="actionId"></param>
        public void ChangeChildState(int stateId, int actionId = 0)
        {
            states[stateId].Enter(actionId);
            
            nextId = this.stateId = stateId;
            nextActionId = actionId;
        }

        public void OnDestroy()
        {
            foreach (var state in states)
            {
                foreach (var behaviour in state.behaviours)
                    behaviour.OnDestroy();
                foreach (var transition in state.transitions)
                    foreach (var behaviour in transition.behaviours)
                        behaviour.OnDestroy();
                foreach (var action in state.actions)
                    foreach (var behaviour in action.behaviours)
                        behaviour.OnDestroy();
            }
        }

#if UNITY_EDITOR
        public void OnScriptReload(StateMachineView view)
        {
            view.stateMachines.Add(this);
            foreach (var state in States)
            {
                if (state.Type == StateType.SubStateMachine)
                {
                    state.subStateMachine.View = view;
                    state.subStateMachine.Parent = this;
                    state.subStateMachine.OnScriptReload(view);
                }
                state.stateMachine = this;
                for (int i = 0; i < state.behaviours.Length; i++)
                    ReloadBehaviour(ref state.behaviours, ref i, state.ID);
                foreach (var t in state.transitions)
                {
                    t.stateMachine = this;
                    for (int i = 0; i < t.behaviours.Length; i++)
                        ReloadBehaviour(ref t.behaviours, ref i, state.ID);
                }
                foreach (var a in state.actions)
                {
                    a.stateMachine = this;
                    for (int i = 0; i < a.behaviours.Length; i++)
                        ReloadBehaviour(ref a.behaviours, ref i, state.ID);
                }
            }
        }
        private void ReloadBehaviour(ref BehaviourBase[] behaviours, ref int i, int id)
        {
            var type = AssemblyHelper.GetType(behaviours[i].name);
            if (type == null)
            {
                ArrayExtend.RemoveAt(ref behaviours, i);
                if (i >= 0) i--;
                return;
            }
            //如果类型相等，说明已经初始化过了，默认是BehaviourBase类型
            //如果在运行中再次初始化会把正常游戏的初始化丢弃导致报错
            if (behaviours[i].GetType() == type && Application.isPlaying)
                return;
            var metadatas = new List<Metadata>(behaviours[i].Metadatas);
            var active = behaviours[i].Active;
            var show = behaviours[i].show;
            behaviours[i] = (BehaviourBase)Activator.CreateInstance(type);
            behaviours[i].Reload(type, metadatas);
            behaviours[i].ID = id;
            behaviours[i].Active = active;
            behaviours[i].show = show;
            behaviours[i].stateMachine = this;
        }
#endif
    }
}