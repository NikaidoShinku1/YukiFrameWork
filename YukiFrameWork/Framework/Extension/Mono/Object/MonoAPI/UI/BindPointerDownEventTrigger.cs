
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindPointerDownEventTrigger : MonoAPI<PointerEventData>,IPointerDownHandler
    {      
        public void OnPointerDown(PointerEventData eventData)
        {
            onEvent.SendEvent(eventData);
        }
    }

    public static class BindPointerDownEventTriggerExtension
    {
        public static IUnRegister BindPointerDownEvent<T>(this T self, Action<PointerEventData> onPointerDownEvent)
            where T : Component
        {
            return self.GetOrAddComponent<BindPointerDownEventTrigger>()
                .Register(onPointerDownEvent);
        }
        
        public static IUnRegister BindPointerDownEvent(this GameObject self, Action<PointerEventData> onPointerDownEvent)
        {
            return self.GetOrAddComponent<BindPointerDownEventTrigger>()
                .Register(onPointerDownEvent);
        }
    }
}