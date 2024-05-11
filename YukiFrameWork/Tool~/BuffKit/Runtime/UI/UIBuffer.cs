///=====================================================
/// - FileName:      UIBuffer.cs
/// - NameSpace:     YukiFrameWork.Buffer
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/10 22:12:48
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Buffer
{
	public abstract class UIBuffer : MonoBehaviour
	{				
		/// <summary>
		/// 当Buff启动时同步调用的UI生命周期方法
		/// </summary>
		/// <param name="buffKey">buff标识</param>		
		public abstract void OnBuffStart(IBuff buff,string buffKey,int buffLayer);

		/// <summary>
		/// 当Buff正在运行时，持续调用UIBuffer的Update周期反复方法
		/// </summary>		
		/// <param name="remainingTime">Buff剩余时间</param>
		/// <param name="buffLayer">Buff的层级</param>
		public abstract void OnBuffUpdate(float remainingTime);

		/// <summary>
		/// 当Buff移除时，该方法也会同步执行
		/// </summary>
		public abstract void OnBuffRemove(int buffLayer);

		/// <summary>
		/// 当Buff销毁时，该方法也会同步执行
		/// </summary>
		public abstract void OnBuffDestroy();

		public abstract void OnDispose();
	}
}
