///=====================================================
/// - FileName:      BackGroundView.cs
/// - NameSpace:     YukiFrameWork.DiaLogue
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/7/27 19:08:43
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using YukiFrameWork.Extension;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEditor;
namespace YukiFrameWork.DiaLogue
{
	public class BackGroundView : GraphView
	{
        public new class UxmlFactory : UxmlFactory<BackGroundView, UxmlTraits> { }

        public event Action<GraphNodeView> onNodeSelected = null;
        public static event Action<BackGroundView> onNodeUpdate = null;
        internal NodeTreeBase tree;
        private Vector2 startPosition;

        private static NodeTreeBase[] allNodeTreeBases;

        private IEnumerable<NodeTreeBase.ColorTip> allNodeTypes => allNodeTreeBases?
            .SelectMany(x => x.allNodeColorTypeTips);           
       
		public BackGroundView() 
		{
            Insert(0, new GridBackground());
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
            var importPath = ImportSettingWindow.GetData().path;
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(importPath + "/DiaLogKit/Editor/GUI/BackGround.uss");
            styleSheets.Add(styleSheet);

            allNodeTreeBases = YukiAssetDataBase.FindAssets<NodeTreeBase>();
           
            DiaLogGraphWindow.onUpdate -= Update;
            DiaLogGraphWindow.onUpdate += Update;

            DiaLogGraphWindow.OnValidate += () => 
            {
                Pop(tree);
                if (tree.IsPerformance)
                    UpdateColor();
            };

            startPosition = viewTransform.position;

        }
       
        private void Update()
        {           
            onNodeUpdate?.Invoke(this);
            if (!tree) return;
            if(!tree.IsPerformance)
                UpdateColor();
        }

        private void UpdateColor()
        {
            foreach (var node in graphElements)
            {
                if (node is GraphNodeView view)
                {
                    if (view.node.IsRoot)
                        view.style.backgroundColor = tree.rootColorTip;
                    else 
                    {
                        if (Application.isPlaying)
                        {
                            if (DiaLogKit.RuntimeControllers.TryGetValue(tree.Key, out var controller))
                            {
                                if (controller.CurrentNode == view.node)                               
                                    continue;                                
                            }
                        }
                        view.style.backgroundColor = default;
                        if (allNodeTreeBases == null || allNodeTreeBases.Length == 0) return;

                        foreach (var item in allNodeTypes)
                        {
                            if (item.key == view.node.NodeType)
                                view.style.backgroundColor = item.color;
                        }
                    }
                }
            }
        }
        public void Pop(NodeTreeBase tree)
        {
            this.tree = tree;
            graphViewChanged -= OnGraphViewChanged;

            graphElements.ForEach(e => 
            {
                if (e is GraphNodeView view)
                {
                    view.onNodeSelected -= NodeSelected;

                    RemoveElement(e);
                }
            });

            graphViewChanged += OnGraphViewChanged; 

            if (tree != null)
            {
                tree.ForEach(x => BuildNodeView(x));
                tree.ForEach(x =>
                {
                    GraphNodeView parent = FindNodeViewByGUID(x.GetHashCode().ToString());
                    if (parent != null)
                    {
                        tree.ForEach(item =>
                        {
                            if (x == item) return;
                            if (x.LinkNodes.Contains(item.Id))
                            {
                                GraphNodeView child = FindNodeViewByGUID(item.GetHashCode().ToString());
                                Port po = parent.outputport;

                                if (po == null) return;

                                Edge edge = po.ConnectTo(child.inputPort);
                                AddElement(edge);
                            }
                        });                      
                    }
                });
            }
        }

        public GraphNodeView FindNodeViewByGUID(string id)
        {
            return GetNodeByGuid(id) as GraphNodeView;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
        {
            if (tree == null) return graphViewChange;

            graphViewChange.elementsToRemove?.ForEach(x =>
            {
                if (x is GraphNodeView nodeView)
                {
                    nodeView.onNodeSelected -= NodeSelected;
                    tree.DeleteNode(nodeView.node);
                }
                else if (x is Edge edge)
                {                  
                    GraphNodeView parent = edge.output.node as GraphNodeView;
                    int id = ((GraphNodeView)edge.input.node).node.Id;
                    if (parent.node.LinkNodes.Contains(id))
                        parent.node.LinkNodes.Remove(id);                                     
                    AssetDatabase.SaveAssets();

                }
            });

            graphViewChange.edgesToCreate?.ForEach(x => 
            {
                GraphNodeView nodeView = x.output.node as GraphNodeView;
                GraphNodeView nodeChild = x.input.node as GraphNodeView;
                if (!nodeView.node.LinkNodes.Contains(nodeChild.node.Id))
                    nodeView.node.LinkNodes.Add(nodeChild.node.Id);               
                AssetDatabase.SaveAssets();
            });

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (tree == null) return;

            var pos = ((evt.localMousePosition - ((Vector2)viewTransform.position - startPosition))) * (1 / scale);
            // bool c = false;
            foreach (Type type in TypeCache.GetTypesDerivedFrom<INode>())
            {
                string menu = string.Empty;
                if (type.IsAbstract) continue; 
                
                menu = graphElements.Count() == 0 ? "创建根节点" : $"创建新的对话节点";
                evt.menu.AppendAction(menu, x => 
                {
                    INode node = tree.CreateNode(type);
                    var item = BuildNodeView(node);
                    item.SetPosition(new Rect(item.contentRect) { x = pos.x, y = pos.y });
                }, DropdownMenuAction.Status.Normal);

                ;               
            }

            
            
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            return ports.ToList().Where(endPorts =>
                endPorts.direction != startPort.direction
                && endPorts.node != startPort.node &&
                endPorts.portType == startPort.portType
            ).ToList();
        }

        private GraphNodeView BuildNodeView(INode node)
        {
            GraphNodeView item = new GraphNodeView(node);
            item.onNodeSelected += NodeSelected;
            item.onNodeValidate += NodeValidate;        
            AddElement(item);

            return item;
        }
  
        private void NodeSelected(GraphNodeView nodeView)
        {
            onNodeSelected?.Invoke(nodeView);
        }

        private void NodeValidate(GraphNodeView node)
        {
            graphViewChanged -= OnGraphViewChanged;
            node.ResetBuildOutPutPorts(this);
            graphViewChanged += OnGraphViewChanged;          
        }
    }
}
#endif