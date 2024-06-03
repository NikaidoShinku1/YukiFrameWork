using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
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
        [LabelText("开启")]   
        Open,
        [LabelText("关闭")]  
        Close      
    }

    public enum StateExtension
    {
        [LabelText("默认状态机")]
        None,
        [LabelText("兼容Playable")]
        Playable
    }

}
namespace YukiFrameWork.States
{
    internal class PlayableInfo
    {
        public int clipIndex;
        public AnimationClipPlayable clipPlayable;
      

        public PlayableInfo(int clipIndex,PlayableGraph playableGraph, AnimationClip clip)
        {
            this.clipIndex = clipIndex;
            this.clipPlayable = AnimationClipPlayable.Create(playableGraph, clip);
           
        }
    }
    public class StateManager : MonoBehaviour,IState
    {
        private const string defaultSystem = "设置";    
        #region 字段    
        [LabelText("状态机初始化方式:"),BoxGroup(defaultSystem)]
        [InfoBox("决定了状态机初始化的生命周期")]
        public RuntimeInitType initType;

        [LabelText("状态机是否开启调试:"), BoxGroup(defaultSystem)]
        [InfoBox("开启后每次切换状态都会Debug一次")]
        public DeBugLog deBugLog;

        [SerializeField,LabelText("状态机的默认类型"),BoxGroup(defaultSystem)]
        public StateExtension StateExtension;

        [SerializeField,LabelText("该状态机使用的动画状态机"),BoxGroup(defaultSystem),ShowIf(nameof(StateExtension),StateExtension.Playable)]       
        internal Animator bindAnimator;

        [LabelText("图名"),SerializeField,BoxGroup(defaultSystem), ShowIf(nameof(StateExtension), StateExtension.Playable)]
        internal string graphName;

       // [LabelText("状态剪辑绑定"),SerializeField, BoxGroup(defaultSystem), ShowIf(nameof(StateExtension), StateExtension.Playable)]
        internal Dictionary<string, PlayableInfo> stateOrAnimationDicts = new Dictionary<string, PlayableInfo>();   

#if UNITY_EDITOR
        [ShowIf("IsMechineOrEmpty")]
#endif
        [LabelText("状态机本体:"), BoxGroup(defaultSystem), PropertySpace(15)]
        public StateMechine stateMechine;                   

        public Dictionary<string,StateParameterData> ParametersDicts => parametersDict;

        private Dictionary<string, StateParameterData> parametersDict = DictionaryPools<string,StateParameterData>.Get();    
      
        public List<StateTransition> transitions = ListPools<StateTransition>.Get();

        public Dictionary<string, SubStateData> runTimeSubStatePair { get; set; } = new Dictionary<string, SubStateData>();

        public Dictionary<string, List<StateTransition>> subTransitions = DictionaryPools<string, List<StateTransition>>.Get();

        public List<StateBase> currents { get; } = new List<StateBase>();

        private PlayableGraph playableGraph;
        private AnimationPlayableOutput playableOutput;

        private AnimationLayerMixerPlayable baseMixerPlayable;

        internal Dictionary<string, int> state_switchCount = new Dictionary<string, int>();

        internal int currentSeconds;

#if UNITY_EDITOR
        private bool IsMechineOrEmpty => stateMechine != null;

        [Button("创建状态机",ButtonHeight = 40),BoxGroup(defaultSystem),HideIf(nameof(IsMechineOrEmpty)),PropertySpace(25)]
        private void CreateMechine()
        {
            StateMechine stateMechine = GetComponentInChildren<StateMechine>();

            if (stateMechine == null)
            {
                stateMechine = new GameObject(typeof(StateMechine).Name).AddComponent<StateMechine>();

                stateMechine.transform.SetParent(transform);

                StateNodeFactory.CreateStateNode(stateMechine, StateConst.entryState, new Rect(0, -100, StateConst.StateWith, StateConst.StateHeight));
            }
            this.stateMechine = stateMechine;
        }

        [Button("打开状态机编辑器", ButtonHeight = 40), BoxGroup(defaultSystem), ShowIf(nameof(IsMechineOrEmpty)), PropertySpace(25)]
        private void Open() => StateMechineEditorWindow.OpenWindow();

#endif

        #endregion

        #region 方法
        private void Awake()
        {
            if (initType == RuntimeInitType.Awake) Init();
        }     
        private void Start()
        {
            if (initType == RuntimeInitType.Start) Init();
        }

