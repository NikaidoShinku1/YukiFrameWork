#if UNITY_EDITOR

using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace YukiFrameWork.Extension
{
    [HideLabel]
    [Serializable]
    public class SerializationWindow
    {
        [Serializable]
        class FileData
        {
            [LabelText("需要转换的脚本")]
            [InfoBox("注意:不允许拖入派生自UnityEngine.Object的脚本文件",InfoMessageType.Warning)]
            public MonoScript monoScript;
            [LabelText("转换类型")]
            public SerializationType serialization;
            [LabelText("文件路径"),Sirenix.OdinInspector.FolderPath,]
            public string folderPath = "Assets/Data";
            [LabelText("文件名称")]
            public string fileName;
        }
        [Serializable]
        class JsonData
        {
            [LabelText("Excel的文件路径"),Sirenix.OdinInspector.FilePath]
            public string excelPath;
            [LabelText("文件路径"),HorizontalGroup("Json文件的路径设置")]
            public string jsonPath;
            [LabelText("文件名称"),HorizontalGroup("Json文件的路径设置")]
            public string jsonName;
            [InfoBox("判断多少行是表头")]
            [LabelText("表头")]
            public int header;

            [InfoBox("该项会过滤包含这里输入的字符的列")]
            [LabelText("过滤字符")]
            public string ex_suffix;
        }
        [HideInInspector]
        public int selectIndex;

        [SerializeField, ShowIf(nameof(selectIndex), 0),PropertySpace,LabelText("需要添加处理的脚本文件")]
        [TableList]
        private List<FileData> fileDatas = new List<FileData>();

        [Button("生成文件流",ButtonHeight = 30),PropertySpace,GUIColor("yellow"), ShowIf(nameof(selectIndex), 0)]
        void Generic()
        {
            if (fileDatas.Count == 0)
            {
                Debug.LogError("没有添加文件，请重试");
                return;
            }

            foreach (var item in fileDatas)
            {
                if (item.monoScript == null) 
                    continue;

                if (string.IsNullOrEmpty(item.folderPath) || string.IsNullOrEmpty(item.fileName))
                    continue;

                if (item.monoScript.GetClass().IsSubclassOf(typeof(UnityEngine.Object)))
                {
                    Debug.LogError("不允许拖入派生自UnityEngine.Object的脚本类,该项已略过,FileName:" + item.fileName);
                    continue;
                }

                switch (item.serialization)
                {
                    case SerializationType.Json:
                        SerializationTool.SerializedObject(Activator.CreateInstance(item.monoScript.GetClass())).CreateFileStream(item.folderPath, item.fileName, ".json");
                        break;
                    case SerializationType.Xml:
                        SerializationTool.XmlSerializedObject(Activator.CreateInstance(item.monoScript.GetClass())).CreateFileStream(item.folderPath, item.fileName, ".xml");
                        break;                   
                }

            }
        }
     
        [LabelText("保存所有的excel路径配置"),SerializeField,Sirenix.OdinInspector.DictionaryDrawerSettings(KeyLabel = "Excel文件路径",ValueLabel = "导出的Json文件路径"),ShowIf(nameof(selectIndex),1)]
        [TableList]
        private List<JsonData> excelPaths = new List<JsonData>();         
        private bool showBtn => excelPaths != null && excelPaths.Count > 0 && selectIndex == 1;
        [Button("创建Json文件",ButtonHeight = 30),PropertySpace(20), GUIColor("yellow"), ShowIf(nameof(showBtn))]
        private void DrawExcelToJsonInfo()
        {
            foreach (var data in excelPaths)
            {
                var excelPath = data.excelPath;
                if (string.IsNullOrEmpty(excelPath))
                    continue;

                string json = SerializationTool.ExcelToJson(excelPath, data.header, data.ex_suffix);
                json.CreateFileStream(data.jsonPath,data.jsonName,".json");
            }
        }

      
    }    
}
#endif