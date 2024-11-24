///=====================================================
/// - FileName:      BackGroundView.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   高级定制脚本生成
/// - Creation Time: 2024/11/14 17:37:48
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using YukiFrameWork.Extension;
using System.Linq;




#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace YukiFrameWork.Behaviours
{
    public class BackGroundView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<BackGroundView, UxmlTraits> { }
        public BehaviourTreeSO behaviourTree;
        public event Action<GraphBehaviourView> onNodeSelected = null;
        public BackGroundView()
        {
            graphViewChanged = GraphViewChangedCallback;           
            Insert(0, new GridBackground()); //格子背景

            //添加背景网格样式
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ImportSettingWindow.GetData().path + "/BehaviourTree/Editor/GUI/BackGround.uss");
            styleSheets.Add(styleSheet);

            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale); //可缩放            
            this.AddManipulator(new SelectionDragger()); //节点可拖动
            this.AddManipulator(new ContentDragger()); //节点图可移动
            this.AddManipulator(new RectangleSelector()); //可框选多个节点
            
            BehaviourTreeGraphWindow.onValidate += () =>
            {
                Refresh(behaviourTree);
            };
         
        }

        public void BehaviourSelected(GraphBehaviourView view)
        {
            onNodeSelected?.Invoke(view);
        }

        private GraphViewChange GraphViewChangedCallback(GraphViewChange changes)
        {         
            //移动
            if (changes.movedElements != null)
            {
                foreach (GraphElement element in changes.movedElements)
                {
                    if (element is GraphBehaviourView nodeView)
                    {
                        nodeView.SetPos(nodeView.GetPosition());
                        nodeView.Behaviour.ReLoadChild();
                        if (nodeView.Behaviour.Parent)
                            nodeView.Behaviour.Parent.ReLoadChild();
                    }                    
                }               
            }          
            //删除
            if (changes.elementsToRemove != null)
            {
                foreach (GraphElement element in changes.elementsToRemove)
                {                   
                    if (element is GraphBehaviourView nodeView)
                    {
                        nodeView.onNodeSelected -= BehaviourSelected;
                        nodeView.RemoveSelf();                       
                    }
                    else if (element is Edge edge)
                    {
                        var parentNode = (GraphBehaviourView)edge.output.node;
                        var childNode = (GraphBehaviourView)edge.input.node;
                        parentNode.RemoveChild(childNode,this);
                    }                  
                }
            }
            changes.edgesToCreate?.ForEach(x => 
            {
                GraphBehaviourView nodeView = x.output.node as GraphBehaviourView;
                GraphBehaviourView nodeChild = x.input.node as GraphBehaviourView;

                nodeView.Behaviour.AddChild(nodeChild.Behaviour);

                EditorUtility.SetDirty(nodeView.Behaviour);  
                AssetDatabase.SaveAssets();
            });

            return changes;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (Application.isPlaying) return;
            if (behaviourTree == null) return;
            if (behaviourTree.AllNodes.Count == 0)
            {
                evt.menu.AppendAction("新建行为树根节点", arg =>
                {
                    GraphBehaviourView.Create(typeof(AIRootBehaviour), behaviourTree, this, evt.mousePosition);
                });

            }
            else
            {
                if (evt.target is GraphBehaviourView view)
                {
                    if (view.Behaviour && view.Behaviour.GetType() != typeof(AIRootBehaviour))
                    {
                        evt.menu.AppendAction("Edit Script", arg => 
                        {
                           
                            AssetDatabase.GetAllAssetPaths().Select(AssetDatabase.LoadAssetAtPath<MonoScript>)
                            .FirstOrDefault(x => x && x.GetClass() != null && x.GetClass() == view.Behaviour.GetType()).Open();
                        });
                    }
                }
                base.BuildContextualMenu(evt);
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(BehaviourTreeGraphWindow window)
        {
            this.behaviourTree = window.behaviourTree;
            if (behaviourTree == null) return;
            //节点创建时的搜索窗口
            var searchWindowProvider = ScriptableObject.CreateInstance<NodeSearchWindowProvider>();           
            searchWindowProvider.Init(window,this,behaviourTree);

            void OpenSearch(NodeCreationContext context)
            {            
                //打开搜索窗口
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), searchWindowProvider);
            }
            nodeCreationRequest = OpenSearch;         
            viewTransform.position = behaviourTree.ViewportRect.position;
            BehaviourTreeGraphWindow.onUpdate += () => 
            {
                if(behaviourTree)
                behaviourTree.ViewportRect = new Rect(viewTransform.position, viewTransform.scale);
            };
            behaviourTree.onValidate += () => Refresh(behaviourTree);
        }

        /// <summary>
        /// 刷新
        /// </summary>
        public void Refresh(BehaviourTreeSO btSO)
        {
            BuildGraphView();
            if (!btSO) return;
            behaviourTree = btSO;          
            //还原位置与大小
            if (btSO.ViewportRect != default)
            {
                viewTransform.position = btSO.ViewportRect.position;
                viewTransform.scale = new Vector3(btSO.ViewportRect.size.x, btSO.ViewportRect.size.y, 1);
            }                       
            btSO.Save();
        }

        /// <summary>
        /// 构建行为树节点图
        /// </summary>
        private void BuildGraphView()
        {
            Dictionary<AIBehaviour, GraphBehaviourView> nodeDict = new Dictionary<AIBehaviour, GraphBehaviourView>();

            //先删掉旧的节点 线 和注释块
            foreach (Node node in nodes)
            {
                RemoveElement(node);
            }

            foreach (Edge edge in edges)
            {
                RemoveElement(edge);
            }
            if (!behaviourTree) return;
            //创建节点
            CreateGraphNode(nodeDict, behaviourTree.AllNodes);

            //根据父子关系连线
            BuildConnect(nodeDict, behaviourTree.AllNodes);         
        }

        /// <summary>
        /// 创建节点图节点
        /// </summary >
        private void CreateGraphNode(Dictionary<AIBehaviour, GraphBehaviourView> nodeDict, List<AIBehaviour> allNodes)
        {         
            foreach (AIBehaviour node in allNodes)
            {
                GraphBehaviourView nodeView = new GraphBehaviourView();
                nodeView.Init(behaviourTree, node);               
                nodeView.onNodeSelected += BehaviourSelected;
                AddElement(nodeView);
                nodeDict.Add(node, nodeView);
            }
        }

        /// <summary>
        /// 构建节点连接
        /// </summary>
        private void BuildConnect(Dictionary<AIBehaviour, GraphBehaviourView> nodeDict, List<AIBehaviour> allNodes)
        {
            foreach (AIBehaviour node in allNodes)
            {
                GraphBehaviourView nodeView = nodeDict[node];
                if (nodeView.outputContainer.childCount == 0)
                {
                    //不需要子节点 跳过
                    continue;
                }

                Port selfOutput = (Port)nodeView.outputContainer[0];

                //与当前节点的子节点进行连线
                node.ForEach((child =>
                {
                    if (child == null)
                    {
                        return;
                    }

                    GraphBehaviourView childNodeView = nodeDict[child];

                    Port childInput = (Port)childNodeView.inputContainer[0];
                    Edge edge = selfOutput.ConnectTo(childInput);
                    AddElement(edge);
                }));
            }
        }
    
        /// <summary>
        /// 获取可连线的节点列表
        /// </summary>
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            foreach (var port in ports.ToList())
            {
                if (startPort.node == port.node || startPort.direction == port.direction)
                {
                    //不能自己连自己
                    //只能input和output连接
                    continue;
                }

                compatiblePorts.Add(port);
            }

            return compatiblePorts;

        }  
    }
}
#endif