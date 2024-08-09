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
    public class NodeTree : ScriptableObject
    {
        [LabelText("对话树识别标识:"),SerializeField]
        [InfoBox("不同的对话树标识必须唯一,必须填写"),FoldoutGroup("配置", -2)]
        internal string nodekey;
#if UNITY_EDITOR
        [Button("初始化", ButtonHeight = 30), PropertySpace, FoldoutGroup("配置", -1)]
        void DataInit()
        {
            if (treeState == NodeTreeState.Running)
                OnTreeEnd();

            DiaLogGraphWindow.CloseWindow();
        }
#endif
        // 对话树的开始 根节点
        [SerializeField, LabelText("对话开始的根节点"), FoldoutGroup("配置")] internal Node rootNode;
        // 当前正在播放的对话
        public Node runningNode { get; private set; }
        // 对话树当前状态 用于判断是否要开始这段对话
        [LabelText("对话树当前的状态"), FoldoutGroup("配置")] public NodeTreeState treeState = NodeTreeState.Waiting;   

        // 所有对话内容的存储列表
        [LabelText("所有对话内容的存储列表"),SerializeField, ReadOnly, FoldoutGroup("配置")]
        internal List<Node> nodes = new List<Node>();

        private Coroutine mEnterCoroutine;
        
        // 判断当前对话树和对话内容都是运行中状态则进行OnUpdate()方法更新
        internal MoveNodeState MoveNext()
        {
            if (treeState == NodeTreeState.Waiting || runningNode == null)
            {
                LogKit.W("对话树没有被启动或者分支已经结束，无法推进");
                return MoveNodeState.Failed;
            }

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
            if (treeState == NodeTreeState.Waiting || runningNode == null)
            {
                LogKit.W("对话树没有被启动或者分支已经结束，无法推进");
                return MoveNodeState.Failed;
            }

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
            treeState = NodeTreeState.Running;
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
            if (treeState == NodeTreeState.Waiting) return;
            treeState = NodeTreeState.Waiting;
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
              
        [Button("Edit Graph", ButtonHeight = 30)]
        void OpenWindow()
        {
            DiaLogGraphWindow.ShowExample(this);
        }
   
        [Button("导出Json配置表", ButtonHeight = 30),InfoBox("该配置表为基本数据的配表，不支持对分支节点中的Option进行配置，应该自行在编辑器设置")]
        void ReImport(string assetPath = "Assets/DiaLogData",string assetName = "DiaLogConfig")
        {        
            foreach (var item in nodes)
            {                
                if (item.GetIcon() != null)
                    item.spritePath = AssetDatabase.GetAssetPath(item.GetIcon());
                else item.spritePath = string.Empty;

                item.nodeType = item.nodeType.IsNullOrEmpty() ? item.GetType().FullName : item.nodeType;
            }
            string json = SerializationTool.SerializedObject(nodes,settings:new JsonSerializerSettings() {NullValueHandling = NullValueHandling.Ignore });                 
            json.CreateFileStream(assetPath, assetName, ".json");         
        }

     
        [Button("Json配置表导入", ButtonHeight = 30)]
        void Import(TextAsset textAsset,bool nodeClear = true)
        {
            List<JObject> models = SerializationTool.DeserializedObject<List<JObject>>(textAsset.text);

            if (nodeClear)
            {
                while (nodes.Count > 0)
                {
                    DeleteNode(nodes[nodes.Count - 1]);
                }
            }
          
            foreach (var model in models) 
            {
                string nodeType = model[nameof(nodeType)].ToString();
                string title = model["dialogueTitle"].ToString();
                string context = model["dialogueContext"].ToString();
                string spritePath = model["spritePath"].ToString();
                Type type = AssemblyHelper.GetType(nodeType);
                var node = CreateNode(type,true);
                node.dialogueTitle = title;
                node.dialogueContext = context;
                if(!spritePath.IsNullOrEmpty())
                    node.icon = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);

                foreach (var field in type.GetRuntimeFields())
                {                   
                    if (field.HasCustomAttribute<DeSerializedNodeFieldAttribute>(true))
                    {
                        JToken token = model[field.Name];
                        if (token == null) continue;                        
                        field.SetValue(node, token.ToObject(field.FieldType));
                    }
                }

                foreach (var field in type.GetRuntimeProperties())
                {
                    if (field.HasCustomAttribute<DeSerializedNodeFieldAttribute>(true))
                    {
                        JToken token = model[field.Name];
                        if (token == null) continue;
                        field.SetValue(node, token.ToObject(field.PropertyType));
                    }
                }
            }
        }
   
        public Node CreateNode(System.Type type,bool IsCustomPosition = false)
        {          
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.id = GUID.Generate().ToString();
            node.nodeType = type.FullName;
            // node.NodeIndex = nodes.Count;
            if (node.DiscernType == typeof(RootNodeAttribute))
                rootNode = node;           
            Undo.RecordObject(this, "Node Tree (CreateNode)");         

            nodes.Add(node);
            if (IsCustomPosition)
            {
                node.position = new Vector2(nodes.Count * 100, 0);
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
