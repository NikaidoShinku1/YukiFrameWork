

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindDragEventTrigger: MonoAPI<PointerEventData>, IDragHandler
    {
        public void OnDrag(PointerEventData eventData)
        {
            onEvent.SendEvent(eventData);

        }
    }

    public static class BindDragEventTriggerExtension
    {
        public static IUnRegister BindDragEvent<T>(this T self, Action<PointerEventData> onDrag)
            where T : Component
        {
            return self.GetOrAddComponent<BindDragEventTrigger>().Register(onDrag);
        }
        
        public static IUnRegister BindDragEvent(this GameObject self, Action<PointerEventData> onDrag)
        {
            return self.GetOrAddComponent<BindDragEventTrigger>().Register(onDrag);
        }
    }
}