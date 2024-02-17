using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindPointerEnterEventTrigger : MonoAPI<PointerEventData>, IPointerEnterHandler
    {
       
        public void OnPointerEnter(PointerEventData eventData)
        {
            onEvent.SendEvent(eventData);
        }
    }

    public static class BindPointerEnterEventTriggerExtension
    {
        public static IUnRegister BindPointerEnterEvent<T>(this T self, Action<PointerEventData> onPointerEnter) where T : Component
        {
            return self.GetOrAddComponent<BindPointerEnterEventTrigger>().Register(onPointerEnter);
        }
        public static IUnRegister BindPointerEnterEvent(this GameObject self, Action<PointerEventData> onPointerEnter)
        {
            return self.GetOrAddComponent<BindPointerEnterEventTrigger>().Register(onPointerEnter);
        }
    }
}