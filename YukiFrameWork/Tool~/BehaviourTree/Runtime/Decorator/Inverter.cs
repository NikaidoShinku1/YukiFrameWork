///=====================================================
/// - FileName:      Inverter.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/13 13:53:51
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Behaviours
{
    public class Inverter : Decorator
    {         
        public override void OnInit()
        {
            
        }

        public override void OnStart()
        {
            child.Start();
        }

        public override BehaviourStatus OnUpdate()
        {
            if (child == null) return BehaviourStatus.Failed;
            child.Update();
            if (child.IsSuccess) return BehaviourStatus.Failed;
            else if(child.IsFailed) return BehaviourStatus.Success;

            return BehaviourStatus.Running;
        }
    }
}
