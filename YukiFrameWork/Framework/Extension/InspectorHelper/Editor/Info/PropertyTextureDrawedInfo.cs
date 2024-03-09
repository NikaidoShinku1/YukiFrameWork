///=====================================================
/// - FileName:      PropertyTextureDrawedInfo.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/3 20:02:43
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
    public sealed class PropertyTextureDrawedInfo : PropertyDrawedInfo
    {   
        public PropertyTextureDrawedInfo(UnityEngine.Object target,BoolanPopupAttribute boolanPopup,ArrayLabelAttribute arrayLabel,ListDrawerSettingAttribute listDrawerSetting,DisplayTextureAttribute displayTexture,PropertyRangeAttribute propertyRange
            , SerializedProperty property, LabelAttribute label, GUIColorAttribute color
            , EnableEnumValueIfAttribute[] enableEnumValueIfAttribute, DisableEnumValueIfAttribute[] disableEnumValueIfAttribute
            , EnableIfAttribute[] enableIf, DisableIfAttribute[] disableIf, HelperBoxAttribute helperBox,RuntimeDisabledGroupAttribute rd,EditorDisabledGroupAttribute ed) 
            : base(target,property,boolanPopup,arrayLabel,listDrawerSetting,displayTexture,propertyRange, label, color, enableEnumValueIfAttribute, disableEnumValueIfAttribute, enableIf, disableIf, helperBox,rd,ed)
        {
            this.DisplayTexture = displayTexture;
        }

        public override void DrawHelperBox(bool rectValue, Rect rect)
        {
            base.DrawHelperBox();
            if (PropertyRange != null)
            {
                Debug.LogWarning("PropertyRange特性只能用于Intter跟Single的操作变量", PropertyUtility.GetTargetObject(Property));
                if(!rectValue)
                    EditorGUILayout.HelpBox("PropertyRange特性只能用于Intter跟Single的操作变量", MessageType.Warning);
                else EditorGUI.HelpBox(rect,"PropertyRange特性只能用于Intter跟Single的操作变量", MessageType.Warning);
            }
            if (ArrayLabel != null)
            {
                Debug.LogWarning("ArrayLabel只能在列表(数组)并且列表(数组)标记了ListDrawerSetting特性才可以生效", PropertyUtility.GetTargetObject(Property));
                if (!rectValue)
                    EditorGUILayout.HelpBox("ArrayLabel只能在列表(数组)并且列表(数组)标记了ListDrawerSetting特性才可以生效", MessageType.Warning);
                else EditorGUI.HelpBox(rect, "ArrayLabel只能在列表(数组)并且列表(数组)标记了ListDrawerSetting特性才可以生效", MessageType.Warning);
            }
            if (ListDrawerSetting != null)
            {
                Debug.LogWarning("ListDrawerSetting只能在数组/列表标记转换为ReorderableList", PropertyUtility.GetTargetObject(Property));
                if (!rectValue)
                    EditorGUILayout.HelpBox("ListDrawerSetting只能在数组/列表标记转换为ReorderableList", MessageType.Warning);
                else EditorGUI.HelpBox(rect, "ListDrawerSetting只能在数组/列表标记转换为ReorderableList", MessageType.Warning);
            }
            if (BoolanPopup != null)
            {
                Debug.LogWarning("BoolanPopup只能在bool变量上使用", PropertyUtility.GetTargetObject(Property));
                if (!rectValue)
                    EditorGUILayout.HelpBox("BoolanPopup只能在bool变量上使用", MessageType.Warning);
                else EditorGUI.HelpBox(rect, "BoolanPopup只能在bool变量上使用", MessageType.Warning);
            }
            if (PropertyRange != null)
            {
                Debug.LogWarning("PropertyRange特性只能用于Intter跟Single的操作变量", PropertyUtility.GetTargetObject(Property));
                if (!rectValue)
                    EditorGUILayout.HelpBox("PropertyRange特性只能用于Intter跟Single的操作变量", MessageType.Warning);
                else EditorGUI.HelpBox(rect, "PropertyRange特性只能用于Intter跟Single的操作变量", MessageType.Warning);
            }
        }

        public override void OnGUI()
        {    
            Rect rect = new Rect(EditorGUILayout.BeginVertical(GUILayout.Width(EditorGUIUtility.currentViewWidth)))
            {
                x = EditorGUIUtility.currentViewWidth - 100,
                width = DisplayTexture.Width,
                height = DisplayTexture.Height,
            };
            float x = rect.x;

            Rect labelRect = new Rect(rect)
            {
                x = 20,
                width = EditorGUIUtility.currentViewWidth
            };
            GUI.color = GUIColor == null ? Color.white : GUIColor.Color;
            EditorGUI.LabelField(labelRect, Content);
            GUI.color = Color.white;
            GUILayout.FlexibleSpace();
            Property.objectReferenceValue = EditorGUI.ObjectField(rect, Property.objectReferenceValue, typeof(Texture), false);
            EditorGUILayout.Space(DisplayTexture.Height);
            EditorGUILayout.EndVertical();
        }   
    }
}
#endif