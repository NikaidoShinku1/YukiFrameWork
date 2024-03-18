///=====================================================
/// - FileName:      PropertyMemberDrawedInfo.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/3 20:02:05
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;

namespace YukiFrameWork
{
    [Serializable]
    public sealed class PropertyMemberDrawedInfo : PropertyDrawedInfo
    {  
        public PropertyMemberDrawedInfo(UnityEngine.Object target,BoolanPopupAttribute boolanPopup,ArrayLabelAttribute arrayLabel,ListDrawerSettingAttribute listDrawerSetting,DisplayTextureAttribute displayTexture,LabelAttribute label,PropertyRangeAttribute propertyRange, SerializedProperty property, GUIColorAttribute color, EnableEnumValueIfAttribute[] e
            , DisableEnumValueIfAttribute[] d, EnableIfAttribute[] e2, DisableIfAttribute[] d2, HelperBoxAttribute helperBox,RuntimeDisabledGroupAttribute rd
            ,EditorDisabledGroupAttribute ed,DisableGroupIfAttribute dgf,DisableGroupEnumValueIfAttribute def,SpaceAttribute spaceAttribute) :
            base(target,property,boolanPopup,arrayLabel,listDrawerSetting,displayTexture,propertyRange, label, color, e, d, e2, d2, helperBox,rd,ed,dgf,def,spaceAttribute)
        {
            
        }

        public override void OnGUI()
        {
            base.OnGUI();
            EditorGUI.BeginDisabledGroup(OnDefaultDisableGroup() && !CheckPropertyInGeneric());
            if ((this.Property.propertyType == SerializedPropertyType.Integer
                || this.Property.propertyType == SerializedPropertyType.Float)
                && this.PropertyRange != null)
            {
                DefaultProperty(Content, this, true);
            }
            else
            {
                if (this.PropertyRange != null)
                    EditorGUILayout.HelpBox("PropertyRange特性只能用于Intter跟Single的操作变量", MessageType.Warning);
                DefaultProperty(Content,this);
            }
            EditorGUI.EndDisabledGroup();
        }

        public override void DrawHelperBox(bool rectValue = false,Rect rect = new Rect())
        {
            base.DrawHelperBox();
            if (ArrayLabel != null)
            {
                Debug.LogWarning("ArrayLabel只能在列表(数组)并且列表(数组)标记了ListDrawerSetting特性才可以生效", PropertyUtility.GetTargetObject(Property));
                if(!rectValue)
                    EditorGUILayout.HelpBox("ArrayLabel只能在列表(数组)并且列表(数组)标记了ListDrawerSetting特性才可以生效", MessageType.Warning);
                else EditorGUI.HelpBox(rect,"ArrayLabel只能在列表(数组)并且列表(数组)标记了ListDrawerSetting特性才可以生效", MessageType.Warning);
            }
            if (DisplayTexture != null)
            {
                Debug.LogWarning("DisplayTexture只能用于Sprite,Texture变量", PropertyUtility.GetTargetObject(Property));
                if (!rectValue)
                    EditorGUILayout.HelpBox("DisplayTexture只能用于Sprite,Texture变量", MessageType.Warning);
                else EditorGUI.HelpBox(rect,"DisplayTexture只能用于Sprite,Texture变量", MessageType.Warning);
            }
            if (ListDrawerSetting != null)
            {
                Debug.LogWarning("ListDrawerSetting只能在数组/列表标记转换为ReorderableList", PropertyUtility.GetTargetObject(Property));
                if(!rectValue)
                    EditorGUILayout.HelpBox("ListDrawerSetting只能在数组/列表标记转换为ReorderableList", MessageType.Warning);
                else EditorGUI.HelpBox(rect,"ListDrawerSetting只能在数组/列表标记转换为ReorderableList", MessageType.Warning);
            }
            if (BoolanPopup != null)
            {
                Debug.LogWarning("BoolanPopup只能在bool变量上使用", PropertyUtility.GetTargetObject(Property));
                if (!rectValue) 
                    EditorGUILayout.HelpBox("BoolanPopup只能在bool变量上使用", MessageType.Warning);
                else EditorGUI.HelpBox(rect,"BoolanPopup只能在bool变量上使用", MessageType.Warning);
            }

            if (ItemType != null && (ItemType.IsArray || ItemType.IsGenericType))
            {
                if (EditorDisabledGroup != null)
                {
                    string message = "EditorDisabledGroup无法在派生自Unity Engine.Object的类中使用(标记Serializable的类正常使用)，请使用ListDrawingSetting的只读标记";
                    Debug.LogWarning(message, PropertyUtility.GetTargetObject(Property));
                    if (!rectValue)
                        EditorGUILayout.HelpBox(message, MessageType.Warning);
                    else EditorGUI.HelpBox(rect, message, MessageType.Warning);
                }

                if (RuntimeDisabledGroup != null)
                {
                    string message = "RuntimeDisabledGroup无法在派生自Unity Engine.Object的类中使用(标记Serializable的类正常使用)，请使用ListDrawingSetting的只读标记";
                    Debug.LogWarning(message, PropertyUtility.GetTargetObject(Property));
                    if (!rectValue)
                        EditorGUILayout.HelpBox(message, MessageType.Warning);
                    else EditorGUI.HelpBox(rect, message, MessageType.Warning);
                }

                if (DisableGroupEnumValueIf != null)
                {
                    string message = "DisableGroupEnumValueIf无法在派生自Unity Engine.Object的类中使用(标记Serializable的类正常使用)，请使用ListDrawingSetting的只读标记";
                    Debug.LogWarning(message, PropertyUtility.GetTargetObject(Property));
                    if (!rectValue)
                        EditorGUILayout.HelpBox(message, MessageType.Warning);
                    else EditorGUI.HelpBox(rect, message, MessageType.Warning);
                }

                if (DisableGroupIf != null)
                {
                    string message = "DisableGroupIf无法在派生自Unity Engine.Object的类中对数组/列表使用(标记Serializable的类正常使用)，请使用ListDrawingSetting的只读标记";
                    Debug.LogWarning(message, PropertyUtility.GetTargetObject(Property));
                    if (!rectValue)
                        EditorGUILayout.HelpBox(message, MessageType.Warning);
                    else EditorGUI.HelpBox(rect, message, MessageType.Warning);
                }
            }
        }

        private void DefaultProperty(GUIContent content, PropertyDrawedInfo member, bool isValueType = false)
        {        
            if (member.GUIColor != null)
                GUI.color = member.GUIColor.Color;          
            if (!isValueType)
                EditorGUILayout.PropertyField(member.Property, content, true);
            else
            {
                float minValue = member.PropertyRange.MinValue;
                float maxValue = member.PropertyRange.MaxValue;
                if (member.ItemType == typeof(float) || member.ItemType == typeof(double))
                    EditorGUILayout.Slider(member.Property, minValue, maxValue, content);
                else
                    EditorGUILayout.IntSlider(member.Property, (int)minValue, (int)maxValue, content);
            }
            GUI.color = Color.white;          
        }
    }
}
#endif