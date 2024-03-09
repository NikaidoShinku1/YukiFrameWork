///=====================================================
/// - FileName:      MemberInfoLayer.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/1 21:14:53
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using YukiFrameWork.Extension;
using System.Collections.Generic;
using System;
using Object = UnityEngine.Object;
using System.Reflection;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;

namespace YukiFrameWork
{
    public class MemberInfoLayer : GenericLayer
    {
        class InfoData
        {
            public string groupName;
            public FastList<PropertyDrawedInfo> PropertyDrawedInfos = new FastList<PropertyDrawedInfo>();

            public InfoData Add(PropertyDrawedInfo info)
            {
                PropertyDrawedInfos.Add(info);
                return this;
            }
        }
        private Editor editor;
        private Queue<InfoData> infoDatas;
        private SerializedObject serializedObject;
        public MemberInfoLayer(Object target, Type type,Editor editor)
        {
            infoDatas = new Queue<InfoData>();
            this.editor = editor;
            InitMemberInfos(target, type);
        }

        public MemberInfoLayer(SerializedObject serializedObject, Type type)
        {
            infoDatas = new Queue<InfoData>();
            this.serializedObject = serializedObject;
            this.target = serializedObject.targetObject;
            InitMemberInfos(type);
        }
        private Object target;

        private void InitMemberInfos(Type type)
        {       
            IEnumerable<MemberInfo> memberInfos = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.SetProperty | BindingFlags.Instance | BindingFlags.Default | BindingFlags.FlattenHierarchy);
            foreach (var member in memberInfos)
            {              
                SerializedProperty property = serializedObject.FindProperty(member.Name);               
                HideInInspector hideInInspector = member.GetCustomAttribute<HideInInspector>();
               
                if (property == null || hideInInspector != null) continue;

                member.CreateAllSettingAttribute
                (out LabelAttribute label
                , out GUIColorAttribute color, out EnableEnumValueIfAttribute[] enableEnumValueIfAttribute
                , out DisableEnumValueIfAttribute[] disable, out EnableIfAttribute[] enableIf, out DisableIfAttribute[] disableIf
                , out HelperBoxAttribute helperBox, out GUIGroupAttribute group
                , out DisplayTextureAttribute displayTexture
                ,out PropertyRangeAttribute propertyRange,out ArrayLabelAttribute arrayLabel
                ,out RangeAttribute defaultRange
                ,out RuntimeDisabledGroupAttribute runtimeDisabledGroup,out EditorDisabledGroupAttribute editorDisabledGroup
                ,out ListDrawerSettingAttribute listDrawerSetting
                ,out BoolanPopupAttribute boolanPopup);            
                Type itemType = null;
                if (member is FieldInfo field)
                    itemType = field.FieldType;
                else if (member is PropertyInfo pInfo)
                    itemType = pInfo.PropertyType;

                var setting = itemType.GetCustomAttributes<CustomPropertySettingAttribute>(true).Where(x => x.GetType().Equals(typeof(CustomPropertySettingAttribute))).FirstOrDefault();
                if (setting != null)
                {
                    property = property.FindPropertyRelative(setting.ItemName);
                    if (property == null)
                        continue;
                    FieldInfo m = itemType.GetRuntimeFields().FirstOrDefault(x => x.Name.Equals(setting.ItemName));                     
                    if (m == null)
                    {
                        PropertyInfo p = itemType.GetRuntimeProperties().FirstOrDefault(x => x.Name.Equals(setting.ItemName));
                        if (p != null)
                        {
                            p.CreateAllSettingAttribute
                           (out _
                           , out color, out enableEnumValueIfAttribute
                           , out disable, out enableIf, out disableIf
                           , out helperBox, out _
                           , out displayTexture,  out _, out var newLabel
                           , out _
                           , out runtimeDisabledGroup, out editorDisabledGroup
                           , out _
                           , out _);

                            if (arrayLabel == null)
                                arrayLabel = newLabel;
                        }
                    }
                    else
                    {                      
                        m.CreateAllSettingAttribute
                            (out _
                            , out color, out enableEnumValueIfAttribute
                            , out disable, out enableIf, out disableIf
                            , out helperBox, out _
                            , out displayTexture,  out _, out var newLabel
                            , out _
                            , out runtimeDisabledGroup, out editorDisabledGroup
                            , out _
                            , out _);

                        if (arrayLabel == null)
                            arrayLabel = newLabel;
                    }

                    if (label == null) label = new LabelAttribute(member.Name);

                    if (property.propertyType == SerializedPropertyType.Generic && arrayLabel == null)
                        arrayLabel = new ArrayLabelAttribute(setting.ArrayLabel);                 
                }


                if (CheckPropertyInGeneric(itemType) && listDrawerSetting != null)
                {
                    var listInfo = new PropertyReorderableListDrawedInfo
                        (target,boolanPopup, displayTexture, listDrawerSetting, arrayLabel, defaultRange, propertyRange, label, serializedObject
                        , property, color, enableEnumValueIfAttribute, disable, enableIf, disableIf, helperBox, runtimeDisabledGroup, editorDisabledGroup);
                    listInfo.ItemType = itemType;
                    CreateInfo(listInfo, group);
                }
                else if (CheckPropertyInBoolan(itemType, property) && boolanPopup != null)
                {
                    var bInfo = new PropertyBoolanDrawedInfo(target, property,boolanPopup, arrayLabel, listDrawerSetting, displayTexture,propertyRange,label, color, enableEnumValueIfAttribute, disable, enableIf, disableIf, helperBox, runtimeDisabledGroup, editorDisabledGroup) ;
                    bInfo.ItemType = itemType;
                    CreateInfo(bInfo, group);
                }
                else if (CheckPropertyInEnum(itemType, property))
                {
                    var eInfo = new PropertyEnumDrawedInfo(target, boolanPopup,arrayLabel, listDrawerSetting, displayTexture, label, propertyRange, property, color, enableEnumValueIfAttribute, disable, enableIf, disableIf, helperBox, runtimeDisabledGroup, editorDisabledGroup);
                    eInfo.ItemType = itemType;
                    CreateInfo(eInfo, group);
                }
                else if (CheckPropertyInTexture(itemType, displayTexture))
                {
                    var tInfo = new PropertyTextureDrawedInfo(target,boolanPopup, arrayLabel, listDrawerSetting, displayTexture, propertyRange, property, label, color, enableEnumValueIfAttribute, disable, enableIf, disableIf, helperBox, runtimeDisabledGroup, editorDisabledGroup);
                    tInfo.ItemType = itemType;
                    CreateInfo(tInfo, group);
                }
                else
                {
                    var info = new PropertyMemberDrawedInfo(target, boolanPopup, arrayLabel, listDrawerSetting, displayTexture, label, propertyRange, property, color, enableEnumValueIfAttribute, disable, enableIf, disableIf, helperBox, runtimeDisabledGroup, editorDisabledGroup);
                    info.ItemType = itemType;
                    CreateInfo(info, group);
                }
            }
        }

