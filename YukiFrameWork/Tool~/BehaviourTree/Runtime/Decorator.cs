///=====================================================
/// - FileName:      Decorator.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/13 11:49:31
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using YukiFrameWork.Extension;
using System.Collections.Generic;
namespace YukiFrameWork.Behaviours
{
    /// <summary>
    /// 行为装饰器
    /// </summary>
    [ChildModeInfo(ChildMode = ChildMode.Single)]
    [ClassAPI("装饰节点")]
	public abstract class Decorator : AIBehaviour
	{
        [field: SerializeField,HideInInspector]
        public AIBehaviour child;
        public sealed override void AddChild(AIBehaviour behaviour)
        {
            if (behaviour == null) return;
            if (behaviour.Parent != null)
                behaviour.Parent.RemoveChild(behaviour);
            behaviour.Parent = this;
            this.child = behaviour;
        }

        public override void OnStart()
        {
            child.Start();
        }

        public sealed override void ForEach(Action<AIBehaviour> each)
        {
            each?.Invoke(child);
        }
       
        public sealed override void RemoveChild(AIBehaviour behaviour)
        {
            if (behaviour != child) return;
            if (behaviour)
                behaviour.Parent = null;
            child = null;
        }        
        public sealed override void Clear()
        {
            RemoveChild(child);
        }

        public override BehaviourStatus OnUpdate()
        {
            if (!child) return BehaviourStatus.Failed;

            child.Update();
            return child.Status;                   
        }

        public override void OnFixedUpdate()
        {
            if (child != null && child.Status == BehaviourStatus.Running)
                child.OnFixedUpdate();
        }
        public override void OnLateUpdate()
        {
            if (child != null && child.Status == BehaviourStatus.Running)
                child.OnLateUpdate();
        }
    }
}
