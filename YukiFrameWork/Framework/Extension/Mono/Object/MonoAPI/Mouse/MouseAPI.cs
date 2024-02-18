using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace YukiFrameWork
{
    public abstract class MouseAPI : DefaultAPI<EasyEvent,Action>
    {      
        public override IUnRegister Register(Action callBack) => onEvent.RegisterEvent(callBack);
    }
}
