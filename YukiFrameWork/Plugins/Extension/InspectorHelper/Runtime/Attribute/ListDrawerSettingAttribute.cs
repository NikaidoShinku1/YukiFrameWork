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
	/// 将列表在编辑器中以ReorderableList展示(只对派生自UnityEngine.Object的类生效,否则请使用ArrayLabelSetting)
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class ListDrawerSettingAttribute : PropertyAttribute
	{
		public bool IsReadOnly { get; }

		public string ValueName { get; }

		public bool IsAutoOnly { get; }

		public bool IsReversal { get; }

		public ListDrawerSettingAttribute(bool IsReadOnly = false)
		{
            this.IsReadOnly = IsReadOnly;
			IsAutoOnly = true;
        }

		public ListDrawerSettingAttribute(string ValueName,bool isReversal = false)
        {
			this.ValueName = ValueName;
			IsAutoOnly = false;
			this.IsReversal = isReversal;
        }
	}
}
