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
namespace YukiFrameWork
{
    public abstract class EasyEventBase<T> : IEasyEvent where T : Delegate
    {
        protected T OnEasyEvent;
        public abstract IUnRegister RegisterEvent(T onEvent);
        public abstract void UnRegister(T onEvent);
        public void UnRegisterAllEvent() => OnEasyEvent = null;  
    }
}
