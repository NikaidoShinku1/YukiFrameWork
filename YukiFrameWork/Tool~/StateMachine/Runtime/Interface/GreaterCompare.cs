///=====================================================
/// - FileName:      GreaterCompare.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/12 18:19:07
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Machine
{
    public class GreaterCompare : IParameterCompare
    {
        public bool IsCondition(StateParameterData parameterData, float v)
        {
            if (parameterData == null)
                return false;
            return parameterData.Parameter.Value > v;
        }
    }
}
