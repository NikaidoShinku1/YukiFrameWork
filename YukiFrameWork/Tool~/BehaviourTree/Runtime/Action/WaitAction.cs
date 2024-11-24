///=====================================================
/// - FileName:      WaitAction.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/16 18:19:27
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Behaviours
{
	public class WaitAction : Action
	{
        public override void OnStart()
        {
            base.OnStart();
            timer = 0;       
        }
        public float time = 5;
        private float timer = 0;  
        public override BehaviourStatus OnUpdate() 
        {
            base.OnUpdate();           
            timer += Time.deltaTime;           
            return timer > time ? BehaviourStatus.Success : BehaviourStatus.Running;
        }

        public override void OnSuccess()
        {
            base.OnSuccess();
            timer = 0;
        }

        public override void OnFailed()
        {
            base.OnFailed();
            timer = 0;
        }
    }
}
