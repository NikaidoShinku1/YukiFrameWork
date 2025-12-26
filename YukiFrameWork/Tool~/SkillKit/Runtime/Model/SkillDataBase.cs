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
using System.Collections;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif
using YukiFrameWork.Extension;
namespace YukiFrameWork.Skill
{
	[CreateAssetMenu(fileName = "SkillDataBase",menuName = "YukiFrameWork/Skill Data Base")]
    [HideMonoScript]
	public class SkillDataBase : ScriptableObject,IExcelSyncScriptableObject
	{
        [FoldoutGroup("代码设置"), SerializeField]
        private string fileName;

        [FoldoutGroup("代码设置"), SerializeField, FolderPath]
        private string filePath = "Assets/Scripts/Skill";

        [FoldoutGroup("代码设置"), SerializeField]
        private string nameSpace = "YukiFrameWork.Skill";

        public Action onValidate;     

#if UNITY_EDITOR
        [HideInInspector, SerializeField]
        public int selectIndex;
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
                .Using("YukiFrameWork.Skill")
                .Descripton(fileName, nameSpace, "自动化代码生成的Skill派生类");

            codeCore.CodeSetting(nameSpace, fileName, nameof(SkillData), null).Create(fileName, filePath);
        }        

        [Button("生成标识代码"), GUIColor("green"), PropertySpace(15), FoldoutGroup("代码设置")]
        [InfoBox("标识代码则为所有配置的标识以及SkillData的快捷获取，仅在配置完毕且没有标识为空的时候使用")]
        void CreateDefaultCode(string InfoScriptNames = "SkillInfos")
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
               .Using("YukiFrameWork.Skill")
               .Descripton(InfoScriptNames, nameSpace, "自动化代码生成的SkillData标识类");
            CodeWriter writer = new CodeWriter();
            foreach (var item in SkillDataConfigs)
            {
                if (item == null) continue;
                if (string.IsNullOrEmpty(item.SkillKey)) continue;

                writer.CustomCode($"public static string {item.SkillKey}_Key = \"{item.SkillKey}\";");
                writer.CustomCode($"public static ISkillData {item.SkillKey} => SkillKit.GetSkillDataByKey(\"{item.SkillKey}\");");
            }
            codeCore.CodeSetting(nameSpace, InfoScriptNames, string.Empty, writer,false,true).Create(InfoScriptNames, filePath);
        }
#endif

        [SerializeField, LabelText("SkillData配置")]
        [InfoBox("添加SkillData配置需要选择SkillData的类型，当项目中没有任何SkillData类文件时，无法新建SkillData配置，且删除对应文件时，会自动消除对应的类", InfoMessageType.Warning)]
        [VerticalGroup("配置")]
        [ListDrawerSettings(HideAddButton = true,DraggableItems = false)]
        public List<SkillData> SkillDataConfigs = new List<SkillData>();


#if UNITY_EDITOR

        [UnityEditor.Callbacks.OnOpenAsset(0)]
        private static bool OnOpenAsset(int insId, int line)
        {
            SkillDataBase obj = EditorUtility.InstanceIDToObject(insId) as SkillDataBase ;
            if (obj != null)
            {
                SkillDataBaseEditorWindow.ShowWindow();
            }
            return obj != null;
        }
        public static IEnumerable allSkillKeys => YukiAssetDataBase.FindAssets<SkillDataBase>().SelectMany(x => x.SkillDataConfigs).Select(x => new ValueDropdownItem() { Text = x.SkillKey, Value = x.SkillKey });
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
            DeleteSkillData(SkillData);
        }

        public void DeleteSkillData(SkillData data)
        {        
            SkillDataConfigs.Remove(data);

#if UNITY_EDITOR
            if(data)
            AssetDatabase.RemoveObjectFromAsset(data);
            this.Save();
#endif
        }

        public void Create(int maxLength)
        {
            while (SkillDataConfigs.Count > 0)
                DeleteSkillData(SkillDataConfigs.Count - 1);
        }

        public void Import(int index, object userData)
        {
            var skill = userData as SkillData;

            SkillDataConfigs.Add(skill);          
#if UNITY_EDITOR
            AssetDatabase.AddObjectToAsset(skill, this);
            this.Save();
#endif

        }

        public void Completed()
        {
#if UNITY_EDITOR
            if (SkillDataBaseEditorWindow.Instance)
                SkillDataBaseEditorWindow.Instance.ForceMenuTreeRebuild();
#endif
        }

        public IList Array => SkillDataConfigs;

        public Type ImportType => typeof(SkillData);

        public bool ScriptableObjectConfigImport => false;

#if UNITY_EDITOR
        [Sirenix.OdinInspector.FilePath(Extensions = "xlsx"), PropertySpace(50), LabelText("Excel路径")]
        public string excelPath;
        [Button("导出Excel"), HorizontalGroup("Excel")]
        void CreateExcel()
        {
            if (excelPath.IsNullOrEmpty() || !System.IO.File.Exists(excelPath))
                throw new NullReferenceException("路径为空或不存在!");
            if (SerializationTool.ScriptableObjectToExcel(this, excelPath, out string error))
                Debug.Log("导出成功");
            else throw new Exception(error);
        }
        [Button("导入Excel"), HorizontalGroup("Excel")]
        void ImportExcel()
        {
            if (excelPath.IsNullOrEmpty() || !System.IO.File.Exists(excelPath))
                throw new NullReferenceException("路径为空或不存在!");
            if (SerializationTool.ExcelToScriptableObject(excelPath, 3, this))
            {
                Debug.Log("导入成功");
            }
        }
#endif
    }
}

