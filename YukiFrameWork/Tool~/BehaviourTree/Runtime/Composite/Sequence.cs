///=====================================================
/// - FileName:      Sequence.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/13 12:10:00
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
namespace YukiFrameWork.Behaviours
{
    public class Sequence : Composite
    {
        [SerializeField,HideInInspector,JsonIgnore]
        protected AIBehaviour currentChild;
        protected int currentIndex;     
        public override void OnInit()
        {
           
        }
        public override void ForEach(Action<AIBehaviour> each)
        {
            foreach (var item in childs)
                each?.Invoke(item);
        }
        public override void OnStart()
        {
            currentIndex = -1;
            Next();  
        }
      
       
        public override BehaviourStatus OnUpdate()
        {         
            if (CanExecute())
            {
                currentChild.Update();            
                BehaviourStatus status = currentChild.Status;
                if (status == BehaviourStatus.Failed)
                {                   
                    return BehaviourStatus.Failed;
                }

                if (status == BehaviourStatus.Success)
                    Next();

                return BehaviourStatus.Running;
            }
            else
            {
                if (currentIndex >= childs.Count)
                    return BehaviourStatus.Success;
                return BehaviourStatus.Failed;
            }
        }

        protected internal override void Self()
        {           
            for(int i = 0;i < currentIndex; i++)
            {
                var child = childs[i];
                if (child.Status == BehaviourStatus.Running) continue;

                if (child.OnUpdate() == BehaviourStatus.Failed)
                {
                    currentChild.OnInterruption();
                    currentChild.ResetBehaviour();                 
                    child.Status = BehaviourStatus.Failed;
                    Status = BehaviourStatus.Failed;
                    currentIndex = childs.IndexOf(child);
                    currentChild = child;
                    break;
                }
            }
        }

        protected internal override void LowerPriority()
        {
            base.LowerPriority();

            if (Parent is Composite composite)
                composite.Self();
        }

        protected override void OnCondtionAbort(int childIndex)
        {
            if (currentChild)
            {
                currentChild.ResetBehaviour();
            }
            currentIndex = childIndex - 1;
            Next();            
        }
        public override void OnFixedUpdate()
        {
            if (currentChild != null && currentChild.Status == BehaviourStatus.Running)
                currentChild.OnFixedUpdate();
        }

        public override void OnLateUpdate()
        {
            if (currentChild != null && currentChild.Status == BehaviourStatus.Running)
                currentChild.OnLateUpdate();
        }

        protected virtual void Next()
        {          
            currentIndex++;
            if (CanExecute())
            {
                currentChild = childs[currentIndex];
                currentChild.Start();
            }
        }

        protected override bool CanExecute()
        {
            return currentIndex < childs.Count && Status != BehaviourStatus.InActive;
        }
    }
}
