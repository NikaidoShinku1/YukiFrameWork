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
        public bool IsTransition;

        //当状态播放完
        public bool IsEnter;

        public bool isNextState;

        public int nextStateID = 0;

        public string normalAnimClipName;

        public float animSpeed = 1;

        public float animLength = 100;
       
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
                    if (normalAnimClipName != string.Empty)
                    {
                        animation.Play(normalAnimClipName);
                        animation[normalAnimClipName].speed = animSpeed;                      
                    }
                    break;
                case AnimType.Animator:                                      
                    if (normalAnimClipName != string.Empty)
                    {                      
                        animator.Play(normalAnimClipName);
                        animator.speed = animSpeed;                       
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
                if (isActiveNormalAnim)
                {
                    if (normalAnimClipName != string.Empty)
                    {
                        switch (type)
                        {
                            case AnimType.None:
                                break;
                            case AnimType.Animation:
                                {
                                    float normalTime = animation[normalAnimClipName].normalizedTime;                                  
                                    if (!animation[normalAnimClipName].clip.isLooping && normalTime * 100 >= (animLength - 5 / animLength))
                                    {
                                        animation.Stop();
                                        IsEnter = true;
                                        Debug.Log(IsEnter);
                                    }
                                }
                                break;
                            case AnimType.Animator:
                                {                                    
                                    var info = animator.GetCurrentAnimatorStateInfo(0);

                                    if (!info.loop && info.normalizedTime * 100 >= animLength)
                                    {                                     
                                        IsEnter = true;
                                    }
                                }
                                break;
                        }
                    }
                    else IsEnter = true;
                }
                else
                {
                    if (stateBehaviours == null || stateBehaviours.Count <= 0)
                    {
                        //如果没有状态脚本直接完成状态
                        IsEnter = true;
                    }
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
