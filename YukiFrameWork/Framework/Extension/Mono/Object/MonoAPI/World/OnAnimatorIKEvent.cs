///=====================================================
/// - FileName:      OnAnimatorIK.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/4/1 20:04:02
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using YukiFrameWork;
using UnityEngine;
using System;
namespace YukiFrameWork
{
	public class OnAnimatorIKEvent : MonoAPI<int>
	{
        private void OnAnimatorIK(int layerIndex)
        {
            onEvent?.SendEvent(layerIndex);
        }
    }

    public static class OnAnimatorIKExtension
    {
        public static IUnRegister BindAnimatorIKEvent<T>(this T core,Action<int> layerEvent) where T : Component
        {
            return core.GetOrAddComponent<OnAnimatorIKEvent>().Register(layerEvent);
        }

        public static IUnRegister BindAnimatorIKEvent(this GameObject core, Action<int> layerEvent)
        {
            return core.GetOrAddComponent<OnAnimatorIKEvent>().Register(layerEvent);
        }
    }
}
