///=====================================================
/// - FileName:      SkillDataBase.cs
/// - NameSpace:     YukiFrameWork.Skill
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/10 18:07:08
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using YukiFrameWork.Extension;
namespace YukiFrameWork.Skill
{
	[CreateAssetMenu(fileName = "SkillDataBase",menuName = "YukiFrameWork/Skill Data Base")]
	public class SkillDataBase : ScriptableObject
	{
        [FoldoutGroup("代码设置"), SerializeField]
        private string fileName;

        [FoldoutGroup("代码设置"), SerializeField, FolderPath]
        private string filePath = "Assets/Scripts/Skill";

        [FoldoutGroup("代码设置"), SerializeField]
        private string nameSpace = "YukiFrameWork.Skill";

#if UNITY_EDITOR
        [Button("生成Skill代码"), GUIColor("green"), FoldoutGroup("代码设置")]
        void CreateCode()
        {
            if (string.IsNullOrEmpty(fileName))
            {
                Debug.LogError("请输入脚本文件名");
                return;
            }

            if (string.IsNullOrEmpty(nameSpace))
            {
                Debug.LogError("请输入命名空间，保持良好的命名空间习惯");
                return;
            }
            CodeCore codeCore = new CodeCore();

            codeCore
                .Using("System")
                .Using("UnityEngine")
                .Descripton(fileName, nameSpace, "自动化代码生成的Skill派生类");

            codeCore.CodeSetting(nameSpace, fileName, nameof(SkillData), null).Create(fileName, filePath);
        }

        [Button("生成标识代码"), GUIColor("green"), PropertySpace(15), FoldoutGroup("代码设置")]
        [InfoBox("标识代码则为所有配置的标识以及SkillData的快捷获取，类名为SkillInfos,仅在配置完毕且没有标识为空的时候使用")]
        void CreateDefaultCode()
        {
            if (string.IsNullOrEmpty(nameSpace))
            {
                Debug.LogError("请输入命名空间，保持良好的命名空间习惯");
                return;
            }

            if (SkillDataConfigs.Count == 0)
            {
                Debug.LogError("当前不存在配置");
                return;
            }

            CodeCore codeCore = new CodeCore();
            codeCore
               .Using("System")
               .Using("UnityEngine")
               .Descripton("SkillInfos", nameSpace, "自动化代码生成的SkillData标识类");
            CodeWriter writer = new CodeWriter();
            foreach (var item in SkillDataConfigs)
            {
                if (item == null) continue;
                if (string.IsNullOrEmpty(item.GetSkillKey)) continue;

                writer.CustomCode($"public static string {item.GetSkillKey}_Key = \"{item.GetSkillKey}\";");
                writer.CustomCode($"public static ISkillData {item.GetSkillKey} => SkillKit.GetSkillDataByKey(\"{item.GetSkillKey}\");");
            }
            codeCore.CodeSetting(nameSpace, "SkillInfos", string.Empty, writer,false,true).Create("SkillInfos", filePath);
        }
#endif

        [SerializeField, LabelText("SkillData配置")]
        [InfoBox("添加SkillData配置需要选择SkillData的类型，当项目中没有任何SkillData类文件时，无法新建SkillData配置，且删除对应文件时，会自动消除对应的类", InfoMessageType.Warning)]
        [VerticalGroup("配置")]
        [ListDrawerSettings(HideAddButton = true, CustomRemoveIndexFunction = nameof(DeleteSkillData), DraggableItems = false)]
        public List<SkillData> SkillDataConfigs = new List<SkillData>();
#if UNITY_EDITOR
        bool reimportTable => SkillDataConfigs.Count > 0;
        [ShowIf(nameof(reimportTable)), FoldoutGroup("JsonSkillData"), SerializeField]
        private string jsonName = "SkillDatas";
        [ShowIf(nameof(reimportTable)), FoldoutGroup("JsonSkillData"), SerializeField]
        private string jsonPath = "Assets/SkillData";
        [ShowIf(nameof(reimportTable)), Button("将现有配置导出Json"), FoldoutGroup("JsonSkillData")]
        void CreateTable()
        {
            if (SkillDataConfigs.Count == 0) return;
            foreach (var item in SkillDataConfigs)
            {
                if (item.GetIcon != null)
                {
                    item.Sprite = AssetDatabase.GetAssetPath(item.GetIcon);
                }

                item.SKillType = item.GetType().ToString();
            }
            SerializationTool.SerializedObject(SkillDataConfigs,settings: new Newtonsoft.Json.JsonSerializerSettings() 
            {
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore
            }).CreateFileStream(jsonPath, jsonName, ".json");
        }

        [Button("打开编辑器配置窗口", ButtonHeight = 30)]
        [PropertySpace(20)]
        void OpenWindow()
        {
            SkillDesignerWindow.OpenWindow();
        }
#endif     

        internal void CreateSkillData(Type SkillDataType)
        {
            SkillData SkillData = SkillData.CreateInstance(SkillDataType.Name, SkillDataType);
       
            SkillDataConfigs.Add(SkillData);
          
#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(SkillData, this);
            this.Save();
#endif
        }

        public void DeleteSkillData(int index)
        {
            var SkillData = SkillDataConfigs[index];

            SkillDataConfigs.RemoveAt(index);

#if UNITY_EDITOR
            AssetDatabase.RemoveObjectFromAsset(SkillData);
            //OnValidate();
            this.Save();
#endif
        }
    }
}

