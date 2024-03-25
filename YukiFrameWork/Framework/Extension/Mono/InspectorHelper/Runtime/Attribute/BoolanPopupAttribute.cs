///=====================================================
/// - FileName:      BoolanPopupAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/7 19:25:45
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	/// <summary>
	/// 把bool转换为显示开启关闭的列表
	/// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property,AllowMultiple = false)]
	public sealed class BoolanPopupAttribute : PropertyAttribute
	{
		private string mFalse;
		private string mTrue;

		public string FalseValue => mFalse;
		public string TrueValue => mTrue;	

		public BoolanPopupAttribute()
		{
			mFalse = "关闭";
			mTrue = "开启";
		}

        public BoolanPopupAttribute(string mFalseValue,string mTrueValue)
        {
			mFalse = mFalseValue;
			mTrue = mTrueValue;
        }
    }
}
