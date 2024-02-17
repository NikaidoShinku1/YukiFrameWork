
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindUpdateSelectedEventTrigger: MonoAPI<BaseEventData>, IUpdateSelectedHandler
    {
               
        public void OnUpdateSelected(BaseEventData eventData)
        {
            onEvent.SendEvent(eventData);
        }
    }

    public static class  BindUpdateSelectedEventTriggerExtension
    {
        public static IUnRegister BindUpdateSelectedEvent<T>(this T self, Action<BaseEventData> onUpdateSelected)
            where T : Component
        {
            return self.GetOrAddComponent<BindUpdateSelectedEventTrigger>().Register(onUpdateSelected);
        }
        
        public static IUnRegister BindUpdateSelectedEvent(this GameObject self, Action<BaseEventData> onUpdateSelected)
        {
            return self.GetOrAddComponent<BindUpdateSelectedEventTrigger>().Register(onUpdateSelected);
        }
    }
}