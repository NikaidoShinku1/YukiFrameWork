using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Extension;
using YukiFrameWork.Pools;

namespace YukiFrameWork
{
    public enum RuntimeInitType
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
    public class StateManager : MonoBehaviour, IState,ISendEvent
    {
        #region 字段      
        public RuntimeInitType initType;

        public DeBugLog deBugLog;
     
        public StateMechine stateMechine;      
        
        public StateBase CurrentState { get; set; } = null;        

        public Dictionary<string,StateParameterData> ParametersDicts => parametersDict;

        private Dictionary<string, StateParameterData> parametersDict = DictionaryPools<string,StateParameterData>.Get();

        public Dictionary<int, StateBase> runTimeStatesDict { get; } = DictionaryPools<int, StateBase>.Get();

        public List<StateTransition> transitions = ListPools<StateTransition>.Get();

        private bool isDefaultTransition = false;

/*#if UNITY_EDITOR
        //是否绘制过渡
        public bool isDrawLine = false;
#endif*/

#endregion

        #region 方法
        private void Awake()
        {
            if (initType == RuntimeInitType.Awake) this.SendEvent(StateMechineSystem.StateInited, this);
        }

        private void Start()
        {
            if (initType == RuntimeInitType.Start) this.SendEvent(StateMechineSystem.StateInited, this);
        }

        private void OnEnable()
        {
            this.GetSystem<StateMechineSystem>().AddStateManager(this);
        }

        private void OnDisable()
        {
            this.GetSystem<StateMechineSystem>().RemoveStateManager(this);
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
                    foreach (var condition in transition.conditions)
                        condition.CheckParameterValueChange();
                }
                isDefaultTransition = true;
            }
        }

        public void OnChangeState(StateBase state, System.Action callBack = null, bool isBack = true)
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

        public void OnChangeState(int index, System.Action callBack = null, bool isBack = true)
        {
            /*#if UNITY_EDITOR
                        isDrawLine = false;
            #endif*/
            runTimeStatesDict.TryGetValue(index,out var stateBase);
            OnChangeState(stateBase,callBack,isBack);
        }

        public void OnChangeState(string name, System.Action callBack = null, bool isBack = true)
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

        public IArchitecture GetArchitecture()
        {
            return StateModule.Global;
        }

        #endregion

    }

}