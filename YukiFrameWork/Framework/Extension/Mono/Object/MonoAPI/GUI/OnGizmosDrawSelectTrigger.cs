using System;
using UnityEngine;

namespace YukiFrameWork
{
    public class OnGizmosDrawSelectedTrigger : GUIAPI
    {
        private void OnDrawGizmosSelected()
        {
            onEvent?.SendEvent();
        }
    }
    
    public static class OnGizmosDrawSelectedEventExtension
    {
        public static IUnRegister BindDrawGizmosSelectedEvent<T>(this T core, Action callBack) where T : Component
            => core.GetOrAddComponent<OnGizmosDrawSelectedTrigger>().Register(callBack);

        public static IUnRegister BindDrawGizmosSelectedEvent(this GameObject core, Action callBack)
            => core.GetOrAddComponent<OnGizmosDrawSelectedTrigger>().Register(callBack);
    }
}