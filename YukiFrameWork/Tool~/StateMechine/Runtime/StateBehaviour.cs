using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using YukiFrameWork.Events;
using YukiFrameWork.Extension;


namespace YukiFrameWork.States
{
    public class StateBehaviour : AbstractController,IController
    {       
        [HideField]
        public string name;

        [HideField]
        public int index;

        [HideField]
        public string layerName;

        public IState StateManager { get; set; } 

        protected Transform transform => StateManager.transform;
        
        private StateDataBase _dataBase;

        private StateBase _state;

        //protected AnimationClipPlayable animationClipPlayable => state.clipPlayable;
        protected StateBase state
        {
            get
            {
                if(_state == null)
                    _state = StateManager.runTimeSubStatePair[layerName].stateBases.Find(x => x.name == name);              
                return _state;
            }
        }

        protected StateDataBase stateBehaviourData
        {
            get
            {
                if (_dataBase == null)
                    _dataBase = state.dataBases.Find(x => x.typeName == this.GetType().FullName);
                return _dataBase;
            }
        }     

        protected bool enabled
        {
            get => stateBehaviourData.isActive;
            set => stateBehaviourData.isActive = value;
        }
        #region 生命周期重写
        public override void OnInit() { }

        public virtual void OnEnter() { }

        public virtual void OnUpdate() { }

        public virtual void OnFixedUpdate() { }

        public virtual void OnLateUpdate() { }

        public virtual void OnExit() { }
        #endregion

        /// <summary>
        /// 过渡到下一个状态的进入时会持续检测的回调
        /// </summary>
        /// /// <param name="velocity">过渡累加时间</param>
        public virtual void OnTransitionEnter(float velocity,bool completed)
        {
            
        }

        /// <summary>
        /// 在状态退出时会持续检测的回调
        /// </summary>
        /// <param name="velocity">过渡累加时间</param>
        public virtual void OnTransitionExit(float velocity,bool completed)
        {
            
        }

        /// <summary>
        /// 仅当StateManager启用了Playable兼容时可用，用于处理当该状态所绑定的动画结束时的回调。如果是循环动画，则在每一次循环结尾调用
        /// </summary>
        public virtual void OnAnimationExit()
        {
            
        }
     
        protected void OnChangeState(int index)
            => StateManager.OnChangeState(index);

        protected void OnChangeState(string name)
            => StateManager.OnChangeState(name,layerName);

        protected void SetInt(string name, int v) 
            => StateManager.SetInt(name, v);

        protected void SetFloat(string name, float v) 
            => StateManager.SetFloat(name, v);

        protected void SetBool(string name, bool v) 
            => StateManager.SetBool(name, v);

        protected void SetTrigger(string name)
            => StateManager.SetTrigger(name);

        protected void ResetTrigger(string name)
            => StateManager.ResetTrigger(name);

        protected bool GetBool(string name)
            => StateManager.GetBool(name);

        protected int GetInt(string name)
            => StateManager.GetInt(name);

        protected float GetFloat(string name)
            => StateManager.GetFloat(name);
  
    }
}
