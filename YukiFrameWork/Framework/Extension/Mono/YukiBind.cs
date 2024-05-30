///=====================================================
/// - FileName:      YukiBind.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/25 18:41:42
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEditor.Experimental.GraphView;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork
{
	public class YukiBind : MonoBehaviour
	{
        [SerializeField]
		public SerializeFieldData _fields;
        internal bool isInited = false;
        [SerializeField]
        public string description;
    }

#if UNITY_EDITOR 
    [CustomEditor(typeof(YukiBind))]
	public class YukiBindEditor : Editor
	{
        [MenuItem("YukiFrameWork/创建YukiBind %W")]
        static void AddBind()
        {
            if (Selection.objects == null) return;
            foreach (var item in Selection.objects)
            {
                if (item is GameObject gameObject)
                {
                    gameObject.GetOrAddComponent<YukiBind>();
                }         
            }
           
        }
        private void Awake()
        {
            YukiBind fieldInfo = target as YukiBind;

            if (fieldInfo == null) return;
            if (fieldInfo.isInited) return;

            Init(fieldInfo);
            fieldInfo.isInited = true;
        }
        private void OnEnable()
        {
            YukiBind fieldInfo = target as YukiBind;

            if (fieldInfo == null) return;
            if (fieldInfo.isInited) return;

            Init(fieldInfo);
            fieldInfo.isInited = true;
        }

        void Init(YukiBind fieldInfo)
        {
            fieldInfo._fields = new SerializeFieldData(fieldInfo.gameObject);         
        }


        public override void OnInspectorGUI()
        {
            YukiBind fieldInfo = target as YukiBind;
            if (fieldInfo == null) return;
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.HelpBox("设置自定义绑定组件", MessageType.Info);
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("字段级别",GUILayout.Width(60));
            fieldInfo._fields.fieldLevelIndex = EditorGUILayout.Popup(fieldInfo._fields.fieldLevelIndex, fieldInfo._fields.fieldLevel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();           
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("字段类型",GUILayout.Width(60));
            fieldInfo._fields.fieldTypeIndex = EditorGUILayout.Popup(fieldInfo._fields.fieldTypeIndex, fieldInfo._fields.Components.ToArray());
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("字段名称",GUILayout.Width(60));
            fieldInfo._fields.fieldName = EditorGUILayout.TextField(fieldInfo._fields.fieldName);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("注释",GUILayout.Width(30));
            fieldInfo.description = EditorGUILayout.TextArea(fieldInfo.description,GUILayout.Height(60));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.ObjectField("本体:",fieldInfo._fields.target,typeof(UnityEngine.Object),true);
            if (EditorGUI.EndChangeCheck())
            {
                fieldInfo.Save();
            }

        }
    }
#endif
}
