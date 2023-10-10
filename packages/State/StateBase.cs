using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace YukiFrameWork.States
{  
    [Serializable]
    public class StateBase
    {
        [Header("状态名")]
        public string name;

        [Header("状态下标")]
        public int index;

        [Header("是否需要使该状态拥有默认动画")]
        public bool isActiveNormalAnim;

        [Header("新版状态机状态图层")]
        public int stateIndex;

        [Header("状态下标(用于标识默认动画剪辑)")]
        public int currentStateIndex;

        [Header("动画选择")]
        public AnimType type;

        public Animator animator;

        public Animation animation;

        public List<StateBehaviour> stateBehaviours = new List<StateBehaviour>();       

        public Rect rect;
     
        [HideInInspector]
        public float initRectPositionX;
        
        [HideInInspector]
        public float initRectPositionY;

        public StateMechine stateMechine => stateManager.stateMechine;

        public StateManager stateManager = null;

        public virtual void Init(StateManager stateManager)
        { 
            this.stateManager = stateManager;
            CheckAnimEmpty();
            for (int i = 0;i< stateBehaviours.Count;i++)
            {
                Type type = Type.GetType(stateBehaviours[i].name);
                int index = stateBehaviours[i].index;
                string name = stateBehaviours[i].name;
                stateBehaviours[i] = Activator.CreateInstance(type) as StateBehaviour;
                stateBehaviours[i].SetStateManager(stateManager,name,index);
                stateBehaviours[i].Init();
            }
        }

        public void CheckAnimEmpty()
        {
            switch (type)
            {
                case AnimType.None:
                    break;
                case AnimType.Animation:
                    if (animation == null)
                    {
                        animation = stateManager.GetComponent<Animation>();
                        if (animation == null)
                        {
                            animation = stateManager.GetComponentInChildren<Animation>();
                            if (animation == null)
                            {
                                throw new NullReferenceException("The Animation is Empty!");
                            }
                        }                       
                    }                   
                    break;
                case AnimType.Animator:
                    if (animator == null)
                    {
                        animator = stateManager.GetComponent<Animator>();
                        if (animator == null)
                        {
                            animator = stateManager.GetComponentInChildren<Animator>();
                            if (animator == null)
                            {
                                throw new NullReferenceException("The Animatior is Empty!");
                            }
                        }
                    }                  
                    break;
                default:
                    break;
            }
        }

        public void AddBehaviours(StateBehaviour behaviour)
        {
            stateBehaviours.Add(behaviour);
        }

        public void RemoveBehviours(string name)
        {
            stateBehaviours.Remove(stateBehaviours.Find(x => x.name == name));
        }
    }
}