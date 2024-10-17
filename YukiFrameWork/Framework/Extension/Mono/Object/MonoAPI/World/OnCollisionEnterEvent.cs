using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class OnCollisionEnterEvent : MonoAPI<Collision>
    {
        private void OnCollisionEnter(Collision collision)
        {
            onEvent?.SendEvent(collision);
        }
        protected override void OnTrigger(Collision t)
        {
            if (!onUnityEvent.layerMask.Contains(t.gameObject.layer))
                return;

            onUnityEvent.onUnityEvent?.Invoke(t);
        }
    }

    public static class OnCollisionEnterExtension
    {
        public static IUnRegister BindCollisionEnterEvent<T>(this T core, Action<Collision> callBack) where T : Component
            => core.GetOrAddComponent<OnCollisionEnterEvent>().Register(callBack);

        public static IUnRegister BindCollisionEnterEvent(this GameObject core, Action<Collision> callBack)
           => core.GetOrAddComponent<OnCollisionEnterEvent>().Register(callBack);
    }
}