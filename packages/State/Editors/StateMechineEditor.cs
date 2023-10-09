using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.IO;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{

    public class StateMechineEditor : EditorWindow
    {
        private StateMechine stateMechine;
        private StateManager stateManager;
        private State selectedState;

        private Vector2 GridOffset;
        private static StateMechineEditor instance;
        private bool IsInit;
        private static bool isTranslation;
        private GUIContent Create;
        private GUIContent Delete;
        private GUIContent Normal;
        private GUIContent translation;
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
            if (stateMechine != null)
            {
                selectedState = stateManager.stateMechine.states[stateManager.controllerID];
            }
        }

        private bool EditorInit()
        {
            if (stateMechine != null && stateManager != null)
            {
                return true;
            }
            else if(stateManager == null)
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

                if (stateMechine != null)
                {
                    foreach (var state in stateMechine.states)
                    {
                        state.rect.position = new Vector2(state.initRectPositionX, state.initRectPositionY);
                    }
                }
            }
            GUILayout.EndHorizontal();
        }

        private void OnGUI()
        {           
            LoadStateMechine();
            GridRefrech();
            DrawGrid(20, 0.2f, Color.gray);
            DrawGrid(100, 0.4f, Color.gray);
            ProcessEvent(Event.current);
            if (stateMechine == null || !IsInit) return;
            DrawTranslationEnter();
            DrawTranslation();
            UpdateManagerStateController();
           
            DrawStateMenu();
           
            if (GUI.changed)Repaint();
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
                        return;
                    }
                    Vector3 start;
                    Vector3 end;

                    if (state.rect.x - nextState.rect.x > state.rect.width)
                    {
                        start = new Vector3(state.rect.position.x + 40, state.rect.position.y + 10, 0);
                        end = new Vector3(nextState.rect.position.x + 100, nextState.rect.position.y + 25, 0);
                    }
                    else if (state.rect.x - nextState.rect.x < -state.rect.width)
                    {
                        start = new Vector3(state.rect.position.x + 60, state.rect.position.y + 10, 0);
                        end = new Vector3(nextState.rect.position.x , nextState.rect.position.y + 25, 0);
                    }
                    else if (state.rect.y > nextState.rect.y)
                    {
                        start = new Vector3(state.rect.position.x + 40, state.rect.position.y, 0);
                        end = new Vector3(nextState.rect.position.x + 40, nextState.rect.position.y + 40, 0);
                    }
                    else
                    {
                        start = new Vector3(state.rect.position.x + 60, state.rect.position.y + 10, 0);
                        end = new Vector3(nextState.rect.position.x + 60, nextState.rect.position.y, 0);
                    }

                    Handles.BeginGUI();
                    DrawTraingle(start, end);
                    Handles.DrawAAPolyLine(5, start, end);
                    Handles.EndGUI();
                }
            }
        }

        /// <summary>
        /// 绘制线段三角头
        /// </summary>
        private void DrawTraingle(Vector3 start, Vector3 end)
        {
            float triangleSize = 10.0f;
            Vector3 direction = (end - start).normalized;
            Vector3 p1 = end;
            Vector3 p2 = end + Quaternion.Euler(0, 0, 135) * direction * triangleSize;
            Vector3 p3 = end + Quaternion.Euler(0, 0, -135) * direction * triangleSize;
            Handles.DrawAAConvexPolygon(p1, p2, p3);
        }

        private void DrawTranslation()
        {
            if (!isTranslation || selectedState == null) return;
            Handles.BeginGUI();
            var mouse = Event.current.mousePosition;

            Handles.color = Color.white;
            Vector3 start = new Vector3(selectedState.rect.position.x + 30, selectedState.rect.position.y + 10, 0);
            Vector3 end = Event.current.mousePosition;
            Handles.DrawAAPolyLine(5, start, end);
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
            DrawTraingle(start, end);
            Handles.DrawAAPolyLine(5, start, end);
            Handles.EndGUI();
        }

        private void UpdateManagerStateController()
        {
            if (selectedState != null)
            {
                stateManager.controllerID = selectedState.index;
            }
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
                Repaint();
                return;
            }

            isTranslation = true;
            selectedState.isNextState = true;
        }

        private void RemoveState()
        {
            if (selectedState == null) return;          

            if(stateManager.normalID == selectedState.index)stateManager.normalID = -1;
            stateMechine.states.Remove(selectedState);
            selectedState = null;
            CheckStateOrNext();
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
            switch (e.type)
            {
                case EventType.MouseDown:
                    {
                        if (e.button == 1)
                        {
                            CreateMenu(e.mousePosition);
                        }

                        if (isTranslation) return;

                        if (e.button == 0)
                        {                          
                            if (stateManager != null)
                            {
                                foreach (var state in stateMechine.states)
                                {
                                    if (state.rect.Contains(e.mousePosition))
                                    {
                                        selectedState = state;                                        
                                        break;
                                    }
                                    else
                                    {
                                        selectedState = null;
                                        stateManager.controllerID = -1;
                                    }
                                }
                            }
                            
                        }
                        e.Use();
                        break;
                    }
                case EventType.MouseDrag:
                    {
                        if (selectedState != null)
                        {
                            selectedState.rect.position += e.delta;
                        }

                        if (e.button == 2)
                        {
                            GridOffset += e.delta;

                            if(stateMechine != null)
                            foreach (var state in stateMechine.states)
                            {
                                if (state == selectedState) continue;
                                state.rect.position += e.delta;
                            }
                        }
                        e.Use();
                    }
                    break;
            }         
        }

        private void CreateState(Vector2 position)
        {
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
                    stateManager.stateMechine = stateMechine;
                    IsInit = EditorInit();                   
                }
                return;
            }
            State state = new State(position);
            state.name = "新的状态";
            stateMechine.AddState(state);
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

            style.alignment = TextAnchor.MiddleCenter;
            style.normal.textColor = Color.white;
            style.fixedWidth = 0;
            style.fixedHeight = 0;
            Texture2D texture = EditorGUIUtility.Load("Assets/YukiFrameWork/State/Gizmos/Btn.png") as Texture2D;
            if (state.index == stateManager.normalID)
            {
                
                GUI.DrawTexture(state.rect,texture, ScaleMode.StretchToFill, false,0,Color.yellow, new Vector4(450, 80), 0);
            }
           
            if (selectedState != null && selectedState == state)
            {
               
                GUI.DrawTexture(state.rect, texture, ScaleMode.StretchToFill, false, 0, Color.cyan, new Vector4(450, 80), 0);
            }
            else if(state.index != stateManager.normalID)
            {
               
                GUI.DrawTexture(state.rect, texture, ScaleMode.StretchToFill, false, 0, Color.gray, new Vector4(450, 80), 0);
            }
           
            style.normal.background = null;
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
            IsInit = false;          
        }
    }
}
#endif