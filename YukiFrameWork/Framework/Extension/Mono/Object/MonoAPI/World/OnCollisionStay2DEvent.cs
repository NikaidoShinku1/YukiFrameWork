using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class OnCollisionStay2DEvent : MonoAPI<Collision2D>
    {
        private void OnCollisionStay2D(Collision2D collision)
        {
            onEvent?.SendEvent(collision);
        }
    }

    public static class OnCollisionStay2DExtension
    {
        public static IUnRegister BindCollisionStay2DEvent<T>(this T core, Action<Collision2D> callBack) where T : Component
            => core.GetOrAddComponent<OnCollisionStay2DEvent>().Register(callBack);

        public static IUnRegister BindCollisionStay2DEvent(this GameObject core, Action<Collision2D> callBack)
           => core.GetOrAddComponent<OnCollisionStay2DEvent>().Register(callBack);
    }
}