        private void Init()
        {
            if (stateMechine == null)
            {
                stateMechine = transform.GetComponentInChildren<StateMechine>();
                if (stateMechine == null)
                    return;
            }

            if (StateExtension == StateExtension.Playable)
            {
                bindAnimator ??= GetComponentInChildren<Animator>();
            }

            for (int i = 0; i < stateMechine.parameters.Count; i++)
            {
                if (ParametersDicts.ContainsKey(stateMechine.parameters[i].name))
                {
                    LogKit.E("参数名称重复！" + stateMechine.parameters[i].name);
                    continue;
                }
                ParametersDicts.Add(stateMechine.parameters[i].name, stateMechine.parameters[i]);
            }
            List<StateBase> list = ListPools<StateBase>.Get();
            for (int i = 0; i < stateMechine.states.Count; i++)
            {
                if (stateMechine.states[i].name.Equals(StateConst.entryState) || stateMechine.states[i].index == -1)
                    continue;
                list.Add(stateMechine.states[i]);
            }
            runTimeSubStatePair = stateMechine.subStatesPair.ToDictionary(v => v.Key, v => v.Value);
            runTimeSubStatePair.Add("BaseLayer", new SubStateData(list));

            foreach (var item in stateMechine.transitions)
            {
                StateTransition stateTransition = new StateTransition(this, item, "BaseLayer");
                transitions.Add(stateTransition);
            }

            subTransitions = stateMechine.subTransitions.ToDictionary(v => v.Key, v =>
            {
                List<StateTransition> transitions = new List<StateTransition>();

                for (int i = 0; i < v.Value.Count; i++)
                {
                    transitions.Add(new StateTransition(this, v.Value[i], v.Key, true));
                }

                return transitions;
            });
            subTransitions.Add("BaseLayer", transitions);     

            if (StateExtension == StateExtension.Playable)
            {
                playableGraph = PlayableGraph.Create($"{graphName}");
                playableOutput = AnimationPlayableOutput.Create(playableGraph, "StateMechine", bindAnimator);
                baseMixerPlayable = AnimationLayerMixerPlayable.Create(playableGraph);               
                playableOutput.SetSourcePlayable(baseMixerPlayable);
            }
            int inputCount = 0;
            foreach (var state in runTimeSubStatePair.Values)
            {
                foreach (var item in state.stateBases)
                {
                    item.OnInit(this);

                    if (StateExtension == StateExtension.Playable
                        && !item.name.StartsWith(StateConst.entryState)
                        && !item.name.StartsWith(StateConst.upState)
                        && item.statePlayble.animationClip != null)
                    {
                        inputCount++;
                        PlayableInfo info = new PlayableInfo(stateOrAnimationDicts.Count, playableGraph,item.statePlayble.animationClip);
                        stateOrAnimationDicts[item.name] = info;
                        item.clipPlayable = info.clipPlayable;
                    }
                }
            }

            if (StateExtension == StateExtension.Playable)
            {
                baseMixerPlayable.SetInputCount(inputCount);
                foreach (var key in stateOrAnimationDicts.Values)
                {
                    playableGraph.Connect(key.clipPlayable, 0, baseMixerPlayable, key.clipIndex);
                }

                playableGraph.Play();
            }      

            if (deBugLog == DeBugLog.Open)
            {
                LogKit.I($"状态机归属： {gameObject.name},初始化完成！");
            }            

            foreach (var item in runTimeSubStatePair["BaseLayer"].stateBases)
            {
                if (item.defaultState)
                {
                    OnChangeState(item);
                    break;
                }
            }
        }

        private void OnEnable()
        {
            StateMechineSystem.Instance.AddStateManager(this);
        }

        private void OnDisable()
        {
            StateMechineSystem.Instance.RemoveStateManager(this);
        }  

        public bool GetBool(string name)
        {
            if (SetParameter(name, ParameterType.Bool, out var data))
            {
                return data.Value == 1;
            }
            else
            {
                LogKit.E("条件参数类型不一致或者没有此参数！参数名：" + name);
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
                LogKit.E("条件参数类型不一致或者没有此参数！参数名：" + name);
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
                LogKit.E("条件参数类型不一致或者没有此参数！参数名：" + name);
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
                //CheckConditionInStateEnter();
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
                //CheckConditionInStateEnter();
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
                //CheckConditionInStateEnter();
                data.Value = v;              
            }
            else
            {
                LogKit.E("条件参数类型不一致或者没有此参数！参数名：" + name);
            }
        }

        public void SetTrigger(string name)
        {
            if (SetParameter(name, ParameterType.Trigger, out var data))
            {
                data.Value = 1;
            }
            else
            {
                LogKit.E("条件参数类型不一致或者没有此参数！参数名：" + name);
            }
        }

