using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public class StateMechineEditor : EditorWindow
    {
        private StateMechine stateMechine;
        private StateManager stateManager;
        private State selectedState;
        private Stack<int> selectedStateIndex;
        private Vector2 GridOffset;
        private Vector2 boxStartPos;
        private Rect selectRect;
        private static StateMechineEditor instance;
        private bool IsInit;
        private static bool isTranslation;
        private static bool ismultiple;
        private static bool isSelectNodes;
        private StateStyle stateStyle;
        private GUIContent Create;
        private GUIContent Delete;
        private GUIContent Normal;
        private GUIContent translation;

        private static float graphSpacingX = 15;
        private static float graphSpacingY = 150;

        private static int fontSize = 12;

        [MenuItem("YukiFrameWork/StateMechine")]

        public static void StateMechineEditorShow()
        {
            instance = GetWindow<StateMechineEditor>();
            instance.titleContent = new GUIContent("状态机编辑器");
            instance.Show();
        }
        public static void StateMechineEditorShow(StateManager manager)
        {
            instance = GetWindow<StateMechineEditor>();
            instance.titleContent = new GUIContent("状态机编辑器");
            instance.Show();
            instance.stateManager = manager;
            instance.stateMechine = manager.stateMechine;
            instance.IsInit = instance.EditorInit();
        }

        private void OnEnable() 
        {
            wantsMouseEnterLeaveWindow = true;          
        }

        private bool EditorInit()
        {
            if (stateMechine != null && stateManager != null)
            {
                selectedStateIndex = new Stack<int>();
                selectedState = null;        
                stateStyle = new StateStyle();
                return true;
            }
            else if (stateManager == null)
            {
                if (stateMechine == null) return false;
                stateManager = stateMechine.GetComponentInParent<StateManager>();
                if (stateManager == null) return false;
                return true;
            }
            return false;
        }

        private void LoadStateMechine()
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("刷新脚本", GUILayout.Width(200)))
            {
                IsInit = EditorInit();
                if (IsInit)
                    Debug.Log("刷新成功");
                else Debug.LogWarning("刷新失败，原因：未添加stateMechine");
                Repaint();
            }
            stateMechine = (StateMechine)EditorGUILayout.ObjectField(stateMechine, typeof(StateMechine), true, GUILayout.Width(300));
            //GUILayout.EndHorizontal();
        }

        private void GridRefrech()
        {
            GUILayout.Space(5);
            if (GUILayout.Button("复位", GUILayout.Width(100)))
            {
                GridOffset = Vector2.zero;
                graphSpacingX = 15;
                graphSpacingY = 150;
                fontSize = 12;
                if (stateMechine != null)
                {
                    foreach (var state in stateMechine.states)
                    {
                        state.rect.position = new Vector2(state.initRectPositionX, state.initRectPositionY);
                        state.rect.width = 150;
                        state.rect.height = 40;
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void OnGUI()
        {
            
            LoadStateMechine();
            GridRefrech();
            DrawGrid(graphSpacingX, 0.2f, Color.black);
            DrawGrid(graphSpacingY, 0.4f, Color.black);           
            ProcessEvent(Event.current);
            if (stateMechine == null || !IsInit) return;
            Update_SelectedState();
            DrawTranslationEnter();
            DrawTranslation();            
            DrawStateMenu();
            UpdateManagerStateController();
            DrawSelectBox();
            if (GUI.changed) Repaint();
        }
             
        private void Update_SelectedState()
        {
            if(selectedStateIndex.Count > 0)
                selectedState = stateMechine.GetState(selectedStateIndex.Peek());
        }

        private void DrawTranslationEnter()
        {
            foreach (var state in stateMechine.states)
            {
                if (state.isNextState && state.nextStateID != -1)
                {
                    var nextState = stateMechine.GetState(state.nextStateID);
                    if (nextState == null)
                    {
                        state.isNextState = false;
                        return;
                    }
                    Handles.BeginGUI();

                    var targetPos = nextState.rect.center + GetTranslationOffect(state.rect.center, nextState.rect.center);
                    
                    DrawTransition(state.rect.center, targetPos, Color.yellow);

                    Handles.EndGUI();
                }
            }
        }

        public Vector2 GetTranslationOffect(Vector2 start, Vector2 end)
        {
            var direction = end - start;
            var offect = Vector2.zero;

            if (Mathf.Abs(direction.y) > Mathf.Abs(direction.x))
            {
                offect.x = direction.y < 0 ? 10 : -10;
            }
            else
            {
                offect.y = direction.x > 0 ? 10 : -10;
            }
            return offect;
        }

        private void DrawTransition(Vector2 start, Vector2 end, Color color)
        {
            Handles.color = color;
            Handles.DrawAAPolyLine(5f, start, end);

            Vector2 direction = (end - start);
            Vector2 center = start + direction * 0.5f;

            Vector2 crossDir = Vector3.Cross(direction, Vector3.forward);

            Vector3[] triangle = { center + direction.normalized * 10
                    , center + crossDir.normalized * 5,
                    center - crossDir.normalized * 5
            };
            Handles.DrawAAConvexPolygon(triangle);
        }

        private void DrawTranslation()
        {
            if (!isTranslation || selectedState == null) return;
            Handles.BeginGUI();
            var mouse = Event.current.mousePosition;

            Handles.color = Color.white;
            Vector3 start = selectedState.rect.center;
            Vector3 end = Event.current.mousePosition;
            Handles.DrawAAPolyLine(5, start, end);
            DrawTransition(start, end, Color.white);
            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    {
                        if (Event.current.button == 0)
                        {
                            foreach (var state in stateMechine.states)
                            {
                                if (state.rect.Contains(mouse))
                                {
                                    selectedState.nextStateID = state.index;
                                    isTranslation = false;
                                    return;
                                }
                            }
                            isTranslation = false;
                            selectedState.isNextState = false;
                        }
                    }
                    break;
            }      
            Handles.EndGUI();
        }

        private void UpdateManagerStateController()
        {
            if(selectedStateIndex != null)
                stateManager.stateIndexs = selectedStateIndex;
        }

        private void CreateMenu(Vector2 position)
        {
            GenericMenu menu = new GenericMenu();
            if (selectedState != null)
            {
                if (selectedState.isNextState)
                    translation = new GUIContent("删除状态连接");
                else
                    translation = new GUIContent("创建状态连接");
                menu.AddItem(translation, false, () => SetTranslation());
            }
            GUILayout.Space(10);
            if (stateMechine == null)
                Create = new GUIContent("生成状态机");
            else
                Create = new GUIContent("创建新状态");

            Delete = new GUIContent("删除状态");
            Normal = new GUIContent("设置默认状态");

            menu.AddItem(Create, false, () => CreateState(position));
            menu.AddItem(Delete, false, () => RemoveState());
            menu.AddItem(Normal, false, () => SetNormalState());
            menu.ShowAsContext();
        }

        private void SetTranslation()
        {                       
            if (selectedState == null) return;

            if (selectedState.isNextState)
            {
                selectedState.isNextState = false;
                selectedState.nextStateID = -1;
                AssetDatabase.Refresh();
                Repaint();
                return;
            }           
            isTranslation = true;
            selectedState.isNextState = true;
            AssetDatabase.Refresh();
        }

        private void RemoveState()
        {
            if (selectedStateIndex.Contains(stateManager.normalID))
                stateManager.normalID = -1;
            foreach (var index in selectedStateIndex)
            {              
                stateMechine.RemoveState(index);
            }
            if (stateMechine.states.Count > 0)
            {
                for (int i = 0; i < stateMechine.states.Count; i++)
                {
                    stateMechine.states[i].index = i;
                }
            }
            selectedState = null;
            CheckStateOrNext();
            AssetDatabase.Refresh();
        }

        private void CheckStateOrNext()
        {
            foreach (var state in stateMechine.states)
            {
                if (state.isNextState)
                {
                    var nextState = stateMechine.GetState(state.nextStateID);
                    if (nextState == null) state.isNextState = false;
                }
            }
        }

        private void SetNormalState()
        {
            if (selectedState != null)
            {
                stateManager.normalID = selectedState.index;
                AssetDatabase.Refresh();
            }
        }

        private void ProcessEvent(Event e)
        {

            if (e.keyCode == KeyCode.LeftControl || e.keyCode == KeyCode.RightControl)
                ismultiple = e.type == EventType.KeyUp ? false : true;          
           
            switch (e.type)
            {
                case EventType.MouseDown:
                    {
                        if (e.button == 1)
                        {
                            CreateMenu(e.mousePosition);
                            e.Use();
                            
                        }                       
                        if (isTranslation) return;

                        bool isSelectEnter = false;                      

                        if (stateManager != null && stateMechine != null && selectedStateIndex != null)
                        {
                            if (e.button == 0 && ismultiple)
                            {
                                foreach (var state in stateMechine.states)
                                {
                                    if (state.rect.Contains(e.mousePosition))
                                    {
                                        isSelectEnter = true;
                                        selectedStateIndex.Push(state.index);
                                        e.Use();
                                        return;
                                    }

                                }
                            }

                            else if (e.button == 0 && !ismultiple)
                            {
                                if (selectedStateIndex.Count == 1)
                                    selectedStateIndex.Clear();
                                selectedState = null;
                                foreach (var state in stateMechine.states)
                                {
                                    if (state.rect.Contains(e.mousePosition))
                                    {
                                        isSelectEnter = true;
                                        selectedStateIndex.Push(state.index);                                      
                                        return;
                                    }
                                    continue;

                                }
                            }
                            selectedStateIndex.Clear();
                            if (e.button == 0 && !isSelectEnter)
                            {
                                boxStartPos = e.mousePosition;
                                isSelectNodes = true;
                            }

                        }
                        e.Use();
                        break;
                    }
                case EventType.MouseUp:
                    {
                        isSelectNodes = false;
                        e.Use();
                    }
                    break;
                case EventType.MouseLeaveWindow:
                    {
                        isSelectNodes = false;
                    }
                    break;
                case EventType.MouseDrag:
                    {                       
                        if (selectedStateIndex != null)
                        {
                            if(stateMechine != null)
                            foreach (var state in stateMechine.states)
                                if (e.button == 0 && selectedStateIndex.Contains(state.index) && !isSelectNodes)
                                {
                                    state.rect.position += e.delta;
                                }
                        }
                        if (e.button == 2)
                        {
                            GridOffset += e.delta;

                            if (selectedStateIndex != null && stateMechine != null)
                                foreach (var state in stateMechine.states)
                                {
                                    if (selectedStateIndex.Contains(state.index)) continue;
                                    state.rect.position += e.delta;
                                }
                        }
                      
                        e.Use();
                    }
                    break;
                case EventType.ScrollWheel:
                    {
                        if (e.isScrollWheel)
                        {                            
                            graphSpacingX += e.delta.y;
                            graphSpacingY += e.delta.y * 5;
                            if (selectedStateIndex != null && stateMechine != null)
                                foreach (var state in stateMechine.states)
                                {                                                                    
                                    state.rect.width += e.delta.y;
                                    state.rect.height += e.delta.y;
                                   
                                    state.rect.width = Mathf.Clamp(state.rect.width, 140, 165);
                                    state.rect.height = Mathf.Clamp(state.rect.height, 30, 55);
                                }
                           

                            fontSize += (int)e.delta.y / 3;
                            fontSize = Mathf.Clamp(fontSize, 7, 17);
                            graphSpacingX = Mathf.Clamp(graphSpacingX, 3, 30);
                            graphSpacingY = Mathf.Clamp(graphSpacingY, 90, 225);
                        }
                        e.Use();
                    }
                    break;
            }

            if (e.keyCode == KeyCode.Delete)
            {              
                if(selectedStateIndex.Count > 0)
                RemoveState();
            }        
        }

        private void DrawSelectBox()
        {          
            if (!isSelectNodes)
            {
                selectRect = Rect.zero;

                return;
            }
            Vector2 detal = Event.current.mousePosition - boxStartPos;

            selectRect.center = boxStartPos + detal / 2f;
            selectRect.width = Mathf.Abs(detal.x);
            selectRect.height = Mathf.Abs(detal.y);

            foreach (var state in stateMechine.states)
            {
                if (selectRect.Contains(state.rect.center))
                {
                    selectedStateIndex.Push(state.index);
                }
            }

            GUI.Box(selectRect, string.Empty, stateStyle.GetStyle(Style.SelectionRect));
        }

        private void CreateState(Vector2 position)
        {
            if (stateManager == null) return;
            if (stateMechine == null)
            {
                stateMechine = stateManager.GetComponentInChildren<StateMechine>();
                if (stateMechine == null)
                {
                    GameObject obj = new GameObject()
                    {
                        name = typeof(StateMechine).Name
                    };
                    obj.transform.SetParent(stateManager.transform);
                    stateMechine = obj.AddComponent<StateMechine>();                   
                    IsInit = EditorInit();                   
                }
                stateManager.stateMechine = stateMechine;
                return;
            }
            State state = new State(position);
            if (stateMechine.states.Count == 0) stateManager.normalID = state.index;
            state.name = "New State " + stateMechine.states.Count;
            stateMechine.AddState(state);
            AssetDatabase.Refresh();
        }
           
      
        private void DrawStateMenu()
        {
            foreach (var state in stateMechine.states)
            {
                if (state.stateManager == null) state.stateManager = stateManager;
                DrawState(state);
            }
        }

        private void DrawState(State state)
        {
            GUIStyle style = new GUIStyle(GUI.skin.box);
            
            if (selectedStateIndex.Contains(state.index) & state.index == stateManager.normalID)
                style = stateStyle?.GetStyle(Style.OrangeOn);
            else if (state.index == stateManager.normalID)
                style = stateStyle?.GetStyle(Style.Orange);                        
            if (selectedStateIndex.Contains(state.index) & state.index != stateManager.normalID)           
                style = stateStyle?.GetStyle(Style.NormalOn);           
            else if (state.index != stateManager.normalID)           
                style = stateStyle?.GetStyle(Style.Normal);

            style.fontStyle = FontStyle.Bold;
            style.fontSize = fontSize;          
            GUI.Box(state.rect, state.name,style);
            
            GUI.changed = true;
            Repaint();
        }

        private void DrawGrid(float gridSpacing, float gridOpacity, Color gridColor)
        {
            //宽度分段
            int widthDivs = Mathf.CeilToInt(position.width / gridSpacing);
            //高度分段
            int heightDivs = Mathf.CeilToInt(position.height / gridSpacing);

            Handles.BeginGUI();//在 3D Handle GUI 内开始一个 2D GUI 块。
            {
                //设置颜色：
                Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

                //单格的偏移，算是GridOffset的除余
                Vector3 gridOffset = new Vector3(GridOffset.x % gridSpacing, GridOffset.y % gridSpacing, 0);

                //绘制所有的竖线
                for (int i = 0; i < widthDivs; i++)
                {
                    Handles.DrawLine(
                        new Vector3(gridSpacing * i, 0 - gridSpacing, 0) + gridOffset,                  //起点
                        new Vector3(gridSpacing * i, position.height + gridSpacing, 0f) + gridOffset);  //终点
                }
                //绘制所有的横线
                for (int j = 0; j < heightDivs; j++)
                {
                    Handles.DrawLine(
                        new Vector3(0 - gridSpacing, gridSpacing * j, 0) + gridOffset,                  //起点
                        new Vector3(position.width + gridSpacing, gridSpacing * j, 0f) + gridOffset);   //终点
                }

                //重设颜色
                Handles.color = Color.white;
            }
            Handles.EndGUI(); //结束一个 2D GUI 块并返回到 3D Handle GUI。
        }

        private void OnDisable()
        {
            if (stateManager != null)
                stateManager.stateIndexs.Clear();
            IsInit = false;              
        }
    }
}
#endif