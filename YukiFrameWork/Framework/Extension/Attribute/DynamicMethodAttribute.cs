///=====================================================
/// - FileName:      DynamicMethodAttribute.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         1274672030@qq.com
/// - Description:   动态方法特性
/// - Creation Time: 2023/12/23 17:40:13
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
namespace YukiFrameWork
{
    /// <summary>
    /// 动态
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class DynamicMethodAttribute : Attribute
    {
       
    }
}