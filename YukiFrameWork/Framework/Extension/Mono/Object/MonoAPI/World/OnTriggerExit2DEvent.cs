using UnityEngine;
using System;

namespace YukiFrameWork
{
    public class OnTriggerExit2DEvent : MonoAPI<Collider2D>
    {
        private void OnTriggerExit2D(Collider2D collision)
        {
            onEvent?.SendEvent(collision);
        }
        protected override void OnTrigger(Collider2D t)
        {
            if (!onUnityEvent.layerMask.Contains(t.gameObject.layer))
                return;

            onUnityEvent.onUnityEvent?.Invoke(t);
        }
    }

    public static class OnTriggerExit2DEventExtension
    {
        public static IUnRegister BindTriggerExit2DEvent<T>(this T core, Action<Collider2D> callBack) where T : Component
            => core.GetOrAddComponent<OnTriggerExit2DEvent>().Register(callBack);

        public static IUnRegister BindTriggerExit2DEvent(this GameObject core, Action<Collider2D> callBack)
           => core.GetOrAddComponent<OnTriggerExit2DEvent>().Register(callBack);
    }
}