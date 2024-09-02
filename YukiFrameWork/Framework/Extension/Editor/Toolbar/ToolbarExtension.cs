///=====================================================
/// - FileName:      ToolbarExtender.cs
/// - NameSpace:     YukiFrameWork.ToolBar
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/9/2 12:24:58
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
#if UNITY_EDITOR
using UnityEngine;
using System;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
namespace YukiFrameWork.ToolBar
{
    public enum OnGUISide : byte
    {
        Left,
        Right,
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public sealed class ToolbarAttribute : Attribute
    {
        public OnGUISide Side { get; }
        public int Priority { get; }

        public ToolbarAttribute(OnGUISide side, int priority)
        {
            Side = side;
            Priority = priority;
        }
    }
    [InitializeOnLoad]
    public static class ToolbarExtension
    {
        private static readonly List<(int, Action)> s_LeftToolbarGUI = new List<(int, Action)>();
        private static readonly List<(int, Action)> s_RightToolbarGUI = new List<(int, Action)>();

        static ToolbarExtension()
        {
            ToolbarCallback.OnToolbarGUILeft = GUILeft;
            ToolbarCallback.OnToolbarGUIRight = GUIRight;
            Type attributeType = typeof(ToolbarAttribute);

            foreach (var methodInfo in TypeCache.GetMethodsWithAttribute<ToolbarAttribute>())
            {
                var attributes = methodInfo.GetCustomAttributes(attributeType, false);
                if (attributes.Length > 0)
                {
                    ToolbarAttribute attribute = (ToolbarAttribute)attributes[0];
                    if (attribute.Side == OnGUISide.Left)
                    {
                        s_LeftToolbarGUI.Add((attribute.Priority, delegate
                        {
                            methodInfo.Invoke(null, null);
                        }
                        ));
                        continue;
                    }
                    if (attribute.Side == OnGUISide.Right)
                    {
                        s_RightToolbarGUI.Add((attribute.Priority, delegate
                        {
                            methodInfo.Invoke(null, null);
                        }
                        ));
                        continue;
                    }
                }
            }
            s_LeftToolbarGUI.Sort((tuple1, tuple2) => tuple1.Item1 - tuple2.Item1);
            s_RightToolbarGUI.Sort((tuple1, tuple2) => tuple2.Item1 - tuple1.Item1);
        }

        static void GUILeft()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            foreach (var handler in s_LeftToolbarGUI)
            {
                handler.Item2();
            }

            GUILayout.EndHorizontal();
        }

        static void GUIRight()
        {
            GUILayout.BeginHorizontal();
            foreach (var handler in s_RightToolbarGUI)
            {
                handler.Item2();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }    
}
#endif