///=====================================================
/// - FileName:      StateMechineSystem.cs
/// - NameSpace:     YukiFrameWork.States
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/2/13 17:13:31
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YukiFrameWork.States
{
    public class StateMechineSystem : AbstractSystem
    {
        internal const string StateInited = "StateInited";

        internal const string AddManager = "AddManager";

        internal const string RemoveManager = "RemoveManager";
    
        private FastList<StateManager> stateManagers = new FastList<StateManager>();

        public override void Init()
        {           
            MonoHelper.Update_AddListener(_ => Update());

            MonoHelper.FixedUpdate_AddListener(_ => FixedUpdate());

            MonoHelper.LateUpdate_AddListener(_ => LateUpdate());

            this.RegisterEvent<StateManager>(AddManager, AddStateManager);
            this.RegisterEvent<StateManager>(RemoveManager, RemoveStateManager);
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
                List<StateBase> list = new List<StateBase>();
                for (int i = 0; i < manager.stateMechine.states.Count; i++)
                {
                    if (manager.stateMechine.states[i].name.Equals(StateConst.entryState) || manager.stateMechine.states[i].index == -1)
                        continue;

                    list.Add(manager.stateMechine.states[i]);
                }

                manager.runTimeSubStatePair = manager.stateMechine.subStatesPair.ToDictionary(v => v.Key,v => v.Value);

                manager.runTimeSubStatePair.Add("BaseLayer", new SubStateData(list));

                foreach (var item in manager.stateMechine.transitions)
                {
                    StateTransition stateTransition = new StateTransition(manager, item,"BaseLayer");
                    manager.transitions.Add(stateTransition);
                }

                manager.subTransitions = manager.stateMechine.subTransitions.ToDictionary(v => v.Key, v => 
                {
                    List<StateTransition> transitions = new List<StateTransition>();

                    for (int i = 0; i < v.Value.Count; i++)
                    {
                        transitions.Add(new StateTransition(manager, v.Value[i],v.Key,true));
                    }

                    return transitions;
                });
                manager.subTransitions.Add("BaseLayer", manager.transitions);
                foreach (var state in manager.runTimeSubStatePair.Values)
                {
                    foreach (var item in state.stateBases)
                    {
                        item.OnInit(manager);
                    }
                }
                if (manager.deBugLog == DeBugLog.Open)
                {
                    Debug.Log($"状态机归属： {manager.gameObject.name},初始化完成！");
                }

                foreach (var item in manager.runTimeSubStatePair["BaseLayer"].stateBases)
                {
                    if (item.defaultState)
                    {
                        manager.OnChangeState(item);
                        break;
                    }
                }       
            });
        }

        private void Update()
        {           
            for (int i = 0; i < stateManagers.Count; i++)
            {
                for (int j = 0; j < stateManagers[i].currents.Count; j++)
                {
                    stateManagers[i].currents[j].OnUpdate();
                }
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < stateManagers.Count; i++)
            {
                for (int j = 0; j < stateManagers[i].currents.Count; j++)
                {
                    stateManagers[i].currents[j].OnFixedUpdate();
                }
            }
        }
        private void LateUpdate()
        {
            for (int i = 0; i < stateManagers.Count; i++)
            {
                for (int j = 0; j < stateManagers[i].currents.Count; j++)
                {
                    stateManagers[i].currents[j].OnLateUpdate();
                }
            }
        }

        internal void AddStateManager(StateManager stateManager) => stateManagers.Add(stateManager);
 
        internal void RemoveStateManager(StateManager stateManager) => stateManagers.Remove(stateManager);
    } 
}
