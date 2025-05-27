///=====================================================
/// - FileName:      StateMachine.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/7 19:43:10
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

namespace YukiFrameWork.Machine
{
    /// <summary>
    /// 状态机
    /// </summary>
    public class StateMachine : IController
    {       
        public StateMachine Parent { get; internal set; }
        public StateMachineCore StateMachineCore { get; internal set; }

        public string Name { get; internal set; }

        /// <summary>
        /// 状态切换计数(为了避免状态切换进入死循环)
        /// </summary>
        private Dictionary<string, int> state_count = new Dictionary<string, int>();

        /// <summary>
        /// 在这个状态机层级下包含的所有子状态机
        /// </summary>
        private List<StateMachine> childs = new List<StateMachine>();
        /// <summary>
        /// 这个状态机下管理的所有的过渡
        /// </summary>
        private List<StateTransition> stateTransitions = new List<StateTransition>();
     
        /// <summary>
        /// 根据状态名称查找某一个状态运行基类
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public StateBase GetState(string name)
        {
            for (int i = 0; i < runtime_States.Count; i++)
            {
                var item = runtime_States[i];
                if (item.Name == name)
                    return item;
            }
            return null;
        }

        /// <summary>
        /// 运行时所有的状态
        /// </summary>
        private List<StateBase> runtime_States = new List<StateBase>();

        internal void AddTransition(StateTransition stateTransition)
        {          
            stateTransitions.Add(stateTransition);
        }

        internal void RemoveTransition(StateTransition stateTransition)
        {
            stateTransitions.Remove(stateTransition);
        }

        internal void AddChildMachine(StateMachine child)
        {
            childs.Add(child);
        }

        internal void RemoveChildMachine(StateMachine child)
        {
            childs.Remove(child);
        }

        /// <summary>
        /// 查找指定的子状态机
        /// </summary>
        /// <param name="machineName"></param>
        /// <returns></returns>
        public StateMachine GetChildMachine(string machineName)
        {
            foreach (var machine in childs)
            {
                if (machine.Name == machineName)
                    return machine;
            }

            return null;
        }


        internal void AddState(StateBase stateBase)
        {
            runtime_States.Add(stateBase);
        }

        internal void RemoveState(StateBase stateBase)
        {
            runtime_States.Remove(stateBase);
        }

        /// <summary>
        /// 当前运行的状态
        /// <para>Tip:CurrentState获取当前状态机正在运行的状态。GetCurrentStateInfo方法获取精确到子状态机的状态</para>
        /// </summary>
        public StateBase CurrentState { get; private set; }

        /// <summary>
        /// 当前运行的子状态机
        /// </summary>
        public StateMachine CurrentChildMachine { get; private set; }

        /// <summary>
        /// 这个状态机的默认状态
        /// </summary>
        public StateBase DefaultState { get; private set; }        

        IArchitecture IGetArchitecture.GetArchitecture()
        {
            return  StateMachineCore.GetArchitecture();
        }

        /// <summary>
        /// 获取当前正在运行的状态信息
        /// <para>Tip:CurrentState属性获取当前状态机正在运行的状态。GetCurrentStateInfo方法获取精确到子状态机的状态</para>
        /// </summary>
        /// <returns></returns>
        public StateBase GetCurrentStateInfo()
        {
            if (!CurrentState.Runtime_StateData.IsSubStateMachine)
                return CurrentState;
            var subMachine = GetChildMachine(CurrentState.Name);
            return subMachine.GetCurrentStateInfo();
        }    
        internal void Init()
        {
            for (int i = 0; i < runtime_States.Count; i++)
            {
                var item = runtime_States[i];
                item.Init();
                if (item.Runtime_StateData.IsDefaultState)
                    DefaultState = item;
            }
        }
        /// <summary>
        /// 最后一个执行的过渡
        /// <para>Tip:状态机通过切换默认状态是不会有当前过渡的。</para>
        /// </summary>
        public StateTransition CurrentTransition { get; private set; }  

        private float currentSeconds;

