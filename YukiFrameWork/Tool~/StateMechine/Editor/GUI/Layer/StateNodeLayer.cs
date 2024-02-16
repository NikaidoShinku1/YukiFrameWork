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
        private Rect stateRunningRect;

        private float widthScale = 0;
        private string currentStateName = string.Empty;

        public StateNodeLayer(StateMechineEditor editorWindow) : base(editorWindow)
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

                for (int i = 0; i < this.Context.StateMechine.states.Count; i++)
                {
                    DrawNode(this.Context.StateMechine.states[i]);
                }
            }           

            DrawSelectBox();

            GUILayout.BeginArea(rect);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("复位", GUILayout.Width(80)))
            {
                this.Context.Reset();
            }

            if (GUILayout.Button("刷新脚本", GUILayout.Width(120)))
            {
                StateManager manager = Selection.activeObject as StateManager;
                if (manager == null)
                    manager = (Selection.activeObject as GameObject)?.GetComponent<StateManager>();
                if (manager == null)
                {
                    Debug.LogWarning("当前没有选择对应的状态机管理器,请使用鼠标选中包含StateManager的物体");
                    GUIUtility.ExitGUI();
                    return;
                }
                this.Context.StateMechine = manager.stateMechine;
                Debug.Log("刷新成功");
                EditorUtility.SetDirty(this.Context.StateMechine);
                AssetDatabase.Refresh();
            }
            EditorGUILayout.ObjectField(this.Context.StateMechine, typeof(StateMechine), true, GUILayout.Width(rect.width > 250 + 120 + 80 ? 250 : rect.width - 120 - 80));
            EditorGUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        public override void ProcessEvents()
        {
            base.ProcessEvents();

            if (this.Context.StateMechine == null) return;

            this.Context.hoverState = null;

            for (int i = this.Context.StateMechine.states.Count - 1; i >= 0; i--)
            {
                CheckNodeClick(this.Context.StateMechine.states[i]);
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
                            foreach (var item in this.Context.StateMechine.states)
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

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.Delete)
            {             
                DeleteState();
            }
      
        }

        public override void Update()
        {
            base.Update();

            if (Application.isPlaying && this.Context.StateManager != null && this.Context.StateManager.CurrentState != null)
            {
                if (string.IsNullOrEmpty(currentStateName) || !currentStateName.Equals(this.Context.StateManager.CurrentState.name))
                {
                    currentStateName = this.Context.StateManager.CurrentState.name;
                    widthScale = 0;
                }
                widthScale += Time.deltaTime;
                widthScale %= 1;
            }
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

            foreach (var item in Context.StateMechine.states)
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
            bool isEntry = data.name.Equals(StateConst.entryState);
            if (!isEntry)
            {
                genricMenu.AddItem(new GUIContent("Make Transition"), false, () => 
                {
                    this.Context.StartPreviewTransition(data);
                });

                genricMenu.AddItem(new GUIContent("Set as Layer Default State"), false, () =>
                {
                    if (this.Context.SelectNodes != null && this.Context.StateMechine != null && this.Context.SelectNodes.Count > 0)
                    {
                        if (data.defaultState) return;
                        foreach (var item in this.Context.StateMechine.states)
                        {
                            item.defaultState = false;
                        }
                        data.defaultState = true;

                        this.Context.StateMechine.SaveToMechine();
                    }
                });
                genricMenu.AddItem(new GUIContent("Delete State"), false, DeleteState);
            }
            else
            {
                genricMenu.AddItem(new GUIContent("Make Transition"), false, null);
            }          

            genricMenu.ShowAsContext();
           
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
            if (position.Overlaps(rect))
            {
                GUI.Box(rect, node.name,GetStateStyle(node));

                if (this.Context.StateManager != null && Application.isPlaying && this.Context.StateManager.CurrentState != null)
                {
                    if (node == this.Context.StateManager.CurrentState)
                    {
                        float offect = this.Context.ZoomFactor * 3;
                        stateRunningRect.Set(rect.x + offect, rect.y + rect.height / 4 * 3 + offect, rect.width - offect * 2, rect.height / 4 - offect * 2);

                        GUI.Box(stateRunningRect,string.Empty,stateRunningStyleBG);
                        stateRunningRect.width *= widthScale;
                        GUI.Box(stateRunningRect, string.Empty, stateRunningStyle);
                    }
                }
            }
          
        }

        private GUIStyle GetStateStyle(StateBase StateBase)
        {
            bool isSelected = this.Context.SelectNodes.Contains(StateBase);

            if (Application.isPlaying 
                && this.Context.StateManager != null 
                && this.Context.StateManager.CurrentState != null
                && StateBase == this.Context.StateManager.CurrentState
                && this.Context.StateManager.stateMechine == this.Context.StateMechine)            
                //当前正在执行的状态
                return this.stateStyles.GetStyle(isSelected ? Style.BlueOn : Style.BlueOn);
            
            else if (!Application.isPlaying && StateBase.defaultState)
                return this.stateStyles.GetStyle(isSelected ? Style.BlueOn : Style.MintOn);
            else if (StateBase.name == StateConst.entryState)
                return this.stateStyles.GetStyle(isSelected ? Style.RedOn : Style.Red);        
            else
                return this.stateStyles.GetStyle(isSelected ? Style.BlueOn : Style.Normal);           

        }
    }
}
#endif