///=====================================================
/// - FileName:      BindSkillController.cs
/// - NameSpace:     YukiFrameWork.Skills
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/6/19 21:03:04
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Skill
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct,AllowMultiple = false,Inherited = true)]
	public class BindSkillControllerAttribute : Attribute
	{
		internal Type controllerType;

		public BindSkillControllerAttribute(Type controllerType)
			=> this.controllerType = controllerType;
	}
}
