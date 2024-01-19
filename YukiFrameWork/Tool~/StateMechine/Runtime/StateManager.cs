using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;
using YukiFrameWork.Pools;

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

        internal Dictionary<string, StateParameterData> parametersDict = DictionaryPools<string,StateParameterData>.Get();

        internal readonly Dictionary<int, StateBase> runTimeStatesDict = DictionaryPools<int, StateBase>.Get();

        internal List<StateTransition> transitions = ListPools<StateTransition>.Get();

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

            for (int i = 0; i < stateMechine.states.Count; i++)
            {
                if (stateMechine.states[i].name.Equals(StateConst.entryState) || stateMechine.states[i].index == -1)
                    continue;
                runTimeStatesDict.Add(stateMechine.states[i].index, stateMechine.states[i]);
            }

            foreach (var item in stateMechine.transitions)
            {
                StateTransition stateTransition = new StateTransition(this, item);
                transitions.Add(stateTransition);
            }

            foreach (var state in runTimeStatesDict.Values)
            {
                state.OnInit(this);
            }

            if (deBugLog == DeBugLog.开启)
            {
                Debug.Log($"状态机归属： {gameObject.name},初始化完成！");
            }

            foreach (var state in runTimeStatesDict.Values)
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
        }

        public bool GetBool(string name)
        {
            if (SetParameter(name, ParameterType.Bool, out var data))
            {
                return data.Value == 1;
            }
            else
            {
                Debug.LogError("条件参数类型不一致或者没有此参数！参数名：" + name);
                return false;
            }
        }

        public int GetInt(string name)
        {
            if (SetParameter(name, ParameterType.Int, out var data))
            {
                return (int)data.Value;
            }
            else
            {
                Debug.LogError("条件参数类型不一致或者没有此参数！参数名：" + name);
                return 0;
            }
        }

        public float GetFloat(string name)
        {
            if (SetParameter(name, ParameterType.Float, out var data))
            {
                return data.Value;
            }
            else
            {
                Debug.LogError("条件参数类型不一致或者没有此参数！参数名：" + name);
                return 0f;
            }
        }
        [MethodAPI("状态切换条件(bool)")]
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
        [MethodAPI("状态切换条件(Float)")]
        public void SetFloat(string name, float v)
        {
/*#if UNITY_EDITOR
            isDrawLine = true;
#endif   */
            if (SetParameter(name,ParameterType.Float, out var data))
            {
                CheckConditionInStateEnter();
                data.Value = v;            
            }
            else
            {
                Debug.LogError("条件参数类型不一致或者没有此参数！参数名：" + name);
            }
        }
        [MethodAPI("状态切换条件(Int)")]
        public void SetInt(string name, int v)
        {
/*#if UNITY_EDITOR
            isDrawLine = true;
#endif*/
            if (SetParameter(name,ParameterType.Int, out var data))
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
                if (parameterData.parameterType.Equals(type))
                {
                    data = parameterData;
                    return true;
                }
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
            runTimeStatesDict.TryGetValue(index,out var stateBase);
            OnChangeState(stateBase,callBack,isBack);
        }

        internal void OnChangeState(string name, System.Action callBack = null, bool isBack = true)
        {
            /*#if UNITY_EDITOR
                        isDrawLine = false;
            #endif*/
            StateBase stateBase = null;
            foreach (var s in runTimeStatesDict.Values)
            {
                if (s.name == name)
                {
                    stateBase = s;
                    break;
                }
            }

            OnChangeState(stateBase,callBack,isBack);
        }

        #endregion

        public void OnTriggerEnter(Collider other)
        {
            CurrentState?.OnTriggerEnter(other);
        }

        public void OnTriggerStay(Collider other)
        {
            CurrentState?.OnTriggerStay(other);
        }

        public void OnTriggerExit(Collider other)
        {
            CurrentState?.OnTriggerExit(other);
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            CurrentState?.OnTriggerEnter2D(collision);
        }

        public void OnTriggerExit2D(Collider2D collision)
        {
            CurrentState?.OnTriggerExit2D(collision);
        }

        public void OnTriggerStay2D(Collider2D collision)
        {
            CurrentState?.OnTriggerStay2D(collision);
        }

        public void OnCollisionEnter(Collision collision)
        {
            CurrentState?.OnCollisionEnter(collision);
        }

        public void OnCollisionStay(Collision collision)
        {
            CurrentState?.OnCollisionStay(collision);
        }

        public void OnCollisionExit(Collision collision)
        {
            CurrentState?.OnCollisionExit(collision);
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            CurrentState?.OnCollisionEnter2D(collision);
        }

        public void OnCollisionStay2D(Collision2D collision)
        {
            CurrentState?.OnCollisionStay2D(collision);
        }

        public void OnCollisionExit2D(Collision2D collision)
        {
            CurrentState?.OnCollisionExit2D(collision);
        }

        public void OnMouseDown()
        {
            CurrentState?.OnMouseDown();
        }

        public void OnMouseDrag()
        {
            CurrentState?.OnMouseDrag();
        }

        public void OnMouseEnter()
        {
            CurrentState?.OnMouseEnter();
        }

        public void OnMouseExit()
        {
            CurrentState?.OnMouseExit();
        }

        public void OnMouseUp()
        {
            CurrentState?.OnMouseUp();
        }

        public void OnMouseOver()
        {
            CurrentState?.OnMouseOver();
        }

        public void OnValidate()
        {
            CurrentState?.OnValidate();
        }
    }

}