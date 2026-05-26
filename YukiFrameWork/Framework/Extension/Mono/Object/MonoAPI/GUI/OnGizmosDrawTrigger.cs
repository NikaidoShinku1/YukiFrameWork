using System;
using UnityEngine;

namespace YukiFrameWork
{
    public class OnGizmosDrawTrigger : GUIAPI
    {
        private void OnDrawGizmos()
        {
            onEvent?.SendEvent();
        }
    }
    
    public static class OnGizmosDrawEventExtension
    {
        public static IUnRegister BindDrawGizmosEvent<T>(this T core, Action callBack) where T : Component
            => core.GetOrAddComponent<OnGizmosDrawTrigger>().Register(callBack);

        public static IUnRegister BindDrawGizmosEvent(this GameObject core, Action callBack)
            => core.GetOrAddComponent<OnGizmosDrawTrigger>().Register(callBack);
    }
}