///=====================================================
/// - FileName:      DiaLogKit.cs
/// - NameSpace:     YukiFrameWork.DiaLogueue
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/12 20:50:57
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using System.Collections.Generic;
using YukiFrameWork.Pools;
using Sirenix.OdinInspector;
namespace YukiFrameWork.DiaLogue
{
    public enum DiaLogLoadMode
    {
        [LabelText("默认模式")]
        Normal,
        [LabelText("安全模式")]
        Safe
    }

    public enum DiaLogLoadType
    {
        [LabelText("内部编辑器设置")]
        Inspector,
        [LabelText("外部初始化")]
        Custom
    }

    public enum DiaLogPlayMode
    {
        [LabelText("默认模式")]
        Normal,
        [LabelText("打字机")]
        Writer
    }
    public static class DiaLogKit 
	{	
        private static Dictionary<string, DiaLog> diaLogController;

        static DiaLogKit()
        {            
            diaLogController = new Dictionary<string, DiaLog>();
        }

        public static DiaLog GetDiaLogueByKey(string key)
        {
            diaLogController.TryGetValue(key, out DiaLog diaLog);
            return diaLog;
        }

        /// <summary>
        /// 创建一个新的对话器
        /// </summary>
        /// <param name="key">标识</param>
        /// <param name="nodeTree">对话树配置</param>
        /// <param name="autoRelease">是否自动回收，开启后在结束运行时会将整个对话树初始化</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static DiaLog CreateDiaLog(string key,NodeTree nodeTree)
        {           
            if (diaLogController.ContainsKey(key))
            {
                throw new Exception("当前已经存在该标识的对话控制器 Key：" + key);
            }

            var log = GlobalObjectPools.GlobalAllocation<DiaLog>();
            log.Init(key, nodeTree);

            diaLogController.Add(key, log);
            return log;
        }

        internal static bool RemoveDiaLog(string key)
            => diaLogController.Remove(key);

        public static bool CheckDiaLogIsActive(string key) => diaLogController.ContainsKey(key);

        public static bool OnDiaLogRelease(DiaLog diaLog)
            => GlobalObjectPools.GlobalRelease(diaLog);

        /// <summary>
        /// 全局的节点变化事件注册(任何对话控制器执行MoveNext都会触发该回调)
        /// </summary>
        public readonly static EasyEvent<string,Node> onGlobalNodeChanged = new EasyEvent<string,Node>(); 
        
    }

    public class DiaLog : IGlobalSign
    {
        public bool IsMarkIdle { get; set; }
        public string DiaLogKey { get; private set; }

        internal NodeTree DiaLogTree;

        private bool isInited = false;      
      
        private Language mCurrentLanguage;

        public Language NodeCurrentLanguage
        {
            get => mCurrentLanguage;
            set
            {
                if (mCurrentLanguage != value)
                {
                    mCurrentLanguage = value;

                    for (int i = 0; i < DiaLogTree.nodes.Count; i++)
                    {
                        var node = DiaLogTree.nodes[i];

                        if (node == null) continue;

                        node.currentLanguage = mCurrentLanguage;
                    }
                }
            }
        }

        void IGlobalSign.Init()
        {

        }

        /// <summary>
        /// 初始化对话控制器
        /// </summary>
        /// <param name="key"></param>
        /// <param name="nodeTree"></param>
        /// <param name="autoRelease"></param>
        public void Init(string key, NodeTree nodeTree)
        {
            if (nodeTree == null)
            {
                Debug.LogError("对话树配置添加失败请检查是否不为空!");
                return;
            }

            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("标识添加失败，是空字符串");
                return;
            }
            if (isInited) return;
            isInited = true;
            this.DiaLogKey = key;
            this.DiaLogTree = nodeTree;
          
            MonoHelper.Update_AddListener(Update);
            MonoHelper.FixedUpdate_AddListener(FixedUpdate);
            MonoHelper.LateUpdate_AddListener(LateUpdate);
        }

        private void Update(MonoHelper helper)
        {            
            if (DiaLogTree.treeState == NodeTreeState.Running && DiaLogTree.runningNode != null)
            {
                DiaLogTree.runningNode.OnUpdate();
            }
        }

        private void FixedUpdate(MonoHelper helper)
        {
            if (DiaLogTree.treeState == NodeTreeState.Running && DiaLogTree.runningNode != null)
            {
                DiaLogTree.runningNode.OnFixedUpdate();
            }
        }

        private void LateUpdate(MonoHelper helper)
        {
            if (DiaLogTree.treeState == NodeTreeState.Running && DiaLogTree.runningNode != null)
            {
                DiaLogTree.runningNode.OnLateUpdate();
            }
        }

