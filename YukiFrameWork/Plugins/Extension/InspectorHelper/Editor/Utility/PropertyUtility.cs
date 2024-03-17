///=====================================================
/// - FileName:      ProjectUtility.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/7 18:57:09
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;

namespace YukiFrameWork
{
	public static class PropertyUtility
	{
		public static Object GetTargetObject(SerializedProperty property)
		{
			return property.serializedObject.targetObject;
		}
	}
}
#endif