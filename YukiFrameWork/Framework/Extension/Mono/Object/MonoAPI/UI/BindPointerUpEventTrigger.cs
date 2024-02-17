
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindPointerUpEventTrigger : MonoAPI<PointerEventData>,IPointerUpHandler
    {       
        public void OnPointerUp(PointerEventData eventData)
        {
            onEvent.SendEvent(eventData);
        }
    }

    public static class BindPointerUpEventTriggerExtension
    {
        public static IUnRegister BindPointerUpEvent<T>(this T self, Action<PointerEventData> onPointerUpEvent) where T : Component
        {
            return self.GetOrAddComponent<BindPointerUpEventTrigger>().Register(onPointerUpEvent);
        }
        public static IUnRegister BindPointerUpEvent(this GameObject self, Action<PointerEventData> onPointerUpEvent)
        {
            return self.GetOrAddComponent<BindPointerUpEventTrigger>().Register(onPointerUpEvent);
        }
    }
}