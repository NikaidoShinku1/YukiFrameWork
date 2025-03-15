///=====================================================
/// - FileName:      IParameterCompare.cs
/// - NameSpace:     YukiFrameWork.Example
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/7 13:58:57
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Machine
{
    /// <summary>
    /// 参数比较器
    /// </summary>
    public interface IParameterCompare 
    {

        bool IsCondition(StateParameterData parameterData,float v);

    }
}
