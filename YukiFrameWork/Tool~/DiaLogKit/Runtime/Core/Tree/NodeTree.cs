using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Sirenix.OdinInspector;

namespace YukiFrameWork.DiaLog
{
    [CreateAssetMenu(fileName = "NodeTree", menuName = "YukiFrameWork/NodeTree")]
    public class NodeTree : ScriptableObject
    {
        // 对话树的开始 根节点
        [SerializeField, LabelText("对话开始的根节点")] private Node rootNode;
        // 当前正在播放的对话
        public Node runningNode { get; private set; }
        // 对话树当前状态 用于判断是否要开始这段对话
        [LabelText("对话树当前的状态")] public NodeState treeState = NodeState.Waiting;
        // 所有对话内容的存储列表
        [LabelText("所有对话内容的存储列表"), ReadOnly]
        public List<Node> nodes = new List<Node>();

        // 判断当前对话树和对话内容都是运行中状态则进行OnUpdate()方法更新
        public virtual bool MoveNext()
        {
            if (runningNode == null || treeState == NodeState.Waiting)
            {
                Debug.LogWarning("该对话树已经完成所有的对话或者还没有被启动!", this);
                return false;
            }
            if (runningNode.MoveToNext(out Node nextNode))
            {
                runningNode.OnStop();
                runningNode.state = NodeState.Waiting;
                runningNode = nextNode;
            }

            if (runningNode == null)
            {
                OnTreeEnd();
                return false;
            }
            else
            {
                runningNode.state = NodeState.Running;
                runningNode.OnStart();
            }
            return true;
        }
        // 对话树开始的触发方法
        public virtual void OnTreeStart()
        {
            rootNode.state = NodeState.Running;
            runningNode = rootNode;
            runningNode.OnStart();
            treeState = NodeState.Running;
        }

        /// <summary>
        /// 树在运行时进行复位初始化的操作，可用于直接回档或跳转到哪一个节点，从该处继续
        /// </summary>
        /// <param name="currentNode">需要回档的节点</param>
        /// <param name="callBack">更新的回调</param>
        public void OnTreeRunningInitialization(Language language, int nodeIndex, System.Action<string, Sprite, string> callBack)
        {
            OnTreeEnd();
            Node currentNode = nodes.Find(x => x.NodeIndex == nodeIndex);
            if (currentNode == null)
                throw new System.Exception("当前下标无法查找到节点 Index:" + nodeIndex);
            runningNode?.OnStop();
            currentNode.state = NodeState.Running;
            runningNode = currentNode;
            runningNode.OnStart();
            treeState = NodeState.Running;
            UpdateNode(language, callBack);          
        }

        public virtual void UpdateNode(Language language, System.Action<string, Sprite, string> callBack)
        {
            runningNode.currentLanguage = language;
            string dialog = runningNode.DialogueContent();
            Sprite sprite = runningNode.SpeakerAvatar();
            string name = runningNode.SpeakerName();
            callBack?.Invoke(dialog, sprite, name);
        }
        // 对话树结束的触发方法
        public virtual void OnTreeEnd()
        {
            treeState = NodeState.Waiting;

            for (int i = 0; i < nodes.Count; i++)
            {
                nodes[i].state = NodeState.Waiting;
            }
        }

#if UNITY_EDITOR
        public Node CreateNode(System.Type type)
        {
            Node node = ScriptableObject.CreateInstance(type) as Node;
            node.name = type.Name;
            node.guid = GUID.Generate().ToString();
            node.NodeIndex = nodes.Count;

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
        public Node DeleteeNode(Node node)
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
                    compositeNode.children.Add(child);
                    EditorUtility.SetDirty(compositeNode);
                }
            }
        }
        public void RemoveChild(Node parent, Node child)
        {
            SingleNode singleNode = parent as SingleNode;
            if (singleNode)
            {
                Undo.RecordObject(singleNode, "Node Tree (AddChild)");
                singleNode.child = null;
                EditorUtility.SetDirty(singleNode);
            }
            CompositeNode compositeNode = parent as CompositeNode;
            if (compositeNode)
            {
                Undo.RecordObject(compositeNode, "Node Tree (AddChild)");
                compositeNode.children.Remove(child);
                EditorUtility.SetDirty(compositeNode);
            }
        }

        public List<Node> GetChildren(Node parent)
        {
            List<Node> Children = new List<Node>();
            SingleNode singleNode = parent as SingleNode;
            if (singleNode && singleNode.child != null)
            {
                Children.Add(singleNode.child);
            }
            CompositeNode composite = parent as CompositeNode;
            if (composite)
            {
                return composite.children;
            }
            return Children;
        }
#endif

    }
}
