

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindPointerClickEventTrigger : MonoAPI<PointerEventData>, IPointerClickHandler
    {        
        public void OnPointerClick(PointerEventData eventData)
        {
            onEvent.SendEvent(eventData);
        }
    }

    public static class BindPointerClickEventTriggerExtension
    {
        public static IUnRegister BindPointerClickEvent<T>(this T self, Action<PointerEventData> onPointerClick)
            where T : Component
        {
            return self.GetOrAddComponent<BindPointerClickEventTrigger>().Register(onPointerClick);
        }
        
        public static IUnRegister BindPointerClickEvent(this GameObject self, Action<PointerEventData> onPointerClick)
        {
            return self.GetOrAddComponent<BindPointerClickEventTrigger>().Register(onPointerClick);
        }
    }
}