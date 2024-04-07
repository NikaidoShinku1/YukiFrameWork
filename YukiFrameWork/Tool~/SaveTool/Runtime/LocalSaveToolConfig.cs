///=====================================================
/// - FileName:      LocalSaveToolConfig.cs
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

namespace YukiFrameWork
{
    public enum FolderType
    {
        [LabelText("Application.persistentDataPath")]
        persistentDataPath,
        [LabelText("Application.dataPath")]
        dataPath,
        [LabelText("Application.streamingAssetsPath")]
        streamingAssetsPath,
        [LabelText("Application.temporaryCachePath")]
        temporaryCachePath,           
        [LabelText("自定义存档路径")]
        custom
    }
    [CreateAssetMenu(fileName = "SaveConfig",menuName = " Yuki/Config")]
	public class LocalSaveToolConfig : ScriptableObject
    {
        [LabelText("文件夹名称:")]
        public string saveFolderName = "SaveData";

        [HideInInspector, LabelText("保存的文件路径:")]
        public string saveFolder;

        public string saveDirPath => saveFolder + @"/" + saveFolderName;
        [HideInInspector]
        [LabelText("文件夹方式选择:"),SerializeField]
        public FolderType folderType = FolderType.persistentDataPath;     

        [HideInInspector]
        public bool saveInfoFoldOut; 
     
        [LabelText("当前所有的存档信息:"),ReadOnly]
        public List<SaveInfo> infos = new List<SaveInfo>();

        [LabelText("当前存档的id:")]
        [HideInInspector]
        public int currentID => infos.Count;       
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
       // [JsonProperty]
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
