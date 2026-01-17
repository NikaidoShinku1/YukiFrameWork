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
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
namespace YukiFrameWork.Missions
{
    public class BackGroundView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<BackGroundView, UxmlTraits> { }
        public MissionTreeSO missionTree;
        public event Action<GraphMissionView> onNodeSelected = null;

        private Vector2 startPosition;
        public BackGroundView()
        {
            graphViewChanged = GraphViewChangedCallback;           
            Insert(0, new GridBackground()); //格子背景

            //添加背景网格样式
            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(ImportSettingWindow.GetData().path + "/BehaviourTree/Editor/GUI/BackGround.uss");
            styleSheets.Add(styleSheet);
           // MissionTreeGraphWindow.onUpdate += Update;
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale); //可缩放            
            this.AddManipulator(new SelectionDragger()); //节点可拖动
            this.AddManipulator(new ContentDragger()); //节点图可移动
            this.AddManipulator(new RectangleSelector()); //可框选多个节点
            
            MissionTreeGraphWindow.onValidate += () =>
            {
                Refresh(missionTree);
            };

            startPosition = viewTransform.position;
         
        }

        public void BehaviourSelected(GraphMissionView view)
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
                    if (element is GraphMissionView nodeView)
                    {
                        //nodeView.SetPosition(nodeView.GetPosition());
                        nodeView.Mission.ReLoadChild();
                        if (nodeView.Mission.IsChild)
                        {
                            foreach (var item in nodeView.Mission.Parents)
                            {
                                item.ReLoadChild();
                            }
                        }
                    }                    
                }               
            }          
            //删除
            if (changes.elementsToRemove != null)
            {
                foreach (GraphElement element in changes.elementsToRemove)
                {                   
                    if (element is GraphMissionView nodeView)
                    {
                        nodeView.onNodeSelected -= BehaviourSelected;
                        nodeView.RemoveSelf();                       
                    }
                    else if (element is Edge edge)
                    {
                        var parentNode = (GraphMissionView)edge.output.node;
                        var childNode = (GraphMissionView)edge.input.node;
                        parentNode.RemoveChild(childNode,this);
                    }                  
                }
            }
            changes.edgesToCreate?.ForEach(x => 
            {
                GraphMissionView nodeView = x.output.node as GraphMissionView;
                GraphMissionView nodeChild = x.input.node as GraphMissionView;

                nodeView.Mission.AddChild(nodeChild.Mission);

                EditorUtility.SetDirty(nodeView.Mission);  
                AssetDatabase.SaveAssets();
            });


            return changes;
        }

        public override void BuildContextualMenu(ContextualMenuPopulateEvent evt)
        {
            if (Application.isPlaying) return;
            var pos = ((evt.localMousePosition - ((Vector2)viewTransform.position - startPosition))) * (1 / scale);
            evt.menu.AppendAction("创建新的任务", x =>
            {
                if (graphWindow)
                {                   
                    
                    var item = GraphMissionView.Create(typeof(Mission), missionTree, this);
                    Debug.Log(pos);
                    item.SetPosition(new Rect(item.contentRect) {x = pos.x,y = pos.y});
                }
            }, DropdownMenuAction.Status.Normal);

            ;

        }

        private MissionTreeGraphWindow graphWindow;
        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(MissionTreeGraphWindow window)
        {
            this.missionTree = window.missionTree;
            this.graphWindow = window;
            if (missionTree == null) return;
            //节点创建时的搜索窗口        
            viewTransform.position = missionTree.ViewportRect.position;
            MissionTreeGraphWindow.onUpdate += () => 
            {
                if(missionTree)
                missionTree.ViewportRect = new Rect(viewTransform.position, viewTransform.scale);
            };

            missionTree.onValidate -= Refresh;
            missionTree.onValidate += Refresh;
        }
        private void Refresh()
        {
            Refresh(missionTree);
        }
        /// <summary>
        /// 刷新
        /// </summary>
        public void Refresh(MissionTreeSO btSO)
        {
            BuildGraphView();
            if (!btSO) return;
            missionTree = btSO;          
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
            Dictionary<int, GraphMissionView> nodeDict = new Dictionary<int, GraphMissionView>();

#if UNITY_2021_1_OR_NEWER
            //先删掉旧的节点 线 和注释块
            foreach (Node node in nodes)
            {
                RemoveElement(node);
            }

            foreach (Edge edge in edges)
            {
                RemoveElement(edge);
            }
#else
            //先删掉旧的节点 线 和注释块
            foreach (Node node in nodes.ToList())
            {
                RemoveElement(node);
            }

            foreach (Edge edge in edges.ToList())
            {
                RemoveElement(edge);
            }
#endif

            if (!missionTree) return;
            //创建节点
            CreateGraphNode(nodeDict, missionTree.AllMissions);
            //根据父子关系连线
            BuildConnect(nodeDict, missionTree.AllMissions);         
        }

        /// <summary>
        /// 创建节点图节点
        /// </summary >
        private void CreateGraphNode(Dictionary<int, GraphMissionView> nodeDict, List<Mission> allNodes)
        {
            foreach (Mission node in allNodes)
            {
                GraphMissionView nodeView = new GraphMissionView(missionTree, node);             
                nodeView.onNodeSelected += BehaviourSelected;
                AddElement(nodeView);
                nodeDict.Add(node.MissionId, nodeView);
            }
        }

        /// <summary>
        /// 构建节点连接
        /// </summary>
        private void BuildConnect(Dictionary<int, GraphMissionView> nodeDict, List<Mission> allNodes)
        {
            foreach (Mission node in allNodes)
            {
                GraphMissionView nodeView = nodeDict[node.MissionId];
                if (nodeView.outputContainer.childCount == 0)
                {
                    //不需要子节点 跳过
                    continue;
                }

                Port selfOutput = (Port)nodeView.outputContainer[0];

                //与当前节点的子节点进行连线
                node.ForEachChildrens((child =>
                {
                    if (child == null)
                    {
                        return;
                    }

                    GraphMissionView childNodeView = nodeDict[child.MissionId];

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