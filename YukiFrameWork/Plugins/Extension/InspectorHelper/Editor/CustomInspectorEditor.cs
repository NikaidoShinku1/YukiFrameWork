///=====================================================
/// - FileName:      CustomInspector.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/1 20:42:48
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditorInternal;
using UnityEditor;
#endif
namespace YukiFrameWork
{
#if UNITY_EDITOR	
    public class InfoData
    {
        public MemberInfo member;
        public SerializedProperty property;

        public bool active;
    }
    [CustomEditor(typeof(MonoBehaviour), true)]
    [CanEditMultipleObjects]
    public class CustomInspectorEditor : Editor
    {
        private Dictionary<string, List<InfoData>> infoDataPairs = new Dictionary<string, List<InfoData>>();
        private Dictionary<string, ReorderableList> listPairs = new Dictionary<string, ReorderableList>();
        private List<string> enumDisplayNames = new List<string>();
        private SerializedProperty script;
        protected virtual void OnEnable()
        {
            infoDataPairs.Clear();
            listPairs.Clear();
            enumDisplayNames.Clear();
            infoDataPairs.Add("Default", new List<InfoData>());           
            try
            {
                IEnumerable<FieldInfo> memberInfos = target.GetType().GetRuntimeFields()
                .Where(x => this.serializedObject.FindProperty(x.Name) != null && x.GetCustomAttribute<HideInInspector>() == null);

                foreach (var field in memberInfos)
                {
                    var group = field.GetCustomAttribute<GUIGroupAttribute>(true);

                    SerializedProperty property = serializedObject.FindProperty(field.Name);

                    CustomPropertySettingAttribute settingAttribute = field.FieldType.GetCustomAttribute<CustomPropertySettingAttribute>(true);
                    if (settingAttribute != null)
                    {
                        property = property.FindPropertyRelative(settingAttribute.ItemName);
                    }

                    group ??= new GUIGroupAttribute("Default");
                    SetGroup(group, field, property, field.FieldType);
                }
                script = serializedObject.FindProperty("m_Script");         
            }
            catch { }
            
        }

        public override void OnInspectorGUI()
        {           
            serializedObject.Update();       

            if (script != null)
            {
                GUI.enabled = false;
                EditorGUILayout.PropertyField(script);
                GUI.enabled = true;
            }

            foreach (var key in infoDataPairs.Keys)
            {
                var pair = infoDataPairs[key];

                bool changeTip = false;
                foreach (var item in pair)
                {
                    if (item.active)
                    {
                        changeTip = true;
                    }
                }
                if (!key.Equals("Default") && changeTip)
                {
                    GUIStyle style = new GUIStyle("OL box NoExpand");
                    style.fontSize = 14;
                    style.alignment = TextAnchor.MiddleCenter;
                    style.fontStyle = FontStyle.Bold;
                    GUILayout.Label(key, style, GUILayout.Width(EditorGUIUtility.currentViewWidth - 25));
                    EditorGUILayout.BeginVertical("Wizard Box");
                    EditorGUILayout.Space();
                }
                else EditorGUILayout.BeginVertical();          
                foreach (var item in pair)
                {
                    SerializedField(item);
                }

                EditorGUILayout.EndVertical();
            }
            MethodInfo[] methodInfos = target.GetType().GetRuntimeMethods().ToArray();

            if (methodInfos.Length > 0)
            {
                EditorGUILayout.Space(15);
                foreach (var methodInfo in methodInfos)
                {
                    var button = methodInfo.GetCustomAttribute<MethodButtonAttribute>(true);

                    if (button != null)
                    {
                        SerializeMethod(methodInfo, button);
                    }
                }
            }
            this.serializedObject.ApplyModifiedProperties();
        }

