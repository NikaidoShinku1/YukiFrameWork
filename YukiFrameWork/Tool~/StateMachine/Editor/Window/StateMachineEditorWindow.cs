///=====================================================
/// - FileName:      StateMachineEditorWindow.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/8 20:05:52
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor.Graphs;

using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
namespace YukiFrameWork.Machine
{
    public class StateMachineEditorWindow : EditorWindow
    {

        private float percent_of_param = 0.3f; // 参数所占用的比例

        private VisualElement left = null;
        private VisualElement middle = null;
        private VisualElement right = null;

        private Rect paramResizeArea;

        const float ResizeWidth = 5;

        private bool isResizingParamArea = false; // 是否正在调整参数区域

        private Rect stateRect;

        private StateGraphView graphGUI = null;

        private ParamLayer paramLayer = null;

        private IMGUIContainer state_top_container;
        private IMGUIContainer state_bottom_container;

        private Vector3 splitLineStart;
        private Vector3 splitLineEnd;

        private string parentLayer;

        private int lastRepaintCount = 0;

        private void Awake()
        {
            try
            {
                this.titleContent = new GUIContent("YUKI 有限状态机", EditorGUIUtility.IconContent("AnimatorController Icon").image);
            }
            catch 
            {
                this.titleContent = new GUIContent("YUKI 有限状态机");
            }
        }

        [MenuItem("YukiFrameWork/Yuki有限状态机",false,-1)]
        static void ShowWindow()
        {
            StateMachineEditorWindow stateMachineEditorWindow = CreateWindow<StateMachineEditorWindow>();
            stateMachineEditorWindow.Show();
        }      
        private void Init()
        {
            rootVisualElement.Clear();

            ClearInspetor();

            rootVisualElement.style.flexDirection = FlexDirection.Row;
            left = new VisualElement();
            IMGUIContainer parameter_Container = new IMGUIContainer();
            
            parameter_Container.onGUIHandler += () => 
            {
                if (paramLayer == null)
                {
                    paramLayer = new ParamLayer(this);
                }
                paramLayer.OnGUI(parameter_Container.layout);
                paramLayer.ProcessEvents();
            };          
            parameter_Container.StretchToParentSize();
            left.Add(parameter_Container);
            middle = new VisualElement();
            right = new VisualElement();


            state_top_container = new IMGUIContainer();
            state_top_container.onGUIHandler += () =>
            {
                DoGraphToolbar(state_top_container.layout);
            };
            right.Add(state_top_container);

            graphGUI = new StateGraphView();
           // graphGUI.StretchToParentSize(); 
            right.Add(graphGUI);

            state_bottom_container = new IMGUIContainer();
            state_bottom_container.onGUIHandler += () =>
            {
                DrawButtom(state_bottom_container.layout);
            };
            right.Add(state_bottom_container);

            rootVisualElement.Add(left);
            rootVisualElement.Add(middle);
            rootVisualElement.Add(right);


            middle.RegisterCallback<MouseDownEvent>((e) =>
            {
                if (e != null && e.button == 0)
                {
                    isResizingParamArea = true;
                }
            });

            rootVisualElement.RegisterCallback<MouseUpEvent>(OnMouseUpEvent);
         
        }
        private void ClearInspetor()
        {
            // 清空当前的Inspector
            if (Selection.activeObject == null)
                return;

          /*  if (Selection.activeObject.GetType() == typeof(FSMStateInspectorHelper) ||
                    Selection.activeObject.GetType() == typeof(FSMTransitionInspectorHelper))
            {
                Selection.activeObject = null;
            }*/

        }

        private void Update()
        {
            lastRepaintCount++;
            if (lastRepaintCount >= 10)
            {
                Repaint();
                lastRepaintCount = 0;
            }

            if (graphGUI != null)
                graphGUI.Update();

        }
        private void OnGUI()
        {
            wantsMouseEnterLeaveWindow = true;
            wantsMouseMove = true;

            stateRect.Set(this.position.width * percent_of_param + 2, 20, this.position.width * (1 - percent_of_param), position.height - 15);
            if (Event.current.type == EventType.Repaint)
                Styles.graphBackground.Draw(stateRect, isHover: false, isActive: false, on: false, hasKeyboardFocus: false);
            if (left == null)
                Init();
            ParamAreaResize();

            left.style.width = this.position.width * percent_of_param - ResizeWidth / 2;
            left.style.height = this.position.height;

            middle.style.width = ResizeWidth;
            middle.style.height = this.position.height;

            right.style.width = this.position.width * (1 - percent_of_param) - ResizeWidth / 2;
            right.style.height = this.position.height;


            state_top_container.style.width = right.style.width;
            state_top_container.style.height = 20;

            graphGUI.style.width = right.style.width;
            graphGUI.style.height = this.position.height - 35;


            state_bottom_container.style.width = right.style.width;
            state_bottom_container.style.height = 15;

            splitLineStart.Set(this.position.width * percent_of_param + 2, 0, 0);
            splitLineEnd.Set(this.position.width * percent_of_param + 2, this.position.height, 0);
            Handles.color = Color.black;
            Handles.DrawAAPolyLine(2, splitLineStart, splitLineEnd);


            graphGUI.OnGUI();


            // 取消拖拽
            if (Event.current.type == EventType.MouseLeaveWindow && Event.current.button == 0)
            {
                isResizingParamArea = false;
            }


            instance = this;
        }  

