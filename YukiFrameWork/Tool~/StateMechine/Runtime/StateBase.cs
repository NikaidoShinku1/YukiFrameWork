using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using System;
using YukiFrameWork.Extension;
using Sirenix.OdinInspector;
using UnityEngine.Animations;
using System.Linq;



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

        [SerializeField, LabelText("在拥有动画剪辑时，是否自动进行过渡")]
        internal bool IsAutoTransition;
        [SerializeField,ReadOnly]
        internal int selectIndex;
        [SerializeField, ReadOnly]
        internal List<string> targets = new List<string>()
        {
            "没有任何可以自动过渡的连线"
        };

#if UNITY_EDITOR
        internal void OnInspectorGUI(bool playable,StateBase stateBase,StateManager stateManager)
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

                if (animationClip != null)
                {
                    StateMechine stateMechine = stateManager.stateMechine;
                    string tName = string.Empty;
                    if (!stateMechine || stateBase == null) return;

                    if (!stateMechine.subTransitions.TryGetValue(stateBase.layerName, out var value))
                    {
                        value = stateMechine.transitions;

                    }
                    else
                    {

                    }
                    var datas = value.Where(x => x.fromStateName == stateBase.name && x.conditionDatas.Count == 0 && x.conditions.Count == 0).ToList();
                    targets = datas.Select(x => x.toStateName).ToList();
                    if (!tName.IsNullOrEmpty())
                        targets.Add(tName);
                    targets.Add("没有任何可以自动过渡的连线");
                    IsAutoTransition = EditorGUILayout.Toggle("是否在动画结束后开启自动过渡", IsAutoTransition);
                    if (selectIndex >= targets.Count) selectIndex = 0;
                    if (IsAutoTransition)
                    {
                        EditorGUILayout.HelpBox("开启自动过渡时，必须持有连线的同时保证该连线没有任何条件", MessageType.Info);
                        EditorGUILayout.BeginHorizontal();
                        GUILayout.Label("自动过渡的目标状态");
                        selectIndex = EditorGUILayout.Popup(selectIndex, targets.ToArray());
                        EditorGUILayout.EndHorizontal();
                    }
                }
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

        internal void OnTransitionEnter(float velocity,bool completed)
        {
            for (int i = 0; i < dataBases.Count; i++)
            {
                if (dataBases[i].isActive)
                {
                    StateBehaviour state = dataBases[i].Behaviour;
                    if (state == null) continue;
                    state.OnTransitionEnter(velocity,completed);
                }
            }
        }

        internal void OnTransitionExit(float velocity,bool completed)
        {
            for (int i = 0; i < dataBases.Count; i++)
            {
                if (dataBases[i].isActive)
                {
                    StateBehaviour state = dataBases[i].Behaviour;
                    if (state == null) continue;
                    state.OnTransitionExit(velocity,completed);
                }
            }
        }

        internal void OnAnimationExit()
        {
            for (int i = 0; i < dataBases.Count; i++)
            {
                if (dataBases[i].isActive)
                {
                    StateBehaviour state = dataBases[i].Behaviour;
                    if (state == null) continue;
                    state.OnAnimationExit();
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

