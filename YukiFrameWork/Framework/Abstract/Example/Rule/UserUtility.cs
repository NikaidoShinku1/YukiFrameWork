///=====================================================
/// - FileName:      UserUtility.cs
/// - NameSpace:     YukiFrameWork.ExampleRule.ExampleFrameWork
/// - Description:   示例工具，非必要不可修改，仅供参考
/// - Creation Time: 2024/5/10 18:19:42
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.ExampleRule.ExampleFrameWork
{
	[Registration(typeof(User))]//自动注册特性，非Controller层级都应该标记该特性
    public class UserUtility : IUtility
	{
		public void DebugInfo()
		{
			Debug.Log("该方法仅用于触发一次日志");
		}
	}
}
