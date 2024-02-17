using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    public class OnMouseEnterEvent : MouseAPI
    {
        private void OnMouseEnter()
        {
            onEvent?.SendEvent();
        }
    }

    public static class OnMouseEnterEventExtension
    {
        public static IUnRegister BindMouseEnterEvent<T>(this T core, Action callBack) where T : Component
            => core.GetOrAddComponent<OnMouseEnterEvent>().Register(callBack);

        public static IUnRegister BindMouseEnterEvent(this GameObject core, Action callBack)
           => core.GetOrAddComponent<OnMouseEnterEvent>().Register(callBack);
    }
}