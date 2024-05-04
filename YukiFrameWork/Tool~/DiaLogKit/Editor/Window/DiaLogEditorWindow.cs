///=====================================================
/// - FileName:      DiaLogEditorWindow.cs
/// - NameSpace:     YukiFrameWork.DiaLog
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/30 0:45:17
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
using Sirenix.OdinInspector.Editor;
namespace YukiFrameWork.DiaLog
{
	public class DiaLogEditorWindow : OdinEditorWindow
	{
		internal static DiaLogEditorWindow instance;
		public Context Context { get; private set; } = new Context();

        private Rect paramArea;
        private Rect stateArea;
        //参数所占用的比例
        private float percent_of_param = 0.4f;

        private Rect paramResizeArea;

        const float ResizeWidth = 10f;

        //是否正在调整参数区域
        private bool isResizingParamArea = false;

        private List<DiaLogGraphLayer> graphLayers = new List<DiaLogGraphLayer>();

        private DiaLogParamLayer paramLayer;

        // 双击资源文件时自动打开一个编辑器窗口
        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            if (Selection.activeObject is NodeTree)
            {
                PlayerPrefs.SetString("TreePath", AssetDatabase.GetAssetPath(Selection.activeObject));
                OpenWindow();
                return true;
            }
            return false;
        }

        [MenuItem("YukiFrameWork/DiaLog Window",false,2)]
		public static void OpenWindow() 
		{
			var window = GetWindow<DiaLogEditorWindow>();
			window.Show();
			window.titleContent = new GUIContent("对话编辑器");
		}

        protected override void OnEnable()
        {
            base.OnEnable();
            wantsMouseEnterLeaveWindow = true;
            instance = this;
            instance.minSize = new Vector2(1300, 600);
            var path = PlayerPrefs.GetString("TreePath");
            if (!string.IsNullOrEmpty(path))
            {
                instance.Context.NodeTree = AssetDatabase.LoadAssetAtPath<NodeTree>(path);
            }
        } 

        protected override void OnDisable()
        {
            base.OnDisable();
			instance = null;
        }

        protected override void OnImGUI()
        {            
            SetArea();       
            GraphAllLayer();
            base.OnImGUI();

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
                paramLayer = new DiaLogParamLayer(this);
            }

            paramLayer.OnGUI(paramArea);
            paramLayer.ProcessEvents();
        }

        public static void OnShowNotification(string message)
        {
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
                percent_of_param = Mathf.Clamp(Event.current.mousePosition.x / position.width, 0.3f, 0.5f);
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
            graphLayers.Add(new DiaLogNodeLayer(this));
            /*graphLayers.Add(new TransitionLayer(this));
            graphLayers.Add(new StateNodeLayer(this));*/
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
    }
}
#endif