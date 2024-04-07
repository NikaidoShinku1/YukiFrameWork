///=====================================================
/// - FileName:      ApplicationHelper.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/7 19:26:08
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Extension
{
	public static class ApplicationHelper
	{
		public static bool GetRuntimeOrEditor() => Application.isPlaying;
	}
}
