using UnityEngine;

namespace YukiFrameWork.ActionStates
{
    public class StateMachineSystem : SingletonMono<StateMachineSystem>
    {       
        public FastList<IStateMachine> stateMachines = new FastList<IStateMachine>();  

        private void Update()
        {
            for (int i = 0; i < stateMachines._size; i++)
            {
                stateMachines._items[i].Execute(UpdateStatus.OnUpdate);
            }
        }

        private void FixedUpdate()
        {
            for (int i = 0; i < stateMachines._size; i++)
            {              
                if ((stateMachines._items[i].UpdateStatus & UpdateStatus.OnFixedUpdate) != 0)
                    stateMachines._items[i].Execute(UpdateStatus.OnFixedUpdate);
            }
        }

        private void LateUpdate()
        {
            for (int i = 0; i < stateMachines._size; i++)
            {
                if ((stateMachines._items[i].UpdateStatus & UpdateStatus.OnLateUpdate) != 0)
                    stateMachines._items[i].Execute(UpdateStatus.OnLateUpdate);
            }
        }

        public static void AddStateMachine(IStateMachine stateMachine)
        {
            var i = Instance;
            if (i == null | stateMachine == null)
                return;
            i.stateMachines.Add(stateMachine);
        }

        public static void RemoveStateMachine(IStateMachine stateMachine)
        {
            var i = Instance;
            if (i == null | stateMachine == null)
                return;
            i.stateMachines.Remove(stateMachine);
        }
    }
}