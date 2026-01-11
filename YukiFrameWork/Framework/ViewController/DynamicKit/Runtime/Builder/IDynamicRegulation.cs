///=====================================================
/// - FileName:      DynamicSingleManager.cs
/// - NameSpace:     YukiFrameWork.Dynamic
/// - Description:   高级定制脚本生成
/// - Creation Time: 1/6/2026 1:00:12 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    /// <summary>
    /// 动态参数构造器。实现该接口的类型传递给DynamicParameter特性
    /// </summary>
    public interface IDynamicRegulation 
    {
        object Build(Type parameterType,IDynamicMonoBehaviour builder);
    }
}
