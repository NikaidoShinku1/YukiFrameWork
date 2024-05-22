using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using YukiFrameWork.Events;
using YukiFrameWork.Extension;


namespace YukiFrameWork.States
{
    public class StateBehaviour : IController
    {
        private IArchitecture mArchitecture;
        private readonly object _lock = new object();

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

        protected AnimationClipPlayable animationClipPlayable => state.clipPlayable;
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
        public virtual void OnInit() { }

        public virtual void OnEnter() { }

        public virtual void OnUpdate() { }

        public virtual void OnFixedUpdate() { }

        public virtual void OnLateUpdate() { }

        public virtual void OnExit() { }
        #endregion

        /// <summary>
        /// 过渡到下一个状态的进入时会持续检测的回调
        /// </summary>
        /// /// <param name="velocity">过渡的增量速度</param>
        public virtual void OnTransitionEnter(float velocity)
        {
            
        }

        /// <summary>
        /// 在状态退出时会持续检测的回调
        /// </summary>
        /// <param name="velocity">过渡的增量速度</param>
        public virtual void OnTransitionExit(float velocity)
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

        protected bool GetBool(string name)
            => StateManager.GetBool(name);

        protected int GetInt(string name)
            => StateManager.GetInt(name);

        protected float GetFloat(string name)
            => StateManager.GetFloat(name);

        #region Architecture
        /// <summary>
        /// 可重写的架构属性,不使用特性初始化时需要重写该属性
        /// </summary>
        protected virtual IArchitecture RuntimeArchitecture
        {
            get
            {
                lock (_lock)
                {
                    if (mArchitecture == null)
                    {
                        mArchitecture = ArchitectureConstructor.I.Enquene(this);                      
                    }
                    return mArchitecture;
                }
            }
        }
        IArchitecture IGetArchitecture.GetArchitecture()
        {
            return RuntimeArchitecture;
        }
        #endregion
    }
}
