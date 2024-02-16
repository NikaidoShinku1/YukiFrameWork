using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using YukiFrameWork.Extension;
using System.Linq;
namespace YukiFrameWork.States
{
    public enum StateAnimType
    {
        None = 0,
        Animator,
        Animation
    }

    public enum StateLifeCycle
    {
        OnInit = 0,
        OnEnter,
        OnUpdate,
        OnFixedUpdate,
        OnLateUpdate,
        OnExit
    }

    public enum UnityAPIExtension
    {
        OnTriggerEnter,
        OnTriggerStay,
        OnTriggerExit,
        OnTriggerEnter2D,
        OnTriggerExit2D,
        OnTriggerStay2D,
        OnCollisionEnter,
        OnCollisionStay,
        OnCollisionExit,
        OnCollisionEnter2D,
        OnCollisionStay2D,
        OnCollisionExit2D,
        OnMouseDown,
        OnMouseDrag,
        OnMouseEnter,
        OnMouseExit,
        OnMouseUp,
        OnMouseOver,       
    }

    [System.Serializable]
    public class StateBase 
    {
#if UNITY_EDITOR
        public Rect rect;
#endif

        public bool defaultState;

        public string name;

        public int index;      

        public List<StateDataBase> dataBases = new List<StateDataBase>();       

        private readonly Stack<Action> callBacks = new Stack<Action>();

        public void OnInit(IState stateManager)
        {
            for (int i = 0; i < dataBases.Count; i++)
            {
                if (dataBases[i].isActive)
                {
                    StateBehaviour state = dataBases[i].Behaviour;
                    state.StateManager = stateManager;
                }
            }          
            SetAllBaseLifeCycle(StateLifeCycle.OnInit);
        }

        public void OnEnter(Action callBack = null)
        {
            if(callBack != null)callBacks.Push(callBack);
            SetAllBaseLifeCycle(StateLifeCycle.OnEnter);         
        }

        public void OnUpdate()
        {
            SetAllBaseLifeCycle(StateLifeCycle.OnUpdate);          
        }

        public void OnFixedUpdate() 
        {
            SetAllBaseLifeCycle(StateLifeCycle.OnFixedUpdate);
        }

        public void OnLateUpdate()
        {
            SetAllBaseLifeCycle(StateLifeCycle.OnLateUpdate);
        }

