///=====================================================
/// - FileName:      EventListener.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   高级定制脚本生成
/// - Creation Time: 12/15/2025 8:49:36 PM
/// -  (C) Copyright 2008 - 2025
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using YukiFrameWork.Events;
using UnityEngine.Events;
namespace YukiFrameWork
{
    public abstract class EventListener<TEventArg> : YMonoBehaviour where TEventArg : IEventArgs
    {
        [SerializeField, LabelText("可视化事件")]
        [InfoBox("使用本组件事件本身会受到组件生命周期影响，注册时机在OnEnable，在OnDisable注销")]
        protected UnityEvent<TEventArg> onEvent;

        protected virtual void OnEnable() 
        {
            Register();
        }

        protected virtual void Register()
        {
            
        }

        public virtual void Remove()
        {
            
        }

        protected void Trigger(TEventArg arg)
        {
            onEvent?.Invoke(arg);
        }

        public virtual void SendEvent(TEventArg arg)
        {
            
        }

        protected virtual void OnDisable() 
        {
            Remove();
        }
    }
    public abstract class TypeEventListener<TEventArg> : EventListener<TEventArg> where TEventArg : IEventArgs 
    {
        protected override void Register()
        {
            base.Register();
            EventManager.AddListener<TEventArg>(Trigger);
        }

        public override void Remove()
        {
            base.Remove();
            EventManager.RemoveListener<TEventArg>(Trigger);
        }

        public override void SendEvent(TEventArg arg)
        {
            base.SendEvent(arg);
            EventManager.SendEvent(arg);
        }

    }

    public abstract class StringEventListener<TEventArg> : EventListener<TEventArg> where TEventArg : IEventArgs
    {
        [SerializeField,LabelText("事件名称")]
        private string eventName;
       
        public override void Remove()
        {
            EventManager.RemoveListener<TEventArg>(eventName, Trigger);
        }
     
        protected override void Register()
        {
            base.Register();
            EventManager.AddListener<TEventArg>(eventName, Trigger);
        }

        public override void SendEvent(TEventArg arg)
        {
            base.SendEvent(arg);
            EventManager.SendEvent(eventName,arg);
        }

    }

    public abstract class EnumEventListener<TEnum,TEventArg> : EventListener<TEventArg> where TEventArg : IEventArgs where TEnum : Enum
    {
        [SerializeField, LabelText("事件类型")]
        private TEnum eventType;
        public override void Remove()
        {
            EventManager.RemoveListener<TEventArg>(eventType, Trigger);
        }

        protected override void Register()
        {
            base.Register();
            EventManager.AddListener<TEventArg>(eventType, Trigger);
        }

        public override void SendEvent(TEventArg arg)
        {
            base.SendEvent(arg);
            EventManager.SendEvent(eventType, arg);
        }

    }
}
