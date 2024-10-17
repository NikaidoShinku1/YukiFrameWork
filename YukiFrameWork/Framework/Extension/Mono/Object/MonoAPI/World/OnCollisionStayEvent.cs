using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class OnCollisionStayEvent : MonoAPI<Collision>
    {
        private void OnCollisionStay(Collision collision)
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

    public static class OnCollisionStayExtension
    {
        public static IUnRegister BindCollisionStayEvent<T>(this T core, Action<Collision> callBack) where T : Component
            => core.GetOrAddComponent<OnCollisionStayEvent>().Register(callBack);

        public static IUnRegister BindCollisionStayEvent(this GameObject core, Action<Collision> callBack)
           => core.GetOrAddComponent<OnCollisionStayEvent>().Register(callBack);
    }
}
