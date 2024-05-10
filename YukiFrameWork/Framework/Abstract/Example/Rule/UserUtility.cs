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
	public class UserUtility : IUtility
	{
		public void DebugInfo()
		{
			Debug.Log("该方法仅用于触发一次日志");
		}
	}
}
