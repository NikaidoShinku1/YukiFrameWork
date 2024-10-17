using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class OnTriggerStay2DEvent : MonoAPI<Collider2D>
    {
        private void OnTriggerStay2D(Collider2D collision)
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

    public static class OnTriggerStay2DEventExtension
    {
        public static IUnRegister BindTriggerStay2DEvent(this GameObject core, Action<Collider2D> callBack)
            => core.GetOrAddComponent<OnTriggerStay2DEvent>().Register(callBack);

        public static IUnRegister BindTriggerStay2DEvent<T>(this T core, Action<Collider2D> callBack) where T : Component
            => core.GetOrAddComponent<OnTriggerStay2DEvent>().Register(callBack);
    }
}