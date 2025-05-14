///=====================================================
/// - FileName:      LocalizationConfig.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   本地化配置
/// - Creation Time: 2024/4/8 13:44:56
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using UnityEngine;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections;

namespace YukiFrameWork
{

    public class LocalizationConfig : ScriptableObject
    {
        protected const string groupName = "本地设置:";
        //[DictionaryDrawerSettings(KeyLabel = "标识", ValueLabel = "配置信息"), BoxGroup(groupName)]       
        [LabelText("本地数据配置:"), BoxGroup(groupName)]
        [JsonProperty]
        [TableList(NumberOfItemsPerPage = 5, DrawScrollView = true), Searchable]
        [SerializeField]
        public List<LocalizationData> localizations = new List<LocalizationData>();    
        internal IList ExcelArray => localizations;
    }
}
