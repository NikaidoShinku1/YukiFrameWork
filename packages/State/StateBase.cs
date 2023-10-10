using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace YukiFrameWork.States
{  
    [Serializable]
    public class StateBase
    {
        [Header("״̬��")]
        public string name;

        [Header("״̬�±�")]
        public int index;

        [Header("�Ƿ���Ҫʹ��״̬ӵ��Ĭ�϶���")]
        public bool isActiveNormalAnim;

        [Header("�°�״̬��״̬ͼ��")]
        public int stateIndex;

        [Header("״̬�±�(���ڱ�ʶĬ�϶�������)")]
        public int currentStateIndex;

        [Header("����ѡ��")]
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