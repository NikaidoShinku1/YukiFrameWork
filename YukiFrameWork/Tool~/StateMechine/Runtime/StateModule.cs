///=====================================================
/// - FileName:      StateModule.cs
/// - NameSpace:     YukiFrameWork.States
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/2/13 16:54:28
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Extension;

namespace YukiFrameWork.States
{
    [NoGenericArchitecture]
    public class StateModule : Architecture<StateModule>
    {
        public override void OnInit()
        {            
            this.RegisterSystem(new StateMechineSystem());
        }
    }
}
