
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YukiFrameWork
{
    public class BindSubmitEventTrigger: MonoAPI<BaseEventData>, ISubmitHandler
    {      
        public void OnSubmit(BaseEventData eventData)
        {
            onEvent.SendEvent(eventData);
        }
    }

    public static class BindSubmitEventTriggerExtension
    {
        public static IUnRegister BindSubmitEvent<T>(this T self, Action<BaseEventData> onSubmit)
            where T : Component
        {
            return self.GetOrAddComponent<BindSubmitEventTrigger>().Register(onSubmit);
        }
        
        public static IUnRegister BindSubmitEvent(this GameObject self, Action<BaseEventData> onSubmit)
        {
            return self.GetOrAddComponent<BindSubmitEventTrigger>().Register(onSubmit);
        }
    }
}