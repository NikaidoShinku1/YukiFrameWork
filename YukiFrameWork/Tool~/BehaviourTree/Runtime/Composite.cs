///=====================================================
/// - FileName:      Composite.cs
/// - NameSpace:     YukiFrameWork.Behaviours
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/11/13 11:43:47
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using YukiFrameWork.Extension;
using Newtonsoft.Json;
using Sirenix.OdinInspector;
using Newtonsoft.Json.Converters;
namespace YukiFrameWork.Behaviours
{
    public enum AbortType
    {
        [LabelText("不进行任何打断")]
        None,
        [LabelText("打断自身的直接子节点")]
        Self,
        [LabelText("打断同级低优先级节点")]
        LowerPriority,
        [LabelText("同时打断自身与同级低优先级节点")]
        Both
    }
    /// <summary>
    /// 复合行为
    /// </summary>
    [ChildModeInfo(ChildMode = ChildMode.Multiple)]
    [ClassAPI("复合节点")]
	public abstract class Composite : AIBehaviour
	{
        [SerializeField,HideInInspector]
        [JsonIgnore] public List<AIBehaviour> childs = new List<AIBehaviour>();

        [LabelText("打断类型")]
        [field:SerializeField]
        [JsonProperty]
        public AbortType AbortType { get; set; }
        public sealed override void AddChild(AIBehaviour behaviour)
        {
            if (behaviour == null) return;
            if (behaviour.Parent != null)
                behaviour.Parent.RemoveChild(behaviour);
            behaviour.Parent = this;
			childs.Add(behaviour);
        }      
        public override void OnInit()
        {
            base.OnInit();            
        }
        internal override void OnInternalUpdate()
        {
            base.OnInternalUpdate();
            switch (AbortType)
            {
                case AbortType.None:
                    break;
                case AbortType.Self:
                    Self();
                    break;
                case AbortType.LowerPriority:
                    LowerPriority();
                    break;
                case AbortType.Both:
                    Self();
                    LowerPriority();
                    break;
            }
        }

        protected internal virtual void Self()
        { }

        protected internal virtual void LowerPriority() 
        { }

        protected virtual void OnCondtionAbort(int childIndex) { }

        internal override void ReLoadChild()
        {
            if (Application.isPlaying) return;
            base.ReLoadChild();
           
            if (childs != null && childs.Count > 0)
            {
                childs.Sort((a, b) =>
                {
                    if (Math.Abs(a.position.x - b.position.x) > float.Epsilon)
                    {
                        return a.position.x.CompareTo(b.position.x);
                    }

                    return a.ID.CompareTo(b.ID);
                });
            }
            
        }    
        public sealed override void RemoveChild(AIBehaviour behaviour)
        {
            if (behaviour == null) return;
            behaviour.Parent = null;
            childs.Remove(behaviour);
        }        
        public sealed override void Clear()
        {
        
            for (int i = childs.Count - 1; i >= 0; i--)
            {
                RemoveChild(childs[i]);
            }
        }

        protected virtual bool CanExecute() { return true; }

    }
}
