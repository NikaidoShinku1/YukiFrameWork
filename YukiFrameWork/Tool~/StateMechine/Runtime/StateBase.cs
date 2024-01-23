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
        OnValidate
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

        public StateAnim animData;

        private readonly Stack<Action> callBacks = new Stack<Action>();

        public void OnInit(StateManager stateManager)
        {
            for (int i = 0; i < dataBases.Count; i++)
            {
                if (dataBases[i].isActive)
                {
                    StateBehaviour state = dataBases[i].Behaviour;
                    state.StateManager = stateManager;
                }
            }

            switch (animData.type)
            {
                case StateAnimType.Animator:
                    if (animData.animator == null)                  
                        throw new NullReferenceException("当前没有为该状态正确添加animator组件！");
                    animData.animator.speed = animData.animSpeed;                  
                    break;
                case StateAnimType.Animation:
                    if (animData.animation == null)
                        throw new NullReferenceException("当前没有为该状态正确添加animation组件！");
                    foreach (AnimationState state in animData.animation)
                    {
                        state.speed = animData.animSpeed;
                    }
                    break;
            }
            SetAllBaseLifeCycle(StateLifeCycle.OnInit);
        }

        public void OnEnter(Action callBack = null)
        {
            if(callBack != null)callBacks.Push(callBack);
            SetAllBaseLifeCycle(StateLifeCycle.OnEnter);

            switch (animData.type)
            {
                case StateAnimType.Animator:
                    if (animData.animator == null)
                        throw new NullReferenceException("当前没有为该状态正确添加animator组件！");
                    if (animData.isActiveDefaultAnim)
                        animData.animator.Play(animData.clipName,animData.layer,0);
                    break;
                case StateAnimType.Animation:
                    if (animData.animation == null)
                        throw new NullReferenceException("当前没有为该状态正确添加animation组件！");
                    if(animData.isActiveDefaultAnim)
                        animData.animation.Play(animData.clipName);                
                    break;
            }
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
        public void OnTriggerEnter(Collider other)
        {
            for (int i = 0; i < dataBases.Count; i++)
            {
                if (dataBases[i].isActive)
                {
                    StateBehaviour state = dataBases[i].Behaviour;
                    if (state == null) continue;

                    state.OnTriggerEnter(other);
                }
            }
        }

        public void OnTriggerStay(Collider other)
        {
            SetAllAPIExtension(UnityAPIExtension.OnTriggerStay, other);
        }

        public void OnTriggerExit(Collider other)
        {
            SetAllAPIExtension(UnityAPIExtension.OnTriggerExit, other);
        }

        public void OnTriggerEnter2D(Collider2D collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnTriggerEnter2D, collision);
        }

        public void OnTriggerExit2D(Collider2D collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnTriggerExit2D, collision);
        }

        public void OnTriggerStay2D(Collider2D collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnTriggerStay2D, collision);
        }

        public void OnCollisionEnter(Collision collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnCollisionEnter, collision);
        }

        public void OnCollisionStay(Collision collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnCollisionStay, collision);
        }

        public void OnCollisionExit(Collision collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnCollisionExit, collision);
        }

        public void OnCollisionEnter2D(Collision2D collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnCollisionEnter2D, collision);
        }

        public void OnCollisionStay2D(Collision2D collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnCollisionStay2D, collision);
        }

        public void OnCollisionExit2D(Collision2D collision)
        {
            SetAllAPIExtension(UnityAPIExtension.OnCollisionExit2D, collision);
        }

        public void OnMouseDown()
        {
            SetAllAPIExtension(UnityAPIExtension.OnMouseDown);
        }

        public void OnMouseDrag()
        {
            SetAllAPIExtension(UnityAPIExtension.OnMouseDrag);
        }

        public void OnMouseEnter()
        {
            SetAllAPIExtension(UnityAPIExtension.OnMouseEnter);
        }

        public void OnMouseExit()
        {
            SetAllAPIExtension(UnityAPIExtension.OnMouseExit);
        }

        public void OnMouseUp()
        {
            SetAllAPIExtension(UnityAPIExtension.OnMouseUp);
        }

        public void OnMouseOver()
        {
            SetAllAPIExtension(UnityAPIExtension.OnMouseOver);
        }

        public void OnValidate()
        {
            SetAllAPIExtension(UnityAPIExtension.OnValidate);
        }

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
                        case UnityAPIExtension.OnValidate:
                            state.OnValidate();
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

        public List<MetaData> metaDatas = new List<MetaData>();

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
                                switch (metaDatas[i].dataType)
                                {
                                    case DataType.Object:
                                        if (metaDatas[i].Value != null)
                                            field.SetValue(behaviour, metaDatas[i].Value);
                                        break;
                                    case DataType.Int16:
                                        {
                                            var data = short.Parse(metaDatas[i].value);
                                            field.SetValue(behaviour, data);
                                        }
                                        break;
                                    case DataType.Int32:
                                        {
                                            var data = int.Parse(metaDatas[i].value);
                                            field.SetValue(behaviour, data);
                                        }
                                        break;
                                    case DataType.Int64:
                                        {
                                            var data = long.Parse(metaDatas[i].value);
                                            field.SetValue(behaviour, data);
                                        }
                                        break;
                                    case DataType.UInt16:
                                        {
                                            var data = ushort.Parse(metaDatas[i].value);
                                            field.SetValue(behaviour, data);
                                        }
                                        break;
                                    case DataType.UInt32:
                                        {
                                            var data = uint.Parse(metaDatas[i].value);
                                            field.SetValue(behaviour, data);
                                        }
                                        break;
                                    case DataType.UInt64:
                                        {
                                            var data = ulong.Parse(metaDatas[i].value);
                                            field.SetValue(behaviour, data);
                                        }
                                        break;
                                    case DataType.Single:
                                        {
                                            var data = float.Parse(metaDatas[i].value);
                                            field.SetValue(behaviour, data);
                                        }
                                        break;
                                    case DataType.Double:
                                        {
                                            var data = double.Parse(metaDatas[i].value);
                                            field.SetValue(behaviour, data);
                                        }
                                        break;
                                    case DataType.Boolan:
                                        {
                                            bool data = bool.Parse(metaDatas[i].value);
                                            field.SetValue(behaviour, data);
                                        }
                                        break;
                                    case DataType.String:                                                                                 
                                            field.SetValue(behaviour, metaDatas[i].value);
                                        break;
                                    case DataType.Enum:                                        
                                        var value = Enum.Parse(field.FieldType, metaDatas[i].value);
                                        if(value != null)
                                            field.SetValue(behaviour, value);
                                        break;
                                }
                            }
                        }
                    }
                }
                return behaviour;
            }
        }

    }

    [System.Serializable]
    public class StateAnim
    {
        public int layer = 0;

        public string clipName;

        public int clipIndex;

        public float animSpeed = 1f;

        public float animLength = 100f;

        public bool isLoop;

        public bool isActiveDefaultAnim;

        public StateAnimType type;

        public Animator animator;

        public Animation animation;
    }
}
