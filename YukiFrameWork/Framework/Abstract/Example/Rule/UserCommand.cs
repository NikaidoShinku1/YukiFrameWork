///=====================================================
/// - FileName:      UserCommand.cs
/// - NameSpace:     YukiFrameWork.ExampleRule.ExampleFrameWork
/// - Description:   示例命令，非必要不可修改，仅供参考
/// - Creation Time: 2024/5/10 16:33:18
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.ExampleRule.ExampleFrameWork
{
    public class UserCommand : AbstractCommand
    {
        public override void Execute()
        {
            var model = this.GetModel<UserModel>();
            if (model == null) return;
            Debug.Log("这是命令触发的对Model的数据修改 Model.Age:" + ++model.Age);
        }
    }
}
