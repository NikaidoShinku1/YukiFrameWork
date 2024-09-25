///=====================================================
/// - FileName:      UIOption.cs
/// - NameSpace:     YukiFrameWork.DiaLogue
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/5/2 21:39:48
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEngine.UI;
using Sirenix.OdinInspector;
using UnityEngine.Events;
namespace YukiFrameWork.DiaLogue
{
	[DisableViewWarning]
	public abstract class UIOption : MonoBehaviour
	{
		public abstract void InitUIOption(DiaLog diaLog, Node node);

        public Option Option { get; internal set; }

		protected virtual void OnEnable() { }

		protected virtual void OnDisable()
		{
			Option = null;
		}
    }
}
