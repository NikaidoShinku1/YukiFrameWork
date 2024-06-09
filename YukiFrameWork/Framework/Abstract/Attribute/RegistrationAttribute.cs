///=====================================================
/// - FileName:      RuntimeInitializeModel.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/6 13:56:31
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{	
	/// <summary>
	/// 自动化注册特性，标记后可以自动注册各个层级的模块
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
	public class RegistrationAttribute : Attribute
	{
		internal Type architectureType;
		internal Type registerType;

		internal bool IsCustomType;
		/// <summary>
		/// 注册模块进架构的自动化特性
		/// </summary>
		/// <param name="architectureType">架构类型</param>
		/// <param name="registerType">具体的注册类型(例如希望注册到接口时)</param>
		public RegistrationAttribute(Type architectureType, Type registerType = null)
		{
			this.architectureType = architectureType;					
			this.registerType = registerType;
			IsCustomType = !this.registerType.IsNull();
		}
	}
}
