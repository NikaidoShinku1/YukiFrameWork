///=====================================================
/// - FileName:      GraphBehaviourView.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   高级定制脚本生成
/// - Creation Time: 2024/11/14 17:25:18
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using System.ComponentModel;


#if UNITY_EDITOR
using UnityEditor.UIElements;
using UnityEngine;
using System;
using UnityEngine.UIElements;
using YukiFrameWork.Extension;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using System.Linq;

namespace YukiFrameWork.Missions
{
    public class GraphMissionView : Node
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

        private MissionTreeSO missionTree;
        private Port inputPort;
        private Port outputPort;
        private EnumField stateField;

        public Mission Mission;

        public event System.Action<GraphMissionView> onNodeSelected = null;
        
        public override void OnSelected()
        {
            base.OnSelected();         
            onNodeSelected?.Invoke(this);
        }

        /// <summary>
        /// 获取节点在节点图的名字
        /// </summary>
        public static string GetNodeDisplayName(Mission behaviour,Type type)
        {
            return $"{behaviour.MissionName}:{behaviour.Description}";
        }
        private BackGroundView backGroundView;
        /// <summary>
        /// 创建节点
        /// </summary>
        public static GraphMissionView Create(Type type, MissionTreeSO missionTree, BackGroundView view)
        {          
            Mission runtimeNode = missionTree.Create(type);
            GraphMissionView nodeView = new GraphMissionView(missionTree, runtimeNode);
            nodeView.backGroundView = view;
            nodeView.onNodeSelected += view.BehaviourSelected;
            view.AddElement(nodeView);
            Undo.RecordObject(missionTree, $"Create {type.Name} Mission");
            missionTree.Save();         
            return nodeView;
        }

        public GraphMissionView(MissionTreeSO missionTree, Mission mission)
        {
            Init(missionTree,mission);
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(MissionTreeSO missionTree, Mission mission)
        {
            this.missionTree = missionTree;
            Mission = mission;      
            SetNameAndPos();
            SetVertical();
            AddPort();
            SetNodeColor();
            AddStateField();
            MissionTreeGraphWindow.onValidate -= OnValidate;
            MissionTreeGraphWindow.onValidate += OnValidate;            
            mission.onValidate -= OnValidate;
            mission.onValidate += OnValidate;
            onValidate = view => 
            {
                view.SetNameAndPos();
            };          
        }
        public System.Action<GraphMissionView> onValidate = null;
        private void OnValidate() => onValidate?.Invoke(this);
        /// <summary>
        /// 设置节点名和位置
        /// </summary>
        private void SetNameAndPos()
        {           
            Type nodeType = Mission.GetType();
            title = GetNodeDisplayName(Mission, nodeType);
            style.left = Mission.position.x;
            style.top = Mission.position.y;
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
            inputPort = InstantiatePort(Orientation.Vertical, Direction.Input, Port.Capacity.Multi, typeof(bool));
            inputPort.portName = "父节点";
            inputPort.portColor = inputPortColor;
            inputContainer.Add(inputPort);
            

            // 设置连接点位置
           
            Port.Capacity capacity = Port.Capacity.Multi;
            outputPort = InstantiatePort(Orientation.Vertical, Direction.Output, capacity, typeof(bool));
            outputPort.portName = "子节点";
            outputPort.portColor = outputPortColor;
            outputContainer.Add(outputPort);
        }


        /// <summary>
        /// 设置节点颜色
        /// </summary>
        private void SetNodeColor()
        {
            Color borderColor = Color.green;
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
            stateField = new EnumField(Mission.MissionStatus);
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
            stateField.value = Mission.MissionStatus;

            var element = stateField.ElementAt(0);

            Color color = default;
            switch (Mission.MissionStatus)
            {
                case MissionStatus.Lock:
                    color = actionColor;
                    break;
                case MissionStatus.InActive:
                    color = freeColor;
                    break;
                case MissionStatus.Running:
                    color = runningColor;
                    break;
                case MissionStatus.Success:
                    color = successColor;
                    break;
                case MissionStatus.Failed:
                    color = failedColor;
                    break;
            }

            element.style.backgroundColor = color;
        }

        public override void SetPosition(Rect newPos)
        {
            base.SetPosition(newPos);          
            Mission.position = new Position(newPos.x, newPos.y);
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        public void AddChild(GraphMissionView child, BackGroundView view)
        {
            Undo.RecordObject(missionTree, $"AddChild {this}");

            //如果要添加的子节点已有父节点了 就将它从旧的父节点那里删掉
            if (child.inputPort.connected)
            {
                var edgeToOldParent = child.inputPort.connections.FirstOrDefault();
                var oldParent = (GraphMissionView)edgeToOldParent.output.node;
                oldParent.RemoveChild(child, view);
            }
            Debug.Log("添加");
            //添加子节点
            Mission.AddChild(child.Mission);
            var edge = outputPort.ConnectTo(child.inputPort);
            view.AddElement(edge);
            missionTree.Save();
        }       
        /// <summary>
        /// 删除子节点
        /// </summary>
        public void RemoveChild(GraphMissionView child, BackGroundView view)
        {
            Undo.RecordObject(missionTree, $"RemoveChild {this}");

            Mission.RemoveChild(child.Mission);
            Debug.Log("移除");
            var edge = child.inputPort.connections.FirstOrDefault();
            outputPort.Disconnect(edge);

            view.RemoveElement(edge);
            missionTree.Save();
        }

        /// <summary>
        /// 清空子节点
        /// </summary>
        public void ClearChild(BackGroundView view)
        {
            Undo.RecordObject(missionTree, $"ClearChild {this}");

            Mission.Clear();
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
            missionTree.Save();
        }

        /// <summary>
        /// 删除节点
        /// </summary>
        public void RemoveSelf()
        {
            Undo.RecordObject(missionTree, $"RemoveNode {this}");

            missionTree.Remove(Mission);
            //RemoveChild(this,backGroundView);
        }

        public override string ToString()
        {
            return Mission.ToString();
        }

    }
}
#endif