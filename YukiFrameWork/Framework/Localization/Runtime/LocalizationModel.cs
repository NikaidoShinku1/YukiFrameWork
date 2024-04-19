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
		[field:SerializeField,LabelText("文本内容")]
        public string Context { get; set; }      
    }
}
