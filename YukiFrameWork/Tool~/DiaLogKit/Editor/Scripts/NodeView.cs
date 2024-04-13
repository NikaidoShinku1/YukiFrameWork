using System;
#if UNITY_EDITOR
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using YukiFrameWork.Extension;

namespace YukiFrameWork.DiaLog
{
    public class NodeView : UnityEditor.Experimental.GraphView.Node
    {
        public Action<NodeView> OnNodeSelected;
        public Node node;
        public Port input;
        public Port output;
        public NodeView(Node node) : base(ImportSettingWindow.GetData().path + "/DiaLogKit/Editor/UI/NodeView.uxml")
        {
            this.node = node;
            this.title = node.name;
            // 将guid作为Node类中的viewDataKey关联进行后续的视图层管理
            this.viewDataKey = node.guid;
            style.left = node.position.x;
            style.top = node.position.y;

            CreateInputPorts();
            CreateOutputPorts();
            SetupClass();
        }

        private void CreateInputPorts()
        {
            // 默认所有节点为多入口类型
            input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
            // 判断是否为单独节点 则将出口改为单入口
            if (node is SingleNode)
            {
                input = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
            }

            if (node is RootNode)
            {
                input = null;
            }
            if (input != null)
            {
                // 将端口名设置为空
                input.portName = "";
                // 因为input端口的label默认是在端口右边因此需要将排列方式调整为竖向自上往下
                input.style.flexDirection = FlexDirection.Column;
                inputContainer.Add(input);
            }
        }

        private void CreateOutputPorts()
        {
            // 默认所有节点为单出口类型
            output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Single, typeof(bool));
            // 判断是否为复合型节点 则将出口改为多出口
            if (node is CompositeNode)
            {
                output = InstantiatePort(Orientation.Vertical, Direction.Output, Port.Capacity.Multi, typeof(bool));
            }

            if (output != null)
            {
                output.portName = "";
                // 因为output端口的label默认是在端口左边边因此需要将排列方式调整为竖向自下往上
                output.style.flexDirection = FlexDirection.ColumnReverse;
                outputContainer.Add(output);
            }
        }
        private void SetupClass()
        {
            if (node is SingleNode)
            {
                AddToClassList("single");
            }
            else if (node is CompositeNode)
            {
                AddToClassList("composite");
            }
            else if (node is RootNode)
            {
                AddToClassList("root");
            }
        }
        // 设置节点在节点树视图中的位置
        public override void SetPosition(Rect newPos)
        {
            // 将视图中节点位置设置为最新位置newPos
            base.SetPosition(newPos);
            // 撤回记录
            Undo.RecordObject(node, "Node Tree(Set Position)");
            // 将最新位置记录到运行时节点树中持久化存储
            node.position.x = newPos.xMin;
            node.position.y = newPos.yMin;
            EditorUtility.SetDirty(node);
        }

        // 复写Node类中的选中方法OnSelected
        public override void OnSelected()
        {
            base.OnSelected();
            // 如果当前OnNodeSelected选中部位空则将该节点视图传递到OnNodeSelected方法中视为选中
            if (OnNodeSelected != null)
            {
                OnNodeSelected.Invoke(this);
            }
        }

        public void UpdateState()
        {
            RemoveFromClassList("running");
            RemoveFromClassList("Waiting");
            if (Application.isPlaying)
            {
                switch (node.state)
                {
                    case NodeState.Running:
                        AddToClassList("running");
                        break;
                    case NodeState.Waiting:
                        AddToClassList("Waiting");
                        break;
                }
            }
        }
    }
}
#endif