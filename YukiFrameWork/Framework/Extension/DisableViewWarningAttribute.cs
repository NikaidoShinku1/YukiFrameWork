///=====================================================
/// - FileName:      DisableViewWarningAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/9/25 15:57:50
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	[AttributeUsage(AttributeTargets.Class)]
	public class DisableViewWarningAttribute : Attribute
	{

	}

	[AttributeUsage(AttributeTargets.Method)]
	public class DisableEnumeratorWarningAttribute : Attribute
	{
		
	}
}
