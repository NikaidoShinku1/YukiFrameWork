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
using Newtonsoft.Json;
#if UNITY_EDITOR
using UnityEditor;
#endif
namespace YukiFrameWork
{
    [Serializable]
    public class SaveInfo
    {
        [LabelText("存档的ID")]
        [JsonProperty] public int saveID;

        [JsonIgnore]
        private DateTime lastDateTime;

        [JsonIgnore]
        public DateTime LastDateTime
        {
            get
            {               
                DateTime.TryParse(lastDateTimeString, out lastDateTime);
                return lastDateTime;
            }
        }

        [SerializeField, LabelText("存档时间")]
        //[JsonProperty]
        [JsonProperty]private string lastDateTimeString;
        public SaveInfo(int saveID, DateTime dateTime)
        {
            this.saveID = saveID;
            this.lastDateTime = dateTime;
            lastDateTimeString = lastDateTime.ToString();
        }
        [JsonConstructor]
        private SaveInfo()
        {
            
        }

        public void Update_LastTime(DateTime dateTime)
        {
            lastDateTime = dateTime;
            lastDateTimeString = lastDateTime.ToString();
        }
    }
}
