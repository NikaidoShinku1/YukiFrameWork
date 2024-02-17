
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindSelectEventTrigger: MonoAPI<BaseEventData>, ISelectHandler
    {     
        public void OnSelect(BaseEventData eventData)
        {
           onEvent.SendEvent(eventData);
        }
    }

    public static class BindSelectEventTriggerTriggerExtension
    {
        public static IUnRegister BindSelectEvent<T>(this T self, Action<BaseEventData> onSelect)
            where T : Component
        {
            return self.GetOrAddComponent<BindSelectEventTrigger>().Register(onSelect);
        }
        
        public static IUnRegister BindSelectEvent(this GameObject self, Action<BaseEventData> onSelect)
        {
            return self.GetOrAddComponent<BindSelectEventTrigger>().Register(onSelect);
        }
    }
}