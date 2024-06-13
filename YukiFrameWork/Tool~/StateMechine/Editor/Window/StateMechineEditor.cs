using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using YukiFrameWork.States;

#if UNITY_EDITOR
namespace YukiFrameWork.States
{
    public class StateMechineEditorWindow : EditorWindow
    {
        #region 字段
        private static StateManager stateManager = null;

        private static StateMechineEditorWindow instance;

        private Rect paramArea;
        private Rect stateArea;
        //参数所占用的比例
        private float percent_of_param = 0.4f;

        private Rect paramResizeArea;

        const float ResizeWidth = 10f;

        //是否正在调整参数区域
        private bool isResizingParamArea = false;

        private List<GraphLayer> graphLayers = new List<GraphLayer>();

        private ParamLayer paramLayer;
        #endregion

        #region 属性
        public Context Context { get; private set; } = new Context();
        #endregion

        #region 生命周期
        private void OnGUI()
        {
            SetArea();
            GraphAllLayer();           
        }
        private void OnEnable()
        {
            wantsMouseEnterLeaveWindow = true;          
            instance = this;
        }

        private void Update()
        {
            Repaint();

            if (graphLayers != null)
            {
                foreach (var item in graphLayers)
                {
                    item.Update();
                }
            }
        }

        private void SetArea()
        {
            paramArea.Set(0, 0, this.position.width * percent_of_param /*- ResizeWidth / 2*/, this.position.height);
           
            stateArea.Set(paramArea.width/* + ResizeWidth*/, 0, this.position.width * (1 - percent_of_param) /*- ResizeWidth / 2*/, this.position.height);
        }

        private void GraphAllLayer()
        {
            ParamAreaResize();
            if (graphLayers.Count == 0)            
                InitLayer();
            
            for (int i = 0; i < graphLayers.Count; i++)
            {
                graphLayers[i].OnGUI(stateArea);
            }
          
            if (IsProcessEventOnStateArea())
            {
                for (int i = graphLayers.Count - 1; i >= 0; i--)
                {
                    graphLayers[i].ProcessEvents();
                }
            }

            if (paramLayer == null)
            {
                paramLayer = new ParamLayer(this);
            }

            paramLayer.OnGUI(paramArea);
            paramLayer.ProcessEvents();           
        }

        #endregion

        #region 方法
        [MenuItem("YukiFrameWork/StateMechine",false,1)]
        public static void OpenWindow()
        {
            instance = GetWindow<StateMechineEditorWindow>();
            instance.Show();
            instance.titleContent = new GUIContent("游戏状态机编辑器");
        }

        public static void OpenWindow(StateManager stateManager)
        {
            OpenWindow();
            instance.Context.StateManager = stateManager;
            StateMechineEditorWindow.stateManager = stateManager;
        }

        public static void OnShowNotification(string message)
        {
#if UNITY_2023_1_OR_NEWER
            message = System.Text.Encoding.UTF8.GetString(System.Text.Encoding.UTF8.GetBytes(message));
#endif
            instance.ShowNotification(new GUIContent(message.ToString()));
        }    

        /// <summary>
        /// 调整参数区域
        /// </summary>
        private void ParamAreaResize()
        {
            paramResizeArea.Set(paramArea.width - ResizeWidth / 2, 0, ResizeWidth, position.height);
            EditorGUIUtility.AddCursorRect(paramResizeArea, MouseCursor.ResizeHorizontal);

            if (paramResizeArea.Contains(Event.current.mousePosition))
            {
                Vector3 start = paramResizeArea.center - new Vector2(-1, paramResizeArea.height / 2);
                Vector3 end = paramResizeArea.center + new Vector2(1, paramResizeArea.height / 2);
                Handles.color = isResizingParamArea ? Color.cyan : Color.white;
                Handles.DrawLine(start, end);
            }

            if (isResizingParamArea)
            {              
                percent_of_param = Mathf.Clamp(Event.current.mousePosition.x / position.width,0.1f,0.5f);                
            }

            switch (Event.current.type)
            {
                case EventType.MouseDown:
                    {
                        if (paramResizeArea.Contains(Event.current.mousePosition))
                        {
                            isResizingParamArea = true;
                            Event.current.Use();
                        }
                    }
                    break;
                case EventType.MouseUp:
                    {
                        isResizingParamArea = false;
                        if (paramResizeArea.Contains(Event.current.mousePosition))
                            Event.current.Use();
                    }
                    break;
            }          
        }

        private void InitLayer()
        {
            graphLayers.Add(new BackGroundLayer(this));
            graphLayers.Add(new TransitionLayer(this));
            graphLayers.Add(new StateNodeLayer(this));
        }

        public bool IsProcessEventOnStateArea()
        {
            bool mousePosition = stateArea.Contains(Event.current.mousePosition);
            bool isMoveUp = Event.current.type == EventType.MouseUp && Event.current.button == 0;
            bool isMouseLeaveWindow = Event.current.type == EventType.MouseLeaveWindow;
            return mousePosition || isMouseLeaveWindow || isMoveUp;
        }
        private void OnLostFocus()
        {
            if (graphLayers == null) return;

            foreach (var item in graphLayers)
            {
                item.OnLostFocus();
            }

            paramLayer?.OnLostFocus();
        }     
        #endregion
    }
}
#endif