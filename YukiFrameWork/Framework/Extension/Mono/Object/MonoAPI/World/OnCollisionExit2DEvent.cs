using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class OnCollisionExit2DEvent : MonoAPI<Collision2D>
    {
        private void OnCollisionExit2D(Collision2D collision)
        {
            onEvent?.SendEvent(collision);
        }
    }

    public static class OnCollisionExit2DExtension
    {
        public static IUnRegister BindCollisionExit2DEvent<T>(this T core, Action<Collision2D> callBack) where T : Component
            => core.GetOrAddComponent<OnCollisionExit2DEvent>().Register(callBack);

        public static IUnRegister BindCollisionExit2DEvent(this GameObject core, Action<Collision2D> callBack)
           => core.GetOrAddComponent<OnCollisionExit2DEvent>().Register(callBack);
    }
}