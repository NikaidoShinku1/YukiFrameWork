using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork.States
{
    public class StateBehaviour
    {
        [HideField]
        public string name;

        [HideField]
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
        #region 生命周期重写
        public virtual void OnInit() { }

        public virtual void OnEnter() { }

        public virtual void OnUpdate() { }

        public virtual void OnFixedUpdate() { }

        public virtual void OnLateUpdate() { }

        public virtual void OnExit() { }
        #endregion

        #region Unity API拓展
        public virtual void OnTriggerEnter(Collider other)
        {

        }

        public virtual void OnTriggerStay(Collider other)
        {

        }

        public virtual void OnTriggerExit(Collider other)
        {

        }

        public virtual void OnTriggerEnter2D(Collider2D collision)
        {

        }

        public virtual void OnTriggerExit2D(Collider2D collision)
        {

        }

        public virtual void OnTriggerStay2D(Collider2D collision)
        {

        }

        public virtual void OnCollisionEnter(Collision collision)
        {

        }

        public virtual void OnCollisionStay(Collision collision)
        {

        }

        public virtual void OnCollisionExit(Collision collision)
        {

        }

        public virtual void OnCollisionEnter2D(Collision2D collision)
        {

        }

        public virtual void OnCollisionStay2D(Collision2D collision)
        {

        }

        public virtual void OnCollisionExit2D(Collision2D collision)
        {

        }

        public virtual void OnMouseDown()
        {

        }

        public virtual void OnMouseDrag()
        {

        }

        public virtual void OnMouseEnter()
        {

        }

        public virtual void OnMouseExit()
        {

        }

        public virtual void OnMouseUp()
        {

        }

        public virtual void OnMouseOver()
        {

        }

        public virtual void OnValidate()
        {

        }
        #endregion

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

        protected bool GetBool(string name)
            => StateManager.GetBool(name);

        protected int GetInt(string name)
            => StateManager.GetInt(name);

        protected float GetFloat(string name)
            => StateManager.GetFloat(name);

    }
}
