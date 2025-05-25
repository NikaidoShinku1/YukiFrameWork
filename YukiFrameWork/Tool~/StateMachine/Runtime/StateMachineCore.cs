///=====================================================
/// - FileName:      StateMachineCore.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/7 19:42:49
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.Machine
{
    /// <summary>
    /// 状态机集合本体,每一个RuntimeStateMachineCore配置都可以转换成一个新的StateMachineCore本体，视为状态机集合本身
    /// </summary>
    public class StateMachineCore : IController
    {
        /// <summary>
        /// 这个状态机本体是否已经激活使用
        /// </summary>
        public bool IsActive { get; private set; }

        public StateManager StateManager => stateManager;

        /// <summary>
        /// 这个状态机集合下的所有状态机
        /// </summary>
        private Dictionary<string, StateMachine> runtime_Machines = new Dictionary<string, StateMachine>();
        /// <summary>
        /// 这个状态机集合下的所有参数
        /// </summary>
        private Dictionary<int, StateParameterData> runtime_Parameters = new Dictionary<int, StateParameterData>();
        /// <summary>
        /// 所有参数的默认值
        /// </summary>
        private Dictionary<int, float> runtime_default_Parameters = new Dictionary<int, float>();

        /// <summary>
        /// 触发Trigger的次数
        /// </summary>
        private Dictionary<int, int> trigger_count = new Dictionary<int, int>();

        public StateMachineCore(StateManager stateManager,RuntimeStateMachineCore runtimeStateMachineCore)
        {        
            this.stateManager = stateManager;
            this.runtimeStateMachineCore = runtimeStateMachineCore.Instantiate();
#if UNITY_EDITOR
            this.runtimeStateMachineCore.runtime_GUID = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(runtimeStateMachineCore));
#endif            
            Init();
        }

        private StateManager stateManager;
        private RuntimeStateMachineCore runtimeStateMachineCore;      
        public RuntimeStateMachineCore RuntimeStateMachineCore => runtimeStateMachineCore;
        /// <summary>
        /// 默认层的状态机
        /// </summary>
        private StateMachine baseMachine;

        public IArchitecture GetArchitecture()
        {
            return stateManager.GetArchitecture();
        }

        void Init()
        {
            foreach (var item in runtimeStateMachineCore.all_runtime_parameters)
            {
                int hash = StateManager.StringToHash(item.parameterName);
                runtime_Parameters.Add(hash,item);
                runtime_default_Parameters.Add(hash, item.Parameter.Value);
            }
            foreach (var item in runtimeStateMachineCore.all_runtime_States)
            {
                StateMachine stateMachine = new StateMachine();
                stateMachine.StateMachineCore = this;
                stateMachine.Name = item.Key;
                runtime_Machines.Add(item.Key, stateMachine);

            }
            foreach (var items in runtimeStateMachineCore.all_runtime_States.Values)
            {
                foreach (var item in items)
                {
                    if (runtime_Machines.TryGetValue(item.parentStateMachineName, out StateMachine stateMachine))
                    {
                        //如果这个状态同时是子状态机
                        if (item.IsSubStateMachine)
                        {
                            //找到状态机。赋值父状态机
                            if (runtime_Machines.TryGetValue(item.name, out StateMachine subMachine))
                            {
                                subMachine.Parent = stateMachine;
                                stateMachine.AddChildMachine(subMachine);
                            }
                        }
                        stateMachine.AddState(new StateBase(item,stateMachine));
                    }
                    
                }
            }
            foreach (var item in runtime_Machines.Values)
            {
                if (runtimeStateMachineCore.all_runtime_Transitions.ContainsKey(item.Name))
                {
                    var transitions = runtimeStateMachineCore.all_runtime_Transitions[item.Name];
                    var machine = item;
                    foreach (var transition in transitions)
                    {
                        item.AddTransition(new StateTransition(machine, transition));
                    }
                }              
                item.Init();
            }
            baseMachine = runtime_Machines[StateMachineConst.BaseLayer];
        }

        /// <summary>
        /// 启动状态机集合本体
        /// </summary>
        public void Start()
        {
            if (IsActive) return;
            IsActive = true;
            baseMachine.SwitchState(baseMachine.DefaultState,null);

            //状态机启动后触发的事件
            onStateMachineStarted?.Invoke();           
            onStateMachineStarted = null;
        }

        /// <summary>
        /// 当状态机启动时触发的事件
        /// </summary>
        public event Action onStateMachineStarted;

        /// <summary>
        /// 获取当前运行的状态信息
        /// </summary>
        /// <returns></returns>
        public StateBase GetCurrentStateInfo()
        {
            return baseMachine.GetCurrentStateInfo();
        }

        /// <summary>
        /// 获取指定状态机下的运行状态信息
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public StateBase GetCurrentMachineStateInfo(string name)
        {           
            if (runtime_Machines.TryGetValue(name, out var machine))
            {
                return machine.CurrentState;
            }
            return null;
        }

       
        internal void Update()
        {
            if (!IsActive) return;           
            baseMachine.Update();

            //更新所有的Trigger
            UpdateTrigger();
        }

        internal void FixedUpdate()
        {
            if (!IsActive) return;
            baseMachine.FixedUpdate();
        }

        internal void LateUpdate()
        {
            if (!IsActive) return;
            baseMachine.LateUpdate();
        }      

        public void Cancel()
        {
            IsActive = false;
            //切换到空状态     
            baseMachine.SwitchState(null, null);
            //走一趟退出队列的执行                 
            //重置所有的参数
            ResetParameters();
        }      

        /// <summary>
        /// 根据名称获取某一个参数信息
        /// </summary>
        /// <param name="parameterName"></param>
        /// <returns></returns>
        public StateParameterData GetStateParameterData(string parameterName)
        {
            int hash = StateManager.StringToHash(parameterName);
            runtime_Parameters.TryGetValue(hash, out var data);
            return data;
        }

        /// <summary>
        /// 根据名称获取某一个状态机。默认的状态机名称为Base Layer,所有的子状态机都可以通过该API获取到。子状态机也是状态机
        /// </summary>
        /// <param name="machineName"></param>
        /// <returns></returns>
        public StateMachine GetRuntimeMachine(string machineName)
        {
            runtime_Machines.TryGetValue(machineName, out var machine);
            return machine;
        }

        #region Setting Parameters
        public void SetBool(string name, bool value)
        {
            SetBool(StateManager.StringToHash(name), value);
        }

        public void SetBool(int nameToHash, bool value)
        {
            SetParamter(nameToHash, value ? 1 : 0, ParameterType.Bool);
        }

        public void SetFloat(string name, float value)
        {
            SetFloat(StateManager.StringToHash(name), value);
        }

        public void SetFloat(int nameToHash, float value)
        {
            SetParamter(nameToHash, value, ParameterType.Float);
        }

        public void SetInt(string name, int value)
        {
            SetInt(StateManager.StringToHash(name), value);
        }

        public void SetInt(int nameToHash, int value)
        {
            SetParamter(nameToHash, value, ParameterType.Int);
        }

        /// <summary>
        /// 设置参数
        /// </summary>
        /// <param name="name"></param>
        /// <param name="v"></param>
        /// <param name="type"></param>
        private void SetParamter(int name, float v, ParameterType type)
        {
            void SetParam()
            {
                if (IsContainParameter(name, type, out var parameter))
                {
                    parameter.Parameter.Value = v;
                }
                else
                {
                    Debug.LogWarning($"参数查询失败或者参数类型不正确! 参数名称:{StateManager.HashToString(name)} --- 参数类型:{type} ");
                }
            }
            if (!IsActive)
            {
                onStateMachineStarted += SetParam;
            }
            else SetParam();
            
        }

        public void SetTrigger(string name)
        {
            SetTrigger(StateManager.StringToHash(name));
        }

        public void SetTrigger(int nameToHash)
        {
            void SetParamTrigger()
            {
                if (IsContainParameter(nameToHash, ParameterType.Trigger, out StateParameterData parameter))
                {
                    if (parameter.parameterType != ParameterType.Trigger)
                        return;
                    if (parameter.Parameter.Value == 1)
                    {
                        if (trigger_count.ContainsKey(nameToHash))
                            trigger_count[nameToHash]++;
                        else
                            trigger_count.Add(nameToHash, 1);
                    }
                    else
                    {
                        parameter.Parameter.Value = 1;
                    }

                }
            }

            if (!IsActive)
            {
                onStateMachineStarted += SetParamTrigger;
            }
            else SetParamTrigger();
            
        }

        /// <summary>
        /// 当Trigger触发后还原trigger
        /// </summary>
        /// <param name="name"></param>
        internal void ClearTrigger(int nameToHash)
        {
            SetParamter(nameToHash, 0, ParameterType.Trigger);
        }

        public void ResetTrigger(string name)
        {
            ResetTrigger(StateManager.StringToHash(name));
        }

        public void ResetTrigger(int nameToHash)
        {
            if (trigger_count.ContainsKey(nameToHash))
                trigger_count[nameToHash] = 0;
            ClearTrigger(nameToHash);
        }
        #endregion
        #region Getter Parameter
        public bool GetBool(string name)
        {
            return GetBool(StateManager.StringToHash(name));
        }

        public bool GetBool(int nameToHash)
        {
            return GetParameter(nameToHash) == 1;
        }

        public float GetFloat(string name)
        {
            return GetFloat(StateManager.StringToHash(name));
        }

        public float GetFloat(int nameToHash)
        {
            return GetParameter(nameToHash);
        }

        public int GetInt(string name)
        {
            return GetInt(StateManager.StringToHash(name));
        }

        public int GetInt(int nameToHash)
        {
            return (int)GetParameter(nameToHash);
        }

        public bool GetTrigger(string name)
        {
            return GetTrigger(StateManager.StringToHash(name));
        }

        public bool GetTrigger(int nameToHash)
        {
            return GetTriggerCount(nameToHash) > 0 || GetParameter(nameToHash) == 1;
        }

        public int GetTriggerCount(string name)
        {
            return GetTriggerCount(StateManager.StringToHash(name));
        }

        public int GetTriggerCount(int nameToHash)
        {
            return trigger_count.ContainsKey(nameToHash) ? trigger_count[nameToHash] : 0;
        }

        public float GetParameter(int nameToHash)
        {
            if (runtime_Parameters.TryGetValue(nameToHash, out var data))
                return data.Parameter.Value;
            return 0;
        }
        #endregion
        #region Trigger
        //保存Trigger的触发次数
        private List<int> triggerKeys = new List<int>();

        private void UpdateTrigger()
        {
            if (trigger_count.Count == 0) return;          
            foreach (var item in trigger_count.Keys)
            {
                if (!triggerKeys.Contains(item))
                    triggerKeys.Add(item);
            }

            foreach (var item in triggerKeys)
            {
                if (!trigger_count.ContainsKey(item))
                    continue;
                if (trigger_count[item] > 0 && GetParameter(item) == 0)
                {
                    SetParamter(item, 1, ParameterType.Trigger);
                    trigger_count[item]--;
                    if (trigger_count[item] < 0)
                        trigger_count[item] = 0;
                }
            }
        }
        #endregion
        internal bool IsContainParameter(int nameToHash, ParameterType type,out StateParameterData data)
        {
             return runtime_Parameters.TryGetValue(nameToHash,out data) && data.parameterType == type;           
        }

        /// <summary>
        /// 重置所有的参数
        /// </summary>
        internal void ResetParameters()
        {
            foreach (var item in runtime_Parameters)
            {
                item.Value.Parameter.Value = runtime_default_Parameters[item.Key];
            }
        }

    }
}
