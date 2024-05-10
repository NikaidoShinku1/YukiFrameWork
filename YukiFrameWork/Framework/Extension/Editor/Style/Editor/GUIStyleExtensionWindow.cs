///=====================================================
/// - FileName:      GUIStyleExtensionWindow.cs
/// - NameSpace:     YukiFrameWork.UI
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// - Creation Time: 2024/1/16 15:46:05
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

# if UNITY_EDITOR
using UnityEngine;
using System;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using Sirenix.OdinInspector;

namespace YukiFrameWork.Extension
{
    public class MEditorWindow : IDisposable
    {
        ~MEditorWindow()
        {
            Dispose();
        }
        protected bool isInit = false;

        [OnInspectorGUI]
        protected virtual void OnGUI()
        {
            if (!isInit) OnEnable();
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            OnTitleGUI();
            EditorGUILayout.EndHorizontal();
            OnBodyGUI();
        }

        public MEditorWindow()
        {
            OnEnable();
        }

        protected virtual void OnEnable()
        {
             
        }

        protected virtual void OnDisable()
        {

        }

        /// <summary>
        /// 绘制标题区域
        /// </summary>
        protected virtual void OnTitleGUI()
        {
        }

        /// <summary>
        /// 绘制主体区域
        /// </summary>
        protected virtual void OnBodyGUI()
        {
        }

        public void Dispose()
        {
            OnDisable();
        }

        /*/// <summary>
        /// 拷贝字符串到剪贴板
        /// </summary>
        protected void CopyString(string value)
        {
            TextEditor textEditor = new TextEditor();
            textEditor.text = value;
            textEditor.OnFocus();
            textEditor.Copy();
        }*/
    }

    public class GUIStyleExtensionWindow : MEditorWindow
    {
        private string[] toolArray;

        private int toolIndex;
        private bool itemAutoChange = false;
        private TreeViewState treeViewState_Style;
        private MultiColumnHeader mHeader_Style;
        private GUIStylesTreeView guiTreeView;
        private GUIStyle[] customStyles;
        private IconArea iconArea;

        protected override void OnEnable()
        {            
            isInit = true;
            itemAutoChange = false;
            toolArray = GetToolArray();
            treeViewState_Style = new TreeViewState();  
            mHeader_Style = CreateMMultiColumnHeader();          
            guiTreeView = new GUIStylesTreeView(treeViewState_Style,mHeader_Style);
            iconArea = new IconArea();
        }

        protected override void OnDisable()
        {
            isInit = false;
        }

        protected override void OnBodyGUI()
        {
            toolIndex = GUILayout.Toolbar(toolIndex,toolArray);

            switch (toolIndex)
            {
                case 0:
                    DrawGUIStyles();
                    break;
                case 1:
                    DrawIcons(); 
                    break;
                default:
                    break;
            }
        }

        private void DrawGUIStyles()
        {
            if (!itemAutoChange)
            {
                customStyles = GUI.skin.customStyles;

                foreach (var style in customStyles)
                    guiTreeView.AddTreeViewItem(style, false);
                itemAutoChange = true;  
            }
            guiTreeView.Reload();
            try
            {
                guiTreeView.OnGUI(new Rect(0, 25, FrameWorkDisignWindow.LocalPosition.width, FrameWorkDisignWindow.LocalPosition.height));
            }
            catch { EditorGUIUtility.ExitGUI(); }
        }       

        private MultiColumnHeader CreateMMultiColumnHeader()
        {
            MultiColumnHeaderState.Column[] columns =
            {
                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("GUIStyleName","GUI样式名称"),
                    width = 200,
                    minWidth = 100,
                    maxWidth = 300
                },

                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("GUIStyle","GUI样式"),
                    width = 300,
                    minWidth = 200,
                    maxWidth = 400
                },

                new MultiColumnHeaderState.Column()
                {
                    headerContent = new GUIContent("",""),
                    width = 70,
                    minWidth = 90,
                    maxWidth= 50,
                }
            };

            MultiColumnHeaderState state = new MultiColumnHeaderState(columns);
            MultiColumnHeader header = new MultiColumnHeader(state);
            return header;
        }    

        private void DrawIcons()
        {
            try
            {
                iconArea.OnGUI(new Rect(0, 25, FrameWorkDisignWindow.LocalPosition.width, FrameWorkDisignWindow.LocalPosition.height - 30));
            }
            catch { EditorGUIUtility.ExitGUI(); }
        }

        private string[] GetToolArray()
        {
            return new string[2] {"内置GUIStyle","内置Icon"};               
        }
    }
}

#endif