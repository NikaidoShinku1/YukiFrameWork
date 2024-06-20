///=====================================================
/// - FileName:      InitControllerAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/9 20:03:59
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	[AttributeUsage(AttributeTargets.Class,AllowMultiple = false)]
	public class InitControllerAttribute : PropertyAttribute
	{
		public InitControllerAttribute(int order = 0)
			=> this.order = order;
	}
}
