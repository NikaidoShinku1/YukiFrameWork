///=====================================================
/// - FileName:      EasyEventBase.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   普通事件基类
/// - Creation Time: 2024/3/22 14:11:35
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
using System.Collections.Generic;
namespace YukiFrameWork
{ 
    //注销标识类型
    public enum UnRegisterType
    {
        None,
        OnDisable,
        OnDestroy
    }
    public abstract class EasyEventBase<T> : IEasyEvent where T : Delegate
    {     
        protected T OnEasyEvent;       
        public abstract IUnRegister RegisterEvent(T onEvent);
        public abstract void UnRegister(T onEvent);
        public virtual void UnRegisterAllEvent() => OnEasyEvent = null;  
    }

    /// <summary>
    /// 自动注册特性，所有层级在内，标记该特性且有所属架构时，会自动将标记的方法全部注册,仅支持单参数TypeEventSystem
    /// <para>事件的注册类型为IEasyArgs,可通过自行定义结构体/类继承该接口后使用，调用也必须是IEasyArgs类型，通过架构规则ISendEvent接口下拓展的的this.SendEvent方法调用</para>
    /// <para>仅能使用于同步且返回值为Void的事件注册</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RegisterEventAttribute : BaseRegisterEvent
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="unRegisterType">如果是ViewController，可设置注销生命周期</param>
        public RegisterEventAttribute(UnRegisterType unRegisterType = UnRegisterType.None) : base(unRegisterType)
        {

        }
    }

    /// <summary>
    /// 自动注册特性，所有层级在内，标记该特性且有所属架构时，会自动将标记的方法全部注册,仅支持单参数StringEventSystem
    /// <para>事件的注册类型为IEasyArgs,可通过自行定义结构体/类继承该接口后使用，调用也必须是IEasyArgs类型，通过架构规则ISendEvent接口下拓展的的this.SendEvent方法调用</para>
    /// <para>仅能使用于同步且返回值为Void的事件注册</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class StringRegisterEventAttribute : BaseRegisterEvent  
    {
        internal string eventName;
        /// <summary>
        /// 自定义事件标识
        /// </summary>
        /// <param name="name"></param>
        /// /// <param name="unRegisterType">如果是ViewController，可设置注销生命周期</param>
        public StringRegisterEventAttribute(string name, UnRegisterType unRegisterType = UnRegisterType.None) : base(unRegisterType)
        {
            eventName = name;
        }
        /// <summary>
        /// 仅用方法名作为标识
        /// </summary>
        public StringRegisterEventAttribute(UnRegisterType unRegisterType = UnRegisterType.None) : base(unRegisterType){ }
    }

    /// <summary>
    /// 自动注册特性，所有层级在内，标记该特性且有所属架构时，会自动将标记的方法全部注册,仅支持单参数EnumEventSystem
    /// <para>事件的注册类型为IEasyArgs,可通过自行定义结构体/类继承该接口后使用，调用也必须是IEasyArgs类型，通过架构规则ISendEvent接口下拓展的的this.SendEvent方法调用</para>
    /// <para>仅能使用于同步且返回值为Void的事件注册</para>    
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EnumRegisterEventAttribute : BaseRegisterEvent
    {
        internal int enumId;
        internal Type enumType;
        /// <summary>
        /// 枚举自动特性必须单独传递枚举类型以及对应的枚举int值。
        /// </summary>
        /// <param name="enumId"></param>
        /// <param name="type"></param>
        /// <param name="unRegisterType">如果是ViewController，可设置注销生命周期</param>
        public EnumRegisterEventAttribute(int enumId,Type type, UnRegisterType unRegisterType = UnRegisterType.None) : base(unRegisterType)
        {
            this.enumId = enumId;
            this.enumType = type;
        }
    }

    public abstract class BaseRegisterEvent : Attribute
    {
        internal UnRegisterType unRegisterType = UnRegisterType.None;

        public BaseRegisterEvent(UnRegisterType unRegisterType = UnRegisterType.None) 
        {
            this.unRegisterType = unRegisterType;
        }
    }
}
