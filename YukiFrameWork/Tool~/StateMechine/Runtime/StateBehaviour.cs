using System;
using System.Linq;
using UnityEngine;
using YukiFrameWork.Extension;


namespace YukiFrameWork.States
{
    public class StateBehaviour : IGetArchitecture,ISendEvent,ISendCommand,IGetRegisterEvent
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

        #region Unity API拓展
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindTriggerEnterEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnTriggerEnter(Collider other)
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindTriggerStayEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnTriggerStay(Collider other)
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindTriggerExitEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnTriggerExit(Collider other)
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindTriggerEnter2DEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnTriggerEnter2D(Collider2D collision)
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindTriggerExit2DEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnTriggerExit2D(Collider2D collision)
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindTriggerStay2DEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnTriggerStay2D(Collider2D collision)
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindCollisionEnterEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnCollisionEnter(Collision collision)
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindCollisionStayEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnCollisionStay(Collision collision)
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindCollisionExitEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnCollisionExit(Collision collision)
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindCollisionEnter2DEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnCollisionEnter2D(Collision2D collision)
        {

        }

        [Obsolete("该方法已经废弃无法使用,请使用transform.BindCollisionStay2DEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnCollisionStay2D(Collision2D collision)
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindCollisionExit2DEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnCollisionExit2D(Collision2D collision)
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindMouseDownEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnMouseDown()
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindMouseDragEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnMouseDrag()
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindMouseEnterEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnMouseEnter()
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindMouseExitEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnMouseExit()
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindMouseUpEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnMouseUp()
        {

        }
        [Obsolete("该方法已经废弃无法使用,请使用transform.BindMouseOverEvent")]
        [MethodAPI("弃用的拓展")]
        public virtual void OnMouseOver()
        {

        }      
        #endregion

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

        IArchitecture IGetArchitecture.GetArchitecture()      
            => StateManager.GetArchitecture();
        
    }
}
