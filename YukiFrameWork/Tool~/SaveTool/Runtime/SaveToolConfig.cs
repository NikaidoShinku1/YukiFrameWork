///=====================================================
/// - FileName:      SaveToolConfig.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/2 11:18:57
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork
{  
    [HideMonoScript]
	public class SaveToolConfig : ScriptableObject
    {
#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        static void CreateConfig()
        {
            string path = "Assets/Resources";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
            }

            SaveToolConfig config = Resources.Load<SaveToolConfig>(nameof(SaveToolConfig));

            if (config == null)
            {              
                config = ScriptableObject.CreateInstance<SaveToolConfig>();
                AssetDatabase.CreateAsset(config, path + "/" + nameof(SaveToolConfig) + ".asset");
                AssetDatabase.Refresh();
            }
        }
#endif

        [LabelText("文件夹名称:"), BoxGroup("文件路径设置")]
        public string saveFolderName = "SaveData";

        [LabelText("保存的文件路径:"), BoxGroup("文件路径设置"),FolderPath(AbsolutePath = true), ShowIf(nameof(IsCustom))]
        public string saveFolder;

        public string saveDirPath
        {
            get
            {
                string fileName = @"/" + saveFolderName;
                switch (folderType)
                {
                    case FolderType.persistentDataPath:
                        return Application.persistentDataPath + fileName;
                    case FolderType.dataPath:
                        return Application.dataPath + fileName;                    
                    case FolderType.streamingAssetsPath:
                        return Application.streamingAssetsPath + fileName;                      
                    case FolderType.temporaryCachePath:
                        return Application.temporaryCachePath + fileName;                       
                    case FolderType.custom:
                        return saveFolder + fileName;
                }

                return default;
            }
        }
        [LabelText("文件夹方式选择:"),SerializeField, BoxGroup("文件路径设置")]
        public FolderType folderType = FolderType.persistentDataPath;   
       
        [LabelText("当前存档的id:")]       
        public int currentID => infos.Count;

        private bool IsCustom => folderType == FolderType.custom;

        [Button("打开文件夹"), BoxGroup("文件路径设置"), HideIf(nameof(IsCustom))]
        private void CheckMouseToPosition2()
        {
            System.Diagnostics.Process.Start("explorer.exe", saveFolder.Replace("/", "\\"));
        }

        [LabelText("当前所有的存档信息:"),BoxGroup]
        public List<SaveInfo> infos = new List<SaveInfo>();    
    }


    [Serializable]
    public class SaveInfo
    {
        [LabelText("存档的ID")]
        public int saveID;

        [NonSerialized]
        private DateTime lastDateTime;

        //[JsonIgnore]
        public DateTime LastDateTime
        {
            get
            {
                if (lastDateTime == default(DateTime))
                {
                    DateTime.TryParse(lastDateTimeString, out lastDateTime);
                }
                return lastDateTime;
            }
        }

        [SerializeField, LabelText("存档时间")]
        //[JsonProperty]
        private string lastDateTimeString;
        public SaveInfo(int saveID, DateTime dateTime)
        {
            this.saveID = saveID;
            this.lastDateTime = dateTime;
            lastDateTimeString = lastDateTime.ToString();
        }

        public SaveInfo()
        {
            var dateTime = System.DateTime.Now;
            this.lastDateTime = dateTime;
            lastDateTimeString = lastDateTime.ToString();
        }

        public void Update_LastTime(DateTime dateTime)
        {
            lastDateTime = dateTime;
            lastDateTimeString = lastDateTime.ToString();
        }
    }
}
