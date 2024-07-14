///=====================================================
/// - FileName:      ILoader.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/7/14 14:01:31
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	public interface IResLoader<in T>
	{
		public TItem Load<TItem>(string name) where TItem : T;

		public void LoadAsync<TItem>(string name, Action<TItem> onCompleted) where TItem : T;
	}
}
