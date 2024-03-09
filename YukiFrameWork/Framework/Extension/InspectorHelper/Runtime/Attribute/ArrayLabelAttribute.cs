///=====================================================
/// - FileName:      ArrayLabelAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/4 19:24:01
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    /// <summary>
    /// 可以自定义对于数组/列表元素的名称
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field,AllowMultiple = false)]
    public sealed class ArrayLabelAttribute : PropertyAttribute
    {
        private string label;

        public string Label => label;
        public ArrayLabelAttribute(string label)
        {
            this.label = label;
        }
    }
}
