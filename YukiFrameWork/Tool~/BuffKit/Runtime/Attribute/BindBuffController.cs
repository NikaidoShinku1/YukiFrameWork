///=====================================================
/// - FileName:      BindBuffController.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/19 3:33:01
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Buffer
{
	/// <summary>
	/// Buff控制器绑定特性,需要为Buff标记该特性,标记后会自动绑定控制器,
	///<para> 或者在BuffKit初始化数据后通过BuffKit.BindController手动对Buff的控制器进行绑定</para>
	/// </summary>
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct,AllowMultiple = false,Inherited = true)]
	public class BindBuffControllerAttribute : Attribute
	{
		internal Type ControllerType;

		public BindBuffControllerAttribute(Type controllerType)
			=> this.ControllerType = controllerType;
	}
}
