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
	[Obsolete("过时的特性，请使用技能配置中的SkillControllerType进行对控制绑定的配置，该特性已弃用",true)]
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct,AllowMultiple = false,Inherited = true)]
	public class BindSkillControllerAttribute : Attribute
	{
		internal Type controllerType;

		public BindSkillControllerAttribute(Type controllerType)
			=> this.controllerType = controllerType;
	}
}
