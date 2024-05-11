///========================================	=============
/// - FileName:      BuffType.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/5 17:16:29
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
namespace YukiFrameWork.Buffer
{
	public enum BuffRepeatAdditionType
	{
		[LabelText("不执行任何操作")]
		None,
		[LabelText("重置Buff的时间")]
		Reset,
		[LabelText("可叠加的Buff")]
		Multiple,
		[LabelText("可叠加且每次都重置时间的Buff")]
		MultipleAndReset,
		[LabelText("该Buff不叠加但可以同时存在多个")]
		MultipleCount
	}

	public enum BuffSurvivalType
	{
		[LabelText("计时器")]
		Timer,
		[LabelText("永久性")]
		Permanent,

	}
}
