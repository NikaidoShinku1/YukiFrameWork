///=====================================================
/// - FileName:      Selector.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/13 13:47:44
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork.Behaviours
{
    /// <summary>
    /// 选择器
    /// </summary>
	public class Selector : Sequence
	{
        public override BehaviourStatus OnUpdate()
        {          
            if (CanExecute())
            {             
                currentChild.Update();
                BehaviourStatus status = currentChild.Status;              
                if (status == BehaviourStatus.Success)
                    return BehaviourStatus.Success;             
                if (status == BehaviourStatus.Failed)
                    Next();              
            }
            return Status;
        }

        protected internal override void Self()
        {           
            for (int i = 0; i < currentIndex; i++)
            {
                var child = childs[i];
                if (child.Status == BehaviourStatus.Running) continue;
                if (child.Status == BehaviourStatus.Failed)
                {
                    var status = child.OnUpdate();
                    if (status == BehaviourStatus.Success)
                    {
                        Status = BehaviourStatus.Success;
                        child.Status = BehaviourStatus.Success;
                        currentChild.OnInterruption();
                        currentChild.ResetBehaviour();
                        OnStart();
                        break;
                    }
                    else if (status == BehaviourStatus.Running)
                    {                      
                        currentIndex = childs.IndexOf(child);
                        currentChild.OnInterruption();
                        currentChild.ResetBehaviour();
                        currentChild = child;
                        Status = BehaviourStatus.Running;
                        break;
                    }
                }
            }
        }
    }
}
