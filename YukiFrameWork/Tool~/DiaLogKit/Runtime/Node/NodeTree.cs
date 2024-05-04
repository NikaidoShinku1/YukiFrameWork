using System.Collections.Generic;
using UnityEngine;
using UnityEditor.Callbacks;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Math;
using System.Reflection;
using System.Collections;
using System;
using YukiFrameWork.Extension;
using System.Linq;







#if UNITY_EDITOR
using UnityEditor;
#endif
using Sirenix.OdinInspector;

namespace YukiFrameWork.DiaLog
{
    public enum MoveNodeState
    {
        Idle,//没有推进
        Succeed,//成功推进
        Failed,//推进失败
    }
    [CreateAssetMenu(fileName = "NodeTree", menuName = "YukiFrameWork/NodeTree")]
    public class NodeTree : ScriptableObject
    {
        [Button("初始化", ButtonHeight = 30), PropertySpace, FoldoutGroup("配置", -1)]
        void DataInit()
        {
            if (treeState == NodeTreeState.Running)
                OnTreeEnd();
        }
        // 对话树的开始 根节点
        [SerializeField, LabelText("对话开始的根节点"), FoldoutGroup("配置")] private Node rootNode;
        // 当前正在播放的对话
        public Node runningNode { get; private set; }
        // 对话树当前状态 用于判断是否要开始这段对话
        [LabelText("对话树当前的状态"), FoldoutGroup("配置")] public NodeTreeState treeState = NodeTreeState.Waiting;

        [SerializeField, LabelText("设置对话树的语言:"), DisableIf(nameof(IsSyncLocalization)), FoldoutGroup("配置")]
        private Language mCurrentLanguage;

        public Language CurrentLanguage => IsSyncLocalization ? LocalizationKit.LanguageType : mCurrentLanguage;

        [LabelText("语言是否同步本地化配置默认语言"), SerializeField, FoldoutGroup("配置")]
        private bool IsSyncLocalization;

        // 所有对话内容的存储列表
        [LabelText("所有对话内容的存储列表"), ReadOnly, FoldoutGroup("配置")]
        public List<Node> nodes = new List<Node>();

        
        // 判断当前对话树和对话内容都是运行中状态则进行OnUpdate()方法更新
        internal virtual MoveNodeState MoveNext()
        {
            if (treeState == NodeTreeState.Waiting || runningNode == null)
            {
                Debug.LogError("对话树没有被启动或者分支已经结束，无法推进");
                return MoveNodeState.Failed;
            }

            if (!runningNode.IsCompleted)
            {
                return MoveNodeState.Failed;
            }

            if (runningNode.IsDefaultMoveNext)
            {
                runningNode.OnExit();
                onExitCallBack.SendEvent(runningNode);              
                runningNode = runningNode.MoveToNext();

                if (runningNode == null)
                {
                    return MoveNodeState.Failed;
                }
                MonoHelper.Start(OnStateEnter(runningNode));
                return MoveNodeState.Succeed;
            }

            return MoveNodeState.Idle;

        }

        private IEnumerator OnStateEnter(Node node)
        {
            onEnterCallBack.SendEvent(node);
            yield return CoroutineTool.WaitUntil(() => node.IsCompleted);
            node.OnEnter();            
        }

        /// <summary>
        /// 根据节点设置的Index位移对话到某一个节点上
        /// </summary>
        /// <param name="index">节点下标/ID</param>
        /// <exception cref="System.Exception"></exception>
        internal virtual void MoveByNodeIndex(int index)
        {
            if (treeState == NodeTreeState.Waiting)
            {
                Debug.LogError("对话树没有被启动，无法推进");
                return;
            }

            var node = nodes.Find(x => x.NodeIndex == index);

            if (node == null)
            {
                throw new System.Exception("对话节点不存在，请检查Index: -- " + index);
            }

            if (runningNode != null)
            {
                runningNode.OnExit();
                onExitCallBack.SendEvent(runningNode);
            }

            runningNode = node;
            MonoHelper.Start(OnStateEnter(runningNode));            
        }

        // 对话树开始的触发方法
        internal virtual void OnTreeStart()
        {           
            runningNode = rootNode;            
            treeState = NodeTreeState.Running;
            OnLanguageChange(CurrentLanguage);
            if (IsSyncLocalization)           
                LocalizationKit.RegisterLanguageEvent(OnLanguageChange);           
            
        }

        private void OnLanguageChange(Language language)
        {
            foreach (var node in nodes)
            {
                node.currentLanguage = language;            
            }

            ///如果树正在运行且存在正在运作的节点，则在改变语言后重新触发一次进入
            if (treeState == NodeTreeState.Running && runningNode != null)
            {
                MonoHelper.Start(OnStateEnter(runningNode));
            }
        }

