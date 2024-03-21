///=====================================================
/// - FileName:      HelperBoxAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/3 16:26:25
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using UnityEditor;

namespace YukiFrameWork
{
	public enum Message
	{       
        //
        // 摘要:
        //     Info message.
        Info,
        //
        // 摘要:
        //     Warning message.
        Warning,
        //
        // 摘要:
        //     Error message.
        Error
    }

    /// <summary>
    /// 用于在方法或者字段属性上显示提示信息
    /// </summary>
	[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Class | AttributeTargets.Property)]
	public class HelperBoxAttribute : PropertyAttribute
	{
        private Message message;
        public Message Message => message;
        private string info;
        public string Info => info;

        public string boolValueName;
		public HelperBoxAttribute(string info,Message message = Message.Info) 
		{
            this.message = message;
            this.info = info;          
		}
	}
}