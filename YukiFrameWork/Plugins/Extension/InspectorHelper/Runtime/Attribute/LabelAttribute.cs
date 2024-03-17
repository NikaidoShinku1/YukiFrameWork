///=====================================================
/// - FileName:      LabelAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/1 19:57:25
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	/// <summary>
	/// 对于字段或者属性,可以进行在编辑器下的名称自定义
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class LabelAttribute : PropertyAttribute
	{
		private string label;

		public string Label => label;
		public LabelAttribute(string label)
		{
			this.label = label;
		}
	}
}
