///=====================================================
/// - FileName:      IMissionCondition.cs
/// - NameSpace:     YukiFrameWork.MissionKit
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/10/14 16:01:33
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
namespace YukiFrameWork.Missions
{
	public interface IMissionCondition 
	{		
		Mission Mission { get; set; }

		void OnInit();

		/// <summary>
		/// 设定完成的条件，返回True时代表任务
		/// </summary>		
		bool Condition();		
	}
}
