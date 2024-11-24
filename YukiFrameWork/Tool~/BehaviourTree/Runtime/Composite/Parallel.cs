///=====================================================
/// - FileName:      Parallel.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/13 12:48:25
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
namespace YukiFrameWork.Behaviours
{
    public enum ParallelMode
    {
        [LabelText("任意子节点成功则成功")]
        FirstSuccess,
        [LabelText("任意子节点失败则失败")]
        FirstFailed,
    }
    public class Parallel : Composite
    {
        [field:SerializeField]
        [JsonProperty]   
        public ParallelMode Mode { get; internal set; }       

        public sealed override void ForEach(Action<AIBehaviour> each)
        {
            foreach (var item in childs)
                each?.Invoke(item);
        }

        public override void OnInit()
        {
            
        }       
        public override void OnStart()
        {          
            for (int i = 0; i < childs.Count; i++)
            {
                childs[i].Start();
            }
        }
      
        protected override bool CanExecute()
        {
            return Status != BehaviourStatus.InActive; 
        }

        protected internal override void Self()
        {
            base.Self();
            switch (Mode)
            {
                case ParallelMode.FirstSuccess:
                    for (int i = 0; i < childs.Count; i++)
                    {
                        if (childs[i].Status == BehaviourStatus.Running) continue;
                        if (childs[i].Status == BehaviourStatus.Failed)
                        {
                            if (childs[i].OnUpdate() == BehaviourStatus.Success)
                            {
                                childs[i].Status = BehaviourStatus.Success;
                                for (int j = 0; j < childs.Count; j++)
                                {
                                    if (childs[j] == childs[i]) continue;
                                    childs[j].OnInterruption();
                                    childs[j].ResetBehaviour();
                                }
                                Status = BehaviourStatus.Success;
                            }
                        }
                    }
                    break;
                case ParallelMode.FirstFailed:
                    for (int i = 0; i < childs.Count; i++)
                    {
                        if (childs[i].Status == BehaviourStatus.Running) continue;
                        if (childs[i].Status == BehaviourStatus.Success)
                        {
                            if (childs[i].OnUpdate() == BehaviourStatus.Failed)
                            {
                                childs[i].Status = BehaviourStatus.Failed;
                                for (int j = 0; j < childs.Count; j++)
                                {
                                    if (childs[j] == childs[i]) continue;
                                    childs[j].OnInterruption();
                                    childs[j].ResetBehaviour();
                                }
                                Status = BehaviourStatus.Failed;
                            }
                        }
                    }
                    break;              
            }
        }

        protected internal override void LowerPriority()
        {
            base.LowerPriority();
            if (Parent is Composite composite)
                composite.Self();
        }

        public override BehaviourStatus OnUpdate()
        {     
            if (childs.Count == 0)
            {                
                return BehaviourStatus.Success;
            }          
            if (CanExecute())
            {
                for (int i = 0; i < childs.Count; i++)
                {
                    if (childs[i].Status != BehaviourStatus.Running) continue;
                    childs[i].Update();
                    BehaviourStatus status = childs[i].Status;
                    switch (Mode)
                    {
                        case ParallelMode.FirstSuccess:
                            if (status == BehaviourStatus.Success)
                                return BehaviourStatus.Success;
                            break;
                        case ParallelMode.FirstFailed:
                            if (status == BehaviourStatus.Failed)
                                return BehaviourStatus.Failed;
                            break;
                    }
                }
            }          
            return Status;
            
        }     
        public override void OnFixedUpdate()
        {
            for (int i = 0; i < childs.Count; i++)
            {
                if (childs[i].Status == BehaviourStatus.Running)
                    childs[i].OnFixedUpdate();
            }
        }

        public override void OnLateUpdate()
        {
            for (int i = 0; i < childs.Count; i++)
            {
                if (childs[i].Status == BehaviourStatus.Running)
                    childs[i].OnLateUpdate();
            }
        }        
    }
}
