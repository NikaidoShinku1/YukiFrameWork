///=====================================================
/// - FileName:      DiaLogController.cs
/// - NameSpace:     YukiFrameWork.DiaLogue
/// - Description:   高级定制脚本生成
/// - Creation Time: 8/24/2025 12:41:46 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Pools;
using System.Collections.Generic;
namespace YukiFrameWork.DiaLogue
{
    public enum DiaLogState
    {
        Idle,
        Running,
        Completed,
    }
    public abstract class DiaLogController : IController, IGlobalSign
    {
        internal static DiaLogController CreateInstance(NodeTreeBase nodeTree)
        {
            DiaLogController diaLogController = GlobalObjectPools.GlobalAllocation(nodeTree.RuntimeControllerType) as DiaLogController;
            diaLogController.Init(nodeTree);
            return diaLogController;
        }

        internal static DiaLogController CreateInstance(Type type,IEnumerable<INode> nodes)
        {
            DiaLogController diaLogController = GlobalObjectPools.GlobalAllocation(type) as DiaLogController;
            diaLogController.Init(nodes);
            return diaLogController;
        }

        private NodeTreeBase nodeTree;
        private Dictionary<int,INode> nodes = new Dictionary<int, INode>();
        private bool isInited = false;
        
        /// <summary>
        /// 当前的节点Id，在Move执行后同步
        /// </summary>
        public int CurrentNodeId { get; private set; }
        /// <summary>
        /// 当前的节点
        /// </summary>
        public INode CurrentNode => TryFindNode(CurrentNodeId);
       
        /// <summary>
        /// 该对话控制的状态
        /// </summary>
        public DiaLogState DiaLogState { get; private set; }
        /// <summary>
        /// 对话控制的配置标识
        /// </summary>
        public string Key => nodeTree.Key;
        /// <summary>
        /// 对话控制的配置本体
        /// </summary>
        public NodeTreeBase NodeTree => nodeTree;
       
        /// <summary>
        /// 等效于生命周期的事件
        /// </summary>
        public event Action<DiaLogController, object[]> onStart;
        /// <summary>
        /// 等效于生命周期的事件
        /// </summary>
        public event Action<DiaLogController,INode,INode> onMove;
        /// <summary>
        /// 等效于生命周期的事件
        /// </summary>
        public event Action<DiaLogController> onCompleted;

        public bool IsMarkIdle { get; set; }


        protected virtual void OnUpdate() { }

        protected virtual void OnFixedUpdate() { }

        protected virtual void OnLateUpdate() { }

        /// <summary>
        /// 当对话控制器启动时触发
        /// </summary>
        /// <param name="param"></param>
        protected abstract void OnStart(params object[] param);

        /// <summary>
        /// 当对话控制器完成时触发
        /// </summary>
        protected abstract void OnCompleted();

        /// <summary>
        /// 当对话变化时触发,当默认通过根节点启动时，lastNode为空
        /// </summary>
        /// <param name="lastNode">变化前的节点</param>
        /// <param name="nextNode">变化后的节点</param>
        protected abstract void OnMove(INode lastNode, INode nextNode);
        
        internal void Init(NodeTreeBase nodeTree)
        {
            if (isInited) return;
            if (!nodeTree) return;
            this.nodeTree = nodeTree;
            isInited = true;
            DiaLogState = DiaLogState.Idle;
            foreach (var item in nodeTree.Nodes)
            {
                nodes.Add(item.Id, item);
            }
        }

        internal void Init(IEnumerable<INode> nodes)
        {
            if (isInited) return;            
            isInited = true;
            DiaLogState = DiaLogState.Idle;
            foreach (var item in nodes)
            {
                this.nodes.Add(item.Id, item);
            }
        }

