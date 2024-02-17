
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindDropEventTrigger: MonoAPI<PointerEventData>, IDropHandler
    {              
        public void OnDrop(PointerEventData eventData)
        {
            onEvent.SendEvent(eventData);
        }
    }

    public static class BindDropEventTriggerExtension
    {
        public static IUnRegister BindDropEvent<T>(this T self, Action<PointerEventData> onDrop)
            where T : Component
        {
            return self.GetOrAddComponent<BindDropEventTrigger>().Register(onDrop);
        }
        
        public static IUnRegister BindDropEvent(this GameObject self, Action<PointerEventData> onDrop)
        {
            return self.GetOrAddComponent<BindDropEventTrigger>().Register(onDrop);
        }
    }
}