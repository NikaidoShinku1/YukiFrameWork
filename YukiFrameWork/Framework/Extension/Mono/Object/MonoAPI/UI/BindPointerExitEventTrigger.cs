

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindPointerExitEventTrigger : MonoAPI<PointerEventData>, IPointerExitHandler
    {      
        public void OnPointerExit(PointerEventData eventData)
        {
            onEvent.SendEvent(eventData);
        }
    }

    public static class BindPointerExitEventTriggerExtension
    {
        public static IUnRegister BindPointerExitEvent<T>(this T self, Action<PointerEventData> onPointerExit)
            where T : Component
        {
            return self.GetOrAddComponent<BindPointerExitEventTrigger>().Register(onPointerExit);
        }
        
        public static IUnRegister BindPointerExitEvent(this GameObject self, Action<PointerEventData> onPointerExit)
        {
            return self.GetOrAddComponent<BindPointerExitEventTrigger>().Register(onPointerExit);
        }
    }
}