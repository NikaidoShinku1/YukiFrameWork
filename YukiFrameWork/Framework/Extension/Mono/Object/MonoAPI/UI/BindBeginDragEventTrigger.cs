

using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindBeginDragEventTrigger: MonoAPI<PointerEventData>, IBeginDragHandler
    {              
        public void OnBeginDrag(PointerEventData eventData)
        {
            onEvent.SendEvent(eventData);
        }
    }

    public static class BindBeginDragEventTriggerExtension
    {
        public static IUnRegister BindBeginDragEvent<T>(this T self, Action<PointerEventData> onBeganDrag)
            where T : Component
        {
            return self.GetOrAddComponent<BindBeginDragEventTrigger>().Register(onBeganDrag);
        }
        
        public static IUnRegister BindBeginDragEvent(this GameObject self, Action<PointerEventData> onBeganDrag)
        {
            return self.GetOrAddComponent<BindBeginDragEventTrigger>().Register(onBeganDrag);
        }
    }
}