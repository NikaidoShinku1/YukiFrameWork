///=====================================================
/// - FileName:      LocalizationConfig.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   本地化配置
/// - Creation Time: 2024/4/8 13:44:56
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using YukiFrameWork.Extension;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.U2D;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor.Callbacks;
using UnityEditor;
#endif
namespace YukiFrameWork
{
    public abstract class LocalizationConfigBase : ScriptableObject
    {
        protected const string groupName = "本地设置:";                          
       
        public abstract ILocalizationData[] LocalizationDatas { get; }
    }
  
    public abstract class LocalizationConfigBase<LocalizationData> : LocalizationConfigBase, IExcelSyncScriptableObject where LocalizationData : ILocalizationData,new()
    {      
        [LabelText("打开配置表导入:"),BoxGroup(groupName)]
        [JsonIgnore]
        public bool openLoadMode;      
       
        //[DictionaryDrawerSettings(KeyLabel = "标识", ValueLabel = "配置信息"), BoxGroup(groupName)]       
        [LabelText("本地数据配置:"),BoxGroup(groupName)]
        [JsonProperty]      
        [TableList(NumberOfItemsPerPage = 5,DrawScrollView = true),Searchable]       
        [SerializeField]        
        public LocalizationData[] config;       

        public IList Array => config.ToArray();
        public Type ImportType => typeof(LocalizationData);

        public bool ScriptableObjectConfigImport => false;

        public override ILocalizationData[] LocalizationDatas => config.Select(x => x as ILocalizationData).ToArray();
        [Sirenix.OdinInspector.FilePath(Extensions = "xlsx"),PropertySpace(50),LabelText("Excel路径")]
        public string excelPath;
#if UNITY_EDITOR
        [Button("导出Excel"),HorizontalGroup("Excel")]
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
        public void Create(int maxLength)
        {
            config = new LocalizationData[maxLength];
        }

        public void Import(int index, object userData)
        {
            config[index] = (LocalizationData)userData;
            
        }

        public void Completed()
        {

        }

    } 
    public class LocalizationConfig : LocalizationConfigBase<LocalizationData>
    {

    }
}
