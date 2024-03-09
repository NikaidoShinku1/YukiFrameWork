///=====================================================
/// - FileName:      GUIGroupAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/3 17:33:24
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class GUIGroupAttribute : PropertyAttribute
	{
		public string GroupName { get; private set; }
		public GUIGroupAttribute(string groupName)
		{
			this.GroupName = groupName;
		}
	}
}
