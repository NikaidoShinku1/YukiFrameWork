using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using YukiFrameWork.Extension;
using Sirenix.OdinInspector;
using UnityEngine.Animations;


#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.States
{
  
    public enum StateLifeCycle
    {
        OnInit = 0,
        OnEnter,
        OnUpdate,
        OnFixedUpdate,
        OnLateUpdate,
        OnExit
    }
    [Serializable]
    public class StatePlayable
    {      
        [LabelText("动画剪辑")]
        public AnimationClip animationClip;

        [LabelText("过渡时间")]
        public float speed = 0.25f;

        [LabelText("状态权重"),Range(0,1)]
        public float clipWidth = 1;

#if UNITY_EDITOR
        internal void OnInspectorGUI(bool playable)
        {            
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("过渡时间");
            speed = EditorGUILayout.FloatField(speed);
            EditorGUILayout.EndHorizontal();            

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("状态权重");
            clipWidth = EditorGUILayout.Slider(clipWidth, 0, 1);
            EditorGUILayout.EndHorizontal();

            if (playable)
            {
                EditorGUILayout.HelpBox("当前在StateManager已经启动了Playable兼容,请进行动画剪辑设置!", MessageType.Warning);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("动画剪辑");
                animationClip = (AnimationClip)EditorGUILayout.ObjectField(animationClip, typeof(AnimationClip), true);
                EditorGUILayout.EndHorizontal();
            }          
        }
#endif
    }

    [System.Serializable]
    public class StateBase
    {
#if UNITY_EDITOR
        public Rect rect;
#endif

        public bool defaultState;

        public string name;

        public string layerName;

        public int index;

        internal Coroutine PlayableCoroutine;

        public List<StateDataBase> dataBases = new List<StateDataBase>();
     
        public bool IsSubingState;

        internal AnimationClipPlayable clipPlayable;
        [SerializeField]
        public StatePlayable statePlayble;

        private readonly Stack<Action> callBacks = new Stack<Action>();

        internal void OnInit(IState stateManager)
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

        internal void OnEnter(Action callBack = null)
        {
            if (callBack != null) callBacks.Push(callBack);
            SetAllBaseLifeCycle(StateLifeCycle.OnEnter);
        }

        internal void OnUpdate()
        {
            SetAllBaseLifeCycle(StateLifeCycle.OnUpdate);
        }

        internal void OnFixedUpdate()
        {
            SetAllBaseLifeCycle(StateLifeCycle.OnFixedUpdate);
        }

        internal void OnLateUpdate()
        {
            SetAllBaseLifeCycle(StateLifeCycle.OnLateUpdate);
        }

        internal void OnTransitionEnter(float velocity)
        {
            for (int i = 0; i < dataBases.Count; i++)
            {
                if (dataBases[i].isActive)
                {
                    StateBehaviour state = dataBases[i].Behaviour;
                    if (state == null) continue;
                    state.OnTransitionEnter(velocity);
                }
            }
        }

        internal void OnTransitionExit(float velocity)
        {
            for (int i = 0; i < dataBases.Count; i++)
            {
                if (dataBases[i].isActive)
                {
                    StateBehaviour state = dataBases[i].Behaviour;
                    if (state == null) continue;
                    state.OnTransitionExit(velocity);
                }
            }
        }

        internal void OnExit(bool isBack = true)
        {
            SetAllBaseLifeCycle(StateLifeCycle.OnExit);
            if (isBack && callBacks.Count > 0)
                callBacks?.Pop()?.Invoke();
        }
        private void SetAllBaseLifeCycle(StateLifeCycle lifeCycle)
        {
            for (int i = 0; i < dataBases.Count; i++)
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
        public string name;
        public int index;
        public bool isActive;
        public string layerName;

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
                        behaviour.name = name;
                        behaviour.layerName = layerName;
                    }

                    foreach (var field in type.GetRuntimeFields())
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

