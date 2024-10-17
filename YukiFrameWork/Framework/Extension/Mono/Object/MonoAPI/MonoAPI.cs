using Sirenix.OdinInspector;
using System;
using UnityEngine;
using UnityEngine.Events;

namespace YukiFrameWork
{
    [Serializable]
    public class SerializeCollisionUnityEvent<T>
    {
        [SerializeField, LabelText("检测层级")]
        public LayerMask layerMask;
        [SerializeField, LabelText("可视化注册事件")]
        public UnityEvent<T> onUnityEvent;        
    }
    public abstract class MonoAPI<T> : DefaultAPI<EasyEvent<T>,Action<T>>
    {
        [SerializeField,LabelText("碰撞检测可视")]
        protected SerializeCollisionUnityEvent<T> onUnityEvent = new SerializeCollisionUnityEvent<T>();        
        public override IUnRegister Register(Action<T> callBack) => onEvent.RegisterEvent(callBack);

        protected override void Start()
        {
            Register(OnTrigger);              
        }

        protected virtual void OnTrigger(T t) { }
    }
}
