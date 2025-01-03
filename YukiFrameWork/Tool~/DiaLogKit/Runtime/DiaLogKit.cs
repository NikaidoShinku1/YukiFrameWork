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
using System.Linq;
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
        private static IDiaLogLoader loader;     
        private static Dictionary<string, DiaLog> diaLogController;

        public static IReadOnlyDictionary<string, DiaLog> DiaLogs => diaLogController;

        static DiaLogKit()
        {            
            diaLogController = new Dictionary<string, DiaLog>();
        }

        public static void Init(string projectName)
        {
            Init(new ABManagerDiaLogLoader(projectName));
        }

        public static void Init(IDiaLogLoader loader)
        {
            DiaLogKit.loader = loader;
        }

        /// <summary>
        /// 绑定UI对话组件
        /// </summary>
        /// <param name="nodeKey"></param>
        /// <param name="uiDiaLog"></param>
        public static void Bind(string nodeKey, UIDiaLog uiDiaLog)
            => Bind(GetDiaLogueByKey(nodeKey),uiDiaLog);

        public static void Bind(DiaLog diaLog,UIDiaLog uiDiaLog)
            => uiDiaLog.InitDiaLog(diaLog);

        /// <summary>
        /// 通过标识得到对话控制器
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static DiaLog GetDiaLogueByKey(string key)
        {
            diaLogController.TryGetValue(key, out DiaLog diaLog);
            return diaLog;
        }

        /// <summary>
        /// 创建对话控制器
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static DiaLog CreateDiaLogue(string key,string path)
            => CreateDiaLogue(key,loader.Load<NodeTree>(path));
    
        public static void CreateDiaLogueAsync(string key,string path, Action<DiaLog> onCompleted)
        {
            loader.LoadAsync<NodeTree>(path, tree => onCompleted?.Invoke(CreateDiaLogue(key,tree)));
        }

        /// <summary>
        /// 创建一个新的对话器
        /// </summary>
        /// <param name="key">标识</param>
        /// <param name="nodeTree">对话树配置</param>
        /// <param name="autoRelease">是否自动回收，开启后在结束运行时会将整个对话树初始化</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static DiaLog CreateDiaLogue(string key,NodeTree nodeTree)
        {           
            if (diaLogController.ContainsKey(key))
            {
                throw new Exception("当前已经存在该标识的对话控制器 Key：" + key);
            }

            var log = GlobalObjectPools.GlobalAllocation<DiaLog>();
            log.Init(key, nodeTree);

            diaLogController.Add(key, log);
            loader?.UnLoad(nodeTree);
            return log;
        }

        internal static bool RemoveDiaLogue(string key)
            => diaLogController.Remove(key);

        /// <summary>
        /// 检查控制器是否启动
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static bool CheckDiaLogIsActive(string key) => diaLogController.ContainsKey(key) && diaLogController[key].treeState == NodeTreeState.Running;

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

        internal NodeTree tree;
        public NodeTreeState treeState { get; protected set; } = NodeTreeState.Waiting;
        private bool isInited = false;              

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
            this.tree = nodeTree.Instantiate();
            this.tree.nodes = nodeTree.nodes.Select(x => x.Instantiate()).ToList();       
            foreach (var item in this.tree.nodes)
            {

                for (int i = 0; i < item.randomItems.Count; i++)
                {
                    item.randomItems[i] = this.tree.nodes.Find(x => x.nodeId == item.randomItems[i].nodeId);
                }
                
                for (int i = 0; i < item.optionItems.Count; i++)
                {
                    var option = item.optionItems[i];
                    if (option.nextNode)
                        option.nextNode = this.tree.nodes.Find(x => x.nodeId == option.nextNode.nodeId);
                }

                if (item.child)
                {
                    item.child = tree.nodes.Find(x => x.nodeId == item.child.nodeId);
                }
            }

            this.tree.rootNode = this.tree.nodes.FirstOrDefault(x => x.IsRoot);
            MonoHelper.Update_AddListener(Update);
            MonoHelper.FixedUpdate_AddListener(FixedUpdate);
            MonoHelper.LateUpdate_AddListener(LateUpdate);           
        }

        private void Update(MonoHelper helper)
        {            
            if (treeState == NodeTreeState.Running && tree.runningNode != null)
            {
                tree.runningNode.OnUpdate();
            }
        }

        private void FixedUpdate(MonoHelper helper)
        {
            if (treeState == NodeTreeState.Running && tree.runningNode != null)
            {
                tree.runningNode.OnFixedUpdate();
            }
        }

        private void LateUpdate(MonoHelper helper)
        {
            if (treeState == NodeTreeState.Running && tree.runningNode != null)
            {
                tree.runningNode.OnLateUpdate();
            }
        }

        void IGlobalSign.Release()
        {
            DiaLogKey = string.Empty;
            if (tree != null)
            {
                tree.onEnterCallBack.UnRegisterAllEvent();
                tree.onExitCallBack.UnRegisterAllEvent();
                tree.onCompletedCallBack.UnRegisterAllEvent();
                tree.onFailedCallBack.UnRegisterAllEvent();
                DiaLogKit.RemoveDiaLogue(DiaLogKey);
                End();
            }
            tree.Destroy();
            tree = null;
            isInited = false;            
            MonoHelper.Update_RemoveListener(Update);
            MonoHelper.FixedUpdate_RemoveListener(FixedUpdate);
            MonoHelper.LateUpdate_RemoveListener(LateUpdate);
            
        }

        public void Start()
        {          
            if (treeState == NodeTreeState.Running)
            {
                Debug.Log("该控制器绑定的对话树已经被启动，不会重复执行Start方法");
                return;            
            }
            treeState = NodeTreeState.Running;
            tree.OnTreeStart();
            DiaLogKit.onGlobalNodeChanged.SendEvent(DiaLogKey, tree.runningNode);           
        }
        public void End()
        {          
            if (treeState == NodeTreeState.Waiting)
            {
                LogKit.I("该控制器没有被启动，End方法是不会触发的,请至少调用一次Start方法");
                return;
            }
            treeState = NodeTreeState.Waiting;
            tree.OnTreeEnd();             
        }
        /// <summary>
        /// 默认推进
        /// </summary>
        /// <returns></returns>
        public MoveNodeState MoveNext()
        {
            var state = tree.MoveNext();
            if (state == MoveNodeState.Succeed)
            {
                DiaLogKit.onGlobalNodeChanged.SendEvent(DiaLogKey, tree.runningNode);
            }
            else if (state == MoveNodeState.Failed)
            {
                tree.onFailedCallBack.SendEvent();
            }
            return state;
        }

        /// <summary>
        /// 根据条件推进
        /// </summary>
        /// <param name="option"></param>
        /// <returns></returns>
        public MoveNodeState MoveNextByOption(Option option)
        {
            var state =  tree.MoveNextByOption(option);

            if (state == MoveNodeState.Succeed)
            {
                DiaLogKit.onGlobalNodeChanged.SendEvent(DiaLogKey, tree.runningNode);
            }
            else if (state == MoveNodeState.Failed)
            {
                tree.onFailedCallBack.SendEvent();
            }
            return state;
        }

        /// <summary>
        /// 选择指定节点推进
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public MoveNodeState MoveNode(Node node)
        {
            var state = tree.MoveNode(node);
            if (state == MoveNodeState.Succeed)
            {
                DiaLogKit.onGlobalNodeChanged.SendEvent(DiaLogKey, tree.runningNode);
            }
            else if (state == MoveNodeState.Failed)
            {
                tree.onFailedCallBack.SendEvent();
            }
            return state;

        }

        /// <summary>
        /// 根据Id选择指定节点推进
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public MoveNodeState MoveNode(string id)
        {
            return MoveNode(GetAllDiaLogNodes().Find(x => x.id == id));
        }

        public bool Release() => this.GlobalRelease();     
        public IUnRegister RegisterWithNodeEnterEvent(Action<Node> startEvent)
        {         
            return tree.onEnterCallBack.RegisterEvent(startEvent);
        }

        public IUnRegister RegisterWithNodeExitEvent(Action<Node> exitEvent)
        {            
            return tree.onExitCallBack.RegisterEvent(exitEvent);
        }

        public IUnRegister RegisterWithNodeCompleteEvent(Action<Node> completeEvent)
        {
            return tree.onCompletedCallBack.RegisterEvent(completeEvent);
        } 

        /// <summary>
        /// 注册当对话树推进状态为Failed时触发的事件
        /// </summary>
        /// <param name="endEvent"></param>
        /// <returns></returns>
        public IUnRegister RegisterNextFailedEvent(Action endEvent)
            => tree.onFailedCallBack.RegisterEvent(endEvent);

        public Node GetCurrentRuntimeNode() => tree.runningNode;

        public Node GetRootNode() => tree.rootNode;
   
        public List<Node> GetAllDiaLogNodes() => tree.nodes;

        public Node GetNodeById(int id) => tree.nodes.Find(x => x.nodeId == id);

        public void Foreach(Action<Node> each) => tree.ForEach(each);
    }
}
