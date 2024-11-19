///=====================================================
/// - FileName:      BehaviourParamView.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/19 13:58:30
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Behaviours
{
	[Serializable]
	public class BehaviourParamView
	{
		public string paramName;
		public string fieldName;
		public BehaviourParamType paramType;
		public int selectIndex;
		
	}
}
