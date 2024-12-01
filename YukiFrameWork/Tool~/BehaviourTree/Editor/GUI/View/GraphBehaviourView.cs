///=====================================================
/// - FileName:      GraphBehaviourView.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   高级定制脚本生成
/// - Creation Time: 2024/11/14 17:25:18
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;

#if UNITY_EDITOR
using UnityEditor.UIElements;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using YukiFrameWork.Extension;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace YukiFrameWork.Behaviours
{
    public class GraphBehaviourView : Node
    {
        private static Color inputPortColor = Color.cyan;
        private static Color outputPortColor = Color.cyan;

        private static Color compositeColor = new Color(81 / 255f, 81 / 255f, 81 / 255f, 255f / 255);
        private static Color decoratorColor = new Color(81 / 255f, 81 / 255f, 81 / 255f, 255f / 255);
        private static Color actionColor = new Color(81 / 255f, 81 / 255f, 81 / 255f, 255f / 255);
        private static Color conditionColor = new Color(81 / 255f, 81 / 255f, 81 / 255f, 255f / 255);
        private static Color freeColor = new Color(81 / 255f, 81 / 255f, 81 / 255f, 255f / 255);
        private static Color runningColor = new Color(38 / 255f, 130 / 255f, 205 / 255f, 255f / 255);
        private static Color successColor = new Color(36 / 255f, 178 / 255f, 50 / 255f, 255f / 255);
        private static Color failedColor = new Color(203 / 255f, 81 / 255f, 61 / 255f, 255f / 255);

        private BehaviourTreeSO behaviourTree;
        private Port inputPort;
        private Port outputPort;
        private EnumField stateField;

        public AIBehaviour Behaviour;

        public event System.Action<GraphBehaviourView> onNodeSelected = null;
        
        public override void OnSelected()
        {
            base.OnSelected();         
            onNodeSelected?.Invoke(this);
        }

        /// <summary>
        /// 获取节点在节点图的名字
        /// </summary>
        public static string GetNodeDisplayName(AIBehaviour behaviour,Type type)
        {          
            return behaviour.Description.IsNullOrEmpty() ? type.Name : behaviour.Description;
        }

        /// <summary>
        /// 创建节点
        /// </summary>
        public static GraphBehaviourView Create(Type type, BehaviourTreeSO behaviourTree, BackGroundView view, Vector2 pos)
        {

            AIBehaviour runtimeNode = behaviourTree.Create(type);
            runtimeNode.position = pos;
            if (runtimeNode is AIRootBehaviour rootBehaviour)            
                behaviourTree.Root = rootBehaviour;
           

            GraphBehaviourView nodeView = new GraphBehaviourView();
            nodeView.Init(behaviourTree, runtimeNode);
            nodeView.onNodeSelected += view.BehaviourSelected;
            view.AddElement(nodeView);
            Undo.RecordObject(behaviourTree, $"Create {type.Name} Behaviour");
            behaviourTree.Save();         
            return nodeView;
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(BehaviourTreeSO behaviourTree, AIBehaviour behaviour)
        {
            this.behaviourTree = behaviourTree;
            Behaviour = behaviour;

            if (Behaviour is AIRootBehaviour)
            {
                //根节点不可删除
                capabilities -= Capabilities.Deletable;
            }          
            SetNameAndPos();
            SetVertical();
            AddPort();
            SetNodeColor();
            AddStateField();
            BehaviourTreeGraphWindow.onValidate -= OnValidate;
            BehaviourTreeGraphWindow.onValidate += OnValidate;            
            behaviour.onValidate -= OnValidate;
            behaviour.onValidate += OnValidate;
            onValidate = view => 
            {
                view.SetNameAndPos();
            };          
        }
        public System.Action<GraphBehaviourView> onValidate = null;
        private void OnValidate() => onValidate?.Invoke(this);
        /// <summary>
        /// 设置节点名和位置
        /// </summary>
        private void SetNameAndPos()
        {           
            Type nodeType = Behaviour.GetType();
            title = (Behaviour is Composite composite) ? GetNodeDisplayName(Behaviour,nodeType) + $" <color=green>{composite.AbortType}</color>" : GetNodeDisplayName(Behaviour, nodeType);
            
            SetPosition(new Rect(Behaviour.position, GetPosition().size));
        }

        /// <summary>
        /// 将端口方向改成垂直的
        /// </summary>
        private void SetVertical()
        {
            var titleButtonContainer = contentContainer.Q<VisualElement>("title-button-container");
            titleButtonContainer.RemoveAt(0); //删掉收起箭头 否则会有bug

            var titleContainer = this.Q<VisualElement>("title");
            var topContainer = this.Q("input");
            var bottomContainer = this.Q("output");

            var nodeBorder = this.Q<VisualElement>("node-border");
            nodeBorder.RemoveAt(0);
            nodeBorder.RemoveAt(0);

            nodeBorder.Add(topContainer);
            nodeBorder.Add(titleContainer);
            nodeBorder.Add(bottomContainer);
        }

        /// <summary>
        /// 根据节点类型添加端口
        /// </summary>
        private void AddPort()
        {
            if (!(Behaviour is AIRootBehaviour))
            {
                inputPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Single, typeof(bool));
                inputPort.portName = "父节点";
                inputPort.portColor = inputPortColor;                
                inputContainer.Add(inputPort);
            }

            if ((Behaviour is Action) || (Behaviour is Condition))
                return;
            if (!Behaviour.GetType().HasCustomAttribute<ChildModeInfoAttribute>(true, out ChildModeInfoAttribute capacityInfo))
                return;
            Port.Capacity outputCount;
            if (capacityInfo.ChildMode == ChildMode.Single)
            {
                outputCount = Port.Capacity.Single;
            }
            else
            {
                outputCount = Port.Capacity.Multi;
            }

            outputPort = InstantiatePort(Orientation.Vertical, Direction.Output, outputCount, typeof(bool));
            outputPort.portName = "子节点";
            outputPort.portColor = outputPortColor;
            outputContainer.Add(outputPort);
        }


        /// <summary>
        /// 设置节点颜色
        /// </summary>
        private void SetNodeColor()
        {
            Color borderColor = default;

            if (Behaviour is Action)
            {
                borderColor = actionColor;
            }
            else if (Behaviour is Composite)
            {
                borderColor = compositeColor;
            }
            else if (Behaviour is Decorator)
            {
                borderColor = decoratorColor;
            }
            else if (Behaviour is Condition)
            {
                borderColor = conditionColor;
            }
            else
            {
                return;
            }

            var nodeBorder = this.Q<VisualElement>("node-border");
            nodeBorder.style.borderTopColor = borderColor;
            nodeBorder.style.borderBottomColor = borderColor;
            nodeBorder.style.borderLeftColor = borderColor;
            nodeBorder.style.borderRightColor = borderColor;
        }

        /// <summary>
        /// 添加状态显示UI
        /// </summary>
        private void AddStateField()
        {
            var nodeBorder = contentContainer.Q<VisualElement>("node-border");
            stateField = new EnumField(Behaviour.Status);
            nodeBorder.Insert(2, stateField);

            IMGUIContainer imguiContainer = new IMGUIContainer();
            imguiContainer.onGUIHandler += RefreshNodeState;           
            nodeBorder.Add(imguiContainer);
        }

        /// <summary>
        /// 刷新节点状态显示
        /// </summary>
        private void RefreshNodeState()
        {
            stateField.value = Behaviour.Status;

            var element = stateField.ElementAt(0);

            Color color = default;
            switch (Behaviour.Status)
            {
                case BehaviourStatus.InActive:
                    color = freeColor;
                    break;
                case BehaviourStatus.Running:
                    color = runningColor;
                    break;
                case BehaviourStatus.Success:
                    color = successColor;
                    break;
                case BehaviourStatus.Failed:
                    color = failedColor;
                    break;
            }

            element.style.backgroundColor = color;
        }

        /// <summary>
        /// 设置位置
        /// </summary>
        public void SetPos(Rect newPos)
        {
            Undo.RecordObject(behaviourTree, $"SetPosition {this}");

            SetPosition(newPos);
            Behaviour.position = newPos.position;
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        public void AddChild(GraphBehaviourView child, BackGroundView view)
        {
            var info = Behaviour.GetType().HasCustomAttribute<ChildModeInfoAttribute>(true, out ChildModeInfoAttribute childModeInfo);
            if (!info) return;

            Undo.RecordObject(behaviourTree, $"AddChild {this}");

            //如果当前节点的子节点容量为single 就先清空子节点
            if (childModeInfo.ChildMode == ChildMode.Single)
            {
                ClearChild(view);
            }

            //如果要添加的子节点已有父节点了 就将它从旧的父节点那里删掉
            if (child.inputPort.connected)
            {
                var edgeToOldParent = child.inputPort.connections.FirstOrDefault();
                var oldParent = (GraphBehaviourView)edgeToOldParent.output.node;
                oldParent.RemoveChild(child, view);
            }

            //添加子节点
            Behaviour.AddChild(child.Behaviour);
            var edge = outputPort.ConnectTo(child.inputPort);
            view.AddElement(edge);
            behaviourTree.Save();
        }       
        /// <summary>
        /// 删除子节点
        /// </summary>
        public void RemoveChild(GraphBehaviourView child, BackGroundView view)
        {
            Undo.RecordObject(behaviourTree, $"RemoveChild {this}");

            Behaviour.RemoveChild(child.Behaviour);

            var edge = child.inputPort.connections.FirstOrDefault();
            outputPort.Disconnect(edge);

            view.RemoveElement(edge);
            behaviourTree.Save();
        }

        /// <summary>
        /// 清空子节点
        /// </summary>
        public void ClearChild(BackGroundView view)
        {
            Undo.RecordObject(behaviourTree, $"ClearChild {this}");

            Behaviour.Clear();
            if (outputPort != null)
            {
                //遍历output端口的所有线 让线的input端口都断开连接 并删除线
                foreach (Edge edge in outputPort.connections.ToList())
                {
                    edge.input.DisconnectAll();
                    view.RemoveElement(edge);
                }

                //断开output断开的所有连接
                outputPort.DisconnectAll();
            }
            behaviourTree.Save();
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        public void RemoveSelf()
        {
            Undo.RecordObject(behaviourTree, $"RemoveNode {this}");

            behaviourTree.Remove(Behaviour);
        }

        public override string ToString()
        {
            return Behaviour.ToString();
        }

    }
}
#endif