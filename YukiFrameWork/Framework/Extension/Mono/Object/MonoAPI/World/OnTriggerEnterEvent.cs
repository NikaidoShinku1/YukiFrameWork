﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork
{
    public class OnTriggerEnterEvent : MonoAPI<Collider>
    {      
        private void OnTriggerEnter(Collider other)
        {
            onEvent?.SendEvent(other);
        }

        protected override void OnTrigger(Collider t)
        {
            if (!onUnityEvent.layerMask.Contains(t.gameObject.layer))
                return;

            onUnityEvent.onUnityEvent?.Invoke(t);
        }
    }

    public static class OnTriggerEnterEventExtension
    {
        public static IUnRegister BindTriggerEnterEvent<T>(this T core,Action<Collider> callBack) where T : Component
        {
            OnTriggerEnterEvent trigger = core.GetOrAddComponent<OnTriggerEnterEvent>();
            return trigger.Register(callBack);
        }

        public static IUnRegister BindTriggerEnterEvent(this GameObject core, Action<Collider> callBack)
        {
            OnTriggerEnterEvent trigger = core.GetOrAddComponent<OnTriggerEnterEvent>();
            return trigger.Register(callBack);
        }
    }
}