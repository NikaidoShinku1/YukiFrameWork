///=====================================================
/// - FileName:      DiaLogDebugger.cs
/// - NameSpace:     YukiFrameWork.DiaLog
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/30 0:12:54
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
namespace YukiFrameWork.DiaLog
{
	public class DiaLogDebugger : MonoBehaviour
	{
		[LabelText("输入DiaLog Tree的标识")]
		public string Key;

		public Color selectColor;
	}
}
