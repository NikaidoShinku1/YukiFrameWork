///=====================================================
/// - FileName:      VersionLoadInfo.cs
/// - NameSpace:     YukiFrameWork.Cook
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/4/24 14:40:53
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Extension;
using System.IO;


#if UNITY_EDITOR
using UnityEditor;
namespace YukiFrameWork
{
    public class VersionInfoWindow : EditorWindow
    {
        private void OnGUI()
        {
            if (titleStyle == null)
            {
                ReLoad();
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("版本更新日志",titleStyle);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(15);
           
            scrollView = EditorGUILayout.BeginScrollView(scrollView);
            try
            {
                string all_Version = File.ReadAllText(ImportSettingWindow.packagePath + "/Framework/Extension/UpdateInfo.md");
                GUILayout.Label(all_Version);
            }
            catch { }
            
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(10);
            EditorGUILayout.HelpBox("每次框架的更新，如涉及到模块的更新而非本体，则应该打开ImportSettingWindow窗口。进行对模块的重新导入操作。", MessageType.Info);
            if (GUILayout.Button("快捷打开"))
            {
                ImportSettingWindow.Open();
            }
        }
        [MenuItem("YukiFrameWork/版本更新日志",false,999)]
        internal static void Open()
        {
            var window = GetWindow<VersionInfoWindow>();
            window.titleContent = new GUIContent("版本更新公告!");
          
        }
        private Vector2 scrollView;
        private GUIStyle titleStyle;

        private void ReLoad()
        {
            titleStyle = new GUIStyle();
            titleStyle.normal.textColor = Color.cyan;
            titleStyle.alignment = TextAnchor.MiddleCenter;
            titleStyle.fontSize = 24;
        }

    }
}
#endif