///=====================================================
/// - FileName:      LocalizationExcelConvertTool.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/5/14 15:58:04
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;

#if UNITY_EDITOR
using UnityEngine;
using System;
using OfficeOpenXml;
using System.Collections.Generic;

using System.IO;
using YukiFrameWork.Extension;
using Newtonsoft.Json.Linq;



using UnityEditor;
namespace YukiFrameWork
{
    public class LocalizationExcelConvertTool 
    {

        public static void ExportExcel(LocalizationManager localizationManager)
        {

            if (string.IsNullOrEmpty(localizationManager.excelPath))
            {
                // 选择
                string excelPath = EditorUtility.OpenFolderPanel("请选择Excel存放目录", string.Format("{0}/../", Application.dataPath), string.Empty);
                if (string.IsNullOrEmpty(excelPath))
                    return;

                string fileName = localizationManager.name;               

                localizationManager.excelPath = string.Format("{0}/{1}.xlsx", excelPath, fileName);
                EditorUtility.SetDirty(localizationManager);
            }

            using ExcelPackage excel = new ExcelPackage(new FileInfo(localizationManager.excelPath));

            List<string> sheetNames = new List<string>();

            foreach (var item in excel.Workbook.Worksheets)
            {
                sheetNames.Add(item.Name);
            }

            // 清空excel
            foreach (var item in sheetNames)
            {
                excel.Workbook.Worksheets.Delete(item);
            }

            foreach (var item in localizationManager.localizationConfig_language_dict.Keys)
            {
                string language = item.ToString();              
                ExcelWorksheet sheet = excel.Workbook.Worksheets.Add(language);
                var config = localizationManager.localizationConfig_language_dict[item];               
                //foreach(var item in )
                sheet.SetValue(1, 1, "Key");
                sheet.SetValue(2, 1, "string");
                sheet.SetValue(3, 1, "唯一标识");

                sheet.SetValue(1, 2, "Context");
                sheet.SetValue(2, 2, "string");
                sheet.SetValue(3, 2, "文本数据");

                sheet.SetValue(1, 3, "LocalizationImageLoadType");
                sheet.SetValue(2, 3, "enum");
                sheet.SetValue(3, 3, "图片的加载类型");

                sheet.SetValue(1, 4, "LocalImage");
                sheet.SetValue(2, 4, "string");
                sheet.SetValue(3, 4, "本地图片的guid");

                sheet.SetValue(1, 5, "ImageLoader");
                sheet.SetValue(2, 5, "string");
                sheet.SetValue(3, 5, "需要通过ImageLoader加载图片的数据配置");

                sheet.SetValue(1, 6, "NetWorkImageLoader");
                sheet.SetValue(2, 6, "string");
                sheet.SetValue(3, 6, "通过ImageLoader Network加载图片的Url");

                int index = 3;

                foreach (var languageInfo in config.localizations)
                {
                    index++;

                    string info = $"ces";
                    sheet.SetValue(index, 1, languageInfo.Key);
                    sheet.SetValue(index, 2, languageInfo.Context);
                    sheet.SetValue(index, 3, languageInfo.Image.LocalizationImageLoadType);
                    sheet.SetValue(index, 4, YukiAssetDataBase.InstanceToGUID(languageInfo.Image.Icon));
                    sheet.SetValue(index, 5, $"{{\"ProjectName\":\"{languageInfo.Image.ProjectName}\",\"AssetName\":\"{languageInfo.Image.AssetName}\"}}");              
                    sheet.SetValue(index, 6, languageInfo.Notes);
                }

            }

            try
            {
                excel.Save();
                EditorWindow.focusedWindow.ShowNotification(new GUIContent("导出成功!"));
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.ToString());
                EditorWindow.focusedWindow.ShowNotification(new GUIContent("导出失败,请检查excel文件是否被其他应用使用中!"));
            }

        }


        public static void ImportExcel(LocalizationManager localizationManager)
        {

            if (string.IsNullOrEmpty(localizationManager.excelPath))
            {
                string excelPath = EditorUtility.OpenFilePanel("请选择Excel文件", string.Format("{0}/../", Application.dataPath), "xlsx");
                if (string.IsNullOrEmpty(excelPath))
                    return;
                localizationManager.excelPath = excelPath;
                EditorUtility.SetDirty(localizationManager);
            }

            using ExcelPackage excel = new ExcelPackage(new FileInfo(localizationManager.excelPath));

            //localizationManager.displayName = GetDisplayName(Path.GetFileNameWithoutExtension(localizationManager.excelPath));

         
            foreach (var sheet in excel.Workbook.Worksheets)
            {
                Language language = ParseLanguage(sheet.Name);
                LocalizationConfig localizationConfig = localizationManager.GetLocalizationConfig(language);
                if (localizationConfig == null)
                {
                    localizationConfig = ScriptableObject.CreateInstance<LocalizationConfig>();
                    localizationConfig.name = language.ToString();
                    AssetDatabase.AddObjectToAsset(localizationConfig, localizationManager);
                    localizationManager.localizationConfig_language_dict.Add(language, localizationConfig);
                    localizationManager.onValidate?.Invoke();
                    localizationManager.Save();
                    AssetDatabase.Refresh();
                }

                localizationConfig.localizations.Clear();

                int row = SerializationTool.GetExcelRow(sheet);

                for (int i = 4; i <= row; i++)
                {
                    LocalizationData localizationData = new LocalizationData();

                    object keyObj = sheet.GetValue(i, 1);

                    if (keyObj != null)
                        localizationData.key = keyObj.ToString();

                    object contextObj = sheet.GetValue(i, 2);

                    if (contextObj != null)
                        localizationData.context = contextObj.ToString();


                    object imageLoadType = sheet.GetValue(i, 3);
                    if (imageLoadType != null)
                        localizationData.Image.LocalizationImageLoadType = (LocalizationImageLoadType)Enum.Parse(typeof(LocalizationImageLoadType),imageLoadType.ToString());

                    object imageGUID = sheet.GetValue(i, 4);
                    if (imageGUID != null)
                    {
                        localizationData.Image.Icon = YukiAssetDataBase.GUIDToInstance<Sprite>(imageGUID.ToString());
                    }

                    object imageLoaderAB = sheet.GetValue(i, 5);
                    if (imageLoaderAB != null)
                    {
                        try
                        {
                            JObject obj = SerializationTool.DeserializedObject<JObject>(imageLoaderAB.ToString());
                            localizationData.Image.AssetName = obj["AssetName"].ToString();
                            localizationData.Image.ProjectName = obj["ProjectName"].ToString();
                        }
                        catch(Exception ex) 
                        {
                            throw new InvalidCastException("解析图片加载数据失败 ---> ProjectName Or AssetName" + ex.ToString());
                        }
                    }

                    object notesObj = sheet.GetValue(i, 6);
                    if (notesObj != null)
                        localizationData.notes = notesObj.ToString();
                    localizationConfig.localizations.Add(localizationData);
                }
             
                localizationConfig.Save();
            }
          
            EditorUtility.SetDirty(localizationManager);
            AssetDatabase.SaveAssets();
            EditorWindow.focusedWindow.ShowNotification(new GUIContent("导入成功!"));
        }

        private static Language ParseLanguage(string name)
        {
            string n = name;            
            object obj = Enum.Parse(typeof(Language), n);
            return (Language)obj;
        }   

    }
}
#endif