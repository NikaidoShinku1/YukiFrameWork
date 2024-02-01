using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class OnCollisionExitEvent : MonoAPI<Collision>
    {
        private void OnCollisionExit(Collision collision)
        {
            onEvent?.SendEvent(collision);
        }      
    }

    public static class OnCollisionExitExtension
    {
        public static IUnRegister BindCollisionExitEvent<T>(this T core, Action<Collision> callBack) where T : Component
            => core.GetOrAddComponent<OnCollisionExitEvent>().Register(callBack);

        public static IUnRegister BindCollisionExitEvent(this GameObject core, Action<Collision> callBack)
           => core.GetOrAddComponent<OnCollisionExitEvent>().Register(callBack);
    }
}