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

        public virtual void OnInit() { }

        /// <summary>
        /// 根据状态名称切换状态
        /// </summary>
        /// <param name="index">状态名称</param>
        /// <param name="action">回调，在切换状态后保存在下一个状态内，直到下一个状态退出时触发</param>
        public void OnChangeState(string name, Action action = null)
        {
            stateManager.stateMechine.OnChangeState(name, action);
        }

        /// <summary>
        /// 根据状态标识切换状态
        /// </summary>
        /// <param name="index">状态标识</param>
        /// <param name="action">回调，在切换状态后保存在下一个状态内，直到下一个状态退出时触发</param>
        public void OnChangeState(int index, Action action = null)
        {
            stateManager.stateMechine.OnChangeState(index, action);
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