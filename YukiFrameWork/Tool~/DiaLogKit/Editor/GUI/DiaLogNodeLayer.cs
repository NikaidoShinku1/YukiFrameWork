using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.DiaLogue
{
    public class DiaLogNodeLayer : DiaLogGraphLayer
    {
        private CustomStyles stateStyles;
        private Vector2 startPos;
        private bool isSelectNodes = false;
        private Rect selectRect;
        private GUIStyle selectStyle = new GUIStyle("SelectionRect");
        private GUIStyle stateRunningStyleBG = new GUIStyle("MeLivePlayBackground");
        private GUIStyle stateRunningStyle = new GUIStyle("MeLivePlayBar");
        private GUIStyle fontStyle;
        private Rect stateRunningRect;

        private float widthScale = 0f;
        private string currentStateName = string.Empty;

        public DiaLogNodeLayer(DiaLogEditorWindow editorWindow) : base(editorWindow)
        {
            stateStyles = new CustomStyles();           
        }

        public override void OnModeChange(PlayModeStateChange mode)
        {
            switch (mode)
            {
                case PlayModeStateChange.EnteredEditMode:
                    if (Selection.activeObject != null)
                    {
                        Context.NodeTree = Selection.activeObject as NodeTree;                        
                    }
                    
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    break;
                case PlayModeStateChange.ExitingPlayMode:

                    break;
            }
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
                 
            if (this.Context.NodeTree != null)
            {         
                stateStyles.ApplyZoomFactor(this.Context.ZoomFactor);


                for (int i = 0; i < this.Context.NodeTree.nodes.Count; i++)
                {
                    DrawNode(this.Context.NodeTree.nodes[i]);
                }

            }           

            DrawSelectBox();

            float x = rect.x;
            GUILayout.BeginArea(new Rect(rect) { x = x + 2});
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("复位", GUILayout.Width(80)))
            {
                this.Context.Reset();
            }

            if (GUILayout.Button("刷新脚本", GUILayout.Width(120)))
            {
                if (this.Context.NodeTree == null)
                {
                    NodeTree manager = Selection.activeObject as NodeTree;                 
                    if (manager == null)
                    {
                        string message = "当前没有选择对应的节点数,请使用鼠标选中NodeTree";
                        Debug.LogWarning(message);
                        DiaLogEditorWindow.OnShowNotification(message);
                        GUIUtility.ExitGUI();
                        return;
                    }
                    this.Context.NodeTree = manager;
                }
                if (this.Context.NodeTree == null)
                {
                    string message = "刷新失败，请检查StateManager是否拥有NodeTree!";
                    Debug.LogWarning(message);
                    DiaLogEditorWindow.OnShowNotification(message);
                    GUIUtility.ExitGUI();
                    return;
                }              
                Debug.Log("刷新成功");
                DiaLogEditorWindow.OnShowNotification("刷新成功");
                EditorUtility.SetDirty(this.Context.NodeTree);
                AssetDatabase.Refresh();
            }
            this.Context.NodeTree = (NodeTree)EditorGUILayout.ObjectField(this.Context.NodeTree, typeof(NodeTree), true, GUILayout.Width(rect.width > 250 + 120 + 80 ? 250 : rect.width - 120 - 80));
            
            if (this.Context.NodeTree != null)
            {
                GUILayout.BeginHorizontal(GUI.skin.box);
                GUILayout.FlexibleSpace();
                GUILayout.Label("Connection Color");
                this.Context.NodeTree.connectColor = EditorGUILayout.ColorField(this.Context.NodeTree.connectColor);
                GUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical( GUILayout.Width(rect.width),GUILayout.Height(10));
            GUI.color = Color.cyan;
            GUILayout.FlexibleSpace();
            GUILayout.Label("NodeTree Window --- YukiFrameWork 对话树编辑窗口");
            GUI.color = Color.white;           
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        } 

        public override void ProcessEvents()
        {
            base.ProcessEvents();

            if (this.Context.NodeTree == null) return;

            this.Context.hoverState = null;

          
            for (int i = this.Context.NodeTree.nodes.Count - 1; i >= 0; i--)
            {
                CheckNodeClick(this.Context.NodeTree.nodes[i]);
            }
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    {
                        if (Event.current.button == 0 && !IsSelectActive())
                        {
                            startPos = Event.current.mousePosition;
                            isSelectNodes = true;
                        }
                    }
                    break;
                case EventType.MouseUp:
                    {
                        if (Event.current.button == 0)                        
                            isSelectNodes = false;

                        if (Event.current.button == 1)
                        {
                            foreach (var item in this.Context.NodeTree.nodes)
                            {
                                if (GetTransformedRect(item.rect).Contains(Event.current.mousePosition))
                                {
                                    this.Context.CancelPreviewTransition();
                                    ShowMenu(item);
                                    //显示菜单选项
                                    Event.current.Use();
                                    break;
                                }
                            }
                        }
                    }
                    break;
                case EventType.MouseLeaveWindow:
                    {
                        isSelectNodes = false;  
                    }
                    break;
                case EventType.MouseDrag:
                    {
                        if (Event.current.button == 0 && !isSelectNodes)
                        {
                            foreach (var item in this.Context.SelectNodes)
                            {
                                item.rect.center += Event.current.delta / this.Context.ZoomFactor;
                                EditorUtility.SetDirty(this.Context.NodeTree);
                            }
                        }
                    }
                    break;
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
            {
                if (Application.isPlaying)
                {
                    Debug.LogError("运行模式下不允许删除对话节点");
                    return;
                }
                DeleteState();
            }
      
        }

        public override void Update()
        {
            base.Update();

            if (Application.isPlaying && this.Context.NodeTree != null && this.Context.NodeTree.runningNode != null && this.Context.NodeTree.treeState == NodeTreeState.Running)
            {         
                widthScale += Time.deltaTime;
                widthScale %= 1;
            }
        }     
        private void CheckNodeClick(Node nodeData)
        {
            Rect rect = GetTransformedRect(nodeData.rect);           
            if (position.Overlaps(rect) && rect.Contains(Event.current.mousePosition))
            {
                this.Context.hoverState = nodeData;
                if (Event.current.type == EventType.MouseDown && (Event.current.button == 0 || Event.current.button == 1))
                {
                    if (!this.Context.SelectNodes.Contains(nodeData))
                    {
                        this.Context.SelectNodes.Clear();
                        this.Context.SelectNodes.Add(nodeData);
                    }                

                    if (Event.current.button == 0)
                    {
                        OnNodeClick(nodeData);
                    }

                    this.Context.parameterState = nodeData;
                }               
            }
            

        }

        public void OnNodeClick(Node state)
        {           
            if (this.Context.isPreviewTransition)
            {
                var attribute = state.GetType().GetCustomAttribute<RootNodeAttribute>(true);
                if (attribute != null)
                {                    
                    this.Context.CancelPreviewTransition();
                    return;
                }
                var fromAttribute = this.Context.fromState.GetType().GetCustomAttribute<RootNodeAttribute>(true);
                if (fromAttribute != null)
                {
                    this.Context.fromState.GetType().GetField("child",BindingFlags.NonPublic | BindingFlags.Instance).SetValue(this.Context.fromState, state);
                }
                else if (this.Context.fromState is CompositeNode composite)
                {
                    this.Context.NodeTree.AddChild(composite,state);
                }
                else if (this.Context.fromState is SingleNode single)
                {
                    this.Context.NodeTree.AddChild(single, state);                 
                }
               
                this.Context.CancelPreviewTransition();
            }        
        }

        private void DrawSelectBox()
        {
            if (!isSelectNodes)
            {
                selectRect = Rect.zero;
                return;
            }

            Vector2 detal = Event.current.mousePosition - startPos;

            selectRect.center = startPos + detal * 0.5f;
            selectRect.width = Mathf.Abs(detal.x);
            selectRect.height = Mathf.Abs(detal.y);
            GUI.Box(selectRect, string.Empty,selectStyle);

            this.Context.SelectNodes.Clear();

            foreach (var item in this.Context.NodeTree?.nodes)
            {
                CheckSelectNode(item);
            }
           
        }

        private void CheckSelectNode(Node state)
        {
            if (GetTransformedRect(state.rect).Overlaps(selectRect, true))
            {
                this.Context.SelectNodes.Add(state);
            }
        
        }

        private void ShowMenu(Node data)
        {
            var genricMenu = new GenericMenu();

            if (Application.isPlaying)
            {
                genricMenu.AddDisabledItem(new GUIContent("Make Connection"));
                genricMenu.AddDisabledItem(new GUIContent("Delete Node"));
            }
            else
            {
                genricMenu.AddItem(new GUIContent("Make Connection"), false, () =>
                {
                    this.Context.StartPreviewTransition(data);
                });

                genricMenu.AddItem(new GUIContent("Delete Node"), false, DeleteState);

                GUILayout.Space(10);
                var attribute = data.GetType().GetCustomAttribute<RootNodeAttribute>(true);
                if (attribute != null)
                {
                    FieldInfo fieldInfo = data.GetType().GetField("child",BindingFlags.NonPublic | BindingFlags.Instance);
                    var child = ((Node)fieldInfo.GetValue(data));
                    if (child != null)
                    genricMenu.AddItem(new GUIContent("Cancel Connect Node_" + child.name + "_" + child.NodeIndex), false, () =>
                    {
                        fieldInfo.SetValue(data, null);
                    });
                }
                else if (data is SingleNode single)
                {
                    if(single.child != null)
                    genricMenu.AddItem(new GUIContent("Cancel Connect Node_" + single.child.name + "_" + single.child.NodeIndex), false, () =>
                    {
                        single.child = null;
                    });
                }
                else if (data is CompositeNode composite)
                {
                    foreach (var c in composite.options)
                    {
                        var child = this.Context.NodeTree.nodes.Find(x => x.NodeIndex == c.childIndex);
                        if (c == null || child == null) continue;
                        genricMenu.AddItem(new GUIContent("Cancel Connect Node_" + child.name + "_" + child.NodeIndex), false, () =>
                        {
                            composite.options.Remove(c);
                        });
                    }
                }
            }

            genricMenu.ShowAsContext();
           
        }

        private void DeleteState()
        {
            if (Application.isPlaying)
            {
                Debug.LogError("运行模式下不允许删除对话节点之间的连接");
                return;
            }
            foreach (var item in this.Context.SelectNodes)
            {
                this.Context.NodeTree.DeleteNode(item);
            }
            this.Context.SelectNodes.Clear();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void DrawNode(Node node)
        {
            Rect rect = GetTransformedRect(node.rect);           
            if (position.Overlaps(rect))
            {
                GUI.Box(rect, string.Empty, GetStateStyle(node));
                fontStyle ??= new GUIStyle();
                fontStyle.alignment = TextAnchor.UpperCenter;
                fontStyle.normal.textColor = Color.white;
                fontStyle.fontSize = 6 + (int)(this.Context.ZoomFactor * 20);              
                GUI.Label(new Rect(rect) { y = rect.y + (this.Context.ZoomFactor * 10)}, new GUIContent(node.name + "_" + node.NodeIndex), fontStyle);
                if (this.Context.NodeTree != null && Application.isPlaying && this.Context.NodeTree.treeState == NodeTreeState.Running
                    && this.Context.NodeTree.runningNode != null
                    && this.Context.NodeTree.runningNode == node)
                {
                    RuntimeProgress(rect);
                }
            }
          
        }

        private void RuntimeProgress(Rect rect)
        {
            float offect = this.Context.ZoomFactor * 3;
            stateRunningRect.Set(rect.x + offect * 2 , rect.y + rect.height / 4 * 3 - offect, rect.width - (offect * 4), rect.height / 4 + offect * 2);
            stateRunningStyle.fixedHeight = rect.height / 16 + offect * 4;
            stateRunningStyleBG.fixedHeight = rect.height / 16 + offect * 4;
            GUI.Box(stateRunningRect, string.Empty, stateRunningStyleBG);
            stateRunningRect.width *= widthScale;
            GUI.Box(stateRunningRect, string.Empty, stateRunningStyle);
        }

        private GUIStyle GetStateStyle(Node node,bool subing = false)
        {
            bool isSelected = this.Context.SelectNodes.Contains(node);

            if (Application.isPlaying
               && this.Context.NodeTree != null
               && this.Context.NodeTree.runningNode != null
               && this.Context.NodeTree.runningNode == node)
                return this.stateStyles.GetStyle(isSelected ? Style.BlueOn : Style.BlueOn, subing);
            else if (!Application.isPlaying && node.GetType().GetCustomAttribute<RootNodeAttribute>() != null)
                return this.stateStyles.GetStyle(isSelected ? Style.OrangeOn : Style.Orange, subing);
            else if (!Application.isPlaying && node is CompositeNode)
                return this.stateStyles.GetStyle(isSelected ? Style.MintOn : Style.Mint);
            else
                return this.stateStyles.GetStyle(isSelected ? Style.NormalOn : Style.Normal, subing);           

        }
    }
}
#endif