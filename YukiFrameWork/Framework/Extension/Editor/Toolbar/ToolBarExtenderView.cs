///=====================================================
/// - FileName:      ToolBarExtenderView.cs
/// - NameSpace:     YukiFrameWork.ToolBar
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/9/2 12:26:11
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;




#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
namespace YukiFrameWork.ToolBar
{
    static class ToolbarStyles
    {
        public static readonly GUIStyle ToolBarExtenderBtnStyle;

        static ToolbarStyles()
        {
            ToolBarExtenderBtnStyle = new GUIStyle("Command")
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                imagePosition = ImagePosition.ImageAbove,
                fontStyle = FontStyle.Normal,
                fixedWidth = 60
            };
        }
    }

    [InitializeOnLoad]
    public class ToolbarExtenderView
    {
        private static List<(string sceneName,string scenePath)> current_Scenes = new List<(string, string)>();
        private static string[] sceneNames;     
        static ToolbarExtenderView()
        {
            void Update()
            {
                current_Scenes.Clear();              
                string[] guids = AssetDatabase.FindAssets("t:Scene");

                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                    string sceneName = Path.GetFileNameWithoutExtension(path);
                    current_Scenes.Add((sceneName, path));
                }

                Scene scene = SceneManager.GetActiveScene();
              
                sceneNames = new string[current_Scenes.Count];
                for (int i = 0; i < current_Scenes.Count; i++)
                {
                    (string, string) current = current_Scenes[i];

                    string lastPath = current.Item2.Split("/").LastOrDefault();

                    sceneNames[i] =  current.Item1;

                    if (SceneManager.GetActiveScene().name == sceneNames[i])
                        selectIndex = i;
                }
            }           
            Update();
            EditorApplication.projectChanged -= Update;
            EditorApplication.projectChanged += Update;

            
        }
        private static int selectIndex;
        private static string[] herar = new string[]
        {
            "开启视图快捷显示",
            "关闭视图快捷显示"
        };
        [Toolbar(OnGUISide.Left,0)]
        static void OnToolBarGUILeft()
        {
            FrameworkConfigInfo.IsShowHerarchy
                = EditorGUILayout.Popup(FrameworkConfigInfo.IsShowHerarchy ? 0 : 1, herar) == 0;
            if (GUILayout.Button("Open Local Configuration"))
            {
                FrameWorkDisignWindow.OpenWindow();
            }

            if (GUILayout.Button("Open Architecutre Debugger"))
            {
                ArchitectureDebuggerWindow.OpenWindow();
            }

            if (EditorApplication.isPlaying == false) return;           
            Time.timeScale = EditorGUILayout.FloatField("Time Scale:", Time.timeScale);

        }
        [Toolbar(OnGUISide.Right,0)]
        static void OnToolbarGUIRight()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (selectIndex >= sceneNames.Length || selectIndex < 0 || scene.name != sceneNames[selectIndex])
            {               
                string sceneName = sceneNames.FirstOrDefault(x => x == scene.name);
                if (sceneName.IsNullOrEmpty())
                    selectIndex = 0;
                else selectIndex = Array.IndexOf(sceneNames, sceneName);               
            }
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.LabelField("Active Scene:",GUILayout.Width(100));
            selectIndex = EditorGUILayout.Popup(selectIndex,sceneNames);
            GUI.color = Color.yellow;
            EditorGUILayout.LabelField("使用该可视化场景选择请遵守命名唯一的规则，不应该出现两个相同名称的场景", GUI.skin.box);
            GUI.color = Color.white;
            if (EditorGUI.EndChangeCheck())
            {
                if (scene == null) return;
                if (scene.isDirty)
                {
                    bool save = EditorUtility.DisplayDialog("保存场景提示:", "该场景还没有保存，是否需要进行保存后再跳转?", "保存", "取消");

                    if (save)
                    {
                        EditorSceneManager.SaveScene(scene);
                    }
                }

                EditorSceneManager.OpenScene(current_Scenes[selectIndex].scenePath);
            }

           
        }
    }
}
#endif