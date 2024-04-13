#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using System.Linq;
using System;
using YukiFrameWork.Extension;
using YukiFrameWork.DiaLog;
using Node = YukiFrameWork.DiaLog.Node;

public class NodeTreeViewer : GraphView
{
    public Action<NodeView> OnNodeSelected;
    public new class UxmlFactory : UxmlFactory<NodeTreeViewer, GraphView.UxmlTraits> { }
    NodeTree tree;
    public NodeTreeViewer()
    {
        Insert(0, new GridBackground());

        // 添加视图缩放
        this.AddManipulator(new ContentZoomer());
        // 添加视图拖拽
        this.AddManipulator(new ContentDragger());
        // 添加选中对象拖拽
        this.AddManipulator(new SelectionDragger());
        // 
        this.AddManipulator(new RectangleSelector());
        var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ImportSettingWindow.GetData().path + "/DiaLogKit/Editor/UI/NodeTreeViewer.uss");
        styleSheets.Add(styleSheet);

        // 添加撤回方法OnUndoRedo绑定
        Undo.undoRedoPerformed += OnUndoRedo;
    }

    private void OnUndoRedo()
    {
        PopulateView(tree);
        AssetDatabase.SaveAssets();
    }
    // NodeTreeViewer视图中添加右键节点创建栏
    public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
    {
        {
            var types = TypeCache.GetTypesDerivedFrom<CompositeNode>();
            foreach (var type in types)
            {
                if (!type.IsAbstract)
                {
                    evt.menu.AppendAction($"[Composite-{type.BaseType.Name}] 创建分支节点", (a) => CreateNode(type)
                    , tree.nodes.Count == 0 ? DropdownMenuAction.Status.Disabled : DropdownMenuAction.Status.Normal);
                }
            }
        }
        {
            var types = TypeCache.GetTypesDerivedFrom<SingleNode>();
            foreach (var type in types)
            {
                if (!type.IsAbstract)
                {
                    DropdownMenuAction.Status status = DropdownMenuAction.Status.Normal;
                    if (tree.nodes.Count == 0)
                    {
                        if (type.BaseType != typeof(RootNode))
                            status = DropdownMenuAction.Status.Disabled;
                    }
                    string menuName = string.Empty;
                    if (type.BaseType == typeof(RootNode))
                        menuName = "创建根节点";
                    else menuName = "创建默认节点";
                    evt.menu.AppendAction($"[Single-{type.BaseType.Name}] {menuName}", (a) => CreateNode(type), status);
                }
            }
        }
    }

    void CreateNode(System.Type type)
    {
        // 创建运行时节点树上的对应类型节点
        Node node = tree.CreateNode(type);
        CreateNodeView(node);
    }

    void CreateNodeView(Node node)
    {
        // 创建节点UI
        NodeView nodeView = new NodeView(node);
        // 节点创建成功后 让nodeView.OnNodeSelected与当前节点树上的OnNodeSelected关联 让该节点属性显示在InspectorViewer上
        nodeView.OnNodeSelected = OnNodeSelected;
        // 将对应节点UI添加到节点树视图上
        AddElement(nodeView);
    }

    NodeView FindNodeView(Node node)
    {
        return GetNodeByGuid(node.guid) as NodeView;
    }

    // 每次选中打开节点树时都进行重新的视图绘制
    internal void PopulateView(NodeTree tree)
    {
        this.tree = tree;
        // 在节点树视图重新绘制之前需要取消视图变更方法OnGraphViewChanged的订阅
        // 以防止视图变更记录方法中的信息是上一个节点树的变更信息
        graphViewChanged -= OnGraphViewChanged;
        // 清除之前渲染的graphElements图层元素
#if UNITY_2021_1_OR_NEWER
        DeleteElements(graphElements);
#else
        DeleteElements(graphElements.ToList());
#endif
        // 在清除节点树视图所有的元素之后重新订阅视图变更方法OnGraphViewChanged
        graphViewChanged += OnGraphViewChanged;

        // 是否一定需要根节点
        if (tree != null)
        {
            EditorUtility.SetDirty(tree);
            AssetDatabase.SaveAssets();
        }

        // 遍历当前节点树中的节点集合并绘制到节点树视图当中
        try
        {
            tree.nodes.ForEach(n => CreateNodeView(n));
            // 遍历当前节点树中的所有的父子节点关系并对应的链接线绘制到节点树视图当中
            // 当前遍历出所有节点均视为父节点 n
            tree.nodes.ForEach(n =>
            {
                // 通过GetChildren方法获取到每个节点n下的子节点c
                var children = tree.GetChildren(n);
                children.ForEach(c =>
                {
                    // 通过遍历每个节点guid进行对应链接
                    NodeView parentView = FindNodeView(n);
                    NodeView childView = FindNodeView(c);
                    // 绘制父节点出口到子节点入口的链接线
                    Edge edge = parentView.output.ConnectTo(childView.input);
                    AddElement(edge);
                });
            });
        }
        catch { }

    }

    // 只要节点树视图发生改变就会触发OnGraphViewChanged方法
    private GraphViewChange OnGraphViewChanged(GraphViewChange graphViewChange)
    {
        // 对所有删除进行遍历记录 只要视图内有元素删除进行判断
        if (graphViewChange.elementsToRemove != null)
        {
            graphViewChange.elementsToRemove.ForEach(elem =>
            {
                // 找到节点树视图中删除的NodeView
                NodeView nodeView = elem as NodeView;
                if (nodeView != null)
                {
                    // 并将该NodeView所关联的运行时节点删除
                    tree.DeleteeNode(nodeView.node);
                }
                // 若该删除元素为链接线 则删除父节点中子节点列里对应的子节点
                Edge edge = elem as Edge;
                if (edge != null)
                {
                    NodeView parentView = edge.output.node as NodeView;
                    NodeView childView = edge.input.node as NodeView;
                    tree.RemoveChild(parentView.node, childView.node);
                }
            });
        }
        if (graphViewChange.edgesToCreate != null)
        {
            graphViewChange.edgesToCreate.ForEach(edge =>
            {
                NodeView parentView = edge.output.node as NodeView;
                NodeView childView = edge.input.node as NodeView;
                tree.AddChild(parentView.node, childView.node);
            });
        }
        // 对所有节点移动进行遍历记录
        // if(graphViewChange.movedElements != null){
        //     nodes.ForEach((n)=>{
        //         NodeView view = n as NodeView;
        //         view .SortChildren();
        //     });
        // }
        return graphViewChange;
    }

    // 获取节点兼容接口
    public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
    {
        /* 
            返回兼容接口列表
            需要判断接入的兼容接口仅为入口 endport.direction != startPort.direction
            且同个节点的出口不能接入自己的入口 endport.node != startPort.node
        */
        return ports.ToList().Where(
            endport => endport.direction != startPort.direction
            && endport.node != startPort.node).ToList();
    }

    public void UpdateNodeStates()
    {
        nodes.ForEach(n =>
        {
            NodeView view = n as NodeView;
            view.UpdateState();
        });
    }
}
#endif