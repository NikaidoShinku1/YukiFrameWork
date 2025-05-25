///=====================================================
/// - FileName:      StateBase.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/7 15:36:20
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using YukiFrameWork.Extension;

namespace YukiFrameWork.Machine
{
    public enum StateExecuteType
    {
        Init,
        Enter,
        Update,
        FixedUpdate,
        LateUpdate,
        Exit
    }
    /// <summary>
    /// 运行时状态类
    /// </summary>
    [Serializable]
    public class StateBase 
    {
        private List<StateBehaviour> runtime_StateBehaviours;
        private StateNodeData runtime_StateData;
        public StateNodeData Runtime_StateData => runtime_StateData;
        private StateMachine stateMachine;       
        public StateMachine StateMachine => stateMachine;

        public string Name => Runtime_StateData.name;

        /// <summary>
        /// 仅当状态退出时有值
        /// <para>Value:在这个状态退出后进入的下一个状态</para>
        /// </summary>
        public StateBase NextState { get; internal set; }

        /// <summary>
        /// 仅当状态进入时有值
        /// <para>Value:上一个退出的状态</para>
        /// </summary>
        public StateBase LastState { get; internal set; }

        public StateBase(StateNodeData runtime_StateData,StateMachine stateMachine)
        {
            this.runtime_StateData = runtime_StateData;
            this.stateMachine = stateMachine;
            runtime_StateBehaviours = new List<StateBehaviour>();
            CreateBehaviours();
        }

        private void CreateBehaviours()
        {
            var infos = runtime_StateData.behaviourInfos;
            foreach (var info in infos) 
            {
                Type type = AssemblyHelper.GetType(info.typeName);
                if (type == null)
                    throw new NullReferenceException($"状态类型错误，无法转换Type:{info.typeName}");

                StateBehaviour stateBehaviour = type.CreateInstance() as StateBehaviour;

                stateBehaviour.CurrentStateInfo = this;
                stateBehaviour.Type = type;
                runtime_StateBehaviours.Add(stateBehaviour);
            }
        }

        public StateBehaviour[] GetAllBehaviours()
            => runtime_StateBehaviours.ToArray();

        public T GetBehaviour<T>() where T : StateBehaviour
        {
            for (int i = 0; i < runtime_StateBehaviours.Count; i++)
            {
                var item = runtime_StateBehaviours[i];
                if (item.Type == typeof(T))
                    return item as T;
            }

            return null;
        }
        #region 生命周期
        internal void Init() => Execute(StateExecuteType.Init);  
        internal void Enter() => Execute(StateExecuteType.Enter);
        internal void Update() => Execute(StateExecuteType.Update);
        internal void FixedUpdate() => Execute(StateExecuteType.FixedUpdate);
        internal void LateUpdate() => Execute(StateExecuteType.LateUpdate);
        internal void Exit() => Execute(StateExecuteType.Exit);
        #endregion

        private void Execute(StateExecuteType stateExecuteType)
        {
            for (int i = 0; i < runtime_StateBehaviours.Count; i++)
            {
                var item = runtime_StateBehaviours[i];                           
                switch (stateExecuteType)
                {
                    case StateExecuteType.Init:
                        item.OnInit();
                        break;
                    case StateExecuteType.Enter:
                        item.OnEnter();
                        break;
                    case StateExecuteType.Update:
                        item.OnUpdate();
                        break;
                    case StateExecuteType.FixedUpdate:
                        item.OnFixedUpdate();
                        break;
                    case StateExecuteType.LateUpdate:
                        item.OnLateUpdate();
                        break;
                    case StateExecuteType.Exit:
                        item.OnExit();
                        break;
                }
            }
        }

    }
}
