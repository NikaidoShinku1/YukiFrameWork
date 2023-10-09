using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace YukiFrameWork.States
{
    public enum AnimType
    {
        None,
        Animation,
        Animator
    }
    [Serializable]
    public class State : StateBase
    {
        [Header("动画选择")]
        public AnimType type;

        public Animator animator;

        public Animation animation;

        public bool IsTransition;

        //当状态播放完
        public bool IsEnter;

        public bool isNextState;

        public int nextStateID = 0;        
       
        public State() { }

        public State(Vector2 position)
        {
            this.rect = new Rect(position.x, position.y, 100, 40);    
            this.initRectPositionX = position.x;
            this.initRectPositionY = position.y;
        }       

        public void OnEnterState(Action action)
        {
            CheckAnim();

            foreach (StateBehaviour behaviour in stateBehaviours)
            {
                if (behaviour.IsActive)
                {                    
                    behaviour.OnEnter(action);
                }
            }
        }

        public void CheckAnim()
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
                            Debug.Log("The Animator is Empty");
                            animator = stateManager.GetComponentInChildren<Animator>();
                            if (animator == null)
                            {
                                throw new NullReferenceException("The Animation is Empty!");
                            }
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public void OnFixedUpdateState()
        {
            foreach (StateBehaviour behaviour in stateBehaviours)
            {
                if(behaviour.IsActive)
                behaviour.OnFixedUpdate();
            }
        }

        public void OnUpdateState()
        {            
            foreach (StateBehaviour behaviour in stateBehaviours)
            {
                if (behaviour.IsActive)
                    behaviour.OnUpdate();
            }
        }

        public void OnCheckState()
        {                    
            if (isNextState)
            {
                if (stateBehaviours == null || stateBehaviours.Count <= 0)
                {                  
                    //如果没有状态脚本直接完成状态
                    IsEnter = true;
                }
            }

            if (IsEnter)
            {       
                if(nextStateID != -1)
                stateMechine.OnChangeState(nextStateID);
                else OnExitState();
                IsEnter = false;
            }
        }

        public void OnExitState()
        {
            foreach (StateBehaviour behaviour in stateBehaviours)
            {
                if (behaviour.IsActive)
                    behaviour.OnExit();
            }
        }
    }
}
