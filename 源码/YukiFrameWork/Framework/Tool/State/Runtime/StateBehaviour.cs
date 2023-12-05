using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork.States
{
    public class StateBehaviour
    {
        public string name;
        public int index;

        public StateManager StateManager { get; set; } 

        protected Transform transform => StateManager.transform;
        
        private StateDataBase _dataBase;

        protected StateBase state => StateManager.runTimeStatesDict[index];

        protected StateDataBase stateBehaviourData
        {
            get
            {
                if (_dataBase == null)
                    _dataBase = state.dataBases.Find(x => x.typeName == this.GetType().Namespace +"."+ this.GetType().Name);
                return _dataBase;
            }
        }

        protected Animator animator => state.animData.animator;

        protected Animation animation => state.animData.animation;

        protected bool enabled
        {
            get => stateBehaviourData.isActive;
            set => stateBehaviourData.isActive = value;
        }

        public virtual void OnInit() { }

        public virtual void OnEnter() { }

        public virtual void OnUpdate() { }

        public virtual void OnFixedUpdate() { }

        public virtual void OnLateUpdate() { }

        public virtual void OnExit() { }

        protected void OnChangeState(int index)
            => StateManager.OnChangeState(index);

        protected void OnChangeState(string name)
            => StateManager.OnChangeState(name);

        protected void SetInt(string name, int v) 
            => StateManager.SetInt(name, v);

        protected void SetFloat(string name, float v) 
            => StateManager.SetFloat(name, v);

        protected void SetBool(string name, bool v) 
            => StateManager.SetBool(name, v);

    }
}
