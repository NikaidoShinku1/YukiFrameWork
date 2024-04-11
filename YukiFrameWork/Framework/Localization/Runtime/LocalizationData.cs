///=====================================================
/// - FileName:      LocalizationConfigData.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/8 22:05:21
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
namespace YukiFrameWork
{ 
    /// <summary>
    /// 本地数据接口
    /// </summary>
    public interface ILocalizationData
	{		
        /// <summary>
        /// 文本内容
        /// </summary>
		string Context { get; set; }
        /// <summary>
        /// 精灵(图片)
        /// </summary>
		Sprite Sprite { get; set; }
    } 
    
    [Serializable]
    public class LocalizationData : ILocalizationData
    {              
        [field: MultiLineProperty,JsonProperty]
        [field: SerializeField]
        public string Context { get; set ; }             
        
        [JsonIgnore]
        [field: PreviewField(50)]
        [field: SerializeField]
        public Sprite Sprite { get; set; }      
    }    
}
