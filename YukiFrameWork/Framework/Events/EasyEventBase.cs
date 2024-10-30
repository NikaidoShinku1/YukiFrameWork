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
using YukiFrameWork.Events;
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
#if UNITY_2022_1_OR_NEWER
        EventRegisterType IUnRegister.RegisterType { get; set; }
#endif
        public abstract IUnRegister RegisterEvent(T onEvent);
        public abstract void UnRegister(T onEvent);
        public virtual void UnRegisterAllEvent() => OnEasyEvent = null;  
    }
    public abstract class ListenerAttribute : Attribute
    {
        public bool OnlyMonoEnable = false;
        public ListenerAttribute(bool onlyMonoEnable)
        {
            OnlyMonoEnable = onlyMonoEnable;
        }
    }

    /*/// <summary>
    /// 自动化事件注册，为参数类型派生自IEventArgs且是唯一参数的方法标记该特性，可取代EventManager.AddListener的调用，进行自动注册的效果
    /// <para>特性参数:bool onlyMonoEnable ---> 是否开启Mono激活检测，开启后，当组件/对象失活时，无法进行指定参数的调用</para>
    /// <para>使用方式: 当你有一个继承IEventArgs的参数类MyCustomArg</para>
    /// [AddListener]
    /// <para>public void Test(MyCustomArg arg)</para>
    /// {
    /// <para>...</para>
    /// <para>}</para>
    /// Tip:使用该特性注册的事件，不建议Awake中发送事件，当对象直接置于场景时，Awake的周期可能会导致事件发送失败。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class AddListenerAttribute : ListenerAttribute
    {       
        public AddListenerAttribute(bool onlyMonoEnable = false) : base(onlyMonoEnable)
        {
        }
    }

    /// <summary>
    /// 自动化事件注销，为任意一个无参方法标记该特性。那么当该方法在运行时被调用后，会自动同步对该特性所标记的已经注册的指定类型的事件进行清空
    /// ，如需单独注销某一个事件请手动使用EventManager.RemoveListener方法。
    /// <para>特性参数:params Type[] argTypes ---> 传递指定继承IEventArgs的类型，在指定方法被调用后，会同时处理传递类型的事件清空</para>
    /// <para>使用方式: 当你有一个继承IEventArgs的参数类MyCustomArg</para>
    /// [RemoveAllListeners(typeof(MyCustomArg))]
    /// <para>public void OnDestroy()</para>
    /// {
    /// <para>///假设这个是MonoBehaviour的销毁方法，那么当生命周期执行OnDestroy时，会自动进行事件注销，无需手动的代码编写</para>
    /// <para>}</para>   
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class RemoveAllListenersAttribute : Attribute
    {
        internal Type[] argTypes;
        public RemoveAllListenersAttribute(params Type[] argTypes)
        {
            this.argTypes = argTypes;
        }
    }*/
}
