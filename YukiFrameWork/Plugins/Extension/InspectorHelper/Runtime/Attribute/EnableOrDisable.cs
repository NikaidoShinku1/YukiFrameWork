///=====================================================
/// - FileName:      EnableValue.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/2 17:30:40
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    /// <summary>
    /// 设置枚举变量以判断数据是否可视化(需要传入类内对应的变量名称以及实际的枚举(可以是字符串))
    /// 注意:错传以及传递其他数据均判断为False不会进行执行 仅对派生自UnityEngine.Object的类生效
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class EnableEnumValueIfAttribute : PropertyAttribute
	{
        private object e;
        public object Enum => e;
        private string name;
		public string Name => name;	
		public EnableEnumValueIfAttribute(string name, object e)
		{
			this.name = name;
			this.e = e;
		}
	}
    /// <summary>
    /// 设置枚举变量以判断数据是否不进行可视化(需要传入类内对应的变量名称以及实际的枚举(可以是字符串))
    /// 注意:错传以及传递其他数据均判断为False不会进行执行 仅对派生自UnityEngine.Object的类生效
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class DisableEnumValueIfAttribute : PropertyAttribute
    {
        private object e;
        public object Enum => e;    
        private string name;
        public string Name => name;      

        public DisableEnumValueIfAttribute(string name, object e)
        {
            this.name = name;
            this.e = e;
        }
    }
    /// <summary>
    /// 设置枚举变量以判断数据是否可视化(需要传入类内对应的变量名称(必须是bool变量))
    /// 注意:错传以及传递其他数据均判断为False不会进行执行 仅对派生自UnityEngine.Object的类生效
    /// </summary>
    [AttributeUsage(AttributeTargets.All,AllowMultiple = true)]
    public sealed class DisableIfAttribute : PropertyAttribute
    {
        private string valueName;
        public string ValueName => valueName;
        public DisableIfAttribute(string boolValueName)
        {
            this.valueName = boolValueName;
        }
    }
    /// <summary>
    /// 设置枚举变量以判断数据是否不进行可视化(需要传入类内对应的变量名称(必须是bool变量))
    /// 注意:错传以及传递其他数据均判断为False不会进行执行 仅对派生自UnityEngine.Object的类生效
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class EnableIfAttribute : PropertyAttribute
    {
        private string valueName;
        public string ValueName => valueName;
        public EnableIfAttribute(string boolValueName)
        {
            this.valueName = boolValueName;
        }
    }
}
