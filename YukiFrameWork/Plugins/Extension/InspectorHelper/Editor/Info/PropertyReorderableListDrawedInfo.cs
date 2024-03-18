///=====================================================
/// - FileName:      PropertyReorderableListDrawedInfo.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/3 20:04:43
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using Object = UnityEngine.Object;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

namespace YukiFrameWork
{
    [Serializable]
    public sealed class PropertyReorderableListDrawedInfo : PropertyDrawedInfo
    {
        public ReorderableList ReorderableList { get; private set; }
        private ArrayLabelAttribute arrayLabel => ArrayLabel;
        private RangeAttribute defaultRange;
        private ListDrawerSettingAttribute listDrawerSetting => ListDrawerSetting;
        public PropertyReorderableListDrawedInfo(Object target,BoolanPopupAttribute boolanPopup,DisplayTextureAttribute displayTexture,ListDrawerSettingAttribute listDrawerSetting,ArrayLabelAttribute arrayLabel,RangeAttribute defaultRange,PropertyRangeAttribute propertyRange,LabelAttribute label, SerializedObject serializedObject, SerializedProperty property, GUIColorAttribute color
            , EnableEnumValueIfAttribute[] enableEnumValueIfAttribute, DisableEnumValueIfAttribute[] disableEnumValueIfAttribute, EnableIfAttribute[] enableIf
            , DisableIfAttribute[] disableIf, HelperBoxAttribute helperBox,RuntimeDisabledGroupAttribute rd
            ,EditorDisabledGroupAttribute ed,DisableGroupEnumValueIfAttribute def,DisableGroupIfAttribute dgf,SpaceAttribute sp) 
            : base(target,property,boolanPopup,arrayLabel,listDrawerSetting,displayTexture,propertyRange, label, color, enableEnumValueIfAttribute
                , disableEnumValueIfAttribute, enableIf, disableIf, helperBox,rd,ed,dgf,def,sp)
        {     
            this.defaultRange = defaultRange;   
            ReorderableList = new ReorderableList(serializedObject, property, true, true, true,true);
            Init();
        }     
        private FieldInfo[] fieldInfos = null;

        private Type type;
        private Dictionary<int, GUIColorAttribute> colors = new Dictionary<int, GUIColorAttribute>();

        public override void OnGUI()
        {
            base.OnGUI();
            ReorderableList.DoLayoutList();
        }
        private void Init()
        {          
            ReorderableList.drawHeaderCallback = (Rect rect) =>
            {
                if (GUIColor != null)
                    GUI.color = GUIColor.Color;               
                var newRect = new Rect(rect.xMax - 60, rect.y, 60, rect.height);
                EditorGUI.LabelField(rect, Content);
                GUI.color = Color.white;
               
                if (GUI.Button(newRect, "Clear", "minibutton"))
                {
                    ReorderableList.serializedProperty.arraySize = 0;
                }
            };
            ReorderableList.elementHeightCallback = ElementHeightCallback;
            ReorderableList.drawElementCallback = DrawElementCallback;
            ReorderableList.onRemoveCallback = OnRemoveCallBack;       
            
        }

