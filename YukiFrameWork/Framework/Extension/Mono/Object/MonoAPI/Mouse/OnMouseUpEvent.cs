using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    public class OnMouseUpEvent : MouseAPI
    {
        private void OnMouseUp()
        {
            onEvent?.SendEvent();
        }
    }

    public static class OnMouseUpEventExtension
    {
        public static IUnRegister BindMouseUpEvent<T>(this T core, Action callBack) where T : Component
            => core.GetOrAddComponent<OnMouseUpEvent>().Register(callBack);

        public static IUnRegister BindMouseUpEvent(this GameObject core, Action callBack)
           => core.GetOrAddComponent<OnMouseUpEvent>().Register(callBack);
    }
}
