///=====================================================
/// - FileName:      GUIColorAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/2 17:12:02
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	public enum ColorType
	{
		Red,
		Green,
		White,
		Black,
		Blue,
		Yellow,
		Cyan,
		Gamma,
		Gray,
		Grey,		
	}
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Method)]
	public class GUIColorAttribute : PropertyAttribute
	{
		private Color color;

		public Color Color => color;
		public GUIColorAttribute (float r,float g,float b,float a)
		{
            color = new Color(r, g, b, a);
        }

		public GUIColorAttribute (float r, float g, float b) 
		{
			color = new Color(r, g, b, 1);
		}

		public GUIColorAttribute(ColorType color)
		{
			switch (color)
			{
				case ColorType.Red:
					this.color = Color.red;
					break;
				case ColorType.Green:
					this.color = Color.green;
					break;
				case ColorType.White:
					this.color = Color.white;
					break;
				case ColorType.Black:
					this.color = Color.black;
					break;
				case ColorType.Blue:
					this.color = Color.blue;
					break;
				case ColorType.Yellow:
					this.color = Color.yellow;
					break;
				case ColorType.Cyan:
					this.color = Color.cyan;
					break;
				case ColorType.Gamma:
					this.color = Color.gamma;
					break;
				case ColorType.Gray:
					this.color = Color.gray;
					break;
				case ColorType.Grey:
					this.color = Color.grey;
					break;				
			}
		}
	}
}
