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
	public class YukiBind : MonoBehaviour,ISerializedFieldInfo
	{		
		internal List<SerializeFieldData> _fields = new List<SerializeFieldData>();
        internal bool isInited = false;
        public void AddFieldData(SerializeFieldData data)
        {
            _fields.Add(data);
        }

        public void ClearFieldData()
        {
            _fields.Clear();
        }

        public SerializeFieldData Find(Func<SerializeFieldData, bool> func)
        {
            for (int i = 0; i < _fields.Count; i++)
            {
                if (func(_fields[i]))
                    return _fields[i];
            }
            return null;
        }

        public IEnumerable<SerializeFieldData> GetSerializeFields()
        {
            return _fields;
        }

        public void RemoveFieldData(SerializeFieldData data)
        {
            _fields.Remove(data);
        }
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
        private void OnEnable()
        {
            YukiBind fieldInfo = target as YukiBind;

            if (fieldInfo == null) return;
            if (fieldInfo.isInited) return;

            Init();
            fieldInfo.isInited = true;
        }

        void Init()
        {
            Undo.RecordObject(target, "Init All Bind Field");
            ISerializedFieldInfo fieldInfo = target as ISerializedFieldInfo;

            if (fieldInfo == null) return;

            Component[] components = (target as YukiBind).GetComponents<Component>();

            fieldInfo.ClearFieldData();
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i].GetType() == typeof(YukiBind)) continue;
                var data = new SerializeFieldData((target as Component).gameObject);
                data.fieldName = $"{target.name}{components[i].GetType().Name}";
                data.fieldTypeIndex = i + 1;
                fieldInfo.AddFieldData(data);
            }
        }


        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("该脚本会自动为为该对象所有的组件进行绑定，选定后在其父物体的ViewController/BasePanel中点击生成代码即可", MessageType.Info);
            if (GUILayout.Button("重置字段绑定", GUILayout.Height(30))) Init();
            CodeManager.BindInspector(target as ISerializedFieldInfo, (target as Component));
        }
    }
#endif
}