        public override void DrawHelperBox(bool rectValue,Rect rect)
        {
            base.DrawHelperBox();

            if (DisplayTexture != null)
            {
                string message = "DisplayTexture只能用于Sprite,Texture变量";
                Debug.LogWarning(message, PropertyUtility.GetTargetObject(Property));
                if (!rectValue)
                    EditorGUILayout.HelpBox(message, MessageType.Warning);
                else EditorGUI.HelpBox(rect, message, MessageType.Warning);
            }
            if (BoolanPopup != null)
            {
                string message = "BoolanPopup只能在bool变量上使用";
                Debug.LogWarning(message, PropertyUtility.GetTargetObject(Property));
                if(!rectValue)
                    EditorGUILayout.HelpBox(message, MessageType.Warning);
                else EditorGUI.HelpBox(rect,message, MessageType.Warning);
            }

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

        private void OnRemoveCallBack(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
            if (ReorderableList.count == 0) ReorderableList.elementHeight = 20;

        }
        private void DrawElementCallback(Rect rect, int index, bool selected, bool focused)
        {
            try
            {
                if (listDrawerSetting.IsAutoOnly)
                    EditorGUI.BeginDisabledGroup(listDrawerSetting.IsReadOnly);
                else
                {
                    var obj = GlobalReflectionSystem.GetValue(target.GetType(), target, listDrawerSetting.ValueName);

                    if (obj is bool value)
                        EditorGUI.BeginDisabledGroup(listDrawerSetting.IsReversal ? value : !value);
                    else EditorGUI.BeginDisabledGroup(false);
                }
                //根据index获取对应元素

                SerializedProperty item = null;
                if (index >= 0 && index < ReorderableList.serializedProperty.arraySize)
                    item = ReorderableList.serializedProperty.GetArrayElementAtIndex(index);               
                rect.height -= 6;
                rect.y += 4;
                
                GUIContent elementContent = new GUIContent(arrayLabel != null ? arrayLabel.Label + " " + index.ToString() : string.Empty);

                var defaultRect =
                    new Rect
                    (arrayLabel != null ? rect.x + 20 : rect.x, rect.y, rect.width - (arrayLabel != null ? 20 : 0), rect.height);

                if (ItemType.IsGenericType)
                    type = ItemType.GetGenericArguments()[0];
                else if (ItemType.IsArray)
                    type = ItemType.GetElementType();

                if (item? .propertyType == SerializedPropertyType.Enum)
                {
                    string[] names = item.enumNames;
                    int indexed = item.enumValueIndex;
                    string[] displayNames = new string[names.Length];

                    fieldInfos ??= type.GetRuntimeFields().ToArray();
                    for (int i = 0; i < displayNames.Length; i++)
                    {
                        LabelAttribute label = fieldInfos[i + 1].GetCustomAttribute<LabelAttribute>();
                        GUIColorAttribute color = fieldInfos[i + 1].GetCustomAttribute<GUIColorAttribute>();
                        if (color != null)
                            colors[i] = color;
                        else colors[i] = new GUIColorAttribute(ColorType.White);
                        displayNames[i] = label == null ? names[i] : label.Label;
                    }

                    GUI.color = colors[indexed].Color;
                    indexed = EditorGUI.Popup(defaultRect, indexed, displayNames);
                    GUI.color = Color.white;

                    item.enumValueIndex = indexed;
                }
                else if ((item.propertyType == SerializedPropertyType.Integer
                   || item.propertyType == SerializedPropertyType.Float)
                   && (PropertyRange != null || defaultRange != null))
                {
                    float minValue = PropertyRange != null ? PropertyRange.MinValue : defaultRange.min;
                    float maxValue = PropertyRange != null ? PropertyRange.MaxValue : defaultRange.max;
                    if (type == typeof(float) || type == typeof(double))
                    {
                        EditorGUI.Slider(defaultRect, item, minValue, maxValue, elementContent);
                    }
                    else
                        EditorGUI.IntSlider(defaultRect, item, (int)minValue, (int)maxValue, elementContent);
                }
                else
                {
                    if ((PropertyRange != null || defaultRange != null))
                    {
                        EditorGUILayout.HelpBox("PropertyRange特性只能用于Intter跟Single的操作变量", MessageType.Warning);
                    }
                    EditorGUI.PropertyField(defaultRect, item, elementContent, true);
                }            
                EditorGUI.EndDisabledGroup();
            }
            catch { }
        }     
        private float ElementHeightCallback(int index)
        {
            SerializedProperty element = ReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            float height = EditorGUI.GetPropertyHeight(element) + EditorGUIUtility.standardVerticalSpacing;
            return height;          
        }
    }
}
#endif