        /// <summary>
        /// 切换状态
        /// </summary>
        /// <param name="state"></param>
        /// <param name="transition"></param>
        internal void SwitchState(StateBase state,StateTransition transition)
        {
            void Switch()
            {              
                int second = Mathf.FloorToInt(Time.time);

                if (second != currentSeconds)
                {
                    currentSeconds = second;
                    state_count.Clear(); // 清空状态切换计数
                }               
                // 添加状态
                if (state != null)
                {
                    AddStateCount(state.Name);                    
                }

                this.CurrentTransition = transition;

                var current = CurrentState;
                if (CurrentState != null)
                {
                    CurrentState.NextState = state;
                    // 退出当前状态
                    ExitState();
                }
                // 进入状态
                if (state != null)
                {
                    state.LastState = current;
                    EnterState(state);
                    // 检测当前状态是否有已经满足的条件
                    CheckConditionAndSwitch();

                }             
            }
            Switch();           
        }        
        private void EnterState(StateBase stateBase)
        {
            if (stateBase == null) return;
            CurrentState = stateBase;
            CurrentState.Enter();
            CurrentState.LastState = null;
            if (CurrentState.Runtime_StateData.IsSubStateMachine)
            {
                // 如果当前运行的状态是子状态机则在进入该状态后将子状态机切换到默认状态
                var childMachine = GetChildMachine(CurrentState.Name);
                if (childMachine == null)
                {
                    throw new NullReferenceException($"状态已被标识为子状态机，但子状态机查询失败! State Name:{CurrentState.Name}");
                }               
                CurrentChildMachine = childMachine;
                //子状态机进入默认状态                       
                CurrentChildMachine.EnterState(childMachine.DefaultState);
                // 检测当前状态是否有已经满足的条件
                CurrentChildMachine.CheckConditionAndSwitch();

            }
            StateMachineCore.StateManager.onChangeState?.Invoke(StateMachineCore, this, CurrentState.Name);

        }

        private void ExitState()
        {
            if (CurrentState == null) return;
            if (CurrentState.Runtime_StateData.IsSubStateMachine)            
                CurrentChildMachine.ExitState();            
            CurrentState.Exit();
            CurrentState.NextState = null;
            CurrentState = null;
            CurrentChildMachine = null;
        }

        /// <summary>
        /// 遍历所有的过渡条件，当检查有一个过渡满足条件则触发切换状态
        /// </summary>
        private void CheckConditionAndSwitch()
        {
            foreach (var item in stateTransitions)
            {
                if (item.CheckConditionAndSwitch())
                    break;
            }
        }     

        /// <summary>
        /// 添加计数
        /// </summary>
        /// <param name="stateName"></param>
        private void AddStateCount(string stateName)
        {
            if (state_count.ContainsKey(stateName))
                state_count[stateName]++;
            else
                state_count.Add(stateName, 0);

            if (state_count[stateName] > 30)
            {
                StringBuilder stringBuilder = new StringBuilder();              
                stringBuilder
                    .Append("游戏物体:")
                    .Append(StateMachineCore.StateManager.gameObject.name)
                    .Append("状态机:")
                    .Append(this.StateMachineCore.RuntimeStateMachineCore.name)
                    .Append("层级:")
                    .Append(Name)
                    .Append("检测到状态:");
                foreach (var item in state_count.Keys)
                {
                    if (state_count[item] >= 29)
                    {
                        stringBuilder.Append(item).Append(",");
                    }
                }
                stringBuilder.Remove(stringBuilder.Length - 1, 1);
                stringBuilder.Append("之间频繁切换,请检查状态之间的过渡是否同时满足?请设置合理的状态切换条件!");
                state_count.Clear();
                throw new System.Exception(stringBuilder.ToString());
            }
        }
        internal void Update()
        {          
            CurrentState?.Update();
            CurrentChildMachine?.Update();
            
        }
        internal void LateUpdate()
        {           
            CurrentState?.LateUpdate();
            CurrentChildMachine?.LateUpdate();
        }

        internal void FixedUpdate()
        {          
            CurrentState?.FixedUpdate();
            CurrentChildMachine?.FixedUpdate();
        }
        
    }
}
