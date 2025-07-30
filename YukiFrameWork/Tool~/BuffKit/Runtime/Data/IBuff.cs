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
	public enum BuffMode
	{
		Single,
		Multiple
	}

	/// <summary>
	/// Buff配置接口
	/// </summary>
	public interface IBuff
	{
		/// <summary>
		/// Buff的唯一标识
		/// </summary>
		string Key { get; set; }

		/// <summary>
		/// Buff的名称
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// Buff的介绍
		/// </summary>
		string Description { get; set; }

		/// <summary>
		/// Buff的可存在方式(如果Buff存在，是否可以叠加)
		/// </summary>
		BuffMode BuffMode { get; set; }

        /// <summary>
        /// Buff持续时间,Duration小于0时视为无限时间
        /// </summary>
        float Duration { get; set; }

		/// <summary>
		/// Buff的图标
		/// </summary>
		Sprite Icon { get; set; }

		/// <summary>
		/// 这个Buff存在的所有效果(如继承框架提供的Buff基类，需要自定义Effect的类型，可通过override的方式重写该属性)
		/// </summary>
		List<IEffect> EffectDatas { get; }

		/// <summary>
		/// Buff绑定的控制器类型
		/// <para>Tip:如自定义IBuff的情况下，该属性需要传递完全限定类型(包含命名空间)</para>
		/// </summary>
		string BuffControllerType { get; set; }
	}

	/// <summary>
	/// Buff效果配置接口
	/// </summary>
	public interface IEffect
	{
		/// <summary>
		/// 唯一标识
		/// </summary>
		string Key { get; set; }

		/// <summary>
		/// 效果的名称
		/// </summary>
		string Name { get; set; }

		/// <summary>
		/// 效果的介绍
		/// </summary>
		string Description { get; set; }	
		
		/// <summary>
		/// 效果的类型(当一个Buff有多个效果时,可以为效果指定类型，在查询时获取复数的效果)
		/// </summary>
		string Type { get; set; }
	}
}
