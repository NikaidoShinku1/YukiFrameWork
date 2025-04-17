///=====================================================
/// - FileName:      BehaviourTree.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/13 19:32:26
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Linq;
using System.Collections;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork.Behaviours
{
    public class BehaviourTree : YMonoBehaviour
    {
        public AIRootBehaviour rootBehaviour { get; private set; }
        [SerializeField, LabelText("Config配置")]
#if UNITY_EDITOR
        [OnValueChanged(nameof(InspectorChange))]
#endif
        internal BehaviourTreeSO View;

        public BehaviourTreeSO Source { get; private set; }

        [LabelText("初始化时机")]
        public RuntimeInitMode initMode;

        [LabelText("是否异步初始化")]
        public bool async_Start;

        [LabelText("是否在初始化完成后自动启动")]
        public bool auto_Start;

        [LabelText("是否在行为树结束后自动重新开始")]
        public bool Repeat_tree;
        [LabelText("行为树开启时触发")]
        [FoldoutGroup("Event")]
        public UnityEvent OnStarted;
        [FoldoutGroup("Event")]
        [LabelText("行为树关闭时触发")]
        public UnityEvent OnCanceled;

        [DictionaryDrawerSettings(KeyLabel = "参数标识", ValueLabel = "参数设置")]
        [LabelText("参数添加集合")]
        [SerializeField, InfoBox("可为运行组件BehaviourTree添加公共参数集合,当存在参数且类型符合时，为行为的字段添加BehaviourParam可自动赋值，否则也可手动获取参数!")]
        internal YDictionary<string, BehaviourParam> inspector_Params_Dict = new YDictionary<string, BehaviourParam>();

        public IReadOnlyDictionary<string, BehaviourParam> Params => inspector_Params_Dict;

        public void AddParam(string key, BehaviourParam param)
        {
            inspector_Params_Dict.Add(key, param);
        }

        public void RemoveParam(string key)
        {
            inspector_Params_Dict.Remove(key);
        }

        protected override void Awake()
        {
            base.Awake();
            if (initMode == RuntimeInitMode.Awake)
                Init();
        }
        private void Start()
        {
            if (initMode == RuntimeInitMode.Start)
                Init();
        }
        internal List<AIBehaviour> runtime_behaviours = new List<AIBehaviour>();

        private void OnEnable()
        {
            BehaviourManager.Instance.AddBehaviourTree(this);
        }

        private void OnDisable()
        {
            BehaviourManager.Instance.RemoveBehaviourTree(this);
        }
        public bool IsInited { get; private set; }
        private async void Init()
        {
            if (IsInited) return;
            if (!View)
                throw new Exception("丢失配置");
            for (int i = 0; i < View.AllNodes.Count; i++)
            {
                //等两帧加载一个
                if (async_Start)
                    await CoroutineTool.WaitForFrames(2);
                runtime_behaviours.Add(View.AllNodes[i].Instantiate());
            }
            rootBehaviour = runtime_behaviours.FirstOrDefault(x => x.IsRoot) as AIRootBehaviour;
            if (!rootBehaviour)
                throw new Exception("根节点丢失");
            Source = View.Instantiate();
            Source.AllNodes = new List<AIBehaviour>(runtime_behaviours);
            for (int i = 0; i < runtime_behaviours.Count; i++)
            {
                AIBehaviour behaviour = runtime_behaviours[i];
                behaviour.behaviourTreeSO = Source;
                List<int> behaivourIds = new List<int>();
                behaviour.ForEach(x =>
                {
                    if (x)
                        behaivourIds.Add(x.ID);
                });
                behaviour.Clear();
                for (int j = 0; j < behaivourIds.Count; j++)
                {
                    behaviour.AddChild(runtime_behaviours.Find(x => x.ID == behaivourIds[j]));
                }
                behaviour.Init(this);
            }

            IsInited = true;
            if (auto_Start)
                StartTree();
        }

        public void StartTree()
        {
            if (rootBehaviour.Status == BehaviourStatus.Running)
                return;

            rootBehaviour.Start();
            OnStarted?.Invoke();
        }

        public bool IsRuning => rootBehaviour.Status == BehaviourStatus.Running;

        /// <summary>
        /// 是否暂停行为树
        /// </summary>
        public bool IsPaused { get; set; }

        public void CancelTree()
        {
            foreach (var item in runtime_behaviours)
            {
                item.ResetBehaviour();
            }
            OnCanceled?.Invoke();
        }
        public void CheckResetTree()
        {
            if (Repeat_tree && (rootBehaviour.IsSuccess || rootBehaviour.IsFailed))
            {
                ResetTree();
            }
        }

        public void ResetTree()
        {
            CancelTree();
            StartTree();
        }
        private void OnDestroy()
        {
            CancelTree();
        }
        bool IsPlaying => Application.isPlaying;
        [SerializeField, HideInInspector]
        internal YDictionary<int, List<BehaviourParamView>> paramViews = new YDictionary<int, List<BehaviourParamView>>();
#if UNITY_EDITOR
        [Button("打开运行时编辑器窗口", ButtonHeight = 30)]
        [ShowIf(nameof(IsPlaying))]
        void EditGraph()
        {
            BehaviourTreeGraphWindow.ShowExample(Source);
        }
        bool IsShow => View;       
        private IEnumerable bes => View ? View.AllNodes.Select(x => new ValueDropdownItem() { Text = x.name + "_" + x.ID, Value = x }) : default;
        [ShowIf(nameof(IsShow))]
        [ValueDropdown(nameof(bes))]
        [InfoBox("当AIBehaviour重写DrawGizmos方法，选择高亮节点")]       
        public AIBehaviour gizmosView;

        void InspectorChange(BehaviourTreeSO treeSO)
        {            
            paramViews.Clear();
            if (treeSO)
            {
                paramViews = treeSO.AllNodes.ToDictionary(x => x.ID,_ => new List<BehaviourParamView>());
            }
        }
        private void OnDrawGizmos()
        {
            //如果存在则绘制
            if (gizmosView)
                gizmosView.DrawGizmos(transform);         
        }      
#endif

    }



}