        internal readonly EasyEvent<Node> onEnterCallBack = new EasyEvent<Node>();
        internal readonly EasyEvent<Node> onExitCallBack = new EasyEvent<Node>();
        // 对话树结束的触发方法
        internal virtual void OnTreeEnd()
        {
            if (treeState == NodeTreeState.Waiting) return;
            treeState = NodeTreeState.Waiting;
            if (runningNode != null)
            {
                runningNode.OnExit();
                onExitCallBack.SendEvent(runningNode);
            }
            if (IsSyncLocalization)
                LocalizationKit.UnRegisterLanguageEvent(OnLanguageChange);
        }

#if UNITY_EDITOR

        [HideInInspector]
        public Color connectColor = new Color(0,0,0,1);

        [Button("Edit Graph", ButtonHeight = 30)]
        void OpenWindow()
        {
            DiaLogEditorWindow.OpenWindow();
        }

        [Button("导出Json配置表", ButtonHeight = 30),InfoBox("该配置表为基本数据的配表，不支持对分支节点中的Option进行配置，应该自行在编辑器设置")]
        void ReImport(string assetPath = "Assets/DiaLogData",string assetName = "DiaLogConfig")
        {
            YDictionary<Language,List<DiaLogModel>> diaLogModels = new YDictionary<Language, List<DiaLogModel>>();           
            for(int i = 0;i < nodes.Count;i++)
            {
                var node = nodes[i];
                if (node == null) continue;
                foreach (var n in node.nodeDatas.Keys)
                {
                    if (!diaLogModels.ContainsKey(n))
                        diaLogModels[n] = new List<DiaLogModel>();

                    diaLogModels[n].Add(new DiaLogModel()
                    {
                        Name = node.nodeDatas[n].name,
                        NodeType = node.GetType().FullName,
                        positionX = node.rect.x,
                        positionY = node.rect.y,                      
                        nodeIndex = node.NodeIndex,
                        Context = node.nodeDatas[n].dialogueContent,                      
                        Sprite = AssetDatabase.GetAssetPath(node.nodeDatas[n].icon)
                    });
                }              
            }

            SerializationTool.SerializedObject(diaLogModels).CreateFileStream(assetPath, assetName, ".json");          
        }

        [Button("Json配置表导入", ButtonHeight = 30)]
        void Import(TextAsset textAsset,bool nodeClear = true)
        {
            if (textAsset == null) return;
            YDictionary<Language, List<DiaLogModel>> diaLogModels = SerializationTool.DeserializedObject<YDictionary<Language, List<DiaLogModel>>>(textAsset.text);

            if (nodeClear)
            {
                nodes.Clear();
            }

            foreach (var key in diaLogModels.Keys)
            {
                var model = diaLogModels[key];
                foreach (var item in model)
                {
                    var node = CreateNode(AssemblyHelper.GetType(item.NodeType));
                    node.nodeDatas[key] = new NodeData()
                    {
                        name = item.Name,
                        dialogueContent = item.Context,
                        icon = AssetDatabase.LoadAssetAtPath<Sprite>(item.Sprite)
                    };
                    node.rect = new Rect(item.positionX, item.positionY,480,120);
                    node.NodeIndex = item.nodeIndex;
                }
            }
        }

        [Serializable]
        class DiaLogModel
        {
            public string Name;
            public float positionX;
            public float positionY;          
            public string NodeType;
            public int nodeIndex;
            public string Context;
            public string Sprite;           
        }  
        public Node CreateNode(System.Type type)
        {
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            node.NodeIndex = nodes.Count;
            var rootNodeAttribute = type.GetCustomAttribute<RootNodeAttribute>(true);           
            if (rootNodeAttribute != null && nodes.Count == 0)
                rootNode = node;
            Undo.RecordObject(this, "Node Tree (CreateNode)");

            nodes.Add(node);
            if (!Application.isPlaying)
            {
                AssetDatabase.AddObjectToAsset(node, this);
            }
            Undo.RegisterCreatedObjectUndo(node, "Node Tree (CreateNode)");
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

        public void AddChild(Node parent, Node child)
        {
            if (parent != null && child != null)
            {
                SingleNode singleNode = parent as SingleNode;
                if (singleNode)
                {
                    Undo.RecordObject(singleNode, "Node Tree (AddChild)");
                    singleNode.child = child;
                    EditorUtility.SetDirty(singleNode);
                }
                CompositeNode compositeNode = parent as CompositeNode;
                if (compositeNode)
                {
                    Undo.RecordObject(compositeNode, "Node Tree (AddChild)");
                    compositeNode.options.Add(new Option()
                    {
                        childIndex = child.NodeIndex,
                    });
                    EditorUtility.SetDirty(compositeNode);
                }
            }
        }          
        
#endif

    }
}
