///=====================================================
/// - FileName:      ActionExtension.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/26 10:45:48
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections;
namespace YukiFrameWork
{
	public static class ActionExtension
	{
		public static IActionNode ToAction(this IEnumerator enumerator,Action callBack = null)
		{
			bool end = false;
            IEnumerator e()
            {
				yield return enumerator;
				end = true;
            }
			
            return ActionKit.Sequence()
				.CallBack(() => MonoHelper.Start(e()))
				.ExecuteFrame(() => end,callBack);
		}	
		public static YieldTask GetAwaiter(this IActionNode action)
		{			
			return action.ToCoroutine().GetAwaiter();
		}
	}
}
