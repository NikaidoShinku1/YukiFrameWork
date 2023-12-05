using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public enum InitType
    {
        Awake,      
        Start
    }

    public enum DeBugLog
    {
        关闭,
        开启      
    }

}
namespace YukiFrameWork.States
{
    public class StateManager : MonoBehaviour
    {
        #region 字段      
        public InitType initType;

        public DeBugLog deBugLog;
        
        public StateMechine stateMechine;      
        
        public StateBase CurrentState { get; set; } = null;

        internal Dictionary<string, StateParameterData> parametersDict = new Dictionary<string, StateParameterData>();

        internal List<StateTransition> transitions = new List<StateTransition>();

        private bool isDefaultTransition = false;

/*#if UNITY_EDITOR
        //是否绘制过渡
        public bool isDrawLine = false;
#endif*/

#endregion

        #region 方法

        private void Awake()
        {
            if (initType == InitType.Awake) Init();
        }

        private void Start()
        {
            if (initType == InitType.Start) Init();
        }

        private void Update()
        {
            CurrentState?.OnUpdate();

            if (CurrentState != null && transitions.Count > 0)
            {
                for (int i = 0; i < transitions.Count; i++)
                {
                    transitions[i].CheckConditionOrAnimIsMeet(CurrentState);
                    transitions[i].CheckConditionOrTimeIsMeet();
                }
            }
        }

        private void FixedUpdate()
        {
            CurrentState?.OnFixedUpdate();
        }

        private void LateUpdate()
        {
            CurrentState?.OnLateUpdate();
        }
     

        private void Init()
        {
            if (stateMechine == null)
            {
                stateMechine = transform.GetComponentInChildren<StateMechine>();
                if(stateMechine == null)
                return;
            }

            for (int i = 0; i < stateMechine.parameters.Count; i++)
            {
                if (parametersDict.ContainsKey(stateMechine.parameters[i].name))
                {
                    Debug.LogError("参数名称重复！" + stateMechine.parameters[i].name);
                    continue;
                }               
                parametersDict.Add(stateMechine.parameters[i].name, stateMechine.parameters[i]);          
            }

            foreach (var item in stateMechine.transitions)
            {
                StateTransition stateTransition = new StateTransition(this, item);
                transitions.Add(stateTransition);
            }

            foreach (var state in stateMechine.states)
            {
                state.OnInit(this);
            }

            foreach (var state in stateMechine.states)
            {
                if (state.defaultState)
                {
                    OnChangeState(state);
                    break;
                }
            }

            foreach (var item in transitions)
            {
                item.CheckConditionIsMeet();
            }

            if (deBugLog == DeBugLog.开启)
            {
                Debug.Log($"状态机归属： {gameObject.name},初始化完成！");
            }

        }

        public void SetBool(string name, bool v)
        {
/*#if UNITY_EDITOR
            isDrawLine = true;
#endif*/
            if (SetParameter(name,ParameterType.Bool, out var data))
            {
                CheckConditionInStateEnter();
                data.Value = v ? 1 : 0;                              
            }
            else
            {
                Debug.LogError("条件参数类型不一致或者没有此参数！参数名：" + name);
            }

        }

        public void SetFloat(string name, float v)
        {
/*#if UNITY_EDITOR
            isDrawLine = true;
#endif   */
            if (SetParameter(name,ParameterType.Bool, out var data))
            {
                CheckConditionInStateEnter();
                data.Value = v;            
            }
            else
            {
                Debug.LogError("条件参数类型不一致或者没有此参数！参数名：" + name);
            }
        }

        public void SetInt(string name, int v)
        {
/*#if UNITY_EDITOR
            isDrawLine = true;
#endif*/
            if (SetParameter(name,ParameterType.Bool, out var data))
            {
                CheckConditionInStateEnter();
                data.Value = v;              
            }
            else
            {
                Debug.LogError("条件参数类型不一致或者没有此参数！参数名：" + name);
            }
        }

        private bool SetParameter(string name,ParameterType type,out StateParameterData data)
        {
            if (parametersDict.TryGetValue(name, out var parameterData))
            {
                data = parameterData;             
                return true;
            }
            data = null;
            return false;
        }
        /// <summary>
        /// 在进入状态时如果使用有限状态机条件检查则调用该方法判断在刚进入这个状态时的条件是否已经满足
        /// </summary>
        private void CheckConditionInStateEnter()
        {
            if (!isDefaultTransition)
            {
                foreach (var transition in transitions)
                {
                    if (transition.TransitionMode == TransitionMode.有限条件模式)
                        foreach (var condition in transition.conditions)
                            condition.CheckParameterValueChange();
                    else
                        transition.CheckConditionOrAnimIsMeet(CurrentState);
                }
                isDefaultTransition = true;
            }
        }

        internal void OnChangeState(StateBase state, System.Action callBack = null, bool isBack = true)
        {   
            if (state == null) return;
            CurrentState?.OnExit(isBack);
            if (deBugLog == DeBugLog.开启)
            {
                Debug.Log($"状态切换: {(CurrentState != null ? CurrentState.name : "进入默认状态") } -> {state.name} 状态机归属： {gameObject.name}");
            }
            CurrentState = state;

            state?.OnEnter(callBack);
            isDefaultTransition = false;
        }

        internal void OnChangeState(int index, System.Action callBack = null, bool isBack = true)
        {
/*#if UNITY_EDITOR
            isDrawLine = false;
#endif*/
            StateBase stateBase = stateMechine.states.Find(x => x.index == index);
            OnChangeState(stateBase,callBack,isBack);
        }

        internal void OnChangeState(string name, System.Action callBack = null, bool isBack = true)
        {
/*#if UNITY_EDITOR
            isDrawLine = false;
#endif*/
            StateBase stateBase = stateMechine.states.Find(x => x.name == name);
            OnChangeState(stateBase,callBack,isBack);
        }

        #endregion
    }

}