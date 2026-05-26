using System;

namespace YukiFrameWork
{
    public class GUIAPI : DefaultAPI<EasyEvent,Action>
    {
        public override IUnRegister Register(Action callBack)
        {
            return onEvent.RegisterEvent(callBack);
        }
    }
}