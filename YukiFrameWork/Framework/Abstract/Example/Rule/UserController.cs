///=====================================================
/// - FileName:      UserController.cs
/// - NameSpace:     YukiFrameWork.ExampleRule.ExampleFrameWork
/// - Description:   示例控制器，非必要不可修改，仅供参考
/// - Creation Time: 2024/5/10 16:33:30
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.ExampleRule.ExampleFrameWork
{
    [RuntimeInitializeOnArchitecture(typeof(User), true)]
    public class UserController : ViewController
    {
        internal void SendCommand()
        {
            this.SendCommand(new UserCommand());
        }

        internal void GetUserModel()
        {
            Debug.Log(this.GetModel<UserModel>());
        }

        internal void GetUserSystem()
        {
            Debug.Log(this.GetSystem<UserSystem>());
        }

        internal void GetUserUtility()
        {
            this.GetUtility<UserUtility>().DebugInfo();
        }
    }
}
