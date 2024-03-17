///=====================================================
/// - FileName:      DisplayTextureAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/3 18:47:37
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	/// <summary>
	/// 只对类型为Sprite,Texture,Texture2D的变量生效!可以自定义图片的显示
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public sealed class DisplayTextureAttribute : PropertyAttribute
	{
		public float Width { get; }
		public float Height { get; }
		public DisplayTextureAttribute(float width = 64, float height = 64)
		{
			this.Width = width;
			this.Height = height;
		}
	}
}
