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
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;
namespace YukiFrameWork.DiaLogue
{
    public class GraphNodeView : UnityEditor.Experimental.GraphView.Node
    {
        public event Action<GraphNodeView> onNodeSelected = null;       
        public Node node { get; private set; }
        public Port outputport { get; private set; }       
        public GraphNodeView(Node node)
        {          
            this.node = node;         
            this.name = node.name;
            title = node.GetType().Name + "  " + node.GetName();
            titleContainer.style.color = Color.blue;
            viewDataKey = node.id;
            style.left = node.position.x;
            style.top = node.position.y;
            CreateInputPorts();
            CreateOutPutPorts();

            node.onValidate += OnValidate;
            DiaLogGraphWindow.OnValidate += OnValidate;
            style.fontSize = 20;

            void Update_View(BackGroundView view)
            {
                
                if (!Application.isPlaying) return;

                if (view.tree == null) return;

                if (view.tree.runningNode == null) return;
                if (view.tree.runningNode == node)
                {
                    style.backgroundColor = Color.cyan;
                }
                else
                {
                    style.backgroundColor = default;
                }
            }
            BackGroundView.onNodeUpdate -= Update_View;
            BackGroundView.onNodeUpdate +=  Update_View;                  
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

        private void Create(Node item) 
        {                     
            Port o = InstantiatePort(Orientation.Horizontal, Direction.Output, (node.IsComposite || node.IsRandom) ? Port.Capacity.Multi : Port.Capacity.Single, typeof(bool));
            o.name = "OutPut";
            o.portName = item.GetContext();
            o.portColor = Color.green;
            outputport = o;
            outputContainer.Add(o);
        }
   
        private void CreateInputPorts()
        {
            if (!node.GetType().HasCustomAttribute<RootNodeAttribute>())
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

            node.position = new Node.Position(newPos.x,newPos.y);
        }

        public override void OnSelected()
        {
            base.OnSelected();
            onNodeSelected?.Invoke(this);
        }

        internal void ResetBuildOutPutPorts(BackGroundView backGroundView)
        {          
            title = node.GetType().Name + "  " + node.GetName();
            outputport.portName = node.GetContext();                           
                 
            foreach (var edge in outputport.connections)
            {
                edge.input.DisconnectAll();
                backGroundView.RemoveElement(edge);
            }               
        }

        public Port inputPort { get; private set; }
    }
}
#endif