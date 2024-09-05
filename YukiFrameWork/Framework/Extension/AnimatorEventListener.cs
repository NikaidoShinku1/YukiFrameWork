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
		[SerializeField]
		private AnimatorEvent onAnimatorEvent = new AnimatorEvent();

		public void OnAnimatorEvent(string name)
			=> onAnimatorEvent?.Invoke(name);

		public void AddListener(UnityAction<string> action)
			=> onAnimatorEvent.AddListener(action);

		public void RemoveListener(UnityAction<string> action)
			=> onAnimatorEvent.RemoveListener(action);

		public void RemoveAllListeners()
			=> onAnimatorEvent.RemoveAllListeners();
	}
}