        internal void Start(int id,params object[] param) 
        {
            //如果是正在运行的对话控制器，则不执行
            if (DiaLogState == DiaLogState.Running)
                return;
            DiaLogState = DiaLogState.Running;
            OnStart(param);
            onStart?.Invoke(this,param);
            //如果没有传递指定的节点id，则使用默认开始
            if (id == -1)
            {
                if (!FindRootNode(out INode node))
                {
                    throw new Exception("当前启动对话没有默认节点，请检查配置!");
                }

                MoveInternal(node);
            }
            else
            {
                MoveInternal(id);
            }
        }

        /// <summary>
        /// 查找根节点
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public bool FindRootNode(out INode node)
        {
            node = null;
            foreach (var item in nodes)
            {
                if (item.Value.IsRoot)
                {
                    node = item.Value;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 根据标识查找节点，不会抛出异常
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public INode TryFindNode(int id)
        {
            nodes.TryGetValue(id, out var node);
            return node;
        }

        /// <summary>
        /// 根据标识查找节点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public INode FindNode(int id)
        {
            return nodes[id];
        }

        internal bool MoveInternal(INode node)
        {
            return MoveInternal(node, out _);
        }

        internal bool MoveInternal(INode node,out string failedTip)
        {
            failedTip = string.Empty;
            //如果不在运行则不能进行对话跳转
            if (DiaLogState != DiaLogState.Running)
            {
                failedTip = "对话控制器不在运行状态，请先调用Start方法启动";
                return false;
            }
            if (node == null)
            {
                failedTip = "节点为空，请检查对话是否正确连接或已经完成对话";
                return false;
            }
            OnMove(CurrentNode, node);
            onMove?.Invoke(this,CurrentNode, node);
            CurrentNodeId = node.Id;          
            
            //如果节点没有连接，则视为完成对话
            if (node.LinkNodes == null || node.LinkNodes.Count == 0)
                Completed();
            return true;
        }

        internal bool MoveInternal(int id)
        {
            return MoveInternal(id, out _);
        }

        internal bool MoveInternal(int id,out string failedTip)
        { 
            //如果不在运行则不能进行对话跳转
            if (DiaLogState != DiaLogState.Running)
            {
                failedTip = "对话控制器不在运行状态，请先调用Start方法启动";
                return false;
            }
            INode node = TryFindNode(id);
            return MoveInternal(node,out failedTip);
        }

        internal void Completed() 
        {
            if (DiaLogState == DiaLogState.Completed)
                return;
            DiaLogState = DiaLogState.Completed;
            OnCompleted();
            onCompleted?.Invoke(this);
        }

        internal void Update()
        {
            if (DiaLogState == DiaLogState.Running)
                OnUpdate();
        }

        internal void FixedUpdate()
        {
            if (DiaLogState == DiaLogState.Running)
                OnFixedUpdate();
        }

        internal void LateUpdate()
        {
            if (DiaLogState == DiaLogState.Running)
                OnLateUpdate();
        }


        #region Architecture
        private object _object = new object();
        private IArchitecture mArchitecture;

        /// <summary>
        /// 可重写的架构属性,不使用特性初始化时需要重写该属性
        /// </summary>
        protected virtual IArchitecture RuntimeArchitecture
        {
            get
            {
                lock (_object)
                {
                    if (mArchitecture == null)
                        Build();
                    return mArchitecture;
                }
            }
        }
        IArchitecture IGetArchitecture.GetArchitecture()
        {
            return RuntimeArchitecture;
        }

        internal void Build()
        {
            if (mArchitecture == null)
            {
                mArchitecture = ArchitectureConstructor.I.Enquene(this);
            }
        }
        #endregion

        void IGlobalSign.Init()
        {
            DiaLogState = DiaLogState.Idle;
            isInited = false;
        }

        void IGlobalSign.Release()
        {
            if(nodeTree)
            DiaLogKit.UnLoad(nodeTree);
            nodeTree = null;
            isInited = false;
            CurrentNodeId = -1;
            nodes.Clear();
        }

    }
}