        public bool CheckPropertyInGeneric(Type itemType)
        {
            if (itemType == null) return false;        
            if (itemType.IsGenericType || itemType.IsArray) return true;

            return false;
        }

        protected bool OnDisableGroup(PropertyDrawedInfo info)
        {
            if (info.RuntimeDisabledGroup != null && info.EditorDisabledGroup != null)
                return true;

            if (info.RuntimeDisabledGroup != null)
                return IsPlaying;
            else if (info.EditorDisabledGroup != null)
                return !IsPlaying;

            return false;
        }


        public bool CheckPropertyInTexture(Type itemType,DisplayTextureAttribute displayTexture)
        {
            if (itemType == null) return false;

            return (itemType.Equals(typeof(Texture)) || itemType.Equals(typeof(Texture2D)) || itemType.Equals(typeof(Sprite))) && displayTexture != null;
        }

        public bool CheckPropertyInBoolan(Type itemType, SerializedProperty property)
        {
            if (itemType == null || property == null) return false;

            return (itemType.IsSubclassOf(typeof(bool)) || itemType.Equals(typeof(bool)) || property.propertyType == SerializedPropertyType.Boolean);
        }


        public bool CheckPropertyInEnum(Type itemType,SerializedProperty property)
        {
            if (itemType == null || property == null) return false;

            return itemType.IsSubclassOf(typeof(Enum)) || itemType.Equals(typeof(Enum)) || property.propertyType == SerializedPropertyType.Enum;
        }

        private void InitMemberInfos(Object target, Type type)
        {
            this.target = target;
            serializedObject = new SerializedObject(target);
            InitMemberInfos(type);
        }

        private void CreateInfo(PropertyDrawedInfo info, GUIGroupAttribute group)
        {
            if (group == null)
                infoDatas.Enqueue(new InfoData
                {
                    groupName = "null",
                }.Add(info));
            else
            {
                InfoData data = infoDatas.FirstOrDefault(x => x.groupName.Equals(group.GroupName));
                if (data == null)
                {
                    data = new InfoData
                    {
                        groupName = group.GroupName,
                    }.Add(info);
                    infoDatas.Enqueue(data);
                }
                else data.Add(info);
            }
        }

        public override void OnInspectorGUI()
        {
            foreach (var info in infoDatas)
            {
                Rect rect;
                if (info.groupName != "null")
                {
                    bool groupDrawed = false;
                    foreach (var member in info.PropertyDrawedInfos)
                    {
                        if (member.DrawConditionIf(target.GetType(), target))
                        {
                            groupDrawed = true;
                            break;
                        }
                    }
                    if (groupDrawed)
                    {
                        GUIStyle style = new GUIStyle("OL box NoExpand");
                        style.fontSize = 14;
                        style.alignment = TextAnchor.MiddleCenter;
                        style.fontStyle = FontStyle.Bold;
                        GUILayout.Label(info.groupName, style, GUILayout.Width(EditorGUIUtility.currentViewWidth - 25));
                        rect = EditorGUILayout.BeginVertical("Wizard Box");

                        EditorGUILayout.Space();
                    }
                    else rect = EditorGUILayout.BeginVertical();
                }
                else rect = EditorGUILayout.BeginVertical();
                foreach (var member in info.PropertyDrawedInfos)
                {              
                    if (!member.DrawConditionIf(target.GetType(), target))
                        continue;
                    EditorGUI.BeginChangeCheck();
                    EditorGUI.BeginDisabledGroup(OnDisableGroup(member));
                    Property(member);
                    EditorGUI.EndDisabledGroup();
                    if (EditorGUI.EndChangeCheck())
                    {
                        EditorUtility.SetDirty(target);
                        AssetDatabase.SaveAssets();
                        editor?.Repaint();                       
                        serializedObject.ApplyModifiedProperties();
                    }
                }
                EditorGUILayout.EndVertical();

            }
        }
        private void Property(PropertyDrawedInfo member)
        {            
            member.DrawHelperBox();
            member.OnGUI();                   
        }          
    }    

}
#endif