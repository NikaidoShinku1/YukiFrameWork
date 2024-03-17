///=====================================================
/// - FileName:      RuntimeDisableGroupDrawer.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/8 22:20:05
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
namespace YukiFrameWork
{
	[CustomPropertyDrawer(typeof(RuntimeDisabledGroupAttribute),true)]
	public class RuntimeDisableGroupDrawer : BasePropertyDrawer
	{
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            base.OnGUI(position, property, label);
        }
    }
}
#endif