        private static StateMachineEditorWindow instance;
        public static StateMachineEditorWindow Instance => instance;
        public static void ShowNotification(string tip)
        {
            instance.ShowNotification(new GUIContent(tip));
        }



        private void ParamAreaResize()
        {
            paramResizeArea.Set(this.position.width * percent_of_param - ResizeWidth / 2, 0, ResizeWidth, position.height);

            EditorGUIUtility.AddCursorRect(paramResizeArea, MouseCursor.ResizeHorizontal);

            if (paramResizeArea.Contains(Event.current.mousePosition))
            {
                Vector3 start = paramResizeArea.center - new Vector2(-1, paramResizeArea.height / 2);
                Vector3 end = paramResizeArea.center + new Vector2(1, paramResizeArea.height / 2);
                Handles.color = isResizingParamArea ? Color.green : Color.white;
                Handles.DrawLine(start, end);
            }

            if (isResizingParamArea)
            {
                percent_of_param = Mathf.Clamp(Event.current.mousePosition.x / position.width, 0.1f, 0.5f);
                if (this.position.width * percent_of_param < 150)
                    percent_of_param = 150 / this.position.width;
            }

        }

        private void DoGraphToolbar(Rect toolbarRect)
        {
            Handles.color = new Color(0, 0, 0, 0.4f);
            Handles.DrawLine(new Vector3(toolbarRect.x, 0), new Vector3(toolbarRect.x + toolbarRect.width, 0));

            GUILayout.BeginArea(toolbarRect);
            GUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUI.BeginChangeCheck();
            BreadCrumb(0, Global.Instance.Layers.Count + 1, "Base Layer");
            if (EditorGUI.EndChangeCheck() && Global.Instance.Layers.Count > 0)
            {
                Global.Instance.RemoveLayer(0, Global.Instance.Layers.Count);
            }

            for (int i = 0; i < Global.Instance.Layers.Count; i++)
            {
                EditorGUI.BeginChangeCheck();
                BreadCrumb(i + 1, Global.Instance.Layers.Count + 1, Global.Instance.Layers[i]);
                if (EditorGUI.EndChangeCheck())
                {
                    Global.Instance.RemoveLayer(i + 1, Global.Instance.Layers.Count - 1 - i);
                }
            }

            if (parentLayer != Global.Instance.LayerParent && graphGUI != null)
            {
                graphGUI.RefreshNodes();
                parentLayer = Global.Instance.LayerParent;
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
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

        private void BreadCrumb(int index, int maxCount, string name)
        {
            GUIStyle style = index == 0 ? "GUIEditor.BreadcrumbLeft" : "GUIEditor.BreadcrumbMid";
            GUIStyle gUIStyle = ((index == 0) ? "GUIEditor.BreadcrumbLeftBackground" : "GUIEditor.BreadcrumbMidBackground");
            GUIContent content = new GUIContent(name);
            Rect breadcrumbLayoutRect = GetBreadcrumbLayoutRect(content, style);
            if (Event.current.type == EventType.Repaint)
            {
                gUIStyle.Draw(breadcrumbLayoutRect, GUIContent.none, 0);
            }

            GUI.Toggle(breadcrumbLayoutRect, index == maxCount - 1, content, style);
        }

        private GUIStyle pingStyle;
        private void DrawButtom(Rect rect)
        {
            rect.Set(0, 0, rect.width, rect.height);
            pingStyle ??= new GUIStyle("ShurikenLabel");
           
            pingStyle.alignment = TextAnchor.MiddleRight;
            
            GUILayout.BeginArea(rect);
            GUILayout.BeginHorizontal();
            if (Global.Instance.StateManager != null)
            {
                if (GUILayout.Button(Global.Instance.StateManager.name, pingStyle))
                    EditorGUIUtility.PingObject(Global.Instance.StateManager.gameObject);
            }

            GUILayout.FlexibleSpace();
            if (GUILayout.Button(AssetDatabase.GetAssetPath(Global.Instance.RuntimeStateMachineCore), pingStyle))
                EditorGUIUtility.PingObject(Global.Instance.RuntimeStateMachineCore);

            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }


        private void OnMouseUpEvent(MouseUpEvent e)
        {
            if (e != null && e.button == 0)
            {
                isResizingParamArea = false;
            }

            if (graphGUI != null && e.button == 0)
            {
                graphGUI.selector.CancelDrag();
            }
        }


        private void OnDisable()
        {
            ClearInspetor();
            instance = null;
        }



    }
}
#endif