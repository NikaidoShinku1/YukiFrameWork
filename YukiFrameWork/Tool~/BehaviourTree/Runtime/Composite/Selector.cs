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
                Debug.Log( currentChild.Description+status);
                if (status == BehaviourStatus.Success)
                    return BehaviourStatus.Success;

                if (status == BehaviourStatus.Failed)
                    Next();              
            }
            return Status;
        }

        protected internal override void Self()
        {
            if (Status != BehaviourStatus.Running) return;
            for (int i = 0; i < currentIndex; i++)
            {
                var child = childs[i];
                if (child.Status == BehaviourStatus.Failed)
                {
                    if (child.OnUpdate() == BehaviourStatus.Success)
                    {
                        Status = BehaviourStatus.Success;
                        child.Status = BehaviourStatus.Success;
                        currentChild.ResetBehaviour();
                        break;
                    }
                }
            }
        }
    }
}
