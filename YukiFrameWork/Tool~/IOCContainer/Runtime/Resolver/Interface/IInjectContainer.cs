///=====================================================
/// - FileName:      IInjectContainer.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/2/15 18:44:48
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.IOC
{
	public interface IInjectContainer
	{
		public IResolveContainer Container { get; set; }
	}
}
