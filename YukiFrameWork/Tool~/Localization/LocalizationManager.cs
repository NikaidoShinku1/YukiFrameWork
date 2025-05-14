///=====================================================
/// - FileName:      LocalizationManager.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/19 18:42:36
/// -  (C) Copyright 2008 - 2025
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
namespace YukiFrameWork
{
    [CreateAssetMenu(fileName ="LocalizationManager",menuName = "YukiFrameWork/本地化配置管理器LocalizationManager")]
    public class LocalizationManager : ScriptableObject
    {      
        [LabelText("多语言本地化配置添加"),ReadOnly]
        public YDictionary<Language, LocalizationConfig> localizationConfig_language_dict;

#if UNITY_EDITOR

        public static LocalizationManager[] LocalizationManagers
        {
            get
            {
                return YukiAssetDataBase.FindAssets<LocalizationManager>();
            }
        }

        [UnityEditor.Callbacks.OnOpenAsset(0)]
        private static bool OnOpenAsset(int insId, int line)
        {
           LocalizationManager obj = EditorUtility.InstanceIDToObject(insId) as LocalizationManager;
            if (obj != null)
            {
                LocalizationManagerWindow.ShowWindow();
            }
            return obj != null;            
        }

        [Sirenix.OdinInspector.FilePath(Extensions = "xlsx"), PropertySpace(50), LabelText("Excel路径")]
        public string excelPath;
        [Button("导出Excel"), HorizontalGroup("Excel")]
        void CreateExcel()
        {
            if (excelPath.IsNullOrEmpty() || !System.IO.File.Exists(excelPath))
                throw new NullReferenceException("路径为空或不存在!");

            LocalizationExcelConvertTool.ExportExcel(this);

        }
        [Button("导入Excel"), HorizontalGroup("Excel")]
        void ImportExcel()
        {
            if (excelPath.IsNullOrEmpty() || !System.IO.File.Exists(excelPath))
                throw new NullReferenceException("路径为空或不存在!");

            LocalizationExcelConvertTool.ImportExcel(this);
        }
#endif        
        private void OnValidate()
        {   
            onValidate?.Invoke();
        }

        public LocalizationConfig GetLocalizationConfig(Language language)
        {
            if (localizationConfig_language_dict.TryGetValue(language, out var config))
                return config;
            return null;
        }

        public Action onValidate;
    }
}
