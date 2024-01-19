///=====================================================
/// - FileName:      LifeTimeScopeEditor.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   生命容器拓展
/// - Creation Time: 2023/12/30 2:58:55
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
namespace YukiFrameWork
{
    [CustomEditor(typeof(LifeTimeScope),true)]
    [CanEditMultipleObjects]
    public class LifeTimeScopeEditor : Editor
    {       
        private readonly List<string> parentTypesName = new List<string>();
      
        public override void OnInspectorGUI()
        {
            LifeTimeScope scope = target as LifeTimeScope;
            if (scope == null) return;
            SelectScopeParent(scope);
            SerializedInjectedGameObjects();
        }
        /// <summary>
        /// 选择自己的父类
        /// </summary>
        /// <param name="scopeType"></param>
        private void SelectScopeParent(LifeTimeScope scope)
        {
            parentTypesName.Clear();
            Type parentType = scope.GetType();

            do 
            {
                if(parentType.Equals(scope.GetType()))
                    parentTypesName.Add("None");
                else
                    parentTypesName.Add(parentType.ToString());             
                parentType = parentType.BaseType;
            }while (parentType != null && !parentType.Equals(typeof(LifeTimeScope)));

            EditorGUILayout.BeginVertical();
            EditorGUILayout.LabelField("Parent:");
            scope.ParentTypeIndex = EditorGUILayout.Popup(scope.ParentTypeIndex,parentTypesName.ToArray());
            EditorGUILayout.EndVertical();

            scope.ParentTypeName = parentTypesName[scope.ParentTypeIndex] == "None" ? typeof(LifeTimeScope).ToString() : parentTypesName[scope.ParentTypeIndex];          
        }

        private void SerializedInjectedGameObjects()
        {
            serializedObject.Update();
            SerializedProperty autoRunProperty = serializedObject.FindProperty("IsAutoInjectObject");
            SerializedProperty property = serializedObject.FindProperty("gameObjects");

            EditorGUILayout.PropertyField(autoRunProperty);
            EditorGUILayout.PropertyField(property);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif