///=====================================================
/// - FileName:      IBuff.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/5 16:26:49
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
namespace YukiFrameWork.Buffer
{
	public interface IBuff
	{
		/// <summary>
		/// BUff唯一标识
		/// </summary>
		string GetBuffKey { get; }

		/// <summary>
		/// Buff名称
		/// </summary>
		string GetBuffName { get; }

		/// <summary>
		/// Buff介绍
		/// </summary>
		string GetDescription { get; }

		/// <summary>
		/// 当Buff存在时重复添加类型
		/// </summary>
		BuffRepeatAdditionType AdditionType { get; }

		/// <summary>
		/// Buff的周期类型(计时器/永久)
		/// </summary>
		BuffSurvivalType SurvivalType { get; }

		/// <summary>
		/// 如果Buff是可叠加的，True：Buff一层一层减少，False：一次性消失
		/// </summary>
		bool IsBuffRemoveBySlowly { get; }

		/// <summary>
		/// 如果Buff是可叠加的，开启该属性则代表Buff有最大添加上限。
		/// </summary>
        bool IsMaxStackableLayer { get; }

		/// <summary>
		/// 最大层数上限
		/// </summary>
        int MaxStackableLayer { get; }

        /// <summary>
        /// Buff的精灵
        /// </summary>
        Sprite BuffIcon { get; }		
		
		/// <summary>
		/// Buff的持续时间,当SurivialType选择计时器时使用，单位秒
		/// </summary>
		float BuffTimer { get; }

		/// <summary>
		/// buff相互抵消的标识(相互抵消的标识如果同时存在禁止添加的标识中，则以禁止添加为主)
		/// </summary>
		string[] BuffCounteractID { get; }

		/// <summary>
		/// buff运行期间禁止添加的标识
		/// </summary>
		string[] BuffDisableID { get; }
	}
}
