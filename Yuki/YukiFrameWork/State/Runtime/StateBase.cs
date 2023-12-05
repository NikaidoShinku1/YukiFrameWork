using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
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
                    animData.animator.Play(animData.clipName,animData.layer,0);
                    break;
                case StateAnimType.Animation:
                    if (animData.animation == null)
                        throw new NullReferenceException("当前没有为该状态正确添加animation组件！");
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

        private StateBehaviour behaviour;

        public StateBehaviour Behaviour
        {
            get
            {
                if (behaviour == null)
                {
                    System.Type type = System.Type.GetType(typeName);
                    if (type != null)
                    {
                        behaviour = System.Activator.CreateInstance(type) as StateBehaviour;
                        behaviour.index = index;
                        behaviour.name = typeName;
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
