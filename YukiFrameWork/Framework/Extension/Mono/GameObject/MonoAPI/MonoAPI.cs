using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public abstract class MonoAPI<T> : DefaultAPI<EasyEvent<T>,Action<T>>
    {
        protected EasyEvent<T> onEvent = new EasyEvent<T>();      

        public override IUnRegister Register(Action<T> callBack) => onEvent.RegisterEvent(callBack);      
    }
}
