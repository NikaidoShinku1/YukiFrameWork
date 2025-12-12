///=====================================================
/// - FileName:      ScreenTool.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/24 19:55:07
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	public static class ScreenTool
	{
		public static readonly EasyEvent OnScreenChanged = new EasyEvent();
		public static readonly EasyEvent OnvSyncChanged = new EasyEvent();
		public static void SetFullScreen(bool active)
		{
            Screen.fullScreen = active;
            OnScreenChanged.SendEvent();           
		}

		public static FullScreenMode GetScreenMode()
			=> Screen.fullScreenMode;

		public static void SetResolution(int width, int height, bool fullScreen)
		{
			Screen.SetResolution(width, height, fullScreen);
			OnScreenChanged.SendEvent();
		}

		public static void SetvSyncCount(int count)
		{
			QualitySettings.vSyncCount = count;
			OnvSyncChanged.SendEvent();
		}
    }
}
