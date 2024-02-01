using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YukiFrameWork
{
    public abstract class DefaultAPI<T,Action> : MonoBehaviour where T : IEasyEventSystem where Action : System.Delegate
    {
        public abstract IUnRegister Register(Action callBack);

        //激活生命周期开关
        private void Start()
        {
            
        }
    }
}
