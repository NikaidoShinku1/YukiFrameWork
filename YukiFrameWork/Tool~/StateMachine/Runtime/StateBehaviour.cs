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
        /// 仅当状态退出时有值
        /// <para>Value:在这个状态退出后进入的下一个状态</para>
        /// </summary>
        public StateBase NextState => CurrentStateInfo.NextState;

        /// <summary>
        /// 仅当状态进入时有值 当状态为启动时默认状态，则为空
        /// <para>Value:上一个退出的状态</para>
        /// </summary>
        public StateBase LastState => CurrentStateInfo.LastState;
        
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
        #region Parameter
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


        public float GetFloat(int nameToHash)
            => StateMachine.StateMachineCore.GetFloat(nameToHash);
        public int GetInt(int nameToHash)
            => StateMachine.StateMachineCore.GetInt(nameToHash);
        public bool GetBool(int nameToHash)
            => StateMachine.StateMachineCore.GetBool(nameToHash);
        public bool GetTrigger(int nameToHash)
            => StateMachine.StateMachineCore.GetTrigger(nameToHash);


        public void SetFloat(int nameToHash, float v)
            => StateMachine.StateMachineCore.SetFloat(nameToHash, v);
        public void SetInt(int nameToHash, int v)
            => StateMachine.StateMachineCore.SetInt(nameToHash, v);
        public void SetBool(int nameToHash, bool v)
            => StateMachine.StateMachineCore.SetBool(nameToHash, v);
        public void SetTrigger(int nameToHash)
            => StateMachine.StateMachineCore.SetTrigger(nameToHash);

        public void ResetTrigger(string name)
            => StateMachine.StateMachineCore.ResetTrigger(name);

        public void ResetTrigger(int nameToHash)
            => StateMachine.StateMachineCore.ResetTrigger(nameToHash);
         
        #endregion
        IArchitecture IGetArchitecture.GetArchitecture()
        {
            return StateMachine.StateMachineCore.GetArchitecture();
        }
    }
}
