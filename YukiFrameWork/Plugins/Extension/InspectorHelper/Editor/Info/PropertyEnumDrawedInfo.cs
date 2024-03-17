///=====================================================
/// - FileName:      PropertyEnumDrawedInfo.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/7 18:11:52
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace YukiFrameWork
{
    public class PropertyEnumDrawedInfo : PropertyDrawedInfo
    {
        public FieldInfo[] fieldInfos { get; set; } = null;

        private Dictionary<int, GUIColorAttribute> colors = new Dictionary<int, GUIColorAttribute>();
        public PropertyEnumDrawedInfo(UnityEngine.Object target,BoolanPopupAttribute boolanPopup, ArrayLabelAttribute arrayLabel,ListDrawerSettingAttribute listDrawerSetting, DisplayTextureAttribute displayTexture, LabelAttribute label, PropertyRangeAttribute propertyRange, SerializedProperty property, GUIColorAttribute color, EnableEnumValueIfAttribute[] e
            , DisableEnumValueIfAttribute[] d, EnableIfAttribute[] e2, DisableIfAttribute[] d2, HelperBoxAttribute helperBox, RuntimeDisabledGroupAttribute rd, EditorDisabledGroupAttribute ed,DisableGroupIfAttribute dgf,DisableGroupEnumValueIfAttribute def) :
            base(target, property,boolanPopup,arrayLabel,listDrawerSetting, displayTexture, propertyRange, label, color, e, d, e2, d2, helperBox, rd, ed,dgf,def)   
        {
            
        }

        public GUIColorAttribute this[int index]
        {
            get => colors[index];
            set
            {
                colors[index] = value;
            }
        }

        public override void DrawHelperBox(bool rectValue,Rect rect)
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
            if (BoolanPopup != null)
            {
                Debug.LogWarning("BoolanPopup只能在bool变量上使用", PropertyUtility.GetTargetObject(Property));
                if (!rectValue)
                    EditorGUILayout.HelpBox("BoolanPopup只能在bool变量上使用", MessageType.Warning);
                else EditorGUI.HelpBox(rect, "BoolanPopup只能在bool变量上使用", MessageType.Warning);
            }
        }

        public override void OnGUI()
        {
            string[] names = Property.enumNames;
            int indexed =  Property.enumValueIndex;
            string[] displayNames = new string[names.Length];
            if (indexed == -1) indexed = 0;
            fieldInfos ??= ItemType.GetRuntimeFields().ToArray();
            for (int i = 0; i < displayNames.Length; i++)
            {
                LabelAttribute label = fieldInfos[i + 1].GetCustomAttribute<LabelAttribute>();
                GUIColorAttribute color = fieldInfos[i + 1].GetCustomAttribute<GUIColorAttribute>();
                if (color != null)
                    colors[i] = color;
                else colors[i] = new GUIColorAttribute(ColorType.White);
                displayNames[i] = label == null ? names[i] : label.Label;
            }
            EditorGUILayout.BeginHorizontal();
            GUI.color = GUIColor != null ? GUIColor.Color : Color.white;
            GUILayout.Label(Content, GUILayout.Width(EditorGUIUtility.labelWidth - 3));
            GUI.color = Color.white;    
            GUI.color = colors[indexed].Color;
            indexed = EditorGUILayout.Popup(indexed, displayNames);
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();

            Property.enumValueIndex = indexed;
        }
    }
}
#endif