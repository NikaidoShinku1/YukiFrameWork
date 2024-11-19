///=====================================================
/// - FileName:      LogAction.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/16 18:19:36
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Behaviours
{
	public enum LogLevel
	{
		Info,
		Warning,
		Error
	}
	public class LogAction : Action
	{
		public LogLevel logLevel;

        public override void OnStart()
        {
            base.OnStart();
			string info = BehaviourTree.Params["log"].StringValue;
			switch (logLevel)
			{
				case LogLevel.Info:
					Debug.Log(info);
					break;
				case LogLevel.Warning:
					Debug.LogWarning(info);
					break;
				case LogLevel.Error:
					Debug.LogError(info);
					break;
				
			}
		}
    }
}
