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
    public class StateMechineSystem : SingletonMono<StateMechineSystem>
    {      
        [SerializeField]private FastList<StateManager> stateManagers = new FastList<StateManager>();      
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
