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
        internal NodeTree tree;
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

            void Update() { onNodeUpdate?.Invoke(this); }
            DiaLogGraphWindow.onUpdate -= Update;
            DiaLogGraphWindow.onUpdate += Update;

            DiaLogGraphWindow.OnValidate += () => 
            {
                Pop(tree);
            };

        }    
       
        public void Pop(NodeTree tree)
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
                    GraphNodeView parent = FindNodeViewByGUID(x.id);
                    if (parent != null)
                    {                       
                        if (parent.node.IsRandom)
                        {
                            foreach (var item in parent.node.randomItems)
                            {
                                GraphNodeView child = FindNodeViewByGUID(item.id);
                                if (child == null) continue;

                                Port po = parent.outputport;

                                if (po == null) continue;

                                Edge edge = po.ConnectTo(child.inputPort);
                                AddElement(edge);
                            }
                        }
                        else if (parent.node.IsComposite)
                        {
                            foreach (var item in parent.node.optionItems)
                            {
                                GraphNodeView child = FindNodeViewByGUID(item.nextNode.id);
                                if (child != null)
                                {
                                    Port po = parent.outputport;
                                    if (po != null)
                                    {
                                        Edge edge = po.ConnectTo(child.inputPort);
                                        AddElement(edge);
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (x.child != null)
                            {
                                GraphNodeView child = FindNodeViewByGUID(x.child.id);
                                if (child != null)
                                {
                                    Port po = parent.outputport;
                                    if (po != null)
                                    {
                                        Edge edge = po.ConnectTo(child.inputPort);
                                        AddElement(edge);
                                    }
                                }
                            }
                        }

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
                    if (parent.node.IsComposite)
                    {
                        Option option = null;
                        foreach (var item in parent.node.optionItems)
                        {
                            if (item.nextNode == ((GraphNodeView)edge.input.node).node)
                            {
                                option = item;
                                break;
                            }
                        }
                        parent.node.optionItems.Remove(option);
                    }
                    else if (parent.node.IsRandom)
                    {
                        Node node = null;
                        foreach (var item in parent.node.RandomItems)
                        {
                            if (item == ((GraphNodeView)edge.input.node).node)
                            {
                                node = item;
                                break;
                            }
                        }

                        parent.node.RandomItems.Remove(node);
                    }
                    else
                    {
                        parent.node.child = null;
                    }

                    EditorUtility.SetDirty(parent.node);
                    AssetDatabase.SaveAssets();

                }
            });

            graphViewChange.edgesToCreate?.ForEach(x => 
            {
                GraphNodeView nodeView = x.output.node as GraphNodeView;
                GraphNodeView nodeChild = x.input.node as GraphNodeView;
                if (nodeView.node.IsComposite)
                {
                    nodeView.node.optionItems.Add(new Option()
                    {
                        nextNode = nodeChild.node
                    });
                }
                else if (nodeView.node.IsRandom)
                {
                    nodeView.node.randomItems.Add(nodeChild.node);
                }
                else
                {
                    nodeView.node.child = nodeChild.node;
                }

                EditorUtility.SetDirty(nodeView.node);
                AssetDatabase.SaveAssets();
            });

            return graphViewChange;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (tree == null) return;

            Vector2 pos = evt.localMousePosition;
            bool c = false;
            foreach (Type type in TypeCache.GetTypesDerivedFrom<Node>())
            {
                string menu = string.Empty;
                if (type.IsAbstract) continue;               
                DiaLogueNodeAttribute nodeAttribute = type.GetCustomAttributes<DiaLogueNodeAttribute>().FirstOrDefault();

                if (nodeAttribute == null) continue;
                c = true;
                if (nodeAttribute is RootNodeAttribute)
                {
                    if (tree.rootNode != null)
                        continue;

                    menu = $"创建根节点Node --- {type}";
                }
                else if (nodeAttribute is CompositeNodeAttribute)
                {
                    menu = $"创建分支节点Node --- {type}";
                }
                else if (nodeAttribute is SingleNodeAttribute)
                {
                    menu = $"创建默认节点Node --- {type}";
                }
                else if (nodeAttribute is RandomNodeAttribute)
                {
                    menu = $"创建随机节点Node --- {type}";
                }
                if (IsNotRoot() && !type.HasCustomAttribute<RootNodeAttribute>())
                {
                    evt.menu.AppendAction(menu, null, DropdownMenuAction.Status.Disabled);
                }
                else
                {
                    evt.menu.AppendAction(menu, x =>
                    {
                        Node node = tree.CreateNode(type);
                        var item = BuildNodeView(node);
                        item.SetPosition(new Rect(item.contentRect) {x = pos.x,y = pos.y });     
                    }, DropdownMenuAction.Status.Normal);
                }
            }
            if (!c)
            {
                evt.menu.AppendAction("当前没有创建任何继承Node的节点类,无法添加", null, DropdownMenuAction.Status.Disabled);
            }
            bool IsNotRoot()
            {
                return tree.rootNode == null && !tree.nodes.Find(x => x.DiscernAttribute is RootNodeAttribute);
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

        private GraphNodeView BuildNodeView(Node node)
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