///=====================================================
/// - FileName:      BasePropertyDrawer.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/7 22:21:55
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection; 
using System.Linq;

namespace YukiFrameWork
{   
    public class BasePropertyDrawer : PropertyDrawer
	{
        protected PropertyDrawedInfo info;
        protected SerializedProperty property;
        protected bool IsMonoObjectValue => fieldInfo?.DeclaringType.IsSubclassOf(typeof(UnityEngine.Object)) == true;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label);
        }

        //protected bool IsIEnumerableValue => fieldInfo?.DeclaringType.GetInterfaces().FirstOrDefault(x => x == typeof(IEnumerable)) != null;
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {           
            EditorGUI.BeginChangeCheck();          
            if (IsMonoObjectValue)
            {             
                property.serializedObject.Update();
                GUIColorAttribute color = fieldInfo.GetCustomAttribute<GUIColorAttribute>(true);                               
                GUI.color = color != null ? color.Color : Color.white;               
                EditorGUI.PropertyField(position, property, label,true);                
                GUI.color = Color.white;
            }
            else
            {                
                if (info == null)
                {
                    InitInfo(property);
                }
            
                EditorGUI.BeginDisabledGroup(OnDisableGroup(info) || info.OnDefaultDisableGroup());        
                if (info.Label != null)
                    label = new GUIContent(info.Label.Label);
                GUI.color = info.GUIColor != null ? info.GUIColor.Color : Color.white;
                if (info is PropertyMemberDrawedInfo memberInfo)
                {
                    property.serializedObject.Update();
                    if (memberInfo.PropertyRange != null
                        && property.propertyType == SerializedPropertyType.Integer || property.propertyType == SerializedPropertyType.Float)
                    {
                        if (property.propertyType == SerializedPropertyType.Integer)
                            EditorGUI.IntSlider(position, property, (int)memberInfo.PropertyRange.MinValue, (int)memberInfo.PropertyRange.MaxValue, label);
                        else EditorGUI.Slider(position, property, memberInfo.PropertyRange.MinValue, memberInfo.PropertyRange.MaxValue, label);
                    }
                    else
                    {
                        EditorGUI.PropertyField(position, property, label, true);
                    }
                }
                else if (info is PropertyBoolanDrawedInfo boolanDrawedInfo)
                {
                    if (boolanDrawedInfo.BoolanPopup != null)
                    {
                        int indexed = property.boolValue ? 1 : 0;

                        indexed = EditorGUI.Popup(position, label.text, indexed, boolanDrawedInfo.values);
                        property.boolValue = indexed == 1;
                    }
                    else
                        EditorGUI.PropertyField(position, property, label, true);
                }              
                else if (info is PropertyEnumDrawedInfo enumDrawedInfo)
                {
                    string[] names = property.enumNames;
                    int indexed = property.enumValueIndex;
                    string[] displayNames = new string[names.Length];
                    if (indexed == -1) indexed = 0;
                    var fieldInfos = enumDrawedInfo.fieldInfos;
                    fieldInfos ??= fieldInfo.FieldType.GetRuntimeFields().ToArray();
                    for (int i = 0; i < displayNames.Length; i++)
                    {
                        LabelAttribute l = fieldInfos[i + 1].GetCustomAttribute<LabelAttribute>();
                        GUIColorAttribute color = fieldInfos[i + 1].GetCustomAttribute<GUIColorAttribute>();
                        if (color != null)
                            enumDrawedInfo[i] = color;
                        else enumDrawedInfo[i] = new GUIColorAttribute(ColorType.White);
                        displayNames[i] = l == null ? names[i] : l.Label;
                    }
                    GUI.color = enumDrawedInfo[indexed].Color;
                    float x = position.x;
                    indexed = EditorGUI.Popup(position, label.text, indexed, displayNames);
                    GUI.color = Color.white;
                    property.enumValueIndex = indexed;
                }
                else
                {
                    EditorGUI.PropertyField(position, property, label, true);
                }

                GUI.color = Color.white;


            }
            EditorGUI.EndDisabledGroup();
            if (EditorGUI.EndChangeCheck())
            {               
                EditorUtility.SetDirty(property.serializedObject.targetObject);
                AssetDatabase.SaveAssets();
                property.serializedObject.ApplyModifiedProperties();                
            }
        }

        public bool CheckPropertyInTexture(Type itemType, DisplayTextureAttribute displayTexture)
        {
            if (itemType == null) return false;

            return (itemType.Equals(typeof(Texture)) || itemType.Equals(typeof(Texture2D)) || itemType.Equals(typeof(Sprite))) && displayTexture != null;
        }

        public bool CheckPropertyInBoolan(Type itemType, SerializedProperty property)
        {
            if (itemType == null || property == null) return false;

            return ((itemType.IsSubclassOf(typeof(bool)) || itemType.Equals(typeof(bool))) && property.propertyType == SerializedPropertyType.Boolean);
        }

        public bool CheckPropertyInGeneric(Type itemType)
        {
            if (itemType == null) return false;
            if (itemType.IsGenericType || itemType.IsArray) return true;

            return false;
        }

        protected bool OnDisableGroup(PropertyDrawedInfo info)
        {
            bool IsPlaying = Application.isPlaying;
            if (info.RuntimeDisabledGroup != null && info.EditorDisabledGroup != null)
                return true;

            if (info.RuntimeDisabledGroup != null)
                return IsPlaying;
            else if (info.EditorDisabledGroup != null)
                return !IsPlaying;

            return false;
        }

        public bool CheckPropertyInEnum(Type itemType, SerializedProperty property)
        {
            if (itemType == null || property == null) return false;

            return itemType.IsSubclassOf(typeof(Enum)) || itemType.Equals(typeof(Enum));
        }

        public virtual void InitInfo(SerializedProperty property)
        {
            if (this.info != null) return;

            fieldInfo.CreateAllSettingAttribute(out var label, out var color,
                out var enableEnumValue, out var disableEnumValue
                , out var enableIf, out var disableIf, out var helperBox
                , out var group, out DisplayTextureAttribute displayTexture
               , out var propertyRange, out var arrayLabel, out var defaultRange, out var runtimeDisableGroup
               , out var editorDisabledGroup, out var listDrawerSetting
               , out BoolanPopupAttribute boolanPopup,out DisableGroupIfAttribute disableGroupIf,out DisableGroupEnumValueIfAttribute disableGroupEnumValueIf);


            if (CheckPropertyInGeneric(fieldInfo.FieldType) && listDrawerSetting != null)
            {              
                var listInfo = new PropertyReorderableListDrawedInfo
                    (property.serializedObject.targetObject, boolanPopup, displayTexture, listDrawerSetting, arrayLabel, defaultRange, propertyRange, label, property.serializedObject
                    , property, color, enableEnumValue, disableEnumValue, enableIf, disableIf, helperBox, runtimeDisableGroup, editorDisabledGroup,disableGroupEnumValueIf,disableGroupIf);
                listInfo.ItemType = fieldInfo.FieldType;
                this.info = listInfo;
               
            }
            else if (CheckPropertyInBoolan(fieldInfo.FieldType, property) && boolanPopup != null)
            {
                var bInfo = new PropertyBoolanDrawedInfo(property.serializedObject.targetObject
                    , property, boolanPopup, arrayLabel, listDrawerSetting, displayTexture
                    , propertyRange, label, color, enableEnumValue
                    , disableEnumValue, enableIf, disableIf, helperBox
                    , runtimeDisableGroup, editorDisabledGroup,disableGroupIf,disableGroupEnumValueIf);
                bInfo.ItemType = fieldInfo.FieldType;
                this.info = bInfo;
            }
            else if (CheckPropertyInEnum(fieldInfo.FieldType, property))
            {
                var eInfo = new PropertyEnumDrawedInfo(property.serializedObject.targetObject
                    , boolanPopup, arrayLabel, listDrawerSetting, displayTexture, label
                    , propertyRange, property, color, enableEnumValue, disableEnumValue
                    , enableIf, disableIf, helperBox, runtimeDisableGroup, editorDisabledGroup,disableGroupIf,disableGroupEnumValueIf);
                eInfo.ItemType = fieldInfo.FieldType;
                this.info = eInfo;
            }
            else if (CheckPropertyInTexture(fieldInfo.FieldType, displayTexture))
            {
                var tInfo = new PropertyTextureDrawedInfo(property.serializedObject.targetObject
                    , boolanPopup, arrayLabel, listDrawerSetting, displayTexture, propertyRange, property, label
                    , color, enableEnumValue, disableEnumValue
                    , enableIf, disableIf, helperBox, runtimeDisableGroup, editorDisabledGroup,disableGroupIf,disableGroupEnumValueIf);
                tInfo.ItemType = fieldInfo.FieldType;
                this.info = tInfo;
            }
            else
            {
                var info = new PropertyMemberDrawedInfo(property.serializedObject.targetObject, boolanPopup
                    , arrayLabel, listDrawerSetting, displayTexture, label, propertyRange
                    , property, color, enableEnumValue, disableEnumValue, enableIf
                    , disableIf, helperBox, runtimeDisableGroup, editorDisabledGroup,disableGroupIf,disableGroupEnumValueIf);
                info.ItemType = fieldInfo.FieldType;
                this.info = info;
            }

        }
    }  

 

}
#endif
