///=====================================================
/// - FileName:      OnMouseUpAsButtonEvent.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/1 20:07:28
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    public class OnMouseUpAsButtonEvent : MouseAPI
    {
        private void OnMouseUpAsButton()
        {
            onEvent?.SendEvent();
        }
    }

    public static class OnMouseUpAsButtonExtension
    {
        public static IUnRegister BindMouseUpAsButton(this GameObject core,Action onEvent)
            => core.GetOrAddComponent<OnMouseUpAsButtonEvent>().Register(onEvent);

        public static IUnRegister BindMouseUpAsButton<T>(this T core, Action onEvent) where T : Component
           => core.GetOrAddComponent<OnMouseUpAsButtonEvent>().Register(onEvent);
    }
}
