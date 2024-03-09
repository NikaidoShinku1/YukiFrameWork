///=====================================================
/// - FileName:      CustomSerializeSettingAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/6 14:19:05
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	/// <summary>
	/// 指定需要被序列化的变量，数据，一般作用于对数组/列表的套壳封装或者自己的类,标记后只会序列化指定的参数
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class CustomPropertySettingAttribute : PropertyAttribute
	{
		public string ItemName { get; }		
		public string ArrayLabel { get; }
		public CustomPropertySettingAttribute(string itemName,string arrayLabel) 
		{
			this.ItemName = itemName;
			this.ArrayLabel = arrayLabel;
		}
	}
}
