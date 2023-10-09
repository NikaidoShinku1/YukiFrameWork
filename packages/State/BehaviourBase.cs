using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEditor;
namespace YukiFrameWork.States
{ 
    [Serializable]
    public class BehaviourBase
    {
        public string name;

        public int ID;
        
        public StateManager stateManager { get; set; }

        public StateMechine stateMechine => stateManager.stateMechine;

        ///µ±Ç°×´Ì¬
        public State state => stateMechine.states[ID];

        public Transform transform => stateManager.transform;

        public Stack<Action> actionStack = new Stack<Action>();
       
        protected Animator animator;
       
        protected Animation animation;       

        public bool IsActive = true;

        public bool IsTransition { get; set; } = false;                                          

        public void SetStateManager(StateManager stateManager)
        {
            if(this.stateManager == null)
            this.stateManager = stateManager;            
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
    }
}