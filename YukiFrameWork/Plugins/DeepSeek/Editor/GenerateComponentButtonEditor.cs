///=====================================================
/// - FileName:      GenerateComponentButtonEditor.cs
/// - NameSpace:     YukiFrameWork.ItemDemo
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/4/15 19:00:46
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine.UIElements;
namespace YukiFrameWork.ItemDemo
{
    [InitializeOnLoad]
    public class GenerateComponentButtonEditor : Editor
    {
        public static GameObject SelectGameObject { get; private set; }
        private static double lastTime;
        private static Button button;
        static GenerateComponentButtonEditor()
        {
            EditorApplication.update += Update;
           
            Selection.selectionChanged += () => {
              
                if (button == null) return;
                button.visible = Selection.activeGameObject != null;
            };
        }

        private static void Update()
        {
            // 防止过于频繁调用。仅在0.1s后运行
            if (EditorApplication.timeSinceStartup - lastTime < 0.1f) return;
            // 最后更新时间
            lastTime = EditorApplication.timeSinceStartup;

            if (button != null) return;

            var windows = Resources.FindObjectsOfTypeAll<EditorWindow>();
            foreach (var window in windows)
            {
                if (window.GetType().Name != "InspectorWindow") continue;
               
                var buttons = window.rootVisualElement.Q(className: "unity-inspector-add-component-button");

                if (buttons == null) continue;

                var container = new VisualElement
                {
                    style = { flexDirection = FlexDirection.Row, justifyContent = Justify.Center, marginTop = -8 }
                };
             
                buttons.parent.Add(container);
              
                var mButton = new Button(ClickEvent)
                {
                    text = "DeepSeek 生成组件",
                    style = { width = 230, height = 25, marginLeft = 2, marginRight = 2 }
                };
             
                container.Add(mButton);

                button = mButton;
             
                button.visible = Selection.activeGameObject != null;

                EditorApplication.update -= Update;
            }
        }

      
        private static void ClickEvent()
        {           
            var selection = Selection.activeGameObject;
            SelectGameObject = selection;
         
            var window = EditorWindow.GetWindow<GenerateComponentWindow>(true, "生成组件窗口");
            // Displays the window
            window.Show();

            // Get the main Unity Editor window's position and size
            var mainWindow = EditorGUIUtility.GetMainWindowPosition();

            // Calculate center position
            float centerX = mainWindow.x + (mainWindow.width - 500) / 2; // 500 is the estimated width of the new window
            float centerY = mainWindow.y + (mainWindow.height - 120) / 2; // 120 is the window height

            // Set the position of the window to be centered
            window.position = new Rect(centerX, centerY, 500, 120);
        }
    }
}
#endif