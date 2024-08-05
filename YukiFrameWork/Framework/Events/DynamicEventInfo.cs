///=====================================================
/// - FileName:      DynamicEventInfo.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/8/2 13:26:24
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Reflection;
namespace YukiFrameWork
{
	public struct DynamicEventInfo<T> where T : Delegate
	{
		public T onEvent;
		public MethodInfo methodInfo;		
	}
}
