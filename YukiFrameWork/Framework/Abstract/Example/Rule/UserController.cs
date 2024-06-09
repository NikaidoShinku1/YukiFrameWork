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
using System.Collections;
namespace YukiFrameWork.ExampleRule.ExampleFrameWork
{
    [RuntimeInitializeOnArchitecture(typeof(User), true)]
    public class UserController : ViewController
    {
        protected override void Awake()
        {
            base.Awake();           

        }

        private IEnumerator Start()
        {
            ///准备架构模块

            ArchitectureStartUpRequest request = ArchitectureStartUpRequest.StartUpArchitecture<User>();

            while (!request.isDone)
            {
                yield return null;
                LogKit.I("正在准备架构模块:" + request.progress);  
            }

            if (request.error.IsNullOrEmpty())
            {
                LogKit.I("架构准备成功，执行所有层级的初始化以及调用架构的OnInit方法");
            }
            else
            {
                LogKit.I("架构准备失败，请重试!");
            }
        }
    }
}