        private void SerializedField(InfoData item)
        {
            item.member.CreateAllSettingAttribute
             (out LabelAttribute label
             , out GUIColorAttribute color, out EnableEnumValueIfAttribute[] enableEnumValueIfAttribute
             , out DisableEnumValueIfAttribute[] disable, out EnableIfAttribute[] enableIf, out DisableIfAttribute[] disableIf
             , out HelperBoxAttribute helperBox, out _
             , out DisplayTextureAttribute displayTexture
             , out PropertyRangeAttribute propertyRange
             , out RangeAttribute defaultRange
             , out RuntimeDisabledGroupAttribute runtimeDisabledGroup, out EditorDisabledGroupAttribute editorDisabledGroup
             , out ListDrawerSettingAttribute listDrawerSetting
             , out BoolanPopupAttribute boolanPopup, out DisableGroupIfAttribute disableGroupIf
             , out DisableGroupEnumValueIfAttribute disableGroupEnumValueIf
             , out SpaceAttribute spaceAttribute);

            GUI.color = color != null ? color.Color : Color.white;          
            if (ConditionUtility.DrawConditionIf(enableEnumValueIfAttribute, enableIf, disable, disableIf, target.GetType(), target))
            {
                item.active = true;
                EditorGUI.BeginDisabledGroup(ConditionUtility.DisableGroupLifeCycle(runtimeDisabledGroup, editorDisabledGroup) || ConditionUtility.DisableGroupInValue(target.GetType(), target, disableGroupEnumValueIf, disableGroupIf));
                if (spaceAttribute != null)              
                    EditorGUILayout.Space(spaceAttribute.height);

                if (helperBox != null)
                    DrawingUtility.PropertyFieldInHelperBox(helperBox);

                if ((defaultRange != null || propertyRange != null) && (item.property.propertyType == SerializedPropertyType.Integer || item.property.propertyType == SerializedPropertyType.Float))
                {
                    DrawingUtility.PropertyFieldInSlider(item, label, defaultRange, propertyRange);
                }
                else if (PropertyUtility.CheckPropertyInBoolan(target.GetType(), item.property) && boolanPopup != null)
                {                         
                    HelperUtility.DrawHelperWarning(item.property,displayTexture != null,false, defaultRange != null || propertyRange != null);                   
                    DrawingUtility.PropertyFieldInBoolValue(item, label, boolanPopup);
                }
                else if (PropertyUtility.CheckPropertyInEnum(target.GetType(), item.property))
                {
                    HelperUtility.DrawHelperWarning(item.property, displayTexture != null, boolanPopup != null, defaultRange != null || propertyRange != null);
                    DrawingUtility.PropertyFieldInEnum(item, label,enumDisplayNames);
                }
                else if (PropertyUtility.CheckPropertyInTexture((item.member as FieldInfo).FieldType, displayTexture))
                {
                    HelperUtility.DrawHelperWarning(item.property, false, boolanPopup != null, defaultRange != null || propertyRange != null);
                    DrawingUtility.PropertyFieldInTexture(displayTexture,item,label);
                }
                else
                {
                    HelperUtility.DrawHelperWarning(item.property, displayTexture != null, boolanPopup != null, (defaultRange != null || propertyRange != null) && listDrawerSetting == null);
                    DrawingUtility.PropertyField(item, label,listPairs);
                }
                EditorGUI.EndDisabledGroup();
            }
            else item.active = false;
            GUI.color = Color.white;
        }

        private void SerializeMethod(MethodInfo methodInfo,MethodButtonAttribute Method)
        {
            methodInfo.CreateAllSettingAttribute
               (out _
               , out GUIColorAttribute color, out EnableEnumValueIfAttribute[] enableEnumValueIfAttribute
               , out DisableEnumValueIfAttribute[] disable, out EnableIfAttribute[] enableIf, out DisableIfAttribute[] disableIf
               , out HelperBoxAttribute helperBox
               , out _, out _
               , out _, out _
               , out RuntimeDisabledGroupAttribute runtimeDisabledGroup, out EditorDisabledGroupAttribute editorDisabledGroup
               , out _, out _, out var disableGroupIf, out var disableGroupEnumValueIf, out _);      
            if (ConditionUtility.DrawConditionIf(enableEnumValueIfAttribute, enableIf, disable, disableIf, target.GetType(), target))
            {
                GUI.color = color != null ? color.Color : Color.white;
                EditorGUI.BeginDisabledGroup(ConditionUtility.DisableGroupLifeCycle(runtimeDisabledGroup, editorDisabledGroup) || ConditionUtility.DisableGroupInValue(target.GetType(), target, disableGroupEnumValueIf, disableGroupIf));
                if (helperBox != null)
                    DrawingUtility.PropertyFieldInHelperBox(helperBox);

                bool executed = Method.Width == -1
            ? GUILayout.Button(string.IsNullOrEmpty(Method.Label) ? methodInfo.Name : Method.Label, GUILayout.Height(Method.Height))
            : GUILayout.Button(string.IsNullOrEmpty(Method.Label) ? methodInfo.Name : Method.Label, GUILayout.Height(Method.Height), GUILayout.Width(Method.Width));
                GUI.color = Color.white;
                EditorGUI.EndDisabledGroup();
                if (executed)
                {
                    object[] args = null;
                    if (methodInfo.GetParameters().Length != Method.Args?.Length)
                        args = new object[methodInfo.GetParameters().Length];
                    else args = Method.Args;
                    methodInfo.Invoke(target, args);
                    AssetDatabase.Refresh();
                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();

                    serializedObject.ApplyModifiedProperties();
                }

                EditorGUI.EndDisabledGroup();
                GUI.color = Color.white;
            }
        }

        private void SetGroup(GUIGroupAttribute group, MemberInfo info,SerializedProperty property,Type type)
        {
           
            if (infoDataPairs.ContainsKey(group.GroupName))
            { }
            else
                infoDataPairs.Add(group.GroupName, new List<InfoData>());

            if (infoDataPairs[group.GroupName].Find(x => x.member == info || x.property == property) != null)
            {
                Debug.Log("存在这个变量");
                return;
            }
            var data = new InfoData()
            {
                member = info,
                property = property
            };
            var label = info.GetCustomAttribute<LabelAttribute>(true);
            var listDrawerSetting = info.GetCustomAttribute<ListDrawerSettingAttribute>(true);
            if (PropertyUtility.CheckPropertyInGeneric(type) && listDrawerSetting != null)
            {
                var list = new UnityEditorInternal.ReorderableList(serializedObject, property, true, true, true, true);
                DrawingUtility.SetReoderableList(data, list, label,listDrawerSetting,target,target.GetType());
                listPairs[data.member.Name] = list;
            }
            infoDataPairs[group.GroupName].Add(data);
        }
    }
    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public class CustomScriptableObjectInspectorEditor : CustomInspectorEditor
    {
        
    }
#endif
}
