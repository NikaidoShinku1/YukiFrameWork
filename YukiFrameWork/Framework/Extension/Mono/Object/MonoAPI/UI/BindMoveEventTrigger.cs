
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindMoveEventTrigger: MonoAPI<AxisEventData>, IMoveHandler
    {       
        public void OnMove(AxisEventData eventData)
        {
            onEvent.SendEvent(eventData);
        }
    }

    public static class BindMoveEventTriggerExtension
    {
        public static IUnRegister BindMoveEvent<T>(this T self, Action<AxisEventData> onMove)
            where T : Component
        {
            return self.GetOrAddComponent<BindMoveEventTrigger>().Register(onMove);
        }
        
        public static IUnRegister BindMoveEvent(this GameObject self, Action<AxisEventData> onMove)
        {
            return self.GetOrAddComponent<BindMoveEventTrigger>().Register(onMove);
        }
    }
}