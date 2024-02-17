
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindDeselectEventTrigger: MonoAPI<BaseEventData>, IDeselectHandler
    {
     
        public void OnDeselect(BaseEventData eventData)
        {
           onEvent.SendEvent(eventData);
        }
    }

    public static class BindDeselectEventTriggerExtension
    {
        public static IUnRegister BindDeselectEvent<T>(this T self, Action<BaseEventData> onDeselect)
            where T : Component
        {
            return self.GetOrAddComponent<BindDeselectEventTrigger>().Register(onDeselect);
        }
        
        public static IUnRegister BindDeselectEvent(this GameObject self, Action<BaseEventData> onDeselect)
        {
            return self.GetOrAddComponent<BindDeselectEventTrigger>().Register(onDeselect);
        }
    }
}