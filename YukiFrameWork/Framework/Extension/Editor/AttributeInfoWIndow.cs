///=====================================================
/// - FileName:      AttributeInfoWIndow.cs
/// - NameSpace:     YukiFrameWork.Tower
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/24 17:09:27
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

#if UNITY_EDITOR
using UnityEditor;
namespace YukiFrameWork
{
    [Serializable]
	public class AttributeInfoWIndow
	{       
        [LabelText("特性信息"), TableList, ReadOnly,ShowInInspector]
        [InfoBox("特性同时只能挂一个，多个组件并用会出问题")]
        public ValueDropdownList<string> infos
            = new ValueDropdownList<string>()
            {
                { "VGetComponent","形同GetComponent" },
                { "VGetOrAddComponent","形同GetComponent,如果找不到则添加一个新组件"},
                { "VAddComponent","形同AddComponent" },
                { "VGetComponentInChildren","形同GetComponentInChildren" },
                { "VFindObjectOfType","形同FindObjectOfType" },
                { "VFindChildComponentByName","根据GameObject名称获取组件/必须是子物体或者自身" },
                { "VFindChildComponentByPath","形同transform.Find"}
            };

        [OnInspectorGUI]
        void OnInspectorGUI()
        {
            EditorGUILayout.Space(20);

            EditorGUILayout.HelpBox("请注意，使用该特性必须让脚本继承YMonoBehaviour！" +
                "\n默认已经支持ViewController、SingletonMono、UI模块的BasePanel",MessageType.Warning);
        }
	}
}
#endif