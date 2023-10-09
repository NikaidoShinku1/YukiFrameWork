using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;

namespace YukiFrameWork.States
{
    public class StateMechine : MonoBehaviour
    {      
        public int CurrentIndex { get; private set; } = -1;
        public State CurrentState { get; private set; }  
                       
        public List<State> states = new List<State>();      

        public State GetState(int index)
            => states.Find(x => x.index == index);

        public void AddState(State state)
        {
            states.Add(state);
            CheckStateCount(state);
        }

        public void RemoveState(int stateID)
        {
            states.Remove(states.Find(x => x.index == stateID));
        }
      
        /// <summary>
        /// 根据id改变状态
        /// </summary>
        /// <param name="index">状态id</param>
        /// <param name="action">状态退出回调，用于过渡</param>
        public void OnChangeState(int index, Action action = null)
        {
            if (CurrentState != null)
            {
                CurrentState.IsTransition = true;
                CurrentState.OnExitState();
                CurrentState.IsTransition = false;
            }
            var newState = states.Find(x => x.index == index);
            if (newState == null) return;
            newState.OnEnterState(action);
            CurrentState = newState;
            CurrentIndex = index;                     
        }

        
        public void OnChangeState(string name, Action action = null)
        {
            if (CurrentState != null)
            {
                CurrentState.IsTransition = true;
                CurrentState.OnExitState();
            }
            CurrentState.IsTransition = false;
            CurrentState = states.Find(x => x.name == name);
            CurrentIndex = CurrentState.index;

            CurrentState.OnEnterState(action);
        }       
       
        /// <summary>
        /// 检查并赋予状态id
        /// </summary>
        /// <param name="state">状态</param>
        private void CheckStateCount(State state)
        {
            if (states.Count > 0)
            {
                state.index = states.Count - 1;
            }
        }

        private void Update()
        {
            if(CurrentState != null && !CurrentState.IsTransition)
            {
                CurrentState.OnCheckState();
                CurrentState.OnUpdateState();
            }
        }

        private void FixedUpdate()
        {
            if (CurrentState != null && !CurrentState.IsTransition)
            {
                CurrentState.OnFixedUpdateState();
            }
        }

    }
}
