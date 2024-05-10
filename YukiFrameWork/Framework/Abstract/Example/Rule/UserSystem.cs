///=====================================================
/// - FileName:      UserSystem.cs
/// - NameSpace:     YukiFrameWork.ExampleRule.ExampleFrameWork
/// - Description:   示例系统，非必要不可修改，仅供参考
/// - Creation Time: 2024/5/10 16:33:06
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.ExampleRule.ExampleFrameWork
{
    public class UserSystem : AbstractSystem
    {
        public override void Init()
        {
            this.RegisterEvent<EventArgs>(Example.UserEventKey, _ => 
            {
                var model = this.GetModel<UserModel>();
                Debug.Log($"这是System注册的事件 --- Model --- Name:{model.Name} Age:{model.Age}");        
            });
        }
    }
}
