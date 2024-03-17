///=====================================================
/// - FileName:      PropertyBoolanDrawedInfo.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/7 19:27:45
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
    public class PropertyBoolanDrawedInfo : PropertyDrawedInfo
    {
        public string[] values { get; private set; } = { "关闭", "开启" };

        public PropertyBoolanDrawedInfo(UnityEngine.Object target, SerializedProperty property, BoolanPopupAttribute boolanPopup, ArrayLabelAttribute arrayLabel, ListDrawerSettingAttribute listDrawerSetting, DisplayTextureAttribute displayTexture, PropertyRangeAttribute propertyRange
            , LabelAttribute label, GUIColorAttribute color, EnableEnumValueIfAttribute[] enableEnumValueIfAttribute
            , DisableEnumValueIfAttribute[] disableEnumValueIfAttribute, EnableIfAttribute[] enableIf, DisableIfAttribute[] disableIf, HelperBoxAttribute helperBox, RuntimeDisabledGroupAttribute runtimeDisabledGroup
            , EditorDisabledGroupAttribute editorDisabledGroup,DisableGroupIfAttribute dgf,DisableGroupEnumValueIfAttribute def) 
            : base(target, property, boolanPopup, arrayLabel, listDrawerSetting, displayTexture, propertyRange, label, color, enableEnumValueIfAttribute
                  , disableEnumValueIfAttribute, enableIf, disableIf, helperBox, runtimeDisabledGroup, editorDisabledGroup,dgf,def)
        {
            values = new string[] { boolanPopup.FalseValue, boolanPopup.TrueValue };
        }

        public override void DrawHelperBox(bool rectValue, Rect rect)
        {
            base.DrawHelperBox();

            if (ArrayLabel != null)
            {
                Debug.LogWarning("ArrayLabel只能在列表(数组)并且列表(数组)标记了ListDrawerSetting特性才可以生效", PropertyUtility.GetTargetObject(Property));
                if (!rectValue)
                    EditorGUILayout.HelpBox("ArrayLabel只能在列表(数组)并且列表(数组)标记了ListDrawerSetting特性才可以生效", MessageType.Warning);
                else EditorGUI.HelpBox(rect, "ArrayLabel只能在列表(数组)并且列表(数组)标记了ListDrawerSetting特性才可以生效", MessageType.Warning);
            }
            if (DisplayTexture != null)
            {
                Debug.LogWarning("DisplayTexture只能用于Sprite,Texture变量", PropertyUtility.GetTargetObject(Property));
                if (!rectValue)
                    EditorGUILayout.HelpBox("DisplayTexture只能用于Sprite,Texture变量", MessageType.Warning);
                else EditorGUI.HelpBox(rect, "DisplayTexture只能用于Sprite,Texture变量", MessageType.Warning);
            }
            if (ListDrawerSetting != null)
            {
                Debug.LogWarning("ListDrawerSetting只能在数组/列表标记转换为ReorderableList", PropertyUtility.GetTargetObject(Property));
                if (!rectValue)
                    EditorGUILayout.HelpBox("ListDrawerSetting只能在数组/列表标记转换为ReorderableList", MessageType.Warning);
                else EditorGUI.HelpBox(rect, "ListDrawerSetting只能在数组/列表标记转换为ReorderableList", MessageType.Warning);
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
            int indexed = Property.boolValue ? 1 : 0;
            GUI.color = GUIColor != null ? GUIColor.Color : Color.white;
            indexed = EditorGUILayout.Popup(Content,indexed, values);
            GUI.color = Color.white;

            Property.boolValue = indexed == 1;
        }
    }
}
#endif