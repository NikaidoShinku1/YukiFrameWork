using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace YukiFrameWork.States
{   
    public class StateBehaviour : BehaviourBase
    {     
        /// <summary>
        /// 进入状态时
        /// </summary>
        public virtual void OnEnter(Action action = null)
        {
            
            actionStack.Push(action);
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
            if(actionStack.Count>0)
            actionStack.Pop()?.Invoke();
        }
    }
}