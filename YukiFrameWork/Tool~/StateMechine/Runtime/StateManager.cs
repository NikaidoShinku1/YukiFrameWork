using System.Collections;
using System.Collections.Generic;
using System.Text;
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
        [Label("开启")]   
        Open,
        [Label("关闭")]  
        Close      
    }

}
namespace YukiFrameWork.States
{
    public class StateManager : MonoBehaviour, IState,ISendEvent
    {
        #region 字段    
        [Label("状态机初始化方式:")]
        [HelperBox("决定了状态机初始化的生命周期")]
        public RuntimeInitType initType;

        [Label("状态机是否开启调试:")]
        [HelperBox("开启后每次切换状态都会Debug一次")]
        public DeBugLog deBugLog;

#if UNITY_EDITOR
        [EnableIf("IsMechineOrEmpty")]
#endif
        [Label("状态机本体:")]
        public StateMechine stateMechine;                   

        public Dictionary<string,StateParameterData> ParametersDicts => parametersDict;

        private Dictionary<string, StateParameterData> parametersDict = DictionaryPools<string,StateParameterData>.Get();    
      
        public List<StateTransition> transitions = ListPools<StateTransition>.Get();

        public Dictionary<string, SubStateData> runTimeSubStatePair { get; set; } = new Dictionary<string, SubStateData>();

        public Dictionary<string, List<StateTransition>> subTransitions = DictionaryPools<string, List<StateTransition>>.Get();

        private bool isDefaultTransition = false;

        public List<StateBase> currents { get; } = new List<StateBase>();

        private StateMechineSystem mechineSystem;

        internal Dictionary<string, int> state_switchCount = new Dictionary<string, int>();

        internal int currentSeconds;

#if UNITY_EDITOR
        private bool IsMechineOrEmpty => stateMechine != null;
#endif

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
            this.SendEvent(StateMechineSystem.AddManager,this);
        }

        private void OnDisable()
        {
            this.SendEvent(StateMechineSystem.RemoveManager,this);
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
            int second = Mathf.FloorToInt(Time.time);

            if (second != currentSeconds)
            {
                second = currentSeconds;
                state_switchCount.Clear();
            }
            if (state == null) return;

            AddStateChangeCount(state.name);

            if (runTimeSubStatePair[state.layerName].CurrentState != null)
            {
                OnExitState(runTimeSubStatePair[state.layerName].CurrentState, isBack);              
            }
            if (deBugLog == DeBugLog.Open)
            {
                Debug.Log($"状态切换: -> {state.name} 状态机归属： {gameObject.name}");              
            }
            runTimeSubStatePair[state.layerName].CurrentState = state;
            OnEnterState(state, callBack);
            MonoHelper.Start(DelayChange());         
            isDefaultTransition = false;
        }

        private void AddStateChangeCount(string name)
        {
            if (state_switchCount.ContainsKey(name))
                state_switchCount[name]++;
            else state_switchCount.Add(name, 0);

            if (state_switchCount[name] >= 30)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder
                    .Append("该游戏物体:").Append(gameObject.name).Append("中的状态机").Append("检测到状态:")
                    .Append("{").Append(name).Append("}")
                    .Append("切换超过多次，请检查条件是否设置合理，如果强制切换逻辑是否正确!");
                LogKit.Exception(stringBuilder);
            }
        }

        private void OnEnterState(StateBase state,System.Action callBack)
        {           
            currents.Add(state);
            state.OnEnter(callBack);
            if (state.IsSubingState)
            {
                foreach (var item in runTimeSubStatePair[state.name].stateBases)
                {
                    if (item.defaultState)
                    {
                        runTimeSubStatePair[state.name].CurrentState = item;
                        OnEnterState(item,callBack);
                        break;
                    }
                }
            }
        }

        private IEnumerator DelayChange()
        {
            yield return null;
            CheckConditionInStateEnter();
        }

        private void OnExitState(StateBase state, bool isBack)
        {            
            currents.Remove(state);
            state.OnExit(isBack);
            if (state.IsSubingState)
            {
                if (runTimeSubStatePair[state.name].CurrentState != null)
                {
                    OnExitState(runTimeSubStatePair[state.name].CurrentState, isBack);
                    runTimeSubStatePair[state.name].CurrentState = null;
                }
            }
        }

        public void OnChangeState(int index, System.Action callBack = null, bool isBack = true)
        {
            var items = runTimeSubStatePair["BaseLayer"].stateBases;

            StateBase stateBase = items.Find(x => x.index == index);
            OnChangeState(stateBase,callBack,isBack);
        }

        public void OnChangeState(string name,string layerName, System.Action callBack = null, bool isBack = true)
        {
            var items = runTimeSubStatePair[layerName].stateBases;

            StateBase stateBase = items.Find(x => x.name == name);

            ///如果是子状态机的父节点则特殊查找
            if (stateBase == null)
            {
                foreach (var item in currents)
                { 
                    if (runTimeSubStatePair.ContainsKey(item.name))
                        stateBase = runTimeSubStatePair[item.name].stateBases.Find(x => x.name == name);

                    if (stateBase != null) break;
                }
            }

            if (stateBase == null)
            {
                Debug.LogError("无法强制切换不同图层的状态!,需要切换的状态层级:" + layerName);
                return;
            }

            OnChangeState(stateBase, callBack, isBack);         
        }    

        public IArchitecture GetArchitecture()
        {
            return StateModule.Global;
        }
        #endregion

    }

}