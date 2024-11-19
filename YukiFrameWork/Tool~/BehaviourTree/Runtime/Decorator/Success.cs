///=====================================================
/// - FileName:      Success.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/13 13:57:15
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Behaviours
{
	public class Success : Decorator
	{      
      
        public override void OnInit()
        {

        }

        public override void OnStart()
        {

        }

        public override BehaviourStatus OnUpdate()
        {
            return BehaviourStatus.Success;
        }
    }
}
