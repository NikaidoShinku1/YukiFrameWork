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
    [HideMonoScript]
	public class SaveToolConfig : ScriptableObject
    {
        [LabelText("文件夹名称:"), BoxGroup("文件路径设置")]
        public string saveFolderName = "SaveData";

        [LabelText("保存的文件路径:"), BoxGroup("文件路径设置"), ShowIf(nameof(IsCustom))]
        public string saveFolder;

        public string saveDirPath => saveFolder + @"/" + saveFolderName;    
        [LabelText("文件夹方式选择:"),SerializeField, BoxGroup("文件路径设置")]
        public FolderType folderType = FolderType.persistentDataPath;   
       
        [LabelText("当前存档的id:")]       
        public int currentID => infos.Count;

        private bool IsCustom => folderType == FolderType.custom;
        [Button("定位到指定文件夹"), BoxGroup("文件路径设置"), ShowIf(nameof(IsCustom))]
        private void CheckMouseToPosition()
        {
#if UNITY_EDITOR
            saveFolder = UnityEditor.EditorUtility.OpenFolderPanel("定位到指定文件夹", string.Empty, string.Empty);
#endif
        }

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
