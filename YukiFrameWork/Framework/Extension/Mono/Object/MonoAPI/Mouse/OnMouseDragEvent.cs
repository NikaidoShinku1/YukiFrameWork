using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace YukiFrameWork
{
    public class OnMouseDragEvent : MouseAPI
    {
        private void OnMouseDrag()
        {
            onEvent?.SendEvent();
        }
    }

    public static class OnMouseDragEventExtension
    {
        public static IUnRegister BindMouseDragEvent<T>(this T core, Action callBack) where T : Component
            => core.GetOrAddComponent<OnMouseDragEvent>().Register(callBack);

        public static IUnRegister BindMouseDragEvent(this GameObject core, Action callBack)
           => core.GetOrAddComponent<OnMouseDragEvent>().Register(callBack);
    }
}

