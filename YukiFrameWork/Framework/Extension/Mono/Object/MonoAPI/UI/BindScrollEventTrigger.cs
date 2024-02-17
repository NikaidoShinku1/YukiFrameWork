
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindScrollEventTrigger: MonoAPI<PointerEventData>, IScrollHandler
    {     
        public void OnScroll(PointerEventData eventData)
        {
            onEvent.SendEvent(eventData);
        }
    }

    public static class BindScrollEventTriggerExtension
    {
        public static IUnRegister BindScrollEvent<T>(this T self, Action<PointerEventData> onScroll)
            where T : Component
        {
            return self.GetOrAddComponent<BindScrollEventTrigger>().Register(onScroll);
        }
        
        public static IUnRegister BindScrollEvent(this GameObject self, Action<PointerEventData> onScroll)
        {
            return self.GetOrAddComponent<BindScrollEventTrigger>().Register(onScroll);
        }
    }
}