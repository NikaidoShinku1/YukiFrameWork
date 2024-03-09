///=====================================================
/// - FileName:      StateMechineSystem.cs
/// - NameSpace:     YukiFrameWork.States
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/2/13 17:13:31
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
namespace YukiFrameWork.States
{
    public class StateMechineSystem : AbstractSystem
    {
        public const string StateInited = "StateInited";
    
        private FastList<StateManager> stateManagers = new FastList<StateManager>();

        public override void Init()
        {           
            MonoHelper.Update_AddListener(_ => Update());

            MonoHelper.FixedUpdate_AddListener(_ => FixedUpdate());

            MonoHelper.LateUpdate_AddListener(_ => LateUpdate());

            this.RegisterEvent<StateManager>(StateInited,manager =>
            {
                if (manager.stateMechine == null)
                {
                    manager.stateMechine = manager.transform.GetComponentInChildren<StateMechine>();
                    if (manager.stateMechine == null)
                        return;
                }

                for (int i = 0; i < manager.stateMechine.parameters.Count; i++)
                {
                    if (manager.ParametersDicts.ContainsKey(manager.stateMechine.parameters[i].name))
                    {
                        Debug.LogError("参数名称重复！" + manager.stateMechine.parameters[i].name);
                        continue;
                    }
                    manager.ParametersDicts.Add(manager.stateMechine.parameters[i].name, manager.stateMechine.parameters[i]);
                }

                for (int i = 0; i < manager.stateMechine.states.Count; i++)
                {
                    if (manager.stateMechine.states[i].name.Equals(StateConst.entryState) || manager.stateMechine.states[i].index == -1)
                        continue;
                    manager.runTimeStatesDict.Add(manager.stateMechine.states[i].index, manager.stateMechine.states[i]);
                }

                foreach (var item in manager.stateMechine.transitions)
                {
                    StateTransition stateTransition = new StateTransition(manager, item);
                    manager.transitions.Add(stateTransition);
                }

                foreach (var state in manager.runTimeStatesDict.Values)
                {
                    state.OnInit(manager);
                }

                if (manager.deBugLog == DeBugLog.Open)
                {
                    Debug.Log($"状态机归属： {manager.gameObject.name},初始化完成！");
                }

                foreach (var state in manager.runTimeStatesDict.Values)
                {
                    if (state.defaultState)
                    {
                        manager.OnChangeState(state);
                        break;
                    }
                }

                foreach (var item in manager.transitions)
                {
                    item.CheckConditionIsMeet();
                }
            });
        }          

        private void Update()
        {           
            for (int i = 0; i < stateManagers.Count; i++)
            {
                stateManagers[i].CurrentState?.OnUpdate();
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < stateManagers.Count; i++)
            {
                stateManagers[i].CurrentState?.OnFixedUpdate();
            }
        }
        private void LateUpdate()
        {
            for (int i = 0; i < stateManagers.Count; i++)
            {
                stateManagers[i].CurrentState?.OnLateUpdate();
            }
        }

        public void AddStateManager(StateManager stateManager) => stateManagers.Add(stateManager);
 
        public void RemoveStateManager(StateManager stateManager) => stateManagers.Remove(stateManager);
    } 
}
