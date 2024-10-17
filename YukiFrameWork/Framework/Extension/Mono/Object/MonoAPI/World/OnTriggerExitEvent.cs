using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class OnTriggerExitEvent : MonoAPI<Collider>
    {
        private void OnTriggerExit(Collider other)
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

    public static class OnTriggerExitEventExtension
    {
        public static IUnRegister BindTriggerExitEvent<T>(this T core, Action<Collider> callBack) where T : Component
            => core.GetOrAddComponent<OnTriggerExitEvent>().Register(callBack);

        public static IUnRegister BindTriggerExitEvent(this GameObject core, Action<Collider> callBack)
            => core.GetOrAddComponent<OnTriggerExitEvent>().Register(callBack);
    }
}
