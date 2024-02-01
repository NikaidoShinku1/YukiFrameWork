using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class OnMouseDownEvent : MouseAPI
    {
        private void OnMouseDown()
        {
            onEvent?.SendEvent();
        }
    }

    public static class OnMouseDownEventExtension
    {
        public static IUnRegister BindMouseDownEvent<T>(this T core, Action callBack) where T : Component
            => core.GetOrAddComponent<OnMouseDownEvent>().Register(callBack);

        public static IUnRegister BindMouseDownEvent(this GameObject core, Action callBack)
           => core.GetOrAddComponent<OnMouseDownEvent>().Register(callBack);
    }
}
