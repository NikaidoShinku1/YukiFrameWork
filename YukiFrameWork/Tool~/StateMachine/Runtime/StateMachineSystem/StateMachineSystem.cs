///=====================================================
/// - FileName:      StateMachineSystem.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   框架自定ViewController
/// - Creation Time: 2025/3/15 18:07:24
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
namespace YukiFrameWork.Machine
{
	public partial class StateMachineSystem : SingletonMono<StateMachineSystem>
	{
		private List<StateManager> runtime_All_StateManagers = new List<StateManager>();

		public void AddStateManager(StateManager stateManager)
		{
			runtime_All_StateManagers.Add(stateManager);
		}

		public void RemoveStateManager(StateManager stateManager)
		{
			runtime_All_StateManagers.Remove(stateManager);
		}

        private void Update()
        {
            for (int i = 0; i < runtime_All_StateManagers.Count; i++)
            {
                var stateManager = runtime_All_StateManagers[i];
                if (!stateManager) continue;

                foreach (var core in stateManager)
                {
                    if (!core.IsActive)
                        core.Start();

                    core.Update();
                }
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < runtime_All_StateManagers.Count; i++)
            {
                var stateManager = runtime_All_StateManagers[i];
                if (!stateManager) continue;

                foreach (var core in stateManager)
                {
                    core.FixedUpdate();
                }
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < runtime_All_StateManagers.Count; i++)
            {
                var stateManager = runtime_All_StateManagers[i];
                if (!stateManager) continue;

                foreach (var core in stateManager)
                {
                    core.LateUpdate();
                }
            }
        }
    }
}
