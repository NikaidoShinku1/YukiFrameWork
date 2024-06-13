using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public class StateNodeLayer : GraphLayer
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

        public StateNodeLayer(StateMechineEditorWindow editorWindow) : base(editorWindow)
        {
            stateStyles = new CustomStyles();           
        }

        public override void OnModeChange(PlayModeStateChange mode)
        {
            switch (mode)
            {
                case PlayModeStateChange.EnteredEditMode:
                    if (Selection.activeGameObject != null)
                    {
                        Context.StateMechine = Selection.activeGameObject.GetComponent<StateManager>()?.stateMechine;                        
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
                 
            if (this.Context.StateMechine != null)
            {         
                stateStyles.ApplyZoomFactor(this.Context.ZoomFactor);

                if (this.Context.StateMechine.IsSubLayerAndContainsName())
                {
                    var data = this.Context.StateMechine.subStatesPair[this.Context.StateMechine.layerName];                  
                    for (int i = 0; i < data.stateBases.Count; i++)
                    {
                        DrawNode(data.stateBases[i]);
                    }
                }
                else
                {
                    for (int i = 0; i < this.Context.StateMechine.states.Count; i++)
                    {
                        DrawNode(this.Context.StateMechine.states[i]);
                    }
                }
            }           

            DrawSelectBox();

            float x = rect.x;
            GUILayout.BeginArea(new Rect(rect) { x = x + 2});
            EditorGUILayout.BeginVertical();
            if (this.Context.StateMechine != null)
            {
                EditorGUILayout.BeginHorizontal("PreferencesSectionBox",GUILayout.Height(20));
                for (int i = 0; i < this.Context.StateMechine.parents.Count; i++)
                {
                    BreadCrumb(i, 30, this.Context.StateMechine.parents[i],out var value);

                    if (value)
                        ChangeParent(this.Context.StateMechine.parents[i]);           
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();          
            if (GUILayout.Button("复位", GUILayout.Width(80)))
            {
                this.Context.Reset();
            }

            if (GUILayout.Button("刷新脚本", GUILayout.Width(120)))
            {
                if (this.Context.StateMechine == null)
                {
                    StateManager manager = Selection.activeObject as StateManager;
                    if (manager == null)
                        manager = (Selection.activeObject as GameObject)?.GetComponent<StateManager>();
                    if (manager == null)
                    {
                        string message = "当前没有选择对应的状态机管理器,请使用鼠标选中包含StateManager的物体";
                        Debug.LogWarning(message);
                        StateMechineEditorWindow.OnShowNotification(message);
                        GUIUtility.ExitGUI();
                        return;
                    }
                    this.Context.StateMechine = manager.stateMechine;
                }
                if (this.Context.StateMechine == null)
                {
                    string message = "刷新失败，请检查StateManager是否拥有StateMechine!";
                    Debug.LogWarning(message);
                    StateMechineEditorWindow.OnShowNotification(message);
                    GUIUtility.ExitGUI();
                    return;
                }
                this.Context.StateMechine.IsSubLayer = false;
                Debug.Log("刷新成功");
                StateMechineEditorWindow.OnShowNotification("刷新成功");
                EditorUtility.SetDirty(this.Context.StateMechine);
                AssetDatabase.Refresh();
            }
            EditorGUILayout.ObjectField(this.Context.StateMechine, typeof(StateMechine), true, GUILayout.Width(rect.width > 250 + 120 + 80 ? 250 : rect.width - 120 - 80));                              
            
            EditorGUILayout.EndHorizontal();

            GUILayout.FlexibleSpace();
            EditorGUILayout.BeginVertical( GUILayout.Width(rect.width),GUILayout.Height(10));
            GUI.color = Color.cyan;
            GUILayout.FlexibleSpace();
            GUILayout.Label("YukiFrameWork - StateMechine Window");
            GUI.color = Color.white;
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void BreadCrumb(int index, int maxCount, string name,out bool changeValue)
        {
            GUIStyle style = index == 0 ? "GUIEditor.BreadcrumbLeft" : "GUIEditor.BreadcrumbMid";
            GUIStyle gUIStyle = ((index == 0) ? "GUIEditor.BreadcrumbLeftBackground" : "GUIEditor.BreadcrumbMidBackground");
            GUIContent content = new GUIContent(name);
            Rect breadcrumbLayoutRect = GetBreadcrumbLayoutRect(content, style);
            if (Event.current.type == EventType.Repaint)
            {
                gUIStyle.Draw(breadcrumbLayoutRect, GUIContent.none, 0);              
            }

            changeValue = GUI.Toggle(breadcrumbLayoutRect, index == maxCount - 1, content, style);
        }

        private static Rect GetBreadcrumbLayoutRect(GUIContent content, GUIStyle style)
        {
            Texture image = content.image;
            content.image = null;
            Vector2 vector = style.CalcSize(content);
            content.image = image;
            if (image != null)
            {
                vector.x += vector.y;
            }

            return GUILayoutUtility.GetRect(content, style, GUILayout.MaxWidth(vector.x));
        }

        public override void ProcessEvents()
        {
            base.ProcessEvents();

            if (this.Context.StateMechine == null) return;

            this.Context.hoverState = null;

            List<StateBase> stateBases = null;

            if (this.Context.StateMechine.IsSubLayerAndContainsName())
            {
                stateBases = this.Context.StateMechine.subStatesPair[this.Context.StateMechine.layerName].stateBases;
            }
            else stateBases = this.Context.StateMechine.states;

            for (int i = stateBases.Count - 1; i >= 0; i--)
            {
                CheckNodeClick(stateBases[i]);
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
                            foreach (var item in stateBases)
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
                                EditorUtility.SetDirty(this.Context.StateMechine);
                            }
                        }
                    }
                    break;
            }

            if (Event.current.type == EventType.KeyDown                                     
                && Event.current.keyCode == KeyCode.F)
            {
                foreach (var item in stateBases)
                {
                    if (GetTransformedRect(item.rect).Contains(Event.current.mousePosition))
                    {
                        this.Context.CancelPreviewTransition();
                        SetLayerDefaultState(item);
                        //显示菜单选项
                        Event.current.Use();
                        break;
                    }
                }
            }

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
            {             
                DeleteState();
            }
      
        }

        public override void Update()
        {
            base.Update();

            if (Application.isPlaying && this.Context.StateManager != null && this.Context.StateManager.runTimeSubStatePair[this.Context.StateMechine.layerName].CurrentState != null)
            {
                if (string.IsNullOrEmpty(currentStateName) || !currentStateName.Equals(this.Context.StateManager.runTimeSubStatePair[this.Context.StateMechine.layerName].CurrentState.name))
                {
                    currentStateName = this.Context.StateManager.runTimeSubStatePair[this.Context.StateMechine.layerName].CurrentState.name;
                    widthScale = 0;
                }
                widthScale += Time.deltaTime;
                widthScale %= 1;
            }
        }

        private void ChangeParent(string name)
        {
            this.Context.selectTransition = null;
            string layer = name.Replace(StateConst.upState, "");
            int index = this.Context.StateMechine.parents.IndexOf(layer);
            if (index != -1)
            {
                if (this.Context.StateMechine.parents[index] == "BaseLayer")
                {
                    this.Context.StateMechine.IsSubLayer = false;
                }
                else
                {
                    for (int i = index + 1; i < this.Context.StateMechine.parents.Count; i++)
                    {
                        this.Context.StateMechine.parents.RemoveAt(i);
                        i--;
                    }
                }
            }
            else
            {
                this.Context.StateMechine.parents.Add(layer);
                this.Context.StateMechine.IsSubLayer = true;
            }          
            this.Context.StateMechine.SaveToMechine();
        }

        private void CheckNodeClick(StateBase nodeData)
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

                    if (Event.current.button == 0 && Event.current.clickCount == 2 && nodeData.IsSubingState && this.Context.StateMechine != null)
                    {
                        ChangeParent(nodeData.name);
                    }

                    if (Event.current.button == 0)
                    {
                        OnNodeClick(nodeData);
                    }

                    StateInspectorHelper.Instance.Inspect(this.Context.StateMechine, nodeData);
                }               
            }
            

        }

        public void OnNodeClick(StateBase state)
        {
            if (this.Context.isPreviewTransition)
            {             
                TransitionFactory.CreateTransition(this.Context.StateMechine,this.Context.fromState.name,state.name);
                this.Context.CancelPreviewTransition();
            }

            this.Context.selectTransition = null;
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

            foreach (var item in GetLayerStates())
            {
                CheckSelectNode(item);
            }
           
        }

        private void CheckSelectNode(StateBase state)
        {
            if (GetTransformedRect(state.rect).Overlaps(selectRect, true))
            {
                this.Context.SelectNodes.Add(state);
            }
        
        }

        private void ShowMenu(StateBase data)
        {
            var genricMenu = new GenericMenu();          
            bool isEntry = data.name.Equals(StateConst.entryState) || data.name.StartsWith(StateConst.upState);
            if (!isEntry)
            {               
                genricMenu.AddItem(new GUIContent("Make Transition"), false, () => 
                {
                    this.Context.StartPreviewTransition(data);
                });
                if (!data.name.Equals(StateConst.anyState))
                    genricMenu.AddItem(new GUIContent("Set as Layer Default State(F)"), false, () =>
                    {
                        SetLayerDefaultState(data);
                    });
                if (!data.name.Equals(StateConst.anyState))
                    genricMenu.AddItem(new GUIContent("Delete State(Delete)"), false, DeleteState);
            }
            else
            {
                genricMenu.AddItem(new GUIContent("Make Transition"), false, null);
            }          

            genricMenu.ShowAsContext();
           
        }

        private void SetLayerDefaultState(StateBase data)
        {
            if (this.Context.SelectNodes != null && this.Context.StateMechine != null && this.Context.SelectNodes.Count > 0)
            {
                if (data.defaultState) return;
                if (data.layerName == "BaseLayer")
                {
                    foreach (var item in this.Context.StateMechine.states)
                    {
                        item.defaultState = false;
                    }
                    data.defaultState = true;
                }
                else if (this.Context.StateMechine.subStatesPair.ContainsKey(data.layerName))
                {
                    foreach (var item in this.Context.StateMechine.subStatesPair[data.layerName].stateBases)
                    {
                        item.defaultState = false;
                    }

                    data.defaultState = true;
                }

                this.Context.StateMechine.SaveToMechine();
            }
        }

        private void DeleteState()
        {
            foreach (var item in this.Context.SelectNodes)
            {
                StateNodeFactory.DeleteState(this.Context.StateMechine,item);
            }
            this.Context.SelectNodes.Clear();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void DrawNode(StateBase node)
        {
            Rect rect = GetTransformedRect(node.rect);
            bool subing = node.IsSubingState;
            if (position.Overlaps(rect))
            {                
                GUI.Box(rect,string.Empty, GetStateStyle(node,subing));
                fontStyle ??= new GUIStyle();
                fontStyle.alignment = TextAnchor.UpperCenter;
                fontStyle.normal.textColor = Color.white;
                fontStyle.fontSize = 6 + (int)(this.Context.ZoomFactor * 20);              
                GUI.Label(new Rect(rect) { y = rect.y + (this.Context.ZoomFactor * 10)}, new GUIContent(node.name), fontStyle);
                if (this.Context.StateManager != null && Application.isPlaying)
                {
                    var n = this.Context.StateManager.currents.Find(x => x.name == node.name);
                    if (n != null && node.layerName == this.Context.StateMechine.layerName)
                    {                       
                        RuntimeProgress(rect, subing);
                    }

                }
            }
          
        }

        private void RuntimeProgress(Rect rect,bool subing)
        {
            float offect = this.Context.ZoomFactor * 3;
            stateRunningRect.Set(rect.x + (!subing ? offect * 2 : offect * 18), rect.y + rect.height / 4 * 3 - offect, rect.width - (subing ? (offect * 36) : (offect * 4)), rect.height / 4 + offect * 2);
            stateRunningStyle.fixedHeight = rect.height / 16 + offect * 4;
            stateRunningStyleBG.fixedHeight = rect.height / 16 + offect * 4;
            GUI.Box(stateRunningRect, string.Empty, stateRunningStyleBG);
            stateRunningRect.width *= widthScale;
            GUI.Box(stateRunningRect, string.Empty, stateRunningStyle);
        }

        private GUIStyle GetStateStyle(StateBase StateBase,bool subing = false)
        {
            bool isSelected = this.Context.SelectNodes.Contains(StateBase);

            if (Application.isPlaying
               && this.Context.StateManager != null
               && this.Context.StateManager.runTimeSubStatePair.ContainsKey(this.Context.StateManager.stateMechine.layerName)
               && this.Context.StateManager.runTimeSubStatePair[this.Context.StateManager.stateMechine.layerName].CurrentState != null
               && this.Context.StateManager.runTimeSubStatePair[this.Context.StateManager.stateMechine.layerName].CurrentState == StateBase)
                return this.stateStyles.GetStyle(isSelected ? Style.BlueOn : Style.BlueOn, subing);
            else if (!Application.isPlaying && StateBase.defaultState)
                return this.stateStyles.GetStyle(isSelected ? Style.OrangeOn : Style.Orange, subing);
            else if (StateBase.name == StateConst.entryState)
                return this.stateStyles.GetStyle(isSelected ? Style.MintOn : Style.Mint, subing);
            else if (StateBase.name == StateConst.anyState || StateBase.IsAnyState)
                return this.stateStyles.GetStyle(isSelected ? Style.YellowOn : Style.Yellow);
            else if (StateBase.name.StartsWith(StateConst.upState))
                return this.stateStyles.GetStyle(isSelected ? Style.RedOn : Style.Red, subing);
            else
                return this.stateStyles.GetStyle(isSelected ? Style.BlueOn : Style.Normal, subing);           

        }
    }
}
#endif