using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace YukiFrameWork.States
{
    [Serializable]
    public class StateBehaviour 
    {
        public string name;

        public int index;

        public StateManager stateManager { get; set; }

        public StateMechine stateMechine => stateManager.stateMechine;

        ///当前状态
        public State state => stateMechine.states[index];

        public Transform transform => stateManager.transform;      

        protected Animator animator;

        protected Animation animation;

        public bool IsActive = true;

        public bool IsTransition { get; set; } = false;

        public void SetStateManager(StateManager stateManager,string name,int index)
        {
            if (this.stateManager == null)
                this.stateManager = stateManager;

            this.name = name;
            this.index = index;
            switch (state.type)
            {
                case AnimType.None:
                    break;
                case AnimType.Animation:
                    animation = state.animation;
                    break;
                case AnimType.Animator:
                    animator = state.animator;
                    break;
            }
        }

        public virtual void Init() { }

        public void OnChangeState(string name, Action action = null)
        {
            stateManager.stateMechine.OnChangeState(name, action);
        }

        public void OnChangeState(int id, Action action = null)
        {
            stateManager.stateMechine.OnChangeState(id, action);
        }

        public T GetComponent<T>() where T : Component
        {
            return transform.GetComponent<T>();
        }

        public T GetComponentInChildren<T>(string name) where T : Component
        {
            Transform[] trans = transform.GetComponentsInChildren<Transform>();
            for (int i = 0; i < trans.Length; i++)
            {
                if (trans[i].name == name)
                    return trans[i].GetComponent<T>();
            }
            return null;
        }

        public T GetComponentInChildren<T>() where T : Component
        {
            Transform[] trans = transform.GetComponentsInChildren<Transform>();
            for (int i = 0; i < trans.Length; i++)
            {
                if (trans[i].GetComponent<T>() != null)
                    return trans[i].GetComponent<T>();
            }
            return null;
        }
        /// <summary>
        /// 进入状态时
        /// </summary>
        public virtual void OnEnter()
        {           
            
        }

        /// <summary>
        /// 状态动作更新
        /// </summary>        
        public virtual void OnUpdate() { }

        public virtual void OnFixedUpdate() { }

        /// <summary>
        /// 退出状态
        /// </summary>
        public virtual void OnExit()
        {
            
        }
    }
}