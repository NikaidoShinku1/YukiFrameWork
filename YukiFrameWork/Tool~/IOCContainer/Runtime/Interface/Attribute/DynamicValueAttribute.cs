///=====================================================
/// - FileName:      DynamicValueAttribute.cs
/// - NameSpace:     YukiFrameWork.IOC
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/2/23 21:28:42
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.IOC
{
    /// <summary>
    /// 动态值标记,在容器中注册的类可以给其中参数给予此特性标记，标记后可以在容器解析出IEntryPoint时不进行Value获取而直接注入参数。
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
	public class DynamicValueAttribute : Attribute
	{
        /// public class A
        /// {
        ///		[DynamicValue("数字1")]
        ///		public float number;
        /// }	
        /// 
        /// IEntryPoint point = Container.ResolveEntry(typeof(A));
        /// point.Inject(10,"数字1");
        private string labelName;

		public string LabelName => labelName;

        /// <summary>
        /// 传递标识名称，注入时精确
        /// </summary>
        /// <param name="labelName"></param>
		public DynamicValueAttribute(string labelName) => this.labelName = labelName;	

        /// <summary>
        /// 默认查找同类型参数进行注入
        /// </summary>
		public DynamicValueAttribute() => labelName = string.Empty;
	}
}
