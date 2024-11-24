///=====================================================
/// - FileName:      Running.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/24 13:47:17
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Behaviours
{
	public class Running : Decorator
	{
        public override void OnStart()
        {
            
        }
        public override BehaviourStatus OnUpdate()
        {
            return BehaviourStatus.Running;
        }
    }
}
