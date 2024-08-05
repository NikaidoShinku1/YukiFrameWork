///=====================================================
/// - FileName:      EventSystemCore.cs
/// - NameSpace:     YukiFrameWork
/// - Created:       Yuki
/// - Email:         Yuki@qq.com
/// - Description:   事件系统本体
/// -  (C) Copyright 2008 - 2023,Yuki
/// -  All Rights Reserved.
///======================================================

using UnityEngine;
using System;
using System.Reflection;
namespace YukiFrameWork
{
    public interface IEasyEvent : IUnRegister
    {

    }

    public interface IEventArgs
    {
        
    }

    public interface IUnRegister
    {
        void UnRegisterAllEvent();
    }

    public interface IDynamicEvent : IEasyEvent 
    {
        IUnRegister RegisterEvent_Dynamic(MethodInfo methodInfo, object target);
        void UnRegisterEvent_Dynamic(MethodInfo methodInfo);    
    }  
}