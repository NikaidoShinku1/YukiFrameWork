using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public class OnValidateEvent : MouseAPI
    {
        private void OnValidate()
        {
            onEvent?.SendEvent();
        }
    }

    public static class OnValidateEventExtension
    {
        public static IUnRegister BindValidateEvent<T>(this T core, Action callBack) where T : Component
            => core.GetOrAddComponent<OnValidateEvent>().Register(callBack);

        public static IUnRegister BindValidateEvent(this GameObject core, Action callBack)
         => core.GetOrAddComponent<OnValidateEvent>().Register(callBack);

    }
}
