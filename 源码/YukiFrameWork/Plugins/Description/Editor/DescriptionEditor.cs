///=====================================================
/// - FileName:      DirectionEditor.cs
/// - NameSpace:     YukiFrameWork.Editors
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   这是一个框架工具创建的脚本
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using UnityEditor;
using System.Linq;
using YukiFrameWork.States;
#if UNITY_EDITOR
namespace YukiFrameWork.Editors
{
    public class DescriptionEditor : EditorWindow
    {
        private static DescriptionEditor instance;
        private static Rect rect;
        private static float currentChildWidth => currentWidth / 5;
        private static float currentWidth = 1000f;
        private static Vector2 sliderPosition;
        private static string[] alldirectionTextName = new string[] 
        {
            "1.简介",
            "2.LifeTimeScope",
            "3.Architecture",
            "4.MVC",
            "5.单例",
            "6.状态机",
            "7.资源管理模块",
            "8.动作时序管理模块",
            "9.UI模块",
            "10.声音管理模块",
            "11.全局广播",
            "12.Command",
            "13.BindableProperty", 
            "14.协程拓展"
        };
        private static StateStyles stateStyle;
        private static string tempDescription;
        [MenuItem("YukiFrameWork/Description", false,-1001)]
        public static void OpenWindow()
        {        
            rect = new Rect(0, 0, currentWidth, currentWidth * 1.5f);
            instance = GetWindow<DescriptionEditor>();
            instance.titleContent = new GUIContent("YukiFrameWork");
            instance.position = rect;
            instance.Show();
            tempDescription = alldirectionTextName[1];
            stateStyle = new StateStyles();
        }

        private void OnGUI()
        {
            if (instance == null)
            {
                OpenWindow();
            }
            EditorGUILayout.BeginHorizontal();
            AddDirections();
            OnCreateChildBtnWindow();            
            GUILayout.EndHorizontal();
        }

        private void OnCreateChildBtnWindow()
        {           
            Rect newRect = new Rect(0, 0, currentChildWidth, position.height);
            var style = new GUIStyle(GUI.skin.label);
            style.fontSize = 14;
            GUILayout.BeginArea(newRect,EditorStyles.helpBox);          
            GUILayout.Label("YukiFrameWork介绍列表",style);
            GUILayout.Label("当前版本 V0.8");
            GUILayout.Space(20);
            OnDirectionOpenClick();
            GUILayout.EndArea();
        }

        private void OnDirectionOpenClick()
        {
            
            foreach (var direction in alldirectionTextName) 
            {
                var style = stateStyle.GetStyle(Style.Normal);
                if (direction == tempDescription)
                    style = stateStyle.GetStyle(Style.NormalOn);
                EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
                if (GUILayout.Button(direction, style,GUILayout.Width(150)))
                {
                    tempDescription = direction;                  
                }
                EditorGUILayout.EndVertical();
                GUILayout.Space(10);
            }
        }

        private void AddDirections()
        {
            Rect newRect = new Rect(currentChildWidth ,0, instance.position.width, position.height);
            newRect.width = instance.position.width;
            GUILayout.BeginArea(newRect,EditorStyles.label);
            var sliderStyle = new GUIStyle(GUI.skin.verticalSlider);          
            GUILayout.FlexibleSpace();
            sliderPosition = GUILayout.BeginScrollView(sliderPosition, sliderStyle, GUILayout.Width(instance.position.width-100), GUILayout.Height(instance.position.height));
            var style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.UpperLeft;
            style.fontSize = 14;          
            if (tempDescription != string.Empty)
            {
                var directionText = Resources.Load<TextAsset>(tempDescription);
                EditorGUILayout.BeginHorizontal(GUILayout.Width(currentChildWidth));                               
                GUILayout.Label(directionText.text,style,GUILayout.ExpandHeight(true),GUILayout.ExpandWidth(true));
                EditorGUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
            GUILayout.EndArea();
          
        }     
    }
}
#endif