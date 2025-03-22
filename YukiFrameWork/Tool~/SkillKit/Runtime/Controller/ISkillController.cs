///=====================================================
/// - FileName:      ISkillController.cs
/// - NameSpace:     YukiFrameWork.Skill
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/28 21:19:21
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Pools;
using System.Collections.Generic;
namespace YukiFrameWork.Skill
{
	public interface ISkillController : IGlobalSign
	{
		/// <summary>
		/// 技能绑定的游戏对象
		/// </summary>
		ISkillExecutor Player { get; internal set; }

		List<string> SimultaneousSkillKeys { get; set; }

        /// <summary>
        /// 技能中枢管理者
        /// </summary>
        SkillHandler Handler { get; }

		/// <summary>
		/// 冷却中触发的回调
		/// </summary>
		Action<float> onCooling { get; set; }
		/// <summary>
		/// 释放中触发的回调
		/// </summary>
		Action<float> onReleasing { get; set; }
        /// <summary>
        /// 冷却结束触发的回调
        /// </summary>
        Action onCoolingComplete { get; set; }
        /// <summary>
        /// 释放结束触发的回调
        /// </summary>
        Action onReleaseComplete { get; set; }

        /// <summary>
        /// 等级被改变时触发的回调(仅技能数据开启等级机制后生效)
        /// </summary>
        Action<int> onLevelChanged { get; set; }
        /// <summary>
        /// 技能信息/数据
        /// </summary>
        ISkillData SkillData { get; internal set; }
		[Obsolete("已废弃的属性，技能的等级应该在指定的位置单独配表。技能系统应该更侧重于对逻辑本身的拓展")]
		int SkillLevel { get; set; }
		/// <summary>
		/// 技能是否正在释放
		/// </summary>
		bool IsSkillRelease { get; internal set; }
		/// <summary>
		/// 已经释放的时间
		/// </summary>
		float ReleasingTime { get; internal set; }
		/// <summary>
		/// 释放进度:进度(0-1)
		/// </summary>
		float ReleasingProgress { get; }
		/// <summary>
		/// 是否冷却完成
		/// </summary>
		bool IsSkillCoolDown { get; internal set; }
		/// <summary>
		/// 已经冷却的时间
		/// </summary>
		float CoolDownTime { get; internal set; }
		/// <summary>
		/// 冷却进度:进度(0-1)
		/// </summary>
		float CoolDownProgress { get; }
		/// <summary>
		/// 是否可以释放技能
		/// </summary>
		/// <returns></returns>
		bool IsCanRelease();

        /// <summary>
        /// 判断技能是否释放完成，默认是返回True的，代表可以直接结束。当技能不受到释放时间影响时，需要自己重写逻辑
        /// </summary>
        /// <returns></returns>
        bool IsComplete();

		/// <summary>
		/// 生命周期初始化
		/// </summary>
		void OnAwake();
		/// <summary>
		/// 释放技能时触发一次
		/// </summary>
		void OnRelease(params object[] param);
		/// <summary>
		/// 释放结束触发一次
		/// </summary>
		void OnReleaseComplete();
		/// <summary>
		/// 技能被打断时触发
		/// </summary>
		void OnInterruption();

        #region 技能释放时触发的生命周期方法
		/// <summary>
		/// 技能释放时执行的Update方法
		/// </summary>
        void OnUpdate();

		/// <summary>
		/// 技能释放时执行的FixedUpdate方法
		/// </summary>
		void OnFixedUpdate();

		/// <summary>
		/// 技能释放时执行的LateUpdate方法
		/// </summary>
		void OnLateUpdate();
        #endregion
        /// <summary>
        /// 技能冷却时持续触发
        /// </summary>
        void OnCooling();		
		/// <summary>
		/// 冷却结束时触发的回调
		/// </summary>
		void OnCoolingComplete();
		/// <summary>
		/// 技能销毁时触发的回调
		/// </summary>
		void OnDestroy();
	}
}
