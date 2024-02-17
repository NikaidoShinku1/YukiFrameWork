
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindInitializePotentialDragEventTrigger:MonoAPI<PointerEventData>, IInitializePotentialDragHandler
    {     
        public void OnInitializePotentialDrag(PointerEventData eventData)
        {
            onEvent.SendEvent(eventData);
        }
    }

    public static class BindInitializePotentialDragEventTriggerExtension
    {
        public static IUnRegister BindInitializePotentialDragEvent<T>(this T self, Action<PointerEventData> onInitializePotentialDrag)
            where T : Component
        {
            return self.GetOrAddComponent<BindInitializePotentialDragEventTrigger>().Register(onInitializePotentialDrag);
        }
        
        public static IUnRegister BindInitializePotentialDragEvent(this GameObject self, Action<PointerEventData> onInitializePotentialDrag)
        {
            return self.GetOrAddComponent<BindInitializePotentialDragEventTrigger>().Register(onInitializePotentialDrag);
        }
    }
}