        void IGlobalSign.Release()
        {
            DiaLogKey = string.Empty;
            if (DiaLogTree != null)
            {
                DiaLogTree.onEnterCallBack.UnRegisterAllEvent();
                DiaLogTree.onExitCallBack.UnRegisterAllEvent();
                DiaLogKit.RemoveDiaLog(DiaLogKey);
                End();
            }           
            DiaLogTree = null;
            isInited = false;            
            MonoHelper.Update_RemoveListener(Update);
            MonoHelper.FixedUpdate_RemoveListener(FixedUpdate);
            MonoHelper.LateUpdate_RemoveListener(LateUpdate);
        }

        public void Start()
        {          
            if (DiaLogTree.treeState == NodeTreeState.Running)
            {
                Debug.Log("该控制器绑定的对话树已经被启动，不会重复执行Start方法");
                return;            
            }           
            DiaLogTree.OnTreeStart();
            DiaLogKit.onGlobalNodeChanged.SendEvent(DiaLogKey, DiaLogTree.runningNode);           
        }
        public void End()
        {          
            if (DiaLogTree.treeState != NodeTreeState.Running)
            {
                LogKit.I("该控制器没有被启动，End方法是不会触发的,请至少调用一次Start方法");
                return;
            }
            DiaLogTree.OnTreeEnd();             
        }
        public MoveNodeState MoveNext()
        {
            var state = DiaLogTree.MoveNext();
            if (state == MoveNodeState.Succeed)
            {
                DiaLogKit.onGlobalNodeChanged.SendEvent(DiaLogKey, DiaLogTree.runningNode);
            }
            else if (state == MoveNodeState.Failed)
            {
                DiaLogTree.onEndCallBack.SendEvent();
            }
            return state;
        }
        public bool Release() => this.GlobalRelease();
        public Node GetNodeByIndex(int index)
        {        
            return DiaLogTree.nodes.Find(x => x.NodeIndex == index);
        }
        /// <summary>
        /// 根据节点设置的Index位移对话到某一个节点上
        /// </summary>
        /// <param name="index">节点下标/ID</param>
        /// <exception cref="System.Exception"></exception>
        public void MoveByNodeIndex(int index)
        {
            DiaLogTree.MoveByNodeIndex(index);
        }

        /// <summary>
        /// 通过对比节点对象进行跳转
        /// </summary>
        /// <param name="node">要被对比的节点对象</param>
        public void MoveByNode(Node node)
        {
            DiaLogTree.MoveByNodeIndex(node.NodeIndex);
        }

        public IUnRegister RegisterWithNodeEnterEvent(Action<Node> startEvent)
        {         
            return DiaLogTree.onEnterCallBack.RegisterEvent(startEvent);
        }

        public IUnRegister RegisterWithNodeExitEvent(Action<Node> exitEvent)
        {            
            return DiaLogTree.onExitCallBack.RegisterEvent(exitEvent);
        }

        /// <summary>
        /// 注册当对话树结束或者推进状态为Failed时触发的事件
        /// </summary>
        /// <param name="endEvent"></param>
        /// <returns></returns>
        public IUnRegister RegisterTreeEndEvent(Action endEvent)
            => DiaLogTree.onEndCallBack.RegisterEvent(endEvent);

        /// <summary>
        /// 通过对应下标查找到分支节点后设置分支的完成回调，以及分支结束时的回调
        /// </summary>
        /// <param name="nodeIndex">节点下标</param>
        /// <param name="action">完成回调</param>
        /// <param name="onFinish">结束回调</param>
        public void OnOptionsCompleted(int nodeIndex, Action<CompositeNode, Option[]> action,Action onFinish)
        {
            OnOptionsCompleted(GetNodeByIndex(nodeIndex) as CompositeNode, action,onFinish);
        }
        /// <summary>
        /// 通过分支节点设置分支的完成回调，以及分支结束时的回调
        /// </summary>
        /// <param name="node">分支节点</param>
        /// <param name="action">完成回调</param>
        /// <param name="onFinish">结束回调</param>
        public void OnOptionsCompleted(CompositeNode node, Action<CompositeNode, Option[]> action,Action onFinish)
        {
            if (node == null)
            {
                LogKit.E("该API仅可以用于分支节点，请检查传入的下标/节点是否是分支节点，且传入的下标Index是存在节点的 Node Null");
                return;
            }
            node.OnOptionsCompleted(action,onFinish);
        }
        /// <summary>
        /// 设置所有分支的完成回调，以及分支结束时的回调
        /// </summary>     
        /// <param name="action">完成回调</param>
        /// <param name="onFinish">结束回调</param>
        public void OnOptionsCompleted(Action<CompositeNode, Option[]> action,Action onFinish)
        {
            List<Node> nodes = DiaLogTree.nodes.FindAll(x => typeof(CompositeNode).IsInstanceOfType(x));
            CompositeNode[] compositeNodes = new CompositeNode[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                compositeNodes[i] = nodes[i] as CompositeNode;
            }
            foreach (var item in compositeNodes)
            {
                OnOptionsCompleted(item, action, onFinish);
            }          
        }

    }
}