        public void ResetTrigger(string name)
        {                     
            if (SetParameter(name, ParameterType.Trigger, out var data))
            {
                data.Value = 0;
            }
            else
            {
                LogKit.E("条件参数类型不一致或者没有此参数！参数名：" + name);
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
            foreach (var transition in subTransitions)
            {
                foreach (var item in transition.Value)
                {
                    if (item.CheckConditionInStateEnter())
                    {                        
                        break;
                    }
                }              
            }
        }

        /// <summary>
        /// 可以对正在运行的状态进行重新进入的初始化操作
        /// </summary>
        /// <param name="stateName"></param>
        public void OnInitRuntimeState(string stateName)
        {
            StateBase state = currents.Find(x => x.name.Equals(stateName));
            if (state == null) return;
            state.OnEnter();
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
                LogKit.I($"状态切换: -> {state.name} 状态机归属： {gameObject.name}");              
            }
            runTimeSubStatePair[state.layerName].CurrentState = state;
            OnEnterState(state, callBack);
            MonoHelper.Start(DelayChange());                    
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
                throw new System.Exception(stringBuilder.ToString());
            }
        }

        private void OnEnterState(StateBase state,System.Action callBack)
        {
            currents.Add(state);
            state.OnEnter(callBack);
            if (state.PlayableCoroutine != null) MonoHelper.Stop(state.PlayableCoroutine);
            state.PlayableCoroutine = MonoHelper.Start(PlayableEnter(state));
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

        private IEnumerator PlayableEnter(StateBase stateBase)
        {
            bool IsGetInfo = StateExtension == StateExtension.Playable;
            if (!stateOrAnimationDicts.TryGetValue(stateBase.name, out var info))
            {
                IsGetInfo = false;
            }          
            if (IsGetInfo)
            {
                stateBase.clipPlayable.SetSpeed(1);
                stateBase.clipPlayable.SetTime(0);
            }
            float clipWidth = stateBase.statePlayble.clipWidth;
            float current = 0;
            float speed = 1 / stateBase.statePlayble.speed;

            while (current < clipWidth)
            {
                yield return CoroutineTool.WaitForFrame();
                current = Mathf.Clamp01(current + speed * Time.deltaTime);
                if (IsGetInfo)
                    Update_SourceWeight(baseMixerPlayable, info.clipIndex, current);
                stateBase.OnTransitionEnter(speed * Time.deltaTime,false);
            }
            if (IsGetInfo)
                Update_SourceWeight(baseMixerPlayable, info.clipIndex, current);
            stateBase.OnTransitionEnter(speed * Time.deltaTime,true);       
        }

        private IEnumerator PlayableExit(StateBase stateBase)
        {
            bool IsGetInfo = StateExtension == StateExtension.Playable;
            if (!stateOrAnimationDicts.TryGetValue(stateBase.name, out var info))
            {
                IsGetInfo = false;
            }
            if (IsGetInfo)
            {
                stateBase.clipPlayable.SetSpeed(0);
            }
            float clipWidth = stateBase.statePlayble.clipWidth;

            float speed = 1 / stateBase.statePlayble.speed;          
            while (clipWidth > 0)
            {
                yield return CoroutineTool.WaitForFrame();
                clipWidth = Mathf.Clamp01(clipWidth - speed * Time.deltaTime);
                if(IsGetInfo)
                    Update_SourceWeight(baseMixerPlayable, info.clipIndex, clipWidth);
                stateBase.OnTransitionExit(speed * Time.deltaTime,false);
            }
            if (IsGetInfo)
                Update_SourceWeight(baseMixerPlayable, info.clipIndex, clipWidth);
            stateBase.OnTransitionExit(speed * Time.deltaTime,true);           
           
        }

        private void Update_SourceWeight(AnimationLayerMixerPlayable animationMixer, int index, float clipWidth)
        {
            animationMixer.SetInputWeight(index, clipWidth);
        }

        private IEnumerator DelayChange()
        {
            yield return CoroutineTool.WaitForFrame();
            CheckConditionInStateEnter();
        }

        private void OnExitState(StateBase state, bool isBack)
        {            
            currents.Remove(state);
            state.OnExit(isBack);
            if (state.PlayableCoroutine != null) 
            { 
                MonoHelper.Stop(state.PlayableCoroutine); 

            }
            state.PlayableCoroutine = MonoHelper.Start(PlayableExit(state));
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

            if (runTimeSubStatePair["BaseLayer"].CurrentState?.index == index)
                return;
            StateBase stateBase = items.Find(x => x.index == index);
            OnChangeState(stateBase,callBack,isBack);
        }

        public void OnChangeState(string name,string layerName, System.Action callBack = null, bool isBack = true)
        {
            var root = runTimeSubStatePair[layerName];
            var items = root.stateBases;         

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
                LogKit.E("无法强制切换不同图层的状态!,需要切换的状态层级:" + layerName + "当前判定的状态:" + name);
                return;
            }

            OnChangeState(stateBase, callBack, isBack);         
        }
    
        #endregion

        private void OnDestroy()
        {            
            if(StateExtension == StateExtension.Playable)
                playableGraph.Destroy();
        }
    }

}