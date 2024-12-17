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
        class SceneData
        {
            public string sceneName;
            public string scenePath;
            public string sceneDisplay;
        }
        private static List<SceneData> current_Scenes = new List<SceneData>();
        private static string[] sceneNames;     
        static ToolbarExtenderView()
        {
            config = Resources.Load<FrameworkConfigInfo>(nameof(FrameworkConfigInfo));
            void Update()
            {
                current_Scenes.Clear();              
                string[] guids = AssetDatabase.FindAssets("t:Scene");

                for (int i = 0; i < guids.Length; i++)
                {
                    string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                    string sceneName = Path.GetFileNameWithoutExtension(path);

                    string[] dis = path.Split('/');
                    string display = string.Format("{0}/{1}", dis[dis.Length - 2], sceneName);
                    current_Scenes.Add(new SceneData()
                    {
                        sceneName = sceneName,
                        scenePath = path,
                        sceneDisplay = display
                    });
                }

                Scene scene = SceneManager.GetActiveScene();
              
                sceneNames = new string[current_Scenes.Count];
                for (int i = 0; i < current_Scenes.Count; i++)
                {
                    var current = current_Scenes[i];               

                    sceneNames[i] =  current.sceneDisplay;
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
        private static FrameworkConfigInfo config;
        [Toolbar(OnGUISide.Left,0)]
        static void OnToolBarGUILeft()
        {
            if (!config) return;
            config.IsShowHerarchy
                = EditorGUILayout.Popup(config.IsShowHerarchy ? 0 : 1, herar) == 0;
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
            try
            {
                Scene scene = SceneManager.GetActiveScene();
                if (selectIndex >= sceneNames.Length || selectIndex < 0 || scene.name != current_Scenes[selectIndex].sceneName)
                {
                    var data = current_Scenes.FirstOrDefault(x => x.sceneName == scene.name && x.scenePath == scene.path);
                    if (data == null || data.sceneName.IsNullOrEmpty())
                        selectIndex = -1;
                    else selectIndex = Array.IndexOf(current_Scenes.ToArray(), data);
                }
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.LabelField("Active Scene:", GUILayout.Width(100));
                GUI.enabled = !Application.isPlaying;
                selectIndex = EditorGUILayout.Popup(selectIndex, sceneNames);
                GUI.enabled = true;
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
            catch { }
           
        }
    }
}
#endif