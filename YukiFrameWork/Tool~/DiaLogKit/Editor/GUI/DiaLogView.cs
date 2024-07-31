///=====================================================
/// - FileName:      DiaLogView.cs
/// - NameSpace:     YukiFrameWork.DiaLogue
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/7/27 18:51:12
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEngine.UIElements;
namespace YukiFrameWork.DiaLogue
{
	public class DiaLogView : TwoPaneSplitView
	{
		public new class UxmlFactory : UxmlFactory<DiaLogView, UxmlTraits> { }

		public DiaLogView()
		{
			
		}
	}
}
