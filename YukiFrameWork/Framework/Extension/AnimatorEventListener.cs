///=====================================================
/// - FileName:      AnimatorEventListener.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/22 18:51:43
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEngine.Events;
namespace YukiFrameWork
{
	[Serializable]
	public class AnimatorEvent : UnityEvent<string>
	{ }
	public class AnimatorEventListener : MonoBehaviour
	{
		public AnimatorEvent onAnimatorEvent;

		public void OnAnimatorEvent(string name)
			=> onAnimatorEvent?.Invoke(name);

		public void AddListener(UnityAction<string> action)
			=> onAnimatorEvent.AddListener(action);
	}
}
