///=====================================================
/// - FileName:      GUIColorPropertyDrawer.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/8 3:38:57
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace YukiFrameWork
{
    [CustomPropertyDrawer(typeof(GUIColorAttribute), true)]
    public class GUIColorPropertyDrawer : BasePropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }
    }
}
#endif