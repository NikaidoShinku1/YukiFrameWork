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
            , DisableIfAttribute[] disableIf, HelperBoxAttribute helperBox,RuntimeDisabledGroupAttribute rd,EditorDisabledGroupAttribute ed) : base(target,property,boolanPopup,arrayLabel,listDrawerSetting,displayTexture,propertyRange, label, color, enableEnumValueIfAttribute
                , disableEnumValueIfAttribute, enableIf, disableIf, helperBox,rd,ed)
        {     
            this.defaultRange = defaultRange;   
            ReorderableList = new ReorderableList(serializedObject, property, true, true, false,false);
            Init();
        }     
        private FieldInfo[] fieldInfos = null;

        private Type type;
        private Dictionary<int, GUIColorAttribute> colors = new Dictionary<int, GUIColorAttribute>();

        public override void OnGUI()
        {
            ReorderableList.DoLayoutList();
        }
        private void Init()
        {          
            ReorderableList.drawHeaderCallback = (Rect rect) =>
            {
                if (GUIColor != null)
                    GUI.color = GUIColor.Color;               
                var newRect = new Rect(rect.xMax - 30, rect.y, 30, rect.height);
                EditorGUI.LabelField(rect, Content);
                GUI.color = Color.white;
               
                if (GUI.Button(newRect, "+"))
                {
                    ReorderableList.serializedProperty.arraySize++;
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
                Debug.LogWarning("DisplayTexture只能用于Sprite,Texture变量", PropertyUtility.GetTargetObject(Property));
                if (!rectValue)
                    EditorGUILayout.HelpBox("DisplayTexture只能用于Sprite,Texture变量", MessageType.Warning);
                else EditorGUI.HelpBox(rect, "DisplayTexture只能用于Sprite,Texture变量", MessageType.Warning);
            }
            if (BoolanPopup != null)
            {
                Debug.LogWarning("BoolanPopup只能在bool变量上使用", PropertyUtility.GetTargetObject(Property));
                if(!rectValue)
                    EditorGUILayout.HelpBox("BoolanPopup只能在bool变量上使用", MessageType.Warning);
                else EditorGUI.HelpBox(rect,"BoolanPopup只能在bool变量上使用", MessageType.Warning);
            }
        }

        private void OnRemoveCallBack(ReorderableList list)
        {
            ReorderableList.defaultBehaviours.DoRemoveButton(list);
            if (ReorderableList.count == 0) ReorderableList.elementHeight = 20;

        }
        private void DrawElementCallback(Rect rect, int index, bool selected, bool focused)
        {
            EditorGUI.BeginDisabledGroup(listDrawerSetting.IsReadOnly);
            //根据index获取对应元素 
            SerializedProperty item = ReorderableList.serializedProperty.GetArrayElementAtIndex(index);
            rect.height -= 6;
            rect.y += 4;
            Rect removeRect;            
            removeRect = new Rect(rect.xMax - 30, rect.y, 30, rect.height);
            GUIContent elementContent = new GUIContent(arrayLabel != null ? arrayLabel.Label + " " + index.ToString() : string.Empty);

            var defaultRect =
                new Rect
                (arrayLabel != null ? rect.x + 20 : rect.x, rect.y, rect.width - (arrayLabel != null ? 60 : 30), rect.height);

            if (ItemType.IsGenericType)
                type = ItemType.GetGenericArguments()[0];
            else if (ItemType.IsArray)
                type = ItemType.GetElementType();
         
            if (item.propertyType == SerializedPropertyType.Enum)
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
                float minValue = PropertyRange.MinValue;
                float maxValue = PropertyRange.MaxValue;
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
            if (GUI.Button(removeRect, "X"))
            {
                ReorderableList.onRemoveCallback?.Invoke(ReorderableList);                
            }
            EditorGUI.EndDisabledGroup();
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