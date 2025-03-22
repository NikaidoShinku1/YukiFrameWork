///=====================================================
/// - FileName:      ISkillData.cs
/// - NameSpace:     YukiFrameWork.Skill
/// - Description:   技能数据接口
/// - Creation Time: 2024/5/28 21:13:44
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
namespace YukiFrameWork.Skill
{	
	public interface ISkillData
	{
		/// <summary>
		/// 技能标识(唯一)
		/// </summary>
		string SkillKey { get; set; }
		/// <summary>
		/// 技能名称
		/// </summary>
		string SkillName { get; set; }	
		/// <summary>
		/// 技能介绍
		/// </summary>
		string Description { get; set; }
		/// <summary>
		/// 技能图标
		/// </summary>
		Sprite Icon { get; set; }		

		/// <summary>
		/// 技能释放是否是无限时间的
		/// </summary>		
		bool IsInfiniteTime { get; set; }

		/// <summary>
		/// 技能是否可以主动取消
		/// </summary>
		bool ActiveCancellation { get; set; }

		/// <summary>
		/// 技能释放时间
		/// </summary>
		float RealeaseTime { get; set; }
		
		/// <summary>
		/// 技能冷却时间
		/// </summary>
		float CoolDownTime { get; set; }
		/// <summary>
		/// 技能是否可以被中途打断
		/// </summary>
		bool SkillInterruption { get; set; }
		/// <summary>
		/// 可以同时释放的技能标识
		/// </summary>
		string[] SimultaneousSkillKeys { get; set; }
	
	}
}
