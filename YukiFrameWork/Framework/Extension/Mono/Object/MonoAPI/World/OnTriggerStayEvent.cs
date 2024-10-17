using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class OnTriggerStayEvent : MonoAPI<Collider>
    {
        private void OnTriggerStay(Collider other)
        {
            onEvent?.SendEvent(other);
        }
        protected override void OnTrigger(Collider t)
        {
            if (!onUnityEvent.layerMask.Contains(t.gameObject.layer))
                return;

            onUnityEvent.onUnityEvent?.Invoke(t);
        }
    }

    public static class OnTriggerStayEventExtension
    {
        public static IUnRegister BindTriggerStayEvent<T>(this T core, Action<Collider> callBack) where T : Component
            => core.GetOrAddComponent<OnTriggerStayEvent>().Register(callBack);

        public static IUnRegister BindTriggerStayEvent(this GameObject core, Action<Collider> callBack)
           => core.GetOrAddComponent<OnTriggerStayEvent>().Register(callBack);
    }
}
