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
using System;
using Object = UnityEngine.Object;
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

        public static GUIContent GetLabel(InfoData item, LabelAttribute label)
        {
            return new GUIContent(label != null ? label.Label : char.ToUpper(item.property.displayName[0]) + item.property.displayName.Substring(1));
        }

        public static bool CheckPropertyInGeneric(Type itemType)
        {
            if (itemType == null) return false;
            if (itemType.IsGenericType || itemType.IsArray) return true;

            return false;
        }

        public static bool CheckPropertyInTexture(Type itemType, DisplayTextureAttribute displayTexture)
        {
            if (itemType == null) return false;

            return (itemType.Equals(typeof(Texture)) || itemType.Equals(typeof(Texture2D)) || itemType.Equals(typeof(Sprite))) && displayTexture != null;
        }

        public static bool CheckPropertyInBoolan(Type itemType, SerializedProperty property)
        {
            if (itemType == null || property == null) return false;

            return (itemType.IsSubclassOf(typeof(bool)) || itemType.Equals(typeof(bool)) || property.propertyType == SerializedPropertyType.Boolean);
        }

        public static bool CheckPropertyInEnum(Type itemType, SerializedProperty property)
        {
            if (itemType == null || property == null) return false;

            return (itemType.IsSubclassOf(typeof(Enum)) || itemType.Equals(typeof(Enum)) || property.propertyType == SerializedPropertyType.Enum);
        }
    }
}
#endif