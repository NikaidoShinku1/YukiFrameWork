///=====================================================
/// - FileName:      StateParameterData.cs
/// - NameSpace:     YukiFrameWork.Machine
/// - Description:   高级定制脚本生成
/// - Creation Time: 2025/3/9 13:27:36
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Machine
{
    public enum ParameterType
    {
        Float,
        Int,
        Bool,
        Trigger
    }
    [Serializable]
    public class StateParameterData 
    {
        public string parameterName;
        public BindablePropertyStruct<float> Parameter = new BindablePropertyStruct<float>((v1, v2) => 
        {
            return v1 == v2;
        });
        public ParameterType parameterType;

         


    }
}
