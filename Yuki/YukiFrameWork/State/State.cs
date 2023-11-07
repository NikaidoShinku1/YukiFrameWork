using UnityEngine;
using System;
using YukiFrameWork.Events;

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

        private bool isWaitAnim;
       
        public State() { }

        public State(Vector2 position)
        {
            this.rect = new Rect(position.x, position.y, 150, 40);     
            this.initRectPositionX = position.x;
            this.initRectPositionY = position.y;
        }       

        public void OnEnterState(Action action)
        {
            if (stateManager.IsDebugLog)
            {
                Debug.Log($"进入{name}状态，状态机归属：{stateManager.gameObject.name}");

            }
            actionStack.Push(action);
            CheckAnim();         
            foreach (StateBehaviour behaviour in stateBehaviours)
            {               
                if (behaviour.IsActive)
                {                    
                    behaviour.OnEnter();
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
                        animator.Play(normalAnimClipName,stateIndex,0);
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
                                    CheckOnAnimation();
                                }
                                break;
                            case AnimType.Animator:
                                {
                                    if (!isWaitAnim)
                                    {
                                        ActionKit.NextFrame(() =>
                                        {
                                            CheckOnAnimator();
                                            isWaitAnim = true;
                                        });                                      
                                    }
                                    else
                                    {
                                        CheckOnAnimator();
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

        private void CheckOnAnimator()
        {
            var info = animator.GetCurrentAnimatorStateInfo(0);           
            if (!info.loop && info.normalizedTime * 100 >= animLength)
            {
                IsEnter = true;
            }            
        }

        private void CheckOnAnimation()
        {
            float normalTime = animation[normalAnimClipName].normalizedTime;
            if (!animation[normalAnimClipName].clip.isLooping && normalTime * 100 >= (animLength - 5 / animLength))
            {
                animation.Stop();
                IsEnter = true;
            }
        }

        public void OnExitState()
        {
            if (stateManager.IsDebugLog)
            {
                Debug.Log($"退出{name}状态，状态机归属：{stateManager.gameObject.name}");
            }
            isWaitAnim = false;
            if (actionStack != null && actionStack.Count > 0)
            {
                actionStack.Pop()?.Invoke();
            }
            foreach (StateBehaviour behaviour in stateBehaviours)
            {
                if (behaviour.IsActive)
                    behaviour.OnExit();
            }
        }
    }
}
