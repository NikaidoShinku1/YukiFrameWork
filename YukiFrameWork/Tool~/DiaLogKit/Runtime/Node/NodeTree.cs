using System.Collections.Generic;
using UnityEngine;
using System.Reflection;
using System.Collections;
using System;
using YukiFrameWork.Extension;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using Sirenix.OdinInspector;

namespace YukiFrameWork.DiaLogue
{
    public enum MoveNodeState
    {
        Idle,//没有推进
        Succeed,//成功推进
        Failed,//推进失败       
    }
    [CreateAssetMenu(fileName = "NodeTree", menuName = "YukiFrameWork/NodeTree")]
    public class NodeTree : ScriptableObject,IExcelSyncScriptableObject
    {     
        // 对话树的开始 根节点
        [SerializeField, LabelText("对话开始的根节点"), FoldoutGroup("配置")] internal Node rootNode;
        // 当前正在播放的对话
        public Node runningNode { get; private set; }

        public IList Array => nodes;

        public Type ImportType => typeof(Node);

        public bool ScriptableObjectConfigImport => false;

        // 所有对话内容的存储列表
        [LabelText("所有对话内容的存储列表"),SerializeField,  FoldoutGroup("配置")]
        internal List<Node> nodes = new List<Node>();

        private Coroutine mEnterCoroutine;

        void IExcelSyncScriptableObject.Create(int maxLength)
        {
#if UNITY_EDITOR
            while (nodes.Count > 0)
                DeleteNode(nodes[nodes.Count - 1]);
#endif
        }

        void IExcelSyncScriptableObject.Import(int index, object userData)
        {
            var node = userData as Node;
            if (!Application.isPlaying)
            {
#if UNITY_EDITOR
                AssetDatabase.AddObjectToAsset(node, this);
#endif
            }
            nodes.Add(node);
            if (node.IsRoot && !rootNode)
                rootNode = node;
#if UNITY_EDITOR
            if (node.IsComposite)
            {
                foreach (var item in node.optionItems)
                {
                    if (item.iconGUID.IsNullOrEmpty()) continue;
                    item.icon = YukiAssetDataBase.GUIDToInstance<Sprite>(item.iconGUID);
                }
            }
#endif
#if UNITY_EDITOR
            Undo.RegisterCreatedObjectUndo(node, "Node Tree (CreateNode)");
            // EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
#endif
        }

        void IExcelSyncScriptableObject.Completed()
        {
            foreach (var node in nodes)
            {
                foreach (var randomId in node.randomIdItems)
                {
                    var temp = nodes.Find(x => x.nodeId == randomId);
                    if (!temp) continue;
                    node.randomItems.Add(temp);

                    
                }

                for (int i = 0; i < node.optionIdItems.Count; i++)
                {
                    var temp = node.optionItems[i];

                    if (!temp.nextNode)
                    {
                        int id = node.optionIdItems[i];
                        if (id != default)
                        {
                            temp.nextNode = nodes.Find(x => x.nodeId == id);
                        }
                    }
                }

                if (node.IsRoot || node.IsSingle)
                {
                    var child = nodes.Find(x => x.nodeId == node.childNodeId);
                    if (child)
                    {
                        node.child = child;
                    }
                }
            }
        }

        // 判断当前对话树和对话内容都是运行中状态则进行OnUpdate()方法更新
        internal MoveNodeState MoveNext()
        {
            if (!runningNode)
                return MoveNodeState.Failed;
            if (!runningNode.IsCompleted)
            {
                return MoveNodeState.Idle;
            }

            if (runningNode.IsSingle || runningNode.IsRoot)
            {
                runningNode.OnExit();
                onExitCallBack.SendEvent(runningNode);
                runningNode = runningNode.child;

                if (runningNode == null)
                {
                    return MoveNodeState.Failed;
                }
                mEnterCoroutine = MonoHelper.Start(OnStateEnter(runningNode));
                return MoveNodeState.Succeed;
            }
            else if (runningNode.IsComposite)
            {
                return MoveNodeState.Idle;
            }
            else if (runningNode.IsRandom)
            {
                if (runningNode.RandomItems.Count == 0)
                {
                    LogKit.W("没有为随机节点{0} -- {1}添加可选分支进行随机变化，请检查", runningNode.id, runningNode.name);
                    return MoveNodeState.Idle;
                }
                else
                {
                    runningNode.OnExit();
                    onExitCallBack.SendEvent(runningNode);
                    int random = UnityEngine.Random.Range(0, runningNode.RandomItems.Count);
                    runningNode = runningNode.RandomItems[random];

                    if (runningNode == null)                  
                        return MoveNodeState.Failed;

                    mEnterCoroutine = MonoHelper.Start(OnStateEnter(runningNode));
                    return MoveNodeState.Succeed;
                }
            }
            return MoveNodeState.Idle;

        }

