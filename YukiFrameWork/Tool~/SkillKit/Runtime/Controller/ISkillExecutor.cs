///=====================================================
/// - FileName:      ISkillExecutor.cs
/// - NameSpace:     YukiFrameWork.Skill
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/10 13:21:44
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Skill
{
	public interface ISkillExecutor
	{
		/// <summary>
		/// 技能中枢管理者
		/// </summary>
		SkillHandler Handler { get; }	

		/// <summary>
		/// 全局的是否可以释放技能API，当执行者的IsCanRelease为False时，什么技能都无法释放。
		/// </summary>
		/// <returns></returns>
		bool IsCanRelease();
	}
}
