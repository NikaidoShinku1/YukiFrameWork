///=====================================================
/// - FileName:      Condition.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/13 11:55:12
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
    
    [ClassAPI("条件节点")]
    [ChildModeInfo(ChildMode = ChildMode.None)]
	public abstract class Condition : AIBehaviour
	{
        public sealed override void AddChild(AIBehaviour behaviour)
        {
            
        }
        public sealed override void RemoveChild(AIBehaviour behaviour)
        {
            
        }
      
        public sealed override void Clear()
        {
            
        }

        public sealed override void ForEach(Action<AIBehaviour> each)
        {
            
        }
    }
}