        public void OnExit(bool isBack = true)
        {
            SetAllBaseLifeCycle(StateLifeCycle.OnExit);
            if (isBack && callBacks.Count > 0)
                callBacks?.Pop()?.Invoke();
        }
        #region Unity API拓展
        [Obsolete("该方法不推荐使用,请使用transform.BindTriggerEnterEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnTriggerEnter(Collider other)
        {
            SetAllAPIExtension(UnityAPIExtension.OnTriggerEnter, other);
        }
        [Obsolete("该方法不推荐使用,请使用transform.BindTriggerStayEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnTriggerStay(Collider other)
        {
            SetAllAPIExtension(UnityAPIExtension.OnTriggerStay, other);
        }

        [Obsolete("该方法不推荐使用,请使用transform.BindTriggerExitEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnTriggerExit(Collider other)
        {
            SetAllAPIExtension(UnityAPIExtension.OnTriggerExit, other);
        }

        [Obsolete("该方法不推荐使用,请使用transform.BindTriggerEnter2DEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnTriggerEnter2D(Collider2D collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnTriggerEnter2D, collision);
        }
        [Obsolete("该方法不推荐使用,请使用transform.BindTriggerExit2DEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnTriggerExit2D(Collider2D collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnTriggerExit2D, collision);
        }

        [Obsolete("该方法不推荐使用,请使用transform.BindTriggerStay2DEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnTriggerStay2D(Collider2D collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnTriggerStay2D, collision);
        }

        [Obsolete("该方法不推荐使用,请使用transform.BindCollisionEnterEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnCollisionEnter(Collision collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnCollisionEnter, collision);
        }

        [Obsolete("该方法不推荐使用,请使用transform.BindCollisionStayEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnCollisionStay(Collision collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnCollisionStay, collision);
        }

        [Obsolete("该方法不推荐使用,请使用transform.BindCollisionExitEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnCollisionExit(Collision collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnCollisionExit, collision);
        }

        [Obsolete("该方法不推荐使用,请使用transform.BindCollisionEnter2DEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnCollisionEnter2D(Collision2D collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnCollisionEnter2D, collision);
        }

        [Obsolete("该方法不推荐使用,请使用transform.BindCollisionStay2DEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnCollisionStay2D(Collision2D collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnCollisionStay2D, collision);
        }

        [Obsolete("该方法不推荐使用,请使用transform.BindCollisionExit2DEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnCollisionExit2D(Collision2D collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnCollisionExit2D, collision);
        }

        [Obsolete("该方法不推荐使用,请使用transform.BindMouseDownEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnMouseDown()
        {
            SetAllAPIExtension(UnityAPIExtension.OnMouseDown);
        }

        [Obsolete("该方法不推荐使用,请使用transform.BindMouseDragEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnMouseDrag()
        {
            SetAllAPIExtension(UnityAPIExtension.OnMouseDrag);
        }

        [Obsolete("该方法不推荐使用,请使用transform.BindMouseEnterEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnMouseEnter()
        {
            SetAllAPIExtension(UnityAPIExtension.OnMouseEnter);
        }
        [Obsolete("该方法不推荐使用,请使用transform.BindMouseExitEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnMouseExit()
        {
            SetAllAPIExtension(UnityAPIExtension.OnMouseExit);
        }
        [Obsolete("该方法不推荐使用,请使用transform.BindMouseUpEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnMouseUp()
        {
            SetAllAPIExtension(UnityAPIExtension.OnMouseUp);
        }
        [Obsolete("该方法不推荐使用,请使用transform.BindMouseOverEvent")]
        [MethodAPI("弃用的拓展")]
        public void OnMouseOver()
        {
            SetAllAPIExtension(UnityAPIExtension.OnMouseOver);
        }

        [Obsolete]
        private void SetAllAPIExtension(UnityAPIExtension extension,object paremater = null)
        {
            for (int i = 0; i < dataBases.Count; i++)
            {
                if (dataBases[i].isActive)
                {
                    StateBehaviour state = dataBases[i].Behaviour;

                    if (state == null) continue;

                    switch (extension)
                    {
                        case UnityAPIExtension.OnTriggerEnter:
                            state.OnTriggerEnter(paremater as Collider);
                            break;
                        case UnityAPIExtension.OnTriggerStay:
                            state.OnTriggerStay(paremater as Collider);
                            break;
                        case UnityAPIExtension.OnTriggerExit:
                            state.OnTriggerExit(paremater as Collider);
                            break;
                        case UnityAPIExtension.OnTriggerEnter2D:
                            state.OnTriggerEnter2D(paremater as Collider2D);
                            break;
                        case UnityAPIExtension.OnTriggerExit2D:
                            state.OnTriggerExit2D(paremater as Collider2D);
                            break;
                        case UnityAPIExtension.OnTriggerStay2D:
                            state.OnTriggerStay2D(paremater as Collider2D);
                            break;
                        case UnityAPIExtension.OnCollisionEnter:
                            state.OnCollisionEnter(paremater as Collision);
                            break;
                        case UnityAPIExtension.OnCollisionStay:
                            state.OnCollisionStay(paremater as Collision);
                            break;
                        case UnityAPIExtension.OnCollisionExit:
                            state.OnCollisionExit(paremater as Collision);
                            break;
                        case UnityAPIExtension.OnCollisionEnter2D:
                            state.OnCollisionEnter2D(paremater as Collision2D);
                            break;
                        case UnityAPIExtension.OnCollisionStay2D:
                            state.OnCollisionStay2D(paremater as Collision2D);
                            break;
                        case UnityAPIExtension.OnCollisionExit2D:
                            state.OnCollisionExit2D(paremater as Collision2D);
                            break;
                        case UnityAPIExtension.OnMouseDown:
                            state.OnMouseDown();
                            break;
                        case UnityAPIExtension.OnMouseDrag:
                            state.OnMouseDrag();
                            break;
                        case UnityAPIExtension.OnMouseEnter:
                            state.OnMouseEnter();
                            break;
                        case UnityAPIExtension.OnMouseExit:
                            state.OnMouseExit();
                            break;
                        case UnityAPIExtension.OnMouseUp:
                            state.OnMouseUp();
                            break;
                        case UnityAPIExtension.OnMouseOver:
                            state.OnMouseOver();
                            break;                      
                    }
                }
            }
        }

        #endregion
        private void SetAllBaseLifeCycle(StateLifeCycle lifeCycle)
        {
            for (int i = 0;i < dataBases.Count;i++)
            {
                if (dataBases[i].isActive)
                {
                    StateBehaviour state = dataBases[i].Behaviour;
                    if (state == null) continue;
                    switch (lifeCycle)
                    {
                        case StateLifeCycle.OnInit:
                            state.OnInit();
                            break;
                        case StateLifeCycle.OnEnter:
                            state.OnEnter();
                            break;
                        case StateLifeCycle.OnUpdate:
                            state.OnUpdate();
                            break;
                        case StateLifeCycle.OnFixedUpdate:
                            state.OnFixedUpdate();
                            break;
                        case StateLifeCycle.OnLateUpdate:
                            state.OnLateUpdate();
                            break;
                        case StateLifeCycle.OnExit:
                            state.OnExit();
                            break;
                    }
                }
            }
        }
    }

    [System.Serializable]
    public class StateDataBase
    {
        public string typeName;
        public int index;
        public bool isActive;

        public List<Metadata> metaDatas = new List<Metadata>();

        private StateBehaviour behaviour;

        public StateBehaviour Behaviour
        {
            get
            {
                if (behaviour == null)
                {
                    System.Type type = AssemblyHelper.GetType(typeName);
                    if (type != null)
                    {
                        behaviour = System.Activator.CreateInstance(type) as StateBehaviour;
                        behaviour.index = index;
                        behaviour.name = typeName;
                    }

                    foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                    {
                        for (int i = 0; i < metaDatas.Count; i++)
                        {
                            if (metaDatas[i].name.Equals(field.Name) && metaDatas[i].typeName.Equals(field.FieldType.ToString()))
                            {
                                TypeCode typeCode = metaDatas[i].type;

                                if (typeCode == TypeCode.Object)
                                {
                                    if (metaDatas[i].Value != null)
                                        field.SetValue(behaviour, metaDatas[i].Value);
                                }
                                else
                                {
                                    field.SetValue(Behaviour, metaDatas[i].value);
                                }                              
                            }
                        }
                    }
                }
                return behaviour;
            }
        }

    }
   
}
