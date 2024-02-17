/****************************************************************************
 * Copyright (c) 2016 - 2023 liangxiegame UNDER MIT License
 * 
 * https://YukiFrameWork.cn
 * https://github.com/liangxiegame/YukiFrameWork
 * https://gitee.com/liangxiegame/YukiFrameWork
 ****************************************************************************/

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindCancelEventTrigger: MonoAPI<BaseEventData>, ICancelHandler
    {
      
        public void OnCancel(BaseEventData eventData)
        {
            onEvent.SendEvent(eventData);
        }
    }

    public static class BindCancelEventTriggerExtension
    {
        public static IUnRegister BindCancelEvent<T>(this T self, Action<BaseEventData> onCancel)
            where T : Component
        {
            return self.GetOrAddComponent<BindCancelEventTrigger>().Register(onCancel);
        }
        
        public static IUnRegister BindCancelEvent(this GameObject self, Action<BaseEventData> onCancel)
        {
            return self.GetOrAddComponent<BindCancelEventTrigger>().Register(onCancel);
        }
    }
}