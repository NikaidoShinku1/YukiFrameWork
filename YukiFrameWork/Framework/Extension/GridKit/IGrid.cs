///=====================================================
/// - FileName:      IGrid.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/7/18 1:20:20
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	public interface IGrid<T>
	{	
		void ForEach(Action<int, int, T> each);

		void ForEach(Action<T> each);

		T this[int xIndex, int yIndex] { get; set; }

		void Clear(Action<T> cleanItem);		

	}
}
