///=====================================================
/// - FileName:      GraphNodeView.cs
/// - NameSpace:     YukiFrameWork.DiaLogue
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/7/28 15:37:02
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================

#if UNITY_EDITOR
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEditor.Experimental.GraphView;
namespace YukiFrameWork.DiaLogue
{
    public class GraphNodeView : UnityEditor.Experimental.GraphView.Node
    {
        public event Action<GraphNodeView> onNodeSelected = null;       
        public INode node { get; private set; }
        public Port outputport { get; private set; }       
        public GraphNodeView(INode node)
        {          
            this.node = node;         
            this.name = node.Name;
            title = node.GetType().Name + "  " + node.Name;
            titleContainer.style.color = Color.blue;
            viewDataKey = node.GetHashCode().ToString();
            style.left = node.NodePosition.x;
            style.top = node.NodePosition.y;
            CreateInputPorts();
            CreateOutPutPorts();

            //node.onValidate += OnValidate;
            DiaLogGraphWindow.OnValidate += OnValidate;
            style.fontSize = 20;
                   
            BackGroundView.onNodeUpdate -= UpdateView; 
            BackGroundView.onNodeUpdate +=  UpdateView;                  
        }

        private void UpdateView(BackGroundView backGroundView)
        {
            if (!backGroundView.tree.IsPerformance)
            {
                UpdateInfo();
            }
            if (!Application.isPlaying) return;

            if (backGroundView.tree == null) return;

            if (DiaLogKit.RuntimeControllers.TryGetValue(backGroundView.tree.Key, out var controller))
            {
                if (controller.CurrentNode == node)
                {
                    style.backgroundColor = GetButtonColor();
                }
            }
        }

        private static Color GetButtonColor()
        {
            return Color.HSVToRGB(Mathf.Cos((float)UnityEditor.EditorApplication.timeSinceStartup + 1f) * 0.225f + 0.325f, 1, 1);
        }

        public event Action<GraphNodeView> onNodeValidate = null;

        private void OnValidate()
        {         
            onNodeValidate?.Invoke(this);
        }      
  
        private void CreateOutPutPorts()
        {
            if (node == null) return;
            Create(node);
        }       

        private void Create(INode item) 
        {                     
            Port o = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(bool));
            o.name = "OutPut";
            o.portName = item.Text;
            o.portColor = Color.green;
            outputport = o;
            outputContainer.Add(o);
        }
   
        private void CreateInputPorts()
        {
            if (!node.IsRoot)
            {
                Port i = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(bool));

                i.name = "Input";
                i.portName = string.Empty;
                i.portColor = Color.green;
                inputPort = i;
                inputContainer.Add(i);
            }
        }
     
        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);

            node.NodePosition = new Node.Position(newPos.x,newPos.y);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            onNodeSelected?.Invoke(this);
        }

        internal void ResetBuildOutPutPorts(BackGroundView backGroundView)
        {
            UpdateInfo();   
                 
            foreach (var edge in outputport.connections)
            {
                edge.input.DisconnectAll();
                backGroundView.RemoveElement(edge);
            }               
        }

        public void UpdateInfo()
        {
            if (node == null) return;
            title = node.GetType().Name + "  " + node.Name;
            outputport.portName = node.Text;
        }

        public Port inputPort { get; private set; }
    }
}
#endif