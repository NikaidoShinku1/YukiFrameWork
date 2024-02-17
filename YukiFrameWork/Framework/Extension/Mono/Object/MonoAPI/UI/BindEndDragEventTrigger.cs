
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindEndDragEventTrigger: MonoAPI<PointerEventData>, IEndDragHandler
    {           
        public void OnEndDrag(PointerEventData eventData)
        {
            onEvent.SendEvent(eventData);

        }
    }

    public static class BindEndDragEventTriggerExtension
    {
        public static IUnRegister BindEndDragEvent<T>(this T self, Action<PointerEventData> onEndDrag)
            where T : Component
        {
            return self.GetOrAddComponent<BindEndDragEventTrigger>().Register(onEndDrag);
        }
        
        public static IUnRegister BindEndDragEvent(this GameObject self, Action<PointerEventData> onEndDrag)
        {
            return self.GetOrAddComponent<BindEndDragEventTrigger>().Register(onEndDrag);
        }
    }
}