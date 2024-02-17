using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class OnCollisionEnter2DEvent : MonoAPI<Collision2D>
    {
        private void OnCollisionEnter2D(Collision2D collision)
        {
            onEvent?.SendEvent(collision);
        }
    }

    public static class OnCollisionEnter2DExtension
    {
        public static IUnRegister BindCollisionEnter2DEvent<T>(this T core, Action<Collision2D> callBack) where T : Component
            => core.GetOrAddComponent<OnCollisionEnter2DEvent>().Register(callBack);

        public static IUnRegister BindCollisionEnter2DEvent(this GameObject core, Action<Collision2D> callBack)
           => core.GetOrAddComponent<OnCollisionEnter2DEvent>().Register(callBack);
    }
}