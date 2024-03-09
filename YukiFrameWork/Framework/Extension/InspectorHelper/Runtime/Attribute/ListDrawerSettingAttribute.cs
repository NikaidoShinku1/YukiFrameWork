///=====================================================
/// - FileName:      ListDrawerSettingAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/6 20:07:24
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	/// <summary>
	/// 将列表在编辑器中以ReorderableList展示
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class ListDrawerSettingAttribute : PropertyAttribute
	{
		public bool IsReadOnly { get; }

		public ListDrawerSettingAttribute(bool IsReadOnly = false)
			=> this.IsReadOnly = IsReadOnly;
	}
}
