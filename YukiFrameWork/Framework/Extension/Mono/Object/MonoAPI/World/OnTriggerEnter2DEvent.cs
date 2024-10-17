using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class OnTriggerEnter2DEvent : MonoAPI<Collider2D>
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            onEvent?.SendEvent(collision);
        }
        protected override void OnTrigger(Collider2D t)
        {
            if (!onUnityEvent.layerMask.Contains(t.gameObject.layer))
                return;

            onUnityEvent.onUnityEvent?.Invoke(t);
        }
    }

    public static class OnTriggerEnter2DEventExtension
    {
        public static IUnRegister BindTriggerEnter2DEvent<T>(this T core, Action<Collider2D> callBack) where T : Component
            => core.GetOrAddComponent<OnTriggerEnter2DEvent>().Register(callBack);

        public static IUnRegister BindTriggerEnter2DEvent(this GameObject core, Action<Collider2D> callBack)
            => core.GetOrAddComponent<OnTriggerEnter2DEvent>().Register(callBack);
    }
}