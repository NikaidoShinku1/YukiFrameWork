///=====================================================
/// - FileName:      CustomInspector.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/1 20:42:48
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Object = UnityEngine.Object;
using System.Collections.Generic;
using YukiFrameWork.Extension;
using System.Reflection;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork
{
#if UNITY_EDITOR
	[CustomEditor(typeof(MonoBehaviour),true)]
	[CanEditMultipleObjects]
	public class CustomInspectorEditor : Editor
	{
        protected FastList<GenericLayer> layers = new FastList<GenericLayer>();
        private SerializedProperty script;
        protected virtual void OnEnable()
        {
            script = serializedObject.FindProperty("m_Script");

            if (layers.Count == 0)
                InitLayers();
        }

        protected virtual void InitLayers()
        {
            layers.Add(new MemberInfoLayer(target,target.GetType(),this));
            layers.Add(new MethodInfoLayer(serializedObject, target.GetType()));
        }

        protected virtual void OnDisable()
        {
            layers.Clear(); 
        }

        public override void OnInspectorGUI()
        {                 
            serializedObject.Update();
            var rect = EditorGUILayout.BeginVertical();
            EditorGUI.BeginDisabledGroup(true);
            if(script != null)
                EditorGUILayout.PropertyField(script);
            EditorGUI.EndDisabledGroup();

            OnDrawingLayer(rect);
            
            EditorGUILayout.EndVertical();
        }

        public virtual void OnDrawingLayer(Rect rect)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                layers[i].OnInspectorGUI();
                layers[i].OnGUI(rect);
                EditorGUILayout.Space();
            }
        }
      
    }
    [CustomEditor(typeof(ScriptableObject), true)]
    [CanEditMultipleObjects]
    public class CustomScriptableObjectInspectorEditor : CustomInspectorEditor
    {
        
    }
#endif
}
