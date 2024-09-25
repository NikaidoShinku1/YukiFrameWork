///=====================================================
/// - FileName:      EasyEvent.cs
/// - NameSpace:     YukiFrameWork
/// - Description:   通过本地的代码生成器创建的脚本
/// - Creation Time: 2024/3/22 14:18:46
/// -  (C) Copyright 2008 - 2024
/// -  All Rights Reserved.
///=====================================================
using System;
using System.Threading.Tasks;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using YukiFrameWork.Pools;
namespace YukiFrameWork
{
    public class EasyEvent : EasyEventBase<Action>
    {
        public override IUnRegister RegisterEvent(Action onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent()
        {
            OnEasyEvent?.Invoke();
        }

        public override void UnRegister(Action onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K> : EasyEventBase<Action<T, K>>
    {
        public override IUnRegister RegisterEvent(Action<T, K> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k)
            => OnEasyEvent?.Invoke(t, k);

        public override void UnRegister(Action<T, K> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K, Q> : EasyEventBase<Action<T, K, Q>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }
        public void SendEvent(T t, K k, Q q)
            => OnEasyEvent?.Invoke(t, k, q);

        public override void UnRegister(Action<T, K, Q> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T, K, Q, P> : EasyEventBase<Action<T, K, Q, P>>
    {
        public override IUnRegister RegisterEvent(Action<T, K, Q, P> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t, K k, Q q, P p)
            => OnEasyEvent?.Invoke(t, k, q, p);

        public override void UnRegister(Action<T, K, Q, P> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class EasyEvent<T> : EasyEventBase<Action<T>>
    {
        public override IUnRegister RegisterEvent(Action<T> onEvent)
        {
            OnEasyEvent += onEvent;
            return this;
        }

        public void SendEvent(T t)
        {          
            OnEasyEvent?.Invoke(t);
        }

        public override void UnRegister(Action<T> onEvent)
        {
            OnEasyEvent -= onEvent;
        }
    }

    public class AsyncEasyEvent<T> : EasyEventBase<Func<T, Task>>
    {
        private List<Func<T, Task>> onAsyncEvent = new List<Func<T, Task>>();
        public override IUnRegister RegisterEvent(Func<T, Task> onEvent)
        {
            onAsyncEvent.Add(onEvent);
            return this;
        }    
    
        public override void UnRegister(Func<T, Task> onEvent)
        {
            onAsyncEvent.Remove(onEvent);            
        }

        public async Task SendEvent(T arg)
        {
            foreach (Func<T,Task> ev in onAsyncEvent)
            {
                if (ev == null) continue;
                var task = ev.Invoke(arg);
                if (task == null) continue;
                await task;
            } 
        }

        public override void UnRegisterAllEvent()
        {
            onAsyncEvent.Clear();          
        } 
    }


}
