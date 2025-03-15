///=====================================================
/// - FileName:      User.cs
/// - NameSpace:     YukiFrameWork.ExampleRule.ExampleFrameWork
/// - Description:   示例架构，非必要不可修改，仅供参考
/// - Creation Time: 2024/5/10 16:33:49
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.ExampleRule.ExampleFrameWork
{
    /// <summary>
    /// 架构类，创建层级规则必不可少的核心类，架构的OnInit方法会在架构准备完成后运行。
    /// </summary>
    internal class User : Architecture<User>
    {
        public override void OnInit()
        {
            //架构准备完成后会执行的架构初始化方法                    
        }
    }
}
