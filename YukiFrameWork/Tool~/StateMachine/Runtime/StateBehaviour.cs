///=====================================================
/// - FileName:      StateBehaviour.cs
/// - NameSpace:     YukiFrameWork.StateMachine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/7 15:39:01
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Machine
{
    public abstract class StateBehaviour : IController
    {
        /// <summary>
        /// 这个状态所属的状态机
        /// </summary>
        public StateMachine StateMachine => CurrentStateInfo.StateMachine;

        /// <summary>
        /// 这个状态所在的Transform
        /// </summary>
        public Transform transform => StateMachine.StateMachineCore.StateManager.transform;

        /// <summary>
        /// 这个状态的状态信息
        /// </summary>
        public StateBase CurrentStateInfo { get; internal set; }         
        
        /// <summary>
        /// 这个状态类的真实类型缓存
        /// </summary>
        public Type Type { get; internal set; }
        
        /// <summary>
        /// 当状态初始化
        /// </summary>
        public virtual void OnInit() { }

        /// <summary>
        /// 当状态进入
        /// </summary>
        public virtual void OnEnter() { }

        /// <summary>
        /// 当状态退出
        /// </summary>
        public virtual void OnExit() { }

        /// <summary>
        /// 当状态更新
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// 当状态间隔更新
        /// </summary>
        public virtual void OnFixedUpdate() { }

        /// <summary>
        /// 当状态晚于更新
        /// </summary>
        public virtual void OnLateUpdate() { }

        public float GetFloat(string name)
            => StateMachine.StateMachineCore.GetFloat(name);
        public int GetInt(string name)
            => StateMachine.StateMachineCore.GetInt(name);
        public bool GetBool(string name)
            => StateMachine.StateMachineCore.GetBool(name);
        public bool GetTrigger(string name)
            => StateMachine.StateMachineCore.GetTrigger(name);


        public void SetFloat(string name,float v)
            => StateMachine.StateMachineCore.SetFloat(name, v);
        public void SetInt(string name,int v)
            => StateMachine.StateMachineCore.SetInt(name, v);
        public void SetBool(string name,bool v)
            => StateMachine.StateMachineCore.SetBool(name,v);
        public void SetTrigger(string name)
            => StateMachine.StateMachineCore.SetTrigger(name);

        IArchitecture IGetArchitecture.GetArchitecture()
        {
            return StateMachine.StateMachineCore.GetArchitecture();
        }
    }
}
