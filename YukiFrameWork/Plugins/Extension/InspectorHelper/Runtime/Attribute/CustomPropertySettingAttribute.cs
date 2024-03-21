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
    /// 在标记了Serializable的类中指定需要被序列化的变量，数据,标记后只会序列化指定的参数(仅对类内指定的数组/列表参数生效)，标记该特性后可以为声明的字段标记ListDrawingSetting特性修改样式
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
	public class CustomPropertySettingAttribute : PropertyAttribute
	{
		public string ItemName { get; }		
		public CustomPropertySettingAttribute(string itemName) 
		{
			this.ItemName = itemName;
			
		}
	}
}
