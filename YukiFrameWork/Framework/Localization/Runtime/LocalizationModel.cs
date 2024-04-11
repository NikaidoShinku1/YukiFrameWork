///=====================================================
/// - FileName:      LocalizationModel.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/11 1:27:51
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
namespace YukiFrameWork
{
	[Serializable]
	public class LocalizationModel
	{
        [field:SerializeField,LabelText("标识")]
		public string Key { get; set; }		
        [field:SerializeField,LabelText("简体中文SimplifiedChinese")]
        public string SimplifiedChinese { get; set; }
        [field:SerializeField,LabelText("繁体中文TraditionalChinese")]
        public string TraditionalChinese { get; set; }
        [field:SerializeField,LabelText("英English")]
        public string English { get; set; }
        [field:SerializeField,LabelText("日本Japanese")]
        public string Japanese { get; set; }
        [field:SerializeField,LabelText("西班牙Spanish")]
        public string Spanish { get; set; }
        [field:SerializeField,LabelText("葡萄牙Portuguese")]
        public string Portuguese { get; set; }
        [field:SerializeField,LabelText("德Garman")]
        public string German { get; set; }
        [field:SerializeField,LabelText("法French")]
        public string French { get; set; }
        [field:SerializeField,LabelText("意大利Italian")]
        public string Italian { get; set; }
        [field:SerializeField,LabelText("韩Korean")]
        public string Korean { get; set; }
        [field:SerializeField,LabelText("俄Russian")]
        public string Russian { get; set; }
        [field:SerializeField,LabelText("波兰Polish")]
        public string Polish { get; set; }
        [field:SerializeField,LabelText("土耳其Turkish")]
        public string Turkish { get; set; }
        [field:SerializeField,LabelText("阿拉伯Arabic")]
        public string Arabic { get; set; }
        [field:SerializeField,LabelText("Thai泰")]
        public string Thai { get; set; }
        [field:SerializeField,LabelText("印尼Indonesian")]
        public string Indonesian { get; set; }
        [field:SerializeField,LabelText("荷兰Dutch")]
        public string Dutch { get; set; }
        [field:SerializeField,LabelText("印度Hindi")]
        public string Hindi { get; set; }
        [field:SerializeField,LabelText("越南Vietnamese")]
        public string Vietnamese { get; set; }


    }
}
