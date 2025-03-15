///=====================================================
/// - FileName:      StateConditionData.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/9 13:27:00
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Machine
{
    public enum CompareType
    {
        Greater = 0,
        Less,
        Equal,
        NotEqual,
    }
    [Serializable]
    public class StateConditionData 
    {

        public float targetValue;

        public string parameterName;

        public CompareType compareType;     
    }
}
