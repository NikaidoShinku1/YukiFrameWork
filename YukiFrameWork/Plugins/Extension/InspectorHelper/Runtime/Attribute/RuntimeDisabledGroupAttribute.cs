///=====================================================
/// - FileName:      RuntimeDisabledGroupAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/5 23:31:11
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    /// <summary>
    /// 运行时禁止在Inspector修改，对于数组/列表，仅在标记了Serializable的类中可以使用，对于派生自UnityEngine.Object的类，数组/列表应该使用ListDrawingSetting自定义
    /// </summary>
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
	public class RuntimeDisabledGroupAttribute : PropertyAttribute
	{
		
	}
    /// <summary>
    /// 编辑器模式时禁止在Inspector修改，对于数组/列表，仅在标记了Serializable的类中可以使用，对于派生自UnityEngine.Object的类，数组/列表应该使用ListDrawingSetting自定义
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class EditorDisabledGroupAttribute : PropertyAttribute
    {

    }
    /// <summary>
    /// 当传入的bool变量为True时禁用，对于数组/列表，仅在标记了Serializable的类中可以使用，对于派生自UnityEngine.Object的类，数组/列表应该使用ListDrawingSetting自定义
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class DisableGroupIfAttribute : PropertyAttribute
	{
        private string valueName;
        public string ValueName => valueName;
        /// <summary>
        /// 为该特性传入一个bool变量
        /// </summary>
        /// <param name="boolValueName">变量名称</param>
        public DisableGroupIfAttribute(string boolValueName)
        {                      
            this.valueName = boolValueName;
        }
    }
    /// <summary>
    /// 当传入的枚举变量/字符串与变量名称相同时禁用，对于数组/列表，仅在标记了Serializable的类中可以使用，对于派生自UnityEngine.Object的类，数组/列表应该使用ListDrawingSetting自定义
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Method | AttributeTargets.Property)]
    public class DisableGroupEnumValueIfAttribute : PropertyAttribute
    {
        private object e;
        public object Enum => e;
        private string name;
        public string Name => name;

        public DisableGroupEnumValueIfAttribute(string name, object e)
        {
            this.name = name;
            this.e = e;
        }
    }
}
