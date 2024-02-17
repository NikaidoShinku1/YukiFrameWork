using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    public class OnMouseExitEvent : MouseAPI
    {
        private void OnMouseExit()
        {
            onEvent?.SendEvent();
        }
    }

    public static class OnMouseExitEventExtension
    {
        public static IUnRegister BindMouseExitEvent<T>(this T core, Action callBack) where T : Component
            => core.GetOrAddComponent<OnMouseExitEvent>().Register(callBack);

        public static IUnRegister BindMouseExitEvent(this GameObject core, Action callBack)
           => core.GetOrAddComponent<OnMouseExitEvent>().Register(callBack);
    }
}
