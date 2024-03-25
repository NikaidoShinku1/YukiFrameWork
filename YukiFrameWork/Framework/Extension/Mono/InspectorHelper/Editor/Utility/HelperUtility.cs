///=====================================================
/// - FileName:      HelperUtility.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/19 22:20:53
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
	public static class HelperUtility
	{
		public static void DrawHelperWarning(SerializedProperty property,bool displayTexture,bool boolanPopup,bool propertyRange)
		{
            if (displayTexture)
            {
                string message = "DisplayTexture只能用于Sprite,Texture变量";
                EditorGUILayout.HelpBox(message, MessageType.Warning);
                Debug.LogWarning(message, PropertyUtility.GetTargetObject(property));              
               
            }
            if (boolanPopup)
            {
                string message = "BoolanPopup只能在bool变量上使用"; 
                EditorGUILayout.HelpBox(message, MessageType.Warning);
                Debug.LogWarning(message, PropertyUtility.GetTargetObject(property));
               
                
            }

            if (propertyRange)
            {
                string message = "PropertyRange特性只能用于Intter跟Single的操作变量";
                EditorGUILayout.HelpBox(message, MessageType.Warning);
                Debug.LogWarning(message, PropertyUtility.GetTargetObject(property));  
            }
        }
	}
}
#endif