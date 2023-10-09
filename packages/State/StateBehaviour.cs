using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace YukiFrameWork.States
{   
    public class StateBehaviour : BehaviourBase
    {     
        /// <summary>
        /// ����״̬ʱ
        /// </summary>
        public virtual void OnEnter(Action action = null)
        {
            
            actionStack.Push(action);
        }

        /// <summary>
        /// ״̬��������
        /// </summary>        
        public virtual void OnUpdate() { }

        public virtual void OnFixedUpdate() { }

        /// <summary>
        /// �˳�״̬
        /// </summary>
        public virtual void OnExit()
        {
            if(actionStack.Count>0)
            actionStack.Pop()?.Invoke();
        }
    }
}