        internal Vector2 position;

        internal MoveNodeState MoveNextByOption(Option option)
        {         
            if (option == null)
            {
                LogKit.E("条件不存在!");
                return MoveNodeState.Failed;
            }

            if (!runningNode.IsComposite)
            {
                LogKit.W("当前运行的节点并不是分支节点，不会进行推进");
                return MoveNodeState.Idle;
            }

            return Move(option.nextNode);
        }
        private void OnValidate()
        {
           
        }
        internal MoveNodeState MoveNode(Node node)
        {            
            return Move(node);

        }

        private MoveNodeState Move(Node node)
        {
            if (runningNode != null)
            {
                if (!runningNode.IsCompleted)
                {
                    return MoveNodeState.Idle;
                }
                runningNode.OnExit();
                onExitCallBack.SendEvent(runningNode);
            }
            runningNode = node;

            if (runningNode == null)
            {
                return MoveNodeState.Failed;
            }
            mEnterCoroutine = MonoHelper.Start(OnStateEnter(runningNode));
            return MoveNodeState.Succeed;
        }

        internal MoveNodeState MoveNode(string key)
        {
            return MoveNode(nodes.Find(x => x.id == key));
        }
   

        private IEnumerator OnStateEnter(Node node)
        {
            node.IsCompleted = false;
            onEnterCallBack.SendEvent(node);
            node.OnEnter();
            yield return CoroutineTool.WaitUntil(() => node.IsCompleted);
            onCompletedCallBack.SendEvent(node);
        }    

        // 对话树开始的触发方法
        internal virtual void OnTreeStart()
        {           
            runningNode = rootNode;                       
            MonoHelper.Start(OnStateEnter(runningNode));
        }  

        internal readonly EasyEvent<Node> onEnterCallBack = new EasyEvent<Node>();
        internal readonly EasyEvent<Node> onExitCallBack = new EasyEvent<Node>();
        internal readonly EasyEvent<Node> onCompletedCallBack = new EasyEvent<Node>();

        //当对话树推进失败/结束时调用
        internal readonly EasyEvent onFailedCallBack = new EasyEvent();
        // 对话树结束的触发方法
        internal virtual void OnTreeEnd()
        {          
            if (runningNode != null)
            {
                runningNode.OnExit();
                onExitCallBack.SendEvent(runningNode);
            }

            MonoHelper.Stop(mEnterCoroutine);        
        }

        public void ForEach(Action<Node> each)
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                each?.Invoke(nodes[i]);
            }
        }      

#if UNITY_EDITOR

       
        [Sirenix.OdinInspector.FilePath(Extensions = "xlsx"), PropertySpace(50), LabelText("Excel路径")]
        public string excelPath;
        [Button("导出Excel"), HorizontalGroup("Excel")]
        void CreateExcel()
        {
            if (excelPath.IsNullOrEmpty() || !System.IO.File.Exists(excelPath))
                throw new NullReferenceException("路径为空或不存在!");
            if (SerializationTool.ScriptableObjectToExcel(this, excelPath, out string error))
                Debug.Log("导出成功");
            else throw new Exception(error);
        }
        [Button("导入Excel"), HorizontalGroup("Excel")]
        void ImportExcel()
        {
            if (excelPath.IsNullOrEmpty() || !System.IO.File.Exists(excelPath))
                throw new NullReferenceException("路径为空或不存在!");
            if (SerializationTool.ExcelToScriptableObject(excelPath, 3, this))
            {
                Debug.Log("导入成功");
            }
        }
        [Button("Edit Graph", ButtonHeight = 30),PropertySpace(20)] 
        void OpenWindow()
        {
            DiaLogGraphWindow.ShowExample(this);
        }
        public Node CreateNode(System.Type type,bool IsCustomPosition = false)
        {          
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.id = GUID.Generate().ToString();
            int id = (1000) + nodes.Count + 1;
            node.nodeId = id;
            // node.nodeType = type.FullName;
            while (nodes.Any(x => x.nodeId == id))
            {
                node.nodeId = ++id;
            }
            // node.NodeIndex = nodes.Count;
            if (node.DiscernType == typeof(RootNodeAttribute))
                rootNode = node;           
            Undo.RecordObject(this, "Node Tree (CreateNode)");         

            nodes.Add(node);
            if (IsCustomPosition)
            {
                node.position = new Node.Position(nodes.Count * 100, 0);
            }
            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
            }
            Undo.RegisterCreatedObjectUndo(node, "Node Tree (CreateNode)");
           // EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
            
            return node;
        }
        public Node DeleteNode(Node node)
        {
            Undo.RecordObject(this, "Node Tree (DeleteNode)");
            nodes.Remove(node);           
            Undo.DestroyObjectImmediate(node);
            AssetDatabase.SaveAssets();
            return node;
        }        
#endif

    }
}
