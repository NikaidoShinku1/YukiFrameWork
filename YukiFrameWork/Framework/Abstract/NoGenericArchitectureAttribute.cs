///=====================================================
/// - FileName:      NoGenericArchitectureAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/2/13 17:00:46
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	/// <summary>
	/// 自动化特性不标记的架构
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class NoGenericArchitectureAttribute : Attribute
	{

	}
}
