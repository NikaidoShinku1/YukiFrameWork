///=====================================================
/// - FileName:      PropertyRangeAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/4 18:07:09
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	/// <summary>
	/// 设置给数据类型属性/字段一个滑条值
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public class PropertyRangeAttribute : PropertyAttribute
	{
		private readonly float minValue;
		private readonly float maxValue;

		public float MinValue => minValue;
		public float MaxValue => maxValue;
		public PropertyRangeAttribute(float minValue,float maxValue)
		{
			this.minValue = minValue;
			this.maxValue = maxValue;
		}
	}
}
