using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public abstract class DefaultAPI<T,Action> : MonoBehaviour where T : IEasyEvent,new() where Action : System.Delegate
    {
        protected T onEvent = new T();
        public abstract IUnRegister Register(Action callBack);

        //激活生命周期开关
        protected virtual void Start()
        {
           
        }

        public void Clear()
        {
            onEvent.UnRegisterAllEvent();
        }
    }
}
