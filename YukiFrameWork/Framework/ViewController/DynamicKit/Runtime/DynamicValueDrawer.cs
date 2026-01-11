///=====================================================
/// - FileName:      DynamicValueDrawer.cs
/// - NameSpace:     YukiFrameWork.Dynamic
/// - Description:   高级定制脚本生成
/// - Creation Time: 1/8/2026 2:59:09 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
using YukiFrameWork.QF;


#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
#endif
namespace YukiFrameWork
{
    public class DynamicValueDrawer : OdinAttributeDrawer<DynamicValueAttribute>
    { 
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var attribute = base.Attribute;

            if (attribute == null)
                throw new Exception("特性异常");
            GUI.color = ExpertCodeConfig.IdentifierColor;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("[DynamicValue]",GUILayout.Width(100));
            GUI.color = Color.cyan;
            if (!attribute.ChildObjName.IsNullOrEmpty())
            {
                
                //GUILayout.FlexibleSpace();
                GUILayout.Label("Bind SceneObj:" + attribute.ChildObjName);
            }
            if (Application.isPlaying)
            {
                bool v = Property.ValueEntry.WeakSmartValue != null;
                if (v)
                {
                    GUI.color = Color.green;
                    EditorGUILayout.LabelField("Value Completed");
                }
                else
                {
                    GUI.color = Color.red;
                    EditorGUILayout.LabelField("Not Value");
                }
               
            }
            EditorGUILayout.EndHorizontal();
            //EditorGUILayout.HelpBox("已标记DynamicValue特性,该值由DynamicValue运行时自动赋值,如类型无自动模式，则手动调用DynamicValue.Inject即可完成赋值",MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.white;
            GUILayout.Label(label);     
            //GUILayout.Label($"[DynamicValue]", GUILayout.Width(180));
            
            EditorGUILayout.EndHorizontal();
            GUI.color = attribute.OnlyMonoEnable ? Color.green : Color.yellow;


            //EditorGUILayout.BeginVertical();
            GUILayout.Label(attribute.OnlyMonoEnable ? "Active" : "IncludeInActive",GUILayout.Width(100));           
            //EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            
            GUI.color = Color.white;
        }

    }

    public class DynamicValueFromSceneDrawer : OdinAttributeDrawer<DynamicValueFromSceneAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {                      
            var attribute = base.Attribute;
           
            if (attribute == null)
                throw new Exception("特性异常");
            //this.CallNextDrawer(label);
            GUI.color = ExpertCodeConfig.IdentifierColor; 
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("[DynamicValueFromScene]",GUILayout.Width(160));
            if (!attribute.SceneObjLabel.IsNullOrEmpty())
            {
                GUI.color = Color.cyan;
                //GUILayout.FlexibleSpace();
                GUILayout.Label("Bind SceneObj:" + attribute.SceneObjLabel);
            }
            if (Application.isPlaying)
            {
                bool v = Property.ValueEntry.WeakSmartValue != null;
                if (v)
                {
                    GUI.color = Color.green;
                    EditorGUILayout.LabelField("Value Completed");
                }
                else
                {
                    GUI.color = Color.red;
                    EditorGUILayout.LabelField("Not Value");
                }

            }
            EditorGUILayout.EndHorizontal();
            //EditorGUILayout.HelpBox("已标记DynamicValue特性,该值由DynamicValue运行时自动赋值,如类型无自动模式，则手动调用DynamicValue.Inject即可完成赋值",MessageType.Info);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.white;
            GUILayout.Label(label);
           
            EditorGUILayout.EndHorizontal();
            GUI.color = attribute.OnlyMonoEnable ? Color.green : Color.yellow;

            //EditorGUILayout.BeginVertical();
            GUILayout.Label(attribute.OnlyMonoEnable ? "Active" : "IncludeInActive", GUILayout.Width(100));
            GUILayout.Label("FindMode:" + attribute.DynamicValueFromSceneMode,GUILayout.Width(100));
            //EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            GUI.color = Color.white;
        }

    }

    public class DynamicRegulationDrawer : OdinAttributeDrawer<DynamicRegulationAttribute>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            var attribute = base.Attribute;

            if (attribute == null)
                throw new Exception("特性异常");
            //this.CallNextDrawer(label);
            GUI.color = ExpertCodeConfig.IdentifierColor;
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("[DynamicRegulation]", GUILayout.Width(140));
            if (Application.isPlaying)
            {
                bool v = Property.ValueEntry.WeakSmartValue != null;
                if (v)
                {
                    GUI.color = Color.green;
                    EditorGUILayout.LabelField("Value Completed");
                }
                else
                {
                    GUI.color = Color.red;
                    EditorGUILayout.LabelField("Not Value");
                }

            }
            GUI.color = Color.cyan;
            //GUILayout.FlexibleSpace();
            GUILayout.Label("Bind RegulationType:" + attribute.RegulationType);
          
            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.white;
            if(label.IsNotNull())
            GUILayout.Label(label);

            EditorGUILayout.EndHorizontal();          
            EditorGUILayout.EndHorizontal();

            GUI.color = Color.white;
        }
    }
}
#endif